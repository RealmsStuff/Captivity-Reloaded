using System.Collections.Generic;
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

	private readonly List<Door> m_modDoors = new List<Door>();

	private readonly List<DoorRoller> m_modRollerDoors = new List<DoorRoller>();

	public event DelOnSwitchOn OnSwitchOn;

	public event DelOnSwitchOff OnSwitchOff;

	public void ConfigureModDoorTargets(IEnumerable<Door> i_doors, IEnumerable<DoorRoller> i_rollerDoors)
	{
		ResetModActivationLinks();
		m_interactableToActivate = null;
		m_isOn = false;
		m_priceToActivate = 0;
		m_isSingleUse = false;
		m_isContinuesActivation = false;
		m_isCanBeUsedToActivate = true;
		m_isCanBeTouchedToActivate = false;
		m_isCanBeShotToActivate = false;
		m_isCanBeActivatedByNPC = false;
		m_isHideNotificationInteract = false;
		m_isUnInteractable = false;
		m_modDoors.Clear();
		m_modRollerDoors.Clear();
		if (i_doors != null) m_modDoors.AddRange(i_doors);
		if (i_rollerDoors != null) m_modRollerDoors.AddRange(i_rollerDoors);
	}

	public bool IsUsableModTemplate()
	{
		return m_sprOn != null && m_sprOff != null && GetComponent<SpriteRenderer>() != null;
	}

	public void ConfigureModVisuals(Sprite i_on, Sprite i_off)
	{
		if (i_on != null) m_sprOn = i_on;
		if (i_off != null) m_sprOff = i_off;
	}

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
		ToggleModDoors();
	}

	public virtual void HandleSwitchOff()
	{
		if (!(m_interactableToActivate == null))
		{
			m_interactableToActivate.Activate(CommonReferences.Instance.GetPlayer(), InteractableActivationType.Operator);
		}
		ToggleModDoors();
	}

	private void ToggleModDoors()
	{
		foreach (Door door in m_modDoors)
			if (door != null) door.OpenOrClose();
		foreach (DoorRoller door in m_modRollerDoors)
			if (door != null) door.OpenOrClose();
	}
}
