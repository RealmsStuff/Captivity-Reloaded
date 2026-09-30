using System.Collections.Generic;
using UnityEngine;


public class Vendor : Interactable
{
	[SerializeField]
	private VendorType m_vendorType;

	[SerializeField]
	private bool m_isWorking;

	private List<PickUpable> m_pickUpables = new List<PickUpable>();

	private bool m_isEnabled = true;

	/// <summary>Removes stage-authored state from a vendor copied into a JSON/Tiled stage.</summary>
	public void ConfigureModVendor(VendorType i_vendorType, bool i_isWorking = true)
	{
		ResetModActivationLinks();
		m_vendorType = i_vendorType;
		m_isWorking = i_isWorking;
		m_isEnabled = false;
		m_pickUpables.Clear();
		m_priceToActivate = 0;
		m_isSingleUse = false;
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
		return GetComponentInChildren<SpriteRenderer>(true) != null;
	}

	private void Awake()
	{
		Disable();
	}

	public void InsertItems()
	{
		if (m_vendorType == VendorType.Weapons)
		{
			InsertRandomItems();
		}
		else
		{
			InsertAllItems();
		}
	}

	private void InsertRandomItems()
	{
		m_pickUpables.Clear();
		List<Weapon> allWeapons = CommonReferences.Instance.GetManagerStages().GetStageCurrent().GetAllWeapons();
		int num = 50;
		int num2 = 0;
		if (m_vendorType == VendorType.Weapons)
		{
			int num3 = 4;
			int num4 = 0;
			while (num4 < num3)
			{
				num2++;
				if (num2 >= num)
				{
					break;
				}
				Gun randomGun = Library.Instance.Guns.GetRandomGun();
				if (m_pickUpables.Contains(randomGun) || (bool)CommonReferences.Instance.GetPlayerController().GetInventory().GetPickUpableByName(randomGun.GetName()) || !randomGun.IsMarketable())
				{
					continue;
				}
				bool flag = false;
				for (int i = 0; i < allWeapons.Count; i++)
				{
					if (randomGun.GetName() == allWeapons[i].GetName())
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					m_pickUpables.Add(randomGun);
					num4++;
				}
			}
		}
		else
		{
			int num5 = Random.Range(3, 7);
			int num6 = 0;
			while (num6 < num5)
			{
				Usable component = Library.Instance.Usables.GetRandomUsable().GetComponent<Usable>();
				if (component.IsMarketable())
				{
					m_pickUpables.Add(component);
					num6++;
				}
			}
		}
		SortItemsByPrice();
	}

	private void InsertAllItems()
	{
		m_pickUpables.Clear();
		List<Item> allItems = CommonReferences.Instance.GetManagerStages().GetStageCurrent().GetAllItems();
		List<Gun> list = new List<Gun>();
		for (int i = 0; i < allItems.Count; i++)
		{
			if (allItems[i] is Gun)
			{
				Gun item = (Gun)allItems[i];
				list.Add(item);
			}
		}
		if (m_vendorType == VendorType.Weapons)
		{
			foreach (Gun allGun in Library.Instance.Guns.GetAllGuns())
			{
				if (allGun.GetName() == "Pistol" || (bool)CommonReferences.Instance.GetPlayerController().GetInventory().CheckAndReturnIfHasPickUpable(allGun))
				{
					continue;
				}
				bool flag = false;
				for (int j = 0; j < list.Count; j++)
				{
					if (allGun.GetName() == list[j].GetName())
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					AddPickUpable(allGun);
				}
			}
		}
		else
		{
			foreach (GameObject allUsable in Library.Instance.Usables.GetAllUsables())
			{
				Usable component = allUsable.GetComponent<Usable>();
				if (component.IsMarketable())
				{
					AddPickUpable(component);
				}
			}
		}
		SortItemsByPrice();
	}

	private void SortItemsByPrice()
	{
		bool flag = false;
		while (!flag)
		{
			flag = true;
			for (int i = 0; i < m_pickUpables.Count && i + 1 < m_pickUpables.Count; i++)
			{
				if (m_pickUpables[i].GetValue() > m_pickUpables[i + 1].GetValue())
				{
					PickUpable value = m_pickUpables[i];
					m_pickUpables[i] = m_pickUpables[i + 1];
					m_pickUpables[i + 1] = value;
					flag = false;
				}
			}
		}
	}

	public void Enable()
	{
		if (m_isWorking)
		{
			m_isEnabled = true;
			UnityEngine.Rendering.Universal.Light2D[] componentsInChildren = GetComponentsInChildren<UnityEngine.Rendering.Universal.Light2D>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = true;
			}
		}
	}

	public void Disable()
	{
		m_isEnabled = false;
		UnityEngine.Rendering.Universal.Light2D[] componentsInChildren = GetComponentsInChildren<UnityEngine.Rendering.Universal.Light2D>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = false;
		}
		if (CommonReferences.Instance.GetManagerHud().GetVendorHud().GetIsOpen())
		{
			CommonReferences.Instance.GetManagerHud().GetVendorHud().Hide();
		}
	}

	protected override void HandleActivation(Actor i_initiator, InteractableActivationType i_activationType)
	{
		if (i_activationType == InteractableActivationType.Operator)
		{
			SetIsWorking(!m_isWorking);
		}
		else if (m_isWorking)
		{
			if (m_pickUpables.Count == 0)
			{
				InsertItems();
			}
			if (!m_isEnabled)
			{
				Enable();
			}
			CommonReferences.Instance.GetPlayerController().SetIsForceIgnoreInput(i_isForceIgnoreInput: true);
			CommonReferences.Instance.GetManagerHud().GetVendorHud().OpenVendor(this);
		}
	}

	public void AddPickUpable(PickUpable i_pickUpable)
	{
		m_pickUpables.Add(i_pickUpable);
	}

	public void RemovePickUpable(PickUpable i_pickUpable)
	{
		m_pickUpables.Remove(i_pickUpable);
	}

	public void SetIsWorking(bool i_isWorking)
	{
		m_isWorking = i_isWorking;
		if (m_isWorking && CommonReferences.Instance.GetManagerStages().GetStageCurrent().GetManagerWave()
			.GetStateWave() == StateWave.Wait)
		{
			Enable();
		}
	}

	public List<PickUpable> GetAllPickUpables()
	{
		return m_pickUpables;
	}

	public VendorType GetVendorType()
	{
		return m_vendorType;
	}
}
