using UnityEngine;

public class ManagerMenus : MonoBehaviour
{
	[SerializeField]
	private Menu m_menuLocations;

	[SerializeField]
	private Menu m_menuShop;

	[SerializeField]
	private Menu m_menuDiary;

	private Menu m_menuCurrent;

	private bool m_isFirstMenu = true;

	private float m_nextMenuOpenTime;

	private void Start()
	{
		CloseAllMenus();
		m_menuCurrent = m_menuLocations;
		OpenMenu(m_menuCurrent);
	}

	public void OpenMenu(Menu i_menu)
	{
		if (i_menu == null || (m_menuCurrent == i_menu && Time.unscaledTime < m_nextMenuOpenTime))
		{
			return;
		}
		m_nextMenuOpenTime = Time.unscaledTime + 0.25f;
		CloseAllMenus();
		i_menu.Open();
		m_menuCurrent = i_menu;
	}

	private void CloseAllMenus()
	{
		m_menuLocations.Hide();
		m_menuShop.Hide();
		m_menuDiary.Hide();
	}
}
