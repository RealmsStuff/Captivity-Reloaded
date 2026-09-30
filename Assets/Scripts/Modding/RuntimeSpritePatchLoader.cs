using System;
using System.Collections.Generic;
using UnityEngine;

namespace CaptivityReloaded.Modding
{
	public static class RuntimeSpritePatchLoader
	{
		public static List<AssetPatchRequest> Load(IEnumerable<AssetPatchDefinition> i_definitions,
			IEnumerable<ModPack> i_packs, AssetSlotRegistry i_slots, ValidationReport io_report,
			ISet<ContentId> i_excludedSlots = null, bool i_deferUnknownSlots = false)
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
					if (i_excludedSlots != null && i_excludedSlots.Contains(replacement.SlotId)) continue;
					if (!i_slots.TryGet(replacement.SlotId, out AssetSlotRegistration slot))
					{
						if (i_deferUnknownSlots) continue;
						io_report?.Add(ValidationSeverity.Error, "asset-patch.target", "Patch targets an unknown public asset slot: " + replacement.SlotId, definition.Source);
						continue;
					}
					if (!(slot.BaselineAsset is Sprite baseline))
					{
						io_report?.Add(ValidationSeverity.Error, "asset-patch.runtime-type", "PNG replacement requires a Sprite slot: " + replacement.SlotId, definition.Source);
						continue;
					}
					if (!RuntimePngAssetLoader.TryLoad(pack.RootPath, replacement.AssetPath,
						definition.Id + "/" + replacement.SlotId.Path, FilterMode.Point, io_report,
						"asset-patch.file", "asset-patch.decode", definition.Source, out Texture2D texture)) continue;
					Vector2 pivot = RuntimePngAssetLoader.GetReplacementPivot(baseline, texture.width, texture.height);
					Rect rect = new Rect(0, 0, texture.width, texture.height);
					Sprite sprite = Sprite.Create(texture, rect, pivot, baseline.pixelsPerUnit, 0, SpriteMeshType.FullRect, baseline.border);
					sprite.name = texture.name;
					bool authorized = pack.Manifest.Overrides != null && pack.Manifest.Overrides.Contains(replacement.SlotId.ToString());
					requests.Add(new AssetPatchRequest(definition.Id, replacement.SlotId, definition.PackId, definition.Source, sprite, authorized));
				}
			}
			return requests;
		}
	}
}
