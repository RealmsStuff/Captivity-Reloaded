using System.Collections.Generic;
using UnityEngine;

public class ManagerHealthDisplay : MonoBehaviour
{
	[SerializeField]
	private HealthDisplayItem m_healthDisplayItemDefault;

	[SerializeField]
	private GameObject m_healthDisplayItemsParent;

	private List<HealthDisplayItem> m_healthDisplayItems = new List<HealthDisplayItem>();

	public void AddNPCHealthDisplay(NPC i_npc)
	{
		HealthDisplayItem healthDisplayItem = Object.Instantiate(m_healthDisplayItemDefault, m_healthDisplayItemDefault.transform.parent);
		healthDisplayItem.Initialize(i_npc);
		healthDisplayItem.gameObject.SetActive(value: true);
		m_healthDisplayItems.Add(healthDisplayItem);
	}

	public void ClearHealthDisplayItems()
	{
		foreach (HealthDisplayItem healthDisplayItem in m_healthDisplayItems)
		{
			if (!(healthDisplayItem == null) && healthDisplayItem.gameObject != null)
			{
				Object.Destroy(healthDisplayItem.gameObject);
			}
		}
		m_healthDisplayItems.Clear();
	}

	public void ShowNpcHealthDisplay(NPC i_npc)
	{
		foreach (HealthDisplayItem healthDisplayItem in m_healthDisplayItems)
		{
			if (healthDisplayItem.GetNpc() == i_npc)
			{
				healthDisplayItem.Show();
				break;
			}
		}
	}

	public void HideNpcHealthDisplay(NPC i_npc)
	{
		foreach (HealthDisplayItem healthDisplayItem in m_healthDisplayItems)
		{
			if (healthDisplayItem.GetNpc() == i_npc)
			{
				healthDisplayItem.Hide();
				break;
			}
		}
	}

	public void Show()
	{
		m_healthDisplayItemsParent.SetActive(value: true);
	}

	public void Hide()
	{
		m_healthDisplayItemsParent.SetActive(value: false);
	}
}
