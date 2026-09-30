using System.Collections;
using System.Collections.Generic;
using CaptivityReloaded.Modding;
using UnityEngine;

public class Spawner : MonoBehaviour
{
	[SerializeField]
	private List<NPC> m_npcsPossibleToSpawn = new List<NPC>();

	private readonly List<float> m_modNpcSelectionWeights = new List<float>();
	private readonly List<string> m_requiredOpenDoors = new List<string>();
	private readonly List<string> m_requiredClosedDoors = new List<string>();

	[SerializeField]
	private float m_spawnChance01;

	[SerializeField]
	private int m_numWaveBeforeCanSpawn;

	[SerializeField]
	private float m_delayBetweenSpawns;

	[SerializeField]
	private float m_delayRandomOffsetDelaySpawn;

	[SerializeField]
	private float m_delayBeforeStartSpawning;

	[SerializeField]
	private float m_delayRandomOffsetBeforeStartSpawning;

	[SerializeField]
	private bool m_isSpawnOutOfSight;

	[SerializeField]
	private bool m_isEnabled;

	private float m_spawnAmount;

	private Coroutine m_coroutineSpawn;

	private Vector2 m_posSpawnFeet;

	public void StartSpawning()
	{
		if (m_npcsPossibleToSpawn.Count >= 1 && !(m_spawnAmount <= 0f))
		{
			if (m_coroutineSpawn == null)
			{
				m_coroutineSpawn = StartCoroutine(CoroutineSpawn());
			}
			SetSpawnFeetPos();
		}
	}

	private void SetSpawnFeetPos()
	{
		int mask = LayerMask.GetMask("Platform");
		RaycastHit2D raycastHit2D = Physics2D.Raycast(base.transform.position, Vector2.down, Mathf.Infinity, mask);
		if ((bool)raycastHit2D)
		{
			m_posSpawnFeet = raycastHit2D.point;
		}
		else
		{
			m_posSpawnFeet = base.transform.position;
		}
	}

	private IEnumerator CoroutineSpawn()
	{
		float initialDelay = ExternalRuleProfileFactory.ApplyInitialSpawnDelay(m_delayBeforeStartSpawning);
		float initialJitter = ExternalRuleProfileFactory.ApplyInitialSpawnDelay(m_delayRandomOffsetBeforeStartSpawning);
		float num = initialDelay + Random.Range(0f - initialJitter, initialJitter);
		if (num < 0f)
		{
			num = 0f;
		}
		yield return new WaitForSeconds(num);
		while (m_spawnAmount > 0f)
		{
			while (!IsDoorStateReady()) yield return new WaitForSeconds(0.25f);
			float delayJitter = ExternalRuleProfileFactory.ApplySpawnDelay(m_delayRandomOffsetDelaySpawn);
			float num2 = Random.Range(0f, delayJitter);
			float delayBetweenSpawns = ExternalRuleProfileFactory.ApplySpawnDelay(m_delayBetweenSpawns);
			delayBetweenSpawns = ((!(num2 < delayJitter / 2f)) ? (delayBetweenSpawns - num2) : (delayBetweenSpawns + num2));
			if (delayBetweenSpawns < 0.02f) delayBetweenSpawns = 0.02f;
			yield return new WaitForSeconds(delayBetweenSpawns);
			if (!IsDoorStateReady()) continue;
			if (CommonReferences.Instance.GetManagerStages().GetStageCurrent().GetManagerWave()
				.IsHasStageRoomForNpc())
			{
				if (m_isSpawnOutOfSight && IsPlayerTooCloseToSpawn())
				{
					CommonReferences.Instance.GetManagerStages().GetStageCurrent().GetManagerWave()
						.AddSingleNumToSpawn();
				}
				else
				{
					Spawn();
				}
				m_spawnAmount -= 1f;
			}
		}
		m_coroutineSpawn = null;
	}

	private void Spawn()
	{
		EnsureModSelectionWeights();
		float totalWeight = 0f;
		foreach (float weight in m_modNpcSelectionWeights) totalWeight += weight;
		float roll = Random.Range(0f, totalWeight);
		int index = 0;
		for (; index < m_modNpcSelectionWeights.Count - 1; index++)
		{
			roll -= m_modNpcSelectionWeights[index];
			if (roll <= 0f) break;
		}
		NPC nPC = Object.Instantiate(m_npcsPossibleToSpawn[index], CommonReferences.Instance.GetManagerStages().GetStageCurrent().GetActorsParent()
			.transform);
			nPC.transform.position = base.transform.position;
			nPC.gameObject.SetActive(value: true);
			nPC.Spawn(i_isFadeIn: true);
			if (nPC is Walker)
			{
				nPC.PlaceFeetOnPos(m_posSpawnFeet);
			}
		}

