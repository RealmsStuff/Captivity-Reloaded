using System.Collections;
using UnityEngine;

public class Hook : MonoBehaviour
{
	public delegate void DelOnAttach();

	[SerializeField]
	private Sprite m_sprHookAttached;

	private float m_speed = 20f;

	private bool m_isAttached;

	public event DelOnAttach OnAttach;

	public void Fly()
	{
		GetComponent<Rigidbody2D>().velocity = base.transform.right * m_speed;
		StartCoroutine(CoroutineFly());
	}

	private IEnumerator CoroutineFly()
	{
		while (!m_isAttached)
		{
			if (Time.timeScale > 0f && GetComponent<Rigidbody2D>().velocity != Vector2.zero)
			{
				base.transform.rotation = Quaternion.LookRotation(base.transform.forward, base.transform.up);
				base.transform.eulerAngles = new Vector3(0f, 0f, base.transform.eulerAngles.z);
			}
			yield return new WaitForEndOfFrame();
		}
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (!m_isAttached && collision.collider.gameObject.layer == LayerMask.NameToLayer("Platform"))
		{
			AttachHook();
		}
	}

	private void AttachHook()
	{
		m_isAttached = true;
		GetComponent<SpriteRenderer>().sprite = m_sprHookAttached;
		GetComponent<Rigidbody2D>().isKinematic = true;
		GetComponent<Rigidbody2D>().gravityScale = 0f;
		GetComponent<Rigidbody2D>().velocity = Vector2.zero;
		GetComponent<Rigidbody2D>().freezeRotation = true;
		if (this.OnAttach != null)
		{
			this.OnAttach();
		}
	}
}
