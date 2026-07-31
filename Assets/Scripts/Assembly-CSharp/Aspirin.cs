public class Aspirin : Usable
{
	protected override bool HandleUse(bool i_isAltFire)
	{
		base.HandleUse(i_isAltFire);
		CommonReferences.Instance.GetPlayer().RestoreHealth(35f);
		return true;
	}
}
