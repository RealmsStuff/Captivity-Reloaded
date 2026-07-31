public class Morphine : Usable
{
	protected override bool HandleUse(bool i_isAltFire)
	{
		CommonReferences.Instance.GetPlayer().RestoreHealth(100f);
		return true;
	}
}
