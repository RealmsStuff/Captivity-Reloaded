using System.Collections.Generic;
using UnityEngine;

public class Clothing : MonoBehaviour
{
	[SerializeField]
	private int m_id;

	[SerializeField]
	private Sprite m_sprIcon;

	[SerializeField]
	private ClothingCategory m_categoryClothing;

	private List<ClothingPiece> m_clothingPieces = new List<ClothingPiece>();

	[SerializeField]
	private List<ClothingCategory> m_clothingCategoriesIncompatible = new List<ClothingCategory>();

	[SerializeField]
	private List<Clothing> m_clothesIncompatible = new List<Clothing>();

	[SerializeField]
	private List<Clothing> m_clothesCompatibleOverride = new List<Clothing>();

	public void Initialize()
	{
		m_clothingPieces.Clear();
		ClothingPiece[] componentsInChildren = GetComponentsInChildren<ClothingPiece>(includeInactive: true);
		foreach (ClothingPiece item in componentsInChildren)
		{
			m_clothingPieces.Add(item);
		}
	}

	public List<ClothingPiece> GetClothingPieces()
	{
		return m_clothingPieces;
	}

	public ClothingCategory GetCatergoryClothing()
	{
		return m_categoryClothing;
	}

	public int GetId()
	{
		return m_id;
	}

	public void SetId(int i_id)
	{
		m_id = i_id;
		foreach (ClothingPiece clothingPiece in m_clothingPieces)
		{
			clothingPiece.SetId(m_id);
		}
	}

	public Sprite GetIcon()
	{
		return m_sprIcon;
	}

	public void SetIcon(Sprite i_icon)
	{
		m_sprIcon = i_icon;
	}

	public bool IsCompatibleWithClothing(Clothing i_clothingToCheck)
	{
		if (m_clothesCompatibleOverride.Contains(i_clothingToCheck) || i_clothingToCheck.GetClothesCompatibleOverride().Contains(this))
		{
			return true;
		}
		if (m_clothingCategoriesIncompatible.Contains(i_clothingToCheck.GetCatergoryClothing()) || i_clothingToCheck.GetClothingCategoriesIncompatible().Contains(m_categoryClothing))
		{
			return false;
		}
		if (i_clothingToCheck.GetCatergoryClothing() == m_categoryClothing)
		{
			return false;
		}
		if (m_clothesIncompatible.Contains(i_clothingToCheck))
		{
			return false;
		}
		if (i_clothingToCheck.GetClothesIncompatible().Contains(this))
		{
			return false;
		}
		return true;
	}

	public List<ClothingCategory> GetClothingCategoriesIncompatible()
	{
		return m_clothingCategoriesIncompatible;
	}

	public List<Clothing> GetClothesIncompatible()
	{
		return m_clothesIncompatible;
	}

	public List<Clothing> GetClothesCompatibleOverride()
	{
		return m_clothesCompatibleOverride;
	}
}
