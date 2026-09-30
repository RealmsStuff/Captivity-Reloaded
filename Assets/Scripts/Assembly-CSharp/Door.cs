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

	public void ConfigureModDoor(bool i_initiallyOpen)
	{
		ConfigureModDoor(i_initiallyOpen, 0, false, true, null, null, null);
	}

	public void ConfigureModDoor(bool i_initiallyOpen, int i_price, bool i_singleUse,
		bool i_initiallyInteractable, PickUpable i_keyToOpen, Sprite i_openSprite, Sprite i_closedSprite)
	{
		ResetModActivationLinks();
		m_priceToActivate = i_price;
		m_isSingleUse = i_singleUse;
		m_isContinuesActivation = false;
		m_isCanBeUsedToActivate = true;
		m_isCanBeTouchedToActivate = false;
		m_isCanBeShotToActivate = false;
		m_isCanBeActivatedByNPC = false;
		m_isHideNotificationInteract = false;
		m_isUnInteractable = !i_initiallyInteractable;
		m_keyToOpen = i_keyToOpen;
		m_isOpenedWithKey = i_keyToOpen != null;
		if (i_openSprite != null) m_sprOpen = i_openSprite;
		if (i_closedSprite != null) m_sprClosed = i_closedSprite;
		m_isOpen = i_initiallyOpen;
		ApplyModVisualState();
	}

	private void ApplyModVisualState()
	{
		BoxCollider2D doorCollider = GetComponent<BoxCollider2D>();
		if (doorCollider != null) doorCollider.enabled = !m_isOpen;
		SpriteRenderer renderer = GetComponent<SpriteRenderer>();
		Sprite stateSprite = m_isOpen ? m_sprOpen : m_sprClosed;
		if (renderer != null && stateSprite != null) renderer.sprite = stateSprite;
	}

	public bool IsUsableModTemplate()
	{
		return m_sprOpen != null && m_sprClosed != null && GetComponent<SpriteRenderer>() != null
			&& GetComponent<BoxCollider2D>() != null;
	}

	protected override void Start()
	{
		if (m_keyToOpen != null)
		{
			m_isOpenedWithKey = true;
		}
		ApplyModVisualState();
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
