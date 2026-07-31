using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fly : Flier
{
	[SerializeField]
	private float m_distancePrepareCharge;

	[SerializeField]
	private float m_distanceCharge;

	[SerializeField]
	private float m_secsDelayBetweenCharges;

	private bool m_isCanCharge = true;

	private bool m_isPreparingCharge;

	private bool m_isCharging;

	private bool m_isHitPlayerDuringCharge;

	private List<StatModifier> m_modifiersCharge = new List<StatModifier>();

	private Coroutine m_coroutinePrepareCharge;

	private Coroutine m_coroutineCharge;

	public override void Awake()
	{
		base.Awake();
		m_isCanAttack = false;
	}

	protected override void AddXAIComponent()
	{
		m_xAI = base.gameObject.AddComponent<XAIFly>();
		m_xAI.Initialize(this);
	}

	public override void FixedUpdate()
	{
		base.FixedUpdate();
		if (!m_isDead && m_isCharging && !m_isHitPlayerDuringCharge && IsCloseEnoughToHitDuringCharge() && CommonReferences.Instance.GetPlayer().GetIsCanBeAttacked())
		{
			HitPlayerDuringCharge();
		}
	}

	public override void UpdateAnim()
	{
		if (!m_isPreparingCharge && !m_isCharging)
		{
			base.UpdateAnim();
		}
	}

	public void PrepareCharge()
	{
		if (!IsDead())
		{
			SetIsThinking(i_isThinking: false);
			m_coroutinePrepareCharge = StartCoroutine(CoroutinePrepareCharge());
		}
	}

	private IEnumerator CoroutinePrepareCharge()
	{
		m_isPreparingCharge = true;
		m_isCanCharge = false;
		m_animator.Play("PrepareCharge");
		yield return new WaitForSeconds(0.5f);
		m_isPreparingCharge = false;
		m_coroutineCharge = StartCoroutine(CoroutineCharge());
	}

	private IEnumerator CoroutineCharge()
	{
		m_isCharging = true;
		m_modifiersCharge.Add(AddStatModifier("SpeedAccel", 30f));
		m_modifiersCharge.Add(AddStatModifier("SpeedMax", 30f));
		Vector2 pos = GetPos();
		Vector2 vector = CommonReferences.Instance.GetPlayer().GetSkeletonPlayer().GetBone(BoneTypePlayer.Head)
			.transform.position;
		Vector2 l_direction = vector - pos;
		Vector2 l_posUpdate = GetPos();
		float l_distanceTraveled = 0f;
		int l_updates = 0;
		bool l_isDone = false;
		while (!l_isDone)
		{
			MoveToDirection(l_direction);
			l_distanceTraveled += Vector2.Distance(l_posUpdate, GetPos());
			l_posUpdate = GetPos();
			l_updates++;
			if (l_distanceTraveled >= m_distanceCharge)
			{
				l_isDone = true;
			}
			if (l_updates >= 180)
			{
				l_isDone = true;
			}
			if (m_isHitPlayerDuringCharge)
			{
				l_isDone = true;
			}
			yield return new WaitForFixedUpdate();
		}
		m_animator.Play("Idle");
		RemoveStatModifier(m_modifiersCharge);
		m_modifiersCharge.Clear();
		m_isCharging = false;
		SetIsThinking(i_isThinking: true);
		StartCoroutine(CoroutineWaitBeforeCanChargeAgain());
	}

	private IEnumerator CoroutineWaitBeforeCanChargeAgain()
	{
		yield return new WaitForSeconds(m_secsDelayBetweenCharges);
		m_isHitPlayerDuringCharge = false;
		m_isCanCharge = true;
	}

	private void InterruptCharge()
	{
		if (m_coroutineCharge != null)
		{
			StopCoroutine(m_coroutineCharge);
		}
		RemoveStatModifier(m_modifiersCharge);
		m_modifiersCharge.Clear();
		m_animator.Play("Idle");
		m_isCharging = false;
		SetIsThinking(i_isThinking: true);
		StartCoroutine(CoroutineWaitBeforeCanChargeAgain());
	}

	private void HitPlayerDuringCharge()
	{
		m_isHitPlayerDuringCharge = true;
		InterruptCharge();
		if (CommonReferences.Instance.GetPlayer().GetIsCanBeRaped() && !GetIsPlayerBackTurnedToMe())
		{
			StartRape();
			CommonReferences.Instance.GetManagerHud().GetStatusPlayerHud().CreateAndAddStatus("Fly Face Assault", "The fly abruptly crammed itself into your face!", StatusPlayerHudItemColor.Rape, 8f);
			m_interactions[0].Trigger(this);
		}
		else
		{
			PerformAttack(CommonReferences.Instance.GetPlayer());
		}
	}

	public bool IsCloseEnoughToPlayerToCharge()
	{
		if (Vector2.Distance(CommonReferences.Instance.GetPlayer().GetPos(), GetPos()) <= m_distancePrepareCharge)
		{
			return true;
		}
		return false;
	}

	private bool IsCloseEnoughToHitDuringCharge()
	{
		if (Vector2.Distance(CommonReferences.Instance.GetPlayer().GetPos(), GetPos()) <= 2f)
		{
			return true;
		}
		return false;
	}

	public float GetDistancePrepareCharge()
	{
		return m_distancePrepareCharge;
	}

	public bool IsCanCharge()
	{
		return m_isCanCharge;
	}

	public bool IsChargingOrPreparingCharge()
	{
		if (m_isCharging || m_isPreparingCharge)
		{
			return true;
		}
		return false;
	}

	public override void Die()
	{
		base.Die();
		if (m_coroutinePrepareCharge != null)
		{
			StopCoroutine(m_coroutinePrepareCharge);
		}
		if (m_coroutineCharge != null)
		{
			StopCoroutine(m_coroutineCharge);
		}
	}
}
