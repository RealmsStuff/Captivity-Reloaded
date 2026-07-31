using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HudSmasher : MonoBehaviour
{
	[SerializeField]
	private Image m_imgMeterBg;

	[SerializeField]
	private Image m_imgMeterCurrent;

	[SerializeField]
	private Image m_imgMeterBar;

	[SerializeField]
	private Text m_txtLeft;

	[SerializeField]
	private Text m_txtRight;

	[SerializeField]
	private Text m_txtUp;

	[SerializeField]
	private Color m_colorArrowToPress;

	[SerializeField]
	private Color m_colorArrowToNotPress;

	[SerializeField]
	private GameObject m_posXMax;

	[SerializeField]
	private GameObject m_posXMin;

	[SerializeField]
	private Image m_imgMeterTimeBg;

	[SerializeField]
	private Image m_imgMeterTimeCurrent;

	[SerializeField]
	private Image m_imgMeterTimeBar;

	private RaperSmasher m_smasher;

	private Color m_colorMeterOriginal;

	private Color m_colorMeterBarNormal;

	private Color m_colorMeterBarAuto;

	private void Start()
	{
		m_colorMeterOriginal = m_imgMeterCurrent.color;
		m_colorMeterBarNormal = m_imgMeterBar.color;
		m_colorMeterBarAuto = new Color(0f, 0.25f, 1f);
	}

	private void Update()
	{
		UpdateMeter();
		UpdateMeterTime();
	}

	private void UpdateMeter()
	{
		float meterCurrent = m_smasher.GetMeterCurrent();
		Vector3 vector = m_imgMeterCurrent.GetComponent<RectTransform>().anchoredPosition;
		vector.x = m_imgMeterBar.GetComponent<RectTransform>().rect.width / m_smasher.GetMeterMax() * meterCurrent - m_imgMeterBar.GetComponent<RectTransform>().rect.width;
		m_imgMeterCurrent.GetComponent<RectTransform>().anchoredPosition = vector;
		if (CommonReferences.Instance.GetManagerInput().IsButton(InputButton.Jump))
		{
			m_txtUp.color = m_colorArrowToPress;
			m_txtLeft.color = m_colorArrowToNotPress;
			m_txtRight.color = m_colorArrowToNotPress;
			m_imgMeterBar.color = m_colorMeterBarAuto;
			return;
		}
		m_txtUp.color = m_colorArrowToNotPress;
		m_imgMeterBar.color = m_colorMeterBarNormal;
		if (m_smasher.GetKeyCodeToPress() == KeyCode.A)
		{
			m_txtLeft.color = m_colorArrowToPress;
			m_txtRight.color = m_colorArrowToNotPress;
		}
		else
		{
			m_txtLeft.color = m_colorArrowToNotPress;
			m_txtRight.color = m_colorArrowToPress;
		}
	}

	private void UpdateMeterTime()
	{
		float timeLeft = m_smasher.GetTimeLeft();
		Vector3 vector = m_imgMeterTimeCurrent.GetComponent<RectTransform>().anchoredPosition;
		vector.x = m_imgMeterTimeBar.GetComponent<RectTransform>().rect.width / m_smasher.GetTimeMax() * timeLeft - m_imgMeterTimeBar.GetComponent<RectTransform>().rect.width;
		m_imgMeterTimeCurrent.GetComponent<RectTransform>().anchoredPosition = vector;
	}

	public void Thrust()
	{
		StartCoroutine(CoroutineAnimateThrust());
	}

	private IEnumerator CoroutineAnimateThrust()
	{
		m_imgMeterCurrent.color = Color.white;
		float l_colorRFrom = m_imgMeterCurrent.color.r;
		float l_colorGFrom = m_imgMeterCurrent.color.g;
		float l_colorBFrom = m_imgMeterCurrent.color.b;
		float l_colorRTo = m_colorMeterOriginal.r;
		float l_colorGTo = m_colorMeterOriginal.g;
		float l_colorBTo = m_colorMeterOriginal.b;
		float l_timeToMove = 0.2f;
		float l_timeCurrent = 0f;
		while (l_timeCurrent < l_timeToMove)
		{
			l_timeCurrent += Time.fixedDeltaTime;
			float i_time = l_timeCurrent / l_timeToMove;
			float r = AnimationTools.CalculateOverTime(AnimationTools.Transition.Steep, AnimationTools.Transition.Smooth, l_colorRFrom, l_colorRTo, i_time);
			float g = AnimationTools.CalculateOverTime(AnimationTools.Transition.Steep, AnimationTools.Transition.Smooth, l_colorGFrom, l_colorGTo, i_time);
			float b = AnimationTools.CalculateOverTime(AnimationTools.Transition.Steep, AnimationTools.Transition.Smooth, l_colorBFrom, l_colorBTo, i_time);
			Color color = new Color(0f, 0f, 0f, 1f);
			color.r = r;
			color.g = g;
			color.b = b;
			m_imgMeterCurrent.color = color;
			yield return new WaitForFixedUpdate();
		}
	}

	public void Show(RaperSmasher i_raperSmasher)
	{
		base.gameObject.SetActive(value: true);
		m_smasher = i_raperSmasher;
		UpdateMeterPos();
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}

	public void DropArrow(bool i_leftArrow)
	{
		GameObject gameObject = ((!i_leftArrow) ? m_txtRight.gameObject : m_txtLeft.gameObject);
		GameObject gameObject2 = Object.Instantiate(gameObject.gameObject);
		gameObject2.transform.SetParent(base.transform.parent);
		gameObject2.transform.position = gameObject.transform.position;
		gameObject2.transform.SetSiblingIndex(gameObject.transform.GetSiblingIndex() - 1);
		gameObject2.AddComponent<Rigidbody2D>();
		gameObject2.GetComponent<Rigidbody2D>().gravityScale = 100f;
		gameObject2.GetComponent<Rigidbody2D>().AddForce(new Vector2(Random.Range(-50000, 50000), -10000f));
		gameObject2.GetComponent<Rigidbody2D>().AddTorque(75f);
		gameObject2.GetComponent<Image>().color = new Color(0.5f, 0.5f, 0.5f, 1f);
		StartCoroutine(CoroutineDestroyDroppedArrow(gameObject2));
	}

	private IEnumerator CoroutineDestroyDroppedArrow(GameObject i_arrow)
	{
		yield return new WaitForSeconds(3f);
		Object.Destroy(i_arrow);
	}

	private void UpdateMeterPos()
	{
		float meterCurrent = m_smasher.GetMeterCurrent();
		Vector3 vector = m_imgMeterCurrent.GetComponent<RectTransform>().anchoredPosition;
		float num = m_imgMeterBar.GetComponent<RectTransform>().rect.width - 12.5f;
		vector.x = num / m_smasher.GetMeterMax() * meterCurrent - num;
		m_imgMeterCurrent.GetComponent<RectTransform>().anchoredPosition = vector;
	}
}
