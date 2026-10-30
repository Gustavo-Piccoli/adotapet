# CLAUDE.md

Guide for Claude Code in this repository.

## Overview

AdotaPet is a pet adoption site (portfolio project). Shelters ("ONGs" in the UI) publish pets; adopters send adoption requests; shelters approve or reject. Backend: ASP.NET Core (.NET 10) with Clean Architecture. Frontend: Angular 21 (`web/`). Database: SQL Server. Design doc: `docs/superpowers/specs/2026-10-30-adotapet-design.md`.

**The backlog (technical debts, future features, improvements) lives only in `docs/BACKLOG.md` — read and update it there; never duplicate it here.**

## Commands

```bash
dotnet build AdotaPet.sln                                   # must stay at 0 warnings (TreatWarningsAsErrors)
dotnet test tests/AdotaPet.Domain.Tests                     # unit tests, no Docker
dotnet test tests/AdotaPet.Application.Tests
dotnet test tests/AdotaPet.Infrastructure.Tests
dotnet test tests/AdotaPet.Api.IntegrationTests             # needs Docker (Testcontainers + SQL Server)
dotnet run --project src/AdotaPet.Api                       # API on http://localhost:5080 (Scalar at /scalar)

# EF Core (dotnet-ef is a local tool: dotnet tool restore)
dotnet ef migrations add <Name> -p src/AdotaPet.Infrastructure -s src/AdotaPet.Api
dotnet ef database update -p src/AdotaPet.Infrastructure -s src/AdotaPet.Api

# Web (from web/)
npm ci
npm start             # dev server on http://localhost:4200, proxies /api to 5080
npx ng lint
npm test              # ng test --no-watch
npx ng build

docker compose up --build   # sqlserver:1433, api:5080, web:4200
```

Use `npx @angular/cli@21` rather than a global Angular CLI. Shell on the dev machine is Git Bash on Windows.

## Architecture and conventions

- Projects under `src/`: Domain (no NuGet packages) <- Application <- Infrastructure <- Api. Tests under `tests/` mirror them, plus `AdotaPet.Api.IntegrationTests`.
- `net10.0`, `Nullable` and `ImplicitUsings` enabled, `TreatWarningsAsErrors` in `Directory.Build.props`.
- Feature folders first (`Pets/`, `AdoptionRequests/`, `Shelters/`, `Adopters/`, `Accounts/`, `Common/`), technical aspect as subfolder (`UseCases/`, `Dtos/`, `Persistence/`...).
- Use cases: `I<Name>UseCase` + `<Name>UseCase` in the same file under `<Feature>/UseCases/`, single method `ExecuteAsync(...)`, registered explicitly (Scoped) in `AddApplication()`. Input DTOs are `sealed record`s. No mediator.
- Entities validate themselves and throw domain exceptions; the API maps them to `ProblemDetails` (400/404/409).
- Paging: `page` >= 1, `pageSize` default 12, clamped to 1..48.
- Photos: JPEG/PNG/WebP detected by file signature, max 5 MB, server-generated GUID file name.
- JWT lifetime 60 minutes, no refresh token. Claims: `sub`, `email`, `role` (`Shelter` | `Adopter`), `profile_id`, `name`.
- Access to another shelter's pet or request returns 404, never 403. Adopter on shelter endpoints returns 403.
- Code, identifiers, DB names and commit messages in English; all user-facing UI strings in Brazilian Portuguese (pt-BR). "Shelter" is shown as "ONG".
- Nothing from PROCERGS/SEFAZ code, names or concepts.
- Development seed (`DevelopmentSeeder`) runs only in the `Development` environment; demo accounts use password `Adota123`.

## Git

- Conventional-commit messages (`feat:`, `fix:`, `test:`, `chore:`, `docs:`).
- **Commits never include AI co-authorship** (no `Co-Authored-By` or any other attribution), including in PR descriptions.
- Create the GitHub repository and push only after Gustavo confirms.
