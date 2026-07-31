using System.Collections;
using UnityEngine;

public abstract class Projectile : MonoBehaviour
{
	[SerializeField]
	protected float m_speed;

	[SerializeField]
	protected float m_gravityScale01;

	[SerializeField]
	protected bool m_isHitsInvulnerable;

	protected bool m_isFlying;

	protected Actor m_owner;

	public void Fly()
	{
		GetComponent<Rigidbody2D>().velocity = base.transform.right * m_speed;
		GetComponent<Rigidbody2D>().gravityScale = m_gravityScale01;
		base.transform.parent = CommonReferences.Instance.GetManagerStages().GetStageCurrent().transform;
		m_isFlying = true;
		StartCoroutine(CoroutineFly());
		Object.Destroy(base.gameObject, 10f);
	}

	protected IEnumerator CoroutineFly()
	{
		while (m_isFlying)
		{
			if (Time.timeScale > 0f)
			{
				base.transform.position += (base.transform.right *= m_speed);
				base.transform.rotation = Quaternion.LookRotation(base.transform.forward, base.transform.up);
				base.transform.eulerAngles = new Vector3(0f, 0f, base.transform.eulerAngles.z);
			}
			yield return new WaitForEndOfFrame();
		}
	}

	protected void StopFlying()
	{
		m_isFlying = false;
	}

	public void SetOwner(Actor i_owner)
	{
		m_owner = i_owner;
	}

	public void SetDirection(Vector3 i_targetPos)
	{
		Vector3 vector = i_targetPos - base.transform.position;
		vector.Normalize();
		float angle = Mathf.Atan2(vector.y, vector.x) * 57.29578f;
		base.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
	}

	private void OnTriggerEnter2D(Collider2D i_collider)
	{
		HandleHit(i_collider);
	}

	private void OnCollisionEnter2D(Collision2D i_collision)
	{
		HandleHit(i_collision.collider);
	}

	protected abstract void HandleHit(Collider2D i_collider);
}
