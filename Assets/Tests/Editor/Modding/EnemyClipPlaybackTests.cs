using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEngine;

namespace CaptivityReloaded.Modding.Tests
{
	public sealed class EnemyClipPlaybackTests
	{
		private const string Enemy = @"{
  'schemaVersion': 1, 'type': 'enemy', 'id': 'example.enemies:enemy/long-clips', 'displayName': 'Long Clips',
  'stats': { 'healthMax': 10, 'speedAcceleration': 4, 'speedMax': 3, 'traction': 0.2 },
  'spawn': { 'inheritTemplateSpawners': false },
  'behavior': { 'modules': [] },
  'ai': { 'type': 'groundChase', 'preferredRange': 1, 'reactionSeconds': 0.1 },
  'attacks': [{ 'id': 'hit', 'type': 'melee', 'animation': 'idle', 'chance': 1, 'damage': 1,
    'cooldownSeconds': 1, 'initiateRange': 1, 'hitRange': 1, 'durationSeconds': 0.5, 'hitTimeSeconds': 0.2 }],
  'animation': { 'clips': {
    'idle': { 'durationSeconds': 1, 'loop': true, 'frames': [{ 'time': 0, 'bones': { 'hips': { 'y': 0 } } }] },
    'move': { 'durationSeconds': 1, 'loop': true, 'frames': [{ 'time': 0, 'bones': { 'hips': { 'x': 0 } } }] }
  } },
  'animationRefs': { 'long': 'example.enemies:enemy-animation/long-clips/long' },
  'visual': { 'type': 'originalSkeletonAtlas', 'atlas': 'assets/long.png', 'pixelsPerUnit': 32,
    'bodyWidth': 1, 'bodyHeight': 2, 'regions': { 'part': { 'x': 0, 'y': 0, 'width': 16, 'height': 16 } },
    'bones': [{ 'id': 'hips', 'region': 'part' }],
    'hitZones': [{ 'bone': 'hips', 'shape': 'box', 'width': 1, 'height': 1 }]
  }
}";

		// A referenced clip with 300 distinct key times: the runtime plays it from 256 uniform samples and warns.
		private static string LongClip()
		{
			JArray keys = new JArray(Enumerable.Range(0, 300).Select(i_index => new JObject { ["time"] = i_index / 100f, ["value"] = i_index % 2 == 0 ? 0f : 0.1f }));
			return new JObject
			{
				["schemaVersion"] = 1, ["type"] = "enemyAnimation", ["id"] = "example.enemies:enemy-animation/long-clips/long",
				["enemy"] = "example.enemies:enemy/long-clips", ["displayName"] = "Long", ["durationSeconds"] = 3, ["frameRate"] = 100, ["loop"] = false,
				["tracks"] = new JArray(new JObject { ["target"] = "bone/hips", ["property"] = "position.x", ["keys"] = keys })
			}.ToString();
		}

		[Test]
		public void ReferencedClipWithOnlyAWarningIsKept()
		{
			EnemyDefinitionLoadResult enemy = EnemyDefinitionParser.Parse(Enemy, "example.enemies", "long-clips.json");
			Assert.That(enemy.Report.IsValid, Is.True, string.Join("\n", enemy.Report.Issues.Select(i_issue => i_issue.Code + ": " + i_issue.Message).ToArray()));
			NormalizedEnemyAnimationLoadResult clip = NormalizedEnemyAnimationParser.Parse(LongClip(), "example.enemies", "long.enemy-animation.json");
			Assert.That(clip.Report.IsValid, Is.True, string.Join("\n", clip.Report.Issues.Select(i_issue => i_issue.Code + ": " + i_issue.Message).ToArray()));
			ValidationReport report = new ValidationReport();
			try
			{
				NormalizedEnemyAnimationRegistry.Initialize(new[] { clip.Definition }, report);
				MethodInfo resolve = typeof(ModLoaderRuntime).GetMethod("ResolveEnemyAnimationReferences", BindingFlags.Static | BindingFlags.NonPublic);
				Assert.That(resolve, Is.Not.Null);
				resolve.Invoke(null, new object[] { new[] { enemy.Definition }, report });
			}
			finally
			{
				NormalizedEnemyAnimationRegistry.Initialize(new NormalizedEnemyAnimationDefinition[0], new ValidationReport());
			}
			Assert.That(report.Issues.Select(i_issue => i_issue.Code), Does.Contain("enemy-animation.sample-limit"));
			Assert.That(report.IsValid, Is.True);
			Assert.That(enemy.Definition.Animation.Clips.ContainsKey("long"), Is.True, "A warning must not drop the clip.");
		}

