using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Usable : Weapon
{
	[SerializeField]
	private UsableType m_usableType;

	[SerializeField]
	private AudioClip m_audioUse;

	[SerializeField]
	private List<string> m_descriptionsGoodEffects = new List<string>();

	[SerializeField]
	private List<string> m_descriptionsBadEffects = new List<string>();

	private bool m_isUsed;

	public override bool Use(bool i_isAltFire)
	{
		bool flag = base.Use(i_isAltFire);
		switch (m_usableType)
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
}
