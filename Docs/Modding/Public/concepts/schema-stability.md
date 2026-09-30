# Schema stability

**Current status: freeze review.**

Freezing the first public schemas means choosing the fields that are ready to become a compatibility promise. It does not mean development stops.

After Mod API v1 is frozen:

* a valid stable v1 field keeps the same name, data type, and meaning throughout v1;
* required stable fields cannot suddenly become stricter;
* new v1 fields must normally be optional and have a safe default;
* removing, renaming, or changing the meaning of a stable field requires a migration path or a new major API version;
* published stable content and asset-slot IDs are not silently reused for something else.

Fields marked **Experimental** are intentionally excluded from that promise. They can be tested publicly, but may be renamed, redesigned, or removed before promotion. We will mark them in reference documentation and with the custom JSON Schema annotation `"x-stability": "experimental"`; mod JSON itself does not need to contain a stability field.

Before freezing a schema, it should have a complete JSON Schema, useful validation errors, at least one working example mod, automated parser/runtime tests, and a documented fallback or migration rule.

## Current schema inventory

| Contract | JSON Schema | Field classification |
| --- | --- | --- |
| Manifest | Available | Stable candidate |
| Challenge | Available | Stable candidate |
| Rule profile/game mode | Available | Stable identity; experimental modules |
| Enemy | Available | Stable identity/stats/inherited visual; experimental gameplay modules and spawning weight |
| Normalized enemy rig/animation | Available | Frozen schema version 1; future incompatible changes require a new schema version |
| Clothing | Available | Stable identity/inherited visual; experimental attachments, compatibility, and effects |
| Weapon | Available | Stable identity/stats/inherited visual; experimental behavior and animation modules |
| Usable/item | Available | Stable identity; experimental visuals, stats, and effects |
| Stage | Available | Stable identity/waves/spawners; experimental authored layout and audio |
| Difficulty | Available | Stable candidate |
| Asset patch | Available | Stable candidate |
| Tiled map/gameplay objects | Available | Experimental |
| External Tiled JSON/TSJ tileset | Available | Experimental |

The schemas live in `Docs/Modding/schemas`. Their `x-stability` annotations are the machine-readable classification; tables and prose are explanatory. A field labeled stable is a compatibility candidate, not a release promise until the v1 release checklist is complete. Tiled layouts and the newer weapon, enemy, clothing, usable, and rule-profile modules remain explicitly experimental.
