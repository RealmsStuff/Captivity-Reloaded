using System.Collections;
using System.Collections.Generic;
using CaptivityReloaded.Modding;
using UnityEngine;

public class ManagerWave : MonoBehaviour
{
	public delegate void DelWaveStart();

	public delegate void DelWaveEnd();

	[SerializeField]
	private int m_numOfSpawnsFirstWave;

	private List<Spawner> m_spawners = new List<Spawner>();

	private int m_numOfSpawns;

	private int m_numOfSpawnsToSpawn;

	private int m_numWaveCurrent;

	private int m_numMaxEnemies = 40;

	private int m_numMaxEnemiesRoom = 25;

	private int m_secsWaveEndWait = 30;

	private bool m_isWave;

	private bool m_isWaitingForNextWave;

	private StateWave m_stateWaveCurrent;

	private Stage m_stage;

	private bool m_isHighscoreAchieved;
	private bool m_isRuleGoalComplete;

	private Coroutine m_coroutineWaitForNextWave;
	private IReadOnlyList<StageSpecialWaveDefinition> m_specialWaves = new StageSpecialWaveDefinition[0];
	private readonly Dictionary<string, int> m_lastSpecialWaveById = new Dictionary<string, int>();
	private readonly HashSet<string> m_signalArmedSpecialWaveIds = new HashSet<string>();
	private readonly List<NPC> m_specialWaveTemplates = new List<NPC>();
	private StageSpecialWaveDefinition m_activeSpecialWave;
	private string m_forcedSpecialWaveId = string.Empty;
	private string m_forcedSpecialWaveEnemyId = string.Empty;

	public event DelWaveStart OnWaveStart;

	public event DelWaveEnd OnWaveEnd;

	public void ConfigureCoreWave(int i_firstWaveEnemyCount, IReadOnlyList<StageSpecialWaveDefinition> i_specialWaves = null)
	{
		m_numOfSpawnsFirstWave = i_firstWaveEnemyCount;
		m_specialWaves = i_specialWaves ?? new StageSpecialWaveDefinition[0];
	}

	public void CopyRuntimeSettingsFrom(ManagerWave i_source)
	{
		if (i_source == null) return;
		m_specialWaves = i_source.m_specialWaves ?? new StageSpecialWaveDefinition[0];
	}

	public void Initialize(Stage i_stage)
	{
		int num = 0;
		m_spawners.Clear();
		m_stateWaveCurrent = StateWave.Wait;
		m_lastSpecialWaveById.Clear();
		m_signalArmedSpecialWaveIds.Clear();
		m_activeSpecialWave = null;
		m_forcedSpecialWaveId = string.Empty;
		m_forcedSpecialWaveEnemyId = string.Empty;
		Spawner[] componentsInChildren = GetComponentsInChildren<Spawner>(includeInactive: false);
		foreach (Spawner spawner in componentsInChildren)
		{
			m_spawners.Add(spawner);
			if (spawner.IsEnabled())
			{
				num++;
			}
		}
		if (m_spawners.Count != 0 && num != 0)
		{
			m_numWaveCurrent = 0;
			m_numOfSpawns = m_numOfSpawnsFirstWave;
			m_stage = i_stage;
			m_isHighscoreAchieved = false;
			m_isRuleGoalComplete = false;
			StartNewWave();
		}
	}

	private void Update()
	{
		bool isMobileWavePressed = CommonReferences.Instance != null 
			&& CommonReferences.Instance.GetPlayerController() != null 
			&& CommonReferences.Instance.GetPlayerController().GetIsMobileControlsEnabled() 
			&& CommonReferences.Instance.GetPlayerController().GetIsWavePressed();

		if ((CommonReferences.Instance.GetManagerInput().IsWaveSkipPressed() || isMobileWavePressed) && !m_isWave && m_isWaitingForNextWave)
		{
			SkipWaitingForNextWave();
		}
	}

	public void SkipWaitingForNextWave()
	{
		if (!m_isWave && m_isWaitingForNextWave)
		{
			if (m_coroutineWaitForNextWave != null)
			{
				StopCoroutine(m_coroutineWaitForNextWave);
			}
			m_isWaitingForNextWave = false;
			StartNewWave();
		}
	}

