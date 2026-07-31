using System.Collections;
using UnityEngine;

public class Marksman : Walker
{
	[SerializeField]
	private PoisonDart m_poisonDart;

	[SerializeField]
	private Transform m_pointShoot;

	[SerializeField]
	private float m_distanceToShootFrom;

	[SerializeField]
	private float m_delayBetweenShots;

	[SerializeField]
	private AudioClip m_audioShoot;

	private bool m_isCanShoot = true;

	private bool m_isShooting;

	private float m_chanceToShoot01 = 0.5f;

	private bool m_isCanShootOverride = true;

	private Coroutine m_coroutineWaitCanShootDartAgain;

	protected override void AddXAIComponent()
	{
		m_xAI = base.gameObject.AddComponent<XAIMarksman>();
		m_xAI.Initialize(this);
	}

	private bool TryShoot()
	{
		if (Random.Range(0f, 1f) >= 1f - m_chanceToShoot01)
		{
			return true;
		}
		return false;
	}

	private IEnumerator CoroutineWaitCanShootDartAgain()
	{
		m_isCanShootOverride = false;
		yield return new WaitForSeconds(1.5f);
		m_isCanShootOverride = true;
		m_coroutineWaitCanShootDartAgain = null;
	}

	public void Shoot()
	{
		if (!m_isCanShootOverride)
		{
			return;
		}
		if (!TryShoot())
		{
			if (m_coroutineWaitCanShootDartAgain != null)
			{
				StopCoroutine(m_coroutineWaitCanShootDartAgain);
			}
			m_coroutineWaitCanShootDartAgain = StartCoroutine(CoroutineWaitCanShootDartAgain());
		}
		else if (CommonReferences.Instance.GetPlayer().GetStateActorCurrent() != StateActor.Ragdoll)
		{
			SetIsThinking(i_isThinking: false);
			m_isShooting = true;
			m_animator.SetBool("IsShooting", value: true);
			StartCoroutine(CoroutineFireDartFailFallback());
		}
	}

	public void FireDart()
	{
		if (CommonReferences.Instance.GetPlayer().GetStateActorCurrent() == StateActor.Ragdoll || CommonReferences.Instance.GetPlayer().GetIsBeingRaped())
		{
			StartCoroutine(CoroutineWaitForDelayBetweenShots());
			SetIsThinking(i_isThinking: true);
			m_isShooting = false;
			m_animator.SetBool("IsShooting", value: false);
			return;
		}
		PoisonDart poisonDart = Object.Instantiate(m_poisonDart);
		poisonDart.transform.position = m_pointShoot.transform.position;
		poisonDart.gameObject.SetActive(value: true);
		poisonDart.SetDirection(CommonReferences.Instance.GetPlayer().GetSkeletonPlayer().GetBone(BoneTypePlayer.Chest)
			.transform.position);
		poisonDart.Fly();
		PlayAudioSFX(m_audioShoot);
		StartCoroutine(CoroutineWaitForDelayBetweenShots());
		SetIsThinking(i_isThinking: true);
		m_isShooting = false;
		m_animator.SetBool("IsShooting", value: false);
	}

	private IEnumerator CoroutineWaitForDelayBetweenShots()
	{
		m_isCanShoot = false;
		yield return new WaitForSeconds(Random.Range(4, 7));
		m_isCanShoot = true;
	}

	private IEnumerator CoroutineFireDartFailFallback()
	{
		yield return new WaitForSeconds(3.5f);
		if (!m_isDead && !GetRaper().GetIsRaping())
		{
			SetIsThinking(i_isThinking: true);
		}
	}

	public float GetDistanceToShootFrom()
	{
		return m_distanceToShootFrom;
	}

	public bool IsShooting()
	{
		return m_isShooting;
	}

	public bool IsCanShoot()
	{
		return m_isCanShoot;
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
