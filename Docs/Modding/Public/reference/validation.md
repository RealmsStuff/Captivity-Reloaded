# Validation and errors

The loader isolates invalid external packs and continues loading unrelated valid packs. Validation should identify the pack, file, JSON path, and reason. An omitted optional asset field uses its documented inherited fallback. An explicitly referenced asset must exist and pass validation; an invalid explicit patch retains the previously resolved asset.

Check the title-screen Mods panel first for loaded, disabled, conflicting, and invalid pack state. When gameplay begins, every distinct validation error is queued as a temporary top-left status card using the same UI and animation as Captivity's normal player-status notifications. The alert includes the validation code, message, and source filename; mod-loader validation no longer emits duplicate red Unity Console errors.

For the exact omission, fallback, unknown-field, ID, and migration contract, see [Defaults and compatibility](defaults-and-compatibility.md).

## Validate before launching the game

From a source checkout, validate all repository examples with:

```powershell
dotnet run --project Tools/ModSchemaValidator -- .
```

To validate one unpacked pack, pass its directory as the second argument:

```powershell
dotnet run --project Tools/ModSchemaValidator -- . Mods/example.my-mod
```

This checks manifests, loader-owned content documents, Tiled maps, and external JSON/TSJ tilesets against the published Draft 2020-12 schemas. It catches structural errors without starting Unity. It does not replace the in-game validation pass, which resolves dependencies, content IDs, Core-template compatibility, final asset containment, and relationships between files.
