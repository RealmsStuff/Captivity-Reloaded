using System.Collections.Generic;
using UnityEngine;

public class DefeatScene : MonoBehaviour
{
	[SerializeField]
	private List<DefeatSceneFrame> m_frames = new List<DefeatSceneFrame>();

	public DefeatSceneFrame GetFrame(int i_numFrame)
	{
		return m_frames[i_numFrame - 1];
	}

	public int GetNumOfFrames()
	{
		return m_frames.Count;
	}
}
