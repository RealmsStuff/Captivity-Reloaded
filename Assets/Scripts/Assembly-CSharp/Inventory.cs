using System.Collections.Generic;
using UnityEngine;

public class Inventory
{
	private int m_money;

	private int m_room = 10;

	private List<PickUpable> m_pickUpables = new List<PickUpable>();

	public void LoadFromSave()
	{
	}

	public void AddItem(PickUpable i_pickUpable)
	{
		if (i_pickUpable.GetIsStackable())
		{
			foreach (PickUpable pickUpable in m_pickUpables)
			{
				if (CheckIfPickUpablesAreClones(pickUpable, i_pickUpable))
				{
					pickUpable.IncreaseAmount(i_pickUpable.GetAmount());
					return;
				}
			}
		}
		if (i_pickUpable is Gun)
		{
			foreach (PickUpable pickUpable2 in m_pickUpables)
			{
				if (CheckIfPickUpablesAreClones(pickUpable2, i_pickUpable))
				{
					((Gun)i_pickUpable).Destroy();
					return;
				}
			}
		}
		m_pickUpables.Add(i_pickUpable);
	}

	private bool CheckIfPickUpablesAreClones(PickUpable i_pickUpable1, PickUpable i_pickUpable2)
	{
		if (i_pickUpable1 == null || i_pickUpable2 == null)
		{
			return false;
		}
		if (i_pickUpable1.GetName() != i_pickUpable2.GetName())
		{
			return false;
		}
		if (i_pickUpable1.GetSpriteIcon() != i_pickUpable2.GetSpriteIcon())
		{
			return false;
		}
		if (i_pickUpable1.GetIsCanDrop() != i_pickUpable2.GetIsCanDrop())
		{
			return false;
		}
		if (i_pickUpable1.GetDescription() != i_pickUpable2.GetDescription())
		{
			return false;
		}
		if (i_pickUpable1.GetIsStackable() != i_pickUpable2.GetIsStackable())
		{
			return false;
		}
		return true;
	}

	public List<PickUpable> GetAllPickUpables()
	{
		return m_pickUpables;
	}

	public List<Weapon> GetAllWeapons()
	{
		List<Weapon> list = new List<Weapon>();
		foreach (PickUpable pickUpable in m_pickUpables)
		{
			if (pickUpable is Weapon)
			{
				list.Add((Weapon)pickUpable);
			}
		}
		return list;
	}

	public List<Gun> GetAllGuns()
	{
		List<Gun> list = new List<Gun>();
		foreach (PickUpable pickUpable in m_pickUpables)
		{
			if (pickUpable is Gun)
			{
				list.Add((Gun)pickUpable);
			}
		}
		return list;
	}

	public List<Usable> GetAllUsables()
	{
		List<Usable> list = new List<Usable>();
		foreach (PickUpable pickUpable in m_pickUpables)
		{
			if (pickUpable is Usable)
			{
				list.Add((Usable)pickUpable);
			}
		}
		return list;
	}

	public void RemovePickUpable(PickUpable i_pickUpable)
	{
		foreach (PickUpable pickUpable in m_pickUpables)
		{
			if (pickUpable == i_pickUpable)
			{
				m_pickUpables.Remove(pickUpable);
				return;
			}
		}
		Debug.Log("Item to remove not found!");
	}

	public void RemovePickUpable(PickUpable i_pickUpable, int i_amount)
	{
		foreach (PickUpable pickUpable in m_pickUpables)
		{
			if (!(pickUpable == i_pickUpable))
			{
				continue;
			}
			if (pickUpable.GetIsStackable())
			{
				pickUpable.DecreaseAmount(i_amount);
				if (pickUpable.GetAmount() < 1)
				{
					m_pickUpables.Remove(pickUpable);
				}
				return;
			}
			Debug.Log("Item to remove amount of is not stackable!");
		}
		Debug.Log("Item to remove not found!");
	}

	public void HideAllItems()
	{
		foreach (PickUpable allPickUpable in GetAllPickUpables())
		{
			allPickUpable.gameObject.SetActive(value: false);
		}
	}

	public List<Weapon> GetAllEquippables()
	{
		List<Weapon> list = new List<Weapon>();
		foreach (PickUpable allPickUpable in GetAllPickUpables())
		{
			if (allPickUpable is Weapon)
			{
				list.Add((Weapon)allPickUpable);
			}
		}
		return list;
	}

	public int GetAmountOfPickUpable(PickUpable i_pickUpableToCount)
	{
		int num = 0;
		foreach (PickUpable allPickUpable in GetAllPickUpables())
		{
			if (CheckIfPickUpablesAreClones(i_pickUpableToCount, allPickUpable))
			{
				num += allPickUpable.GetAmount();
			}
		}
		return num;
	}

	public PickUpable CheckAndReturnIfHasPickUpable(PickUpable i_pickUpableToSearchFor)
	{
		foreach (PickUpable pickUpable in m_pickUpables)
		{
			if (pickUpable == i_pickUpableToSearchFor)
			{
				return pickUpable;
			}
		}
		return null;
	}

	public PickUpable GetPickUpableByName(string i_namePickUpableToSearchFor)
	{
		foreach (PickUpable pickUpable in m_pickUpables)
		{
			if (pickUpable.GetName() == i_namePickUpableToSearchFor)
			{
				return pickUpable;
			}
		}
		return null;
	}

	public int GetMoney()
	{
		return m_money;
	}

	public void AddMoney(int i_amount)
	{
		m_money += i_amount;
	}

	public void DepleteMoney(int i_amount)
	{
		m_money -= i_amount;
		if (m_money < 0)
		{
			m_money = 0;
		}
	}

	public int GetEncumbrance()
	{
		int num = 0;
		foreach (PickUpable allPickUpable in GetAllPickUpables())
		{
			num += allPickUpable.GetWeight();
		}
		return num;
	}

	public int GetRoomLeft()
	{
		int num = 0;
		foreach (PickUpable allPickUpable in GetAllPickUpables())
		{
			num += allPickUpable.GetWeight();
		}
		return m_room - num;
	}

	public bool IsHasRoomForPickUpable(PickUpable i_pickUpable)
	{
		if (ExternalRuleProfileFactory.ShouldIgnoreWeightLimit()) return true;
		if (GetEncumbrance() + i_pickUpable.GetWeight() > m_room)
		{
			return false;
		}
		return true;
	}

	public int GetRoom()
	{
		return m_room;
	}

	public void Reset()
	{
		DepleteMoney(999999);
		List<PickUpable> list = new List<PickUpable>();
		foreach (PickUpable allPickUpable in GetAllPickUpables())
		{
			list.Add(allPickUpable);
		}
		foreach (PickUpable item in list)
		{
			RemovePickUpable(item);
		}
	}
}
