# HolidayApp.Api

This is an ASP.NET Core Web API that ingests public holiday data from the Nager.Date API (https://date.nager.at) and stores it in a Microsoft SQL Server database using Entity Framework Core.

Features implemented:
- Ingest holidays from the external API and save to MSSQL
- Get last celebrated N holidays for a country (date + local name)
- For a given year and list of countries, get number of public holidays not falling on weekends (sorted desc)
- For a given year and two countries, return deduplicated list of dates celebrated in both countries (with local names)

How to run
1. Ensure you have .NET 7 SDK installed: https://dotnet.microsoft.com/en-us/download
2. Ensure a SQL Server instance is available and reachable from your machine. Update connection string in `appsettings.json` under `ConnectionStrings:DefaultConnection`.

3. From the project folder, add EF tools (if not installed):

```bash
dotnet tool install --global dotnet-ef
```

4. Create the initial migration and update database:

```bash
cd HolidayApp.Api
dotnet ef migrations add InitialCreate --project .
dotnet ef database update --project .
```

5. Run the API:

```bash
dotnet run --project HolidayApp.Api
```

6. Use the Swagger UI (if in Development) at `https://localhost:5001/swagger` to try endpoints. Example flows:

- Import holidays for UK in 2024: POST /api/holidays/import/2024/GB
- Get last 3 holidays for GB: GET /api/holidays/last/GB?count=3
- Get non-weekend counts: GET /api/holidays/nonweekend-count?year=2024&countries=GB,US
- Get common: GET /api/holidays/common?year=2024&c1=GB&c2=US

Tests
- No tests are included in this initial scaffold. Add unit/integration tests as needed.

Publishing to GitHub
- Initialize a git repo, commit files, and push to your personal GitHub. Example:

```bash
git init
git add .
git commit -m "Initial HolidayApp API implementation"
git branch -M main
git remote add origin https://github.com/<your-username>/HolidayApp.git
git push -u origin main
```

Notes and assumptions
- The project uses EF Core alternate key to deduplicate identical country+date+localName rows. For large-scale ingestion consider using bulk methods or raw SQL MERGE for better performance.
- The Nager.Date API mapping is intentionally minimal; extend fields if needed.
