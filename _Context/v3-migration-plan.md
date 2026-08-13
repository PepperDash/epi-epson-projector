# v3 Migration Plan — epi-epson-projector

> Generated 2026-08-13 by essentials-epi agent (read-only `scan-plugin-v3-readiness.ps1`, Mode V3). Re-run the scan before starting work in case the repo has drifted since this was written.
> Work happens on branch `feature/v3-migration`.

## Program Context

This repo is 1 of 5 EPIs being converted to Essentials v3 in this effort:
`epi-videoCodec-ciscoExtended`, `epi-cisco-cli`, `epi-crestron-nvx`, `epi-epson-projector`, `epi-netgear-cli`.
**This one is the pilot** — smallest/lowest-risk, done first to validate the pattern before the others.
`epi-videoCodec-ciscoExtended` is on hold until its in-progress feature branches are merged to `main`.

## Current State

| | |
|---|---|
| Target framework | `net472` |
| Essentials version | `2.4.7` |
| `SERIES4` define | present (both Debug/Release `PropertyGroup`s) |
| C# files | 18 |
| `#if SERIES4` conditionals | 0 |
| Removed .NET APIs | 0 |
| Factory | 1 — `src\DeviceFactory.cs`, class `DeviceFactory`, `MinimumEssentialsFrameworkVersion = "2.4.7"` |
| 3-Series artifacts | `packages.config` |
| Debug.Console remaining | 0 (already 12 calls migrated to Serilog) |

## Risk: **Low**

No conditionals, no removed APIs, single factory, small file count, logging already mostly migrated.

## Skill to Use

[`sub-agents/essentials-epi/skills/migrate-v2-to-v3/SKILL.md`](../../../skills/migrate-v2-to-v3/SKILL.md) — standard 5-phase workflow. No routing migration needed (no `IRouting`/`IMatrixRouting` usage detected).

## Pre-Flight (do first, every session — per essentials-epi's mandatory git pre-flight rule)

1. `git fetch`, confirm branch is `feature/v3-migration`, confirm 0 behind upstream, confirm clean working tree.
2. Confirm this branch is still correctly based on an up-to-date `main`.

## Task Checklist

- [ ] Re-run `analyze-plugin` scan to confirm nothing changed since this plan was written
- [ ] Phase 2a: `<TargetFramework>net472</TargetFramework>` → `net8` in `src\epi-epson-projector.4Series.csproj`
- [ ] Phase 2a: remove both `SERIES4` `DefineConstants` `PropertyGroup` blocks
- [ ] Phase 2a: bump `PepperDashEssentials` PackageReference to `3.0.0`
- [ ] Phase 2b: `MinimumEssentialsFrameworkVersion = "2.4.7"` → `"3.0.0"` in `DeviceFactory.cs`
- [ ] Phase 2c: delete `packages.config`
- [ ] Phase 2c: check for a stray 3-Series `.csproj`/`.sln` (none detected in this scan, but re-verify)
- [ ] Phase 3a: no `#if SERIES4` conditionals to remove (verify still true)
- [ ] Phase 3b: no `Debug.Console` calls to migrate (verify still true — grep to confirm 0 remaining)
- [ ] Phase 3c: check `Initialize()`/`CustomActivate()` visibility (`public` → `protected override`) and any external callers
- [ ] Phase 4: verify join map — all fields `public`, `base(joinStart, typeof(...))`, `[JoinName]` attributes match
- [ ] Phase 5: `dotnet restore` + `dotnet build` on `epi-epson-projector.4Series.sln`, fix any compile errors
- [ ] Update README's "Minimum Essentials Framework Versions" section if present
- [ ] Commit with `feat!:` / `BREAKING CHANGE:` footer (major version bump)
- [ ] Verify `output/` `.cplz` builds

## Notes / Flags

- None outstanding — cleanest of the 5 repos.
