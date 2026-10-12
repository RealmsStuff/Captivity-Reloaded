using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace CaptivityReloaded.Modding
{
	[JsonObject(MemberSerialization.OptIn)]
	public sealed class RuleModuleDocument
	{
		[JsonProperty("type", Required = Required.Always)] public string Type { get; set; }
		[JsonProperty("trigger")] public string Trigger { get; set; }
		[JsonProperty("selection")] public string Selection { get; set; }
		[JsonProperty("starterWeapon")] public string StarterWeapon { get; set; }
		[JsonProperty("weaponPool")] public List<string> WeaponPool { get; set; }
		[JsonProperty("replaceExistingWeapons")] public bool? ReplaceExistingWeapons { get; set; }
		[JsonProperty("waveSpawnMultiplier")] public float? WaveSpawnMultiplier { get; set; }
		[JsonProperty("waveSpawnAdd")] public int? WaveSpawnAdd { get; set; }
		[JsonProperty("maxStageEnemiesMultiplier")] public float? MaxStageEnemiesMultiplier { get; set; }
		[JsonProperty("maxStageEnemiesAdd")] public int? MaxStageEnemiesAdd { get; set; }
		[JsonProperty("maxRoomEnemiesMultiplier")] public float? MaxRoomEnemiesMultiplier { get; set; }
		[JsonProperty("maxRoomEnemiesAdd")] public int? MaxRoomEnemiesAdd { get; set; }
		[JsonProperty("spawnerChanceMultiplier")] public float? SpawnerChanceMultiplier { get; set; }
		[JsonProperty("spawnDelayMultiplier")] public float? SpawnDelayMultiplier { get; set; }
		[JsonProperty("initialSpawnDelayMultiplier")] public float? InitialSpawnDelayMultiplier { get; set; }
		[JsonProperty("enemySpeedVarianceMin")] public float? EnemySpeedVarianceMin { get; set; }
		[JsonProperty("enemySpeedVarianceMax")] public float? EnemySpeedVarianceMax { get; set; }
		[JsonProperty("enemyHealthVarianceMin")] public float? EnemyHealthVarianceMin { get; set; }
		[JsonProperty("enemyHealthVarianceMax")] public float? EnemyHealthVarianceMax { get; set; }
		[JsonProperty("enemyWaveHealthGrowthRandomBase")] public float? EnemyWaveHealthGrowthRandomBase { get; set; }
		[JsonProperty("enemySpeedGrowthAfterWave")] public int? EnemySpeedGrowthAfterWave { get; set; }
		[JsonProperty("enemySpeedGrowthPerWave")] public float? EnemySpeedGrowthPerWave { get; set; }
		[JsonProperty("zombieChaseChancePercent")] public int? ZombieChaseChancePercent { get; set; }
		[JsonProperty("zombieFallChancePercent")] public int? ZombieFallChancePercent { get; set; }
		[JsonProperty("zombieChaseCheckSeconds")] public float? ZombieChaseCheckSeconds { get; set; }
		[JsonProperty("zombieChaseDurationMin")] public float? ZombieChaseDurationMin { get; set; }
		[JsonProperty("zombieChaseDurationMax")] public float? ZombieChaseDurationMax { get; set; }
		[JsonProperty("zombieChaseSpeedMaxBonus")] public float? ZombieChaseSpeedMaxBonus { get; set; }
		[JsonProperty("deathHoundRetrySeconds")] public float? DeathHoundRetrySeconds { get; set; }
		[JsonProperty("deathHoundPrepareSeconds")] public float? DeathHoundPrepareSeconds { get; set; }
		[JsonProperty("deathHoundRoamSpeedBonus")] public float? DeathHoundRoamSpeedBonus { get; set; }
		[JsonProperty("deathHoundChargeSpeedBonus")] public float? DeathHoundChargeSpeedBonus { get; set; }
		[JsonProperty("deathHoundChargeSeconds")] public float? DeathHoundChargeSeconds { get; set; }
		[JsonProperty("deathHoundLeapXMultiplier")] public float? DeathHoundLeapXMultiplier { get; set; }
		[JsonProperty("deathHoundLandCheckDelay")] public float? DeathHoundLandCheckDelay { get; set; }
		[JsonProperty("deathHoundPrepareInnerDivisor")] public float? DeathHoundPrepareInnerDivisor { get; set; }
		[JsonProperty("deathHoundLeapDistanceDivisor")] public float? DeathHoundLeapDistanceDivisor { get; set; }
		[JsonProperty("deathHoundHitDistance")] public float? DeathHoundHitDistance { get; set; }
		[JsonProperty("flyChargeAccelBonus")] public float? FlyChargeAccelBonus { get; set; }
		[JsonProperty("flyChargeSpeedBonus")] public float? FlyChargeSpeedBonus { get; set; }
		[JsonProperty("flyRechargeDelayMultiplier")] public float? FlyRechargeDelayMultiplier { get; set; }
		[JsonProperty("mindBreakTargetMultiplier")] public float? MindBreakTargetMultiplier { get; set; }
		[JsonProperty("hypnosisProgressMultiplier")] public int? HypnosisProgressMultiplier { get; set; }
		[JsonProperty("legacyShackAberrants")] public bool? LegacyShackAberrants { get; set; }
		[JsonProperty("legacyShackTankLimit")] public int? LegacyShackTankLimit { get; set; }
		[JsonProperty("legacyShackTankDelaySeconds")] public float? LegacyShackTankDelaySeconds { get; set; }
		[JsonProperty("legacyShackBoomerLimit")] public int? LegacyShackBoomerLimit { get; set; }
		[JsonProperty("legacyShackBoomerDelaySeconds")] public float? LegacyShackBoomerDelaySeconds { get; set; }
		[JsonProperty("legacyShackPinkHoundLimit")] public int? LegacyShackPinkHoundLimit { get; set; }
		[JsonProperty("legacyShackPinkHoundDelaySeconds")] public float? LegacyShackPinkHoundDelaySeconds { get; set; }
		[JsonProperty("legacyShackBoomerExplodes")] public bool? LegacyShackBoomerExplodes { get; set; }
		[JsonProperty("spawnGrowthPerWave")] public int? SpawnGrowthPerWave { get; set; }
		[JsonProperty("intermissionSeconds")] public int? IntermissionSeconds { get; set; }
		[JsonProperty("maximumWaves")] public int? MaximumWaves { get; set; }
		[JsonProperty("startingMoney")] public int? StartingMoney { get; set; }
		[JsonProperty("infiniteMoney")] public bool? InfiniteMoney { get; set; }
		[JsonProperty("bountyMultiplier")] public float? BountyMultiplier { get; set; }
		[JsonProperty("ammoDropChance")] public float? AmmoDropChance { get; set; }
		[JsonProperty("ammoDropChancePerWave")] public float? AmmoDropChancePerWave { get; set; }
		[JsonProperty("ammoDropChanceMinimum")] public float? AmmoDropChanceMinimum { get; set; }
		[JsonProperty("repeatConsumablePurchases")] public bool? RepeatConsumablePurchases { get; set; }
		[JsonProperty("clothingRepairEnabled")] public bool? ClothingRepairEnabled { get; set; }
		[JsonProperty("clothingRepairCostMultiplier")] public float? ClothingRepairCostMultiplier { get; set; }
		[JsonProperty("ammoDropLifetimeSeconds")] public float? AmmoDropLifetimeSeconds { get; set; }
		[JsonProperty("vendorsAlwaysAvailable")] public bool? VendorsAlwaysAvailable { get; set; }
		[JsonProperty("itemValueOffsets")] public Dictionary<string, int> ItemValueOffsets { get; set; }
		[JsonProperty("weaponAmmoCapacityOffsets")] public Dictionary<string, int> WeaponAmmoCapacityOffsets { get; set; }
		[JsonProperty("pistolEquipSeconds")] public float? PistolEquipSeconds { get; set; }
		[JsonProperty("pistolReloadSeconds")] public float? PistolReloadSeconds { get; set; }
		[JsonProperty("smgOneHandedEquipSeconds")] public float? SmgOneHandedEquipSeconds { get; set; }
		[JsonProperty("smgOneHandedReloadSeconds")] public float? SmgOneHandedReloadSeconds { get; set; }
		[JsonProperty("smgTwoHandedEquipSeconds")] public float? SmgTwoHandedEquipSeconds { get; set; }
		[JsonProperty("smgTwoHandedReloadSeconds")] public float? SmgTwoHandedReloadSeconds { get; set; }
		[JsonProperty("shotgunOneHandedEquipSeconds")] public float? ShotgunOneHandedEquipSeconds { get; set; }
		[JsonProperty("shotgunOneHandedReloadSeconds")] public float? ShotgunOneHandedReloadSeconds { get; set; }
		[JsonProperty("shotgunTwoHandedEquipSeconds")] public float? ShotgunTwoHandedEquipSeconds { get; set; }
		[JsonProperty("shotgunTwoHandedReloadSeconds")] public float? ShotgunTwoHandedReloadSeconds { get; set; }
		[JsonProperty("rifleEquipSeconds")] public float? RifleEquipSeconds { get; set; }
		[JsonProperty("rifleReloadSeconds")] public float? RifleReloadSeconds { get; set; }
		[JsonProperty("singleBarrelReloadSeconds")] public float? SingleBarrelReloadSeconds { get; set; }
		[JsonProperty("minimumShotDelaySeconds")] public float? MinimumShotDelaySeconds { get; set; }
		[JsonProperty("playerDamageTakenMultiplier")] public float? PlayerDamageTakenMultiplier { get; set; }
		[JsonProperty("grantAllWeapons")] public bool? GrantAllWeapons { get; set; }
		[JsonProperty("ignoreWeightLimit")] public bool? IgnoreWeightLimit { get; set; }
		[JsonProperty("debugHotkeys")] public bool? DebugHotkeys { get; set; }
		[JsonProperty("startWithoutWeapon")] public bool? StartWithoutWeapon { get; set; }
		[JsonProperty("gunDamageMultiplier")] public float? GunDamageMultiplier { get; set; }
		[JsonProperty("selfPleasureEnabled")] public bool? SelfPleasureEnabled { get; set; }
		[JsonProperty("selfPleasurePerSecond")] public float? SelfPleasurePerSecond { get; set; }
		[JsonProperty("selfPleasureHeartCost")] public int? SelfPleasureHeartCost { get; set; }
		[JsonProperty("enemyFinishersEnabled")] public bool? EnemyFinishersEnabled { get; set; }
		[JsonProperty("clothingDamageEnabled")] public bool? ClothingDamageEnabled { get; set; }
		[JsonProperty("safeKnockouts")] public bool? SafeKnockouts { get; set; }
		[JsonProperty("playerHealthMultiplier")] public float? PlayerHealthMultiplier { get; set; }
		[JsonProperty("maximumHearts")] public int? MaximumHearts { get; set; }
		[JsonProperty("pleasureMaximumMultiplier")] public float? PleasureMaximumMultiplier { get; set; }
		[JsonProperty("libidoMaximumMultiplier")] public float? LibidoMaximumMultiplier { get; set; }
		[JsonProperty("pleasureGainMultiplier")] public float? PleasureGainMultiplier { get; set; }
		[JsonProperty("rapeLibidoGainMultiplier")] public float? RapeLibidoGainMultiplier { get; set; }
		[JsonProperty("voluntaryExposeSkipsEscape")] public bool? VoluntaryExposeSkipsEscape { get; set; }
		[JsonProperty("retainBuffsOnClimax")] public bool? RetainBuffsOnClimax { get; set; }
		[JsonProperty("strengthDamageWhileRestrainedMultiplier")] public float? StrengthDamageWhileRestrainedMultiplier { get; set; }
		[JsonProperty("strengthDamageFreeMultiplier")] public float? StrengthDamageFreeMultiplier { get; set; }
		[JsonProperty("clothingDamageChance")] public float? ClothingDamageChance { get; set; }
		[JsonProperty("forceAutomaticWeapons")] public bool? ForceAutomaticWeapons { get; set; }
		[JsonProperty("autoReloadOnEmpty")] public bool? AutoReloadOnEmpty { get; set; }
		[JsonProperty("weaponRangeMultiplier")] public float? WeaponRangeMultiplier { get; set; }
		[JsonProperty("cameraZoomMultiplier")] public float? CameraZoomMultiplier { get; set; }
		[JsonProperty("item")] public string Item { get; set; }
		[JsonProperty("healthRestore")] public float? HealthRestore { get; set; }
		[JsonProperty("strengthRestoreMultiplier")] public float? StrengthRestoreMultiplier { get; set; }
		[JsonProperty("libidoReduction")] public float? LibidoReduction { get; set; }
		[JsonProperty("pleasureReduction")] public float? PleasureReduction { get; set; }
		[JsonProperty("effectDurationSeconds")] public float? EffectDurationSeconds { get; set; }
		[JsonProperty("restoreHeart")] public bool? RestoreHeart { get; set; }
		[JsonProperty("invulnerabilitySeconds")] public float? InvulnerabilitySeconds { get; set; }
		[JsonProperty("gunDamageBonus")] public float? GunDamageBonus { get; set; }
		[JsonProperty("accelerationBonus")] public float? AccelerationBonus { get; set; }
		[JsonProperty("sprintBonus")] public float? SprintBonus { get; set; }
		[JsonProperty("dashBonus")] public float? DashBonus { get; set; }
		[JsonProperty("healthReward")] public float? HealthReward { get; set; }
		[JsonProperty("strengthReward")] public float? StrengthReward { get; set; }
		[JsonProperty("pleasureReductionReward")] public float? PleasureReductionReward { get; set; }
		[JsonProperty("libidoReductionReward")] public float? LibidoReductionReward { get; set; }
		[JsonProperty("moneyReward")] public int? MoneyReward { get; set; }
		[JsonProperty("perCompletedExperimentMultiplier")] public float? PerCompletedExperimentMultiplier { get; set; }
		[JsonProperty("speedPerExperimentPercent")] public float? SpeedPerExperimentPercent { get; set; }
		[JsonProperty("dashPerExperimentPercent")] public float? DashPerExperimentPercent { get; set; }
		[JsonProperty("gunDamagePerExperimentPercent")] public float? GunDamagePerExperimentPercent { get; set; }
		[JsonProperty("healthPerExperimentPercent")] public float? HealthPerExperimentPercent { get; set; }
		[JsonProperty("knockbackPerExperimentPercent")] public float? KnockbackPerExperimentPercent { get; set; }
		[JsonProperty("killScore")] public int? KillScore { get; set; }
		[JsonProperty("waveScore")] public int? WaveScore { get; set; }
		[JsonProperty("damageTakenPenalty")] public int? DamageTakenPenalty { get; set; }
		[JsonProperty("targetScore")] public int? TargetScore { get; set; }
		[JsonProperty("targetKills")] public int? TargetKills { get; set; }
		[JsonProperty("targetWave")] public int? TargetWave { get; set; }
		[JsonProperty("goalMatch")] public string GoalMatch { get; set; }
		[JsonProperty("timeLimitSeconds")] public float? TimeLimitSeconds { get; set; }
		[JsonProperty("maxDamageEvents")] public int? MaxDamageEvents { get; set; }
	}

	public sealed class WaveRule
	{
		public int SpawnGrowthPerWave { get; }
		public int IntermissionSeconds { get; }
		public int MaximumWaves { get; }
		internal WaveRule(RuleModuleDocument i_document)
		{
			SpawnGrowthPerWave = i_document.SpawnGrowthPerWave ?? 2;
			IntermissionSeconds = i_document.IntermissionSeconds ?? 30;
			MaximumWaves = i_document.MaximumWaves ?? 0;
		}
	}

	public sealed class EconomyRule
	{
		public int StartingMoney { get; }
		public bool? InfiniteMoney { get; }
		public float BountyMultiplier { get; }
		public float AmmoDropChance { get; }
		public float AmmoDropChancePerWave { get; }
		public float AmmoDropChanceMinimum { get; }
		public bool? RepeatConsumablePurchases { get; }
		public bool? ClothingRepairEnabled { get; }
		public float ClothingRepairCostMultiplier { get; }
		public float AmmoDropLifetimeSeconds { get; }
		public bool? VendorsAlwaysAvailable { get; }
		public IReadOnlyDictionary<ContentId, int> ItemValueOffsets { get; }
		public IReadOnlyDictionary<ContentId, int> WeaponAmmoCapacityOffsets { get; }
		internal EconomyRule(RuleModuleDocument i_document, IReadOnlyDictionary<ContentId, int> i_values, IReadOnlyDictionary<ContentId, int> i_ammo)
		{
			StartingMoney = i_document.StartingMoney ?? 0;
			InfiniteMoney = i_document.InfiniteMoney;
			BountyMultiplier = i_document.BountyMultiplier ?? 1f;
			AmmoDropChance = i_document.AmmoDropChance ?? 0.07f;
			AmmoDropChancePerWave = i_document.AmmoDropChancePerWave ?? 0f;
			AmmoDropChanceMinimum = i_document.AmmoDropChanceMinimum ?? 0f;
			RepeatConsumablePurchases = i_document.RepeatConsumablePurchases;
			ClothingRepairEnabled = i_document.ClothingRepairEnabled;
			ClothingRepairCostMultiplier = i_document.ClothingRepairCostMultiplier ?? 1f;
			AmmoDropLifetimeSeconds = i_document.AmmoDropLifetimeSeconds ?? 15f;
			VendorsAlwaysAvailable = i_document.VendorsAlwaysAvailable;
			ItemValueOffsets = i_values; WeaponAmmoCapacityOffsets = i_ammo;
		}
	}

	public sealed class WeaponTuningRule
	{
		internal readonly RuleModuleDocument Document;
		internal WeaponTuningRule(RuleModuleDocument i_document) { Document = i_document; }
		public float EquipSeconds(string i_type, bool i_oneHanded, float i_fallback)
		{
			if (i_type == "Pistol") return Document.PistolEquipSeconds ?? i_fallback;
			if (i_type == "Smg") return (i_oneHanded ? Document.SmgOneHandedEquipSeconds : Document.SmgTwoHandedEquipSeconds) ?? i_fallback;
			if (i_type == "Shotgun") return (i_oneHanded ? Document.ShotgunOneHandedEquipSeconds : Document.ShotgunTwoHandedEquipSeconds) ?? i_fallback;
			if (i_type == "Rifle") return Document.RifleEquipSeconds ?? i_fallback;
			return i_fallback;
		}
		public float ReloadSeconds(string i_type, bool i_oneHanded, bool i_singleBarrel, float i_fallback)
		{
			if (i_singleBarrel && Document.SingleBarrelReloadSeconds.HasValue) return Document.SingleBarrelReloadSeconds.Value;
			if (i_type == "Pistol") return Document.PistolReloadSeconds ?? i_fallback;
			if (i_type == "Smg") return (i_oneHanded ? Document.SmgOneHandedReloadSeconds : Document.SmgTwoHandedReloadSeconds) ?? i_fallback;
			if (i_type == "Shotgun") return (i_oneHanded ? Document.ShotgunOneHandedReloadSeconds : Document.ShotgunTwoHandedReloadSeconds) ?? i_fallback;
			if (i_type == "Rifle") return Document.RifleReloadSeconds ?? i_fallback;
			return i_fallback;
		}
		public float MinimumShotDelaySeconds => Document.MinimumShotDelaySeconds ?? 0f;
	}

	public sealed class PlayerRule
	{
		public float DamageTakenMultiplier { get; }
		public bool? GrantAllWeapons { get; }
		public bool? IgnoreWeightLimit { get; }
		public bool? DebugHotkeys { get; }
		public bool? StartWithoutWeapon { get; }
		public float GunDamageMultiplier { get; }
		public bool? SelfPleasureEnabled { get; }
		public float SelfPleasurePerSecond { get; }
		public int SelfPleasureHeartCost { get; }
		public bool? EnemyFinishersEnabled { get; }
		public bool? ClothingDamageEnabled { get; }
		public bool? SafeKnockouts { get; }
		public float PlayerHealthMultiplier { get; }
		public int? MaximumHearts { get; }
		public float PleasureMaximumMultiplier { get; }
		public float LibidoMaximumMultiplier { get; }
		public float PleasureGainMultiplier { get; }
		public float RapeLibidoGainMultiplier { get; }
		public bool? VoluntaryExposeSkipsEscape { get; }
		public bool? RetainBuffsOnClimax { get; }
		public float StrengthDamageWhileRestrainedMultiplier { get; }
		public float StrengthDamageFreeMultiplier { get; }
		public float? ClothingDamageChance { get; }
		public bool? ForceAutomaticWeapons { get; }
		public bool? AutoReloadOnEmpty { get; }
		public float WeaponRangeMultiplier { get; }
		public float CameraZoomMultiplier { get; }
		internal PlayerRule(RuleModuleDocument i_document)
		{
			DamageTakenMultiplier = i_document.PlayerDamageTakenMultiplier ?? 1f;
			GrantAllWeapons = i_document.GrantAllWeapons;
			IgnoreWeightLimit = i_document.IgnoreWeightLimit;
			DebugHotkeys = i_document.DebugHotkeys;
			StartWithoutWeapon = i_document.StartWithoutWeapon;
			GunDamageMultiplier = i_document.GunDamageMultiplier ?? 1f;
			SelfPleasureEnabled = i_document.SelfPleasureEnabled;
			SelfPleasurePerSecond = i_document.SelfPleasurePerSecond ?? 12f;
			SelfPleasureHeartCost = i_document.SelfPleasureHeartCost ?? 0;
			EnemyFinishersEnabled = i_document.EnemyFinishersEnabled;
			ClothingDamageEnabled = i_document.ClothingDamageEnabled;
			SafeKnockouts = i_document.SafeKnockouts;
			PlayerHealthMultiplier = i_document.PlayerHealthMultiplier ?? 1f;
			MaximumHearts = i_document.MaximumHearts;
			PleasureMaximumMultiplier = i_document.PleasureMaximumMultiplier ?? 1f;
			LibidoMaximumMultiplier = i_document.LibidoMaximumMultiplier ?? 1f;
			PleasureGainMultiplier = i_document.PleasureGainMultiplier ?? 1f;
			RapeLibidoGainMultiplier = i_document.RapeLibidoGainMultiplier ?? 1f;
			VoluntaryExposeSkipsEscape = i_document.VoluntaryExposeSkipsEscape;
			RetainBuffsOnClimax = i_document.RetainBuffsOnClimax;
			StrengthDamageWhileRestrainedMultiplier = i_document.StrengthDamageWhileRestrainedMultiplier ?? 1f;
			StrengthDamageFreeMultiplier = i_document.StrengthDamageFreeMultiplier ?? 1f;
			ClothingDamageChance = i_document.ClothingDamageChance;
			ForceAutomaticWeapons = i_document.ForceAutomaticWeapons;
			AutoReloadOnEmpty = i_document.AutoReloadOnEmpty;
			WeaponRangeMultiplier = i_document.WeaponRangeMultiplier ?? 1f;
			CameraZoomMultiplier = i_document.CameraZoomMultiplier ?? 1f;
		}
	}

	public sealed class ConsumableRule
	{
		public string Item { get; }
		public float? HealthRestore { get; }
		public float StrengthRestoreMultiplier { get; }
		public float LibidoReduction { get; }
		public float PleasureReduction { get; }
		public float EffectDurationSeconds { get; }
		public bool RestoreHeart { get; }
		public float InvulnerabilitySeconds { get; }
		public float GunDamageBonus { get; }
		public float AccelerationBonus { get; }
		public float SprintBonus { get; }
		public float DashBonus { get; }
		internal ConsumableRule(RuleModuleDocument i_document)
		{
			Item = i_document.Item; HealthRestore = i_document.HealthRestore;
			StrengthRestoreMultiplier = i_document.StrengthRestoreMultiplier ?? 1f;
			LibidoReduction = i_document.LibidoReduction ?? 0f; PleasureReduction = i_document.PleasureReduction ?? 0f;
			EffectDurationSeconds = i_document.EffectDurationSeconds ?? 0f; RestoreHeart = i_document.RestoreHeart == true;
			InvulnerabilitySeconds = i_document.InvulnerabilitySeconds ?? 0f; GunDamageBonus = i_document.GunDamageBonus ?? 0f;
			AccelerationBonus = i_document.AccelerationBonus ?? 0f; SprintBonus = i_document.SprintBonus ?? 0f; DashBonus = i_document.DashBonus ?? 0f;
		}
	}

	public sealed class EventRewardRule
	{
		public string Trigger { get; }
		public float Health { get; }
		public float Strength { get; }
		public float PleasureReduction { get; }
		public float LibidoReduction { get; }
		public int Money { get; }
		public float PerCompletedExperimentMultiplier { get; }
		internal EventRewardRule(RuleModuleDocument i_document)
		{
			Trigger = i_document.Trigger; Health = i_document.HealthReward ?? 0f; Strength = i_document.StrengthReward ?? 0f;
			PleasureReduction = i_document.PleasureReductionReward ?? 0f; LibidoReduction = i_document.LibidoReductionReward ?? 0f;
			Money = i_document.MoneyReward ?? 0;
			PerCompletedExperimentMultiplier = i_document.PerCompletedExperimentMultiplier ?? 0f;
		}
	}

	public sealed class ExperimentScalingRule
	{
		public float SpeedPercent { get; }
		public float DashPercent { get; }
		public float GunDamagePercent { get; }
		public float HealthPercent { get; }
		public float KnockbackPercent { get; }
		internal ExperimentScalingRule(RuleModuleDocument i_document)
		{
			SpeedPercent = i_document.SpeedPerExperimentPercent ?? 0f; DashPercent = i_document.DashPerExperimentPercent ?? 0f;
			GunDamagePercent = i_document.GunDamagePerExperimentPercent ?? 0f; HealthPercent = i_document.HealthPerExperimentPercent ?? 0f;
			KnockbackPercent = i_document.KnockbackPerExperimentPercent ?? 0f;
		}
	}

	public sealed class ScoringRule
	{
		public int KillScore { get; }
		public int WaveScore { get; }
		public int DamageTakenPenalty { get; }
		internal ScoringRule(RuleModuleDocument i_document)
		{
			KillScore = i_document.KillScore ?? 0;
			WaveScore = i_document.WaveScore ?? 0;
			DamageTakenPenalty = i_document.DamageTakenPenalty ?? 0;
		}
	}

	public sealed class GoalRule
	{
		public int TargetScore { get; }
		public int TargetKills { get; }
		public int TargetWave { get; }
		public string GoalMatch { get; }
		public float TimeLimitSeconds { get; }
		public int MaxDamageEvents { get; }
		internal GoalRule(RuleModuleDocument i_document)
		{
			TargetScore = i_document.TargetScore ?? 0; TargetKills = i_document.TargetKills ?? 0;
			TargetWave = i_document.TargetWave ?? 0; GoalMatch = i_document.GoalMatch ?? "all";
			TimeLimitSeconds = i_document.TimeLimitSeconds ?? 0f; MaxDamageEvents = i_document.MaxDamageEvents ?? 0;
		}
	}

	[JsonObject(MemberSerialization.OptIn)]
	public sealed class RuleProfileDocument
	{
		[JsonProperty("schemaVersion", Required = Required.Always)] public int SchemaVersion { get; set; }
		[JsonProperty("type", Required = Required.Always)] public string Type { get; set; }
		[JsonProperty("id", Required = Required.Always)] public string Id { get; set; }
		[JsonProperty("displayName", Required = Required.Always)] public string DisplayName { get; set; }
		[JsonProperty("description")] public string Description { get; set; }
		[JsonProperty("activation")] public string Activation { get; set; }
		[JsonProperty("modules", Required = Required.Always)] public List<RuleModuleDocument> Modules { get; set; }
	}

	public sealed class WeaponProgressionRule
	{
		public string Trigger { get; }
		public string Selection { get; }
		public ContentId StarterWeapon { get; }
		public IReadOnlyList<ContentId> WeaponPool { get; }
		public bool ReplaceExistingWeapons { get; }

		internal WeaponProgressionRule(RuleModuleDocument i_document, ContentId i_starter, IReadOnlyList<ContentId> i_pool)
		{
			Trigger = i_document.Trigger; Selection = i_document.Selection; StarterWeapon = i_starter;
			WeaponPool = i_pool; ReplaceExistingWeapons = i_document.ReplaceExistingWeapons == true;
		}
	}

	public sealed class SpawnModifierRule
	{
		public float WaveSpawnMultiplier { get; }
		public int WaveSpawnAdd { get; }
		public float MaxStageEnemiesMultiplier { get; }
		public int MaxStageEnemiesAdd { get; }
		public float MaxRoomEnemiesMultiplier { get; }
		public int MaxRoomEnemiesAdd { get; }
		public float SpawnerChanceMultiplier { get; }
		public float SpawnDelayMultiplier { get; }
		public float InitialSpawnDelayMultiplier { get; }
		public RuleModuleDocument Document { get; }

		internal SpawnModifierRule(RuleModuleDocument i_document)
		{
			Document = i_document;
			WaveSpawnMultiplier = i_document.WaveSpawnMultiplier ?? 1f;
			WaveSpawnAdd = i_document.WaveSpawnAdd ?? 0;
			MaxStageEnemiesMultiplier = i_document.MaxStageEnemiesMultiplier ?? 1f;
			MaxStageEnemiesAdd = i_document.MaxStageEnemiesAdd ?? 0;
			MaxRoomEnemiesMultiplier = i_document.MaxRoomEnemiesMultiplier ?? 1f;
			MaxRoomEnemiesAdd = i_document.MaxRoomEnemiesAdd ?? 0;
			SpawnerChanceMultiplier = i_document.SpawnerChanceMultiplier ?? 1f;
			SpawnDelayMultiplier = i_document.SpawnDelayMultiplier ?? 1f;
			InitialSpawnDelayMultiplier = i_document.InitialSpawnDelayMultiplier ?? 1f;
		}
	}

	public sealed class RuleProfileDefinition
	{
		public ContentId Id { get; }
		public string PackId { get; }
		public string Source { get; }
		public string DisplayName { get; }
		public string Description { get; }
		public string Activation { get; }
		public bool IsSelectable => Activation == "selectable";
		public bool IsAutomaticallyActive => Activation == "pack";
		public IReadOnlyList<WeaponProgressionRule> WeaponProgressions { get; }
		public IReadOnlyList<SpawnModifierRule> SpawnModifiers { get; }
		public IReadOnlyList<WaveRule> WaveRules { get; }
		public IReadOnlyList<EconomyRule> EconomyRules { get; }
		public IReadOnlyList<PlayerRule> PlayerRules { get; }
		public IReadOnlyList<ScoringRule> ScoringRules { get; }
		public IReadOnlyList<GoalRule> GoalRules { get; }
		public IReadOnlyList<ConsumableRule> ConsumableRules { get; }
		public IReadOnlyList<EventRewardRule> EventRewardRules { get; }
		public IReadOnlyList<ExperimentScalingRule> ExperimentScalingRules { get; }
		public IReadOnlyList<WeaponTuningRule> WeaponTuningRules { get; }

		internal RuleProfileDefinition(ContentId i_id, string i_packId, string i_source, RuleProfileDocument i_document,
			IReadOnlyList<WeaponProgressionRule> i_progressions, IReadOnlyList<SpawnModifierRule> i_spawnModifiers,
			IReadOnlyList<WaveRule> i_waveRules, IReadOnlyList<EconomyRule> i_economyRules, IReadOnlyList<PlayerRule> i_playerRules,
			IReadOnlyList<ScoringRule> i_scoringRules, IReadOnlyList<GoalRule> i_goalRules,
			IReadOnlyList<ConsumableRule> i_consumableRules, IReadOnlyList<EventRewardRule> i_eventRewardRules,
			IReadOnlyList<ExperimentScalingRule> i_experimentScalingRules, IReadOnlyList<WeaponTuningRule> i_weaponTuningRules)
		{
			Id = i_id; PackId = i_packId; Source = i_source ?? string.Empty; DisplayName = i_document.DisplayName;
			Activation = string.IsNullOrEmpty(i_document.Activation) ? "selectable" : i_document.Activation;
			Description = i_document.Description ?? string.Empty; WeaponProgressions = i_progressions; SpawnModifiers = i_spawnModifiers;
			WaveRules = i_waveRules; EconomyRules = i_economyRules; PlayerRules = i_playerRules;
			ScoringRules = i_scoringRules; GoalRules = i_goalRules; ConsumableRules = i_consumableRules; EventRewardRules = i_eventRewardRules;
			ExperimentScalingRules = i_experimentScalingRules;
			WeaponTuningRules = i_weaponTuningRules;
		}
	}

	public static class RuleProfileRegistry
	{
		private const string PreferenceKey = "ModGameMode";
		private static readonly List<RuleProfileDefinition> m_definitions = new List<RuleProfileDefinition>();
		private static RuleProfileDefinition m_current;
		public static IReadOnlyList<RuleProfileDefinition> Definitions => m_definitions;
		public static IEnumerable<RuleProfileDefinition> SelectableDefinitions
		{
			get
			{
				foreach (RuleProfileDefinition definition in m_definitions)
					if (definition.IsSelectable) yield return definition;
			}
		}

		public static IEnumerable<RuleProfileDefinition> AutomaticallyActiveDefinitions
		{
			get
			{
				foreach (RuleProfileDefinition definition in m_definitions)
					if (definition.IsAutomaticallyActive) yield return definition;
			}
		}

		public static void Initialize(IEnumerable<RuleProfileDefinition> i_definitions)
		{
			m_definitions.Clear();
			if (i_definitions != null) m_definitions.AddRange(i_definitions);
			m_definitions.Sort((left, right) => string.Compare(left.DisplayName, right.DisplayName, StringComparison.OrdinalIgnoreCase));
			m_current = null;
			string saved = UnityEngine.PlayerPrefs.GetString(PreferenceKey, string.Empty);
			foreach (RuleProfileDefinition definition in m_definitions)
				if (definition.IsSelectable && definition.Id.ToString() == saved) { m_current = definition; break; }
			if (m_current == null && !string.IsNullOrEmpty(saved)) UnityEngine.PlayerPrefs.SetString(PreferenceKey, string.Empty);
		}

		public static RuleProfileDefinition Current => m_current;

		public static string CurrentId => Current == null ? string.Empty : Current.Id.ToString();
		public static void SetCurrent(string i_id)
		{
			if (string.IsNullOrEmpty(i_id)) { m_current = null; UnityEngine.PlayerPrefs.SetString(PreferenceKey, string.Empty); return; }
			foreach (RuleProfileDefinition definition in m_definitions)
				if (definition.IsSelectable && definition.Id.ToString() == i_id) { m_current = definition; UnityEngine.PlayerPrefs.SetString(PreferenceKey, i_id); return; }
			m_current = null;
			UnityEngine.PlayerPrefs.SetString(PreferenceKey, string.Empty);
		}
	}

	public sealed class RuleProfileLoadResult
	{
		public RuleProfileDefinition Definition { get; internal set; }
		public ValidationReport Report { get; } = new ValidationReport();
	}

	public static class RuleProfileParser
	{
		public const int SupportedSchemaVersion = 1;

		public static RuleProfileLoadResult Parse(string i_json, string i_packId, string i_source)
		{
			RuleProfileLoadResult result = new RuleProfileLoadResult();
			RuleProfileDocument document;
			try { document = JsonConvert.DeserializeObject<RuleProfileDocument>(i_json, new JsonSerializerSettings { MissingMemberHandling = MissingMemberHandling.Error }); }
			catch (JsonException exception) { Error(result, "json", exception.Message, i_source); return result; }
			if (document == null) { Error(result, "null", "Rule profile resolved to null.", i_source); return result; }
			if (document.SchemaVersion != SupportedSchemaVersion) Error(result, "schema-version", "Unsupported schemaVersion.", i_source);
			if (document.Type != "ruleProfile") Error(result, "type", "Definition type must be 'ruleProfile'.", i_source);
			if (!string.IsNullOrEmpty(document.Activation) && document.Activation != "selectable" && document.Activation != "pack")
				Error(result, "activation", "activation must be 'selectable' or 'pack'.", i_source);
			bool validId = ContentId.TryParse(document.Id, out ContentId id) && id.Namespace == i_packId && id.Path.StartsWith("rule/", StringComparison.Ordinal);
			if (!validId) Error(result, "id", "Rule profile ID must use the defining pack namespace and a rule/ path.", i_source);
			if (string.IsNullOrWhiteSpace(document.DisplayName) || document.DisplayName.Length > 80) Error(result, "display-name", "displayName must contain 1 to 80 characters.", i_source);
			if (document.Modules == null || document.Modules.Count < 1 || document.Modules.Count > 16) Error(result, "modules", "A rule profile requires 1 to 16 modules.", i_source);

			List<WeaponProgressionRule> progressions = new List<WeaponProgressionRule>();
			List<SpawnModifierRule> spawnModifiers = new List<SpawnModifierRule>();
			List<WaveRule> waveRules = new List<WaveRule>();
			List<EconomyRule> economyRules = new List<EconomyRule>();
			List<PlayerRule> playerRules = new List<PlayerRule>();
			List<ScoringRule> scoringRules = new List<ScoringRule>();
			List<GoalRule> goalRules = new List<GoalRule>();
			List<ConsumableRule> consumableRules = new List<ConsumableRule>();
			List<EventRewardRule> eventRewardRules = new List<EventRewardRule>();
			List<ExperimentScalingRule> experimentScalingRules = new List<ExperimentScalingRule>();
			List<WeaponTuningRule> weaponTuningRules = new List<WeaponTuningRule>();
			foreach (RuleModuleDocument module in document.Modules ?? new List<RuleModuleDocument>())
			{
				if (module == null) { Error(result, "module-null", "Rule modules cannot be null.", i_source); continue; }
				if (module.Type == "weaponProgression") ParseWeaponProgression(module, progressions, result, i_source);
				else if (module.Type == "spawnModifiers") ParseSpawnModifiers(module, spawnModifiers, result, i_source);
				else if (module.Type == "waveRules") ParseWaveRules(module, waveRules, result, i_source);
				else if (module.Type == "economyRules") ParseEconomyRules(module, economyRules, result, i_source);
				else if (module.Type == "playerRules") ParsePlayerRules(module, playerRules, result, i_source);
				else if (module.Type == "scoringRules") ParseScoringRules(module, scoringRules, result, i_source);
				else if (module.Type == "goalRules") ParseGoalRules(module, goalRules, result, i_source);
				else if (module.Type == "consumableOverride") ParseConsumableRule(module, consumableRules, result, i_source);
				else if (module.Type == "eventReward") ParseEventReward(module, eventRewardRules, result, i_source);
				else if (module.Type == "experimentScaling") ParseExperimentScaling(module, experimentScalingRules, result, i_source);
				else if (module.Type == "weaponTuning") ParseWeaponTuning(module, weaponTuningRules, result, i_source);
				else Error(result, "module-type", "Unsupported rule module type.", i_source);
			}
			if (result.Report.IsValid) result.Definition = new RuleProfileDefinition(id, i_packId, i_source, document, progressions, spawnModifiers, waveRules, economyRules, playerRules, scoringRules, goalRules, consumableRules, eventRewardRules, experimentScalingRules, weaponTuningRules);
			return result;
		}

		private static void ParseWeaponProgression(RuleModuleDocument i_module, List<WeaponProgressionRule> io_progressions, RuleProfileLoadResult io_result, string i_source)
		{
			if (io_progressions.Count > 0) Error(io_result, "weapon-progression-duplicate", "A profile can contain only one weaponProgression module.", i_source);
			if (i_module.Trigger != "enemyKilled") Error(io_result, "trigger", "weaponProgression currently supports only the enemyKilled trigger.", i_source);
			if (i_module.Selection != "random" && i_module.Selection != "ordered") Error(io_result, "selection", "selection must be random or ordered.", i_source);
			if (!i_module.ReplaceExistingWeapons.HasValue) Error(io_result, "replace-existing-weapons", "replaceExistingWeapons is required for weaponProgression.", i_source);
			bool validStarter = TryWeaponId(i_module.StarterWeapon, out ContentId starter);
			if (!validStarter) Error(io_result, "starter-weapon", "starterWeapon must be a valid item/weapon content ID.", i_source);
			List<ContentId> pool = new List<ContentId>(); HashSet<ContentId> unique = new HashSet<ContentId>();
			if (i_module.WeaponPool == null || i_module.WeaponPool.Count < 1 || i_module.WeaponPool.Count > 256)
				Error(io_result, "weapon-pool", "weaponPool requires 1 to 256 weapon IDs.", i_source);
			else foreach (string value in i_module.WeaponPool)
			{
				if (!TryWeaponId(value, out ContentId weapon) || !unique.Add(weapon)) Error(io_result, "weapon-pool", "weaponPool entries must be unique item/weapon content IDs: " + (value ?? "<null>"), i_source);
				else pool.Add(weapon);
			}
			if (validStarter && pool.Count > 0 && i_module.ReplaceExistingWeapons.HasValue && (i_module.Selection == "random" || i_module.Selection == "ordered"))
				io_progressions.Add(new WeaponProgressionRule(i_module, starter, pool));
		}

		private static void ParseSpawnModifiers(RuleModuleDocument i_module, List<SpawnModifierRule> io_modifiers, RuleProfileLoadResult io_result, string i_source)
		{
			if (io_modifiers.Count > 0) Error(io_result, "spawn-modifiers-duplicate", "A profile can contain only one spawnModifiers module.", i_source);
			ValidateMultiplier(i_module.WaveSpawnMultiplier, 0.1f, 10f, "wave-spawn-multiplier", io_result, i_source);
			ValidateInt(i_module.WaveSpawnAdd, -1000, 1000, "wave-spawn-add", io_result, i_source);
			ValidateMultiplier(i_module.MaxStageEnemiesMultiplier, 0.1f, 10f, "max-stage-enemies-multiplier", io_result, i_source);
			ValidateInt(i_module.MaxStageEnemiesAdd, -1000, 1000, "max-stage-enemies-add", io_result, i_source);
			ValidateMultiplier(i_module.MaxRoomEnemiesMultiplier, 0.1f, 10f, "max-room-enemies-multiplier", io_result, i_source);
			ValidateInt(i_module.MaxRoomEnemiesAdd, -1000, 1000, "max-room-enemies-add", io_result, i_source);
			ValidateMultiplier(i_module.SpawnerChanceMultiplier, 0.1f, 10f, "spawner-chance-multiplier", io_result, i_source);
			ValidateMultiplier(i_module.SpawnDelayMultiplier, 0.1f, 10f, "spawn-delay-multiplier", io_result, i_source);
			ValidateMultiplier(i_module.InitialSpawnDelayMultiplier, 0.1f, 10f, "initial-spawn-delay-multiplier", io_result, i_source);
			ValidateMultiplier(i_module.EnemySpeedVarianceMin, -10f, 10f, "enemy-speed-variance-min", io_result, i_source);
			ValidateMultiplier(i_module.EnemySpeedVarianceMax, -10f, 10f, "enemy-speed-variance-max", io_result, i_source);
			ValidateMultiplier(i_module.EnemyHealthVarianceMin, -0.99f, 10f, "enemy-health-variance-min", io_result, i_source);
			ValidateMultiplier(i_module.EnemyHealthVarianceMax, -0.99f, 10f, "enemy-health-variance-max", io_result, i_source);
			ValidateMultiplier(i_module.EnemyWaveHealthGrowthRandomBase, 0.01f, 10f, "enemy-wave-health-growth", io_result, i_source);
			ValidateInt(i_module.EnemySpeedGrowthAfterWave, 0, 10000, "enemy-speed-growth-after-wave", io_result, i_source);
			ValidateMultiplier(i_module.EnemySpeedGrowthPerWave, 0f, 10f, "enemy-speed-growth-per-wave", io_result, i_source);
			ValidateInt(i_module.ZombieChaseChancePercent, 0, 100, "zombie-chase-chance", io_result, i_source);
			ValidateInt(i_module.ZombieFallChancePercent, 0, 100, "zombie-fall-chance", io_result, i_source);
			ValidateMultiplier(i_module.ZombieChaseCheckSeconds, 0.01f, 300f, "zombie-chase-check", io_result, i_source);
			ValidateMultiplier(i_module.ZombieChaseDurationMin, 0.01f, 300f, "zombie-chase-duration-min", io_result, i_source);
			ValidateMultiplier(i_module.ZombieChaseDurationMax, 0.01f, 300f, "zombie-chase-duration-max", io_result, i_source);
			ValidateMultiplier(i_module.ZombieChaseSpeedMaxBonus, 0f, 100f, "zombie-chase-speed", io_result, i_source);
			ValidateMultiplier(i_module.DeathHoundRetrySeconds, 0.01f, 300f, "death-hound-retry", io_result, i_source);
			ValidateMultiplier(i_module.DeathHoundPrepareSeconds, 0.01f, 300f, "death-hound-prepare", io_result, i_source);
			ValidateMultiplier(i_module.DeathHoundRoamSpeedBonus, 0f, 100f, "death-hound-roam-speed", io_result, i_source);
			ValidateMultiplier(i_module.DeathHoundChargeSpeedBonus, 0f, 100f, "death-hound-charge-speed", io_result, i_source);
			ValidateMultiplier(i_module.DeathHoundChargeSeconds, 0.01f, 300f, "death-hound-charge-seconds", io_result, i_source);
			ValidateMultiplier(i_module.DeathHoundLeapXMultiplier, 0.01f, 10f, "death-hound-leap-x", io_result, i_source);
			ValidateMultiplier(i_module.DeathHoundLandCheckDelay, 0f, 10f, "death-hound-land-delay", io_result, i_source);
			ValidateMultiplier(i_module.DeathHoundPrepareInnerDivisor, 0.01f, 100f, "death-hound-inner-divisor", io_result, i_source);
			ValidateMultiplier(i_module.DeathHoundLeapDistanceDivisor, 0.01f, 100f, "death-hound-leap-divisor", io_result, i_source);
			ValidateMultiplier(i_module.DeathHoundHitDistance, 0.01f, 100f, "death-hound-hit-distance", io_result, i_source);
			ValidateMultiplier(i_module.FlyChargeAccelBonus, 0f, 100f, "fly-charge-accel", io_result, i_source);
			ValidateMultiplier(i_module.FlyChargeSpeedBonus, 0f, 100f, "fly-charge-speed", io_result, i_source);
			ValidateMultiplier(i_module.FlyRechargeDelayMultiplier, 0.1f, 10f, "fly-recharge-delay", io_result, i_source);
			ValidateMultiplier(i_module.MindBreakTargetMultiplier, 0.01f, 10f, "mind-break-target", io_result, i_source);
			ValidateInt(i_module.HypnosisProgressMultiplier, 1, 100, "hypnosis-progress", io_result, i_source);
			ValidateInt(i_module.LegacyShackTankLimit, 0, 100, "legacy-shack-tank-limit", io_result, i_source);
			ValidateMultiplier(i_module.LegacyShackTankDelaySeconds, 0f, 300f, "legacy-shack-tank-delay", io_result, i_source);
			ValidateInt(i_module.LegacyShackBoomerLimit, 0, 100, "legacy-shack-boomer-limit", io_result, i_source);
			ValidateMultiplier(i_module.LegacyShackBoomerDelaySeconds, 0f, 300f, "legacy-shack-boomer-delay", io_result, i_source);
			ValidateInt(i_module.LegacyShackPinkHoundLimit, 0, 100, "legacy-shack-pink-hound-limit", io_result, i_source);
			ValidateMultiplier(i_module.LegacyShackPinkHoundDelaySeconds, 0f, 300f, "legacy-shack-pink-hound-delay", io_result, i_source);
			if (!i_module.WaveSpawnMultiplier.HasValue && !i_module.WaveSpawnAdd.HasValue &&
				!i_module.MaxStageEnemiesMultiplier.HasValue && !i_module.MaxStageEnemiesAdd.HasValue &&
				!i_module.MaxRoomEnemiesMultiplier.HasValue && !i_module.MaxRoomEnemiesAdd.HasValue &&
				!i_module.SpawnerChanceMultiplier.HasValue && !i_module.SpawnDelayMultiplier.HasValue &&
				!i_module.InitialSpawnDelayMultiplier.HasValue && !i_module.EnemySpeedVarianceMin.HasValue &&
				!i_module.EnemySpeedVarianceMax.HasValue && !i_module.EnemyHealthVarianceMin.HasValue && !i_module.EnemyHealthVarianceMax.HasValue &&
				!i_module.EnemyWaveHealthGrowthRandomBase.HasValue && !i_module.EnemySpeedGrowthAfterWave.HasValue && !i_module.EnemySpeedGrowthPerWave.HasValue &&
				!i_module.ZombieChaseChancePercent.HasValue && !i_module.ZombieFallChancePercent.HasValue && !i_module.ZombieChaseCheckSeconds.HasValue &&
				!i_module.ZombieChaseDurationMin.HasValue && !i_module.ZombieChaseDurationMax.HasValue && !i_module.ZombieChaseSpeedMaxBonus.HasValue &&
				!i_module.DeathHoundRetrySeconds.HasValue && !i_module.DeathHoundPrepareSeconds.HasValue && !i_module.DeathHoundRoamSpeedBonus.HasValue && !i_module.DeathHoundChargeSpeedBonus.HasValue &&
				!i_module.DeathHoundChargeSeconds.HasValue && !i_module.DeathHoundLeapXMultiplier.HasValue && !i_module.DeathHoundLandCheckDelay.HasValue &&
				!i_module.DeathHoundPrepareInnerDivisor.HasValue && !i_module.DeathHoundLeapDistanceDivisor.HasValue && !i_module.DeathHoundHitDistance.HasValue &&
				!i_module.FlyChargeAccelBonus.HasValue && !i_module.FlyChargeSpeedBonus.HasValue && !i_module.FlyRechargeDelayMultiplier.HasValue &&
				!i_module.MindBreakTargetMultiplier.HasValue && !i_module.HypnosisProgressMultiplier.HasValue && !i_module.LegacyShackAberrants.HasValue &&
				!i_module.LegacyShackTankLimit.HasValue && !i_module.LegacyShackTankDelaySeconds.HasValue &&
				!i_module.LegacyShackBoomerLimit.HasValue && !i_module.LegacyShackBoomerDelaySeconds.HasValue &&
				!i_module.LegacyShackPinkHoundLimit.HasValue && !i_module.LegacyShackPinkHoundDelaySeconds.HasValue && !i_module.LegacyShackBoomerExplodes.HasValue)
				Error(io_result, "spawn-modifiers-empty", "spawnModifiers must define at least one supported modifier.", i_source);
			io_modifiers.Add(new SpawnModifierRule(i_module));
		}

		private static void ParseWaveRules(RuleModuleDocument i_module, List<WaveRule> io_rules, RuleProfileLoadResult io_result, string i_source)
		{
			if (io_rules.Count > 0) Error(io_result, "wave-rules-duplicate", "A profile can contain only one waveRules module.", i_source);
			ValidateInt(i_module.SpawnGrowthPerWave, 0, 1000, "spawn-growth-per-wave", io_result, i_source);
			ValidateInt(i_module.IntermissionSeconds, 0, 300, "intermission-seconds", io_result, i_source);
			ValidateInt(i_module.MaximumWaves, 1, 10000, "maximum-waves", io_result, i_source);
			if (!i_module.SpawnGrowthPerWave.HasValue && !i_module.IntermissionSeconds.HasValue && !i_module.MaximumWaves.HasValue)
				Error(io_result, "wave-rules-empty", "waveRules must define at least one supported rule.", i_source);
			io_rules.Add(new WaveRule(i_module));
		}

		private static void ParseEconomyRules(RuleModuleDocument i_module, List<EconomyRule> io_rules, RuleProfileLoadResult io_result, string i_source)
		{
			if (io_rules.Count > 0) Error(io_result, "economy-rules-duplicate", "A profile can contain only one economyRules module.", i_source);
			ValidateInt(i_module.StartingMoney, 0, 1000000, "starting-money", io_result, i_source);
			ValidateMultiplier(i_module.BountyMultiplier, 0f, 10f, "bounty-multiplier", io_result, i_source);
			ValidateMultiplier(i_module.AmmoDropChance, 0f, 1f, "ammo-drop-chance", io_result, i_source);
			ValidateMultiplier(i_module.AmmoDropChancePerWave, -1f, 1f, "ammo-drop-chance-per-wave", io_result, i_source);
			ValidateMultiplier(i_module.AmmoDropChanceMinimum, 0f, 1f, "ammo-drop-chance-minimum", io_result, i_source);
			ValidateMultiplier(i_module.ClothingRepairCostMultiplier, 0f, 10f, "clothing-repair-cost-multiplier", io_result, i_source);
			ValidateMultiplier(i_module.AmmoDropLifetimeSeconds, 1f, 600f, "ammo-drop-lifetime-seconds", io_result, i_source);
			Dictionary<ContentId, int> values = ParseOffsets(i_module.ItemValueOffsets, false, "item-value-offsets", io_result, i_source);
			Dictionary<ContentId, int> ammo = ParseOffsets(i_module.WeaponAmmoCapacityOffsets, true, "weapon-ammo-capacity-offsets", io_result, i_source);
			if (!i_module.StartingMoney.HasValue && !i_module.InfiniteMoney.HasValue && !i_module.BountyMultiplier.HasValue && !i_module.AmmoDropChance.HasValue
				&& !i_module.AmmoDropChancePerWave.HasValue && !i_module.AmmoDropChanceMinimum.HasValue
				&& !i_module.RepeatConsumablePurchases.HasValue && !i_module.ClothingRepairEnabled.HasValue && !i_module.ClothingRepairCostMultiplier.HasValue
				&& !i_module.AmmoDropLifetimeSeconds.HasValue && !i_module.VendorsAlwaysAvailable.HasValue
				&& values.Count == 0 && ammo.Count == 0)
				Error(io_result, "economy-rules-empty", "economyRules must define at least one supported rule.", i_source);
			io_rules.Add(new EconomyRule(i_module, values, ammo));
		}

		private static Dictionary<ContentId, int> ParseOffsets(Dictionary<string, int> i_values, bool i_weaponsOnly, string i_code, RuleProfileLoadResult io_result, string i_source)
		{
			Dictionary<ContentId, int> result = new Dictionary<ContentId, int>();
			if (i_values == null) return result;
			if (i_values.Count > 64) Error(io_result, i_code, i_code + " supports at most 64 entries.", i_source);
			foreach (KeyValuePair<string, int> entry in i_values)
			{
				if (!ContentId.TryParse(entry.Key, out ContentId id) || !id.Path.StartsWith("item/", StringComparison.Ordinal)
					|| (i_weaponsOnly && !id.Path.StartsWith("item/weapon/", StringComparison.Ordinal)) || entry.Value < -100000 || entry.Value > 100000)
					Error(io_result, i_code, "Invalid content ID or offset: " + entry.Key, i_source);
				else result[id] = entry.Value;
			}
			return result;
		}

		private static void ParseWeaponTuning(RuleModuleDocument i_module, List<WeaponTuningRule> io_rules, RuleProfileLoadResult io_result, string i_source)
		{
			if (io_rules.Count > 0) Error(io_result, "weapon-tuning-duplicate", "A profile can contain only one weaponTuning module.", i_source);
			float?[] values = { i_module.PistolEquipSeconds, i_module.PistolReloadSeconds, i_module.SmgOneHandedEquipSeconds, i_module.SmgOneHandedReloadSeconds,
				i_module.SmgTwoHandedEquipSeconds, i_module.SmgTwoHandedReloadSeconds, i_module.ShotgunOneHandedEquipSeconds, i_module.ShotgunOneHandedReloadSeconds,
				i_module.ShotgunTwoHandedEquipSeconds, i_module.ShotgunTwoHandedReloadSeconds, i_module.RifleEquipSeconds, i_module.RifleReloadSeconds,
				i_module.SingleBarrelReloadSeconds, i_module.MinimumShotDelaySeconds };
			bool any = false;
			foreach (float? value in values) { if (!value.HasValue) continue; any = true; ValidateMultiplier(value, 0.02f, 30f, "weapon-timing-seconds", io_result, i_source); }
			if (!any) Error(io_result, "weapon-tuning-empty", "weaponTuning must define at least one timing.", i_source);
			io_rules.Add(new WeaponTuningRule(i_module));
		}

		private static void ParsePlayerRules(RuleModuleDocument i_module, List<PlayerRule> io_rules, RuleProfileLoadResult io_result, string i_source)
		{
			if (io_rules.Count > 0) Error(io_result, "player-rules-duplicate", "A profile can contain only one playerRules module.", i_source);
			ValidateMultiplier(i_module.PlayerDamageTakenMultiplier, 0f, 10f, "player-damage-taken-multiplier", io_result, i_source);
			ValidateMultiplier(i_module.GunDamageMultiplier, 0.1f, 10f, "gun-damage-multiplier", io_result, i_source);
			ValidateMultiplier(i_module.SelfPleasurePerSecond, 0.1f, 1000f, "self-pleasure-per-second", io_result, i_source);
			ValidateInt(i_module.SelfPleasureHeartCost, 0, 1, "self-pleasure-heart-cost", io_result, i_source);
			ValidateMultiplier(i_module.PlayerHealthMultiplier, 0.1f, 10f, "player-health-multiplier", io_result, i_source);
			ValidateInt(i_module.MaximumHearts, 1, 12, "maximum-hearts", io_result, i_source);
			ValidateMultiplier(i_module.PleasureMaximumMultiplier, 0.1f, 10f, "pleasure-maximum-multiplier", io_result, i_source);
			ValidateMultiplier(i_module.LibidoMaximumMultiplier, 0.1f, 10f, "libido-maximum-multiplier", io_result, i_source);
			ValidateMultiplier(i_module.PleasureGainMultiplier, 0f, 10f, "pleasure-gain-multiplier", io_result, i_source);
			ValidateMultiplier(i_module.RapeLibidoGainMultiplier, 0f, 10f, "rape-libido-gain-multiplier", io_result, i_source);
			ValidateMultiplier(i_module.StrengthDamageWhileRestrainedMultiplier, 0f, 10f, "strength-damage-while-restrained-multiplier", io_result, i_source);
			ValidateMultiplier(i_module.StrengthDamageFreeMultiplier, 0f, 10f, "strength-damage-free-multiplier", io_result, i_source);
			ValidateMultiplier(i_module.ClothingDamageChance, 0f, 1f, "clothing-damage-chance", io_result, i_source);
			ValidateMultiplier(i_module.WeaponRangeMultiplier, 0.1f, 10f, "weapon-range-multiplier", io_result, i_source);
			ValidateMultiplier(i_module.CameraZoomMultiplier, 0.25f, 4f, "camera-zoom-multiplier", io_result, i_source);
			if (!i_module.PlayerDamageTakenMultiplier.HasValue && !i_module.GrantAllWeapons.HasValue && !i_module.IgnoreWeightLimit.HasValue && !i_module.DebugHotkeys.HasValue && !i_module.StartWithoutWeapon.HasValue && !i_module.GunDamageMultiplier.HasValue
				&& !i_module.SelfPleasureEnabled.HasValue && !i_module.SelfPleasurePerSecond.HasValue && !i_module.SelfPleasureHeartCost.HasValue
				&& !i_module.EnemyFinishersEnabled.HasValue && !i_module.ClothingDamageEnabled.HasValue
				&& !i_module.SafeKnockouts.HasValue && !i_module.PlayerHealthMultiplier.HasValue && !i_module.MaximumHearts.HasValue
				&& !i_module.PleasureMaximumMultiplier.HasValue && !i_module.LibidoMaximumMultiplier.HasValue && !i_module.PleasureGainMultiplier.HasValue &&
				!i_module.RapeLibidoGainMultiplier.HasValue && !i_module.VoluntaryExposeSkipsEscape.HasValue && !i_module.RetainBuffsOnClimax.HasValue
				&& !i_module.StrengthDamageWhileRestrainedMultiplier.HasValue && !i_module.StrengthDamageFreeMultiplier.HasValue
				&& !i_module.ClothingDamageChance.HasValue && !i_module.ForceAutomaticWeapons.HasValue && !i_module.AutoReloadOnEmpty.HasValue
				&& !i_module.WeaponRangeMultiplier.HasValue && !i_module.CameraZoomMultiplier.HasValue)
				Error(io_result, "player-rules-empty", "playerRules must define at least one supported rule.", i_source);
			io_rules.Add(new PlayerRule(i_module));
		}

		private static void ParseConsumableRule(RuleModuleDocument i_module, List<ConsumableRule> io_rules, RuleProfileLoadResult io_result, string i_source)
		{
			if (io_rules.Count >= 16) Error(io_result, "consumable-count", "A profile supports at most 16 consumable overrides.", i_source);
			if (i_module.Item != "aspirin" && i_module.Item != "morphine" && i_module.Item != "anaphrodisiac" && i_module.Item != "buffout" && i_module.Item != "psycho" && i_module.Item != "second-wind" && i_module.Item != "siid-21")
				Error(io_result, "consumable-item", "item must be aspirin, morphine, anaphrodisiac, buffout, psycho, second-wind, or siid-21.", i_source);
			foreach (ConsumableRule rule in io_rules) if (rule.Item == i_module.Item) Error(io_result, "consumable-duplicate", "Each consumable can be overridden only once.", i_source);
			ValidateMultiplier(i_module.HealthRestore, 0f, 10000f, "health-restore", io_result, i_source);
			ValidateMultiplier(i_module.StrengthRestoreMultiplier, 0f, 10f, "strength-restore-multiplier", io_result, i_source);
			ValidateMultiplier(i_module.LibidoReduction, 0f, 10000f, "libido-reduction", io_result, i_source);
			ValidateMultiplier(i_module.PleasureReduction, 0f, 10000f, "pleasure-reduction", io_result, i_source);
			ValidateMultiplier(i_module.EffectDurationSeconds, 0f, 100000f, "effect-duration-seconds", io_result, i_source);
			ValidateMultiplier(i_module.InvulnerabilitySeconds, 0f, 10000f, "invulnerability-seconds", io_result, i_source);
			ValidateMultiplier(i_module.GunDamageBonus, 0f, 100f, "gun-damage-bonus", io_result, i_source);
			ValidateMultiplier(i_module.AccelerationBonus, 0f, 100f, "acceleration-bonus", io_result, i_source);
			ValidateMultiplier(i_module.SprintBonus, 0f, 100f, "sprint-bonus", io_result, i_source);
			ValidateMultiplier(i_module.DashBonus, 0f, 100f, "dash-bonus", io_result, i_source);
			io_rules.Add(new ConsumableRule(i_module));
		}

		private static void ParseEventReward(RuleModuleDocument i_module, List<EventRewardRule> io_rules, RuleProfileLoadResult io_result, string i_source)
		{
			if (io_rules.Count >= 16) Error(io_result, "event-reward-count", "A profile supports at most 16 event rewards.", i_source);
			if (i_module.Trigger != "enemyKilled" && i_module.Trigger != "playerClimax" && i_module.Trigger != "playerBirth" &&
				i_module.Trigger != "playerInfusion" && i_module.Trigger != "playerImplantation")
				Error(io_result, "event-reward-trigger", "eventReward trigger must be enemyKilled, playerClimax, playerBirth, playerInfusion, or playerImplantation.", i_source);
			ValidateMultiplier(i_module.HealthReward, 0f, 10000f, "health-reward", io_result, i_source);
			ValidateMultiplier(i_module.StrengthReward, 0f, 10000f, "strength-reward", io_result, i_source);
			ValidateMultiplier(i_module.PleasureReductionReward, 0f, 10000f, "pleasure-reduction-reward", io_result, i_source);
			ValidateMultiplier(i_module.LibidoReductionReward, 0f, 10000f, "libido-reduction-reward", io_result, i_source);
			ValidateInt(i_module.MoneyReward, 0, 1000000, "money-reward", io_result, i_source);
			ValidateMultiplier(i_module.PerCompletedExperimentMultiplier, 0f, 10f, "per-completed-experiment-multiplier", io_result, i_source);
			if (!i_module.HealthReward.HasValue && !i_module.StrengthReward.HasValue && !i_module.PleasureReductionReward.HasValue && !i_module.LibidoReductionReward.HasValue && !i_module.MoneyReward.HasValue)
				Error(io_result, "event-reward-empty", "eventReward must define at least one reward.", i_source);
			io_rules.Add(new EventRewardRule(i_module));
		}

		private static void ParseExperimentScaling(RuleModuleDocument i_module, List<ExperimentScalingRule> io_rules, RuleProfileLoadResult io_result, string i_source)
		{
			if (io_rules.Count > 0) Error(io_result, "experiment-scaling-duplicate", "A profile can contain only one experimentScaling module.", i_source);
			ValidateMultiplier(i_module.SpeedPerExperimentPercent, 0f, 100f, "speed-per-experiment-percent", io_result, i_source);
			ValidateMultiplier(i_module.DashPerExperimentPercent, 0f, 100f, "dash-per-experiment-percent", io_result, i_source);
			ValidateMultiplier(i_module.GunDamagePerExperimentPercent, 0f, 100f, "gun-damage-per-experiment-percent", io_result, i_source);
			ValidateMultiplier(i_module.HealthPerExperimentPercent, 0f, 100f, "health-per-experiment-percent", io_result, i_source);
			ValidateMultiplier(i_module.KnockbackPerExperimentPercent, 0f, 100f, "knockback-per-experiment-percent", io_result, i_source);
			if (!i_module.SpeedPerExperimentPercent.HasValue && !i_module.DashPerExperimentPercent.HasValue
				&& !i_module.GunDamagePerExperimentPercent.HasValue && !i_module.HealthPerExperimentPercent.HasValue
				&& !i_module.KnockbackPerExperimentPercent.HasValue)
				Error(io_result, "experiment-scaling-empty", "experimentScaling must define at least one percentage.", i_source);
			io_rules.Add(new ExperimentScalingRule(i_module));
		}

		private static void ParseScoringRules(RuleModuleDocument i_module, List<ScoringRule> io_rules, RuleProfileLoadResult io_result, string i_source)
		{
			if (io_rules.Count > 0) Error(io_result, "scoring-rules-duplicate", "A profile can contain only one scoringRules module.", i_source);
			ValidateInt(i_module.KillScore, 0, 1000000, "kill-score", io_result, i_source);
			ValidateInt(i_module.WaveScore, 0, 1000000, "wave-score", io_result, i_source);
			ValidateInt(i_module.DamageTakenPenalty, 0, 1000000, "damage-taken-penalty", io_result, i_source);
			if (!i_module.KillScore.HasValue && !i_module.WaveScore.HasValue && !i_module.DamageTakenPenalty.HasValue)
				Error(io_result, "scoring-rules-empty", "scoringRules must define at least one score value.", i_source);
			io_rules.Add(new ScoringRule(i_module));
		}

		private static void ParseGoalRules(RuleModuleDocument i_module, List<GoalRule> io_rules, RuleProfileLoadResult io_result, string i_source)
		{
			if (io_rules.Count > 0) Error(io_result, "goal-rules-duplicate", "A profile can contain only one goalRules module.", i_source);
			ValidateInt(i_module.TargetScore, 1, 100000000, "target-score", io_result, i_source);
			ValidateInt(i_module.TargetKills, 1, 1000000, "target-kills", io_result, i_source);
			ValidateInt(i_module.TargetWave, 1, 10000, "target-wave", io_result, i_source);
			ValidateInt(i_module.MaxDamageEvents, 1, 1000000, "max-damage-events", io_result, i_source);
			if (i_module.TimeLimitSeconds.HasValue && (float.IsNaN(i_module.TimeLimitSeconds.Value) || float.IsInfinity(i_module.TimeLimitSeconds.Value)
				|| i_module.TimeLimitSeconds.Value < 1f || i_module.TimeLimitSeconds.Value > 86400f)) Error(io_result, "time-limit-seconds", "timeLimitSeconds must be between 1 and 86400.", i_source);
			if (i_module.GoalMatch != null && i_module.GoalMatch != "all" && i_module.GoalMatch != "any") Error(io_result, "goal-match", "goalMatch must be all or any.", i_source);
			if (!i_module.TargetScore.HasValue && !i_module.TargetKills.HasValue && !i_module.TargetWave.HasValue
				&& !i_module.TimeLimitSeconds.HasValue && !i_module.MaxDamageEvents.HasValue)
				Error(io_result, "goal-rules-empty", "goalRules must define at least one victory target or loss limit.", i_source);
			io_rules.Add(new GoalRule(i_module));
		}

		private static bool TryWeaponId(string i_value, out ContentId o_id)
		{
			return ContentId.TryParse(i_value, out o_id) && o_id.Path.StartsWith("item/weapon/", StringComparison.Ordinal);
		}

		private static void ValidateMultiplier(float? i_value, float i_min, float i_max, string i_name, RuleProfileLoadResult io_result, string i_source)
		{
			if (!i_value.HasValue) return;
			float value = i_value.Value;
			if (float.IsNaN(value) || float.IsInfinity(value) || value < i_min || value > i_max)
				Error(io_result, i_name, i_name + " must be between " + i_min + " and " + i_max + ".", i_source);
		}

		private static void ValidateInt(int? i_value, int i_min, int i_max, string i_name, RuleProfileLoadResult io_result, string i_source)
		{
			if (!i_value.HasValue) return;
			if (i_value.Value < i_min || i_value.Value > i_max)
				Error(io_result, i_name, i_name + " must be between " + i_min + " and " + i_max + ".", i_source);
		}

		private static void Error(RuleProfileLoadResult io_result, string i_code, string i_message, string i_source)
		{
			io_result.Report.Add(ValidationSeverity.Error, "rule." + i_code, i_message, i_source);
		}
	}
}
