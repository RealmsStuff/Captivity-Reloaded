using UnityEngine;

public class ManagerTransmogrifierHud : MonoBehaviour
{
	private TransmogrifierHud m_transmogrifierHud;

	private void Awake()
	{
		m_transmogrifierHud = GetComponentInChildren<TransmogrifierHud>(includeInactive: true);
		m_transmogrifierHud.gameObject.SetActive(value: false);
	}

	public void Show()
	{
		m_transmogrifierHud.Show();
	}

	public void Hide()
	{
		m_transmogrifierHud.Hide();
	}
}
