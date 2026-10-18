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
	// spawn.joinCoreSpawners adds an enemy to every spawner that offers a named Core enemy through
	// Spawner.AddNpcVariant: a weighted extra choice that waits for its minimum wave, also inside special waves
	// that keep the spawner's own roster. Spawner and the NPCs live in Assembly-CSharp, which this test assembly
	// cannot reference, so they are reached by reflection.
	public sealed class SpawnerJoinTests
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

		private Component Spawner(params Component[] i_offered)
		{
			GameObject item = new GameObject("spawner");
			m_objects.Add(item);
			Component spawner = item.AddComponent(SpawnerType);
			IList list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(RuntimeType("NPC")));
			foreach (Component npc in i_offered) list.Add(npc);
			SpawnerType.GetField("m_npcsPossibleToSpawn", Instance).SetValue(spawner, list);
			return spawner;
		}

		private static object Call(Component i_spawner, string i_method, params object[] i_arguments) => SpawnerType.GetMethod(i_method, Instance).Invoke(i_spawner, i_arguments);

		private static bool Join(Component i_spawner, Component i_template, Component i_variant, float i_weight, int i_minimumWave) =>
			(bool)Call(i_spawner, "AddNpcVariant", i_template, i_variant, i_weight, i_minimumWave);

		private static IEnumerable<T> Get<T>(Component i_spawner, string i_field) => ((IEnumerable)SpawnerType.GetField(i_field, Instance).GetValue(i_spawner)).Cast<T>();

		private static Array Npcs(params Component[] i_npcs)
		{
			Array array = Array.CreateInstance(RuntimeType("NPC"), i_npcs.Length);
			for (int index = 0; index < i_npcs.Length; index++) array.SetValue(i_npcs[index], index);
			return array;
		}

		[Test]
		public void AddNpcVariant_JoinsWithItsWeightAndMinimumWave()
		{
			Component abby = Npc("abby", 7), jenny = Npc("jenny", 8), original = Npc("original", -1);
			Component spawner = Spawner(abby, jenny);
			Assert.That(Join(spawner, abby, original, 0.5f, 4), Is.True);
			Assert.That(Get<Component>(spawner, "m_npcsPossibleToSpawn"), Is.EqualTo(new[] { abby, jenny, original }));
			Assert.That(Get<float>(spawner, "m_modNpcSelectionWeights"), Is.EqualTo(new[] { 1f, 1f, 0.5f }));
			Assert.That(Get<int>(spawner, "m_modNpcMinimumWaves"), Is.EqualTo(new[] { 0, 0, 4 }));
			// A spawner is joined once, and not at all where the Core enemy is not offered.
			Assert.That(Join(spawner, jenny, original, 1f, 0), Is.False);
			Assert.That(Join(Spawner(jenny), abby, original, 1f, 4), Is.False);
			// Inherited variants keep their old behaviour: no wave gate.
			Component variant = Npc("variant", -1);
			Assert.That(Join(spawner, jenny, variant, 1f, 0), Is.True);
			Assert.That(Get<int>(spawner, "m_modNpcMinimumWaves").Last(), Is.EqualTo(0));
		}

		[Test]
		public void SpecialWave_KeepsTheJoinedEntrysMinimumWave()
		{
			Component abby = Npc("abby", 7), original = Npc("original", -1), guest = Npc("guest", 9);
			Component spawner = Spawner(abby);
			Join(spawner, abby, original, 1f, 4);
			Call(spawner, "BeginSpecialWave", Npcs(guest), new[] { 2f }, true);
			Assert.That(Get<Component>(spawner, "m_waveOverrideNpcs"), Is.EqualTo(new[] { abby, original, guest }));
			Assert.That(Get<int>(spawner, "m_waveOverrideMinimumWaves"), Is.EqualTo(new[] { 0, 4, 0 }));
			Call(spawner, "ClearSpecialWave");
			Assert.That(Get<int>(spawner, "m_waveOverrideMinimumWaves"), Is.Empty);
			// A special wave that replaces the roster carries no gate of the spawner's own.
			Call(spawner, "BeginSpecialWave", Npcs(guest), new[] { 1f }, false);
			Assert.That(Get<int>(spawner, "m_waveOverrideMinimumWaves"), Is.EqualTo(new[] { 0 }));
		}
	}
}
