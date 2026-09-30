using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CaptivityReloaded.Modding;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CaptivityReloaded.Editor.Modding
{
	public static class CoreClothingMigrationExporter
	{
		private const string CatalogPath = "Assets/Resources/Modding/Core/Content/core-clothing.json";
		private const string SpriteRoot = "Assets/Resources/Modding/Core/Clothing";

		[MenuItem("Captivity Reloaded/Modding/Export Core Clothing Presentation")]
		public static void Export()
		{
			JObject catalog = JObject.Parse(File.ReadAllText(CatalogPath));
			JArray entries = (JArray)catalog["entries"];
			Dictionary<int, List<ClothingSource>> sources = FindSources(out Scene temporaryScene);
			List<string> errors = new List<string>();
			int exported = 0;
			foreach (JObject entry in entries.OfType<JObject>())
			{
				int legacyId = (int)entry["legacyId"];
				if (!sources.TryGetValue(legacyId, out List<ClothingSource> matches) || matches.Count != 1)
				{
					errors.Add("Legacy ID " + legacyId + " resolved to " + (matches == null ? 0 : matches.Count) + " clothing prefabs.");
					continue;
				}
				try { ExportEntry(entry, matches[0]); exported++; }
				catch (Exception exception) { errors.Add(entry["id"] + ": " + exception.Message); }
			}
			if (errors.Count > 0)
			{
				if (temporaryScene.IsValid()) EditorSceneManager.CloseScene(temporaryScene, true);
				Debug.LogError("[Modding] Core clothing export stopped without changing the catalog:\n" + string.Join("\n", errors));
				EditorUtility.DisplayDialog("Core clothing export", "Export failed for " + errors.Count + " entries. See Console; the catalog was not changed.", "OK");
				return;
			}
			File.WriteAllText(CatalogPath, catalog.ToString(Formatting.Indented) + Environment.NewLine);
			if (temporaryScene.IsValid()) EditorSceneManager.CloseScene(temporaryScene, true);
			AssetDatabase.Refresh();
			Debug.Log("[Modding] Exported lossless presentation metadata and sprites for " + exported + " Core clothing entries.");
			EditorUtility.DisplayDialog("Core clothing export", "Exported " + exported + " Core garments.", "OK");
		}

		private static Dictionary<int, List<ClothingSource>> FindSources(out Scene o_temporaryScene)
		{
			o_temporaryScene = default(Scene);
			Dictionary<int, List<ClothingSource>> result = new Dictionary<int, List<ClothingSource>>();
			foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Clothes" }))
			{
				string path = AssetDatabase.GUIDToAssetPath(guid);
				GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
				foreach (Clothing clothing in prefab == null ? new Clothing[0] : prefab.GetComponentsInChildren<Clothing>(true))
				{
					if (!result.TryGetValue(clothing.GetId(), out List<ClothingSource> list)) result.Add(clothing.GetId(), list = new List<ClothingSource>());
					list.Add(new ClothingSource(path, clothing));
				}
			}
			Scene main = SceneManager.GetSceneByPath("Assets/Scenes/Main.unity");
			bool alreadyLoaded = main.IsValid() && main.isLoaded;
			if (!alreadyLoaded)
			{
				main = EditorSceneManager.OpenScene("Assets/Scenes/Main.unity", OpenSceneMode.Additive);
				o_temporaryScene = main;
			}
			foreach (GameObject root in main.GetRootGameObjects())
				foreach (Clothing clothing in root.GetComponentsInChildren<Clothing>(true))
				{
					if (result.ContainsKey(clothing.GetId())) continue;
					result.Add(clothing.GetId(), new List<ClothingSource> { new ClothingSource("Assets/Scenes/Main.unity", clothing) });
				}
			return result;
		}

		private static void ExportEntry(JObject io_entry, ClothingSource i_source)
		{
			string contentId = (string)io_entry["id"];
			string slug = contentId.Substring(contentId.LastIndexOf('/') + 1);
			string directory = SpriteRoot + "/" + slug;
			Directory.CreateDirectory(directory);
			io_entry["migrationVersion"] = 2;
			io_entry["sourceAsset"] = i_source.Path;
			io_entry["icon"] = ExportSprite(i_source.Clothing.GetIcon(), directory + "/icon.png");
			ClothingPiece[] pieces = i_source.Clothing.GetComponentsInChildren<ClothingPiece>(true);
			Dictionary<ClothingPiece, string> ids = new Dictionary<ClothingPiece, string>();
			List<string> migrationWarnings = new List<string>();
			for (int index = 0; index < pieces.Length; index++) ids[pieces[index]] = "piece-" + index + "-" + Slug(pieces[index].name);
			JArray pieceDocuments = new JArray();
			for (int index = 0; index < pieces.Length; index++)
			{
				ClothingPiece piece = pieces[index];
				SpriteRenderer renderer = piece.GetComponent<SpriteRenderer>();
				if (renderer == null || renderer.sprite == null) throw new InvalidOperationException(piece.name + " has no SpriteRenderer artwork.");
				string slot = ClothingSlotCatalog.FromCorePieceName(piece.name);
				string spriteFile = directory + "/" + index.ToString("00") + "-" + Slug(slot) + ".png";
				List<ClothingPiece> connectedPieces = piece.GetClothingPiecesConnected() ?? new List<ClothingPiece>();
				int invalidConnections = connectedPieces.Count(connected => connected == null || !ids.ContainsKey(connected));
				if (invalidConnections > 0) migrationWarnings.Add(ids[piece] + " contains " + invalidConnections + " missing or external connected-piece reference(s).");
				pieceDocuments.Add(new JObject
				{
					["id"] = ids[piece], ["slot"] = slot, ["sprite"] = ExportSprite(renderer.sprite, spriteFile),
					["bone"] = piece.GetBoneToAttachTo().ToString(), ["offsetX"] = piece.GetLocalPosition().x,
					["offsetY"] = piece.GetLocalPosition().y, ["rotation"] = piece.GetLocalEulerAngleZ(),
					["sortingOffset"] = piece.GetSortingNumberClothingPiece(), ["attachToBone"] = piece.IsAttachToBoneInsteadOfBodyPart(),
					["hideBodyPart"] = piece.IsHideBodyPartAttachedTo(), ["droppable"] = piece.IsDroppable(),
					["destroyable"] = piece.IsDestroyable(), ["dropOnOralThrust"] = piece.IsDropOnOralThrust(),
					["destroyOnOralThrust"] = piece.IsDestroyOnOralThrust(),
					["playRipSound"] = piece.IsPlayRipSoundOnDropOrDestroy(),
					["pieceType"] = piece is ClothingPieceHat ? "hat" : "standard",
					["hidesHair"] = piece is ClothingPieceHat hat && hat.IsHidesHair(),
					["connectedPieces"] = new JArray(connectedPieces.Where(connected => connected != null && ids.ContainsKey(connected)).Select(connected => ids[connected]))
				});
			}
			io_entry["pieces"] = pieceDocuments;
			io_entry["incompatibleCategories"] = new JArray(i_source.Clothing.GetClothingCategoriesIncompatible().Select(value => value.ToString()));
			io_entry["incompatibleClothing"] = new JArray(i_source.Clothing.GetClothesIncompatible().Where(value => value != null).Select(value => value.GetId()));
			io_entry["compatibleOverrides"] = new JArray(i_source.Clothing.GetClothesCompatibleOverride().Where(value => value != null).Select(value => value.GetId()));
			io_entry["migrationWarnings"] = new JArray(migrationWarnings);
		}

		private static JObject ExportSprite(Sprite i_sprite, string i_outputPath)
		{
			if (i_sprite == null) throw new InvalidOperationException("Sprite reference is missing.");
			Rect rect = i_sprite.textureRect;
			RenderTexture temporary = RenderTexture.GetTemporary(i_sprite.texture.width, i_sprite.texture.height, 0, RenderTextureFormat.ARGB32);
			RenderTexture previous = RenderTexture.active;
			Texture2D readable = null;
			Texture2D cropped = null;
			try
			{
				Graphics.Blit(i_sprite.texture, temporary); RenderTexture.active = temporary;
				readable = new Texture2D(i_sprite.texture.width, i_sprite.texture.height, TextureFormat.RGBA32, false);
				readable.ReadPixels(new Rect(0, 0, readable.width, readable.height), 0, 0); readable.Apply();
				cropped = new Texture2D(Mathf.RoundToInt(rect.width), Mathf.RoundToInt(rect.height), TextureFormat.RGBA32, false);
				cropped.SetPixels(readable.GetPixels(Mathf.RoundToInt(rect.x), Mathf.RoundToInt(rect.y), cropped.width, cropped.height)); cropped.Apply();
				File.WriteAllBytes(i_outputPath, cropped.EncodeToPNG());
			}
			finally
			{
				RenderTexture.active = previous; RenderTexture.ReleaseTemporary(temporary);
				if (readable != null) UnityEngine.Object.DestroyImmediate(readable);
				if (cropped != null) UnityEngine.Object.DestroyImmediate(cropped);
			}
			float pivotX = i_sprite.pivot.x / i_sprite.rect.width, pivotY = i_sprite.pivot.y / i_sprite.rect.height;
			string resource = System.IO.Path.ChangeExtension(i_outputPath.Substring("Assets/Resources/".Length), null).Replace('\\', '/');
			return new JObject { ["resource"] = resource, ["sourceAsset"] = AssetDatabase.GetAssetPath(i_sprite),
				["pivotX"] = pivotX, ["pivotY"] = pivotY, ["pixelsPerUnit"] = i_sprite.pixelsPerUnit };
		}

		private static string Slug(string i_value)
		{
			return new string((i_value ?? string.Empty).ToLowerInvariant().Select(value => char.IsLetterOrDigit(value) ? value : '-').ToArray()).Trim('-');
		}

		private sealed class ClothingSource
		{
			public readonly string Path; public readonly Clothing Clothing;
			public ClothingSource(string i_path, Clothing i_clothing) { Path = i_path; Clothing = i_clothing; }
		}
	}
}
