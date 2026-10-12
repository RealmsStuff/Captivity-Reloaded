# Building and testing

Use the smallest check that covers the change, then run the broader gates before a release.

## Validate the wiki

```powershell
powershell -ExecutionPolicy Bypass -File .\Tools\Documentation\Validate-Wiki.ps1
```

This checks public navigation coverage, relative Markdown links, literal repository paths, and README coverage for every example pack.

## Validate schemas and examples

From the repository root:

```powershell
dotnet run --project Tools/ModSchemaValidator -- .
```

This scans repository example manifests, content documents, Tiled maps, and external JSON/TSJ tilesets. The first run may restore NuGet packages into `Temp/NuGetPackages`.

The catalog validator requires network access because it downloads and verifies release archives and previews:

```powershell
dotnet run --project Tools/ModCatalogValidator -- . ModCatalog/catalog-v1.json
```

## Run Unity tests

Use Unity's Test Runner for local iteration. For a release-style batch run:

```powershell
powershell -ExecutionPolicy Bypass -File .\Tools\Release\Invoke-UnityTestSuites.ps1 -Label local
```

The script runs EditMode and PlayMode suites, rejects missing output and zero-test runs, and writes results below `TestResults/Release/local` with logs below `Logs/Release/local`.

If Unity is not in the expected sibling directory, provide `-UnityExe` with the full path to `Unity.exe`. Batch tests require a valid Unity license.

## Run the release audit

The audit is read-only:

```powershell
powershell -ExecutionPolicy Bypass -File .\Tools\Release\Invoke-ReleaseAudit.ps1
```

It checks source cleanliness, generated files, Unity platform modules, test-suite presence, example discovery, stress-test content, catalog configuration, and licensing gates. A development worktree can legitimately fail the clean-source gate; release candidates cannot.

## Build players

`Assets/Editor/BuildReleasePlayers.cs` exposes these Unity execute methods:

- `BuildReleasePlayers.BuildWindows`
- `BuildReleasePlayers.BuildAndroid`
- `BuildReleasePlayers.BuildWebGL`
- `BuildReleasePlayers.BuildAll`

Build one platform at a time during iteration. `BuildAll` is for a deliberate release build with all required Unity modules installed. Outputs go below `Builds` and are not source-controlled.

Example command-line shape:

```powershell
& "C:\Path\To\Unity.exe" -batchmode -quit -projectPath "$PWD" -executeMethod BuildReleasePlayers.BuildWindows -logFile "Logs\windows-build.log"
```

## Release order

1. Validate schemas and examples.
2. Run complete EditMode and PlayMode suites.
3. Run the read-only release audit from a clean candidate.
4. Build Windows, Android, and WebGL from the same commit.
5. Perform the platform and catalog matrix in the [release verification runbook](../reference/release-verification-runbook.md).
6. Archive logs, test XML, build identity, and the source commit.

A successful compile is not a substitute for gameplay, save, platform, or licensing verification.
