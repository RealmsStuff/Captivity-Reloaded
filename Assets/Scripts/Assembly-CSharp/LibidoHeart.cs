using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LibidoHeart : MonoBehaviour
{
	private const float HudWidth = 513f;

	private const float HudLeftInset = 124f;

	private const float HeartSize = 112f;

	[SerializeField]
	private Image m_imgHeartFrigid;

	[SerializeField]
	private Image m_imgHeartHorny;

	[SerializeField]
	private Image m_imgHeartFlashGainLibido;

	[SerializeField]
	private Image m_imgHeartFlashLoseLibido;

	[SerializeField]
	private Text m_txtLibidoPercentage;

	[SerializeField]
	private Text m_txtLibidoPercentageBg;

	private float m_libidoMax;

	private float m_libidoCurrent;

	private float m_heartSizeXNormal;

	private void Awake()
	{
		NormalizeLayout();
	}

	private void Start()
	{
		m_libidoMax = CommonReferences.Instance.GetPlayer().GetLibidoMax();
	}

	private void NormalizeLayout()
	{
		RectTransform rectTransform = transform as RectTransform;
		RectTransform rectTransform2 = rectTransform.parent as RectTransform;
		if (rectTransform2 != null)
		{
			Vector2 anchorMin = rectTransform2.anchorMin;
			Vector2 anchorMax = rectTransform2.anchorMax;
			anchorMin.x = 0f;
			anchorMax.x = 0f;
			rectTransform2.anchorMin = anchorMin;
			rectTransform2.anchorMax = anchorMax;
			Vector2 anchoredPosition = rectTransform2.anchoredPosition;
			anchoredPosition.x = HudLeftInset + HudWidth * rectTransform2.pivot.x;
			rectTransform2.anchoredPosition = anchoredPosition;
			Vector2 sizeDelta = rectTransform2.sizeDelta;
			sizeDelta.x = HudWidth;
			rectTransform2.sizeDelta = sizeDelta;
		}
		Vector2 anchorMin2 = rectTransform.anchorMin;
		Vector2 anchorMax2 = rectTransform.anchorMax;
		anchorMin2.x = 1f;
		anchorMax2.x = 1f;
		rectTransform.anchorMin = anchorMin2;
		rectTransform.anchorMax = anchorMax2;
		Vector2 anchoredPosition2 = rectTransform.anchoredPosition;
		anchoredPosition2.x = (0f - HeartSize) * (1f - rectTransform.pivot.x);
		rectTransform.anchoredPosition = anchoredPosition2;
		Vector2 sizeDelta2 = rectTransform.sizeDelta;
		sizeDelta2.x = HeartSize;
		sizeDelta2.y = HeartSize;
		rectTransform.sizeDelta = sizeDelta2;
	}

	private void Update()
	{
		m_libidoCurrent = CommonReferences.Instance.GetPlayer().GetLibidoCurrent();
		string text = Mathf.Round(m_libidoCurrent * 100f) / 100f + "%";
		m_txtLibidoPercentage.text = text;
		m_txtLibidoPercentageBg.text = text;
		Color color = m_imgHeartHorny.color;
		float num = m_libidoCurrent / m_libidoMax * 100f;
		color.a = num / 100f;
		m_imgHeartHorny.color = color;
	}

	public void Show()
	{
		Image[] componentsInChildren = GetComponentsInChildren<Image>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = true;
		}
		m_txtLibidoPercentage.enabled = true;
		m_txtLibidoPercentageBg.enabled = true;
		StartCoroutine(CoroutineAnimateHeartBeat());
	}

	public void Hide()
	{
		Image[] componentsInChildren = GetComponentsInChildren<Image>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = false;
		}
		m_txtLibidoPercentage.enabled = false;
		m_txtLibidoPercentageBg.enabled = false;
	}

	public void FlashGainLibido(int i_amountOfTimes)
	{
		StartCoroutine(CoroutineFlashGainLibido(i_amountOfTimes));
	}

	private IEnumerator CoroutineFlashGainLibido(int i_amountOfTimes)
	{
		for (int l_index = 0; l_index < i_amountOfTimes; l_index++)
		{
			m_imgHeartFlashGainLibido.color = new Color(1f, 1f, 1f, 0.5f);
			yield return new WaitForSeconds(0.1f);
			m_imgHeartFlashGainLibido.color = new Color(1f, 1f, 1f, 0f);
			yield return new WaitForSeconds(0.1f);
		}
	}

	public void FlashLoseLibido(int i_amountOfTimes)
	{
		StartCoroutine(CoroutineFlashLoseLibido(i_amountOfTimes));
	}

	private IEnumerator CoroutineFlashLoseLibido(int i_amountOfTimes)
	{
		int l_index = 0;
		while (l_index < i_amountOfTimes)
		{
			float l_transparencyFrom = 0f;
			float l_transparencyTo = 0.5f;
			float l_timeToMove = 1f;
			float l_timeCurrent = 0f;
			while (l_timeCurrent < l_timeToMove)
			{
				l_timeCurrent += Time.fixedDeltaTime;
				float i_time = l_timeCurrent / l_timeToMove;
				float a = AnimationTools.CalculateOverTime(AnimationTools.Transition.Steep, AnimationTools.Transition.Steep, l_transparencyFrom, l_transparencyTo, i_time);
				m_imgHeartFlashLoseLibido.color = new Color(m_imgHeartFlashLoseLibido.color.r, m_imgHeartFlashLoseLibido.color.g, m_imgHeartFlashLoseLibido.color.b, a);
				yield return new WaitForFixedUpdate();
				l_index++;
			}
			l_transparencyFrom = 0.5f;
			l_transparencyTo = 0f;
			l_timeToMove = 1f;
			l_timeCurrent = 0f;
			while (l_timeCurrent < l_timeToMove)
			{
				l_timeCurrent += Time.fixedDeltaTime;
				float i_time2 = l_timeCurrent / l_timeToMove;
				float a2 = AnimationTools.CalculateOverTime(AnimationTools.Transition.Steep, AnimationTools.Transition.Steep, l_transparencyFrom, l_transparencyTo, i_time2);
				m_imgHeartFlashLoseLibido.color = new Color(m_imgHeartFlashLoseLibido.color.r, m_imgHeartFlashLoseLibido.color.g, m_imgHeartFlashLoseLibido.color.b, a2);
				yield return new WaitForFixedUpdate();
				l_index++;
			}
		}
	}

	private IEnumerator CoroutineAnimateHeartBeat()
	{
		yield return new WaitForSeconds(0.25f);
		if (m_heartSizeXNormal == 0f)
		{
			m_heartSizeXNormal = 0f;
		}
		while (true)
		{
			float l_sizeXFrom = m_heartSizeXNormal;
			float l_sizeXTo = 25f;
			float l_timeToMove = 0.1f;
			float l_timeCurrent = 0f;
			while (l_timeCurrent < l_timeToMove)
			{
				l_timeCurrent += Time.fixedDeltaTime;
				float i_time = l_timeCurrent / l_timeToMove;
				float num = AnimationTools.CalculateOverTime(AnimationTools.Transition.Steep, AnimationTools.Transition.Steep, l_sizeXFrom, l_sizeXTo, i_time);
				m_imgHeartFrigid.GetComponent<RectTransform>().sizeDelta = new Vector2(num, num);
				m_imgHeartHorny.GetComponent<RectTransform>().sizeDelta = new Vector2(num, num);
				m_imgHeartFlashGainLibido.GetComponent<RectTransform>().sizeDelta = new Vector2(num, num);
				m_imgHeartFlashLoseLibido.GetComponent<RectTransform>().sizeDelta = new Vector2(num, num);
				yield return new WaitForFixedUpdate();
			}
			l_sizeXFrom = l_sizeXTo;
			l_sizeXTo = m_heartSizeXNormal;
			float num2 = m_libidoCurrent / m_libidoMax * 100f;
			l_timeToMove = 1f - num2 * 0.9f / 100f;
			l_timeCurrent = 0f;
			while (l_timeCurrent < l_timeToMove)
			{
				l_timeCurrent += Time.fixedDeltaTime;
				float i_time2 = l_timeCurrent / l_timeToMove;
				float num3 = AnimationTools.CalculateOverTime(AnimationTools.Transition.Steep, AnimationTools.Transition.Steep, l_sizeXFrom, l_sizeXTo, i_time2);
				m_imgHeartFrigid.GetComponent<RectTransform>().sizeDelta = new Vector2(num3, num3);
				m_imgHeartHorny.GetComponent<RectTransform>().sizeDelta = new Vector2(num3, num3);
				m_imgHeartFlashGainLibido.GetComponent<RectTransform>().sizeDelta = new Vector2(num3, num3);
				m_imgHeartFlashLoseLibido.GetComponent<RectTransform>().sizeDelta = new Vector2(num3, num3);
				yield return new WaitForFixedUpdate();
			}
		}
	}

	public void ResetLibidoHeart()
	{
		m_imgHeartFlashGainLibido.color = new Color(1f, 1f, 1f, 0f);
	}
}
