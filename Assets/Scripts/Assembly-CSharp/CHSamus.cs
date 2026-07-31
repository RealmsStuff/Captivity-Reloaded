using System.Collections.Generic;
using UnityEngine;

public class CHSamus : Challenge
{
	[SerializeField]
	private int m_timesToDodge;

	private List<Sqoid> m_sqoidsHandled = new List<Sqoid>();

	private int m_timesDodged;

	protected override void HandleActivation()
	{
		m_timesDodged = 0;
		m_sqoidsHandled.Clear();
	}

	protected override void HandleDeActivation()
	{
		m_timesDodged = 0;
		m_sqoidsHandled.Clear();
	}

	protected override void TrackCompletion()
	{
	}

	public void DodgeEnergyBlast(Sqoid i_sqoid)
	{
		if (!m_sqoidsHandled.Contains(i_sqoid))
		{
			m_timesDodged++;
			m_sqoidsHandled.Add(i_sqoid);
			if (m_timesDodged >= m_timesToDodge)
			{
				Complete();
			}
		}
	}
}
