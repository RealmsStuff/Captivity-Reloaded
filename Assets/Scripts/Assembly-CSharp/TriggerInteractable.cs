using UnityEngine;

public abstract class TriggerInteractable : MonoBehaviour
{
	[SerializeField]
	private bool m_isTriggerOnce;

	private bool m_isTriggeredAlready;

	public void Trigger()
	{
		if (!m_isTriggeredAlready || !m_isTriggerOnce)
		{
			m_isTriggeredAlready = true;
			HandleTrigger();
		}
	}

	protected abstract void HandleTrigger();
}
