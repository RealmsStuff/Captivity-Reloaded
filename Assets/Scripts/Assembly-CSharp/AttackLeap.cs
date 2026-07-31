using System.Collections;
using UnityEngine;

public class AttackLeap : AttackNPC
{
	[Header("- AttackLeap - Use skeleton animation event in leap animation to trigger leap")]
	[SerializeField]
	private Vector2 m_powerLeap;

	[SerializeField]
	private float m_radiusHit01;

	[SerializeField]
	private bool m_isFlashRed;

	[SerializeField]
	private bool m_isInstantRapeOnHit;

	[SerializeField]
	private AudioClip m_audioLeap;

	private bool m_isAttackingBeforeLeap;

	private bool m_isLeaping;

	private Coroutine m_coroutineWaitForLandAfterLeap;

	private float m_rangeInitiateDefault;

	private float m_radiusHit01Default;

	private bool m_isHasHitPlayerWhileLeapingCurrent;

	protected override void Start()
	{
		base.Start();
		m_rangeInitiateDefault = m_rangeInitiate;
		m_radiusHit01Default = m_radiusHit01;
	}

	private void LateUpdate()
	{
		if (!m_npc.IsDead())
		{
			if (m_isAttackingBeforeLeap)
			{
				m_npc.FacePlayer();
			}
			if (m_player.GetStateActorCurrent() == StateActor.Ragdoll || m_player.IsDead())
			{
				SetToImmobileRange();
			}
			else
			{
				SetToNormalRange();
			}
		}
	}

	private void SetToImmobileRange()
	{
		m_rangeInitiate = 2f;
		m_radiusHit01 = 1f;
	}

	private void SetToNormalRange()
	{
		m_rangeInitiate = m_rangeInitiateDefault;
		m_radiusHit01 = m_radiusHit01Default;
	}

	public override void HandleAttackStart()
	{
		base.HandleAttackStart();
		m_isAttackingBeforeLeap = true;
		m_npc.SetIsThinking(i_isThinking: false);
		if (m_isFlashRed)
		{
			m_npc.GetSkeletonActor().StartFlashingLeapAttack();
		}
	}

	public override void HandleAttackHit()
	{
		if (!m_isHasHitPlayerWhileLeapingCurrent)
		{
			m_isHasHitPlayerWhileLeapingCurrent = true;
			if (m_interactionToTriggerOnAttackHit != null)
			{
				m_interactionToTriggerOnAttackHit.Trigger(m_npc);
			}
			if (m_isInstantRapeOnHit)
			{
				m_npc.StartRape();
				m_isHasHitPlayerWhileLeapingCurrent = false;
			}
			else
			{
				m_npc.PerformAttack(m_player);
			}
		}
	}

	public override void HandleAttackEnd()
	{
		base.HandleAttackEnd();
		m_isHasHitPlayerWhileLeapingCurrent = false;
	}

	public void Leap()
	{
		if (m_isFlashRed)
		{
			m_npc.GetSkeletonActor().StopFlashingLeapAttack();
		}
		m_isLeaping = true;
		m_isAttackingBeforeLeap = false;
		Vector2 powerLeap = m_powerLeap;
		if (m_npc.GetIsPlayerLeftOfMe())
		{
			powerLeap.x *= -1f;
		}
		m_npc.GetRigidbody2D().AddForce(powerLeap, ForceMode2D.Impulse);
		m_npc.PlayAudioSFX(m_audioLeap);
		m_coroutineWaitForLandAfterLeap = StartCoroutine(CoroutineWaitForLandAfterLeap());
		m_npc.GetRaper().OnStartRape += InterruptWaitForLandAfterLeap;
		StartCoroutine(CoroutineWaitForHit());
	}

	private IEnumerator CoroutineWaitForHit()
	{
		while (m_isLeaping)
		{
			if (m_npc.GetDistanceBetweenPlayerHips() < 4f * m_radiusHit01)
			{
				HitPlayerDuringLeap();
			}
			yield return new WaitForEndOfFrame();
		}
	}

	private void HitPlayerDuringLeap()
	{
		if (!m_npc.IsDead())
		{
			m_isLeaping = false;
			if (m_player.GetIsCanBeAttacked() && !m_player.GetIsBeingRaped())
			{
				HandleAttackHit();
			}
		}
	}

	private IEnumerator CoroutineWaitForLandAfterLeap()
	{
		yield return new WaitForSeconds(0.15f);
		bool l_isLanded = false;
		while (!l_isLanded)
		{
			yield return new WaitForEndOfFrame();
			if (m_npc.GetIsGroundedRayCast())
			{
				l_isLanded = true;
			}
		}
		m_npc.GetRaper().OnStartRape -= InterruptWaitForLandAfterLeap;
		m_npc.GetAnimator().Play("Idle");
		m_npc.SetIsThinking(i_isThinking: true);
		m_isLeaping = false;
	}

	private void InterruptWaitForLandAfterLeap()
	{
		StopCoroutine(m_coroutineWaitForLandAfterLeap);
		m_npc.GetRaper().OnStartRape -= InterruptWaitForLandAfterLeap;
		m_isLeaping = false;
	}

	public override float GetRangeHit()
	{
		return 4f * m_radiusHit01;
	}
}
