using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using FilePath = System.IO.Path;

namespace CaptivityReloaded.Editor.Modding
{
	/// <summary>
	/// Writes the Core animation reference that tools outside Unity read: every Core enemy's semantic rig and
	/// normalized clips, the player rig and its reference clips, and a sprite atlas per rig cut from the Core art.
	/// Standalone builds carry it in StreamingAssets/CoreAnimationReference (CoreAnimationReferenceBuildStep).
	/// </summary>
	public static class CoreAnimationReferenceExporter
	{
		public const string FolderName = "CoreAnimationReference";
		public const int FormatVersion = 1;
		private const string EnemyRigRoot = "ModSDK/AnimationReference/NormalizedEnemies";
		private const string EnemyClipRoot = "Assets/Resources/Modding/Core/Content/Animations";
		private const string PlayerRoot = "ModSDK/AnimationReference/Normalized";
		private const string PlayerPrefabPath = "Assets/Actors/Players/Alex.prefab";
		private const int Padding = 1;

		[MenuItem("Captivity Reloaded/Modding/Export Core Animation Reference...")]
		private static void ExportFromMenu()
		{
			string folder = EditorUtility.SaveFolderPanel("Export Core Animation Reference", string.Empty, string.Empty);
			if (string.IsNullOrEmpty(folder)) return;
			CoreAnimationReferenceSummary summary = Export(FilePath.Combine(folder, FolderName));
			EditorUtility.DisplayDialog("Core animation reference", summary.ToString(), "OK");
		}

		/// <summary>Replaces i_outputRoot with a fresh reference. A folder that is not an earlier reference is refused.</summary>
		public static CoreAnimationReferenceSummary Export(string i_outputRoot)
		{
			string root = FilePath.GetFullPath(i_outputRoot);
			ClearEarlierExport(root);
			Directory.CreateDirectory(root);
			CoreAnimationReferenceSummary summary = new CoreAnimationReferenceSummary { OutputRoot = root };
			Dictionary<string, Texture2DPixels> textures = new Dictionary<string, Texture2DPixels>(StringComparer.Ordinal);
			JArray enemies = new JArray();
			foreach (string rigFolder in Directory.GetDirectories(ProjectPath(EnemyRigRoot)).OrderBy(i_path => i_path, StringComparer.Ordinal))
			{
				string rigPath = FilePath.Combine(rigFolder, "rig.json");
				if (!File.Exists(rigPath)) continue;
				string slug = FilePath.GetFileName(rigFolder);
				string folder = FilePath.Combine(root, "enemies", slug);
				JObject rig = JObject.Parse(File.ReadAllText(rigPath));
				List<SpriteKey> sprites = new List<SpriteKey>();
				foreach (JObject bone in (rig["bones"] as JArray ?? new JArray()).OfType<JObject>())
					foreach (JObject sprite in (bone["sprites"] as JArray ?? new JArray()).OfType<JObject>())
						sprites.Add(new SpriteKey(sprite.Value<string>("asset"), sprite.Value<string>("name")));
				Directory.CreateDirectory(folder);
				File.Copy(rigPath, FilePath.Combine(folder, "rig.json"));
				int clips = CopyClips(ProjectPath(EnemyClipRoot + "/" + slug), "enemyAnimation", FilePath.Combine(folder, "clips"), sprites);
				AtlasResult atlas = WriteAtlas(folder, sprites, null, textures);
				summary.Missing.AddRange(atlas.Missing.Select(i_key => slug + ": " + i_key));
				enemies.Add(new JObject
				{
					["id"] = rig.Value<string>("enemy"), ["rig"] = rig.Value<string>("id"), ["folder"] = "enemies/" + slug,
					["clips"] = clips, ["sprites"] = atlas.Count
				});
				summary.Enemies++;
				summary.Clips += clips;
				summary.Sprites += atlas.Count;
			}

			JObject player = null;
			string playerRigPath = ProjectPath(PlayerRoot + "/player-rig.json");
			if (File.Exists(playerRigPath))
			{
				string folder = FilePath.Combine(root, "player");
				Directory.CreateDirectory(folder);
				File.Copy(playerRigPath, FilePath.Combine(folder, "rig.json"));
				Dictionary<string, SpriteKey> bodies = PlayerBodySprites();
				List<SpriteKey> sprites = bodies.Values.ToList();
				int clips = CopyClips(ProjectPath(PlayerRoot), "playerAnimation", FilePath.Combine(folder, "clips"), sprites);
				AtlasResult atlas = WriteAtlas(folder, sprites, bodies, textures);
				summary.Missing.AddRange(atlas.Missing.Select(i_key => "player: " + i_key));
				player = new JObject
				{
					["rig"] = JObject.Parse(File.ReadAllText(playerRigPath)).Value<string>("id"), ["folder"] = "player",
					["clips"] = clips, ["sprites"] = atlas.Count
				};
				summary.Clips += clips;
				summary.Sprites += atlas.Count;
			}

			JObject index = new JObject
			{
				["schemaVersion"] = 1, ["type"] = "coreAnimationReference", ["formatVersion"] = FormatVersion,
				["gameVersion"] = Application.version, ["enemies"] = enemies, ["player"] = player
			};
			File.WriteAllText(FilePath.Combine(root, "index.json"), index.ToString(Formatting.Indented) + "\n");
			return summary;
		}

