using UnityEngine;

public class Radio : Interactable
{
	[SerializeField]
	private AudioClip m_audioToPlay;

	[SerializeField]
	private bool m_isOn;

	private bool m_isBroken;

	private AudioSource m_audioSource;

	private new void Start()
	{
		if (!GetComponent<AudioSource>())
		{
			base.gameObject.AddComponent<AudioSource>();
		}
		m_audioSource = GetComponent<AudioSource>();
		m_audioSource.loop = true;
		m_audioSource.clip = m_audioToPlay;
		m_audioSource.playOnAwake = false;
		if (m_isOn)
		{
			Play();
		}
	}

	protected override void HandleActivation(Actor i_initiator, InteractableActivationType i_activationType)
	{
		if (!m_isBroken)
		{
			if (i_activationType == InteractableActivationType.Use)
			{
				PlayOrPause();
			}
			if (i_activationType == InteractableActivationType.Shot)
			{
				DestroyRadio();
			}
		}
	}

	private void PlayOrPause()
	{
		if (m_audioSource.isPlaying)
		{
			Pause();
		}
		else
		{
			Play();
		}
	}

	private void Play()
	{
		m_audioSource.Play();
	}

	private void Pause()
	{
		m_audioSource.Pause();
	}

	private void DestroyRadio()
	{
		Pause();
		m_isBroken = true;
	}
}
