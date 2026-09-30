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

	public void AddRuntimeUsable(Usable i_usable)
	{
		if (i_usable == null || m_usables.Contains(i_usable.gameObject)) return;
		i_usable.gameObject.SetActive(false);
		m_usables.Add(i_usable.gameObject);
	}
}
