using UnityEngine;

public class Door : Interactable
{
	public delegate void DelUnlockDoor(Door i_door);

	[SerializeField]
	protected bool m_isOpen;

	[SerializeField]
	protected PickUpable m_keyToOpen;

	[SerializeField]
	protected Sprite m_sprOpen;

	[SerializeField]
	protected Sprite m_sprClosed;

	[SerializeField]
	protected AudioClip m_audioOpen;

	[SerializeField]
	protected AudioClip m_audioClose;

	protected bool m_isOpenedWithKey;

	public event DelUnlockDoor OnUnlockDoor;

	protected override void Start()
	{
		if (m_keyToOpen != null)
		{
			m_isOpenedWithKey = true;
		}
		if (m_isOpen)
		{
			if ((bool)GetComponent<BoxCollider2D>())
			{
				GetComponent<BoxCollider2D>().enabled = false;
			}
			GetComponent<SpriteRenderer>().sprite = m_sprOpen;
			m_isOpen = true;
		}
		if (m_audioOpen == null)
		{
			m_audioOpen = Resources.Load<AudioClip>("Audio\\DoorOpenDefault");
		}
		if (m_audioClose == null)
		{
			m_audioClose = Resources.Load<AudioClip>("Audio\\DoorCloseDefault");
		}
	}

	public override void Activate(Actor i_initiator, InteractableActivationType i_activationType)
	{
		if (!m_isOpen)
		{
			if (m_isOpenedWithKey && !CommonReferences.Instance.GetPlayerController().GetIsHasPickUpable(m_keyToOpen))
			{
				CommonReferences.Instance.GetManagerHud().GetManagerNotification().CreateNotification("Door is locked", ColorTextNotification.UnlockDoor, i_isContinues: false);
				CommonReferences.Instance.GetManagerAudio().PlayAudioSFX(m_audioClose);
			}
			else
			{
				base.Activate(i_initiator, i_activationType);
			}
		}
	}

	protected override void HandleActivation(Actor i_initiator, InteractableActivationType i_activationType)
	{
		if (m_isOpenedWithKey)
		{
			Open();
			this.OnUnlockDoor?.Invoke(this);
			CommonReferences.Instance.GetManagerHud().GetManagerNotification().CreateNotification("Unlocked door with " + m_keyToOpen.GetName(), ColorTextNotification.UnlockDoor, i_isContinues: false);
			CommonReferences.Instance.GetPlayerController().GetInventory().RemovePickUpable(m_keyToOpen);
		}
		else
		{
			Open();
		}
	}

	public virtual void Open()
	{
		GetComponent<BoxCollider2D>().enabled = false;
		GetComponent<SpriteRenderer>().sprite = m_sprOpen;
		CommonReferences.Instance.GetManagerAudio().PlayAudioSFX(m_audioOpen);
		m_isOpen = true;
	}

	public virtual void Close()
	{
		GetComponent<BoxCollider2D>().enabled = true;
		GetComponent<SpriteRenderer>().sprite = m_sprClosed;
		CommonReferences.Instance.GetManagerAudio().PlayAudioSFX(m_audioClose);
		m_isOpen = false;
	}

	public void OpenOrClose()
	{
		if (m_isOpen)
		{
			Close();
		}
		else
		{
			Open();
		}
	}

	public PickUpable GetKeyToOpen()
	{
		return m_keyToOpen;
	}

	public bool GetIsOpen()
	{
		return m_isOpen;
	}
}
