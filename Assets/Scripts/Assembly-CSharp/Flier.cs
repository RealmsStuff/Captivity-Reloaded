using System.Collections;
using UnityEngine;

public class Flier : NPC
{
	protected float m_frictionAir01 = 0.1f;

	[SerializeField]
	private AudioClip m_audioFlying;

	protected AudioSource m_audioSourceFlying;

	[SerializeField]
	protected bool m_isHasWings;

	public override void Awake()
	{
		base.Awake();
		if ((bool)m_audioFlying)
		{
			AddFlyingAudio();
		}
	}

	protected override void AddXAIComponent()
	{
		m_xAI = base.gameObject.AddComponent<XAIFlier>();
		m_xAI.Initialize(this);
	}

	private void AddFlyingAudio()
	{
		m_audioSourceFlying = CommonReferences.Instance.GetManagerAudio().CreateAndAddAudioSourceSFX(base.gameObject);
		m_audioSourceFlying.clip = m_audioFlying;
		m_audioSourceFlying.loop = true;
		m_audioSourceFlying.volume *= 0.25f;
		m_audioSourceFlying.Play();
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
	}

	public override void Start()
	{
		base.Start();
		StartCoroutine(CoroutineFrictionAir());
		GetComponent<Rigidbody2D>().gravityScale = 0f;
		if (m_isHasWings)
		{
			ActivateWings();
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		StartCoroutine(CoroutineFrictionAir());
	}

	public override void UpdateAnim()
	{
		if (GetComponent<Rigidbody2D>().velocity == Vector2.zero)
		{
			m_stateActorCurrent = StateActor.Idle;
		}
		else
		{
			m_stateActorCurrent = StateActor.Moving;
		}
		switch (m_stateActorCurrent)
		{
		case StateActor.Idle:
			SetAllAnimatorBoolsToFalse();
			if (m_stateNPCCurrent == StateNPC.Await)
			{
				GetAnimator().SetBool("IsAwaiting", value: true);
			}
			break;
		case StateActor.Moving:
			SetAllAnimatorBoolsToFalse();
			m_animator.SetBool("IsMoving", value: true);
			break;
		}
	}

	protected override void SetAllAnimatorBoolsToFalse()
	{
		m_animator.SetBool("IsMoving", value: false);
		m_animator.SetBool("IsAwaiting", value: false);
	}

	private IEnumerator CoroutineFrictionAir()
	{
		while (true)
		{
			yield return new WaitForSeconds(0.2f);
			GetRigidbody2D().velocity = GetRigidbody2D().velocity * (1f - m_frictionAir01);
		}
	}

	public override void StartAttack()
	{
		StopMoving();
		Attack(CommonReferences.Instance.GetPlayer());
	}

	public override bool GetIsCloseEnoughToAttackPlayer()
	{
		if (Vector3.Distance(base.transform.position, CommonReferences.Instance.GetPlayer().transform.position) < GetRangeInitiateAttackAttackCurrent())
		{
			return true;
		}
		return false;
	}

	public override void MoveToPlayer()
	{
		MoveTo(CommonReferences.Instance.GetPlayer().GetPosHips());
	}

	public override void MoveAwayFromPlayer()
	{
		Vector2 pos = GetPos();
		Vector2 vector = CommonReferences.Instance.GetPlayer().GetPos() - pos;
		vector = -vector;
		MoveToDirection(vector);
	}

	public void MoveTo(Vector2 i_destination)
	{
		Vector2 pos = GetPos();
		Vector2 i_direction = i_destination - pos;
		MoveToDirection(i_direction);
	}

	public void MoveToDirection(Vector2 i_direction)
	{
		if (i_direction.x < 0f)
		{
			SetIsFacingLeft(i_isFacingLeft: true);
		}
		else
		{
			SetIsFacingLeft(i_isFacingLeft: false);
		}
		if (m_isCanMove && m_stateActorCurrent != StateActor.Ragdoll)
		{
			float valueTotal = GetStat("SpeedMax").GetValueTotal();
			if (!(GetVelocity().x > valueTotal) && !(GetVelocity().y > valueTotal))
			{
				float valueTotal2 = GetStat("SpeedAccel").GetValueTotal();
				GetRigidbody2D().AddForce(i_direction.normalized * valueTotal2, ForceMode2D.Force);
			}
		}
	}

	protected void ActivateWings()
	{
		m_animator.SetBool("IsWingsActive", value: true);
	}

	protected void DeActivateWings()
	{
		m_animator.SetBool("IsWingsActive", value: false);
	}

	public override void Die()
	{
		base.Die();
		DeActivateWings();
		if ((bool)m_audioSourceFlying)
		{
			m_audioSourceFlying.Stop();
		}
	}

	public override bool IsHasLineOfSightToPlayerRaycast()
	{
		Vector2 posTopHead = GetPosTopHead();
		posTopHead.y -= 0.1f;
		Vector2 pos = CommonReferences.Instance.GetPlayer().GetPos();
		Vector2 direction = pos - posTopHead;
		int mask = LayerMask.GetMask("Player", "Platform");
		RaycastHit2D raycastHit2D = Physics2D.Raycast(posTopHead, direction, 999f, mask);
		bool flag = false;
		if ((bool)raycastHit2D && raycastHit2D.collider.gameObject.layer == LayerMask.NameToLayer("Player"))
		{
			flag = true;
		}
		if (!flag)
		{
			m_isHasLineOfSightToPlayer = false;
			return false;
		}
		posTopHead = GetPosFeet();
		posTopHead.y += 0.1f;
		direction = pos - posTopHead;
		raycastHit2D = Physics2D.Raycast(posTopHead, direction, 999f, mask);
		bool flag2 = false;
		if ((bool)raycastHit2D && raycastHit2D.collider.gameObject.layer == LayerMask.NameToLayer("Player"))
		{
			flag2 = true;
		}
		if (!flag2)
		{
			m_isHasLineOfSightToPlayer = false;
			return false;
		}
		m_isHasLineOfSightToPlayer = true;
		return true;
	}
}