		// The output folder is rewritten on every build, so only a folder this exporter made may be deleted.
		private static void ClearEarlierExport(string i_root)
		{
			if (!Directory.Exists(i_root)) return;
			if (!Directory.EnumerateFileSystemEntries(i_root).Any()) return;
			string indexPath = FilePath.Combine(i_root, "index.json");
			JObject index = null;
			try { if (File.Exists(indexPath)) index = JObject.Parse(File.ReadAllText(indexPath)); }
			catch (JsonException) { }
			if (index?.Value<string>("type") != "coreAnimationReference")
				throw new InvalidOperationException(i_root + " already exists and is not a Core animation reference; choose an empty folder.");
			Directory.Delete(i_root, true);
		}

		// Copies the folder's clips of one type and adds the sprites their sprite-swap keys show.
		private static int CopyClips(string i_source, string i_type, string i_target, List<SpriteKey> io_sprites)
		{
			if (!Directory.Exists(i_source)) return 0;
			int count = 0;
			foreach (string path in Directory.GetFiles(i_source, "*.json").OrderBy(i_path => i_path, StringComparer.Ordinal))
			{
				JObject clip;
				try { clip = JObject.Parse(File.ReadAllText(path)); }
				catch (JsonException) { continue; }
				if (clip.Value<string>("type") != i_type) continue;
				Directory.CreateDirectory(i_target);
				File.Copy(path, FilePath.Combine(i_target, FilePath.GetFileName(path)));
				foreach (JObject track in (clip["objectTracks"] as JArray ?? new JArray()).OfType<JObject>())
					foreach (JObject key in (track["keys"] as JArray ?? new JArray()).OfType<JObject>())
						if (!string.IsNullOrEmpty(key.Value<string>("asset"))) io_sprites.Add(new SpriteKey(key.Value<string>("asset"), key.Value<string>("name")));
				count++;
			}
			return count;
		}

		// The sprite each player bone draws in Alex's prefab, as the player rig exporter finds the bones.
		private static Dictionary<string, SpriteKey> PlayerBodySprites()
		{
			Dictionary<string, SpriteKey> result = new Dictionary<string, SpriteKey>(StringComparer.Ordinal);
			GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
			SkeletonPlayer skeleton = prefab == null ? null : prefab.GetComponentInChildren<SkeletonPlayer>(true);
			if (skeleton == null) return result;
			foreach (BonePlayer bone in skeleton.GetComponentsInChildren<BonePlayer>(true))
			{
				string semantic = NormalizedPlayerAnimationExporter.NormalizeBone(bone.GetBoneType().ToString());
				SpriteRenderer renderer = bone.GetBodyPart() == null ? null : bone.GetBodyPart().GetComponent<SpriteRenderer>();
				if (semantic == null || renderer == null || renderer.sprite == null || result.ContainsKey(semantic)) continue;
				result[semantic] = new SpriteKey(AssetDatabase.GetAssetPath(renderer.sprite), renderer.sprite.name);
			}
			return result;
		}

