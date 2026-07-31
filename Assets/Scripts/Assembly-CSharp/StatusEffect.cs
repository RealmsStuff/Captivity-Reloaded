using System.Collections.Generic;

public abstract class StatusEffect
{
	protected string m_name;

	protected string m_source;

	protected TypeStatusEffect m_type;

	protected float m_duration;

	protected bool m_isActive;

	protected bool m_isStackable;

	protected List<StatusPlayerHudItem> m_statusPlayerHudItems = new List<StatusPlayerHudItem>();

	protected List<StatusPlayerHudItem> m_statusPlayerHudItemsAdded = new List<StatusPlayerHudItem>();

	protected Actor m_actor;

	public StatusEffect(string i_name, string i_source, TypeStatusEffect i_type, float i_duration, bool i_isStackable)
	{
		m_name = i_name;
		m_source = i_source;
		m_type = i_type;
		m_duration = i_duration;
		m_isStackable = i_isStackable;
	}

	public virtual void Activate()
	{
		m_isActive = true;
		StatusPlayerHud statusPlayerHud = CommonReferences.Instance.GetManagerHud().GetStatusPlayerHud();
		foreach (StatusPlayerHudItem statusPlayerHudItem in m_statusPlayerHudItems)
		{
			statusPlayerHud.AddStatus(statusPlayerHudItem);
			m_statusPlayerHudItemsAdded.Add(statusPlayerHudItem);
		}
	}

	public void SetActor(Actor i_actor)
	{
		m_actor = i_actor;
	}

	public virtual void End()
	{
		m_isActive = false;
		StatusPlayerHud statusPlayerHud = CommonReferences.Instance.GetManagerHud().GetStatusPlayerHud();
		foreach (StatusPlayerHudItem item in m_statusPlayerHudItemsAdded)
		{
			statusPlayerHud.DestroyStatusItem(item);
		}
	}

	public void AddPlayerStatusHudItem(string i_nameStatus, string i_descriptionStatus, StatusPlayerHudItemColor i_color)
	{
		m_statusPlayerHudItems.Add(CommonReferences.Instance.GetManagerHud().GetStatusPlayerHud().CreateStatus(i_nameStatus, i_descriptionStatus, i_color));
	}

	public List<StatusPlayerHudItem> GetStatusPlayerHudItems()
	{
		return m_statusPlayerHudItems;
	}

	public string GetName()
	{
		return m_name;
	}

	public void SetName(string i_name)
	{
		m_name = i_name;
	}

	public string GetSource()
	{
		return m_source;
	}

	public TypeStatusEffect GetTypeStatusEffect()
	{
		return m_type;
	}

	public float GetDuration()
	{
		return m_duration;
	}

	public void SetDuration(float i_duration)
	{
		m_duration = i_duration;
	}

	public bool IsActive()
	{
		return m_isActive;
	}

	public bool IsStackable()
	{
		return m_isStackable;
	}
}
