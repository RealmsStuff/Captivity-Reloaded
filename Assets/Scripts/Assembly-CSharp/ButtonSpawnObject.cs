using System.Collections.Generic;
using UnityEngine;

public class ButtonSpawnObject : Button
{
	[SerializeField]
	private List<GameObject> m_objectsToSpawn = new List<GameObject>();

	protected override void HandlePressButton()
	{
		foreach (GameObject item in m_objectsToSpawn)
		{
			if (!(item == null))
			{
				Object.Instantiate(item, item.transform.parent).SetActive(value: true);
			}
		}
	}
}
