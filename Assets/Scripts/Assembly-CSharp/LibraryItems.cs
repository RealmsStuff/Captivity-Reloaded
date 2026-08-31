using UnityEngine;

public class LibraryItems : MonoBehaviour
{
	[SerializeField]
	private AmmoBox m_ammoBox;

	public AmmoBox GetAmmoBoxDupe()
	{
		return Object.Instantiate(m_ammoBox);
	}

	public AmmoBox GetAmmoBoxTemplate()
	{
		return m_ammoBox;
	}
}
