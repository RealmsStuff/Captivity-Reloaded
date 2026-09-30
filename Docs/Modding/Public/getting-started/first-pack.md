# Your first pack

Start by copying the closest pack from `ExampleMods`. Give the manifest and every content entry a unique lowercase namespace, then change one feature at a time.

Every content definition begins with this envelope:

```json
{
  "schemaVersion": 1,
  "type": "enemy",
  "id": "example.my-mod:enemy/my-enemy",
  "displayName": "My Enemy"
}
```

Content references use stable IDs, not Unity object names or numeric database IDs. Run the game and inspect the Mods panel after each change; its validation message includes the offending file and field when possible.
