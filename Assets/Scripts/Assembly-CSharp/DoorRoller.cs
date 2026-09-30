using System.Collections;
using UnityEngine;

public class DoorRoller : Interactable
{
	[SerializeField]
	private GameObject m_doorRollable;

	[SerializeField]
	private float m_distanceSeeActor;

	[SerializeField]
	private AudioClip m_audioOpen;

	[SerializeField]
	private AudioClip m_audioClose;

	[SerializeField]
	private AudioClip m_audioEnd;

	private bool m_isOpen;

	private bool m_isOpening;

	private bool m_isClosing;

	private Coroutine m_coroutineOpen;

	private Coroutine m_coroutineClose;

	private bool m_hasModConfiguration;

	private bool m_modUseProximity;

	private bool m_modInitiallyOpen;

	public void ConfigureModDoor(float i_proximityRadius, bool i_useProximity, bool i_initiallyOpen)
	{
		ResetModActivationLinks();
		m_hasModConfiguration = true;
		m_modUseProximity = i_useProximity;
		m_modInitiallyOpen = i_initiallyOpen;
		m_distanceSeeActor = i_proximityRadius;
		m_priceToActivate = 0;
		m_isSingleUse = false;
		m_isContinuesActivation = false;
		m_isCanBeUsedToActivate = false;
		m_isCanBeTouchedToActivate = false;
		m_isCanBeShotToActivate = false;
		m_isCanBeActivatedByNPC = false;
		m_isHideNotificationInteract = true;
		m_isUnInteractable = false;
	}

	public bool IsUsableModTemplate()
	{
		return m_doorRollable != null && m_audioOpen != null && m_audioClose != null && m_audioEnd != null;
	}

	public bool ConfigureModVisual(Sprite i_sprite)
	{
		if (i_sprite == null) return true;
		SpriteRenderer renderer = m_doorRollable == null ? null
			: m_doorRollable.GetComponentInChildren<SpriteRenderer>(true);
		if (renderer == null) return false;
		renderer.sprite = i_sprite;
		return true;
	}

	private new void Start()
	{
		if (m_hasModConfiguration && m_modInitiallyOpen)
		{
			m_doorRollable.transform.localScale = new Vector3(m_doorRollable.transform.localScale.x, 0f,
				m_doorRollable.transform.localScale.z);
			m_isOpen = true;
		}
		if (m_hasModConfiguration ? m_modUseProximity : m_priceToActivate == 0)
		{
			StartCoroutine(CoroutineCheckIfCanOpenOrClose());
		}
	}

	private IEnumerator CoroutineCheckIfCanOpenOrClose()
	{
		while (true)
		{
			if (!m_isOpen)
			{
				bool flag = false;
				foreach (Actor allActor in CommonReferences.Instance.GetManagerStages().GetStageCurrent().GetAllActors())
				{
					if (Vector2.Distance(allActor.GetPosHips(), base.transform.position) <= m_distanceSeeActor)
					{
						flag = true;
					}
				}
				if (flag)
				{
					Open();
				}
			}
			else
			{
				bool flag2 = true;
				foreach (Actor allActor2 in CommonReferences.Instance.GetManagerStages().GetStageCurrent().GetAllActors())
				{
					if (Vector2.Distance(allActor2.GetPosHips(), base.transform.position) <= m_distanceSeeActor)
					{
						flag2 = false;
						break;
					}
				}
				if (flag2)
				{
					Close();
				}
			}
			yield return new WaitForSeconds(0.25f);
		}
	}

	protected override void HandleActivation(Actor i_initiator, InteractableActivationType i_activationType)
	{
		StartCoroutine(CoroutineCheckIfCanOpenOrClose());
	}

	public void Open()
	{
		if (!m_isOpening)
		{
			m_isClosing = false;
			m_isOpening = true;
			m_isOpen = true;
			m_audioSourceSFX.PlayOneShot(m_audioOpen);
			if (m_coroutineClose != null)
			{
				StopCoroutine(m_coroutineClose);
			}
			m_coroutineOpen = StartCoroutine(CoroutineOpen());
		}
	}

	private IEnumerator CoroutineOpen()
	{
		float l_heightFrom = m_doorRollable.transform.localScale.y;
		float l_heightTo = 0f;
		float l_timeToMove = 1f;
		float l_timeCurrent = 0f;
		while (l_timeCurrent < l_timeToMove)
		{
			l_timeCurrent += Time.fixedDeltaTime;
			float i_time = l_timeCurrent / l_timeToMove;
			float y = AnimationTools.CalculateOverTime(AnimationTools.Transition.Steep, AnimationTools.Transition.Smooth, l_heightFrom, l_heightTo, i_time);
			m_doorRollable.transform.localScale = new Vector3(m_doorRollable.transform.localScale.x, y, m_doorRollable.transform.localScale.z);
			yield return new WaitForFixedUpdate();
		}
		m_audioSourceSFX.PlayOneShot(m_audioEnd);
		m_isOpening = false;
	}

	public void Close()
	{
		if (!m_isClosing)
		{
			m_isClosing = true;
			m_isOpening = false;
			m_isOpen = false;
			m_audioSourceSFX.PlayOneShot(m_audioClose);
			if (m_coroutineOpen != null)
			{
				StopCoroutine(m_coroutineOpen);
			}
			m_coroutineClose = StartCoroutine(CoroutineClose());
		}
	}

	private IEnumerator CoroutineClose()
	{
		float l_heightFrom = m_doorRollable.transform.localScale.y;
		float l_heightTo = 1f;
		float l_timeToMove = 1f;
		float l_timeCurrent = 0f;
		while (l_timeCurrent < l_timeToMove)
		{
			l_timeCurrent += Time.fixedDeltaTime;
			float i_time = l_timeCurrent / l_timeToMove;
			float y = AnimationTools.CalculateOverTime(AnimationTools.Transition.Smooth, AnimationTools.Transition.Steep, l_heightFrom, l_heightTo, i_time);
			m_doorRollable.transform.localScale = new Vector3(m_doorRollable.transform.localScale.x, y, m_doorRollable.transform.localScale.z);
			yield return new WaitForFixedUpdate();
		}
		m_audioSourceSFX.PlayOneShot(m_audioEnd);
	}

	public void OpenOrClose()
	{
		if (m_isOpen) Close();
		else Open();
	}

	public bool GetIsOpen()
	{
		return m_isOpen;
	}
}
