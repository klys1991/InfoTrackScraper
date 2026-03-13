using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InfoTrack.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LocationBaselines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Location = table.Column<string>(type: "TEXT", nullable: false),
                    SiteId = table.Column<string>(type: "TEXT", nullable: false),
                    LastKnownCount = table.Column<int>(type: "INTEGER", nullable: false),
                    LastHealth = table.Column<string>(type: "TEXT", nullable: false),
                    LastVerified = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocationBaselines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SearchRuns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StartedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Locations = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SearchRuns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SolicitorRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SearchRunId = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Location = table.Column<string>(type: "TEXT", nullable: false),
                    SiteId = table.Column<string>(type: "TEXT", nullable: false),
                    Phone = table.Column<string>(type: "TEXT", nullable: true),
                    Email = table.Column<string>(type: "TEXT", nullable: true),
                    Address = table.Column<string>(type: "TEXT", nullable: true),
                    Rating = table.Column<double>(type: "REAL", nullable: true),
                    ReviewCount = table.Column<int>(type: "INTEGER", nullable: true),
                    Website = table.Column<string>(type: "TEXT", nullable: true),
                    IsNew = table.Column<bool>(type: "INTEGER", nullable: false),
                    ParseHealth = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitorRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitorRecords_SearchRuns_SearchRunId",
                        column: x => x.SearchRunId,
                        principalTable: "SearchRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LocationBaselines_Location_SiteId",
                table: "LocationBaselines",
                columns: new[] { "Location", "SiteId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SolicitorRecords_SearchRunId_Location",
                table: "SolicitorRecords",
                columns: new[] { "SearchRunId", "Location" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "SolicitorRecords");
            migrationBuilder.DropTable(name: "SearchRuns");
            migrationBuilder.DropTable(name: "LocationBaselines");
        }
    }
}