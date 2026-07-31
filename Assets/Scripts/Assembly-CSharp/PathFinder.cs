using System.Collections.Generic;
using UnityEngine;

public class PathFinder
{
	private NPC m_npc;

	private List<PathNode> m_pathNodes = new List<PathNode>();

	public Path CreatePathToPlayer(NPC i_npc)
	{
		m_npc = i_npc;
		NavNode navNodeNpcStart = m_npc.gameObject.GetComponent<XAI>().GetNavNodeNpcStart();
		NavNode i_nodeDestination = ((!(m_npc is Flier)) ? GetNodeClosestToPlayerOnPlayerCurrentPlatform() : GetNodeClosestToPos(CommonReferences.Instance.GetPlayer().GetPos()));
		return CreatePathToNode(navNodeNpcStart, i_nodeDestination);
	}

	public Path CreatePathToNode(NPC i_npc, NavNode i_nodeDestination)
	{
		m_npc = i_npc;
		NavNode navNodeNpcStart = m_npc.gameObject.GetComponent<XAI>().GetNavNodeNpcStart();
		return CreatePathToNode(navNodeNpcStart, i_nodeDestination);
	}

	private Path CreatePathToNode(NavNode i_nodeOrigin, NavNode i_nodeDestination)
	{
		if (i_nodeOrigin == null || i_nodeDestination == null)
		{
			return null;
		}
		if ((!(m_npc is Flier) && i_nodeOrigin.IsFlyNode()) || (!(m_npc is Flier) && i_nodeDestination.IsFlyNode()))
		{
			return null;
		}
		if ((m_npc is Flier && !i_nodeOrigin.IsFlyNode()) || (m_npc is Flier && !i_nodeDestination.IsFlyNode()))
		{
			return null;
		}
		if (i_nodeOrigin == i_nodeDestination)
		{
			return new Path();
		}
		m_pathNodes.Clear();
		foreach (NavNode allNavNodesIncludingGeneratedNode in CommonReferences.Instance.GetManagerStages().GetStageCurrent().GetNavMap()
			.GetAllNavNodesIncludingGeneratedNodes())
		{
			if (allNavNodesIncludingGeneratedNode.IsFlyNode() && m_npc is Flier)
			{
				m_pathNodes.Add(new PathNode(allNavNodesIncludingGeneratedNode));
			}
			if (!allNavNodesIncludingGeneratedNode.IsFlyNode() && !(m_npc is Flier))
			{
				m_pathNodes.Add(new PathNode(allNavNodesIncludingGeneratedNode));
			}
		}
		List<PathNode> list = new List<PathNode>();
		List<PathNode> list2 = new List<PathNode>();
		list.Add(FindPathNodeByNavNode(i_nodeOrigin));
		int num = 100;
		int num2 = 0;
		bool flag = false;
		while (list.Count > 0)
		{
			num2++;
			if (num2 >= num)
			{
				Debug.Log("xxxxxxxx - Max iterations reached");
				break;
			}
			PathNode pathNode = list[0];
			foreach (PathNode item in list)
			{
				if (item.GetFCost() < pathNode.GetFCost() || (pathNode.GetFCost() == item.GetFCost() && item.GetGCost() < pathNode.GetGCost()))
				{
					pathNode = item;
				}
			}
			list.Remove(pathNode);
			list2.Add(pathNode);
			if (pathNode == FindPathNodeByNavNode(i_nodeDestination))
			{
				flag = true;
				break;
			}
			foreach (NodeConnection nodeConnection in pathNode.GetNavNode().GetNodeConnections())
			{
				PathNode pathNode2 = FindPathNodeByNavNode(nodeConnection.GetNodeDest());
				if (!IsNodeConnectionTraversable(nodeConnection) || list2.Contains(pathNode2))
				{
					continue;
				}
				float num3 = pathNode.GetGCost() + GetDistanceBetweenNodes(pathNode, pathNode2);
				if (num3 < pathNode.GetGCost() || !list.Contains(pathNode2))
				{
					pathNode2.SetGCost(num3);
					pathNode2.SetHCost(GetDistanceBetweenNodes(pathNode2, FindPathNodeByNavNode(i_nodeDestination)));
					pathNode2.SetParent(pathNode);
					pathNode2.SetNodeConnectionToParent(nodeConnection);
					if (!list.Contains(pathNode2))
					{
						list.Add(pathNode2);
					}
				}
			}
		}
		if (!flag)
		{
			return null;
		}
		return CreatePath(FindPathNodeByNavNode(i_nodeOrigin), FindPathNodeByNavNode(i_nodeDestination));
	}

	private Path CreatePath(PathNode i_pathNodeOrigin, PathNode i_pathNodeDest)
	{
		List<PathNode> list = new List<PathNode>();
		for (PathNode pathNode = i_pathNodeDest; pathNode != i_pathNodeOrigin; pathNode = pathNode.GetPathNodeParent())
		{
			list.Add(pathNode);
			if (pathNode.GetPathNodeParent() == null)
			{
				Debug.Log("!---- Null parent node: " + pathNode.GetNavNode().name);
				break;
			}
		}
		list.Add(i_pathNodeOrigin);
		list.Reverse();
		return new Path(list);
	}