	private IEnumerator CoroutineCheckIfWaveIsOver()
	{
		Player l_player = CommonReferences.Instance.GetPlayer();
		while (!l_player.IsDead())
		{
			if (m_isWave && m_numOfSpawnsToSpawn > 0)
			{
				FillSpawners();
				StartSpawners();
			}
			if (m_isWave && IsCurrentWaveOver())
			{
				EndCurrentWave();
			}
			yield return new WaitForSeconds(1f);
		}
	}

	private void StartNewWave()
	{
		DisableAllVendors();
		m_stateWaveCurrent = StateWave.Wave;
		m_numWaveCurrent++;
		SelectSpecialWave();
		if (m_numWaveCurrent != 1)
		{
			m_numOfSpawns += ExternalRuleProfileFactory.GetSpawnGrowthPerWave(2);
		}
		int numOfSpawnsThisWave = ExternalRuleProfileFactory.ApplyWaveSpawnCount(m_numOfSpawns);
		if (m_activeSpecialWave != null)
			numOfSpawnsThisWave = Mathf.Max(1, Mathf.CeilToInt(numOfSpawnsThisWave * m_activeSpecialWave.EnemyCountMultiplier));
		_ = m_numOfSpawns;
		_ = m_numMaxEnemies;
		m_numOfSpawnsToSpawn = numOfSpawnsThisWave;
		this.OnWaveStart?.Invoke();
		if (m_numWaveCurrent > ManagerDB.GetHighscore(m_stage))
		{
			m_isHighscoreAchieved = true;
			ManagerDB.SetHighscore(m_stage, m_numWaveCurrent);
		}
		StartCoroutine(CoroutineStartWave());
	}

	private IEnumerator CoroutineStartWave()
	{
		if (m_numWaveCurrent == 1)
		{
			yield return new WaitForSeconds(0.5f);
			CommonReferences.Instance.GetManagerHud().GetWaveHud().ShowStageName(m_stage.GetName());
			CommonReferences.Instance.GetManagerAudio().PlayAudioMusic(m_stage.GetAudioEnterStage());
			yield return new WaitForSeconds(5f);
		}
		else
		{
			CommonReferences.Instance.GetManagerAudio().PlayAudioMusic(m_stage.GetAudioStartWave());
		}
		CommonReferences.Instance.GetManagerHud().SetIsShowWaveNumber(i_isShow: true);
		string specialWaveName = m_activeSpecialWave == null ? string.Empty : m_activeSpecialWave.DisplayName;
		CommonReferences.Instance.GetManagerHud().SetWaveNumber(m_numWaveCurrent, m_isHighscoreAchieved, specialWaveName);
		CommonReferences.Instance.GetManagerHud().GetWaveHud().ShowWaveStart(m_numWaveCurrent, specialWaveName);
		if (m_activeSpecialWave != null)
			CommonReferences.Instance.GetManagerHud().GetManagerNotification().CreateNotification(
				"SPECIAL WAVE: " + m_activeSpecialWave.DisplayName, ColorTextNotification.Other, i_isContinues: false);
		yield return new WaitForSeconds(5f);
		FillSpawners();
		StartSpawners();
		m_isWave = true;
		StartCoroutine(CoroutineCheckIfWaveIsOver());
	}

	private void DisableAllVendors()
	{
		if (ExternalRuleProfileFactory.ShouldKeepVendorsAvailable()) return;
		foreach (Vendor allVendor in CommonReferences.Instance.GetManagerStages().GetStageCurrent().GetAllVendors())
		{
			allVendor.Disable();
		}
	}

	private void EnableAllVendors()
	{
		foreach (Vendor allVendor in CommonReferences.Instance.GetManagerStages().GetStageCurrent().GetAllVendors())
		{
			allVendor.Enable();
		}
	}

	private void FillVendors()
	{
		foreach (Vendor allVendor in CommonReferences.Instance.GetManagerStages().GetStageCurrent().GetAllVendors())
		{
			allVendor.InsertItems();
		}
	}

