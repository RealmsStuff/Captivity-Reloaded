using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CaptivityReloaded.Modding.Tests
{
	// spawn.joinCoreSpawners writes its weight and minimum wave into the spawners of the registered stages, but the
	// stage the player plays is a copy ManagerStages makes when it opens. Unity copies the serialized enemy list, not
	// those runtime-only settings, so RestoreRuntimeStageClone has to carry them over, for Core stages too: they are
	// the stages joinCoreSpawners exists for. Stage and Spawner live in Assembly-CSharp, reached by reflection.
	public sealed class CoreStageSpawnerJoinTests
	{
		private const BindingFlags Instance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
		private readonly List<GameObject> m_objects = new List<GameObject>();

		private static Type RuntimeType(string i_name)
		{
			Type type = AppDomain.CurrentDomain.GetAssemblies().Select(i_assembly => i_assembly.GetType(i_name, false)).FirstOrDefault(i_type => i_type != null);
			Assert.That(type, Is.Not.Null, "Runtime type was not loaded: " + i_name);
			return type;
		}

		private static Type SpawnerType => RuntimeType("Spawner");

		[TearDown]
		public void TearDown()
		{
			foreach (GameObject item in m_objects) if (item != null) Object.DestroyImmediate(item);
			m_objects.Clear();
		}

		private Component Npc(string i_name, int i_id)
		{
			GameObject item = new GameObject(i_name);
			m_objects.Add(item);
			Component npc = item.AddComponent(RuntimeType("Zombie"));
			npc.GetType().GetMethod("SetId", Instance).Invoke(npc, new object[] { i_id });
			return npc;
		}

		private static IEnumerable<T> Get<T>(Component i_spawner, string i_field) => ((IEnumerable)SpawnerType.GetField(i_field, Instance).GetValue(i_spawner)).Cast<T>();

		private Component Stage(string i_id, Component i_offered, out Component o_spawner)
		{
			GameObject stage = new GameObject(i_id);
			m_objects.Add(stage);
			Component component = stage.AddComponent(RuntimeType("Stage"));
			stage.AddComponent<RuntimeContentIdentity>().Configure(ContentId.Parse(i_id), ContentCategory.Stage);
			GameObject spawnerObject = new GameObject("spawner");
			spawnerObject.transform.SetParent(stage.transform, false);
			o_spawner = spawnerObject.AddComponent(SpawnerType);
			IList list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(RuntimeType("NPC")));
			list.Add(i_offered);
			SpawnerType.GetField("m_npcsPossibleToSpawn", Instance).SetValue(o_spawner, list);
			return component;
		}

		// What ManagerStages.CopyStage does when the player enters a stage.
		private Component PlayableCopy(Component i_stage)
		{
			GameObject clone = Object.Instantiate(i_stage.gameObject);
			m_objects.Add(clone);
			Component cloneStage = clone.GetComponent(RuntimeType("Stage"));
			RuntimeType("ExternalStageFactory").GetMethod("RestoreRuntimeStageClone").Invoke(null, new object[] { i_stage, cloneStage });
			return clone.GetComponentInChildren(SpawnerType, true);
		}

		[TestCase("core:stage/shack")]
		[TestCase("example.stages:stage/yard")]
		public void PlayableCopy_KeepsAJoinedEnemysMinimumWaveAndWeight(string i_stageId)
		{
			Component abby = Npc("abby", 7), joined = Npc("joined", -1);
			Component stage = Stage(i_stageId, abby, out Component spawner);
			Assert.That((bool)SpawnerType.GetMethod("AddNpcVariant", Instance).Invoke(spawner, new object[] { abby, joined, 0.2f, 5 }), Is.True);

			Component copy = PlayableCopy(stage);
			Assert.That(Get<Component>(copy, "m_npcsPossibleToSpawn"), Is.EqualTo(new[] { abby, joined }), "Unity copies the enemy list itself.");
			Assert.That(Get<int>(copy, "m_modNpcMinimumWaves"), Is.EqualTo(new[] { 0, 5 }),
				"The joined enemy must still wait for wave 5 on the stage the player plays, not show up from wave 1.");
			Assert.That(Get<float>(copy, "m_modNpcSelectionWeights"), Is.EqualTo(new[] { 1f, 0.2f }));
		}
	}
}
