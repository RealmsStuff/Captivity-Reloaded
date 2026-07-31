using UnityEngine;

public class SecondWind : Usable
{
	[SerializeField]
	private AudioClip m_audioKoPrevention;

	protected override bool HandleUse(bool i_isAltFire)
	{
		base.HandleUse(i_isAltFire);
		SESecondWind sESecondWind = new SESecondWind("Second Wind", GetName() + GetInstanceID(), TypeStatusEffect.Positive, 9999f, i_isStackable: false, m_audioKoPrevention);
		sESecondWind.AddPlayerStatusHudItem("Second Wind", "K.O. protection", StatusPlayerHudItemColor.Buff);
		bool num = CommonReferences.Instance.GetPlayer().ApplyStatusEffect(sESecondWind);
		if (num)
		{
			CommonReferences.Instance.GetManagerHud().GetManagerOverlay().PlayOverlayPopup(new Color(0.4f, 0.4f, 1f), i_isUseOverlayWithHole: true, i_isDestroyOverlayAfterAnimation: true, 0.25f, 0f, 0.5f, 1f, 0.5f, 0f, 0f);
		}
		return num;
	}
}
