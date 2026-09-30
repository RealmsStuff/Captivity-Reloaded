using System;
using System.Collections.Generic;
using CaptivityReloaded.Modding;
using UnityEngine;

public static class ExternalRuleProfileFactory
{
	public static void Schedule()
	{
		if (ModLoaderRuntime.RuleProfileDefinitions.Count > 0)
		{
			ExternalFactoryRunner.GetOrAdd<ExternalRuleProfileController>();
			ExternalFactoryRunner.GetOrAdd<DeveloperToolkitPanel>();
		}
	}

	public static Gun ResolveStarterWeapon(Gun i_fallback)
	{
		foreach (RuleProfileDefinition profile in ActiveProfiles())
			foreach (PlayerRule rule in profile.PlayerRules)
				if (rule.StartWithoutWeapon.HasValue && rule.StartWithoutWeapon.Value) return null;
		foreach (RuleProfileDefinition profile in ActiveProfiles())
			foreach (WeaponProgressionRule progression in profile.WeaponProgressions)
				if (TryResolveGun(progression.StarterWeapon, out Gun gun)) return gun;
		return i_fallback;
	}

	public static int ApplyWaveSpawnCount(int i_baseCount)
	{
		float value = i_baseCount;
		foreach (RuleProfileDefinition profile in ActiveProfiles())
			foreach (SpawnModifierRule modifier in profile.SpawnModifiers)
				value = value * modifier.WaveSpawnMultiplier + modifier.WaveSpawnAdd;
		return Mathf.Clamp(Mathf.RoundToInt(value), 1, 10000);
	}

	public static int ApplyMaxStageEnemies(int i_baseCount)
	{
		float value = i_baseCount;
		foreach (RuleProfileDefinition profile in ActiveProfiles())
			foreach (SpawnModifierRule modifier in profile.SpawnModifiers)
				value = value * modifier.MaxStageEnemiesMultiplier + modifier.MaxStageEnemiesAdd;
		return Mathf.Clamp(Mathf.RoundToInt(value), 1, 10000);
	}

	public static int ApplyMaxRoomEnemies(int i_baseCount)
	{
		float value = i_baseCount;
		foreach (RuleProfileDefinition profile in ActiveProfiles())
			foreach (SpawnModifierRule modifier in profile.SpawnModifiers)
				value = value * modifier.MaxRoomEnemiesMultiplier + modifier.MaxRoomEnemiesAdd;
		return Mathf.Clamp(Mathf.RoundToInt(value), 1, 10000);
	}

	public static float ApplySpawnerChance(float i_baseChance)
	{
		if (i_baseChance <= 0f) return 0f;
		float value = i_baseChance;
		foreach (RuleProfileDefinition profile in ActiveProfiles())
			foreach (SpawnModifierRule modifier in profile.SpawnModifiers)
				value *= modifier.SpawnerChanceMultiplier;
		return Mathf.Clamp(value, 0.001f, 10000f);
	}

	public static float ApplySpawnDelay(float i_baseDelay)
	{
		if (i_baseDelay <= 0f) return 0f;
		float value = i_baseDelay;
		foreach (RuleProfileDefinition profile in ActiveProfiles())
			foreach (SpawnModifierRule modifier in profile.SpawnModifiers)
				value *= modifier.SpawnDelayMultiplier;
		return Mathf.Clamp(value, 0.02f, 300f);
	}

	public static float ApplyInitialSpawnDelay(float i_baseDelay)
	{
		float value = i_baseDelay;
		foreach (RuleProfileDefinition profile in ActiveProfiles())
			foreach (SpawnModifierRule modifier in profile.SpawnModifiers)
				value *= modifier.InitialSpawnDelayMultiplier;
		return Mathf.Clamp(value, 0f, 300f);
	}

	private static RuleModuleDocument SpawnTuning()
	{
		foreach (RuleProfileDefinition profile in ActiveProfiles())
			foreach (SpawnModifierRule modifier in profile.SpawnModifiers) return modifier.Document;
		return null;
	}

