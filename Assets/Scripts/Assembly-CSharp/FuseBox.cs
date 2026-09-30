using UnityEngine;


public class FuseBox : Interactable
{
	public void ConfigureModFuseBox(SpriteRenderer i_indicator, UnityEngine.Rendering.Universal.Light2D i_indicatorLight)
	{
		ResetModActivationLinks();
		m_sprRendererLightBulbToTurnOn = i_indicator;
		m_lightLightBulb = i_indicatorLight;
		m_priceToActivate = 0;
		m_isSingleUse = true;
		m_isContinuesActivation = false;
		m_isCanBeUsedToActivate = true;
		m_isCanBeTouchedToActivate = false;
		m_isCanBeShotToActivate = false;
		m_isCanBeActivatedByNPC = false;
		m_isHideNotificationInteract = false;
		m_isUnInteractable = false;
	}

	public bool IsUsableModTemplate()
	{
		return m_sprFuseBoxOn != null && m_sprRendererLightBulbToTurnOn != null
			&& m_sprLightBulbTurnedOn != null && m_lightLightBulb != null
			&& GetComponent<SpriteRenderer>() != null;
	}

	public void SuppressInheritedIndicatorForMod()
	{
		// The portable fuse indicator supplies both its sprite and its light.
		// The original indicator is a sibling of this fuse box, so disabling
		// the inherited interactable alone leaves its red light behind.
		if (m_sprRendererLightBulbToTurnOn != null)
			m_sprRendererLightBulbToTurnOn.gameObject.SetActive(false);
		if (m_lightLightBulb != null)
			m_lightLightBulb.gameObject.SetActive(false);
	}

	[SerializeField]
	private Sprite m_sprFuseBoxOn;

	[SerializeField]
	private SpriteRenderer m_sprRendererLightBulbToTurnOn;

	[SerializeField]
	private Sprite m_sprLightBulbTurnedOn;

	[SerializeField]
	private UnityEngine.Rendering.Universal.Light2D m_lightLightBulb;

	[SerializeField]
	private AudioClip m_audioActivate;

	protected override void HandleActivation(Actor i_initiator, InteractableActivationType i_activationType)
	{
		SpriteRenderer renderer = GetComponent<SpriteRenderer>();
		if (renderer != null) renderer.sprite = m_sprFuseBoxOn;
		if (m_sprRendererLightBulbToTurnOn != null) m_sprRendererLightBulbToTurnOn.sprite = m_sprLightBulbTurnedOn;
		if (m_lightLightBulb != null) m_lightLightBulb.color = Color.green;
		if (m_audioActivate != null) CommonReferences.Instance.GetManagerAudio().PlayAudioSFX(m_audioActivate);
	}
}
