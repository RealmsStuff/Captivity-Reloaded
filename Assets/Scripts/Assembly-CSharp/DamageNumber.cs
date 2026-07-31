using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DamageNumber : MonoBehaviour
{
	[SerializeField]
	private Image m_img;

	[SerializeField]
	private Sprite m_sprX075;

	[SerializeField]
	private Sprite m_sprX1;

	[SerializeField]
	private Sprite m_sprX2;

	[SerializeField]
	private Sprite m_sprBlock;

	[SerializeField]
	private Color m_colorLowDamage;

	[SerializeField]
	private Color m_colorNormalDamage;

	[SerializeField]
	private Color m_colorCrit;

	[SerializeField]
	private Color m_colorBlock;

	private Vector2 m_spawnPointPos;

	private void OnEnable()
	{
		if (m_img.color == m_colorCrit)
		{
			StartCoroutine(CoroutineAnimateColorDance());
		}
		StartCoroutine(CoroutineAnimateFadeOut());
		StartCoroutine(CoroutineAnimateFloatUp());
		StartCoroutine(CoroutineAnimatePopupAndShrink());
	}

	private void Update()
	{
		PlaceInWorldPos();
	}

	public void Initialize(float i_dmgNum, Vector2 i_posSpawn)
	{
		m_spawnPointPos = i_posSpawn;
		if (i_dmgNum < 1f && i_dmgNum > 0f)
		{
			m_img.sprite = m_sprX075;
			m_img.color = m_colorLowDamage;
			m_img.GetComponent<RectTransform>().sizeDelta *= 2f;
		}
		if (i_dmgNum == 1f)
		{
			m_img.sprite = m_sprX1;
			m_img.color = m_colorNormalDamage;
			m_img.GetComponent<RectTransform>().sizeDelta *= 2f;
		}
		if (i_dmgNum == 2f)
		{
			m_img.sprite = m_sprX2;
			m_img.color = m_colorCrit;
			m_img.GetComponent<RectTransform>().sizeDelta *= 3f;
		}
		if (i_dmgNum == 0f)
		{
			m_img.sprite = m_sprBlock;
			m_img.color = m_colorBlock;
		}
		PlaceInWorldPos();
	}

	private void PlaceInWorldPos()
	{
		GetComponent<RectTransform>().anchoredPosition = CommonReferences.Instance.GetUtilityTools().WorldPosToCanvasPos(m_spawnPointPos);
	}

	private void KeepInCorrectPosX()
	{
		Vector2 anchoredPosition = RectTransformUtility.WorldToScreenPoint(CommonReferences.Instance.GetManagerCamerasXGame().GetCameraXGameCurrent().GetCameraUnity(), m_spawnPointPos) - CommonReferences.Instance.GetManagerHud().GetComponent<RectTransform>().sizeDelta / 2f;
		anchoredPosition.y = GetComponent<RectTransform>().anchoredPosition.y;
		GetComponent<RectTransform>().anchoredPosition = anchoredPosition;
	}

	private IEnumerator CoroutineAnimateColorDance()
	{
		Color l_colorOriginal = m_img.color;
		Color l_colorDark = Color.white;
		while (true)
		{
			float l_colorRFrom = l_colorOriginal.r;
			float l_colorGFrom = l_colorOriginal.g;
			float l_colorBFrom = l_colorOriginal.b;
			float l_colorRTo = l_colorDark.r;
			float l_colorGTo = l_colorDark.g;
			float l_colorBTo = l_colorDark.b;
			float l_timeToMove = 0.05f;
			float l_timeCurrent = 0f;
			while (l_timeCurrent < l_timeToMove)
			{
				l_timeCurrent += Time.fixedDeltaTime;
				float i_time = l_timeCurrent / l_timeToMove;
				float r = AnimationTools.CalculateOverTime(AnimationTools.Transition.Steep, AnimationTools.Transition.Steep, l_colorRFrom, l_colorRTo, i_time);
				float g = AnimationTools.CalculateOverTime(AnimationTools.Transition.Steep, AnimationTools.Transition.Steep, l_colorGFrom, l_colorGTo, i_time);
				float b = AnimationTools.CalculateOverTime(AnimationTools.Transition.Steep, AnimationTools.Transition.Steep, l_colorBFrom, l_colorBTo, i_time);
				m_img.color = new Color(r, g, b, m_img.color.a);
				yield return new WaitForFixedUpdate();
			}
			l_colorRFrom = m_img.color.r;
			l_colorGFrom = m_img.color.g;
			l_colorBFrom = m_img.color.b;
			l_colorRTo = l_colorOriginal.r;
			l_colorGTo = l_colorOriginal.g;
			l_colorBTo = l_colorOriginal.b;
			l_timeToMove = 0.05f;
			l_timeCurrent = 0f;
			while (l_timeCurrent < l_timeToMove)
			{
				l_timeCurrent += Time.fixedDeltaTime;
				float i_time2 = l_timeCurrent / l_timeToMove;
				float r2 = AnimationTools.CalculateOverTime(AnimationTools.Transition.Steep, AnimationTools.Transition.Steep, l_colorRFrom, l_colorRTo, i_time2);
				float g2 = AnimationTools.CalculateOverTime(AnimationTools.Transition.Steep, AnimationTools.Transition.Steep, l_colorGFrom, l_colorGTo, i_time2);
				float b2 = AnimationTools.CalculateOverTime(AnimationTools.Transition.Steep, AnimationTools.Transition.Steep, l_colorBFrom, l_colorBTo, i_time2);
				m_img.color = new Color(r2, g2, b2, m_img.color.a);
				yield return new WaitForFixedUpdate();
			}
		}
	}

	private IEnumerator CoroutineAnimateFadeOut()
	{
		float seconds = 0.3f;
		yield return new WaitForSeconds(seconds);
		float l_tranparencyFrom = 1f;
		float l_tranparencyTo = 0f;
		float l_timeToMove = 0.1f;
		float l_timeCurrent = 0f;
		while (l_timeCurrent < l_timeToMove)
		{
			l_timeCurrent += Time.fixedDeltaTime;
			float i_time = l_timeCurrent / l_timeToMove;
			float a = AnimationTools.CalculateOverTime(AnimationTools.Transition.Steep, AnimationTools.Transition.Steep, l_tranparencyFrom, l_tranparencyTo, i_time);
			m_img.color = new Color(m_img.color.r, m_img.color.g, m_img.color.b, a);
			yield return new WaitForFixedUpdate();
		}
		Object.Destroy(base.gameObject);
	}

	private IEnumerator CoroutineAnimateFloatUp()
	{
		float seconds = 0.1f;
		yield return new WaitForSeconds(seconds);
		float l_strengthMoveUp = 0.05f;
		float l_maxSpeedUp = 2f;
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

	private IEnumerator CoroutineAnimatePopupAndShrink()
	{
		float l_sizeToX = m_img.GetComponent<RectTransform>().sizeDelta.x;
		float l_sizeFromX = m_img.GetComponent<RectTransform>().sizeDelta.x * 2f;
		float l_timeToMove = 0.1f;
		float l_timeCurrent = 0f;
		while (l_timeCurrent < l_timeToMove)
		{
			l_timeCurrent += Time.fixedDeltaTime;
			float i_time = l_timeCurrent / l_timeToMove;
			float num = AnimationTools.CalculateOverTime(AnimationTools.Transition.Steep, AnimationTools.Transition.Steep, l_sizeFromX, l_sizeToX, i_time);
			m_img.GetComponent<RectTransform>().sizeDelta = new Vector2(num, num);
			yield return new WaitForFixedUpdate();
		}
		float seconds = 0f;
		yield return new WaitForSeconds(seconds);
		l_sizeFromX = m_img.GetComponent<RectTransform>().sizeDelta.x;
		l_sizeToX = 0f;
		l_timeToMove = 0.1f;
		l_timeCurrent = 0f;
		while (l_timeCurrent < l_timeToMove)
		{
			l_timeCurrent += Time.fixedDeltaTime;
			float i_time2 = l_timeCurrent / l_timeToMove;
			float num2 = AnimationTools.CalculateOverTime(AnimationTools.Transition.Steep, AnimationTools.Transition.Steep, l_sizeFromX, l_sizeToX, i_time2);
			if (num2 < 3f)
			{
				Object.Destroy(base.gameObject);
			}
			else
			{
				m_img.GetComponent<RectTransform>().sizeDelta = new Vector2(num2, num2);
			}
			yield return new WaitForFixedUpdate();
		}
	}
}
