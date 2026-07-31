using System.Collections;
using UnityEngine;

public class XAIFlier : XAI
{
	protected Flier m_flier;

	public override void Initialize(NPC i_npc)
	{
		base.Initialize(i_npc);
		m_flier = (Flier)i_npc;
	}

	private IEnumerator CoroutineAwaitBeforeChase()
	{
		float seconds = Random.Range(0.25f, 4f);
		yield return new WaitForSeconds(seconds);
		m_isHasPlayerJustBeenRaped = false;
		m_coroutineAwaitBeforeChase = null;
	}

	private void InterruptAwaitBeforeChase()
	{
		StopCoroutine(m_coroutineAwaitBeforeChase);
		m_isHasPlayerJustBeenRaped = false;
		m_coroutineAwaitBeforeChase = null;
	}

	protected override void ChooseChaseDestination()
	{
		if (m_flier.IsHasLineOfSightToPlayer())
		{
			m_destination = m_player.GetPosHips();
			m_pathCurrent = null;
		}
		else
		{
			base.ChooseChaseDestination();
		}
	}

	protected override void HandleCombat()
	{
		if (m_npc.GetIsCanAttack() && m_npc.GetIsCloseEnoughToAttackPlayer())
		{
			m_npc.StartAttack();
		}
	}

	public override void CreateNavNodeNpcStart()
	{
		if (m_nodeStart != null)
		{
			CommonReferences.Instance.GetManagerStages().GetStageCurrent().GetNavMap()
				.DestroyNode(m_nodeStart);
		}
		m_nodeStart = CommonReferences.Instance.GetManagerStages().GetStageCurrent().GetNavMap()
			.CreateNpcStartNodeFlier(m_npc);
	}

	protected override void MoveToDestination()
	{
		if (m_destination == Vector2.zero)
		{
			return;
		}
		if (!m_flier.IsHasLineOfSightToPos(m_destination))
		{
			m_pathCurrent = null;
			m_destination = Vector2.zero;
			return;
		}
		m_flier.MoveTo(m_destination);
		if (Vector2.Distance(m_npc.GetPos(), m_destination) < m_radiusCompleteDestination)
		{
			m_npc.StopMoving();
		}
	}

	protected override void PerformStepAction()
	{
	}
}
