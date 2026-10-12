# Tiled Training Yard

Small authored-stage example for the experimental Tiled pipeline.

The pack demonstrates:

- a `stage` definition that extends Field Day;
- a finite orthogonal Tiled JSON layout;
- authored collision, navigation, visual layers, and gameplay objects;
- two JSON-defined enemy spawners;
- a `stageScript` that runs a stage-open notification and counts wave starts.

Use `ModSDK/MapTemplates/TiledStage` for the clean copyable starter kit and `Docs/Modding/Public/tools/tiled.md` for the complete workflow. This example is a compact runtime demonstration rather than a full map template.

Validate from the repository root:

```powershell
dotnet run --project Tools/ModSchemaValidator -- . ExampleMods/tiled-training-yard
```