		// Cuts each sprite out of its texture and packs them on shelves into sprites.png; sprites.json gives each
		// one's rectangle (pixels from the image's bottom-left, like a pack's atlas regions) and pivot.
		private static AtlasResult WriteAtlas(string i_folder, List<SpriteKey> i_sprites, Dictionary<string, SpriteKey> i_bones,
			Dictionary<string, Texture2DPixels> io_textures)
		{
			AtlasResult result = new AtlasResult();
			List<Cut> cuts = new List<Cut>();
			foreach (SpriteKey key in i_sprites.Distinct())
			{
				Cut cut = CutSprite(key, io_textures);
				if (cut == null) result.Missing.Add(key.ToString());
				else cuts.Add(cut);
			}
			int width = 64;
			int widest = cuts.Count == 0 ? 0 : cuts.Max(i_cut => i_cut.Width) + Padding * 2;
			int area = cuts.Sum(i_cut => (i_cut.Width + Padding) * (i_cut.Height + Padding));
			while (width < widest || width * width < area * 2) width *= 2;
			int x = Padding, y = Padding, shelf = 0;
			foreach (Cut cut in cuts.OrderByDescending(i_cut => i_cut.Height).ThenBy(i_cut => i_cut.Key.ToString(), StringComparer.Ordinal))
			{
				if (x + cut.Width + Padding > width) { x = Padding; y += shelf + Padding; shelf = 0; }
				cut.AtlasX = x; cut.AtlasY = y;
				x += cut.Width + Padding;
				shelf = Mathf.Max(shelf, cut.Height);
			}
			int height = Mathf.Max(4, y + shelf + Padding);
			Color32[] pixels = new Color32[width * height];
			foreach (Cut cut in cuts)
				for (int row = 0; row < cut.Height; row++)
					Array.Copy(cut.Pixels, row * cut.Width, pixels, (cut.AtlasY + row) * width + cut.AtlasX, cut.Width);
			Texture2D image = new Texture2D(width, height, TextureFormat.RGBA32, false);
			try
			{
				image.SetPixels32(pixels);
				image.Apply();
				File.WriteAllBytes(FilePath.Combine(i_folder, "sprites.png"), image.EncodeToPNG());
			}
			finally { UnityEngine.Object.DestroyImmediate(image); }

			JObject document = new JObject
			{
				["schemaVersion"] = 1, ["type"] = "spriteAtlasReference", ["image"] = "sprites.png", ["width"] = width, ["height"] = height,
				["sprites"] = new JArray(cuts.OrderBy(i_cut => i_cut.Key.ToString(), StringComparer.Ordinal).Select(i_cut => new JObject
				{
					["asset"] = i_cut.Key.Asset, ["name"] = i_cut.Key.Name,
					["x"] = i_cut.AtlasX, ["y"] = i_cut.AtlasY, ["width"] = i_cut.Width, ["height"] = i_cut.Height,
					["pivotX"] = Math.Round(i_cut.PivotX, 6), ["pivotY"] = Math.Round(i_cut.PivotY, 6), ["pixelsPerUnit"] = i_cut.PixelsPerUnit
				}))
			};
			if (i_bones != null)
				document["bones"] = new JObject(i_bones.OrderBy(i_pair => i_pair.Key, StringComparer.Ordinal)
					.Select(i_pair => new JProperty(i_pair.Key, new JObject { ["asset"] = i_pair.Value.Asset, ["name"] = i_pair.Value.Name })));
			if (result.Missing.Count > 0) document["missing"] = new JArray(result.Missing);
			File.WriteAllText(FilePath.Combine(i_folder, "sprites.json"), document.ToString(Formatting.Indented) + "\n");
			result.Count = cuts.Count;
			return result;
		}

		private static Cut CutSprite(SpriteKey i_key, Dictionary<string, Texture2DPixels> io_textures)
		{
			if (string.IsNullOrEmpty(i_key.Asset)) return null;
			Sprite sprite = AssetDatabase.LoadAllAssetsAtPath(i_key.Asset).OfType<Sprite>().FirstOrDefault(i_sprite => i_sprite.name == i_key.Name)
				?? AssetDatabase.LoadAssetAtPath<Sprite>(i_key.Asset);
			if (sprite == null || sprite.texture == null) return null;
			Texture2DPixels texture = LoadTexture(sprite.texture, io_textures);
			if (texture == null) return null;
			Rect rect = sprite.rect;
			int x0 = Mathf.FloorToInt(rect.xMin), y0 = Mathf.FloorToInt(rect.yMin);
			int width = Mathf.Max(1, Mathf.CeilToInt(rect.xMax) - x0), height = Mathf.Max(1, Mathf.CeilToInt(rect.yMax) - y0);
			Color32[] pixels = new Color32[width * height];
			for (int row = 0; row < height; row++)
				for (int column = 0; column < width; column++)
				{
					int tx = x0 + column, ty = y0 + row;
					if (tx >= 0 && ty >= 0 && tx < texture.Width && ty < texture.Height) pixels[row * width + column] = texture.Pixels[ty * texture.Width + tx];
				}
			Vector2 pivot = rect.position + sprite.pivot - new Vector2(x0, y0);
			return new Cut
			{
				Key = i_key, Width = width, Height = height, Pixels = pixels,
				PivotX = pivot.x / width, PivotY = pivot.y / height, PixelsPerUnit = sprite.pixelsPerUnit
			};
		}

