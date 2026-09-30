using System.Collections;
using UnityEngine;

public class Walker : NPC
{
	[SerializeField]
	protected float m_powerJump;

	[SerializeField]
	protected bool m_isCanClimb;

	protected float m_durationClimb;

	private bool m_isTryingToClimb;

	private Coroutine m_coroutineJump;

	private Coroutine m_coroutineJumpZeroGravity;

	private Coroutine m_coroutineClimb;

	private Coroutine m_coroutineWalkOff;

	protected override void OnEnable()
	{
		base.OnEnable();
	}

	protected override void RetrieveAnimationInfos(RuntimeAnimatorController i_runtimeAnimatorController)
	{
		base.RetrieveAnimationInfos(i_runtimeAnimatorController);
		AnimationClip[] animationClips = GetAnimator().runtimeAnimatorController.animationClips;
		foreach (AnimationClip animationClip in animationClips)
		{
			if (animationClip.name == "Climb")
			{
				m_durationClimb = animationClip.length;
				return;
			}
		}
		if (m_durationClimb == 0f)
		{
			m_durationClimb = 1f;
		}
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
	}

	protected override void AddXAIComponent()
	{
		m_xAI = base.gameObject.AddComponent<XAIWalker>();
		m_xAI.Initialize(this);
	}

	public override void MoveToPlayer()
	{
		bool i_left = ((base.transform.position.x - CommonReferences.Instance.GetPlayer().GetPosHips().x > 0f) ? true : false);
		MoveHorizontal(i_left);
	}

	public override void MoveAwayFromPlayer()
	{
		bool flag = ((base.transform.position.x - CommonReferences.Instance.GetPlayer().transform.position.x > 0f) ? true : false);
		MoveHorizontal(!flag);
	}

	public override void MoveHorizontal(bool i_left)
	{
		base.MoveHorizontal(i_left);
	}

	public void MoveDuration(float i_duration, bool i_isLeft)
	{
		StartCoroutine(CoroutineWaitForMoveDuration(i_duration));
	}

	private IEnumerator CoroutineWaitForMoveDuration(float i_duration)
	{
		Coroutine l_coroutineMoveDuration = StartCoroutine(CoroutineMoveDuration());
		yield return new WaitForSeconds(i_duration);
		StopCoroutine(l_coroutineMoveDuration);
	}

	private IEnumerator CoroutineMoveDuration()
	{
		while (true)
		{
			MoveHorizontal(GetIsFacingLeft());
			yield return new WaitForEndOfFrame();
		}
	}

	public void MoveDistance(float i_distanceXToMove)
	{
		StartCoroutine(CoroutineMoveDistance(i_distanceXToMove));
	}

	private IEnumerator CoroutineMoveDistance(float i_distance)
	{
		float l_posXStart = GetPos().x;
		_ = l_posXStart;
		_ = i_distance;
		float l_distanceMade = 0f;
		while (l_distanceMade < i_distance)
		{
			MoveHorizontal(GetIsFacingLeft());
			l_distanceMade = GetPos().x - l_posXStart;
			if (l_distanceMade < 0f)
			{
				l_distanceMade *= -1f;
			}
			yield return new WaitForFixedUpdate();
		}
	}

	private void HandleStairs()
	{
		float num = 0.3f;
		Vector2 posFeet = GetPosFeet();
		posFeet.y += 0.05f;
		Vector2 posFeet2 = GetPosFeet();
		posFeet2.y += 0.75f;
		Debug.DrawRay(posFeet, Vector2.right * num, Color.red);
		Debug.DrawRay(posFeet2, Vector2.right * num, Color.blue);
		RaycastHit2D raycastHit2D = default(RaycastHit2D);
		RaycastHit2D raycastHit2D2 = default(RaycastHit2D);
		if (GetIsFacingLeft())
		{
			raycastHit2D = Physics2D.Raycast(posFeet, Vector2.left, num, LayerMask.GetMask("Platform"));
			raycastHit2D2 = Physics2D.Raycast(posFeet2, Vector2.left, num, LayerMask.GetMask("Platform"));
		}
		else
		{
			raycastHit2D = Physics2D.Raycast(posFeet, Vector2.right, num, LayerMask.GetMask("Platform"));
			raycastHit2D2 = Physics2D.Raycast(posFeet2, Vector2.right, num, LayerMask.GetMask("Platform"));
		}
		if (((bool)raycastHit2D & !raycastHit2D2) && raycastHit2D.transform.eulerAngles.z == 0f)
		{
			float num2 = 0f;
			num2 = ((!GetIsFacingLeft()) ? 0.1f : (-0.1f));
			PlaceFeetOnPos(new Vector2(GetPos().x + num2, raycastHit2D.collider.transform.position.y));
		}
	}

