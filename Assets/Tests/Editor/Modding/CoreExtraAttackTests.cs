using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace CaptivityReloaded.Modding.Tests
{
	// Experimental additionalAttacks: an inherited enemy's new attacks in the original-enemy format, played on its
	// Core template's rig.
	public sealed class CoreExtraAttackTests
	{
		private const string Pack = "example.zombies";
		private const string Zombie = "core:enemy/zombie-1";
		private const string Brawler = "example.zombies:enemy/brawler";

		private static string Q(string i_json) => i_json.Replace('\'', '"');

		private static string Variant(string i_attack, string i_refs = "'slam': 'example.zombies:enemy-animation/slam'") => Q(@"{
  'schemaVersion': 1, 'type': 'enemy', 'id': '" + Brawler + @"', 'displayName': 'Brawler', 'extends': '" + Zombie + @"',
  'spawn': { 'inheritTemplateSpawners': false },
  'animationRefs': { " + i_refs + @" },
  'additionalAttacks': [ " + i_attack + @" ] }");

		private const string Slam = "{ 'id': 'slam', 'type': 'area', 'animation': 'slam', 'chance': 0.5, 'damage': 10, 'cooldownSeconds': 3, 'initiateRange': 1.5, 'hitRange': 1.5, 'areaRadius': 1.2, 'durationSeconds': 1.2 }";

		private static string Clip(string i_id, string i_events, float i_duration = 1.2f) => Q(@"{
  'schemaVersion': 1, 'type': 'enemyAnimation', 'id': 'example.zombies:enemy-animation/" + i_id + @"', 'enemy': '" + Brawler + @"',
  'displayName': 'Slam', 'durationSeconds': " + i_duration.ToString(System.Globalization.CultureInfo.InvariantCulture) + @", 'frameRate': 60, 'loop': false,
  'tracks': [ { 'target': 'bone/head', 'property': 'rotation.z', 'keys': [ { 'time': 0, 'value': 50 } ] } ],
  'events': [ " + i_events + @" ] }");

		private const string OneHit = "{ 'time': 0.5, 'type': 'attackHit' }";

		private ValidationReport m_report;

		[TearDown]
		public void TearDown()
		{
			NormalizedEnemyAnimationRegistry.Initialize(ModLoaderRuntime.EnemyAnimationDefinitions, new ValidationReport());
			NormalizedPlayerAnimationRegistry.Initialize(ModLoaderRuntime.PlayerAnimationDefinitions, new ValidationReport());
			CoreAnimationOverrides.Resolve(ModLoaderRuntime.AssetPatches, ModLoaderRuntime.EnemyDefinitions, ModLoaderRuntime.LoadedPacks, CoreAnimationRuntimeMap.Load(), new ValidationReport());
		}

		private static string Issues(ValidationReport i_report) => string.Join("\n", i_report.Issues.Select(i_issue => i_issue.Severity + " " + i_issue.Code + ": " + i_issue.Message).ToArray());

		private static EnemyDefinitionLoadResult Parse(string i_json) => EnemyDefinitionParser.Parse(i_json, Pack, "brawler.json");

		private static bool Has(ValidationReport i_report, string i_code) => i_report.Issues.Any(i_issue => i_issue.Code == i_code);

		private CoreAnimationOverrideSet Resolve(string i_variant, params string[] i_clips)
		{
			EnemyDefinitionLoadResult variant = Parse(i_variant);
			Assert.That(variant.Report.IsValid, Is.True, Issues(variant.Report));
			m_report = new ValidationReport();
			List<NormalizedEnemyAnimationDefinition> clips = new List<NormalizedEnemyAnimationDefinition>();
			foreach (string json in i_clips)
			{
				NormalizedEnemyAnimationLoadResult clip = NormalizedEnemyAnimationParser.Parse(json, Pack, "clip.json");
				Assert.That(clip.Report.IsValid, Is.True, Issues(clip.Report));
				clips.Add(clip.Definition);
			}
			NormalizedEnemyAnimationRegistry.Initialize(clips, m_report);
			NormalizedPlayerAnimationRegistry.Initialize(new NormalizedPlayerAnimationDefinition[0], m_report);
			SemanticVersion.TryParse("1.0.0", out SemanticVersion version);
			ModPack pack = new ModPack(new ModManifest { SchemaVersion = 1, Id = Pack, DisplayName = Pack, Version = "1.0.0", ModApiVersion = 1, ContentRoots = new List<string> { "content" } }, version, Pack);
			CoreAnimationOverrides.Resolve(new AssetPatchDefinition[0], new[] { variant.Definition }, new[] { pack }, CoreAnimationRuntimeMap.Load(), m_report);
			return CoreAnimationOverrides.For(Brawler, Zombie);
		}

		[Test]
		public void Parse_AnInheritedEnemyAddsAttacksInTheOriginalFormat()
		{
			EnemyDefinitionLoadResult result = Parse(Variant(Slam));
			Assert.That(result.Report.IsValid, Is.True, Issues(result.Report));
			Assert.That(result.Definition.AdditionalAttacks.Single().Id, Is.EqualTo("slam"));
			Assert.That(result.Definition.AdditionalAttacks.Single().AreaRadius, Is.EqualTo(1.2f));
		}

		[TestCase("'type': 'area'", "'type': 'grab'", "enemy.additional-attacks.grab")]
		[TestCase("'animation': 'slam'", "'animation': 'kick'", "enemy.additional-attacks.animation")]
		[TestCase("'id': 'slam',", "'id': 'slam', 'index': 0,", "enemy.additional-attacks.index")]
		[TestCase("'chance': 0.5, ", "", "enemy.behavior.attack-chance")]
		[TestCase("'id': 'slam'", "'id': 'Slam'", "enemy.additional-attacks.id")]
		[TestCase("'type': 'area'", "'type': 'projectile', 'projectileRegion': 'head'", "enemy.behavior.attack-projectile-speed")]
		public void Parse_RejectsAttacksTheCoreRigCannotRun(string i_from, string i_to, string i_code)
		{
			EnemyDefinitionLoadResult result = Parse(Variant(Slam.Replace(i_from, i_to)));
			Assert.That(Has(result.Report, i_code), Is.True, Issues(result.Report));
		}

		[Test]
		public void Parse_OriginalEnemiesListEveryAttackUnderAttacks()
		{
			string original = Variant(Slam).Replace("'extends': '" + Zombie + "',", string.Empty).Replace("\"extends\": \"" + Zombie + "\",", string.Empty);
			Assert.That(Has(Parse(original).Report, "enemy.additional-attacks.original"), Is.True);
		}

		[Test]
		public void Resolve_KeepsAnAttackWhoseClipLandsItsHits()
		{
			CoreAnimationOverrideSet set = Resolve(Variant(Slam), Clip("slam", OneHit));
			Assert.That(m_report.IsValid, Is.True, Issues(m_report));
			Assert.That(set.ExtraAttacks.Select(i_attack => i_attack.Id), Is.EquivalentTo(new[] { "slam" }));
			Assert.That(set.Enemy["slam"].Events.Single().Type, Is.EqualTo("attackHit"), "An attack's own clip carries its hits.");
			Assert.That(CoreAnimationOverrides.For(Zombie, Zombie), Is.Null, "The Core enemy itself gains nothing.");
		}

		[Test]
		public void Resolve_ChecksTheClipsTimingAgainstTheAttack()
		{
			Resolve(Variant(Slam), Clip("slam", string.Empty));
			Assert.That(Has(m_report, "core-extra-attack.hit-timing"), Is.True, Issues(m_report));
			Assert.That(CoreAnimationOverrides.For(Brawler, Zombie)?.ExtraAttacks ?? new List<EnemyAttackDefinition>(), Is.Empty, "An attack that could never hit is dropped.");

			Resolve(Variant(Slam.Replace("'durationSeconds': 1.2", "'durationSeconds': 0.4")), Clip("slam", OneHit));
			Assert.That(Has(m_report, "core-extra-attack.event-after-attack"), Is.True, Issues(m_report));

			string twoStages = Slam.Replace("'type': 'area'", "'type': 'multiStage', 'stages': [ { 'type': 'melee' }, { 'type': 'area' } ]");
			Resolve(Variant(twoStages), Clip("slam", OneHit));
			Assert.That(Has(m_report, "core-extra-attack.stage-events"), Is.True, Issues(m_report));
			Resolve(Variant(twoStages), Clip("slam", OneHit + ", { 'time': 0.8, 'type': 'attackHit' }"));
			Assert.That(m_report.IsValid, Is.True, Issues(m_report));
		}

		[Test]
		public void Resolve_AProjectileShowsABoneOfTheCoreRig()
		{
			string spit = Slam.Replace("'type': 'area'", "'type': 'projectile', 'projectileRegion': 'tail', 'projectileSpeed': 6, 'projectileLifetimeSeconds': 2, 'projectileRadius': 0.2");
			Resolve(Variant(spit), Clip("slam", OneHit));
			Assert.That(Has(m_report, "core-extra-attack.projectile-region"), Is.True, Issues(m_report));
			Resolve(Variant(spit.Replace("'tail'", "'head'")), Clip("slam", OneHit));
			Assert.That(m_report.IsValid, Is.True, Issues(m_report));
		}

		[Test]
		public void Resolve_ACoreStateKeepsItsOwnGameplayEvenWhenAnAttackUsesIt()
		{
			// "idle" is a Core state: its clip plays over that state too, so it may not carry hits.
			Resolve(Variant(Slam.Replace("'animation': 'slam'", "'animation': 'idle'"), "'idle': 'example.zombies:enemy-animation/slam'"), Clip("slam", OneHit, 4f));
			Assert.That(Has(m_report, "core-animation.event"), Is.True, Issues(m_report));
		}

		[Test]
		public void Driver_PlaysAnAttackClipOverTheAnimatorAndHandsBackItsHits()
		{
			CoreAnimationOverrideSet set = Resolve(Variant(Slam), Clip("slam", OneHit));
			CoreAnimationRuntimeMap.Load().TryGet(Zombie, out CoreAnimationRuntimeEnemy rig);
			rig.TryGetBone("head", out CoreAnimationRuntimeBone head);
			GameObject zombie = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Actors/Npcs/npc_zombie1.prefab"));
			try
			{
				Animator animator = zombie.GetComponentsInChildren<Animator>(true).First(i_animator => i_animator.runtimeAnimatorController != null);
				Transform headBone = animator.transform.Find(head.Path);
				Type driverType = Type.GetType("CoreAnimationOverrideDriver, Assembly-CSharp");
				Component driver = zombie.AddComponent(driverType);
				Assert.That((bool)driverType.GetMethod("Configure", new[] { typeof(CoreAnimationOverrideSet), typeof(CoreAnimationRuntimeEnemy) }).Invoke(driver, new object[] { set, rig }), Is.True);
				MethodInfo lateUpdate = driverType.GetMethod("LateUpdate", BindingFlags.Instance | BindingFlags.NonPublic);
				MethodInfo advance = driverType.GetMethod("AdvanceExtra", BindingFlags.Instance | BindingFlags.NonPublic);
				List<string> received = new List<string>();
				Action<EnemyAnimationEventDefinition> onEvent = i_event => received.Add(i_event.Type);
				Assert.That((bool)driverType.GetMethod("PlayExtra").Invoke(driver, new object[] { "slam", onEvent, 0f }), Is.True);

				animator.Rebind();
				animator.Play("Idle", 0, 0f);
				animator.Update(0.1f);
				lateUpdate.Invoke(driver, null);
				Assert.That(Mathf.DeltaAngle(0f, headBone.localEulerAngles.z), Is.EqualTo(50f).Within(0.01f), "The attack's pose wins over the Animator's idle.");
				advance.Invoke(driver, new object[] { 0.6f });
				Assert.That(received, Is.EqualTo(new[] { "attackHit" }), "The hit goes to the attack, not to the game's cosmetic handler.");
				advance.Invoke(driver, new object[] { 0.6f });
				Assert.That(received.Count, Is.EqualTo(1), "A non-looping clip hits once.");

				driverType.GetMethod("StopExtra").Invoke(driver, null);
				driverType.GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(driver, null);
				animator.Update(0.1f);
				lateUpdate.Invoke(driver, null);
				Assert.That(Mathf.DeltaAngle(0f, headBone.localEulerAngles.z), Is.Not.EqualTo(50f).Within(0.01f), "Stopped, the Animator poses the head again.");
			}
			finally { UnityEngine.Object.DestroyImmediate(zombie); }
		}

		[Test]
		public void Controller_WaitsForCoreMovesThatRunOutsideTheAttackList()
		{
			Type controller = Type.GetType("CoreExtraAttackController, Assembly-CSharp");
			Assert.That(controller, Is.Not.Null);
			MethodInfo busy = controller.GetMethod("IsBusyWithOwnMove");
			GameObject zombie = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Actors/Npcs/npc_zombie1.prefab"));
			try
			{
				Component npc = zombie.GetComponent(Type.GetType("NPC, Assembly-CSharp"));
				Assert.That((bool)busy.Invoke(null, new object[] { npc }), Is.False, "Zombie I has no such move.");
			}
			finally { UnityEngine.Object.DestroyImmediate(zombie); }
		}
	}
}
