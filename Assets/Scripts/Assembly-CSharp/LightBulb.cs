using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LightBulb : Interactable
{
	[SerializeField]
	private Sprite m_sprBulb;

	[SerializeField]
	private Sprite m_sprBulbBroken;

	[SerializeField]
	protected UnityEngine.Rendering.Universal.Light2D m_light;

	[SerializeField]
	private float m_flickerness01;

	[SerializeField]
	private ParticleSystem m_particleExplode;

	[SerializeField]
	private bool m_isEnabled;

	private bool m_isBroken;

	private List<AudioClip> m_audiosFlicker = new List<AudioClip>();

	private Coroutine m_coroutineFlicker;

	protected override void Start()
	{
		base.Start();
		m_light.enabled = m_isEnabled;
		if ((bool)m_sprBulb && (bool)GetComponent<SpriteRenderer>())
		{
			GetComponent<SpriteRenderer>().sprite = m_sprBulb;
		}
		if (m_isEnabled && m_flickerness01 > 0f)
		{
			m_coroutineFlicker = StartCoroutine(CoroutineFlicker());
		}
	}

	protected override void HandleActivation(Actor i_initiator, InteractableActivationType i_activationType)
	{
		if (i_activationType == InteractableActivationType.Shot && !m_isBroken)
		{
			Break();
		}
		if (i_activationType == InteractableActivationType.Use || i_activationType == InteractableActivationType.Operator)
		{
			if (m_isEnabled)
			{
				TurnOff();
			}
			else
			{
				TurnOn();
			}
		}
	}

	private IEnumerator CoroutineFlicker()
	{
		while (!m_isBroken)
		{
			m_light.enabled = true;
			float seconds = Random.Range(0f + (1f - m_flickerness01) * 5f, 6f - m_flickerness01 * 5f);
			yield return new WaitForSeconds(seconds);
			bool l_doneFlickering = false;
			while (!l_doneFlickering)
			{
				m_light.enabled = false;
				float seconds2 = Random.Range(0.05f, 0.15f);
				yield return new WaitForSeconds(seconds2);
				m_light.enabled = true;
				PlayRandomAudioFlicker();
				yield return new WaitForSeconds(Random.Range(0.05f, 0.15f));
				float num = Random.Range(0, 100);
				float num2 = m_flickerness01 * 75f;
				if (num >= num2)
				{
					l_doneFlickering = true;
				}
			}
		}
	}

	private void PlayRandomAudioFlicker()
	{
		switch (Random.Range(0, 2))
		{
		case 0:
			m_audioSourceSFX.PlayOneShot(Resources.Load<AudioClip>("Audio/Click1"));
			break;
		case 1:
			m_audioSourceSFX.PlayOneShot(Resources.Load<AudioClip>("Audio/Click2"));
			break;
		}
	}

	private void Break()
	{
		StopAllCoroutines();
		m_light.enabled = false;
		m_particleExplode.Play();
		if ((bool)m_sprBulbBroken && (bool)GetComponent<SpriteRenderer>())
		{
			GetComponent<SpriteRenderer>().sprite = m_sprBulbBroken;
		}
		m_isBroken = true;
		m_isEnabled = false;
	}

	private void TurnOff()
	{
		if (!m_isBroken)
		{
			m_light.enabled = false;
			m_isEnabled = false;
		}
	}

	private void TurnOn()
	{
		if (!m_isBroken)
		{
			m_light.enabled = true;
			m_isEnabled = true;
			if (m_flickerness01 > 0f)
			{
				StartCoroutine(CoroutineFlicker());
			}
		}
	}

	public void SetFlickerness01(float i_flickerness01)
	{
		m_flickerness01 = i_flickerness01;
		if (m_flickerness01 > 0f)
		{
			if (m_coroutineFlicker == null)
			{
				m_coroutineFlicker = StartCoroutine(CoroutineFlicker());
			}
		}
		else if (m_coroutineFlicker != null)
		{
			StopCoroutine(m_coroutineFlicker);
			m_coroutineFlicker = null;
		}
	}
}
