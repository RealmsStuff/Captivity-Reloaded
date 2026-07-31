using System.Collections.Generic;
using UnityEngine;

public class TriggerEnableGameObjects : Trigger
{
	[SerializeField]
	private List<GameObject> m_objectsToEnable = new List<GameObject>();

	protected override void HandleTriggerEnter()
	{
		foreach (GameObject item in m_objectsToEnable)
		{
			item.SetActive(value: true);
		}
	}

	protected override void HandleTriggerExit()
	{
	}
}