	private bool IsNodeConnectionTraversable(NodeConnection i_nodeConnection)
	{
		if (!i_nodeConnection.IsConnectionOpen())
		{
			return false;
		}
		if (m_npc is Flier && (!i_nodeConnection.GetNodeOrigin().IsFlyNode() || !i_nodeConnection.GetNodeDest().IsFlyNode()))
		{
			return false;
		}
		if (m_npc is Walker)
		{
			if (i_nodeConnection.GetNodeOrigin().IsFlyNode() || i_nodeConnection.GetNodeDest().IsFlyNode())
			{
				return false;
			}
			if (!((Walker)m_npc).GetIsCanClimb() && i_nodeConnection.GetNodeConnectionType() == NodeConnectionType.Climb)
			{
				return false;
			}
		}
		return true;
	}

	private float GetDistanceBetweenNodes(PathNode i_pathNodeOrigin, PathNode i_pathNodeDest)
	{
		return Vector2.Distance(i_pathNodeOrigin.GetNavNode().GetPos(), i_pathNodeDest.GetNavNode().GetPos());
	}

	private PathNode FindPathNodeByNavNode(NavNode i_navNode)
	{
		for (int i = 0; i < m_pathNodes.Count; i++)
		{
			if (m_pathNodes[i].GetNavNode() == i_navNode)
			{
				return m_pathNodes[i];
			}
		}
		if (i_navNode.name == "nn_startNpc:" + m_npc.GetName() + m_npc.GetInstanceID())
		{
			for (int j = 0; j < m_pathNodes.Count; j++)
			{
				if (m_pathNodes[j].GetNavNode().name == "nn_startNpc:" + m_npc.GetName() + m_npc.GetInstanceID())
				{
					return m_pathNodes[j];
				}
			}
		}
		return null;
	}

	private NavNode GetNodeClosestToPlayerOnPlayerCurrentPlatform()
	{
		CommonReferences.Instance.GetPlayer().CheckCurrentPlatform();
		Platform platformCurrent = CommonReferences.Instance.GetPlayer().GetPlatformCurrent();
		List<NavNode> list = new List<NavNode>();
		foreach (NavNode allNavNode in CommonReferences.Instance.GetManagerStages().GetStageCurrent().GetNavMap()
			.GetAllNavNodes())
		{
			if (allNavNode.GetPlatform() == platformCurrent)
			{
				list.Add(allNavNode);
			}
		}
		NavNode navNode = null;
		Vector2 posHips = CommonReferences.Instance.GetPlayer().GetPosHips();
		foreach (NavNode item in list)
		{
			if (item.IsNpcStartNode() || (!(m_npc is Flier) && item.IsFlyNode()) || (m_npc is Flier && !item.IsFlyNode()))
			{
				continue;
			}
			Vector2 i_posOrigin = ((CommonReferences.Instance.GetPlayer().GetStateActorCurrent() != StateActor.Climbing) ? posHips : CommonReferences.Instance.GetPlayer().GetPos());
			if (IsLineUnobstructed(i_posOrigin, item.GetPos()))
			{
				if (navNode == null)
				{
					navNode = item;
				}
				else if (Vector2.Distance(item.GetPos(), posHips) < Vector2.Distance(navNode.GetPos(), posHips))
				{
					navNode = item;
				}
			}
		}
		return navNode;
	}

	private NavNode GetNodeClosestToPos(Vector2 i_pos)
	{
		NavNode navNode = null;
		foreach (NavNode allNavNode in CommonReferences.Instance.GetManagerStages().GetStageCurrent().GetNavMap()
			.GetAllNavNodes())
		{
			if ((m_npc is Flier || !allNavNode.IsFlyNode()) && (!(m_npc is Flier) || allNavNode.IsFlyNode()) && (!(m_npc is Flier) || IsLineUnobstructed(i_pos, allNavNode.GetPos())))
			{
				if (navNode == null)
				{
					navNode = allNavNode;
				}
				else if (Vector2.Distance(allNavNode.GetPos(), i_pos) < Vector2.Distance(navNode.GetPos(), i_pos))
				{
					navNode = allNavNode;
				}
			}
		}
		return navNode;
	}

	private bool IsLineUnobstructed(Vector2 i_posOrigin, Vector2 i_posDest)
	{
		Vector2 direction = i_posDest - i_posOrigin;
		direction.Normalize();
		float distance = Vector2.Distance(i_posOrigin, i_posDest);
		int mask = LayerMask.GetMask("Platform", "Interactable");
		RaycastHit2D[] array = Physics2D.RaycastAll(i_posOrigin, direction, distance, mask);
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].collider.gameObject.layer == LayerMask.NameToLayer("Interactable"))
			{
				if ((bool)array[i].collider.GetComponent<Interactable>() && array[i].collider.GetComponent<Interactable>().IsObstructionPaths())
				{
					return false;
				}
				continue;
			}
			return false;
		}
		return true;
	}
}
