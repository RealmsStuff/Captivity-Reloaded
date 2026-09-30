using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace CaptivityReloaded.Modding.Tests
{
	public sealed class LegacyRuleProfileRegressionTests
	{
		private static RuleProfileDefinition Load(string i_folder, string i_file, string i_packId)
		{
			string path = Path.Combine(Application.dataPath, "..", "ExampleMods", i_folder, "content", i_file);
			Assert.That(File.Exists(path), Is.True, "Missing legacy conversion: " + path);
			RuleProfileLoadResult result = RuleProfileParser.Parse(File.ReadAllText(path), i_packId, path);
			Assert.That(result.Report.IsValid, Is.True,
				string.Join("\n", result.Report.Issues.Select(issue => issue.Code + ": " + issue.Message)));
			Assert.That(result.Definition, Is.Not.Null);
			return result.Definition;
		}

		[Test]
		public void C4C_PreservesFiveHeartAndExtendedArousalRules()
		{
			RuleProfileDefinition profile = Load("legacy-c4c", "c4c.json", "legacy.c4c");
			PlayerRule player = profile.PlayerRules.Single();
			EconomyRule economy = profile.EconomyRules.Single();
			RuleModuleDocument shack = profile.SpawnModifiers.Single().Document;

			Assert.That(player.MaximumHearts, Is.EqualTo(5));
			Assert.That(player.LibidoMaximumMultiplier, Is.EqualTo(5f));
			Assert.That(player.PleasureGainMultiplier, Is.EqualTo(0.8333333f).Within(0.00001f));
			Assert.That(player.RapeLibidoGainMultiplier, Is.EqualTo(2.5f));
			Assert.That(player.VoluntaryExposeSkipsEscape, Is.True);
			Assert.That(player.RetainBuffsOnClimax, Is.True);
			Assert.That(economy.RepeatConsumablePurchases, Is.True);
			Assert.That(economy.VendorsAlwaysAvailable, Is.True);
			Assert.That(economy.AmmoDropChance, Is.EqualTo(0.08f));
			Assert.That(economy.BountyMultiplier, Is.EqualTo(1f), "C4C did not alter ordinary kill bounties.");
			Assert.That(shack.LegacyShackAberrants, Is.True);
			Assert.That(shack.LegacyShackBoomerLimit, Is.EqualTo(3));
			Assert.That(shack.LegacyShackPinkHoundDelaySeconds, Is.EqualTo(30f));
			Assert.That(shack.LegacyShackBoomerExplodes, Is.False);
			CollectionAssert.AreEquivalent(new[] { "playerInfusion", "playerImplantation" },
				profile.EventRewardRules.Select(rule => rule.Trigger).ToArray());
			Assert.That(profile.ConsumableRules.Single(rule => rule.Item == "psycho").EffectDurationSeconds,
				Is.EqualTo(99999f));
		}

		[Test]
		public void CaptivityMultiTool_PreservesC4CBaselineWithoutSilentCheats()
		{
			RuleProfileDefinition profile = Load("developer-toolkit", "developer-mode.json",
				"legacy.captivity-multi-tool");
			PlayerRule player = profile.PlayerRules.Single();
			EconomyRule economy = profile.EconomyRules.Single();

			Assert.That(profile.Id.ToString(), Is.EqualTo("legacy.captivity-multi-tool:rule/multi-tool"));
			Assert.That(player.MaximumHearts, Is.EqualTo(5));
			Assert.That(player.LibidoMaximumMultiplier, Is.EqualTo(5f));
			Assert.That(player.VoluntaryExposeSkipsEscape, Is.True);
			Assert.That(player.GrantAllWeapons, Is.Not.True, "The original menu granted only the selected weapon.");
			Assert.That(player.DebugHotkeys, Is.Not.True, "CMT used its panel, not permanently enabled debug hotkeys.");
			Assert.That(player.IgnoreWeightLimit, Is.Not.True);
			Assert.That(player.DamageTakenMultiplier, Is.EqualTo(1f));
			Assert.That(economy.InfiniteMoney, Is.Not.True, "The original panel added money on demand.");
			Assert.That(economy.StartingMoney, Is.EqualTo(0));
			Assert.That(economy.RepeatConsumablePurchases, Is.True);
			Assert.That(profile.ConsumableRules, Has.Count.EqualTo(7));
		}

		[Test]
		public void ClothingOverhaul_PreservesShackAberrantsAndClothingDamage()
		{
			RuleProfileDefinition profile = Load("legacy-clothing-overhaul", "clothing-overhaul.json",
				"legacy.clothing-overhaul");

			Assert.That(profile.PlayerRules.Single().ClothingDamageChance, Is.EqualTo(0.41f));
			Assert.That(profile.SpawnModifiers.Single().Document.LegacyShackAberrants, Is.True);
		}

		[Test]
		public void LuinsBalance_PreservesEconomyWeaponAndEnemyTuning()
		{
			RuleProfileDefinition profile = Load("legacy-luins-balance", "luins-balance.json",
				"legacy.luins-balance");
			EconomyRule economy = profile.EconomyRules.Single();
			SpawnModifierRule spawns = profile.SpawnModifiers.Single();

			Assert.That(economy.AmmoDropChance, Is.EqualTo(0.26f));
			Assert.That(economy.AmmoDropChanceMinimum, Is.EqualTo(0.05f));
			Assert.That(profile.WeaponTuningRules, Has.Count.EqualTo(1));
			Assert.That(spawns.SpawnDelayMultiplier, Is.EqualTo(0.85f));
			Assert.That(spawns.Document.ZombieChaseChancePercent, Is.EqualTo(20));
			Assert.That(spawns.Document.FlyChargeSpeedBonus, Is.EqualTo(20f));
		}

		[Test]
		public void Overpower_PreservesZoomRangeAndExperimentScaling()
		{
			RuleProfileDefinition profile = Load("legacy-overpower", "overpower.json", "legacy.overpower");
			PlayerRule player = profile.PlayerRules.Single();
			ExperimentScalingRule experiments = profile.ExperimentScalingRules.Single();

			Assert.That(player.MaximumHearts, Is.EqualTo(5));
			Assert.That(player.WeaponRangeMultiplier, Is.EqualTo(2.0666667f).Within(0.00001f));
			Assert.That(player.CameraZoomMultiplier, Is.EqualTo(1.4f));
			Assert.That(experiments.SpeedPercent, Is.EqualTo(1.6666667f).Within(0.00001f));
			Assert.That(experiments.GunDamagePercent, Is.EqualTo(8f));
			Assert.That(experiments.KnockbackPercent, Is.EqualTo(10f));
		}
	}
}