	private void FillSpawners()
	{
		if (m_numOfSpawnsToSpawn <= 0)
		{
			return;
		}
		float num = 0f;
		List<SpawnerPickModel> list = new List<SpawnerPickModel>();
		for (int i = 0; i < m_spawners.Count; i++)
		{
			if (m_spawners[i].IsAvailableForWaveAllocation())
			{
				float i_startIndex = num;
				num += m_spawners[i].GetSpawnChance01();
				float i_endIndex = num;
				list.Add(new SpawnerPickModel(m_spawners[i], i_startIndex, i_endIndex));
			}
		}
		if (list.Count == 0) return;
		while (m_numOfSpawnsToSpawn > 0)
		{
			float num2 = Random.Range(0f, num);
			for (int j = 0; j < list.Count; j++)
			{
				if (num2 >= list[j].m_startIndex && num2 <= list[j].m_endIndex)
				{
					list[j].m_spawner.AddSpawnOneAmount();
					m_numOfSpawnsToSpawn--;
					break;
				}
			}
		}
	}

	public void AddSingleNumToSpawn()
	{
		m_numOfSpawnsToSpawn++;
		FillSpawners();
	}

	public void RequeueSpawns(int i_amount)
	{
		if (i_amount <= 0) return;
		m_numOfSpawnsToSpawn += i_amount;
		if (m_isWave) { FillSpawners(); StartSpawners(); }
	}

	private void StartSpawners()
	{
		foreach (Spawner spawner in m_spawners)
		{
			if (spawner.IsCanSpawn())
			{
				spawner.StartSpawning();
			}
		}
	}

	private void EndCurrentWave()
	{
		m_isWave = false;
		ClearSpecialWaveOverrides();
		CommonReferences.Instance.GetManagerHud().SetWaveNumber(m_numWaveCurrent, m_isHighscoreAchieved);
		m_stateWaveCurrent = StateWave.Wait;
		FillVendors();
		EnableAllVendors();
		this.OnWaveEnd?.Invoke();
		if (m_isRuleGoalComplete) return;
		int maximumWaves = ExternalRuleProfileFactory.GetMaximumWaves();
		if (maximumWaves > 0 && m_numWaveCurrent >= maximumWaves)
		{
			m_isWaitingForNextWave = false;
			CommonReferences.Instance.GetManagerHud().GetWaveHud().ShowWaveEnd(0);
			CommonReferences.Instance.GetManagerHud().GetManagerNotification().CreateNotification(
				ExternalRuleProfileFactory.ActiveModeName + " complete!", ColorTextNotification.Other, i_isContinues: false);
			CommonReferences.Instance.GetManagerAudio().PlayAudioSFX(m_stage.GetAudioWinWave());
			return;
		}
		WaitForNextWave();
		int intermissionSeconds = ExternalRuleProfileFactory.GetIntermissionSeconds(m_secsWaveEndWait);
		CommonReferences.Instance.GetManagerHud().GetWaveHud().ShowWaveEnd(intermissionSeconds);
		CommonReferences.Instance.GetManagerAudio().PlayAudioSFX(m_stage.GetAudioWinWave());
	}

	private void WaitForNextWave()
	{
		m_isWaitingForNextWave = true;
		m_coroutineWaitForNextWave = StartCoroutine(CoroutineWaitForNextWave());
	}

	private IEnumerator CoroutineWaitForNextWave()
	{
		yield return new WaitForSeconds(ExternalRuleProfileFactory.GetIntermissionSeconds(m_secsWaveEndWait));
		m_isWaitingForNextWave = false;
		StartNewWave();
	}

	private bool IsCurrentWaveOver()
	{
		if (m_numOfSpawnsToSpawn > 0) return false;
		foreach (NPC allNPC in CommonReferences.Instance.GetManagerStages().GetStageCurrent().GetAllNPCs())
		{
			if (allNPC.gameObject.activeSelf && !allNPC.IsDead() && !allNPC.IsIgnoreWave())
			{
				return false;
			}
		}
		foreach (Spawner spawner in m_spawners)
		{
			if (spawner.HasPendingSpawns())
			{
				return false;
			}
		}
		return true;
	}

	public int GetNumWaveCurrent()
	{
		return m_numWaveCurrent;
	}

	public bool IsWaveActive()
	{
		return m_isWave;
	}

	public bool IsHasStageRoomForNpc()
	{
		int maxEnemies = ExternalRuleProfileFactory.ApplyMaxStageEnemies(m_numMaxEnemies);
		int maxEnemiesRoom = ExternalRuleProfileFactory.ApplyMaxRoomEnemies(m_numMaxEnemiesRoom);
		if (m_stage.GetAllNPCs().Count >= maxEnemies || m_stage.GetAllNPCs().Count >= maxEnemiesRoom)
		{
			return false;
		}
		return true;
	}