	public void Jump()
	{
		Vector2 velocity = GetComponent<Rigidbody2D>().velocity;
		velocity.y += m_powerJump;
		GetComponent<Rigidbody2D>().velocity = velocity;
	}

	public void Jump(bool i_isLeft)
	{
		Jump();
		if (m_coroutineJump != null)
		{
			StopCoroutine(m_coroutineJump);
		}
		m_coroutineJump = StartCoroutine(CoroutineJump(i_isLeft));
	}

	private IEnumerator CoroutineJump(bool i_isLeft)
	{
		bool flag = false;
		while (!flag)
		{
			yield return new WaitForFixedUpdate();
			MoveHorizontal(i_isLeft);
			flag = GetIsGroundedRayCast();
		}
		m_coroutineJump = null;
	}

	public void JumpZeroGravity(Vector2 i_posToJumpTo, float i_duration)
	{
		if (m_coroutineJumpZeroGravity != null)
		{
			StopCoroutine(m_coroutineJumpZeroGravity);
		}
		m_coroutineJumpZeroGravity = StartCoroutine(CoroutineJumpNoGravity(i_posToJumpTo, i_duration));
	}

	public void WalkOffPlatformTowards(Vector2 i_destination)
	{
		if (m_coroutineWalkOff != null || GetRigidbody2D().bodyType == RigidbodyType2D.Static) return;
		if (m_coroutineJump != null) { StopCoroutine(m_coroutineJump); m_coroutineJump = null; }
		if (m_coroutineJumpZeroGravity != null) { StopCoroutine(m_coroutineJumpZeroGravity); m_coroutineJumpZeroGravity = null; }
		m_coroutineWalkOff = StartCoroutine(CoroutineWalkOffPlatform(i_destination));
	}

	private IEnumerator CoroutineWalkOffPlatform(Vector2 i_destination)
	{
		m_isThinking = false;
		m_isCanMove = true;
		Platform platform = GetPlatformCurrent();
		Collider2D platformCollider = platform == null ? null : platform.GetComponent<Collider2D>();
		bool walkLeft;
		if (platformCollider != null)
			walkLeft = GetPos().x <= platformCollider.bounds.center.x;
		else
			walkLeft = i_destination.x < GetPos().x;
		float elapsed = 0f;
		while (GetIsGroundedRayCast() && elapsed < 0.6f)
		{
			MoveHorizontal(walkLeft);
			elapsed += Time.fixedDeltaTime;
			yield return new WaitForFixedUpdate();
		}
		// A destination directly below the actor supplies no useful horizontal
		// direction. If ordinary movement has not cleared the platform, put the
		// actor's feet just beyond its edge and let normal gravity do the fall.
		if (GetIsGroundedRayCast() && platformCollider != null)
		{
			Collider2D actorCollider = GetComponent<Collider2D>();
			float halfWidth = actorCollider == null ? 0.25f : actorCollider.bounds.extents.x;
			float edgeX = walkLeft
				? platformCollider.bounds.min.x - halfWidth - 0.05f
				: platformCollider.bounds.max.x + halfWidth + 0.05f;
			Vector2 position = GetRigidbody2D().position;
			GetRigidbody2D().position = new Vector2(edgeX, position.y);
			GetRigidbody2D().velocity = new Vector2(walkLeft ? -0.5f : 0.5f, GetRigidbody2D().velocity.y);
			yield return new WaitForFixedUpdate();
		}
		for (elapsed = 0f; elapsed < 0.2f; elapsed += Time.fixedDeltaTime)
		{
			MoveHorizontal(walkLeft);
			yield return new WaitForFixedUpdate();
		}
		m_isThinking = true;
		m_coroutineWalkOff = null;
	}

