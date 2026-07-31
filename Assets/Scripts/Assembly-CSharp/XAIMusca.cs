using UnityEngine;

public class XAIMusca : XAIFlier
{
	private Musca m_musca;

	private NavNode m_nodeNest;

	public override void Initialize(NPC i_npc)
	{
		base.Initialize(i_npc);
		m_musca = (Musca)i_npc;
	}

	public override void HandleIntelligence()
	{
		if (m_musca.IsCarryingPlayerToNest())
		{
			CarryToNest();
			CheckIfArrivedAtNest();
		}
		else
		{
			base.HandleIntelligence();
		}
	}

	public void CarryToNest()
	{
		if (m_pathCurrent == null)
		{
			foreach (NavNode flyNode in CommonReferences.Instance.GetManagerStages().GetStageCurrent().GetNavMap()
				.GetFlyNodes())
			{
				if (flyNode.name == "NavNodeAirNestMain")
				{
					m_nodeNest = flyNode;
				}
			}
			m_pathCurrent = new PathFinder().CreatePathToNode(m_flier, m_nodeNest);
		}
		try
		{
			m_destination = m_pathCurrent.GetPathNodeCurrent().GetNavNode().GetPos();
			MoveToDestination();
			HandlePath();
		}
		catch
		{
			m_musca.DropPlayer();
		}
	}

	private void CheckIfArrivedAtNest()
	{
		if (Vector2.Distance(m_npc.GetPos(), m_nodeNest.GetPos()) <= m_radiusCompleteDestination)
		{
			m_musca.DropPlayer();
		}
	}
}
