"""Inventory serialized-object differences between two legacy Unity asset files.

The tool is diagnostic only: it never writes either archive. Objects are matched by
their stable path ID and type, then compared using their serialized bytes. Output is
JSON so conversion audits can preserve exact evidence without relying on filenames.
"""

import argparse
import hashlib
import json
import os
import sys

from PIL import Image

sys.path.insert(0, os.path.abspath(os.path.join(os.path.dirname(__file__), "..", "..", ".tools", "unitypy")))


def read_value(reader):
    try:
        return reader.read()
    except Exception:
        return None


def object_name(value):
    return str(getattr(value, "m_Name", getattr(value, "name", "")) or "") if value is not None else ""


def read_objects(path):
    import UnityPy

    environment = UnityPy.load(path)
    result = {}
    for reader in environment.objects:
        key = (reader.path_id, reader.type.name)
        raw = reader.get_raw_data()
        value = read_value(reader)
        content_hash = hashlib.sha256(raw).hexdigest()
        width = None
        height = None
        if reader.type.name == "Texture2D" and value is not None:
            try:
                image = value.image.convert("RGBA")
                width, height = image.size
                content_hash = hashlib.sha256(image.tobytes()).hexdigest()
            except Exception:
                pass
        result[key] = {
            "pathId": reader.path_id,
            "type": reader.type.name,
            "name": object_name(value),
            "size": len(raw),
            "sha256": hashlib.sha256(raw).hexdigest(),
            "contentSha256": content_hash,
            "width": width,
            "height": height,
        }
    return result


def png_index(root):
    result = {}
    if not root:
        return result
    for directory, _, filenames in os.walk(root):
        for filename in filenames:
            if not filename.lower().endswith(".png"):
                continue
            path = os.path.join(directory, filename)
            try:
                image = Image.open(path).convert("RGBA")
            except OSError:
                continue
            relative = os.path.relpath(path, root).replace(os.sep, "/")
            for candidate in (image, image.transpose(Image.Transpose.FLIP_TOP_BOTTOM)):
                result.setdefault(hashlib.sha256(candidate.tobytes()).hexdigest(), []).append(relative)
    return result


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("current", help="Modified Unity asset file")
    parser.add_argument("baseline", help="Original Unity asset file")
    parser.add_argument("--changed-only", action="store_true", help="Omit unchanged records")
    parser.add_argument("--summary-only", action="store_true", help="Print counts without individual object records")
    parser.add_argument("--match-png-root", help="Optional PNG tree matched against decoded Texture2D pixels")
    args = parser.parse_args()

    current = read_objects(args.current)
    baseline = read_objects(args.baseline)
    matches = png_index(args.match_png_root)
    records = []
    for key in sorted(set(current) | set(baseline)):
        left = current.get(key)
        right = baseline.get(key)
        state = "added" if right is None else "removed" if left is None else (
            "unchanged" if left["contentSha256"] == right["contentSha256"] else "changed"
        )
        if args.changed_only and state == "unchanged":
            continue
        source = left or right
        record = {
            "state": state,
            "pathId": source["pathId"],
            "type": source["type"],
            "name": (left or {}).get("name") or (right or {}).get("name") or "",
        }
        if left is not None:
            record["currentSize"] = left["size"]
            record["currentSha256"] = left["sha256"]
            record["currentContentSha256"] = left["contentSha256"]
            if left["width"] is not None:
                record["currentDimensions"] = [left["width"], left["height"]]
                record["matchingPngs"] = matches.get(left["contentSha256"], [])
        if right is not None:
            record["baselineSize"] = right["size"]
            record["baselineSha256"] = right["sha256"]
            record["baselineContentSha256"] = right["contentSha256"]
            if right["width"] is not None:
                record["baselineDimensions"] = [right["width"], right["height"]]
        records.append(record)

    counts = {}
    for record in records:
        key = record["state"] + ":" + record["type"]
        counts[key] = counts.get(key, 0) + 1
    print(json.dumps({
        "current": os.path.abspath(args.current),
        "baseline": os.path.abspath(args.baseline),
        "counts": counts,
        "objects": [] if args.summary_only else records,
    }, indent=2))


if __name__ == "__main__":
    main()
