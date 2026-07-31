using UnityEngine;

public class XAIAndroid : XAIWalker
{
	private Android m_android;

	public override void Initialize(NPC i_npc)
	{
		base.Initialize(i_npc);
		m_android = (Android)i_npc;
		m_android.OnTakeHitBullet += TryDodge;
	}

	private void TryDodge()
	{
		if (!m_android.IsDodging() && !m_android.GetIsAttacking() && !m_android.IsDead() && m_android.GetStateActorCurrent() != StateActor.Jumping && m_android.GetStateActorCurrent() != StateActor.Climbing && !((float)Random.Range(1, 100) > m_android.GetChangeToDodge() * 100f))
		{
			m_android.Dodge();
		}
	}
}
