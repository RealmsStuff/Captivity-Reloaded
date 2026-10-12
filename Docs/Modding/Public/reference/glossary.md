# Glossary

**Asset patch**

An explicit replacement of a published Core or dependency-owned asset slot. Matching a filename alone never causes an override.

**Asset slot**

A stable public ID for replaceable artwork or audio, independent of the Unity source filename or GUID.

**Content definition**

A JSON document describing one registered enemy, clothing item, weapon, usable, stage, script, challenge, difficulty, rule profile, animation, attachment, or patch.

**Content ID**

A stable namespaced identifier such as `example.my-mod:enemy/acid-zombie`. Saves and other packs may retain it, so released IDs should be treated as permanent.

**Core**

The required, read-only `core` content pack shipped with the game. Some Core definitions still bind to Unity prefabs through compatibility adapters.

**Experimental**

A supported field or format that is excluded from the stable-v1 compatibility promise and may change before promotion.

**Inherited content**

Content that uses `extends` to clone a registered Core template and overrides only published fields or visuals.

**Loose pack**

An unpacked mod folder with `manifest.json` at its root. This is the normal development format.

**Manifest**

The root `manifest.json` that declares a pack's identity, version, content roots, dependencies, conflicts, previews, and related metadata.

**Mod API version**

The content-contract version expected by a pack. It is separate from the pack's own semantic version and the game's release version.

**Namespace**

The pack-owned prefix before `:` in a content ID. External content IDs normally use the manifest's pack ID as their namespace.

**Original content**

Content constructed from data and pack assets without cloning the equivalent Core gameplay prefab. Original formats generally require more explicit rig, behavior, or presentation data.

**Pack**

A manifest plus content documents and optional assets loaded as one dependency and enable-state unit.

**`.capmod`**

A deterministic ZIP-compatible distribution archive with `manifest.json` at its root. It may be data-only or contain packager-generated platform bundles.

**Schema validation**

Structural checking against the published JSON Schemas. It does not replace runtime validation of dependencies, IDs, referenced files, templates, or cross-file relationships.

**Stable**

A compatibility candidate intended to keep the same name, type, and meaning through Mod API v1 after the release freeze is completed.

**Tiled stage**

An authored stage layout using the supported finite orthogonal Tiled JSON subset and the neutral stage shell or a Core compatibility base.
