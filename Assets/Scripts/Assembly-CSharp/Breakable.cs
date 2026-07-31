using System.Collections;
using UnityEngine;

public class Breakable : Interactable
{
	[SerializeField]
	private float m_secsDelayBeforeBreak;

	[SerializeField]
	private AudioClip m_audioStartingToBreak;

	[SerializeField]
	private AudioClip m_audioBreak;

	private bool m_isStartedBreaking;

	private new void Start()
	{
		m_isStartedBreaking = false;
	}

	private IEnumerator CoroutineBreak()
	{
		if (m_audioStartingToBreak != null && m_secsDelayBeforeBreak > 0f)
		{
			CommonReferences.Instance.GetManagerAudio().PlayAudioSFX(m_audioStartingToBreak);
		}
		yield return new WaitForSeconds(m_secsDelayBeforeBreak);
		SpriteRenderer component = GetComponent<SpriteRenderer>();
		component.color = new Color(component.color.r, component.color.g, component.color.b, 0.5f);
		GetComponent<SpriteRenderer>().color = component.color;
		if (m_audioBreak != null)
		{
			CommonReferences.Instance.GetManagerAudio().PlayAudioSFX(m_audioBreak);
		}
		GetComponent<Collider2D>().enabled = false;
	}

	protected override void HandleActivation(Actor i_initiator, InteractableActivationType i_activationType)
	{
		if (!m_isStartedBreaking)
		{
			m_isStartedBreaking = true;
			StartCoroutine(CoroutineBreak());
		}
	}
}
