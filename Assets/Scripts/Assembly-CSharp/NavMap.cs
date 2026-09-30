using System.Collections.Generic;
using UnityEngine;

public class NavMap : MonoBehaviour
{
	private List<NavNode> m_nodes = new List<NavNode>();

	private Stage m_stage;

	private int l_numOfNodesCreated;

	private bool m_isModPlatformNetwork;

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

	public void ConnectNodeToPlatformNetwork(NavNode i_node)
	{
		if (i_node == null || i_node.IsFlyNode() || i_node.GetPlatform() == null) return;
		ConnectNodesBothWays(i_node, GetClosestNodeOnSamePlatform(i_node, i_isCheckLeftSide: true));
		ConnectNodesBothWays(i_node, GetClosestNodeOnSamePlatform(i_node, i_isCheckLeftSide: false));
	}

	private static void ConnectNodesBothWays(NavNode i_first, NavNode i_second)
	{
		if (i_first == null || i_second == null || i_first == i_second) return;
		if (!HasConnection(i_first, i_second))
			i_first.CreateNodeConnection(i_second, NodeConnectionType.Move);
		if (!HasConnection(i_second, i_first))
			i_second.CreateNodeConnection(i_first, NodeConnectionType.Move);
	}

	private static bool HasConnection(NavNode i_origin, NavNode i_destination)
	{
		foreach (NodeConnection connection in i_origin.GetNodeConnections())
			if (connection != null && connection.GetNodeDest() == i_destination) return true;
		return false;
	}

	public void CopyModRuntimeSettingsFrom(NavMap i_source,
		Dictionary<UnityEngine.Object, UnityEngine.Object> i_counterparts)
	{
		m_isModPlatformNetwork = i_source.m_isModPlatformNetwork;
		m_nodes.Clear();
		foreach (NavNode node in i_source.m_nodes)
			if (node != null && i_counterparts.TryGetValue(node, out UnityEngine.Object counterpart)
				&& counterpart is NavNode clonedNode) m_nodes.Add(clonedNode);
	}

	public void ConfigureModPlatforms(IEnumerable<Platform> i_platforms)
	{
		m_isModPlatformNetwork = true;
		foreach (NavNode inherited in GetComponentsInChildren<NavNode>(includeInactive: true))
			inherited.gameObject.SetActive(false);
		m_nodes.Clear();
		foreach (Platform platform in i_platforms)
		{
			if (platform == null) continue;
			BoxCollider2D collider = platform.GetComponent<BoxCollider2D>();
			if (collider == null) continue;
			float left = platform.transform.position.x + collider.offset.x - collider.size.x * 0.5f + 0.25f;
			float right = platform.transform.position.x + collider.offset.x + collider.size.x * 0.5f - 0.25f;
			float top = platform.transform.position.y + collider.offset.y + collider.size.y * 0.5f + 0.25f;
			NavNode previous = null;
			foreach (float x in new[] { left, (left + right) * 0.5f, right })
			{
				NavNode node = CreateNode();
				node.transform.position = new Vector2(x, top);
				node.SetPlatform(platform);
				if (previous != null)
				{
					previous.CreateNodeConnection(node, NodeConnectionType.Move);
					node.CreateNodeConnection(previous, NodeConnectionType.Move);
				}
				previous = node;
			}
		}
	}

	public void FinalizeModPlatformConnections()
	{
		if (!m_isModPlatformNetwork) return;
		// Mod stages are built while inactive. Repeat platform discovery and the
		// local walking links after Stage.OpenStage activates their colliders.
		foreach (NavNode node in GetAllNavNodes())
		{
			if (node == null || node.IsFlyNode()) continue;
			node.UpdatePlatform();
		}
		foreach (NavNode node in GetAllNavNodes())
			ConnectNodeToPlatformNetwork(node);
	}

	public NavNode CreateNpcStartNode(NPC i_npc)
	{
		NavNode navNode = CreateNode();
		navNode.name = "nn_startNpc:" + i_npc.GetName() + i_npc.GetInstanceID();
		navNode.SetIsNpcStartNode(i_isNpcStartNode: true);
		Vector2 feet = i_npc.GetPosFeet();
		RaycastHit2D ground = Physics2D.Raycast(feet + Vector2.up * 0.5f, Vector2.down,
			Mathf.Infinity, LayerMask.GetMask("Platform"));
		if (ground)
		{
			navNode.transform.position = ground.point + Vector2.up * 0.25f;
		}
		else
		{
			NavNode closest = GetClosestNode(feet);
			navNode.transform.position = closest == null ? feet : closest.GetPos();
		}
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
