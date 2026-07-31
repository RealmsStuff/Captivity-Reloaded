using UnityEngine;
using UnityEngine.UI;

public class StatusPlayerHudItem : MonoBehaviour
{
	[SerializeField]
	private Text m_txtNameStatus;

	[SerializeField]
	private Text m_txtDescriptionStatus;

	public void Initialise(string i_name, string i_description, Color i_color)
	{
		GetComponent<Image>().color = i_color;
		m_txtNameStatus.color = i_color;
		m_txtDescriptionStatus.color = new Color(m_txtDescriptionStatus.color.r, m_txtDescriptionStatus.color.g, m_txtDescriptionStatus.color.b, i_color.a);
		m_txtNameStatus.text = i_name;
		m_txtDescriptionStatus.text = i_description;
	}

	public string GetName()
	{
		return m_txtNameStatus.text;
	}

	public Color GetColor()
	{
		return m_txtNameStatus.color;
	}

	public void SetColor(Color i_color)
	{
		GetComponent<Image>().color = i_color;
		m_txtNameStatus.color = i_color;
		m_txtDescriptionStatus.color = new Color(m_txtDescriptionStatus.color.r, m_txtDescriptionStatus.color.g, m_txtDescriptionStatus.color.b, i_color.a);
	}
}