		// Reads the texture file itself, so this works in -nographics batch builds and never changes a Core
		// texture's import settings; a texture imported at another size than its file must be readable instead.
		private static Texture2DPixels LoadTexture(Texture2D i_texture, Dictionary<string, Texture2DPixels> io_cache)
		{
			string path = AssetDatabase.GetAssetPath(i_texture);
			if (io_cache.TryGetValue(path, out Texture2DPixels cached)) return cached;
			Texture2DPixels result = null;
			string full = ProjectPath(path);
			if (File.Exists(full) && (path.EndsWith(".png", StringComparison.OrdinalIgnoreCase) || path.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)))
			{
				Texture2D decoded = new Texture2D(2, 2, TextureFormat.RGBA32, false);
				try
				{
					if (decoded.LoadImage(File.ReadAllBytes(full)) && decoded.width == i_texture.width && decoded.height == i_texture.height)
						result = new Texture2DPixels { Width = decoded.width, Height = decoded.height, Pixels = decoded.GetPixels32() };
				}
				finally { UnityEngine.Object.DestroyImmediate(decoded); }
			}
			if (result == null && i_texture.isReadable)
				result = new Texture2DPixels { Width = i_texture.width, Height = i_texture.height, Pixels = i_texture.GetPixels32() };
			io_cache[path] = result;
			return result;
		}

		private static string ProjectPath(string i_path) { return FilePath.GetFullPath(FilePath.Combine(Application.dataPath, "..", i_path)); }

		private readonly struct SpriteKey : IEquatable<SpriteKey>
		{
			public readonly string Asset;
			public readonly string Name;
			public SpriteKey(string i_asset, string i_name) { Asset = i_asset ?? string.Empty; Name = i_name ?? string.Empty; }
			public bool Equals(SpriteKey i_other) { return Asset == i_other.Asset && Name == i_other.Name; }
			public override bool Equals(object i_other) { return i_other is SpriteKey other && Equals(other); }
			public override int GetHashCode() { return (Asset.GetHashCode() * 397) ^ Name.GetHashCode(); }
			public override string ToString() { return Asset + "#" + Name; }
		}

		private sealed class Texture2DPixels { public int Width; public int Height; public Color32[] Pixels; }

		private sealed class Cut
		{
			public SpriteKey Key; public int Width; public int Height; public Color32[] Pixels;
			public float PivotX; public float PivotY; public float PixelsPerUnit; public int AtlasX; public int AtlasY;
		}

		private sealed class AtlasResult { public int Count; public readonly List<string> Missing = new List<string>(); }
	}

	public sealed class CoreAnimationReferenceSummary
	{
		public string OutputRoot;
		public int Enemies;
		public int Clips;
		public int Sprites;
		public readonly List<string> Missing = new List<string>();

		public override string ToString()
		{
			return Enemies + " enemies, " + Clips + " clips and " + Sprites + " sprites in " + OutputRoot
				+ (Missing.Count == 0 ? "." : "; " + Missing.Count + " sprites could not be read: " + string.Join(", ", Missing.Take(8)) + (Missing.Count > 8 ? ", ..." : "."));
		}
	}

	/// <summary>Adds the Core animation reference to every standalone player build's StreamingAssets.</summary>
	public sealed class CoreAnimationReferenceBuildStep : IPostprocessBuildWithReport
	{
		public int callbackOrder { get { return 100; } }

		public void OnPostprocessBuild(BuildReport i_report)
		{
			string streamingAssets = StreamingAssetsFolder(i_report.summary.platform, i_report.summary.outputPath);
			if (streamingAssets == null) return;
			CoreAnimationReferenceSummary summary = CoreAnimationReferenceExporter.Export(FilePath.Combine(streamingAssets, CoreAnimationReferenceExporter.FolderName));
			if (summary.Missing.Count == 0) Debug.Log("[CoreAnimationReference] " + summary);
			else Debug.LogWarning("[CoreAnimationReference] " + summary);
		}

		// Where a standalone player's StreamingAssets folder is, from the path the build was written to.
		public static string StreamingAssetsFolder(BuildTarget i_target, string i_outputPath)
		{
			if (string.IsNullOrEmpty(i_outputPath)) return null;
			switch (i_target)
			{
			case BuildTarget.StandaloneWindows:
			case BuildTarget.StandaloneWindows64:
			case BuildTarget.StandaloneLinux64:
				return FilePath.Combine(FilePath.GetDirectoryName(i_outputPath), FilePath.GetFileNameWithoutExtension(i_outputPath) + "_Data", "StreamingAssets");
			case BuildTarget.StandaloneOSX:
				return FilePath.Combine(i_outputPath, "Contents", "Resources", "Data", "StreamingAssets");
			default:
				return null;
			}
		}
	}
}
