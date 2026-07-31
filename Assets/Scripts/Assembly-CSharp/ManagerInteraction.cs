using System.Collections.Generic;
using UnityEngine;

public class ManagerInteraction : MonoBehaviour
{
	private List<Interaction> m_interactions = new List<Interaction>();

	private void Awake()
	{
		Interaction[] componentsInChildren = GetComponentsInChildren<Interaction>();
		foreach (Interaction item in componentsInChildren)
		{
			m_interactions.Add(item);
		}
	}

	public void TriggerInteraction(Interaction i_interaction, NPC i_npc)
	{
		TriggerInteraction(i_interaction.GetId(), i_npc);
	}

	public void TriggerInteraction(int i_idInteraction, NPC i_npc)
	{
		for (int i = 0; i < m_interactions.Count; i++)
		{
			if (m_interactions[i].GetId() == i_idInteraction)
			{
				m_interactions[i].Trigger(i_npc);
				break;
			}
		}
	}

	public Interaction GetInteraction(int i_idInteraction)
	{
		for (int i = 0; i < m_interactions.Count; i++)
		{
			if (m_interactions[i].GetId() == i_idInteraction)
			{
				return m_interactions[i];
			}
		}
		return null;
	}
}
