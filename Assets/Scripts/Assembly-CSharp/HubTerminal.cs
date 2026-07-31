using UnityEngine;

public class HubTerminal : MonoBehaviour
{
	[SerializeField]
	private GameObject m_hubTerminal;

	public void Open()
	{
		m_hubTerminal.SetActive(value: true);
	}

	public void Close()
	{
		m_hubTerminal.SetActive(value: false);
	}
}
