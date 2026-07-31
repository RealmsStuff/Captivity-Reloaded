using System.Collections;
using UnityEngine;

public class Orc : Walker
{
	[SerializeField]
	private Collider2D m_colliderShield;

	private bool m_isShieldRaised;

	private bool m_isCanLowerBackturned;

	private Coroutine m_coroutineWaitBeforeWillLowerBackturn;

	protected override void AddXAIComponent()
	{
		m_xAI = base.gameObject.AddComponent<XAIOrc>();
		m_xAI.Initialize(this);
	}

	public override void Start()
	{
		base.Start();
		CommonReferences.Instance.GetPlayer().OnChangeFacingSide += OnPlayerChangeFacingSide;
	}

	public void RaiseShield()
	{
		m_isShieldRaised = true;
		m_animator.SetBool("IsShieldRaised", value: true);
		m_colliderShield.enabled = true;
		StartCoroutine(CoroutineRaiseOrLowerShield(i_isRaise: true));
		StartCoroutine(CoroutineWaitBeforeWillLowerBackturn());
	}

	public void LowerShield()
	{
		m_isShieldRaised = false;
		m_animator.SetBool("IsShieldRaised", value: false);
		m_colliderShield.enabled = false;
		StartCoroutine(CoroutineRaiseOrLowerShield(i_isRaise: false));
	}

	private IEnumerator CoroutineRaiseOrLowerShield(bool i_isRaise)
	{
		float l_tranparencyFrom = ((!i_isRaise) ? 1 : 0);
		float l_tranparencyTo = (i_isRaise ? 1 : 0);
		float l_timeToMove = 0.5f;
		float l_timeCurrent = 0f;
		while (l_timeCurrent < l_timeToMove)
		{
			l_timeCurrent += Time.fixedDeltaTime;
			float i_time = l_timeCurrent / l_timeToMove;
			float weight = AnimationTools.CalculateOverTime(AnimationTools.Transition.Steep, AnimationTools.Transition.Smooth, l_tranparencyFrom, l_tranparencyTo, i_time);
			m_animator.SetLayerWeight(m_animator.GetLayerIndex("Shield"), weight);
			yield return new WaitForFixedUpdate();
		}
	}

	private IEnumerator CoroutineWaitBeforeWillLowerBackturn()
	{
		m_isCanLowerBackturned = false;
		yield return new WaitForSeconds(1.5f);
		m_isCanLowerBackturned = true;
	}

	private void OnPlayerChangeFacingSide(bool i_isFacingLeft)
	{
		if (m_coroutineWaitBeforeWillLowerBackturn != null)
		{
			StopCoroutine(m_coroutineWaitBeforeWillLowerBackturn);
		}
		m_coroutineWaitBeforeWillLowerBackturn = StartCoroutine(CoroutineWaitBeforeWillLowerBackturn());
	}

	public bool IsShieldRaised()
	{
		return m_isShieldRaised;
	}

	public bool IsCanLowerBackturnedPlayer()
	{
		return m_isCanLowerBackturned;
	}

	public override void Die()
	{
		base.Die();
		m_colliderShield.enabled = false;
		CommonReferences.Instance.GetPlayer().OnChangeFacingSide -= OnPlayerChangeFacingSide;
	}

	public override void DropAllPickupAbles()
	{
		StageJungle stageJungle = (StageJungle)CommonReferences.Instance.GetManagerStages().GetStageCurrent();
		if (!stageJungle.IsFetishDropped(m_pickUpablesToDrop[0]))
		{
			stageJungle.SetFetishDropped(m_pickUpablesToDrop[0]);
			base.DropAllPickupAbles();
		}
	}
}
