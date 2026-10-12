# Contributing documentation

The public wiki is stored in `Docs/Modding/Public` and published through the root `.gitbook.yaml`. `README.md` is the wiki home and `SUMMARY.md` is its navigation tree.

## Audience and structure

Write for one of four audiences:

1. Players installing or managing mods.
2. First-time mod authors following a tutorial.
3. Experienced authors looking up a supported contract.
4. Project contributors building, testing, or changing the implementation.

Put workflows in the public wiki, exact JSON structure in schemas, detailed cross-file behavior in `spec-v1.md`, runnable compositions in `ExampleMods`, and unsettled design work in `Docs/Planning`.

## Source-of-truth rules

- Do not manually edit generated field, asset-slot, or validation-code tables. Change their authoritative source and rerun the corresponding script under `Tools/Documentation`.
- A tutorial example must validate against the current schema and runtime parser.
- A page describing current support must link to [Implementation status](../reference/status.md) instead of maintaining a separate feature checklist.
- Planning documents must state that they are plans or audits and must not be linked as current user instructions.
- Do not mark a release or validation checklist item complete until the associated command or hands-on test has passed.
- When a pack or tool is removed, update the example index, navigation, and every literal path reference in the same change.

## Style

- Begin with the outcome or task.
- Use exact menu labels, paths, content types, and IDs.
- Keep tutorials sequential and put background after the first usable path.
- Mark experimental behavior explicitly.
- Prefer small examples that can be copied and validated.
- Use relative Markdown links and descriptive link text.
- Do not rely on screenshots for information that can be stated in text.
- Capture screenshots from the current release candidate or installed authoring tool. Do not use mock UI, an older game build, personal paths, or account information.

## Required checks

Before merging documentation changes:

1. Run `powershell -ExecutionPolicy Bypass -File .\Tools\Documentation\Validate-Wiki.ps1`.
2. Regenerate API fields, asset slots, and validation codes when their schemas or source changed.
3. Run the schema validator when schemas, examples, snippets, or capability claims changed.
4. Confirm menu names and runtime behavior against the current source.
5. Review the diff for duplicated or contradictory status statements.

The reference generators are:

```powershell
.\Tools\Documentation\Generate-ApiReference.ps1
.\Tools\Documentation\Generate-AssetSlotCatalog.ps1
.\Tools\Documentation\Generate-ValidationCatalog.ps1
```

CI reruns them and fails when committed generated pages are stale.

## GitHub Wiki export

GitBook is the repository-native format. Generate the flattened GitHub Wiki publication bundle with:

```powershell
.\Tools\Documentation\Export-GitHubWiki.ps1
```

The exporter writes only to the ignored `Temp/GitHubWiki` directory. It converts GitBook hints, rewrites internal pages and repository links, copies public assets, creates `_Sidebar.md` and `_Footer.md`, detects filename collisions, and validates every exported target. Never edit the exported copy as the source of truth; change `Docs/Modding/Public`, validate it, and export again.

For an ordinary GitHub repository rather than GitHub's Wiki feature, run:

```powershell
.\Tools\Documentation\Export-GitHubGuideRepository.ps1
```

This produces `Temp/GitHubGuide`, adds a repository landing `README.md`, and uses `.md` links that work in GitHub's normal file browser.