	public StateWave GetStateWave()
	{
		return m_stateWaveCurrent;
	}

	public bool IsHighscoreAchieved()
	{
		return m_isHighscoreAchieved;
	}

	public void CompleteRuleGoal()
	{
		if (m_isRuleGoalComplete) return;
		m_isRuleGoalComplete = true;
		m_isWave = false;
		m_isWaitingForNextWave = false;
		if (m_coroutineWaitForNextWave != null) StopCoroutine(m_coroutineWaitForNextWave);
		m_coroutineWaitForNextWave = null;
		StopAllCoroutines();
		foreach (Spawner spawner in m_spawners) if (spawner != null) { spawner.StopForRuleGoal(); spawner.ClearSpecialWave(); }
		m_activeSpecialWave = null;
		CommonReferences.Instance.GetManagerHud().SetWaveNumber(m_numWaveCurrent, m_isHighscoreAchieved);
		CommonReferences.Instance.GetManagerHud().GetWaveHud().ShowWaveEnd(0);
	}

	public void ConfigureModWave(int i_firstWaveEnemyCount, IReadOnlyList<StageSpecialWaveDefinition> i_specialWaves = null)
	{
		m_numOfSpawnsFirstWave = i_firstWaveEnemyCount;
		m_specialWaves = i_specialWaves ?? new StageSpecialWaveDefinition[0];
	}

	public string GetActiveSpecialWaveId()
	{
		return m_activeSpecialWave == null ? string.Empty : m_activeSpecialWave.Id;
	}

	public string GetQueuedSpecialWaveId()
	{
		return m_forcedSpecialWaveId;
	}

	public string GetQueuedSpecialWaveEnemyId()
	{
		return m_forcedSpecialWaveEnemyId;
	}

	public bool GetIsWaitingForNextWave()
	{
		return m_isWaitingForNextWave;
	}

	public IReadOnlyList<StageSpecialWaveDefinition> GetSpecialWaves()
	{
		return m_specialWaves;
	}

	public bool NotifyStageSignal(string i_signal)
	{
		if (string.IsNullOrEmpty(i_signal)) return false;
		bool armed = false;
		foreach (StageSpecialWaveDefinition specialWave in m_specialWaves)
		{
			if (specialWave == null || specialWave.TriggerSignal != i_signal) continue;
			m_signalArmedSpecialWaveIds.Add(specialWave.Id);
			Debug.Log("[SpecialWave] Signal '" + i_signal + "' armed '" + specialWave.Id + "'.");
			armed = true;
		}
		return armed;
	}

	public bool IsSpecialWaveArmed(string i_id)
	{
		return !string.IsNullOrEmpty(i_id) && m_signalArmedSpecialWaveIds.Contains(i_id);
	}

	public bool QueueSpecialWave(string i_id)
	{
		if (string.IsNullOrEmpty(i_id)) return false;
		foreach (StageSpecialWaveDefinition specialWave in m_specialWaves)
		{
			if (specialWave == null || specialWave.Id != i_id) continue;
			m_forcedSpecialWaveId = i_id;
			m_forcedSpecialWaveEnemyId = string.Empty;
			Debug.Log("[SpecialWave] Queued '" + i_id + "' for the next wave.");
			return true;
		}
		return false;
	}

	public bool QueueSpecialWaveEnemy(string i_waveId, ContentId i_enemyId)
	{
		if (string.IsNullOrEmpty(i_waveId)) return false;
		foreach (StageSpecialWaveDefinition specialWave in m_specialWaves)
		{
			if (specialWave == null || specialWave.Id != i_waveId) continue;
			foreach (StageSpecialWaveEnemyDefinition enemy in specialWave.Enemies)
			{
				if (enemy == null || enemy.Enemy != i_enemyId) continue;
				m_forcedSpecialWaveId = i_waveId;
				m_forcedSpecialWaveEnemyId = i_enemyId.ToString();
				Debug.Log("[SpecialWave] Queued '" + i_waveId + "' with enemy '" + i_enemyId + "' for the next wave.");
				return true;
			}
			return false;
		}
		return false;
	}