		private bool IsPlayerTooCloseToSpawn()
		{
			if (Vector2.Distance(CommonReferences.Instance.GetPlayer().GetPos(), base.transform.position) <= 20f)
			{
				return true;
			}
			return false;
		}

		public void AddSpawnOneAmount()
		{
			m_spawnAmount += 1f;
			if (CommonReferences.Instance.GetManagerStages().GetStageCurrent().GetManagerWave()
				.IsWaveActive())
			{
				StartSpawning();
			}
		}

		public bool IsEnabled()
		{
			return m_isEnabled;
		}

		public float GetSpawnChance01()
		{
			return ExternalRuleProfileFactory.ApplySpawnerChance(m_spawnChance01);
		}

		public bool IsCanSpawn()
		{
			if (m_npcsPossibleToSpawn.Count < 1)
			{
				return false;
			}
			if (!m_isEnabled)
			{
				return false;
			}
			if (!IsDoorStateReady()) return false;
			if (m_spawnAmount == 0f)
			{
				return false;
			}
			if (CommonReferences.Instance.GetManagerStages().GetStageCurrent().GetManagerWave()
				.GetNumWaveCurrent() < m_numWaveBeforeCanSpawn)
			{
				return false;
			}
			return true;
		}

		public bool IsAvailableForWaveAllocation()
		{
			if (!m_isEnabled || m_npcsPossibleToSpawn.Count == 0 || !IsDoorStateReady()) return false;
			ManagerWave wave = CommonReferences.Instance == null || CommonReferences.Instance.GetManagerStages() == null
				? null : CommonReferences.Instance.GetManagerStages().GetStageCurrent().GetManagerWave();
			return wave != null && wave.GetNumWaveCurrent() >= m_numWaveBeforeCanSpawn;
		}

		public bool HasPendingSpawns()
		{
			return m_spawnAmount > 0f;
		}

		public void Enable()
		{
			m_isEnabled = true;
		}

		public int DisableAndDrainPendingSpawns()
		{
			m_isEnabled = false;
			int pending = Mathf.CeilToInt(m_spawnAmount);
			m_spawnAmount = 0f;
			if (m_coroutineSpawn != null) StopCoroutine(m_coroutineSpawn);
			m_coroutineSpawn = null;
			return pending;
		}

		public void StopForRuleGoal()
		{
			m_spawnAmount = 0f;
			if (m_coroutineSpawn != null) StopCoroutine(m_coroutineSpawn);
			m_coroutineSpawn = null;
		}

		public bool AddNpcVariant(NPC i_template, NPC i_variant, float i_selectionWeight = 1f)
		{
			if (i_template == null || i_variant == null || m_npcsPossibleToSpawn.Contains(i_variant)) return false;
			EnsureModSelectionWeights();
			for (int index = 0; index < m_npcsPossibleToSpawn.Count; index++)
			{
				NPC candidate = m_npcsPossibleToSpawn[index];
				if (candidate != null && candidate.GetId() == i_template.GetId())
				{
					m_npcsPossibleToSpawn.Add(i_variant);
					m_modNpcSelectionWeights.Add(Mathf.Max(0.001f, i_selectionWeight));
					return true;
				}
			}
			return false;
		}

		public void ConfigureModSpawner(List<NPC> i_enemies, StageSpawnerDefinition i_definition)
		{
			m_npcsPossibleToSpawn = i_enemies ?? new List<NPC>();
			m_modNpcSelectionWeights.Clear();
			EnsureModSelectionWeights();
			m_spawnChance01 = i_definition.SelectionWeight;
			m_numWaveBeforeCanSpawn = i_definition.MinimumWave;
			m_delayBetweenSpawns = i_definition.DelaySeconds;
			m_delayRandomOffsetDelaySpawn = i_definition.DelayJitterSeconds;
			m_delayBeforeStartSpawning = i_definition.InitialDelaySeconds;
			m_delayRandomOffsetBeforeStartSpawning = i_definition.InitialDelayJitterSeconds;
			m_isSpawnOutOfSight = i_definition.SpawnOutOfSight;
			m_requiredOpenDoors.Clear();
			m_requiredOpenDoors.AddRange(i_definition.RequiredOpenDoors);
			m_requiredClosedDoors.Clear();
			m_requiredClosedDoors.AddRange(i_definition.RequiredClosedDoors);
			m_isEnabled = i_definition.Enabled;
		}

