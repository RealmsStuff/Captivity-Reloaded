using UnityEngine;
using UnityEngine.UI;

public class WeaponSlot : MonoBehaviour
{
	[SerializeField]
	private Image m_imgBorder;

	[SerializeField]
	private Image m_icon;

	[SerializeField]
	private Text m_txtName;

	[SerializeField]
	private Text m_txtAmmo;

	private Weapon m_weapon;

	private void Update()
	{
		if (m_weapon != null && m_weapon is Gun)
		{
			UpdateAmmo();
		}
	}

	public void Initialize(Weapon i_weapon)
	{
		m_weapon = i_weapon;
		m_icon.sprite = m_weapon.GetSpriteIcon();
		m_txtName.text = i_weapon.GetName();
		if (m_weapon is Gun)
		{
			m_txtAmmo.enabled = true;
		}
		else
		{
			m_txtAmmo.enabled = false;
		}
	}

	private void UpdateAmmo()
	{
		Gun gun = (Gun)m_weapon;
		if (gun.GetIsAmmoInfinite())
		{
			m_txtAmmo.text = gun.GetAmmoMagazineLeft() + "/-";
		}
		else
		{
			m_txtAmmo.text = gun.GetAmmoMagazineLeft() + "/" + gun.GetAmmoLeft();
		}
	}

	public void Select()
	{
		m_imgBorder.enabled = true;
	}

	public void Deselect()
	{
		m_imgBorder.enabled = false;
	}

	public Weapon GetWeapon()
	{
		return m_weapon;
	}
}
