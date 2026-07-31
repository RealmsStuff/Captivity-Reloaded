using System.Collections.Generic;
using UnityEngine;

public class ManagerNotification : MonoBehaviour
{
	[SerializeField]
	private Notification m_notificationDefault;

	[SerializeField]
	private GameObject m_parentObject;

	[SerializeField]
	private Color m_colorPickupWeapon;

	[SerializeField]
	private Color m_colorPickupAmmo;

	[SerializeField]
	private Color m_colorPickupKey;

	[SerializeField]
	private Color m_colorPickupUsable;

	[SerializeField]
	private Color m_colorUnlockDoor;

	[SerializeField]
	private Color m_colorOther;

	private List<Notification> m_notifications = new List<Notification>();

	private AudioClip m_audioNotification;

	private void Start()
	{
		m_audioNotification = Resources.Load<AudioClip>("Audio/Notification");
	}

	private void ListenToNotificationCreate()
	{
	}

	public void CreateNotificationPickUp(PickUpable i_pickUpable)
	{
		Color i_colorText = m_colorOther;
		string text = "Picked up: ";
		if (i_pickUpable.GetAmount() > 1)
		{
			text = text + "(" + i_pickUpable.GetAmount() + "x)  ";
		}
		text += i_pickUpable.GetName();
		if (i_pickUpable is Gun)
		{
			i_colorText = m_colorPickupWeapon;
		}
		if (i_pickUpable is Usable)
		{
			i_colorText = m_colorPickupUsable;
		}
		CreateNotification(text, i_colorText, i_isContinues: false);
	}

	public Notification CreateNotification(string i_text, ColorTextNotification i_colorText, bool i_isContinues)
	{
		Notification notification = Object.Instantiate(m_notificationDefault, m_notificationDefault.transform.parent);
		notification.gameObject.SetActive(value: true);
		int num = 0;
		bool flag = false;
		for (int i = 0; i < 10; i++)
		{
			foreach (Notification notification2 in m_notifications)
			{
				if (notification2.GetNumPosY() == i)
				{
					flag = false;
					break;
				}
				flag = true;
			}
			if (flag)
			{
				num = i;
				break;
			}
		}
		Color i_colorText2 = Color.black;
		switch (i_colorText)
		{
		case ColorTextNotification.Other:
			i_colorText2 = m_colorOther;
			break;
		case ColorTextNotification.Equippable:
			i_colorText2 = m_colorPickupWeapon;
			break;
		case ColorTextNotification.Ammo:
			i_colorText2 = m_colorPickupAmmo;
			break;
		case ColorTextNotification.Usable:
			i_colorText2 = m_colorPickupUsable;
			break;
		case ColorTextNotification.Key:
			i_colorText2 = m_colorPickupKey;
			break;
		case ColorTextNotification.UnlockDoor:
			i_colorText2 = m_colorUnlockDoor;
			break;
		}
		notification.Initialize(i_text, i_colorText2, num, i_isContinues);
		Vector2 anchoredPosition = notification.GetComponent<RectTransform>().anchoredPosition;
		anchoredPosition.y += notification.GetComponent<RectTransform>().sizeDelta.y * (float)num;
		notification.GetComponent<RectTransform>().anchoredPosition = anchoredPosition;
		m_notifications.Add(notification);
		return notification;
	}

	private Notification CreateNotification(string i_text, Color i_colorText, bool i_isContinues)
	{
		Notification notification = Object.Instantiate(m_notificationDefault, m_notificationDefault.transform.parent);
		notification.gameObject.SetActive(value: true);
		int num = 0;
		bool flag = false;
		for (int i = 0; i < 10; i++)
		{
			foreach (Notification notification2 in m_notifications)
			{
				if (notification2.GetNumPosY() == i)
				{
					flag = false;
					break;
				}
				flag = true;
			}
			if (flag)
			{
				num = i;
				break;
			}
		}
		notification.Initialize(i_text, i_colorText, num, i_isContinues);
		Vector2 anchoredPosition = notification.GetComponent<RectTransform>().anchoredPosition;
		anchoredPosition.y += notification.GetComponent<RectTransform>().sizeDelta.y * (float)num;
		notification.GetComponent<RectTransform>().anchoredPosition = anchoredPosition;
		m_notifications.Add(notification);
		return notification;
	}

	private void CreateNotificationUse(Interactable i_interactable)
	{
		if (i_interactable is Door)
		{
			CreateNotificationUnlockDoor((Door)i_interactable);
			return;
		}
		string i_text = "You used: " + i_interactable.name;
		CreateNotification(i_text, m_colorOther, i_isContinues: false);
	}

	private void CreateNotificationUnlockDoor(Door i_door)
	{
		string text = "";
		bool flag = false;
		bool isOpen = i_door.GetIsOpen();
		if (i_door.GetKeyToOpen() != null)
		{
			flag = true;
		}
		if (!isOpen && flag)
		{
			if (CommonReferences.Instance.GetPlayer().GetIsHasPickUpable(i_door.GetKeyToOpen()))
			{
				text = text + "Unlocked door with " + i_door.GetKeyToOpen().GetName();
				CreateNotification(text, m_colorUnlockDoor, i_isContinues: false);
				return;
			}
			text = ((!flag) ? (text + "It's opened elsewhere") : (text + "It's locked"));
		}
		CreateNotification(text, m_colorOther, i_isContinues: false);
	}

	public void Show()
	{
		m_parentObject.SetActive(value: true);
	}

	public void Hide()
	{
		DestroyAllNotifications();
		m_parentObject.SetActive(value: false);
	}

	public void DestroyNotification(Notification i_notification)
	{
		Object.Destroy(i_notification.gameObject);
		m_notifications.Remove(i_notification);
	}

	public void DestroyAllNotifications()
	{
		foreach (Notification notification in m_notifications)
		{
			Object.Destroy(notification.gameObject);
		}
		m_notifications.Clear();
	}
}
