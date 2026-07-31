using UnityEngine;

public class XAIAndroidCrawler : XAIAndroid
{
	public override void HandleIntelligence()
	{
		HandleStateChase();
	}

	protected override void ChooseChaseDestination()
	{
		if (m_pathCurrent == null || m_pathCurrent.GetPathNodeCurrent() == null)
		{
			m_pathCurrent = GetPathToRandomAccesibleNode();
		}
		if (m_pathCurrent != null && m_pathCurrent.GetPathNodeCurrent() != null)
		{
			m_destination = m_pathCurrent.GetPathNodeCurrent().GetNavNode().GetPos();
		}
		else
		{
			m_destination = Vector2.zero;
		}
	}
}
