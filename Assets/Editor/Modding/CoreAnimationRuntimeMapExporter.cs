using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CaptivityReloaded.Modding;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace CaptivityReloaded.Editor.Modding
{
	// Writes Resources/Modding/Core/core-animation-map.json from the Core rigs (ModSDK/AnimationReference/
	// NormalizedEnemies/<slug>/rig.json) and the animation catalog: each bone's transform and renderer path, each
	// Animator state's semantic clip and length, and each finisher phase's player clip. The game reads it to play
	// mod clips in place of Core Animator states. Batch: -executeMethod
	// CaptivityReloaded.Editor.Modding.CoreAnimationRuntimeMapExporter.ExportBatch
	public static class CoreAnimationRuntimeMapExporter
	{
		public const string CatalogPath = "ModSDK/AnimationReference/core-animation-catalog.json";
		public const string RigRoot = "ModSDK/AnimationReference/NormalizedEnemies";
		public const string OutputPath = "Assets/Resources/Modding/Core/core-animation-map.json";

		[MenuItem("Captivity Reloaded/Modding/Refresh Core Animation Runtime Map")]
		public static void ExportFromMenu()
		{
			Export();
			AssetDatabase.ImportAsset(OutputPath);
			Debug.Log("Wrote " + OutputPath);
		}

		public static void ExportBatch()
		{
			try { Export(); AssetDatabase.ImportAsset(OutputPath); EditorApplication.Exit(0); }
			catch (Exception exception) { Debug.LogException(exception); EditorApplication.Exit(1); }
		}

		public static void Export()
		{
			File.WriteAllText(OutputPath, Serialize(Build()).Replace("\r\n", "\n"));
		}

		public static string Serialize(CoreAnimationRuntimeMap i_map)
		{
			return JsonConvert.SerializeObject(i_map, Formatting.Indented) + "\n";
		}

		public static CoreAnimationRuntimeMap Build()
		{
			JObject catalog = JObject.Parse(File.ReadAllText(CatalogPath));
			CoreAnimationRuntimeMap map = new CoreAnimationRuntimeMap();
			foreach (JObject enemy in ((JArray)catalog["enemies"]).OfType<JObject>().OrderBy(i_enemy => (string)i_enemy["id"], StringComparer.Ordinal))
			{
				string id = (string)enemy["id"];
				string slug = id.Substring(id.IndexOf("enemy/", StringComparison.Ordinal) + "enemy/".Length);
				string rigPath = System.IO.Path.Combine(RigRoot, slug, "rig.json");
				if (!File.Exists(rigPath)) continue;
				JObject rig = JObject.Parse(File.ReadAllText(rigPath));
				CoreAnimationRuntimeEnemy entry = new CoreAnimationRuntimeEnemy { Id = id };
				foreach (JObject bone in ((JArray)rig["bones"]).OfType<JObject>())
				{
					string renderer = (bone["sprites"] as JArray)?.OfType<JObject>().Select(i_sprite => (string)i_sprite["unityPath"]).FirstOrDefault();
					entry.Bones.Add(new CoreAnimationRuntimeBone { Name = (string)bone["name"], Path = (string)bone["unityPath"] ?? string.Empty, Renderer = renderer });
				}
				Dictionary<string, float> lengths = new Dictionary<string, float>(StringComparer.Ordinal);
				foreach (JObject clip in ((JArray)enemy["enemyClips"]).OfType<JObject>()) lengths[(string)clip["name"]] = (float)clip["length"];
				foreach (JObject state in ((JArray)enemy["states"]).OfType<JObject>())
				{
					string semantic = (string)state["semanticName"];
					if (string.IsNullOrEmpty(semantic)) continue;
					string motion = (string)state["motion"] ?? string.Empty;
					entry.States.Add(new CoreAnimationRuntimeState
					{
						Layer = (string)state["layer"], Path = (string)state["path"], Semantic = semantic,
						Seconds = lengths.TryGetValue(motion, out float seconds) ? (float)Math.Round(seconds, 4) : 0f
					});
				}
				Dictionary<string, float> playerLengths = new Dictionary<string, float>(StringComparer.Ordinal);
				foreach (JObject clip in ((JArray)enemy["playerInteractionClips"] ?? new JArray()).OfType<JObject>()) playerLengths[(string)clip["name"]] = (float)clip["length"];
				foreach (JObject pair in ((JArray)enemy["interactionPairs"] ?? new JArray()).OfType<JObject>())
				{
					string playerClip = (string)pair["playerClip"];
					if (string.IsNullOrEmpty(playerClip) || entry.Phases.Any(i_phase => i_phase.Semantic == (string)pair["semanticName"])) continue;
					entry.Phases.Add(new CoreAnimationRuntimePhase
					{
						Semantic = (string)pair["semanticName"], PlayerClip = playerClip,
						PlayerSeconds = playerLengths.TryGetValue(playerClip, out float seconds) ? (float)Math.Round(seconds, 4) : 0f
					});
				}
				map.Enemies.Add(entry);
			}
			return map;
		}
	}
}
