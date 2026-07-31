using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XAIFly : XAIFlier
{
	private Fly m_fly;

	private bool m_isCanCharge = true;

	private int m_chanceToChargeMinium = 40;

	public override void Initialize(NPC i_npc)
	{
		base.Initialize(i_npc);
		m_fly = (Fly)i_npc;
		StartCoroutine(CoroutineIncreaseChanceToChargeOverTime());
	}

	protected override void HandleStateChase()
	{
		if (m_isCanCharge && m_fly.IsCloseEnoughToPlayerToCharge() && m_fly.IsHasLineOfSightToPlayer() && m_fly.IsCanCharge())
		{
			TryCharge();
		}
		if (!m_fly.IsChargingOrPreparingCharge())
		{
			base.HandleStateChase();
		}
	}

	private void TryCharge()
	{
		List<NPC> allNPCs = CommonReferences.Instance.GetManagerStages().GetStageCurrent().GetAllNPCs();
		int num = 100;
		foreach (NPC item in allNPCs)
		{
			_ = item;
			num -= 10;
		}
		if (num < m_chanceToChargeMinium)
		{
			num = m_chanceToChargeMinium;
		}
		if (Random.Range(0, 101) > 100 - num)
		{
			m_fly.PrepareCharge();
		}
		StartCoroutine(CoroutineWaitForNextTryCharge());
	}

	private IEnumerator CoroutineWaitForNextTryCharge()
	{
		m_isCanCharge = false;
		yield return new WaitForSeconds(Random.Range(3f, 10f));
		m_isCanCharge = true;
	}

	private IEnumerator CoroutineIncreaseChanceToChargeOverTime()
	{
		int l_secsPassed = 0;
		for (int l_secsMax = 90; l_secsPassed < l_secsMax; l_secsPassed += 3)
		{
			m_chanceToChargeMinium += 2;
			yield return new WaitForSeconds(3f);
		}
	}
}
