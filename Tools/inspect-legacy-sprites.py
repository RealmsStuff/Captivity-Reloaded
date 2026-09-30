import argparse
import hashlib
import io
import os
import re
import sys

sys.path.insert(0, os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "..", ".tools", "unitypy")))
from PIL import Image


def read_sprites(path, wanted, wanted_path_ids=None):
    import UnityPy

    sprites = {}
    environment = UnityPy.load(path)
    for obj in environment.objects:
        if obj.type.name != "Sprite":
            continue
        if wanted_path_ids is not None and obj.path_id not in wanted_path_ids:
            continue
        sprite = obj.read()
        name = str(getattr(sprite, "m_Name", getattr(sprite, "name", "")))
        if not wanted.search(name):
            continue
        image = sprite.image
        encoded = io.BytesIO()
        image.save(encoded, format="PNG")
        data = encoded.getvalue()
        rect = getattr(sprite, "m_Rect", None)
        pivot = getattr(sprite, "m_Pivot", None)
        metadata = ""
        texture_data = None
        if rect is not None and pivot is not None:
            metadata = (f"rect={float(rect.x):.3f},{float(rect.y):.3f},{float(rect.width):.3f},{float(rect.height):.3f}"
                        f" pivot={float(pivot.x):.3f},{float(pivot.y):.3f} ppu={float(sprite.m_PixelsToUnits):.3f}")
        try:
            texture_pointer = sprite.m_RD.texture
            texture = texture_pointer.deref_parse_as_object()
            texture_name = str(getattr(texture, "m_Name", getattr(texture, "name", "")))
            metadata += f" texture={texture_pointer.path_id}:{texture_name}"
            texture_encoded = io.BytesIO()
            texture.image.save(texture_encoded, format="PNG")
            texture_data = texture_encoded.getvalue()
        except Exception:
            pass
        sprites[obj.path_id] = (name, image.width, image.height, data, metadata, texture_data)
    return sprites


def collect_references(path, target_path_ids):
    import UnityPy

    references = {path_id: [] for path_id in target_path_ids}
    environment = UnityPy.load(path)

    def visit(value, location, matches):
        if isinstance(value, dict):
            path_id = value.get("m_PathID")
            file_id = value.get("m_FileID")
            if file_id == 0 and path_id in target_path_ids:
                matches.add((path_id, location))
            for key, child in value.items():
                visit(child, f"{location}.{key}" if location else key, matches)
        elif isinstance(value, list):
            for index, child in enumerate(value):
                visit(child, f"{location}[{index}]", matches)

    for obj in environment.objects:
        if obj.path_id in target_path_ids:
            continue
        try:
            tree = obj.read_typetree()
        except Exception:
            continue
        matches = set()
        visit(tree, "", matches)
        if not matches:
            continue
        owner_name = str(tree.get("m_Name", "")) if isinstance(tree, dict) else ""
        for path_id, location in sorted(matches):
            references[path_id].append((obj.path_id, obj.type.name, owner_name, location))
    return references


def safe_name(value):
    value = re.sub(r"[^A-Za-z0-9._-]+", "-", value).strip("-.")
    return value or "sprite"


def normalized_image_key(image):
    image = image.convert("RGBA")
    bounds = image.getchannel("A").getbbox()
    cropped = image.crop(bounds) if bounds else image
    return cropped.width, cropped.height, hashlib.sha256(cropped.tobytes()).digest()


def build_png_index(root):
    index = {}
    for directory, _, files in os.walk(root):
        for filename in files:
            if not filename.lower().endswith(".png"):
                continue
            path = os.path.join(directory, filename)
            try:
                image = Image.open(path).convert("RGBA")
                keys = {normalized_image_key(image), normalized_image_key(image.transpose(Image.Transpose.FLIP_TOP_BOTTOM))}
            except OSError:
                continue
            relative = os.path.relpath(path, root).replace(os.sep, "/")
            for key in keys:
                index.setdefault(key, []).append(relative)
    return index


def nearest_pngs(data, root, sprite_name, limit=3):
    source = Image.open(io.BytesIO(data)).convert("RGBA")
    bounds = source.getchannel("A").getbbox()
    source = (source.crop(bounds) if bounds else source).resize((32, 32), Image.Resampling.NEAREST)
    source_bytes = source.tobytes()
    scored = []
    wanted = sprite_name.lower()
    for directory, _, files in os.walk(root):
        for filename in files:
            if not filename.lower().endswith(".png") or wanted not in filename.lower():
                continue
            path = os.path.join(directory, filename)
            try:
                candidate = Image.open(path).convert("RGBA")
            except OSError:
                continue
            for image in (candidate, candidate.transpose(Image.Transpose.FLIP_TOP_BOTTOM)):
                candidate_bounds = image.getchannel("A").getbbox()
                image = (image.crop(candidate_bounds) if candidate_bounds else image).resize((32, 32), Image.Resampling.NEAREST)
                score = sum(abs(left - right) for left, right in zip(source_bytes, image.tobytes()))
                scored.append((score, os.path.relpath(path, root).replace(os.sep, "/")))
    unique = []
    for score, path in sorted(scored):
        if path not in [item[1] for item in unique]:
            unique.append((score, path))
        if len(unique) == limit:
            break
    return unique


