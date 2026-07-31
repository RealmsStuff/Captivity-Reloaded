using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SpeechBubble : MonoBehaviour
{
	[SerializeField]
	private Sprite m_sprLeft;

	[SerializeField]
	private Sprite m_sprRight;

	[SerializeField]
	private Text m_txt;

	[SerializeField]
	private Color m_colorPleasure1;

	[SerializeField]
	private Color m_colorPleasure2;

	[SerializeField]
	private Color m_colorPleasure3;

	[SerializeField]
	private Color m_colorPleasure4;

	[SerializeField]
	private Color m_colorOrgasm;

	private Vector2 m_spawnPointPos;

	private void OnEnable()
	{
		StartCoroutine(CoroutineAnimateFadeOut());
		StartCoroutine(CoroutineAnimateFloatUp());
	}

	private void Update()
	{
		KeepInCorrectPosX();
	}

	private void PlaceInWorldPos()
	{
		Vector2 anchoredPosition = RectTransformUtility.WorldToScreenPoint(CommonReferences.Instance.GetManagerCamerasXGame().GetCameraXGameCurrent().GetCameraUnity(), m_spawnPointPos) - CommonReferences.Instance.GetManagerHud().GetComponent<RectTransform>().sizeDelta / 2f;
		GetComponent<RectTransform>().anchoredPosition = anchoredPosition;
	}

	private void KeepInCorrectPosX()
	{
		Vector2 anchoredPosition = RectTransformUtility.WorldToScreenPoint(CommonReferences.Instance.GetManagerCamerasXGame().GetCameraXGameCurrent().GetCameraUnity(), m_spawnPointPos) - CommonReferences.Instance.GetManagerHud().GetComponent<RectTransform>().sizeDelta / 2f;
		anchoredPosition.y = GetComponent<RectTransform>().anchoredPosition.y;
		GetComponent<RectTransform>().anchoredPosition = anchoredPosition;
	}

	public void Initialize(string i_text, SpeechBubbleTextColor i_color, Vector2 i_spawnPointPos, bool i_left)
	{
		m_txt.text = i_text;
		m_spawnPointPos = i_spawnPointPos;
		switch (i_color)
		{
		case SpeechBubbleTextColor.Pleasure1:
			m_txt.color = m_colorPleasure1;
			break;
		case SpeechBubbleTextColor.Pleasure2:
			m_txt.color = m_colorPleasure2;
			break;
		case SpeechBubbleTextColor.Pleasure3:
			m_txt.color = m_colorPleasure3;
			break;
		case SpeechBubbleTextColor.Pleasure4:
			m_txt.color = m_colorPleasure4;
			break;
		case SpeechBubbleTextColor.Orgasm:
			m_txt.color = m_colorOrgasm;
			break;
		}
		if (i_left)
		{
			GetComponent<Image>().sprite = m_sprLeft;
		}
		else
		{
			GetComponent<Image>().sprite = m_sprRight;
		}
		PlaceInWorldPos();
	}

	private IEnumerator CoroutineAnimateFadeOut()
	{
		float num = 1f;
		if (m_txt.text.Length > 10)
		{
			num += 2f;
		}
		yield return new WaitForSeconds(num);
		float l_tranparencyFrom = GetComponent<Image>().color.a;
		float l_tranparencyTo = 0f;
		float l_timeToMove = 1f;
		float l_timeCurrent = 0f;
		while (l_timeCurrent < l_timeToMove)
		{
			l_timeCurrent += Time.fixedDeltaTime;
			float i_time = l_timeCurrent / l_timeToMove;
			float a = AnimationTools.CalculateOverTime(AnimationTools.Transition.Steep, AnimationTools.Transition.Steep, l_tranparencyFrom, l_tranparencyTo, i_time);
			GetComponent<Image>().color = new Color(GetComponent<Image>().color.r, GetComponent<Image>().color.g, GetComponent<Image>().color.b, a);
			m_txt.color = new Color(m_txt.color.r, m_txt.color.g, m_txt.color.b, a);
			yield return new WaitForFixedUpdate();
		}
		CommonReferences.Instance.GetManagerHud().GetManagerSpeechBubble().RemoveSpeechBubbleFromList(this);
		Object.Destroy(base.gameObject);
	}

	private IEnumerator CoroutineAnimateFloatUp()
	{
		float l_strengthMoveUp = 0.05f;
		float l_maxSpeedUp = 2f;
		if (m_txt.text.Length > 10)
		{
			l_strengthMoveUp = 0.025f;
			l_maxSpeedUp = 1f;
		}
		if (m_txt.text.Length > 15)
		{
			l_strengthMoveUp = 0.015f;
			l_maxSpeedUp = 0.75f;
		}
		if (m_txt.text.Length > 20)
		{
			l_strengthMoveUp = 0.01f;
			l_maxSpeedUp = 0.5f;
		}
		float l_amountToMoveUp = 0.2f;
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
}
