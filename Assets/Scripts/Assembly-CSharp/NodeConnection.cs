using System.Collections;
using UnityEngine;

public class NodeConnection : MonoBehaviour
{
	private NavNode m_nodeOrigin;

	[SerializeField]
	private NavNode m_nodeDest;

	[SerializeField]
	private NodeConnectionType m_nodeConnectionType;

	[SerializeField]
	private float m_cost01;

	private bool m_isConnectionOpen = true;

	private float m_walkCost;

	private float m_jumpCost;

	private float m_climbCost;

	private void Awake()
	{
		RetrieveNodeOrigin();
		switch (m_nodeConnectionType)
		{
		case NodeConnectionType.Move:
			m_cost01 = m_walkCost;
			break;
		case NodeConnectionType.Jump:
			m_cost01 = m_jumpCost;
			break;
		case NodeConnectionType.Climb:
			m_cost01 = m_climbCost;
			break;
		}
	}

	private void RetrieveNodeOrigin()
	{
		m_nodeOrigin = GetComponentInParent<NavNode>();
	}

	private void Start()
	{
		m_isConnectionOpen = true;
		if (m_nodeConnectionType == NodeConnectionType.Move)
		{
			StartCoroutine(CoroutineCheckIfConnectionIsOpen());
		}
	}

	private IEnumerator CoroutineCheckIfConnectionIsOpen()
	{
		yield return new WaitForFixedUpdate();
		if (m_nodeDest == null)
		{
			Debug.Log("!Destroyed NodeConnection: node connection has no destination, node origin: " + m_nodeOrigin.name);
			Object.Destroy(base.gameObject);
		}
		while (true)
		{
			m_isConnectionOpen = IsLineUnObstructed();
			yield return new WaitForSeconds(1f);
		}
	}

	private bool IsLineUnObstructed()
	{
		if (m_nodeConnectionType == NodeConnectionType.Climb)
		{
			return true;
		}
		Vector2 direction = m_nodeDest.GetPos() - m_nodeOrigin.GetPos();
		direction.Normalize();
		float distance = Vector2.Distance(m_nodeOrigin.GetPos(), m_nodeDest.GetPos());
		int mask = LayerMask.GetMask("Platform", "Interactable");
		RaycastHit2D[] array = Physics2D.RaycastAll(m_nodeOrigin.GetPos(), direction, distance, mask);
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].collider.gameObject.layer == LayerMask.NameToLayer("Platform"))
			{
				return false;
			}
			if (array[i].collider.gameObject.layer == LayerMask.NameToLayer("Interactable"))
			{
				Interactable component = array[i].collider.GetComponent<Interactable>();
				if (component != null && component.IsObstructionPaths())
				{
					return false;
				}
				component = array[i].collider.GetComponentInParent<Interactable>();
				if (component != null && component.IsObstructionPaths())
				{
					return false;
				}
			}
		}
		return true;
	}

	public NavNode GetNodeOrigin()
	{
		return m_nodeOrigin;
	}

	public NavNode GetNodeDest()
	{
		return m_nodeDest;
	}

	public void SetNodeDest(NavNode i_nodeDest)
	{
		m_nodeDest = i_nodeDest;
	}

	public void SetNodeConnectionType(NodeConnectionType i_type)
	{
		m_nodeConnectionType = i_type;
	}

	public NodeConnectionType GetNodeConnectionType()
	{
		return m_nodeConnectionType;
	}

	public bool IsConnectionOpen()
	{
		return m_isConnectionOpen;
	}

	public float GetCost01()
	{
		return m_cost01;
	}

	public float GetDistanceBetweenNodes()
	{
		return Vector2.Distance(m_nodeOrigin.GetPos(), m_nodeDest.GetPos());
	}

	private void OnDrawGizmos()
	{
		RetrieveNodeOrigin();
		if (m_nodeOrigin.IsDrawNode() && !(m_nodeDest == null))
		{
			switch (m_nodeConnectionType)
			{
			case NodeConnectionType.Move:
				DrawMoveLine();
				break;
			case NodeConnectionType.Jump:
				Gizmos.color = Color.blue;
				break;
			case NodeConnectionType.Climb:
				DrawClimbLine();
				break;
			}
		}
	}

	private void DrawMoveLine()
	{
		if (m_nodeOrigin.IsFlyNode() || m_nodeDest.IsFlyNode())
		{
			Gizmos.color = new Color(0.25f, 0.5f, 1f);
		}
		else
		{
			Gizmos.color = Color.red;
		}
		if (!IsLineUnObstructed())
		{
			Gizmos.color = new Color(Gizmos.color.r / 3f, Gizmos.color.g / 3f, Gizmos.color.b / 3f, 1f);
		}
		Gizmos.DrawLine(m_nodeOrigin.GetPos(), m_nodeDest.GetPos());
		Vector2 vector = m_nodeDest.GetPos() - m_nodeOrigin.GetPos();
		vector.Normalize();
		Gizmos.DrawWireSphere(m_nodeDest.GetPos() - vector * 0.5f, 0.33f);
	}

	private void DrawClimbLine()
	{
		Gizmos.color = Color.yellow;
		Vector2 pos = m_nodeOrigin.GetPos();
		Vector2 pos2 = m_nodeDest.GetPos();
		Gizmos.DrawLine(pos, pos2);
		Vector2 vector = pos2 - pos;
		vector.Normalize();
		Gizmos.DrawWireSphere(pos2 - vector * 0.5f, 0.33f);
	}
}
