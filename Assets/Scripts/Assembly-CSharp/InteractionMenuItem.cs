using System;
using UnityEngine;
using UnityEngine.UI;

public class InteractionMenuItem : MonoBehaviour
{
	[SerializeField]
	private Text m_txtName;

	[SerializeField]
	private Text m_txtDescription;

	[SerializeField]
	private Text m_txtTimesEncountered;

	private Interaction m_interaction;

	private NPC m_npc;

	public void Initialize(Interaction i_interaction, NPC i_npc)
	{
		m_interaction = i_interaction;
		m_npc = i_npc;
		m_txtName.text = i_interaction.GetName();
		m_txtDescription.text = i_interaction.GetDescription();
		m_txtTimesEncountered.text = GetTimesEncountered().ToString();
	}

	private int GetTimesEncountered()
	{
		object executeReader = ManagerDB.GetExecuteReader("SELECT timesInteraction FROM tbl_interactionRelationship WHERE idNpc = " + m_npc.GetId() + " AND idInteraction = " + m_interaction.GetId(), 0);
		if (executeReader == DBNull.Value)
		{
			return 0;
		}
		return Convert.ToInt32(executeReader);
	}
}
