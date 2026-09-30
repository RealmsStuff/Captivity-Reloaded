# Save compatibility

Mod API v1 keeps the existing vanilla save tables and their integer identifiers unchanged. This avoids a destructive database rewrite and preserves compatibility with existing desktop, Android, and WebGL saves.

## Vanilla records

The packaged Core catalog supplies a category-safe bidirectional map between existing integers and stable content IDs. A legacy ID is never interpreted without its category, because enemy, stage, clothing, and challenge IDs overlap.

Examples:

```text
(Enemy, 12)    <-> core:enemy/gremlin
(Stage, 6)     <-> core:stage/field-day
(Clothing, 78) <-> core:clothing/hazmat-suit
```

Weapons and usables currently use legacy names rather than database IDs, so they are not included in numeric save translation.

## Mod records

Data belonging to external content is stored separately using:

- the full namespaced content ID;
- the expected content category;
- an opaque JSON state payload.

Desktop saves use `tbl_contentState`. Android and WebGL add the equivalent `contentStates` collection to their JSON save model. Existing saves receive the new storage automatically; their old rows are not rewritten.

## Missing content

Loading a saved content state has three outcomes:

- `Available`: the ID is registered and its category matches;
- `Missing`: the ID remains stored, but its pack is not currently available;
- `CategoryMismatch`: the ID exists but cannot safely consume state written for another content type.

Missing and mismatched records are not instantiated. They remain in storage so reinstalling the same pack can reconnect them. Only an explicit full save reset clears the sidecar content-state storage.

## Future aliases

Public content IDs are permanent. A future definition format may declare an explicit old-ID alias, but the loader must validate aliases before redirecting saved state. It must never infer a rename from display names or filenames.
