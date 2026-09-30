# Mod schema validator

This repository-local tool validates all public content documents, Tiled maps, and external JSON/TSJ tilesets under `ExampleMods` against the Draft 2020-12 schemas in `Docs/Modding/schemas`.

Run from the repository root:

```powershell
dotnet run --project Tools/ModSchemaValidator -- .
```

NuGet packages restore to `Temp/NuGetPackages`, inside the project workspace. The tool does not scan installed player mods and does not replace the runtime's cross-file validation. Unity `.meta` sidecars are ignored explicitly, including on Windows filesystems whose wildcard matching may treat names such as `manifest.json.meta` as JSON matches.

The `mod-schema-validation.yml` GitHub Actions workflow runs the same command when schemas, public examples, or this validator change.
