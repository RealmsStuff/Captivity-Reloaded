using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace CaptivityReloaded.Modding
{
	[JsonObject(MemberSerialization.OptIn)]
	public sealed class ClothingVisualDefinition
	{
		[JsonProperty("type", Required = Required.Always)]
		public string Type { get; set; }

		[JsonProperty("atlas")]
		public string Atlas { get; set; }

		[JsonProperty("sprites")]
		public Dictionary<string, string> Sprites { get; set; }

		[JsonProperty("pixelsPerUnit")]
		public float PixelsPerUnit { get; set; } = 32f;

		[JsonProperty("regions")]
		public Dictionary<string, AtlasRegionDefinition> Regions { get; set; }

		[JsonProperty("attachments")]
		public Dictionary<string, ClothingAttachmentDefinition> Attachments { get; set; }

		[JsonProperty("bodyVariants")]
		public Dictionary<string, Dictionary<string, string>> BodyVariants { get; set; }
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class ClothingPhysicsDefinition
	{
		[JsonProperty("mode")] public string Mode { get; set; }
		[JsonProperty("spring")] public float? Spring { get; set; }
		[JsonProperty("damping")] public float? Damping { get; set; }
		[JsonProperty("gravity")] public float? Gravity { get; set; }
		[JsonProperty("motionInfluence")] public float? MotionInfluence { get; set; }
		[JsonProperty("maxAngle")] public float? MaxAngle { get; set; }
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class ClothingAttachmentDefinition
	{
		[JsonProperty("bone")] public string Bone { get; set; }
		[JsonProperty("offsetX")] public float? OffsetX { get; set; }
		[JsonProperty("offsetY")] public float? OffsetY { get; set; }
		[JsonProperty("rotation")] public float? Rotation { get; set; }
		[JsonProperty("pivotX")] public float? PivotX { get; set; }
		[JsonProperty("pivotY")] public float? PivotY { get; set; }
		[JsonProperty("sortingOffset")] public int? SortingOffset { get; set; }
		[JsonProperty("attachToBone")] public bool? AttachToBone { get; set; }
		[JsonProperty("hideBodyPart")] public bool? HideBodyPart { get; set; }
		[JsonProperty("droppable")] public bool? Droppable { get; set; }
		[JsonProperty("destroyable")] public bool? Destroyable { get; set; }
		[JsonProperty("dropOnOralThrust")] public bool? DropOnOralThrust { get; set; }
		[JsonProperty("destroyOnOralThrust")] public bool? DestroyOnOralThrust { get; set; }
		[JsonProperty("physics")] public ClothingPhysicsDefinition Physics { get; set; }
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class ClothingEffectsDefinition
	{
		[JsonProperty("damageTakenMultiplier")] public float? DamageTakenMultiplier { get; set; }
		[JsonProperty("statModifiers")] public Dictionary<string, float> StatModifiers { get; set; }
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class ClothingDefinitionDocument
	{
		[JsonProperty("schemaVersion", Required = Required.Always)]
		public int SchemaVersion { get; set; }

		[JsonProperty("type", Required = Required.Always)]
		public string Type { get; set; }

		[JsonProperty("id", Required = Required.Always)]
		public string Id { get; set; }

		[JsonProperty("displayName", Required = Required.Always)]
		public string DisplayName { get; set; }

		[JsonProperty("extends")]
		public string Extends { get; set; }

		[JsonProperty("description")]
		public string Description { get; set; }

		[JsonProperty("unlockedByDefault")]
		public bool UnlockedByDefault { get; set; }

		[JsonProperty("category")]
		public string Category { get; set; }

		[JsonProperty("incompatibleCategories")]
		public List<string> IncompatibleCategories { get; set; } = new List<string>();

		[JsonProperty("visual", Required = Required.Always)]
		public ClothingVisualDefinition Visual { get; set; }

		[JsonProperty("effects")]
		public ClothingEffectsDefinition Effects { get; set; }
	}

	public sealed class ClothingDefinition
	{
		public ContentId Id { get; }
		public ContentId? Extends { get; }
		public bool IsOriginal => !Extends.HasValue;
		public string PackId { get; }
		public string Source { get; }
		public string DisplayName { get; }
		public string Description { get; }
		public ClothingVisualDefinition Visual { get; }
		public bool UnlockedByDefault { get; }
		public string Category { get; }
		public IReadOnlyList<string> IncompatibleCategories { get; }
		public ClothingEffectsDefinition Effects { get; }

		public ClothingDefinition(ContentId i_id, ContentId? i_extends, string i_packId, string i_source, ClothingDefinitionDocument i_document)
		{
			Id = i_id;
			Extends = i_extends;
			PackId = i_packId;
			Source = i_source;
			DisplayName = i_document.DisplayName;
			Description = i_document.Description ?? string.Empty;
			UnlockedByDefault = i_document.UnlockedByDefault;
			Category = i_document.Category;
			IncompatibleCategories = i_document.IncompatibleCategories ?? new List<string>();
			Visual = i_document.Visual;
			Effects = i_document.Effects ?? new ClothingEffectsDefinition();
		}
	}

	public sealed class ClothingDefinitionLoadResult
	{
		public ClothingDefinition Definition { get; internal set; }
		public ValidationReport Report { get; } = new ValidationReport();
	}

	public static class ClothingDefinitionParser
	{
		public const int SupportedSchemaVersion = 1;

		public static ClothingDefinitionLoadResult Parse(string i_json, string i_packId, string i_source)
		{
			ClothingDefinitionLoadResult result = new ClothingDefinitionLoadResult();
			ClothingDefinitionDocument document;
			try
			{
				document = JsonConvert.DeserializeObject<ClothingDefinitionDocument>(i_json, new JsonSerializerSettings
				{
					MissingMemberHandling = MissingMemberHandling.Error
				});
			}
			catch (JsonException exception)
			{
				result.Report.Add(ValidationSeverity.Error, "clothing.json", exception.Message, i_source);
				return result;
			}
			if (document == null)
			{
				result.Report.Add(ValidationSeverity.Error, "clothing.null", "Clothing definition resolved to null.", i_source);
				return result;
			}

			if (document.SchemaVersion != SupportedSchemaVersion) Error(result, "schema-version", "Unsupported schemaVersion " + document.SchemaVersion + ".", i_source);
			if (!string.Equals(document.Type, "clothing", StringComparison.Ordinal)) Error(result, "type", "Definition type must be 'clothing'.", i_source);
			if (!ContentId.TryParse(document.Id, out ContentId id) || id.Namespace != i_packId || !id.Path.StartsWith("clothing/", StringComparison.Ordinal))
				Error(result, "id", "Clothing ID must use the defining pack namespace and a clothing/ path.", i_source);
			ContentId? extends = null;
			if (!string.IsNullOrWhiteSpace(document.Extends))
			{
				if (!ContentId.TryParse(document.Extends, out ContentId parsedExtends) || parsedExtends.Namespace != "core"
					|| !parsedExtends.Path.StartsWith("clothing/", StringComparison.Ordinal) || parsedExtends == id)
					Error(result, "extends", "extends must reference a Core clothing content ID.", i_source);
				else extends = parsedExtends;
			}
			if (string.IsNullOrWhiteSpace(document.DisplayName)) Error(result, "display-name", "displayName is required.", i_source);
			bool original = document.Visual != null && string.Equals(document.Visual.Type, "originalClothingSprites", StringComparison.Ordinal);
			if (original && extends.HasValue) Error(result, "original-extends", "originalClothingSprites must not inherit Core clothing.", i_source);
			if (!original && !extends.HasValue) Error(result, "extends-required", "Inherited clothing requires extends.", i_source);
			if (original && string.IsNullOrWhiteSpace(document.Category)) Error(result, "original-category", "Fully original clothing requires category.", i_source);
			ValidateCategory(document.Category, "category", result.Report, i_source, i_optional: true);
			if (document.IncompatibleCategories == null) document.IncompatibleCategories = new List<string>();
			HashSet<string> categories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			foreach (string category in document.IncompatibleCategories)
			{
				ValidateCategory(category, "incompatible-category", result.Report, i_source, i_optional: false);
				if (!categories.Add(category ?? string.Empty)) Error(result, "incompatible-category", "incompatibleCategories cannot contain duplicates.", i_source);
			}
			ValidateVisual(document.Visual, original, result.Report, i_source);
			ValidateEffects(document.Effects, result.Report, i_source);
			if (result.Report.IsValid) result.Definition = new ClothingDefinition(id, extends, i_packId, i_source, document);
			return result;
		}

		private static void ValidateEffects(ClothingEffectsDefinition i_effects, ValidationReport io_report, string i_source)
		{
			if (i_effects == null) return;
			if (i_effects.DamageTakenMultiplier.HasValue && (!IsFiniteRange(i_effects.DamageTakenMultiplier.Value, 0.05f, 2f)))
				io_report.Add(ValidationSeverity.Error, "clothing.effects.damage-taken", "damageTakenMultiplier must be between 0.05 and 2.", i_source);
			HashSet<string> stats = new HashSet<string>(StringComparer.Ordinal) { "HealthMax", "SpeedAccel", "SpeedMax", "Traction", "DamageMultiplierGun", "SpeedSprint", "PowerJump", "PowerDash" };
			if (i_effects.StatModifiers != null && i_effects.StatModifiers.Count > 8)
				io_report.Add(ValidationSeverity.Error, "clothing.effects.stat-count", "statModifiers supports at most 8 entries.", i_source);
			foreach (KeyValuePair<string, float> modifier in i_effects.StatModifiers ?? new Dictionary<string, float>())
				if (!stats.Contains(modifier.Key) || !IsFiniteRange(modifier.Value, -10000f, 10000f))
					io_report.Add(ValidationSeverity.Error, "clothing.effects.stat", "statModifiers contains an unsupported stat or value: " + modifier.Key, i_source);
		}

		private static bool IsFiniteRange(float i_value, float i_min, float i_max)
		{
			return !float.IsNaN(i_value) && !float.IsInfinity(i_value) && i_value >= i_min && i_value <= i_max;
		}

		private static void ValidateCategory(string i_value, string i_field, ValidationReport io_report, string i_source, bool i_optional)
		{
			if (i_optional && string.IsNullOrWhiteSpace(i_value)) return;
			string[] valid = { "Hair", "Upper", "Lower", "Shoes", "Hat", "Sleeves", "Stockings", "Other" };
			foreach (string value in valid) if (string.Equals(value, i_value, StringComparison.OrdinalIgnoreCase)) return;
			io_report.Add(ValidationSeverity.Error, "clothing." + i_field, i_field + " must name a valid clothing category.", i_source);
		}

		private static void ValidateVisual(ClothingVisualDefinition i_visual, bool i_original, ValidationReport io_report, string i_source)
		{
			if (i_visual == null)
			{
				io_report.Add(ValidationSeverity.Error, "clothing.visual", "visual is required.", i_source);
				return;
			}
			bool atlasMode = string.Equals(i_visual.Type, "coreClothingAtlas", StringComparison.Ordinal);
			bool spriteMode = string.Equals(i_visual.Type, "coreClothingSprites", StringComparison.Ordinal);
			bool originalMode = string.Equals(i_visual.Type, "originalClothingSprites", StringComparison.Ordinal);
			if (!atlasMode && !spriteMode && !originalMode)
				io_report.Add(ValidationSeverity.Error, "clothing.visual.type", "visual.type must be coreClothingAtlas, coreClothingSprites, or originalClothingSprites.", i_source);
			if (atlasMode && (!ModPath.IsSafeRelativePath(i_visual.Atlas) || !i_visual.Atlas.EndsWith(".png", StringComparison.OrdinalIgnoreCase)))
				io_report.Add(ValidationSeverity.Error, "clothing.visual.atlas", "atlas must be a safe pack-relative PNG path.", i_source);
			if (float.IsNaN(i_visual.PixelsPerUnit) || float.IsInfinity(i_visual.PixelsPerUnit) || i_visual.PixelsPerUnit <= 0f || i_visual.PixelsPerUnit > 1024f)
				io_report.Add(ValidationSeverity.Error, "clothing.visual.pixels-per-unit", "pixelsPerUnit must be greater than 0 and at most 1024.", i_source);
			if (atlasMode && (i_visual.Regions == null || i_visual.Regions.Count == 0))
			{
				io_report.Add(ValidationSeverity.Error, "clothing.visual.regions", "At least one atlas region is required.", i_source);
				return;
			}
			foreach (KeyValuePair<string, AtlasRegionDefinition> region in i_visual.Regions ?? new Dictionary<string, AtlasRegionDefinition>())
			{
				if (!ClothingSlotCatalog.IsPublished(region.Key))
					io_report.Add(ValidationSeverity.Error, "clothing.visual.region-name", "Region is not a published V1 clothing slot: " + region.Key, i_source);
				if (region.Value == null || region.Value.X < 0 || region.Value.Y < 0 || region.Value.Width <= 0 || region.Value.Height <= 0)
					io_report.Add(ValidationSeverity.Error, "clothing.visual.region-rect", "Region rectangle must have a non-negative origin and positive size: " + region.Key, i_source);
			}
			if ((spriteMode || originalMode) && (i_visual.Sprites == null || i_visual.Sprites.Count == 0))
				io_report.Add(ValidationSeverity.Error, "clothing.visual.sprites", "At least one sprite mapping is required.", i_source);
			if (originalMode && i_visual.Sprites != null && i_visual.Sprites.Count > 33)
				io_report.Add(ValidationSeverity.Error, "clothing.visual.original-piece-count", "Fully original clothing supports an icon and at most 32 pieces.", i_source);
			if (originalMode && (i_visual.Sprites == null || !i_visual.Sprites.ContainsKey("icon")))
				io_report.Add(ValidationSeverity.Error, "clothing.visual.original-icon", "Fully original clothing requires an icon sprite.", i_source);
			foreach (KeyValuePair<string, string> sprite in i_visual.Sprites ?? new Dictionary<string, string>())
			{
				if ((!originalMode && !ClothingSlotCatalog.IsPublished(sprite.Key)) || (originalMode && !ClothingSlotCatalog.IsValidSlot(sprite.Key)))
					io_report.Add(ValidationSeverity.Error, "clothing.visual.sprite-name", "Sprite is not a published V1 clothing slot: " + sprite.Key, i_source);
				if (!ModPath.IsSafeRelativePath(sprite.Value) || !sprite.Value.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
					io_report.Add(ValidationSeverity.Error, "clothing.visual.sprite-path", "Sprite paths must be safe pack-relative PNG paths.", i_source);
			}
			foreach (KeyValuePair<string, ClothingAttachmentDefinition> attachment in i_visual.Attachments ?? new Dictionary<string, ClothingAttachmentDefinition>())
			{
				if (attachment.Key == "icon" || (originalMode ? !ClothingSlotCatalog.IsValidPieceSlot(attachment.Key) : !ClothingSlotCatalog.IsPublished(attachment.Key)) || attachment.Value == null)
				{
					io_report.Add(ValidationSeverity.Error, "clothing.visual.attachment", "attachments must target a published clothing piece slot.", i_source);
					continue;
				}
				ClothingAttachmentDefinition value = attachment.Value;
				if (originalMode && (i_visual.Sprites == null || !i_visual.Sprites.ContainsKey(attachment.Key)))
					io_report.Add(ValidationSeverity.Error, "clothing.visual.original-attachment-slot", "Original attachment definitions must match a supplied piece sprite: " + attachment.Key, i_source);
				if (originalMode && string.IsNullOrWhiteSpace(value.Bone))
					io_report.Add(ValidationSeverity.Error, "clothing.visual.original-attachment-bone", "Every original clothing piece must name its player bone: " + attachment.Key, i_source);
				string[] bones = { "Hips", "Butt", "Spine", "Chest", "Neck", "Head", "rArmUpper", "rArmLower", "rHand", "lArmUpper", "lArmLower", "lHand", "rLegUpper", "rLegLower", "rFoot", "lLegUpper", "lLegLower", "lFoot", "Ear", "Face" };
				if (!string.IsNullOrWhiteSpace(value.Bone) && Array.FindIndex(bones, bone => string.Equals(bone, value.Bone, StringComparison.OrdinalIgnoreCase)) < 0)
					io_report.Add(ValidationSeverity.Error, "clothing.visual.attachment-bone", "Unknown player bone: " + value.Bone, i_source);
				if ((value.OffsetX.HasValue && !IsFiniteRange(value.OffsetX.Value, -10f, 10f)) || (value.OffsetY.HasValue && !IsFiniteRange(value.OffsetY.Value, -10f, 10f))
					|| (value.Rotation.HasValue && !IsFiniteRange(value.Rotation.Value, -360f, 360f))
					|| (value.PivotX.HasValue && !IsFiniteRange(value.PivotX.Value, 0f, 1f)) || (value.PivotY.HasValue && !IsFiniteRange(value.PivotY.Value, 0f, 1f))
					|| (value.SortingOffset.HasValue && (value.SortingOffset.Value < -100 || value.SortingOffset.Value > 100)))
					io_report.Add(ValidationSeverity.Error, "clothing.visual.attachment-range", "Attachment offsets, pivot, rotation, or sortingOffset are outside supported bounds.", i_source);
				ValidatePhysics(value.Physics, io_report, i_source);
			}
			if (originalMode)
				foreach (string slot in i_visual.Sprites?.Keys ?? new Dictionary<string, string>().Keys)
					if (slot != "icon" && (i_visual.Attachments == null || !i_visual.Attachments.ContainsKey(slot)))
						io_report.Add(ValidationSeverity.Error, "clothing.visual.original-attachment", "Every original clothing piece requires an attachment definition: " + slot, i_source);
			foreach (KeyValuePair<string, Dictionary<string, string>> variant in i_visual.BodyVariants ?? new Dictionary<string, Dictionary<string, string>>())
			{
				if (!IsVariantName(variant.Key) || variant.Value == null || variant.Value.Count == 0 || variant.Value.Count > 32)
				{
					io_report.Add(ValidationSeverity.Error, "clothing.visual.body-variant", "Body variants need a safe name and 1 through 32 sprite overrides.", i_source);
					continue;
				}
				foreach (KeyValuePair<string, string> sprite in variant.Value)
				{
					bool known = i_visual.Sprites != null && i_visual.Sprites.ContainsKey(sprite.Key);
					if (sprite.Key == "icon" || !known || !ModPath.IsSafeRelativePath(sprite.Value) || !sprite.Value.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
						io_report.Add(ValidationSeverity.Error, "clothing.visual.body-variant-sprite", "Body variants may replace existing non-icon piece slots with safe PNG paths: " + sprite.Key, i_source);
				}
			}
		}

		private static void ValidatePhysics(ClothingPhysicsDefinition i_physics, ValidationReport io_report, string i_source)
		{
			if (i_physics == null) return;
			if (i_physics.Mode != "sway") io_report.Add(ValidationSeverity.Error, "clothing.physics.mode", "physics.mode must be sway.", i_source);
			if ((i_physics.Spring.HasValue && !IsFiniteRange(i_physics.Spring.Value, 0.1f, 200f))
				|| (i_physics.Damping.HasValue && !IsFiniteRange(i_physics.Damping.Value, 0f, 50f))
				|| (i_physics.Gravity.HasValue && !IsFiniteRange(i_physics.Gravity.Value, 0f, 1f))
				|| (i_physics.MotionInfluence.HasValue && !IsFiniteRange(i_physics.MotionInfluence.Value, 0f, 10f))
				|| (i_physics.MaxAngle.HasValue && !IsFiniteRange(i_physics.MaxAngle.Value, 0f, 90f)))
				io_report.Add(ValidationSeverity.Error, "clothing.physics.range", "Clothing sway values are outside supported bounds.", i_source);
		}

		private static bool IsVariantName(string i_value)
		{
			if (string.IsNullOrWhiteSpace(i_value) || i_value.Length > 80) return false;
			foreach (char value in i_value)
				if (!(char.IsLower(value) || char.IsDigit(value) || value == '.' || value == '_' || value == '-')) return false;
			return true;
		}

		private static void Error(ClothingDefinitionLoadResult io_result, string i_code, string i_message, string i_source)
		{
			io_result.Report.Add(ValidationSeverity.Error, "clothing." + i_code, i_message, i_source);
		}
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class ChallengeObjectiveDocument
	{
		[JsonProperty("objective", Required = Required.Always)] public string Objective { get; set; }
		[JsonProperty("count", Required = Required.Always)] public int Count { get; set; }
		[JsonProperty("enemies")] public List<string> Enemies { get; set; } = new List<string>();
		[JsonProperty("items")] public List<string> Items { get; set; } = new List<string>();
	}

	public sealed class ChallengeObjectiveDefinition
	{
		public string Objective { get; }
		public int Count { get; }
		public IReadOnlyList<ContentId> Enemies { get; }
		public IReadOnlyList<ContentId> Items { get; }
		public ChallengeObjectiveDefinition(string i_objective, int i_count, IEnumerable<string> i_enemies, IEnumerable<string> i_items)
		{
			Objective = i_objective; Count = i_count;
			List<ContentId> enemies = new List<ContentId>(); foreach (string value in i_enemies ?? new string[0]) enemies.Add(ContentId.Parse(value)); Enemies = enemies;
			List<ContentId> items = new List<ContentId>(); foreach (string value in i_items ?? new string[0]) items.Add(ContentId.Parse(value)); Items = items;
		}
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class ChallengeGrantDocument
	{
		[JsonProperty("id", Required = Required.Always)] public string Id { get; set; }
		[JsonProperty("amount")] public int Amount { get; set; } = 1;
	}

	public sealed class ChallengeGrantDefinition
	{
		public ContentId Id { get; }
		public int Amount { get; }
		public ChallengeGrantDefinition(ChallengeGrantDocument i_document) { Id = ContentId.Parse(i_document.Id); Amount = i_document.Amount; }
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class ChallengeRewardBundleDocument
	{
		[JsonProperty("currency")] public int Currency { get; set; }
		[JsonProperty("items")] public List<ChallengeGrantDocument> Items { get; set; } = new List<ChallengeGrantDocument>();
		[JsonProperty("weapons")] public List<ChallengeGrantDocument> Weapons { get; set; } = new List<ChallengeGrantDocument>();
		[JsonProperty("content")] public List<string> Content { get; set; } = new List<string>();
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class ChallengeDefinitionDocument
	{
		[JsonProperty("schemaVersion", Required = Required.Always)] public int SchemaVersion { get; set; }
		[JsonProperty("type", Required = Required.Always)] public string Type { get; set; }
		[JsonProperty("id", Required = Required.Always)] public string Id { get; set; }
		[JsonProperty("displayName", Required = Required.Always)] public string DisplayName { get; set; }
		[JsonProperty("description", Required = Required.Always)] public string Description { get; set; }
		[JsonProperty("extends")] public string Extends { get; set; }
		[JsonProperty("objective")] public string Objective { get; set; }
		[JsonProperty("count")] public int? Count { get; set; }
		[JsonProperty("stage")] public string Stage { get; set; }
		[JsonProperty("enemies")] public List<string> Enemies { get; set; } = new List<string>();
		[JsonProperty("items")] public List<string> Items { get; set; } = new List<string>();
		[JsonProperty("steps")] public List<ChallengeObjectiveDocument> Steps { get; set; } = new List<ChallengeObjectiveDocument>();
		[JsonProperty("rewards")] public List<string> Rewards { get; set; } = new List<string>();
		[JsonProperty("rewardBundle")] public ChallengeRewardBundleDocument RewardBundle { get; set; } = new ChallengeRewardBundleDocument();
	}

	public sealed class ChallengeDefinition
	{
		public ContentId Id { get; }
		public string PackId { get; }
		public string Source { get; }
		public string DisplayName { get; }
		public string Description { get; }
		public ContentId? Extends { get; }
		public string Objective { get; }
		public int Count { get; }
		public ContentId? Stage { get; }
		public IReadOnlyList<ContentId> Enemies { get; }
		public IReadOnlyList<ContentId> Items { get; }
		public IReadOnlyList<ContentId> Rewards { get; }
		public IReadOnlyList<ChallengeObjectiveDefinition> Steps { get; }
		public int RewardCurrency { get; }
		public IReadOnlyList<ChallengeGrantDefinition> RewardItems { get; }
		public IReadOnlyList<ChallengeGrantDefinition> RewardWeapons { get; }
		public IReadOnlyList<ContentId> RewardContent { get; }
		public ChallengeDefinition(ContentId i_id, string i_packId, string i_source, ChallengeDefinitionDocument i_document)
		{
			Id = i_id; PackId = i_packId; Source = i_source; DisplayName = i_document.DisplayName;
			Description = i_document.Description; Objective = i_document.Objective ?? string.Empty; Count = i_document.Count ?? 0;
			Extends = string.IsNullOrEmpty(i_document.Extends) ? (ContentId?)null : ContentId.Parse(i_document.Extends);
			Stage = string.IsNullOrEmpty(i_document.Stage) ? (ContentId?)null : ContentId.Parse(i_document.Stage);
			List<ContentId> enemies = new List<ContentId>(); foreach (string value in i_document.Enemies) enemies.Add(ContentId.Parse(value)); Enemies = enemies;
			List<ContentId> items = new List<ContentId>(); foreach (string value in i_document.Items) items.Add(ContentId.Parse(value)); Items = items;
			List<ContentId> rewards = new List<ContentId>(); foreach (string value in i_document.Rewards) rewards.Add(ContentId.Parse(value)); Rewards = rewards;
			List<ChallengeObjectiveDefinition> steps = new List<ChallengeObjectiveDefinition>(); foreach (ChallengeObjectiveDocument step in i_document.Steps) steps.Add(new ChallengeObjectiveDefinition(step.Objective, step.Count, step.Enemies, step.Items)); Steps = steps;
			RewardCurrency = i_document.RewardBundle.Currency;
			List<ChallengeGrantDefinition> rewardItems = new List<ChallengeGrantDefinition>(); foreach (ChallengeGrantDocument reward in i_document.RewardBundle.Items) rewardItems.Add(new ChallengeGrantDefinition(reward)); RewardItems = rewardItems;
			List<ChallengeGrantDefinition> rewardWeapons = new List<ChallengeGrantDefinition>(); foreach (ChallengeGrantDocument reward in i_document.RewardBundle.Weapons) rewardWeapons.Add(new ChallengeGrantDefinition(reward)); RewardWeapons = rewardWeapons;
			List<ContentId> rewardContent = new List<ContentId>(); foreach (string value in i_document.RewardBundle.Content) rewardContent.Add(ContentId.Parse(value)); RewardContent = rewardContent;
		}
	}

	public sealed class ChallengeDefinitionLoadResult
	{
		public ChallengeDefinition Definition { get; internal set; }
		public ValidationReport Report { get; } = new ValidationReport();
	}

	public static class ChallengeDefinitionParser
	{
		private static readonly HashSet<string> s_objectives = new HashSet<string>(StringComparer.Ordinal)
		{
			"killCount", "reachWave", "surviveWaves", "pickupCount", "interactionCount", "shotsFired",
			"damageTaken", "birthCount", "impregnationCount", "rapeCount", "orgasmCount", "mindBreakCount",
			"useItemCount", "weaponKillCount", "flawlessWaves"
		};

		public static ChallengeDefinitionLoadResult Parse(string i_json, string i_packId, string i_source)
		{
			ChallengeDefinitionLoadResult result = new ChallengeDefinitionLoadResult();
			ChallengeDefinitionDocument document;
			try { document = JsonConvert.DeserializeObject<ChallengeDefinitionDocument>(i_json, new JsonSerializerSettings { MissingMemberHandling = MissingMemberHandling.Error }); }
			catch (JsonException exception) { result.Report.Add(ValidationSeverity.Error, "challenge.json", exception.Message, i_source); return result; }
			if (document == null) { result.Report.Add(ValidationSeverity.Error, "challenge.null", "Challenge definition resolved to null.", i_source); return result; }
			if (document.SchemaVersion != 1) Error(result, "schema-version", "Unsupported schemaVersion.", i_source);
			if (document.Type != "challenge") Error(result, "type", "Definition type must be challenge.", i_source);
			if (!ContentId.TryParse(document.Id, out ContentId id) || id.Namespace != i_packId || !id.Path.StartsWith("challenge/", StringComparison.Ordinal)) Error(result, "id", "Challenge ID must use the defining pack namespace and a challenge/ path.", i_source);
			if (string.IsNullOrWhiteSpace(document.DisplayName) || string.IsNullOrWhiteSpace(document.Description)) Error(result, "text", "displayName and description are required.", i_source);
			if (document.Enemies == null) document.Enemies = new List<string>();
			if (document.Items == null) document.Items = new List<string>();
			if (document.Steps == null) document.Steps = new List<ChallengeObjectiveDocument>();
			if (document.Rewards == null) document.Rewards = new List<string>();
			if (document.RewardBundle == null) document.RewardBundle = new ChallengeRewardBundleDocument();
			if (document.RewardBundle.Items == null) document.RewardBundle.Items = new List<ChallengeGrantDocument>();
			if (document.RewardBundle.Weapons == null) document.RewardBundle.Weapons = new List<ChallengeGrantDocument>();
			if (document.RewardBundle.Content == null) document.RewardBundle.Content = new List<string>();
			bool inherited = !string.IsNullOrWhiteSpace(document.Extends);
			if (inherited && (!ContentId.TryParse(document.Extends, out ContentId inheritedId) || !inheritedId.Path.StartsWith("challenge/", StringComparison.Ordinal)))
				Error(result, "extends", "extends must be a challenge content ID.", i_source);
			bool hasSteps = document.Steps.Count > 0;
			if (inherited && (!string.IsNullOrEmpty(document.Objective) || document.Count.HasValue || hasSteps))
				Error(result, "inheritance-fields", "Inherited challenges retain their objective; objective, count, and steps must be omitted.", i_source);
			if (!inherited && hasSteps && (!string.IsNullOrEmpty(document.Objective) || document.Count.HasValue || document.Enemies.Count > 0 || document.Items.Count > 0))
				Error(result, "steps-fields", "A multi-step challenge uses steps instead of the top-level objective, count, enemies, or items fields.", i_source);
			if (!inherited && !hasSteps) ValidateObjective(result, document.Objective, document.Count, document.Enemies, document.Items, i_source, "");
			if (!inherited && hasSteps)
			{
				if (document.Steps.Count > 32) Error(result, "steps-count", "A challenge may contain at most 32 ordered steps.", i_source);
				for (int index = 0; index < document.Steps.Count; index++)
				{
					ChallengeObjectiveDocument step = document.Steps[index];
					if (step == null) { Error(result, "step-null", "Challenge steps cannot be null.", i_source); continue; }
					if (step.Enemies == null) step.Enemies = new List<string>();
					if (step.Items == null) step.Items = new List<string>();
					ValidateObjective(result, step.Objective, step.Count, step.Enemies, step.Items, i_source, "step " + (index + 1) + ": ");
				}
			}
			if (!string.IsNullOrEmpty(document.Stage) && (!ContentId.TryParse(document.Stage, out ContentId stage) || !stage.Path.StartsWith("stage/", StringComparison.Ordinal))) Error(result, "stage", "stage must be a stage content ID when supplied.", i_source);
			if (inherited && (document.Enemies.Count > 0 || document.Items.Count > 0))
				Error(result, "inheritance-filters", "Inherited challenges retain their exact objective filters; enemies and items must be omitted.", i_source);
			foreach (string value in document.Enemies ?? new List<string>()) if (!ContentId.TryParse(value, out ContentId enemy) || !enemy.Path.StartsWith("enemy/", StringComparison.Ordinal)) Error(result, "enemy", "Enemy references must be enemy content IDs.", i_source);
			foreach (string value in document.Items ?? new List<string>()) if (!ContentId.TryParse(value, out ContentId item) || !item.Path.StartsWith("item/", StringComparison.Ordinal)) Error(result, "item", "Item references must be item content IDs.", i_source);
			foreach (string value in document.Rewards ?? new List<string>()) if (!ContentId.TryParse(value, out ContentId reward) || !reward.Path.StartsWith("clothing/", StringComparison.Ordinal)) Error(result, "reward", "Rewards must be clothing content IDs.", i_source);
			foreach (ChallengeObjectiveDocument step in document.Steps) if (step != null)
			{
				foreach (string value in step.Enemies) if (!ContentId.TryParse(value, out ContentId enemy) || !enemy.Path.StartsWith("enemy/", StringComparison.Ordinal)) Error(result, "step-enemy", "Step enemy references must be enemy content IDs.", i_source);
				foreach (string value in step.Items) if (!ContentId.TryParse(value, out ContentId item) || !item.Path.StartsWith("item/", StringComparison.Ordinal)) Error(result, "step-item", "Step item references must be item content IDs.", i_source);
			}
			bool hasBundleReward = document.RewardBundle.Currency > 0 || document.RewardBundle.Items.Count > 0 || document.RewardBundle.Weapons.Count > 0 || document.RewardBundle.Content.Count > 0;
			if (document.Rewards.Count == 0 && !hasBundleReward) Error(result, "rewards", "At least one legacy clothing reward or rewardBundle entry is required.", i_source);
			if (document.RewardBundle.Currency < 0 || document.RewardBundle.Currency > 1000000) Error(result, "reward-currency", "rewardBundle.currency must be between 0 and 1000000.", i_source);
			ValidateGrants(result, document.RewardBundle.Items, "item", i_source, false);
			ValidateGrants(result, document.RewardBundle.Weapons, "weapon", i_source, true);
			foreach (string value in document.RewardBundle.Content) if (!ContentId.TryParse(value, out ContentId _)) Error(result, "reward-content", "rewardBundle.content entries must be content IDs.", i_source);
			if (result.Report.IsValid) result.Definition = new ChallengeDefinition(id, i_packId, i_source, document);
			return result;
		}
		private static void ValidateObjective(ChallengeDefinitionLoadResult i_result, string i_objective, int? i_count, List<string> i_enemies, List<string> i_items, string i_source, string i_prefix)
		{
			if (!s_objectives.Contains(i_objective ?? string.Empty)) Error(i_result, "objective", i_prefix + "Unsupported objective.", i_source);
			if (!i_count.HasValue || i_count.Value < 1 || i_count.Value > 1000000) Error(i_result, "count", i_prefix + "count must be between 1 and 1000000.", i_source);
			if (i_objective != "killCount" && i_objective != "weaponKillCount"
				&& i_objective != "rapeCount" && i_objective != "orgasmCount"
				&& i_objective != "impregnationCount" && i_enemies.Count > 0)
				Error(i_result, "enemies-objective", i_prefix + "enemies is only supported by killCount, weaponKillCount, rapeCount, orgasmCount, or impregnationCount.", i_source);
			if (i_objective != "pickupCount" && i_objective != "useItemCount" && i_objective != "weaponKillCount" && i_items.Count > 0) Error(i_result, "items-objective", i_prefix + "items is only supported by pickupCount, useItemCount, or weaponKillCount.", i_source);
		}
		private static void ValidateGrants(ChallengeDefinitionLoadResult i_result, List<ChallengeGrantDocument> i_grants, string i_kind, string i_source, bool i_weapon)
		{
			if (i_grants.Count > 32) Error(i_result, "reward-" + i_kind + "-count", "At most 32 " + i_kind + " rewards are allowed.", i_source);
			foreach (ChallengeGrantDocument grant in i_grants)
			{
				if (grant == null || !ContentId.TryParse(grant.Id, out ContentId id) || !id.Path.StartsWith("item/", StringComparison.Ordinal)) Error(i_result, "reward-" + i_kind, i_kind + " rewards must use item content IDs.", i_source);
				else if (grant.Amount < 1 || grant.Amount > (i_weapon ? 1 : 99)) Error(i_result, "reward-" + i_kind + "-amount", i_kind + " reward amount is outside the supported range.", i_source);
			}
		}
		private static void Error(ChallengeDefinitionLoadResult i_result, string i_code, string i_message, string i_source) { i_result.Report.Add(ValidationSeverity.Error, "challenge." + i_code, i_message, i_source); }
	}
}
