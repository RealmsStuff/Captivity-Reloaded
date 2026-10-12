# Setting up the modding environment

This guide takes you from an empty machine to an open Captivity Reloaded project with the authoring tools ready. You do not need every optional tool for every kind of mod.

## Table of contents

| Section | Go there |
| --- | --- |
| Choose what to install | [Requirements](#requirements) |
| Install the Unity editor | [Downloading Unity](#downloading-unity) |
| Set up without Git | [Quick setup: ZIP and Unity Hub](#quick-setup-zip-and-unity-hub) |
| Set up for contributing | [Contributor setup: Git and a C# editor](#contributor-setup-git-and-a-c-editor) |
| Add the map editor | [Installing Tiled](#installing-tiled) |
| Add the animation editor | [Installing DragonBones Pro](#installing-dragonbones-pro) |
| Confirm everything works | [Checking the setup](#checking-the-setup) |

## Requirements

Pick the row that matches what you want to make:

| Work | Required tools |
| --- | --- |
| JSON packs, asset replacements, weapons, clothing, and rules | A text editor; Unity is recommended for validation and packaging |
| Authored stages | Tiled; Unity is recommended for validation and play testing |
| Original or paired skeletal animation | Unity and DragonBones Pro 5.6.3 |
| Game development or release builds | Unity, Git, a C# editor, and the applicable build-support modules |

{% hint style="warning" %}
Captivity Reloaded is already a complete Unity project. There is no separate Captivity `.unitypackage`. Do not create a blank project and import the repository into it.
{% endhint %}

## Downloading Unity

Captivity Reloaded uses Unity Editor **2021.3.45f1**. Use that exact patch version; `ProjectSettings/ProjectVersion.txt` records the same version.

1. Install [Unity Hub](https://unity.com/download).
2. Sign in with an account that has a valid Unity license.
3. In the Hub, install [Unity Editor 2021.3.45f1](https://unity.com/releases/editor/whats-new/2021.3.45f1).
4. For ordinary Windows mod authoring, install the editor and your preferred C# editor integration.
5. Add **Android Build Support** and **WebGL Build Support** only if you will build or verify those targets. They can be added later through **Installs > 2021.3.45f1 > Add modules**.

After Unity is installed, choose one of the following project setup routes.

## Quick setup: ZIP and Unity Hub

Use this route if you want to make a mod and do not plan to contribute changes through Git.

### Download the project

1. Open the [Captivity Reloaded repository](https://github.com/RealmsStuff/Captivity-Reloaded).
2. Select **Code > Download ZIP**.
3. Extract the archive to a normal writable directory. Do not work from inside the ZIP file.
4. Open the extracted directory and confirm that `Assets`, `Packages`, and `ProjectSettings` are directly inside it.

### Open the project in Unity

1. Open Unity Hub.
2. Select **Open > Add project from disk**.
3. Choose the extracted `Captivity-Reloaded` directory. Do not select its `Assets` directory.
4. If the Hub asks which editor to use, select **2021.3.45f1**.
5. Let Unity restore `Packages/manifest.json` and finish its first import. This can take several minutes.
6. Wait until the progress indicator and script compilation finish before opening scenes or authoring windows.
7. Open `Assets/Scenes/Main.unity`.

The **Captivity Reloaded > Modding** menu should now be visible at the top of the Unity editor.

## Contributor setup: Git and a C# editor

Use this route if you want version control, plan to update your copy, or expect to contribute code or documentation.

### Requirements

- [Git](https://git-scm.com/downloads)
- Unity Editor 2021.3.45f1
- A C# editor; the repository's `.vsconfig` selects Visual Studio's **Game development with Unity** workload

### Clone the project

Open PowerShell in the directory where you keep projects and run:

```powershell
git clone https://github.com/RealmsStuff/Captivity-Reloaded.git
cd Captivity-Reloaded
```

If you intend to contribute through your own GitHub fork, clone the fork instead and keep the main repository as an upstream remote.

### Open the cloned project

1. In Unity Hub, select **Open > Add project from disk**.
2. Select the cloned `Captivity-Reloaded` directory containing `Assets`, `Packages`, and `ProjectSettings`.
3. Open it with Unity **2021.3.45f1** and allow the initial package restore and import to finish.
4. Open `Assets/Scenes/Main.unity`.
5. Open the generated solution in your C# editor only after Unity has finished creating the project files.

Unity creates `Library`, `Temp`, `Logs`, `obj`, solution files, and user settings locally. These generated files should not be copied into a mod or committed.

## Installing Tiled

Tiled is required only for authored stages.

1. Download the desktop application for Windows, macOS, or Linux from the [official Tiled download page](https://www.mapeditor.org/download.html).
2. Install and launch it. Captivity maps do not require a Tiled plugin or scripted extension.
3. Copy the complete `ModSDK/MapTemplates/TiledStage` directory into the workspace for your mod.
4. Keep its `levels`, `templates`, `tilesets`, `assets`, and `reference` directories together. Their files use relative paths.
5. Open the copied `levels/starter-map.json` in Tiled.
6. Keep the map in finite, orthogonal JSON form.

Do not copy only `starter-map.json`; its preview art, templates, and tilesets will appear missing. Continue with [Tiled maps](tiled.md) for the supported layers, gameplay objects, properties, restrictions, and verification command.

## Installing DragonBones Pro

DragonBones is optional and is needed only when editing bone animation outside Unity. Captivity neither uses nor ships the DragonBones runtime.

### Get the verified editor

The project author's Windows tool bundle contains DragonBones Pro 5.6.3 at:

```text
../Tools/DragonBones/DragonBonesPro-v5.6.3.exe
```

The path is relative to the Unity project. A normal Git clone may not include this sibling `Tools` directory; obtain the authoring bundle from the project maintainer. Avoid unofficial executable mirrors. The [official DragonBones download page](https://dragonbones.github.io/en/download.html) is the upstream reference, but Captivity's bridge is tested specifically with version 5.6.3 on Windows.

### Verify and install it

In PowerShell, run:

```powershell
Get-FileHash ..\Tools\DragonBones\DragonBonesPro-v5.6.3.exe -Algorithm SHA256
```

The expected SHA-256 digest is:

```text
593053196DA93BBE9822E2923145F800AFEFEA3DA00E6E9E174FECE0AAF8C796
```

1. Run `DragonBonesPro-v5.6.3.exe`.
2. Retain its bundled Adobe AIR support.
3. Launch `C:/Program Files/Egret/DragonBonesPro/DragonBonesPro.exe`.
4. Confirm that the displayed version is 5.6.3.

Do not add DragonBones packages to `Packages/manifest.json`, install a DragonBones Unity runtime, or import a DragonBones `.unitypackage`. In Unity, open **Captivity Reloaded > Modding > DragonBones Round Trip**. This window exchanges JSON and PNG files with the standalone editor.

Continue with [DragonBones round trip](dragonbones-round-trip.md) for exporting, editing, analyzing, backing up, and importing animations.

## Checking the setup

### Unity

1. Confirm that `Assets/Scenes/Main.unity` opens without compile errors.
2. Confirm that **Captivity Reloaded > Modding** contains the creation, packaging, preview, and DragonBones commands.
3. Open **Captivity Reloaded > Modding > Create Mod...**.
4. Enter a test pack ID such as `yourname.test-mod` and confirm that the wizard can select or create an `ExampleMods` folder.

The [first-pack guide](../getting-started/first-pack.md) continues from this point.

### Tiled

Confirm that the copied starter map opens with its preview artwork, templates, and tilesets intact.

### DragonBones

When animation work is required, confirm that DragonBones Pro 5.6.3 can import a Unity-generated `<enemy>_ske.json` file.

## Where to go next

- New mod authors: [Your first pack](../getting-started/first-pack.md)
- Map authors: [Tiled maps](tiled.md)
- Animation authors: [DragonBones round trip](dragonbones-round-trip.md)
- Project contributors: [Development setup](../development/setup.md) and [Building and testing](../development/building-and-testing.md)
