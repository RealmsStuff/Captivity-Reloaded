using System.Collections.Generic;
using UnityEngine;

public class LibraryClothes : MonoBehaviour
{
	[SerializeField]
	private List<Clothing> m_clothesUnlockedDefault;

	[SerializeField]
	private List<Clothing> m_clothesEquippedDefault;

	private List<Clothing> m_clothes = new List<Clothing>();

	private void Awake()
	{
		foreach (Clothing allClothe in GetAllClothes())
		{
			allClothe.Initialize();
			allClothe.SetId(allClothe.GetId());
			allClothe.gameObject.SetActive(value: false);
			foreach (ClothingPiece clothingPiece in allClothe.GetClothingPieces())
			{
				if ((bool)clothingPiece.GetComponent<Rigidbody2D>())
				{
					clothingPiece.GetComponent<Rigidbody2D>().isKinematic = true;
				}
				if ((bool)clothingPiece.GetComponent<Collider2D>())
				{
					clothingPiece.GetComponent<Collider2D>().enabled = false;
				}
			}
		}
	}

	public void FirstTimeStart()
	{
		if (ManagerDB.IsFirstTimeStart())
		{
			for (int i = 0; i < m_clothesUnlockedDefault.Count; i++)
			{
				ManagerDB.UnlockClothing(m_clothesUnlockedDefault[i]);
			}
			ManagerDB.EquipClothes(m_clothesEquippedDefault);
			ManagerDB.FirstTimeStart();
		}
	}

	public List<Clothing> GetAllClothes()
	{
		if (m_clothes.Count == 0)
		{
			Clothing[] componentsInChildren = GetComponentsInChildren<Clothing>(includeInactive: true);
			foreach (Clothing item in componentsInChildren)
			{
				m_clothes.Add(item);
			}
		}
		return m_clothes;
	}

	public Clothing GetClothing(int i_idClothing)
	{
		foreach (Clothing allClothe in GetAllClothes())
		{
			if (allClothe.GetId() == i_idClothing)
			{
				return allClothe;
			}
		}
		return null;
	}

	public void AddRuntimeClothing(Clothing i_clothing)
	{
		if (i_clothing == null || m_clothes.Contains(i_clothing)) return;
		i_clothing.Initialize();
		i_clothing.gameObject.SetActive(false);
		m_clothes.Add(i_clothing);
	}

	public void ReplaceCoreClothing(int i_legacyId, Clothing i_replacement)
	{
		if (i_replacement == null) return;
		List<Clothing> clothes = GetAllClothes();
		int insertAt = clothes.FindIndex(item => item != null && item.GetId() == i_legacyId);
		clothes.RemoveAll(item => item != null && item != i_replacement && item.GetId() == i_legacyId);
		if (!clothes.Contains(i_replacement))
		{
			if (insertAt < 0 || insertAt > clothes.Count) clothes.Add(i_replacement);
			else clothes.Insert(insertAt, i_replacement);
		}
		i_replacement.gameObject.SetActive(false);
	}

	public void ClearAndReassignIds()
	{
		for (int i = 0; i < GetAllClothes().Count; i++)
		{
			GetAllClothes()[i].SetId(i);
		}
	}
}
