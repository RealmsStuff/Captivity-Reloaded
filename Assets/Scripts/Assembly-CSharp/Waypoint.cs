using UnityEngine;

public class Waypoint : MonoBehaviour
{
	[SerializeField]
	private Waypoint m_waypointDestination;

	private bool m_isIgnorePlayer;

	private void OnTriggerEnter2D(Collider2D i_collision)
	{
		if (!m_isIgnorePlayer && !(m_waypointDestination == null) && ((bool)i_collision.GetComponent<Player>() || (bool)i_collision.GetComponent<PlayerCollisionHandler>()))
		{
			CommonReferences.Instance.GetManagerStages().GoToWaypoint(m_waypointDestination);
		}
	}

	private void OnTriggerExit2D(Collider2D i_collision)
	{
		if ((bool)i_collision.GetComponent<Player>() || (bool)i_collision.GetComponent<PlayerCollisionHandler>())
		{
			m_isIgnorePlayer = false;
		}
	}

	public Waypoint GetDestination()
	{
		return m_waypointDestination;
	}

	public Stage GetStage()
	{
		return GetComponentsInParent<Stage>(includeInactive: true)[0];
	}

	public Vector2 GetPos()
	{
		return base.transform.position;
	}

	public void DisableUntilPlayerExitsCollider()
	{
		m_isIgnorePlayer = true;
	}
}
