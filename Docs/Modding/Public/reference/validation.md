# Validation and errors

The loader isolates invalid external packs and continues loading unrelated valid packs. Validation should identify the pack, file, JSON path, and reason. An omitted optional asset field uses its documented inherited fallback. An explicitly referenced asset must exist and pass validation; an invalid explicit patch retains the previously resolved asset.

Check the title-screen Mods panel first for loaded, disabled, conflicting, and invalid pack state. When gameplay begins, every distinct validation error is queued as a temporary top-left status card using the same UI and animation as Captivity's normal player-status notifications. The alert includes the validation code, message, and source filename; mod-loader validation no longer emits duplicate red Unity Console errors.

For the exact omission, fallback, unknown-field, ID, and migration contract, see [Defaults and compatibility](defaults-and-compatibility.md).

## Which source is authoritative?

For a particular game build, its runtime parser and the schemas from the matching source tag or commit are authoritative. The public wiki normally tracks the current supported API, but a development wiki can be newer than an installed release and an old downloaded template can be older than both. Always record the game version or source commit used for validation.

`schemaVersion` selects the shape of one document and `modApiVersion` selects the broad API line. While Mod API v1 remains in freeze review, neither value makes an experimental field from an unrelated development revision valid. A field becomes usable only when it exists in the matching schema, parser, runtime implementation, and reference documentation.

Unknown fields fail closed. The loader does not ignore a plausible-looking property, because silently ignoring gameplay configuration can produce a mod that appears installed while behaving differently from its description.

To look up a specific diagnostic such as `manifest.id` or `bundle.integrity`, use the generated [validation/error-code catalog](error-codes.md). The catalog is grouped by the prefix before the first period and is searchable by code, message text, or source file.

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

For the release artifact, also use **Captivity Reloaded > Modding > Captivity Mod Packager > Validate Mod** and install the resulting `.capmod` in the target player build. A JSON parser reporting “valid JSON” only confirms syntax; it does not validate Captivity fields, enums, event names, cross-file references, or runtime support.

## Generated and AI-assisted content

Treat generated JSON exactly like handwritten code:

1. Confirm that every property appears on the correct definition in the current [field reference](api-fields.md).
2. Do not transfer a property between content types. For example, a Tiled interaction property is not automatically a valid enemy-attack property.
3. Confirm every event name against the documented allowlist for that event track.
4. Run structural validation, the Unity packager validation, and the target player installation test.
5. Reject generated instructions that reference tools, source files, menus, or schemas that are absent from the matching checkout.

Plausible names such as `blendSeconds`, `pairing`, or `hitReaction` are not extension points. Loader-owned documents reject undocumented properties even when the intended behavior seems obvious.