def image_diff_stats(left_data, right_data):
    left = Image.open(io.BytesIO(left_data)).convert("RGBA")
    right = Image.open(io.BytesIO(right_data)).convert("RGBA")
    if left.size != right.size:
        return f"size={left.width}x{left.height}->{right.width}x{right.height}"
    differing_pixels = 0
    total_absolute_difference = 0
    maximum_channel_difference = 0
    for left_pixel, right_pixel in zip(left.get_flattened_data(), right.get_flattened_data()):
        differences = [abs(left_channel - right_channel)
                       for left_channel, right_channel in zip(left_pixel, right_pixel)]
        if any(differences):
            differing_pixels += 1
            total_absolute_difference += sum(differences)
            maximum_channel_difference = max(maximum_channel_difference, max(differences))
    return (f"pixels={differing_pixels}/{left.width * left.height}"
            f" total={total_absolute_difference} max={maximum_channel_difference}")


def main():
    parser = argparse.ArgumentParser(description="List or compare sprites in legacy Unity asset archives.")
    parser.add_argument("asset")
    parser.add_argument("--name", default=".*", help="Case-insensitive regex applied to the sprite name")
    parser.add_argument("--compare", help="Optional original archive; only changed, added, and removed sprites are printed")
    parser.add_argument("--extract", help="Optional directory that receives PNGs printed from the primary archive")
    parser.add_argument("--extract-textures", action="store_true",
                        help="Extract each Sprite's full source Texture2D canvas instead of its tight rendered bounds")
    parser.add_argument("--match-png-root", help="Optional PNG tree searched for exact source-image matches")
    parser.add_argument("--nearest-png-root", help="Optional PNG tree searched for the closest same-named source images")
    parser.add_argument("--metadata", action="store_true", help="Print the serialized sprite rect, pivot, and pixels-per-unit")
    parser.add_argument("--path-id", action="append", type=int,
                        help="Only inspect this serialized Sprite path ID; may be supplied more than once")
    parser.add_argument("--references", action="store_true",
                        help="Print serialized objects that directly reference each selected Sprite path ID")
    parser.add_argument("--diff-stats", action="store_true",
                        help="With --compare, print decoded pixel-difference statistics")
    args = parser.parse_args()
    wanted = re.compile(args.name, re.IGNORECASE)
    wanted_path_ids = set(args.path_id) if args.path_id else None
    current = read_sprites(args.asset, wanted, wanted_path_ids)
    original = read_sprites(args.compare, wanted, wanted_path_ids) if args.compare else {}
    png_index = build_png_index(args.match_png_root) if args.match_png_root else None
    if args.extract:
        os.makedirs(args.extract, exist_ok=True)

    path_ids = sorted(set(current) | set(original)) if args.compare else sorted(current)
    printed_path_ids = []
    for path_id in path_ids:
        current_sprite = current.get(path_id)
        original_sprite = original.get(path_id)
        if args.compare and current_sprite and original_sprite and current_sprite[1:4] == original_sprite[1:4]:
            continue
        if current_sprite is None:
            name, width, height, data, metadata, texture_data = original_sprite
            print(f"removed\t{path_id}\t{name}\t{width}x{height}\t{hashlib.sha256(data).hexdigest()[:16]}")
            continue
        name, width, height, data, metadata, texture_data = current_sprite
        state = "added" if original_sprite is None else "changed"
        prefix = f"{state}\t" if args.compare else ""
        suffix = ""
        if png_index is not None:
            matches = png_index.get(normalized_image_key(Image.open(io.BytesIO(data))), [])
            suffix = "\t" + (",".join(matches) if matches else "-")
        if args.nearest_png_root:
            nearest = nearest_pngs(data, args.nearest_png_root, name)
            suffix += "\t" + ",".join(f"{path}:{score}" for score, path in nearest)
        if args.metadata:
            suffix += "\t" + metadata
        if args.diff_stats and original_sprite is not None:
            suffix += "\t" + image_diff_stats(original_sprite[3], data)
        print(f"{prefix}{path_id}\t{name}\t{width}x{height}\t{hashlib.sha256(data).hexdigest()[:16]}{suffix}")
        printed_path_ids.append(path_id)
        if args.extract:
            output = os.path.join(args.extract, f"{path_id}-{safe_name(name)}.png")
            with open(output, "wb") as stream:
                stream.write(texture_data if args.extract_textures and texture_data is not None else data)

    if args.references:
        reference_path_ids = wanted_path_ids or set(printed_path_ids)
        if not reference_path_ids:
            parser.error("--references requires selected or printed sprites")
        for path_id, owners in collect_references(args.asset, reference_path_ids).items():
            for owner_path_id, owner_type, owner_name, location in owners:
                print(f"reference\t{path_id}\t{owner_path_id}\t{owner_type}\t{owner_name}\t{location}")


if __name__ == "__main__":
    main()
