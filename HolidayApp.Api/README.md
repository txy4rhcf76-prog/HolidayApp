# HolidayApp.Api

This is an ASP.NET Core Web API built with .NET 10 that imports public holiday data from the Nager.Date API (https://date.nager.at) and stores it in a Microsoft SQL Server database using Entity Framework Core 10.0.0.

Features implemented:
- Import holidays from the external API and save to MSSQL
- Get last celebrated N holidays for a country (date + local name)
- For a given year and list of countries, get number of public holidays not falling on weekends (sorted desc)
- For a given year and two countries, return deduplicated list of dates celebrated in both countries (with local names)

How to run
1. Ensure you have .NET 10 SDK installed: https://dotnet.microsoft.com/en-us/download
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

6. Use the Swagger UI (if in Development) at `http://localhost:5000/swagger` to try endpoints. Example flows:

- Import holidays for UK in 2024: POST /api/holidays/import/2024/GB
- Get last 3 holidays for GB: GET /api/holidays/last/GB?count=3
- Get non-weekend counts: GET /api/holidays/nonweekend-count?year=2024&countries=GB,US
- Get common: GET /api/holidays/common?year=2024&c1=GB&c2=US

Tests
- Unit tests are included in `HolidayApp.Api.Tests/` using xUnit and EF Core InMemory provider.
- Run tests: `dotnet test`
- Tests cover repository operations: last holidays, non-weekend counts, and common holidays.

Docker Setup (optional)
- Use the provided `docker-compose.yml` to run SQL Server locally:

```bash
docker compose up -d
```

- Update the connection string in `appsettings.json` to point to the container (default: localhost,1433).

Publishing to GitHub
- This project is already configured with git. To push updates:

```bash
cd /Users/ardemirkan/HolidayApp
git add .
git commit -m "Your commit message"
git push origin main
```

Notes and assumptions
- The project uses EF Core to fetch data efficiently with AsNoTracking queries.
- Weekend filtering (DayOfWeek checks) is performed in-memory to avoid translation issues with SQL Server.
- Duplicate prevention uses in-memory key hashing (country|date|localName) before inserting to avoid EF tracking conflicts.
- For large-scale ingestion (millions of rows), consider using SqlBulkCopy or raw SQL MERGE for better performance.
- The Nager.Date API mapping is intentionally minimal; extend fields in the `Holiday` model if needed.
