using UnityEngine;

public abstract class Trigger : MonoBehaviour
{
	[Tooltip("If set to true, method OnTriggerExit2D will be called which reverts the applied changes from entering the trigger.")]
	[SerializeField]
	private bool m_isAreaTrigger;

	[SerializeField]
	private bool m_isTriggerOnce;

	private bool m_isPlayerInTrigger;

	private bool m_isTriggeredAlready;

	private void OnTriggerEnter2D(Collider2D i_collision)
	{
		if (!m_isPlayerInTrigger && (!m_isTriggerOnce || !m_isTriggeredAlready) && ((bool)i_collision.GetComponent<Player>() || (bool)i_collision.GetComponent<PlayerCollisionHandler>()))
		{
			m_isPlayerInTrigger = true;
			m_isTriggeredAlready = true;
			HandleTriggerEnter();
		}
	}

	protected abstract void HandleTriggerEnter();

	private void OnTriggerExit2D(Collider2D i_collision)
	{
		if (((bool)i_collision.GetComponent<Player>() || (bool)i_collision.GetComponent<PlayerCollisionHandler>()) && m_isAreaTrigger)
		{
			m_isPlayerInTrigger = false;
			HandleTriggerExit();
		}
	}

	protected abstract void HandleTriggerExit();
}
