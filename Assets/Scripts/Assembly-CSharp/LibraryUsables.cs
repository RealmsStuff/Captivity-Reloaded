using System.Collections.Generic;
using UnityEngine;

public class LibraryUsables : MonoBehaviour
{
	private List<GameObject> m_usables = new List<GameObject>();

	private void Awake()
	{
		Usable[] componentsInChildren = GetComponentsInChildren<Usable>(includeInactive: true);
		foreach (Usable usable in componentsInChildren)
		{
			m_usables.Add(usable.gameObject);
		}
	}

	public List<GameObject> GetAllUsables()
	{
		return m_usables;
	}

	public GameObject GetRandomUsable()
	{
		int index = Random.Range(0, m_usables.Count);
		return m_usables[index];
	}
}
