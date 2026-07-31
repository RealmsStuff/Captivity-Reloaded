using System.Collections.Generic;
using UnityEngine;

public class NavMap : MonoBehaviour
{
	private List<NavNode> m_nodes = new List<NavNode>();

	private Stage m_stage;

	private int l_numOfNodesCreated;

	private void Awake()
	{
		RetrieveAllNodesInChildren();
	}

	private void RetrieveAllNodesInChildren()
	{
		NavNode[] componentsInChildren = GetComponentsInChildren<NavNode>();
		foreach (NavNode item in componentsInChildren)
		{
			if (!m_nodes.Contains(item))
			{
				m_nodes.Add(item);
			}
		}
	}

	public void CheckForBadNodeConnections()
	{
		NodeConnection[] componentsInChildren = GetComponentsInChildren<NodeConnection>();
		foreach (NodeConnection nodeConnection in componentsInChildren)
		{
			if (nodeConnection.GetNodeDest() == null)
			{
				Debug.Log("!!! - " + nodeConnection.GetNodeOrigin().name + "'s nodedest is null!");
			}
		}
	}

	public void ShowNodes()
	{
		foreach (NavNode item in GetAllNavNodesGetComponent())
		{
			item.SetIsDrawNode(i_isDrawNode: true);
		}
	}

	public void HideNodes()
	{
		foreach (NavNode item in GetAllNavNodesGetComponent())
		{
			item.SetIsDrawNode(i_isDrawNode: false);
		}
	}

	private bool GetIsPlatformInBetweenOtherPlatform(Platform i_platformToCheck, Platform i_platformToCompare)
	{
		if (i_platformToCheck.GetLedges()[0].transform.position.x > i_platformToCompare.GetLedges()[0].transform.position.x && i_platformToCheck.GetLedges()[1].transform.position.x < i_platformToCompare.GetLedges()[1].transform.position.x)
		{
			return true;
		}
		return false;
	}

	public NavNode GetClosestNode(Vector2 i_pos)
	{
		NavNode navNode = null;
		foreach (NavNode allNavNode in GetAllNavNodes())
		{
			if (!navNode)
			{
				navNode = allNavNode;
			}
			else if (Vector2.Distance(i_pos, allNavNode.GetPos()) < Vector2.Distance(i_pos, navNode.GetPos()))
			{
				navNode = allNavNode;
			}
		}
		return navNode;
	}

	public List<NavNode> GetAllNavNodes()
	{
		List<NavNode> list = new List<NavNode>();
		for (int i = 0; i < m_nodes.Count; i++)
		{
			NavNode navNode = m_nodes[i];
			if (!navNode.IsNpcStartNode())
			{
				list.Add(navNode);
			}
		}
		return list;
	}

	public List<NavNode> GetAllNavNodesNoFly()
	{
		List<NavNode> list = new List<NavNode>();
		foreach (NavNode node in m_nodes)
		{
			if (!node.IsNpcStartNode() && !node.IsFlyNode())
			{
				list.Add(node);
			}
		}
		return list;
	}

	public List<NavNode> GetAllNavNodesOnlyFly()
	{
		List<NavNode> list = new List<NavNode>();
		foreach (NavNode node in m_nodes)
		{
			if (!node.IsNpcStartNode() && node.IsFlyNode())
			{
				list.Add(node);
			}
		}
		return list;
	}

	public List<NavNode> GetAllNavNodesIncludingGeneratedNodes()
	{
		return m_nodes;
	}

	private List<NavNode> GetAllNavNodesGetComponent()
	{
		List<NavNode> list = new List<NavNode>();
		NavNode[] componentsInChildren = GetComponentsInChildren<NavNode>();
		foreach (NavNode item in componentsInChildren)
		{
			list.Add(item);
		}
		return list;
	}

	public List<NavNode> GetFlyNodes()
	{
		List<NavNode> list = new List<NavNode>();
		foreach (NavNode allNavNode in GetAllNavNodes())
		{
			if (!allNavNode.IsNpcStartNode() && allNavNode.IsFlyNode())
			{
				list.Add(allNavNode);
			}
		}
		return list;
	}

	public NavNode CreateNode()
	{
		l_numOfNodesCreated++;
		GameObject obj = new GameObject("nn_");
		obj.transform.parent = base.transform;
		obj.transform.localPosition = Vector3.zero;
		obj.layer = LayerMask.NameToLayer("Node");
		NavNode navNode = obj.AddComponent<NavNode>();
		m_nodes.Add(navNode);
		navNode.SetIsDrawNode(i_isDrawNode: true);
		return navNode;
	}

