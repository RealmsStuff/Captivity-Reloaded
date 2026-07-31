using UnityEngine;


public class FuseBox : Interactable
{
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
		GetComponent<SpriteRenderer>().sprite = m_sprFuseBoxOn;
		m_sprRendererLightBulbToTurnOn.sprite = m_sprLightBulbTurnedOn;
		m_lightLightBulb.color = Color.green;
		CommonReferences.Instance.GetManagerAudio().PlayAudioSFX(m_audioActivate);
	}
}
