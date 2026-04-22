# BobsCornApp

Bob's Corn is a small full-stack app where each client can successfully buy at most 1 corn per rolling window.

## Repository structure

- `BobsCornApp-Back`: ASP.NET Core Web API using a layered architecture.
- `BobsCornApp-Client`: Angular client for buying corn and displaying rate-limit feedback.

## Business rule

Each client can buy at most 1 corn every `WindowSeconds`.

- Successful purchase: `200 OK`
- Missing `clientId`: `400 Bad Request`
- Rate limit exceeded: `429 Too Many Requests`

The `clientId` is now required as a query string parameter:

`POST /api/corn/buy?clientId=<guid>`

The frontend generates a GUID automatically, stores it in `localStorage`, and lets you generate a new one from the UI. Generating a new GUID replaces the previous value in `localStorage` and resets the purchase counters shown in the browser.

## API responses

Successful response example:

```json
{
  "corn": "\ud83c\udf3d",
  "message": "Corn purchased successfully.",
  "purchasedAtUtc": "2026-04-18T18:00:00+00:00"
}
```

Missing client id example:

```json
{
  "message": "The clientId query parameter is required."
}
```

Rate-limited response example for `WindowSeconds = 60`:

```json
{
  "message": "Rate limit exceeded. Clients can buy at most 1 corn every 60 seconds.",
  "retryAfterSeconds": 60
}
```

The API also sets the `Retry-After` response header on `429` responses.

## Local prerequisites

- .NET 9 SDK
- Node.js
- npm
- SQL Server Express or another SQL Server instance reachable from your machine

## Backend configuration

The backend uses `BobsCornApp-Back/BobsCornApp.Api/appsettings.Development.json`.

Relevant settings:

```json
{
  "CornRateLimit": {
    "WindowSeconds": 60
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=BobsCornDB;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False;"
  }
}
```

Notes:

- Update `DefaultConnection` if your SQL Server instance name is different.
- The repository creates the database and `dbo.CornPurchases` table automatically when the configured credentials allow it.

## Running locally without Docker

From the repository root:

```powershell
dotnet build BobsCornApp-Back\BobsCornApp-Back.sln -m:1
dotnet run --project BobsCornApp-Back\BobsCornApp.Api --launch-profile http
```

From `BobsCornApp-Client`:

```powershell
npm install
npm start
```

Local URLs:

- Frontend: `http://localhost:4200`
- Backend: `http://localhost:5224`
- Swagger: `http://localhost:5224/swagger`

The Angular app now uses `/api` and the dev server proxy in `BobsCornApp-Client/proxy.conf.json`, so `npm start` works with the backend running on `http://localhost:5224`.

## Running with Docker

Prerequisite:

- Docker Desktop installed and running

From the repository root:

```powershell
docker compose up --build
```

Exposed services:

- Frontend: `http://localhost:4200`
- Backend API: `http://localhost:5224`
- Swagger: `http://localhost:5224/swagger`
- SQL Server: `localhost:1433`

The compose stack includes:

- `client`: Angular app built and served by Nginx
- `api`: ASP.NET Core API
- `db`: SQL Server 2022

Optional override for the SQL Server password:

```powershell
$env:MSSQL_SA_PASSWORD="YourStrongPassword123!"
docker compose up --build
```

Default local compose password:

`BobsCornApp!234`

## Running tests

Backend tests:

```powershell
dotnet test BobsCornApp-Back\BobsCornApp-Back.sln -m:1
```

Frontend tests:

```powershell
cd BobsCornApp-Client
npx jest --runInBand --passWithNoTests
```

## Implementation notes

- Rate limiting is enforced in the application service layer.
- Persistence is implemented with Dapper in the infrastructure layer.
- The frontend sends `clientId` as a query parameter instead of an HTTP header.
- The rate-limit message now uses `CornRateLimit:WindowSeconds`.
- Docker Compose runs frontend, backend, and SQL Server with a single command.

## Architecture summary

- The solution uses Clean Architecture: API handles HTTP, Application contains use-case logic, Infrastructure handles SQL Server access, and Domain holds the core entity model.
- The rate-limit rule lives in the application service so the business rule stays independent from controllers, Angular, Docker, and storage details.
- Persistence is accessed through a repository abstraction, which keeps Dapper and SQL Server concerns out of the use-case layer.
- ASP.NET Core dependency injection wires services and configuration through interfaces and options instead of hardcoded dependencies.
- The frontend calls `/api` through a same-origin proxy strategy: Angular dev proxy in local development and Nginx reverse proxy in Docker. This avoids hardcoded backend hosts in the browser bundle.
- Docker Compose orchestrates the three runtime pieces as separate services: client, API, and SQL Server.

## Manual test flow

1. Start the app locally or with Docker.
2. Open the frontend at `http://localhost:4200`.
3. Buy one corn successfully.
4. Try again before the window expires and confirm the UI shows the `429` message and countdown.
5. Generate a new client id from the UI and confirm the purchase counters reset locally.
6. Call `POST /api/corn/buy` without `clientId` and confirm the API returns `400`.
