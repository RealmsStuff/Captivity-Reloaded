using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hunter : Walker
{
	private bool m_isRunningAwayAfterRape;

	private bool m_isProwling;

	private List<StatModifier> m_modifiersProwl = new List<StatModifier>();

	private bool m_isCanDodge = true;

	private Coroutine m_coroutineWaitForLandAfterLeap;

	public override void Start()
	{
		base.Start();
		m_isRunningAwayAfterRape = false;
		m_isProwling = false;
		CommonReferences.Instance.GetPlayer().OnShoot += TryToDodge;
		GetRaper().OnEndRape += RunAwayAfterRape;
	}

	protected override void AddXAIComponent()
	{
		m_xAI = base.gameObject.AddComponent<XAIHunter>();
		m_xAI.Initialize(this);
	}

	private void RunAwayAfterRape()
	{
		if (GetIsFacingLeft())
		{
			Jump(i_isLeft: true);
		}
		else
		{
			Jump(i_isLeft: false);
		}
		m_isRunningAwayAfterRape = true;
		StartCoroutine(CoroutineWaitForRunAwayEnd());
	}

	private IEnumerator CoroutineWaitForRunAwayEnd()
	{
		yield return new WaitForSeconds(3f);
		m_isRunningAwayAfterRape = false;
	}

	public override void UpdateAnim()
	{
		base.UpdateAnim();
		if (m_isRunningAwayAfterRape)
		{
			if (m_isProwling)
			{
				StopProwling();
			}
			m_animator.SetBool("IsRunningAway", value: true);
		}
		else
		{
			m_animator.SetBool("IsRunningAway", value: false);
		}
		if (m_stateActorCurrent == StateActor.Idle)
		{
			return;
		}
		if (Vector2.Distance(CommonReferences.Instance.GetPlayer().GetPos(), GetPos()) < 10f && CommonReferences.Instance.GetPlayer().GetPlatformCurrent() == GetPlatformCurrent() && CommonReferences.Instance.GetPlayer().GetStateActorCurrent() != StateActor.Ragdoll && !CommonReferences.Instance.GetPlayer().IsDead())
		{
			m_animator.SetBool("IsProwling", value: true);
			if (!m_isProwling)
			{
				StartProwling();
			}
		}
		else if (m_isProwling)
		{
			StopProwling();
		}
	}

	private void StartProwling()
	{
		m_isProwling = true;
		m_animator.SetBool("IsProwling", value: true);
		m_modifiersProwl.Add(AddStatModifier("SpeedAccel", -4f));
		m_modifiersProwl.Add(AddStatModifier("SpeedMax", -4f));
	}

	private void StopProwling()
	{
		m_isProwling = false;
		m_animator.SetBool("IsProwling", value: false);
		RemoveStatModifier(m_modifiersProwl);
		m_modifiersProwl.Clear();
	}

	protected override void SetAllAnimatorBoolsToFalse()
	{
		base.SetAllAnimatorBoolsToFalse();
		m_animator.SetBool("IsProwling", value: false);
	}

	private void TryToDodge(List<Bullet> i_bulletsShot)
	{
		if (!GetIsCloseEnoughToSeePlayer() || !m_isThinking || m_stateActorCurrent == StateActor.Jumping || !m_isCanDodge)
		{
			return;
		}
		foreach (Bullet item in i_bulletsShot)
		{
			if (item.IsActorHit(this))
			{
				Dodge();
				break;
			}
		}
	}

	private void Dodge()
	{
		SetIsInvulnerable(i_isInvulnerable: true, i_isAffectSkeleton: true);
		StartCoroutine(CoroutineDodge());
		switch (Random.Range(0, 2))
		{
		case 0:
			Jump();
			break;
		case 1:
			RunAwayAfterRape();
			break;
		}
		StartCoroutine(CoroutineWaitBeforeCanDodgeAgain());
	}

	private IEnumerator CoroutineDodge()
	{
		yield return new WaitForSeconds(0.25f);
		SetIsInvulnerable(i_isInvulnerable: false, i_isAffectSkeleton: true);
	}

	private IEnumerator CoroutineWaitBeforeCanDodgeAgain()
	{
		m_isCanDodge = false;
		yield return new WaitForSeconds(1f);
		m_isCanDodge = true;
	}

	protected override void HandleThinking()
	{
		if (m_isRunningAwayAfterRape)
		{
			MoveAwayFromPlayer();
		}
		else
		{
			base.HandleThinking();
		}
	}

	public override void Die()
	{
		base.Die();
		CommonReferences.Instance.GetPlayer().OnShoot -= TryToDodge;
		GetRaper().OnEndRape -= RunAwayAfterRape;
	}
}
