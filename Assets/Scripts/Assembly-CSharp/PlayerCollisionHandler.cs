using UnityEngine;

public class PlayerCollisionHandler : MonoBehaviour
{
	[SerializeField]
	public Collider2D[] m_collidersStanding;

	[SerializeField]
	public Collider2D[] m_collidersCrouching;

	private void OnTriggerEnter2D(Collider2D i_collision)
	{
	}
}
