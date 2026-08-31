using UnityEngine;

public abstract class Item : MonoBehaviour
{
	[SerializeField]
	protected string m_name;

	[Multiline]
	[SerializeField]
	protected string m_description;

	[SerializeField]
	protected Sprite m_sprItem;

	public string GetName()
	{
		return m_name;
	}

	public string GetDescription()
	{
		return m_description;
	}

	public void ConfigureModItem(string i_name, string i_description)
	{
		m_name = i_name;
		m_description = i_description ?? string.Empty;
	}

	public void SetModItemIcon(Sprite i_icon)
	{
		m_sprItem = i_icon;
	}
}