		[Test]
		public void AnEventThatStartsAnotherClipEndsTheOldClipsEvents()
		{
			System.Type enemyType = System.Type.GetType("ModularEnemy, Assembly-CSharp");
			System.Type controllerType = System.Type.GetType("ModularEnemyAnimationController, Assembly-CSharp");
			Assert.That(enemyType, Is.Not.Null);
			Assert.That(controllerType, Is.Not.Null);
			GameObject host = new GameObject("enemy-clip-playback-test");
			try
			{
				Component enemy = host.AddComponent(enemyType);
				Component controller = host.AddComponent(controllerType);
				List<EnemyAnimationFrameDefinition> frames = new List<EnemyAnimationFrameDefinition> { new EnemyAnimationFrameDefinition { Time = 0f } };
				EnemyAnimationDefinition animation = new EnemyAnimationDefinition();
				// The attack clip lands two hits; the first misses, and its missAnimation takes over from that moment.
				animation.Clips["attack"] = new EnemyAnimationClipDefinition
				{
					DurationSeconds = 1f, Frames = frames,
					Events = new List<EnemyAnimationEventDefinition>
					{
						new EnemyAnimationEventDefinition { Time = 0.1f, Type = "attackHit" },
						new EnemyAnimationEventDefinition { Time = 0.2f, Type = "attackHit" }
					}
				};
				animation.Clips["miss"] = new EnemyAnimationClipDefinition { DurationSeconds = 1f, Frames = frames };
				Set(controller, "m_animation", animation);
				Set(enemy, "m_modAnimation", controller);
				Set(enemy, "m_isAttacking", true);
				Set(enemy, "m_animationAttackCooldownStarted", true);
				Set(enemy, "m_animationAttackDefinition", new EnemyAttackDefinition { Id = "hit", Type = "melee", Animation = "attack", MissAnimation = "miss" });
				controllerType.GetMethod("Play", new[] { typeof(string), typeof(bool) }).Invoke(controller, new object[] { "attack", true });

				// Both hits are due in this update. The first starts the miss clip; the second belongs to the clip
				// that was replaced and must not run (it used to restart the miss clip from its own time).
				MethodInfo apply = controllerType.GetMethod("ApplyEvents", BindingFlags.Instance | BindingFlags.NonPublic);
				apply.Invoke(controller, new object[] { animation.Clips["attack"], 0.25f, enemy });

				Assert.That(Get(controller, "m_clipName"), Is.EqualTo("miss"));
				float clipTime = Time.time - (float)Get(controller, "m_started");
				Assert.That(clipTime, Is.EqualTo(0.1f).Within(0.02f), "The miss clip starts at the first hit's time, once.");
			}
			finally
			{
				Object.DestroyImmediate(host);
			}
		}

		private static void Set(object i_target, string i_field, object i_value)
		{
			for (System.Type type = i_target.GetType(); type != null; type = type.BaseType)
			{
				FieldInfo field = type.GetField(i_field, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly);
				if (field == null) continue;
				field.SetValue(i_target, i_value);
				return;
			}
			Assert.Fail("No field " + i_field + " on " + i_target.GetType().Name);
		}

		private static object Get(object i_target, string i_field)
		{
			for (System.Type type = i_target.GetType(); type != null; type = type.BaseType)
			{
				FieldInfo field = type.GetField(i_field, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly);
				if (field != null) return field.GetValue(i_target);
			}
			Assert.Fail("No field " + i_field + " on " + i_target.GetType().Name);
			return null;
		}
	}
}
