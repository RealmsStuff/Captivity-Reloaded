using System.Collections;
using UnityEngine;

public class XAIWalker : XAI
{
	private Walker m_walker;

	private Coroutine m_coroutineWaitForClimb;

	public override void Initialize(NPC i_npc)
	{
		base.Initialize(i_npc);
		m_walker = (Walker)i_npc;
		m_npc.OnPlatformChange += OnWalkerOPlatformChange;
		CommonReferences.Instance.GetPlayer().OnPlatformChange += OnPlayerPlatformChange;
	}

	protected virtual void OnWalkerOPlatformChange()
	{
		if (m_xAIStateCurrent != XAIState.Linger && !m_npc.IsDead())
		{
			m_pathCurrent = GetPathToPlayer(i_isForceGetPath: true);
		}
	}

	protected virtual void OnPlayerPlatformChange()
	{
		if (m_xAIStateCurrent != XAIState.Linger && !m_npc.IsDead())
		{
			m_pathCurrent = GetPathToPlayer(i_isForceGetPath: true);
		}
	}

	protected override void HandleStateAwait()
	{
		if (!(m_npc.GetPlatformCurrent() != m_player.GetPlatformCurrent()))
		{
			m_npc.IsHasLineOfSightToPlayer();
		}
		base.HandleStateAwait();
	}

	protected override void HandleAwaitDistance()
	{
		base.HandleAwaitDistance();
	}

	protected override void ChooseChaseDestination()
	{
		if (m_player.GetPlatformCurrent() == m_npc.GetPlatformCurrent() && m_npc.IsHasLineOfSightToPlayer())
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
		if (m_npc.GetIsCanAttack() && !m_npc.GetIsAttacking() && !(m_npc.GetPlatformCurrent() != m_player.GetPlatformCurrent()) && m_npc.GetStateActorCurrent() != StateActor.Jumping && m_npc.GetIsCloseEnoughToAttackPlayer())
		{
			m_npc.StartAttack();
		}
	}

	protected override void MoveToDestination()
	{
		if (!(m_destination == Vector2.zero) && !(Vector2.Distance(m_npc.GetPosFeet(), m_destination) < m_radiusCompleteDestination))
		{
			bool i_left = ((m_npc.GetPos().x > m_destination.x) ? true : false);
			m_npc.MoveHorizontal(i_left);
		}
	}

	protected override void CompletePathNodeCurrent()
	{
		HandleVelocityAfterNodeCompletion();
		base.CompletePathNodeCurrent();
	}

	private void HandleVelocityAfterNodeCompletion()
	{
		if (m_pathCurrent != null && m_pathCurrent.GetPathNodeCurrent() != null)
		{
			Vector2 pos = m_pathCurrent.GetPathNodeCurrent().GetNavNode().GetPos();
			if (m_pathCurrent.GetPathNodeCurrent().GetNavNode().IsNpcStartNode())
			{
				return;
			}
			if (pos.x < m_npc.GetPos().x)
			{
				if (m_npc.GetVelocity().x > 0f)
				{
					m_npc.StopMovingHorizontally();
				}
			}
			else if (m_npc.GetVelocity().x < 0f)
			{
				m_npc.StopMovingHorizontally();
			}
		}
		else
		{
			m_npc.StopMovingHorizontally();
		}
	}

	protected override void PerformStepAction()
	{
		if (m_pathCurrent.GetPathNodeCurrent() == null || m_pathCurrent.GetPathNodeCurrent().GetNodeConnectionToParent() == null)
		{
			return;
		}
		switch (m_pathCurrent.GetPathNodeCurrent().GetNodeConnectionToParent().GetNodeConnectionType())
		{
		case NodeConnectionType.Climb:
		{
			m_walker.StopMoving();
			Vector2 pos = m_pathCurrent.GetPathNodeCurrent().GetNavNode().GetPos();
			pos.y -= m_walker.GetHeight() / 2f;
			m_walker.JumpZeroGravity(pos, 0.5f);
			m_walker.TryToClimb(CommonReferences.Instance.GetManagerStages().GetStageCurrent().GetLedgeClosestToPos(pos));
			if (m_coroutineWaitForClimb != null)
			{
				StopCoroutine(m_coroutineWaitForClimb);
			}
			m_coroutineWaitForClimb = StartCoroutine(CoroutineWaitForClimb(pos));
			break;
		}
		case NodeConnectionType.Move:
		case NodeConnectionType.Jump:
			break;
		}
	}

	private IEnumerator CoroutineWaitForClimb(Vector2 i_posClimb)
	{
		yield return new WaitForSeconds(0.5f);
		if (m_npc.GetStateActorCurrent() != StateActor.Climbing)
		{
			m_npc.PlaceHeadOnPos(i_posClimb);
		}
	}

	protected override void OnDie()
	{
		base.OnDie();
		m_npc.OnPlatformChange -= OnWalkerOPlatformChange;
		CommonReferences.Instance.GetPlayer().OnPlatformChange -= OnPlayerPlatformChange;
	}

	public override void CreateNavNodeNpcStart()
	{
		if (m_nodeStart != null)
		{
			CommonReferences.Instance.GetManagerStages().GetStageCurrent().GetNavMap()
				.DestroyNode(m_nodeStart);
		}
		m_nodeStart = CommonReferences.Instance.GetManagerStages().GetStageCurrent().GetNavMap()
			.CreateNpcStartNode(m_npc);
	}
}
