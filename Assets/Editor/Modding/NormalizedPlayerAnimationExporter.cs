using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CaptivityReloaded.Modding;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace CaptivityReloaded.Editor.Modding
{
	public static class NormalizedPlayerAnimationExporter
	{
		private const string PlayerPrefabPath = "Assets/Actors/Players/Alex.prefab";
		private const string OutputDirectory = "ModSDK/AnimationReference/Normalized";
		private static readonly Dictionary<string, string> BoneNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
		{
			{ "hips", "hips" }, { "butt", "butt" }, { "spine", "spine" }, { "chest", "chest" },
			{ "neck", "neck" }, { "head", "head" }, { "rArmUpper", "arm-right-upper" },
			{ "rArmLower", "arm-right-lower" }, { "rHand", "hand-right" }, { "lArmUpper", "arm-left-upper" },
			{ "lArmLower", "arm-left-lower" }, { "lHand", "hand-left" }, { "rLegUpper", "leg-right-upper" },
			{ "rLegLower", "leg-right-lower" }, { "rFoot", "foot-right" }, { "lLegUpper", "leg-left-upper" },
			{ "lLegLower", "leg-left-lower" }, { "lFoot", "foot-left" }, { "ear", "ear" }, { "face", "face" }
		};

		[MenuItem("Captivity Reloaded/Modding/Export Normalized Player Reference")]
		public static void ExportReference()
		{
			Directory.CreateDirectory(OutputDirectory);
			ExportRig();
			ExportClip("Assets/AnimationClip/Idle_15.anim", "core:player-animation/idle");
			ExportClip("Assets/AnimationClip/Walk_4.anim", "core:player-animation/walk");
			ExportClip("Assets/AnimationClip/Crouch.anim", "core:player-animation/crouch");
			ExportClip("Assets/AnimationClip/Jump_7.anim", "core:player-animation/jump");
			ExportClip("Assets/AnimationClip/Climb_5.anim", "core:player-animation/climb");
			ExportClip("Assets/AnimationClip/ReloadPumpAction.anim", "core:player-animation/reload-pump-action");
			ExportClip("Assets/AnimationClip/UseSyringe.anim", "core:player-animation/use-syringe");
			ExportClip("Assets/AnimationClip/Birth.anim", "core:player-animation/birth");
			ExportClip("Assets/Resources/Raper/zombie/Zombie1.anim", "core:player-animation/finisher-zombie-1");
			ExportClip("Assets/Resources/Raper/deathhound/DeathHound1.anim", "core:player-animation/finisher-death-hound-1");
			ExportClip("Assets/Resources/Raper/headhumper/HeadHumper1.anim", "core:player-animation/finisher-head-humper-1");
			ExportClip("Assets/Resources/Raper/musca/Musca1.anim", "core:player-animation/finisher-musca-1");
			ExportClip("Assets/Resources/Raper/sqoid/Sqoid8.anim", "core:player-animation/finisher-sqoid-8");
			ExportClip("Assets/Resources/Raper/sunny/Sunny1.anim", "core:player-animation/finisher-sunny-1");
			ExportClip("Assets/Resources/Raper/jacky/Jacky5.anim", "core:player-animation/finisher-jacky-5");
			AssetDatabase.Refresh();
			Debug.Log("[Modding] Exported normalized player rig and representative animations to " + OutputDirectory + ".");
		}

		private static void ExportRig()
		{
			Write("player-rig.json", BuildRigDocument());
		}

		public static JObject BuildRigDocument()
		{
			GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
			if (prefab == null) throw new InvalidOperationException("Player prefab was not found: " + PlayerPrefabPath);
			SkeletonPlayer skeleton = prefab.GetComponentInChildren<SkeletonPlayer>(true);
			if (skeleton == null) throw new InvalidOperationException("Player prefab has no SkeletonPlayer.");
			RigDocument document = new RigDocument
			{
				SchemaVersion = 1,
				Type = "playerRig",
				Id = "core:player-rig/alex",
				Prefab = PlayerPrefabPath,
				SampleRoot = GetHierarchyPath(prefab.transform, skeleton.transform),
				CoordinateSystem = new CoordinateSystem { Units = "world", YUp = true, Rotation = "degrees-clockwise-negative", PixelsPerUnit = 32f },
				Clothing = new ClothingContract
				{
					Slots = ClothingSlotCatalog.Slots.OrderBy(i_slot => i_slot, StringComparer.Ordinal).ToList(),
					Categories = Enum.GetNames(typeof(ClothingCategory)).ToList(),
					PivotCoordinates = "normalized-bottom-left",
					CanvasBounds = "sprite-rectangle",
					SortingOrderMin = -32768,
					SortingOrderMax = 32767,
					SortingOffsetMin = -100,
					SortingOffsetMax = 100,
					SameCategoryCompatible = false,
					IncompatibleCategoriesSymmetric = true,
					BodyVariantMatchOrder = new List<string> { "ModBodyVariant", "player-prefab-name", "loaded-pack-id", "default" },
					BodyVariantFallback = "visual.sprites"
				},
				Source = new SourceReference { UnityPrefab = PlayerPrefabPath }
			};
			// Skeleton.m_bones is populated by Awake during gameplay, which does not run
			// when reading a prefab asset in an editor exporter.
			foreach (BonePlayer bone in skeleton.GetComponentsInChildren<BonePlayer>(true).OrderBy(i_bone => (int)i_bone.GetBoneType()))
			{
				string semantic = NormalizeBone(bone.GetBoneType().ToString());
				BonePlayer parent = FindParentBone(bone.transform);
				SpriteRenderer renderer = bone.GetBodyPart() == null ? null : bone.GetBodyPart().GetComponent<SpriteRenderer>();
				SortingGroup sorting = bone.GetBodyPart() == null ? null : bone.GetBodyPart().GetComponent<SortingGroup>();
				document.Bones.Add(new RigBone
				{
					Name = semantic,
					Parent = parent == null ? null : NormalizeBone(parent.GetBoneType().ToString()),
					UnityPath = GetHierarchyPath(skeleton.transform, bone.transform),
					DefaultTransform = TransformValue.From(bone.transform),
					Attachment = new AttachmentPoint { Target = "bone/" + semantic, PivotX = 0.5f, PivotY = 0.5f },
					SpriteCanvas = renderer == null || renderer.sprite == null ? null : new SpriteCanvas
					{
						Width = Mathf.RoundToInt(renderer.sprite.rect.width), Height = Mathf.RoundToInt(renderer.sprite.rect.height),
						PivotX = renderer.sprite.pivot.x / renderer.sprite.rect.width,
						PivotY = renderer.sprite.pivot.y / renderer.sprite.rect.height,
						SortingOrder = sorting == null ? renderer.sortingOrder : sorting.sortingOrder
					},
					Sprites = renderer == null || renderer.sprite == null ? new List<SpriteReference>() : new List<SpriteReference>
					{
						new SpriteReference
						{
							UnityPath = GetHierarchyPath(skeleton.transform, renderer.transform), Name = renderer.sprite.name,
							Asset = AssetDatabase.GetAssetPath(renderer.sprite), PivotX = renderer.sprite.pivot.x / renderer.sprite.rect.width,
							PivotY = renderer.sprite.pivot.y / renderer.sprite.rect.height,
							SortingLayer = renderer.sortingLayerName, SortingOrder = sorting == null ? renderer.sortingOrder : sorting.sortingOrder
						}
					}
				});
			}
			return JObject.FromObject(document, JsonSerializer.CreateDefault());
		}

		private static void ExportClip(string i_assetPath, string i_id)
		{
			JObject document = BuildClipDocument(i_assetPath, i_id);
			Write(i_id.Split('/').Last() + ".json", document);
		}

		public static JObject BuildClipDocument(string i_assetPath, string i_id)
		{
			AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(i_assetPath);
			if (clip == null) throw new InvalidOperationException("Animation clip was not found: " + i_assetPath);
			AnimationDocument document = new AnimationDocument
			{
				SchemaVersion = 1, Type = "playerAnimation", Id = i_id, Rig = "core:player-rig/alex",
				DisplayName = clip.name, DurationSeconds = clip.length, FrameRate = clip.frameRate,
				Loop = AnimationUtility.GetAnimationClipSettings(clip).loopTime,
				Source = new SourceReference { UnityClip = i_assetPath }
			};
			foreach (EditorCurveBinding binding in AnimationUtility.GetCurveBindings(clip))
			{
				string target = NormalizeTarget(binding.path);
				string property = NormalizeProperty(binding.type, binding.propertyName);
				if (target == null || property == null)
				{
					if ((binding.propertyName ?? string.Empty).IndexOf("sort", StringComparison.OrdinalIgnoreCase) >= 0)
						document.Warnings.Add("Unresolved sorting binding: " + binding.path + " | " + binding.type.FullName + " | " + binding.propertyName);
					continue;
				}
				AnimationCurve curve = AnimationUtility.GetEditorCurve(clip, binding);
				Track track = new Track { Target = target, Property = property };
				if (curve != null) foreach (Keyframe key in curve.keys)
					track.Keys.Add(new NumericKey { Time = key.time, Value = key.value, InTangent = Finite(key.inTangent), OutTangent = Finite(key.outTangent) });
				Track existing = document.Tracks.FirstOrDefault(i_track => i_track.Target == target && i_track.Property == property);
				if (existing == null) document.Tracks.Add(track);
				else if (property == "sortingOrder" && binding.type == typeof(BodyPartPlayer))
				{
					document.Tracks.Remove(existing);
					document.Tracks.Add(track);
					document.Warnings.Add("Both renderer and effective body-part sorting targeted " + target + "; exported the BodyPartPlayer track.");
				}
				else document.Warnings.Add("Duplicate normalized track ignored: " + target + " | " + property + ".");
				if (property == "sortingOrder" && track.Keys.Any(i_key => i_key.Value < -32768f || i_key.Value > 32767f))
					document.Warnings.Add("Sorting values exceed the supported range on " + target + ".");
			}
			foreach (EditorCurveBinding binding in AnimationUtility.GetObjectReferenceCurveBindings(clip))
			{
				string target = NormalizeTarget(binding.path);
				if (target == null || binding.type != typeof(SpriteRenderer) || binding.propertyName != "m_Sprite") continue;
				ObjectTrack track = new ObjectTrack { Target = target, Property = "sprite" };
				foreach (ObjectReferenceKeyframe key in AnimationUtility.GetObjectReferenceCurve(clip, binding))
					track.Keys.Add(new ObjectKey { Time = key.time, Asset = key.value == null ? null : AssetDatabase.GetAssetPath(key.value), Name = key.value == null ? null : key.value.name });
				document.ObjectTracks.Add(track);
			}
			foreach (AnimationEvent item in AnimationUtility.GetAnimationEvents(clip))
				document.Events.Add(new EventKey { Time = item.time, Name = item.functionName, StringValue = item.stringParameter, FloatValue = item.floatParameter, IntValue = item.intParameter });
			FinisherMetadata finisher = FindFinisherMetadata(clip);
			if (finisher != null) ExportFinisherVfx(document, finisher);
			foreach (string warning in document.Warnings) Debug.LogWarning("[Modding] " + clip.name + ": " + warning);
			return JObject.FromObject(document, JsonSerializer.CreateDefault());
		}

		private static FinisherMetadata FindFinisherMetadata(AnimationClip i_playerClip)
		{
			foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Actors/Npcs" }))
			{
				string prefabPath = AssetDatabase.GUIDToAssetPath(guid);
				GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
				if (prefab == null) continue;
				foreach (RaperAnimation animation in prefab.GetComponentsInChildren<RaperAnimation>(true))
				{
					SerializedObject serialized = new SerializedObject(animation);
					AnimationClip playerClip = serialized.FindProperty("m_animClipPlayer")?.objectReferenceValue as AnimationClip;
					if (playerClip != i_playerClip) continue;
					return new FinisherMetadata
					{
						Animation = animation,
						EnemyClip = serialized.FindProperty("m_animClipRaper")?.objectReferenceValue as AnimationClip,
						Prefab = prefab,
						PrefabPath = prefabPath
					};
				}
			}
			return null;
		}

		private static void ExportFinisherVfx(AnimationDocument io_document, FinisherMetadata i_finisher)
		{
			AddEffects(io_document, i_finisher, "thrust", i_finisher.Animation.GetParticlesThrust());
			AddEffects(io_document, i_finisher, "cumThrust", i_finisher.Animation.GetParticlesCumThrust());
			AddEffects(io_document, i_finisher, "unique", i_finisher.Animation.GetParticlesUnique());
			if (i_finisher.EnemyClip == null) return;
			foreach (AnimationEvent animationEvent in AnimationUtility.GetAnimationEvents(i_finisher.EnemyClip))
			{
				string trigger = animationEvent.functionName == "Thrust" ? "thrust"
					: animationEvent.functionName == "CumThrust" ? "cumThrust"
					: animationEvent.functionName == "PlayParticleUnique" ? "unique" : null;
				if (trigger == null) continue;
				int uniqueIndex = trigger == "unique" ? Mathf.Max(0, animationEvent.intParameter - 1) : -1;
				List<string> effectIds = io_document.Effects
					.Where(i_effect => i_effect.Trigger == trigger && (uniqueIndex < 0 || i_effect.Index == uniqueIndex))
					.Select(i_effect => i_effect.Id).ToList();
				if (effectIds.Count == 0) continue;
				io_document.EffectTriggers.Add(new EffectTrigger
				{
					Time = animationEvent.time,
					Source = "enemy:" + animationEvent.functionName,
					Effects = effectIds
				});
			}
		}

		private static void AddEffects(AnimationDocument io_document, FinisherMetadata i_finisher,
			string i_trigger, IList<RapeParticleSystem> i_effects)
		{
			for (int index = 0; index < i_effects.Count; index++)
			{
				RapeParticleSystem marker = i_effects[index];
				ParticleSystem particles = marker == null ? null : marker.GetParticleSystem();
				if (particles == null) continue;
				SerializedObject serializedMarker = new SerializedObject(marker);
				Bone enemyBone = serializedMarker.FindProperty("m_boneRaperToParentTo")?.objectReferenceValue as Bone;
				SerializedProperty playerBoneProperty = serializedMarker.FindProperty("m_bonePlayerToParentTo");
				string target = enemyBone != null ? "enemy-bone/" + enemyBone.name : null;
				if (target == null && playerBoneProperty != null)
				{
					string bone = NormalizeBone(((BoneTypePlayer)playerBoneProperty.intValue).ToString());
					if (bone != null) target = "bone/" + bone;
				}
				ParticleSystem.MainModule main = particles.main;
				ParticleSystem.EmissionModule emission = particles.emission;
				ParticleSystem.ShapeModule shape = particles.shape;
				ParticleSystem.TextureSheetAnimationModule textureSheet = particles.textureSheetAnimation;
				ParticleSystemRenderer renderer = particles.GetComponent<ParticleSystemRenderer>();
				List<string> textureSheetSprites = new List<string>();
				if (textureSheet.enabled)
					for (int spriteIndex = 0; spriteIndex < textureSheet.spriteCount; spriteIndex++)
					{
						Sprite sprite = textureSheet.GetSprite(spriteIndex);
						if (sprite != null) textureSheetSprites.Add(AssetDatabase.GetAssetPath(sprite));
					}
				Color colorMin = main.startColor.colorMin;
				Color colorMax = main.startColor.colorMax;
				io_document.Effects.Add(new EffectDefinition
				{
					Id = "vfx/" + i_trigger.ToLowerInvariant() + "-" + (index + 1),
					Trigger = i_trigger,
					Index = index,
					Target = target ?? "bone/hips",
					Duration = main.duration,
					Loop = main.loop,
					MaxParticles = main.maxParticles,
					StartLifetimeMin = main.startLifetime.constantMin,
					StartLifetimeMax = main.startLifetime.constantMax,
					StartSpeedMin = main.startSpeed.constantMin,
					StartSpeedMax = main.startSpeed.constantMax,
					StartSizeMin = main.startSize.constantMin,
					StartSizeMax = main.startSize.constantMax,
					GravityMin = main.gravityModifier.constantMin,
					GravityMax = main.gravityModifier.constantMax,
					ScaleX = particles.transform.localScale.x,
					ScaleY = particles.transform.localScale.y,
					ScaleZ = particles.transform.localScale.z,
					PositionX = particles.transform.localPosition.x,
					PositionY = particles.transform.localPosition.y,
					PositionZ = particles.transform.localPosition.z,
					RotationX = particles.transform.localEulerAngles.x,
					RotationY = particles.transform.localEulerAngles.y,
					RotationZ = particles.transform.localEulerAngles.z,
					SimulationSpace = main.simulationSpace.ToString(),
					StartColorMin = ColorValue.From(colorMin),
					StartColorMax = ColorValue.From(colorMax),
					EmissionRateMin = emission.rateOverTime.constantMin,
					EmissionRateMax = emission.rateOverTime.constantMax,
					ShapeEnabled = shape.enabled,
					Shape = shape.shapeType.ToString(),
					ShapeRadius = shape.radius,
					ShapeAngle = shape.angle,
					TextureSheetTilesX = textureSheet.enabled ? textureSheet.numTilesX : 0,
					TextureSheetTilesY = textureSheet.enabled ? textureSheet.numTilesY : 0,
					TextureSheetSprites = textureSheetSprites,
					SortingLayer = renderer == null ? null : renderer.sortingLayerName,
					SortingOrder = renderer == null ? 0 : renderer.sortingOrder,
					RenderMode = renderer == null ? null : renderer.renderMode.ToString(),
					Mesh = renderer == null || renderer.mesh == null ? null : AssetDatabase.GetAssetPath(renderer.mesh),
					Material = renderer == null || renderer.sharedMaterial == null ? null : AssetDatabase.GetAssetPath(renderer.sharedMaterial),
					Texture = renderer == null || renderer.sharedMaterial == null || renderer.sharedMaterial.mainTexture == null ? null : AssetDatabase.GetAssetPath(renderer.sharedMaterial.mainTexture),
					Source = new EffectSource { UnityPrefab = i_finisher.PrefabPath, HierarchyPath = GetHierarchyPath(i_finisher.Prefab.transform, marker.transform) }
				});
			}
		}

		private static string GetHierarchyPath(Transform i_root, Transform i_target)
		{
			List<string> parts = new List<string>();
			for (Transform current = i_target; current != null && current != i_root; current = current.parent) parts.Add(current.name);
			parts.Reverse();
			return string.Join("/", parts.ToArray());
		}

		private static string NormalizeTarget(string i_path)
		{
			string[] parts = (i_path ?? string.Empty).Split('/');
			if (parts.Length > 0 && parts[0].StartsWith("Skeleton", StringComparison.OrdinalIgnoreCase)
				&& !string.Equals(parts[0], "SkeletonPlayer", StringComparison.OrdinalIgnoreCase)) return null;
			for (int index = parts.Length - 1; index >= 0; index--)
			{
				string part = parts[index];
				if (part.StartsWith("bp_", StringComparison.OrdinalIgnoreCase))
				{
					string bodyBone = NormalizeBone(part.Substring(3));
					return bodyBone == null ? null : "sprite/" + bodyBone;
				}
				string bone = NormalizeBone(part);
				if (bone != null) return "bone/" + bone;
			}
			return null;
		}

		private static string NormalizeBone(string i_name)
		{
			return i_name != null && BoneNames.TryGetValue(i_name, out string value) ? value : null;
		}

		private static string NormalizeProperty(Type i_type, string i_property)
		{
			if (i_type == typeof(Transform))
			{
				if (i_property.StartsWith("m_LocalPosition.")) return "position." + i_property.Last();
				if (i_property.StartsWith("m_LocalScale.")) return "scale." + i_property.Last();
				if (i_property.StartsWith("localEulerAnglesRaw.")) return "rotation." + i_property.Last();
			}
			if (i_type == typeof(SpriteRenderer))
			{
				if (i_property.StartsWith("m_Color.")) return "color." + i_property.Last();
				if (i_property == "m_SortingOrder") return "sortingOrder";
			}
			if (i_type == typeof(BodyPartPlayer) && i_property == "m_sortingOrder") return "sortingOrder";
			return null;
		}

		private static BonePlayer FindParentBone(Transform i_transform)
		{
			Transform parent = i_transform.parent;
			while (parent != null)
			{
				BonePlayer bone = parent.GetComponent<BonePlayer>();
				if (bone != null) return bone;
				parent = parent.parent;
			}
			return null;
		}

		private static float? Finite(float i_value) { return float.IsNaN(i_value) || float.IsInfinity(i_value) ? (float?)null : i_value; }
		private static void Write(string i_name, object i_value) { File.WriteAllText(System.IO.Path.Combine(OutputDirectory, i_name), JsonConvert.SerializeObject(i_value, Formatting.Indented)); }

		[Serializable] private sealed class RigDocument { [JsonProperty("schemaVersion", Order = 0)] public int SchemaVersion; [JsonProperty("type", Order = 1)] public string Type; [JsonProperty("id", Order = 2)] public string Id; [JsonProperty("prefab", Order = 3)] public string Prefab; [JsonProperty("sampleRoot", Order = 4)] public string SampleRoot; [JsonProperty("coordinateSystem", Order = 5)] public CoordinateSystem CoordinateSystem; [JsonProperty("bones", Order = 6)] public List<RigBone> Bones = new List<RigBone>(); [JsonProperty("clothingContract", Order = 7)] public ClothingContract Clothing; [JsonProperty("source", Order = 8)] public SourceReference Source; }
		[Serializable] private sealed class CoordinateSystem { [JsonProperty("units")] public string Units; [JsonProperty("yUp")] public bool YUp; [JsonProperty("rotation")] public string Rotation; [JsonProperty("pixelsPerUnit")] public float PixelsPerUnit; }
		[Serializable] private sealed class RigBone { [JsonProperty("name")] public string Name; [JsonProperty("parent", NullValueHandling = NullValueHandling.Include)] public string Parent; [JsonProperty("unityPath")] public string UnityPath; [JsonProperty("defaultTransform")] public TransformValue DefaultTransform; [JsonProperty("attachment")] public AttachmentPoint Attachment; [JsonProperty("spriteCanvas", NullValueHandling = NullValueHandling.Ignore)] public SpriteCanvas SpriteCanvas; [JsonProperty("sprites")] public List<SpriteReference> Sprites = new List<SpriteReference>(); }
		[Serializable] private sealed class SpriteReference { [JsonProperty("unityPath")] public string UnityPath; [JsonProperty("name")] public string Name; [JsonProperty("asset")] public string Asset; [JsonProperty("pivotX")] public float PivotX; [JsonProperty("pivotY")] public float PivotY; [JsonProperty("sortingLayer")] public string SortingLayer; [JsonProperty("sortingOrder")] public int SortingOrder; }
		[Serializable] private sealed class TransformValue { [JsonProperty("x")] public float X; [JsonProperty("y")] public float Y; [JsonProperty("rotation")] public float Rotation; [JsonProperty("scaleX")] public float ScaleX; [JsonProperty("scaleY")] public float ScaleY; public static TransformValue From(Transform value) { return new TransformValue { X = value.localPosition.x, Y = value.localPosition.y, Rotation = value.localEulerAngles.z, ScaleX = value.localScale.x, ScaleY = value.localScale.y }; } }
		[Serializable] private sealed class AttachmentPoint { [JsonProperty("target")] public string Target; [JsonProperty("pivotX")] public float PivotX; [JsonProperty("pivotY")] public float PivotY; }
		[Serializable] private sealed class SpriteCanvas { [JsonProperty("width")] public int Width; [JsonProperty("height")] public int Height; [JsonProperty("pivotX")] public float PivotX; [JsonProperty("pivotY")] public float PivotY; [JsonProperty("sortingOrder")] public int SortingOrder; }
		[Serializable] private sealed class ClothingContract { [JsonProperty("slots")] public List<string> Slots; [JsonProperty("categories")] public List<string> Categories; [JsonProperty("pivotCoordinates")] public string PivotCoordinates; [JsonProperty("canvasBounds")] public string CanvasBounds; [JsonProperty("sortingOrderMin")] public int SortingOrderMin; [JsonProperty("sortingOrderMax")] public int SortingOrderMax; [JsonProperty("sortingOffsetMin")] public int SortingOffsetMin; [JsonProperty("sortingOffsetMax")] public int SortingOffsetMax; [JsonProperty("sameCategoryCompatible")] public bool SameCategoryCompatible; [JsonProperty("incompatibleCategoriesSymmetric")] public bool IncompatibleCategoriesSymmetric; [JsonProperty("bodyVariantMatchOrder")] public List<string> BodyVariantMatchOrder; [JsonProperty("bodyVariantFallback")] public string BodyVariantFallback; }
		[Serializable] private sealed class SourceReference { [JsonProperty("unityPrefab", NullValueHandling = NullValueHandling.Ignore)] public string UnityPrefab; [JsonProperty("unityClip", NullValueHandling = NullValueHandling.Ignore)] public string UnityClip; }
		[Serializable] private sealed class AnimationDocument { [JsonProperty("schemaVersion", Order = 0)] public int SchemaVersion; [JsonProperty("type", Order = 1)] public string Type; [JsonProperty("id", Order = 2)] public string Id; [JsonProperty("rig", Order = 3)] public string Rig; [JsonProperty("displayName", Order = 4)] public string DisplayName; [JsonProperty("durationSeconds", Order = 5)] public float DurationSeconds; [JsonProperty("frameRate", Order = 6)] public float FrameRate; [JsonProperty("loop", Order = 7)] public bool Loop; [JsonProperty("tracks", Order = 8)] public List<Track> Tracks = new List<Track>(); [JsonProperty("objectTracks", Order = 9)] public List<ObjectTrack> ObjectTracks = new List<ObjectTrack>(); [JsonProperty("events", Order = 10)] public List<EventKey> Events = new List<EventKey>(); [JsonProperty("effects", Order = 11)] public List<EffectDefinition> Effects = new List<EffectDefinition>(); [JsonProperty("effectTriggers", Order = 12)] public List<EffectTrigger> EffectTriggers = new List<EffectTrigger>(); [JsonProperty("warnings", Order = 13)] public List<string> Warnings = new List<string>(); [JsonProperty("source", Order = 14)] public SourceReference Source; }
		[Serializable] private sealed class Track { [JsonProperty("target")] public string Target; [JsonProperty("property")] public string Property; [JsonProperty("keys")] public List<NumericKey> Keys = new List<NumericKey>(); }
		[Serializable] private sealed class NumericKey { [JsonProperty("time")] public float Time; [JsonProperty("value")] public float Value; [JsonProperty("inTangent", NullValueHandling = NullValueHandling.Include)] public float? InTangent; [JsonProperty("outTangent", NullValueHandling = NullValueHandling.Include)] public float? OutTangent; }
		[Serializable] private sealed class ObjectTrack { [JsonProperty("target")] public string Target; [JsonProperty("property")] public string Property; [JsonProperty("keys")] public List<ObjectKey> Keys = new List<ObjectKey>(); }
		[Serializable] private sealed class ObjectKey { [JsonProperty("time")] public float Time; [JsonProperty("asset")] public string Asset; [JsonProperty("name")] public string Name; }
		[Serializable] private sealed class EventKey { [JsonProperty("time")] public float Time; [JsonProperty("name")] public string Name; [JsonProperty("stringValue", DefaultValueHandling = DefaultValueHandling.Ignore)] public string StringValue; [JsonProperty("floatValue", DefaultValueHandling = DefaultValueHandling.Ignore)] public float FloatValue; [JsonProperty("intValue", DefaultValueHandling = DefaultValueHandling.Ignore)] public int IntValue; }
		[Serializable] private sealed class EffectDefinition { [JsonProperty("id")] public string Id; [JsonProperty("trigger")] public string Trigger; [JsonProperty("index")] public int Index; [JsonProperty("target")] public string Target; [JsonProperty("durationSeconds")] public float Duration; [JsonProperty("loop")] public bool Loop; [JsonProperty("maxParticles")] public int MaxParticles; [JsonProperty("startLifetimeMin")] public float StartLifetimeMin; [JsonProperty("startLifetimeMax")] public float StartLifetimeMax; [JsonProperty("startSpeedMin")] public float StartSpeedMin; [JsonProperty("startSpeedMax")] public float StartSpeedMax; [JsonProperty("startSizeMin")] public float StartSizeMin; [JsonProperty("startSizeMax")] public float StartSizeMax; [JsonProperty("gravityMin")] public float GravityMin; [JsonProperty("gravityMax")] public float GravityMax; [JsonProperty("scaleX")] public float ScaleX; [JsonProperty("scaleY")] public float ScaleY; [JsonProperty("scaleZ")] public float ScaleZ; [JsonProperty("positionX")] public float PositionX; [JsonProperty("positionY")] public float PositionY; [JsonProperty("positionZ")] public float PositionZ; [JsonProperty("rotationX")] public float RotationX; [JsonProperty("rotationY")] public float RotationY; [JsonProperty("rotationZ")] public float RotationZ; [JsonProperty("simulationSpace")] public string SimulationSpace; [JsonProperty("startColorMin")] public ColorValue StartColorMin; [JsonProperty("startColorMax")] public ColorValue StartColorMax; [JsonProperty("emissionRateMin")] public float EmissionRateMin; [JsonProperty("emissionRateMax")] public float EmissionRateMax; [JsonProperty("shapeEnabled")] public bool ShapeEnabled; [JsonProperty("shape")] public string Shape; [JsonProperty("shapeRadius")] public float ShapeRadius; [JsonProperty("shapeAngle")] public float ShapeAngle; [JsonProperty("textureSheetTilesX")] public int TextureSheetTilesX; [JsonProperty("textureSheetTilesY")] public int TextureSheetTilesY; [JsonProperty("textureSheetSprites")] public List<string> TextureSheetSprites; [JsonProperty("sortingLayer", NullValueHandling = NullValueHandling.Ignore)] public string SortingLayer; [JsonProperty("sortingOrder")] public int SortingOrder; [JsonProperty("renderMode", NullValueHandling = NullValueHandling.Ignore)] public string RenderMode; [JsonProperty("mesh", NullValueHandling = NullValueHandling.Ignore)] public string Mesh; [JsonProperty("material", NullValueHandling = NullValueHandling.Ignore)] public string Material; [JsonProperty("texture", NullValueHandling = NullValueHandling.Ignore)] public string Texture; [JsonProperty("source")] public EffectSource Source; }
		[Serializable] private sealed class EffectTrigger { [JsonProperty("time")] public float Time; [JsonProperty("source")] public string Source; [JsonProperty("effects")] public List<string> Effects; }
		[Serializable] private sealed class EffectSource { [JsonProperty("unityPrefab")] public string UnityPrefab; [JsonProperty("hierarchyPath")] public string HierarchyPath; }
		[Serializable] private sealed class ColorValue { [JsonProperty("r")] public float R; [JsonProperty("g")] public float G; [JsonProperty("b")] public float B; [JsonProperty("a")] public float A; public static ColorValue From(Color value) { return new ColorValue { R = value.r, G = value.g, B = value.b, A = value.a }; } }
		private sealed class FinisherMetadata { public RaperAnimation Animation; public AnimationClip EnemyClip; public GameObject Prefab; public string PrefabPath; }
	}
}
