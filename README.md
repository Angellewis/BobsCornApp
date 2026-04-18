# BobsCornApp

Bob's Corn is a small full-stack app where each client can successfully buy at most 1 corn per rolling minute.

## Repository structure

- `BobsCornApp-Back`: ASP.NET Core Web API using a layered architecture.
- `BobsCornApp-Client`: Angular client for buying corn and displaying rate-limit feedback.

## Backend overview

The backend is organized into the following projects:

- `BobsCornApp.Api`: controllers, HTTP pipeline, Swagger/OpenAPI, CORS, configuration.
- `BobsCornApp.Application`: DTOs, service layer, interfaces, options, AutoMapper profiles.
- `BobsCornApp.Domain`: domain entities.
- `BobsCornApp.Infrastructure`: Dapper repository and SQL Server access.
- `BobsCornApp.UnitTests`: MSTest test project for API, application, and repository behavior.

### Business rule

Each client can buy at most 1 corn per 60 seconds.

- Successful purchase: `200 OK`
- Rate limit exceeded: `429 Too Many Requests`

Client identity is resolved in this order:

1. `X-Client-Id` request header
2. Remote IP address

## Main endpoint

`POST /api/corn/buy`

Successful response example:

```json
{
  "corn": "\ud83c\udf3d",
  "message": "Corn purchased successfully.",
  "purchasedAtUtc": "2026-04-18T18:00:00+00:00"
}
```

Rate-limited response example:

```json
{
  "message": "Rate limit exceeded. Clients can buy at most 1 corn per minute.",
  "retryAfterSeconds": 60
}
```

The API also sets the `Retry-After` response header on `429` responses.

## Backend prerequisites

- .NET 9 SDK
- SQL Server Express or another SQL Server instance reachable from your machine

## Backend configuration

The backend uses `BobsCornApp-Back/BobsCornApp.Api/appsettings.Development.json`.

Current relevant settings:

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
- The repository creates the `dbo.CornPurchases` table automatically if it does not exist.
- The database itself must already exist, or SQL Server must be configured to allow creating it through your chosen connection and workflow.

## Running the backend

From the repository root:

```powershell
dotnet build BobsCornApp-Back\BobsCornApp-Back.sln -m:1
dotnet run --project BobsCornApp-Back\BobsCornApp.Api --launch-profile http
```

Default backend URLs:

- HTTP: `http://localhost:5224`
- HTTPS profile: `https://localhost:7115`
- IIS Express SSL profile: `https://localhost:44354`

Swagger UI is available in Development at:

- `http://localhost:5224/swagger`
- `https://localhost:7115/swagger`
- `https://localhost:44354/swagger` when using IIS Express

## Frontend prerequisites

- Node.js
- npm

## Running the frontend

From `BobsCornApp-Client`:

```powershell
npm install
npm start
```

The Angular development server runs on:

- `http://localhost:4200`

### Important frontend/backend note

The Angular development environment currently points to:

```ts
apiUrl: "https://localhost:44354"
```

That means the frontend is configured to call the backend through the IIS Express SSL profile by default.

You have two valid ways to run the app locally:

1. Run the backend through Visual Studio/IIS Express so it is available at `https://localhost:44354`.
2. Keep using `dotnet run` and update `BobsCornApp-Client/src/environments/environment.development.ts` to match the backend URL you are actually using, such as `http://localhost:5224` or `https://localhost:7115`.

## Running tests

Backend tests:

```powershell
dotnet test BobsCornApp-Back\BobsCornApp-Back.sln -m:1
```

Frontend tests:

```powershell
cd BobsCornApp-Client
npm test
```

## Implementation details

- Rate limiting is enforced in the application service layer.
- Persistence is implemented with Dapper in the infrastructure layer.
- AutoMapper is used for API response mapping.
- Swagger is enabled in Development.
- CORS is configured to allow the Angular client at `http://localhost:4200`.

## Manual test flow

1. Start the backend.
2. Open Swagger.
3. Call `POST /api/corn/buy` with a fixed `X-Client-Id`.
4. Call the same endpoint again within 60 seconds.
5. Confirm the second request returns `429`.
