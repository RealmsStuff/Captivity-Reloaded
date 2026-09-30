"""Build the faithful source form of the legacy Prey/Bunny Girls visual overhaul.

The supplied legacy release edits Texture2D objects in sharedassets0.assets.  This
tool compares those decoded images with Captivity v1.0.5b, maps the original
images back to the AssetRipper project, and emits the player/clothing asset-patch
documents that can already be represented by the public mod API.  Unmapped
artwork is retained in a machine-readable report for the next conversion slice.
"""

import argparse
import hashlib
import io
import json
import os
import re
import shutil
import sys
from pathlib import Path


PROJECT_ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(PROJECT_ROOT.parent / ".tools" / "unitypy"))

from PIL import Image
import UnityPy


PACK_ID = "draco66electro.prey-bunny-girls"
NOISE_PATH_IDS = {2182, 2502, 2912, 2937}
SOURCE_NAME_OVERRIDES = {
    # These source canvases are pixel-identical, so their archive order is the
    # only way to recover AssetRipper's distinct stable names.
    2253: "Eyelid",
    2333: "Hand_2",
    2343: "Neck_5",
    2528: "Neck_8",
    2982: "Hand_16",
}

EXISTING_NAMED_TARGETS = {
    "Butt": "core:sprite/butt",
    "Butt_2": "core:sprite/butt-2",
    "Butt_5": "core:sprite/butt-5",
    "Butt_6": "core:sprite/butt-6",
    "Chest_5": "core:sprite/chest-5",
    "Chest_7": "core:sprite/chest-7",
    "TorsoLower_0": "core:sprite/torso-lower-0",
}

CORE_ENEMY_TARGETS = {
    "Butt": [("core:enemy/zombie-1", "body/butt"), ("core:enemy/zombie-3", "body/butt")],
    "Head": [("core:enemy/zombie-1", "body/head")],
    "TorsoLower_5": [("core:enemy/zombie-1", "body/torso-lower"),
                     ("core:enemy/zombie-3", "body/torso-lower")],
    "Butt_2": [("core:enemy/zombie-2", "body/butt")],
    "Head_3": [("core:enemy/zombie-2", "body/head")],
    "TorsoLower_3": [("core:enemy/zombie-2", "body/torso-lower")],
    "Head_4": [("core:enemy/zombie-grabber", "body/head")],
    "TorsoLower_4": [("core:enemy/zombie-grabber", "body/torso-lower")],
}

# Alex.prefab's four authored skin sprites for every body part changed by Prey.
# The eye-cover sprites use the same skin storage but were not part of the first
# public body-slot pass, so CoreAssetSlotBinder publishes them as face slots.
PLAYER_SLOTS = {
    "TorsoLower": "body/torso-lower/pale",
    "TorsoLower_9": "body/torso-lower/white",
    "TorsoLower_6": "body/torso-lower/tan",
    "TorsoLower_7": "body/torso-lower/black",
    "Hand_7": "body/hand/pale",
    "Hand_11": "body/hand/white",
    "Hand_0": "body/hand/tan",
    "Hand": "body/hand/black",
    "Butt_0": "body/butt/pale",
    "Butt_14": "body/butt/white",
    "Butt_4": "body/butt/tan",
    "Butt_1": "body/butt/black",
    "ArmLower_20": "body/arm-lower/pale",
    "ArmLower_16": "body/arm-lower/white",
    "ArmLower_0": "body/arm-lower/tan",
    "ArmLower_4": "body/arm-lower/black",
    "Ear_3": "body/ear/pale",
    "Ear_1": "body/ear/white",
    "Ear_0": "body/ear/tan",
    "Ear_2": "body/ear/black",
    "Head_1": "body/head/pale",
    "Head_5": "body/head/white",
    "Head_0": "body/head/tan",
    "Head_10": "body/head/black",
    "LegUpper_7": "body/leg-upper/pale",
    "LegUpper_14": "body/leg-upper/white",
    "LegUpper_13": "body/leg-upper/tan",
    "LegUpper_2": "body/leg-upper/black",
    "LegLower_0": "body/leg-lower/pale",
    "LegLower": "body/leg-lower/white",
    "LegLower_14": "body/leg-lower/tan",
    "LegLower_18": "body/leg-lower/black",
    "EyelidLowerPale": "face/eyelid-lower/pale",
    "EyelidLowerWhite": "face/eyelid-lower/white",
    "EyelidLowerTan": "face/eyelid-lower/tan",
    "EyelidLowerBlack": "face/eyelid-lower/black",
    "Hip_2": "body/hips/pale",
    "Hip_4": "body/hips/white",
    "Hip_9": "body/hips/tan",
    "Hip_7": "body/hips/black",
    "Foot_8": "body/foot/pale",
    "Foot_13": "body/foot/white",
    "Foot_14": "body/foot/tan",
    "Foot_5": "body/foot/black",
    "ArmUpper_14": "body/arm-upper/pale",
    "ArmUpper_7": "body/arm-upper/white",
    "ArmUpper_21": "body/arm-upper/tan",
    "ArmUpper_12": "body/arm-upper/black",
    "Chest_8": "body/chest/pale",
    "Chest_13": "body/chest/white",
    "Chest_2": "body/chest/tan",
    "Chest_0": "body/chest/black",
    "Neck_0": "body/neck/pale",
    "Neck_2": "body/neck/white",
    "Neck": "body/neck/tan",
    "Neck_7": "body/neck/black",
}


