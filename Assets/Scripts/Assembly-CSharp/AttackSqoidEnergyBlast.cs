using UnityEngine;

public class AttackSqoidEnergyBlast : AttackNPC
{
	public override void HandleAttackPerform()
	{
		base.HandleAttackPerform();
		EnergyBlast energyBlast = Object.Instantiate(((Sqoid)m_npc).GetEnergyBlast(), CommonReferences.Instance.GetManagerStages().GetStageCurrent().transform);
		energyBlast.transform.position = m_npc.GetPos();
		energyBlast.SetOwner(m_npc);
		energyBlast.SetDirection(m_player.GetPosHips());
		energyBlast.gameObject.SetActive(value: true);
		energyBlast.Fly();
	}
}
