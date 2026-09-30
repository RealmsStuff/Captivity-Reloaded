using UnityEngine;

public class Anaphrodisiac : Usable
{
	protected override bool HandleUse(bool i_isAltFire)
	{
		base.HandleUse(i_isAltFire);
		if (ExternalRuleProfileFactory.TryGetConsumableRule("anaphrodisiac", out CaptivityReloaded.Modding.ConsumableRule rule))
		{
			Aspirin.ApplyRelief(CommonReferences.Instance.GetPlayer(), rule, GetName() + GetInstanceID());
			return true;
		}
		SELibidoDecrease sELibidoDecrease = new SELibidoDecrease("Anaphrodisiac", GetName() + GetInstanceID(), TypeStatusEffect.Positive, 20f, i_isStackable: true, 1f, 1.65f);
		sELibidoDecrease.AddPlayerStatusHudItem("Anaphrodisiac", "-33% libido over 20s", StatusPlayerHudItemColor.Buff);
		bool num = CommonReferences.Instance.GetPlayer().ApplyStatusEffect(sELibidoDecrease);
		if (num)
		{
			CommonReferences.Instance.GetManagerHud().GetManagerOverlay().PlayOverlayPopup(Color.yellow, i_isUseOverlayWithHole: true, i_isDestroyOverlayAfterAnimation: true, 0.25f, 0f, 0.5f, 1f, 0.5f, 0f, 0f);
		}
		return num;
	}
}
