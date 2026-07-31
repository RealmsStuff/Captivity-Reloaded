using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MoneyHudGainItem : MonoBehaviour
{
	[SerializeField]
	private Text m_txt;

	[SerializeField]
	private Text m_txtBg;

	[SerializeField]
	private Color m_colorGain;

	[SerializeField]
	private Color m_colorLose;

	private void OnEnable()
	{
		StartCoroutine(CoroutineAnimateFloatUp());
		StartCoroutine(CoroutineAnimateFadeOut());
	}

	public void Initialize(int i_amountMoney, bool i_isGain)
	{
		if (i_isGain)
		{
			m_txt.color = m_colorGain;
			m_txt.text = "+ $" + i_amountMoney;
		}
		else
		{
			m_txt.color = m_colorLose;
			m_txt.text = "- $" + i_amountMoney;
		}
		m_txtBg.text = m_txt.text;
		m_txtBg.color = new Color(m_txt.color.r / 2f, m_txt.color.g / 2f, m_txt.color.b / 2f);
	}

	private IEnumerator CoroutineAnimateFloatUp()
	{
		float l_strengthMoveUp = 0.05f;
		float l_maxSpeedUp = 1f;
		float l_amountToMoveUp = 0.05f;
		float l_durationToMove = 1f;
		float l_durationPassed = 0f;
		while (l_durationPassed < l_durationToMove)
		{
			Vector2 anchoredPosition = GetComponent<RectTransform>().anchoredPosition;
			anchoredPosition.y += l_amountToMoveUp;
			GetComponent<RectTransform>().anchoredPosition = anchoredPosition;
			l_amountToMoveUp += l_strengthMoveUp;
			if (l_amountToMoveUp > l_maxSpeedUp)
			{
				l_amountToMoveUp = l_maxSpeedUp;
			}
			yield return new WaitForFixedUpdate();
		}
	}

	private IEnumerator CoroutineAnimateFadeOut()
	{
		float seconds = 0.25f;
		yield return new WaitForSeconds(seconds);
		float l_tranparencyFrom = 1f;
		float l_tranparencyTo = 0f;
		float l_timeToMove = 1f;
		float l_timeCurrent = 0f;
		while (l_timeCurrent < l_timeToMove)
		{
			l_timeCurrent += Time.fixedDeltaTime;
			float i_time = l_timeCurrent / l_timeToMove;
			float a = AnimationTools.CalculateOverTime(AnimationTools.Transition.Steep, AnimationTools.Transition.Steep, l_tranparencyFrom, l_tranparencyTo, i_time);
			m_txt.color = new Color(m_txt.color.r, m_txt.color.g, m_txt.color.b, a);
			m_txtBg.color = new Color(m_txtBg.color.r, m_txtBg.color.g, m_txtBg.color.b, a);
			yield return new WaitForFixedUpdate();
		}
		Object.Destroy(base.gameObject);
	}
}