	public static float EnemySpeedVarianceMin(float i_fallback) => SpawnTuning()?.EnemySpeedVarianceMin ?? i_fallback;
	public static float EnemySpeedVarianceMax(float i_fallback) => SpawnTuning()?.EnemySpeedVarianceMax ?? i_fallback;
	public static float EnemyHealthVarianceMin(float i_fallback) => SpawnTuning()?.EnemyHealthVarianceMin ?? i_fallback;
	public static float EnemyHealthVarianceMax(float i_fallback) => SpawnTuning()?.EnemyHealthVarianceMax ?? i_fallback;
	public static float EnemyWaveHealthGrowth(float i_amount)
	{
		float? configured = SpawnTuning()?.EnemyWaveHealthGrowthRandomBase;
		if (!configured.HasValue) return i_amount;
		float difficultyHealth = DifficultyRegistry.Current == null ? 1f : DifficultyRegistry.Current.EnemyHealthMultiplier;
		float minimum = Mathf.Max(0.01f, configured.Value + (difficultyHealth - 1f) * 0.2f);
		return i_amount * UnityEngine.Random.Range(minimum, minimum * minimum);
	}
	public static float EnemyPostWaveSpeed(int i_zeroBasedWave)
	{
		RuleModuleDocument tuning = SpawnTuning();
		if (tuning == null || !tuning.EnemySpeedGrowthAfterWave.HasValue || i_zeroBasedWave <= tuning.EnemySpeedGrowthAfterWave.Value) return 0f;
		return (i_zeroBasedWave - tuning.EnemySpeedGrowthAfterWave.Value) * (tuning.EnemySpeedGrowthPerWave ?? 0f);
	}
	public static int ZombieChaseChance(int i_fallback) => SpawnTuning()?.ZombieChaseChancePercent ?? i_fallback;
	public static int ZombieFallChance(int i_fallback) => SpawnTuning()?.ZombieFallChancePercent ?? i_fallback;
	public static float ZombieChaseCheck(float i_fallback) => SpawnTuning()?.ZombieChaseCheckSeconds ?? i_fallback;
	public static float ZombieChaseDurationMin(float i_fallback) => SpawnTuning()?.ZombieChaseDurationMin ?? i_fallback;
	public static float ZombieChaseDurationMax(float i_fallback) => SpawnTuning()?.ZombieChaseDurationMax ?? i_fallback;
	public static float ZombieChaseSpeedMax(float i_fallback) => SpawnTuning()?.ZombieChaseSpeedMaxBonus ?? i_fallback;
	public static float DeathHoundRetry(float i_fallback) => SpawnTuning()?.DeathHoundRetrySeconds ?? i_fallback;
	public static float DeathHoundPrepare(float i_fallback) => SpawnTuning()?.DeathHoundPrepareSeconds ?? i_fallback;
	public static bool TryGetDeathHoundRoamSpeed(out float o_value)
	{
		float? value = SpawnTuning()?.DeathHoundRoamSpeedBonus;
		o_value = value ?? 0f;
		return value.HasValue;
	}
	public static float DeathHoundChargeSpeed(float i_fallback) => SpawnTuning()?.DeathHoundChargeSpeedBonus ?? i_fallback;
	public static float DeathHoundChargeTime(float i_fallback) => SpawnTuning()?.DeathHoundChargeSeconds ?? i_fallback;
	public static float DeathHoundLeapX(float i_fallback) => SpawnTuning()?.DeathHoundLeapXMultiplier ?? i_fallback;
	public static float DeathHoundLandDelay(float i_fallback) => SpawnTuning()?.DeathHoundLandCheckDelay ?? i_fallback;
	public static float DeathHoundInnerDivisor(float i_fallback) => SpawnTuning()?.DeathHoundPrepareInnerDivisor ?? i_fallback;
	public static float DeathHoundLeapDivisor(float i_fallback) => SpawnTuning()?.DeathHoundLeapDistanceDivisor ?? i_fallback;
	public static float DeathHoundHitDistance(float i_fallback) => SpawnTuning()?.DeathHoundHitDistance ?? i_fallback;
	public static float FlyChargeAccel(float i_fallback) => SpawnTuning()?.FlyChargeAccelBonus ?? i_fallback;
	public static float FlyChargeSpeed(float i_fallback) => SpawnTuning()?.FlyChargeSpeedBonus ?? i_fallback;
	public static float FlyRechargeDelay(float i_fallback) => i_fallback * (SpawnTuning()?.FlyRechargeDelayMultiplier ?? 1f);
	public static int MindBreakTarget(int i_fallback) => Mathf.Max(1, Mathf.FloorToInt(i_fallback * (SpawnTuning()?.MindBreakTargetMultiplier ?? 1f)));
	public static int HypnosisProgressCount() => Mathf.Max(1, SpawnTuning()?.HypnosisProgressMultiplier ?? 1);
	public static bool AreLegacyShackAberrantsEnabled() => SpawnTuning()?.LegacyShackAberrants == true;
	public static int LegacyShackTankLimit() => SpawnTuning()?.LegacyShackTankLimit ?? 1;
	public static float LegacyShackTankDelay() => SpawnTuning()?.LegacyShackTankDelaySeconds ?? 30f;
	public static int LegacyShackBoomerLimit() => SpawnTuning()?.LegacyShackBoomerLimit ?? 2;
	public static float LegacyShackBoomerDelay() => SpawnTuning()?.LegacyShackBoomerDelaySeconds ?? 30f;
	public static int LegacyShackPinkHoundLimit() => SpawnTuning()?.LegacyShackPinkHoundLimit ?? 1;
	public static float LegacyShackPinkHoundDelay() => SpawnTuning()?.LegacyShackPinkHoundDelaySeconds ?? 10f;
	public static bool ShouldLegacyShackBoomerExplode() => SpawnTuning()?.LegacyShackBoomerExplodes ?? true;

	public static int GetSpawnGrowthPerWave(int i_fallback)
	{
		foreach (RuleProfileDefinition profile in ActiveProfiles())
			foreach (WaveRule rule in profile.WaveRules) return rule.SpawnGrowthPerWave;
		return i_fallback;
	}

	public static int GetIntermissionSeconds(int i_fallback)
	{
		if (DeveloperToolkitPanel.TryGetIntermissionSeconds(out int developerOverride)) return developerOverride;
		foreach (RuleProfileDefinition profile in ActiveProfiles())
			foreach (WaveRule rule in profile.WaveRules) return rule.IntermissionSeconds;
		return i_fallback;
	}

	public static int GetMaximumWaves()
	{
		foreach (RuleProfileDefinition profile in ActiveProfiles())
			foreach (WaveRule rule in profile.WaveRules) return rule.MaximumWaves;
		return 0;
	}

	public static int GetStartingMoney()
	{
		foreach (RuleProfileDefinition profile in ActiveProfiles())
			foreach (EconomyRule rule in profile.EconomyRules) return rule.StartingMoney;
		return 0;
	}

	public static bool IsInfiniteMoneyEnabled()
	{
		foreach (RuleProfileDefinition profile in ActiveProfiles())
			foreach (EconomyRule rule in profile.EconomyRules)
				if (rule.InfiniteMoney.HasValue) return rule.InfiniteMoney.Value;
		return false;
	}

	public static bool ShouldGrantAllWeapons()
	{
		foreach (RuleProfileDefinition profile in ActiveProfiles())
			foreach (PlayerRule rule in profile.PlayerRules)
				if (rule.GrantAllWeapons.HasValue) return rule.GrantAllWeapons.Value;
		return false;
	}

