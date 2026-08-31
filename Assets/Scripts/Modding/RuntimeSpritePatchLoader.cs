using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CaptivityReloaded.Modding
{
	public static class RuntimeSpritePatchLoader
	{
		private const int MaximumPngBytes = 32 * 1024 * 1024;

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
					string path = Path.GetFullPath(Path.Combine(pack.RootPath, replacement.AssetPath));
					if (!AssetPatchDiscovery.IsInside(path, pack.RootPath) || !File.Exists(path))
					{
						io_report?.Add(ValidationSeverity.Error, "asset-patch.file", "Replacement PNG is missing or outside its pack: " + replacement.AssetPath, definition.Source);
						continue;
					}
					try
					{
						FileInfo file = new FileInfo(path);
						if (file.Length <= 0 || file.Length > MaximumPngBytes) throw new InvalidDataException("PNG size must be between 1 byte and 32 MiB.");
						Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
						if (!texture.LoadImage(File.ReadAllBytes(path), markNonReadable: false))
						{
							UnityEngine.Object.Destroy(texture);
							throw new InvalidDataException("Unity could not decode the PNG.");
						}
						texture.name = definition.Id + "/" + replacement.SlotId.Path;
						texture.filterMode = baseline.texture.filterMode;
						Vector2 pivot = new Vector2(baseline.pivot.x / baseline.rect.width, baseline.pivot.y / baseline.rect.height);
						Rect rect = baseline.rect;
						if (rect.xMin < 0 || rect.yMin < 0 || rect.xMax > texture.width || rect.yMax > texture.height)
							rect = new Rect(0, 0, texture.width, texture.height);
						Sprite sprite = Sprite.Create(texture, rect, pivot, baseline.pixelsPerUnit, 0, SpriteMeshType.FullRect, baseline.border);
						sprite.name = texture.name;
						requests.Add(new AssetPatchRequest(definition.Id, replacement.SlotId, definition.PackId, definition.Source, sprite));
					}
					catch (Exception exception) when (exception is IOException || exception is UnauthorizedAccessException || exception is InvalidDataException)
					{
						io_report?.Add(ValidationSeverity.Error, "asset-patch.decode", exception.Message, path);
					}
				}
			}
			return requests;
		}
	}
}