	private void SelectSpecialWave()
	{
		ClearSpecialWaveOverrides();
		string forcedEnemyId = m_forcedSpecialWaveEnemyId;
		List<StageSpecialWaveDefinition> candidates = new List<StageSpecialWaveDefinition>();
		if (!string.IsNullOrEmpty(m_forcedSpecialWaveId))
		{
			foreach (StageSpecialWaveDefinition specialWave in m_specialWaves)
				if (specialWave != null && specialWave.Id == m_forcedSpecialWaveId)
				{
					candidates.Add(specialWave);
					break;
				}
			m_forcedSpecialWaveId = string.Empty;
			m_forcedSpecialWaveEnemyId = string.Empty;
		}
		else foreach (StageSpecialWaveDefinition specialWave in m_specialWaves)
		{
			if (specialWave == null || m_numWaveCurrent < specialWave.MinimumWave) continue;
			if (m_lastSpecialWaveById.TryGetValue(specialWave.Id, out int previousWave)
				&& m_numWaveCurrent - previousWave <= specialWave.CooldownWaves) continue;
			if (!string.IsNullOrEmpty(specialWave.TriggerSignal))
			{
				if (!m_signalArmedSpecialWaveIds.Contains(specialWave.Id)) continue;
				m_signalArmedSpecialWaveIds.Remove(specialWave.Id);
			}
			else if (specialWave.EveryNthWave > 0 && m_numWaveCurrent % specialWave.EveryNthWave != 0) continue;
			if (Random.value <= specialWave.Chance) candidates.Add(specialWave);
		}
		if (candidates.Count == 0) return;
		m_activeSpecialWave = PickSpecialWave(candidates);
		if (!TryResolveSpecialWaveEnemies(m_activeSpecialWave, out List<NPC> enemies, out List<float> weights,
			out List<ContentId> enemyIds))
		{
			Debug.LogWarning("[SpecialWave] Skipping '" + m_activeSpecialWave.Id + "' because none of its enemy content is available.");
			m_activeSpecialWave = null;
			return;
		}
		if (m_activeSpecialWave.SelectionMode == "singleRandom")
		{
			int selected = -1;
			if (!string.IsNullOrEmpty(forcedEnemyId))
				for (int index = 0; index < enemyIds.Count; index++)
					if (enemyIds[index].ToString() == forcedEnemyId) { selected = index; break; }
			if (selected < 0) selected = PickWeightedIndex(weights);
			NPC enemy = enemies[selected];
			float weight = weights[selected];
			enemies.Clear(); enemies.Add(enemy);
			weights.Clear(); weights.Add(weight);
		}
		bool includeOriginal = m_activeSpecialWave.SelectionMode == "add";
		foreach (Spawner spawner in m_spawners)
			if (spawner != null) spawner.BeginSpecialWave(enemies, weights, includeOriginal);
		m_lastSpecialWaveById[m_activeSpecialWave.Id] = m_numWaveCurrent;
		Debug.Log("[SpecialWave] Started '" + m_activeSpecialWave.Id + "' on wave " + m_numWaveCurrent
			+ " with " + enemies.Count + " resolved enemy choice(s).");
	}

	private static StageSpecialWaveDefinition PickSpecialWave(List<StageSpecialWaveDefinition> i_candidates)
	{
		float total = 0f;
		foreach (StageSpecialWaveDefinition candidate in i_candidates) total += candidate.Chance;
		float roll = Random.Range(0f, total);
		foreach (StageSpecialWaveDefinition candidate in i_candidates)
		{
			roll -= candidate.Chance;
			if (roll <= 0f) return candidate;
		}
		return i_candidates[i_candidates.Count - 1];
	}

