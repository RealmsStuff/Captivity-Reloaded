using UnityEngine;

public class Buffout : Usable
{
	[SerializeField]
	private float m_amountStrengthToRestore;

	protected override bool HandleUse(bool i_isAltFire)
	{
		base.HandleUse(i_isAltFire);
		CommonReferences.Instance.GetPlayer().RestoreStrength(m_amountStrengthToRestore);
		CommonReferences.Instance.GetManagerPostProcessing().PlayEffectBuffout();
		return true;
	}
}
