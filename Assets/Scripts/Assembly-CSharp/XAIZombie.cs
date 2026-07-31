using System.Collections;
using UnityEngine;

public class XAIZombie : XAIWalker
{
	private Zombie m_zombie;

	private Coroutine m_coroutineTryChase;

	public override void Initialize(NPC i_npc)
	{
		base.Initialize(i_npc);
		m_zombie = (Zombie)i_npc;
		m_coroutineTryChase = StartCoroutine(CoroutineTryChase());
		m_npc.OnGetHit += TryFall;
		m_npc.OnDie += OnDie;
	}

	private void TryFall(Actor i_intiator, Actor i_victim)
	{
		if (m_zombie.GetStateActorCurrent() != StateActor.Ragdoll && !m_zombie.IsDead())
		{
			int num = 1;
			if (Random.Range(1, 101) > 100 - num)
			{
				m_zombie.Fall();
			}
		}
	}

	private IEnumerator CoroutineTryChase()
	{
		int l_chanceOfChase = 15;
		while (true)
		{
			if (!m_zombie.IsChasing() && Random.Range(1, 101) > 100 - l_chanceOfChase)
			{
				m_zombie.Chase();
			}
			yield return new WaitForSeconds(10f);
		}
	}

	protected override void OnDie()
	{
		base.OnDie();
		StopCoroutine(m_coroutineTryChase);
		m_npc.OnGetHit -= TryFall;
		m_npc.OnDie -= OnDie;
	}
}