	private bool TryResolveSpecialWaveEnemies(StageSpecialWaveDefinition i_specialWave,
		out List<NPC> o_enemies, out List<float> o_weights, out List<ContentId> o_enemyIds)
	{
		o_enemies = new List<NPC>();
		o_weights = new List<float>();
		o_enemyIds = new List<ContentId>();
		foreach (StageSpecialWaveEnemyDefinition entry in i_specialWave.Enemies)
		{
			if (!TryResolveEnemy(entry.Enemy, out NPC npc))
			{
				Debug.LogWarning("[SpecialWave] Enemy is unavailable: " + entry.Enemy);
				continue;
			}
			NPC resolvedNpc = npc;
			if (entry.Appearance.HasValue)
			{
				if (!TryResolveEnemy(entry.Appearance.Value, out NPC appearanceNpc))
				{
					Debug.LogWarning("[SpecialWave] Appearance is unavailable: " + entry.Appearance.Value);
					continue;
				}
				resolvedNpc = CreateAppearanceVariant(npc, appearanceNpc);
				if (resolvedNpc == null) continue;
				m_specialWaveTemplates.Add(resolvedNpc);
			}
			o_enemies.Add(resolvedNpc);
			o_weights.Add(entry.Weight);
			o_enemyIds.Add(entry.Enemy);
		}
		return o_enemies.Count > 0;
	}

	private static bool TryResolveEnemy(ContentId i_id, out NPC o_npc)
	{
		o_npc = null;
		if (ModLoaderRuntime.Registry.TryGet(i_id, out ContentRegistration registration)
			&& registration.RuntimeAsset is NPC registeredNpc)
		{
			o_npc = registeredNpc;
			return true;
		}
		if (i_id.Namespace != "core") return false;

		CoreContentCatalogEntry catalogEntry = null;
		foreach (CoreContentCatalogEntry entry in ModLoaderRuntime.CoreContentCatalog)
			if (entry.Id == i_id && entry.Category == ContentCategory.Enemy) { catalogEntry = entry; break; }
		if (catalogEntry != null)
		{
			foreach (Library library in Resources.FindObjectsOfTypeAll<Library>())
			{
				if (library == null || library.Actors == null) continue;
				NPC candidate = catalogEntry.LegacyId.HasValue
					? library.Actors.GetNpc(catalogEntry.LegacyId.Value)
					: library.Actors.GetNpc(catalogEntry.LegacyName);
				if (candidate == null) continue;
				o_npc = candidate;
				RepairRuntimeBinding(i_id, candidate);
				return true;
			}
		}

		foreach (NPC candidate in Resources.FindObjectsOfTypeAll<NPC>())
		{
			if (candidate == null || candidate.gameObject.activeInHierarchy) continue;
			RuntimeContentIdentity identity = candidate.GetComponent<RuntimeContentIdentity>();
			if (identity != null && identity.TryGetContentId(out ContentId candidateId) && candidateId == i_id)
			{
				o_npc = candidate;
				RepairRuntimeBinding(i_id, candidate);
				return true;
			}
		}
		return false;
	}

	private static void RepairRuntimeBinding(ContentId i_id, NPC i_npc)
	{
		if (ModLoaderRuntime.Registry.TryGet(i_id, out _))
			ModLoaderRuntime.Registry.BindRuntimeAsset(i_id, i_npc, ModLoaderRuntime.LastReport);
		Debug.Log("[SpecialWave] Recovered Core enemy runtime binding: " + i_id);
	}

	private static NPC CreateAppearanceVariant(NPC i_behavior, NPC i_appearance)
	{
		NPC variant = Object.Instantiate(i_behavior);
		variant.gameObject.SetActive(false);
		variant.name = i_behavior.name + " [" + i_appearance.GetName() + " appearance]";
		SkeletonActor targetSkeleton = variant.GetSkeletonActor()
			?? variant.GetComponentInChildren<SkeletonActor>(true);
		SkeletonActor appearanceSkeleton = i_appearance.GetSkeletonActor()
			?? i_appearance.GetComponentInChildren<SkeletonActor>(true);
		if (targetSkeleton == null || appearanceSkeleton == null)
		{
			Object.Destroy(variant.gameObject);
			Debug.LogWarning("[SpecialWave] Cannot apply appearance because one enemy has no SkeletonActor.");
			return null;
		}

		Dictionary<string, Bone> appearanceBones = new Dictionary<string, Bone>(System.StringComparer.Ordinal);
		foreach (Bone appearanceBone in appearanceSkeleton.GetComponentsInChildren<Bone>(true))
			if (appearanceBone != null && !appearanceBones.ContainsKey(appearanceBone.name))
				appearanceBones.Add(appearanceBone.name, appearanceBone);
		int matchedSprites = 0;
		foreach (Bone targetBone in targetSkeleton.GetComponentsInChildren<Bone>(true))
		{
			if (targetBone == null) continue;
			if (!appearanceBones.TryGetValue(targetBone.name, out Bone appearanceBone)) continue;
			matchedSprites += CopyBoneAppearance(targetBone, appearanceBone);
		}
		if (matchedSprites == 0)
		{
			Object.Destroy(variant.gameObject);
			Debug.LogWarning("[SpecialWave] Appearance rig is incompatible with " + i_behavior.GetName()
				+ ": no named bone renderers matched.");
			return null;
		}
		Debug.Log("[SpecialWave] Applied " + i_appearance.GetName() + " appearance to "
			+ i_behavior.GetName() + " across " + matchedSprites + " sprite renderer(s).");
		return variant;
	}