	private IEnumerator CoroutineJumpNoGravity(Vector2 i_posToJumpTo, float i_duration)
	{
		GetRigidbody2D().isKinematic = true;
		for (float l_timePassed = 0f; l_timePassed < i_duration; l_timePassed += Time.deltaTime)
		{
			Vector2 vector = i_posToJumpTo - GetPos();
			vector.Normalize();
			GetRigidbody2D().MovePosition(GetPos() + vector * Time.deltaTime * 20f);
			yield return new WaitForEndOfFrame();
		}
		GetRigidbody2D().isKinematic = false;
		m_coroutineJumpZeroGravity = null;
	}

	public void Climb(Ledge i_ledge)
	{
		if (m_coroutineClimb != null || GetRigidbody2D().bodyType == RigidbodyType2D.Static)
		{
			return;
		}
		if (m_coroutineJump != null)
		{
			StopCoroutine(m_coroutineJump);
		}
		if (m_coroutineJumpZeroGravity != null)
		{
			StopCoroutine(m_coroutineJumpZeroGravity);
			GetRigidbody2D().isKinematic = false;
		}
		m_isTryingToClimb = false;
		m_coroutineClimb = StartCoroutine(CoroutineClimb(i_ledge));
	}

	private IEnumerator CoroutineClimb(Ledge i_ledge)
	{
		if (GetPos().y - 1.5f < i_ledge.GetPos().y)
		{
			m_stateActorCurrent = StateActor.Climbing;
			m_isThinking = false;
			SetIsFacingLeft(!i_ledge.IsLeft());
			StopMoving();
			PlaceFeetOnPos(i_ledge.GetPos());
			CheckCurrentPlatform();
			GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
			int num = Animator.StringToHash("Climb");
			int num2 = Animator.StringToHash("Base Layer.Climb");
			if (m_animator.HasState(0, num))
			{
				m_animator.Play(num, 0);
			}
			else if (m_animator.HasState(0, num2))
			{
				m_animator.Play(num2, 0);
			}
			Timer timer = new Timer(m_durationClimb);
			yield return timer.CoroutinePlayAndWaitForEnd();
			GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
			m_isThinking = true;
			m_stateActorCurrent = StateActor.Idle;
		}
		m_coroutineClimb = null;
	}

	public bool GetIsCanClimb()
	{
		return m_isCanClimb;
	}

	public void TryToClimb(Ledge i_ledge)
	{
		m_isTryingToClimb = true;
		StartCoroutine(CoroutineTryClimb(i_ledge));
	}

	private IEnumerator CoroutineTryClimb(Ledge i_ledge)
	{
		while (m_isTryingToClimb)
		{
			if (Vector2.Distance(GetPosTopHead(), i_ledge.transform.position) < 3f)
			{
				Climb(i_ledge);
			}
			yield return new WaitForEndOfFrame();
		}
	}

	public bool IsTryingToClimb()
	{
		return m_isTryingToClimb;
	}

	public override void StartRape()
	{
		if (m_coroutineClimb != null)
		{
			InterruptClimb();
		}
		base.StartRape();
		LegacyShackAberrant aberrant = GetComponent<LegacyShackAberrant>();
		if (aberrant != null) aberrant.HandleRape();
	}

	private void InterruptClimb()
	{
		if (m_coroutineClimb != null)
		{
			StopCoroutine(m_coroutineClimb);
			m_coroutineClimb = null;
		}
		GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
		if (m_stateActorCurrent != StateActor.Ragdoll && !m_isDead)
		{
			m_isThinking = true;
			m_stateActorCurrent = StateActor.Idle;
		}
	}
}
