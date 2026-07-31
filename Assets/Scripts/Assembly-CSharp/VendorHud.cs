using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VendorHud : MonoBehaviour
{
	private Vendor m_vendorCurrent;

	private VendorItem m_vendorItemSelected;

	[SerializeField]
	private GameObject m_vendorWindow;

	[SerializeField]
	private Text m_txtMoneyPlayer;

	[SerializeField]
	private Text m_txtWeightPlayer;

	[SerializeField]
	private GameObject m_sectionWeapon;

	[SerializeField]
	private GameObject m_sectionUsable;

	[SerializeField]
	private VendorItem m_itemDefaultVendor;

	[SerializeField]
	private VendorItem m_itemDefaultPlayer;

	[SerializeField]
	private Image m_imgIconItem;

	[SerializeField]
	private Text m_txtNameItem;

	[SerializeField]
	private Text m_txtEffectUsableDefault;

	private List<Text> m_txtsEffectUsable = new List<Text>();

	[SerializeField]
	private Text m_txtWeightWeapon;

	[SerializeField]
	private Text m_txtPriceWeapon;

	[SerializeField]
	private Text m_txtLabelPriceWeapon;

	[SerializeField]
	private UnityEngine.UI.Button m_btnBuySellWeapon;

	[SerializeField]
	private GameObject m_optionsAmmo;

	[SerializeField]
	private Text m_txtAmmo;

	[SerializeField]
	private UnityEngine.UI.Button m_btnBuyMag;

	[SerializeField]
	private UnityEngine.UI.Button m_btnBuyFill;

	[SerializeField]
	private Text m_txtPriceUsable;

	[SerializeField]
	private Text m_txtLabelPriceUsable;

	[SerializeField]
	private UnityEngine.UI.Button m_btnBuySellUsable;

	private List<VendorItem> m_itemsVendor = new List<VendorItem>();

	private List<VendorItem> m_itemsPlayer = new List<VendorItem>();

	private RectTransform m_windowRect;

	private RectTransform m_windowContainerRect;

	private Vector2 m_windowContainerSize = new Vector2(-1f, -1f);

	private bool m_isLayoutConfigured;

	private void Awake()
	{
		ConfigureLayout();
		m_vendorWindow.SetActive(value: false);
	}

	private void LateUpdate()
	{
		if (m_vendorWindow.activeSelf)
		{
			ConfigureLayout();
		}
		if (m_vendorWindow.activeSelf && Input.GetKeyDown(KeyCode.Escape))
		{
			Hide();
		}
	}

	public void OpenVendor(Vendor i_vendor)
	{
		ConfigureLayout();
		m_vendorCurrent = i_vendor;
		ClearData();
		HideSections();
		FillLists();
		LoadPlayerData();
		Show();
	}

	private void ConfigureLayout()
	{
		if (m_windowRect == null)
		{
			m_windowRect = m_vendorWindow.GetComponent<RectTransform>();
			m_windowContainerRect = (m_windowRect != null) ? (m_windowRect.parent as RectTransform) : null;
		}
		if (m_windowRect == null || m_windowContainerRect == null)
		{
			return;
		}
		if (!m_isLayoutConfigured)
		{
			RectTransform component = m_btnBuySellWeapon.GetComponent<RectTransform>();
			ConfigureRect(component, new Vector2(0.5f, 0.5f), new Vector2(0f, 58f), new Vector2(322f, 62f));
			RectTransform component2 = m_btnBuySellUsable.GetComponent<RectTransform>();
			ConfigureRect(component2, new Vector2(0.5f, 0.5f), new Vector2(0f, -169f), new Vector2(322f, 48f));
			RectTransform component3 = m_optionsAmmo.GetComponent<RectTransform>();
			ConfigureRect(component3, new Vector2(0.5f, 0.5f), new Vector2(0f, -83f), new Vector2(322f, 220f));
			RectTransform component4 = m_btnBuyMag.GetComponent<RectTransform>();
			ConfigureRect(component4, Vector2.zero, new Vector2(78f, 48f), new Vector2(156f, 96f));
			RectTransform component5 = m_btnBuyFill.GetComponent<RectTransform>();
			ConfigureRect(component5, new Vector2(1f, 0f), new Vector2(-78f, 48f), new Vector2(156f, 96f));
			Text componentInChildren = m_btnBuyMag.GetComponentInChildren<Text>(includeInactive: true);
			if (componentInChildren != null)
			{
				componentInChildren.text = "Magazine";
			}
			Text componentInChildren2 = m_btnBuyFill.GetComponentInChildren<Text>(includeInactive: true);
			if (componentInChildren2 != null)
			{
				componentInChildren2.text = "Refill";
			}
			UnityEngine.UI.Button[] componentsInChildren = m_vendorWindow.GetComponentsInChildren<UnityEngine.UI.Button>(includeInactive: true);
			foreach (UnityEngine.UI.Button button in componentsInChildren)
			{
				if (button.gameObject.name != "btn_close")
				{
					continue;
				}
				RectTransform component6 = button.GetComponent<RectTransform>();
				ConfigureRect(component6, Vector2.one, new Vector2(-24f, -24f), new Vector2(48f, 48f));
				component6.SetAsLastSibling();
				break;
			}
			m_isLayoutConfigured = true;
		}
		Vector2 size = m_windowContainerRect.rect.size;
		if (m_windowContainerSize == size)
		{
			return;
		}
		float num = Mathf.Max(size.x - 64f, 1f) / 764f;
		float num2 = Mathf.Max(size.y - 64f, 1f) / 840f;
		float num3 = Mathf.Min(1f, Mathf.Min(num, num2));
		m_windowRect.anchorMin = new Vector2(0.5f, 0.5f);
		m_windowRect.anchorMax = new Vector2(0.5f, 0.5f);
		m_windowRect.pivot = new Vector2(0.5f, 0.5f);
		m_windowRect.anchoredPosition = Vector2.zero;
		m_windowRect.sizeDelta = new Vector2(764f, 840f);
		m_windowRect.localScale = new Vector3(num3, num3, 1f);
		m_windowContainerSize = size;
	}

	private static void ConfigureRect(RectTransform i_rectTransform, Vector2 i_anchor, Vector2 i_position, Vector2 i_size)
	{
		i_rectTransform.anchorMin = i_anchor;
		i_rectTransform.anchorMax = i_anchor;
		i_rectTransform.pivot = new Vector2(0.5f, 0.5f);
		i_rectTransform.anchoredPosition = i_position;
		i_rectTransform.sizeDelta = i_size;
		i_rectTransform.localScale = Vector3.one;
	}

	private void LoadPlayerData()
	{
		m_txtMoneyPlayer.text = CommonReferences.Instance.GetPlayerController().GetInventory().GetMoney() + "$";
		m_txtWeightPlayer.text = CommonReferences.Instance.GetPlayerController().GetInventory().GetEncumbrance() + "/" + CommonReferences.Instance.GetPlayerController().GetInventory().GetRoom();
	}

	private void ClearData()
	{
		foreach (VendorItem item in m_itemsVendor)
		{
			Object.Destroy(item.gameObject);
		}
		m_itemsVendor.Clear();
		foreach (VendorItem item2 in m_itemsPlayer)
		{
			Object.Destroy(item2.gameObject);
		}
		m_itemsPlayer.Clear();
		foreach (Text item3 in m_txtsEffectUsable)
		{
			Object.Destroy(item3.gameObject);
		}
		m_txtsEffectUsable.Clear();
		m_itemDefaultVendor.gameObject.SetActive(value: false);
		m_itemDefaultPlayer.gameObject.SetActive(value: false);
		m_txtEffectUsableDefault.gameObject.SetActive(value: false);
		m_imgIconItem.sprite = null;
		m_imgIconItem.color = new Color(1f, 1f, 1f, 0f);
		m_txtNameItem.text = "-";
		m_txtWeightWeapon.text = "-";
		m_txtPriceWeapon.text = "-";
		m_btnBuySellWeapon.interactable = false;
		m_btnBuySellUsable.interactable = false;
		m_txtAmmo.text = "-";
		m_btnBuyMag.interactable = false;
		m_btnBuyFill.interactable = false;
	}

	private void HideSections()
	{
		m_sectionWeapon.SetActive(value: false);
		m_sectionUsable.SetActive(value: false);
		m_optionsAmmo.SetActive(value: false);
		m_btnBuySellWeapon.interactable = false;
		m_btnBuySellWeapon.gameObject.SetActive(value: false);
		foreach (Text item in m_txtsEffectUsable)
		{
			Object.Destroy(item.gameObject);
		}
		m_txtsEffectUsable.Clear();
	}

	private void FillLists()
	{
		FillListVendor();
		FillListPlayer();
	}

	private void FillListVendor()
	{
		foreach (PickUpable allPickUpable in m_vendorCurrent.GetAllPickUpables())
		{
			CreateNewItemVendor(allPickUpable);
		}
	}

	private void CreateNewItemVendor(PickUpable i_pickUpable)
	{
		VendorItem vendorItem = Object.Instantiate(m_itemDefaultVendor, m_itemDefaultVendor.transform.parent);
		vendorItem.Initialize(i_pickUpable, i_isOwnerVendor: true);
		vendorItem.gameObject.SetActive(value: true);
		m_itemsVendor.Add(vendorItem);
	}

	private void FillListPlayer()
	{
		if (m_vendorCurrent.GetVendorType() == VendorType.Weapons)
		{
			foreach (Gun allGun in CommonReferences.Instance.GetPlayerController().GetInventory().GetAllGuns())
			{
				if (allGun.IsMarketable())
				{
					CreateNewItemPlayer(allGun);
				}
			}
			return;
		}
		foreach (Usable allUsable in CommonReferences.Instance.GetPlayerController().GetInventory().GetAllUsables())
		{
			if (allUsable.GetIsCanDrop())
			{
				CreateNewItemPlayer(allUsable);
			}
		}
	}

	private void CreateNewItemPlayer(PickUpable i_pickUpable)
	{
		VendorItem vendorItem = Object.Instantiate(m_itemDefaultPlayer, m_itemDefaultPlayer.transform.parent);
		vendorItem.Initialize(i_pickUpable, i_isOwnerVendor: false);
		RectTransform component = vendorItem.GetComponent<RectTransform>();
		Vector3 vector = component.anchoredPosition;
		vector.y -= (float)m_itemsPlayer.Count * component.sizeDelta.y + (float)(m_itemsPlayer.Count * 10);
		vendorItem.GetComponent<RectTransform>().anchoredPosition = vector;
		vendorItem.gameObject.SetActive(value: true);
		m_itemsPlayer.Add(vendorItem);
	}

	public void SelectItem(VendorItem i_item)
	{
		HideSections();
		m_vendorItemSelected = i_item;
		m_imgIconItem.sprite = m_vendorItemSelected.GetPickUpable().GetSpriteIcon();
		m_imgIconItem.color = new Color(1f, 1f, 1f, 1f);
		m_txtNameItem.text = i_item.GetPickUpable().GetName();
		if (m_vendorItemSelected.GetPickUpable() is Gun)
		{
			m_sectionWeapon.SetActive(value: true);
			Gun i_pickUpable = (Gun)m_vendorItemSelected.GetPickUpable();
			m_btnBuySellWeapon.gameObject.SetActive(value: true);
			m_btnBuySellWeapon.interactable = true;
			m_txtWeightWeapon.text = m_vendorItemSelected.GetPickUpable().GetWeight().ToString();
			if (i_item.GetIsOwnerVendor())
			{
				m_txtPriceWeapon.text = m_vendorItemSelected.GetPickUpable().GetValue() + "$";
				m_txtLabelPriceWeapon.text = "Price";
				if (!CommonReferences.Instance.GetPlayerController().GetInventory().IsHasRoomForPickUpable(i_pickUpable))
				{
					m_btnBuySellWeapon.GetComponentInChildren<Text>().text = "Too heavy";
					m_btnBuySellWeapon.interactable = false;
					return;
				}
				if (CommonReferences.Instance.GetPlayerController().GetInventory().GetMoney() < m_vendorItemSelected.GetPickUpable().GetValue())
				{
					m_btnBuySellWeapon.GetComponentInChildren<Text>().text = "Buy (Need " + m_vendorItemSelected.GetPickUpable().GetValue() + "$)";
					m_btnBuySellWeapon.interactable = false;
					return;
				}
				m_btnBuySellWeapon.GetComponentInChildren<Text>().text = "Buy";
			}
			else
			{
				m_optionsAmmo.SetActive(value: true);
				int num = m_vendorItemSelected.GetPickUpable().GetValue() / 4;
				m_txtPriceWeapon.text = num + "$";
				m_txtLabelPriceWeapon.text = "Sell price (-75%)";
				UpdateAmmoValues();
				CommonReferences.Instance.GetPlayerController().GetInventory().GetMoney();
				m_btnBuySellWeapon.GetComponentInChildren<Text>().text = "Sell";
			}
		}
		if (!(m_vendorItemSelected.GetPickUpable() is Usable))
		{
			return;
		}
		m_sectionUsable.SetActive(value: true);
		Usable i_usable = (Usable)m_vendorItemSelected.GetPickUpable();
		FillUsableDescription(i_usable);
		m_btnBuySellUsable.gameObject.SetActive(value: true);
		m_btnBuySellUsable.interactable = true;
		if (i_item.GetIsOwnerVendor())
		{
			m_txtPriceUsable.text = m_vendorItemSelected.GetPickUpable().GetValue() + "$";
			m_txtLabelPriceUsable.text = "Price";
			if (CommonReferences.Instance.GetPlayerController().GetInventory().GetMoney() >= m_vendorItemSelected.GetPickUpable().GetValue())
			{
				m_btnBuySellUsable.GetComponentInChildren<Text>().text = "Buy";
				m_btnBuySellUsable.interactable = true;
			}
			else
			{
				m_btnBuySellUsable.GetComponentInChildren<Text>().text = "Buy (Need " + m_vendorItemSelected.GetPickUpable().GetValue() + "$)";
				m_btnBuySellUsable.interactable = false;
			}
			if (CommonReferences.Instance.GetPlayerController().GetInventory().GetAllUsables()
				.Count >= 6)
			{
				m_btnBuySellUsable.GetComponentInChildren<Text>().text = "Max 6 drugs";
				m_btnBuySellUsable.interactable = false;
			}
		}
		else
		{
			int num2 = m_vendorItemSelected.GetPickUpable().GetValue() / 4;
			m_txtPriceUsable.text = num2 + "$";
			m_txtLabelPriceUsable.text = "Sell price (-75%)";
			CommonReferences.Instance.GetPlayerController().GetInventory().GetMoney();
			m_btnBuySellUsable.GetComponentInChildren<Text>().text = "Sell";
		}
	}

	private void UpdateAmmoValues()
	{
		Gun gun = (Gun)m_vendorItemSelected.GetPickUpable();
		m_txtAmmo.text = gun.GetAmmoLeftTotal() + "/" + gun.GetAmmoMax();
		GetValueMag(gun);
		if (GetValueFill(gun) == 0)
		{
			m_btnBuyMag.GetComponentsInChildren<Text>()[1].text = "Full";
			m_btnBuyFill.GetComponentsInChildren<Text>()[1].text = "Full";
		}
		else
		{
			m_btnBuyMag.GetComponentsInChildren<Text>()[1].text = GetValueMag(gun).ToString();
			m_btnBuyFill.GetComponentsInChildren<Text>()[1].text = GetValueFill(gun).ToString();
		}
		int money = CommonReferences.Instance.GetPlayerController().GetInventory().GetMoney();
		if (money >= GetValueMag(gun) && gun.GetAmmoLeftTotal() < gun.GetAmmoMax())
		{
			m_btnBuyMag.interactable = true;
		}
		else
		{
			m_btnBuyMag.interactable = false;
		}
		if (money >= GetValueFill(gun) && gun.GetAmmoLeftTotal() < gun.GetAmmoMax())
		{
			m_btnBuyFill.interactable = true;
		}
		else
		{
			m_btnBuyFill.interactable = false;
		}
	}

	private int GetValueBullet(Gun i_gun)
	{
		int num = i_gun.GetValue() / i_gun.GetAmmoMax();
		num /= 2;
		if (num == 0)
		{
			num = 1;
		}
		return num;
	}

	private int GetValueMag(Gun i_gun)
	{
		int valueBullet = GetValueBullet(i_gun);
		int num = i_gun.GetAmmoMax() - i_gun.GetAmmoLeftTotal();
		if (num < i_gun.GetAmmoMagazineMax())
		{
			return valueBullet * num;
		}
		return valueBullet * i_gun.GetAmmoMagazineMax();
	}

	private int GetValueFill(Gun i_gun)
	{
		int valueBullet = GetValueBullet(i_gun);
		int num = i_gun.GetAmmoMax() - i_gun.GetAmmoLeftTotal();
		return valueBullet * num;
	}

	private void FillUsableDescription(Usable i_usable)
	{
		foreach (string descriptionsGoodEffect in i_usable.GetDescriptionsGoodEffects())
		{
			Text text = Object.Instantiate(m_txtEffectUsableDefault, m_txtEffectUsableDefault.transform.parent);
			text.text = descriptionsGoodEffect;
			text.color = Color.green;
			Vector3 vector = m_txtEffectUsableDefault.GetComponent<RectTransform>().anchoredPosition;
			vector.y -= m_txtEffectUsableDefault.GetComponent<RectTransform>().rect.height * (float)m_txtsEffectUsable.Count;
			text.GetComponent<RectTransform>().anchoredPosition = vector;
			text.gameObject.SetActive(value: true);
			m_txtsEffectUsable.Add(text);
		}
		foreach (string descriptionsBadEffect in i_usable.GetDescriptionsBadEffects())
		{
			Text text2 = Object.Instantiate(m_txtEffectUsableDefault, m_txtEffectUsableDefault.transform.parent);
			text2.text = descriptionsBadEffect;
			text2.color = Color.red;
			Vector3 vector2 = m_txtEffectUsableDefault.GetComponent<RectTransform>().anchoredPosition;
			vector2.y -= m_txtEffectUsableDefault.GetComponent<RectTransform>().rect.height * (float)m_txtsEffectUsable.Count;
			text2.GetComponent<RectTransform>().anchoredPosition = vector2;
			text2.gameObject.SetActive(value: true);
			m_txtsEffectUsable.Add(text2);
		}
	}

	public void BuySell()
	{
		if (m_vendorItemSelected.GetIsOwnerVendor())
		{
			BuyItem();
		}
		else
		{
			Sell();
		}
	}

	public void BuyItem()
	{
		CommonReferences.Instance.GetPlayerController().LoseMoney(m_vendorItemSelected.GetPickUpable().GetValue());
		PickUpable pickUpable = Object.Instantiate(m_vendorItemSelected.GetPickUpable());
		if (pickUpable is Gun)
		{
			((Gun)pickUpable).FillEntireGun();
		}
		CommonReferences.Instance.GetPlayer().PickUp(pickUpable, i_isDuplicate: false);
		m_vendorCurrent.RemovePickUpable(m_vendorItemSelected.GetPickUpable());
		m_vendorItemSelected = null;
		ClearData();
		HideSections();
		FillLists();
		LoadPlayerData();
		CommonReferences.Instance.GetManagerAudio().PlayAudioSFX(Resources.Load<AudioClip>("Audio/Buy"));
	}

	public void BuyMag()
	{
		Gun gun = (Gun)m_vendorItemSelected.GetPickUpable();
		CommonReferences.Instance.GetPlayerController().LoseMoney(GetValueMag(gun));
		gun.AddAmmoIncludingMagazine(gun.GetAmmoMagazineMax());
		UpdateAmmoValues();
		CommonReferences.Instance.GetManagerAudio().PlayAudioSFX(Resources.Load<AudioClip>("Audio/BuyAmmo"));
	}

	public void BuyFill()
	{
		Gun gun = (Gun)m_vendorItemSelected.GetPickUpable();
		CommonReferences.Instance.GetPlayerController().LoseMoney(GetValueFill(gun));
		gun.FillEntireGun();
		UpdateAmmoValues();
		CommonReferences.Instance.GetManagerAudio().PlayAudioSFX(Resources.Load<AudioClip>("Audio/BuyAmmo"));
	}

	public void Sell()
	{
		CommonReferences.Instance.GetPlayerController().GainMoney(m_vendorItemSelected.GetPickUpable().GetValue() / 4);
		CommonReferences.Instance.GetPlayer().DropPickupAble(m_vendorItemSelected.GetPickUpable());
		Object.Destroy(m_vendorItemSelected.GetPickUpable().gameObject);
		ClearData();
		HideSections();
		FillLists();
		LoadPlayerData();
		CommonReferences.Instance.GetManagerAudio().PlayAudioSFX(Resources.Load<AudioClip>("Audio/Sell"));
	}

	public void Show()
	{
		m_vendorWindow.SetActive(value: true);
		Time.timeScale = 0f;
	}

	public void Hide()
	{
		m_vendorWindow.SetActive(value: false);
		CommonReferences.Instance.GetPlayerController().SetIsForceIgnoreInput(i_isForceIgnoreInput: false);
		Time.timeScale = 1f;
	}

	public bool GetIsOpen()
	{
		return m_vendorWindow.activeInHierarchy;
	}
}
