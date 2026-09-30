using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CaptivityReloaded.Modding;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using IOPath = System.IO.Path;

namespace CaptivityReloaded.Editor.Modding
{
	public sealed partial class PlayerAnimationPreviewWindow
	{
		[SerializeField] private int m_vfxEffectIndex;
		[SerializeField] private int m_vfxTriggerIndex;
		[SerializeField] private Vector2 m_vfxLibraryScroll;
		[SerializeField] private Vector2 m_vfxInspectorScroll;
		[SerializeField] private Vector2 m_vfxWorkspaceScroll;
		[SerializeField] private int m_vfxLaneIndex;
		private readonly HashSet<NormalizedEffectTrigger> m_vfxSelectedTriggers = new HashSet<NormalizedEffectTrigger>();
		private readonly HashSet<EnemyAnimationEventDefinition> m_vfxSelectedEvents = new HashSet<EnemyAnimationEventDefinition>();
		private Material m_vfxPreviewMaterial;
		private readonly HashSet<string> m_vfxTextureFailures = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		private readonly Dictionary<Texture2D, Material> m_vfxTextureMaterials = new Dictionary<Texture2D, Material>();
		private readonly Dictionary<string, Sprite> m_vfxRegionSprites = new Dictionary<string, Sprite>(StringComparer.Ordinal);
		private static readonly string[] VfxLaneNames = { "Particle", "Audio", "Hitbox", "Camera", "Light", "Trail", "Decal" };
		private readonly List<VfxLibraryEntry> m_currentEnemyVfxLibrary = new List<VfxLibraryEntry>();
		private readonly List<VfxAudioLibraryEntry> m_currentEnemyAudioLibrary = new List<VfxAudioLibraryEntry>();
		private string m_currentEnemyVfxLibrarySignature;

		private void DrawAnimationVfxStudio()
		{
			if (m_normalizedEnemyClip == null)
			{
				EditorGUILayout.HelpBox("Select a modded enemy animation before opening the VFX Timeline.", MessageType.Info);
				return;
			}

			EnsureVfxCollections();
			DrawVfxToolbar();
			m_vfxWorkspaceScroll = EditorGUILayout.BeginScrollView(m_vfxWorkspaceScroll, true, true);
			float contentWidth = Mathf.Max(780f, position.width - 34f);
			float mainHeight = Mathf.Clamp(position.height - 285f, 310f, 520f);
			float libraryWidth = Mathf.Clamp(contentWidth * 0.22f, 175f, 250f);
			float inspectorWidth = Mathf.Clamp(contentWidth * 0.31f, 260f, 355f);
			float viewportWidth = contentWidth - libraryWidth - inspectorWidth - 8f;
			using (new EditorGUILayout.HorizontalScope(GUILayout.Width(contentWidth), GUILayout.Height(mainHeight)))
			{
				DrawVfxLibrary(libraryWidth, mainHeight);
				GUILayout.Space(4f);
				DrawVfxViewport(viewportWidth, mainHeight);
				GUILayout.Space(4f);
				DrawVfxInspector(inspectorWidth, mainHeight);
			}
			DrawVfxTimeline();
			DrawAuthoringValidation();
			EditorGUILayout.EndScrollView();
		}

		private void DrawVfxToolbar()
		{
			using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
			{
				GUILayout.Label("ANIMATION VFX TIMELINE", EditorStyles.miniBoldLabel, GUILayout.Width(155f));
				if (GUILayout.Button(m_playing ? "Pause" : "Play", EditorStyles.toolbarButton, GUILayout.Width(48f))) m_playing = !m_playing;
				if (GUILayout.Button("Restart", EditorStyles.toolbarButton, GUILayout.Width(50f))) { m_playing = false; m_time = 0f; SampleCurrentClip(); }
				if (GUILayout.Button("<", EditorStyles.toolbarButton, GUILayout.Width(24f))) StepFrame(-1);
				if (GUILayout.Button(">", EditorStyles.toolbarButton, GUILayout.Width(24f))) StepFrame(1);
				m_loop = GUILayout.Toggle(m_loop, "Loop", EditorStyles.toolbarButton, GUILayout.Width(45f));
				bool previewVfx = GUILayout.Toggle(m_showFinisherEffects, "Preview VFX", EditorStyles.toolbarButton, GUILayout.Width(82f));
				if (previewVfx != m_showFinisherEffects) { m_showFinisherEffects = previewVfx; SampleCurrentClip(); Repaint(); }
				GUILayout.FlexibleSpace();
				GUILayout.Label(VfxTimeLabel(), EditorStyles.miniLabel, GUILayout.Width(105f));
				using (new EditorGUI.DisabledScope(!string.IsNullOrEmpty(m_authoringValidation)))
					if (GUILayout.Button("Save JSON", EditorStyles.toolbarButton, GUILayout.Width(70f))) SaveAnimationToMod();
			}
		}

		private string VfxTimeLabel()
		{
			float fps = Mathf.Max(1f, m_normalizedEnemyClip.FrameRate);
			return m_time.ToString("0.000") + "s  F" + Mathf.RoundToInt(m_time * fps);
		}

		private static void DrawVfxPanel(Rect i_rect, string i_title)
		{
			EditorGUI.DrawRect(i_rect, new Color(0.105f, 0.105f, 0.105f, 1f));
			EditorGUI.DrawRect(new Rect(i_rect.x, i_rect.y, i_rect.width, 22f), new Color(0.16f, 0.16f, 0.16f, 1f));
			GUI.Label(new Rect(i_rect.x + 6f, i_rect.y + 3f, i_rect.width - 12f, 18f), i_title, EditorStyles.miniBoldLabel);
		}

		private void DrawVfxLibrary(float i_width, float i_height)
		{
			EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(i_width), GUILayout.Height(i_height));
			EditorGUILayout.LabelField("EFFECT LIBRARY", EditorStyles.miniBoldLabel);
			m_vfxLibraryScroll = EditorGUILayout.BeginScrollView(m_vfxLibraryScroll, GUILayout.ExpandHeight(true));
			List<NormalizedEffectDefinition> effects = m_normalizedEnemyClip.Effects;
			using (new EditorGUILayout.HorizontalScope())
			{
				if (GUILayout.Button("+ New")) AddVfxEffect();
				using (new EditorGUI.DisabledScope(effects.Count == 0)) if (GUILayout.Button("Duplicate")) DuplicateVfxEffect();
			}
			using (new EditorGUILayout.HorizontalScope())
			{
				if (GUILayout.Button("Starter preset")) ShowVfxPresetMenu();
				using (new EditorGUI.DisabledScope(effects.Count == 0)) if (GUILayout.Button("Copy")) CopyVfxEffect();
				if (GUILayout.Button("Paste")) PasteVfxEffect();
			}
			using (new EditorGUI.DisabledScope(effects.Count == 0))
				if (GUILayout.Button("Delete effect")) DeleteVfxEffect();
			EditorGUILayout.Space(5f);
			EditorGUILayout.LabelField("STARTER EFFECTS", EditorStyles.miniBoldLabel);
			if (GUILayout.Button("Impact burst")) AddVfxPreset("Impact burst");
			if (GUILayout.Button("Smoke puff")) AddVfxPreset("Smoke puff");
			if (GUILayout.Button("Electric sparks")) AddVfxPreset("Electric sparks");
			if (GUILayout.Button("Glow light")) AddVfxPreset("Glow light");
			if (GUILayout.Button("Motion trail")) AddVfxPreset("Motion trail");
			if (GUILayout.Button("Ground decal")) AddVfxPreset("Ground decal");
			EditorGUILayout.Space(5f);
			RefreshCurrentEnemyVfxLibrary();
			using (new EditorGUILayout.HorizontalScope())
			{
				EditorGUILayout.LabelField("CURRENT ENEMY EFFECTS (" + m_currentEnemyVfxLibrary.Count + ")", EditorStyles.miniBoldLabel);
				if (GUILayout.Button("Refresh", EditorStyles.miniButton, GUILayout.Width(52f))) { m_currentEnemyVfxLibrarySignature = null; RefreshCurrentEnemyVfxLibrary(); }
			}
			if (m_currentEnemyVfxLibrary.Count == 0)
				EditorGUILayout.HelpBox("No reusable effects were found in this enemy's other animations or its loaded paired animation.", MessageType.None);
			foreach (VfxLibraryEntry entry in m_currentEnemyVfxLibrary)
			{
				using (new EditorGUILayout.HorizontalScope())
				{
					GUILayout.Label(new GUIContent(entry.Effect.Id, entry.Source + "\n" + entry.Path), EditorStyles.miniLabel, GUILayout.ExpandWidth(true));
					if (GUILayout.Button("Add copy", EditorStyles.miniButton, GUILayout.Width(62f))) ImportVfxLibraryEntry(entry);
				}
				EditorGUILayout.LabelField(entry.Source, EditorStyles.centeredGreyMiniLabel);
			}
			EditorGUILayout.Space(5f);
			EditorGUILayout.LabelField("CURRENT ENEMY AUDIO (" + m_currentEnemyAudioLibrary.Count + ")", EditorStyles.miniBoldLabel);
			if (m_currentEnemyAudioLibrary.Count == 0)
				EditorGUILayout.HelpBox("No reusable sound markers were found in the enemy's other authored animations.", MessageType.None);
			foreach (VfxAudioLibraryEntry entry in m_currentEnemyAudioLibrary)
			{
				using (new EditorGUILayout.HorizontalScope())
				{
					GUILayout.Label(new GUIContent(IOPath.GetFileName(entry.File), entry.Source + "\n" + entry.File), EditorStyles.miniLabel, GUILayout.ExpandWidth(true));
					if (GUILayout.Button("Add at time", EditorStyles.miniButton, GUILayout.Width(72f))) ImportVfxAudioEntry(entry);
				}
			}
			EditorGUILayout.Space(5f);
			EditorGUILayout.LabelField("CURRENT ANIMATION (" + effects.Count + ")", EditorStyles.miniBoldLabel);
			if (effects.Count == 0)
				EditorGUILayout.HelpBox("Choose a starter effect above or create a blank effect.", MessageType.Info);
			for (int index = 0; index < effects.Count; index++)
			{
				NormalizedEffectDefinition effect = effects[index];
				bool selected = index == m_vfxEffectIndex;
				GUIContent label = new GUIContent((selected ? "● " : "○ ") + (string.IsNullOrEmpty(effect.Id) ? "Unnamed effect" : effect.Id), effect.Target);
				if (GUILayout.Toggle(selected, label, "Button")) m_vfxEffectIndex = index;
			}
			EditorGUILayout.EndScrollView();
			EditorGUILayout.EndVertical();
		}

