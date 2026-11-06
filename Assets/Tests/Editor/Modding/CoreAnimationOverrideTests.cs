using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace CaptivityReloaded.Modding.Tests
{
	// Core animation overrides: an asset patch replacing a Core enemy's clips, an inherited enemy adding its own,
	// their validation, and the runtime driver posing a real Core prefab over its Animator.
	public sealed class CoreAnimationOverrideTests
	{
		private const string Pack = "example.zombies";
		private const string Zombie = "core:enemy/zombie-1";

		private static string Clip(string i_id, string i_enemy, float i_duration, string i_bone = "head", string i_events = "", float i_rotation = 35f) => (@"{
  'schemaVersion': 1, 'type': 'enemyAnimation', 'id': '" + Pack + ":enemy-animation/" + i_id + @"', 'enemy': '" + i_enemy + @"',
  'displayName': 'Tilt', 'durationSeconds': " + i_duration.ToString(System.Globalization.CultureInfo.InvariantCulture) + @", 'frameRate': 60, 'loop': true,
  'tracks': [ { 'target': 'bone/" + i_bone + @"', 'property': 'rotation.z', 'keys': [ { 'time': 0, 'value': " + i_rotation.ToString(System.Globalization.CultureInfo.InvariantCulture) + @" } ] } ]" + i_events + @"
}").Replace('\'', '"');

		private const string PlayerClip = @"{
  ""schemaVersion"": 1, ""type"": ""playerAnimation"", ""id"": ""example.zombies:player-animation/pinned"", ""rig"": ""core:player-rig/alex"",
  ""displayName"": ""Pinned"", ""durationSeconds"": 1, ""frameRate"": 60, ""loop"": true,
  ""tracks"": [ { ""target"": ""bone/head"", ""property"": ""rotation.z"", ""keys"": [ { ""time"": 0, ""value"": -30 } ] } ]
}";

		private static string Patch(string i_id, string i_body) => ("{ 'schemaVersion': 1, 'type': 'assetPatch', 'id': '" + Pack + ":patch/" + i_id + "', 'target': '" + Zombie + "', " + i_body + " }").Replace('\'', '"');

		private ValidationReport m_report;

		[TearDown]
		public void TearDown()
		{
			// Put back what the game loaded.
			NormalizedEnemyAnimationRegistry.Initialize(ModLoaderRuntime.EnemyAnimationDefinitions, new ValidationReport());
			NormalizedPlayerAnimationRegistry.Initialize(ModLoaderRuntime.PlayerAnimationDefinitions, new ValidationReport());
			CoreAnimationOverrides.Resolve(ModLoaderRuntime.AssetPatches, ModLoaderRuntime.EnemyDefinitions, ModLoaderRuntime.LoadedPacks, CoreAnimationRuntimeMap.Load(), new ValidationReport());
		}

		private static string Issues(ValidationReport i_report) => string.Join("\n", i_report.Issues.Select(i_issue => i_issue.Severity + " " + i_issue.Code + ": " + i_issue.Message).ToArray());

		private static ModPack PackOf(string i_id, params string[] i_overrides)
		{
			SemanticVersion.TryParse("1.0.0", out SemanticVersion version);
			return new ModPack(new ModManifest { SchemaVersion = 1, Id = i_id, DisplayName = i_id, Version = "1.0.0", ModApiVersion = 1, ContentRoots = new List<string> { "content" }, Overrides = i_overrides.ToList() }, version, i_id);
		}

		private static NormalizedEnemyAnimationDefinition EnemyClip(string i_json, string i_pack = Pack)
		{
			NormalizedEnemyAnimationLoadResult result = NormalizedEnemyAnimationParser.Parse(i_json, i_pack, i_pack + "/clip.json");
			Assert.That(result.Report.IsValid, Is.True, Issues(result.Report));
			return result.Definition;
		}

		private static AssetPatchDefinition PatchOf(string i_json, string i_pack = Pack)
		{
			AssetPatchLoadResult result = AssetPatchParser.Parse(i_json, i_pack, i_pack + "/patch.json");
			Assert.That(result.Report.IsValid, Is.True, Issues(result.Report));
			return result.Definition;
		}

		// Registers the clips and resolves the patches and enemies, as the bootstrap does.
		private void Resolve(IEnumerable<NormalizedEnemyAnimationDefinition> i_clips, IEnumerable<AssetPatchDefinition> i_patches,
			IEnumerable<EnemyDefinition> i_enemies = null, IEnumerable<ModPack> i_packs = null, IEnumerable<NormalizedPlayerAnimationDefinition> i_playerClips = null)
		{
			m_report = new ValidationReport();
			NormalizedEnemyAnimationRegistry.Initialize(i_clips, m_report);
			NormalizedPlayerAnimationRegistry.Initialize(i_playerClips ?? new NormalizedPlayerAnimationDefinition[0], m_report);
			CoreAnimationOverrides.Resolve(i_patches, i_enemies ?? new EnemyDefinition[0], i_packs ?? new[] { PackOf(Pack) }, CoreAnimationRuntimeMap.Load(), m_report);
		}

		private bool Has(string i_code) => m_report.Issues.Any(i_issue => i_issue.Code == i_code);

		[Test]
		public void AssetPatch_ParsesAnimationsAndPlayerAnimationsWithoutReplacements()
		{
			AssetPatchDefinition patch = PatchOf(Patch("tilt", "'animations': { 'idle': 'example.zombies:enemy-animation/tilt' }, 'playerAnimations': { 'finisher-phase-1': 'example.zombies:player-animation/pinned' }"));
			Assert.That(patch.Replacements, Is.Empty);
			Assert.That(patch.Animations["idle"].ToString(), Is.EqualTo("example.zombies:enemy-animation/tilt"));
			Assert.That(patch.PlayerAnimations["finisher-phase-1"].ToString(), Is.EqualTo("example.zombies:player-animation/pinned"));
		}

		[TestCase("'animations': { 'Idle': 'example.zombies:enemy-animation/tilt' }", "asset-patch.animation-name")]
		[TestCase("'animations': { 'idle': 'example.zombies:patch/tilt' }", "asset-patch.animation-id")]
		[TestCase("'playerAnimations': { 'phase-1': 'example.zombies:player-animation/pinned' }", "asset-patch.player-animation-name")]
		[TestCase("'replacements': { }", "asset-patch.replacements")]
		public void AssetPatch_RejectsBadAnimationEntries(string i_body, string i_code)
		{
			AssetPatchLoadResult result = AssetPatchParser.Parse(Patch("bad", i_body), Pack, "patch.json");
			Assert.That(result.Report.Issues.Any(i_issue => i_issue.Code == i_code), Is.True, Issues(result.Report));
		}

		[Test]
		public void AssetPatch_AnimationsOnlyPatchCoreEnemies()
		{
			string json = Patch("stage", "'animations': { 'idle': 'example.zombies:enemy-animation/tilt' }").Replace(Zombie, "core:stage/field-day");
			AssetPatchLoadResult result = AssetPatchParser.Parse(json, Pack, "patch.json");
			Assert.That(result.Report.Issues.Any(i_issue => i_issue.Code == "asset-patch.animation-target"), Is.True, Issues(result.Report));
		}

		[Test]
		public void EnemyAnimation_APackMayAnimateACoreEnemyButNotAnotherPacksEnemy()
		{
			Assert.That(NormalizedEnemyAnimationParser.Parse(Clip("tilt", Zombie, 4f), Pack, "clip.json").Report.IsValid, Is.True);
			NormalizedEnemyAnimationLoadResult other = NormalizedEnemyAnimationParser.Parse(Clip("tilt", "other.pack:enemy/brute", 4f), Pack, "clip.json");
			Assert.That(other.Report.Issues.Any(i_issue => i_issue.Code == "enemy-animation.enemy"), Is.True);
		}

		[Test]
		public void Resolve_ReplacesACoreClipAndTheGrabsPlayerSide()
		{
			NormalizedPlayerAnimationLoadResult pinned = NormalizedPlayerAnimationParser.Parse(PlayerClip, Pack, "pinned.json");
			Assert.That(pinned.Report.IsValid, Is.True, Issues(pinned.Report));
			Resolve(new[] { EnemyClip(Clip("tilt", Zombie, 4f)) },
				new[] { PatchOf(Patch("tilt", "'animations': { 'idle': 'example.zombies:enemy-animation/tilt' }, 'playerAnimations': { 'finisher-phase-1': 'example.zombies:player-animation/pinned' }")) },
				i_playerClips: new[] { pinned.Definition });
			Assert.That(m_report.IsValid, Is.True, Issues(m_report));
			Assert.That(m_report.Issues.Any(i_issue => i_issue.Severity == ValidationSeverity.Warning), Is.False, Issues(m_report));
			Assert.That(CoreAnimationOverrides.TryGet(Zombie, out CoreAnimationOverrideSet set), Is.True);
			Assert.That(set.Enemy["idle"].Id.ToString(), Is.EqualTo("example.zombies:enemy-animation/tilt"));
			Assert.That(set.Player["finisher-phase-1"].Id.ToString(), Is.EqualTo("example.zombies:player-animation/pinned"));
		}

		[Test]
		public void Resolve_RejectsUnknownSemanticNamesBonesPhasesAndGameplayEvents()
		{
			Resolve(new[] { EnemyClip(Clip("tilt", Zombie, 4f)), EnemyClip(Clip("tail", Zombie, 4f, "tail")),
					EnemyClip(Clip("hit", Zombie, 4f, "head", ", 'events': [ { 'time': 0.5, 'type': 'attackHit' } ]").Replace("\"loop\": true", "\"loop\": false")) },
				new[] { PatchOf(Patch("tilt", "'animations': { 'dance': 'example.zombies:enemy-animation/tilt', 'move': 'example.zombies:enemy-animation/tail', 'attack-primary': 'example.zombies:enemy-animation/hit' }, 'playerAnimations': { 'finisher-phase-9': 'example.zombies:player-animation/pinned' }")) });
			Assert.That(Has("core-animation.semantic"), Is.True, Issues(m_report));
			Assert.That(Has("core-animation.bone"), Is.True, Issues(m_report));
			Assert.That(Has("core-animation.event"), Is.True, Issues(m_report));
			Assert.That(Has("core-animation.phase"), Is.True, Issues(m_report));
			Assert.That(CoreAnimationOverrides.TryGet(Zombie, out _), Is.False, "Nothing invalid plays.");
		}

		[Test]
		public void Resolve_WarnsWhenTheLengthDiffersByMoreThanFivePercent()
		{
			// Zombie I's idle lasts 4 s: 4.15 s is within 5%, 2 s is not.
			Resolve(new[] { EnemyClip(Clip("close", Zombie, 4.15f)) }, new[] { PatchOf(Patch("close", "'animations': { 'idle': 'example.zombies:enemy-animation/close' }")) });
			Assert.That(Has("core-animation.length"), Is.False, Issues(m_report));
			Resolve(new[] { EnemyClip(Clip("short", Zombie, 2f)) }, new[] { PatchOf(Patch("short", "'animations': { 'idle': 'example.zombies:enemy-animation/short' }")) });
			Assert.That(Has("core-animation.length"), Is.True, Issues(m_report));
			Assert.That(CoreAnimationOverrides.TryGet(Zombie, out _), Is.True, "A length warning still plays the clip.");
		}

		[Test]
		public void Resolve_FollowsThePatchConflictRules()
		{
			NormalizedEnemyAnimationDefinition first = EnemyClip(Clip("tilt", Zombie, 4f));
			NormalizedEnemyAnimationDefinition second = EnemyClip(Clip("tilt", Zombie, 4f).Replace(Pack, "second.pack"), "second.pack");
			AssetPatchDefinition firstPatch = PatchOf(Patch("tilt", "'animations': { 'idle': 'example.zombies:enemy-animation/tilt' }"));
			AssetPatchDefinition secondPatch = PatchOf(Patch("tilt", "'animations': { 'idle': 'second.pack:enemy-animation/tilt' }").Replace(Pack + ":patch", "second.pack:patch"), "second.pack");
			Resolve(new[] { first, second }, new[] { firstPatch, secondPatch }, null, new[] { PackOf(Pack), PackOf("second.pack") });
			Assert.That(Has("asset-patch.conflict"), Is.True, Issues(m_report));
			Assert.That(CoreAnimationOverrides.TryGet(Zombie, out _), Is.False, "Unresolved conflicts keep the Core clip.");

			Resolve(new[] { first, second }, new[] { firstPatch, secondPatch }, null, new[] { PackOf(Pack), PackOf("second.pack", "core:enemy/zombie-1/animation/idle") });
			Assert.That(Has("asset-patch.override"), Is.True, Issues(m_report));
			Assert.That(m_report.IsValid, Is.True, Issues(m_report));
			Assert.That(CoreAnimationOverrides.TryGet(Zombie, out CoreAnimationOverrideSet set) ? set.Enemy["idle"].PackId : null, Is.EqualTo("second.pack"));
		}

		[Test]
		public void Resolve_AnInheritedEnemyAddsItsOwnClipsOverTheTemplatesReplacement()
		{
			const string variantJson = @"{ ""schemaVersion"": 1, ""type"": ""enemy"", ""id"": ""example.zombies:enemy/tilted"", ""displayName"": ""Tilted"", ""extends"": ""core:enemy/zombie-1"",
  ""spawn"": { ""inheritTemplateSpawners"": false },
  ""animationRefs"": { ""finisher-phase-1"": ""example.zombies:enemy-animation/hold"" },
  ""playerAnimationRefs"": { ""finisher-phase-1"": ""example.zombies:player-animation/pinned"" } }";
			EnemyDefinitionLoadResult variant = EnemyDefinitionParser.Parse(variantJson, Pack, "tilted.json");
			Assert.That(variant.Report.IsValid, Is.True, Issues(variant.Report));
			NormalizedPlayerAnimationLoadResult pinned = NormalizedPlayerAnimationParser.Parse(PlayerClip, Pack, "pinned.json");
			// Zombie I's first grab phase lasts as long as its Rape1_17 clip; the variant's clip names the variant itself.
			float phaseSeconds = CoreAnimationRuntimeMap.Load().TryGet(Zombie, out CoreAnimationRuntimeEnemy rig) ? rig.OriginalSeconds("finisher-phase-1") ?? 1f : 1f;
			Resolve(new[] { EnemyClip(Clip("tilt", Zombie, 4f)), EnemyClip(Clip("hold", "example.zombies:enemy/tilted", phaseSeconds)) },
				new[] { PatchOf(Patch("tilt", "'animations': { 'idle': 'example.zombies:enemy-animation/tilt' }")) },
				new[] { variant.Definition }, null, new[] { pinned.Definition });
			Assert.That(m_report.IsValid, Is.True, Issues(m_report));
			CoreAnimationOverrideSet set = CoreAnimationOverrides.For("example.zombies:enemy/tilted", Zombie);
			Assert.That(set.Enemy.Keys, Is.EquivalentTo(new[] { "idle", "finisher-phase-1" }), "The variant plays the template's replacement and its own clip.");
			Assert.That(set.Player.Keys, Is.EquivalentTo(new[] { "finisher-phase-1" }));
			Assert.That(CoreAnimationOverrides.For(Zombie, Zombie).Enemy.Keys, Is.EquivalentTo(new[] { "idle" }), "The variant's own clip stays on the variant.");
		}

		[Test]
		public void Parse_PlayerAnimationRefsAreForInheritedEnemiesOnly()
		{
			const string original = @"{ ""schemaVersion"": 1, ""type"": ""enemy"", ""id"": ""example.zombies:enemy/own"", ""displayName"": ""Own"", ""playerAnimationRefs"": { ""finisher-phase-1"": ""example.zombies:player-animation/pinned"" } }";
			EnemyDefinitionLoadResult result = EnemyDefinitionParser.Parse(original, Pack, "own.json");
			Assert.That(result.Report.Issues.Any(i_issue => i_issue.Code == "enemy.player-animation-refs.original"), Is.True, Issues(result.Report));
		}

		[Test]
		public void RuntimeMap_MatchesTheModSdkReference()
		{
			Type exporter = AppDomain.CurrentDomain.GetAssemblies().Select(i_assembly => i_assembly.GetType("CaptivityReloaded.Editor.Modding.CoreAnimationRuntimeMapExporter", false)).FirstOrDefault(i_type => i_type != null);
			Assert.That(exporter, Is.Not.Null);
			object built = exporter.GetMethod("Build").Invoke(null, null);
			string expected = (string)exporter.GetMethod("Serialize").Invoke(null, new[] { built });
			string shipped = File.ReadAllText("Assets/Resources/Modding/Core/core-animation-map.json").Replace("\r\n", "\n");
			Assert.That(shipped, Is.EqualTo(expected.Replace("\r\n", "\n")), "Refresh it: Captivity Reloaded > Modding > Refresh Core Animation Runtime Map.");
		}

		[Test]
		public void Driver_PosesARealCorePrefabOverItsAnimator()
		{
			Resolve(new[] { EnemyClip(Clip("tilt", Zombie, 4f)) }, new[] { PatchOf(Patch("tilt", "'animations': { 'idle': 'example.zombies:enemy-animation/tilt' }")) });
			Assert.That(CoreAnimationOverrides.TryGet(Zombie, out CoreAnimationOverrideSet set), Is.True, Issues(m_report));
			Assert.That(CoreAnimationRuntimeMap.Load().TryGet(Zombie, out CoreAnimationRuntimeEnemy rig), Is.True);
			Assert.That(rig.TryGetBone("head", out CoreAnimationRuntimeBone head), Is.True);
			Assert.That(rig.TryGetBone("hips", out CoreAnimationRuntimeBone hips), Is.True);

			GameObject zombie = UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Actors/Npcs/npc_zombie1.prefab"));
			try
			{
				Animator animator = zombie.GetComponentsInChildren<Animator>(true).First(i_animator => i_animator.runtimeAnimatorController != null);
				Transform headBone = animator.transform.Find(head.Path), hipsBone = animator.transform.Find(hips.Path);
				Type driverType = Type.GetType("CoreAnimationOverrideDriver, Assembly-CSharp");
				Component driver = zombie.AddComponent(driverType);
				Assert.That((bool)driverType.GetMethod("Configure", new[] { typeof(CoreAnimationOverrideSet), typeof(CoreAnimationRuntimeEnemy) }).Invoke(driver, new object[] { set, rig }), Is.True);
				MethodInfo update = driverType.GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic);
				MethodInfo lateUpdate = driverType.GetMethod("LateUpdate", BindingFlags.Instance | BindingFlags.NonPublic);

				animator.Rebind();
				animator.Play("Idle", 0, 0f);
				animator.Update(0f);
				animator.Update(1.3f);
				Assert.That(animator.GetCurrentAnimatorStateInfo(0).fullPathHash, Is.EqualTo(Animator.StringToHash("Base Layer.Idle")));
				float animatorHead = Mathf.DeltaAngle(0f, headBone.localEulerAngles.z), animatorHips = hipsBone.localEulerAngles.z;
				lateUpdate.Invoke(driver, null);
				Assert.That(Mathf.DeltaAngle(0f, headBone.localEulerAngles.z), Is.EqualTo(35f).Within(0.01f), "The override's pose won over the Animator's (" + animatorHead + ").");
				Assert.That(hipsBone.localEulerAngles.z, Is.EqualTo(animatorHips).Within(0.0001f), "Bones the override leaves alone keep the Animator's pose.");
				Assert.That(driverType.GetProperty("CurrentSemantic").GetValue(driver), Is.EqualTo("idle"));

				// Another state: the override steps aside and the head is put back before the Animator poses it again.
				update.Invoke(driver, null);
				animator.Play("Move", 0, 0f);
				animator.Update(0.2f);
				lateUpdate.Invoke(driver, null);
				Assert.That(driverType.GetProperty("IsOverriding").GetValue(driver), Is.EqualTo(false));
				Assert.That(Mathf.DeltaAngle(0f, headBone.localEulerAngles.z), Is.Not.EqualTo(35f).Within(0.01f));
			}
			finally { UnityEngine.Object.DestroyImmediate(zombie); }
		}
	}
}
