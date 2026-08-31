using System.Collections;
using System.Collections.Generic;
using CaptivityReloaded.Modding;
using UnityEngine;

public static class ExternalStageFactory
{
	public static void Schedule(ManagerStages i_manager)
	{
		if (i_manager == null || ModLoaderRuntime.StageDefinitions.Count == 0) return;
		ExternalStageFactoryHost host = i_manager.GetComponent<ExternalStageFactoryHost>();
		if (host == null) host = i_manager.gameObject.AddComponent<ExternalStageFactoryHost>();
		host.Begin(i_manager);
	}

	internal static void Build(ManagerStages i_manager)
	{
		int runtimeId = -2;
		foreach (StageDefinition definition in ModLoaderRuntime.StageDefinitions)
		{
			if (ModLoaderRuntime.Registry.TryGet(definition.Id, out ContentRegistration existing) && existing.RuntimeAsset != null) continue;
			if (!ModLoaderRuntime.Registry.TryGet(definition.Extends, out ContentRegistration baseEntry) || !(baseEntry.RuntimeAsset is Stage template))
			{
				Report("stage.factory-template", "Core stage template is not bound: " + definition.Extends, definition.Source);
				continue;
			}

			Stage clone = UnityEngine.Object.Instantiate(template, i_manager.transform);
			clone.gameObject.name = definition.Id.ToString();
			clone.gameObject.SetActive(false);
			RuntimeContentIdentity identity = clone.GetComponent<RuntimeContentIdentity>();
			if (identity == null) identity = clone.gameObject.AddComponent<RuntimeContentIdentity>();
			identity.Configure(definition.Id, ContentCategory.Stage);
			clone.ConfigureModStage(runtimeId--, definition.DisplayName, definition.Description,
				new Vector2(definition.Layout.PlayerSpawn.X, definition.Layout.PlayerSpawn.Y));

			foreach (Spawner inherited in clone.GetComponentsInChildren<Spawner>(true))
				inherited.gameObject.SetActive(false);

			bool valid = true;
			foreach (StageSpawnerDefinition spawnerDefinition in definition.Spawners)
			{
				List<NPC> enemies = new List<NPC>();
				foreach (ContentId enemyId in spawnerDefinition.Enemies)
				{
					if (!ModLoaderRuntime.Registry.TryGet(enemyId, out ContentRegistration enemyEntry) || !(enemyEntry.RuntimeAsset is NPC enemy))
					{
						Report("stage.factory-enemy", "Spawner enemy is not built: " + enemyId, definition.Source);
						valid = false;
						break;
					}
					enemies.Add(enemy);
				}
				if (!valid) break;
				GameObject spawnerObject = new GameObject("mod-spawner_" + spawnerDefinition.Id);
				spawnerObject.transform.SetParent(clone.transform);
				spawnerObject.transform.localPosition = new Vector2(spawnerDefinition.Position.X, spawnerDefinition.Position.Y);
				Spawner spawner = spawnerObject.AddComponent<Spawner>();
				spawner.ConfigureModSpawner(enemies, spawnerDefinition);
			}

			if (!valid)
			{
				UnityEngine.Object.Destroy(clone.gameObject);
				continue;
			}
			ManagerWave wave = clone.GetManagerWave();
			if (wave == null)
			{
				Report("stage.factory-wave", "Core stage template has no ManagerWave.", definition.Source);
				UnityEngine.Object.Destroy(clone.gameObject);
				continue;
			}
			wave.ConfigureModWave(definition.Waves.FirstWaveEnemyCount);
			i_manager.AddRuntimeStage(clone);
			ModLoaderRuntime.Registry.BindRuntimeAsset(definition.Id, clone, ModLoaderRuntime.LastReport);
			Debug.Log("[ModLoader] Built external stage " + definition.Id + " from " + definition.Extends + ".");
		}
	}

	private static void Report(string i_code, string i_message, string i_source)
	{
		ModLoaderRuntime.LastReport.Add(ValidationSeverity.Error, i_code, i_message, i_source);
		Debug.LogError("[ModLoader] " + i_code + ": " + i_message + " [" + i_source + "]");
	}
}

public sealed class ExternalStageFactoryHost : MonoBehaviour
{
	private bool m_started;

	public void Begin(ManagerStages i_manager)
	{
		if (m_started) return;
		m_started = true;
		StartCoroutine(BuildAfterEnemyFactories(i_manager));
	}

	private IEnumerator BuildAfterEnemyFactories(ManagerStages i_manager)
	{
		yield return null;
		yield return null;
		ExternalStageFactory.Build(i_manager);
	}
}
