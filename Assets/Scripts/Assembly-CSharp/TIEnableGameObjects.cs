using System.Collections.Generic;
using UnityEngine;

public class TIEnableGameObjects : TriggerInteractable
{
	[SerializeField]
	private List<GameObject> m_objectsToEnable = new List<GameObject>();

	protected override void HandleTrigger()
	{
		foreach (GameObject item in m_objectsToEnable)
		{
			item.SetActive(value: true);
		}
	}
}
