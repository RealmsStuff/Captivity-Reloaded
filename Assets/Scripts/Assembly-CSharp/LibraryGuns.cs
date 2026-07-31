using System.Collections.Generic;
using UnityEngine;

public class LibraryGuns : MonoBehaviour
{
	private List<Gun> m_guns = new List<Gun>();

	private void Awake()
	{
		Gun[] componentsInChildren = GetComponentsInChildren<Gun>(includeInactive: true);
		foreach (Gun item in componentsInChildren)
		{
			m_guns.Add(item);
		}
	}

	public List<Gun> GetAllGuns()
	{
		return m_guns;
	}

	public Gun GetRandomGun()
	{
		int index = Random.Range(0, m_guns.Count);
		return m_guns[index];
	}

	public Gun GetGun(string i_nameGun)
	{
		foreach (Gun gun in m_guns)
		{
			if (gun.GetName() == i_nameGun)
			{
				return gun;
			}
		}
		return null;
	}
}
