# Mod API v1 design decisions

These decisions describe the scope already used by the draft implementation. They remain subject to the pre-v1 schema review, but are no longer unimplemented recommendations.

## 1. Meaning of "one sprite sheet"

Current complex enemies use multi-part rigs. The Gremlin has 20 sprite renderers and 19 rigid bodies/colliders rather than one animated renderer.

Draft v1 decision:

- A mod extending a rigged Core enemy supplies one PNG atlas containing named body-part artwork and one JSON file mapping atlas regions to public visual slots.
- Animations, bones, colliders and special interactions are inherited unless the selected template explicitly permits overrides.
- A separate whole-frame template is added later for mechanically simpler enemies.

This keeps the one-image goal realistic without pretending a flat animation sheet contains physics and interaction data.

## 2. External-mod platforms

External folder discovery is Windows-desktop-only for v1. The Core registry and definitions must work on every supported platform. Mobile distribution can be designed later without coupling the content format to one storefront.

## 3. Executable code

Mod API v1 is data-only. It does not load external DLLs. New behaviours become reviewed engine modules selected by JSON keys.

## 4. Core packaging

Core is logically a content pack but remains packaged with the game and cannot be disabled. It may use prefab adapters and Unity references while migration is incomplete. Core assets do not need to be shipped as loose files.

## 5. Overrides

Additive content is the default. Replacing Core data or assets requires an explicit patch and conflict reporting. Matching filenames never causes replacement.

## 6. Stage authoring

Prefab-backed Core stages and template-backed external stages share the stage registry. Authored external layouts use finite orthogonal Tiled JSON rather than a custom map format. The Tiled contract remains experimental until its schema and runtime verification are complete.

## 7. Distribution service

Local folders are the canonical installed-pack format. Steam Workshop, mod.io or another service may download and update those folders later, but distribution must not define the content API.