def safe_name(value):
    return re.sub(r"[^A-Za-z0-9._-]+", "-", value).strip("-.") or "sprite"


def stable_slug(value):
    value = re.sub(r"([a-z0-9])([A-Z])", r"\1-\2", value)
    return safe_name(value.replace("_", "-")).lower()


def named_sprite_target(source_name):
    return EXISTING_NAMED_TARGETS.get(source_name, "core:sprite/" + stable_slug(source_name))


def rgba(image):
    return image.convert("RGBA")


def normalized(image):
    image = rgba(image)
    bounds = image.getchannel("A").getbbox()
    return (image.crop(bounds) if bounds else image).resize((32, 32), Image.Resampling.NEAREST)


def image_score(left, right):
    left_bytes = normalized(left).tobytes()
    right_image = normalized(right)
    candidates = (right_image, right_image.transpose(Image.Transpose.FLIP_TOP_BOTTOM))
    return min(sum(abs(a - b) for a, b in zip(left_bytes, candidate.tobytes())) for candidate in candidates)


def read_sprites(path):
    result = {}
    environment = UnityPy.load(str(path))
    for reader in environment.objects:
        if reader.type.name != "Sprite":
            continue
        sprite = reader.read()
        texture = sprite.m_RD.texture.deref_parse_as_object()
        result[reader.path_id] = {
            "name": str(sprite.m_Name),
            "sprite": rgba(sprite.image),
            "texture": rgba(texture.image),
        }
    return result


def source_candidates(sprite_name, texture_root):
    wanted = sprite_name.casefold()
    return sorted(path for path in texture_root.glob("*.png") if wanted in path.stem.casefold())


def best_source_name(original_image, sprite_name, texture_root):
    scored = []
    for path in source_candidates(sprite_name, texture_root):
        try:
            scored.append((image_score(original_image, Image.open(path)), path.stem))
        except OSError:
            continue
    if not scored:
        return None, None, []
    scored.sort(key=lambda item: (item[0], item[1]))
    best_score, best_name = scored[0]
    ties = [name for score, name in scored if score == best_score]
    return best_name, best_score, ties


def clothing_sources(catalog_path):
    catalog = json.loads(catalog_path.read_text(encoding="utf-8"))
    index = {}
    for entry in catalog["entries"]:
        target = entry["id"]
        icon = entry.get("icon") or {}
        source = icon.get("sourceAsset")
        if source:
            index.setdefault(Path(source).stem, []).append((target, "icon"))
        for piece in entry.get("pieces") or []:
            sprite = piece.get("sprite") or {}
            source = sprite.get("sourceAsset")
            if source:
                index.setdefault(Path(source).stem, []).append((target, piece["slot"]))
    return index


