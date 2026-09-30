using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace CaptivityReloaded.Editor.Modding
{
	/// <summary>
	/// Exports the prefab-owned Core animation data in a neutral JSON format. This is an
	/// authoring reference for the future mod-maker, not a replacement runtime controller.
	/// </summary>
	public static class CoreAnimationCatalogExporter
	{
		private const string OutputDirectory = "ModSDK/AnimationReference";
		private const string OutputPath = OutputDirectory + "/core-animation-catalog.json";
		private const string EnemyPrefabDirectory = "Assets/Actors/Npcs";
		private const string PlayerInteractionDirectory = "Assets/Resources/raper";
		private const string PlayerPrefabPath = "Assets/Actors/Players/Alex.prefab";
		private const string PlayerControllerPath = "Assets/AnimatorController/DefaultPlayer.controller";

		private static readonly Dictionary<int, string> PlayerInteractionSets = new Dictionary<int, string>
		{
			{ 1, "zombie" }, { 2, "zombietwo" }, { 3, "zombiegrabber" },
			{ 4, "deathhound" }, { 5, "hunter" }, { 6, "maggot" },
			{ 7, "fly" }, { 8, "musca" }, { 9, "orc" },
			{ 10, "marksman" }, { 11, "trapper" }, { 12, "goblinminor" },
			{ 13, "litigant" }, { 14, "humpermother" }, { 15, "headhumper" },
			{ 16, "sqoid" }, { 17, "sunny" }, { 18, "jenny" },
			{ 19, "abby" }, { 20, "jacky" },
			// Zombie III reuses the original zombie player-side interaction set.
			{ 21, "zombie" }
		};

		[MenuItem("Captivity Reloaded/Modding/Export Core Animation Catalog")]
		public static void Export()
		{
			CatalogDocument catalog = BuildCatalog();
			List<EnemyAnimationSet> incomplete = catalog.Enemies
				.Where(i_enemy => string.IsNullOrEmpty(i_enemy.Controller) || i_enemy.EnemyClips.Count == 0)
				.ToList();
			if (incomplete.Count > 0)
			{
				string message = "The Core animation catalog was not written because " + incomplete.Count
					+ " enemy set(s) had no controller or clips. See the Console for their IDs.";
				Debug.LogError("[Modding] " + message + " Missing: " + string.Join(", ", incomplete.Select(i_enemy => i_enemy.Id).ToArray()));
				EditorUtility.DisplayDialog("Core animation export incomplete", message, "OK");
				return;
			}
			Directory.CreateDirectory(OutputDirectory);
			File.WriteAllText(OutputPath, JsonConvert.SerializeObject(catalog, Formatting.Indented));
			AssetDatabase.Refresh();
			Debug.Log(string.Format(
				CultureInfo.InvariantCulture,
				"[Modding] Exported {0} Core enemy animation sets and {1} clip records to {2}.",
				catalog.Enemies.Count,
				catalog.Enemies.Sum(i_enemy => i_enemy.EnemyClips.Count + i_enemy.PlayerInteractionClips.Count),
				OutputPath));
		}

		public static CatalogDocument BuildCatalog()
		{
			List<CoreEnemyRecord> coreEnemies = LoadCoreEnemyRecords();
			Dictionary<int, GameObject> prefabsByLegacyId = LoadEnemyPrefabsByLegacyId();
			CatalogDocument catalog = new CatalogDocument
			{
				SchemaVersion = 3,
				UnityVersion = Application.unityVersion,
				Purpose = "Experimental authoring reference. Runtime Core enemies still use their prefab Animator controllers.",
				PlayerPreview = ExportPlayerPreview()
			};
			if (coreEnemies.Count != 21)
				catalog.Warnings.Add("Expected 21 Core enemy definitions, but found " + coreEnemies.Count + ".");

			foreach (CoreEnemyRecord enemy in coreEnemies.OrderBy(i_enemy => i_enemy.LegacyId))
			{
				EnemyAnimationSet output = new EnemyAnimationSet
				{
					Id = enemy.Id,
					LegacyId = enemy.LegacyId,
					DisplayName = enemy.DisplayName
				};

				GameObject prefab;
				if (!prefabsByLegacyId.TryGetValue(enemy.LegacyId, out prefab))
				{
					output.Warnings.Add("No NPC prefab with this legacyId was found.");
				}
				else
				{
					output.Prefab = AssetDatabase.GetAssetPath(prefab);
					// Actor.m_animator is populated during runtime initialization on several Core
					// prefabs. Inspect the serialized hierarchy directly while in edit mode.
					Animator animator = prefab.GetComponentsInChildren<Animator>(true)
						.FirstOrDefault(i_animator => i_animator.runtimeAnimatorController != null);
					if (animator == null || animator.runtimeAnimatorController == null)
					{
						output.Warnings.Add("The NPC prefab has no assigned runtime Animator controller.");
					}
					else
					{
						output.Controller = AssetDatabase.GetAssetPath(animator.runtimeAnimatorController);
						output.States = ExportStates(animator.runtimeAnimatorController);
						output.EnemyClips = ExportClips(animator.runtimeAnimatorController.animationClips);
						AssignClipSemantics(output.States, output.EnemyClips);
					}
				}

				string interactionSet;
				if (PlayerInteractionSets.TryGetValue(enemy.LegacyId, out interactionSet))
				{
					output.PlayerInteractionSet = interactionSet;
					output.PlayerInteractionClips = ExportClips(LoadClipsInDirectory(PlayerInteractionDirectory + "/" + interactionSet));
					output.InteractionPairs = BuildInteractionPairs(output.States, output.PlayerInteractionClips);
					if (output.InteractionPairs.Count > 0)
					{
						HashSet<int> usedPhases = new HashSet<int>(output.InteractionPairs.Select(i_pair => i_pair.Phase));
						output.PlayerInteractionClips = output.PlayerInteractionClips
							.Where(i_clip => TryGetTrailingNumber(i_clip.Name, out int phase) && usedPhases.Contains(phase))
							.ToList();
					}
					if (output.PlayerInteractionClips.Count == 0)
						output.Warnings.Add("No player-side interaction clips were found for '" + interactionSet + "'.");
				}

				catalog.Enemies.Add(output);
			}

			return catalog;
		}

		private static PlayerPreviewSet ExportPlayerPreview()
		{
			PlayerPreviewSet result = new PlayerPreviewSet
			{
				Prefab = PlayerPrefabPath,
				Controller = PlayerControllerPath
			};
			RuntimeAnimatorController controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(PlayerControllerPath);
			if (controller == null)
			{
				result.Warnings.Add("The standard player Animator controller was not found.");
				return result;
			}
			result.States = ExportStates(controller);
			result.Clips = ExportClips(controller.animationClips);
			AssignClipSemantics(result.States, result.Clips);
			return result;
		}

		private static List<InteractionAnimationPair> BuildInteractionPairs(
			IEnumerable<AnimatorStateRecord> i_states,
			IEnumerable<AnimationClipRecord> i_playerClips)
		{
			Dictionary<int, AnimationClipRecord> playersByPhase = i_playerClips
				.Select(i_clip => new { Clip = i_clip, HasPhase = TryGetTrailingNumber(i_clip.Name, out int phase), Phase = phase })
				.Where(i_item => i_item.HasPhase)
				.GroupBy(i_item => i_item.Phase)
				.ToDictionary(i_group => i_group.Key, i_group => i_group.First().Clip);

			List<InteractionAnimationPair> result = new List<InteractionAnimationPair>();
			foreach (AnimatorStateRecord state in i_states)
			{
				int phase;
				if (!TryGetFinisherPhase(state.Path, out phase)) continue;
				AnimationClipRecord playerClip;
				if (!playersByPhase.TryGetValue(phase, out playerClip)) continue;
				result.Add(new InteractionAnimationPair
				{
					Phase = phase,
					SemanticName = "finisher-phase-" + phase.ToString(CultureInfo.InvariantCulture),
					EnemyLayer = state.Layer,
					EnemyState = state.Path,
					EnemyMotion = state.Motion,
					PlayerClip = playerClip.Name,
					PlayerAsset = playerClip.Asset
				});
			}
			return result.OrderBy(i_pair => i_pair.Phase).ThenBy(i_pair => i_pair.EnemyLayer, StringComparer.Ordinal).ToList();
		}

		private static bool TryGetFinisherPhase(string i_statePath, out int o_phase)
		{
			o_phase = 0;
			string state = (i_statePath ?? string.Empty).Split('/').LastOrDefault() ?? string.Empty;
			if (!state.StartsWith("Rape", StringComparison.OrdinalIgnoreCase)) return false;
			int start = 4;
			int end = start;
			while (end < state.Length && char.IsDigit(state[end])) end++;
			return end > start && int.TryParse(state.Substring(start, end - start), NumberStyles.None, CultureInfo.InvariantCulture, out o_phase);
		}

		private static bool TryGetTrailingNumber(string i_name, out int o_number)
		{
			o_number = 0;
			if (string.IsNullOrEmpty(i_name)) return false;
			int start = i_name.Length;
			while (start > 0 && char.IsDigit(i_name[start - 1])) start--;
			return start < i_name.Length && int.TryParse(i_name.Substring(start), NumberStyles.None, CultureInfo.InvariantCulture, out o_number);
		}

		private static List<CoreEnemyRecord> LoadCoreEnemyRecords()
		{
			List<CoreEnemyRecord> records = new List<CoreEnemyRecord>();
			string[] guids = AssetDatabase.FindAssets("t:TextAsset", new[] { "Assets/Resources/Modding/Core/Content" });
			foreach (string guid in guids)
			{
				string path = AssetDatabase.GUIDToAssetPath(guid);
				TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
				if (asset == null || asset.text.IndexOf("\"type\"", StringComparison.Ordinal) < 0 || asset.text.IndexOf("\"coreEnemy\"", StringComparison.Ordinal) < 0)
					continue;

				try
				{
					CoreEnemyRecord record = JsonConvert.DeserializeObject<CoreEnemyRecord>(asset.text);
					if (record != null && !string.IsNullOrWhiteSpace(record.Id)) records.Add(record);
				}
				catch (JsonException exception)
				{
					Debug.LogError("[Modding] Could not read Core enemy definition " + path + ": " + exception.Message);
				}
			}
			return records;
		}

		private static Dictionary<int, GameObject> LoadEnemyPrefabsByLegacyId()
		{
			Dictionary<int, GameObject> result = new Dictionary<int, GameObject>();
			foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { EnemyPrefabDirectory }))
			{
				GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
				NPC npc = prefab == null ? null : prefab.GetComponentInChildren<NPC>(true);
				if (npc != null && npc.GetId() > 0 && !result.ContainsKey(npc.GetId())) result.Add(npc.GetId(), prefab);
			}
			return result;
		}

		private static AnimationClip[] LoadClipsInDirectory(string i_directory)
		{
			return AssetDatabase.FindAssets("t:AnimationClip", new[] { i_directory })
				.Select(i_guid => AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDatabase.GUIDToAssetPath(i_guid)))
				.Where(i_clip => i_clip != null)
				.OrderBy(i_clip => NaturalSortKey(i_clip.name), StringComparer.Ordinal)
				.ToArray();
		}

		private static List<AnimatorStateRecord> ExportStates(RuntimeAnimatorController i_controller)
		{
			AnimatorController controller = i_controller as AnimatorController;
			List<AnimatorStateRecord> result = new List<AnimatorStateRecord>();
			if (controller == null) return result;

			foreach (AnimatorControllerLayer layer in controller.layers)
				ExportStateMachine(layer.name, string.Empty, layer.stateMachine, result);
			return result;
		}

		private static void ExportStateMachine(string i_layer, string i_parent, AnimatorStateMachine i_machine, List<AnimatorStateRecord> io_result)
		{
			foreach (ChildAnimatorState child in i_machine.states)
			{
				io_result.Add(new AnimatorStateRecord
				{
					Layer = i_layer,
					Path = string.IsNullOrEmpty(i_parent) ? child.state.name : i_parent + "/" + child.state.name,
					Motion = child.state.motion == null ? string.Empty : child.state.motion.name,
					SemanticName = GetSemanticName(i_layer, child.state.name),
					Authoring = child.state.motion != null,
					SuggestedRole = SuggestRole(child.state.name),
					Speed = child.state.speed,
					Loop = MotionLoops(child.state.motion)
				});
			}
			foreach (ChildAnimatorStateMachine child in i_machine.stateMachines)
			{
				string path = string.IsNullOrEmpty(i_parent) ? child.stateMachine.name : i_parent + "/" + child.stateMachine.name;
				ExportStateMachine(i_layer, path, child.stateMachine, io_result);
			}
		}

		private static void AssignClipSemantics(IEnumerable<AnimatorStateRecord> i_states, IEnumerable<AnimationClipRecord> io_clips)
		{
			Dictionary<string, List<string>> semanticsByMotion = i_states
				.Where(i_state => i_state.Authoring && !string.IsNullOrEmpty(i_state.Motion) && !string.IsNullOrEmpty(i_state.SemanticName))
				.GroupBy(i_state => i_state.Motion, StringComparer.Ordinal)
				.ToDictionary(i_group => i_group.Key,
					i_group => i_group.Select(i_state => i_state.SemanticName).Distinct(StringComparer.Ordinal).OrderBy(i_name => i_name, StringComparer.Ordinal).ToList(),
					StringComparer.Ordinal);
			foreach (AnimationClipRecord clip in io_clips)
				if (semanticsByMotion.TryGetValue(clip.Name, out List<string> semantics)) clip.SemanticNames = semantics;
		}

		private static string GetSemanticName(string i_layer, string i_state)
		{
			string layer = Slug(i_layer);
			string state = i_state ?? string.Empty;
			if (TryGetFinisherPhase(state, out int phase)) return "finisher-phase-" + phase.ToString(CultureInfo.InvariantCulture);
			if (!string.Equals(layer, "base-layer", StringComparison.Ordinal))
			{
				if (state.Equals("New State", StringComparison.OrdinalIgnoreCase) || state.Equals("None", StringComparison.OrdinalIgnoreCase)) return "internal-" + layer + "-empty";
				if (state.IndexOf("Idle", StringComparison.OrdinalIgnoreCase) >= 0) return layer + "-idle";
				if (state.IndexOf("Move", StringComparison.OrdinalIgnoreCase) >= 0) return layer + "-move";
				if (state.IndexOf("Active", StringComparison.OrdinalIgnoreCase) >= 0) return layer + "-active";
			}
			switch (state.ToLowerInvariant())
			{
			case "idle": return "idle";
			case "move": return "move";
			case "runaway": return "retreat";
			case "await": return "await";
			case "chase": return "chase";
			case "getup": return "get-up";
			case "climb": return "climb";
			case "jump": return "jump";
			case "prowling": return "prowl";
			case "hover": return "hover";
			case "carry": return "carry";
			case "shieldraised": return "shield-raised";
			case "headhugging": return "head-hugging";
			case "dodge": return "dodge";
			case "scare": return "scare";
			case "leap": case "attackleap": return "attack-leap";
			case "chargeprepare": case "preparecharge": return "attack-charge-prepare";
			case "attackcharge": return "attack-charge";
			case "shoot": return "attack-shoot";
			case "placetrap": return "attack-place-trap";
			case "attacksyringe": return "attack-syringe";
			case "attackclaw": return "attack-claw";
			case "attackspiderstab": return layer == "lower-body" ? "attack-spider-stab-lower" : "attack-spider-stab-upper";
			case "attackenergyblast": return "attack-energy-blast";
			case "attackcrotchkick": return "attack-crotch-kick";
			case "attackheel": return "attack-heel";
			case "attack": case "attackdefault": return layer == "base-layer" ? "attack-primary" : layer + "-attack-primary";
			case "new state": case "none": return "internal-" + layer + "-empty";
			default: return Slug(state);
			}
		}

		private static string Slug(string i_value)
		{
			if (string.IsNullOrWhiteSpace(i_value)) return "unnamed";
			System.Text.StringBuilder result = new System.Text.StringBuilder();
			bool separator = false;
			char previous = '\0';
			foreach (char character in i_value)
			{
				if (char.IsLetterOrDigit(character))
				{
					if (separator && result.Length > 0) result.Append('-');
					else if (result.Length > 0 && char.IsUpper(character) && char.IsLower(previous)) result.Append('-');
					result.Append(char.ToLowerInvariant(character));
					separator = false;
				}
				else separator = true;
				previous = character;
			}
			return result.Length == 0 ? "unnamed" : result.ToString();
		}

		private static bool MotionLoops(Motion i_motion)
		{
			AnimationClip clip = i_motion as AnimationClip;
			return clip != null && AnimationUtility.GetAnimationClipSettings(clip).loopTime;
		}

		private static List<AnimationClipRecord> ExportClips(IEnumerable<AnimationClip> i_clips)
		{
			return i_clips.Where(i_clip => i_clip != null)
				.GroupBy(i_clip => AssetDatabase.GetAssetPath(i_clip), StringComparer.OrdinalIgnoreCase)
				.Select(i_group => ExportClip(i_group.First()))
				.OrderBy(i_clip => NaturalSortKey(i_clip.Name), StringComparer.Ordinal)
				.ToList();
		}

		private static AnimationClipRecord ExportClip(AnimationClip i_clip)
		{
			AnimationClipRecord result = new AnimationClipRecord
			{
				Name = i_clip.name,
				Asset = AssetDatabase.GetAssetPath(i_clip),
				SuggestedRole = SuggestRole(i_clip.name),
				Length = i_clip.length,
				FrameRate = i_clip.frameRate,
				Loop = AnimationUtility.GetAnimationClipSettings(i_clip).loopTime
			};

			foreach (AnimationEvent animationEvent in AnimationUtility.GetAnimationEvents(i_clip))
			{
				result.Events.Add(new AnimationEventRecord
				{
					Time = animationEvent.time,
					Function = animationEvent.functionName,
					StringParameter = animationEvent.stringParameter,
					FloatParameter = animationEvent.floatParameter,
					IntParameter = animationEvent.intParameter
				});
			}

			foreach (EditorCurveBinding binding in AnimationUtility.GetCurveBindings(i_clip))
			{
				AnimationCurve curve = AnimationUtility.GetEditorCurve(i_clip, binding);
				CurveTrackRecord track = new CurveTrackRecord
				{
					Path = binding.path,
					Component = binding.type == null ? string.Empty : binding.type.FullName,
					Property = binding.propertyName
				};
				if (curve != null)
				{
					foreach (Keyframe key in curve.keys)
					{
						track.Keys.Add(new CurveKeyRecord
						{
							Time = key.time,
							Value = key.value,
							InTangent = FiniteOrNull(key.inTangent),
							OutTangent = FiniteOrNull(key.outTangent)
						});
					}
				}
				result.Curves.Add(track);
			}

			foreach (EditorCurveBinding binding in AnimationUtility.GetObjectReferenceCurveBindings(i_clip))
			{
				ObjectReferenceTrackRecord track = new ObjectReferenceTrackRecord
				{
					Path = binding.path,
					Component = binding.type == null ? string.Empty : binding.type.FullName,
					Property = binding.propertyName
				};
				foreach (ObjectReferenceKeyframe key in AnimationUtility.GetObjectReferenceCurve(i_clip, binding))
				{
					track.Keys.Add(new ObjectReferenceKeyRecord
					{
						Time = key.time,
						Asset = key.value == null ? string.Empty : AssetDatabase.GetAssetPath(key.value),
						Name = key.value == null ? string.Empty : key.value.name
					});
				}
				result.ObjectReferences.Add(track);
			}

			return result;
		}

		public static AnimationClipRecord ExportClipForAuthoring(AnimationClip i_clip) { return ExportClip(i_clip); }

		private static float? FiniteOrNull(float i_value)
		{
			return float.IsNaN(i_value) || float.IsInfinity(i_value) ? (float?)null : i_value;
		}

		private static string SuggestRole(string i_name)
		{
			string value = (i_name ?? string.Empty).ToLowerInvariant();
			if (value.Contains("idle") || value.Contains("await")) return "idle";
			if (value.Contains("walk") || value.Contains("run") || value.Contains("move") || value.Contains("fly")) return "move";
			if (value.Contains("death") || value.Contains("die")) return "death";
			if (value.Contains("damage") || value.Contains("hurt") || value.Contains("hit")) return "hit";
			if (value.Contains("attack") || value.Contains("shoot") || value.Contains("leap") || value.Contains("bite")) return "attack";
			if (value.Contains("rape") || value.Contains("grapple") || value.Contains("grab") || value.Contains("mount")) return "finisher";
			return "special";
		}

		private static string NaturalSortKey(string i_value)
		{
			if (string.IsNullOrEmpty(i_value)) return string.Empty;
			int split = i_value.Length;
			while (split > 0 && char.IsDigit(i_value[split - 1])) split--;
			int number;
			return int.TryParse(i_value.Substring(split), out number)
				? i_value.Substring(0, split) + number.ToString("D8", CultureInfo.InvariantCulture)
				: i_value;
		}

		[Serializable]
		private sealed class CoreEnemyRecord
		{
			[JsonProperty("id")] public string Id;
			[JsonProperty("legacyId")] public int LegacyId;
			[JsonProperty("displayName")] public string DisplayName;
		}

		[Serializable]
		public sealed class CatalogDocument
		{
			[JsonProperty("schemaVersion", Order = 0)] public int SchemaVersion;
			[JsonProperty("unityVersion", Order = 1)] public string UnityVersion;
			[JsonProperty("purpose", Order = 2)] public string Purpose;
			[JsonProperty("playerPreview", Order = 3)] public PlayerPreviewSet PlayerPreview;
			[JsonProperty("enemies", Order = 4)] public List<EnemyAnimationSet> Enemies = new List<EnemyAnimationSet>();
			[JsonProperty("warnings", Order = 5)] public List<string> Warnings = new List<string>();
		}

		[Serializable]
		public sealed class PlayerPreviewSet
		{
			[JsonProperty("prefab", Order = 0)] public string Prefab;
			[JsonProperty("controller", Order = 1)] public string Controller;
			[JsonProperty("states", Order = 2)] public List<AnimatorStateRecord> States = new List<AnimatorStateRecord>();
			[JsonProperty("clips", Order = 3)] public List<AnimationClipRecord> Clips = new List<AnimationClipRecord>();
			[JsonProperty("warnings", Order = 4)] public List<string> Warnings = new List<string>();
		}

		[Serializable]
		public sealed class EnemyAnimationSet
		{
			[JsonProperty("id", Order = 0)] public string Id;
			[JsonProperty("legacyId", Order = 1)] public int LegacyId;
			[JsonProperty("displayName", Order = 2)] public string DisplayName;
			[JsonProperty("prefab", Order = 3)] public string Prefab;
			[JsonProperty("controller", Order = 4)] public string Controller;
			[JsonProperty("playerInteractionSet", Order = 5)] public string PlayerInteractionSet;
			[JsonProperty("states", Order = 6)] public List<AnimatorStateRecord> States = new List<AnimatorStateRecord>();
			[JsonProperty("enemyClips", Order = 7)] public List<AnimationClipRecord> EnemyClips = new List<AnimationClipRecord>();
			[JsonProperty("playerInteractionClips", Order = 8)] public List<AnimationClipRecord> PlayerInteractionClips = new List<AnimationClipRecord>();
			[JsonProperty("interactionPairs", Order = 9)] public List<InteractionAnimationPair> InteractionPairs = new List<InteractionAnimationPair>();
			[JsonProperty("warnings", Order = 10)] public List<string> Warnings = new List<string>();
		}

		[Serializable]
		public sealed class InteractionAnimationPair
		{
			[JsonProperty("phase")] public int Phase;
			[JsonProperty("semanticName")] public string SemanticName;
			[JsonProperty("enemyLayer")] public string EnemyLayer;
			[JsonProperty("enemyState")] public string EnemyState;
			[JsonProperty("enemyMotion")] public string EnemyMotion;
			[JsonProperty("playerClip")] public string PlayerClip;
			[JsonProperty("playerAsset")] public string PlayerAsset;
		}

		[Serializable]
		public sealed class AnimatorStateRecord
		{
			[JsonProperty("layer")] public string Layer;
			[JsonProperty("path")] public string Path;
			[JsonProperty("motion")] public string Motion;
			[JsonProperty("semanticName")] public string SemanticName;
			[JsonProperty("authoring")] public bool Authoring;
			[JsonProperty("suggestedRole")] public string SuggestedRole;
			[JsonProperty("speed")] public float Speed;
			[JsonProperty("loop")] public bool Loop;
		}

		[Serializable]
		public sealed class AnimationClipRecord
		{
			[JsonProperty("name")] public string Name;
			[JsonProperty("asset")] public string Asset;
			[JsonProperty("suggestedRole")] public string SuggestedRole;
			[JsonProperty("semanticNames")] public List<string> SemanticNames = new List<string>();
			[JsonProperty("length")] public float Length;
			[JsonProperty("frameRate")] public float FrameRate;
			[JsonProperty("loop")] public bool Loop;
			[JsonProperty("events")] public List<AnimationEventRecord> Events = new List<AnimationEventRecord>();
			[JsonProperty("curves")] public List<CurveTrackRecord> Curves = new List<CurveTrackRecord>();
			[JsonProperty("objectReferences")] public List<ObjectReferenceTrackRecord> ObjectReferences = new List<ObjectReferenceTrackRecord>();
		}

		[Serializable]
		public sealed class AnimationEventRecord
		{
			[JsonProperty("time")] public float Time;
			[JsonProperty("function")] public string Function;
			[JsonProperty("stringParameter")] public string StringParameter;
			[JsonProperty("floatParameter")] public float FloatParameter;
			[JsonProperty("intParameter")] public int IntParameter;
		}

		[Serializable]
		public sealed class CurveTrackRecord
		{
			[JsonProperty("path")] public string Path;
			[JsonProperty("component")] public string Component;
			[JsonProperty("property")] public string Property;
			[JsonProperty("keys")] public List<CurveKeyRecord> Keys = new List<CurveKeyRecord>();
		}

		[Serializable]
		public sealed class CurveKeyRecord
		{
			[JsonProperty("time")] public float Time;
			[JsonProperty("value")] public float Value;
			[JsonProperty("inTangent")] public float? InTangent;
			[JsonProperty("outTangent")] public float? OutTangent;
		}

		[Serializable]
		public sealed class ObjectReferenceTrackRecord
		{
			[JsonProperty("path")] public string Path;
			[JsonProperty("component")] public string Component;
			[JsonProperty("property")] public string Property;
			[JsonProperty("keys")] public List<ObjectReferenceKeyRecord> Keys = new List<ObjectReferenceKeyRecord>();
		}

		[Serializable]
		public sealed class ObjectReferenceKeyRecord
		{
			[JsonProperty("time")] public float Time;
			[JsonProperty("asset")] public string Asset;
			[JsonProperty("name")] public string Name;
		}
	}
}
