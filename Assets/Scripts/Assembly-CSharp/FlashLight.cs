

public class FlashLight : Gun
{
	protected override bool HandleUse(bool i_isAltFire)
	{
		base.HandleUse(i_isAltFire);
		GetComponentInChildren<UnityEngine.Rendering.Universal.Light2D>().enabled = !GetComponentInChildren<UnityEngine.Rendering.Universal.Light2D>().enabled;
		return true;
	}
}
