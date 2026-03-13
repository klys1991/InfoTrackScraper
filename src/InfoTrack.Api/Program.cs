using InfoTrack.Api.Data;
using InfoTrack.Api.Middleware;
using InfoTrack.Api.Services;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("Default")
        ?? "Data Source=infotrack.db"));

builder.Services.AddHttpClient("scraper", client =>
{
    client.DefaultRequestHeaders.UserAgent.ParseAdd(
        "Mozilla/5.0 (compatible; InfoTrackBot/1.0)");
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddSingleton<IScrapeRunRegistry, ScrapeRunRegistry>();
builder.Services.AddScoped<IScraperOrchestrator, ScraperOrchestrator>();
builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddCors(opt =>
    opt.AddDefaultPolicy(p =>
    {
        var configured = builder.Configuration["AllowedOrigins"]?.Split(',') ?? [];
        var origins = configured
            .Concat(["http://web:80", "http://localhost:3000", "http://localhost:4200"])
            .Distinct()
            .ToArray();
        p.WithOrigins(origins).AllowAnyMethod().AllowAnyHeader();
    }));

builder.Services.AddHealthChecks();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseMiddleware<ExceptionMiddleware>();
app.UseCors();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");
app.Run();