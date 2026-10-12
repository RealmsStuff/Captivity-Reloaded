# Example pack index

`ExampleMods` contains runnable examples, regression fixtures, and maintained conversions. Copy the smallest example that demonstrates the feature you need. Converted legacy packs are useful compatibility references, but they are often too broad for a first project.

## Recommended starting points

| Goal | Folder | What it demonstrates |
| --- | --- | --- |
| Replace one published asset | `simple-nerf-gun` | Small explicit asset patch |
| Add an inherited weapon | `additive-nerf-pistol` | New weapon based on a Core template |
| Exercise advanced weapons | `weapon-behavior-showcase` | Charge, beam, alternate fire, melee, and sprite animation |
| Add inherited clothing | `authored-hairstyles` | Layered hair with a moving attachment |
| Add original clothing | `advanced-clothing-showcase` | Original rig pieces and persistent attachment |
| Add an inherited enemy | `zombie-1-template` | Core enemy template plus replacement atlas |
| Add an original enemy | `prey-green-zombie` | Original skeleton, attacks, animations, finisher, stage, and challenge |
| Add a simple stage | `training-yard-stage` | Core-backed stage and JSON spawners |
| Author a Tiled stage | `tiled-training-yard` | Neutral stage shell, Tiled map, and stage script |
| Stress-test a map | `tiled-stress-arena` | Large authored map and verification fixture |
| Add a game mode | `survival-sprint-mode` | Spawn, wave, economy, and player rules |
| Add challenge objectives | `challenge-objectives-showcase` | General and ordered challenge objectives |
| Add difficulties | `dakozan-extended-difficulties` | Additional difficulty profiles |

## Complete pack list

| Folder | Primary purpose |
| --- | --- |
| `additive-nerf-pistol` | Additive inherited weapon |
| `advanced-clothing-showcase` | Original clothing and player attachment |
| `animation-reference-gallery` | Core-derived enemy animation references |
| `apothem-gun-game` | Weapon-progression game mode |
| `authored-hairstyles` | Layered moving hairstyle |
| `challenge-objectives-showcase` | Challenge objective coverage |
| `cod-perk-machine` | Core machine artwork patch |
| `cod-wonderweapon` | Core weapon artwork patch |
| `convenience-mechanics` | Experimental player and store rules |
| `core-fer-tiled-port` | Core-stage Tiled migration reference |
| `cry-when-raped` | Small player-face asset patch |
| `dakozan-extended-difficulties` | Difficulty profiles |
| `developer-toolkit` | Rule-gated runtime developer panel |
| `femboy-refitted-shirt` | Clothing artwork refit patches |
| `futanari-small-tweaks` | Player artwork patches |
| `futazombies` | Core enemy artwork patches |
| `goblin-slayer-armor` | Four Core clothing patches |
| `legacy-c4c` | Broad legacy gameplay-rule conversion |
| `legacy-clothing-overhaul` | Clothing damage and stage rules |
| `legacy-luins-balance` | Balance-rule conversion |
| `legacy-overpower` | Gameplay-rule conversion |
| `original-dev-extras` | Original weapons, attachment, and stage content |
| `prey-bunny-girls` | Large asset-patch conversion |
| `prey-green-zombie` | Full original-enemy authoring demonstration |
| `raygun-revolver` | Core weapon artwork patch |
| `shaded-girl` | Player artwork patch |
| `simple-nerf-gun` | Minimal weapon artwork patch |
| `smaller-breast` | Player body artwork patch |
| `smaller-breast-and-butt` | Dependent body and clothing patches |
| `small-tweaks` | Small grouped artwork patches |
| `smiling-blush-small-tweaks` | Dependent mouth artwork patches |
| `start-with-no-gun` | Starter-inventory and economy rules |
| `survival-sprint-mode` | Bounded game-mode rules |
| `tiled-stress-arena` | Large-map verification content |
| `tiled-training-yard` | Tiled stage starter example |
| `training-yard-stage` | Template-backed stage |
| `tweaked-fer` | FER artwork patches |
| `weapon-behavior-showcase` | Advanced weapon behavior |
| `zombie-1-template` | Inherited enemy template |

Each pack has a root `manifest.json` and README with its purpose, provenance or workflow notes, and relevant test status.

## Choosing safely

- Prefer a purpose-built example over a large conversion.
- Do not copy IDs, authors, or asset licenses unchanged.
- Treat experimental examples as implementation demonstrations, not compatibility promises.
- Run the schema validator and in-game validation after changing paths, IDs, dependencies, or content references.
