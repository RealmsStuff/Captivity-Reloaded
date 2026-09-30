using UnityEngine;

public class Buffout : Usable
{
	[SerializeField]
	private float m_amountStrengthToRestore;

	protected override bool HandleUse(bool i_isAltFire)
	{
		base.HandleUse(i_isAltFire);
		float multiplier = ExternalRuleProfileFactory.TryGetConsumableRule("buffout", out CaptivityReloaded.Modding.ConsumableRule rule) ? rule.StrengthRestoreMultiplier : 1f;
		CommonReferences.Instance.GetPlayer().RestoreStrength(m_amountStrengthToRestore * multiplier);
		CommonReferences.Instance.GetManagerPostProcessing().PlayEffectBuffout();
		return true;
	}
}