	public static bool ShouldIgnoreWeightLimit()
	{
		foreach (RuleProfileDefinition profile in ActiveProfiles())
			foreach (PlayerRule rule in profile.PlayerRules)
				if (rule.IgnoreWeightLimit.HasValue) return rule.IgnoreWeightLimit.Value;
		return false;
	}

	public static bool AreDebugHotkeysEnabled()
	{
		foreach (RuleProfileDefinition profile in ActiveProfiles())
			foreach (PlayerRule rule in profile.PlayerRules)
				if (rule.DebugHotkeys.HasValue) return rule.DebugHotkeys.Value;
		return false;
	}

	public static int ApplyBounty(int i_baseBounty)
	{
		float value = i_baseBounty;
		foreach (RuleProfileDefinition profile in ActiveProfiles())
			foreach (EconomyRule rule in profile.EconomyRules) value *= rule.BountyMultiplier;
		return Mathf.Clamp(Mathf.RoundToInt(value), 0, 1000000);
	}

	public static float GetAmmoDropChance(float i_fallback)
	{
		foreach (RuleProfileDefinition profile in ActiveProfiles())
			foreach (EconomyRule rule in profile.EconomyRules)
			{
				int wave = 0;
				if (CommonReferences.Instance != null && CommonReferences.Instance.GetManagerStages() != null &&
					CommonReferences.Instance.GetManagerStages().GetStageCurrent() != null &&
					CommonReferences.Instance.GetManagerStages().GetStageCurrent().GetManagerWave() != null)
					wave = CommonReferences.Instance.GetManagerStages().GetStageCurrent().GetManagerWave().GetNumWaveCurrent();
				return Mathf.Clamp01(Mathf.Max(rule.AmmoDropChanceMinimum, rule.AmmoDropChance + wave * rule.AmmoDropChancePerWave));
			}
		return i_fallback;
	}

	public static float GetAmmoDropLifetime(float i_fallback)
	{
		foreach (RuleProfileDefinition profile in ActiveProfiles()) foreach (EconomyRule rule in profile.EconomyRules) return rule.AmmoDropLifetimeSeconds;
		return i_fallback;
	}

	public static bool ShouldKeepVendorsAvailable()
	{
		foreach (RuleProfileDefinition profile in ActiveProfiles()) foreach (EconomyRule rule in profile.EconomyRules)
			if (rule.VendorsAlwaysAvailable.HasValue) return rule.VendorsAlwaysAvailable.Value;
		return false;
	}

	public static int ApplyPickupValue(PickUpable i_item, int i_value)
	{
		if (i_item == null || !RuntimeContentIdentity.TryResolve(i_item, out ContentId id, out ContentCategory category) || category != ContentCategory.Item) return i_value;
		foreach (RuleProfileDefinition profile in ActiveProfiles()) foreach (EconomyRule rule in profile.EconomyRules)
			if (rule.ItemValueOffsets.TryGetValue(id, out int offset)) i_value += offset;
		return Mathf.Max(0, i_value);
	}

	public static int ApplyWeaponAmmoCapacity(Gun i_gun, int i_value)
	{
		if (i_gun == null || !RuntimeContentIdentity.TryResolve(i_gun, out ContentId id, out ContentCategory category) || category != ContentCategory.Item) return i_value;
		foreach (RuleProfileDefinition profile in ActiveProfiles()) foreach (EconomyRule rule in profile.EconomyRules)
			if (rule.WeaponAmmoCapacityOffsets.TryGetValue(id, out int offset)) i_value += offset;
		return Mathf.Max(0, i_value);
	}

	public static float ApplyWeaponEquipSeconds(Weapon i_weapon, float i_value)
	{
		if (!(i_weapon is Gun gun)) return i_value;
		foreach (RuleProfileDefinition profile in ActiveProfiles()) foreach (WeaponTuningRule rule in profile.WeaponTuningRules)
			i_value = rule.EquipSeconds(gun.GetWeaponType().ToString(), gun.GetHoldTypeGun() == GunHoldType.OneHanded, i_value);
		return i_value;
	}

	public static float ApplyWeaponReloadSeconds(Gun i_gun, float i_value)
	{
		foreach (RuleProfileDefinition profile in ActiveProfiles()) foreach (WeaponTuningRule rule in profile.WeaponTuningRules)
			i_value = rule.ReloadSeconds(i_gun.GetWeaponType().ToString(), i_gun.GetHoldTypeGun() == GunHoldType.OneHanded,
				i_gun.GetReloadTypeGun() == GunReloadType.SingleBarrel, i_value);
		return i_value;
	}

	public static float ApplyMinimumShotDelay(float i_value)
	{
		foreach (RuleProfileDefinition profile in ActiveProfiles()) foreach (WeaponTuningRule rule in profile.WeaponTuningRules)
			if (rule.MinimumShotDelaySeconds > 0f) i_value = Mathf.Max(i_value, rule.MinimumShotDelaySeconds);
		return i_value;
	}

	public static bool ShouldRetainConsumableVendorStock()
	{
		foreach (RuleProfileDefinition profile in ActiveProfiles())
			foreach (EconomyRule rule in profile.EconomyRules)
				if (rule.RepeatConsumablePurchases.HasValue) return rule.RepeatConsumablePurchases.Value;
		return false;
	}

	public static bool IsClothingRepairEnabled()
	{
		foreach (RuleProfileDefinition profile in ActiveProfiles())
			foreach (EconomyRule rule in profile.EconomyRules)
				if (rule.ClothingRepairEnabled.HasValue) return rule.ClothingRepairEnabled.Value;
		return false;
	}

	public static float GetClothingRepairCostMultiplier()
	{
		foreach (RuleProfileDefinition profile in ActiveProfiles())
			foreach (EconomyRule rule in profile.EconomyRules) return rule.ClothingRepairCostMultiplier;
		return 1f;
	}

