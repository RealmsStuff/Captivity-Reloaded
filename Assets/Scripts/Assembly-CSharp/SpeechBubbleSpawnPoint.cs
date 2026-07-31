using UnityEngine;

public class SpeechBubbleSpawnPoint : MonoBehaviour
{
	[SerializeField]
	private bool m_isToRight;

	public bool GetIsToRight()
	{
		return m_isToRight;
	}
}
