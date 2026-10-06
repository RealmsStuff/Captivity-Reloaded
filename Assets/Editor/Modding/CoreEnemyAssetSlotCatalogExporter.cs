using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using FilePath = System.IO.Path;

namespace CaptivityReloaded.Editor.Modding
{
	/// <summary>Generates the runtime and public Core enemy sprite-slot catalogs from the normalized rig references.</summary>
	public static class CoreEnemyAssetSlotCatalogExporter
	{
		private const string RuntimeOutput = "Assets/Resources/Modding/Core/enemy-asset-slots.json";
		private const string PublicOutput = "Docs/Modding/Public/reference/enemy-asset-slots.md";
		private static readonly Dictionary<string, string> s_canonicalNames = new Dictionary<string, string>(StringComparer.Ordinal)
		{
			{ "spine", "torso-lower" }, { "butt", "butt" }, { "hips", "hips" }, { "chest", "chest" },
			{ "neck", "neck" }, { "head", "head" }, { "foot-left", "foot-left" },
			{ "foot-right", "foot-right" }, { "penis", "penis" }
		};
		private static readonly Dictionary<string, string[]> s_legacyAliases = new Dictionary<string, string[]>(StringComparer.Ordinal)
		{
			{ "body/arm-upper", new[] { "body/arm-left-upper", "body/arm-right-upper" } },
			{ "body/arm-lower", new[] { "body/arm-left-lower", "body/arm-right-lower" } },
			{ "body/hand", new[] { "body/hand-left", "body/hand-right" } },
			{ "body/leg-upper", new[] { "body/leg-left-upper", "body/leg-right-upper" } },
			{ "body/leg-lower", new[] { "body/leg-left-lower", "body/leg-right-lower" } }
		};

		[MenuItem("Captivity Reloaded/Modding/Refresh Core Enemy Asset Slot Catalog")]
		public static void ExportMenu()
		{
			int count = Export();
			AssetDatabase.Refresh();
			EditorUtility.DisplayDialog("Core enemy asset slots", "Generated " + count + " enemy-specific sprite slots.", "OK");
		}

		public static void ExportBatch()
		{
			Export();
			AssetDatabase.Refresh();
		}

