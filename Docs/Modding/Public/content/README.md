# Content types

Choose the narrowest supported definition for the job. Inheritance is currently the safest way to add content because the Core prefab retains its tested rig and mechanics while JSON changes published fields and artwork.

Unsupported Unity components and arbitrary class names cannot be selected from JSON.

Experimental authored stages can also use [stage scripts](stage-scripts.md) for bounded event sequences, state, notifications, signals, and object activation without loading executable mod code.

| Goal | Start here | Good first example |
| --- | --- | --- |
| Replace a published sprite or sound | [Asset patches](asset-patches.md) | `ExampleMods/simple-nerf-gun` |
| Add or change clothing | [Clothing](clothing.md) | `ExampleMods/authored-hairstyles` |
| Add an enemy | [Enemies](enemies.md) | `ExampleMods/zombie-1-template` |
| Add a weapon or usable | [Weapons and items](weapons-and-items.md) | `ExampleMods/additive-nerf-pistol` |
| Add a stage | [Stages and maps](stages-and-maps.md) | `ExampleMods/training-yard-stage` |
| Script a stage sequence | [Stage scripts](stage-scripts.md) | `ExampleMods/tiled-training-yard` |
| Add objectives or game rules | [Challenges, difficulties, and rules](rules.md) | `ExampleMods/survival-sprint-mode` |

For every available pack, see the [example pack index](../reference/example-packs.md). Exact fields and numeric bounds are listed in the [schema index](../reference/schemas.md).
