"""Rebuild the legacy Femboy Edition asset-patch PNGs from their real Sprites.

AssetRipper gives duplicate Texture2D and Sprite names independent numeric
suffixes.  Pairing files by those suffixes can therefore combine artwork with
the wrong sprite rectangle.  This tool instead maps the current Core Sprite's
render-data key back to the exact Sprite in the legacy asset bundle.
"""

from __future__ import annotations

import json
import hashlib
import re
import struct
from pathlib import Path

import UnityPy


PROJECT = Path(__file__).resolve().parents[1]
SOURCE_BUNDLE = (
    PROJECT.parent
    / "Mods to possible features"
    / "Femboy edition"
    / "sharedassets0.assets"
)
PACK_ROOTS = (
    PROJECT / "Mods" / "femboy-refitted-shirt",
    PROJECT / "ExampleMods" / "femboy-refitted-shirt",
)
CORE_CLOTHING = (
    PROJECT
    / "Assets"
    / "Resources"
    / "Modding"
    / "Core"
    / "Content"
    / "core-clothing.json"
)
PLAYER_PREFAB = PROJECT / "Assets" / "Actors" / "Players" / "Alex.prefab"
LEGACY_TEXTURES = PROJECT.parent / "ModExtraction" / "femboy" / "Assets" / "Texture2D"

BODY_PARTS = {
    "bp_spine": "torso-lower",
    "bp_chest": "chest",
    "bp_neck": "neck",
}
SKIN_FIELDS = {
    "m_sprPale": "pale",
    "m_sprWhite": "white",
    "m_sprTan": "tan",
    "m_sprBlack": "black",
}
BODY_TEXTURE_NAMES = {
    "torso-lower-pale": "TorsoLower",
    "torso-lower-white": "TorsoLower_3",
    "torso-lower-tan": "TorsoLower_1",
    "torso-lower-black": "TorsoLower_2",
    "chest-pale": "Chest_2",
    "chest-white": "Chest_1",
    "chest-tan": "Chest_0",
    "chest-black": "Chest",
    "neck-pale": "Neck_0",
    "neck-white": "Neck_1",
    "neck-tan": "Neck",
    "neck-black": "Neck_2",
}


def unity_guid_text(guid) -> str:
    """Convert UnityPy's four integer GUID fields to Unity YAML text order."""
    raw = struct.pack("<IIII", guid.data_0_, guid.data_1_, guid.data_2_, guid.data_3_)
    return "".join(f"{byte:02x}"[::-1] for byte in raw)


def sprite_render_key(asset_path: Path) -> str:
    text = asset_path.read_text(encoding="utf-8")
    match = re.search(r"^\s{4}([0-9a-f]{32}):\s*21300000\s*$", text, re.MULTILINE)
    if not match:
        raise RuntimeError(f"No Sprite render-data key in {asset_path}")
    return match.group(1)


def image_hash(image) -> str:
    return hashlib.sha1(image.convert("RGBA").tobytes()).hexdigest()


def guid_asset_map() -> dict[str, Path]:
    result: dict[str, Path] = {}
    for meta_path in (PROJECT / "Assets" / "Sprite").glob("*.asset.meta"):
        match = re.search(r"^guid:\s*([0-9a-f]{32})\s*$", meta_path.read_text(encoding="utf-8"), re.MULTILINE)
        if match:
            result[match.group(1)] = meta_path.with_suffix("")
    return result


def player_body_sources() -> dict[str, Path]:
    text = PLAYER_PREFAB.read_text(encoding="utf-8")
    game_objects: dict[str, str] = {}
    for match in re.finditer(r"--- !u!1 &(\d+)\s+GameObject:\s*(.*?)(?=\n--- !u!|\Z)", text, re.DOTALL):
        name = re.search(r"^\s{2}m_Name:\s*(.+?)\s*$", match.group(2), re.MULTILINE)
        if name:
            game_objects[match.group(1)] = name.group(1)

    assets_by_guid = guid_asset_map()
    result: dict[str, Path] = {}
    for match in re.finditer(r"--- !u!114 &\d+\s+MonoBehaviour:\s*(.*?)(?=\n--- !u!|\Z)", text, re.DOTALL):
        block = match.group(1)
        owner = re.search(r"^\s{2}m_GameObject:\s*\{fileID:\s*(\d+)\}", block, re.MULTILINE)
        if not owner or game_objects.get(owner.group(1)) not in BODY_PARTS:
            continue
        semantic = BODY_PARTS[game_objects[owner.group(1)]]
        for field, tone in SKIN_FIELDS.items():
            sprite = re.search(
                rf"^\s{{2}}{field}:\s*\{{fileID:\s*21300000,\s*guid:\s*([0-9a-f]{{32}})",
                block,
                re.MULTILINE,
            )
            if sprite and sprite.group(1) in assets_by_guid:
                result[f"{semantic}-{tone}"] = assets_by_guid[sprite.group(1)]
    return result


