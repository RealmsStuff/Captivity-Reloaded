using System.Collections.Generic;
using CaptivityReloaded.Modding;
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
	private float m_modEscapePowerMultiplier = 1f;
	private float m_modBountyMultiplier = 1f;
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
			m_modEscapePowerMultiplier = i_effects.EscapePowerMultiplier ?? 1f;
			m_modBountyMultiplier = i_effects.BountyMultiplier ?? 1f;
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
	public float GetModEscapePowerMultiplier() { return m_modEscapePowerMultiplier; }
	public float GetModBountyMultiplier() { return m_modBountyMultiplier; }

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

public static class ModClothingEffects
{
	public static float GetEscapePowerMultiplier(Player i_player)
	{
		float multiplier = 1f;
		foreach (Clothing clothing in GetEquipped(i_player))
			if (clothing != null) multiplier *= clothing.GetModEscapePowerMultiplier();
		return Mathf.Clamp(multiplier, 0.05f, 10f);
	}

	public static int ApplyBountyMultiplier(Player i_player, int i_bounty)
	{
		float multiplier = 1f;
		foreach (Clothing clothing in GetEquipped(i_player))
			if (clothing != null) multiplier *= clothing.GetModBountyMultiplier();
		return Mathf.Max(0, Mathf.RoundToInt(i_bounty * Mathf.Clamp(multiplier, 0f, 10f)));
	}

	public static bool TryForceEquip(Player i_player, string i_clothingId)
	{
		if (i_player == null || !ContentId.TryParse(i_clothingId, out ContentId clothingId)
			|| !ModLoaderRuntime.Registry.TryGet(clothingId, out ContentRegistration registration)
			|| registration.Category != ContentCategory.Clothing || !(registration.RuntimeAsset is Clothing clothing)) return false;
		SkeletonPlayer skeleton = i_player.GetSkeletonPlayer();
		if (skeleton == null) return false;
		if (skeleton.IsClothingEquipped(clothing)) return true;

		List<Clothing> incompatible = new List<Clothing>();
		foreach (Clothing equipped in skeleton.GetClothesEquipped())
			if (equipped != null && !clothing.IsCompatibleWithClothing(equipped)) incompatible.Add(equipped);
		foreach (Clothing equipped in incompatible) skeleton.RemoveClothing(equipped);
		skeleton.EquipClothing(clothing);
		ManagerDB.UnlockClothing(clothing);
		ManagerDB.EquipClothes(new List<Clothing>(skeleton.GetClothesEquipped()));
		return true;
	}

	private static IEnumerable<Clothing> GetEquipped(Player i_player)
	{
		SkeletonPlayer skeleton = i_player == null ? null : i_player.GetSkeletonPlayer();
		if (skeleton == null) return new Clothing[0];
		return skeleton.GetClothesEquipped();
	}
}
