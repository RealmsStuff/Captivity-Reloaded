using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : PickUpable
{
	public delegate void DelOnUse();

	[SerializeField]
	protected WeaponType m_weaponType;

	[SerializeField]
	protected float m_durationEquip;

	[SerializeField]
	protected bool m_isMarketable;

	private Dictionary<SpriteRenderer, int> m_dicSpriteSort = new Dictionary<SpriteRenderer, int>();

	public event DelOnUse OnUse;

	protected override void Awake()
	{
		base.Awake();
		RememberSortOrderSprites();
		m_isVisible = true;
	}

	private void RememberSortOrderSprites()
	{
		SpriteRenderer[] componentsInChildren = GetComponentsInChildren<SpriteRenderer>();
		foreach (SpriteRenderer spriteRenderer in componentsInChildren)
		{
			m_dicSpriteSort.Add(spriteRenderer, spriteRenderer.sortingOrder);
		}
	}

	public override void PickUp(Actor i_owner)
	{
		if (!CommonReferences.Instance.GetPlayerController().GetInventory().IsHasRoomForPickUpable(this))
		{
			CommonReferences.Instance.GetManagerHud().GetManagerNotification().CreateNotification("Too heavy (weight weapon: " + GetWeight() + ", room left: " + CommonReferences.Instance.GetPlayerController().GetInventory().GetRoomLeft() + ")", ColorTextNotification.Other, i_isContinues: false);
		}
		else
		{
			base.PickUp(i_owner);
		}
	}

	public abstract void Equip();

	public virtual bool Use(bool i_isAltFire)
	{
		if (this.OnUse != null)
		{
			this.OnUse();
		}
		return HandleUse(i_isAltFire);
	}

	protected abstract bool HandleUse(bool i_isAltFire);

	public WeaponType GetWeaponType()
	{
		return m_weaponType;
	}

	public float GetDurationEquip()
	{
		return m_durationEquip;
	}

	public Dictionary<SpriteRenderer, int> GetDicSpriteSortOriginal()
	{
		return m_dicSpriteSort;
	}

	public override void Show()
	{
		base.Show();
		if ((bool)GetComponent<Animator>())
		{
			GetComponent<Animator>().enabled = true;
		}
	}

	public override void Hide()
	{
		base.Hide();
		if ((bool)GetComponent<Animator>())
		{
			GetComponent<Animator>().enabled = false;
		}
	}

	public bool IsMarketable()
	{
		return m_isMarketable;
	}

	public bool IsVisible()
	{
		return m_isVisible;
	}
}
