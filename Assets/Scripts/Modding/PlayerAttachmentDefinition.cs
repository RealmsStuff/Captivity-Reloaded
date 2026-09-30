using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace CaptivityReloaded.Modding
{
	[JsonObject(MemberSerialization.OptIn)]
	public sealed class PlayerAttachmentDocument
	{
		[JsonProperty("schemaVersion", Required = Required.Always)] public int SchemaVersion { get; set; }
		[JsonProperty("type", Required = Required.Always)] public string Type { get; set; }
		[JsonProperty("id", Required = Required.Always)] public string Id { get; set; }
		[JsonProperty("sprite")] public string Sprite { get; set; }
		[JsonProperty("skinSprites")] public Dictionary<string, string> SkinSprites { get; set; } = new Dictionary<string, string>();
		[JsonProperty("pixelsPerUnit")] public float PixelsPerUnit { get; set; } = 32f;
		[JsonProperty("bone", Required = Required.Always)] public string Bone { get; set; }
		[JsonProperty("attachToBone")] public bool AttachToBone { get; set; } = true;
		[JsonProperty("offsetX")] public float OffsetX { get; set; }
		[JsonProperty("offsetY")] public float OffsetY { get; set; }
		[JsonProperty("rotation")] public float Rotation { get; set; }
		[JsonProperty("pivotX")] public float PivotX { get; set; } = 0.5f;
		[JsonProperty("pivotY")] public float PivotY { get; set; } = 0.5f;
		[JsonProperty("sortingOffset")] public int SortingOffset { get; set; } = 1;
		[JsonProperty("tintWithSkin")] public bool TintWithSkin { get; set; }
		[JsonProperty("physics")] public ClothingPhysicsDefinition Physics { get; set; }
		[JsonProperty("pregnancyGrowth")] public PlayerAttachmentPregnancyGrowthDefinition PregnancyGrowth { get; set; }
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class PlayerAttachmentPregnancyGrowthDefinition
	{
		[JsonProperty("transitionSeconds")] public float TransitionSeconds { get; set; } = 1.5f;
		[JsonProperty("breakSpineClothing")] public bool BreakSpineClothing { get; set; }
		[JsonProperty("clothingBreakDelaySeconds")] public float ClothingBreakDelaySeconds { get; set; } = 0.5f;
		[JsonProperty("stages", Required = Required.Always)] public List<PlayerAttachmentPregnancyStageDefinition> Stages { get; set; } = new List<PlayerAttachmentPregnancyStageDefinition>();
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class PlayerAttachmentPregnancyStageDefinition
	{
		[JsonProperty("minimumFetuses", Required = Required.Always)] public int MinimumFetuses { get; set; }
		[JsonProperty("offsetX", Required = Required.Always)] public float OffsetX { get; set; }
		[JsonProperty("offsetY", Required = Required.Always)] public float OffsetY { get; set; }
		[JsonProperty("scaleX", Required = Required.Always)] public float ScaleX { get; set; }
		[JsonProperty("scaleY", Required = Required.Always)] public float ScaleY { get; set; }
	}

	public sealed class PlayerAttachmentDefinition
	{
		public ContentId Id { get; }
		public string PackId { get; }
		public string Source { get; }
		public PlayerAttachmentDocument Document { get; }

		public PlayerAttachmentDefinition(ContentId i_id, string i_packId, string i_source, PlayerAttachmentDocument i_document)
		{
			Id = i_id; PackId = i_packId; Source = i_source; Document = i_document;
		}
	}

	public sealed class PlayerAttachmentLoadResult
	{
		public PlayerAttachmentDefinition Definition { get; internal set; }
		public ValidationReport Report { get; } = new ValidationReport();
	}

	public static class PlayerAttachmentParser
	{
		private static readonly HashSet<string> s_bones = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
		{
			"Hips", "Butt", "Spine", "Chest", "Neck", "Head", "rArmUpper", "rArmLower", "rHand",
			"lArmUpper", "lArmLower", "lHand", "rLegUpper", "rLegLower", "rFoot", "lLegUpper",
			"lLegLower", "lFoot", "Ear", "Face"
		};
		private static readonly HashSet<string> s_skins = new HashSet<string>(StringComparer.Ordinal)
		{
			"pale", "white", "tan", "black", "olive", "brown", "deep"
		};

		public static PlayerAttachmentLoadResult Parse(string i_json, string i_packId, string i_source)
		{
			PlayerAttachmentLoadResult result = new PlayerAttachmentLoadResult();
			PlayerAttachmentDocument document;
			try
			{
				document = JsonConvert.DeserializeObject<PlayerAttachmentDocument>(i_json,
					new JsonSerializerSettings { MissingMemberHandling = MissingMemberHandling.Error });
			}
			catch (JsonException exception)
			{
				Error(result, "json", exception.Message, i_source); return result;
			}
			if (document == null) { Error(result, "null", "Player attachment resolved to null.", i_source); return result; }
			if (document.SchemaVersion != 1) Error(result, "schema-version", "Unsupported schemaVersion " + document.SchemaVersion + ".", i_source);
			if (document.Type != "playerAttachment") Error(result, "type", "Definition type must be 'playerAttachment'.", i_source);
			if (!ContentId.TryParse(document.Id, out ContentId id) || id.Namespace != i_packId
				|| !id.Path.StartsWith("player-attachment/", StringComparison.Ordinal))
				Error(result, "id", "ID must use the defining pack namespace and a player-attachment/ path.", i_source);
			if (!s_bones.Contains(document.Bone ?? string.Empty)) Error(result, "bone", "Unknown player bone: " + (document.Bone ?? "<null>"), i_source);
			bool hasDefault = IsPng(document.Sprite);
			if (!string.IsNullOrWhiteSpace(document.Sprite) && !hasDefault) Error(result, "sprite", "sprite must be a safe pack-relative PNG path.", i_source);
			if (!hasDefault && (document.SkinSprites == null || document.SkinSprites.Count == 0))
				Error(result, "sprite-missing", "Provide sprite or at least one skinSprites entry.", i_source);
			foreach (KeyValuePair<string, string> sprite in document.SkinSprites ?? new Dictionary<string, string>())
			{
				if (!s_skins.Contains(sprite.Key)) Error(result, "skin", "Unknown supported skinSprites tone: " + sprite.Key, i_source);
				if (!IsPng(sprite.Value)) Error(result, "skin-sprite", "skinSprites values must be safe pack-relative PNG paths.", i_source);
			}
			if (!Finite(document.PixelsPerUnit) || document.PixelsPerUnit < 1f || document.PixelsPerUnit > 1024f
				|| !Finite(document.OffsetX) || Math.Abs(document.OffsetX) > 10f || !Finite(document.OffsetY) || Math.Abs(document.OffsetY) > 10f
				|| !Finite(document.Rotation) || Math.Abs(document.Rotation) > 360f || !Finite(document.PivotX) || document.PivotX < 0f || document.PivotX > 1f
				|| !Finite(document.PivotY) || document.PivotY < 0f || document.PivotY > 1f || document.SortingOffset < -100 || document.SortingOffset > 100)
				Error(result, "range", "Pixels-per-unit, transform, pivot, or sorting values are outside supported bounds.", i_source);
			ValidatePhysics(document.Physics, result, i_source);
			ValidatePregnancyGrowth(document.PregnancyGrowth, result, i_source);
			if (result.Report.IsValid) result.Definition = new PlayerAttachmentDefinition(id, i_packId, i_source, document);
			return result;
		}

		private static bool IsPng(string i_path) => ModPath.IsSafeRelativePath(i_path) && i_path.EndsWith(".png", StringComparison.OrdinalIgnoreCase);
		private static bool Finite(float i_value) => !float.IsNaN(i_value) && !float.IsInfinity(i_value);
		private static void ValidatePhysics(ClothingPhysicsDefinition i_physics, PlayerAttachmentLoadResult io_result, string i_source)
		{
			if (i_physics == null) return;
			if (i_physics.Mode != "sway") Error(io_result, "physics-mode", "physics.mode must be sway.", i_source);
			if ((i_physics.Spring.HasValue && (!Finite(i_physics.Spring.Value) || i_physics.Spring.Value < 0.1f || i_physics.Spring.Value > 200f))
				|| (i_physics.Damping.HasValue && (!Finite(i_physics.Damping.Value) || i_physics.Damping.Value < 0f || i_physics.Damping.Value > 50f))
				|| (i_physics.Gravity.HasValue && (!Finite(i_physics.Gravity.Value) || i_physics.Gravity.Value < 0f || i_physics.Gravity.Value > 1f))
				|| (i_physics.MotionInfluence.HasValue && (!Finite(i_physics.MotionInfluence.Value) || i_physics.MotionInfluence.Value < 0f || i_physics.MotionInfluence.Value > 10f))
				|| (i_physics.MaxAngle.HasValue && (!Finite(i_physics.MaxAngle.Value) || i_physics.MaxAngle.Value < 0f || i_physics.MaxAngle.Value > 90f)))
				Error(io_result, "physics-range", "Sway physics values are outside supported bounds.", i_source);
		}
		private static void ValidatePregnancyGrowth(PlayerAttachmentPregnancyGrowthDefinition i_growth,
			PlayerAttachmentLoadResult io_result, string i_source)
		{
			if (i_growth == null) return;
			if (!Finite(i_growth.TransitionSeconds) || i_growth.TransitionSeconds < 0.05f || i_growth.TransitionSeconds > 10f
				|| !Finite(i_growth.ClothingBreakDelaySeconds) || i_growth.ClothingBreakDelaySeconds < 0f || i_growth.ClothingBreakDelaySeconds > 10f)
				Error(io_result, "pregnancy-timing", "Pregnancy transition or clothing-break timing is outside supported bounds.", i_source);
			if (i_growth.Stages == null || i_growth.Stages.Count < 2 || i_growth.Stages.Count > 16)
			{
				Error(io_result, "pregnancy-stages", "pregnancyGrowth requires 2-16 stages.", i_source);
				return;
			}
			HashSet<int> minimums = new HashSet<int>();
			foreach (PlayerAttachmentPregnancyStageDefinition stage in i_growth.Stages)
			{
				if (stage == null || stage.MinimumFetuses < 0 || stage.MinimumFetuses > 64 || !minimums.Add(stage.MinimumFetuses)
					|| !Finite(stage.OffsetX) || Math.Abs(stage.OffsetX) > 10f || !Finite(stage.OffsetY) || Math.Abs(stage.OffsetY) > 10f
					|| !Finite(stage.ScaleX) || stage.ScaleX < 0f || stage.ScaleX > 10f
					|| !Finite(stage.ScaleY) || stage.ScaleY < 0f || stage.ScaleY > 10f)
					Error(io_result, "pregnancy-stage", "Pregnancy stages need unique fetus thresholds and bounded transforms.", i_source);
			}
			if (!minimums.Contains(0)) Error(io_result, "pregnancy-stage-zero", "pregnancyGrowth must include a stage with minimumFetuses 0.", i_source);
		}
		private static void Error(PlayerAttachmentLoadResult io_result, string i_code, string i_message, string i_source)
			=> io_result.Report.Add(ValidationSeverity.Error, "player-attachment." + i_code, i_message, i_source);
	}
}