	public static bool IsSelfPleasureEnabled()
	{
		foreach (RuleProfileDefinition profile in ActiveProfiles())
			foreach (PlayerRule rule in profile.PlayerRules)
				if (rule.SelfPleasureEnabled.HasValue) return rule.SelfPleasureEnabled.Value;
		return false;
	}

	public static float GetSelfPleasurePerSecond()
	{
		foreach (RuleProfileDefinition profile in ActiveProfiles())
			foreach (PlayerRule rule in profile.PlayerRules) return rule.SelfPleasurePerSecond;
		return 12f;
	}

	public static int GetSelfPleasureHeartCost()
	{
		foreach (RuleProfileDefinition profile in ActiveProfiles())
			foreach (PlayerRule rule in profile.PlayerRules) return rule.SelfPleasureHeartCost;
		return 0;
	}

	public static bool AreEnemyFinishersEnabled()
	{
		foreach (RuleProfileDefinition profile in ActiveProfiles())
			foreach (PlayerRule rule in profile.PlayerRules)
				if (rule.EnemyFinishersEnabled.HasValue) return rule.EnemyFinishersEnabled.Value;
		return true;
	}

	public static bool IsClothingDamageEnabled()
	{
		foreach (RuleProfileDefinition profile in ActiveProfiles())
			foreach (PlayerRule rule in profile.PlayerRules)
				if (rule.ClothingDamageEnabled.HasValue) return rule.ClothingDamageEnabled.Value;
		return true;
	}

	public static bool AreSafeKnockoutsEnabled()
	{
		foreach (RuleProfileDefinition profile in ActiveProfiles())
			foreach (PlayerRule rule in profile.PlayerRules)
				if (rule.SafeKnockouts.HasValue) return rule.SafeKnockouts.Value;
		return false;
	}

	public static float GetPlayerHealthMultiplier()
	{
		float value = 1f;
		foreach (RuleProfileDefinition profile in ActiveProfiles())
			foreach (PlayerRule rule in profile.PlayerRules) value *= rule.PlayerHealthMultiplier;
		return Mathf.Clamp(value, 0.1f, 10f);
	}

	public static float ApplyPlayerDamageTaken(float i_damage)
	{
		float value = i_damage;
		foreach (RuleProfileDefinition profile in ActiveProfiles())
			foreach (PlayerRule rule in profile.PlayerRules) value *= rule.DamageTakenMultiplier;
		return value;
	}

	public static float ApplyGunDamageMultiplier(float i_multiplier)
	{
		float value = i_multiplier;
		foreach (RuleProfileDefinition profile in ActiveProfiles())
			foreach (PlayerRule rule in profile.PlayerRules) value *= rule.GunDamageMultiplier;
		return value;
	}

	public static int GetMaximumHearts(int i_fallback)
	{
		foreach (RuleProfileDefinition profile in ActiveProfiles()) foreach (PlayerRule rule in profile.PlayerRules)
			if (rule.MaximumHearts.HasValue) return rule.MaximumHearts.Value;
		return i_fallback;
	}

	public static float ApplyPleasureMaximum(float i_value)
	{
		foreach (RuleProfileDefinition profile in ActiveProfiles()) foreach (PlayerRule rule in profile.PlayerRules) i_value *= rule.PleasureMaximumMultiplier;
		return Mathf.Max(1f, i_value);
	}

	public static float ApplyLibidoMaximum(float i_value)
	{
		foreach (RuleProfileDefinition profile in ActiveProfiles()) foreach (PlayerRule rule in profile.PlayerRules) i_value *= rule.LibidoMaximumMultiplier;
		return Mathf.Max(1f, i_value);
	}

	public static float ApplyPleasureGain(float i_value)
	{
		foreach (RuleProfileDefinition profile in ActiveProfiles()) foreach (PlayerRule rule in profile.PlayerRules) i_value *= rule.PleasureGainMultiplier;
		return Mathf.Max(0f, i_value);
	}

	public static float ApplyRapeLibidoGain(float i_value)
	{
		foreach (RuleProfileDefinition profile in ActiveProfiles()) foreach (PlayerRule rule in profile.PlayerRules) i_value *= rule.RapeLibidoGainMultiplier;
		return Mathf.Max(0f, i_value);
	}

	public static bool ShouldVoluntaryExposeSkipEscape()
	{
		foreach (RuleProfileDefinition profile in ActiveProfiles()) foreach (PlayerRule rule in profile.PlayerRules)
			if (rule.VoluntaryExposeSkipsEscape.HasValue) return rule.VoluntaryExposeSkipsEscape.Value;
		return false;
	}

	public static void NotifyEventReward(string i_trigger)
	{
		ExternalRuleProfileController controller = UnityEngine.Object.FindObjectOfType<ExternalRuleProfileController>();
		if (controller != null) controller.HandleExternalEventReward(i_trigger);
	}

	public static bool ShouldRetainBuffsOnClimax()
	{
		foreach (RuleProfileDefinition profile in ActiveProfiles()) foreach (PlayerRule rule in profile.PlayerRules)
			if (rule.RetainBuffsOnClimax.HasValue) return rule.RetainBuffsOnClimax.Value;
		return false;
	}

	public static float ApplyStrengthDamage(float i_damage, bool i_restrained)
	{
		foreach (RuleProfileDefinition profile in ActiveProfiles()) foreach (PlayerRule rule in profile.PlayerRules)
			i_damage *= i_restrained ? rule.StrengthDamageWhileRestrainedMultiplier : rule.StrengthDamageFreeMultiplier;
		return Mathf.Max(0f, i_damage);
	}

	public static float GetClothingDamageChance(float i_fallback)
	{
		foreach (RuleProfileDefinition profile in ActiveProfiles()) foreach (PlayerRule rule in profile.PlayerRules)
			if (rule.ClothingDamageChance.HasValue) return rule.ClothingDamageChance.Value;
		return i_fallback;
	}

