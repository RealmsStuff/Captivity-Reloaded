# Mod API migration guides

There is currently no released cross-version migration: Mod API v1 is the first public contract and is still in release verification. This page establishes the migration process that future API releases will follow.

## Updating a pack within v1

1. Read the newest [API changelog](api-changelog.md).
2. Validate the unpacked pack with the repository validator.
3. Open it in a current game build and inspect the Mods panel details.
4. Search the [validation-code catalog](error-codes.md) for every warning or error.
5. Rebuild platform AssetBundles and the `.capmod` archive if any bundled asset changed.
6. Increment the pack's own `version`; leave `modApiVersion` at `1` unless a later contract has actually shipped.

## Future major-version migration

When Mod API v2 or later is introduced, its guide will be added here and will contain:

- a compatibility matrix for game and API versions;
- every changed or removed field;
- mechanical before-and-after JSON examples;
- changes to defaults, fallbacks, IDs, asset slots, and resource limits;
- packaging or AssetBundle rebuild requirements;
- replacement validation codes;
- a test checklist and rollback instructions.

Migration guides will describe shipped behavior only. Draft proposals remain in internal design documents and will not be presented as required author work.
