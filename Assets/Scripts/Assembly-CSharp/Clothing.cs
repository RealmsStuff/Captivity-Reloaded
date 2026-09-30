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
	private float m_modDamageTakenMultiplier = 1f;
	private Dictionary<string, float> m_modStatModifiers = new Dictionary<string, float>();
	private List<StatModifier> m_appliedModStatModifiers = new List<StatModifier>();

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

	public void ConfigureModClothing(string i_category, IEnumerable<string> i_incompatibleCategories,
		CaptivityReloaded.Modding.ClothingEffectsDefinition i_effects)
	{
		if (!string.IsNullOrWhiteSpace(i_category) && System.Enum.TryParse(i_category, true, out ClothingCategory category))
			m_categoryClothing = category;
		if (i_incompatibleCategories != null)
		{
			m_clothingCategoriesIncompatible.Clear();
			foreach (string value in i_incompatibleCategories)
				if (System.Enum.TryParse(value, true, out ClothingCategory incompatible) && !m_clothingCategoriesIncompatible.Contains(incompatible))
					m_clothingCategoriesIncompatible.Add(incompatible);
		}
		if (i_effects != null)
		{
			m_modDamageTakenMultiplier = i_effects.DamageTakenMultiplier ?? 1f;
			m_modStatModifiers = i_effects.StatModifiers == null
				? new Dictionary<string, float>() : new Dictionary<string, float>(i_effects.StatModifiers);
		}
	}

	public void ApplyModEffects(Player i_player)
	{
		if (i_player == null || m_appliedModStatModifiers.Count > 0) return;
		foreach (KeyValuePair<string, float> entry in m_modStatModifiers)
			m_appliedModStatModifiers.Add(i_player.AddStatModifier(entry.Key, entry.Value));
	}

	public void RemoveModEffects(Player i_player)
	{
		if (i_player != null) i_player.RemoveStatModifier(m_appliedModStatModifiers);
		m_appliedModStatModifiers.Clear();
	}

	public float GetModDamageTakenMultiplier() { return m_modDamageTakenMultiplier; }

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

	public void ConfigureCoreCompatibility(IEnumerable<Clothing> i_incompatible, IEnumerable<Clothing> i_compatibleOverrides)
	{
		m_clothesIncompatible.Clear();
		if (i_incompatible != null) foreach (Clothing clothing in i_incompatible)
			if (clothing != null && clothing != this && !m_clothesIncompatible.Contains(clothing)) m_clothesIncompatible.Add(clothing);
		m_clothesCompatibleOverride.Clear();
		if (i_compatibleOverrides != null) foreach (Clothing clothing in i_compatibleOverrides)
			if (clothing != null && clothing != this && !m_clothesCompatibleOverride.Contains(clothing)) m_clothesCompatibleOverride.Add(clothing);
	}
}
