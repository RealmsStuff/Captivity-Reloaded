using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace CaptivityReloaded.Modding
{
	[JsonObject(MemberSerialization.OptIn)]
	public sealed class EnemyStatsDefinition
	{
		[JsonProperty("healthMax")]
		public float? HealthMax { get; set; }

		[JsonProperty("speedAcceleration")]
		public float? SpeedAcceleration { get; set; }

		[JsonProperty("speedMax")]
		public float? SpeedMax { get; set; }

		[JsonProperty("traction")]
		public float? Traction { get; set; }

		[JsonProperty("bounty")]
		public int? Bounty { get; set; }

		[JsonProperty("healthIncreasePerWave")]
		public float? HealthIncreasePerWave { get; set; }
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class AtlasRegionDefinition
	{
		[JsonProperty("x", Required = Required.Always)]
		public int X { get; set; }

		[JsonProperty("y", Required = Required.Always)]
		public int Y { get; set; }

		[JsonProperty("width", Required = Required.Always)]
		public int Width { get; set; }

		[JsonProperty("height", Required = Required.Always)]
		public int Height { get; set; }
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class EnemyVisualDefinition
	{
		[JsonProperty("type", Required = Required.Always)]
		public string Type { get; set; }

		[JsonProperty("atlas", Required = Required.Always)]
		public string Atlas { get; set; }

		[JsonProperty("pixelsPerUnit")]
		public float PixelsPerUnit { get; set; } = 32f;

		[JsonProperty("regions", Required = Required.Always)]
		public Dictionary<string, AtlasRegionDefinition> Regions { get; set; }

		[JsonProperty("bones")] public List<EnemyBoneDefinition> Bones { get; set; } = new List<EnemyBoneDefinition>();
		[JsonProperty("hitZones")] public List<EnemyHitZoneDefinition> HitZones { get; set; } = new List<EnemyHitZoneDefinition>();
		[JsonProperty("bodyWidth")] public float? BodyWidth { get; set; }
		[JsonProperty("bodyHeight")] public float? BodyHeight { get; set; }
		[JsonProperty("bodyOffsetX")] public float? BodyOffsetX { get; set; }
		[JsonProperty("bodyOffsetY")] public float? BodyOffsetY { get; set; }
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class EnemyBoneDefinition
	{
		[JsonProperty("id", Required = Required.Always)] public string Id { get; set; }
		[JsonProperty("parent")] public string Parent { get; set; }
		[JsonProperty("region", Required = Required.Always)] public string Region { get; set; }
		[JsonProperty("x")] public float X { get; set; }
		[JsonProperty("y")] public float Y { get; set; }
		[JsonProperty("rotation")] public float Rotation { get; set; }
		[JsonProperty("pivotX")] public float PivotX { get; set; } = 0.5f;
		[JsonProperty("pivotY")] public float PivotY { get; set; } = 0.5f;
		[JsonProperty("sortingOrder")] public int SortingOrder { get; set; }
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class EnemyHitZoneDefinition
	{
		[JsonProperty("bone", Required = Required.Always)] public string Bone { get; set; }
		[JsonProperty("shape", Required = Required.Always)] public string Shape { get; set; }
		[JsonProperty("offsetX")] public float OffsetX { get; set; }
		[JsonProperty("offsetY")] public float OffsetY { get; set; }
		[JsonProperty("width")] public float? Width { get; set; }
		[JsonProperty("height")] public float? Height { get; set; }
		[JsonProperty("radius")] public float? Radius { get; set; }
		[JsonProperty("damageMultiplier")] public string DamageMultiplier { get; set; } = "normal";
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class EnemySpawnDefinition
	{
		[JsonProperty("inheritTemplateSpawners", Required = Required.Always)]
		public bool InheritTemplateSpawners { get; set; }

		[JsonProperty("selectionWeight")]
		public float? SelectionWeight { get; set; }
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class EnemyAttackDefinition
	{
		[JsonProperty("index")] public int Index { get; set; } = -1;
		[JsonProperty("id")] public string Id { get; set; }
		[JsonProperty("type")] public string Type { get; set; }
		[JsonProperty("animation")] public string Animation { get; set; }
		[JsonProperty("hitTimeSeconds")] public float? HitTimeSeconds { get; set; }
		[JsonProperty("chance")] public float? Chance { get; set; }
		[JsonProperty("damage")] public float? Damage { get; set; }
		[JsonProperty("knockbackX")] public float? KnockbackX { get; set; }
		[JsonProperty("knockbackY")] public float? KnockbackY { get; set; }
		[JsonProperty("cooldownSeconds")] public float? CooldownSeconds { get; set; }
		[JsonProperty("initiateRange")] public float? InitiateRange { get; set; }
		[JsonProperty("hitRange")] public float? HitRange { get; set; }
		[JsonProperty("movesDuringAttack")] public bool? MovesDuringAttack { get; set; }
		[JsonProperty("durationSeconds")] public float? DurationSeconds { get; set; }
		[JsonProperty("areaRadius")] public float? AreaRadius { get; set; }
		[JsonProperty("offsetX")] public float? OffsetX { get; set; }
		[JsonProperty("offsetY")] public float? OffsetY { get; set; }
		[JsonProperty("projectileRegion")] public string ProjectileRegion { get; set; }
		[JsonProperty("projectileSpeed")] public float? ProjectileSpeed { get; set; }
		[JsonProperty("projectileLifetimeSeconds")] public float? ProjectileLifetimeSeconds { get; set; }
		[JsonProperty("projectileRadius")] public float? ProjectileRadius { get; set; }
		[JsonProperty("projectileGravityScale")] public float? ProjectileGravityScale { get; set; }
		[JsonProperty("stages")] public List<EnemyAttackStageDefinition> Stages { get; set; } = new List<EnemyAttackStageDefinition>();
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class EnemyAttackStageDefinition
	{
		[JsonProperty("type", Required = Required.Always)] public string Type { get; set; }
		[JsonProperty("damage")] public float? Damage { get; set; }
		[JsonProperty("knockbackX")] public float? KnockbackX { get; set; }
		[JsonProperty("knockbackY")] public float? KnockbackY { get; set; }
		[JsonProperty("hitRange")] public float? HitRange { get; set; }
		[JsonProperty("areaRadius")] public float? AreaRadius { get; set; }
		[JsonProperty("offsetX")] public float? OffsetX { get; set; }
		[JsonProperty("offsetY")] public float? OffsetY { get; set; }
		[JsonProperty("projectileRegion")] public string ProjectileRegion { get; set; }
		[JsonProperty("projectileSpeed")] public float? ProjectileSpeed { get; set; }
		[JsonProperty("projectileLifetimeSeconds")] public float? ProjectileLifetimeSeconds { get; set; }
		[JsonProperty("projectileRadius")] public float? ProjectileRadius { get; set; }
		[JsonProperty("projectileGravityScale")] public float? ProjectileGravityScale { get; set; }
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class EnemyBehaviorDefinition
	{
		[JsonProperty("visionRange")] public float? VisionRange { get; set; }
		[JsonProperty("ignoreWave")] public bool? IgnoreWave { get; set; }
		[JsonProperty("modules")] public List<EnemyBehaviorModuleDefinition> Modules { get; set; } = new List<EnemyBehaviorModuleDefinition>();
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class EnemyBehaviorModuleDefinition
	{
		[JsonProperty("type", Required = Required.Always)] public string Type { get; set; }
		[JsonProperty("amount")] public float? Amount { get; set; }
		[JsonProperty("intervalSeconds")] public float? IntervalSeconds { get; set; }
		[JsonProperty("healthThreshold")] public float? HealthThreshold { get; set; }
		[JsonProperty("speedBonus")] public float? SpeedBonus { get; set; }
		[JsonProperty("damageMultiplierBonus")] public float? DamageMultiplierBonus { get; set; }
		[JsonProperty("durationSeconds")] public float? DurationSeconds { get; set; }
		[JsonProperty("radius")] public float? Radius { get; set; }
		[JsonProperty("enemy")] public string Enemy { get; set; }
		[JsonProperty("count")] public int? Count { get; set; }
		[JsonProperty("triggerRange")] public float? TriggerRange { get; set; }
		[JsonProperty("startDelaySeconds")] public float? StartDelaySeconds { get; set; }
		[JsonProperty("meterMax")] public float? MeterMax { get; set; }
		[JsonProperty("inputPower")] public float? InputPower { get; set; }
		[JsonProperty("inputPattern")] public string InputPattern { get; set; }
		[JsonProperty("decayPerSecond")] public float? DecayPerSecond { get; set; }
		[JsonProperty("failureDamage")] public float? FailureDamage { get; set; }
		[JsonProperty("successRecoveryHealth")] public float? SuccessRecoveryHealth { get; set; }
		[JsonProperty("successStunSeconds")] public float? SuccessStunSeconds { get; set; }
		[JsonProperty("cooldownSeconds")] public float? CooldownSeconds { get; set; }
		[JsonProperty("animation")] public string Animation { get; set; }
		[JsonProperty("playerAnimation")] public EnemyAnimationClipDefinition PlayerAnimation { get; set; }
		[JsonProperty("playerAnimationRef")] public string PlayerAnimationReference { get; set; }
		[JsonProperty("phases")] public List<EnemyFinisherPhaseDefinition> Phases { get; set; } = new List<EnemyFinisherPhaseDefinition>();
		[JsonProperty("participants")] public List<EnemyFinisherParticipantDefinition> Participants { get; set; } = new List<EnemyFinisherParticipantDefinition>();
		[JsonProperty("successOutcome")] public EnemyFinisherOutcomeDefinition SuccessOutcome { get; set; }
		[JsonProperty("failureOutcome")] public EnemyFinisherOutcomeDefinition FailureOutcome { get; set; }
		[JsonProperty("statuses")] public Dictionary<string, EnemyFinisherStatusTextDefinition> Statuses { get; set; } = new Dictionary<string, EnemyFinisherStatusTextDefinition>();
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class EnemyFinisherStatusTextDefinition
	{
		[JsonProperty("title")] public string Title { get; set; }
		[JsonProperty("description")] public string Description { get; set; }
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class EnemyFinisherPhaseDefinition
	{
		[JsonProperty("id", Required = Required.Always)] public string Id { get; set; }
		[JsonProperty("durationSeconds", Required = Required.Always)] public float DurationSeconds { get; set; }
		[JsonProperty("animation", Required = Required.Always)] public string Animation { get; set; }
		[JsonProperty("playerAnimation")] public EnemyAnimationClipDefinition PlayerAnimation { get; set; }
		[JsonProperty("playerAnimationRef")] public string PlayerAnimationReference { get; set; }
		[JsonProperty("inputPower")] public float? InputPower { get; set; }
		[JsonProperty("inputPattern")] public string InputPattern { get; set; }
		[JsonProperty("decayPerSecond")] public float? DecayPerSecond { get; set; }
		[JsonProperty("participantAnimations")] public Dictionary<string, string> ParticipantAnimations { get; set; } = new Dictionary<string, string>();
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class EnemyFinisherParticipantDefinition
	{
		[JsonProperty("id", Required = Newtonsoft.Json.Required.Always)] public string Id { get; set; }
		[JsonProperty("enemy", Required = Newtonsoft.Json.Required.Always)] public string Enemy { get; set; }
		[JsonProperty("joinPolicy")] public string JoinPolicy { get; set; }
		[JsonProperty("required")] public bool Required { get; set; }
		[JsonProperty("joinRange")] public float? JoinRange { get; set; }
		[JsonProperty("approachRange")] public float? ApproachRange { get; set; }
		[JsonProperty("offsetX")] public float? OffsetX { get; set; }
		[JsonProperty("offsetY")] public float? OffsetY { get; set; }
		[JsonProperty("facing")] public string Facing { get; set; }
		[JsonProperty("animation")] public string Animation { get; set; }
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class EnemyFinisherOutcomeDefinition
	{
		[JsonProperty("healthDamage")] public float? HealthDamage { get; set; }
		[JsonProperty("strengthDamage")] public float? StrengthDamage { get; set; }
		[JsonProperty("pleasure")] public float? Pleasure { get; set; }
		[JsonProperty("libido")] public float? Libido { get; set; }
		[JsonProperty("healthRecovery")] public float? HealthRecovery { get; set; }
		[JsonProperty("enemyStunSeconds")] public float? EnemyStunSeconds { get; set; }
		[JsonProperty("playerRagdollSeconds")] public float? PlayerRagdollSeconds { get; set; }
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class EnemyAnimationDefinition
	{
		[JsonProperty("speedMultiplier")] public float? SpeedMultiplier { get; set; }
		[JsonProperty("clips")] public Dictionary<string, EnemyAnimationClipDefinition> Clips { get; set; } = new Dictionary<string, EnemyAnimationClipDefinition>();
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class EnemyAnimationClipDefinition
	{
		[JsonProperty("durationSeconds", Required = Required.Always)] public float DurationSeconds { get; set; }
		[JsonProperty("loop")] public bool Loop { get; set; }
		[JsonProperty("frames", Required = Required.Always)] public List<EnemyAnimationFrameDefinition> Frames { get; set; }
		[JsonProperty("events")] public List<EnemyAnimationEventDefinition> Events { get; set; } = new List<EnemyAnimationEventDefinition>();
		[JsonIgnore] public NormalizedAnimationPresentation NormalizedPresentation { get; set; }
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class EnemyAnimationEventDefinition
	{
		[JsonProperty("time", Required = Required.Always)] public float Time { get; set; }
		[JsonProperty("type", Required = Required.Always)] public string Type { get; set; }
		[JsonProperty("amount")] public float? Amount { get; set; }
		[JsonProperty("x")] public float? X { get; set; }
		[JsonProperty("y")] public float? Y { get; set; }
		[JsonProperty("relativeToFacing")] public bool? RelativeToFacing { get; set; }
		[JsonProperty("file")] public string File { get; set; }
		[JsonProperty("volume")] public float? Volume { get; set; }
		[JsonProperty("region")] public string Region { get; set; }
		[JsonProperty("bone")] public string Bone { get; set; }
		[JsonProperty("durationSeconds")] public float? DurationSeconds { get; set; }
		[JsonProperty("scale")] public float? Scale { get; set; }
		[JsonProperty("sortingOrder")] public int? SortingOrder { get; set; }
		[JsonProperty("cue")] public string Cue { get; set; }
		[JsonProperty("index")] public int? Index { get; set; }
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class EnemyAnimationFrameDefinition
	{
		[JsonProperty("time", Required = Required.Always)] public float Time { get; set; }
		[JsonProperty("bones", Required = Required.Always)] public Dictionary<string, EnemyBonePoseDefinition> Bones { get; set; }
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class EnemyBonePoseDefinition
	{
		[JsonProperty("region")] public string Region { get; set; }
		[JsonProperty("x")] public float? X { get; set; }
		[JsonProperty("y")] public float? Y { get; set; }
		[JsonProperty("rotation")] public float? Rotation { get; set; }
		[JsonProperty("scaleX")] public float? ScaleX { get; set; }
		[JsonProperty("scaleY")] public float? ScaleY { get; set; }
		[JsonIgnore] public float? ColorR { get; set; }
		[JsonIgnore] public float? ColorG { get; set; }
		[JsonIgnore] public float? ColorB { get; set; }
		[JsonIgnore] public float? ColorA { get; set; }
		[JsonIgnore] public int? RuntimeSortingOrder { get; set; }
		[JsonIgnore] public string RuntimeSpriteName { get; set; }
		[JsonIgnore] public Sprite RuntimeSprite { get; set; }
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class EnemyAiDefinition
	{
		[JsonProperty("type", Required = Required.Always)] public string Type { get; set; }
		[JsonProperty("preferredRange")] public float? PreferredRange { get; set; }
		[JsonProperty("retreatRange")] public float? RetreatRange { get; set; }
		[JsonProperty("reactionSeconds")] public float? ReactionSeconds { get; set; }
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class EnemyDropsDefinition
	{
		[JsonProperty("chance")] public float? Chance { get; set; }
		[JsonProperty("items")] public List<string> Items { get; set; } = new List<string>();
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class EnemyDefinitionDocument
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

		[JsonProperty("stats")]
		public EnemyStatsDefinition Stats { get; set; }

		[JsonProperty("visual")]
		public EnemyVisualDefinition Visual { get; set; }

		[JsonProperty("spawn")]
		public EnemySpawnDefinition Spawn { get; set; }

		[JsonProperty("attacks")] public List<EnemyAttackDefinition> Attacks { get; set; } = new List<EnemyAttackDefinition>();
		[JsonProperty("behavior")] public EnemyBehaviorDefinition Behavior { get; set; }
		[JsonProperty("animation")] public EnemyAnimationDefinition Animation { get; set; }
		[JsonProperty("animationRefs")] public Dictionary<string, string> AnimationReferences { get; set; } = new Dictionary<string, string>();
		[JsonProperty("ai")] public EnemyAiDefinition Ai { get; set; }
		[JsonProperty("drops")] public EnemyDropsDefinition Drops { get; set; }
	}

	public sealed class EnemyDefinition
	{
		public ContentId Id { get; }
		public ContentId? Extends { get; }
		public string PackId { get; }
		public string Source { get; }
		public string DisplayName { get; }
		public string Description { get; }
		public EnemyStatsDefinition Stats { get; }
		public EnemyVisualDefinition Visual { get; }
		public EnemySpawnDefinition Spawn { get; }
		public IReadOnlyList<EnemyAttackDefinition> Attacks { get; }
		public EnemyBehaviorDefinition Behavior { get; }
		public EnemyAnimationDefinition Animation { get; }
		public IReadOnlyDictionary<string, string> AnimationReferences { get; }
		public EnemyAiDefinition Ai { get; }
		public EnemyDropsDefinition Drops { get; }

		public EnemyDefinition(ContentId i_id, ContentId? i_extends, string i_packId, string i_source, EnemyDefinitionDocument i_document)
		{
			Id = i_id;
			Extends = i_extends;
			PackId = i_packId;
			Source = i_source;
			DisplayName = i_document.DisplayName;
			Description = i_document.Description ?? string.Empty;
			Stats = i_document.Stats ?? new EnemyStatsDefinition();
			Visual = i_document.Visual;
			Spawn = i_document.Spawn ?? new EnemySpawnDefinition();
			Attacks = i_document.Attacks ?? new List<EnemyAttackDefinition>();
			Behavior = i_document.Behavior ?? new EnemyBehaviorDefinition();
			Animation = i_document.Animation ?? new EnemyAnimationDefinition();
			AnimationReferences = i_document.AnimationReferences ?? new Dictionary<string, string>();
			Ai = i_document.Ai;
			Drops = i_document.Drops ?? new EnemyDropsDefinition();
		}
	}

	public sealed class EnemyDefinitionLoadResult
	{
		public EnemyDefinition Definition { get; internal set; }
		public ValidationReport Report { get; } = new ValidationReport();
	}

	public static class EnemyDefinitionParser
	{
		public const int SupportedSchemaVersion = 1;

		public static EnemyDefinitionLoadResult Parse(string i_json, string i_packId, string i_source)
		{
			EnemyDefinitionLoadResult result = new EnemyDefinitionLoadResult();
			EnemyDefinitionDocument document;
			try
			{
				document = JsonConvert.DeserializeObject<EnemyDefinitionDocument>(i_json, new JsonSerializerSettings
				{
					MissingMemberHandling = MissingMemberHandling.Error
				});
			}
			catch (JsonException exception)
			{
				result.Report.Add(ValidationSeverity.Error, "enemy.json", exception.Message, i_source);
				return result;
			}
			if (document == null)
			{
				result.Report.Add(ValidationSeverity.Error, "enemy.null", "Enemy definition resolved to null.", i_source);
				return result;
			}

			if (document.SchemaVersion != SupportedSchemaVersion) Error(result, "schema-version", "Unsupported schemaVersion " + document.SchemaVersion + ".", i_source);
			if (!string.Equals(document.Type, "enemy", StringComparison.Ordinal)) Error(result, "type", "Definition type must be 'enemy'.", i_source);
			if (!ContentId.TryParse(document.Id, out ContentId id) || id.Namespace != i_packId || !id.Path.StartsWith("enemy/", StringComparison.Ordinal))
				Error(result, "id", "Enemy ID must use the defining pack namespace and an enemy/ path.", i_source);
			bool original = string.IsNullOrWhiteSpace(document.Extends);
			ContentId? extends = null;
			if (!original)
			{
				if (!ContentId.TryParse(document.Extends, out ContentId parsedExtends) || parsedExtends.Namespace != "core" ||
					!parsedExtends.Path.StartsWith("enemy/", StringComparison.Ordinal) || parsedExtends == id)
					Error(result, "extends", "extends must reference a Core enemy content ID when supplied.", i_source);
				else extends = parsedExtends;
			}
			if (string.IsNullOrWhiteSpace(document.DisplayName)) Error(result, "display-name", "displayName is required.", i_source);

			ValidateStats(document.Stats, result.Report, i_source);
			if (original && (document.Stats == null || !document.Stats.HealthMax.HasValue || !document.Stats.SpeedAcceleration.HasValue || !document.Stats.SpeedMax.HasValue || !document.Stats.Traction.HasValue))
				Error(result, "stats-required", "Original enemies require healthMax, speedAcceleration, speedMax, and traction.", i_source);
			ValidateGameplay(document, original, result.Report, i_source);
			ValidateVisual(document.Visual, original, result.Report, i_source);
			if (result.Report.IsValid) result.Definition = new EnemyDefinition(id, extends, i_packId, i_source, document);
			return result;
		}

		private static void ValidateGameplay(EnemyDefinitionDocument i_document, bool i_original, ValidationReport io_report, string i_source)
		{
			if (i_document.Spawn != null)
				ValidateRange(i_document.Spawn.SelectionWeight, 0.001f, 1000f, "spawn-selection-weight", io_report, i_source);
			if (i_original && i_document.Spawn != null && i_document.Spawn.InheritTemplateSpawners)
				io_report.Add(ValidationSeverity.Error, "enemy.spawn.original-inherit", "Original enemies cannot inherit Core template spawners; reference their ID from a stage spawner instead.", i_source);
			HashSet<int> attackIndexes = new HashSet<int>();
			HashSet<string> attackIds = new HashSet<string>(StringComparer.Ordinal);
			foreach (EnemyAttackDefinition attack in i_document.Attacks ?? new List<EnemyAttackDefinition>())
			{
				if (attack == null)
				{
					io_report.Add(ValidationSeverity.Error, "enemy.attacks.entry", "Attack entries cannot be null.", i_source);
					continue;
				}
				if (i_original)
				{
					if (!IsSemanticName(attack.Id) || !attackIds.Add(attack.Id)) io_report.Add(ValidationSeverity.Error, "enemy.attacks.id", "Original attack IDs must be unique semantic names.", i_source);
					if (!IsAttackDeliveryType(attack.Type) && attack.Type != "multiStage")
						io_report.Add(ValidationSeverity.Error, "enemy.attacks.type", "Original-enemy attacks support melee, hitscan, projectile, area, grab, or multiStage.", i_source);
					if (string.IsNullOrWhiteSpace(attack.Animation)) io_report.Add(ValidationSeverity.Error, "enemy.attacks.animation", "Original attacks require an animation clip name.", i_source);
					ValidateRequiredRange(attack.Chance, 0f, 1f, "attack-chance", io_report, i_source);
					ValidateRequiredRange(attack.Damage, 0f, 100000f, "attack-damage", io_report, i_source);
					ValidateRequiredRange(attack.CooldownSeconds, 0f, 600f, "attack-cooldown", io_report, i_source);
					ValidateRequiredRange(attack.InitiateRange, 0f, 1000f, "attack-initiate-range", io_report, i_source);
					ValidateRequiredRange(attack.HitRange, 0f, 1000f, "attack-hit-range", io_report, i_source);
					ValidateRequiredRange(attack.DurationSeconds, 0.01f, 600f, "attack-duration", io_report, i_source);
					ValidateRange(attack.HitTimeSeconds, 0f, 600f, "attack-hit-time", io_report, i_source);
					if (attack.HitTimeSeconds.HasValue && attack.DurationSeconds.HasValue && attack.HitTimeSeconds > attack.DurationSeconds)
						io_report.Add(ValidationSeverity.Error, "enemy.attacks.hit-time", "hitTimeSeconds cannot exceed durationSeconds.", i_source);
					ValidateAttackDelivery(attack, i_document.Visual, io_report, i_source);
				}
				else if (attack.Index < 0 || attack.Index > 31 || !attackIndexes.Add(attack.Index))
					io_report.Add(ValidationSeverity.Error, "enemy.attacks.index", "Inherited attack indexes must be unique values from 0 through 31.", i_source);
				ValidateRange(attack.Chance, 0f, 1f, "attack-chance", io_report, i_source);
				ValidateRange(attack.Damage, 0f, 100000f, "attack-damage", io_report, i_source);
				ValidateRange(attack.KnockbackX, 0f, 1000f, "attack-knockback-x", io_report, i_source);
				ValidateRange(attack.KnockbackY, 0f, 1000f, "attack-knockback-y", io_report, i_source);
				ValidateRange(attack.CooldownSeconds, 0f, 600f, "attack-cooldown", io_report, i_source);
				ValidateRange(attack.InitiateRange, 0f, 1000f, "attack-initiate-range", io_report, i_source);
				ValidateRange(attack.HitRange, 0f, 1000f, "attack-hit-range", io_report, i_source);
				ValidateRange(attack.DurationSeconds, 0.01f, 600f, "attack-duration", io_report, i_source);
			}
			if (i_original && (i_document.Attacks == null || i_document.Attacks.Count == 0))
				io_report.Add(ValidationSeverity.Error, "enemy.attacks.required", "Original enemies require at least one attack.", i_source);
			if (i_original) EnemyAttackAuthoring.ValidateStrategy(i_document.Attacks, i_document.Ai, io_report, i_source);
			bool usesGrab = (i_document.Attacks ?? new List<EnemyAttackDefinition>()).Exists(attack => attack != null
				&& (attack.Type == "grab" || (attack.Stages ?? new List<EnemyAttackStageDefinition>()).Exists(stage => stage != null && stage.Type == "grab")));
			if (i_original && usesGrab && (i_document.Behavior?.Modules == null
				|| !i_document.Behavior.Modules.Exists(module => module != null && module.Type == "downedFinisher")))
				io_report.Add(ValidationSeverity.Error, "enemy.attacks.grab-finisher", "Grab attacks require a downedFinisher behavior module.", i_source);
			if (i_document.Behavior != null)
			{
				ValidateRange(i_document.Behavior.VisionRange, 0.1f, 10000f, "vision-range", io_report, i_source);
				ValidateBehaviorModules(i_document.Behavior.Modules, i_original, io_report, i_source);
			}
			ValidateAnimationReferences(i_document.AnimationReferences, io_report, i_source);
			if (i_document.Animation != null) ValidateAnimation(i_document.Animation, i_document.AnimationReferences, i_original, i_document.Visual,
				i_document.Attacks, i_document.Behavior?.Modules, io_report, i_source);
			else if (i_original && (i_document.AnimationReferences == null || i_document.AnimationReferences.Count == 0))
				io_report.Add(ValidationSeverity.Error, "enemy.animation.required", "Original enemies require inline animation clips or animationRefs.", i_source);
			if (i_original)
			{
				EnemyAiAuthoring.Validate(i_document.Ai, io_report, i_source);
				foreach (EnemyBehaviorModuleDefinition module in i_document.Behavior?.Modules ?? new List<EnemyBehaviorModuleDefinition>())
				{
					if (module == null || module.Type != "downedFinisher") continue;
					foreach (EnemyFinisherPhaseDefinition phase in module.Phases ?? new List<EnemyFinisherPhaseDefinition>())
						if (phase != null && !string.IsNullOrWhiteSpace(phase.Animation)
							&& !HasAnimation(i_document, phase.Animation))
							io_report.Add(ValidationSeverity.Error, "enemy.behavior.finisher-phase-animation-missing",
								"Finisher phase references an unknown enemy animation clip: " + phase.Animation, i_source);
				}
				foreach (EnemyAttackDefinition attack in i_document.Attacks ?? new List<EnemyAttackDefinition>())
				{
					if (attack == null || string.IsNullOrWhiteSpace(attack.Animation)) continue;
					if (i_document.Animation?.Clips == null || !i_document.Animation.Clips.TryGetValue(attack.Animation, out EnemyAnimationClipDefinition attackClip))
					{
						if (!HasAnimation(i_document, attack.Animation))
							io_report.Add(ValidationSeverity.Error, "enemy.attacks.animation-missing", "Attack references an unknown animation clip: " + attack.Animation, i_source);
						continue;
					}
					int timedHitCount = attackClip.Events == null ? 0 : attackClip.Events.FindAll(item => item != null && item.Type == "attackHit").Count;
					bool hasTimedHit = timedHitCount > 0;
					if (!attack.HitTimeSeconds.HasValue && !hasTimedHit)
						io_report.Add(ValidationSeverity.Error, "enemy.attacks.hit-timing", "Original attacks require hitTimeSeconds or an attackHit event in their animation clip.", i_source);
					if (attack.Type == "multiStage" && (attack.Stages == null || timedHitCount != attack.Stages.Count))
						io_report.Add(ValidationSeverity.Error, "enemy.attacks.stage-events", "multiStage attacks require exactly one ordered attackHit animation event per stage.", i_source);
					float animationSpeed = i_document.Animation.SpeedMultiplier ?? 1f;
					foreach (EnemyAnimationEventDefinition animationEvent in attackClip.Events ?? new List<EnemyAnimationEventDefinition>())
						if (animationEvent != null && attack.DurationSeconds.HasValue && animationEvent.Time / animationSpeed > attack.DurationSeconds.Value)
							io_report.Add(ValidationSeverity.Error, "enemy.animation.event-after-attack", "Animation event at " + animationEvent.Time + " seconds occurs after attack " + attack.Id + " has ended.", i_source);
				}
			}
			if (i_document.Drops == null) return;
			ValidateRange(i_document.Drops.Chance, 0f, 1f, "drop-chance", io_report, i_source);
			HashSet<string> drops = new HashSet<string>(StringComparer.Ordinal);
			foreach (string item in i_document.Drops.Items ?? new List<string>())
				if (!ContentId.TryParse(item, out ContentId id) || !id.Path.StartsWith("item/", StringComparison.Ordinal) || !drops.Add(item))
					io_report.Add(ValidationSeverity.Error, "enemy.drops.item", "Drop items must be unique item content IDs: " + (item ?? "<null>"), i_source);
		}

		private static bool HasAnimation(EnemyDefinitionDocument i_document, string i_name)
		{
			return (i_document.Animation?.Clips != null && i_document.Animation.Clips.ContainsKey(i_name))
				|| (i_document.AnimationReferences != null && i_document.AnimationReferences.ContainsKey(i_name));
		}

		private static void ValidateAnimationReferences(Dictionary<string, string> i_references, ValidationReport io_report, string i_source)
		{
			if (i_references == null) return;
			if (i_references.Count > 64) io_report.Add(ValidationSeverity.Error, "enemy.animation-refs.limit", "animationRefs supports at most 64 clips.", i_source);
			foreach (KeyValuePair<string, string> pair in i_references)
			{
				if (!IsSemanticName(pair.Key)) io_report.Add(ValidationSeverity.Error, "enemy.animation-refs.name", "animationRefs keys must be semantic clip names.", i_source);
				if (!ContentId.TryParse(pair.Value, out ContentId id) || !id.Path.StartsWith("enemy-animation/", StringComparison.Ordinal))
					io_report.Add(ValidationSeverity.Error, "enemy.animation-refs.id", "animationRefs values must be enemy-animation content IDs.", i_source);
			}
		}

		private static void ValidateAttackDelivery(EnemyAttackDefinition i_attack, EnemyVisualDefinition i_visual,
			ValidationReport io_report, string i_source)
		{
			ValidateRange(i_attack.OffsetX, -1000f, 1000f, "attack-offset-x", io_report, i_source);
			ValidateRange(i_attack.OffsetY, -1000f, 1000f, "attack-offset-y", io_report, i_source);
			if (i_attack.Type == "multiStage")
			{
				if (i_attack.Stages == null || i_attack.Stages.Count < 2 || i_attack.Stages.Count > 16)
				{
					io_report.Add(ValidationSeverity.Error, "enemy.attacks.stages", "multiStage attacks require 2..16 stages.", i_source);
					return;
				}
				foreach (EnemyAttackStageDefinition stage in i_attack.Stages)
				{
					if (stage == null || !IsAttackDeliveryType(stage.Type))
					{
						io_report.Add(ValidationSeverity.Error, "enemy.attacks.stage-type", "Each attack stage must use melee, hitscan, projectile, area, or grab.", i_source);
						continue;
					}
					ValidateRange(stage.OffsetX, -1000f, 1000f, "attack-stage-offset-x", io_report, i_source);
					ValidateRange(stage.OffsetY, -1000f, 1000f, "attack-stage-offset-y", io_report, i_source);
					ValidateAttackDeliveryFields(stage.Type, stage.Damage ?? i_attack.Damage, stage.KnockbackX ?? i_attack.KnockbackX,
						stage.KnockbackY ?? i_attack.KnockbackY, stage.HitRange ?? i_attack.HitRange, stage.AreaRadius ?? i_attack.AreaRadius,
						stage.ProjectileRegion ?? i_attack.ProjectileRegion, stage.ProjectileSpeed ?? i_attack.ProjectileSpeed,
						stage.ProjectileLifetimeSeconds ?? i_attack.ProjectileLifetimeSeconds, stage.ProjectileRadius ?? i_attack.ProjectileRadius,
						stage.ProjectileGravityScale ?? i_attack.ProjectileGravityScale, i_visual, io_report, i_source);
				}
				return;
			}
			if (i_attack.Stages != null && i_attack.Stages.Count > 0)
				io_report.Add(ValidationSeverity.Error, "enemy.attacks.unexpected-stages", "Only multiStage attacks may define stages.", i_source);
			if (IsAttackDeliveryType(i_attack.Type))
				ValidateAttackDeliveryFields(i_attack.Type, i_attack.Damage, i_attack.KnockbackX, i_attack.KnockbackY, i_attack.HitRange,
					i_attack.AreaRadius, i_attack.ProjectileRegion, i_attack.ProjectileSpeed, i_attack.ProjectileLifetimeSeconds,
					i_attack.ProjectileRadius, i_attack.ProjectileGravityScale, i_visual, io_report, i_source);
		}

		private static void ValidateAttackDeliveryFields(string i_type, float? i_damage, float? i_knockbackX, float? i_knockbackY,
			float? i_hitRange, float? i_areaRadius, string i_projectileRegion, float? i_projectileSpeed,
			float? i_projectileLifetime, float? i_projectileRadius, float? i_projectileGravity, EnemyVisualDefinition i_visual,
			ValidationReport io_report, string i_source)
		{
			ValidateRequiredRange(i_damage, 0f, 100000f, "attack-delivery-damage", io_report, i_source);
			ValidateRange(i_knockbackX, 0f, 1000f, "attack-delivery-knockback-x", io_report, i_source);
			ValidateRange(i_knockbackY, 0f, 1000f, "attack-delivery-knockback-y", io_report, i_source);
			ValidateRequiredRange(i_hitRange, 0f, 1000f, "attack-delivery-hit-range", io_report, i_source);
			if (i_type == "area") ValidateRequiredRange(i_areaRadius, 0.05f, 100f, "attack-area-radius", io_report, i_source);
			if (i_type != "projectile") return;
			if (!IsSemanticName(i_projectileRegion) || i_visual?.Regions == null || !i_visual.Regions.ContainsKey(i_projectileRegion))
				io_report.Add(ValidationSeverity.Error, "enemy.attacks.projectile-region", "Projectile attacks require projectileRegion to reference an atlas region.", i_source);
			ValidateRequiredRange(i_projectileSpeed, 0.05f, 1000f, "attack-projectile-speed", io_report, i_source);
			ValidateRequiredRange(i_projectileLifetime, 0.05f, 60f, "attack-projectile-lifetime", io_report, i_source);
			ValidateRequiredRange(i_projectileRadius, 0.01f, 10f, "attack-projectile-radius", io_report, i_source);
			ValidateRange(i_projectileGravity, -10f, 10f, "attack-projectile-gravity", io_report, i_source);
		}

		private static bool IsAttackDeliveryType(string i_type)
		{
			return i_type == "melee" || i_type == "hitscan" || i_type == "projectile" || i_type == "area" || i_type == "grab";
		}

		private static void ValidateBehaviorModules(List<EnemyBehaviorModuleDefinition> i_modules, bool i_original, ValidationReport io_report, string i_source)
		{
			if (i_modules == null) return;
			if (i_modules.Count > 8) io_report.Add(ValidationSeverity.Error, "enemy.behavior.modules", "At most 8 behavior modules are supported.", i_source);
			HashSet<string> types = new HashSet<string>(StringComparer.Ordinal);
			foreach (EnemyBehaviorModuleDefinition module in i_modules)
			{
				if (module == null || !types.Add(module.Type ?? string.Empty))
				{
					io_report.Add(ValidationSeverity.Error, "enemy.behavior.module", "Behavior modules must be non-null and have unique types.", i_source);
					continue;
				}
				switch (module.Type)
				{
				case "regeneration":
					ValidateRequiredRange(module.Amount, 0.01f, 100000f, "regeneration-amount", io_report, i_source);
					ValidateRequiredRange(module.IntervalSeconds, 0.05f, 600f, "regeneration-interval", io_report, i_source);
					break;
				case "berserk":
					ValidateRequiredRange(module.HealthThreshold, 0.01f, 1f, "berserk-health-threshold", io_report, i_source);
					ValidateRange(module.SpeedBonus, -1000f, 1000f, "berserk-speed-bonus", io_report, i_source);
					ValidateRange(module.DamageMultiplierBonus, -100f, 100f, "berserk-damage-bonus", io_report, i_source);
					break;
				case "lifesteal":
					ValidateRequiredRange(module.Amount, 0.01f, 100000f, "lifesteal-amount", io_report, i_source);
					break;
				case "thorns":
					ValidateRequiredRange(module.Amount, 0.01f, 100000f, "thorns-amount", io_report, i_source);
					break;
				case "onHitRagdoll":
					ValidateRequiredRange(module.DurationSeconds, 0.05f, 30f, "on-hit-ragdoll-duration", io_report, i_source);
					break;
				case "spawnOnDeath":
					if (!ContentId.TryParse(module.Enemy, out ContentId spawnEnemy) || !spawnEnemy.Path.StartsWith("enemy/", StringComparison.Ordinal))
						io_report.Add(ValidationSeverity.Error, "enemy.behavior.spawn-on-death-enemy", "spawnOnDeath enemy must be an enemy content ID.", i_source);
					if (!module.Count.HasValue || module.Count.Value < 1 || module.Count.Value > 32)
						io_report.Add(ValidationSeverity.Error, "enemy.behavior.spawn-on-death-count", "spawnOnDeath count must be 1..32.", i_source);
					ValidateRange(module.Radius, 0f, 20f, "spawn-on-death-radius", io_report, i_source);
					break;
				case "speedPulse":
					ValidateRequiredRange(module.SpeedBonus, -1000f, 1000f, "speed-pulse-bonus", io_report, i_source);
					ValidateRequiredRange(module.IntervalSeconds, 0.1f, 600f, "speed-pulse-interval", io_report, i_source);
					ValidateRequiredRange(module.DurationSeconds, 0.05f, 600f, "speed-pulse-duration", io_report, i_source);
					if (module.IntervalSeconds.HasValue && module.DurationSeconds.HasValue && module.DurationSeconds.Value >= module.IntervalSeconds.Value)
						io_report.Add(ValidationSeverity.Error, "enemy.behavior.speed-pulse-overlap", "speedPulse durationSeconds must be shorter than intervalSeconds.", i_source);
					break;
				case "downedFinisher":
					if (!i_original) io_report.Add(ValidationSeverity.Error, "enemy.behavior.finisher-original", "downedFinisher is currently supported only by fully original enemies.", i_source);
					ValidateRequiredRange(module.TriggerRange, 0.1f, 20f, "finisher-trigger-range", io_report, i_source);
					ValidateRange(module.StartDelaySeconds, 0f, 10f, "finisher-start-delay", io_report, i_source);
					if (module.Phases != null && module.Phases.Count > 0)
					{
						if (module.Phases.Count < 2 || module.Phases.Count > 8)
							io_report.Add(ValidationSeverity.Error, "enemy.behavior.finisher-phases", "downedFinisher phases must contain 2..8 entries.", i_source);
						HashSet<string> phaseIds = new HashSet<string>(StringComparer.Ordinal);
						float totalDuration = 0f;
						foreach (EnemyFinisherPhaseDefinition phase in module.Phases)
						{
							if (phase == null || !IsSemanticName(phase.Id) || !phaseIds.Add(phase.Id))
							{
								io_report.Add(ValidationSeverity.Error, "enemy.behavior.finisher-phase-id", "Finisher phase IDs must be unique semantic names.", i_source);
								continue;
							}
							if (!IsSemanticName(phase.Animation))
								io_report.Add(ValidationSeverity.Error, "enemy.behavior.finisher-phase-animation", "Each finisher phase requires an enemy animation clip name.", i_source);
							if (!IsFinite(phase.DurationSeconds) || phase.DurationSeconds < 0.1f || phase.DurationSeconds > 60f)
								io_report.Add(ValidationSeverity.Error, "enemy.behavior.finisher-phase-duration", "Each finisher phase durationSeconds must be 0.1..60.", i_source);
							totalDuration += phase.DurationSeconds;
							ValidateRange(phase.InputPower, 0.01f, 10000f, "finisher-phase-input-power", io_report, i_source);
							ValidateFinisherInputPattern(phase.InputPattern, "finisher-phase-input-pattern", io_report, i_source);
							ValidateRange(phase.DecayPerSecond, 0f, 10000f, "finisher-phase-decay", io_report, i_source);
							ValidatePlayerFinisherAnimationSource(phase.PlayerAnimation, phase.PlayerAnimationReference, io_report, i_source);
						}
						if (totalDuration > 300f)
							io_report.Add(ValidationSeverity.Error, "enemy.behavior.finisher-phase-total", "Combined finisher phase duration cannot exceed 300 seconds.", i_source);
					}
					else ValidateRequiredRange(module.DurationSeconds, 1f, 60f, "finisher-duration", io_report, i_source);
					ValidateRequiredRange(module.MeterMax, 1f, 10000f, "finisher-meter-max", io_report, i_source);
					ValidateRequiredRange(module.InputPower, 0.01f, 10000f, "finisher-input-power", io_report, i_source);
					ValidateFinisherInputPattern(module.InputPattern, "finisher-input-pattern", io_report, i_source);
					ValidateRange(module.DecayPerSecond, 0f, 10000f, "finisher-decay", io_report, i_source);
					ValidateRange(module.FailureDamage, 0f, 100000f, "finisher-failure-damage", io_report, i_source);
					ValidateRange(module.SuccessRecoveryHealth, 0f, 100000f, "finisher-success-recovery", io_report, i_source);
					ValidateRange(module.SuccessStunSeconds, 0f, 30f, "finisher-success-stun", io_report, i_source);
					ValidateRange(module.CooldownSeconds, 0f, 600f, "finisher-cooldown", io_report, i_source);
					if (!string.IsNullOrEmpty(module.Animation) && !IsSemanticName(module.Animation))
						io_report.Add(ValidationSeverity.Error, "enemy.behavior.finisher-animation", "downedFinisher animation must be a semantic clip name.", i_source);
					ValidatePlayerFinisherAnimationSource(module.PlayerAnimation, module.PlayerAnimationReference, io_report, i_source);
					ValidateFinisherOutcome(module.SuccessOutcome, "success", io_report, i_source);
					ValidateFinisherOutcome(module.FailureOutcome, "failure", io_report, i_source);
					ValidateFinisherStatuses(module.Statuses, io_report, i_source);
					ValidateFinisherParticipants(module, io_report, i_source);
					break;
				default:
					io_report.Add(ValidationSeverity.Error, "enemy.behavior.module-type", "Unsupported behavior module: " + module.Type, i_source);
					break;
				}
			}
		}

		private static void ValidateFinisherParticipants(EnemyBehaviorModuleDefinition i_module,
			ValidationReport io_report, string i_source)
		{
			List<EnemyFinisherParticipantDefinition> participants = i_module.Participants ?? new List<EnemyFinisherParticipantDefinition>();
			if (participants.Count > 3)
				io_report.Add(ValidationSeverity.Error, "enemy.behavior.finisher-participants", "downedFinisher supports at most 3 secondary NPC participants.", i_source);
			HashSet<string> ids = new HashSet<string>(StringComparer.Ordinal);
			foreach (EnemyFinisherParticipantDefinition participant in participants)
			{
				if (participant == null || !IsSemanticName(participant.Id) || participant.Id == "owner" || !ids.Add(participant.Id))
				{
					io_report.Add(ValidationSeverity.Error, "enemy.behavior.finisher-participant-id", "Participant IDs must be unique semantic names and cannot be 'owner'.", i_source);
					continue;
				}
				if (!ContentId.TryParse(participant.Enemy, out ContentId enemy) || !enemy.Path.StartsWith("enemy/", StringComparison.Ordinal))
					io_report.Add(ValidationSeverity.Error, "enemy.behavior.finisher-participant-enemy", "Each participant enemy must be an enemy content ID.", i_source);
				string policy = string.IsNullOrEmpty(participant.JoinPolicy) ? "phaseBoundary" : participant.JoinPolicy;
				if (policy != "startOnly" && policy != "phaseBoundary")
					io_report.Add(ValidationSeverity.Error, "enemy.behavior.finisher-participant-policy", "Participant joinPolicy must be startOnly or phaseBoundary.", i_source);
				if (participant.Required && policy != "startOnly")
					io_report.Add(ValidationSeverity.Error, "enemy.behavior.finisher-participant-required", "Required participants must use joinPolicy startOnly.", i_source);
				if (policy == "phaseBoundary" && (i_module.Phases == null || i_module.Phases.Count == 0))
					io_report.Add(ValidationSeverity.Error, "enemy.behavior.finisher-participant-phases", "phaseBoundary participants require a phased finisher.", i_source);
				ValidateRange(participant.JoinRange, 0.1f, 20f, "finisher-participant-join-range", io_report, i_source);
				ValidateRange(participant.ApproachRange, 0.1f, 50f, "finisher-participant-approach-range", io_report, i_source);
				if (participant.JoinRange.HasValue && participant.ApproachRange.HasValue && participant.ApproachRange.Value < participant.JoinRange.Value)
					io_report.Add(ValidationSeverity.Error, "enemy.behavior.finisher-participant-approach", "Participant approachRange cannot be smaller than joinRange.", i_source);
				ValidateRange(participant.OffsetX, -20f, 20f, "finisher-participant-offset-x", io_report, i_source);
				ValidateRange(participant.OffsetY, -20f, 20f, "finisher-participant-offset-y", io_report, i_source);
				if (!string.IsNullOrEmpty(participant.Facing) && participant.Facing != "preserve" && participant.Facing != "left" && participant.Facing != "right")
					io_report.Add(ValidationSeverity.Error, "enemy.behavior.finisher-participant-facing", "Participant facing must be preserve, left, or right.", i_source);
				if (!string.IsNullOrEmpty(participant.Animation) && !IsSemanticName(participant.Animation))
					io_report.Add(ValidationSeverity.Error, "enemy.behavior.finisher-participant-animation", "Participant animation must be a semantic clip name.", i_source);
			}

			foreach (EnemyFinisherPhaseDefinition phase in i_module.Phases ?? new List<EnemyFinisherPhaseDefinition>())
				foreach (KeyValuePair<string, string> animation in phase?.ParticipantAnimations ?? new Dictionary<string, string>())
				{
					if (!ids.Contains(animation.Key))
						io_report.Add(ValidationSeverity.Error, "enemy.behavior.finisher-participant-animation-id", "participantAnimations references an unknown participant: " + animation.Key, i_source);
					if (!IsSemanticName(animation.Value))
						io_report.Add(ValidationSeverity.Error, "enemy.behavior.finisher-participant-animation-name", "Participant phase animations must be semantic clip names.", i_source);
				}
		}

		private static void ValidateFinisherInputPattern(string i_pattern, string i_code,
			ValidationReport io_report, string i_source)
		{
			if (string.IsNullOrEmpty(i_pattern)) return;
			if (i_pattern != "adaptive" && i_pattern != "alternate" && i_pattern != "rotate" && i_pattern != "tap")
				io_report.Add(ValidationSeverity.Error, "enemy.behavior." + i_code,
					"Finisher inputPattern must be adaptive, alternate, rotate, or tap.", i_source);
		}

		private static void ValidateFinisherStatuses(Dictionary<string, EnemyFinisherStatusTextDefinition> i_statuses,
			ValidationReport io_report, string i_source)
		{
			if (i_statuses == null) return;
			HashSet<string> allowed = new HashSet<string>(new[] { "active", "mating", "infusion", "succumbed", "orgasm",
				"fertilized", "pregnant", "implanting", "labor", "birth", "mindBroken" }, StringComparer.Ordinal);
			foreach (KeyValuePair<string, EnemyFinisherStatusTextDefinition> pair in i_statuses)
			{
				if (!allowed.Contains(pair.Key))
				{
					io_report.Add(ValidationSeverity.Error, "enemy.behavior.finisher-status-key", "Unsupported finisher status: " + pair.Key, i_source);
					continue;
				}
				EnemyFinisherStatusTextDefinition text = pair.Value;
				if (text == null || (text.Title == null && text.Description == null))
					io_report.Add(ValidationSeverity.Error, "enemy.behavior.finisher-status-empty", "Finisher status " + pair.Key + " must override title or description.", i_source);
				else
				{
					if (text.Title != null && (string.IsNullOrWhiteSpace(text.Title) || text.Title.Length > 64))
						io_report.Add(ValidationSeverity.Error, "enemy.behavior.finisher-status-title", "Finisher status titles must contain 1..64 characters.", i_source);
					if (text.Description != null && text.Description.Length > 256)
						io_report.Add(ValidationSeverity.Error, "enemy.behavior.finisher-status-description", "Finisher status descriptions support at most 256 characters.", i_source);
				}
			}
		}

		private static void ValidateFinisherOutcome(EnemyFinisherOutcomeDefinition i_outcome, string i_name,
			ValidationReport io_report, string i_source)
		{
			if (i_outcome == null) return;
			ValidateRange(i_outcome.HealthDamage, 0f, 100000f, "finisher-" + i_name + "-health-damage", io_report, i_source);
			ValidateRange(i_outcome.StrengthDamage, 0f, 100000f, "finisher-" + i_name + "-strength-damage", io_report, i_source);
			ValidateRange(i_outcome.Pleasure, 0f, 100f, "finisher-" + i_name + "-pleasure", io_report, i_source);
			ValidateRange(i_outcome.Libido, 0f, 10000f, "finisher-" + i_name + "-libido", io_report, i_source);
			ValidateRange(i_outcome.HealthRecovery, 0f, 100000f, "finisher-" + i_name + "-health-recovery", io_report, i_source);
			ValidateRange(i_outcome.EnemyStunSeconds, 0f, 30f, "finisher-" + i_name + "-enemy-stun", io_report, i_source);
			ValidateRange(i_outcome.PlayerRagdollSeconds, 0f, 30f, "finisher-" + i_name + "-player-ragdoll", io_report, i_source);
		}

		private static void ValidatePlayerFinisherAnimation(EnemyAnimationClipDefinition i_clip, ValidationReport io_report, string i_source)
		{
			if (i_clip == null) return;
			if (i_clip.DurationSeconds <= 0f || i_clip.DurationSeconds > 60f || i_clip.Frames == null || i_clip.Frames.Count == 0 || i_clip.Frames.Count > 256)
			{
				io_report.Add(ValidationSeverity.Error, "enemy.behavior.finisher-player-animation", "playerAnimation requires a bounded duration and 1..256 frames.", i_source);
				return;
			}
			HashSet<string> allowed = new HashSet<string>(new[] { "hips", "butt", "spine", "chest", "neck", "head", "arm-right-upper", "arm-right-lower", "hand-right", "arm-left-upper", "arm-left-lower", "hand-left", "leg-right-upper", "leg-right-lower", "foot-right", "leg-left-upper", "leg-left-lower", "foot-left", "ear", "face" }, StringComparer.Ordinal);
			float previous = -1f;
			foreach (EnemyAnimationFrameDefinition frame in i_clip.Frames)
			{
				if (frame == null || frame.Time < previous || frame.Time < 0f || frame.Time > i_clip.DurationSeconds || frame.Bones == null)
				{
					io_report.Add(ValidationSeverity.Error, "enemy.behavior.finisher-player-frame", "playerAnimation frame times must be ordered within the clip and include bone poses.", i_source);
					continue;
				}
				previous = frame.Time;
				foreach (KeyValuePair<string, EnemyBonePoseDefinition> pose in frame.Bones)
				{
					if (!allowed.Contains(pose.Key) || pose.Value == null)
						io_report.Add(ValidationSeverity.Error, "enemy.behavior.finisher-player-bone", "playerAnimation references an unsupported player bone: " + pose.Key, i_source);
					else if (!string.IsNullOrEmpty(pose.Value.Region))
						io_report.Add(ValidationSeverity.Error, "enemy.behavior.finisher-player-region", "playerAnimation cannot replace player sprite regions.", i_source);
				}
			}
			if (i_clip.Events == null || i_clip.Events.Count == 0) return;
			if (i_clip.Events.Count > 64)
				io_report.Add(ValidationSeverity.Error, "enemy.behavior.finisher-player-events", "playerAnimation supports at most 64 timed events.", i_source);
			float previousEventTime = -1f;
			foreach (EnemyAnimationEventDefinition animationEvent in i_clip.Events)
			{
				if (animationEvent == null || animationEvent.Time < previousEventTime || animationEvent.Time < 0f || animationEvent.Time > i_clip.DurationSeconds)
				{
					io_report.Add(ValidationSeverity.Error, "enemy.behavior.finisher-player-event-time", "playerAnimation event times must be ordered and fall within the clip duration.", i_source);
					continue;
				}
				previousEventTime = animationEvent.Time;
				float maximumAmount;
				switch (animationEvent.Type)
				{
				case "pleasure":
				case "scaledPleasure":
					maximumAmount = 100f;
					break;
				case "libido":
				case "struggleDamage":
					maximumAmount = 10000f;
					break;
				case "strengthDamage":
				case "healthDamage":
					maximumAmount = 100000f;
					break;
				default:
					maximumAmount = 0f;
					io_report.Add(ValidationSeverity.Error, "enemy.behavior.finisher-player-event-type", "Supported playerAnimation event types are pleasure, scaledPleasure, libido, strengthDamage, struggleDamage, and healthDamage.", i_source);
					break;
				}
				if (!animationEvent.Amount.HasValue || !IsFinite(animationEvent.Amount.Value) || animationEvent.Amount.Value <= 0f
					|| (maximumAmount > 0f && animationEvent.Amount.Value > maximumAmount))
					io_report.Add(ValidationSeverity.Error, "enemy.behavior.finisher-player-event-amount", "playerAnimation event amount must be greater than 0 and within the limit for its event type.", i_source);
				if (animationEvent.X.HasValue || animationEvent.Y.HasValue || animationEvent.RelativeToFacing.HasValue
					|| animationEvent.File != null || animationEvent.Volume.HasValue || animationEvent.Region != null || animationEvent.Bone != null
					|| animationEvent.DurationSeconds.HasValue || animationEvent.Scale.HasValue || animationEvent.SortingOrder.HasValue)
					io_report.Add(ValidationSeverity.Error, "enemy.behavior.finisher-player-event-fields", "playerAnimation finisher events cannot define fields belonging to enemy-side events.", i_source);
			}
		}

		private static void ValidatePlayerFinisherAnimationSource(EnemyAnimationClipDefinition i_clip, string i_reference,
			ValidationReport io_report, string i_source)
		{
			if (i_clip != null && !string.IsNullOrWhiteSpace(i_reference))
				io_report.Add(ValidationSeverity.Error, "enemy.behavior.finisher-player-animation-source", "Use playerAnimation or playerAnimationRef, not both.", i_source);
			if (!string.IsNullOrWhiteSpace(i_reference)
				&& (!ContentId.TryParse(i_reference, out ContentId id) || !id.Path.StartsWith("player-animation/", StringComparison.Ordinal)))
				io_report.Add(ValidationSeverity.Error, "enemy.behavior.finisher-player-animation-ref", "playerAnimationRef must be a player-animation content ID.", i_source);
			ValidatePlayerFinisherAnimation(i_clip, io_report, i_source);
		}

		private static void ValidateRequiredRange(float? i_value, float i_min, float i_max, string i_name, ValidationReport io_report, string i_source)
		{
			if (!i_value.HasValue)
			{
				io_report.Add(ValidationSeverity.Error, "enemy.behavior." + i_name, i_name + " is required.", i_source);
				return;
			}
			ValidateRange(i_value, i_min, i_max, i_name, io_report, i_source);
		}

		internal static void ValidateStats(EnemyStatsDefinition i_stats, ValidationReport io_report, string i_source)
		{
			if (i_stats == null) return;
			ValidateRange(i_stats.HealthMax, 0.01f, 1000000f, "healthMax", io_report, i_source);
			ValidateRange(i_stats.SpeedAcceleration, 0f, 1000f, "speedAcceleration", io_report, i_source);
			ValidateRange(i_stats.SpeedMax, 0f, 1000f, "speedMax", io_report, i_source);
			ValidateRange(i_stats.Traction, 0f, 1f, "traction", io_report, i_source);
			ValidateRange(i_stats.HealthIncreasePerWave, 0f, 1000000f, "healthIncreasePerWave", io_report, i_source);
			if (i_stats.Bounty.HasValue && i_stats.Bounty.Value < 0) io_report.Add(ValidationSeverity.Error, "enemy.stats.bounty", "bounty cannot be negative.", i_source);
		}

		private static void ValidateVisual(EnemyVisualDefinition i_visual, bool i_original, ValidationReport io_report, string i_source)
		{
			if (i_visual == null)
			{
				if (i_original)
					io_report.Add(ValidationSeverity.Error, "enemy.visual", "Original enemies require a visual definition.", i_source);
				return;
			}
			string expectedType = i_original ? "originalSkeletonAtlas" : "coreRigAtlas";
			if (!string.Equals(i_visual.Type, expectedType, StringComparison.Ordinal))
				io_report.Add(ValidationSeverity.Error, "enemy.visual.type", "visual.type must be '" + expectedType + "' for this enemy kind.", i_source);
			if (!ModPath.IsSafeRelativePath(i_visual.Atlas) || !i_visual.Atlas.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
				io_report.Add(ValidationSeverity.Error, "enemy.visual.atlas", "atlas must be a safe pack-relative PNG path.", i_source);
			if (float.IsNaN(i_visual.PixelsPerUnit) || float.IsInfinity(i_visual.PixelsPerUnit) || i_visual.PixelsPerUnit <= 0f || i_visual.PixelsPerUnit > 1024f)
				io_report.Add(ValidationSeverity.Error, "enemy.visual.pixels-per-unit", "pixelsPerUnit must be greater than 0 and at most 1024.", i_source);
			if (i_visual.Regions == null || i_visual.Regions.Count == 0)
			{
				io_report.Add(ValidationSeverity.Error, "enemy.visual.regions", "At least one atlas region is required.", i_source);
				return;
			}
			foreach (KeyValuePair<string, AtlasRegionDefinition> region in i_visual.Regions)
			{
				if (!ContentId.TryParse("slot:" + region.Key, out _)) io_report.Add(ValidationSeverity.Error, "enemy.visual.region-name", "Region name is not a safe semantic path: " + region.Key, i_source);
				if (region.Value == null || region.Value.X < 0 || region.Value.Y < 0 || region.Value.Width <= 0 || region.Value.Height <= 0)
					io_report.Add(ValidationSeverity.Error, "enemy.visual.region-rect", "Region rectangle must have a non-negative origin and positive size: " + region.Key, i_source);
			}
			if (!i_original) return;
			if (i_visual.Bones == null || i_visual.Bones.Count > 64) io_report.Add(ValidationSeverity.Error, "enemy.visual.bones", "Original skeletons require 1..64 bones.", i_source);
			if (i_visual.HitZones == null || i_visual.HitZones.Count > 64) io_report.Add(ValidationSeverity.Error, "enemy.visual.hit-zones", "Original skeletons require 1..64 hit zones.", i_source);
			ValidateRequiredRange(i_visual.BodyWidth, 0.05f, 100f, "body-width", io_report, i_source);
			ValidateRequiredRange(i_visual.BodyHeight, 0.05f, 100f, "body-height", io_report, i_source);
			HashSet<string> bones = new HashSet<string>(StringComparer.Ordinal);
			foreach (EnemyBoneDefinition bone in i_visual.Bones ?? new List<EnemyBoneDefinition>())
			{
				if (bone == null || !IsSemanticName(bone.Id) || !bones.Add(bone.Id)) { io_report.Add(ValidationSeverity.Error, "enemy.visual.bone-id", "Bone IDs must be unique semantic names.", i_source); continue; }
				if (!i_visual.Regions.ContainsKey(bone.Region ?? string.Empty)) io_report.Add(ValidationSeverity.Error, "enemy.visual.bone-region", "Bone references an unknown atlas region: " + bone.Region, i_source);
				if (!IsFinite(bone.X) || !IsFinite(bone.Y) || !IsFinite(bone.Rotation) || bone.PivotX < 0f || bone.PivotX > 1f || bone.PivotY < 0f || bone.PivotY > 1f)
					io_report.Add(ValidationSeverity.Error, "enemy.visual.bone-transform", "Bone transforms must be finite and pivots must be 0..1.", i_source);
			}
			if (bones.Count == 0 || !bones.Contains("hips")) io_report.Add(ValidationSeverity.Error, "enemy.visual.hips", "Original skeletons require a hips bone.", i_source);
			foreach (EnemyBoneDefinition bone in i_visual.Bones ?? new List<EnemyBoneDefinition>())
				if (bone != null && !string.IsNullOrWhiteSpace(bone.Parent) && (!bones.Contains(bone.Parent) || bone.Parent == bone.Id))
					io_report.Add(ValidationSeverity.Error, "enemy.visual.bone-parent", "Bone references an invalid parent: " + bone.Parent, i_source);
			Dictionary<string, string> parents = new Dictionary<string, string>(StringComparer.Ordinal);
			foreach (EnemyBoneDefinition bone in i_visual.Bones ?? new List<EnemyBoneDefinition>()) if (bone != null) parents[bone.Id ?? string.Empty] = bone.Parent;
			foreach (string boneId in bones)
			{
				HashSet<string> chain = new HashSet<string>(StringComparer.Ordinal);
				string current = boneId;
				while (!string.IsNullOrEmpty(current) && parents.TryGetValue(current, out string parent))
				{
					if (!chain.Add(current)) { io_report.Add(ValidationSeverity.Error, "enemy.visual.bone-cycle", "Skeleton parent hierarchy contains a cycle at " + current + ".", i_source); break; }
					current = parent;
				}
			}
			HashSet<string> hitBones = new HashSet<string>(StringComparer.Ordinal);
			foreach (EnemyHitZoneDefinition zone in i_visual.HitZones ?? new List<EnemyHitZoneDefinition>())
			{
				if (zone == null || !bones.Contains(zone.Bone ?? string.Empty) || !hitBones.Add(zone.Bone ?? string.Empty)) { io_report.Add(ValidationSeverity.Error, "enemy.visual.hit-zone-bone", "Hit zones require one unique existing bone.", i_source); continue; }
				if (zone.Shape == "box") { ValidateRequiredRange(zone.Width, 0.01f, 100f, "hit-zone-width", io_report, i_source); ValidateRequiredRange(zone.Height, 0.01f, 100f, "hit-zone-height", io_report, i_source); }
				else if (zone.Shape == "circle") ValidateRequiredRange(zone.Radius, 0.01f, 100f, "hit-zone-radius", io_report, i_source);
				else io_report.Add(ValidationSeverity.Error, "enemy.visual.hit-zone-shape", "Hit-zone shape must be box or circle.", i_source);
				if (zone.DamageMultiplier != "low" && zone.DamageMultiplier != "normal" && zone.DamageMultiplier != "critical" && zone.DamageMultiplier != "block")
					io_report.Add(ValidationSeverity.Error, "enemy.visual.hit-zone-damage", "damageMultiplier must be low, normal, critical, or block.", i_source);
			}
			if (hitBones.Count == 0) io_report.Add(ValidationSeverity.Error, "enemy.visual.hit-zones", "Original skeletons require at least one hit zone.", i_source);
		}

		private static void ValidateAnimation(EnemyAnimationDefinition i_animation, Dictionary<string, string> i_animationReferences, bool i_original, EnemyVisualDefinition i_visual,
			IEnumerable<EnemyAttackDefinition> i_attacks, IEnumerable<EnemyBehaviorModuleDefinition> i_modules,
			ValidationReport io_report, string i_source)
		{
			ValidateRange(i_animation.SpeedMultiplier, 0.01f, 10f, "animation-speed", io_report, i_source);
			if (!i_original) return;
			HashSet<string> bones = new HashSet<string>((i_visual?.Bones ?? new List<EnemyBoneDefinition>()).ConvertAll(item => item?.Id ?? string.Empty), StringComparer.Ordinal);
			if ((i_animation.Clips == null || !i_animation.Clips.ContainsKey("idle")) && (i_animationReferences == null || !i_animationReferences.ContainsKey("idle"))
				|| (i_animation.Clips == null || !i_animation.Clips.ContainsKey("move")) && (i_animationReferences == null || !i_animationReferences.ContainsKey("move")))
				io_report.Add(ValidationSeverity.Error, "enemy.animation.base-clips", "Original animation sets require idle and move clips.", i_source);
			if ((i_animation.Clips?.Count ?? 0) + (i_animationReferences?.Count ?? 0) > 32) io_report.Add(ValidationSeverity.Error, "enemy.animation.clips", "At most 32 inline and referenced clips are supported.", i_source);
			HashSet<string> eventClips = new HashSet<string>(StringComparer.Ordinal);
			foreach (EnemyAttackDefinition attack in i_attacks ?? new List<EnemyAttackDefinition>())
				if (attack != null && !string.IsNullOrWhiteSpace(attack.Animation)) eventClips.Add(attack.Animation);
			foreach (EnemyBehaviorModuleDefinition module in i_modules ?? new List<EnemyBehaviorModuleDefinition>())
			{
				if (module == null || module.Type != "downedFinisher") continue;
				if (!string.IsNullOrWhiteSpace(module.Animation)) eventClips.Add(module.Animation);
				foreach (EnemyFinisherPhaseDefinition phase in module.Phases ?? new List<EnemyFinisherPhaseDefinition>())
					if (phase != null && !string.IsNullOrWhiteSpace(phase.Animation)) eventClips.Add(phase.Animation);
			}
			foreach (KeyValuePair<string, EnemyAnimationClipDefinition> pair in i_animation.Clips ?? new Dictionary<string, EnemyAnimationClipDefinition>())
			{
				EnemyAnimationClipDefinition clip = pair.Value;
				if (!IsSemanticName(pair.Key) || clip == null || clip.DurationSeconds <= 0f || clip.DurationSeconds > 600f || clip.Frames == null || clip.Frames.Count == 0 || clip.Frames.Count > 256)
				{ io_report.Add(ValidationSeverity.Error, "enemy.animation.clip", "Clips need a semantic name, bounded duration, and 1..256 frames.", i_source); continue; }
				ValidateEnemyAnimationEvents(pair.Key, clip, eventClips, i_visual, io_report, i_source);
				float previous = -1f;
				foreach (EnemyAnimationFrameDefinition frame in clip.Frames)
				{
					if (frame == null || frame.Time < 0f || frame.Time > clip.DurationSeconds || frame.Time < previous || frame.Bones == null)
					{ io_report.Add(ValidationSeverity.Error, "enemy.animation.frame", "Frame times must be ordered within the clip and include bone poses.", i_source); continue; }
					previous = frame.Time;
					foreach (KeyValuePair<string, EnemyBonePoseDefinition> pose in frame.Bones)
					{
						if (!bones.Contains(pose.Key) || pose.Value == null) io_report.Add(ValidationSeverity.Error, "enemy.animation.pose-bone", "Animation references an unknown bone: " + pose.Key, i_source);
						else if (!string.IsNullOrWhiteSpace(pose.Value.Region) && (i_visual?.Regions == null || !i_visual.Regions.ContainsKey(pose.Value.Region)))
							io_report.Add(ValidationSeverity.Error, "enemy.animation.pose-region", "Animation references an unknown atlas region: " + pose.Value.Region, i_source);
					}
				}
			}
		}

		private static void ValidateEnemyAnimationEvents(string i_clipName, EnemyAnimationClipDefinition i_clip,
			HashSet<string> i_eventClips, EnemyVisualDefinition i_visual, ValidationReport io_report, string i_source)
		{
			if (i_clip.Events == null || i_clip.Events.Count == 0) return;
			if (i_clip.Events.Count > 64)
				io_report.Add(ValidationSeverity.Error, "enemy.animation.events", "Animation clips support at most 64 timed events.", i_source);
			if (i_clip.Loop)
				io_report.Add(ValidationSeverity.Error, "enemy.animation.event-loop", "Enemy gameplay events are not allowed on looping clips.", i_source);
			if (!i_eventClips.Contains(i_clipName))
				io_report.Add(ValidationSeverity.Error, "enemy.animation.event-clip", "Enemy animation events are allowed only on clips referenced by an attack or finisher.", i_source);
			float previous = -1f;
			foreach (EnemyAnimationEventDefinition animationEvent in i_clip.Events)
			{
				if (animationEvent == null || animationEvent.Time < previous || animationEvent.Time < 0f || animationEvent.Time > i_clip.DurationSeconds)
				{
					io_report.Add(ValidationSeverity.Error, "enemy.animation.event-time", "Enemy animation event times must be ordered and fall within the clip duration.", i_source);
					continue;
				}
				previous = animationEvent.Time;
				if (animationEvent.Type == "attackHit")
				{
					if (animationEvent.Amount.HasValue || animationEvent.X.HasValue || animationEvent.Y.HasValue || animationEvent.RelativeToFacing.HasValue
						|| animationEvent.File != null || animationEvent.Volume.HasValue || animationEvent.Region != null || animationEvent.Bone != null
						|| animationEvent.DurationSeconds.HasValue || animationEvent.Scale.HasValue || animationEvent.SortingOrder.HasValue || animationEvent.Cue != null || animationEvent.Index.HasValue)
						io_report.Add(ValidationSeverity.Error, "enemy.animation.attack-hit-fields", "attackHit does not accept amount or impulse fields.", i_source);
				}
				else if (animationEvent.Type == "impulse")
				{
					if (animationEvent.Amount.HasValue)
						io_report.Add(ValidationSeverity.Error, "enemy.animation.impulse-amount", "impulse uses x and y rather than amount.", i_source);
					if (animationEvent.File != null || animationEvent.Volume.HasValue || animationEvent.Region != null || animationEvent.Bone != null
						|| animationEvent.DurationSeconds.HasValue || animationEvent.Scale.HasValue || animationEvent.SortingOrder.HasValue || animationEvent.Cue != null || animationEvent.Index.HasValue)
						io_report.Add(ValidationSeverity.Error, "enemy.animation.impulse-fields", "impulse contains fields belonging to another event type.", i_source);
					if ((!animationEvent.X.HasValue && !animationEvent.Y.HasValue)
						|| (animationEvent.X.HasValue && (!IsFinite(animationEvent.X.Value) || animationEvent.X.Value < -1000f || animationEvent.X.Value > 1000f))
						|| (animationEvent.Y.HasValue && (!IsFinite(animationEvent.Y.Value) || animationEvent.Y.Value < -1000f || animationEvent.Y.Value > 1000f)))
						io_report.Add(ValidationSeverity.Error, "enemy.animation.impulse", "impulse requires a finite x or y value between -1000 and 1000.", i_source);
				}
				else if (animationEvent.Type == "sound")
				{
					if (!ModPath.IsSafeRelativePath(animationEvent.File)
						|| (!animationEvent.File.EndsWith(".wav", StringComparison.OrdinalIgnoreCase) && !animationEvent.File.EndsWith(".ogg", StringComparison.OrdinalIgnoreCase)))
						io_report.Add(ValidationSeverity.Error, "enemy.animation.sound-file", "sound requires a safe pack-relative WAV or OGG file.", i_source);
					if (animationEvent.Volume.HasValue && (!IsFinite(animationEvent.Volume.Value) || animationEvent.Volume.Value < 0f || animationEvent.Volume.Value > 1f))
						io_report.Add(ValidationSeverity.Error, "enemy.animation.sound-volume", "sound volume must be between 0 and 1.", i_source);
					if (animationEvent.Amount.HasValue || animationEvent.X.HasValue || animationEvent.Y.HasValue || animationEvent.RelativeToFacing.HasValue
						|| animationEvent.Region != null || animationEvent.Bone != null || animationEvent.DurationSeconds.HasValue || animationEvent.Scale.HasValue || animationEvent.SortingOrder.HasValue || animationEvent.Cue != null || animationEvent.Index.HasValue)
						io_report.Add(ValidationSeverity.Error, "enemy.animation.sound-fields", "sound contains fields belonging to another event type.", i_source);
				}
				else if (animationEvent.Type == "cameraShake")
				{
					if (!animationEvent.Amount.HasValue || !IsFinite(animationEvent.Amount.Value) || animationEvent.Amount.Value < 0f || animationEvent.Amount.Value > 1f)
						io_report.Add(ValidationSeverity.Error, "enemy.animation.camera-shake", "cameraShake amount must be between 0 and 1.", i_source);
					if (animationEvent.X.HasValue || animationEvent.Y.HasValue || animationEvent.RelativeToFacing.HasValue || animationEvent.File != null
						|| animationEvent.Volume.HasValue || animationEvent.Region != null || animationEvent.Bone != null || animationEvent.DurationSeconds.HasValue
						|| animationEvent.Scale.HasValue || animationEvent.SortingOrder.HasValue || animationEvent.Cue != null || animationEvent.Index.HasValue)
						io_report.Add(ValidationSeverity.Error, "enemy.animation.camera-shake-fields", "cameraShake contains fields belonging to another event type.", i_source);
				}
				else if (animationEvent.Type == "spriteEffect")
				{
					if (string.IsNullOrWhiteSpace(animationEvent.Region) || i_visual?.Regions == null || !i_visual.Regions.ContainsKey(animationEvent.Region))
						io_report.Add(ValidationSeverity.Error, "enemy.animation.effect-region", "spriteEffect region must reference an atlas region.", i_source);
					if (!string.IsNullOrWhiteSpace(animationEvent.Bone) && (i_visual?.Bones == null || !i_visual.Bones.Exists(item => item != null && item.Id == animationEvent.Bone)))
						io_report.Add(ValidationSeverity.Error, "enemy.animation.effect-bone", "spriteEffect bone must reference an enemy bone.", i_source);
					ValidateRange(animationEvent.DurationSeconds, 0.02f, 30f, "animation-effect-duration", io_report, i_source);
					ValidateRange(animationEvent.Scale, 0.01f, 100f, "animation-effect-scale", io_report, i_source);
					if (animationEvent.SortingOrder.HasValue && (animationEvent.SortingOrder.Value < -10000 || animationEvent.SortingOrder.Value > 10000))
						io_report.Add(ValidationSeverity.Error, "enemy.animation.effect-sorting", "spriteEffect sortingOrder must be between -10000 and 10000.", i_source);
					if (animationEvent.Amount.HasValue || animationEvent.RelativeToFacing.HasValue || animationEvent.File != null || animationEvent.Volume.HasValue || animationEvent.Cue != null || animationEvent.Index.HasValue)
						io_report.Add(ValidationSeverity.Error, "enemy.animation.effect-fields", "spriteEffect contains fields belonging to another event type.", i_source);
				}
				else if (animationEvent.Type == "cue")
				{
					if (!IsSemanticName(animationEvent.Cue) || animationEvent.Cue.Length > 80)
						io_report.Add(ValidationSeverity.Error, "enemy.animation.cue-name", "cue requires a semantic cue name of at most 80 characters.", i_source);
					if (animationEvent.Index.HasValue && (animationEvent.Index.Value < 0 || animationEvent.Index.Value > 255))
						io_report.Add(ValidationSeverity.Error, "enemy.animation.cue-index", "cue index must be between 0 and 255.", i_source);
					if (animationEvent.Amount.HasValue && (!IsFinite(animationEvent.Amount.Value) || animationEvent.Amount.Value < -100000f || animationEvent.Amount.Value > 100000f))
						io_report.Add(ValidationSeverity.Error, "enemy.animation.cue-amount", "cue amount must be finite and between -100000 and 100000.", i_source);
					if (animationEvent.X.HasValue || animationEvent.Y.HasValue || animationEvent.RelativeToFacing.HasValue || animationEvent.File != null
						|| animationEvent.Volume.HasValue || animationEvent.Region != null || animationEvent.Bone != null || animationEvent.DurationSeconds.HasValue
						|| animationEvent.Scale.HasValue || animationEvent.SortingOrder.HasValue)
						io_report.Add(ValidationSeverity.Error, "enemy.animation.cue-fields", "cue accepts only cue, index, and amount metadata.", i_source);
				}
				else io_report.Add(ValidationSeverity.Error, "enemy.animation.event-type", "Unsupported enemy animation event type: " + animationEvent.Type, i_source);
			}
		}

		private static bool IsSemanticName(string i_value)
		{
			return !string.IsNullOrWhiteSpace(i_value) && ContentId.TryParse("slot:" + i_value, out _);
		}

		private static bool IsFinite(float i_value) { return !float.IsNaN(i_value) && !float.IsInfinity(i_value); }

		private static void ValidateRange(float? i_value, float i_min, float i_max, string i_name, ValidationReport io_report, string i_source)
		{
			if (!i_value.HasValue) return;
			float value = i_value.Value;
			if (float.IsNaN(value) || float.IsInfinity(value) || value < i_min || value > i_max)
				io_report.Add(ValidationSeverity.Error, "enemy.stats." + ToKebab(i_name), i_name + " must be between " + i_min + " and " + i_max + ".", i_source);
		}

		private static string ToKebab(string i_name)
		{
			return i_name.Replace("Acceleration", "-acceleration").Replace("IncreasePerWave", "-increase-per-wave").Replace("Max", "-max").ToLowerInvariant();
		}

		private static void Error(EnemyDefinitionLoadResult io_result, string i_code, string i_message, string i_source)
		{
			io_result.Report.Add(ValidationSeverity.Error, "enemy." + i_code, i_message, i_source);
		}
	}
}
