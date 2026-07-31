public class PathNode
{
	private NavNode m_node;

	private float m_gCost;

	private float m_hCost;

	private PathNode m_pathNodeParent;

	private NodeConnection m_nodeConnectionToParent;

	private bool m_isCompleted;

	public PathNode(NavNode i_node)
	{
		m_node = i_node;
	}

	public NavNode GetNavNode()
	{
		return m_node;
	}

	public float GetGCost()
	{
		return m_gCost;
	}

	public void SetGCost(float i_gCost)
	{
		m_gCost = i_gCost;
	}

	public float GetHCost()
	{
		return m_hCost;
	}

	public void SetHCost(float i_hCost)
	{
		m_hCost = i_hCost;
	}

	public float GetFCost()
	{
		return m_gCost + m_hCost;
	}

	public PathNode GetPathNodeParent()
	{
		return m_pathNodeParent;
	}

	public void SetParent(PathNode i_pathNodeParent)
	{
		m_pathNodeParent = i_pathNodeParent;
	}

	public NodeConnection GetNodeConnectionToParent()
	{
		return m_nodeConnectionToParent;
	}

	public void SetNodeConnectionToParent(NodeConnection i_nodeConnection)
	{
		m_nodeConnectionToParent = i_nodeConnection;
	}

	public bool IsCompleted()
	{
		return m_isCompleted;
	}

	public void Complete()
	{
		m_isCompleted = true;
	}
}
