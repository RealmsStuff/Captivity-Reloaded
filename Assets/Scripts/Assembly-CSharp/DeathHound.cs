using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathHound : Walker
{
	[SerializeField]
	private float m_distancePrepareCharge;

	[SerializeField]
	private float m_distanceLeap;

	[SerializeField]
	private float m_powerXLeap;

	[SerializeField]
	private float m_powerYLeap;

	[SerializeField]
	private float m_delayBetweenCharges;

	[SerializeField]
	private AudioClip m_audioPrepareCharge;

	[SerializeField]
	private AudioClip m_audioLeap;

	private bool m_isCanCharge = true;

	private bool m_isCanLeap = true;

	private bool m_isPreparingCharge;

	private bool m_isCharging;

	private bool m_isLeaping;

	private bool m_isHitPlayerWhileLeaping;

	private List<StatModifier> m_modifiersCharge = new List<StatModifier>();

	private Coroutine m_coroutineWaitForLandAfterLeap;

	private Coroutine m_coroutinePrepareCharge;

	protected override void AddXAIComponent()
	{
		m_xAI = base.gameObject.AddComponent<XAIDeathHound>();
		m_xAI.Initialize(this);
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (!m_isDead && m_isLeaping && !m_isHitPlayerWhileLeaping && GetIsCloseEnoughToHit())
		{
			HitPlayerDuringLeap();
		}
	}

	public void TryCharge()
	{
		if (m_isCanCharge)
		{
			int num = 35;
			if (Random.Range(1, 101) > 100 - num)
			{
				PrepareCharge();
			}
			else
			{
				StartCoroutine(CoroutineWaitBeforeTryChargeAgain());
			}
		}
	}

	private IEnumerator CoroutineWaitBeforeTryChargeAgain()
	{
		m_isCanCharge = false;
		yield return new WaitForSeconds(5f);
		m_isCanCharge = true;
	}

	public void PrepareCharge()
	{
		if (m_isCanCharge)
		{
			if (m_coroutinePrepareCharge != null)
			{
				StopCoroutine(m_coroutinePrepareCharge);
			}
			m_coroutinePrepareCharge = StartCoroutine(CoroutinePrepareCharge());
		}
	}

	private IEnumerator CoroutinePrepareCharge()
	{
		StopMoving();
		m_isThinking = false;
		m_isPreparingCharge = true;
		PlayAudioSFX(m_audioPrepareCharge);
		m_animator.Play("ChargePrepare");
		yield return new WaitForSeconds(2.5f);
		m_isPreparingCharge = false;
		Charge();
		m_isThinking = true;
	}

	private void Charge()
	{
		m_isCharging = true;
		m_isCanCharge = false;
		m_modifiersCharge.Add(AddStatModifier("SpeedAccel", 2f));
		m_modifiersCharge.Add(AddStatModifier("SpeedMax", 2f));
		GetSkeletonActor().StartFlashingLeapAttack();
		StartCoroutine(CoroutineWaitForChargeEnd());
	}

	private IEnumerator CoroutineWaitForChargeEnd()
	{
		yield return new WaitForSeconds(10f);
		RemoveStatModifier(m_modifiersCharge);
		m_modifiersCharge.Clear();
		m_isCharging = false;
		GetSkeletonActor().StopFlashingLeapAttack();
	}

	public void Leap()
	{
		RemoveStatModifier(m_modifiersCharge);
		m_modifiersCharge.Clear();
		m_isCharging = false;
		GetSkeletonActor().StopFlashingLeapAttack();
		m_isLeaping = true;
		m_isThinking = false;
		m_animator.SetTrigger("Leap");
		PlayAudioVoice(m_audioLeap);
		Vector2 zero = Vector2.zero;
		if (GetIsFacingLeft())
		{
			zero.x = 0f - m_powerXLeap;
		}
		else
		{
			zero.x = m_powerXLeap;
		}
		zero.y = m_powerYLeap;
		SetVelocity(zero);
		m_coroutineWaitForLandAfterLeap = StartCoroutine(CoroutineWaitForLandAfterLeap());
		GetRaper().OnStartRape += InterruptWaitForLandAfterLeap;
	}

	private IEnumerator CoroutineWaitForLandAfterLeap()
	{
		yield return new WaitForSeconds(0.15f);
		bool l_isLanded = false;
		while (!l_isLanded)
		{
			yield return new WaitForEndOfFrame();
			if (GetIsGroundedRayCast())
			{
				l_isLanded = true;
			}
		}
		GetRaper().OnStartRape -= InterruptWaitForLandAfterLeap;
		m_isThinking = true;
		m_isLeaping = false;
		m_isHitPlayerWhileLeaping = false;
		m_animator.Play("Idle");
		StartCoroutine(CoroutineWaitForNextAvailableCharge());
	}

	private void InterruptWaitForLandAfterLeap()
	{
		StopCoroutine(m_coroutineWaitForLandAfterLeap);
		GetRaper().OnStartRape -= InterruptWaitForLandAfterLeap;
		m_isLeaping = false;
		m_isHitPlayerWhileLeaping = false;
		StartCoroutine(CoroutineWaitForNextAvailableCharge());
	}

	private IEnumerator CoroutineWaitForNextAvailableCharge()
	{
		yield return new WaitForSeconds(m_delayBetweenCharges);
		m_isCanCharge = true;
	}

	private void HitPlayerDuringLeap()
	{
		m_isHitPlayerWhileLeaping = true;
		if (CommonReferences.Instance.GetPlayer().GetIsInvulnerable())
		{
			return;
		}
		if (CommonReferences.Instance.GetPlayer().GetIsCanBeRaped() && GetIsPlayerBackTurnedToMe())
		{
			StartRape();
			CommonReferences.Instance.GetManagerHud().GetStatusPlayerHud().CreateAndAddStatus("Death Hound Back Mount", "The Death Hound has quickly mounted your back while you were looking away!", StatusPlayerHudItemColor.Rape, 8f);
			m_interactions[0].Trigger(this);
			return;
		}
		PerformAttack(CommonReferences.Instance.GetPlayer());
		if (!CommonReferences.Instance.GetPlayer().GetIsBeingRaped())
		{
			Vector2 force = new Vector2(10f, 5f);
			if (GetIsFacingLeft())
			{
				force.x *= -1f;
			}
			CommonReferences.Instance.GetPlayer().GetRigidbody2D().AddForce(force, ForceMode2D.Impulse);
		}
	}

	public override void Die()
	{
		base.Die();
		if (m_coroutinePrepareCharge != null)
		{
			StopCoroutine(m_coroutinePrepareCharge);
		}
	}

	public bool GetIsCloseEnoughToPrepareCharge()
	{
		if (Vector2.Distance(GetPos(), CommonReferences.Instance.GetPlayer().GetPos()) <= m_distancePrepareCharge && Vector2.Distance(GetPos(), CommonReferences.Instance.GetPlayer().GetPos()) >= m_distancePrepareCharge / 2f)
		{
			return true;
		}
		return false;
	}

	public bool GetIsCloseEnoughToLeap()
	{
		if (Vector2.Distance(GetPos(), CommonReferences.Instance.GetPlayer().GetPos()) <= m_distanceLeap)
		{
			return true;
		}
		return false;
	}

	public bool GetIsCloseEnoughToHit()
	{
		if (Vector2.Distance(GetPos(), CommonReferences.Instance.GetPlayer().GetPos()) <= 2f)
		{
			return true;
		}
		return false;
	}

	public float GetDistanceLeap()
	{
		return m_distanceLeap;
	}

	public bool GetIsPreparingCharge()
	{
		return m_isPreparingCharge;
	}

	public bool GetIsCharging()
	{
		return m_isCharging;
	}

	public float GetDistancePrepareCharge()
	{
		return m_distancePrepareCharge;
	}

	public bool GetIsCanCharge()
	{
		return m_isCanCharge;
	}
}