	public static bool ShouldForceAutomaticWeapons()
	{
		foreach (RuleProfileDefinition profile in ActiveProfiles()) foreach (PlayerRule rule in profile.PlayerRules)
			if (rule.ForceAutomaticWeapons.HasValue) return rule.ForceAutomaticWeapons.Value;
		return false;
	}

	public static bool ShouldAutoReloadOnEmpty()
	{
		foreach (RuleProfileDefinition profile in ActiveProfiles()) foreach (PlayerRule rule in profile.PlayerRules)
			if (rule.AutoReloadOnEmpty.HasValue) return rule.AutoReloadOnEmpty.Value;
		return false;
	}

	public static float ApplyWeaponRange(float i_range)
	{
		foreach (RuleProfileDefinition profile in ActiveProfiles()) foreach (PlayerRule rule in profile.PlayerRules) i_range *= rule.WeaponRangeMultiplier;
		return Mathf.Max(0.1f, i_range);
	}

	public static float ApplyCameraZoom(float i_zoom)
	{
		foreach (RuleProfileDefinition profile in ActiveProfiles()) foreach (PlayerRule rule in profile.PlayerRules) i_zoom *= rule.CameraZoomMultiplier;
		return Mathf.Max(0.01f, i_zoom);
	}

	public static bool TryGetConsumableRule(string i_item, out ConsumableRule o_rule)
	{
		foreach (RuleProfileDefinition profile in ActiveProfiles()) foreach (ConsumableRule rule in profile.ConsumableRules)
			if (rule.Item == i_item) { o_rule = rule; return true; }
		o_rule = null;
		return false;
	}

	public static string ActiveModeName => RuleProfileRegistry.Current == null ? "Standard" : RuleProfileRegistry.Current.DisplayName;

	internal static IEnumerable<RuleProfileDefinition> ActiveProfiles()
	{
		RuleProfileDefinition profile = RuleProfileRegistry.Current;
		if (profile != null && ModLoaderRuntime.Registry.TryGet(profile.Id, out ContentRegistration registration)
			&& registration.Category == ContentCategory.Rule) yield return profile;
	}

	internal static bool TryResolveGun(ContentId i_id, out Gun o_gun)
	{
		o_gun = null;
		if (!ModLoaderRuntime.Registry.TryGet(i_id, out ContentRegistration registration)) return false;
		o_gun = registration.RuntimeAsset as Gun;
		return o_gun != null;
	}
}

public sealed class ExternalRuleProfileController : MonoBehaviour
{
	private readonly Dictionary<ContentId, int> m_orderedIndexes = new Dictionary<ContentId, int>();
	private readonly List<StatModifier> m_experimentModifiers = new List<StatModifier>();
	private Player m_experimentModifierPlayer;
	private Player m_player;
	private Stage m_stage;
	private ManagerWave m_wave;
	private int m_score;
	private int m_kills;
	private int m_damageEvents;
	private float m_elapsed;
	private bool m_modeEnded;
	private StatusPlayerHud m_modeHudOwner;
	private StatusPlayerHudItem m_modeHudItem;
	private string m_modeHudText;
	private float m_nextAberrantScan;
	private int m_aberrantWave = -1;
	private readonly Dictionary<LegacyShackAberrantType, int> m_aberrantCounts = new Dictionary<LegacyShackAberrantType, int>();
	private readonly Dictionary<LegacyShackAberrantType, float> m_aberrantLastSpawn = new Dictionary<LegacyShackAberrantType, float>();

	public int CurrentScore => m_score;

	private void Update()
	{
		Player player = FindCurrentPlayer();
		Stage stage = CommonReferences.Instance == null || CommonReferences.Instance.GetManagerStages() == null
			? null : CommonReferences.Instance.GetManagerStages().GetStageCurrent();
		if (player != m_player || stage != m_stage) BindSession(player, stage);
		if (m_player == null || m_stage == null || !m_stage.gameObject.activeInHierarchy || m_stage is StageHub || m_modeEnded) return;
		MaintainDeveloperEconomy();
		MaintainLegacyShackAberrants();
		m_elapsed += Time.deltaTime;
		UpdateModeHud();
		foreach (RuleProfileDefinition profile in ExternalRuleProfileFactory.ActiveProfiles())
			foreach (GoalRule goal in profile.GoalRules)
				if (goal.TimeLimitSeconds > 0f && m_elapsed >= goal.TimeLimitSeconds) { EndMode(false, "Time limit reached"); return; }
	}

	private void MaintainLegacyShackAberrants()
	{
		if (!ExternalRuleProfileFactory.AreLegacyShackAberrantsEnabled() || m_stage == null || Time.time < m_nextAberrantScan) return;
		m_nextAberrantScan = Time.time + 1f;
		int wave = m_wave == null ? 1 : Mathf.Max(1, m_wave.GetNumWaveCurrent());
		if (wave != m_aberrantWave) { m_aberrantWave = wave; m_aberrantCounts.Clear(); m_aberrantLastSpawn.Clear(); }
		foreach (NPC npc in m_stage.GetAllNPCs())
		{
			if (npc == null || npc.IsDead() || npc.GetComponent<LegacyShackAberrant>() != null) continue;
			string name = npc.name.ToLowerInvariant();
			LegacyShackAberrantType type;
			int baseLimit;
			float delay;
			if (name.Contains("grabber")) { type = LegacyShackAberrantType.Tank; baseLimit = ExternalRuleProfileFactory.LegacyShackTankLimit(); delay = ExternalRuleProfileFactory.LegacyShackTankDelay(); }
			else if (name.Contains("hound")) { type = LegacyShackAberrantType.PinkHound; baseLimit = ExternalRuleProfileFactory.LegacyShackPinkHoundLimit(); delay = ExternalRuleProfileFactory.LegacyShackPinkHoundDelay(); }
			else if (name.Contains("zombie1") || name.Contains("zombie2")) { type = LegacyShackAberrantType.Boomer; baseLimit = ExternalRuleProfileFactory.LegacyShackBoomerLimit(); delay = ExternalRuleProfileFactory.LegacyShackBoomerDelay(); }
			else continue;
			if (baseLimit <= 0) continue;
			m_aberrantCounts.TryGetValue(type, out int count);
			bool hasLastSpawn = m_aberrantLastSpawn.TryGetValue(type, out float lastSpawn);
			int limit = baseLimit + baseLimit * wave / 6;
			if (count >= limit || (hasLastSpawn && Time.time - lastSpawn <= delay)) continue;
			npc.gameObject.AddComponent<LegacyShackAberrant>().Initialize(type, ExternalRuleProfileFactory.ShouldLegacyShackBoomerExplode());
			m_aberrantCounts[type] = count + 1;
			m_aberrantLastSpawn[type] = Time.time;
			m_nextAberrantScan = Time.time + 5f;
			break;
		}
	}

