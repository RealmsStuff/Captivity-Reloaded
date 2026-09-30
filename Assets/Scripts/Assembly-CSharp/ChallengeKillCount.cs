using System.Collections.Generic;
using UnityEngine;

public class ChallengeKillCount : Challenge
{
	[Header("Killcount")]
	[SerializeField]
	private List<NPC> m_npcsToKill = new List<NPC>();

	[SerializeField]
	private int m_timesKillToComplete;

	[SerializeField]
	private bool m_isTotalKillCount;

	private int m_killCount;

	protected override void HandleActivation()
	{
		m_killCount = 0;
		if (m_isTotalKillCount)
		{
			for (int i = 0; i < m_npcsToKill.Count; i++)
			{
				m_killCount += ManagerDB.GetRelationShip(m_npcsToKill[i]).TimesKilled;
			}
		}
		CommonReferences.Instance.GetPlayer().OnKill += OnPlayerKill;
	}

	protected override void HandleDeActivation()
	{
		CommonReferences.Instance.GetPlayer().OnKill -= OnPlayerKill;
	}

	protected override void TrackCompletion()
	{
		if (m_killCount >= m_timesKillToComplete)
		{
			Complete();
		}
	}

	private void OnPlayerKill(NPC i_npcKilled)
	{
		for (int i = 0; i < m_npcsToKill.Count; i++)
		{
			if (m_npcsToKill[i].GetId() == i_npcKilled.GetId())
			{
				m_killCount++;
				break;
			}
		}
	}

	public void ConfigureModKillCount(IEnumerable<NPC> i_npcs, int i_count)
	{
		m_npcsToKill = new List<NPC>();
		if (i_npcs != null) foreach (NPC npc in i_npcs) if (npc != null) m_npcsToKill.Add(npc);
		m_timesKillToComplete = i_count;
		m_isTotalKillCount = false;
	}
}
