using System.Collections;
using UnityEngine;

public class Ledge : MonoBehaviour
{
	private bool m_isLeft;

	private bool m_isClimbable;

	public void Initialize(bool i_isLeft, bool i_isClimable)
	{
		m_isLeft = i_isLeft;
		m_isClimbable = i_isClimable;
		if (m_isClimbable)
		{
			StartCoroutine(CoroutineCheckForClimb());
		}
	}

	private IEnumerator CoroutineCheckForClimb()
	{
		yield return new WaitForFixedUpdate();
		while (true)
		{
			CheckForClimb();
			yield return new WaitForEndOfFrame();
			yield return new WaitForEndOfFrame();
			yield return new WaitForEndOfFrame();
			yield return new WaitForEndOfFrame();
			yield return new WaitForEndOfFrame();
			yield return new WaitForEndOfFrame();
			yield return new WaitForEndOfFrame();
		}
	}

	private void CheckForClimb()
	{
		Player player = CommonReferences.Instance.GetPlayer();
		if (!(Vector2.Distance(player.GetPosTopHead(), base.transform.position) < 1.5f))
		{
			return;
		}
		if (m_isLeft)
		{
			if (!player.GetIsFacingLeft())
			{
				player.Climb(this);
			}
		}
		else if (player.GetIsFacingLeft())
		{
			player.Climb(this);
		}
	}

	public BoxCollider2D GetCollider()
	{
		return GetComponentInParent<BoxCollider2D>();
	}

	public bool IsLeft()
	{
		return m_isLeft;
	}

	public Vector2 GetPos()
	{
		return base.transform.position;
	}
}
