using System.Collections.Generic;

public class Path
{
	private List<PathNode> m_pathNodes = new List<PathNode>();

	public Path()
	{
	}

	public Path(List<PathNode> i_pathNodes)
	{
		m_pathNodes = i_pathNodes;
	}

	public List<PathNode> GetPathNodes()
	{
		return m_pathNodes;
	}

	public PathNode GetPathNodeCurrent()
	{
		foreach (PathNode pathNode in m_pathNodes)
		{
			if (!pathNode.IsCompleted())
			{
				return pathNode;
			}
		}
		return null;
	}
}
