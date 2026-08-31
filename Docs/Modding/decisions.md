# Open design decisions

These decisions should be approved before runtime implementation makes them part of the public Mod API contract.

## 1. Meaning of "one sprite sheet"

Current complex enemies use multi-part rigs. The Gremlin has 20 sprite renderers and 19 rigid bodies/colliders rather than one animated renderer.

Recommended v1 interpretation:

- A mod extending a rigged Core enemy supplies one PNG atlas containing named body-part artwork and one JSON file mapping atlas regions to public visual slots.
- Animations, bones, colliders and special interactions are inherited unless the selected template explicitly permits overrides.
- A separate whole-frame template is added later for mechanically simpler enemies.

This keeps the one-image goal realistic without pretending a flat animation sheet contains physics and interaction data.

## 2. External-mod platforms

Recommendation: external folder discovery is Windows-desktop-only for v1. The Core registry and definitions must work on every supported platform. Mobile distribution can be designed later without coupling the content format to one storefront.

## 3. Executable code

Recommendation: Mod API v1 is data-only. Do not load external DLLs. New behaviours become reviewed engine modules selected by JSON keys.

## 4. Core packaging

Recommendation: Core is logically a content pack but remains packaged with the game and cannot be disabled. It may use prefab adapters and Unity references while migration is incomplete. Core assets do not need to be shipped as loose files.

## 5. Overrides

Recommendation: additive content is the default. Replacing Core data or assets requires an explicit patch and conflict reporting. Matching filenames never causes replacement.

## 6. Stage authoring

Recommendation: register existing prefab-backed stages first. Evaluate Tiled JSON/TMX before defining the external stage schema. Do not invent a custom level format until a representative stage has been reproduced in the candidate tool.

## 7. Distribution service

Recommendation: treat local folders as the canonical installed-pack format. Steam Workshop, mod.io or another service may download and update those folders later, but distribution must not define the content API.
