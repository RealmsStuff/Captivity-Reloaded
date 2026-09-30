using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace CaptivityReloaded.Modding
{
	[JsonObject(MemberSerialization.OptIn)]
	public sealed class WeaponStatsDefinition
	{
		[JsonProperty("damage")]
		public int? Damage { get; set; }

		[JsonProperty("ammoMax")]
		public int? AmmoMax { get; set; }

		[JsonProperty("magazineSize")]
		public int? MagazineSize { get; set; }

		[JsonProperty("bulletsPerShot")]
		public int? BulletsPerShot { get; set; }

		[JsonProperty("penetration")]
		public int? Penetration { get; set; }

		[JsonProperty("rangeMultiplier")]
		public float? RangeMultiplier { get; set; }

		[JsonProperty("fireIntervalSeconds")]
		public float? FireIntervalSeconds { get; set; }

		[JsonProperty("recoil")]
		public float? Recoil { get; set; }

		[JsonProperty("movementRecoil")]
		public float? MovementRecoil { get; set; }

		[JsonProperty("knockbackX")]
		public float? KnockbackX { get; set; }

		[JsonProperty("knockbackY")]
		public float? KnockbackY { get; set; }

		[JsonProperty("reloadSeconds")] public float? ReloadSeconds { get; set; }
		[JsonProperty("equipSeconds")] public float? EquipSeconds { get; set; }
		[JsonProperty("weight")] public int? Weight { get; set; }
		[JsonProperty("value")] public int? Value { get; set; }
		[JsonProperty("infiniteAmmo")] public bool? InfiniteAmmo { get; set; }
		[JsonProperty("semiAutomatic")] public bool? SemiAutomatic { get; set; }
		[JsonProperty("marketable")] public bool? Marketable { get; set; }
		[JsonProperty("holdType")] public string HoldType { get; set; }
		[JsonProperty("reloadType")] public string ReloadType { get; set; }
		[JsonProperty("weaponType")] public string WeaponType { get; set; }
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class WeaponVisualDefinition
	{
		[JsonProperty("type", Required = Required.Always)]
		public string Type { get; set; }

		[JsonProperty("pixelsPerUnit")]
		public float PixelsPerUnit { get; set; } = 32f;

		[JsonProperty("sprites", Required = Required.Always)]
		public Dictionary<string, string> Sprites { get; set; }

		[JsonProperty("bundlePrefab")]
		public string BundlePrefab { get; set; }

		[JsonProperty("hiddenSlots")]
		public List<string> HiddenSlots { get; set; }

		[JsonProperty("pivotX")] public float? PivotX { get; set; }
		[JsonProperty("pivotY")] public float? PivotY { get; set; }
		[JsonProperty("muzzleOffsetX")] public float? MuzzleOffsetX { get; set; }
		[JsonProperty("muzzleOffsetY")] public float? MuzzleOffsetY { get; set; }
		[JsonProperty("colliderWidth")] public float? ColliderWidth { get; set; }
		[JsonProperty("colliderHeight")] public float? ColliderHeight { get; set; }
		[JsonProperty("colliderOffsetX")] public float? ColliderOffsetX { get; set; }
		[JsonProperty("colliderOffsetY")] public float? ColliderOffsetY { get; set; }
		[JsonProperty("sortingOrder")] public int? SortingOrder { get; set; }

		[JsonProperty("parts")]
		public Dictionary<string, WeaponVisualPartDefinition> Parts { get; set; }
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class WeaponVisualPartDefinition
	{
		[JsonProperty("pivotX")] public float? PivotX { get; set; }
		[JsonProperty("pivotY")] public float? PivotY { get; set; }
		[JsonProperty("offsetX")] public float? OffsetX { get; set; }
		[JsonProperty("offsetY")] public float? OffsetY { get; set; }
		[JsonProperty("rotationDegrees")] public float? RotationDegrees { get; set; }
		[JsonProperty("scaleX")] public float? ScaleX { get; set; }
		[JsonProperty("scaleY")] public float? ScaleY { get; set; }
		[JsonProperty("sortingOrder")] public int? SortingOrder { get; set; }
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class WeaponBehaviorDefinition
	{
		[JsonProperty("burstCount")] public int? BurstCount { get; set; }
		[JsonProperty("burstIntervalSeconds")] public float? BurstIntervalSeconds { get; set; }
		[JsonProperty("tracerColor")] public string TracerColor { get; set; }
		[JsonProperty("tracerThickness")] public int? TracerThickness { get; set; }
		[JsonProperty("tracerFrames")] public int? TracerFrames { get; set; }
		[JsonProperty("cameraShake")] public bool? CameraShake { get; set; }
		[JsonProperty("shooterKnockback")] public bool? ShooterKnockback { get; set; }
		[JsonProperty("soundVolume")] public float? SoundVolume { get; set; }
		[JsonProperty("playShootSound")] public bool? PlayShootSound { get; set; }
		[JsonProperty("showMuzzleFlash")] public bool? ShowMuzzleFlash { get; set; }
		[JsonProperty("shootSounds")] public List<string> ShootSounds { get; set; }
		[JsonProperty("reloadSounds")] public List<string> ReloadSounds { get; set; }
		[JsonProperty("muzzleFlashSprites")] public List<string> MuzzleFlashSprites { get; set; }
		[JsonProperty("effectPixelsPerUnit")] public float? EffectPixelsPerUnit { get; set; }
		[JsonProperty("casingSprite")] public string CasingSprite { get; set; }
		[JsonProperty("casingOn")] public string CasingOn { get; set; }
		[JsonProperty("casingForceX")] public float? CasingForceX { get; set; }
		[JsonProperty("casingForceY")] public float? CasingForceY { get; set; }
		[JsonProperty("casingLifetimeSeconds")] public float? CasingLifetimeSeconds { get; set; }
		[JsonProperty("casingOffsetX")] public float? CasingOffsetX { get; set; }
		[JsonProperty("casingOffsetY")] public float? CasingOffsetY { get; set; }
		[JsonProperty("casingAngularVelocity")] public float? CasingAngularVelocity { get; set; }
		[JsonProperty("casingGravityScale")] public float? CasingGravityScale { get; set; }
		[JsonProperty("muzzleFlashOffsetX")] public float? MuzzleFlashOffsetX { get; set; }
		[JsonProperty("muzzleFlashOffsetY")] public float? MuzzleFlashOffsetY { get; set; }
		[JsonProperty("muzzleFlashDurationSeconds")] public float? MuzzleFlashDurationSeconds { get; set; }
		[JsonProperty("muzzleLightIntensityMultiplier")] public float? MuzzleLightIntensityMultiplier { get; set; }
		[JsonProperty("projectile")] public WeaponProjectileDefinition Projectile { get; set; }
		[JsonProperty("melee")] public WeaponMeleeDefinition Melee { get; set; }
		[JsonProperty("charge")] public WeaponChargeDefinition Charge { get; set; }
		[JsonProperty("beam")] public WeaponBeamDefinition Beam { get; set; }
		[JsonProperty("throwable")] public WeaponThrowableDefinition Throwable { get; set; }
		[JsonProperty("alternateFire")] public WeaponAlternateFireDefinition AlternateFire { get; set; }
		[JsonProperty("animations")] public WeaponAnimationDefinition Animations { get; set; }
		[JsonProperty("ammunition")] public WeaponAmmunitionDefinition Ammunition { get; set; }
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class WeaponChargeDefinition
	{
		[JsonProperty("seconds")] public float? Seconds { get; set; }
		[JsonProperty("minimumDamageMultiplier")] public float? MinimumDamageMultiplier { get; set; }
		[JsonProperty("maximumDamageMultiplier")] public float? MaximumDamageMultiplier { get; set; }
		[JsonProperty("chargingText")] public string ChargingText { get; set; }
		[JsonProperty("readyText")] public string ReadyText { get; set; }
		[JsonProperty("chargingColor")] public string ChargingColor { get; set; }
		[JsonProperty("readyColor")] public string ReadyColor { get; set; }
		[JsonProperty("tintStrength")] public float? TintStrength { get; set; }
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class WeaponBeamDefinition
	{
		[JsonProperty("input")] public string Input { get; set; }
		[JsonProperty("durationSeconds")] public float? DurationSeconds { get; set; }
		[JsonProperty("tickIntervalSeconds")] public float? TickIntervalSeconds { get; set; }
		[JsonProperty("damageMultiplier")] public float? DamageMultiplier { get; set; }
		[JsonProperty("ammoPerTick")] public int? AmmoPerTick { get; set; }
		[JsonProperty("color")] public string Color { get; set; }
		[JsonProperty("thickness")] public int? Thickness { get; set; }
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class WeaponThrowableDefinition
	{
		[JsonProperty("throwSpeed")] public float? ThrowSpeed { get; set; }
		[JsonProperty("gravity")] public float? Gravity { get; set; }
		[JsonProperty("angularVelocity")] public float? AngularVelocity { get; set; }
		[JsonProperty("lifetimeSeconds")] public float? LifetimeSeconds { get; set; }
		[JsonProperty("impactDamageMultiplier")] public float? ImpactDamageMultiplier { get; set; }
		[JsonProperty("destroyOnImpact")] public bool? DestroyOnImpact { get; set; }
		[JsonProperty("explodeOnImpact")] public bool? ExplodeOnImpact { get; set; }
		[JsonProperty("fuseSeconds")] public float? FuseSeconds { get; set; }
		[JsonProperty("explosionRadius")] public float? ExplosionRadius { get; set; }
		[JsonProperty("explosionDamageMultiplier")] public float? ExplosionDamageMultiplier { get; set; }
		[JsonProperty("explosionForce")] public float? ExplosionForce { get; set; }
		[JsonProperty("tickIntervalSeconds")] public float? TickIntervalSeconds { get; set; }
		[JsonProperty("impactSounds")] public List<string> ImpactSounds { get; set; }
		[JsonProperty("tickSounds")] public List<string> TickSounds { get; set; }
		[JsonProperty("explosionSounds")] public List<string> ExplosionSounds { get; set; }
		[JsonProperty("explosionColor")] public string ExplosionColor { get; set; }
		[JsonProperty("explosionStyle")] public string ExplosionStyle { get; set; }
		[JsonProperty("igniteDurationSeconds")] public float? IgniteDurationSeconds { get; set; }
		[JsonProperty("igniteDamagePerTick")] public float? IgniteDamagePerTick { get; set; }
		[JsonProperty("igniteTicksPerSecond")] public float? IgniteTicksPerSecond { get; set; }
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class WeaponAlternateFireDefinition
	{
		[JsonProperty("mode")] public string Mode { get; set; }
		[JsonProperty("damageMultiplier")] public float? DamageMultiplier { get; set; }
		[JsonProperty("ammoCost")] public int? AmmoCost { get; set; }
		[JsonProperty("cooldownSeconds")] public float? CooldownSeconds { get; set; }
		[JsonProperty("burstCount")] public int? BurstCount { get; set; }
		[JsonProperty("burstIntervalSeconds")] public float? BurstIntervalSeconds { get; set; }
		[JsonProperty("recoilMultiplier")] public float? RecoilMultiplier { get; set; }
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class WeaponAmmunitionDefinition
	{
		[JsonProperty("type")] public string Type { get; set; }
		[JsonProperty("damageMultiplier")] public float? DamageMultiplier { get; set; }
		[JsonProperty("penetrationBonus")] public int? PenetrationBonus { get; set; }
		[JsonProperty("weakpointMultiplier")] public float? WeakpointMultiplier { get; set; }
		[JsonProperty("damageOverTime")] public float? DamageOverTime { get; set; }
		[JsonProperty("effectDurationSeconds")] public float? EffectDurationSeconds { get; set; }
		[JsonProperty("tickIntervalSeconds")] public float? TickIntervalSeconds { get; set; }
		[JsonProperty("slowMultiplier")] public float? SlowMultiplier { get; set; }
		[JsonProperty("slowDurationSeconds")] public float? SlowDurationSeconds { get; set; }
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class WeaponAnimationDefinition
	{
		[JsonProperty("idleState")] public string IdleState { get; set; }
		[JsonProperty("fireState")] public string FireState { get; set; }
		[JsonProperty("reloadState")] public string ReloadState { get; set; }
		[JsonProperty("fireSpeedMultiplier")] public float? FireSpeedMultiplier { get; set; }
		[JsonProperty("reloadSpeedMultiplier")] public float? ReloadSpeedMultiplier { get; set; }
		[JsonProperty("clips")] public Dictionary<string, WeaponSpriteAnimationClipDefinition> Clips { get; set; }
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class WeaponSpriteAnimationClipDefinition
	{
		[JsonProperty("loop")] public bool? Loop { get; set; }
		[JsonProperty("frames", Required = Required.Always)] public List<WeaponSpriteAnimationFrameDefinition> Frames { get; set; }
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class WeaponSpriteAnimationFrameDefinition
	{
		[JsonProperty("durationSeconds", Required = Required.Always)] public float DurationSeconds { get; set; }
		[JsonProperty("sprites", Required = Required.Always)] public Dictionary<string, string> Sprites { get; set; }
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class WeaponMeleeDefinition
	{
		[JsonProperty("input")] public string Input { get; set; }
		[JsonProperty("range")] public float? Range { get; set; }
		[JsonProperty("radius")] public float? Radius { get; set; }
		[JsonProperty("damageMultiplier")] public float? DamageMultiplier { get; set; }
		[JsonProperty("knockbackX")] public float? KnockbackX { get; set; }
		[JsonProperty("knockbackY")] public float? KnockbackY { get; set; }
		[JsonProperty("maxTargets")] public int? MaxTargets { get; set; }
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class WeaponProjectileDefinition
	{
		[JsonProperty("mode")] public string Mode { get; set; }
		[JsonProperty("sprite")] public string Sprite { get; set; }
		[JsonProperty("pixelsPerUnit")] public float? PixelsPerUnit { get; set; }
		[JsonProperty("speed")] public float? Speed { get; set; }
		[JsonProperty("gravity")] public float? Gravity { get; set; }
		[JsonProperty("lifetimeSeconds")] public float? LifetimeSeconds { get; set; }
		[JsonProperty("impactSprites")] public List<string> ImpactSprites { get; set; }
		[JsonProperty("impactLifetimeSeconds")] public float? ImpactLifetimeSeconds { get; set; }
		[JsonProperty("explosionRadius")] public float? ExplosionRadius { get; set; }
		[JsonProperty("explosionDamageMultiplier")] public float? ExplosionDamageMultiplier { get; set; }
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class WeaponDefinitionDocument
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
		public WeaponStatsDefinition Stats { get; set; }

		[JsonProperty("visual", Required = Required.Always)]
		public WeaponVisualDefinition Visual { get; set; }

		[JsonProperty("behavior")]
		public WeaponBehaviorDefinition Behavior { get; set; }
	}

	public sealed class WeaponDefinition
	{
		public ContentId Id { get; }
		public ContentId? Extends { get; }
		public bool IsOriginal => !Extends.HasValue;
		public string PackId { get; }
		public string Source { get; }
		public string DisplayName { get; }
		public string Description { get; }
		public WeaponStatsDefinition Stats { get; }
		public WeaponVisualDefinition Visual { get; }
		public WeaponBehaviorDefinition Behavior { get; }

		public WeaponDefinition(ContentId i_id, ContentId? i_extends, string i_packId, string i_source, WeaponDefinitionDocument i_document)
		{
			Id = i_id;
			Extends = i_extends;
			PackId = i_packId;
			Source = i_source;
			DisplayName = i_document.DisplayName;
			Description = i_document.Description ?? string.Empty;
			Stats = i_document.Stats ?? new WeaponStatsDefinition();
			Visual = i_document.Visual;
			Behavior = i_document.Behavior ?? new WeaponBehaviorDefinition();
		}
	}

	public sealed class WeaponDefinitionLoadResult
	{
		public WeaponDefinition Definition { get; internal set; }
		public ValidationReport Report { get; } = new ValidationReport();
	}

	public static class WeaponDefinitionParser
	{
		public const int SupportedSchemaVersion = 1;
		public static WeaponDefinitionLoadResult Parse(string i_json, string i_packId, string i_source)
		{
			WeaponDefinitionLoadResult result = new WeaponDefinitionLoadResult();
			WeaponDefinitionDocument document;
			try
			{
				document = JsonConvert.DeserializeObject<WeaponDefinitionDocument>(i_json, new JsonSerializerSettings
				{
					MissingMemberHandling = MissingMemberHandling.Error
				});
			}
			catch (JsonException exception)
			{
				result.Report.Add(ValidationSeverity.Error, "weapon.json", exception.Message, i_source);
				return result;
			}
			if (document == null)
			{
				result.Report.Add(ValidationSeverity.Error, "weapon.null", "Weapon definition resolved to null.", i_source);
				return result;
			}

			if (document.SchemaVersion != SupportedSchemaVersion) Error(result, "schema-version", "Unsupported schemaVersion " + document.SchemaVersion + ".", i_source);
			if (!string.Equals(document.Type, "weapon", StringComparison.Ordinal)) Error(result, "type", "Definition type must be 'weapon'.", i_source);
			if (!ContentId.TryParse(document.Id, out ContentId id) || id.Namespace != i_packId || !id.Path.StartsWith("item/weapon/", StringComparison.Ordinal))
				Error(result, "id", "Weapon ID must use the defining pack namespace and an item/weapon/ path.", i_source);
			bool original = document.Visual != null && string.Equals(document.Visual.Type, "originalWeaponSprites", StringComparison.Ordinal);
			bool extendsSpecified = !string.IsNullOrWhiteSpace(document.Extends);
			ContentId extends = default(ContentId);
			bool hasTemplate = extendsSpecified && ContentId.TryParse(document.Extends, out extends)
				&& WeaponTemplateCatalog.IsSupportedTemplate(extends);
			if (original && extendsSpecified)
				Error(result, "extends", "originalWeaponSprites must not inherit a Core weapon.", i_source);
			else if (!original && !hasTemplate)
				Error(result, "extends", "coreWeaponSprites requires extends to reference a published Core weapon template.", i_source);
			if (string.IsNullOrWhiteSpace(document.DisplayName)) Error(result, "display-name", "displayName is required.", i_source);
			ValidateStats(document.Stats, result.Report, i_source);
			if (original) ValidateOriginalStats(document.Stats, result.Report, i_source);
			ValidateVisual(document.Visual, i_packId, hasTemplate ? extends : default(ContentId), hasTemplate, result.Report, i_source);
			ValidateBehavior(document.Behavior, document.Visual, hasTemplate ? extends : default(ContentId), hasTemplate, original, result.Report, i_source);
			if (result.Report.IsValid) result.Definition = new WeaponDefinition(id, hasTemplate ? extends : (ContentId?)null, i_packId, i_source, document);
			return result;
		}

		private static void ValidateBehavior(WeaponBehaviorDefinition i_behavior, WeaponVisualDefinition i_visual, ContentId i_template, bool i_hasTemplate, bool i_original, ValidationReport io_report, string i_source)
		{
			if (i_behavior == null) return;
			ValidateBehaviorRange(i_behavior.BurstCount, 1, 12, "burst-count", io_report, i_source);
			ValidateBehaviorRange(i_behavior.BurstIntervalSeconds, 0.02f, 2f, "burst-interval-seconds", io_report, i_source);
			if (i_behavior.BurstCount.GetValueOrDefault(1) > 1 && !i_behavior.BurstIntervalSeconds.HasValue)
				io_report.Add(ValidationSeverity.Error, "weapon.behavior.burst-interval", "burstIntervalSeconds is required when burstCount is greater than one.", i_source);
			ValidateBehaviorRange(i_behavior.TracerThickness, 1, 32, "tracer-thickness", io_report, i_source);
			ValidateBehaviorRange(i_behavior.TracerFrames, 1, 60, "tracer-frames", io_report, i_source);
			ValidateBehaviorRange(i_behavior.SoundVolume, 0f, 1f, "sound-volume", io_report, i_source);
			ValidateBehaviorRange(i_behavior.EffectPixelsPerUnit, 1f, 1024f, "effect-pixels-per-unit", io_report, i_source);
			ValidateBehaviorRange(i_behavior.CasingForceX, -100f, 100f, "casing-force-x", io_report, i_source);
			ValidateBehaviorRange(i_behavior.CasingForceY, -100f, 100f, "casing-force-y", io_report, i_source);
			ValidateBehaviorRange(i_behavior.CasingLifetimeSeconds, 0.1f, 60f, "casing-lifetime-seconds", io_report, i_source);
			ValidateBehaviorRange(i_behavior.CasingOffsetX, -10f, 10f, "casing-offset-x", io_report, i_source);
			ValidateBehaviorRange(i_behavior.CasingOffsetY, -10f, 10f, "casing-offset-y", io_report, i_source);
			ValidateBehaviorRange(i_behavior.CasingAngularVelocity, -10000f, 10000f, "casing-angular-velocity", io_report, i_source);
			ValidateBehaviorRange(i_behavior.CasingGravityScale, 0f, 20f, "casing-gravity-scale", io_report, i_source);
			ValidateBehaviorRange(i_behavior.MuzzleFlashOffsetX, -10f, 10f, "muzzle-flash-offset-x", io_report, i_source);
			ValidateBehaviorRange(i_behavior.MuzzleFlashOffsetY, -10f, 10f, "muzzle-flash-offset-y", io_report, i_source);
			ValidateBehaviorRange(i_behavior.MuzzleFlashDurationSeconds, 0.01f, 2f, "muzzle-flash-duration", io_report, i_source);
			ValidateBehaviorRange(i_behavior.MuzzleLightIntensityMultiplier, 0f, 10f, "muzzle-light-intensity", io_report, i_source);
			if (!string.IsNullOrWhiteSpace(i_behavior.TracerColor) && !IsColor(i_behavior.TracerColor))
				io_report.Add(ValidationSeverity.Error, "weapon.behavior.tracer-color", "tracerColor must use #RRGGBB or #RRGGBBAA.", i_source);
			if (!string.IsNullOrWhiteSpace(i_behavior.CasingOn) && i_behavior.CasingOn != "shoot" && i_behavior.CasingOn != "reload" && i_behavior.CasingOn != "both")
				io_report.Add(ValidationSeverity.Error, "weapon.behavior.casing-on", "casingOn must be shoot, reload, or both.", i_source);
			ValidateAssets(i_behavior.ShootSounds, 16, new[] { ".wav", ".ogg" }, "shoot-sounds", io_report, i_source);
			ValidateAssets(i_behavior.ReloadSounds, 16, new[] { ".wav", ".ogg" }, "reload-sounds", io_report, i_source);
			ValidateAssets(i_behavior.MuzzleFlashSprites, 16, new[] { ".png" }, "muzzle-flash-sprites", io_report, i_source);
			if (!string.IsNullOrWhiteSpace(i_behavior.CasingSprite))
				ValidateAssets(new[] { i_behavior.CasingSprite }, 1, new[] { ".png" }, "casing-sprite", io_report, i_source);
			ValidateProjectile(i_behavior.Projectile, io_report, i_source);
			ValidateMelee(i_behavior.Melee, io_report, i_source);
			ValidateCharge(i_behavior.Charge, io_report, i_source);
			ValidateBeam(i_behavior.Beam, io_report, i_source);
			ValidateThrowable(i_behavior.Throwable, i_behavior, io_report, i_source);
			ValidateAlternate(i_behavior.AlternateFire, i_behavior, io_report, i_source);
			ValidateAmmunition(i_behavior.Ammunition, io_report, i_source);
			ValidateAnimations(i_behavior.Animations, i_visual, i_template, i_hasTemplate, i_original, io_report, i_source);
		}

		private static void ValidateThrowable(WeaponThrowableDefinition i_throwable, WeaponBehaviorDefinition i_behavior,
			ValidationReport io_report, string i_source)
		{
			if (i_throwable == null) return;
			ValidateBehaviorRange(i_throwable.ThrowSpeed, 0.1f, 100f, "throwable-speed", io_report, i_source);
			ValidateBehaviorRange(i_throwable.Gravity, 0f, 20f, "throwable-gravity", io_report, i_source);
			ValidateBehaviorRange(i_throwable.AngularVelocity, -10000f, 10000f, "throwable-angular-velocity", io_report, i_source);
			ValidateBehaviorRange(i_throwable.LifetimeSeconds, 0.1f, 120f, "throwable-lifetime", io_report, i_source);
			ValidateBehaviorRange(i_throwable.ImpactDamageMultiplier, 0f, 25f, "throwable-impact-damage", io_report, i_source);
			ValidateBehaviorRange(i_throwable.FuseSeconds, 0.05f, 60f, "throwable-fuse", io_report, i_source);
			ValidateBehaviorRange(i_throwable.ExplosionRadius, 0f, 25f, "throwable-explosion-radius", io_report, i_source);
			ValidateBehaviorRange(i_throwable.ExplosionDamageMultiplier, 0f, 25f, "throwable-explosion-damage", io_report, i_source);
			ValidateBehaviorRange(i_throwable.ExplosionForce, 0f, 1000f, "throwable-explosion-force", io_report, i_source);
			ValidateBehaviorRange(i_throwable.TickIntervalSeconds, 0.05f, 10f, "throwable-tick-interval", io_report, i_source);
			ValidateBehaviorRange(i_throwable.IgniteDurationSeconds, 0.1f, 60f, "throwable-ignite-duration", io_report, i_source);
			ValidateBehaviorRange(i_throwable.IgniteDamagePerTick, 0f, 1000f, "throwable-ignite-damage", io_report, i_source);
			ValidateBehaviorRange(i_throwable.IgniteTicksPerSecond, 0.1f, 20f, "throwable-ignite-rate", io_report, i_source);
			ValidateAssets(i_throwable.ImpactSounds, 16, new[] { ".wav", ".ogg" }, "throwable-impact-sounds", io_report, i_source);
			ValidateAssets(i_throwable.TickSounds, 16, new[] { ".wav", ".ogg" }, "throwable-tick-sounds", io_report, i_source);
			ValidateAssets(i_throwable.ExplosionSounds, 16, new[] { ".wav", ".ogg" }, "throwable-explosion-sounds", io_report, i_source);
			if (!string.IsNullOrWhiteSpace(i_throwable.ExplosionColor) && !IsColor(i_throwable.ExplosionColor))
				io_report.Add(ValidationSeverity.Error, "weapon.throwable.explosion-color", "throwable.explosionColor must use #RRGGBB or #RRGGBBAA.", i_source);
			if (!string.IsNullOrWhiteSpace(i_throwable.ExplosionStyle) && i_throwable.ExplosionStyle != "burst" && i_throwable.ExplosionStyle != "fire")
				io_report.Add(ValidationSeverity.Error, "weapon.throwable.explosion-style", "throwable.explosionStyle must be burst or fire.", i_source);
			if (i_throwable.ExplosionRadius.GetValueOrDefault() > 0f && i_throwable.ExplosionDamageMultiplier.GetValueOrDefault() <= 0f)
				io_report.Add(ValidationSeverity.Error, "weapon.throwable.explosion-damage", "A throwable explosionRadius requires a positive explosionDamageMultiplier.", i_source);
			if (i_throwable.ExplodeOnImpact == true && i_throwable.ExplosionRadius.GetValueOrDefault() <= 0f)
				io_report.Add(ValidationSeverity.Error, "weapon.throwable.explode-on-impact", "throwable.explodeOnImpact requires a positive explosionRadius.", i_source);
			if (i_throwable.IgniteDurationSeconds.HasValue && (!i_throwable.IgniteDamagePerTick.HasValue || !i_throwable.IgniteTicksPerSecond.HasValue))
				io_report.Add(ValidationSeverity.Error, "weapon.throwable.ignite", "throwable igniteDurationSeconds requires igniteDamagePerTick and igniteTicksPerSecond.", i_source);
			if (i_behavior.Projectile != null || i_behavior.Melee != null || i_behavior.Charge != null || i_behavior.Beam != null)
				io_report.Add(ValidationSeverity.Error, "weapon.throwable.primary-conflict", "throwable cannot be combined with projectile, melee, charge, or beam primary behavior.", i_source);
		}

		private static void ValidateCharge(WeaponChargeDefinition i_charge, ValidationReport io_report, string i_source)
		{
			if (i_charge == null) return;
			ValidateBehaviorRange(i_charge.Seconds, 0.1f, 10f, "charge-seconds", io_report, i_source);
			ValidateBehaviorRange(i_charge.MinimumDamageMultiplier, 0.01f, 10f, "charge-minimum-damage", io_report, i_source);
			ValidateBehaviorRange(i_charge.MaximumDamageMultiplier, 0.01f, 25f, "charge-maximum-damage", io_report, i_source);
			if ((i_charge.MinimumDamageMultiplier ?? 0.25f) > (i_charge.MaximumDamageMultiplier ?? 2f))
				io_report.Add(ValidationSeverity.Error, "weapon.charge.damage-range", "minimumDamageMultiplier cannot exceed maximumDamageMultiplier.", i_source);
			ValidateBehaviorRange(i_charge.TintStrength, 0f, 1f, "charge-tint-strength", io_report, i_source);
			ValidateShortText(i_charge.ChargingText, "charge-charging-text", io_report, i_source);
			ValidateShortText(i_charge.ReadyText, "charge-ready-text", io_report, i_source);
			if (!string.IsNullOrWhiteSpace(i_charge.ChargingColor) && !IsColor(i_charge.ChargingColor))
				io_report.Add(ValidationSeverity.Error, "weapon.charge.charging-color", "charge.chargingColor must use #RRGGBB or #RRGGBBAA.", i_source);
			if (!string.IsNullOrWhiteSpace(i_charge.ReadyColor) && !IsColor(i_charge.ReadyColor))
				io_report.Add(ValidationSeverity.Error, "weapon.charge.ready-color", "charge.readyColor must use #RRGGBB or #RRGGBBAA.", i_source);
		}

		private static void ValidateBeam(WeaponBeamDefinition i_beam, ValidationReport io_report, string i_source)
		{
			if (i_beam == null) return;
			string input = string.IsNullOrWhiteSpace(i_beam.Input) ? "primary" : i_beam.Input;
			if (input != "primary" && input != "alternate") io_report.Add(ValidationSeverity.Error, "weapon.beam.input", "beam.input must be primary or alternate.", i_source);
			ValidateBehaviorRange(i_beam.DurationSeconds, 0.05f, 10f, "beam-duration", io_report, i_source);
			ValidateBehaviorRange(i_beam.TickIntervalSeconds, 0.02f, 2f, "beam-tick-interval", io_report, i_source);
			ValidateBehaviorRange(i_beam.DamageMultiplier, 0f, 10f, "beam-damage-multiplier", io_report, i_source);
			ValidateBehaviorRange(i_beam.AmmoPerTick, 0, 1000, "beam-ammo-per-tick", io_report, i_source);
			ValidateBehaviorRange(i_beam.Thickness, 1, 32, "beam-thickness", io_report, i_source);
			if (!string.IsNullOrWhiteSpace(i_beam.Color) && !IsColor(i_beam.Color)) io_report.Add(ValidationSeverity.Error, "weapon.beam.color", "beam.color must use #RRGGBB or #RRGGBBAA.", i_source);
		}

		private static void ValidateAlternate(WeaponAlternateFireDefinition i_alternate, WeaponBehaviorDefinition i_behavior,
			ValidationReport io_report, string i_source)
		{
			if (i_alternate == null) return;
			if (i_alternate.Mode != "hitscan" && i_alternate.Mode != "projectile" && i_alternate.Mode != "melee" && i_alternate.Mode != "beam")
				io_report.Add(ValidationSeverity.Error, "weapon.alternate.mode", "alternateFire.mode must be hitscan, projectile, melee, or beam.", i_source);
			if (i_alternate.Mode == "projectile" && i_behavior.Projectile == null) io_report.Add(ValidationSeverity.Error, "weapon.alternate.projectile", "A projectile alternate fire requires behavior.projectile.", i_source);
			if (i_alternate.Mode == "melee" && i_behavior.Melee == null) io_report.Add(ValidationSeverity.Error, "weapon.alternate.melee", "A melee alternate fire requires behavior.melee.", i_source);
			if (i_alternate.Mode == "beam" && i_behavior.Beam == null) io_report.Add(ValidationSeverity.Error, "weapon.alternate.beam", "A beam alternate fire requires behavior.beam.", i_source);
			ValidateBehaviorRange(i_alternate.DamageMultiplier, 0f, 25f, "alternate-damage-multiplier", io_report, i_source);
			ValidateBehaviorRange(i_alternate.AmmoCost, 0, 10000, "alternate-ammo-cost", io_report, i_source);
			ValidateBehaviorRange(i_alternate.CooldownSeconds, 0.02f, 30f, "alternate-cooldown", io_report, i_source);
			ValidateBehaviorRange(i_alternate.BurstCount, 1, 12, "alternate-burst-count", io_report, i_source);
			ValidateBehaviorRange(i_alternate.BurstIntervalSeconds, 0.02f, 2f, "alternate-burst-interval", io_report, i_source);
			ValidateBehaviorRange(i_alternate.RecoilMultiplier, 0f, 10f, "alternate-recoil-multiplier", io_report, i_source);
			if (i_alternate.BurstCount.GetValueOrDefault(1) > 1 && !i_alternate.BurstIntervalSeconds.HasValue)
				io_report.Add(ValidationSeverity.Error, "weapon.alternate.burst-interval", "alternateFire.burstIntervalSeconds is required when burstCount is greater than one.", i_source);
		}

		private static void ValidateAmmunition(WeaponAmmunitionDefinition i_ammo, ValidationReport io_report, string i_source)
		{
			if (i_ammo == null) return;
			string type = string.IsNullOrWhiteSpace(i_ammo.Type) ? "custom" : i_ammo.Type;
			if (type != "standard" && type != "fmj" && type != "hollow-point" && type != "incendiary"
				&& type != "toxic" && type != "freeze" && type != "custom")
				io_report.Add(ValidationSeverity.Error, "weapon.ammunition.type", "ammunition.type must be standard, fmj, hollow-point, incendiary, toxic, freeze, or custom.", i_source);
			ValidateBehaviorRange(i_ammo.DamageMultiplier, 0f, 10f, "ammunition-damage-multiplier", io_report, i_source);
			ValidateBehaviorRange(i_ammo.PenetrationBonus, 0, 64, "ammunition-penetration-bonus", io_report, i_source);
			ValidateBehaviorRange(i_ammo.WeakpointMultiplier, 0f, 10f, "ammunition-weakpoint-multiplier", io_report, i_source);
			ValidateBehaviorRange(i_ammo.DamageOverTime, 0f, 10000f, "ammunition-damage-over-time", io_report, i_source);
			ValidateBehaviorRange(i_ammo.EffectDurationSeconds, 0.05f, 60f, "ammunition-effect-duration", io_report, i_source);
			ValidateBehaviorRange(i_ammo.TickIntervalSeconds, 0.05f, 10f, "ammunition-tick-interval", io_report, i_source);
			ValidateBehaviorRange(i_ammo.SlowMultiplier, 0f, 1f, "ammunition-slow-multiplier", io_report, i_source);
			ValidateBehaviorRange(i_ammo.SlowDurationSeconds, 0.05f, 60f, "ammunition-slow-duration", io_report, i_source);
			if (i_ammo.DamageOverTime.GetValueOrDefault() > 0f && !i_ammo.EffectDurationSeconds.HasValue)
				io_report.Add(ValidationSeverity.Error, "weapon.ammunition.duration", "effectDurationSeconds is required when damageOverTime is used.", i_source);
			if (i_ammo.SlowMultiplier.GetValueOrDefault(1f) < 1f && !i_ammo.SlowDurationSeconds.HasValue)
				io_report.Add(ValidationSeverity.Error, "weapon.ammunition.slow-duration", "slowDurationSeconds is required when slowMultiplier is below one.", i_source);
		}

		private static void ValidateShortText(string i_text, string i_code, ValidationReport io_report, string i_source)
		{
			if (i_text != null && (string.IsNullOrWhiteSpace(i_text) || i_text.Length > 80))
				io_report.Add(ValidationSeverity.Error, "weapon.behavior." + i_code, i_code + " must contain 1 through 80 characters.", i_source);
		}

		private static void ValidateAnimations(WeaponAnimationDefinition i_animation, WeaponVisualDefinition i_visual, ContentId i_template, bool i_hasTemplate, bool i_original, ValidationReport io_report, string i_source)
		{
			if (i_animation == null) return;
			if (i_original && (i_animation.IdleState != null || i_animation.FireState != null || i_animation.ReloadState != null
				|| i_animation.FireSpeedMultiplier.HasValue || i_animation.ReloadSpeedMultiplier.HasValue))
				io_report.Add(ValidationSeverity.Error, "weapon.animations.original-state", "Fully original weapons use timed sprite clips and cannot reference inherited Animator states or speed multipliers.", i_source);
			foreach (string state in new[] { i_animation.IdleState, i_animation.FireState, i_animation.ReloadState })
				if (state != null && (string.IsNullOrWhiteSpace(state) || state.Length > 80 || state.Contains("/")))
					io_report.Add(ValidationSeverity.Error, "weapon.animations.state", "Animation state names must contain 1 to 80 characters and no path separators.", i_source);
			ValidateBehaviorRange(i_animation.FireSpeedMultiplier, 0.05f, 10f, "animation-fire-speed", io_report, i_source);
			ValidateBehaviorRange(i_animation.ReloadSpeedMultiplier, 0.05f, 10f, "animation-reload-speed", io_report, i_source);
			if (i_animation.Clips != null && i_animation.Clips.Count > 6)
				io_report.Add(ValidationSeverity.Error, "weapon.animations.clip-count", "animations.clips supports at most idle, equip, fire, alternate, charge, and reload.", i_source);
			foreach (KeyValuePair<string, WeaponSpriteAnimationClipDefinition> entry in i_animation.Clips ?? new Dictionary<string, WeaponSpriteAnimationClipDefinition>())
			{
				if (entry.Key != "idle" && entry.Key != "equip" && entry.Key != "fire" && entry.Key != "alternate" && entry.Key != "charge" && entry.Key != "reload")
					io_report.Add(ValidationSeverity.Error, "weapon.animations.clip-name", "Animation clip names must be idle, equip, fire, alternate, charge, or reload.", i_source);
				WeaponSpriteAnimationClipDefinition clip = entry.Value;
				if (clip == null || clip.Frames == null || clip.Frames.Count < 1 || clip.Frames.Count > 120)
				{
					io_report.Add(ValidationSeverity.Error, "weapon.animations.frames", "Each animation clip requires 1 through 120 frames.", i_source);
					continue;
				}
				foreach (WeaponSpriteAnimationFrameDefinition frame in clip.Frames)
				{
					if (frame == null || !IsFiniteRange(frame.DurationSeconds, 0.01f, 10f) || frame.Sprites == null || frame.Sprites.Count == 0)
					{
						io_report.Add(ValidationSeverity.Error, "weapon.animations.frame", "Animation frames require a duration from 0.01 through 10 seconds and at least one sprite.", i_source);
						continue;
					}
					foreach (KeyValuePair<string, string> sprite in frame.Sprites)
					{
						bool published = (i_original && sprite.Key != "icon" && i_visual?.Sprites != null && i_visual.Sprites.ContainsKey(sprite.Key)) || (i_hasTemplate && WeaponTemplateCatalog.IsPublished(i_template, sprite.Key)
							&& WeaponTemplateCatalog.TryGetCorePartName(i_template, sprite.Key, out string partName) && partName != null);
						if (!published) io_report.Add(ValidationSeverity.Error, "weapon.animations.slot", "Animation sprites must target a visible published prefab slot: " + sprite.Key, i_source);
						if (!ModPath.IsSafeRelativePath(sprite.Value) || !sprite.Value.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
							io_report.Add(ValidationSeverity.Error, "weapon.animations.asset-path", "Animation sprites must use safe pack-relative PNG paths.", i_source);
					}
				}
			}
		}

		private static bool IsFiniteRange(float i_value, float i_min, float i_max)
		{
			return !float.IsNaN(i_value) && !float.IsInfinity(i_value) && i_value >= i_min && i_value <= i_max;
		}

		private static void ValidateMelee(WeaponMeleeDefinition i_melee, ValidationReport io_report, string i_source)
		{
			if (i_melee == null) return;
			string input = string.IsNullOrWhiteSpace(i_melee.Input) ? "primary" : i_melee.Input;
			if (input != "primary" && input != "alternate")
				io_report.Add(ValidationSeverity.Error, "weapon.melee.input", "melee.input must be primary or alternate.", i_source);
			ValidateBehaviorRange(i_melee.Range, 0.1f, 10f, "melee-range", io_report, i_source);
			ValidateBehaviorRange(i_melee.Radius, 0.05f, 5f, "melee-radius", io_report, i_source);
			ValidateBehaviorRange(i_melee.DamageMultiplier, 0.01f, 10f, "melee-damage-multiplier", io_report, i_source);
			ValidateBehaviorRange(i_melee.KnockbackX, 0f, 1000f, "melee-knockback-x", io_report, i_source);
			ValidateBehaviorRange(i_melee.KnockbackY, 0f, 1000f, "melee-knockback-y", io_report, i_source);
			ValidateBehaviorRange(i_melee.MaxTargets, 1, 32, "melee-max-targets", io_report, i_source);
		}

		private static void ValidateProjectile(WeaponProjectileDefinition i_projectile, ValidationReport io_report, string i_source)
		{
			if (i_projectile == null) return;
			string mode = string.IsNullOrWhiteSpace(i_projectile.Mode) ? "hitscan" : i_projectile.Mode;
			if (mode != "hitscan" && mode != "physical")
				io_report.Add(ValidationSeverity.Error, "weapon.projectile.mode", "projectile.mode must be hitscan or physical.", i_source);
			if (mode == "physical" && string.IsNullOrWhiteSpace(i_projectile.Sprite))
				io_report.Add(ValidationSeverity.Error, "weapon.projectile.sprite", "A physical projectile requires a PNG sprite.", i_source);
			ValidateBehaviorRange(i_projectile.PixelsPerUnit, 1f, 1024f, "projectile-pixels-per-unit", io_report, i_source);
			ValidateBehaviorRange(i_projectile.Speed, 0.1f, 250f, "projectile-speed", io_report, i_source);
			ValidateBehaviorRange(i_projectile.Gravity, -100f, 100f, "projectile-gravity", io_report, i_source);
			ValidateBehaviorRange(i_projectile.LifetimeSeconds, 0.05f, 30f, "projectile-lifetime", io_report, i_source);
			ValidateBehaviorRange(i_projectile.ImpactLifetimeSeconds, 0.02f, 10f, "impact-lifetime", io_report, i_source);
			ValidateBehaviorRange(i_projectile.ExplosionRadius, 0f, 25f, "explosion-radius", io_report, i_source);
			ValidateBehaviorRange(i_projectile.ExplosionDamageMultiplier, 0f, 10f, "explosion-damage-multiplier", io_report, i_source);
			if (!string.IsNullOrWhiteSpace(i_projectile.Sprite))
				ValidateAssets(new[] { i_projectile.Sprite }, 1, new[] { ".png" }, "projectile-sprite", io_report, i_source);
			ValidateAssets(i_projectile.ImpactSprites, 16, new[] { ".png" }, "impact-sprites", io_report, i_source);
		}

		private static void ValidateAssets(IEnumerable<string> i_paths, int i_max, IEnumerable<string> i_extensions,
			string i_name, ValidationReport io_report, string i_source)
		{
			if (i_paths == null) return;
			List<string> paths = new List<string>(i_paths);
			if (paths.Count == 0 || paths.Count > i_max)
				io_report.Add(ValidationSeverity.Error, "weapon.behavior." + i_name, i_name + " must contain 1 through " + i_max + " paths when supplied.", i_source);
			foreach (string path in paths)
			{
				string extension = System.IO.Path.GetExtension(path ?? string.Empty).ToLowerInvariant();
				bool extensionValid = false;
				foreach (string allowed in i_extensions) if (extension == allowed) extensionValid = true;
				if (!ModPath.IsSafeRelativePath(path) || !extensionValid)
					io_report.Add(ValidationSeverity.Error, "weapon.behavior." + i_name, i_name + " contains an unsafe or unsupported asset path: " + path, i_source);
			}
		}

		private static bool IsColor(string i_value)
		{
			if ((i_value.Length != 7 && i_value.Length != 9) || i_value[0] != '#') return false;
			for (int index = 1; index < i_value.Length; index++) if (!Uri.IsHexDigit(i_value[index])) return false;
			return true;
		}

		private static void ValidateBehaviorRange(int? i_value, int i_min, int i_max, string i_name,
			ValidationReport io_report, string i_source)
		{
			if (i_value.HasValue && (i_value.Value < i_min || i_value.Value > i_max))
				io_report.Add(ValidationSeverity.Error, "weapon.behavior." + i_name, i_name + " must be between " + i_min + " and " + i_max + ".", i_source);
		}

		private static void ValidateBehaviorRange(float? i_value, float i_min, float i_max, string i_name,
			ValidationReport io_report, string i_source)
		{
			if (!i_value.HasValue) return;
			float value = i_value.Value;
			if (float.IsNaN(value) || float.IsInfinity(value) || value < i_min || value > i_max)
				io_report.Add(ValidationSeverity.Error, "weapon.behavior." + i_name, i_name + " must be between " + i_min + " and " + i_max + ".", i_source);
		}

		internal static void ValidateStats(WeaponStatsDefinition i_stats, ValidationReport io_report, string i_source)
		{
			if (i_stats == null) return;
			ValidateRange(i_stats.Damage, 0, 10000, "damage", io_report, i_source);
			ValidateRange(i_stats.AmmoMax, 1, 100000, "ammo-max", io_report, i_source);
			ValidateRange(i_stats.MagazineSize, 1, 100000, "magazine-size", io_report, i_source);
			ValidateRange(i_stats.BulletsPerShot, 1, 64, "bullets-per-shot", io_report, i_source);
			ValidateRange(i_stats.Penetration, 0, 64, "penetration", io_report, i_source);
			ValidateRange(i_stats.RangeMultiplier, 0.05f, 1f, "range-multiplier", io_report, i_source);
			ValidateRange(i_stats.FireIntervalSeconds, 0.02f, 10f, "fire-interval-seconds", io_report, i_source);
			ValidateRange(i_stats.Recoil, 0f, 1f, "recoil", io_report, i_source);
			ValidateRange(i_stats.MovementRecoil, 0f, 1f, "movement-recoil", io_report, i_source);
			ValidateRange(i_stats.KnockbackX, 0f, 1000f, "knockback-x", io_report, i_source);
			ValidateRange(i_stats.KnockbackY, 0f, 1000f, "knockback-y", io_report, i_source);
			ValidateRange(i_stats.ReloadSeconds, 0.01f, 60f, "reload-seconds", io_report, i_source);
			ValidateRange(i_stats.EquipSeconds, 0f, 60f, "equip-seconds", io_report, i_source);
			ValidateRange(i_stats.Weight, 0, 100000, "weight", io_report, i_source);
			ValidateRange(i_stats.Value, 0, 100000000, "value", io_report, i_source);
			ValidateEnum(i_stats.HoldType, new[] { "OneHanded", "TwoHanded1", "TwoHanded2", "TwoHanded3" }, "hold-type", io_report, i_source);
			ValidateEnum(i_stats.ReloadType, new[] { "Magazine", "PumpAction", "SingleBarrel" }, "reload-type", io_report, i_source);
			ValidateEnum(i_stats.WeaponType, new[] { "Pistol", "Smg", "Shotgun", "Rifle", "Special" }, "weapon-type", io_report, i_source);

			if (i_stats.AmmoMax.HasValue != i_stats.MagazineSize.HasValue)
				io_report.Add(ValidationSeverity.Error, "weapon.stats.ammo-pair", "ammoMax and magazineSize must be overridden together.", i_source);
			else if (i_stats.AmmoMax.HasValue && i_stats.MagazineSize.Value > i_stats.AmmoMax.Value)
				io_report.Add(ValidationSeverity.Error, "weapon.stats.magazine-size", "magazineSize cannot exceed ammoMax.", i_source);
		}

		private static void ValidateOriginalStats(WeaponStatsDefinition i_stats, ValidationReport io_report, string i_source)
		{
			if (i_stats == null)
			{
				io_report.Add(ValidationSeverity.Error, "weapon.stats.original-required", "A fully original weapon requires a complete stats object.", i_source);
				return;
			}
			List<string> missing = new List<string>();
			if (!i_stats.Damage.HasValue) missing.Add("damage");
			if (!i_stats.AmmoMax.HasValue) missing.Add("ammoMax");
			if (!i_stats.MagazineSize.HasValue) missing.Add("magazineSize");
			if (!i_stats.RangeMultiplier.HasValue) missing.Add("rangeMultiplier");
			if (!i_stats.FireIntervalSeconds.HasValue) missing.Add("fireIntervalSeconds");
			if (!i_stats.ReloadSeconds.HasValue) missing.Add("reloadSeconds");
			if (!i_stats.EquipSeconds.HasValue) missing.Add("equipSeconds");
			if (!i_stats.Weight.HasValue) missing.Add("weight");
			if (!i_stats.Value.HasValue) missing.Add("value");
			if (!i_stats.SemiAutomatic.HasValue) missing.Add("semiAutomatic");
			if (!i_stats.Marketable.HasValue) missing.Add("marketable");
			if (string.IsNullOrWhiteSpace(i_stats.HoldType)) missing.Add("holdType");
			if (string.IsNullOrWhiteSpace(i_stats.ReloadType)) missing.Add("reloadType");
			if (string.IsNullOrWhiteSpace(i_stats.WeaponType)) missing.Add("weaponType");
			if (missing.Count > 0)
				io_report.Add(ValidationSeverity.Error, "weapon.stats.original-required", "Fully original weapons require: " + string.Join(", ", missing.ToArray()) + ".", i_source);
			if (!string.IsNullOrWhiteSpace(i_stats.ReloadType) && !string.Equals(i_stats.ReloadType, "Magazine", StringComparison.OrdinalIgnoreCase))
				io_report.Add(ValidationSeverity.Error, "weapon.stats.original-reload-type", "Fully original weapons currently support the event-free Magazine reload type.", i_source);
		}

		private static void ValidateEnum(string i_value, IEnumerable<string> i_valid, string i_name, ValidationReport io_report, string i_source)
		{
			if (string.IsNullOrWhiteSpace(i_value)) return;
			foreach (string value in i_valid) if (string.Equals(value, i_value, StringComparison.OrdinalIgnoreCase)) return;
			io_report.Add(ValidationSeverity.Error, "weapon.stats." + i_name, i_name + " has an unsupported value.", i_source);
		}

		private static void ValidateRange(int? i_value, int i_min, int i_max, string i_name, ValidationReport io_report, string i_source)
		{
			if (i_value.HasValue && (i_value.Value < i_min || i_value.Value > i_max))
				io_report.Add(ValidationSeverity.Error, "weapon.stats." + i_name, i_name + " must be between " + i_min + " and " + i_max + ".", i_source);
		}

		private static void ValidateRange(float? i_value, float i_min, float i_max, string i_name, ValidationReport io_report, string i_source)
		{
			if (!i_value.HasValue) return;
			float value = i_value.Value;
			if (float.IsNaN(value) || float.IsInfinity(value) || value < i_min || value > i_max)
				io_report.Add(ValidationSeverity.Error, "weapon.stats." + i_name, i_name + " must be between " + i_min + " and " + i_max + ".", i_source);
		}

		private static void ValidateVisual(WeaponVisualDefinition i_visual, string i_packId, ContentId i_template, bool i_hasTemplate,
			ValidationReport io_report, string i_source)
		{
			if (i_visual == null)
			{
				io_report.Add(ValidationSeverity.Error, "weapon.visual", "visual is required.", i_source);
				return;
			}
			bool original = string.Equals(i_visual.Type, "originalWeaponSprites", StringComparison.Ordinal);
			bool inherited = string.Equals(i_visual.Type, "coreWeaponSprites", StringComparison.Ordinal);
			if (!original && !inherited)
				io_report.Add(ValidationSeverity.Error, "weapon.visual.type", "visual.type must be coreWeaponSprites or originalWeaponSprites.", i_source);
			if (float.IsNaN(i_visual.PixelsPerUnit) || float.IsInfinity(i_visual.PixelsPerUnit) || i_visual.PixelsPerUnit <= 0f || i_visual.PixelsPerUnit > 1024f)
				io_report.Add(ValidationSeverity.Error, "weapon.visual.pixels-per-unit", "pixelsPerUnit must be greater than 0 and at most 1024.", i_source);
			if (i_visual.Sprites == null || i_visual.Sprites.Count == 0)
			{
				io_report.Add(ValidationSeverity.Error, "weapon.visual.sprites", "At least one weapon sprite is required.", i_source);
				return;
			}
			if (!string.IsNullOrEmpty(i_visual.BundlePrefab)
				&& (!ContentId.TryParse(i_visual.BundlePrefab, out ContentId bundleId)
					|| bundleId.Namespace != i_packId
					|| !bundleId.Path.StartsWith("prefab/", StringComparison.Ordinal)))
				io_report.Add(ValidationSeverity.Error, "weapon.visual.bundle-prefab",
					"bundlePrefab must be a prefab/... ID owned by the defining pack.", i_source);
			foreach (KeyValuePair<string, string> sprite in i_visual.Sprites)
			{
				if (inherited && (!i_hasTemplate || !WeaponTemplateCatalog.IsPublished(i_template, sprite.Key)))
					io_report.Add(ValidationSeverity.Error, "weapon.visual.slot", "Sprite slot is not published by the selected Core weapon template: " + sprite.Key, i_source);
				if (original && !IsPartName(sprite.Key))
					io_report.Add(ValidationSeverity.Error, "weapon.visual.slot", "Original weapon sprite names must be lowercase identifiers: " + sprite.Key, i_source);
				if (!ModPath.IsSafeRelativePath(sprite.Value) || !sprite.Value.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
					io_report.Add(ValidationSeverity.Error, "weapon.visual.asset-path", "Weapon sprites must use safe pack-relative PNG paths: " + sprite.Value, i_source);
			}
			if (original && !i_visual.Sprites.ContainsKey("body"))
				io_report.Add(ValidationSeverity.Error, "weapon.visual.body", "A fully original weapon requires the body sprite.", i_source);
			if (i_visual.Parts != null)
			{
				if (!original) io_report.Add(ValidationSeverity.Error, "weapon.visual.parts", "parts is only valid for originalWeaponSprites.", i_source);
				if (i_visual.Parts.Count > 16) io_report.Add(ValidationSeverity.Error, "weapon.visual.parts", "parts supports at most 16 entries.", i_source);
				foreach (KeyValuePair<string, WeaponVisualPartDefinition> entry in i_visual.Parts)
				{
					if (!IsPartName(entry.Key) || entry.Key == "icon" || !i_visual.Sprites.ContainsKey(entry.Key))
						io_report.Add(ValidationSeverity.Error, "weapon.visual.part", "Each parts key must name a visible sprite from visual.sprites.", i_source);
					WeaponVisualPartDefinition part = entry.Value;
					if (part == null) { io_report.Add(ValidationSeverity.Error, "weapon.visual.part", "parts entries cannot be null.", i_source); continue; }
					ValidateVisualRange(part.PivotX, 0f, 1f, "part-pivot-x", io_report, i_source);
					ValidateVisualRange(part.PivotY, 0f, 1f, "part-pivot-y", io_report, i_source);
					ValidateVisualRange(part.OffsetX, -10f, 10f, "part-offset-x", io_report, i_source);
					ValidateVisualRange(part.OffsetY, -10f, 10f, "part-offset-y", io_report, i_source);
					ValidateVisualRange(part.RotationDegrees, -360f, 360f, "part-rotation", io_report, i_source);
					ValidateVisualRange(part.ScaleX, -10f, 10f, "part-scale-x", io_report, i_source);
					ValidateVisualRange(part.ScaleY, -10f, 10f, "part-scale-y", io_report, i_source);
					if (part.SortingOrder.HasValue && (part.SortingOrder < -100 || part.SortingOrder > 100))
						io_report.Add(ValidationSeverity.Error, "weapon.visual.part-sorting-order", "part sortingOrder must be between -100 and 100.", i_source);
				}
			}
			foreach (string slot in i_visual.HiddenSlots ?? new List<string>())
			{
				if (original || !i_hasTemplate || !WeaponTemplateCatalog.IsPublished(i_template, slot)
					|| !WeaponTemplateCatalog.TryGetCorePartName(i_template, slot, out string partName) || partName == null)
					io_report.Add(ValidationSeverity.Error, "weapon.visual.hidden-slot", "hiddenSlots must name visible published prefab slots: " + slot, i_source);
			}
			ValidateVisualRange(i_visual.PivotX, 0f, 1f, "pivot-x", io_report, i_source);
			ValidateVisualRange(i_visual.PivotY, 0f, 1f, "pivot-y", io_report, i_source);
			ValidateVisualRange(i_visual.MuzzleOffsetX, -10f, 10f, "muzzle-offset-x", io_report, i_source);
			ValidateVisualRange(i_visual.MuzzleOffsetY, -10f, 10f, "muzzle-offset-y", io_report, i_source);
			ValidateVisualRange(i_visual.ColliderWidth, 0.01f, 10f, "collider-width", io_report, i_source);
			ValidateVisualRange(i_visual.ColliderHeight, 0.01f, 10f, "collider-height", io_report, i_source);
			ValidateVisualRange(i_visual.ColliderOffsetX, -10f, 10f, "collider-offset-x", io_report, i_source);
			ValidateVisualRange(i_visual.ColliderOffsetY, -10f, 10f, "collider-offset-y", io_report, i_source);
			if (i_visual.SortingOrder.HasValue && (i_visual.SortingOrder.Value < -100 || i_visual.SortingOrder.Value > 100))
				io_report.Add(ValidationSeverity.Error, "weapon.visual.sorting-order", "sortingOrder must be between -100 and 100.", i_source);
			if (inherited && (i_visual.PivotX.HasValue || i_visual.PivotY.HasValue || i_visual.MuzzleOffsetX.HasValue || i_visual.MuzzleOffsetY.HasValue
				|| i_visual.ColliderWidth.HasValue || i_visual.ColliderHeight.HasValue || i_visual.ColliderOffsetX.HasValue || i_visual.ColliderOffsetY.HasValue || i_visual.SortingOrder.HasValue))
				io_report.Add(ValidationSeverity.Error, "weapon.visual.original-fields", "Rig, collider, pivot, and muzzle fields are only valid for originalWeaponSprites.", i_source);
		}

		private static bool IsPartName(string i_name)
		{
			if (string.IsNullOrWhiteSpace(i_name) || i_name.Length > 48) return false;
			for (int index = 0; index < i_name.Length; index++)
			{
				char value = i_name[index];
				if ((value < 'a' || value > 'z') && (value < '0' || value > '9') && value != '-') return false;
			}
			return i_name[0] >= 'a' && i_name[0] <= 'z';
		}

		private static void ValidateVisualRange(float? i_value, float i_min, float i_max, string i_name, ValidationReport io_report, string i_source)
		{
			if (!i_value.HasValue) return;
			float value = i_value.Value;
			if (float.IsNaN(value) || float.IsInfinity(value) || value < i_min || value > i_max)
				io_report.Add(ValidationSeverity.Error, "weapon.visual." + i_name, i_name + " must be between " + i_min + " and " + i_max + ".", i_source);
		}

		private static void Error(WeaponDefinitionLoadResult io_result, string i_code, string i_message, string i_source)
		{
			io_result.Report.Add(ValidationSeverity.Error, "weapon." + i_code, i_message, i_source);
		}
	}
}
