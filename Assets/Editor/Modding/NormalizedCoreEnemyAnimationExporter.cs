using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CaptivityReloaded.Modding;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace CaptivityReloaded.Editor.Modding
{
	/// <summary>Converts a selected Core enemy clip into the same bounded document consumed by external enemies.</summary>
	public static class NormalizedCoreEnemyAnimationExporter
	{
		private const string OutputRoot = "ModSDK/AnimationReference/NormalizedEnemies";
		private const string PackagedCoreOutputRoot = "Assets/Resources/Modding/Core/Content/Animations";
		private static readonly Dictionary<string, string> BoneNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
		{
			{ "hips", "hips" }, { "butt", "butt" }, { "spine", "spine" }, { "chest", "chest" }, { "neck", "neck" }, { "head", "head" },
			{ "rArmUpper", "arm-right-upper" }, { "rArmLower", "arm-right-lower" }, { "rHand", "hand-right" },
			{ "lArmUpper", "arm-left-upper" }, { "lArmLower", "arm-left-lower" }, { "lHand", "hand-left" },
			{ "armUpperR", "arm-right-upper" }, { "armLowerR", "arm-right-lower" }, { "handR", "hand-right" },
			{ "armUpperL", "arm-left-upper" }, { "armLowerL", "arm-left-lower" }, { "handL", "hand-left" },
			{ "rLegUpper", "leg-right-upper" }, { "rLegLower", "leg-right-lower" }, { "rFoot", "foot-right" },
			{ "lLegUpper", "leg-left-upper" }, { "lLegLower", "leg-left-lower" }, { "lFoot", "foot-left" },
			{ "legUpperR", "leg-right-upper" }, { "legLowerR", "leg-right-lower" }, { "footR", "foot-right" },
			{ "legUpperL", "leg-left-upper" }, { "legLowerL", "leg-left-lower" }, { "footL", "foot-left" },
			{ "penis", "penis" }, { "brain", "brain" }, { "bg", "background" }
		};

		[MenuItem("Captivity Reloaded/Modding/Export Representative Core Enemy Animations")]
		public static void ExportRepresentativeSet()
		{
			ExportSet(false, true);
		}

		[MenuItem("Captivity Reloaded/Modding/Export All Core Enemy Animations")]
		public static void ExportCompleteSet()
		{
			ExportSet(true, true);
		}

		/// <summary>Refreshes every normalized Core enemy rig and animation without opening a completion dialog.</summary>
		public static int ExportCompleteSetForAuthoring()
		{
			return ExportSet(true, false);
		}

		private static int ExportSet(bool i_includeAllSemanticClips, bool i_showDialog)
		{
			CoreAnimationCatalogExporter.Export();
			CoreAnimationCatalogExporter.CatalogDocument catalog = CoreAnimationCatalogExporter.BuildCatalog();
			Func<string, bool> includeSemantic = i_includeAllSemanticClips ? (Func<string, bool>)IsPublicSemantic : IsRepresentativeSemantic;
			int written = 0;
			foreach (CoreAnimationCatalogExporter.EnemyAnimationSet enemy in catalog.Enemies)
			{
				string enemyPath = enemy.Id.StartsWith("core:enemy/", StringComparison.Ordinal) ? enemy.Id.Substring("core:enemy/".Length) : Slug(enemy.Id);
				Dictionary<string, string> pathMap = BuildSemanticPathMap(enemy.EnemyClips);
				WriteRig(enemy, enemyPath, pathMap);
				List<CoreAnimationCatalogExporter.AnimationClipRecord> candidates = enemy.EnemyClips
					.Where(i_clip => i_clip.SemanticNames.Any(includeSemantic)).ToList();
				HashSet<string> exportedSemantics = new HashSet<string>(StringComparer.Ordinal);
				Dictionary<string, string> exportedReferences = new Dictionary<string, string>(StringComparer.Ordinal);
				foreach (CoreAnimationCatalogExporter.AnimationClipRecord record in candidates)
				{
					AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(record.Asset);
					if (clip == null) continue;
					foreach (string semantic in record.SemanticNames.Where(includeSemantic))
					{
						if (!exportedSemantics.Add(semantic)) continue;
						NormalizedEnemyAnimationDocument document = Convert(enemy.Id, enemyPath, semantic, clip, pathMap);
						if (document.Tracks.Count == 0) continue;
						Write(enemyPath, semantic, document);
						exportedReferences[semantic] = document.Id;
						written++;
					}
				}
				MergeCoreEnemyAnimationReferences(enemyPath, exportedReferences);
			}
			AssetDatabase.Refresh();
			string scope = i_includeAllSemanticClips ? "complete" : "representative";
			Debug.Log("[Modding] Exported " + written + " " + scope + " normalized Core enemy animations to " + OutputRoot + " and packaged Core content.");
			if (i_showDialog) EditorUtility.DisplayDialog("Normalized enemy export", "Exported " + written + " " + scope + " Core enemy animations.", "OK");
			return written;
		}

		public static void ExportSelected(string i_enemyId, string i_semanticName, AnimationClip i_clip)
		{
			if (i_clip == null || string.IsNullOrWhiteSpace(i_enemyId))
			{
				EditorUtility.DisplayDialog("Normalized enemy export", "Select a Core enemy animation first.", "OK");
				return;
			}
			string enemyPath = i_enemyId.StartsWith("core:enemy/", StringComparison.Ordinal) ? i_enemyId.Substring("core:enemy/".Length) : Slug(i_enemyId);
			string semantic = Slug(i_semanticName);
			CoreAnimationCatalogExporter.EnemyAnimationSet enemy = CoreAnimationCatalogExporter.BuildCatalog().Enemies.FirstOrDefault(i_enemy => i_enemy.Id == i_enemyId);
			Dictionary<string, string> pathMap = enemy == null
				? BuildSemanticPathMap(new[] { CoreAnimationCatalogExporter.ExportClipForAuthoring(i_clip) })
				: BuildSemanticPathMap(enemy.EnemyClips);
			if (enemy != null) WriteRig(enemy, enemyPath, pathMap);
			NormalizedEnemyAnimationDocument document = Convert(i_enemyId, enemyPath, semantic, i_clip, pathMap);
			if (document.Tracks.Count == 0)
			{
				EditorUtility.DisplayDialog("Normalized enemy export", "The selected clip has no supported 2D transform or sprite tracks.", "OK");
				return;
			}
			string path = Write(enemyPath, semantic, document);
			AssetDatabase.Refresh();
			Debug.Log("[Modding] Exported normalized Core enemy clip to " + path);
			EditorUtility.DisplayDialog("Normalized enemy export", "Exported " + path + "\n\nWarnings: " + document.Warnings.Count, "OK");
		}

		private static string Write(string i_enemyPath, string i_semantic, NormalizedEnemyAnimationDocument i_document)
		{
			string directory = OutputRoot + "/" + i_enemyPath;
			Directory.CreateDirectory(directory);
			string path = directory + "/" + Slug(i_semantic) + ".json";
			string json = JsonConvert.SerializeObject(i_document, Formatting.Indented,
				new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
			File.WriteAllText(path, json);
			string packagedDirectory = PackagedCoreOutputRoot + "/" + i_enemyPath;
			Directory.CreateDirectory(packagedDirectory);
			File.WriteAllText(packagedDirectory + "/" + Slug(i_semantic) + ".json", json);
			return path;
		}

		private static void MergeCoreEnemyAnimationReferences(string i_enemyPath, IDictionary<string, string> i_references)
		{
			if (i_references == null || i_references.Count == 0) return;
			string path = "Assets/Resources/Modding/Core/Content/" + i_enemyPath + ".json";
			if (!File.Exists(path))
			{
				Debug.LogWarning("[Modding] Cannot attach exported animations because the Core enemy definition is missing: " + path);
				return;
			}
			JObject root = JObject.Parse(File.ReadAllText(path));
			JObject references = root["animationReferences"] as JObject ?? new JObject();
			foreach (KeyValuePair<string, string> reference in i_references.OrderBy(i_pair => i_pair.Key, StringComparer.Ordinal))
				references[reference.Key] = reference.Value;
			root["animationReferences"] = new JObject(references.Properties().OrderBy(i_property => i_property.Name, StringComparer.Ordinal));
			File.WriteAllText(path, root.ToString(Formatting.Indented) + Environment.NewLine);
		}

		private static bool IsRepresentativeSemantic(string i_semantic)
		{
			if (string.IsNullOrEmpty(i_semantic) || i_semantic.StartsWith("internal-", StringComparison.Ordinal)) return false;
			return i_semantic == "idle" || i_semantic == "move" || i_semantic == "finisher-phase-1"
				|| i_semantic.EndsWith("-idle", StringComparison.Ordinal) || i_semantic.EndsWith("-move", StringComparison.Ordinal)
				|| i_semantic.StartsWith("attack-", StringComparison.Ordinal) || i_semantic.Contains("-attack-");
		}

		private static bool IsPublicSemantic(string i_semantic)
		{
			return !string.IsNullOrEmpty(i_semantic) && !i_semantic.StartsWith("internal-", StringComparison.Ordinal);
		}

		private static Dictionary<string, string> BuildSemanticPathMap(IEnumerable<CoreAnimationCatalogExporter.AnimationClipRecord> i_clips)
		{
			Dictionary<string, bool> paths = new Dictionary<string, bool>(StringComparer.Ordinal);
			foreach (CoreAnimationCatalogExporter.AnimationClipRecord clip in i_clips ?? Enumerable.Empty<CoreAnimationCatalogExporter.AnimationClipRecord>())
			{
				foreach (CoreAnimationCatalogExporter.CurveTrackRecord curve in clip.Curves)
				{
					string path = curve.Path ?? string.Empty;
					if (IsPlayerBindingPath(path)) continue;
					bool transform = curve.Component == typeof(Transform).FullName;
					paths[path] = paths.TryGetValue(path, out bool existing) ? existing || transform : transform;
				}
				foreach (CoreAnimationCatalogExporter.ObjectReferenceTrackRecord track in clip.ObjectReferences)
					if (!IsPlayerBindingPath(track.Path) && !paths.ContainsKey(track.Path ?? string.Empty)) paths.Add(track.Path ?? string.Empty, false);
			}
			Dictionary<string, string> result = new Dictionary<string, string>(StringComparer.Ordinal);
			Dictionary<string, string> primaryPaths = new Dictionary<string, string>(StringComparer.Ordinal);
			foreach (KeyValuePair<string, bool> entry in paths.OrderByDescending(i_entry => i_entry.Value).ThenBy(i_entry => i_entry.Key.Count(i_char => i_char == '/')).ThenBy(i_entry => i_entry.Key, StringComparer.Ordinal))
			{
				string semantic = NormalizeBone(entry.Key);
				if (!primaryPaths.TryGetValue(semantic, out string primary)) primaryPaths.Add(semantic, entry.Key);
				else if (entry.Value && !IsRendererChild(entry.Key) && !string.Equals(primary, entry.Key, StringComparison.Ordinal))
				{
					semantic = QualifyPath(entry.Key);
					int suffix = 2;
					string unique = semantic;
					while (primaryPaths.ContainsKey(unique)) unique = semantic + "-" + suffix++;
					semantic = unique;
					primaryPaths.Add(semantic, entry.Key);
				}
				result[entry.Key] = semantic;
			}
			return result;
		}

		private static bool IsRendererChild(string i_path)
		{
			string leaf = (i_path ?? string.Empty).Split('/').LastOrDefault() ?? string.Empty;
			return leaf.StartsWith("bp_", StringComparison.OrdinalIgnoreCase);
		}

		private static bool IsPlayerBindingPath(string i_path)
		{
			return string.Equals(i_path, "SkeletonPlayer", StringComparison.Ordinal)
				|| (i_path ?? string.Empty).StartsWith("SkeletonPlayer/", StringComparison.Ordinal);
		}

		private static string QualifyPath(string i_path)
		{
			string[] parts = (i_path ?? string.Empty).Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
			string[] filtered = parts.Where(i_part => !i_part.StartsWith("Skeleton", StringComparison.OrdinalIgnoreCase)).ToArray();
			IEnumerable<string> meaningful = filtered.Skip(Mathf.Max(0, filtered.Length - 3));
			return Slug(string.Join("-", meaningful.ToArray()));
		}

		private static void WriteRig(CoreAnimationCatalogExporter.EnemyAnimationSet i_enemy, string i_enemyPath, Dictionary<string, string> i_pathMap)
		{
			CoreEnemyRigReference document = new CoreEnemyRigReference
			{
				SchemaVersion = 1, Type = "enemyRigReference", Id = "core:enemy-rig/" + i_enemyPath,
				Enemy = i_enemy.Id, Prefab = i_enemy.Prefab, Controller = i_enemy.Controller
			};
			GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(i_enemy.Prefab);
			if (prefab == null) { document.Warnings.Add("Core prefab could not be loaded."); WriteRigFile(i_enemyPath, document); return; }
			string[] paths = i_pathMap.Keys.ToArray();
			List<Transform> candidates = prefab.GetComponentsInChildren<Animator>(true).Select(i_animator => i_animator.transform).ToList();
			candidates.Add(prefab.transform);
			Transform sampleRoot = candidates.Distinct().OrderByDescending(i_candidate => paths.Count(i_path => string.IsNullOrEmpty(i_path) || i_candidate.Find(i_path) != null)).First();
			document.SampleRoot = RelativePath(prefab.transform, sampleRoot);
			AugmentPathMapWithArtwork(sampleRoot, i_pathMap);
			Dictionary<string, CoreEnemyRigBoneReference> byName = new Dictionary<string, CoreEnemyRigBoneReference>(StringComparer.Ordinal);
			foreach (KeyValuePair<string, string> pair in i_pathMap.OrderBy(i_pair => i_pair.Key.Count(i_char => i_char == '/')).ThenBy(i_pair => i_pair.Key, StringComparer.Ordinal))
			{
				Transform target = string.IsNullOrEmpty(pair.Key) ? sampleRoot : sampleRoot.Find(pair.Key);
				if (target == null) { document.Warnings.Add("Binding path was not found on the prefab: " + pair.Key); continue; }
				if (!byName.TryGetValue(pair.Value, out CoreEnemyRigBoneReference bone))
				{
					bone = new CoreEnemyRigBoneReference
					{
						Name = pair.Value, UnityPath = pair.Key,
						Transform = new CoreEnemyTransformReference
						{
							X = target.localPosition.x, Y = target.localPosition.y, Z = target.localPosition.z,
							RotationX = target.localEulerAngles.x, RotationY = target.localEulerAngles.y, RotationZ = target.localEulerAngles.z,
							ScaleX = target.localScale.x, ScaleY = target.localScale.y, ScaleZ = target.localScale.z
						}
					};
					Transform parent = target.parent;
					while (parent != null && parent != sampleRoot.parent)
					{
						string parentPath = RelativePath(sampleRoot, parent);
						if (i_pathMap.TryGetValue(parentPath, out string parentName) && parentName != pair.Value) { bone.Parent = parentName; break; }
						if (parent == sampleRoot) break;
						parent = parent.parent;
					}
					byName.Add(pair.Value, bone);
					document.Bones.Add(bone);
				}
				if (!bone.UnityPaths.Contains(pair.Key)) bone.UnityPaths.Add(pair.Key);
				SpriteRenderer renderer = target.GetComponent<SpriteRenderer>();
				if (renderer != null && !bone.Sprites.Any(i_sprite => i_sprite.UnityPath == pair.Key))
				{
					bone.Sprites.Add(new CoreEnemySpriteReference
					{
						UnityPath = pair.Key, Name = renderer.sprite == null ? null : renderer.sprite.name,
						Asset = renderer.sprite == null ? null : AssetDatabase.GetAssetPath(renderer.sprite),
						PivotX = renderer.sprite == null ? 0.5f : renderer.sprite.pivot.x / renderer.sprite.rect.width,
						PivotY = renderer.sprite == null ? 0.5f : renderer.sprite.pivot.y / renderer.sprite.rect.height,
						Width = renderer.sprite == null ? 0f : renderer.sprite.rect.width,
						Height = renderer.sprite == null ? 0f : renderer.sprite.rect.height,
						SortingLayer = renderer.sortingLayerName, SortingOrder = renderer.sortingOrder
					});
				}
			}
			WriteRigFile(i_enemyPath, document);
		}

		private static void AugmentPathMapWithArtwork(Transform i_sampleRoot, Dictionary<string, string> io_pathMap)
		{
			HashSet<string> paths = new HashSet<string>(io_pathMap.Keys, StringComparer.Ordinal);
			foreach (SpriteRenderer renderer in i_sampleRoot.GetComponentsInChildren<SpriteRenderer>(true))
				for (Transform current = renderer.transform; current != null; current = current.parent)
				{
					paths.Add(RelativePath(i_sampleRoot, current));
					if (current == i_sampleRoot) break;
				}

			Dictionary<string, string> primary = new Dictionary<string, string>(StringComparer.Ordinal);
			foreach (KeyValuePair<string, string> pair in io_pathMap)
				if (!primary.ContainsKey(pair.Value) && !IsRendererChild(pair.Key)) primary.Add(pair.Value, pair.Key);
			foreach (string path in paths.OrderBy(i_path => i_path.Count(i_char => i_char == '/')).ThenBy(i_path => i_path, StringComparer.Ordinal))
			{
				if (io_pathMap.ContainsKey(path)) continue;
				string semantic;
				string parentPath = path.Contains("/") ? path.Substring(0, path.LastIndexOf('/')) : string.Empty;
				if (IsRendererChild(path) && io_pathMap.TryGetValue(parentPath, out string parentSemantic)) semantic = parentSemantic;
				else
				{
					semantic = NormalizeBone(path);
					if (primary.ContainsKey(semantic))
					{
						string basis = QualifyPath(path); int suffix = 2; semantic = basis;
						while (primary.ContainsKey(semantic)) semantic = basis + "-" + suffix++;
					}
					primary[semantic] = path;
				}
				io_pathMap[path] = semantic;
			}
		}

		private static void WriteRigFile(string i_enemyPath, CoreEnemyRigReference i_document)
		{
			string directory = OutputRoot + "/" + i_enemyPath;
			Directory.CreateDirectory(directory);
			File.WriteAllText(directory + "/rig.json", JsonConvert.SerializeObject(i_document, Formatting.Indented,
				new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));
		}

		private static string RelativePath(Transform i_root, Transform i_target)
		{
			if (i_target == i_root) return string.Empty;
			List<string> parts = new List<string>();
			for (Transform current = i_target; current != null && current != i_root; current = current.parent) parts.Add(current.name);
			parts.Reverse();
			return string.Join("/", parts.ToArray());
		}

		private static NormalizedEnemyAnimationDocument Convert(string i_enemyId, string i_enemyPath, string i_semantic, AnimationClip i_clip, IReadOnlyDictionary<string, string> i_pathMap)
		{
			NormalizedEnemyAnimationDocument result = new NormalizedEnemyAnimationDocument
			{
				SchemaVersion = 1, Type = "enemyAnimation", Id = "core:enemy-animation/" + i_enemyPath + "/" + i_semantic,
				Enemy = i_enemyId, DisplayName = i_enemyPath + " " + i_semantic, DurationSeconds = Mathf.Max(0.0001f, i_clip.length),
				FrameRate = Mathf.Max(1f, i_clip.frameRate), Loop = AnimationUtility.GetAnimationClipSettings(i_clip).loopTime,
				Source = new JObject { ["unityClip"] = AssetDatabase.GetAssetPath(i_clip), ["semanticName"] = i_semantic }
			};
			HashSet<string> identities = new HashSet<string>(StringComparer.Ordinal);
			foreach (EditorCurveBinding binding in AnimationUtility.GetCurveBindings(i_clip))
			{
				if (IsPlayerBindingPath(binding.path)) continue;
				if (!TryNormalizeProperty(binding, out string targetKind, out string property)) continue;
				string bone = i_pathMap.TryGetValue(binding.path ?? string.Empty, out string mapped) ? mapped : NormalizeBone(binding.path);
				if (string.IsNullOrEmpty(bone)) { result.Warnings.Add("Unresolved binding path: " + binding.path); continue; }
				string target = targetKind + "/" + bone;
				if (!identities.Add(target + "|" + property)) { result.Warnings.Add("Duplicate normalized binding skipped: " + target + " " + property); continue; }
				AnimationCurve curve = AnimationUtility.GetEditorCurve(i_clip, binding);
				if (curve == null || curve.keys.Length == 0) continue;
				NormalizedNumericTrack track = new NormalizedNumericTrack { Target = target, Property = property };
				foreach (Keyframe key in curve.keys)
					track.Keys.Add(new NormalizedNumericKey { Time = Mathf.Clamp(key.time, 0f, result.DurationSeconds), Value = key.value, InTangent = Finite(key.inTangent), OutTangent = Finite(key.outTangent) });
				result.Tracks.Add(track);
			}
			foreach (EditorCurveBinding binding in AnimationUtility.GetObjectReferenceCurveBindings(i_clip))
			{
				if (IsPlayerBindingPath(binding.path)) continue;
				if (binding.type != typeof(SpriteRenderer) || binding.propertyName != "m_Sprite") continue;
				string bone = i_pathMap.TryGetValue(binding.path ?? string.Empty, out string mapped) ? mapped : NormalizeBone(binding.path);
				if (string.IsNullOrEmpty(bone)) continue;
				NormalizedObjectTrack track = new NormalizedObjectTrack { Target = "sprite/" + bone, Property = "sprite" };
				foreach (ObjectReferenceKeyframe key in AnimationUtility.GetObjectReferenceCurve(i_clip, binding))
					track.Keys.Add(new NormalizedObjectKey { Time = Mathf.Clamp(key.time, 0f, result.DurationSeconds), Name = key.value == null ? "missing" : key.value.name, Asset = key.value == null ? null : AssetDatabase.GetAssetPath(key.value) });
				if (track.Keys.Count > 0) result.ObjectTracks.Add(track);
			}
			foreach (AnimationEvent animationEvent in AnimationUtility.GetAnimationEvents(i_clip))
			{
				if (animationEvent.functionName == "AnimEventPerformAttack") result.Events.Add(new EnemyAnimationEventDefinition { Time = animationEvent.time, Type = "attackHit" });
				else if (TryConvertCue(animationEvent, out EnemyAnimationEventDefinition cue)) result.Events.Add(cue);
				else result.Warnings.Add("Unconverted Unity event at " + animationEvent.time.ToString("0.###", CultureInfo.InvariantCulture) + ": " + animationEvent.functionName);
			}
			if (result.Loop && result.Events.Any(animationEvent => animationEvent != null && animationEvent.Type != "cue"))
			{
				result.Loop = false;
				result.Warnings.Add("Loop disabled because this clip contains a gameplay animation event.");
			}
			return result;
		}

		private static bool TryConvertCue(AnimationEvent i_event, out EnemyAnimationEventDefinition o_event)
		{
			o_event = new EnemyAnimationEventDefinition { Time = i_event.time, Type = "cue" };
			switch (i_event.functionName)
			{
			case "Thrust": o_event.Cue = "finisher.thrust"; o_event.Amount = i_event.intParameter; break;
			case "PlayAudioUnique": case "AnimEventPlayAudioUnique": o_event.Cue = "audio.unique"; o_event.Index = Mathf.Max(0, i_event.intParameter - 1); break;
			case "PlayRandomAudioPlayerVoice": o_event.Cue = "audio.player-voice-random"; break;
			case "PlayRandomAudioRaperVoice": o_event.Cue = "audio.enemy-voice-random"; break;
			case "PlayParticleUnique": o_event.Cue = "particle.unique"; o_event.Index = Mathf.Max(0, i_event.intParameter - 1); break;
			case "Slap": o_event.Cue = "finisher.slap"; break;
			case "Chew": o_event.Cue = "finisher.chew"; break;
			case "AnimEventPlaceTrap": o_event.Cue = "attack.place-trap"; break;
			case "AnimEventFlyAttack": o_event.Cue = "attack.contact"; break;
			case "AnimEventShootDart": o_event.Cue = "attack.projectile"; break;
			case "AnimEventLeap": o_event.Cue = "attack.leap"; break;
			default: o_event = null; return false;
			}
			return true;
		}

		private static bool TryNormalizeProperty(EditorCurveBinding i_binding, out string o_targetKind, out string o_property)
		{
			o_targetKind = "bone"; o_property = null;
			if (i_binding.type == typeof(Transform))
			{
				if (i_binding.propertyName == "m_LocalPosition.x") o_property = "position.x";
				else if (i_binding.propertyName == "m_LocalPosition.y") o_property = "position.y";
				else if (i_binding.propertyName == "localEulerAnglesRaw.z") o_property = "rotation.z";
				else if (i_binding.propertyName == "m_LocalScale.x") o_property = "scale.x";
				else if (i_binding.propertyName == "m_LocalScale.y") o_property = "scale.y";
			}
			else if (i_binding.type == typeof(SpriteRenderer))
			{
				o_targetKind = "sprite";
				if (i_binding.propertyName.StartsWith("m_Color.", StringComparison.Ordinal)) o_property = "color." + i_binding.propertyName.Substring("m_Color.".Length);
				else if (i_binding.propertyName == "m_SortingOrder") o_property = "sortingOrder";
			}
			else if (i_binding.type == typeof(BodyPartActor) && i_binding.propertyName == "m_sortingOrder") { o_targetKind = "sprite"; o_property = "sortingOrder"; }
			return o_property != null;
		}

		private static string NormalizeBone(string i_path)
		{
			if (string.IsNullOrWhiteSpace(i_path)) return "root";
			string leaf = (i_path ?? string.Empty).Split('/').LastOrDefault() ?? string.Empty;
			if (leaf.StartsWith("bp_", StringComparison.OrdinalIgnoreCase)) leaf = leaf.Substring(3);
			return BoneNames.TryGetValue(leaf, out string semantic) ? semantic : Slug(leaf);
		}

		private static string Slug(string i_value)
		{
			if (string.IsNullOrWhiteSpace(i_value)) return "unnamed";
			System.Text.StringBuilder result = new System.Text.StringBuilder(); bool separator = false; char previous = '\0';
			foreach (char character in i_value)
			{
				if (char.IsLetterOrDigit(character)) { if ((separator || (char.IsUpper(character) && char.IsLower(previous))) && result.Length > 0) result.Append('-'); result.Append(char.ToLowerInvariant(character)); separator = false; }
				else separator = true;
				previous = character;
			}
			return result.Length == 0 ? "unnamed" : result.ToString();
		}

		private static float? Finite(float i_value) { return float.IsNaN(i_value) || float.IsInfinity(i_value) ? (float?)null : i_value; }

		[Serializable] private sealed class CoreEnemyRigReference { [JsonProperty("schemaVersion")] public int SchemaVersion; [JsonProperty("type")] public string Type; [JsonProperty("id")] public string Id; [JsonProperty("enemy")] public string Enemy; [JsonProperty("prefab")] public string Prefab; [JsonProperty("controller")] public string Controller; [JsonProperty("sampleRoot")] public string SampleRoot; [JsonProperty("bones")] public List<CoreEnemyRigBoneReference> Bones = new List<CoreEnemyRigBoneReference>(); [JsonProperty("warnings")] public List<string> Warnings = new List<string>(); }
		[Serializable] private sealed class CoreEnemyRigBoneReference { [JsonProperty("name")] public string Name; [JsonProperty("parent")] public string Parent; [JsonProperty("unityPath")] public string UnityPath; [JsonProperty("unityPaths")] public List<string> UnityPaths = new List<string>(); [JsonProperty("defaultTransform")] public CoreEnemyTransformReference Transform; [JsonProperty("sprites")] public List<CoreEnemySpriteReference> Sprites = new List<CoreEnemySpriteReference>(); }
		[Serializable] private sealed class CoreEnemyTransformReference { [JsonProperty("x")] public float X; [JsonProperty("y")] public float Y; [JsonProperty("z")] public float Z; [JsonProperty("rotationX")] public float RotationX; [JsonProperty("rotationY")] public float RotationY; [JsonProperty("rotationZ")] public float RotationZ; [JsonProperty("scaleX")] public float ScaleX; [JsonProperty("scaleY")] public float ScaleY; [JsonProperty("scaleZ")] public float ScaleZ; }
		[Serializable] private sealed class CoreEnemySpriteReference { [JsonProperty("unityPath")] public string UnityPath; [JsonProperty("name")] public string Name; [JsonProperty("asset")] public string Asset; [JsonProperty("pivotX")] public float PivotX; [JsonProperty("pivotY")] public float PivotY; [JsonProperty("width")] public float Width; [JsonProperty("height")] public float Height; [JsonProperty("sortingLayer")] public string SortingLayer; [JsonProperty("sortingOrder")] public int SortingOrder; }
	}
}
