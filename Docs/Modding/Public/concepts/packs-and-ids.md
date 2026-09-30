# Pack structure and IDs

Recommended layout:

```text
manifest.json
content/
  enemies/
  clothing/
  items/
  stages/
  rules/
  patches/
assets/
  sprites/
  audio/
```

Pack IDs use lowercase letters, numbers, `.`, `_`, and `-`. Content IDs combine the pack namespace and a category path, such as `example.my-mod:enemy/acid-zombie`.

Once a stable ID is released, treat it as permanent. Saves and other mods may refer to it.
