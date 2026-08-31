using System;
using System.Collections.Generic;
using UnityEngine;

namespace CaptivityReloaded.Modding
{
	public static class RuntimeSpritePatchLoader
	{
		public static List<AssetPatchRequest> Load(IEnumerable<AssetPatchDefinition> i_definitions,
			IEnumerable<ModPack> i_packs, AssetSlotRegistry i_slots, ValidationReport io_report)
		{
			List<AssetPatchRequest> requests = new List<AssetPatchRequest>();
			Dictionary<string, ModPack> packs = new Dictionary<string, ModPack>(StringComparer.Ordinal);
			foreach (ModPack pack in i_packs ?? new ModPack[0])
				if (pack?.Manifest != null) packs[pack.Manifest.Id] = pack;

			foreach (AssetPatchDefinition definition in i_definitions ?? new AssetPatchDefinition[0])
			{
				if (!packs.TryGetValue(definition.PackId, out ModPack pack))
				{
					io_report?.Add(ValidationSeverity.Error, "asset-patch.pack", "Defining pack is not loaded: " + definition.PackId, definition.Source);
					continue;
				}
				foreach (AssetReplacementDefinition replacement in definition.Replacements)
				{
					if (!i_slots.TryGet(replacement.SlotId, out AssetSlotRegistration slot))
					{
						io_report?.Add(ValidationSeverity.Error, "asset-patch.target", "Patch targets an unknown public asset slot: " + replacement.SlotId, definition.Source);
						continue;
					}
					if (!(slot.BaselineAsset is Sprite baseline))
					{
						io_report?.Add(ValidationSeverity.Error, "asset-patch.runtime-type", "PNG replacement requires a Sprite slot: " + replacement.SlotId, definition.Source);
						continue;
					}
					if (!RuntimePngAssetLoader.TryLoad(pack.RootPath, replacement.AssetPath,
						definition.Id + "/" + replacement.SlotId.Path, baseline.texture.filterMode, io_report,
						"asset-patch.file", "asset-patch.decode", definition.Source, out Texture2D texture)) continue;
					Vector2 pivot = new Vector2(baseline.pivot.x / baseline.rect.width, baseline.pivot.y / baseline.rect.height);
					Rect rect = baseline.rect;
					if (rect.xMin < 0 || rect.yMin < 0 || rect.xMax > texture.width || rect.yMax > texture.height)
						rect = new Rect(0, 0, texture.width, texture.height);
					Sprite sprite = Sprite.Create(texture, rect, pivot, baseline.pixelsPerUnit, 0, SpriteMeshType.FullRect, baseline.border);
					sprite.name = texture.name;
					requests.Add(new AssetPatchRequest(definition.Id, replacement.SlotId, definition.PackId, definition.Source, sprite));
				}
			}
			return requests;
		}
	}
}