	private static void MaintainDeveloperEconomy()
	{
		if (!ExternalRuleProfileFactory.IsInfiniteMoneyEnabled() || CommonReferences.Instance == null) return;
		PlayerController controller = CommonReferences.Instance.GetPlayerController();
		Inventory inventory = controller == null ? null : controller.GetInventory();
		if (inventory != null && inventory.GetMoney() < 1000000) inventory.AddMoney(1000000 - inventory.GetMoney());
	}

	private void BindSession(Player i_player, Stage i_stage)
	{
		if (m_player != null) { m_player.OnKill -= HandleEnemyKilled; m_player.OnTakeDamage -= HandleDamageTaken; m_player.OnOrgasm -= HandlePlayerClimax; m_player.OnBirthEnd -= HandlePlayerBirth; }
		if (m_wave != null) m_wave.OnWaveEnd -= HandleWaveEnded;
		ClearExperimentScaling();
		m_player = i_player; m_stage = i_stage; m_wave = i_stage == null ? null : i_stage.GetManagerWave();
		m_score = 0; m_kills = 0; m_damageEvents = 0; m_elapsed = 0f; m_modeEnded = false;
		m_orderedIndexes.Clear();
		m_aberrantWave = -1; m_aberrantCounts.Clear(); m_aberrantLastSpawn.Clear(); m_nextAberrantScan = 0f;
		RemoveModeHud();
		if (m_player != null) { m_player.OnKill += HandleEnemyKilled; m_player.OnTakeDamage += HandleDamageTaken; m_player.OnOrgasm += HandlePlayerClimax; m_player.OnBirthEnd += HandlePlayerBirth; }
		if (m_wave != null) m_wave.OnWaveEnd += HandleWaveEnded;
		if (m_player != null && i_stage != null && !(i_stage is StageHub) && ExternalRuleProfileFactory.ShouldGrantAllWeapons())
			GrantAllRegisteredGuns();
		if (m_player != null && i_stage != null && !(i_stage is StageHub)) ApplyExperimentScaling();
	}

	private void ApplyExperimentScaling()
	{
		ClearExperimentScaling();
		if (m_player == null) return;
		m_experimentModifierPlayer = m_player;
		int completed = ManagerDB.GetIdsCompletedSeenChallenges().Count;
		if (completed <= 0) return;
		foreach (RuleProfileDefinition profile in ExternalRuleProfileFactory.ActiveProfiles())
		foreach (ExperimentScalingRule scaling in profile.ExperimentScalingRules)
		{
			AddPercentModifier(StatNamePlayer.SpeedSprint, scaling.SpeedPercent * completed);
			AddPercentModifier(StatNamePlayer.PowerDash, scaling.DashPercent * completed);
			AddPercentModifier(StatNamePlayer.DamageMultiplierGun, scaling.GunDamagePercent * completed);
			AddPercentModifier(StatNameActor.HealthMax, scaling.HealthPercent * completed);
			AddPercentModifier(StatNameActor.KnockbackXMultiplier, scaling.KnockbackPercent * completed);
			AddPercentModifier(StatNameActor.KnockbackYMultiplier, scaling.KnockbackPercent * completed);
		}
	}

	private void AddPercentModifier(StatNamePlayer i_stat, float i_percent)
	{
		if (i_percent == 0f) return;
		Stat stat = m_player.GetStat(i_stat.ToString());
		m_experimentModifiers.Add(stat.AddModifier(stat.GetValueBase() * i_percent * 0.01f));
	}

	private void AddPercentModifier(StatNameActor i_stat, float i_percent)
	{
		if (i_percent == 0f) return;
		Stat stat = m_player.GetStat(i_stat.ToString());
		m_experimentModifiers.Add(stat.AddModifier(stat.GetValueBase() * i_percent * 0.01f));
	}

	private void ClearExperimentScaling()
	{
		if (m_experimentModifierPlayer != null)
			foreach (StatModifier modifier in m_experimentModifiers)
				if (modifier != null) m_experimentModifierPlayer.RemoveStatModifier(modifier);
		m_experimentModifiers.Clear();
		m_experimentModifierPlayer = null;
	}

	public void RefreshSelectedProfile()
	{
		ClearExperimentScaling();
		if (m_player != null) m_player.ApplyRuleProfileLimits(false);
		if (m_player != null && m_stage != null && !(m_stage is StageHub) && m_stage.gameObject.activeInHierarchy)
			ApplyExperimentScaling();
	}