		public void CopyModRuntimeSettingsFrom(Spawner i_source)
		{
			m_modNpcSelectionWeights.Clear();
			m_modNpcSelectionWeights.AddRange(i_source.m_modNpcSelectionWeights);
			m_requiredOpenDoors.Clear();
			m_requiredOpenDoors.AddRange(i_source.m_requiredOpenDoors);
			m_requiredClosedDoors.Clear();
			m_requiredClosedDoors.AddRange(i_source.m_requiredClosedDoors);
		}

		public void AddRequiredOpenDoor(string i_doorName)
		{
			if (!string.IsNullOrEmpty(i_doorName) && !m_requiredOpenDoors.Contains(i_doorName))
				m_requiredOpenDoors.Add(i_doorName);
		}

		private bool IsDoorStateReady()
		{
			if (m_requiredOpenDoors.Count == 0 && m_requiredClosedDoors.Count == 0) return true;
			Stage stage = GetComponentInParent<Stage>();
			if (stage == null) return false;
			foreach (string id in m_requiredOpenDoors) if (!TryGetDoorState(stage, id, out bool open) || !open) return false;
			foreach (string id in m_requiredClosedDoors) if (!TryGetDoorState(stage, id, out bool open) || open) return false;
			return true;
		}

		private static bool TryGetDoorState(Stage i_stage, string i_id, out bool o_open)
		{
			string expected = "mod-door_" + i_id;
			foreach (Door door in i_stage.GetComponentsInChildren<Door>(true))
				if (HasRetainedObjectId(door, i_id)
					|| string.Equals(door.name, expected, System.StringComparison.OrdinalIgnoreCase)
					|| string.Equals(door.name, i_id, System.StringComparison.OrdinalIgnoreCase)
					|| NormalizeDoorId(door.name) == NormalizeDoorId(i_id)) { o_open = door.GetIsOpen(); return true; }
			foreach (DoorRoller door in i_stage.GetComponentsInChildren<DoorRoller>(true))
				if (HasRetainedObjectId(door, i_id)
					|| string.Equals(door.name, expected, System.StringComparison.OrdinalIgnoreCase)
					|| string.Equals(door.name, i_id, System.StringComparison.OrdinalIgnoreCase)
					|| NormalizeDoorId(door.name) == NormalizeDoorId(i_id)) { o_open = door.GetIsOpen(); return true; }
			o_open = false;
			return false;
		}

		private static bool HasRetainedObjectId(Component i_component, string i_id)
		{
			ModRetainedCoreStageObject retained = i_component == null ? null : i_component.GetComponent<ModRetainedCoreStageObject>();
			return retained != null && string.Equals(retained.ObjectId, i_id, System.StringComparison.Ordinal);
		}

		private static string NormalizeDoorId(string i_value)
		{
			System.Text.StringBuilder result = new System.Text.StringBuilder();
			bool dash = false;
			foreach (char character in (i_value ?? string.Empty).ToLowerInvariant())
				if (char.IsLetterOrDigit(character)) { result.Append(character); dash = false; }
				else if (!dash && result.Length > 0) { result.Append('-'); dash = true; }
			while (result.Length > 0 && result[result.Length - 1] == '-') result.Length--;
			return result.ToString();
		}

		private void EnsureModSelectionWeights()
		{
			while (m_modNpcSelectionWeights.Count < m_npcsPossibleToSpawn.Count) m_modNpcSelectionWeights.Add(1f);
			if (m_modNpcSelectionWeights.Count > m_npcsPossibleToSpawn.Count)
				m_modNpcSelectionWeights.RemoveRange(m_npcsPossibleToSpawn.Count,
					m_modNpcSelectionWeights.Count - m_npcsPossibleToSpawn.Count);
		}

		private void OnDrawGizmos()
		{
			if (m_isEnabled)
			{
				Gizmos.color = new Color(1f, 1f, 0f);
			}
			else
			{
				Gizmos.color = new Color(0.75f, 0.75f, 0.5f);
			}
			Gizmos.DrawSphere(new Vector3(base.transform.position.x, base.transform.position.y, 1f), 1f);
		}
	}
