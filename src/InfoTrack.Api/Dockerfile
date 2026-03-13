FROM mcr.microsoft.com/dotnet/nightly/sdk:10.0 AS build
WORKDIR /src
COPY ["InfoTrack.Api/InfoTrack.Api.csproj", "InfoTrack.Api/"]
COPY ["InfoTrack.Scraper/InfoTrack.Scraper.fsproj", "InfoTrack.Scraper/"]
RUN dotnet restore "InfoTrack.Api/InfoTrack.Api.csproj"
COPY . .
RUN dotnet publish "InfoTrack.Api/InfoTrack.Api.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/nightly/aspnet:10.0 AS runtime
WORKDIR /app
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "InfoTrack.Api.dll"]
