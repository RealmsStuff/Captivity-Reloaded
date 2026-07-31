using System.Collections.Generic;

public class AmmoBox : Consumable
{
	public override void Consume()
	{
		List<Gun> allGuns = CommonReferences.Instance.GetPlayerController().GetInventory().GetAllGuns();
		for (int i = 0; i < allGuns.Count; i++)
		{
			if (!allGuns[i].GetIsAmmoInfinite())
			{
				allGuns[i].AddAmmo(allGuns[i].GetAmmoMaxTotal() / 8);
			}
		}
		CommonReferences.Instance.GetManagerHud().GetManagerNotification().CreateNotification("Picked up some ammo", ColorTextNotification.Ammo, i_isContinues: false);
	}
}