		private void RefreshCurrentEnemyVfxLibrary()
		{
			List<string> files = new List<string>();
			if (m_mode == 3)
				files.AddRange(MatchingModAnimations().Select(i_item => i_item.Path));
			else if (!string.IsNullOrEmpty(m_normalizedEnemyPath))
			{
				string directory = IOPath.GetDirectoryName(IOPath.GetFullPath(m_normalizedEnemyPath));
				if (Directory.Exists(directory)) files.AddRange(Directory.GetFiles(directory, "*.json", SearchOption.TopDirectoryOnly));
			}
			files = files.Where(File.Exists).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(i_path => i_path, StringComparer.OrdinalIgnoreCase).ToList();
			string signature = string.Join("|", files.Select(i_path => i_path + ":" + File.GetLastWriteTimeUtc(i_path).Ticks))
				+ "|pair:" + (m_studioPlayerAnimationPath ?? string.Empty) + ":"
				+ Newtonsoft.Json.JsonConvert.SerializeObject(m_studioPlayerAnimation?.Effects ?? new List<NormalizedEffectDefinition>());
			if (signature == m_currentEnemyVfxLibrarySignature) return;
			m_currentEnemyVfxLibrarySignature = signature;
			m_currentEnemyVfxLibrary.Clear();
			m_currentEnemyAudioLibrary.Clear();
			foreach (string path in files)
			{
				if (!string.IsNullOrEmpty(m_normalizedEnemyPath)
					&& string.Equals(IOPath.GetFullPath(path), IOPath.GetFullPath(m_normalizedEnemyPath), StringComparison.OrdinalIgnoreCase)) continue;
				try
				{
					NormalizedEnemyAnimationDocument document = Newtonsoft.Json.JsonConvert.DeserializeObject<NormalizedEnemyAnimationDocument>(File.ReadAllText(path));
					if (document?.Effects == null || document.Enemy != m_normalizedEnemyClip.Enemy) continue;
					foreach (NormalizedEffectDefinition effect in document.Effects.Where(i_effect => i_effect != null))
						AddCurrentEnemyVfxLibraryEntry(effect, document.DisplayName ?? IOPath.GetFileNameWithoutExtension(path), path);
					foreach (EnemyAnimationEventDefinition audio in (document.Events ?? new List<EnemyAnimationEventDefinition>()).Where(i_event => i_event != null && i_event.Type == "sound" && !string.IsNullOrWhiteSpace(i_event.File)))
						AddCurrentEnemyAudioLibraryEntry(audio, document.DisplayName ?? IOPath.GetFileNameWithoutExtension(path), path);
				}
				catch (Exception) { }
			}
			if (m_studioPlayerAnimation?.Effects != null)
				foreach (NormalizedEffectDefinition effect in m_studioPlayerAnimation.Effects.Where(i_effect => i_effect != null))
					AddCurrentEnemyVfxLibraryEntry(effect, "Paired player: " + (m_studioPlayerAnimation.DisplayName ?? "animation"), m_studioPlayerAnimationPath);
			List<string> sourcePrefabs = m_currentEnemyVfxLibrary
				.Select(i_entry => i_entry.Effect.Source?["unityPrefab"]?.Value<string>())
				.Where(i_path => !string.IsNullOrEmpty(i_path)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
			foreach (string prefabPath in sourcePrefabs) HarvestCorePrefabEffects(prefabPath);
		}

		private void HarvestCorePrefabEffects(string i_prefabPath)
		{
			GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(i_prefabPath);
			if (prefab == null) return;
			RapeParticleSystem[] markers = prefab.GetComponentsInChildren<RapeParticleSystem>(true);
			for (int index = 0; index < markers.Length; index++)
			{
				NormalizedEffectDefinition effect = ConvertCoreParticleEffect(markers[index], i_prefabPath, index);
				if (effect != null) AddCurrentEnemyVfxLibraryEntry(effect, "Core prefab: " + prefab.name, i_prefabPath);
			}
			foreach (AudioSource source in prefab.GetComponentsInChildren<AudioSource>(true))
			{
				if (source == null || source.clip == null) continue;
				string clipPath = AssetDatabase.GetAssetPath(source.clip);
				string extension = IOPath.GetExtension(clipPath).ToLowerInvariant();
				if (extension != ".wav" && extension != ".ogg") continue;
				AddCurrentEnemyAudioLibraryEntry(new EnemyAnimationEventDefinition { Type = "sound", File = clipPath, Volume = source.volume }, "Core prefab: " + prefab.name, i_prefabPath);
			}
		}

		private static NormalizedEffectDefinition ConvertCoreParticleEffect(RapeParticleSystem i_marker, string i_prefabPath, int i_index)
		{
			ParticleSystem particles = i_marker == null ? null : i_marker.GetParticleSystem();
			if (particles == null) return null;
			SerializedObject marker = new SerializedObject(i_marker);
			Bone enemyBone = marker.FindProperty("m_boneRaperToParentTo")?.objectReferenceValue as Bone;
			SerializedProperty playerBone = marker.FindProperty("m_bonePlayerToParentTo");
			string target = enemyBone != null ? "enemy-bone/" + enemyBone.name : "bone/" + NormalizeVfxPlayerBone(playerBone == null ? BoneTypePlayer.Hips : (BoneTypePlayer)playerBone.intValue);
			ParticleSystem.MainModule main = particles.main;
			ParticleSystem.EmissionModule emission = particles.emission;
			ParticleSystem.ShapeModule shape = particles.shape;
			ParticleSystem.TextureSheetAnimationModule sheet = particles.textureSheetAnimation;
			ParticleSystem.ColorOverLifetimeModule colorOverLifetime = particles.colorOverLifetime;
			ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = particles.sizeOverLifetime;
			ParticleSystem.VelocityOverLifetimeModule velocityOverLifetime = particles.velocityOverLifetime;
			ParticleSystem.RotationOverLifetimeModule rotationOverLifetime = particles.rotationOverLifetime;
			ParticleSystem.NoiseModule noise = particles.noise;
			ParticleSystemRenderer renderer = particles.GetComponent<ParticleSystemRenderer>();
			string markerName = i_marker.name ?? "effect";
			string trigger = markerName.IndexOf("cum", StringComparison.OrdinalIgnoreCase) >= 0 ? "cumThrust"
				: markerName.IndexOf("unique", StringComparison.OrdinalIgnoreCase) >= 0 ? "unique" : "thrust";
			Color colorMin = main.startColor.colorMin, colorMax = main.startColor.colorMax;
			return new NormalizedEffectDefinition
			{
				Id = "core-" + SlugForFile(markerName), Kind = "particle", Trigger = trigger, Index = i_index, Target = target,
				DurationSeconds = main.duration, Loop = main.loop, MaxParticles = main.maxParticles,
				StartLifetimeMin = main.startLifetime.constantMin, StartLifetimeMax = main.startLifetime.constantMax,
				StartSpeedMin = main.startSpeed.constantMin, StartSpeedMax = main.startSpeed.constantMax,
				StartSizeMin = main.startSize.constantMin, StartSizeMax = main.startSize.constantMax,
				GravityMin = main.gravityModifier.constantMin, GravityMax = main.gravityModifier.constantMax,
				ScaleX = particles.transform.localScale.x, ScaleY = particles.transform.localScale.y, ScaleZ = particles.transform.localScale.z,
				PositionX = particles.transform.localPosition.x, PositionY = particles.transform.localPosition.y, PositionZ = particles.transform.localPosition.z,
				RotationX = particles.transform.localEulerAngles.x, RotationY = particles.transform.localEulerAngles.y, RotationZ = particles.transform.localEulerAngles.z,
				SimulationSpace = main.simulationSpace.ToString(), StartColorMin = VfxColor(colorMin), StartColorMax = VfxColor(colorMax),
				EmissionRateMin = emission.rateOverTime.constantMin, EmissionRateMax = emission.rateOverTime.constantMax,
				ShapeEnabled = shape.enabled, Shape = shape.shapeType.ToString(), ShapeRadius = shape.radius, ShapeAngle = shape.angle,
				TextureSheetTilesX = sheet.enabled ? Mathf.Max(1, sheet.numTilesX) : 1, TextureSheetTilesY = sheet.enabled ? Mathf.Max(1, sheet.numTilesY) : 1,
				StartLifetimeCurve = CaptureVfxCurve(main.startLifetime), StartSpeedCurve = CaptureVfxCurve(main.startSpeed), StartSizeCurve = CaptureVfxCurve(main.startSize),
				GravityCurve = CaptureVfxCurve(main.gravityModifier), EmissionRateCurve = CaptureVfxCurve(emission.rateOverTime), EmissionRateOverDistanceCurve = CaptureVfxCurve(emission.rateOverDistance),
				StartColorGradient = CaptureVfxGradient(main.startColor),
				Bursts = CaptureVfxBursts(emission), ColorOverLifetime = colorOverLifetime.enabled ? CaptureVfxGradient(colorOverLifetime.color) : null,
				SizeOverLifetime = CaptureVfxAxisCurves(sizeOverLifetime.enabled, sizeOverLifetime.separateAxes, null, sizeOverLifetime.separateAxes ? sizeOverLifetime.x : sizeOverLifetime.size, sizeOverLifetime.y, sizeOverLifetime.z),
				VelocityOverLifetime = CaptureVfxAxisCurves(velocityOverLifetime.enabled, true, velocityOverLifetime.space.ToString(), velocityOverLifetime.x, velocityOverLifetime.y, velocityOverLifetime.z),
				RotationOverLifetime = CaptureVfxAxisCurves(rotationOverLifetime.enabled, rotationOverLifetime.separateAxes, null, rotationOverLifetime.x, rotationOverLifetime.y, rotationOverLifetime.z),
				Noise = noise.enabled ? CaptureVfxNoise(noise) : null,
				TextureSheetFrameOverTime = sheet.enabled ? CaptureVfxCurve(sheet.frameOverTime) : null, TextureSheetStartFrame = sheet.enabled ? CaptureVfxCurve(sheet.startFrame) : null,
				TextureSheetAnimation = sheet.enabled ? sheet.animation.ToString() : null, TextureSheetCycleCount = sheet.enabled ? sheet.cycleCount : 1,
				TextureSheetRowIndex = sheet.enabled ? sheet.rowIndex : 0, TextureSheetUseRandomRow = sheet.enabled && sheet.useRandomRow,
				TextureSheetSprites = new List<string>(), SortingLayer = renderer == null ? null : renderer.sortingLayerName,
				SortingOrder = renderer == null ? 0 : renderer.sortingOrder, RenderMode = renderer == null ? null : renderer.renderMode.ToString(),
				Mesh = renderer == null || renderer.mesh == null ? null : AssetDatabase.GetAssetPath(renderer.mesh),
				Material = renderer == null || renderer.sharedMaterial == null ? null : AssetDatabase.GetAssetPath(renderer.sharedMaterial),
				Texture = renderer == null || renderer.sharedMaterial == null || renderer.sharedMaterial.mainTexture == null ? null : AssetDatabase.GetAssetPath(renderer.sharedMaterial.mainTexture),
				RuntimeTexture = renderer == null || renderer.sharedMaterial == null ? null : renderer.sharedMaterial.mainTexture as Texture2D,
				Source = new Newtonsoft.Json.Linq.JObject { ["unityPrefab"] = i_prefabPath, ["hierarchyPath"] = AnimationVfxHierarchyPath(i_marker.transform.root, i_marker.transform) }
			};
		}

		private static List<NormalizedParticleBurst> CaptureVfxBursts(ParticleSystem.EmissionModule i_emission)
		{
			List<NormalizedParticleBurst> result = new List<NormalizedParticleBurst>();
			for (int index = 0; index < i_emission.burstCount; index++)
			{
				ParticleSystem.Burst burst = i_emission.GetBurst(index);
				result.Add(new NormalizedParticleBurst { Time = burst.time, CountMin = burst.count.constantMin, CountMax = burst.count.constantMax,
					CycleCount = burst.cycleCount, RepeatInterval = burst.repeatInterval, Probability = burst.probability });
			}
			return result;
		}

		private static NormalizedParticleCurve CaptureVfxCurve(ParticleSystem.MinMaxCurve i_curve)
		{
			return new NormalizedParticleCurve { Mode = i_curve.mode.ToString(), Multiplier = i_curve.curveMultiplier, ConstantMin = i_curve.constantMin, ConstantMax = i_curve.constantMax,
				CurveMin = CaptureVfxCurveKeys(i_curve.curveMin), CurveMax = CaptureVfxCurveKeys(i_curve.curveMax) };
		}

		private static List<NormalizedParticleCurveKey> CaptureVfxCurveKeys(AnimationCurve i_curve)
		{
			return i_curve == null ? new List<NormalizedParticleCurveKey>() : i_curve.keys.Select(i_key => new NormalizedParticleCurveKey
				{ Time = i_key.time, Value = i_key.value, InTangent = VfxFinite(i_key.inTangent), OutTangent = VfxFinite(i_key.outTangent) }).ToList();
		}

		private static NormalizedParticleGradient CaptureVfxGradient(ParticleSystem.MinMaxGradient i_gradient)
		{
			return new NormalizedParticleGradient { Mode = i_gradient.mode.ToString(), ColorMin = VfxColor(i_gradient.colorMin), ColorMax = VfxColor(i_gradient.colorMax),
				GradientMin = CaptureVfxGradientKeys(i_gradient.gradientMin), GradientMax = CaptureVfxGradientKeys(i_gradient.gradientMax) };
		}

		private static List<NormalizedParticleColorKey> CaptureVfxGradientKeys(Gradient i_gradient)
		{
			if (i_gradient == null) return new List<NormalizedParticleColorKey>();
			List<float> times = i_gradient.colorKeys.Select(i_key => i_key.time).Concat(i_gradient.alphaKeys.Select(i_key => i_key.time)).Distinct().OrderBy(i_time => i_time).ToList();
			if (times.Count > 8) times = Enumerable.Range(0, 8).Select(i_index => i_index / 7f).ToList();
			if (times.Count == 1) times.Add(times[0] < .5f ? 1f : 0f);
			return times.OrderBy(i_time => i_time).Select(i_time => new NormalizedParticleColorKey { Time = i_time, Color = VfxColor(i_gradient.Evaluate(i_time)) }).ToList();
		}

		private static float VfxFinite(float i_value) { return float.IsNaN(i_value) || float.IsInfinity(i_value) ? 0f : i_value; }

		private static NormalizedParticleAxisCurves CaptureVfxAxisCurves(bool i_enabled, bool i_separate, string i_space,
			ParticleSystem.MinMaxCurve i_x, ParticleSystem.MinMaxCurve i_y, ParticleSystem.MinMaxCurve i_z)
		{
			return i_enabled ? new NormalizedParticleAxisCurves { Enabled = true, SeparateAxes = i_separate, Space = i_space,
				X = CaptureVfxCurve(i_x), Y = CaptureVfxCurve(i_y), Z = CaptureVfxCurve(i_z) } : null;
		}

		private static NormalizedParticleNoise CaptureVfxNoise(ParticleSystem.NoiseModule i_noise)
		{
			return new NormalizedParticleNoise { Enabled = true, SeparateAxes = i_noise.separateAxes, StrengthX = CaptureVfxCurve(i_noise.strengthX),
				StrengthY = CaptureVfxCurve(i_noise.strengthY), StrengthZ = CaptureVfxCurve(i_noise.strengthZ), Frequency = i_noise.frequency,
				ScrollSpeed = CaptureVfxCurve(i_noise.scrollSpeed), Damping = i_noise.damping, OctaveCount = i_noise.octaveCount,
				OctaveMultiplier = i_noise.octaveMultiplier, OctaveScale = i_noise.octaveScale, Quality = i_noise.quality.ToString() };
		}

		private static string AnimationVfxHierarchyPath(Transform i_root, Transform i_target)
		{
			List<string> parts = new List<string>();
			for (Transform current = i_target; current != null && current != i_root; current = current.parent) parts.Add(current.name);
			parts.Reverse(); return string.Join("/", parts.ToArray());
		}

		private static string NormalizeVfxPlayerBone(BoneTypePlayer i_bone)
		{
			switch (i_bone)
			{
				case BoneTypePlayer.rArmUpper: return "arm-right-upper"; case BoneTypePlayer.rArmLower: return "arm-right-lower"; case BoneTypePlayer.rHand: return "hand-right";
				case BoneTypePlayer.lArmUpper: return "arm-left-upper"; case BoneTypePlayer.lArmLower: return "arm-left-lower"; case BoneTypePlayer.lHand: return "hand-left";
				case BoneTypePlayer.rLegUpper: return "leg-right-upper"; case BoneTypePlayer.rLegLower: return "leg-right-lower"; case BoneTypePlayer.rFoot: return "foot-right";
				case BoneTypePlayer.lLegUpper: return "leg-left-upper"; case BoneTypePlayer.lLegLower: return "leg-left-lower"; case BoneTypePlayer.lFoot: return "foot-left";
				default: return i_bone.ToString().ToLowerInvariant();
			}
		}

		private void AddCurrentEnemyVfxLibraryEntry(NormalizedEffectDefinition i_effect, string i_source, string i_path)
		{
			string serialized = Newtonsoft.Json.JsonConvert.SerializeObject(i_effect);
			string prefab = i_effect.Source?["unityPrefab"]?.Value<string>();
			string hierarchy = i_effect.Source?["hierarchyPath"]?.Value<string>();
			VfxLibraryEntry sameSource = m_currentEnemyVfxLibrary.FirstOrDefault(i_entry => !string.IsNullOrEmpty(prefab)
				&& string.Equals(i_entry.Effect.Source?["unityPrefab"]?.Value<string>(), prefab, StringComparison.OrdinalIgnoreCase)
				&& string.Equals(i_entry.Effect.Source?["hierarchyPath"]?.Value<string>(), hierarchy, StringComparison.Ordinal));
			if (sameSource != null)
			{
				if (i_source.StartsWith("Core prefab:", StringComparison.Ordinal))
				{
					i_effect.Trigger = sameSource.Effect.Trigger; i_effect.Index = sameSource.Effect.Index;
					serialized = Newtonsoft.Json.JsonConvert.SerializeObject(i_effect);
					sameSource.Effect = i_effect; sameSource.Source = i_source; sameSource.Path = i_path; sameSource.Serialized = serialized;
				}
				return;
			}
			if (m_currentEnemyVfxLibrary.Any(i_entry => i_entry.Serialized == serialized)) return;
			m_currentEnemyVfxLibrary.Add(new VfxLibraryEntry { Effect = i_effect, Source = i_source, Path = i_path, Serialized = serialized });
		}

		private void AddCurrentEnemyAudioLibraryEntry(EnemyAnimationEventDefinition i_event, string i_source, string i_path)
		{
			if (m_currentEnemyAudioLibrary.Any(i_entry => string.Equals(i_entry.File, i_event.File, StringComparison.OrdinalIgnoreCase))) return;
			m_currentEnemyAudioLibrary.Add(new VfxAudioLibraryEntry { File = i_event.File, Volume = i_event.Volume ?? 1f, Source = i_source, Path = i_path });
		}

		private void ImportVfxAudioEntry(VfxAudioLibraryEntry i_entry)
		{
			try
			{
				string file = CopyVfxAudioIntoMod(i_entry.File, i_entry.Path);
				RecordAuthoringChange("Add current enemy audio", () =>
				{
					EnemyAnimationEventDefinition audio = new EnemyAnimationEventDefinition { Time = SnapVfxTime(m_time), Type = "sound", File = file, Volume = Mathf.Clamp01(i_entry.Volume) };
					if (m_normalizedEnemyClip.Events == null) m_normalizedEnemyClip.Events = new List<EnemyAnimationEventDefinition>();
					m_normalizedEnemyClip.Events.Add(audio); m_normalizedEnemyClip.Events = m_normalizedEnemyClip.Events.OrderBy(i_event => i_event.Time).ToList();
					m_authoringEventIndex = m_normalizedEnemyClip.Events.IndexOf(audio); m_vfxLaneIndex = 1;
				});
				AssetDatabase.Refresh();
			}
			catch (Exception exception) { EditorUtility.DisplayDialog("Import animation audio", exception.Message, "OK"); }
		}

		private void ImportVfxLibraryEntry(VfxLibraryEntry i_entry)
		{
			if (i_entry?.Effect == null) return;
			try
			{
				NormalizedEffectDefinition copy = Newtonsoft.Json.JsonConvert.DeserializeObject<NormalizedEffectDefinition>(i_entry.Serialized);
				MakeImportedVfxPortable(copy, i_entry.Effect, i_entry.Path);
				RecordAuthoringChange("Add current enemy VFX", () =>
				{
					copy.Id = UniqueVfxId(copy.Id, null); copy.RuntimeTexture = null;
					m_normalizedEnemyClip.Effects.Add(copy); m_vfxEffectIndex = m_normalizedEnemyClip.Effects.Count - 1;
				});
				AssetDatabase.Refresh();
			}
			catch (Exception exception) { EditorUtility.DisplayDialog("Import VFX", exception.Message, "OK"); }
		}

		private void MakeImportedVfxPortable(NormalizedEffectDefinition io_effect, NormalizedEffectDefinition i_sourceEffect, string i_sourceDocument)
		{
			if (io_effect == null) return;
			io_effect.TextureSheetTilesX = Mathf.Max(1, io_effect.TextureSheetTilesX);
			io_effect.TextureSheetTilesY = Mathf.Max(1, io_effect.TextureSheetTilesY);
			io_effect.TextureSheetFrame = Mathf.Clamp(io_effect.TextureSheetFrame, 0, io_effect.TextureSheetTilesX * io_effect.TextureSheetTilesY - 1);
			if (!string.IsNullOrWhiteSpace(io_effect.Texture)) io_effect.Texture = CopyVfxTextureIntoMod(io_effect.Texture, i_sourceEffect?.RuntimeTexture, i_sourceDocument, io_effect.Id);
			io_effect.TextureSheetSprites = new List<string>();
			io_effect.Material = null;
			io_effect.Mesh = null;
		}

		private string CopyVfxTextureIntoMod(string i_reference, Texture2D i_runtimeTexture, string i_sourceDocument, string i_effectId)
		{
			string targetRoot = IOPath.GetFullPath(!string.IsNullOrWhiteSpace(m_modPackRoot) ? m_modPackRoot : m_savePackRoot);
			string existing = ResolvePortableSourcePath(i_reference, i_sourceDocument, targetRoot);
			if (existing == null && i_runtimeTexture == null) throw new FileNotFoundException("The source effect texture could not be found: " + i_reference);
			if (existing != null && existing.StartsWith(targetRoot + IOPath.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
				&& string.Equals(IOPath.GetExtension(existing), ".png", StringComparison.OrdinalIgnoreCase))
				return PortableRelativePath(targetRoot, existing);
			string directory = IOPath.Combine(targetRoot, "assets", "vfx"); Directory.CreateDirectory(directory);
			string stem = SlugForFile(i_effectId) + "-" + SlugForFile(IOPath.GetFileNameWithoutExtension(existing ?? i_reference));
			string destination = UniquePortableDestination(directory, stem, ".png");
			if (existing != null && string.Equals(IOPath.GetExtension(existing), ".png", StringComparison.OrdinalIgnoreCase)) File.Copy(existing, destination, false);
			else
			{
				string assetPath = i_reference.Replace('\\', '/');
				Texture texture = i_runtimeTexture != null ? i_runtimeTexture : assetPath.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase) ? AssetDatabase.LoadAssetAtPath<Texture>(assetPath) : null;
				if (texture == null) throw new InvalidDataException("Only Unity texture assets and PNG files can be made portable: " + i_reference);
				File.WriteAllBytes(destination, EncodeVfxTexture(texture));
			}
			return PortableRelativePath(targetRoot, destination);
		}

		private string CopyVfxAudioIntoMod(string i_reference, string i_sourceDocument)
		{
			string targetRoot = IOPath.GetFullPath(!string.IsNullOrWhiteSpace(m_modPackRoot) ? m_modPackRoot : m_savePackRoot);
			string existing = ResolvePortableSourcePath(i_reference, i_sourceDocument, targetRoot);
			if (existing == null) throw new FileNotFoundException("The source animation audio could not be found: " + i_reference);
			string extension = IOPath.GetExtension(existing).ToLowerInvariant();
			if (extension != ".wav" && extension != ".ogg") throw new InvalidDataException("Portable animation audio must be WAV or OGG: " + i_reference);
			if (existing.StartsWith(targetRoot + IOPath.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) return PortableRelativePath(targetRoot, existing);
			string directory = IOPath.Combine(targetRoot, "assets", "audio"); Directory.CreateDirectory(directory);
			string destination = UniquePortableDestination(directory, SlugForFile(IOPath.GetFileNameWithoutExtension(existing)), extension);
			File.Copy(existing, destination, false);
			return PortableRelativePath(targetRoot, destination);
		}

		private static string ResolvePortableSourcePath(string i_reference, string i_sourceDocument, string i_targetRoot)
		{
			if (string.IsNullOrWhiteSpace(i_reference)) return null;
			if (IOPath.IsPathRooted(i_reference)) return File.Exists(i_reference) ? IOPath.GetFullPath(i_reference) : null;
			if (i_reference.Replace('\\', '/').StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
			{
				string projectAsset = IOPath.GetFullPath(IOPath.Combine(ProjectRoot(), i_reference.Replace('/', IOPath.DirectorySeparatorChar)));
				return File.Exists(projectAsset) ? projectAsset : null;
			}
			string sourceRoot = FindVfxPackRoot(i_sourceDocument) ?? i_targetRoot;
			string candidate = IOPath.GetFullPath(IOPath.Combine(sourceRoot, i_reference.Replace('/', IOPath.DirectorySeparatorChar)));
			return candidate.StartsWith(sourceRoot + IOPath.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) && File.Exists(candidate) ? candidate : null;
		}

		private static string FindVfxPackRoot(string i_path)
		{
			if (string.IsNullOrWhiteSpace(i_path)) return null;
			string directory = Directory.Exists(i_path) ? IOPath.GetFullPath(i_path) : IOPath.GetDirectoryName(IOPath.GetFullPath(i_path));
			while (!string.IsNullOrEmpty(directory))
			{
				if (File.Exists(IOPath.Combine(directory, "manifest.json"))) return directory;
				string parent = IOPath.GetDirectoryName(directory); if (parent == directory) break; directory = parent;
			}
			return null;
		}

		private static string UniquePortableDestination(string i_directory, string i_stem, string i_extension)
		{
			string candidate = IOPath.Combine(i_directory, i_stem + i_extension); int suffix = 2;
			while (File.Exists(candidate)) candidate = IOPath.Combine(i_directory, i_stem + "-" + suffix++ + i_extension);
			return candidate;
		}

		private static string PortableRelativePath(string i_root, string i_path)
		{
			Uri root = new Uri(i_root.TrimEnd(IOPath.DirectorySeparatorChar, IOPath.AltDirectorySeparatorChar) + IOPath.DirectorySeparatorChar);
			return Uri.UnescapeDataString(root.MakeRelativeUri(new Uri(i_path)).ToString()).Replace('\\', '/');
		}

		private static byte[] EncodeVfxTexture(Texture i_texture)
		{
			RenderTexture previous = RenderTexture.active;
			RenderTexture render = RenderTexture.GetTemporary(i_texture.width, i_texture.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
			Texture2D readable = new Texture2D(i_texture.width, i_texture.height, TextureFormat.RGBA32, false);
			try
			{
				Graphics.Blit(i_texture, render); RenderTexture.active = render;
				readable.ReadPixels(new Rect(0f, 0f, render.width, render.height), 0, 0); readable.Apply(false, false);
				return readable.EncodeToPNG();
			}
			finally { RenderTexture.active = previous; RenderTexture.ReleaseTemporary(render); DestroyImmediate(readable); }
		}

		private void DrawVfxViewport(float i_width, float i_height)
		{
			EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(i_width), GUILayout.Height(i_height));
			EditorGUILayout.LabelField("PREVIEW", EditorStyles.miniBoldLabel);
			Rect preview = GUILayoutUtility.GetRect(Mathf.Max(80f, i_width - 10f), Mathf.Max(80f, i_width - 10f),
				Mathf.Max(80f, i_height - 29f), Mathf.Max(80f, i_height - 29f), GUILayout.ExpandWidth(false));
			DrawPreview(preview);
			GUI.Label(new Rect(preview.x + 7f, preview.y + 5f, preview.width - 14f, 18f),
				"Scrub below • Add a trigger at the playhead • VFX follows its target bone", EditorStyles.miniLabel);
			EditorGUILayout.EndVertical();
		}

		private void DrawVfxInspector(float i_width, float i_height)
		{
			EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(i_width), GUILayout.Height(i_height));
			EditorGUILayout.LabelField("EFFECT PROPERTIES", EditorStyles.miniBoldLabel);
			m_vfxInspectorScroll = EditorGUILayout.BeginScrollView(m_vfxInspectorScroll, GUILayout.ExpandHeight(true));
			if (DrawVfxLaneEventInspector())
			{
				EditorGUILayout.EndScrollView(); EditorGUILayout.EndVertical(); return;
			}
			List<NormalizedEffectDefinition> effects = m_normalizedEnemyClip.Effects;
			if (effects.Count == 0)
			{
				EditorGUILayout.LabelField("No effect selected", EditorStyles.boldLabel);
				EditorGUILayout.HelpBox("Create an effect in the Library. Its editable particle, light, trail, decal, texture, placement, and rendering properties will appear here.", MessageType.Info);
				if (GUILayout.Button("Create impact burst")) AddVfxPreset("Impact burst");
				if (GUILayout.Button("Create glow light")) AddVfxPreset("Glow light");
				if (GUILayout.Button("Create motion trail")) AddVfxPreset("Motion trail");
			}
			else
			{
				m_vfxEffectIndex = Mathf.Clamp(m_vfxEffectIndex, 0, effects.Count - 1);
				NormalizedEffectDefinition effect = effects[m_vfxEffectIndex];
				EnsureVfxTexture(effect);
				EditorGUI.BeginChangeCheck();
				string id = EditorGUILayout.TextField("Effect ID", effect.Id);
				string[] kinds = { "particle", "light", "trail", "decal" };
				int kindIndex = Mathf.Max(0, Array.IndexOf(kinds, string.IsNullOrWhiteSpace(effect.Kind) ? "particle" : effect.Kind));
				string kind = kinds[EditorGUILayout.Popup("Effect kind", kindIndex, kinds)];
				string[] bones = AvailableBoneNames();
				string currentBone = VfxBoneName(effect.Target);
				int boneIndex = Mathf.Max(0, Array.IndexOf(bones, currentBone));
				string bone = bones.Length == 0 ? EditorGUILayout.TextField("Target bone", currentBone) : bones[EditorGUILayout.Popup("Target bone", boneIndex, bones)];
				float duration = EditorGUILayout.FloatField("System duration", effect.DurationSeconds);
				bool loop = EditorGUILayout.Toggle("Loop", effect.Loop);
				int maxParticles = EditorGUILayout.IntField("Max particles", effect.MaxParticles);
				EditorGUILayout.Space(3f); EditorGUILayout.LabelField("Pack texture", EditorStyles.boldLabel);
				string texture = EditorGUILayout.TextField("PNG path", effect.Texture ?? string.Empty);
				using (new EditorGUILayout.HorizontalScope())
				{
					GUILayout.FlexibleSpace();
					if (GUILayout.Button("Choose PNG...", GUILayout.Width(105f))) texture = ChooseVfxTexture(texture);
				}
				int tilesX = Mathf.Clamp(EditorGUILayout.IntField("Sheet columns", Mathf.Max(1, effect.TextureSheetTilesX)), 1, 64);
				int tilesY = Mathf.Clamp(EditorGUILayout.IntField("Sheet rows", Mathf.Max(1, effect.TextureSheetTilesY)), 1, 64);
				int sheetFrame = EditorGUILayout.IntSlider("Selected region", Mathf.Clamp(effect.TextureSheetFrame, 0, tilesX * tilesY - 1), 0, tilesX * tilesY - 1);
				DrawVfxTextureRegion(effect.RuntimeTexture, tilesX, tilesY, sheetFrame);
				EditorGUILayout.Space(3f); EditorGUILayout.LabelField("Particle", EditorStyles.boldLabel);
				Vector2 lifetime = DrawVfxMinMax("Lifetime", effect.StartLifetimeMin, effect.StartLifetimeMax);
				Vector2 speed = DrawVfxMinMax("Speed", effect.StartSpeedMin, effect.StartSpeedMax);
				Vector2 size = DrawVfxMinMax("Size", effect.StartSizeMin, effect.StartSizeMax);
				Vector2 gravity = DrawVfxMinMax("Gravity", effect.GravityMin, effect.GravityMax);
				Vector2 emission = DrawVfxMinMax("Emission / sec", effect.EmissionRateMin, effect.EmissionRateMax);
				Color colorMin = EditorGUILayout.ColorField("Start color min", VfxColor(effect.StartColorMin));
				Color colorMax = EditorGUILayout.ColorField("Start color max", VfxColor(effect.StartColorMax));
				EditorGUILayout.Space(3f); EditorGUILayout.LabelField("Placement", EditorStyles.boldLabel);
				Vector3 offset = EditorGUILayout.Vector3Field("Local position", new Vector3(effect.PositionX, effect.PositionY, effect.PositionZ));
				Vector3 rotation = EditorGUILayout.Vector3Field("Local rotation", new Vector3(effect.RotationX, effect.RotationY, effect.RotationZ));
				Vector3 scale = EditorGUILayout.Vector3Field("Local scale", new Vector3(effect.ScaleX, effect.ScaleY, effect.ScaleZ));
				string[] spaces = { "Local", "World" }; int spaceIndex = effect.SimulationSpace == "World" ? 1 : 0;
				string simulationSpace = spaces[EditorGUILayout.Popup("Simulation", spaceIndex, spaces)];
				EditorGUILayout.Space(3f); EditorGUILayout.LabelField("Emitter", EditorStyles.boldLabel);
				bool shapeEnabled = EditorGUILayout.Toggle("Use shape", effect.ShapeEnabled);
				string[] shapes = { "Cone", "Circle", "Sphere", "Hemisphere", "Box", "Edge" };
				int shapeIndex = Mathf.Max(0, Array.IndexOf(shapes, effect.Shape));
				string shape = shapes[EditorGUILayout.Popup("Shape", shapeIndex, shapes)];
				float radius = EditorGUILayout.FloatField("Radius", effect.ShapeRadius);
				float angle = EditorGUILayout.Slider("Cone angle", effect.ShapeAngle, 0f, 90f);
				EditorGUILayout.Space(3f); EditorGUILayout.LabelField("Preserved particle modules", EditorStyles.boldLabel);
				EditorGUILayout.LabelField("Bursts", (effect.Bursts?.Count ?? 0).ToString());
				EditorGUILayout.LabelField("Color over lifetime", effect.ColorOverLifetime == null ? "Off" : effect.ColorOverLifetime.Mode);
				EditorGUILayout.LabelField("Size over lifetime", effect.SizeOverLifetime?.Enabled == true ? "On" : "Off");
				EditorGUILayout.LabelField("Velocity over lifetime", effect.VelocityOverLifetime?.Enabled == true ? "On" : "Off");
				EditorGUILayout.LabelField("Rotation over lifetime", effect.RotationOverLifetime?.Enabled == true ? "On" : "Off");
				EditorGUILayout.LabelField("Noise", effect.Noise?.Enabled == true ? "On" : "Off");
				if (effect.ColorOverLifetime != null || effect.SizeOverLifetime?.Enabled == true || effect.VelocityOverLifetime?.Enabled == true
					|| effect.RotationOverLifetime?.Enabled == true || effect.Noise?.Enabled == true || (effect.Bursts?.Count ?? 0) > 0)
					EditorGUILayout.HelpBox("Imported curve and gradient keys are preserved in JSON and reconstructed identically by Preview and runtime.", MessageType.None);
				float lightIntensity = effect.LightIntensity, lightRadius = effect.LightRadius, trailWidth = effect.TrailWidth, trailTime = effect.TrailTime, decalPpu = effect.DecalPixelsPerUnit;
				if (kind == "light")
				{
					EditorGUILayout.Space(3f); EditorGUILayout.LabelField("Light", EditorStyles.boldLabel);
					lightIntensity = EditorGUILayout.Slider("Intensity", lightIntensity, 0f, 20f); lightRadius = EditorGUILayout.FloatField("Radius", lightRadius);
				}
				else if (kind == "trail")
				{
					EditorGUILayout.Space(3f); EditorGUILayout.LabelField("Trail", EditorStyles.boldLabel);
					trailWidth = EditorGUILayout.FloatField("Width", trailWidth); trailTime = EditorGUILayout.FloatField("Persistence", trailTime);
				}
				else if (kind == "decal")
				{
					EditorGUILayout.Space(3f); EditorGUILayout.LabelField("Decal", EditorStyles.boldLabel);
					decalPpu = EditorGUILayout.FloatField("Pixels per unit", decalPpu);
				}
				int sortingOrder = EditorGUILayout.IntField("Sorting order", effect.SortingOrder);
				if (EditorGUI.EndChangeCheck()) RecordAuthoringChange("Edit animation VFX", () =>
				{
					effect.Id = UniqueVfxId(id, effect); effect.Kind = kind; effect.Target = "enemy-bone/" + bone;
					effect.DurationSeconds = Mathf.Clamp(duration, 0.01f, 600f); effect.Loop = loop; effect.MaxParticles = Mathf.Clamp(maxParticles, 1, 100000);
					if (!string.Equals(effect.Texture, texture, StringComparison.Ordinal)) effect.RuntimeTexture = null;
					effect.Texture = string.IsNullOrWhiteSpace(texture) ? null : texture.Replace('\\', '/'); effect.TextureSheetTilesX = tilesX; effect.TextureSheetTilesY = tilesY; effect.TextureSheetFrame = sheetFrame;
					effect.StartLifetimeMin = Mathf.Max(0.01f, Mathf.Min(lifetime.x, lifetime.y)); effect.StartLifetimeMax = Mathf.Max(effect.StartLifetimeMin, Mathf.Max(lifetime.x, lifetime.y));
					effect.StartSpeedMin = Mathf.Min(speed.x, speed.y); effect.StartSpeedMax = Mathf.Max(speed.x, speed.y);
					effect.StartSizeMin = Mathf.Max(0f, Mathf.Min(size.x, size.y)); effect.StartSizeMax = Mathf.Max(effect.StartSizeMin, Mathf.Max(size.x, size.y));
					effect.GravityMin = Mathf.Min(gravity.x, gravity.y); effect.GravityMax = Mathf.Max(gravity.x, gravity.y);
					effect.EmissionRateMin = Mathf.Max(0f, Mathf.Min(emission.x, emission.y)); effect.EmissionRateMax = Mathf.Max(effect.EmissionRateMin, Mathf.Max(emission.x, emission.y));
					effect.StartColorMin = VfxColor(colorMin); effect.StartColorMax = VfxColor(colorMax);
					effect.PositionX = offset.x; effect.PositionY = offset.y; effect.PositionZ = offset.z;
					effect.RotationX = rotation.x; effect.RotationY = rotation.y; effect.RotationZ = rotation.z;
					effect.ScaleX = scale.x; effect.ScaleY = scale.y; effect.ScaleZ = scale.z;
					effect.SimulationSpace = simulationSpace; effect.ShapeEnabled = shapeEnabled; effect.Shape = shape;
					effect.ShapeRadius = Mathf.Max(0f, radius); effect.ShapeAngle = angle; effect.SortingOrder = sortingOrder;
					effect.LightIntensity = Mathf.Clamp(lightIntensity, 0f, 20f); effect.LightRadius = Mathf.Clamp(lightRadius, .01f, 100f);
					effect.TrailWidth = Mathf.Clamp(trailWidth, .001f, 20f); effect.TrailTime = Mathf.Clamp(trailTime, .01f, 30f); effect.DecalPixelsPerUnit = Mathf.Clamp(decalPpu, 1f, 1024f);
				});
			}
			EditorGUILayout.EndScrollView();
			EditorGUILayout.EndVertical();
		}

		private bool DrawVfxLaneEventInspector()
		{
			if (m_vfxLaneIndex < 1 || m_vfxLaneIndex > 3) return false;
			string type = m_vfxLaneIndex == 1 ? "sound" : m_vfxLaneIndex == 2 ? "attackHit" : "cameraShake";
			List<EnemyAnimationEventDefinition> events = m_normalizedEnemyClip.Events ?? (m_normalizedEnemyClip.Events = new List<EnemyAnimationEventDefinition>());
			EnemyAnimationEventDefinition item = events.Count == 0 ? null : events[Mathf.Clamp(m_authoringEventIndex, 0, events.Count - 1)];
			if (item == null || item.Type != type) { EditorGUILayout.HelpBox("Select a " + VfxLaneNames[m_vfxLaneIndex].ToLowerInvariant() + " marker below, or add one at the playhead.", MessageType.Info); return true; }
			EditorGUILayout.LabelField(VfxLaneNames[m_vfxLaneIndex] + " marker", EditorStyles.boldLabel); EditorGUI.BeginChangeCheck();
			float time = EditorGUILayout.FloatField("Time", item.Time); string file = item.File ?? string.Empty; float volume = item.Volume ?? 1f, amount = item.Amount ?? .15f;
			if (type == "sound")
			{
				file = EditorGUILayout.TextField("WAV / OGG path", file); volume = EditorGUILayout.Slider("Volume", volume, 0f, 1f);
				if (GUILayout.Button("Choose pack audio...")) file = ChooseVfxAudio(file);
			}
			else if (type == "cameraShake") amount = EditorGUILayout.Slider("Shake strength", amount, 0f, 1f);
			else EditorGUILayout.HelpBox("This marker performs the configured attack delivery at this exact frame.", MessageType.None);
			if (EditorGUI.EndChangeCheck()) RecordAuthoringChange("Edit animation lane marker", () => { item.Time = SnapVfxTime(time); item.File = type == "sound" ? file.Replace('\\', '/') : null; item.Volume = type == "sound" ? (float?)volume : null; item.Amount = type == "cameraShake" ? (float?)amount : null; m_normalizedEnemyClip.Events = events.OrderBy(i_event => i_event.Time).ToList(); m_authoringEventIndex = m_normalizedEnemyClip.Events.IndexOf(item); });
			return true;
		}

		private static Vector2 DrawVfxMinMax(string i_label, float i_min, float i_max)
		{
			using (new EditorGUILayout.HorizontalScope())
			{
				EditorGUILayout.PrefixLabel(i_label);
				return new Vector2(EditorGUILayout.FloatField(i_min), EditorGUILayout.FloatField(i_max));
			}
		}

		private void EnsureVfxTexture(NormalizedEffectDefinition i_effect)
		{
			if (i_effect == null || i_effect.RuntimeTexture != null || string.IsNullOrWhiteSpace(i_effect.Texture) || m_vfxTextureFailures.Contains(i_effect.Texture)) return;
			string root = !string.IsNullOrWhiteSpace(m_modPackRoot) ? m_modPackRoot : m_savePackRoot;
			ValidationReport report = new ValidationReport();
			if (RuntimePngAssetLoader.TryLoad(root, i_effect.Texture, "VFX preview " + i_effect.Id, FilterMode.Point, report,
				"vfx-preview.texture", "vfx-preview.decode", m_normalizedEnemyPath, out Texture2D texture))
			{
				i_effect.RuntimeTexture = texture; m_modPreviewAssets.Add(texture);
			}
			else m_vfxTextureFailures.Add(i_effect.Texture);
		}

		private string ChooseVfxTexture(string i_current)
		{
			string root = System.IO.Path.GetFullPath(!string.IsNullOrWhiteSpace(m_modPackRoot) ? m_modPackRoot : m_savePackRoot);
			string selected = EditorUtility.OpenFilePanel("Choose pack-local VFX PNG", root, "png");
			if (string.IsNullOrEmpty(selected)) return i_current;
			selected = System.IO.Path.GetFullPath(selected);
			if (!selected.StartsWith(root + System.IO.Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
			{
				EditorUtility.DisplayDialog("Texture outside mod", "Choose a PNG inside the loose mod folder.", "OK"); return i_current;
			}
			Uri rootUri = new Uri(root.TrimEnd(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar) + System.IO.Path.DirectorySeparatorChar);
			string relative = Uri.UnescapeDataString(rootUri.MakeRelativeUri(new Uri(selected)).ToString()).Replace('\\', '/');
			m_vfxTextureFailures.Remove(relative); return relative;
		}

		private string ChooseVfxAudio(string i_current)
		{
			string root = System.IO.Path.GetFullPath(!string.IsNullOrWhiteSpace(m_modPackRoot) ? m_modPackRoot : m_savePackRoot);
			string selected = EditorUtility.OpenFilePanel("Choose pack-local animation audio", root, string.Empty);
			if (string.IsNullOrEmpty(selected)) return i_current; selected = System.IO.Path.GetFullPath(selected);
			string extension = System.IO.Path.GetExtension(selected).ToLowerInvariant();
			if ((extension != ".wav" && extension != ".ogg") || !selected.StartsWith(root + System.IO.Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
			{
				EditorUtility.DisplayDialog("Invalid animation audio", "Choose a WAV or OGG inside the loose mod folder.", "OK"); return i_current;
			}
			Uri rootUri = new Uri(root.TrimEnd(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar) + System.IO.Path.DirectorySeparatorChar);
			return Uri.UnescapeDataString(rootUri.MakeRelativeUri(new Uri(selected)).ToString()).Replace('\\', '/');
		}

		private static void DrawVfxTextureRegion(Texture2D i_texture, int i_tilesX, int i_tilesY, int i_frame)
		{
			if (i_texture == null) { EditorGUILayout.HelpBox("Choose a pack-local PNG to preview its selected sheet region.", MessageType.None); return; }
			Rect box = GUILayoutUtility.GetRect(48f, 96f, 72f, 72f, GUILayout.ExpandWidth(false));
			int frame = Mathf.Clamp(i_frame, 0, i_tilesX * i_tilesY - 1), column = frame % i_tilesX, rowFromTop = frame / i_tilesX;
			Rect uv = new Rect(column / (float)i_tilesX, (i_tilesY - 1 - rowFromTop) / (float)i_tilesY, 1f / i_tilesX, 1f / i_tilesY);
			EditorGUI.DrawRect(box, new Color(.05f, .05f, .05f, 1f)); GUI.DrawTextureWithTexCoords(box, i_texture, uv, true);
		}

		private void CopyVfxEffect()
		{
			NormalizedEffectDefinition effect = SelectedVfxEffect(); if (effect == null) return;
			EditorGUIUtility.systemCopyBuffer = "CAPTIVITY_VFX\n" + Newtonsoft.Json.JsonConvert.SerializeObject(effect, Newtonsoft.Json.Formatting.Indented);
		}

		private void PasteVfxEffect()
		{
			const string marker = "CAPTIVITY_VFX\n"; string clipboard = EditorGUIUtility.systemCopyBuffer ?? string.Empty;
			if (!clipboard.StartsWith(marker, StringComparison.Ordinal)) { EditorUtility.DisplayDialog("Paste effect", "The clipboard does not contain a Captivity VFX preset.", "OK"); return; }
			try
			{
				NormalizedEffectDefinition pasted = Newtonsoft.Json.JsonConvert.DeserializeObject<NormalizedEffectDefinition>(clipboard.Substring(marker.Length));
				if (pasted == null) throw new InvalidDataException("The preset is empty.");
				RecordAuthoringChange("Paste animation VFX", () => { pasted.Id = UniqueVfxId(pasted.Id + "-copy", null); pasted.RuntimeTexture = null; m_normalizedEnemyClip.Effects.Add(pasted); m_vfxEffectIndex = m_normalizedEnemyClip.Effects.Count - 1; });
			}
			catch (Exception exception) { EditorUtility.DisplayDialog("Paste effect", exception.Message, "OK"); }
		}

		private void ShowVfxPresetMenu()
		{
			GenericMenu menu = new GenericMenu();
			foreach (string preset in new[] { "Impact burst", "Smoke puff", "Electric sparks", "Glow light", "Motion trail", "Ground decal" })
			{
				string captured = preset; menu.AddItem(new GUIContent(captured), false, () => AddVfxPreset(captured));
			}
			menu.ShowAsContext();
		}

		private void AddVfxPreset(string i_name)
		{
			string bone = AvailableBoneNames().FirstOrDefault() ?? "root";
			RecordAuthoringChange("Add VFX starter preset", () =>
			{
				NormalizedEffectDefinition effect = NewVfxEffect(UniqueVfxId(i_name.ToLowerInvariant().Replace(' ', '-'), null), bone);
				if (i_name == "Smoke puff") { effect.StartColorMin = VfxColor(new Color(.35f, .35f, .35f, .75f)); effect.StartColorMax = VfxColor(new Color(.12f, .12f, .12f, 0f)); effect.StartLifetimeMax = .8f; effect.StartSpeedMax = .35f; effect.StartSizeMax = .55f; }
				else if (i_name == "Electric sparks") { effect.StartColorMin = VfxColor(new Color(.2f, .75f, 1f, 1f)); effect.StartColorMax = VfxColor(Color.white); effect.StartLifetimeMax = .18f; effect.StartSpeedMin = 2f; effect.StartSpeedMax = 4f; effect.GravityMin = -1f; effect.GravityMax = 1f; }
				else if (i_name == "Glow light") { effect.Kind = "light"; effect.DurationSeconds = .18f; effect.LightIntensity = 1.6f; effect.LightRadius = 1.2f; effect.StartColorMin = VfxColor(new Color(1f, .65f, .2f, 1f)); }
				else if (i_name == "Motion trail") { effect.Kind = "trail"; effect.DurationSeconds = .4f; effect.TrailWidth = .15f; effect.TrailTime = .22f; effect.StartColorMax = VfxColor(new Color(1f, 1f, 1f, 0f)); }
				else if (i_name == "Ground decal") { effect.Kind = "decal"; effect.DurationSeconds = 2f; effect.SortingOrder = -5; }
				m_normalizedEnemyClip.Effects.Add(effect); m_vfxEffectIndex = m_normalizedEnemyClip.Effects.Count - 1;
			});
		}

		private void DrawVfxTimeline()
		{
			PruneVfxMarkerSelection();
			using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
			{
				m_vfxLaneIndex = EditorGUILayout.Popup(m_vfxLaneIndex, VfxLaneNames, EditorStyles.toolbarPopup, GUILayout.Width(82f));
				if (GUILayout.Button("Add lane marker", EditorStyles.toolbarButton, GUILayout.Width(105f))) AddVfxLaneMarker();
				if (GUILayout.Button("Move lane marker", EditorStyles.toolbarButton, GUILayout.Width(112f))) MoveVfxLaneMarker();
				if (GUILayout.Button("Delete lane marker", EditorStyles.toolbarButton, GUILayout.Width(115f))) DeleteVfxLaneMarker();
				using (new EditorGUI.DisabledScope(SelectedVfxMarkerCount() == 0))
					if (GUILayout.Button("Duplicate selected", EditorStyles.toolbarButton, GUILayout.Width(112f))) DuplicateSelectedVfxMarkers();
				GUILayout.FlexibleSpace(); GUILayout.Label(SelectedVfxMarkerCount() + " selected  |  Ctrl/Shift-click, Ctrl+D, Delete", EditorStyles.miniLabel);
			}
			DrawDedicatedVfxLanes();
			List<NormalizedEffectTrigger> triggers = m_normalizedEnemyClip.EffectTriggers;
			float duration = Mathf.Max(0.001f, m_normalizedEnemyClip.DurationSeconds);
			using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
			{
				using (new EditorGUI.DisabledScope(m_normalizedEnemyClip.Effects.Count == 0))
					if (GUILayout.Button("Add trigger at playhead", EditorStyles.toolbarButton, GUILayout.Width(145f))) AddVfxTrigger();
				using (new EditorGUI.DisabledScope(triggers.Count == 0))
				{
					if (GUILayout.Button("Move selected here", EditorStyles.toolbarButton, GUILayout.Width(115f))) MoveVfxTriggerToPlayhead();
					if (GUILayout.Button("Delete trigger", EditorStyles.toolbarButton, GUILayout.Width(88f))) DeleteVfxTrigger();
				}
				GUILayout.FlexibleSpace();
				GUILayout.Label(triggers.Count + " trigger" + (triggers.Count == 1 ? string.Empty : "s"), EditorStyles.miniLabel);
			}
			Rect rect = GUILayoutUtility.GetRect(100f, 10000f, 82f, 82f, GUILayout.ExpandWidth(true));
			EditorGUI.DrawRect(rect, new Color(0.08f, 0.08f, 0.08f, 1f));
			Rect ruler = new Rect(rect.x + 9f, rect.y + 22f, rect.width - 18f, 42f);
			EditorGUI.DrawRect(new Rect(ruler.x, ruler.center.y - 1f, ruler.width, 2f), new Color(0.35f, 0.35f, 0.35f, 1f));
			for (int tick = 0; tick <= 10; tick++)
			{
				float x = ruler.x + ruler.width * tick / 10f;
				EditorGUI.DrawRect(new Rect(x, ruler.y, 1f, ruler.height), new Color(1f, 1f, 1f, tick % 5 == 0 ? 0.18f : 0.07f));
				GUI.Label(new Rect(x - 18f, rect.y + 2f, 36f, 16f), (duration * tick / 10f).ToString("0.##"), EditorStyles.centeredGreyMiniLabel);
			}
			for (int index = 0; index < triggers.Count; index++)
			{
				NormalizedEffectTrigger trigger = triggers[index];
				float x = ruler.x + Mathf.Clamp01(trigger.Time / duration) * ruler.width;
				Rect marker = new Rect(x - 7f, ruler.center.y - 10f, 14f, 20f);
				Color old = GUI.color; GUI.color = m_vfxSelectedTriggers.Contains(trigger) ? new Color(1f, 0.65f, 0.12f) : new Color(0.2f, 0.95f, 1f);
				if (GUI.Button(marker, new GUIContent("◆", string.Join(", ", trigger.Effects.ToArray()) + " at " + trigger.Time.ToString("0.###") + "s"), EditorStyles.miniLabel))
				{
					SelectVfxTriggerMarker(trigger, index, LaneForTrigger(trigger), Event.current);
				}
				GUI.color = old;
			}
			float playhead = ruler.x + Mathf.Clamp01(m_time / duration) * ruler.width;
			EditorGUI.DrawRect(new Rect(playhead, ruler.y - 5f, 2f, ruler.height + 10f), Color.white);
			if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && ruler.Contains(Event.current.mousePosition))
			{
				if (!IsAdditiveVfxSelection(Event.current)) ClearVfxMarkerSelection();
				m_time = SnapVfxTime(Mathf.Clamp01((Event.current.mousePosition.x - ruler.x) / ruler.width) * duration);
				m_playing = false; SampleCurrentClip(); Repaint(); Event.current.Use();
			}
			HandleVfxTimelineShortcuts();
		}

		private void DrawDedicatedVfxLanes()
		{
			float duration = Mathf.Max(.001f, m_normalizedEnemyClip.DurationSeconds); const float labelWidth = 62f, rowHeight = 18f;
			Rect rect = GUILayoutUtility.GetRect(100f, 10000f, rowHeight * VfxLaneNames.Length, rowHeight * VfxLaneNames.Length, GUILayout.ExpandWidth(true));
			EditorGUI.DrawRect(rect, new Color(.075f, .075f, .075f, 1f)); Rect track = new Rect(rect.x + labelWidth, rect.y, rect.width - labelWidth - 6f, rect.height);
			for (int lane = 0; lane < VfxLaneNames.Length; lane++)
			{
				Rect row = new Rect(rect.x, rect.y + lane * rowHeight, rect.width, rowHeight); if (lane == m_vfxLaneIndex) EditorGUI.DrawRect(row, new Color(.2f, .17f, .09f, 1f));
				EditorGUI.DrawRect(new Rect(row.x, row.yMax - 1f, row.width, 1f), new Color(1f, 1f, 1f, .06f));
				if (GUI.Button(new Rect(row.x + 2f, row.y, labelWidth - 4f, rowHeight), VfxLaneNames[lane], EditorStyles.miniLabel)) m_vfxLaneIndex = lane;
			}
			for (int tick = 0; tick <= 10; tick++) { float x = track.x + track.width * tick / 10f; EditorGUI.DrawRect(new Rect(x, track.y, 1f, track.height), new Color(1f, 1f, 1f, tick % 5 == 0 ? .15f : .05f)); }
			List<EnemyAnimationEventDefinition> events = m_normalizedEnemyClip.Events ?? new List<EnemyAnimationEventDefinition>();
			for (int index = 0; index < events.Count; index++)
			{
				int lane = LaneForEvent(events[index]); if (lane < 0) continue; float x = track.x + Mathf.Clamp01(events[index].Time / duration) * track.width;
				Color old = GUI.color; GUI.color = m_vfxSelectedEvents.Contains(events[index]) ? new Color(1f, .65f, .12f) : new Color(.95f, .4f, .25f);
				if (GUI.Button(new Rect(x - 6f, track.y + lane * rowHeight + 2f, 12f, rowHeight - 4f), new GUIContent("●", events[index].Type), EditorStyles.miniLabel)) SelectVfxEventMarker(events[index], index, lane, Event.current);
				GUI.color = old;
			}
			for (int index = 0; index < m_normalizedEnemyClip.EffectTriggers.Count; index++)
			{
				NormalizedEffectTrigger trigger = m_normalizedEnemyClip.EffectTriggers[index]; int lane = LaneForTrigger(trigger); if (lane < 0) continue; float x = track.x + Mathf.Clamp01(trigger.Time / duration) * track.width;
				Color old = GUI.color; GUI.color = m_vfxSelectedTriggers.Contains(trigger) ? new Color(1f, .65f, .12f) : new Color(.2f, .9f, 1f);
				if (GUI.Button(new Rect(x - 6f, track.y + lane * rowHeight + 2f, 12f, rowHeight - 4f), new GUIContent("◆", string.Join(", ", trigger.Effects.ToArray())), EditorStyles.miniLabel)) SelectVfxTriggerMarker(trigger, index, lane, Event.current);
				GUI.color = old;
			}
			float playhead = track.x + Mathf.Clamp01(m_time / duration) * track.width; EditorGUI.DrawRect(new Rect(playhead, track.y, 2f, track.height), Color.white);
			if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && track.Contains(Event.current.mousePosition)) { if (!IsAdditiveVfxSelection(Event.current)) ClearVfxMarkerSelection(); m_vfxLaneIndex = Mathf.Clamp(Mathf.FloorToInt((Event.current.mousePosition.y - track.y) / rowHeight), 0, VfxLaneNames.Length - 1); m_time = SnapVfxTime(Mathf.Clamp01((Event.current.mousePosition.x - track.x) / track.width) * duration); m_playing = false; SampleCurrentClip(); Event.current.Use(); }
		}

		private static bool IsAdditiveVfxSelection(Event i_event)
		{
			return i_event != null && (i_event.control || i_event.command || i_event.shift);
		}

		private void SelectVfxEventMarker(EnemyAnimationEventDefinition i_marker, int i_index, int i_lane, Event i_event)
		{
			bool additive = IsAdditiveVfxSelection(i_event);
			if (!additive) ClearVfxMarkerSelection();
			if (additive && m_vfxSelectedEvents.Contains(i_marker)) m_vfxSelectedEvents.Remove(i_marker);
			else m_vfxSelectedEvents.Add(i_marker);
			m_vfxLaneIndex = i_lane; m_authoringEventIndex = i_index; m_time = i_marker.Time; m_playing = false;
			SampleCurrentClip(); Repaint();
		}

		private void SelectVfxTriggerMarker(NormalizedEffectTrigger i_marker, int i_index, int i_lane, Event i_event)
		{
			bool additive = IsAdditiveVfxSelection(i_event);
			if (!additive) ClearVfxMarkerSelection();
			if (additive && m_vfxSelectedTriggers.Contains(i_marker)) m_vfxSelectedTriggers.Remove(i_marker);
			else m_vfxSelectedTriggers.Add(i_marker);
			if (i_lane >= 0) m_vfxLaneIndex = i_lane;
			m_vfxTriggerIndex = i_index; m_time = i_marker.Time; m_playing = false; SelectEffectFromTrigger(i_marker);
			SampleCurrentClip(); Repaint();
		}

		private void ClearVfxMarkerSelection()
		{
			m_vfxSelectedEvents.Clear(); m_vfxSelectedTriggers.Clear();
		}

		private void PruneVfxMarkerSelection()
		{
			List<EnemyAnimationEventDefinition> events = m_normalizedEnemyClip.Events ?? new List<EnemyAnimationEventDefinition>();
			List<NormalizedEffectTrigger> triggers = m_normalizedEnemyClip.EffectTriggers ?? new List<NormalizedEffectTrigger>();
			m_vfxSelectedEvents.RemoveWhere(i_item => !events.Contains(i_item));
			m_vfxSelectedTriggers.RemoveWhere(i_item => !triggers.Contains(i_item));
		}

		private int SelectedVfxMarkerCount()
		{
			PruneVfxMarkerSelection();
			return m_vfxSelectedEvents.Count + m_vfxSelectedTriggers.Count;
		}

		private void HandleVfxTimelineShortcuts()
		{
			Event current = Event.current;
			if (current == null || current.type != EventType.KeyDown || EditorGUIUtility.editingTextField) return;
			bool actionModifier = current.control || current.command;
			if (actionModifier && current.keyCode == KeyCode.D)
			{
				if (SelectedVfxMarkerCount() > 0) DuplicateSelectedVfxMarkers();
				current.Use();
			}
			else if (current.keyCode == KeyCode.Delete || current.keyCode == KeyCode.Backspace)
			{
				if (SelectedVfxMarkerCount() > 0) DeleteSelectedVfxMarkers();
				current.Use();
			}
			else if (current.keyCode == KeyCode.Escape && SelectedVfxMarkerCount() > 0)
			{
				ClearVfxMarkerSelection(); Repaint(); current.Use();
			}
		}

		private void DeleteSelectedVfxMarkers()
		{
			PruneVfxMarkerSelection();
			if (SelectedVfxMarkerCount() == 0) return;
			HashSet<EnemyAnimationEventDefinition> selectedEvents = new HashSet<EnemyAnimationEventDefinition>(m_vfxSelectedEvents);
			HashSet<NormalizedEffectTrigger> selectedTriggers = new HashSet<NormalizedEffectTrigger>(m_vfxSelectedTriggers);
			RecordAuthoringChange("Delete animation markers", () =>
			{
				if (m_normalizedEnemyClip.Events != null) m_normalizedEnemyClip.Events.RemoveAll(i_item => selectedEvents.Contains(i_item));
				m_normalizedEnemyClip.EffectTriggers.RemoveAll(i_item => selectedTriggers.Contains(i_item));
				ClearVfxMarkerSelection();
				m_authoringEventIndex = Mathf.Clamp(m_authoringEventIndex, 0, Mathf.Max(0, (m_normalizedEnemyClip.Events?.Count ?? 0) - 1));
				m_vfxTriggerIndex = Mathf.Clamp(m_vfxTriggerIndex, 0, Mathf.Max(0, m_normalizedEnemyClip.EffectTriggers.Count - 1));
			});
		}

		private void DuplicateSelectedVfxMarkers()
		{
			PruneVfxMarkerSelection();
			if (SelectedVfxMarkerCount() == 0) return;
			List<EnemyAnimationEventDefinition> sourceEvents = m_vfxSelectedEvents.OrderBy(i_item => i_item.Time).ToList();
			List<NormalizedEffectTrigger> sourceTriggers = m_vfxSelectedTriggers.OrderBy(i_item => i_item.Time).ToList();
			RecordAuthoringChange("Duplicate animation markers", () =>
			{
				List<EnemyAnimationEventDefinition> eventCopies = sourceEvents.Select(CloneVfxMarker).ToList();
				List<NormalizedEffectTrigger> triggerCopies = sourceTriggers.Select(CloneVfxMarker).ToList();
				foreach (EnemyAnimationEventDefinition copy in eventCopies) { copy.Time = DuplicateVfxMarkerTime(copy.Time); m_normalizedEnemyClip.Events.Add(copy); }
				foreach (NormalizedEffectTrigger copy in triggerCopies) { copy.Time = DuplicateVfxMarkerTime(copy.Time); m_normalizedEnemyClip.EffectTriggers.Add(copy); }
				m_normalizedEnemyClip.Events = m_normalizedEnemyClip.Events.OrderBy(i_item => i_item.Time).ToList();
				m_normalizedEnemyClip.EffectTriggers = m_normalizedEnemyClip.EffectTriggers.OrderBy(i_item => i_item.Time).ToList();
				ClearVfxMarkerSelection();
				foreach (EnemyAnimationEventDefinition copy in eventCopies) m_vfxSelectedEvents.Add(copy);
				foreach (NormalizedEffectTrigger copy in triggerCopies) m_vfxSelectedTriggers.Add(copy);
				if (eventCopies.Count > 0) { EnemyAnimationEventDefinition first = eventCopies[0]; m_authoringEventIndex = m_normalizedEnemyClip.Events.IndexOf(first); m_vfxLaneIndex = LaneForEvent(first); }
				if (triggerCopies.Count > 0) { NormalizedEffectTrigger first = triggerCopies[0]; m_vfxTriggerIndex = m_normalizedEnemyClip.EffectTriggers.IndexOf(first); m_vfxLaneIndex = LaneForTrigger(first); SelectEffectFromTrigger(first); }
				m_time = eventCopies.Select(i_item => i_item.Time).Concat(triggerCopies.Select(i_item => i_item.Time)).DefaultIfEmpty(m_time).Min();
				m_playing = false;
			});
		}

		private float DuplicateVfxMarkerTime(float i_time)
		{
			float frame = 1f / Mathf.Max(1f, m_normalizedEnemyClip.FrameRate);
			float candidate = i_time + frame;
			if (candidate > m_normalizedEnemyClip.DurationSeconds + .0001f) candidate = Mathf.Max(0f, i_time - frame);
			return SnapVfxTime(candidate);
		}

		private static EnemyAnimationEventDefinition CloneVfxMarker(EnemyAnimationEventDefinition i_marker)
		{
			return Newtonsoft.Json.JsonConvert.DeserializeObject<EnemyAnimationEventDefinition>(Newtonsoft.Json.JsonConvert.SerializeObject(i_marker));
		}

		private static NormalizedEffectTrigger CloneVfxMarker(NormalizedEffectTrigger i_marker)
		{
			return Newtonsoft.Json.JsonConvert.DeserializeObject<NormalizedEffectTrigger>(Newtonsoft.Json.JsonConvert.SerializeObject(i_marker));
		}

		private int LaneForEvent(EnemyAnimationEventDefinition i_event)
		{
			if (i_event == null) return -1; if (i_event.Type == "sound") return 1; if (i_event.Type == "attackHit") return 2; if (i_event.Type == "cameraShake") return 3; return -1;
		}

		private int LaneForTrigger(NormalizedEffectTrigger i_trigger)
		{
			if (i_trigger?.Effects == null) return -1;
			foreach (string id in i_trigger.Effects) { NormalizedEffectDefinition effect = m_normalizedEnemyClip.Effects.FirstOrDefault(i_item => i_item.Id == id); if (effect == null) continue; string kind = string.IsNullOrWhiteSpace(effect.Kind) ? "particle" : effect.Kind; return kind == "light" ? 4 : kind == "trail" ? 5 : kind == "decal" ? 6 : 0; }
			return -1;
		}

		private void AddVfxLaneMarker()
		{
			if (m_vfxLaneIndex >= 1 && m_vfxLaneIndex <= 3)
			{
				RecordAuthoringChange("Add animation lane marker", () => { EnemyAnimationEventDefinition item = new EnemyAnimationEventDefinition { Time = SnapVfxTime(m_time), Type = m_vfxLaneIndex == 1 ? "sound" : m_vfxLaneIndex == 2 ? "attackHit" : "cameraShake", File = m_vfxLaneIndex == 1 ? "assets/audio/effect.wav" : null, Volume = m_vfxLaneIndex == 1 ? (float?)1f : null, Amount = m_vfxLaneIndex == 3 ? (float?).15f : null }; if (m_normalizedEnemyClip.Events == null) m_normalizedEnemyClip.Events = new List<EnemyAnimationEventDefinition>(); m_normalizedEnemyClip.Events.Add(item); m_normalizedEnemyClip.Events = m_normalizedEnemyClip.Events.OrderBy(i_event => i_event.Time).ToList(); m_authoringEventIndex = m_normalizedEnemyClip.Events.IndexOf(item); });
				return;
			}
			string kind = m_vfxLaneIndex == 4 ? "light" : m_vfxLaneIndex == 5 ? "trail" : m_vfxLaneIndex == 6 ? "decal" : "particle";
			NormalizedEffectDefinition effect = m_normalizedEnemyClip.Effects.FirstOrDefault(i_item => (string.IsNullOrWhiteSpace(i_item.Kind) ? "particle" : i_item.Kind) == kind);
			if (effect == null) { AddVfxPreset(kind == "light" ? "Glow light" : kind == "trail" ? "Motion trail" : kind == "decal" ? "Ground decal" : "Impact burst"); effect = SelectedVfxEffect(); }
			if (effect != null) { m_vfxEffectIndex = m_normalizedEnemyClip.Effects.IndexOf(effect); AddVfxTrigger(); }
		}

		private void MoveVfxLaneMarker()
		{
			if (m_vfxLaneIndex >= 1 && m_vfxLaneIndex <= 3) { List<EnemyAnimationEventDefinition> events = m_normalizedEnemyClip.Events ?? new List<EnemyAnimationEventDefinition>(); if (events.Count == 0) return; EnemyAnimationEventDefinition item = events[Mathf.Clamp(m_authoringEventIndex, 0, events.Count - 1)]; if (LaneForEvent(item) != m_vfxLaneIndex) return; RecordAuthoringChange("Move animation lane marker", () => { item.Time = SnapVfxTime(m_time); m_normalizedEnemyClip.Events = events.OrderBy(i_event => i_event.Time).ToList(); m_authoringEventIndex = m_normalizedEnemyClip.Events.IndexOf(item); }); return; }
			MoveVfxTriggerToPlayhead();
		}

		private void DeleteVfxLaneMarker()
		{
			if (SelectedVfxMarkerCount() > 0) { DeleteSelectedVfxMarkers(); return; }
			if (m_vfxLaneIndex >= 1 && m_vfxLaneIndex <= 3) { List<EnemyAnimationEventDefinition> events = m_normalizedEnemyClip.Events ?? new List<EnemyAnimationEventDefinition>(); if (events.Count == 0) return; int index = Mathf.Clamp(m_authoringEventIndex, 0, events.Count - 1); if (LaneForEvent(events[index]) != m_vfxLaneIndex) return; RecordAuthoringChange("Delete animation lane marker", () => { events.RemoveAt(index); m_authoringEventIndex = Mathf.Clamp(m_authoringEventIndex, 0, Mathf.Max(0, events.Count - 1)); }); return; }
			DeleteVfxTrigger();
		}

		private void AddVfxEffect()
		{
			string[] bones = AvailableBoneNames(); string bone = bones.FirstOrDefault() ?? "root";
			RecordAuthoringChange("Add animation VFX", () =>
			{
				NormalizedEffectDefinition effect = NewVfxEffect(UniqueVfxId("new-effect", null), bone);
				m_normalizedEnemyClip.Effects.Add(effect); m_vfxEffectIndex = m_normalizedEnemyClip.Effects.Count - 1;
			});
		}

		private void DuplicateVfxEffect()
		{
			NormalizedEffectDefinition source = SelectedVfxEffect(); if (source == null) return;
			RecordAuthoringChange("Duplicate animation VFX", () =>
			{
				string json = Newtonsoft.Json.JsonConvert.SerializeObject(source);
				NormalizedEffectDefinition copy = Newtonsoft.Json.JsonConvert.DeserializeObject<NormalizedEffectDefinition>(json);
				copy.Id = UniqueVfxId(source.Id + "-copy", null); m_normalizedEnemyClip.Effects.Add(copy); m_vfxEffectIndex = m_normalizedEnemyClip.Effects.Count - 1;
			});
		}

		private void DeleteVfxEffect()
		{
			NormalizedEffectDefinition effect = SelectedVfxEffect(); if (effect == null) return;
			RecordAuthoringChange("Delete animation VFX", () =>
			{
				m_normalizedEnemyClip.Effects.Remove(effect);
				foreach (NormalizedEffectTrigger trigger in m_normalizedEnemyClip.EffectTriggers) trigger.Effects.RemoveAll(i_id => i_id == effect.Id);
				m_normalizedEnemyClip.EffectTriggers.RemoveAll(i_trigger => i_trigger.Effects.Count == 0);
				m_vfxEffectIndex = Mathf.Clamp(m_vfxEffectIndex, 0, Mathf.Max(0, m_normalizedEnemyClip.Effects.Count - 1));
			});
		}

		private void AddVfxTrigger()
		{
			NormalizedEffectDefinition effect = SelectedVfxEffect(); if (effect == null) return;
			RecordAuthoringChange("Add VFX trigger", () =>
			{
				NormalizedEffectTrigger trigger = new NormalizedEffectTrigger { Time = SnapVfxTime(m_time), Source = "author", Effects = new List<string> { effect.Id } };
				m_normalizedEnemyClip.EffectTriggers.Add(trigger); m_normalizedEnemyClip.EffectTriggers = m_normalizedEnemyClip.EffectTriggers.OrderBy(i_trigger => i_trigger.Time).ToList();
				m_vfxTriggerIndex = m_normalizedEnemyClip.EffectTriggers.IndexOf(trigger);
			});
		}

		private void MoveVfxTriggerToPlayhead()
		{
			if (m_normalizedEnemyClip.EffectTriggers.Count == 0) return;
			NormalizedEffectTrigger trigger = m_normalizedEnemyClip.EffectTriggers[Mathf.Clamp(m_vfxTriggerIndex, 0, m_normalizedEnemyClip.EffectTriggers.Count - 1)];
			RecordAuthoringChange("Move VFX trigger", () =>
			{
				trigger.Time = SnapVfxTime(m_time); m_normalizedEnemyClip.EffectTriggers = m_normalizedEnemyClip.EffectTriggers.OrderBy(i_item => i_item.Time).ToList(); m_vfxTriggerIndex = m_normalizedEnemyClip.EffectTriggers.IndexOf(trigger);
			});
		}

		private void DeleteVfxTrigger()
		{
			if (SelectedVfxMarkerCount() > 0) { DeleteSelectedVfxMarkers(); return; }
			if (m_normalizedEnemyClip.EffectTriggers.Count == 0) return;
			RecordAuthoringChange("Delete VFX trigger", () =>
			{
				m_normalizedEnemyClip.EffectTriggers.RemoveAt(Mathf.Clamp(m_vfxTriggerIndex, 0, m_normalizedEnemyClip.EffectTriggers.Count - 1));
				m_vfxTriggerIndex = Mathf.Clamp(m_vfxTriggerIndex, 0, Mathf.Max(0, m_normalizedEnemyClip.EffectTriggers.Count - 1));
			});
		}

		private void SelectEffectFromTrigger(NormalizedEffectTrigger i_trigger)
		{
			if (i_trigger?.Effects == null || i_trigger.Effects.Count == 0) return;
			int index = m_normalizedEnemyClip.Effects.FindIndex(i_effect => i_effect.Id == i_trigger.Effects[0]); if (index >= 0) m_vfxEffectIndex = index;
		}

		private NormalizedEffectDefinition SelectedVfxEffect()
		{
			EnsureVfxCollections();
			return m_normalizedEnemyClip.Effects.Count == 0 ? null : m_normalizedEnemyClip.Effects[Mathf.Clamp(m_vfxEffectIndex, 0, m_normalizedEnemyClip.Effects.Count - 1)];
		}

		private void EnsureVfxCollections()
		{
			if (m_normalizedEnemyClip.Events == null) m_normalizedEnemyClip.Events = new List<EnemyAnimationEventDefinition>();
			if (m_normalizedEnemyClip.Effects == null) m_normalizedEnemyClip.Effects = new List<NormalizedEffectDefinition>();
			if (m_normalizedEnemyClip.EffectTriggers == null) m_normalizedEnemyClip.EffectTriggers = new List<NormalizedEffectTrigger>();
		}

		private string UniqueVfxId(string i_requested, NormalizedEffectDefinition i_self)
		{
			string root = string.IsNullOrWhiteSpace(i_requested) ? "effect" : new string(i_requested.Trim().ToLowerInvariant().Select(i_char => char.IsLetterOrDigit(i_char) || i_char == '-' || i_char == '_' ? i_char : '-').ToArray()).Trim('-');
			if (string.IsNullOrEmpty(root)) root = "effect"; string candidate = root; int suffix = 2;
			while (m_normalizedEnemyClip.Effects.Any(i_effect => i_effect != i_self && i_effect.Id == candidate)) candidate = root + "-" + suffix++;
			if (i_self != null && i_self.Id != candidate)
				foreach (NormalizedEffectTrigger trigger in m_normalizedEnemyClip.EffectTriggers) for (int index = 0; index < trigger.Effects.Count; index++) if (trigger.Effects[index] == i_self.Id) trigger.Effects[index] = candidate;
			return candidate;
		}

		private float SnapVfxTime(float i_time)
		{
			float fps = Mathf.Max(1f, m_normalizedEnemyClip.FrameRate); return Mathf.Clamp(Mathf.Round(i_time * fps) / fps, 0f, m_normalizedEnemyClip.DurationSeconds);
		}

		private static string VfxBoneName(string i_target)
		{
			if (string.IsNullOrEmpty(i_target)) return string.Empty; int slash = i_target.IndexOf('/'); return slash < 0 ? i_target : i_target.Substring(slash + 1);
		}

		private static Color VfxColor(NormalizedColor i_color) { return i_color == null ? Color.white : new Color(i_color.R, i_color.G, i_color.B, i_color.A); }
		private static NormalizedColor VfxColor(Color i_color) { return new NormalizedColor { R = i_color.r, G = i_color.g, B = i_color.b, A = i_color.a }; }

		private static NormalizedEffectDefinition NewVfxEffect(string i_id, string i_bone)
		{
			return new NormalizedEffectDefinition
			{
				Id = i_id, Kind = "particle", Trigger = "author", Index = 0, Target = "enemy-bone/" + i_bone, DurationSeconds = 0.35f, Loop = false, MaxParticles = 24,
				StartLifetimeMin = 0.12f, StartLifetimeMax = 0.3f, StartSpeedMin = 0.5f, StartSpeedMax = 1.5f, StartSizeMin = 0.08f, StartSizeMax = 0.18f,
				GravityMin = 0f, GravityMax = 0f, ScaleX = 1f, ScaleY = 1f, ScaleZ = 1f, SimulationSpace = "Local",
				StartColorMin = new NormalizedColor { R = 1f, G = 1f, B = 1f, A = 1f }, StartColorMax = new NormalizedColor { R = 1f, G = 1f, B = 1f, A = 1f },
				EmissionRateMin = 12f, EmissionRateMax = 24f, ShapeEnabled = true, Shape = "Cone", ShapeRadius = 0.05f, ShapeAngle = 25f,
				TextureSheetTilesX = 1, TextureSheetTilesY = 1, TextureSheetFrame = 0, TextureSheetSprites = new List<string>(), SortingOrder = 100,
				LightIntensity = 1f, LightRadius = 1f, TrailWidth = .12f, TrailTime = .25f, DecalPixelsPerUnit = 32f,
				Source = new Newtonsoft.Json.Linq.JObject { ["kind"] = "authored" }
			};
		}

		private void SampleAuthoredVfx(float i_time)
		{
			if (!m_showFinisherEffects || m_samplingCameraBounds || m_normalizedEnemyClip == null) return;
			EnsureVfxCollections();
			Dictionary<string, NormalizedEffectDefinition> effects = m_normalizedEnemyClip.Effects.Where(i_effect => i_effect != null && !string.IsNullOrEmpty(i_effect.Id)).GroupBy(i_effect => i_effect.Id).ToDictionary(i_group => i_group.Key, i_group => i_group.First());
			foreach (NormalizedEffectTrigger trigger in m_normalizedEnemyClip.EffectTriggers)
			{
				float elapsed = i_time - trigger.Time; if (elapsed < -0.0001f) continue;
				foreach (string id in trigger.Effects ?? new List<string>())
				{
					if (!effects.TryGetValue(id, out NormalizedEffectDefinition effect) || elapsed > effect.DurationSeconds + effect.StartLifetimeMax + 0.25f) continue;
					Transform parent = ResolveAuthoredVfxParent(effect.Target); if (parent == null) continue;
					CreateAuthoredVfxPreview(effect, parent, elapsed);
				}
			}
		}

		private Transform ResolveAuthoredVfxParent(string i_target)
		{
			string bone = VfxBoneName(i_target);
			if (i_target != null && i_target.StartsWith("bone/", StringComparison.Ordinal)) return FindStudioPlayerBone(bone);
			return m_normalizedEnemyBones.TryGetValue(bone, out Transform target) ? target : m_enemyInstance?.transform;
		}

		private void CreateAuthoredVfxPreview(NormalizedEffectDefinition i_effect, Transform i_parent, float i_elapsed)
		{
			GameObject root = new GameObject("PreviewVFX_" + i_effect.Id) { hideFlags = HideFlags.HideAndDontSave };
			root.transform.SetParent(i_parent, false); root.transform.localPosition = new Vector3(i_effect.PositionX, i_effect.PositionY, i_effect.PositionZ);
			root.transform.localEulerAngles = new Vector3(i_effect.RotationX, i_effect.RotationY, i_effect.RotationZ); root.transform.localScale = new Vector3(i_effect.ScaleX, i_effect.ScaleY, i_effect.ScaleZ);
			string kind = string.IsNullOrWhiteSpace(i_effect.Kind) ? "particle" : i_effect.Kind;
			if (kind == "light")
			{
				UnityEngine.Rendering.Universal.Light2D light = root.AddComponent<UnityEngine.Rendering.Universal.Light2D>();
				light.lightType = UnityEngine.Rendering.Universal.Light2D.LightType.Point; light.color = VfxColor(i_effect.StartColorMin);
				light.intensity = Mathf.Clamp(i_effect.LightIntensity, 0f, 20f); light.pointLightOuterRadius = Mathf.Clamp(i_effect.LightRadius, .01f, 100f); light.pointLightInnerRadius = light.pointLightOuterRadius * .35f;
				m_effectPreviewInstances.Add(root); return;
			}
			if (kind == "decal")
			{
				Sprite sprite = GetVfxRegionSprite(i_effect); if (sprite != null) { SpriteRenderer decalRenderer = root.AddComponent<SpriteRenderer>(); decalRenderer.sprite = sprite; decalRenderer.color = VfxColor(i_effect.StartColorMin); decalRenderer.sortingOrder = i_effect.SortingOrder; }
				m_effectPreviewInstances.Add(root); return;
			}
			if (kind == "trail")
			{
				TrailRenderer trail = root.AddComponent<TrailRenderer>(); trail.time = Mathf.Clamp(i_effect.TrailTime, .01f, 30f); trail.widthMultiplier = Mathf.Clamp(i_effect.TrailWidth, .001f, 20f);
				trail.startColor = VfxColor(i_effect.StartColorMin); trail.endColor = VfxColor(i_effect.StartColorMax); trail.sharedMaterial = GetVfxPreviewMaterial(i_effect.RuntimeTexture);
				trail.AddPosition(root.transform.position - root.transform.right * .3f); trail.AddPosition(root.transform.position);
				m_effectPreviewInstances.Add(root); return;
			}
			ParticleSystem particles = root.AddComponent<ParticleSystem>(); ParticleSystem.MainModule main = particles.main;
			main.duration = Mathf.Max(0.01f, i_effect.DurationSeconds); main.loop = i_effect.Loop; main.maxParticles = Mathf.Clamp(i_effect.MaxParticles, 1, 10000);
			main.startLifetime = new ParticleSystem.MinMaxCurve(i_effect.StartLifetimeMin, i_effect.StartLifetimeMax); main.startSpeed = new ParticleSystem.MinMaxCurve(i_effect.StartSpeedMin, i_effect.StartSpeedMax);
			main.startSize = new ParticleSystem.MinMaxCurve(i_effect.StartSizeMin, i_effect.StartSizeMax); main.gravityModifier = new ParticleSystem.MinMaxCurve(i_effect.GravityMin, i_effect.GravityMax);
			main.simulationSpace = i_effect.SimulationSpace == "World" ? ParticleSystemSimulationSpace.World : ParticleSystemSimulationSpace.Local;
			main.startColor = new ParticleSystem.MinMaxGradient(VfxColor(i_effect.StartColorMin), VfxColor(i_effect.StartColorMax));
			ParticleSystem.EmissionModule emission = particles.emission; emission.rateOverTime = new ParticleSystem.MinMaxCurve(i_effect.EmissionRateMin, i_effect.EmissionRateMax);
			ParticleSystem.ShapeModule shape = particles.shape; shape.enabled = i_effect.ShapeEnabled; if (Enum.TryParse(i_effect.Shape, true, out ParticleSystemShapeType shapeType)) shape.shapeType = shapeType; shape.radius = Mathf.Max(0f, i_effect.ShapeRadius); shape.angle = Mathf.Clamp(i_effect.ShapeAngle, 0f, 90f);
			ParticleSystemRenderer renderer = particles.GetComponent<ParticleSystemRenderer>(); renderer.sharedMaterial = GetVfxPreviewMaterial(i_effect.RuntimeTexture); if (!string.IsNullOrEmpty(i_effect.SortingLayer)) renderer.sortingLayerName = i_effect.SortingLayer; renderer.sortingOrder = i_effect.SortingOrder;
			int tilesX = Mathf.Clamp(i_effect.TextureSheetTilesX, 1, 64), tilesY = Mathf.Clamp(i_effect.TextureSheetTilesY, 1, 64);
			if (i_effect.RuntimeTexture != null && (tilesX > 1 || tilesY > 1)) { ParticleSystem.TextureSheetAnimationModule sheet = particles.textureSheetAnimation; sheet.enabled = true; sheet.mode = ParticleSystemAnimationMode.Grid; sheet.numTilesX = tilesX; sheet.numTilesY = tilesY; sheet.frameOverTime = new ParticleSystem.MinMaxCurve(0f); sheet.startFrame = new ParticleSystem.MinMaxCurve(Mathf.Clamp(i_effect.TextureSheetFrame, 0, tilesX * tilesY - 1) / (float)(tilesX * tilesY)); }
			NormalizedParticleEffectRuntime.ApplyAdvancedModules(particles, i_effect);
			if (Enum.TryParse(i_effect.RenderMode, true, out ParticleSystemRenderMode renderMode) && renderMode != ParticleSystemRenderMode.Mesh) renderer.renderMode = renderMode;
			particles.useAutoRandomSeed = false; particles.randomSeed = (uint)Mathf.Max(1, Math.Abs(i_effect.Id.GetHashCode()) + Mathf.RoundToInt(i_effect.DurationSeconds * 1000f));
			particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); particles.Play(true); if (i_elapsed > 0f) particles.Simulate(i_elapsed, true, false, false); particles.Pause(true);
			m_effectPreviewInstances.Add(root);
		}

		private Material GetVfxPreviewMaterial(Texture2D i_texture = null)
		{
			if (i_texture != null && m_vfxTextureMaterials.TryGetValue(i_texture, out Material cached) && cached != null) return cached;
			if (i_texture == null && m_vfxPreviewMaterial != null) return m_vfxPreviewMaterial;
			Shader shader = Shader.Find("Sprites/Default");
			if (shader == null) shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
			if (shader == null) return m_previewMaterial;
			Material material = new Material(shader) { name = "Animation VFX Preview Material", hideFlags = HideFlags.HideAndDontSave };
			if (i_texture != null) { material.mainTexture = i_texture; m_vfxTextureMaterials[i_texture] = material; }
			else m_vfxPreviewMaterial = material;
			m_modPreviewAssets.Add(material); return material;
		}

		private Sprite GetVfxRegionSprite(NormalizedEffectDefinition i_effect)
		{
			if (i_effect.RuntimeTexture == null) return null;
			int tilesX = Mathf.Clamp(i_effect.TextureSheetTilesX, 1, 64), tilesY = Mathf.Clamp(i_effect.TextureSheetTilesY, 1, 64), frame = Mathf.Clamp(i_effect.TextureSheetFrame, 0, tilesX * tilesY - 1);
			string key = i_effect.RuntimeTexture.GetInstanceID() + ":" + tilesX + ":" + tilesY + ":" + frame + ":" + i_effect.DecalPixelsPerUnit;
			if (m_vfxRegionSprites.TryGetValue(key, out Sprite existing) && existing != null) return existing;
			int column = frame % tilesX, row = tilesY - 1 - frame / tilesX; float width = i_effect.RuntimeTexture.width / (float)tilesX, height = i_effect.RuntimeTexture.height / (float)tilesY;
			Sprite sprite = Sprite.Create(i_effect.RuntimeTexture, new Rect(column * width, row * height, width, height), new Vector2(.5f, .5f), Mathf.Clamp(i_effect.DecalPixelsPerUnit, 1f, 1024f), 0, SpriteMeshType.FullRect);
			sprite.hideFlags = HideFlags.HideAndDontSave; m_vfxRegionSprites[key] = sprite; m_modPreviewAssets.Add(sprite); return sprite;
		}

		private sealed class VfxLibraryEntry
		{
			public NormalizedEffectDefinition Effect;
			public string Source;
			public string Path;
			public string Serialized;
		}

		private sealed class VfxAudioLibraryEntry
		{
			public string File;
			public float Volume;
			public string Source;
			public string Path;
		}
	}
}
