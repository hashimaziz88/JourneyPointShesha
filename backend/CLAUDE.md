# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

---

## Project Overview

**JourneyPoint** backend — built on the **Shesha Framework** (v0.0.0-build105662), which layers on top of ABP.io 9.0 + ASP.NET Core 8 + **NHibernate** + FluentMigrator.

**Database:** SQL Server `localhost,1433` · DB `JourneyPoint` · `sa` / `@123Shesha`  
**API Port:** `http://localhost:21021` · Swagger at `/swagger`

---

## Commands

```bash
# Build solution
dotnet build Hashim.JourneyPoint.sln

# Run backend API
dotnet run --project src/Hashim.JourneyPoint.Web.Host/Hashim.JourneyPoint.Web.Host.csproj

# Run all tests
dotnet test test/Hashim.JourneyPoint.Common.Domain.Tests/Hashim.JourneyPoint.Common.Domain.Tests.csproj

# Run a single test (by name filter)
dotnet test --filter "FullyQualifiedName~MyTestMethodName"

# Clean bin/obj folders
./Delete-BIN-OBJ-Folders.bat
```

---

## Solution Structure

```
src/
  Hashim.JourneyPoint.Domain/       # Entities, enums, migrations, NHibernate config
  Hashim.JourneyPoint.Application/  # AppServices, DTOs, business logic orchestration
  Hashim.JourneyPoint.Web.Core/     # SignalR, Hangfire, JWT auth, Swagger setup
  Hashim.JourneyPoint.Web.Host/     # Entry point: Program.cs, Startup.cs, appsettings.json
test/
  Hashim.JourneyPoint.Common.Domain.Tests/
```

---

## Architecture

### Layer Rules

The dependency chain is strictly one-directional:

```
Web.Host → Web.Core → Application → Domain
```

ABP modules declare this via `[DependsOn(...)]` attributes. `SheshaWebHostModule` (Web.Host) depends on `JourneyPointWebCoreModule`, which in turn depends on both `JourneyPointModule` (Domain) and `JourneyPointApplicationModule`.

### ORM: NHibernate (not EF Core)

Shesha uses **NHibernate**, not EF Core. All entity properties **must be `virtual`** for NHibernate proxy generation. Never use EF-style fluent configuration or migrations.

### Migrations: FluentMigrator (not EF migrations)

All schema changes go in `Domain/Migrations/` using Shesha's `OneWayMigration` base class. Migrations have no `Down()` — they are one-way. Always use idempotent SQL with `IF EXISTS` guards.

```csharp
[Migration(20260409111300)]   // timestamp: YYYYMMDDHHmmSS
public class M20260409111300 : OneWayMigration
{
    public override void Up()
    {
        Alter.Table("Core_Persons").InSchema("dbo")
            .AddColumn("JourneyPoint_FieldName").AsString(500).Nullable();
    }
}
```

---

## Domain Entity Patterns

### Entity Declaration

Every entity needs `[Entity(TypeShortAlias = "JourneyPoint.X")]` — this alias is used by Shesha's dynamic API and form engine.

```csharp
[Entity(TypeShortAlias = "JourneyPoint.MyEntity")]
public class MyEntity : FullAuditedEntity<Guid>
{
    public virtual string Name { get; set; }   // virtual required
}
```

**Base classes:**
- `Person` — for human entities (extends `Core_Persons` table)
- `Organisation` — for org entities
- `FullAuditedEntity<Guid>` — full audit trail + soft delete
- `CreationAuditedEntity<Guid>` — append-only records (logs, snapshots)

### Column Naming on Shared Shesha Tables

When extending `Person` or other Shesha base tables (e.g. `Core_Persons`), always prefix custom columns with `JourneyPoint_`:

```
JourneyPoint_MembershipNumber
JourneyPoint_MembershipStatusLkp
```

### Reference Lists (Enums)

Shesha enums map to its reference list system. Prefix enum class names with `RefList` and decorate with `[ReferenceList]`:

```csharp
[ReferenceList("JourneyPoint", "MyStatuses")]
public enum RefListMyStatuses : long
{
    Active = 1,
    Inactive = 2,
}
```

Use `RefListMyStatuses?` (nullable) on entity properties decorated with `[ReferenceList("JourneyPoint", "MyStatuses")]`.

---

## AppService Pattern

AppServices extend `SheshaAppServiceBase` and auto-generate REST endpoints via ABP. DTOs live in a `Dtos/` subfolder next to the AppService.

```
Application/Services/
  MemberAppService.cs
  Dtos/
    CreateMemberInput.cs
    MemberDto.cs
```

Use `MapToDynamicDtoAsync<TEntity, TId>()` (from `SheshaAppServiceBase`) to return Shesha dynamic DTOs instead of manually mapping to static DTOs where possible.

Repositories are injected as `IRepository<TEntity, Guid>` — never use a DbContext directly.

---

## Infrastructure Details

| Concern | Implementation |
|---|---|
| Authentication | JWT Bearer (symmetric HMAC SHA256, 5-day expiry, configured in `AuthConfigurer.cs`) |
| Background jobs | Hangfire (dashboard at `/hangfire`) |
| Error logging | ElmahCore (logs to `App_Data/ElmahLogs`, UI at `/elmah`) |
| Application logging | Log4Net (`log4net.config`) |
| GraphQL | Shesha GraphQL (playground at `/ui/playground`) |
| Multi-DB | `DbmsType` in `appsettings.json` — `"SQLServer"` or `"PostgreSQL"` |
| Testing | xUnit + Shouldly + Moq/NSubstitute |

CORS is fully open in development (`appsettings.json` → `App.CorsOrigins`). The admin portal runs on `:3000` and public portal on `:3005`.

---

## Key Shesha Gotchas

- **Never use EF Core APIs** — no `DbContext`, `IDesignTimeDbContextFactory`, or EF migrations.
- **Never remove `virtual` from entity properties** — NHibernate proxies will break silently.
- **Admin UI configuration** (forms, navigation, permissions) is stored in the database via Shesha's form designer, not in source files. Don't look for page layout code in the repo.
- The `[Entity(TypeShortAlias = ...)]` attribute must be unique across the solution — Shesha uses it as a global type discriminator.
