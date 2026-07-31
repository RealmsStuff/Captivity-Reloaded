using UnityEngine;

public class ManagerAmmoHud : MonoBehaviour
{
	[SerializeField]
	private AmmoHud m_ammoHud;

	private void Start()
	{
		HideAmmoDisplay();
	}

	private void Update()
	{
		if (CommonReferences.Instance.GetPlayer().GetIsBeingRaped() || CommonReferences.Instance.GetPlayer().IsExposing())
		{
			HideAmmoDisplay();
		}
		else if ((bool)CommonReferences.Instance.GetPlayer().GetEquippableEquipped() && CommonReferences.Instance.GetPlayer().GetEquippableEquipped() is Gun)
		{
			DisplayAmmoGun((Gun)CommonReferences.Instance.GetPlayer().GetEquippableEquipped());
		}
		else
		{
			HideAmmoDisplay();
		}
	}

	public void DisplayAmmoGun(Gun i_gun)
	{
		m_ammoHud.gameObject.SetActive(value: true);
		m_ammoHud.ShowAmmoGun(i_gun);
	}

	public void HideAmmoDisplay()
	{
		m_ammoHud.gameObject.SetActive(value: false);
	}
}
