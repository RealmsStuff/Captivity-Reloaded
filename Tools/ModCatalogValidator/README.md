# Mod catalog validator

This tool validates the community catalog schema, downloads every declared GitHub Release archive, verifies its exact byte count and SHA-256 digest, checks ZIP paths and extensions, compares manifest identity/dependencies/conflicts with the catalog, and runs the normal Mod API schema validator over the extracted packs.

From the project root:

```powershell
dotnet run --project Tools/ModCatalogValidator -- . ModCatalog/catalog-v1.json
```

The command requires network access because release archives and catalog preview PNGs are deliberately checked at contribution time. Downloads are bounded to the same archive and preview limits enforced by the game.
