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
}