	private static int CopyBoneAppearance(Bone i_targetBone, Bone i_appearanceBone)
	{
		List<SpriteRenderer> targets = GetBoneRenderers(i_targetBone);
		List<SpriteRenderer> appearances = GetBoneRenderers(i_appearanceBone);
		Dictionary<string, Queue<SpriteRenderer>> byPath = new Dictionary<string, Queue<SpriteRenderer>>();
		foreach (SpriteRenderer renderer in appearances)
		{
			string path = GetRelativePath(i_appearanceBone.transform, renderer.transform);
			if (!byPath.TryGetValue(path, out Queue<SpriteRenderer> queue))
			{
				queue = new Queue<SpriteRenderer>();
				byPath[path] = queue;
			}
			queue.Enqueue(renderer);
		}

		HashSet<SpriteRenderer> used = new HashSet<SpriteRenderer>();
		int matched = 0;
		foreach (SpriteRenderer target in targets)
		{
			string path = GetRelativePath(i_targetBone.transform, target.transform);
			SpriteRenderer appearance = null;
			if (byPath.TryGetValue(path, out Queue<SpriteRenderer> exact) && exact.Count > 0)
				appearance = exact.Dequeue();
			if (appearance == null)
				foreach (SpriteRenderer candidate in appearances)
					if (!used.Contains(candidate)) { appearance = candidate; break; }
			if (appearance == null) { target.enabled = false; continue; }
			used.Add(appearance);
			target.sprite = appearance.sprite;
			target.enabled = appearance.enabled && appearance.sprite != null;
			target.flipX = appearance.flipX;
			target.flipY = appearance.flipY;
			target.drawMode = appearance.drawMode;
			target.size = appearance.size;
			target.transform.localPosition = appearance.transform.localPosition;
			target.transform.localRotation = appearance.transform.localRotation;
			target.transform.localScale = appearance.transform.localScale;
			matched++;
		}
		return matched;
	}

	private static List<SpriteRenderer> GetBoneRenderers(Bone i_bone)
	{
		List<SpriteRenderer> renderers = new List<SpriteRenderer>();
		foreach (SpriteRenderer renderer in i_bone.GetComponentsInChildren<SpriteRenderer>(true))
			if (renderer.GetComponentInParent<Bone>(true) == i_bone) renderers.Add(renderer);
		return renderers;
	}

	private static string GetRelativePath(Transform i_root, Transform i_target)
	{
		if (i_target == i_root) return string.Empty;
		List<string> parts = new List<string>();
		Transform current = i_target;
		while (current != null && current != i_root)
		{
			parts.Add(current.name);
			current = current.parent;
		}
		parts.Reverse();
		return string.Join("/", parts);
	}

	private static int PickWeightedIndex(IReadOnlyList<float> i_weights)
	{
		float total = 0f;
		foreach (float weight in i_weights) total += weight;
		float roll = Random.Range(0f, total);
		for (int index = 0; index < i_weights.Count - 1; index++)
		{
			roll -= i_weights[index];
			if (roll <= 0f) return index;
		}
		return i_weights.Count - 1;
	}

	private void ClearSpecialWaveOverrides()
	{
		foreach (Spawner spawner in m_spawners) if (spawner != null) spawner.ClearSpecialWave();
		foreach (NPC template in m_specialWaveTemplates)
			if (template != null) Object.Destroy(template.gameObject);
		m_specialWaveTemplates.Clear();
		m_activeSpecialWave = null;
	}
}