def write_json(path, value):
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(value, indent=2) + "\n", encoding="utf-8")


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--prey", required=True, type=Path, help="Prey sharedassets0.assets")
    parser.add_argument("--baseline", required=True, type=Path, help="Captivity v1.0.5b sharedassets0.assets")
    parser.add_argument("--output", type=Path, default=PROJECT_ROOT / "ExampleMods" / "prey-bunny-girls")
    args = parser.parse_args()

    current = read_sprites(args.prey)
    original = read_sprites(args.baseline)
    texture_root = PROJECT_ROOT / "Assets" / "Texture2D"
    clothing_index = clothing_sources(PROJECT_ROOT / "Assets" / "Resources" / "Modding" / "Core" / "Content" / "core-clothing.json")
    output = args.output.resolve()
    content_root = output / "content"
    asset_root = output / "assets" / "legacy"
    # These two directories are entirely generated by this converter. Clearing
    # them makes repeated runs deterministic and prevents removed mappings from
    # lingering as active content documents.
    if content_root.exists():
        shutil.rmtree(content_root)
    if asset_root.exists():
        shutil.rmtree(asset_root)
    content_root.mkdir(parents=True, exist_ok=True)
    asset_root.mkdir(parents=True, exist_ok=True)

    player = {}
    clothing = {}
    enemies = {}
    records = []
    overrides = []
    for path_id in sorted(set(current) & set(original)):
        new = current[path_id]
        old = original[path_id]
        if new["sprite"].tobytes() == old["sprite"].tobytes():
            continue
        source_name, score, ties = best_source_name(old["sprite"], old["name"], texture_root)
        source_name = SOURCE_NAME_OVERRIDES.get(path_id, source_name)
        record = {
            "pathId": path_id,
            "archiveName": new["name"],
            "sourceName": source_name,
            "sourceMatchScore": score,
            "sourceMatchTies": ties,
            "status": "compression-noise" if path_id in NOISE_PATH_IDS else "unmapped",
        }
        if path_id in NOISE_PATH_IDS:
            records.append(record)
            continue

        filename = f"{path_id}-{safe_name(source_name or new['name'])}.png"
        relative_asset = "assets/legacy/" + filename
        new["texture"].save(asset_root / filename, format="PNG")

        if source_name in PLAYER_SLOTS:
            slot = PLAYER_SLOTS[source_name]
            player[slot] = relative_asset
            overrides.append("core:player/" + slot)
            record["status"] = "player"
            record["targets"] = ["core:player/" + slot]

        targets = []
        for target, slot in clothing_index.get(source_name, []):
            clothing.setdefault(target, {})[slot] = relative_asset
            full_slot = target + "/" + slot
            overrides.append(full_slot)
            targets.append(full_slot)
        if targets:
            record["status"] = "clothing" if record["status"] == "unmapped" else "player+clothing"
            record.setdefault("targets", []).extend(targets)

        enemy_targets = []
        for target, slot in CORE_ENEMY_TARGETS.get(source_name, []):
            enemies.setdefault(target, {})[slot] = relative_asset
            full_slot = target + "/" + slot
            overrides.append(full_slot)
            enemy_targets.append(full_slot)
        if enemy_targets:
            record["status"] = "enemy"
            record.setdefault("targets", []).extend(enemy_targets)

        if record["status"] == "unmapped":
            target = named_sprite_target(source_name or new["name"])
            slug = target.split("/", 1)[1]
            write_json(content_root / ("sprite-" + slug + ".json"), {
                "schemaVersion": 1,
                "type": "assetPatch",
                "id": PACK_ID + ":patch/sprite-" + slug,
                "target": target,
                "replacements": {"image": relative_asset},
            })
            full_slot = target + "/image"
            overrides.append(full_slot)
            record["status"] = "named-sprite"
            record["targets"] = [full_slot]
        records.append(record)

    if player:
        write_json(content_root / "player-body.json", {
            "schemaVersion": 1,
            "type": "assetPatch",
            "id": PACK_ID + ":patch/player-body",
            "target": "core:player",
            "replacements": dict(sorted(player.items())),
        })

    for target, replacements in sorted(clothing.items()):
        slug = target.split("/", 1)[1]
        write_json(content_root / ("clothing-" + slug + ".json"), {
            "schemaVersion": 1,
            "type": "assetPatch",
            "id": PACK_ID + ":patch/clothing-" + slug,
            "target": target,
            "replacements": dict(sorted(replacements.items())),
        })

    for target, replacements in sorted(enemies.items()):
        slug = target.split("/", 1)[1]
        write_json(content_root / ("enemy-" + slug + ".json"), {
            "schemaVersion": 1,
            "type": "assetPatch",
            "id": PACK_ID + ":patch/enemy-" + slug,
            "target": target,
            "replacements": dict(sorted(replacements.items())),
        })

    write_json(output / "conversion-report.json", {
        "source": "Prey Mod (Bunny Girls) sharedassets0.assets",
        "baseline": "Captivity v1.0.5b sharedassets0.assets",
        "changedTextureCount": len(records),
        "excludedCompressionNoise": sorted(NOISE_PATH_IDS),
        "convertedPlayerSlots": len(player),
        "convertedClothingTargets": len(clothing),
        "convertedEnemyTargets": len(enemies),
        "convertedNamedSpriteTargets": sum(1 for record in records if record["status"] == "named-sprite"),
        "records": records,
    })
    write_json(output / "manifest.json", {
        "schemaVersion": 1,
        "id": PACK_ID,
        "displayName": "Prey / Bunny Girls Overhaul",
        "version": "0.1.0",
        "modApiVersion": 1,
        "authors": ["Draco66Electro"],
        "description": "Faithful data-only conversion of the legacy Prey/Bunny Girls visual overhaul.",
        "dependencies": [{"id": "core", "version": ">=0.1.0"}],
        "conflicts": [
            "dudleytheschemer.shaded-girl",
            "legacy.smaller-breast",
            "legacy.smaller-breast-and-butt",
            "mousai.femboy-refitted-shirt",
        ],
        "overrides": sorted(set(overrides)),
        "priority": 25,
        "contentRoots": ["content"],
    })

    converted = sum(1 for record in records if record["status"] not in ("unmapped", "compression-noise"))
    unmapped = sum(1 for record in records if record["status"] == "unmapped")
    print(f"Prey conversion: {converted} mapped, {unmapped} retained for later mapping, "
          f"{len(NOISE_PATH_IDS)} compression-only records excluded.")


if __name__ == "__main__":
    main()
