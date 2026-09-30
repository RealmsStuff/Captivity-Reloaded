using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace CaptivityReloaded.Modding
{
	[JsonObject(MemberSerialization.OptIn)]
	public sealed class NormalizedPlayerAnimationDocument
	{
		[JsonProperty("schemaVersion", Required = Required.Always)] public int SchemaVersion { get; set; }
		[JsonProperty("type", Required = Required.Always)] public string Type { get; set; }
		[JsonProperty("id", Required = Required.Always)] public string Id { get; set; }
		[JsonProperty("rig", Required = Required.Always)] public string Rig { get; set; }
		[JsonProperty("displayName", Required = Required.Always)] public string DisplayName { get; set; }
		[JsonProperty("durationSeconds", Required = Required.Always)] public float DurationSeconds { get; set; }
		[JsonProperty("frameRate", Required = Required.Always)] public float FrameRate { get; set; }
		[JsonProperty("loop", Required = Required.Always)] public bool Loop { get; set; }
		[JsonProperty("tracks", Required = Required.Always)] public List<NormalizedNumericTrack> Tracks { get; set; } = new List<NormalizedNumericTrack>();
		[JsonProperty("objectTracks")] public List<NormalizedObjectTrack> ObjectTracks { get; set; } = new List<NormalizedObjectTrack>();
		[JsonProperty("events")] public List<NormalizedPlayerEvent> Events { get; set; } = new List<NormalizedPlayerEvent>();
		[JsonProperty("effects")] public List<NormalizedEffectDefinition> Effects { get; set; } = new List<NormalizedEffectDefinition>();
		[JsonProperty("effectTriggers")] public List<NormalizedEffectTrigger> EffectTriggers { get; set; } = new List<NormalizedEffectTrigger>();
		[JsonProperty("warnings")] public List<string> Warnings { get; set; } = new List<string>();
		[JsonProperty("source")] public JObject Source { get; set; }
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class NormalizedNumericTrack
	{
		[JsonProperty("target", Required = Required.Always)] public string Target { get; set; }
		[JsonProperty("property", Required = Required.Always)] public string Property { get; set; }
		[JsonProperty("keys", Required = Required.Always)] public List<NormalizedNumericKey> Keys { get; set; } = new List<NormalizedNumericKey>();
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class NormalizedNumericKey
	{
		[JsonProperty("time", Required = Required.Always)] public float Time { get; set; }
		[JsonProperty("value", Required = Required.Always)] public float Value { get; set; }
		[JsonProperty("inTangent")] public float? InTangent { get; set; }
		[JsonProperty("outTangent")] public float? OutTangent { get; set; }
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class NormalizedPlayerEvent
	{
		[JsonProperty("time", Required = Required.Always)] public float Time { get; set; }
		[JsonProperty("name", Required = Required.Always)] public string Name { get; set; }
		[JsonProperty("stringValue")] public string StringValue { get; set; }
		[JsonProperty("floatValue")] public float FloatValue { get; set; }
		[JsonProperty("intValue")] public int IntValue { get; set; }
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class NormalizedObjectTrack
	{
		[JsonProperty("target", Required = Required.Always)] public string Target { get; set; }
		[JsonProperty("property", Required = Required.Always)] public string Property { get; set; }
		[JsonProperty("keys", Required = Required.Always)] public List<NormalizedObjectKey> Keys { get; set; } = new List<NormalizedObjectKey>();
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class NormalizedObjectKey
	{
		[JsonProperty("time", Required = Required.Always)] public float Time { get; set; }
		[JsonProperty("asset")] public string Asset { get; set; }
		[JsonProperty("name")] public string Name { get; set; }
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class NormalizedColor
	{
		[JsonProperty("r", Required = Required.Always)] public float R { get; set; }
		[JsonProperty("g", Required = Required.Always)] public float G { get; set; }
		[JsonProperty("b", Required = Required.Always)] public float B { get; set; }
		[JsonProperty("a", Required = Required.Always)] public float A { get; set; }
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class NormalizedEffectDefinition
	{
		[JsonProperty("id", Required = Required.Always)] public string Id { get; set; }
		[JsonProperty("kind")] public string Kind { get; set; } = "particle";
		[JsonProperty("trigger", Required = Required.Always)] public string Trigger { get; set; }
		[JsonProperty("index", Required = Required.Always)] public int Index { get; set; }
		[JsonProperty("target", Required = Required.Always)] public string Target { get; set; }
		[JsonProperty("durationSeconds", Required = Required.Always)] public float DurationSeconds { get; set; }
		[JsonProperty("loop", Required = Required.Always)] public bool Loop { get; set; }
		[JsonProperty("maxParticles", Required = Required.Always)] public int MaxParticles { get; set; }
		[JsonProperty("startLifetimeMin", Required = Required.Always)] public float StartLifetimeMin { get; set; }
		[JsonProperty("startLifetimeMax", Required = Required.Always)] public float StartLifetimeMax { get; set; }
		[JsonProperty("startSpeedMin", Required = Required.Always)] public float StartSpeedMin { get; set; }
		[JsonProperty("startSpeedMax", Required = Required.Always)] public float StartSpeedMax { get; set; }
		[JsonProperty("startSizeMin", Required = Required.Always)] public float StartSizeMin { get; set; }
		[JsonProperty("startSizeMax", Required = Required.Always)] public float StartSizeMax { get; set; }
		[JsonProperty("gravityMin", Required = Required.Always)] public float GravityMin { get; set; }
		[JsonProperty("gravityMax", Required = Required.Always)] public float GravityMax { get; set; }
		[JsonProperty("scaleX", Required = Required.Always)] public float ScaleX { get; set; }
		[JsonProperty("scaleY", Required = Required.Always)] public float ScaleY { get; set; }
		[JsonProperty("scaleZ", Required = Required.Always)] public float ScaleZ { get; set; }
		[JsonProperty("positionX")] public float PositionX { get; set; }
		[JsonProperty("positionY")] public float PositionY { get; set; }
		[JsonProperty("positionZ")] public float PositionZ { get; set; }
		[JsonProperty("rotationX")] public float RotationX { get; set; }
		[JsonProperty("rotationY")] public float RotationY { get; set; }
		[JsonProperty("rotationZ")] public float RotationZ { get; set; }
		[JsonProperty("simulationSpace", Required = Required.Always)] public string SimulationSpace { get; set; }
		[JsonProperty("startColorMin", Required = Required.Always)] public NormalizedColor StartColorMin { get; set; }
		[JsonProperty("startColorMax", Required = Required.Always)] public NormalizedColor StartColorMax { get; set; }
		[JsonProperty("emissionRateMin", Required = Required.Always)] public float EmissionRateMin { get; set; }
		[JsonProperty("emissionRateMax", Required = Required.Always)] public float EmissionRateMax { get; set; }
		[JsonProperty("shapeEnabled", Required = Required.Always)] public bool ShapeEnabled { get; set; }
		[JsonProperty("shape", Required = Required.Always)] public string Shape { get; set; }
		[JsonProperty("shapeRadius", Required = Required.Always)] public float ShapeRadius { get; set; }
		[JsonProperty("shapeAngle", Required = Required.Always)] public float ShapeAngle { get; set; }
		[JsonProperty("textureSheetTilesX", Required = Required.Always)] public int TextureSheetTilesX { get; set; }
		[JsonProperty("textureSheetTilesY", Required = Required.Always)] public int TextureSheetTilesY { get; set; }
		[JsonProperty("textureSheetSprites", Required = Required.Always)] public List<string> TextureSheetSprites { get; set; } = new List<string>();
		[JsonProperty("sortingLayer")] public string SortingLayer { get; set; }
		[JsonProperty("sortingOrder", Required = Required.Always)] public int SortingOrder { get; set; }
		[JsonProperty("renderMode")] public string RenderMode { get; set; }
		[JsonProperty("mesh")] public string Mesh { get; set; }
		[JsonProperty("material")] public string Material { get; set; }
		[JsonProperty("texture")] public string Texture { get; set; }
		[JsonProperty("textureSheetFrame")] public int TextureSheetFrame { get; set; }
		[JsonProperty("startLifetimeCurve")] public NormalizedParticleCurve StartLifetimeCurve { get; set; }
		[JsonProperty("startSpeedCurve")] public NormalizedParticleCurve StartSpeedCurve { get; set; }
		[JsonProperty("startSizeCurve")] public NormalizedParticleCurve StartSizeCurve { get; set; }
		[JsonProperty("gravityCurve")] public NormalizedParticleCurve GravityCurve { get; set; }
		[JsonProperty("emissionRateCurve")] public NormalizedParticleCurve EmissionRateCurve { get; set; }
		[JsonProperty("emissionRateOverDistanceCurve")] public NormalizedParticleCurve EmissionRateOverDistanceCurve { get; set; }
		[JsonProperty("startColorGradient")] public NormalizedParticleGradient StartColorGradient { get; set; }
		[JsonProperty("bursts")] public List<NormalizedParticleBurst> Bursts { get; set; } = new List<NormalizedParticleBurst>();
		[JsonProperty("colorOverLifetime")] public NormalizedParticleGradient ColorOverLifetime { get; set; }
		[JsonProperty("sizeOverLifetime")] public NormalizedParticleAxisCurves SizeOverLifetime { get; set; }
		[JsonProperty("velocityOverLifetime")] public NormalizedParticleAxisCurves VelocityOverLifetime { get; set; }
		[JsonProperty("rotationOverLifetime")] public NormalizedParticleAxisCurves RotationOverLifetime { get; set; }
		[JsonProperty("noise")] public NormalizedParticleNoise Noise { get; set; }
		[JsonProperty("textureSheetFrameOverTime")] public NormalizedParticleCurve TextureSheetFrameOverTime { get; set; }
		[JsonProperty("textureSheetStartFrame")] public NormalizedParticleCurve TextureSheetStartFrame { get; set; }
		[JsonProperty("textureSheetAnimation")] public string TextureSheetAnimation { get; set; }
		[JsonProperty("textureSheetCycleCount")] public int TextureSheetCycleCount { get; set; } = 1;
		[JsonProperty("textureSheetRowIndex")] public int TextureSheetRowIndex { get; set; }
		[JsonProperty("textureSheetUseRandomRow")] public bool TextureSheetUseRandomRow { get; set; }
		[JsonProperty("lightIntensity")] public float LightIntensity { get; set; } = 1f;
		[JsonProperty("lightRadius")] public float LightRadius { get; set; } = 1f;
		[JsonProperty("trailWidth")] public float TrailWidth { get; set; } = 0.12f;
		[JsonProperty("trailTime")] public float TrailTime { get; set; } = 0.25f;
		[JsonProperty("decalPixelsPerUnit")] public float DecalPixelsPerUnit { get; set; } = 32f;
		[JsonProperty("source", Required = Required.Always)] public JObject Source { get; set; }
		[JsonIgnore] public Texture2D RuntimeTexture { get; set; }
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class NormalizedParticleCurveKey
	{
		[JsonProperty("time")] public float Time { get; set; }
		[JsonProperty("value")] public float Value { get; set; }
		[JsonProperty("inTangent")] public float InTangent { get; set; }
		[JsonProperty("outTangent")] public float OutTangent { get; set; }
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class NormalizedParticleCurve
	{
		[JsonProperty("mode")] public string Mode { get; set; } = "Constant";
		[JsonProperty("multiplier")] public float Multiplier { get; set; } = 1f;
		[JsonProperty("constantMin")] public float ConstantMin { get; set; }
		[JsonProperty("constantMax")] public float ConstantMax { get; set; }
		[JsonProperty("curveMin")] public List<NormalizedParticleCurveKey> CurveMin { get; set; } = new List<NormalizedParticleCurveKey>();
		[JsonProperty("curveMax")] public List<NormalizedParticleCurveKey> CurveMax { get; set; } = new List<NormalizedParticleCurveKey>();
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class NormalizedParticleColorKey
	{
		[JsonProperty("time")] public float Time { get; set; }
		[JsonProperty("color")] public NormalizedColor Color { get; set; }
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class NormalizedParticleGradient
	{
		[JsonProperty("mode")] public string Mode { get; set; } = "Color";
		[JsonProperty("colorMin")] public NormalizedColor ColorMin { get; set; }
		[JsonProperty("colorMax")] public NormalizedColor ColorMax { get; set; }
		[JsonProperty("gradientMin")] public List<NormalizedParticleColorKey> GradientMin { get; set; } = new List<NormalizedParticleColorKey>();
		[JsonProperty("gradientMax")] public List<NormalizedParticleColorKey> GradientMax { get; set; } = new List<NormalizedParticleColorKey>();
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class NormalizedParticleAxisCurves
	{
		[JsonProperty("enabled")] public bool Enabled { get; set; }
		[JsonProperty("separateAxes")] public bool SeparateAxes { get; set; }
		[JsonProperty("space")] public string Space { get; set; }
		[JsonProperty("x")] public NormalizedParticleCurve X { get; set; }
		[JsonProperty("y")] public NormalizedParticleCurve Y { get; set; }
		[JsonProperty("z")] public NormalizedParticleCurve Z { get; set; }
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class NormalizedParticleBurst
	{
		[JsonProperty("time")] public float Time { get; set; }
		[JsonProperty("countMin")] public float CountMin { get; set; }
		[JsonProperty("countMax")] public float CountMax { get; set; }
		[JsonProperty("cycleCount")] public int CycleCount { get; set; } = 1;
		[JsonProperty("repeatInterval")] public float RepeatInterval { get; set; }
		[JsonProperty("probability")] public float Probability { get; set; } = 1f;
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class NormalizedParticleNoise
	{
		[JsonProperty("enabled")] public bool Enabled { get; set; }
		[JsonProperty("separateAxes")] public bool SeparateAxes { get; set; }
		[JsonProperty("strengthX")] public NormalizedParticleCurve StrengthX { get; set; }
		[JsonProperty("strengthY")] public NormalizedParticleCurve StrengthY { get; set; }
		[JsonProperty("strengthZ")] public NormalizedParticleCurve StrengthZ { get; set; }
		[JsonProperty("frequency")] public float Frequency { get; set; } = 0.5f;
		[JsonProperty("scrollSpeed")] public NormalizedParticleCurve ScrollSpeed { get; set; }
		[JsonProperty("damping")] public bool Damping { get; set; }
		[JsonProperty("octaveCount")] public int OctaveCount { get; set; } = 1;
		[JsonProperty("octaveMultiplier")] public float OctaveMultiplier { get; set; } = 0.5f;
		[JsonProperty("octaveScale")] public float OctaveScale { get; set; } = 2f;
		[JsonProperty("quality")] public string Quality { get; set; }
	}

	public static class NormalizedParticlePresentationValidator
	{
		public static bool TryValidate(NormalizedEffectDefinition i_effect, out string o_message)
		{
			o_message = null;
			if ((i_effect.Bursts?.Count ?? 0) > 64) { o_message = "An effect supports at most 64 emission bursts."; return false; }
			foreach (NormalizedParticleBurst burst in i_effect.Bursts ?? new List<NormalizedParticleBurst>())
				if (burst == null || !Finite(burst.Time) || burst.Time < 0f || !Finite(burst.CountMin) || !Finite(burst.CountMax) || burst.CountMin < 0f || burst.CountMax < burst.CountMin || burst.CycleCount < 1 || burst.CycleCount > 1000 || !Finite(burst.RepeatInterval) || burst.RepeatInterval < 0f || !Finite(burst.Probability) || burst.Probability < 0f || burst.Probability > 1f)
				{ o_message = "Particle bursts must use finite non-negative counts/times, ordered count ranges, cycleCount 1..1000, and probability 0..1."; return false; }
			foreach (NormalizedParticleCurve curve in Curves(i_effect)) if (!ValidCurve(curve)) { o_message = "Particle curves must contain at most 128 finite, time-ordered keys."; return false; }
			foreach (NormalizedParticleGradient gradient in new[] { i_effect.StartColorGradient, i_effect.ColorOverLifetime }.Where(i_value => i_value != null))
				if (!ValidGradient(gradient)) { o_message = "Particle gradients must contain at most 32 finite, time-ordered color keys between 0 and 1."; return false; }
			if (i_effect.TextureSheetCycleCount < 1 || i_effect.TextureSheetCycleCount > 1000) { o_message = "Texture-sheet cycle count must be 1..1000."; return false; }
			return true;
		}

		private static IEnumerable<NormalizedParticleCurve> Curves(NormalizedEffectDefinition i_effect)
		{
			foreach (NormalizedParticleCurve curve in new[] { i_effect.StartLifetimeCurve, i_effect.StartSpeedCurve, i_effect.StartSizeCurve, i_effect.GravityCurve, i_effect.EmissionRateCurve, i_effect.EmissionRateOverDistanceCurve }.Where(i_value => i_value != null)) yield return curve;
			foreach (NormalizedParticleAxisCurves axes in new[] { i_effect.SizeOverLifetime, i_effect.VelocityOverLifetime, i_effect.RotationOverLifetime }.Where(i_value => i_value != null))
				foreach (NormalizedParticleCurve curve in new[] { axes.X, axes.Y, axes.Z }.Where(i_value => i_value != null)) yield return curve;
			if (i_effect.Noise != null) foreach (NormalizedParticleCurve curve in new[] { i_effect.Noise.StrengthX, i_effect.Noise.StrengthY, i_effect.Noise.StrengthZ, i_effect.Noise.ScrollSpeed }.Where(i_value => i_value != null)) yield return curve;
			if (i_effect.TextureSheetFrameOverTime != null) yield return i_effect.TextureSheetFrameOverTime;
			if (i_effect.TextureSheetStartFrame != null) yield return i_effect.TextureSheetStartFrame;
		}

		private static bool ValidCurve(NormalizedParticleCurve i_curve)
		{
			if (!Finite(i_curve.Multiplier) || !Finite(i_curve.ConstantMin) || !Finite(i_curve.ConstantMax)) return false;
			return ValidKeys(i_curve.CurveMin) && ValidKeys(i_curve.CurveMax);
		}

		private static bool ValidKeys(List<NormalizedParticleCurveKey> i_keys)
		{
			if ((i_keys?.Count ?? 0) > 128) return false; float previous = -float.MaxValue;
			foreach (NormalizedParticleCurveKey key in i_keys ?? new List<NormalizedParticleCurveKey>()) { if (key == null || !Finite(key.Time) || !Finite(key.Value) || !Finite(key.InTangent) || !Finite(key.OutTangent) || key.Time < previous) return false; previous = key.Time; }
			return true;
		}

		private static bool ValidGradient(NormalizedParticleGradient i_gradient)
		{
			return ValidColorKeys(i_gradient.GradientMin) && ValidColorKeys(i_gradient.GradientMax);
		}

		private static bool ValidColorKeys(List<NormalizedParticleColorKey> i_keys)
		{
			if ((i_keys?.Count ?? 0) > 32) return false; float previous = -1f;
			foreach (NormalizedParticleColorKey key in i_keys ?? new List<NormalizedParticleColorKey>()) { if (key == null || !Finite(key.Time) || key.Time < previous || key.Time < 0f || key.Time > 1f || key.Color == null || !Finite(key.Color.R) || !Finite(key.Color.G) || !Finite(key.Color.B) || !Finite(key.Color.A)) return false; previous = key.Time; }
			return true;
		}

		private static bool Finite(float i_value) { return !float.IsNaN(i_value) && !float.IsInfinity(i_value); }
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class NormalizedEffectTrigger
	{
		[JsonProperty("time", Required = Required.Always)] public float Time { get; set; }
		[JsonProperty("source", Required = Required.Always)] public string Source { get; set; }
		[JsonProperty("effects", Required = Required.Always)] public List<string> Effects { get; set; } = new List<string>();
	}

	public sealed class NormalizedAnimationPresentation
	{
		public IReadOnlyList<NormalizedNumericTrack> Tracks { get; }
		public IReadOnlyList<NormalizedObjectTrack> ObjectTracks { get; }
		public IReadOnlyList<NormalizedEffectDefinition> Effects { get; }
		public IReadOnlyList<NormalizedEffectTrigger> EffectTriggers { get; }
		public NormalizedAnimationPresentation(NormalizedPlayerAnimationDocument i_document)
		{
			Tracks = i_document.Tracks ?? new List<NormalizedNumericTrack>();
			ObjectTracks = i_document.ObjectTracks ?? new List<NormalizedObjectTrack>();
			Effects = i_document.Effects ?? new List<NormalizedEffectDefinition>();
			EffectTriggers = i_document.EffectTriggers ?? new List<NormalizedEffectTrigger>();
		}
	}

	public sealed class NormalizedPlayerAnimationDefinition
	{
		public ContentId Id { get; }
		public ContentId Rig { get; }
		public string PackId { get; }
		public string Source { get; }
		public string DisplayName { get; }
		public float DurationSeconds { get; }
		public float FrameRate { get; }
		public bool Loop { get; }
		public IReadOnlyList<NormalizedNumericTrack> Tracks { get; }
		public IReadOnlyList<NormalizedPlayerEvent> Events { get; }
		private readonly NormalizedPlayerAnimationDocument m_document;
		private readonly Dictionary<string, Sprite> m_runtimeSprites = new Dictionary<string, Sprite>(StringComparer.Ordinal);

		internal NormalizedPlayerAnimationDefinition(ContentId i_id, ContentId i_rig, string i_packId,
			string i_source, NormalizedPlayerAnimationDocument i_document)
		{
			m_document = i_document;
			Id = i_id; Rig = i_rig; PackId = i_packId; Source = i_source ?? string.Empty;
			DisplayName = i_document.DisplayName; DurationSeconds = i_document.DurationSeconds;
			FrameRate = i_document.FrameRate; Loop = i_document.Loop;
			Tracks = i_document.Tracks; Events = i_document.Events;
		}

		internal void LoadRuntimeSprites(string i_packRoot, ValidationReport io_report)
		{
			foreach (NormalizedObjectTrack track in m_document.ObjectTracks ?? new List<NormalizedObjectTrack>())
				foreach (NormalizedObjectKey key in track.Keys ?? new List<NormalizedObjectKey>())
				{
					if (string.IsNullOrEmpty(key.Asset) || !key.Asset.EndsWith(".png", StringComparison.OrdinalIgnoreCase)) continue;
					if (!RuntimePngAssetLoader.TryLoad(i_packRoot, key.Asset, Id + "/" + (key.Name ?? "sprite"), FilterMode.Point,
						io_report, "player-animation.sprite-file", "player-animation.sprite-decode", Source, out Texture2D texture)) continue;
					Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 32f, 0, SpriteMeshType.FullRect);
					sprite.name = key.Name ?? Id + "/sprite";
					m_runtimeSprites[key.Asset] = sprite;
				}
			LoadEffectTextures(m_document.Effects, i_packRoot, io_report, Source, Id.ToString(), "player-animation");
		}

		internal static void LoadEffectTextures(IEnumerable<NormalizedEffectDefinition> i_effects, string i_packRoot,
			ValidationReport io_report, string i_source, string i_owner, string i_codePrefix)
		{
			Dictionary<string, Texture2D> loaded = new Dictionary<string, Texture2D>(StringComparer.OrdinalIgnoreCase);
			foreach (NormalizedEffectDefinition effect in i_effects ?? Enumerable.Empty<NormalizedEffectDefinition>())
			{
				if (effect == null || string.IsNullOrWhiteSpace(effect.Texture)) continue;
				if (loaded.TryGetValue(effect.Texture, out Texture2D existing)) { effect.RuntimeTexture = existing; continue; }
				if (!RuntimePngAssetLoader.TryLoad(i_packRoot, effect.Texture, i_owner + "/vfx/" + effect.Id,
					FilterMode.Point, io_report, i_codePrefix + ".effect-texture-file", i_codePrefix + ".effect-texture-decode",
					i_source, out Texture2D texture)) continue;
				loaded.Add(effect.Texture, texture); effect.RuntimeTexture = texture;
			}
		}

		public EnemyAnimationClipDefinition CreateFinisherClip()
		{
			List<NormalizedNumericTrack> runtimeTracks = Tracks.Where(IsRuntimeTrack).ToList();
			SortedSet<float> times = new SortedSet<float> { 0f, DurationSeconds };
			foreach (NormalizedNumericTrack track in runtimeTracks)
				foreach (NormalizedNumericKey key in track.Keys) times.Add(key.Time);
			foreach (NormalizedObjectTrack track in m_document.ObjectTracks ?? new List<NormalizedObjectTrack>())
				foreach (NormalizedObjectKey key in track.Keys ?? new List<NormalizedObjectKey>()) times.Add(key.Time);
			if (times.Count > 256)
			{
				times.Clear();
				for (int index = 0; index < 256; index++) times.Add(DurationSeconds * index / 255f);
			}

			List<EnemyAnimationFrameDefinition> frames = new List<EnemyAnimationFrameDefinition>();
			foreach (float time in times)
			{
				Dictionary<string, EnemyBonePoseDefinition> bones = new Dictionary<string, EnemyBonePoseDefinition>(StringComparer.Ordinal);
				foreach (NormalizedNumericTrack track in runtimeTracks)
				{
					string bone = track.Target.Substring(track.Target.IndexOf('/') + 1);
					if (!bones.TryGetValue(bone, out EnemyBonePoseDefinition pose))
					{
						pose = new EnemyBonePoseDefinition();
						bones.Add(bone, pose);
					}
					float value = Evaluate(track.Keys, time);
					switch (track.Property)
					{
					case "position.x": pose.X = value; break;
					case "position.y": pose.Y = value; break;
					case "rotation.z": pose.Rotation = value; break;
					case "scale.x": pose.ScaleX = value; break;
					case "scale.y": pose.ScaleY = value; break;
					case "color.r": pose.ColorR = value; break;
					case "color.g": pose.ColorG = value; break;
					case "color.b": pose.ColorB = value; break;
					case "color.a": pose.ColorA = value; break;
					case "sortingOrder": pose.RuntimeSortingOrder = (int)Math.Round(value); break;
					}
				}
				foreach (NormalizedObjectTrack track in m_document.ObjectTracks ?? new List<NormalizedObjectTrack>())
				{
					if (track == null || track.Property != "sprite" || !track.Target.StartsWith("sprite/", StringComparison.Ordinal)) continue;
					NormalizedObjectKey selected = null;
					foreach (NormalizedObjectKey key in track.Keys ?? new List<NormalizedObjectKey>())
						if (key.Time <= time) selected = key; else break;
					if (selected == null) continue;
					string bone = track.Target.Substring("sprite/".Length);
					if (!bones.TryGetValue(bone, out EnemyBonePoseDefinition pose)) bones.Add(bone, pose = new EnemyBonePoseDefinition());
					pose.RuntimeSpriteName = selected.Name;
					if (!string.IsNullOrEmpty(selected.Asset) && m_runtimeSprites.TryGetValue(selected.Asset, out Sprite sprite)) pose.RuntimeSprite = sprite;
				}
				frames.Add(new EnemyAnimationFrameDefinition { Time = time, Bones = bones });
			}

			List<EnemyAnimationEventDefinition> events = Events.Select(i_event => new EnemyAnimationEventDefinition
			{
				Time = i_event.Time, Type = i_event.Name, Amount = i_event.FloatValue
			}).ToList();
			return new EnemyAnimationClipDefinition
			{
				DurationSeconds = DurationSeconds, Loop = Loop, Frames = frames, Events = events,
				NormalizedPresentation = new NormalizedAnimationPresentation(m_document)
			};
		}

		private static bool IsRuntimeTrack(NormalizedNumericTrack i_track)
		{
			bool transform = i_track.Target.StartsWith("bone/", StringComparison.Ordinal) &&
				(i_track.Property == "position.x" || i_track.Property == "position.y" || i_track.Property == "rotation.z"
					|| i_track.Property == "scale.x" || i_track.Property == "scale.y");
			bool visual = i_track.Target.StartsWith("sprite/", StringComparison.Ordinal) &&
				(i_track.Property == "color.r" || i_track.Property == "color.g" || i_track.Property == "color.b"
					|| i_track.Property == "color.a" || i_track.Property == "sortingOrder");
			return transform || visual;
		}

		private static float Evaluate(IReadOnlyList<NormalizedNumericKey> i_keys, float i_time)
		{
			if (i_keys.Count == 1 || i_time <= i_keys[0].Time) return i_keys[0].Value;
			if (i_time >= i_keys[i_keys.Count - 1].Time) return i_keys[i_keys.Count - 1].Value;
			int upper = 1;
			while (upper < i_keys.Count && i_keys[upper].Time < i_time) upper++;
			NormalizedNumericKey left = i_keys[upper - 1], right = i_keys[upper];
			float duration = right.Time - left.Time;
			if (duration <= 0f) return right.Value;
			float t = (i_time - left.Time) / duration;
			if (!left.OutTangent.HasValue || !right.InTangent.HasValue) return left.Value + (right.Value - left.Value) * t;
			float t2 = t * t, t3 = t2 * t;
			return (2f * t3 - 3f * t2 + 1f) * left.Value + (t3 - 2f * t2 + t) * duration * left.OutTangent.Value
				+ (-2f * t3 + 3f * t2) * right.Value + (t3 - t2) * duration * right.InTangent.Value;
		}
	}

	public sealed class NormalizedPlayerAnimationLoadResult
	{
		public NormalizedPlayerAnimationDefinition Definition { get; internal set; }
		public ValidationReport Report { get; } = new ValidationReport();
	}

	public static class NormalizedPlayerAnimationParser
	{
		private static readonly HashSet<string> Bones = new HashSet<string>(new[] { "hips", "butt", "spine", "chest", "neck", "head",
			"arm-right-upper", "arm-right-lower", "hand-right", "arm-left-upper", "arm-left-lower", "hand-left",
			"leg-right-upper", "leg-right-lower", "foot-right", "leg-left-upper", "leg-left-lower", "foot-left", "ear", "face" }, StringComparer.Ordinal);
		private static readonly HashSet<string> Properties = new HashSet<string>(new[] { "position.x", "position.y", "position.z",
			"rotation.x", "rotation.y", "rotation.z", "scale.x", "scale.y", "scale.z", "color.r", "color.g", "color.b", "color.a", "sortingOrder" }, StringComparer.Ordinal);
		private static readonly HashSet<string> RuntimeEvents = new HashSet<string>(new[] { "pleasure", "scaledPleasure", "libido", "strengthDamage", "struggleDamage", "healthDamage" }, StringComparer.Ordinal);

		public static NormalizedPlayerAnimationLoadResult Parse(string i_json, string i_packId, string i_source, string i_packRoot = null)
		{
			NormalizedPlayerAnimationLoadResult result = new NormalizedPlayerAnimationLoadResult();
			NormalizedPlayerAnimationDocument document;
			try { document = JsonConvert.DeserializeObject<NormalizedPlayerAnimationDocument>(i_json, new JsonSerializerSettings { MissingMemberHandling = MissingMemberHandling.Error }); }
			catch (JsonException exception) { Error(result, "json", exception.Message, i_source); return result; }
			if (document == null) { Error(result, "null", "Player animation resolved to null.", i_source); return result; }
			if (document.SchemaVersion != 1) Error(result, "schema-version", "Unsupported schemaVersion.", i_source);
			if (document.Type != "playerAnimation") Error(result, "type", "Definition type must be 'playerAnimation'.", i_source);
			if (!ContentId.TryParse(document.Id, out ContentId id) || id.Namespace != i_packId || !id.Path.StartsWith("player-animation/", StringComparison.Ordinal))
				Error(result, "id", "ID must use the defining pack namespace and a player-animation/ path.", i_source);
			if (!ContentId.TryParse(document.Rig, out ContentId rig) || !rig.Path.StartsWith("player-rig/", StringComparison.Ordinal))
				Error(result, "rig", "rig must be a player-rig content ID.", i_source);
			if (rig.ToString() != "core:player-rig/alex") Error(result, "rig-unsupported", "Runtime v1 currently supports core:player-rig/alex.", i_source);
			if (string.IsNullOrWhiteSpace(document.DisplayName) || document.DisplayName.Length > 100) Error(result, "display-name", "displayName must contain 1..100 characters.", i_source);
			if (!Finite(document.DurationSeconds) || document.DurationSeconds <= 0f || document.DurationSeconds > 60f) Error(result, "duration", "Runtime finisher animations must last more than 0 and at most 60 seconds.", i_source);
			if (!Finite(document.FrameRate) || document.FrameRate <= 0f || document.FrameRate > 240f) Error(result, "frame-rate", "frameRate must be greater than 0 and at most 240.", i_source);
			if (document.Tracks == null || document.Tracks.Count == 0 || document.Tracks.Count > 1024) Error(result, "tracks", "tracks must contain 1..1024 entries.", i_source);

			HashSet<string> identities = new HashSet<string>(StringComparer.Ordinal);
			foreach (NormalizedNumericTrack track in document.Tracks ?? new List<NormalizedNumericTrack>())
			{
				string bone = track?.Target != null && track.Target.StartsWith("bone/", StringComparison.Ordinal) ? track.Target.Substring(5) : null;
				string sprite = track?.Target != null && track.Target.StartsWith("sprite/", StringComparison.Ordinal) ? track.Target.Substring(7) : null;
				if (track == null || (bone == null && sprite == null) || !Bones.Contains(bone ?? sprite)) Error(result, "track-target", "Track target is not a published player bone or sprite.", i_source);
				if (track == null || !Properties.Contains(track.Property)) Error(result, "track-property", "Track property is unsupported: " + (track?.Property ?? "<null>"), i_source);
				if (track != null && !identities.Add(track.Target + "|" + track.Property)) Error(result, "track-duplicate", "Duplicate target/property track: " + track.Target + " | " + track.Property, i_source);
				if (track?.Keys == null || track.Keys.Count == 0) { Error(result, "track-keys", "Every track requires at least one key.", i_source); continue; }
				float previous = -1f;
				foreach (NormalizedNumericKey key in track.Keys)
				{
					if (key == null || !Finite(key.Time) || !Finite(key.Value) || key.Time < previous || key.Time < 0f || key.Time > document.DurationSeconds)
						Error(result, "track-key", "Track keys must be finite, ordered, and inside the animation duration.", i_source);
					if (key != null) previous = key.Time;
				}
			}
			foreach (NormalizedObjectTrack track in document.ObjectTracks ?? new List<NormalizedObjectTrack>())
			{
				string sprite = track?.Target != null && track.Target.StartsWith("sprite/", StringComparison.Ordinal) ? track.Target.Substring(7) : null;
				if (track == null || !Bones.Contains(sprite) || track.Property != "sprite") Error(result, "object-track", "Object tracks must target a published player sprite and use the sprite property.", i_source);
				float previous = -1f;
				foreach (NormalizedObjectKey key in track?.Keys ?? new List<NormalizedObjectKey>())
				{
					if (key == null || !Finite(key.Time) || key.Time < previous || key.Time < 0f || key.Time > document.DurationSeconds || (key.Asset == null && key.Name == null))
						Error(result, "object-key", "Sprite keys must be ordered inside the duration and identify an asset or sprite name.", i_source);
					if (key != null) previous = key.Time;
				}
			}

			float previousEvent = -1f;
			foreach (NormalizedPlayerEvent animationEvent in document.Events ?? new List<NormalizedPlayerEvent>())
			{
				if (animationEvent == null || !RuntimeEvents.Contains(animationEvent.Name)) Error(result, "event-name", "Runtime player events must use the published safe event names.", i_source);
				else if (!Finite(animationEvent.Time) || animationEvent.Time < previousEvent || animationEvent.Time < 0f || animationEvent.Time > document.DurationSeconds)
					Error(result, "event-time", "Events must be ordered inside the animation duration.", i_source);
				else if (!Finite(animationEvent.FloatValue) || animationEvent.FloatValue <= 0f) Error(result, "event-amount", "Runtime player events require a positive floatValue amount.", i_source);
				if (animationEvent != null) previousEvent = animationEvent.Time;
			}
			ValidatePresentation(document, result, i_source);
			if (result.Report.IsValid)
			{
				result.Definition = new NormalizedPlayerAnimationDefinition(id, rig, i_packId, i_source, document);
				if (!string.IsNullOrEmpty(i_packRoot)) result.Definition.LoadRuntimeSprites(i_packRoot, result.Report);
			}
			return result;
		}

		private static void ValidatePresentation(NormalizedPlayerAnimationDocument i_document, NormalizedPlayerAnimationLoadResult io_result, string i_source)
		{
			if ((i_document.Effects?.Count ?? 0) > 128 || (i_document.EffectTriggers?.Count ?? 0) > 512) Error(io_result, "presentation-limit", "At most 128 effects and 512 effect triggers are supported.", i_source);
			HashSet<string> ids = new HashSet<string>(StringComparer.Ordinal);
			foreach (NormalizedEffectDefinition effect in i_document.Effects ?? new List<NormalizedEffectDefinition>())
			{
				if (effect == null || string.IsNullOrWhiteSpace(effect.Id) || !ids.Add(effect.Id)) { Error(io_result, "effect-id", "Effect IDs must be non-empty and unique.", i_source); continue; }
				if (string.IsNullOrEmpty(effect.Target) || (!effect.Target.StartsWith("bone/", StringComparison.Ordinal) && !effect.Target.StartsWith("enemy-bone/", StringComparison.Ordinal))) Error(io_result, "effect-target", "Effects must target a player or enemy bone.", i_source);
				if (!Finite(effect.DurationSeconds) || effect.DurationSeconds < 0f || effect.DurationSeconds > 600f || effect.MaxParticles < 0 || effect.MaxParticles > 100000) Error(io_result, "effect-bounds", "Effect duration or particle count is outside the supported range.", i_source);
				string kind = string.IsNullOrWhiteSpace(effect.Kind) ? "particle" : effect.Kind;
				if (kind != "particle" && kind != "light" && kind != "trail" && kind != "decal") Error(io_result, "effect-kind", "Effect kind must be particle, light, trail, or decal.", i_source);
				if (!string.IsNullOrEmpty(effect.Texture) && (!ModPath.IsSafeRelativePath(effect.Texture) || !effect.Texture.EndsWith(".png", StringComparison.OrdinalIgnoreCase))) Error(io_result, "effect-texture", "Effect texture must be a safe pack-relative PNG path.", i_source);
				if (effect.TextureSheetTilesX < 1 || effect.TextureSheetTilesX > 64 || effect.TextureSheetTilesY < 1 || effect.TextureSheetTilesY > 64
					|| effect.TextureSheetFrame < 0 || effect.TextureSheetFrame >= effect.TextureSheetTilesX * effect.TextureSheetTilesY) Error(io_result, "effect-texture-sheet", "Texture sheet dimensions must be 1..64 and the selected frame must be inside the sheet.", i_source);
				if (!Finite(effect.LightIntensity) || effect.LightIntensity < 0f || effect.LightIntensity > 20f || !Finite(effect.LightRadius) || effect.LightRadius <= 0f || effect.LightRadius > 100f) Error(io_result, "effect-light", "Light intensity must be 0..20 and radius greater than 0 and at most 100.", i_source);
				if (!Finite(effect.TrailWidth) || effect.TrailWidth <= 0f || effect.TrailWidth > 20f || !Finite(effect.TrailTime) || effect.TrailTime <= 0f || effect.TrailTime > 30f) Error(io_result, "effect-trail", "Trail width must be greater than 0 and at most 20; trail time must be greater than 0 and at most 30.", i_source);
				if (!Finite(effect.DecalPixelsPerUnit) || effect.DecalPixelsPerUnit < 1f || effect.DecalPixelsPerUnit > 1024f) Error(io_result, "effect-decal", "Decal pixelsPerUnit must be between 1 and 1024.", i_source);
				if (kind == "decal" && string.IsNullOrEmpty(effect.Texture)) Error(io_result, "effect-decal-texture", "Decal effects require a pack-local texture.", i_source);
				if (!NormalizedParticlePresentationValidator.TryValidate(effect, out string advancedError)) Error(io_result, "effect-particle-modules", advancedError, i_source);
			}
			float previous = -1f;
			foreach (NormalizedEffectTrigger trigger in i_document.EffectTriggers ?? new List<NormalizedEffectTrigger>())
			{
				if (trigger == null || !Finite(trigger.Time) || trigger.Time < previous || trigger.Time < 0f || trigger.Time > i_document.DurationSeconds || trigger.Effects == null || trigger.Effects.Count == 0) { Error(io_result, "effect-trigger", "Effect triggers must be ordered inside the clip and reference at least one effect.", i_source); continue; }
				foreach (string id in trigger.Effects) if (!ids.Contains(id)) Error(io_result, "effect-reference", "Effect trigger references an unknown effect: " + id, i_source);
				previous = trigger.Time;
			}
			if ((i_document.ObjectTracks?.Count ?? 0) > 0 || (i_document.Effects?.Count ?? 0) > 0 || (i_document.EffectTriggers?.Count ?? 0) > 0)
				io_result.Report.Add(ValidationSeverity.Warning, "player-animation.presentation-experimental", "Normalized sprite, color, sorting, and particle presentation is experimental and is now applied by the runtime finisher importer.", i_source);
		}

		private static bool Finite(float i_value) { return !float.IsNaN(i_value) && !float.IsInfinity(i_value); }
		private static void Error(NormalizedPlayerAnimationLoadResult io_result, string i_code, string i_message, string i_source)
		{
			io_result.Report.Add(ValidationSeverity.Error, "player-animation." + i_code, i_message, i_source);
		}
	}

	public static class NormalizedPlayerAnimationRegistry
	{
		private static readonly Dictionary<ContentId, NormalizedPlayerAnimationDefinition> Definitions = new Dictionary<ContentId, NormalizedPlayerAnimationDefinition>();
		public static void Initialize(IEnumerable<NormalizedPlayerAnimationDefinition> i_definitions, ValidationReport io_report)
		{
			Definitions.Clear();
			foreach (NormalizedPlayerAnimationDefinition definition in i_definitions ?? new NormalizedPlayerAnimationDefinition[0])
				if (!Definitions.ContainsKey(definition.Id)) Definitions.Add(definition.Id, definition);
				else io_report?.Add(ValidationSeverity.Error, "player-animation.duplicate-id", "Duplicate player animation ID: " + definition.Id, definition.Source);
		}
		public static bool TryGet(ContentId i_id, out NormalizedPlayerAnimationDefinition o_definition) { return Definitions.TryGetValue(i_id, out o_definition); }
	}
}
