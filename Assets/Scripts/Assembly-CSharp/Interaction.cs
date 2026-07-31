using UnityEngine;

public class Interaction : MonoBehaviour
{
	[SerializeField]
	private int m_id;

	[SerializeField]
	private string m_name;

	[TextArea(10, 10)]
	[SerializeField]
	private string m_description;

	public void Trigger(NPC i_npc)
	{
		if (i_npc.GetId() != -1)
		{
			ManagerDB.Interaction(m_id, i_npc);
		}
	}

	public string GetName()
	{
		return m_name;
	}

	public string GetDescription()
	{
		return m_description;
	}

	public int GetId()
	{
		return m_id;
	}
}
