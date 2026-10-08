using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace CaptivityReloaded.Modding
{
	[JsonObject(MemberSerialization.OptIn)]
	public sealed class NormalizedEnemyAnimationDocument
	{
		[JsonProperty("schemaVersion", Required = Required.Always)] public int SchemaVersion { get; set; }
		[JsonProperty("type", Required = Required.Always)] public string Type { get; set; }
		[JsonProperty("id", Required = Required.Always)] public string Id { get; set; }
		[JsonProperty("enemy", Required = Required.Always)] public string Enemy { get; set; }
		[JsonProperty("displayName", Required = Required.Always)] public string DisplayName { get; set; }
		[JsonProperty("durationSeconds", Required = Required.Always)] public float DurationSeconds { get; set; }
		[JsonProperty("frameRate", Required = Required.Always)] public float FrameRate { get; set; }
		[JsonProperty("loop", Required = Required.Always)] public bool Loop { get; set; }
		[JsonProperty("tracks", Required = Required.Always)] public List<NormalizedNumericTrack> Tracks { get; set; } = new List<NormalizedNumericTrack>();
		[JsonProperty("objectTracks")] public List<NormalizedObjectTrack> ObjectTracks { get; set; } = new List<NormalizedObjectTrack>();
		[JsonProperty("events")] public List<EnemyAnimationEventDefinition> Events { get; set; } = new List<EnemyAnimationEventDefinition>();
		[JsonProperty("effects")] public List<NormalizedEffectDefinition> Effects { get; set; } = new List<NormalizedEffectDefinition>();
		[JsonProperty("effectTriggers")] public List<NormalizedEffectTrigger> EffectTriggers { get; set; } = new List<NormalizedEffectTrigger>();
		[JsonProperty("warnings")] public List<string> Warnings { get; set; } = new List<string>();
		[JsonProperty("source")] public JObject Source { get; set; }
	}

	public sealed class NormalizedEnemyAnimationDefinition
	{
		public ContentId Id { get; }
		public ContentId Enemy { get; }
		public string PackId { get; }
		public string Source { get; }
		private readonly NormalizedEnemyAnimationDocument m_document;
		private readonly Dictionary<string, Sprite> m_runtimeSprites = new Dictionary<string, Sprite>(StringComparer.Ordinal);

		internal NormalizedEnemyAnimationDefinition(ContentId i_id, ContentId i_enemy, string i_packId,
			string i_source, NormalizedEnemyAnimationDocument i_document)
		{
			Id = i_id; Enemy = i_enemy; PackId = i_packId; Source = i_source; m_document = i_document;
		}

		internal void LoadRuntimeSprites(string i_packRoot, ValidationReport io_report)
		{
			foreach (NormalizedObjectTrack track in m_document.ObjectTracks ?? new List<NormalizedObjectTrack>())
				foreach (NormalizedObjectKey key in track?.Keys ?? new List<NormalizedObjectKey>())
				{
					if (string.IsNullOrEmpty(key.Asset)) continue;
					if (!key.Asset.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
					{
						io_report.Add(ValidationSeverity.Error, "enemy-animation.sprite-file", "Animation sprite assets must be PNG files.", Source);
						continue;
					}
					if (!RuntimePngAssetLoader.TryLoad(i_packRoot, key.Asset, Id + "/" + (key.Name ?? "sprite"), FilterMode.Point,
						io_report, "enemy-animation.sprite-file", "enemy-animation.sprite-decode", Source, out Texture2D texture)) continue;
					Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 32f, 0, SpriteMeshType.FullRect);
					sprite.name = key.Name ?? Id + "/sprite";
					m_runtimeSprites[key.Asset] = sprite;
				}
			NormalizedPlayerAnimationDefinition.LoadEffectTextures(m_document.Effects, i_packRoot, io_report,
				Source, Id.ToString(), "enemy-animation");
		}

		public EnemyAnimationClipDefinition CreateClip(ISet<string> i_bones, ISet<string> i_regions, ValidationReport io_report)
		{
			foreach (NormalizedNumericTrack track in m_document.Tracks)
			{
				int slash = track.Target.IndexOf('/');
				string bone = slash < 0 ? string.Empty : track.Target.Substring(slash + 1);
				if (!i_bones.Contains(bone)) io_report.Add(ValidationSeverity.Error, "enemy-animation.bone-missing", "Animation " + Id + " references an unknown bone: " + bone, Source);
			}
			foreach (NormalizedObjectTrack track in m_document.ObjectTracks ?? new List<NormalizedObjectTrack>())
			{
				string bone = track?.Target != null && track.Target.StartsWith("sprite/", StringComparison.Ordinal) ? track.Target.Substring(7) : string.Empty;
				if (!i_bones.Contains(bone)) io_report.Add(ValidationSeverity.Error, "enemy-animation.bone-missing", "Animation " + Id + " references an unknown sprite bone: " + bone, Source);
				foreach (NormalizedObjectKey key in track?.Keys ?? new List<NormalizedObjectKey>())
					if (string.IsNullOrEmpty(key.Asset) && !i_regions.Contains(key.Name)) io_report.Add(ValidationSeverity.Error, "enemy-animation.region-missing", "Animation " + Id + " references an unknown atlas region: " + key.Name, Source);
			}
			foreach (NormalizedEffectDefinition effect in m_document.Effects ?? new List<NormalizedEffectDefinition>())
			{
				string prefix = effect?.Target != null && effect.Target.StartsWith("enemy-bone/", StringComparison.Ordinal) ? "enemy-bone/" : "bone/";
				string bone = effect?.Target != null && effect.Target.StartsWith(prefix, StringComparison.Ordinal) ? effect.Target.Substring(prefix.Length) : string.Empty;
				if (!i_bones.Contains(bone)) io_report.Add(ValidationSeverity.Error, "enemy-animation.effect-bone-missing", "Animation " + Id + " attaches an effect to an unknown bone: " + bone, Source);
			}
			List<NormalizedNumericTrack> tracks = m_document.Tracks.Where(track => IsRuntimeTrack(track, i_bones)).ToList();
			SortedSet<float> times = new SortedSet<float> { 0f, m_document.DurationSeconds };
			foreach (NormalizedNumericTrack track in tracks) foreach (NormalizedNumericKey key in track.Keys) times.Add(key.Time);
			foreach (NormalizedObjectTrack track in m_document.ObjectTracks ?? new List<NormalizedObjectTrack>())
				foreach (NormalizedObjectKey key in track?.Keys ?? new List<NormalizedObjectKey>()) times.Add(key.Time);
			if (times.Count > 256)
			{
				int sourceTimeCount = times.Count;
				io_report.Add(ValidationSeverity.Warning, "enemy-animation.sample-limit",
					"Animation " + Id + " has " + sourceTimeCount + " distinct key times. Runtime v1 approximates it with 256 uniform samples; duration is preserved but rapid changes may lose fidelity.", Source);
				times.Clear();
				for (int index = 0; index < 256; index++) times.Add(m_document.DurationSeconds * index / 255f);
			}
			List<EnemyAnimationFrameDefinition> frames = new List<EnemyAnimationFrameDefinition>();
			foreach (float time in times)
			{
				Dictionary<string, EnemyBonePoseDefinition> poses = new Dictionary<string, EnemyBonePoseDefinition>(StringComparer.Ordinal);
				foreach (NormalizedNumericTrack track in tracks)
				{
					string bone = track.Target.Substring(track.Target.IndexOf('/') + 1);
					if (!poses.TryGetValue(bone, out EnemyBonePoseDefinition pose)) poses.Add(bone, pose = new EnemyBonePoseDefinition());
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
					string bone = track.Target.Substring(7);
					if (!i_bones.Contains(bone)) continue;
					NormalizedObjectKey selected = null;
					foreach (NormalizedObjectKey key in track.Keys ?? new List<NormalizedObjectKey>()) { if (key.Time <= time) selected = key; else break; }
					if (selected == null) continue;
					if (!poses.TryGetValue(bone, out EnemyBonePoseDefinition pose)) poses.Add(bone, pose = new EnemyBonePoseDefinition());
					pose.Region = selected.Name;
					pose.RuntimeSpriteName = selected.Name;
					if (!string.IsNullOrEmpty(selected.Asset) && m_runtimeSprites.TryGetValue(selected.Asset, out Sprite sprite)) pose.RuntimeSprite = sprite;
				}
				frames.Add(new EnemyAnimationFrameDefinition { Time = time, Bones = poses });
			}
			return new EnemyAnimationClipDefinition
			{
				DurationSeconds = m_document.DurationSeconds, Loop = m_document.Loop, Frames = frames,
				Events = m_document.Events ?? new List<EnemyAnimationEventDefinition>(), NormalizedPresentation = new NormalizedAnimationPresentation(new NormalizedPlayerAnimationDocument
				{
					Tracks = m_document.Tracks, ObjectTracks = m_document.ObjectTracks,
					Effects = m_document.Effects, EffectTriggers = m_document.EffectTriggers
				})
			};
		}

		private static bool IsRuntimeTrack(NormalizedNumericTrack i_track, ISet<string> i_bones)
		{
			if (i_track == null || string.IsNullOrEmpty(i_track.Target)) return false;
			int slash = i_track.Target.IndexOf('/');
			if (slash < 0 || !i_bones.Contains(i_track.Target.Substring(slash + 1))) return false;
			return (i_track.Target.StartsWith("bone/", StringComparison.Ordinal) &&
				(i_track.Property == "position.x" || i_track.Property == "position.y" || i_track.Property == "rotation.z" || i_track.Property == "scale.x" || i_track.Property == "scale.y"))
				|| (i_track.Target.StartsWith("sprite/", StringComparison.Ordinal) &&
				(i_track.Property == "color.r" || i_track.Property == "color.g" || i_track.Property == "color.b" || i_track.Property == "color.a" || i_track.Property == "sortingOrder"));
		}

		private static float Evaluate(IReadOnlyList<NormalizedNumericKey> i_keys, float i_time)
		{
			if (i_keys.Count == 1 || i_time <= i_keys[0].Time) return i_keys[0].Value;
			if (i_time >= i_keys[i_keys.Count - 1].Time) return i_keys[i_keys.Count - 1].Value;
			int upper = 1; while (upper < i_keys.Count && i_keys[upper].Time < i_time) upper++;
			NormalizedNumericKey left = i_keys[upper - 1], right = i_keys[upper];
			float duration = right.Time - left.Time, t = duration <= 0f ? 1f : (i_time - left.Time) / duration;
			if (!left.OutTangent.HasValue || !right.InTangent.HasValue) return left.Value + (right.Value - left.Value) * t;
			float t2 = t * t, t3 = t2 * t;
			return (2f * t3 - 3f * t2 + 1f) * left.Value + (t3 - 2f * t2 + t) * duration * left.OutTangent.Value
				+ (-2f * t3 + 3f * t2) * right.Value + (t3 - t2) * duration * right.InTangent.Value;
		}
	}

	public sealed class NormalizedEnemyAnimationLoadResult
	{
		public NormalizedEnemyAnimationDefinition Definition { get; internal set; }
		public ValidationReport Report { get; } = new ValidationReport();
	}

	public static class NormalizedEnemyAnimationParser
	{
		private static readonly HashSet<string> Properties = new HashSet<string>(new[] { "position.x", "position.y", "rotation.z", "scale.x", "scale.y", "color.r", "color.g", "color.b", "color.a", "sortingOrder" }, StringComparer.Ordinal);
		public static NormalizedEnemyAnimationLoadResult Parse(string i_json, string i_packId, string i_source, string i_packRoot = null)
		{
			NormalizedEnemyAnimationLoadResult result = new NormalizedEnemyAnimationLoadResult();
			NormalizedEnemyAnimationDocument document;
			try { document = JsonConvert.DeserializeObject<NormalizedEnemyAnimationDocument>(i_json, new JsonSerializerSettings { MissingMemberHandling = MissingMemberHandling.Error }); }
			catch (JsonException exception) { Error(result, "json", exception.Message, i_source); return result; }
			if (document == null) { Error(result, "null", "Enemy animation resolved to null.", i_source); return result; }
			if (document.SchemaVersion != 1) Error(result, "schema-version", "Unsupported schemaVersion.", i_source);
			if (document.Type != "enemyAnimation") Error(result, "type", "Definition type must be 'enemyAnimation'.", i_source);
			if (!ContentId.TryParse(document.Id, out ContentId id) || id.Namespace != i_packId || !id.Path.StartsWith("enemy-animation/", StringComparison.Ordinal)) Error(result, "id", "ID must use the defining pack namespace and an enemy-animation/ path.", i_source);
			if (!ContentId.TryParse(document.Enemy, out ContentId enemy) || enemy.Namespace != i_packId || !enemy.Path.StartsWith("enemy/", StringComparison.Ordinal)) Error(result, "enemy", "enemy must reference an enemy in the defining pack.", i_source);
			if (string.IsNullOrWhiteSpace(document.DisplayName) || document.DisplayName.Length > 100) Error(result, "display-name", "displayName must contain 1..100 characters.", i_source);
			if (!Finite(document.DurationSeconds) || document.DurationSeconds <= 0f || document.DurationSeconds > 60f) Error(result, "duration", "durationSeconds must be greater than 0 and at most 60.", i_source);
			if (!Finite(document.FrameRate) || document.FrameRate <= 0f || document.FrameRate > 240f) Error(result, "frame-rate", "frameRate must be greater than 0 and at most 240.", i_source);
			if (document.Tracks == null || document.Tracks.Count == 0 || document.Tracks.Count > 1024) Error(result, "tracks", "tracks must contain 1..1024 entries.", i_source);
			HashSet<string> identities = new HashSet<string>(StringComparer.Ordinal);
			foreach (NormalizedNumericTrack track in document.Tracks ?? new List<NormalizedNumericTrack>())
			{
				if (track == null || (!track.Target.StartsWith("bone/", StringComparison.Ordinal) && !track.Target.StartsWith("sprite/", StringComparison.Ordinal)) || track.Target.IndexOf('/') == track.Target.Length - 1) Error(result, "track-target", "Track target must name an enemy bone or sprite.", i_source);
				if (track == null || !Properties.Contains(track.Property)) Error(result, "track-property", "Unsupported track property.", i_source);
				if (track != null && !identities.Add(track.Target + "|" + track.Property)) Error(result, "track-duplicate", "Duplicate target/property track.", i_source);
				ValidateKeys(track?.Keys, document.DurationSeconds, result, i_source);
			}
			foreach (NormalizedObjectTrack track in document.ObjectTracks ?? new List<NormalizedObjectTrack>())
			{
				if (track == null || track.Property != "sprite" || !track.Target.StartsWith("sprite/", StringComparison.Ordinal)) Error(result, "object-track", "Object tracks must target an enemy sprite and use the sprite property.", i_source);
				float previous = -1f;
				foreach (NormalizedObjectKey key in track?.Keys ?? new List<NormalizedObjectKey>())
				{
					if (!Finite(key.Time) || key.Time < previous || key.Time < 0f || key.Time > document.DurationSeconds || string.IsNullOrWhiteSpace(key.Name)) Error(result, "object-key", "Sprite keys require ordered times and an atlas-region name.", i_source);
					previous = key.Time;
				}
			}
			ValidateEvents(document, result, i_source);
			ValidateEffects(document, result, i_source);
			if (result.Report.IsValid)
			{
				result.Definition = new NormalizedEnemyAnimationDefinition(id, enemy, i_packId, i_source, document);
				if (!string.IsNullOrEmpty(i_packRoot)) result.Definition.LoadRuntimeSprites(i_packRoot, result.Report);
			}
			return result;
		}

		private static void ValidateEvents(NormalizedEnemyAnimationDocument i_document, NormalizedEnemyAnimationLoadResult io_result, string i_source)
		{
			HashSet<string> allowed = new HashSet<string>(new[] { "attackHit", "impulse", "sound", "cameraShake", "spriteEffect", "cue" }, StringComparer.Ordinal);
			if (i_document.Loop && (i_document.Events?.Any(animationEvent => animationEvent != null && animationEvent.Type != "cue") ?? false))
				Error(io_result, "event-loop", "Looping enemy animations cannot contain gameplay events. Semantic cues are allowed.", i_source);
			float previous = -1f;
			foreach (EnemyAnimationEventDefinition animationEvent in i_document.Events ?? new List<EnemyAnimationEventDefinition>())
			{
				if (animationEvent == null || !allowed.Contains(animationEvent.Type)) { Error(io_result, "event-type", "Enemy animation events must use the published safe event names.", i_source); continue; }
				if (!Finite(animationEvent.Time) || animationEvent.Time < previous || animationEvent.Time < 0f || animationEvent.Time > i_document.DurationSeconds) Error(io_result, "event-time", "Events must be ordered inside the animation duration.", i_source);
				ValidateEventFields(animationEvent, io_result, i_source);
				previous = animationEvent.Time;
			}
		}

		private static void ValidateEventFields(EnemyAnimationEventDefinition i_event, NormalizedEnemyAnimationLoadResult io_result, string i_source)
		{
			bool transformFields = i_event.X.HasValue || i_event.Y.HasValue || i_event.RelativeToFacing.HasValue;
			bool audioFields = i_event.File != null || i_event.Volume.HasValue;
			bool effectFields = i_event.Region != null || i_event.Bone != null || i_event.DurationSeconds.HasValue || i_event.Scale.HasValue || i_event.SortingOrder.HasValue;
			bool cueFields = i_event.Cue != null || i_event.Index.HasValue;
			if (i_event.Type == "attackHit")
			{
				if (i_event.Amount.HasValue || transformFields || audioFields || effectFields || cueFields) Error(io_result, "event-fields", "attackHit does not accept additional fields.", i_source);
			}
			else if (i_event.Type == "impulse")
			{
				if ((!i_event.X.HasValue && !i_event.Y.HasValue) || (i_event.X.HasValue && (!Finite(i_event.X.Value) || Mathf.Abs(i_event.X.Value) > 1000f)) || (i_event.Y.HasValue && (!Finite(i_event.Y.Value) || Mathf.Abs(i_event.Y.Value) > 1000f))) Error(io_result, "event-impulse", "impulse requires a finite x or y between -1000 and 1000.", i_source);
				if (i_event.Amount.HasValue || audioFields || effectFields || cueFields) Error(io_result, "event-fields", "impulse contains fields belonging to another event type.", i_source);
			}
			else if (i_event.Type == "sound")
			{
				if (!ModPath.IsSafeRelativePath(i_event.File) || (!i_event.File.EndsWith(".wav", StringComparison.OrdinalIgnoreCase) && !i_event.File.EndsWith(".ogg", StringComparison.OrdinalIgnoreCase))) Error(io_result, "event-sound", "Sound events require a safe pack-relative WAV or OGG file.", i_source);
				if (i_event.Volume.HasValue && (!Finite(i_event.Volume.Value) || i_event.Volume.Value < 0f || i_event.Volume.Value > 1f)) Error(io_result, "event-volume", "Sound volume must be between 0 and 1.", i_source);
				if (i_event.Amount.HasValue || transformFields || effectFields || cueFields) Error(io_result, "event-fields", "sound contains fields belonging to another event type.", i_source);
			}
			else if (i_event.Type == "cameraShake")
			{
				if (!i_event.Amount.HasValue || !Finite(i_event.Amount.Value) || i_event.Amount.Value < 0f || i_event.Amount.Value > 1f) Error(io_result, "event-camera-shake", "cameraShake requires an amount between 0 and 1.", i_source);
				if (transformFields || audioFields || effectFields || cueFields) Error(io_result, "event-fields", "cameraShake contains fields belonging to another event type.", i_source);
			}
			else if (i_event.Type == "spriteEffect")
			{
				if (string.IsNullOrWhiteSpace(i_event.Region)) Error(io_result, "event-region", "spriteEffect requires an atlas region.", i_source);
				if (i_event.DurationSeconds.HasValue && (!Finite(i_event.DurationSeconds.Value) || i_event.DurationSeconds.Value < 0.02f || i_event.DurationSeconds.Value > 30f)) Error(io_result, "event-duration", "spriteEffect duration must be between 0.02 and 30 seconds.", i_source);
				if (i_event.Scale.HasValue && (!Finite(i_event.Scale.Value) || i_event.Scale.Value < 0.01f || i_event.Scale.Value > 100f)) Error(io_result, "event-scale", "spriteEffect scale must be between 0.01 and 100.", i_source);
				if (i_event.Amount.HasValue || i_event.RelativeToFacing.HasValue || audioFields || cueFields) Error(io_result, "event-fields", "spriteEffect contains fields belonging to another event type.", i_source);
			}
			else if (i_event.Type == "cue")
			{
				if (!IsSemanticCue(i_event.Cue)) Error(io_result, "event-cue", "Cue events require a lowercase semantic cue name of at most 80 characters.", i_source);
				if (i_event.Index.HasValue && (i_event.Index.Value < 0 || i_event.Index.Value > 255)) Error(io_result, "event-cue-index", "Cue index must be between 0 and 255.", i_source);
				if (i_event.Amount.HasValue && (!Finite(i_event.Amount.Value) || i_event.Amount.Value < -100000f || i_event.Amount.Value > 100000f)) Error(io_result, "event-cue-amount", "Cue amount must be finite and between -100000 and 100000.", i_source);
				if (transformFields || audioFields || effectFields) Error(io_result, "event-fields", "cue accepts only cue, index, and amount metadata.", i_source);
			}
		}

		private static bool IsSemanticCue(string i_value)
		{
			if (string.IsNullOrWhiteSpace(i_value) || i_value.Length > 80 || !IsCueCharacter(i_value[0], true)) return false;
			for (int index = 1; index < i_value.Length; index++) if (!IsCueCharacter(i_value[index], false)) return false;
			return true;
		}

		private static bool IsCueCharacter(char i_value, bool i_first)
		{
			bool alphanumeric = (i_value >= 'a' && i_value <= 'z') || (i_value >= '0' && i_value <= '9');
			return alphanumeric || (!i_first && (i_value == '.' || i_value == '-' || i_value == '_'));
		}

		private static void ValidateEffects(NormalizedEnemyAnimationDocument i_document, NormalizedEnemyAnimationLoadResult io_result, string i_source)
		{
			if ((i_document.Effects?.Count ?? 0) > 128 || (i_document.EffectTriggers?.Count ?? 0) > 512) Error(io_result, "effects-limit", "At most 128 effects and 512 triggers are supported.", i_source);
			HashSet<string> ids = new HashSet<string>(StringComparer.Ordinal);
			foreach (NormalizedEffectDefinition effect in i_document.Effects ?? new List<NormalizedEffectDefinition>())
			{
				if (effect == null || string.IsNullOrWhiteSpace(effect.Id) || !ids.Add(effect.Id)) { Error(io_result, "effect-id", "Effect IDs must be non-empty and unique.", i_source); continue; }
				if (string.IsNullOrEmpty(effect.Target) || (!effect.Target.StartsWith("bone/", StringComparison.Ordinal) && !effect.Target.StartsWith("enemy-bone/", StringComparison.Ordinal))) Error(io_result, "effect-target", "Effects must target an enemy bone.", i_source);
				if (!Finite(effect.DurationSeconds) || effect.DurationSeconds < 0f || effect.DurationSeconds > 600f || effect.MaxParticles < 0 || effect.MaxParticles > 100000) Error(io_result, "effect-bounds", "Effect duration or particle count is outside the supported range.", i_source);
				ValidateEffectPresentation(effect, io_result, i_source);
			}
			float previous = -1f;
			foreach (NormalizedEffectTrigger trigger in i_document.EffectTriggers ?? new List<NormalizedEffectTrigger>())
			{
				if (trigger == null || !Finite(trigger.Time) || trigger.Time < previous || trigger.Time < 0f || trigger.Time > i_document.DurationSeconds || trigger.Effects == null || trigger.Effects.Count == 0) { Error(io_result, "effect-trigger", "Effect triggers must be ordered inside the clip and reference at least one effect.", i_source); continue; }
				foreach (string id in trigger.Effects) if (!ids.Contains(id)) Error(io_result, "effect-reference", "Effect trigger references an unknown effect: " + id, i_source);
				previous = trigger.Time;
			}
		}

		private static void ValidateEffectPresentation(NormalizedEffectDefinition i_effect, NormalizedEnemyAnimationLoadResult io_result, string i_source)
		{
			string kind = string.IsNullOrWhiteSpace(i_effect.Kind) ? "particle" : i_effect.Kind;
			if (kind != "particle" && kind != "light" && kind != "trail" && kind != "decal") Error(io_result, "effect-kind", "Effect kind must be particle, light, trail, or decal.", i_source);
			if (!string.IsNullOrEmpty(i_effect.Texture) && (!ModPath.IsSafeRelativePath(i_effect.Texture) || !i_effect.Texture.EndsWith(".png", StringComparison.OrdinalIgnoreCase)))
				Error(io_result, "effect-texture", "Effect texture must be a safe pack-relative PNG path.", i_source);
			if (i_effect.TextureSheetTilesX < 1 || i_effect.TextureSheetTilesX > 64 || i_effect.TextureSheetTilesY < 1 || i_effect.TextureSheetTilesY > 64
				|| i_effect.TextureSheetFrame < 0 || i_effect.TextureSheetFrame >= i_effect.TextureSheetTilesX * i_effect.TextureSheetTilesY)
				Error(io_result, "effect-texture-sheet", "Texture sheet dimensions must be 1..64 and the selected frame must be inside the sheet.", i_source);
			if (!Finite(i_effect.LightIntensity) || i_effect.LightIntensity < 0f || i_effect.LightIntensity > 20f || !Finite(i_effect.LightRadius) || i_effect.LightRadius <= 0f || i_effect.LightRadius > 100f)
				Error(io_result, "effect-light", "Light intensity must be 0..20 and radius greater than 0 and at most 100.", i_source);
			if (!Finite(i_effect.TrailWidth) || i_effect.TrailWidth <= 0f || i_effect.TrailWidth > 20f || !Finite(i_effect.TrailTime) || i_effect.TrailTime <= 0f || i_effect.TrailTime > 30f)
				Error(io_result, "effect-trail", "Trail width must be greater than 0 and at most 20; trail time must be greater than 0 and at most 30.", i_source);
			if (!Finite(i_effect.DecalPixelsPerUnit) || i_effect.DecalPixelsPerUnit < 1f || i_effect.DecalPixelsPerUnit > 1024f)
				Error(io_result, "effect-decal", "Decal pixelsPerUnit must be between 1 and 1024.", i_source);
			if (kind == "decal" && string.IsNullOrEmpty(i_effect.Texture)) Error(io_result, "effect-decal-texture", "Decal effects require a pack-local texture.", i_source);
			if (!NormalizedParticlePresentationValidator.TryValidate(i_effect, out string advancedError)) Error(io_result, "effect-particle-modules", advancedError, i_source);
		}

		private static void ValidateKeys(List<NormalizedNumericKey> i_keys, float i_duration, NormalizedEnemyAnimationLoadResult io_result, string i_source)
		{
			if (i_keys == null || i_keys.Count == 0) { Error(io_result, "track-keys", "Every track requires at least one key.", i_source); return; }
			float previous = -1f;
			foreach (NormalizedNumericKey key in i_keys) { if (key == null || !Finite(key.Time) || !Finite(key.Value) || key.Time < previous || key.Time < 0f || key.Time > i_duration) Error(io_result, "track-key", "Track keys must be finite, ordered, and inside the duration.", i_source); if (key != null) previous = key.Time; }
		}
		private static bool Finite(float i_value) { return !float.IsNaN(i_value) && !float.IsInfinity(i_value); }
		private static void Error(NormalizedEnemyAnimationLoadResult io_result, string i_code, string i_message, string i_source) { io_result.Report.Add(ValidationSeverity.Error, "enemy-animation." + i_code, i_message, i_source); }
	}

	public static class NormalizedEnemyAnimationRegistry
	{
		private static readonly Dictionary<ContentId, NormalizedEnemyAnimationDefinition> Definitions = new Dictionary<ContentId, NormalizedEnemyAnimationDefinition>();
		private static readonly Dictionary<ContentId, List<NormalizedEnemyAnimationDefinition>> ByEnemy = new Dictionary<ContentId, List<NormalizedEnemyAnimationDefinition>>();
		public static void Initialize(IEnumerable<NormalizedEnemyAnimationDefinition> i_definitions, ValidationReport io_report)
		{
			Definitions.Clear(); ByEnemy.Clear();
			foreach (NormalizedEnemyAnimationDefinition definition in i_definitions ?? new NormalizedEnemyAnimationDefinition[0])
				if (!Definitions.ContainsKey(definition.Id))
				{
					Definitions.Add(definition.Id, definition);
					if (!ByEnemy.TryGetValue(definition.Enemy, out List<NormalizedEnemyAnimationDefinition> owned)) ByEnemy.Add(definition.Enemy, owned = new List<NormalizedEnemyAnimationDefinition>());
					owned.Add(definition);
				}
				else io_report.Add(ValidationSeverity.Error, "enemy-animation.duplicate-id", "Duplicate enemy animation ID: " + definition.Id, definition.Source);
			foreach (List<NormalizedEnemyAnimationDefinition> owned in ByEnemy.Values) owned.Sort((left, right) => left.Id.CompareTo(right.Id));
		}
		public static bool TryGet(ContentId i_id, out NormalizedEnemyAnimationDefinition o_definition) { return Definitions.TryGetValue(i_id, out o_definition); }
		public static IReadOnlyList<NormalizedEnemyAnimationDefinition> GetForEnemy(ContentId i_enemy)
		{
			return ByEnemy.TryGetValue(i_enemy, out List<NormalizedEnemyAnimationDefinition> owned) ? owned : (IReadOnlyList<NormalizedEnemyAnimationDefinition>)new NormalizedEnemyAnimationDefinition[0];
		}
	}
}
