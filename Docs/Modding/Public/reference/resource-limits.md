# Resource limits

Mod API v1 applies conservative limits before allocating or constructing runtime content. These are safety ceilings, not performance targets.

| Resource | Limit |
| --- | --- |
| Pack directories scanned | 1,024 |
| `manifest.json` | 256 KiB |
| JSON content files per pack | 4,096 |
| Each loader-owned content JSON | 1 MiB |
| Main Tiled map JSON | 16 MiB |
| Each referenced Tiled object template | 1 MiB |
| Resolved Tiled object templates | 4,096, with at most 16 nested references |
| Tile cells in one layer | 1,000,000 |
| PNG file | 32 MiB, 8,192 pixels on either side, and 32 megapixels decoded |
| Weapon effect PNG | 16 MiB before decode |
| WAV/OGG file | 64 MiB |
| `.capmod` archive | 256 MiB on Windows/Android; 32 MiB on WebGL |
| Catalog dependency install batch | 512 MiB on Windows/Android; 64 MiB on WebGL |
| Weapon sprite-animation frames | 120 per clip and four named clips |
| Stage spawners | 256; 64 enemy IDs per spawner |

The JSON Schemas contain more specific array and numeric limits. Authors should stay substantially below these ceilings, particularly for decoded textures, animated tiles, particle counts, and simultaneous enemies. Passing validation does not guarantee acceptable frame time or memory usage on every device. See [Windows, Android, and WebGL](../getting-started/platforms.md) for platform packaging and verification status.
