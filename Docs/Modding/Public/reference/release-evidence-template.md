# Release evidence template

Copy this page into the release record for each Mod API release candidate. Do not mark a checklist row complete without a result, date, tester, and retained evidence.

## Candidate

| Field | Value |
| --- | --- |
| Version |  |
| Commit |  |
| Branch or tag |  |
| Unity version and revision | `2021.3.45f1 (0da89fac8e79)` |
| Test coordinator |  |
| Verification started |  |
| Verification completed |  |

## Automated tests

| Suite | Platform | Total | Passed | Failed | Skipped | Result XML | Editor log |
| --- | --- | ---: | ---: | ---: | ---: | --- | --- |
| EditMode |  |  |  |  |  |  |  |
| PlayMode |  |  |  |  |  |  |  |
| Schema and example validation |  |  |  |  |  |  |  |
| Wiki validation |  |  |  |  |  |  |  |

Zero discovered Unity tests is a failure, even when the editor process exits successfully.

## Runtime feature checks

| Check | Build/platform | Result | Evidence or issue |
| --- | --- | --- | --- |
| Walking navigation links |  |  |  |
| Climbing navigation links and downward falls |  |  |  |
| Flying navigation links |  |  |  |
| Alternate-fire arm recoil |  |  |  |
| Enemy attack and multi-hit timing |  |  |  |
| Facing-relative impulses |  |  |  |
| Positional event audio and camera shake |  |  |  |
| Atlas sprite effects |  |  |  |
| Physical projectiles and impacts |  |  |  |
| Casings, custom audio, and sprite animation |  |  |  |
| Tiled stress arena |  |  |  |
| Excessive runtime-object behavior |  |  |  |

Record the exact stage, pack IDs, content IDs, enemy types, and reproduction steps in the evidence or linked issue.

## Example-pack matrix

Add one row for every directory containing `ExampleMods/<pack>/manifest.json`.

| Pack ID and version | Clean install | Advertised behavior | Save/reload | Disable/re-enable | Uninstall | Result/evidence |
| --- | --- | --- | --- | --- | --- | --- |
|  |  |  |  |  |  |  |

After the isolated rows pass, record one all-compatible-examples test and list every installed pack ID.

## Save compatibility

Use copies of test saves, never their only copy.

| Scenario | Fixture | Expected result | Actual result | Evidence |
| --- | --- | --- | --- | --- |
| Mod missing |  | Unresolved content is reported; unrelated progress remains intact |  |  |
| Content removed |  | Removed IDs fail safely; remaining content reconnects |  |  |
| Mod upgraded with stable IDs |  | Existing state reconnects |  |  |
| Content renamed without migration |  | Old ID is removed and new ID is added; no silent reassignment |  |  |
| Disabled then re-enabled |  | Saved mod data is retained and reconnects |  |  |
| Update interrupted |  | Last valid version is restored |  |  |

## Build artifacts

| Platform | Build identifier | SHA-256 | Device, OS, or browser | Smoke-test result | Evidence |
| --- | --- | --- | --- | --- | --- |
| Windows |  |  |  |  |  |
| Android |  |  |  |  |  |
| WebGL |  |  |  |  |  |

## Catalog workflow

Complete every applicable row on Windows, Android, and WebGL.

| Check | Windows | Android | WebGL | Evidence |
| --- | --- | --- | --- | --- |
| Catalog and preview load |  |  |  |  |
| First install |  |  |  |  |
| Dependency batch |  |  |  |  |
| Update and restart |  |  |  |  |
| Local-change warning |  |  |  |  |
| Rollback |  |  |  |  |
| Uninstall and restore |  |  |  |  |
| Corrupt download rejection |  |  |  |  |
| Hash mismatch rejection |  |  |  |  |
| Installed content launches |  |  |  |  |
| Persistence after relaunch | n/a |  |  |  |
| Storage/quota failure | n/a | n/a |  |  |

## Licensing and final review

| Gate | Result | Evidence |
| --- | --- | --- |
| `LICENSE` included |  |  |
| Third-party notices included |  |  |
| Every shipped asset group has a provenance decision |  |  |
| Catalog releases have author permission |  |  |
| Generated and local files excluded |  |  |
| Final diff reviewed |  |  |

## Approval

List unresolved issues and explicitly decide whether each blocks the release. A release with an unchecked required gate is not approved.

| Role | Name | Decision | Date |
| --- | --- | --- | --- |
| Engineering |  |  |  |
| Content/licensing |  |  |  |
| Release owner |  |  |  |
