# Draft Mod API v1 contract

Status: Draft 0.2. Manifest, ID, dependency, discovery, Core catalogs, save compatibility and the typed asset-slot resolver are implemented. External asset decoding, content-type schemas and gameplay factories remain draft work.

## Pack location

Desktop builds discover one pack per immediate child directory of `Mods` beside the game executable:

```text
Captivity Reloaded.exe
Mods/
  example.acid-gremlin/
    manifest.json
    content/
    assets/
```

The loader never reads a pack path outside that pack's directory. Absolute paths and `..` traversal are invalid.

## Manifest

Every pack has one UTF-8 `manifest.json`:

```json
{
  "schemaVersion": 1,
  "id": "example.acid-gremlin",
  "displayName": "Acid Gremlin",
  "version": "1.0.0",
  "modApiVersion": 1,
  "authors": ["Example Author"],
  "description": "Adds an Acid Gremlin enemy variant.",
  "dependencies": [
    {
      "id": "core",
      "version": ">=1.0.0"
    }
  ],
  "contentRoots": ["content"]
}
```

`core` is reserved, required and loaded first. External manifests cannot use the `core` pack ID or define content in the `core:` namespace.

Version strings use semantic versioning. The dependency range grammar will be finalized with the parser; v1 must at minimum support an exact version and `>=`.

## IDs

Pack IDs:

- Use lowercase ASCII letters, numbers, `.`, `_` and `-`.
- Begin with a letter or number.
- Do not contain `:` or `/`.
- Are compared ordinally and case-sensitively after lowercase validation.

Content and asset IDs have a namespace and path:

```text
<pack-id>:<category>/<name-or-path>

core:enemy/gremlin
core:stage/field-day
example.acid-gremlin:enemy/acid-gremlin
core:player/body/head/pale
```

The namespace must match the defining pack unless the file is an explicit patch. Path segments use lowercase letters, numbers and hyphens. Once released, an ID is permanent unless the pack supplies a migration alias.

## Pack layout

The recommended v1 layout is:

```text
manifest.json
content/
  enemies/
  clothing/
  items/
  stages/
  rules/
  patches/
assets/
  sprites/
  audio/
```

JSON files are discovered recursively inside declared content roots. Each content file declares its `type`; folder names are organizational and do not determine behaviour.

## Definition envelope

Every definition begins with:

```json
{
  "schemaVersion": 1,
  "type": "enemy",
  "id": "example.pack:enemy/example",
  "displayName": "Example"
}
```

Unknown `schemaVersion`, `type` or required fields invalidate that definition. A pack with invalid required definitions is disabled for the session. The validation report includes the pack, file, JSON path and human-readable reason.

## References and inheritance

Definitions reference other content by ID, never by Unity object name, asset filename or numeric database ID.

An `extends` field inherits from an existing compatible definition:

```json
"extends": "core:enemy/gremlin"
```

V1 permits only fields explicitly marked as overridable for that content type. Engine components, arbitrary C# class names and private prefab hierarchy paths are not valid fields.

## Assets

External paths are forward-slash paths relative to the pack root:

```json
"texture": "assets/sprites/acid-gremlin.png"
```

V1 initially targets PNG sprites and a documented subset of audio formats supported consistently by the selected Unity loaders. Executable files, scripts and arbitrary assemblies are rejected.

Asset replacement targets a stable public slot:

```json
{
  "schemaVersion": 1,
  "type": "assetPatch",
  "id": "example.skin:patch/player-art",
  "target": "core:player",
  "replacements": {
    "body/head/pale": "assets/sprites/head-pale.png"
  }
}
```

No replacement occurs merely because two files share a filename.

## Dependencies and load order

Required dependencies are loaded before the dependent pack. Missing or incompatible required dependencies disable the dependent pack.

After dependencies, ordering is deterministic by pack ID. Patches that target the same field or asset slot are conflicts. V1 reports the conflict and requires explicit user resolution; it must not silently choose based on filesystem enumeration order.

## Core content

Core uses the same logical registry and IDs, but may use Unity prefab adapters and packaged asset references while migration is incomplete. External packs cannot disable the Core pack.

## Error isolation

- Invalid external pack: disable that pack and dependent packs, then continue.
- Missing optional asset with a documented fallback: use the fallback and warn.
- Invalid explicit replacement: retain the previous resolved asset and report the error.
- Invalid Core definition: stop startup with a clear fatal report.

## Save compatibility

Save data uses full content IDs for modded content. Existing vanilla numeric IDs are preserved and mapped to stable `core:` IDs. Disabled or missing content remains in storage but is not instantiated.

## Security boundary

Mod API v1 is data-only. JSON values select supported engine behaviours; they do not execute expressions, reflection targets, shell commands, native libraries or arbitrary managed code.
