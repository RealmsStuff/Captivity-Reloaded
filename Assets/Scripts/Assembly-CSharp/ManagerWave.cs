using System.Collections;
using System.Collections.Generic;
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

	private Coroutine m_coroutineWaitForNextWave;

	public event DelWaveStart OnWaveStart;

	public event DelWaveEnd OnWaveEnd;

	public void Initialize(Stage i_stage)
	{
		int num = 0;
		m_stateWaveCurrent = StateWave.Wait;
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
			StartNewWave();
		}
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Z) && !m_isWave && m_isWaitingForNextWave)
		{
			StopCoroutine(m_coroutineWaitForNextWave);
			m_isWaitingForNextWave = false;
			StartNewWave();
		}
	}

	private IEnumerator CoroutineCheckIfWaveIsOver()
	{
		Player l_player = CommonReferences.Instance.GetPlayer();
		while (!l_player.IsDead())
		{
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
		if (m_numWaveCurrent != 1)
		{
			m_numOfSpawns += 2;
		}
		_ = m_numOfSpawns;
		_ = m_numMaxEnemies;
		m_numOfSpawnsToSpawn = m_numOfSpawns;
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
		CommonReferences.Instance.GetManagerHud().SetWaveNumber(m_numWaveCurrent, m_isHighscoreAchieved);
		CommonReferences.Instance.GetManagerHud().GetWaveHud().ShowWaveStart(m_numWaveCurrent);
		yield return new WaitForSeconds(5f);
		FillSpawners();
		StartSpawners();
		m_isWave = true;
		StartCoroutine(CoroutineCheckIfWaveIsOver());
	}

	private void DisableAllVendors()
	{
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
			if (m_spawners[i].IsEnabled())
			{
				float i_startIndex = num;
				num += m_spawners[i].GetSpawnChance01();
				float i_endIndex = num;
				list.Add(new SpawnerPickModel(m_spawners[i], i_startIndex, i_endIndex));
			}
		}
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
		m_stateWaveCurrent = StateWave.Wait;
		FillVendors();
		EnableAllVendors();
		this.OnWaveEnd?.Invoke();
		WaitForNextWave();
		CommonReferences.Instance.GetManagerHud().GetWaveHud().ShowWaveEnd(m_secsWaveEndWait);
		CommonReferences.Instance.GetManagerAudio().PlayAudioSFX(m_stage.GetAudioWinWave());
	}

	private void WaitForNextWave()
	{
		m_isWaitingForNextWave = true;
		m_coroutineWaitForNextWave = StartCoroutine(CoroutineWaitForNextWave());
	}

	private IEnumerator CoroutineWaitForNextWave()
	{
		yield return new WaitForSeconds(m_secsWaveEndWait);
		m_isWaitingForNextWave = false;
		StartNewWave();
	}

	private bool IsCurrentWaveOver()
	{
		foreach (NPC allNPC in CommonReferences.Instance.GetManagerStages().GetStageCurrent().GetAllNPCs())
		{
			if (allNPC.gameObject.activeSelf && !allNPC.IsDead() && !allNPC.IsIgnoreWave())
			{
				return false;
			}
		}
		foreach (Spawner spawner in m_spawners)
		{
			if (spawner.IsCanSpawn())
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
		if (m_stage.GetAllNPCs().Count >= m_numMaxEnemiesRoom)
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
}