	private void GrantAllRegisteredGuns()
	{
		PlayerController controller = CommonReferences.Instance == null ? null : CommonReferences.Instance.GetPlayerController();
		Inventory inventory = controller == null ? null : controller.GetInventory();
		if (inventory == null) return;
		int grantedCount = 0;
		foreach (ContentRegistration entry in ModLoaderRuntime.Registry.GetByCategory(ContentCategory.Item))
		{
			Gun template = entry.RuntimeAsset as Gun;
			if (template == null || HasRegisteredGun(inventory, entry.Id, template.GetName())) continue;
			Gun granted = Instantiate(template, m_player.transform.parent);
			granted.PickUp(m_player);
			inventory.AddItem(granted);
			grantedCount++;
		}
		if (grantedCount > 0 && CommonReferences.Instance.GetManagerHud() != null)
			CommonReferences.Instance.GetManagerHud().GetManagerNotification().CreateNotification(
				"Developer arsenal granted: " + grantedCount + " guns", ColorTextNotification.Other, false);
	}

	private static bool HasRegisteredGun(Inventory i_inventory, ContentId i_id, string i_fallbackName)
	{
		foreach (Gun gun in i_inventory.GetAllGuns())
		{
			if (RuntimeContentIdentity.TryResolve(gun, out ContentId existingId, out ContentCategory category)
				&& category == ContentCategory.Item && existingId == i_id) return true;
			if (gun != null && gun.GetName() == i_fallbackName) return true;
		}
		return false;
	}

	private void OnDestroy()
	{
		if (m_player != null) { m_player.OnKill -= HandleEnemyKilled; m_player.OnTakeDamage -= HandleDamageTaken; m_player.OnOrgasm -= HandlePlayerClimax; m_player.OnBirthEnd -= HandlePlayerBirth; }
		if (m_wave != null) m_wave.OnWaveEnd -= HandleWaveEnded;
		ClearExperimentScaling();
		RemoveModeHud();
	}

	private void HandleEnemyKilled(NPC i_enemy)
	{
		if (m_player == null || m_modeEnded) return;
		m_kills++;
		foreach (RuleProfileDefinition profile in ExternalRuleProfileFactory.ActiveProfiles())
		{
			foreach (ScoringRule scoring in profile.ScoringRules) ChangeScore(scoring.KillScore);
			foreach (WeaponProgressionRule progression in profile.WeaponProgressions)
				if (progression.Trigger == "enemyKilled") ApplyWeaponProgression(profile, progression);
			ApplyEventRewards(profile, "enemyKilled");
		}
		EvaluateVictory();
	}

	private void HandlePlayerClimax()
	{
		foreach (RuleProfileDefinition profile in ExternalRuleProfileFactory.ActiveProfiles()) ApplyEventRewards(profile, "playerClimax");
	}

	private void HandlePlayerBirth()
	{
		foreach (RuleProfileDefinition profile in ExternalRuleProfileFactory.ActiveProfiles()) ApplyEventRewards(profile, "playerBirth");
	}

	public void HandleExternalEventReward(string i_trigger)
	{
		if (m_player == null || m_player.IsDead()) return;
		foreach (RuleProfileDefinition profile in ExternalRuleProfileFactory.ActiveProfiles()) ApplyEventRewards(profile, i_trigger);
	}

	private void ApplyEventRewards(RuleProfileDefinition i_profile, string i_trigger)
	{
		foreach (EventRewardRule reward in i_profile.EventRewardRules)
		{
			if (reward.Trigger != i_trigger) continue;
			float multiplier = 1f + ManagerDB.GetIdsCompletedSeenChallenges().Count * reward.PerCompletedExperimentMultiplier;
			if (reward.Health > 0f) m_player.RestoreHealth(reward.Health * multiplier);
			if (reward.Strength > 0f) m_player.RestoreStrength(reward.Strength * multiplier);
			if (reward.PleasureReduction > 0f) m_player.LosePleasure(reward.PleasureReduction * multiplier);
			if (reward.LibidoReduction > 0f) m_player.LoseLibido(reward.LibidoReduction * multiplier);
			if (reward.Money > 0 && CommonReferences.Instance != null && CommonReferences.Instance.GetPlayerController() != null)
			{
				Inventory inventory = CommonReferences.Instance.GetPlayerController().GetInventory();
				if (inventory != null) inventory.AddMoney(reward.Money);
			}
		}
	}

	private void HandleDamageTaken()
	{
		if (m_modeEnded) return;
		m_damageEvents++;
		foreach (RuleProfileDefinition profile in ExternalRuleProfileFactory.ActiveProfiles())
		{
			foreach (ScoringRule scoring in profile.ScoringRules) ChangeScore(-scoring.DamageTakenPenalty);
			foreach (GoalRule goal in profile.GoalRules)
				if (goal.MaxDamageEvents > 0 && m_damageEvents >= goal.MaxDamageEvents) { EndMode(false, "Damage limit reached"); return; }
		}
		EvaluateVictory();
	}

	private void HandleWaveEnded()
	{
		if (m_modeEnded) return;
		foreach (RuleProfileDefinition profile in ExternalRuleProfileFactory.ActiveProfiles())
			foreach (ScoringRule scoring in profile.ScoringRules) ChangeScore(scoring.WaveScore);
		EvaluateVictory();
	}

	private void ChangeScore(int i_delta)
	{
		m_score = (int)Math.Min(int.MaxValue, Math.Max(0L, (long)m_score + i_delta));
		UpdateModeHud();
	}

	private void UpdateModeHud()
	{
		RuleProfileDefinition profile = RuleProfileRegistry.Current;
		if (profile == null || (profile.ScoringRules.Count == 0 && profile.GoalRules.Count == 0)) { RemoveModeHud(); return; }
		if (m_modeHudItem == null)
		{
			if (CommonReferences.Instance == null || CommonReferences.Instance.GetManagerHud() == null) return;
			m_modeHudOwner = CommonReferences.Instance.GetManagerHud().GetStatusPlayerHud();
			if (m_modeHudOwner == null) return;
			m_modeHudItem = m_modeHudOwner.CreateAndAddStatus(profile.DisplayName, string.Empty, StatusPlayerHudItemColor.Special);
		}
		string description = BuildModeHudText(profile);
		if (description == m_modeHudText) return;
		m_modeHudText = description;
		m_modeHudItem.SetText(profile.DisplayName, description);
	}

