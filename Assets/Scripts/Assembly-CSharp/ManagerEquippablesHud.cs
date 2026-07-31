using System.Collections.Generic;
using UnityEngine;

public class ManagerEquippablesHud : MonoBehaviour
{
	[SerializeField]
	private ColumnWeapon m_columnDefault;

	private List<ColumnWeapon> m_columns = new List<ColumnWeapon>();

	private ColumnWeapon m_columnSelected;

	private bool m_isShowing;

	private bool m_isListenToInput;

	private void Awake()
	{
		m_columnDefault.gameObject.SetActive(value: false);
	}

	private void Update()
	{
		if (m_isListenToInput)
		{
			if (Input.GetKeyDown(KeyCode.Alpha1))
			{
				Select(WeaponType.Pistol);
			}
			if (Input.GetKeyDown(KeyCode.Alpha2))
			{
				Select(WeaponType.Smg);
			}
			if (Input.GetKeyDown(KeyCode.Alpha3))
			{
				Select(WeaponType.Shotgun);
			}
			if (Input.GetKeyDown(KeyCode.Alpha4))
			{
				Select(WeaponType.Rifle);
			}
			if (Input.GetKeyDown(KeyCode.Alpha5))
			{
				Select(WeaponType.Special);
			}
			if (CommonReferences.Instance.GetManagerInput().IsButtonDown(InputButton.DrugSelection))
			{
				Select(WeaponType.Usable);
			}
			if (CommonReferences.Instance.GetManagerInput().IsButtonDown(InputButton.Fire))
			{
				PickWeaponSlot();
			}
		}
	}

	private void BuildColumns()
	{
		CreateAllColumns();
		foreach (Weapon allEquippable in CommonReferences.Instance.GetPlayerController().GetInventory().GetAllEquippables())
		{
			CreateOrUpdateColumn(allEquippable);
		}
		SortColumns();
	}

	private void CreateAllColumns()
	{
		CreateNewColumn(WeaponType.Pistol);
		CreateNewColumn(WeaponType.Smg);
		CreateNewColumn(WeaponType.Shotgun);
		CreateNewColumn(WeaponType.Rifle);
		CreateNewColumn(WeaponType.Special);
		CreateNewColumn(WeaponType.Usable);
	}

	private void CreateOrUpdateColumn(Weapon i_weapon)
	{
		foreach (ColumnWeapon column in m_columns)
		{
			if (column.GetWeaponType() == i_weapon.GetWeaponType())
			{
				column.AddWeaponSlot(i_weapon);
				return;
			}
		}
		CreateNewColumn(i_weapon.GetWeaponType());
		CreateOrUpdateColumn(i_weapon);
	}

	private void CreateNewColumn(WeaponType i_weaponType)
	{
		ColumnWeapon columnWeapon = Object.Instantiate(m_columnDefault, m_columnDefault.transform.parent);
		columnWeapon.Initialize(i_weaponType);
		m_columns.Add(columnWeapon);
		columnWeapon.gameObject.SetActive(value: true);
	}

	private void SortColumns()
	{
		float num = m_columnDefault.GetComponent<RectTransform>().anchoredPosition.x - (float)m_columns.Count * m_columnDefault.GetComponent<RectTransform>().sizeDelta.x / 2f;
		num += m_columnDefault.GetComponent<RectTransform>().sizeDelta.x / 2f;
		foreach (ColumnWeapon column in m_columns)
		{
			Vector3 vector = column.GetComponent<RectTransform>().anchoredPosition;
			vector.x = num + (float)column.GetSortingOrder() * column.GetComponent<RectTransform>().sizeDelta.x;
			column.GetComponent<RectTransform>().anchoredPosition = vector;
		}
	}

	private void DestroyColumns()
	{
		foreach (ColumnWeapon column in m_columns)
		{
			Object.Destroy(column.gameObject);
		}
		m_columns.Clear();
	}

	private void Select(WeaponType i_weaponType)
	{
		foreach (ColumnWeapon column in m_columns)
		{
			column.DeselectWeaponSlots();
		}
		if (GetColumnFromWeaponType(i_weaponType).GetIsHasWeapon())
		{
			if (m_columnSelected != GetColumnFromWeaponType(i_weaponType))
			{
				GetColumnFromWeaponType(i_weaponType).ResetWeaponSlotSelection();
			}
			m_columnSelected = GetColumnFromWeaponType(i_weaponType);
			m_columnSelected.SelectScrollThroughWeaponSlots();
		}
	}

	private void PickWeaponSlot()
	{
		if (m_columnSelected == null)
		{
			CommonReferences.Instance.GetPlayer().EquipWeapon(CommonReferences.Instance.GetPlayer().GetEquippableEquipped());
			Hide();
		}
		else
		{
			CommonReferences.Instance.GetPlayer().EquipWeapon(m_columnSelected.GetWeaponSelected());
			Hide();
		}
	}

	private ColumnWeapon GetColumnFromWeaponType(WeaponType i_weaponType)
	{
		foreach (ColumnWeapon column in m_columns)
		{
			if (column.GetWeaponType() == i_weaponType)
			{
				return column;
			}
		}
		return null;
	}

	public void Show(WeaponType i_weaponType)
	{
		BuildColumns();
		m_isListenToInput = true;
		m_isShowing = true;
		Select(i_weaponType);
	}

	public void Hide()
	{
		DestroyColumns();
		m_columnSelected = null;
		m_isListenToInput = false;
		m_isShowing = false;
	}

	public bool GetIsShowing()
	{
		return m_isShowing;
	}
}
