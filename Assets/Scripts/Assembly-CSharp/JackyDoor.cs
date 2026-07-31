using System.Collections;
using UnityEngine;

public class JackyDoor : Door
{
	public override void Open()
	{
		base.Open();
		if (!CommonReferences.Instance.GetPlayer().IsStatusEffectAppliedAlready("Jacky Curse"))
		{
			StartCoroutine(CoroutineApplyJackyCurse());
		}
	}

	private IEnumerator CoroutineApplyJackyCurse()
	{
		yield return new WaitForSeconds(15f);
		SEJackyCurse i_statusEffect = new SEJackyCurse("Jacky Curse", "Jacky Door", TypeStatusEffect.Negative, 99999f, i_isStackable: false, 0.25f, 0.175f, 0.05f, 4);
		CommonReferences.Instance.GetPlayer().ApplyStatusEffect(i_statusEffect);
	}
}