		public static int Export()
		{
			string root = FilePath.GetFullPath(FilePath.Combine(Application.dataPath, ".."));
			string rigsRoot = FilePath.Combine(root, "ModSDK", "AnimationReference", "NormalizedEnemies");
			if (!Directory.Exists(rigsRoot)) throw new DirectoryNotFoundException("Normalized Core enemy rigs were not found: " + rigsRoot);

			JArray enemies = new JArray();
			int slotCount = 0;
			foreach (string rigPath in Directory.GetFiles(rigsRoot, "rig.json", SearchOption.AllDirectories).OrderBy(i_path => i_path, StringComparer.Ordinal))
			{
				JObject rig = JObject.Parse(File.ReadAllText(rigPath));
				string enemyId = (string)rig["enemy"];
				string sampleRoot = (string)rig["sampleRoot"] ?? string.Empty;
				if (string.IsNullOrEmpty(enemyId) || !enemyId.StartsWith("core:enemy/", StringComparison.Ordinal))
					throw new InvalidDataException("Normalized rig has an invalid Core enemy ID: " + rigPath);
				JArray slots = new JArray();
				HashSet<string> slotNames = new HashSet<string>(StringComparer.Ordinal);
				Dictionary<string, JObject> slotsByBone = new Dictionary<string, JObject>(StringComparer.Ordinal);
				foreach (JObject bone in ((JArray)rig["bones"] ?? new JArray()).OfType<JObject>())
				{
					string boneName = (string)bone["name"];
					JArray sprites = (JArray)bone["sprites"] ?? new JArray();
					if (sprites.Count > 1) throw new InvalidDataException(enemyId + " bone " + boneName + " has more than one renderer; assign distinct semantic bones before publishing slots.");
					if (sprites.Count == 0) continue;
					JObject sprite = (JObject)sprites[0];
					string rendererPath = CombineUnityPath(sampleRoot, (string)sprite["unityPath"]);
					if (string.IsNullOrEmpty(boneName) || string.IsNullOrEmpty(rendererPath))
						throw new InvalidDataException(enemyId + " contains a sprite renderer without a semantic bone or hierarchy path.");
					string suffix = "body/" + (s_canonicalNames.TryGetValue(boneName, out string canonical) ? canonical : boneName);
					if (!slotNames.Add(suffix)) throw new InvalidDataException(enemyId + " maps more than one renderer to " + suffix + ".");
					JObject slot = new JObject
					{
						["slot"] = suffix,
						["bone"] = boneName,
						["rendererPath"] = rendererPath,
						["spriteName"] = (string)sprite["name"]
					};
					slots.Add(slot);
					slotsByBone[boneName] = slot;
					slotCount++;
				}

				string enemyDirectory = FilePath.GetDirectoryName(rigPath);
				foreach (string animationPath in Directory.GetFiles(enemyDirectory, "*.json", SearchOption.TopDirectoryOnly)
					.Where(i_path => !string.Equals(FilePath.GetFileName(i_path), "rig.json", StringComparison.OrdinalIgnoreCase))
					.OrderBy(i_path => i_path, StringComparer.Ordinal))
				{
					JObject animation = JObject.Parse(File.ReadAllText(animationPath));
					foreach (JObject track in ((JArray)animation["objectTracks"] ?? new JArray()).OfType<JObject>())
					{
						string target = (string)track["target"];
						if (string.IsNullOrEmpty(target) || !target.StartsWith("sprite/", StringComparison.Ordinal)) continue;
						string boneName = target.Substring("sprite/".Length);
						if (!slotsByBone.TryGetValue(boneName, out JObject baseSlot))
							throw new InvalidDataException(animationPath + " targets a sprite bone that is absent from its rig: " + boneName);
						foreach (JObject key in ((JArray)track["keys"] ?? new JArray()).OfType<JObject>())
						{
							string spriteName = (string)key["name"];
							if (string.IsNullOrEmpty(spriteName) || spriteName == (string)baseSlot["spriteName"]) continue;
							string suffix = (string)baseSlot["slot"] + "/variant/" + Slug(spriteName);
							if (!slotNames.Add(suffix)) continue;
							slots.Add(new JObject
							{
								["slot"] = suffix,
								["bone"] = boneName,
								["rendererPath"] = baseSlot["rendererPath"],
								["spriteName"] = spriteName,
								["animatedSpriteName"] = spriteName,
								["sourceAnimation"] = ForwardSlash(animationPath.Substring(root.TrimEnd(FilePath.DirectorySeparatorChar, FilePath.AltDirectorySeparatorChar).Length).TrimStart(FilePath.DirectorySeparatorChar, FilePath.AltDirectorySeparatorChar))
							});
							slotCount++;
						}
					}
				}

				JArray aliases = new JArray();
				foreach (KeyValuePair<string, string[]> alias in s_legacyAliases)
				{
					string[] targets = alias.Value.Where(slotNames.Contains).ToArray();
					if (targets.Length == 0 || slotNames.Contains(alias.Key)) continue;
					aliases.Add(new JObject { ["slot"] = alias.Key, ["targets"] = new JArray(targets) });
				}

				enemies.Add(new JObject
				{
					["id"] = enemyId,
					["sourceRig"] = ForwardSlash(rigPath.Substring(root.TrimEnd(FilePath.DirectorySeparatorChar, FilePath.AltDirectorySeparatorChar).Length).TrimStart(FilePath.DirectorySeparatorChar, FilePath.AltDirectorySeparatorChar)),
					["slots"] = slots,
					["legacyAliases"] = aliases
				});
			}

			if (enemies.Count != 21) throw new InvalidDataException("Expected 21 normalized Core enemy rigs, found " + enemies.Count + ".");
			JObject document = new JObject
			{
				["schemaVersion"] = 1,
				["type"] = "coreEnemyAssetSlotCatalog",
				["enemyCount"] = enemies.Count,
				["slotCount"] = slotCount,
				["enemies"] = enemies
			};
			Write(root, RuntimeOutput, document.ToString(Formatting.Indented) + Environment.NewLine);
			Write(root, PublicOutput, BuildPublicCatalog(enemies, slotCount));
			Debug.Log("[Modding] Generated " + slotCount + " Core enemy asset slots for " + enemies.Count + " enemies.");
			return slotCount;
		}

