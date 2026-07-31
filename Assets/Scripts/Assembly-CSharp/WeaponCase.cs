using UnityEngine;

public class WeaponCase : Interactable
{
	[SerializeField]
	private Weapon m_weapon;

	[Range(1f, 3f)]
	[SerializeField]
	private int m_weaponSize;

	private Sprite m_sprCase;

	private Sprite m_sprCaseBroken;

	private new void Start()
	{
		switch (m_weaponSize)
		{
		case 1:
			m_sprCase = Resources.Load<Sprite>("Graphics/WeaponCase/Case1");
			m_sprCaseBroken = Resources.Load<Sprite>("Graphics/WeaponCase/Case1Broken");
			break;
		case 2:
			m_sprCase = Resources.Load<Sprite>("Graphics/WeaponCase/Case2");
			m_sprCaseBroken = Resources.Load<Sprite>("Graphics/WeaponCase/Case2Broken");
			break;
		case 3:
			m_sprCase = Resources.Load<Sprite>("Graphics/WeaponCase/Case3");
			m_sprCaseBroken = Resources.Load<Sprite>("Graphics/WeaponCase/Case3Broken");
			break;
		}
		GetComponent<SpriteRenderer>().sprite = m_sprCase;
		m_priceToActivate = m_weapon.GetValue();
		GetComponentsInChildren<SpriteRenderer>()[1].sprite = m_weapon.GetSpriteIcon();
	}

	public override void Use()
	{
		if (m_isActivatedOnceAlready)
		{
			return;
		}
		if (m_weapon is Gun)
		{
			_ = (Gun)m_weapon;
			if (CommonReferences.Instance.GetPlayerController().GetInventory().GetPickUpableByName(m_weapon.GetName()) != null)
			{
				base.Use();
				return;
			}
		}
		if (!CommonReferences.Instance.GetPlayerController().GetInventory().IsHasRoomForPickUpable(m_weapon))
		{
			CommonReferences.Instance.GetManagerHud().GetManagerNotification().CreateNotification("Too heavy (weight weapon: " + m_weapon.GetWeight() + ", room left: " + CommonReferences.Instance.GetPlayerController().GetInventory().GetRoomLeft() + ")", ColorTextNotification.Other, i_isContinues: false);
		}
		else
		{
			base.Use();
		}
	}

	protected override void HandleActivation(Actor i_initiator, InteractableActivationType i_activationType)
	{
		CommonReferences.Instance.GetManagerAudio().PlayAudioSFX(Resources.Load<AudioClip>("Audio/WeaponCaseBreak"));
		GetComponent<SpriteRenderer>().sprite = m_sprCaseBroken;
		GetComponentsInChildren<SpriteRenderer>()[1].enabled = false;
		GetComponentInChildren<ParticleSystem>().Play();
		if (CommonReferences.Instance.GetPlayerController().GetInventory().GetPickUpableByName(m_weapon.GetName()) != null)
		{
			((Gun)CommonReferences.Instance.GetPlayerController().GetInventory().GetPickUpableByName(m_weapon.GetName())).FillEntireGun();
			return;
		}
		Weapon weapon = Object.Instantiate(m_weapon);
		if (weapon is Gun)
		{
			Gun obj = (Gun)weapon;
			obj.FillEntireGun();
			weapon = obj;
		}
		((Player)i_initiator).PickUp(weapon, i_isDuplicate: false);
	}
}
