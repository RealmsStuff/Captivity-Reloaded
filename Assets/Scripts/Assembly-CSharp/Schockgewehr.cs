using UnityEngine;

public class Schockgewehr : Gun
{
	[SerializeField]
	private SpriteRenderer m_sprLightReload;

	private void Update()
	{
		if (m_isPickedUp)
		{
			if (CommonReferences.Instance.GetPlayer().GetIsReloading())
			{
				m_sprLightReload.color = Color.yellow;
			}
			else if (GetAmmoMagazineLeft() == 0)
			{
				m_sprLightReload.color = Color.red;
			}
			else
			{
				m_sprLightReload.color = Color.green;
			}
		}
	}

	protected override void Awake()
	{
		base.Awake();
		m_framesLineShoot = 3;
		m_colorLineShoot = new Color(1f, 0.25f, 0.25f);
		m_thicknessLineShoot = 1;
	}
}
