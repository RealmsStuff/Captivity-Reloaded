using UnityEngine;

public class AndroidStatuePose : MonoBehaviour
{
	[SerializeField]
	private string m_nameStateAnimPose;

	[SerializeField]
	private int m_numWave;

	public Vector2 GetPos()
	{
		return base.transform.position;
	}

	public string GetNameStateAnimPose()
	{
		return m_nameStateAnimPose;
	}

	public int GetNumWave()
	{
		return m_numWave;
	}
}
