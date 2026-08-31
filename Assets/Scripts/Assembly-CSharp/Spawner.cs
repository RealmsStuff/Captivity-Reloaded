using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
	[SerializeField]
	private List<NPC> m_npcsPossibleToSpawn = new List<NPC>();

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
		float num = (m_delayBeforeStartSpawning = Random.Range(0f - m_delayRandomOffsetBeforeStartSpawning, m_delayRandomOffsetBeforeStartSpawning));
		if (num < 0f)
		{
			num = 0f;
		}
		yield return new WaitForSeconds(num);
		while (m_spawnAmount > 0f)
		{
			float num2 = Random.Range(0f, m_delayRandomOffsetDelaySpawn);
			float delayBetweenSpawns = m_delayBetweenSpawns;
			delayBetweenSpawns = ((!(num2 < m_delayRandomOffsetDelaySpawn / 2f)) ? (delayBetweenSpawns - num2) : (delayBetweenSpawns + num2));
			yield return new WaitForSeconds(delayBetweenSpawns);
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
		int index = Random.Range(0, m_npcsPossibleToSpawn.Count);
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
			return m_spawnChance01;
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

		public void Enable()
		{
			m_isEnabled = true;
		}

		public bool AddNpcVariant(NPC i_template, NPC i_variant)
		{
			if (i_template == null || i_variant == null || m_npcsPossibleToSpawn.Contains(i_variant)) return false;
			for (int index = 0; index < m_npcsPossibleToSpawn.Count; index++)
			{
				NPC candidate = m_npcsPossibleToSpawn[index];
				if (candidate != null && candidate.GetId() == i_template.GetId())
				{
					m_npcsPossibleToSpawn.Add(i_variant);
					return true;
				}
			}
			return false;
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
