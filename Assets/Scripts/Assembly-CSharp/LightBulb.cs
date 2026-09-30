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
	private Sprite m_modActivatedSprite;
	private Color m_modInitialColor = Color.white;
	private Color m_modActivatedColor = Color.white;
	private bool m_hasModPresentation;

	private List<AudioClip> m_audiosFlicker = new List<AudioClip>();

	private Coroutine m_coroutineFlicker;

	public void ConfigureModLight(bool i_initiallyOn, float i_flicker, bool i_interactive = true)
	{
		ResetModActivationLinks();
		m_priceToActivate = 0;
		m_isSingleUse = false;
		m_isCanBeUsedToActivate = i_interactive;
		m_isCanBeTouchedToActivate = false;
		m_isCanBeShotToActivate = i_interactive;
		m_isCanBeActivatedByNPC = false;
		m_isUnInteractable = false;
		Collider2D collider = GetComponent<Collider2D>();
		if (collider != null) collider.enabled = i_interactive;
		m_isBroken = false;
		m_isEnabled = i_initiallyOn;
		m_flickerness01 = i_flicker;
	}

	public bool IsUsableModTemplate()
	{
		return m_light != null && GetComponent<SpriteRenderer>() != null;
	}

	public void ConfigureModPresentation(Sprite i_initialSprite, Sprite i_activatedSprite, Color i_initialColor,
		Color i_activatedColor, float i_innerRadius, float i_outerRadius, float i_falloffIntensity,
		string i_sortingLayer, int i_sortingOrder)
	{
		m_sprBulb = i_initialSprite;
		m_modActivatedSprite = i_activatedSprite;
		m_modInitialColor = i_initialColor;
		m_modActivatedColor = i_activatedColor;
		m_hasModPresentation = true;
		SpriteRenderer renderer = GetComponent<SpriteRenderer>();
		if (renderer != null)
		{
			// Core work-light templates use a tiled renderer with a large authored
			// size. Portable indicator PNGs are single sprites and must not repeat.
			renderer.drawMode = SpriteDrawMode.Simple;
			renderer.sprite = m_sprBulb;
			renderer.size = m_sprBulb == null ? Vector2.one : (Vector2)m_sprBulb.bounds.size;
			renderer.transform.localScale = Vector3.one;
			renderer.sortingLayerName = i_sortingLayer;
			renderer.sortingOrder = i_sortingOrder;
		}
		foreach (SpriteRenderer childRenderer in GetComponentsInChildren<SpriteRenderer>(true))
			if (childRenderer != renderer) childRenderer.enabled = false;
		if (m_light != null)
		{
			m_light.lightType = UnityEngine.Rendering.Universal.Light2D.LightType.Point;
			m_light.color = m_modInitialColor;
			// The Core template may be a directional bulb. Portable indicators are
			// authored as radial point lights; FER's source lights use 360 degrees.
			m_light.pointLightInnerAngle = 360f;
			m_light.pointLightOuterAngle = 360f;
			m_light.pointLightInnerRadius = i_innerRadius;
			m_light.pointLightOuterRadius = i_outerRadius;
			m_light.falloffIntensity = i_falloffIntensity;
		}
	}

	public void CopyModRuntimePresentationFrom(LightBulb i_source)
	{
		if (i_source == null) return;
		m_modActivatedSprite = i_source.m_modActivatedSprite;
		m_modInitialColor = i_source.m_modInitialColor;
		m_modActivatedColor = i_source.m_modActivatedColor;
		m_hasModPresentation = i_source.m_hasModPresentation;
	}

	public void ApplyModAction(string i_action)
	{
		if (i_action == "activate") ActivateModPresentation();
		else if (i_action == "on") TurnOn();
		else if (i_action == "off") TurnOff();
		else if (m_isEnabled) TurnOff();
		else TurnOn();
	}

	private void ActivateModPresentation()
	{
		if (!m_hasModPresentation || m_isBroken) return;
		SpriteRenderer renderer = GetComponent<SpriteRenderer>();
		if (renderer != null && m_modActivatedSprite != null) renderer.sprite = m_modActivatedSprite;
		if (m_light != null)
		{
			m_light.enabled = true;
			m_light.color = m_modActivatedColor;
		}
		m_isEnabled = true;
	}

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
		if (m_particleExplode != null) m_particleExplode.Play();
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
