using System.Collections.Generic;
using UnityEngine;

public class NavNode : MonoBehaviour
{
	[SerializeField]
	private Platform m_platformConnected;

	[SerializeField]
	private bool m_isFlyNode;

	[SerializeField]
	private bool m_isDrawNode;

	private bool m_isNpcStartNode;

	private List<NodeConnection> m_nodeConnections = new List<NodeConnection>();

	private void Awake()
	{
		base.gameObject.AddComponent<CircleCollider2D>();
		UpdateRetrieveNodeConnections();
		UpdatePlatform();
	}

	public void UpdateRetrieveNodeConnections()
	{
		NodeConnection[] componentsInChildren = GetComponentsInChildren<NodeConnection>();
		foreach (NodeConnection nodeConnection in componentsInChildren)
		{
			if (nodeConnection.transform.parent == base.transform && !m_nodeConnections.Contains(nodeConnection))
			{
				m_nodeConnections.Add(nodeConnection);
			}
		}
		foreach (NodeConnection nodeConnection2 in m_nodeConnections)
		{
			if (nodeConnection2 == null)
			{
				m_nodeConnections.Remove(nodeConnection2);
			}
		}
	}

	public void UpdatePlatform()
	{
		int mask = LayerMask.GetMask("Platform");
		RaycastHit2D raycastHit2D = Physics2D.Raycast(GetPos(), Vector2.down, 6f, mask);
		if ((bool)raycastHit2D && (bool)raycastHit2D.collider.gameObject.GetComponent<Platform>())
		{
			m_platformConnected = raycastHit2D.collider.gameObject.GetComponent<Platform>();
		}
	}

	public NodeConnection CreateNodeConnection(NavNode i_nodeDest, NodeConnectionType i_nodeConnectionType)
	{
		GameObject gameObject = new GameObject();
		gameObject.transform.parent = base.transform;
		gameObject.transform.localPosition = Vector3.zero;
		gameObject.AddComponent<NodeConnection>();
		switch (i_nodeConnectionType)
		{
		case NodeConnectionType.Move:
			gameObject.GetComponent<NodeConnection>().SetNodeConnectionType(NodeConnectionType.Move);
			gameObject.name = "nc_move";
			break;
		case NodeConnectionType.Climb:
			gameObject.GetComponent<NodeConnection>().SetNodeConnectionType(NodeConnectionType.Climb);
			gameObject.name = "nc_climb";
			break;
		}
		gameObject.GetComponent<NodeConnection>().SetNodeDest(i_nodeDest);
		m_nodeConnections.Add(gameObject.GetComponent<NodeConnection>());
		return gameObject.GetComponent<NodeConnection>();
	}

	public Vector2 GetPos()
	{
		return base.transform.position;
	}

	public List<NodeConnection> GetNodeConnections()
	{
		return m_nodeConnections;
	}

	public bool IsFlyNode()
	{
		return m_isFlyNode;
	}

	public void SetIsFlyNode(bool i_isFlyNode)
	{
		m_isFlyNode = i_isFlyNode;
	}

	public Platform GetPlatform()
	{
		return m_platformConnected;
	}

	private void OnDrawGizmos()
	{
		if (m_isDrawNode)
		{
			if (m_isFlyNode)
			{
				Gizmos.color = new Color(0.25f, 0.5f, 1f);
			}
			else
			{
				Gizmos.color = new Color(1f, 0f, 0f);
			}
			Gizmos.DrawSphere(new Vector3(GetPos().x, GetPos().y, 1f), 0.5f);
		}
	}

	public bool IsDrawNode()
	{
		return m_isDrawNode;
	}

	public void SetIsDrawNode(bool i_isDrawNode)
	{
		m_isDrawNode = i_isDrawNode;
	}

	public bool IsNpcStartNode()
	{
		return m_isNpcStartNode;
	}

	public void SetIsNpcStartNode(bool i_isNpcStartNode)
	{
		m_isNpcStartNode = i_isNpcStartNode;
	}
}
