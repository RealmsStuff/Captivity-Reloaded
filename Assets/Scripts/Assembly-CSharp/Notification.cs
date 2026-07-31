using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Notification : MonoBehaviour
{
	public delegate void DelDestroy(Notification i_notification);

	[SerializeField]
	private Text m_txt;

	private int m_numPosY;

	public event DelDestroy OnDestroy;

	public void Initialize(string i_text, Color i_colorText, int i_numPosY, bool i_isContinues)
	{
		m_txt.text = i_text;
		m_txt.color = i_colorText;
		m_numPosY = i_numPosY;
		if (!i_isContinues)
		{
			StartCoroutine(CoroutineAnimateFloatUp(2f));
			StartCoroutine(CoroutineAnimateFadeOut(2f));
		}
	}

	public int GetNumPosY()
	{
		return m_numPosY;
	}

	private IEnumerator CoroutineAnimateFadeOut(float i_delay)
	{
		yield return new WaitForSeconds(i_delay);
		float l_tranparencyFrom = 1f;
		float l_tranparencyTo = 0f;
		float l_timeToMove = 0.5f;
		float l_timeCurrent = 0f;
		while (l_timeCurrent < l_timeToMove)
		{
			l_timeCurrent += Time.fixedDeltaTime;
			float i_time = l_timeCurrent / l_timeToMove;
			float a = AnimationTools.CalculateOverTime(AnimationTools.Transition.Steep, AnimationTools.Transition.Steep, l_tranparencyFrom, l_tranparencyTo, i_time);
			m_txt.color = new Color(m_txt.color.r, m_txt.color.g, m_txt.color.b, a);
			yield return new WaitForFixedUpdate();
		}
		CommonReferences.Instance.GetManagerHud().GetManagerNotification().DestroyNotification(this);
	}

	private IEnumerator CoroutineAnimateFloatUp(float i_delay)
	{
		yield return new WaitForSeconds(i_delay);
		float l_amountToMoveUp = 0f;
		float l_durationToMove = 0.5f;
		float l_durationPassed = 0f;
		while (l_durationPassed < l_durationToMove)
		{
			Vector2 anchoredPosition = GetComponent<RectTransform>().anchoredPosition;
			anchoredPosition.y += l_amountToMoveUp;
			GetComponent<RectTransform>().anchoredPosition = anchoredPosition;
			l_amountToMoveUp += 0.05f;
			yield return new WaitForFixedUpdate();
		}
	}
}