		private static string BuildPublicCatalog(JArray i_enemies, int i_slotCount)
		{
			StringBuilder text = new StringBuilder();
			text.AppendLine("# Core enemy asset slots").AppendLine();
			text.AppendLine("This page is generated from the same normalized rig metadata used by the runtime. Do not edit it by hand. Run **Captivity Reloaded > Modding > Refresh Core Enemy Asset Slot Catalog** after changing a Core rig.").AppendLine();
			text.AppendLine("The catalog currently publishes " + i_slotCount + " enemy-specific sprite slots across " + i_enemies.Count + " Core enemies. Target the enemy ID in an `assetPatch` and use the relative slot shown below. Enemy-specific multipart anatomy slots are preferred over the older global `core:enemy-anatomy` aliases.").AppendLine();
			foreach (JObject enemy in i_enemies.OfType<JObject>())
			{
				string id = (string)enemy["id"];
				text.AppendLine("## `" + id + "`").AppendLine();
				text.AppendLine("| Relative slot | Rig bone | Core sprite | Usage |").AppendLine("| --- | --- | --- | --- |");
				foreach (JObject slot in ((JArray)enemy["slots"]).OfType<JObject>())
					text.AppendLine("| `" + slot["slot"] + "` | `" + slot["bone"] + "` | `" + (slot["spriteName"] ?? "") + "` | " +
						(slot["animatedSpriteName"] == null ? "Default" : "Animation variant") + " |");
				JArray aliases = (JArray)enemy["legacyAliases"];
				if (aliases.Count > 0)
				{
					text.AppendLine().AppendLine("Compatibility aliases:").AppendLine();
					foreach (JObject alias in aliases.OfType<JObject>())
						text.AppendLine("- `" + alias["slot"] + "` replaces: " + string.Join(", ", ((JArray)alias["targets"]).Values<string>().Select(i_target => "`" + i_target + "`").ToArray()));
				}
				text.AppendLine();
			}
			return text.ToString().TrimEnd() + Environment.NewLine;
		}

		private static void Write(string i_root, string i_relativePath, string i_content)
		{
			string path = FilePath.Combine(i_root, i_relativePath.Replace('/', FilePath.DirectorySeparatorChar));
			Directory.CreateDirectory(FilePath.GetDirectoryName(path));
			File.WriteAllText(path, i_content, new UTF8Encoding(false));
		}

		private static string ForwardSlash(string i_path)
		{
			return (i_path ?? string.Empty).Replace('\\', '/');
		}

		private static string CombineUnityPath(string i_parent, string i_child)
		{
			if (string.IsNullOrEmpty(i_parent)) return i_child ?? string.Empty;
			if (string.IsNullOrEmpty(i_child)) return i_parent;
			return i_parent.TrimEnd('/') + "/" + i_child.TrimStart('/');
		}

		private static string Slug(string i_value)
		{
			StringBuilder slug = new StringBuilder();
			foreach (char character in i_value ?? string.Empty)
			{
				if (char.IsLetterOrDigit(character))
				{
					if (char.IsUpper(character) && slug.Length > 0 && slug[slug.Length - 1] != '-') slug.Append('-');
					slug.Append(char.ToLowerInvariant(character));
				}
				else if (slug.Length > 0 && slug[slug.Length - 1] != '-') slug.Append('-');
			}
			return slug.ToString().Trim('-');
		}
	}
}
