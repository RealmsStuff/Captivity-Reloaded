using System.Collections;
using UnityEngine;

public class XAIAbby : XAIAndroid
{
	private bool m_isRunningAway;

	public override void Initialize(NPC i_npc)
	{
		base.Initialize(i_npc);
		m_radiusCompleteDestination = 3f;
	}

	protected override void OnPlayerPlatformChange()
	{
		if (!m_isRunningAway)
		{
			base.OnPlayerPlatformChange();
		}
	}

	protected override void OnWalkerOPlatformChange()
	{
		if (!m_isRunningAway)
		{
			base.OnWalkerOPlatformChange();
		}
	}

	protected override void HandleCombat()
	{
		if (!m_isRunningAway)
		{
			base.HandleCombat();
		}
	}

	public void RunToRandomDestination()
	{
		if (m_player.GetStateActorCurrent() == StateActor.Ragdoll)
		{
			m_isRunningAway = false;
			return;
		}
		m_isRunningAway = true;
		StartCoroutine(CoroutineWaitEndRunAway());
	}

	private IEnumerator CoroutineWaitEndRunAway()
	{
		yield return new WaitForSeconds(6f);
		m_isRunningAway = false;
		m_pathCurrent = GetPathToPlayer(i_isForceGetPath: true);
	}

	protected override void ChooseChaseDestination()
	{
		if (m_isRunningAway)
		{
			m_destination = Vector2.zero;
			if (m_pathCurrent == null || m_pathCurrent.GetPathNodeCurrent() == null)
			{
				m_pathCurrent = GetPathToRandomAccesibleNode();
			}
			if (m_pathCurrent != null)
			{
				m_destination = m_pathCurrent.GetPathNodeCurrent().GetNavNode().GetPos();
			}
		}
		else
		{
			base.ChooseChaseDestination();
		}
	}
}
