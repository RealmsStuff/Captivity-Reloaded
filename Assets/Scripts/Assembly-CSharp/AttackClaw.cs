using UnityEngine;

public class AttackClaw : AttackNPC
{
	public override void HandleAttackStart()
	{
		Vector2 force = new Vector2(5f, 8f);
		if (m_npc.GetIsPlayerLeftOfMe())
		{
			force.x *= -1f;
		}
		m_npc.GetRigidbody2D().AddForce(force, ForceMode2D.Impulse);
		m_npc.SetIsThinking(i_isThinking: false);
	}

	public override void HandleAttackEnd()
	{
		base.HandleAttackEnd();
		if (!m_npc.IsDead() && !m_npc.GetRaper().GetIsRaping())
		{
			m_npc.SetIsThinking(i_isThinking: true);
		}
	}
}
