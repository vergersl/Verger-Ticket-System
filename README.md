# Verger Naturals — IT Service Desk (ASP.NET Core)

An ASP.NET Core 8 (MVC + Identity + EF Core) port of the IT support ticket
platform and Digital & IT KPI dashboard: ticket workflow with SLA tracking,
role-based access (Requester / Technician / Admin), a KPI Library seeded from
the Digital & IT KPI Master Sheet, dashboards/reports, and CSV export.

## ⚠️ Before you run this

This project could **not** be compiled or run in the sandbox that generated
it — that environment has no access to nuget.org, only to the .NET SDK
itself, so none of the NuGet packages below could be restored. The code
follows standard, well-established ASP.NET Core / EF Core / Identity
patterns, but **you are the first real compiler pass this has had.** Run
`dotnet build` yourself before relying on it, and expect to fix the odd
typo or namespace issue.

## Requirements

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- The `dotnet-ef` tool, for the one-time migration step below:
  ```bash
  dotnet tool install --global dotnet-ef
  ```

## First-time setup

```bash
cd VergerITDesk
dotnet restore

# Required — there are no migrations checked in, so the database has no
# tables until you generate and this creates them:
dotnet ef migrations add InitialCreate
dotnet ef database update

dotnet run
```

Then open the URL `dotnet run` prints (typically `https://localhost:5001`),
register an account, and sign in. **The very first account to register
automatically becomes Admin** (mirroring the original artifact's bootstrap
behavior), so someone can start promoting others to Technician/Admin from
the Team & Roles page. Everyone who registers after that starts as a
Requester.

## Switching from SQLite to SQL Server

The app defaults to a local SQLite file (`vergeritdesk.db`) so it runs with
zero external setup. To use SQL Server instead:

1. In `VergerITDesk.csproj`, replace `Microsoft.EntityFrameworkCore.Sqlite`
   with `Microsoft.EntityFrameworkCore.SqlServer`.
2. In `Program.cs`, change `options.UseSqlite(...)` to
   `options.UseSqlServer(...)`.
3. Update `ConnectionStrings:DefaultConnection` in `appsettings.json` to a
   SQL Server connection string.
4. Delete the `Migrations/` folder (if any exists from the SQLite run) and
   re-run `dotnet ef migrations add InitialCreate`.

## What's implemented

- **Auth & roles** — ASP.NET Core Identity (cookie auth). Three app roles:
  `Requester`, `Technician`, `Admin` (see `Models/Enums.cs`). Role checks are
  enforced server-side via `[Authorize(Policy = "TechnicianOrAdmin")]` /
  `"AdminOnly"` on controllers — not just hidden in the UI.
- **Tickets** — create (with mandatory Business Impact / Savings and
  optional USD value, per the original spec), list (Requesters only ever see
  their own; Technicians/Admins see all), detail/manage page with status
  workflow: Open → In Progress → **Awaiting Info** (pauses the SLA clock,
  tracked via `PausedTicks` on the `Ticket` entity) → Resolved/Closed, plus
  Reopen and Resume. "Request more info" logs an activity entry and hands
  back a `mailto:` link — there's no real email server behind it, same as
  the original.
- **KPI Library** — the 20 Digital & IT KPIs seeded from
  `Services/KpiCatalog.cs` on first run. IT.3.5 (SLA) and IT.3.6
  (first-contact resolution) are computed live from ticket data; the rest
  are entered manually per fiscal month (Apr–Mar) via `KpiActual` rows.
- **Dashboard / Reports** — aggregate stats, 14-day and monthly
  raised/resolved charts (plain CSS bars, no JS chart library dependency),
  and a CSV export of every ticket.
- **Team & Roles** (Admin only) — lists every registered user and lets an
  Admin reassign their role.

## What's deliberately simplified vs. a production system

- **Sequence numbers** (`TKT-0001` etc.) are assigned by reading the current
  max and adding one — fine at small scale, but not safe under heavy
  concurrent ticket creation. For real concurrency safety, move this to a
  database sequence/identity-based approach or wrap it in a transaction with
  retry.
- **No email actually sends** — "Request more info" opens a `mailto:` draft
  in the technician's own mail client, same limitation as the original.
  Wiring up real outbound email (e.g. via SMTP or an email API) would live
  in `TicketsController.RequestInfo`.
- **No file attachments** on tickets.
- Client-side validation scripts (jQuery unobtrusive validation) aren't
  wired up — form validation works (server-side `ModelState` still
  validates and re-renders errors), but there's no live in-browser feedback
  before submit. Add `_ValidationScriptsPartial.cshtml` + the jQuery
  validation scripts to `Views/Shared/_Layout.cshtml` if you want that.

## Project layout

```
Controllers/     MVC controllers — one per feature area
Models/          EF Core entities + Models/ViewModels for forms/pages
Data/            DbContext + startup seeding (roles, KPI catalog)
Services/        KPI catalog (seed data) + ticket metrics calculations
Views/           Razor views, grouped by controller
wwwroot/         site.css (ported from the original design) + the logo

```
