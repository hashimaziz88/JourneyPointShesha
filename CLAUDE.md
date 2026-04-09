# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

---

## Project Overview

**JourneyPoint** — Intelligent HR Onboarding Platform built on the **Shesha Framework** (v0.30.1).
Shesha is a low-code platform layered on top of ABP.io + ASP.NET Core 8 + NHibernate + Next.js 14.
Full framework docs: https://docs.shesha.io/docs/get-started/Introduction

---

## Development Servers

| Service | URL | Notes |
|---|---|---|
| Backend API | http://localhost:44362 | ASP.NET Core + Swagger at `/swagger` |
| Admin Portal | http://localhost:3000 | Next.js (Shesha admin UI) |
| Public Portal | http://localhost:3005 | Next.js (enrolee-facing) |

**Database:** SQL Server `localhost,1433` · DB `JourneyPoint` · `sa` / `@123Shesha`

---

## Commands

### Backend
```bash
# From backend/
dotnet build Hashim.JourneyPoint.sln
dotnet run --project src/Hashim.JourneyPoint.Web.Host/Hashim.JourneyPoint.Web.Host.csproj
```

### Admin Portal
```bash
# From adminportal/
npm install
npm run dev          # dev server on :3000
npm run build
npm run lint
npm run test         # Jest (config: test/jest.config.js)
npm run prettier     # format all .ts/.tsx files
```

### Public Portal
```bash
# From publicportal/
npm install
npm run dev
npm run build
npm run lint
npm run test
```

### Shared Frontend Packages
```bash
# From frontend-packages/
npm install
npm run build        # OS-aware: uses build:win32 on Windows, build:default on Unix
npm run build:watch
npm run check-types  # tsc only
npm run lint         # tsc + eslint + stylelint
```

---

## Architecture

### Repository Layout

```
backend/                        # ASP.NET Core solution
  src/
    Hashim.JourneyPoint.Domain/         # Entities, migrations, enums
    Hashim.JourneyPoint.Application/    # AppServices, DTOs
    Hashim.JourneyPoint.Web.Core/       # SignalR, Hangfire, infrastructure
    Hashim.JourneyPoint.Web.Host/       # Startup, appsettings, API host
adminportal/                    # Next.js 14 Shesha admin portal (src/app/)
publicportal/                   # Next.js 14 public/enrolee portal
frontend-packages/              # Shared Rollup component library (@shesha-io/packages)
```

### Backend: Shesha-Specific Patterns

Shesha uses **NHibernate** (not EF Core) and **FluentMigrator** (not EF migrations). All entity properties must be `virtual`.

**Entity declaration:**
```csharp
using Shesha.Domain;
using Shesha.Domain.Attributes;

[Entity(TypeShortAlias = "JourneyPoint.MyEntity")]  // required — used by Shesha's dynamic API
public class MyEntity : FullAuditedEntity<Guid>      // or Person, Organisation, etc.
{
    public virtual string Name { get; set; }         // virtual required for NHibernate proxying

    [ReferenceList("JourneyPoint", "MyStatuses")]    // for reference-list (enum-like) properties
    public virtual RefListMyStatuses? Status { get; set; }
}
```

**Key Shesha base classes to extend:**
- `Person` — for human entities (adds name, contact, address fields)
- `Organisation` — for org entities
- `FullAuditedEntity<Guid>` — full audit trail + soft delete
- `CreationAuditedEntity<Guid>` — append-only records

**Column naming on Shesha base tables** (e.g. extending `Core_Persons`):
Always prefix with `JourneyPoint_` e.g. `JourneyPoint_MembershipNumber`.

**Migrations** use Shesha's FluentMigrator wrapper:
```csharp
[Migration(20260409111300)]          // timestamp: YYYYMMDDHHmmSS
public class M20260409111300 : OneWayMigration
{
    public override void Up()
    {
        // Use idempotent SQL (check IF EXISTS before altering)
        Alter.Table("Core_Persons").InSchema("dbo")
            .AddColumn("JourneyPoint_FieldName").AsString(500).Nullable();
    }
}
```
Migrations live in `Hashim.JourneyPoint.Domain/Migrations/`. Naming: `M{timestamp}.cs`.

**Reference lists (enums):**
```csharp
// Domain/Enums/RefListMyStatuses.cs
[ReferenceList("JourneyPoint", "MyStatuses")]
public enum RefListMyStatuses : long
{
    Active = 1,
    Inactive = 2,
}
```

### Backend: ABP/AppService Layer

AppServices auto-generate REST endpoints via ABP. DTOs live in a `Dtos/` subfolder next to the AppService.

```
Application/
  Hires/
    HireAppService.cs
    Dtos/
      CreateHireRequest.cs
      HireDto.cs
```

### Frontend: Admin Portal Structure

The admin portal is a thin Shesha shell. Most UI is configured dynamically in Shesha's form designer — not in code. Custom code lives in:
```
adminportal/src/
  app/                  # Next.js App Router pages
    (main)/             # Authenticated layout
      dynamic/[...path] # Shesha dynamic page renderer — DO NOT modify
      shesha/           # Shesha built-in settings pages
  components/           # Custom React components
  app-constants/        # Permission names, messages, constants
  enums/
  models/
  utils/
```

Custom pages are added under `src/app/(main)/` and linked via Shesha's navigation config (in the DB/admin UI, not in code).

**Key frontend dependency:** `@shesha-io/reactjs` — provides all Shesha React components, form engine, and data hooks. Import from this package rather than building custom data-fetching for Shesha entities.

### Shared Component Library (`frontend-packages/`)

Built with Rollup, published to Azure DevOps npm registry (`pkgs.dev.azure.com/boxfusion/...`). Import as `@shesha-io/packages` in portals. Add components here when they are needed in both portals.

---

## Key Conventions

- Entity properties are always `virtual` (NHibernate requirement)
- Every domain entity needs `[Entity(TypeShortAlias = "JourneyPoint.X")]`
- Migrations are always `OneWayMigration` and must be idempotent (use `IF EXISTS` guards)
- Reference list enums are prefixed `RefList` and decorated with `[ReferenceList("JourneyPoint", "...")]`
- Custom columns on shared Shesha tables use the `JourneyPoint_` prefix
- Admin portal UI configuration (forms, navigation, permissions) is stored in the database and managed through Shesha's admin interface — not in source files
