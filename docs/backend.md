# NexaStore backend

The backend is a .NET 10 modular monolith split into Domain, Application,
Infrastructure, and API projects. PostgreSQL is the transactional source of
truth for catalog, customer, cart, order, and inventory data.

## Configuration

Set the connection string outside source control:

```powershell
$env:ConnectionStrings__NexaStore = "Host=localhost;Port=5432;Database=nexastore;Username=nexastore;Password=NexaStore_Local_2026!"
```

Development startup applies EF Core migrations and idempotent seed data. For
other environments, apply migrations as a deployment step or explicitly set
`Database__ApplyMigrationsOnStartup=true`.

## Run with .NET

```powershell
dotnet restore NexaStore.sln
dotnet run --project backend/NexaStore.Api/NexaStore.Api.csproj
```

Swagger is available at `http://localhost:5175/swagger` in Development.

Health endpoints:

- `/health/live` verifies that the process is running.
- `/health/ready` verifies PostgreSQL connectivity.
- `/health` retains the combined health endpoint.

## Run with Docker

Copy `.env.example` to `.env`, replace the development password, then run:

```powershell
docker compose up --build
```

The API is exposed at `http://localhost:5175`. PostgreSQL data is retained in
the `nexastore-postgres` named volume.

## Verification

```powershell
dotnet build NexaStore.sln
dotnet test NexaStore.sln
dotnet tool run dotnet-ef migrations has-pending-model-changes `
  --project backend/NexaStore.Infrastructure/NexaStore.Infrastructure.csproj `
  --startup-project backend/NexaStore.Infrastructure/NexaStore.Infrastructure.csproj
```

Order creation and cancellation run within resilient database transactions.
Inventory and orders use PostgreSQL's `xmin` system column for optimistic
concurrency. No credentials are stored in committed configuration files.