	private string BuildModeHudText(RuleProfileDefinition i_profile)
	{
		List<string> values = new List<string>();
		int scoreTarget = 0, killTarget = 0, waveTarget = 0, damageLimit = 0;
		float timeLimit = 0f;
		foreach (GoalRule goal in i_profile.GoalRules)
		{
			scoreTarget = Math.Max(scoreTarget, goal.TargetScore);
			killTarget = Math.Max(killTarget, goal.TargetKills);
			waveTarget = Math.Max(waveTarget, goal.TargetWave);
			damageLimit = Math.Max(damageLimit, goal.MaxDamageEvents);
			timeLimit = Math.Max(timeLimit, goal.TimeLimitSeconds);
		}
		if (i_profile.ScoringRules.Count > 0 || scoreTarget > 0) values.Add("Score " + m_score + (scoreTarget > 0 ? "/" + scoreTarget : string.Empty));
		if (killTarget > 0) values.Add("Kills " + m_kills + "/" + killTarget);
		if (waveTarget > 0) values.Add("Wave " + (m_wave == null ? 0 : m_wave.GetNumWaveCurrent()) + "/" + waveTarget);
		if (damageLimit > 0) values.Add("Hits " + m_damageEvents + "/" + damageLimit);
		if (timeLimit > 0f)
		{
			int seconds = Mathf.Max(0, Mathf.CeilToInt(timeLimit - m_elapsed));
			values.Add("Time " + (seconds / 60).ToString("00") + ":" + (seconds % 60).ToString("00"));
		}
		return string.Join("  |  ", values.ToArray());
	}

	private void RemoveModeHud()
	{
		if (m_modeHudItem != null)
		{
			if (m_modeHudOwner != null) m_modeHudOwner.DestroyStatusItem(m_modeHudItem);
			else Destroy(m_modeHudItem.gameObject);
		}
		m_modeHudOwner = null;
		m_modeHudItem = null;
		m_modeHudText = null;
	}

	private void EvaluateVictory()
	{
		foreach (RuleProfileDefinition profile in ExternalRuleProfileFactory.ActiveProfiles())
			foreach (GoalRule goal in profile.GoalRules)
			{
				int specified = 0; int reached = 0;
				if (goal.TargetScore > 0) { specified++; if (m_score >= goal.TargetScore) reached++; }
				if (goal.TargetKills > 0) { specified++; if (m_kills >= goal.TargetKills) reached++; }
				if (goal.TargetWave > 0) { specified++; if (m_wave != null && m_wave.GetNumWaveCurrent() >= goal.TargetWave) reached++; }
				if (specified > 0 && (goal.GoalMatch == "any" ? reached > 0 : reached == specified))
				{
					EndMode(true, profile.DisplayName + " complete! Score: " + m_score);
					return;
				}
			}
	}

	private void EndMode(bool i_victory, string i_message)
	{
		if (m_modeEnded) return;
		m_modeEnded = true;
		UpdateModeHud();
		if (CommonReferences.Instance != null && CommonReferences.Instance.GetManagerHud() != null)
			CommonReferences.Instance.GetManagerHud().GetManagerNotification().CreateNotification(i_message,
				ColorTextNotification.Other, i_isContinues: false);
		if (i_victory) { if (m_wave != null) m_wave.CompleteRuleGoal(); }
		else if (m_player != null && !m_player.IsDead()) m_player.Die();
	}

	private void ApplyWeaponProgression(RuleProfileDefinition i_profile, WeaponProgressionRule i_progression)
	{
		if (i_progression.WeaponPool.Count == 0) return;
		int index;
		if (i_progression.Selection == "ordered")
		{
			m_orderedIndexes.TryGetValue(i_profile.Id, out index);
			m_orderedIndexes[i_profile.Id] = (index + 1) % i_progression.WeaponPool.Count;
		}
		else index = UnityEngine.Random.Range(0, i_progression.WeaponPool.Count);
		ContentId weaponId = i_progression.WeaponPool[index];
		if (!ExternalRuleProfileFactory.TryResolveGun(weaponId, out Gun template))
		{
			Report("rule.runtime-weapon", "Weapon progression could not resolve " + weaponId + ".", i_profile.Source);
			return;
		}

		PlayerController controller = CommonReferences.Instance.GetPlayerController();
		Inventory inventory = controller == null ? null : controller.GetInventory();
		if (inventory == null) return;
		if (i_progression.ReplaceExistingWeapons)
		{
			m_player.UnEquipEquippedWeapon();
			List<Gun> oldGuns = new List<Gun>(inventory.GetAllGuns());
			foreach (Gun gun in oldGuns)
			{
				inventory.RemovePickUpable(gun);
				if (gun != null) gun.Destroy();
			}
		}

		Gun granted = Instantiate(template, m_player.transform.parent);
		m_player.PickUp(granted, i_isDuplicate: false);
		m_player.EquipWeapon(granted);
		Debug.Log("[ModLoader] " + i_profile.DisplayName + " granted " + weaponId + " after a kill.");
	}

	private static Player FindCurrentPlayer()
	{
		CommonReferences references = FindObjectOfType<CommonReferences>();
		if (references == null) return null;
		PlayerController controller = references.GetPlayerController();
		return controller == null ? null : controller.GetPlayer();
	}

	private static void Report(string i_code, string i_message, string i_source)
	{
		ModLoaderRuntime.LastReport.Add(ValidationSeverity.Error, i_code, i_message, i_source);
	}
}
