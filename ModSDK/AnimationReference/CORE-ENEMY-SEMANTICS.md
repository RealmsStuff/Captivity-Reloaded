# Core enemy animation semantics

Status: reviewed authoring contract, revision 1.

These names describe what an animation does without exposing the inconsistent Unity state and clip names. They do not rename or replace Core assets. `core-animation-catalog.json` schema version 3 records the semantic name on every Animator state and records every semantic use of a reused clip in `semanticNames`.

Common names are `idle`, `move`, `attack-primary`, `await`, `chase`, `get-up`, `climb`, `jump`, and `finisher-phase-N`. Specialized and layered names are listed below. Empty Animator states are exported as `internal-<layer>-empty` with `authoring: false`; mod tools should hide them by default.

| Core enemy | Reviewed semantic animations |
| --- | --- |
| Zombie I | `idle`, `move`, `attack-primary`, `await`, `chase`, `get-up`, `climb`, `jump`, `finisher-phase-1` through `finisher-phase-8` |
| Zombie II | `idle`, `move`, `attack-primary`, `await`, `chase`, `get-up`, `climb`, `jump`, `finisher-phase-1` through `finisher-phase-3` |
| Zombie Grabber | `idle`, `move`, `attack-default-attack-primary`, `await`, `get-up`, `climb`, `jump`, `finisher-phase-1` through `finisher-phase-6` |
| Death Hound | `idle`, `move`, `attack-primary`, `attack-leap`, `attack-charge-prepare`, `finisher-phase-1` through `finisher-phase-8` |
| Hunter | `idle`, `move`, `retreat`, `prowl`, `attack-leap`, `await`, `climb`, `jump`, `finisher-phase-1` through `finisher-phase-4` |
| Maggot | `idle`, `move`, `jump`, `attack-leap`, `finisher-phase-1` through `finisher-phase-3` |
| Fly | `idle`, `move`, `attack-charge-prepare`, `attack-charge`, `wings-idle`, `wings-active`, `finisher-phase-1` through `finisher-phase-5` |
| Musca | `idle`, `move`, `attack-primary`, `carry`, `wings-idle`, `wings-active`, `finisher-phase-1` through `finisher-phase-6` |
| Orc | `idle`, `move`, `attack-primary`, `await`, `climb`, `jump`, `shield-raised`, `finisher-phase-1` through `finisher-phase-8` |
| Goblin Marksman | `idle`, `move`, `attack-primary`, `attack-shoot`, `await`, `climb`, `jump`, `finisher-phase-1` through `finisher-phase-3` |
| Goblin Trapper | `idle`, `move`, `attack-attack-primary`, `attack-place-trap`, `await`, `climb`, `jump`, `finisher-phase-1` through `finisher-phase-3` |
| Gremlin | `idle`, `move`, `attack-primary`, `attack-syringe`, `await`, `get-up`, `climb`, `jump`, `finisher-phase-1` through `finisher-phase-11` |
| Litigant | `upper-body-idle`, `upper-body-move`, `lower-body-move`, `attack-claw`, `upper-body-attack-primary`, `finisher-phase-1` through `finisher-phase-5` |
| Spider Mother | `lower-body-idle`, `lower-body-move`, `upper-body-idle`, `upper-body-move`, `attack-spider-stab-lower`, `attack-spider-stab-upper`, `upper-body-attack-primary`, `finisher-phase-1` through `finisher-phase-5` |
| Head Humper | `idle`, `move`, `attack-leap`, `head-hugging`, `finisher-phase-1`, `finisher-phase-2` |
| Sqoid | `idle`, `move`, `attack-energy-blast`, `hover`, `finisher-phase-1` through `finisher-phase-8` |
| Sunny | `idle`, `move`, `attack-primary`, `attack-crotch-kick`, `dodge`, `climb`, `jump`, `finisher-phase-1` through `finisher-phase-3` |
| Jenny | `idle`, `move`, `attack-heel`, `attack-slap-attack-primary`, `climb`, `jump`, `finisher-phase-1` through `finisher-phase-5` |
| Abby | `idle`, `move`, `attack-primary`, `climb`, `jump`, `finisher-phase-1` through `finisher-phase-6` |
| Jacky | `idle`, `move`, `scare`, `stab-attack-primary`, `climb`, `jump`, `finisher-phase-1` through `finisher-phase-7` |
| Zombie III | `idle`, `move`, `attack-default-attack-primary`, `await`, `get-up`, `climb`, `jump`, `finisher-phase-1`, `finisher-phase-2`, and `finisher-phase-4` through `finisher-phase-6` |

Layer-prefixed attack names preserve the fact that their motions run on a separate Animator layer. A later normalized rig may compose those layers, so collapsing them into `attack-primary` would lose useful information. The missing Zombie III phase 3 reflects the shipped controller rather than an exporter omission.
