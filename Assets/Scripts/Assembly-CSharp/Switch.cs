using UnityEngine;

public class Switch : Interactable
{
	public delegate void DelOnSwitchOn();

	public delegate void DelOnSwitchOff();

	[SerializeField]
	private bool m_isOn;

	[SerializeField]
	private Interactable m_interactableToActivate;

	[SerializeField]
	private Sprite m_sprOn;

	[SerializeField]
	private Sprite m_sprOff;

	[SerializeField]
	private AudioClip m_audioSwitchOn;

	[SerializeField]
	private AudioClip m_audioSwitchOff;

	public event DelOnSwitchOn OnSwitchOn;

	public event DelOnSwitchOff OnSwitchOff;

	private new void Start()
	{
		if (m_isOn)
		{
			SwitchOn();
		}
		else
		{
			GetComponent<SpriteRenderer>().sprite = m_sprOff;
		}
	}

	protected override void HandleActivation(Actor i_initiator, InteractableActivationType i_activationType)
	{
		if (m_isOn)
		{
			SwitchOff();
		}
		else
		{
			SwitchOn();
		}
	}

	private void SwitchOn()
	{
		GetComponent<SpriteRenderer>().sprite = m_sprOn;
		if (m_audioSwitchOn != null)
		{
			CommonReferences.Instance.GetManagerAudio().PlayAudioSFX(m_audioSwitchOn);
		}
		m_isOn = true;
		if (this.OnSwitchOn != null)
		{
			this.OnSwitchOn();
		}
		HandleSwitchOn();
	}

	private void SwitchOff()
	{
		GetComponent<SpriteRenderer>().sprite = m_sprOff;
		if (m_audioSwitchOff != null)
		{
			CommonReferences.Instance.GetManagerAudio().PlayAudioSFX(m_audioSwitchOff);
		}
		m_isOn = false;
		if (this.OnSwitchOn != null)
		{
			this.OnSwitchOff();
		}
		HandleSwitchOff();
	}

	public virtual void HandleSwitchOn()
	{
		if (!(m_interactableToActivate == null))
		{
			m_interactableToActivate.Activate(CommonReferences.Instance.GetPlayer(), InteractableActivationType.Operator);
		}
	}

	public virtual void HandleSwitchOff()
	{
		if (!(m_interactableToActivate == null))
		{
			m_interactableToActivate.Activate(CommonReferences.Instance.GetPlayer(), InteractableActivationType.Operator);
		}
	}
}
