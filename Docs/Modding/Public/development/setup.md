# Development setup

For a first-time installation, including Unity Hub, obtaining the source project, Tiled, and DragonBones, start with [Install the authoring tools](../tools/installing-authoring-tools.md).

## Requirements

- Git
- Unity Editor `2021.3.45f1`
- The Unity Windows, Android, and WebGL build-support modules when working on release builds
- A C# editor; `.vsconfig` selects Visual Studio's **Game development with Unity** workload
- A supported .NET SDK for the standalone schema and catalog validators
- A valid Unity license for batch-mode test and build commands

## Open the project

1. Clone or obtain the complete repository in a new directory. This repository is the Unity project; it is not a `.unitypackage` to import into another project.
2. Confirm `ProjectSettings/ProjectVersion.txt` names Unity `2021.3.45f1`.
3. Add the repository root as a project in Unity Hub and open it with that exact editor line.
4. Allow Unity to restore packages and create `Library`, `Temp`, `Logs`, `obj`, solution files, and project files.
5. Wait for script compilation to finish before opening scenes or authoring windows.
6. Open `Assets/Scenes/Main.unity` for the normal player entry scene.

Generated directories and IDE files are ignored by Git. Do not commit `Library`, `Temp`, `Logs`, `obj`, `UserSettings`, `Builds`, generated `.csproj`/`.sln` files, test results, local mods, saves, or authoring backups.

## Useful editor menus

The **Captivity Reloaded > Modding** menu includes:

- **Create Mod...** for a loose pack and authoring profile;
- **Create Original Enemy...** for a complete original-enemy starter;
- **Captivity Mod Packager** for `.capmod` validation and packaging;
- **Player Animation Preview** for Core and modded animation inspection;
- **DragonBones Round Trip** for optional external animation editing;
- Core animation and clothing exporters used during migration.

These tools may create working data outside the pack under directories such as `DragonBonesExports`, `ModAuthoringBackups`, or `Temp`. Treat those as local authoring state unless a reviewed result is intentionally promoted into `ModSDK` or `ExampleMods`.

## First verification

After Unity imports successfully:

1. Run the EditMode tests.
2. Run the PlayMode tests.
3. Run the repository schema validator.
4. Open the title screen and check that the Mods panel loads the Core registry without errors.

See [Building and testing](building-and-testing.md) for repeatable commands.