def core_clothing_sources() -> dict[tuple[str, str], Path]:
    catalog = json.loads(CORE_CLOTHING.read_text(encoding="utf-8"))
    result: dict[tuple[str, str], Path] = {}
    for entry in catalog["entries"]:
        target = entry["id"]
        result[(target, "icon")] = PROJECT / entry["icon"]["sourceAsset"]
        for piece in entry["pieces"]:
            result[(target, piece["slot"])] = PROJECT / piece["sprite"]["sourceAsset"]
    return result


def main() -> None:
    if not SOURCE_BUNDLE.is_file():
        raise SystemExit(f"Femboy Edition bundle not found: {SOURCE_BUNDLE}")

    environment = UnityPy.load(str(SOURCE_BUNDLE))
    sprites = {}
    sprites_by_texture: dict[int, object] = {}
    for obj in environment.objects:
        if obj.type.name != "Sprite":
            continue
        sprite = obj.read()
        sprites[unity_guid_text(sprite.m_RenderDataKey[0])] = sprite
        sprites_by_texture[sprite.m_RD.texture.path_id] = sprite

    # A handful of duplicated legacy Sprite assets no longer have the same
    # render-data keys as their migrated Core counterparts.  Resolve those by
    # the exact 32x32 Texture2D pixels AssetRipper exported, never by its
    # unreliable collision-number suffix alone.
    texture_ids_by_hash: dict[str, int] = {}
    for obj in environment.objects:
        if obj.type.name != "Texture2D":
            continue
        try:
            texture_ids_by_hash[image_hash(obj.read().image)] = obj.path_id
        except (OSError, EOFError):
            pass

    clothing_sources = core_clothing_sources()
    written: set[Path] = set()
    for pack_root in PACK_ROOTS:
        for definition_path in sorted((pack_root / "content").glob("legacy-*.json")):
            definition = json.loads(definition_path.read_text(encoding="utf-8"))
            target = definition["target"]
            for slot, relative_output in definition["replacements"].items():
                output = pack_root / relative_output
                if output in written:
                    continue
                source_asset = clothing_sources[(target, slot)]
                key = sprite_render_key(source_asset)
                sprite = sprites.get(key)
                if sprite is None:
                    raw_texture = LEGACY_TEXTURES / output.name
                    if raw_texture.is_file():
                        from PIL import Image

                        with Image.open(raw_texture) as image:
                            texture_id = texture_ids_by_hash.get(image_hash(image))
                        sprite = sprites_by_texture.get(texture_id)
                if sprite is None:
                    raise RuntimeError(f"Legacy bundle has no Sprite for {source_asset} ({key})")
                output.parent.mkdir(parents=True, exist_ok=True)
                sprite.image.convert("RGBA").save(output)
                written.add(output)

        for output_name, texture_name in BODY_TEXTURE_NAMES.items():
            from PIL import Image

            raw_texture = LEGACY_TEXTURES / f"{texture_name}.png"
            with Image.open(raw_texture) as image:
                texture_id = texture_ids_by_hash.get(image_hash(image))
            sprite = sprites_by_texture.get(texture_id)
            if sprite is None:
                raise RuntimeError(f"Legacy bundle has no embedded Sprite for {raw_texture}")
            output = pack_root / "assets" / "body" / f"{output_name}.png"
            output.parent.mkdir(parents=True, exist_ok=True)
            sprite.image.convert("RGBA").save(output)
            written.add(output)

    print(f"Rebuilt {len(written)} Femboy Edition PNG files from exact Sprite references.")


if __name__ == "__main__":
    main()
