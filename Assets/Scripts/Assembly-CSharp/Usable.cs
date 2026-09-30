using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Usable : Weapon
{
	public delegate void DelAnyUsed(Usable i_usable);
	public static event DelAnyUsed OnAnyUsed;

	[SerializeField]
	private UsableType m_usableType;

	[SerializeField]
	private AudioClip m_audioUse;

	[SerializeField]
	private List<string> m_descriptionsGoodEffects = new List<string>();

	[SerializeField]
	private List<string> m_descriptionsBadEffects = new List<string>();

	private bool m_isUsed;
	private string m_modEffectMode = "inherit";
	private IReadOnlyList<CaptivityReloaded.Modding.UsableEffectDefinition> m_modEffects;
	private bool m_isOriginalModUsable;

	public override bool Use(bool i_isAltFire)
	{
		bool replace = m_modEffectMode == "replace";
		bool flag;
		if (replace)
		{
			NotifyUse();
			flag = ApplyModEffects();
		}
		else
		{
			flag = base.Use(i_isAltFire);
			if (flag && m_modEffectMode == "add") ApplyModEffects();
		}
		if (!m_isOriginalModUsable) switch (m_usableType)
		{
		case UsableType.Syringe:
			CommonReferences.Instance.GetManagerAudio().PlayAudioSFX(Resources.Load<AudioClip>("Audio/Syringe"));
			break;
		case UsableType.Pills:
			CommonReferences.Instance.GetManagerAudio().PlayAudioSFX(Resources.Load<AudioClip>("Audio/Pills"));
			break;
		}
		if (!flag)
		{
			return false;
		}
		m_isUsed = true;
		OnAnyUsed?.Invoke(this);
		CommonReferences.Instance.GetManagerAudio().PlayAudioSFX(m_audioUse);
		SetIsPickUpable(i_isPickUpable: false);
		StartCoroutine(CoroutineDropAfterUse());
		return true;
	}

	private IEnumerator CoroutineDropAfterUse()
	{
		yield return new WaitForSeconds(0.5f);
		if (m_isPickedUp)
		{
			CommonReferences.Instance.GetPlayer().DropPickupAble(this);
		}
	}

	public override void Equip()
	{
	}

	protected override bool HandleUse(bool i_isAltFire)
	{
		return false;
	}

	public override void Drop()
	{
		base.Drop();
		if (!m_isPickUpable)
		{
			HideOutline();
		}
	}

	public override void Drop(float i_powerDrop01)
	{
		base.Drop(i_powerDrop01);
		if (!m_isPickUpable)
		{
			HideOutline();
		}
	}

	public override void Drop(Vector2 i_forceDrop)
	{
		base.Drop(i_forceDrop);
		if (!m_isPickUpable)
		{
			HideOutline();
		}
	}

	private void Destroy()
	{
		CommonReferences.Instance.GetPlayerController().GetInventory().RemovePickUpable(this);
		Object.Destroy(base.gameObject);
	}

	public List<string> GetDescriptionsGoodEffects()
	{
		return m_descriptionsGoodEffects;
	}

	public List<string> GetDescriptionsBadEffects()
	{
		return m_descriptionsBadEffects;
	}

	public UsableType GetUsableType()
	{
		return m_usableType;
	}

	public bool IsUsed()
	{
		return m_isUsed;
	}

	public void ConfigureModUsable(CaptivityReloaded.Modding.UsableStatsDefinition i_stats, string i_effectMode,
		IReadOnlyList<CaptivityReloaded.Modding.UsableEffectDefinition> i_effects, bool i_isOriginal = false)
	{
		if (i_stats != null)
		{
			ConfigureModEconomy(i_stats.Weight, i_stats.Value);
			ConfigureModWeaponBase(i_stats.EquipSeconds, i_stats.Marketable);
			if (i_stats.GoodEffectDescriptions != null) m_descriptionsGoodEffects = new List<string>(i_stats.GoodEffectDescriptions);
			if (i_stats.BadEffectDescriptions != null) m_descriptionsBadEffects = new List<string>(i_stats.BadEffectDescriptions);
		}
		m_modEffectMode = i_effectMode ?? "inherit";
		m_modEffects = i_effects;
		m_isOriginalModUsable = i_isOriginal;
	}

	private bool ApplyModEffects()
	{
		if (m_modEffects == null || m_modEffects.Count == 0) return false;
		Player player = CommonReferences.Instance.GetPlayer();
		foreach (CaptivityReloaded.Modding.UsableEffectDefinition effect in m_modEffects)
		{
			switch (effect.Type)
			{
			case "restoreHealth": player.RestoreHealth(effect.Amount.Value); break;
			case "restoreStrength": player.RestoreStrength(effect.Amount.Value); break;
			case "restoreStamina": player.RestoreStamina(effect.Amount.Value); break;
			case "reducePleasure": player.LosePleasure(effect.Amount.Value); break;
			case "refillAmmo":
				foreach (Gun gun in CommonReferences.Instance.GetPlayerController().GetInventory().GetAllGuns())
					gun.AddAmmoIncludingMagazine(Mathf.RoundToInt(effect.Amount.Value));
				break;
			case "fillAllAmmo":
				foreach (Gun gun in CommonReferences.Instance.GetPlayerController().GetInventory().GetAllGuns()) gun.FillEntireGun();
				break;
			case "repairClothing": player.GetSkeletonPlayer().RepairEquippedClothing(); break;
			case "gainMoney":
				CommonReferences.Instance.GetPlayerController().GainMoney(Mathf.RoundToInt(effect.Amount.Value));
				break;
			case "ragdoll": player.Ragdoll(effect.DurationSeconds.Value); break;
			case "invulnerability":
				player.BecomeInvulnerable("Mod item invulnerability", GetName(), effect.DurationSeconds.Value, i_isAffectSkeleton: true);
				break;
			case "statModifier":
				StatusEffectStatModifier status = new StatusEffectStatModifier("Mod item: " + GetName() + "/" + effect.Stat, GetName(),
					effect.Value.Value >= 0f ? TypeStatusEffect.Positive : TypeStatusEffect.Negative,
					effect.DurationSeconds.Value, effect.Stackable ?? true);
				status.AddStatModification(effect.Stat, effect.Value.Value);
				player.ApplyStatusEffect(status);
				break;
			}
		}
		return true;
	}
}
