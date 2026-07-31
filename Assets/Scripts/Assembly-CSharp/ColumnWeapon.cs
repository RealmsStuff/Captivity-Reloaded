using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColumnWeapon : MonoBehaviour
{
	[SerializeField]
	private WeaponSlot m_wpnSlotDefault;

	private List<WeaponSlot> m_wpnSlots = new List<WeaponSlot>();

	private WeaponType m_weaponType;

	private int m_sortingOrder;

	private int m_selectIndex;

	private WeaponSlot m_wpnSlotSelected;

	private void Awake()
	{
		m_wpnSlotDefault.gameObject.SetActive(value: false);
	}

	public void Initialize(WeaponType i_gunType)
	{
		m_weaponType = i_gunType;
		switch (i_gunType)
		{
		case WeaponType.Pistol:
			m_sortingOrder = 0;
			GetComponentInChildren<Text>().text = "1";
			break;
		case WeaponType.Smg:
			m_sortingOrder = 1;
			GetComponentInChildren<Text>().text = "2";
			break;
		case WeaponType.Shotgun:
			m_sortingOrder = 2;
			GetComponentInChildren<Text>().text = "3";
			break;
		case WeaponType.Rifle:
			m_sortingOrder = 3;
			GetComponentInChildren<Text>().text = "4";
			break;
		case WeaponType.Special:
			m_sortingOrder = 4;
			GetComponentInChildren<Text>().text = "5";
			break;
		case WeaponType.Usable:
			m_sortingOrder = 5;
			GetComponentInChildren<Text>().text = CommonReferences.Instance.GetManagerInput().GetKeyAssignedToButton(InputButton.DrugSelection).ToString();
			break;
		}
	}

	public void AddWeaponSlot(Weapon i_weapon)
	{
		WeaponSlot weaponSlot = Object.Instantiate(m_wpnSlotDefault, m_wpnSlotDefault.transform.parent);
		weaponSlot.Initialize(i_weapon);
		Vector3 vector = weaponSlot.GetComponent<RectTransform>().anchoredPosition;
		vector.y += (float)m_wpnSlots.Count * weaponSlot.GetComponent<RectTransform>().sizeDelta.y;
		weaponSlot.GetComponent<RectTransform>().anchoredPosition = vector;
		m_wpnSlots.Add(weaponSlot);
		weaponSlot.gameObject.SetActive(value: true);
	}

	public void SelectScrollThroughWeaponSlots()
	{
		DeselectWeaponSlots();
		if (m_selectIndex >= m_wpnSlots.Count - 1)
		{
			m_selectIndex = 0;
		}
		else
		{
			m_selectIndex++;
		}
		m_wpnSlots[m_selectIndex].Select();
		m_wpnSlotSelected = m_wpnSlots[m_selectIndex];
	}

	public void ResetWeaponSlotSelection()
	{
		m_selectIndex = -1;
		m_wpnSlotSelected = null;
	}

	public void DeselectWeaponSlots()
	{
		foreach (WeaponSlot wpnSlot in m_wpnSlots)
		{
			wpnSlot.Deselect();
		}
	}

	public Weapon GetWeaponSelected()
	{
		return m_wpnSlotSelected.GetWeapon();
	}

	public WeaponType GetWeaponType()
	{
		return m_weaponType;
	}

	public int GetSortingOrder()
	{
		return m_sortingOrder;
	}

	public bool GetIsHasWeapon()
	{
		if (m_wpnSlots.Count > 0)
		{
			return true;
		}
		return false;
	}
}
