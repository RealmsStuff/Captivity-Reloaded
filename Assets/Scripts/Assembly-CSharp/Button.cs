using System.Collections;
using UnityEngine;

public abstract class Button : Interactable
{
	[SerializeField]
	private bool m_isCanBePressedOnlyOnce;

	[SerializeField]
	private Sprite m_sprPressed;

	[SerializeField]
	private AudioClip m_audioPress;

	protected Sprite m_sprUnpressed;

	protected bool m_isPressedAlready;

	private void Awake()
	{
		m_sprUnpressed = GetComponent<SpriteRenderer>().sprite;
	}

	protected override void HandleActivation(Actor i_initiator, InteractableActivationType i_activationType)
	{
		if (m_isPressedAlready && m_isCanBePressedOnlyOnce)
		{
			return;
		}
		if (m_audioPress != null)
		{
			CommonReferences.Instance.GetManagerAudio().PlayAudioSFX(m_audioPress);
		}
		m_isPressedAlready = true;
		if (m_isCanBePressedOnlyOnce)
		{
			if (m_sprPressed != null)
			{
				GetComponent<SpriteRenderer>().sprite = m_sprPressed;
			}
		}
		else
		{
			StartCoroutine(CoroutineAnimatePress());
		}
		HandlePressButton();
	}

	private IEnumerator CoroutineAnimatePress()
	{
		if (m_sprPressed != null)
		{
			GetComponent<SpriteRenderer>().sprite = m_sprPressed;
		}
		yield return new WaitForSeconds(0.25f);
		if (m_sprUnpressed != null)
		{
			GetComponent<SpriteRenderer>().sprite = m_sprUnpressed;
		}
	}

	protected abstract void HandlePressButton();
}
