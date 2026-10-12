# Development

This section is for contributors changing the Unity project, runtime Mod API, editor tools, schemas, examples, or documentation. Mod authors who only create data packs do not need the full Unity project.

## Development paths

- [Development setup](setup.md) covers the supported Unity version and opening the project.
- [Building and testing](building-and-testing.md) lists validation, Unity tests, release audits, and player builds.
- [Repository layout](repository-layout.md) explains where authoritative files live.
- [Contributing documentation](contributing-documentation.md) defines the wiki structure and anti-drift rules.

Before making a compatibility change, read [Schema stability](../concepts/schema-stability.md), [Defaults and compatibility](../reference/defaults-and-compatibility.md), and the internal `Docs/Modding/spec-v1.md` contract.

## Important boundary

This is a reconstructed project with mixed source and asset provenance. A technically working build is not automatically redistributable. Review [Licensing and redistribution](../reference/licensing-and-redistribution.md) before publishing binaries or assets.
