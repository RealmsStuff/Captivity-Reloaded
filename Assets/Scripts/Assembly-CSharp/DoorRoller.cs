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

	private new void Start()
	{
		if (m_priceToActivate == 0)
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

	private void Open()
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

	private void Close()
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
}
