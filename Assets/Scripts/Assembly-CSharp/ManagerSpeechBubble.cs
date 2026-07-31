using System.Collections.Generic;
using UnityEngine;

public class ManagerSpeechBubble : MonoBehaviour
{
	[SerializeField]
	private SpeechBubble m_speechBubbleDefault;

	[SerializeField]
	private List<SpeechBubble> m_speechBubbles = new List<SpeechBubble>();

	public void CreateSpeechBubble(string i_text, SpeechBubbleTextColor i_colorText, Vector2 i_pos, bool i_isToLeft)
	{
		SpeechBubble speechBubble = Object.Instantiate(m_speechBubbleDefault, m_speechBubbleDefault.transform.parent);
		speechBubble.Initialize(i_text, i_colorText, i_pos, i_isToLeft);
		speechBubble.gameObject.SetActive(value: true);
		m_speechBubbles.Add(speechBubble);
	}

	public void RemoveSpeechBubbleFromList(SpeechBubble i_speechBubble)
	{
		m_speechBubbles.Remove(i_speechBubble);
	}

	public void ClearSpeechBubbles()
	{
		foreach (SpeechBubble speechBubble in m_speechBubbles)
		{
			m_speechBubbles.Remove(speechBubble);
			Object.Destroy(speechBubble.gameObject);
		}
	}
}