	public NavNode CreateNpcStartNode(NPC i_npc)
	{
		NavNode navNode = CreateNode();
		navNode.name = "nn_startNpc:" + i_npc.GetName() + i_npc.GetInstanceID();
		navNode.SetIsNpcStartNode(i_isNpcStartNode: true);
		i_npc.CheckCurrentPlatform();
		navNode.transform.position = new Vector2(i_npc.GetPosFeet().x, i_npc.GetPlatformCurrent().GetPosTop().y + 0.25f);
		navNode.UpdatePlatform();
		NavNode closestNodeOnSamePlatform = GetClosestNodeOnSamePlatform(navNode, i_isCheckLeftSide: true);
		if (closestNodeOnSamePlatform != null)
		{
			navNode.CreateNodeConnection(closestNodeOnSamePlatform, NodeConnectionType.Move);
		}
		NavNode closestNodeOnSamePlatform2 = GetClosestNodeOnSamePlatform(navNode, i_isCheckLeftSide: false);
		if (closestNodeOnSamePlatform2 != null)
		{
			navNode.CreateNodeConnection(closestNodeOnSamePlatform2, NodeConnectionType.Move);
		}
		return navNode;
	}

	private NavNode GetClosestNodeOnSamePlatform(NavNode i_nodeToCheck, bool i_isCheckLeftSide)
	{
		NavNode navNode = null;
		foreach (NavNode allNavNode in GetAllNavNodes())
		{
			if (allNavNode == i_nodeToCheck || allNavNode.GetPlatform() != i_nodeToCheck.GetPlatform() || allNavNode.IsFlyNode() || !IsNodesUnobstructed(i_nodeToCheck, allNavNode))
			{
				continue;
			}
			bool flag = false;
			foreach (NodeConnection nodeConnection in i_nodeToCheck.GetNodeConnections())
			{
				if (nodeConnection.GetNodeDest() == allNavNode)
				{
					flag = true;
				}
			}
			if (flag)
			{
				continue;
			}
			if (i_isCheckLeftSide)
			{
				if (allNavNode.GetPos().x <= i_nodeToCheck.GetPos().x)
				{
					if (navNode == null)
					{
						navNode = allNavNode;
					}
					else if (Vector2.Distance(allNavNode.GetPos(), i_nodeToCheck.GetPos()) < Vector2.Distance(navNode.GetPos(), i_nodeToCheck.GetPos()))
					{
						navNode = allNavNode;
					}
				}
			}
			else if (allNavNode.GetPos().x > i_nodeToCheck.GetPos().x)
			{
				if (navNode == null)
				{
					navNode = allNavNode;
				}
				else if (Vector2.Distance(allNavNode.GetPos(), i_nodeToCheck.GetPos()) < Vector2.Distance(navNode.GetPos(), i_nodeToCheck.GetPos()))
				{
					navNode = allNavNode;
				}
			}
		}
		return navNode;
	}

	public NavNode CreateNpcStartNodeFlier(NPC i_npc)
	{
		NavNode navNode = CreateNode();
		navNode.name = "nn_startNpc:" + i_npc.GetName() + i_npc.GetInstanceID();
		navNode.SetIsNpcStartNode(i_isNpcStartNode: true);
		navNode.transform.position = i_npc.GetPos();
		navNode.SetIsFlyNode(i_isFlyNode: true);
		List<NavNode> allNodesFlyUnObstructed = GetAllNodesFlyUnObstructed(navNode);
		Flier flier = (Flier)i_npc;
		foreach (NavNode item in allNodesFlyUnObstructed)
		{
			if (flier.IsHasLineOfSightToPos(item.GetPos()))
			{
				navNode.CreateNodeConnection(item, NodeConnectionType.Move);
			}
		}
		return navNode;
	}

	private List<NavNode> GetAllNodesFlyUnObstructed(NavNode i_nodeOrigin)
	{
		List<NavNode> list = new List<NavNode>();
		foreach (NavNode allNavNode in GetAllNavNodes())
		{
			if (!(allNavNode == i_nodeOrigin) && allNavNode.IsFlyNode() && IsNodesUnobstructed(i_nodeOrigin, allNavNode))
			{
				list.Add(allNavNode);
			}
		}
		return list;
	}

	private bool IsNodesUnobstructed(NavNode i_nodeOrigin, NavNode i_nodeDest)
	{
		int mask = LayerMask.GetMask("Platform", "Interactable");
		Vector2 direction = i_nodeDest.GetPos() - i_nodeOrigin.GetPos();
		direction.Normalize();
		float distance = Vector2.Distance(i_nodeOrigin.GetPos(), i_nodeDest.GetPos());
		RaycastHit2D[] array = Physics2D.RaycastAll(i_nodeOrigin.GetPos(), direction, distance, mask);
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].collider.gameObject.layer == LayerMask.NameToLayer("Platform"))
			{
				return false;
			}
			if ((bool)array[i].collider.GetComponent<Interactable>() && array[i].collider.GetComponent<Interactable>().IsObstructionPaths())
			{
				return false;
			}
			if ((bool)array[i].collider.GetComponentInParent<Interactable>() && array[i].collider.GetComponentInParent<Interactable>().IsObstructionPaths())
			{
				return false;
			}
		}
		return true;
	}

	public void DestroyNode(NavNode i_nodeToDestroy)
	{
		m_nodes.Remove(i_nodeToDestroy);
		Object.Destroy(i_nodeToDestroy.gameObject);
	}
}
