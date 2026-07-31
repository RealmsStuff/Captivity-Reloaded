using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HubMainMenu : MonoBehaviour
{
	private const float TopBarHeight = 132.6006f;

	private const float TabWidth = 354f;

	private const float TabStep = 351f;

	private const float ExitButtonWidth = 150f;

	[SerializeField]
	private AudioClip m_audioStart;

	[SerializeField]
	private AudioClip m_audioClose;

	[SerializeField]
	private GameObject m_parent;

	[SerializeField]
	private Image m_overlay;

	[SerializeField]
	private UnityEngine.UI.Button m_btnLocations;

	[SerializeField]
	private UnityEngine.UI.Button m_btnShop;

	[SerializeField]
	private UnityEngine.UI.Button m_btnDiary;

	[SerializeField]
	private Sprite m_sprBtnClosed;

	[SerializeField]
	private Sprite m_sprBtnOpen;

	private void Awake()
	{
		NormalizeTopBarLayout();
		m_parent.SetActive(value: false);
	}

	private void NormalizeTopBarLayout()
	{
		RectTransform rectTransform = m_btnLocations.transform.parent as RectTransform;
		Vector2 anchorMin = rectTransform.anchorMin;
		Vector2 anchorMax = rectTransform.anchorMax;
		anchorMin.y = 1f;
		anchorMax.y = 1f;
		rectTransform.anchorMin = anchorMin;
		rectTransform.anchorMax = anchorMax;
		Vector2 anchoredPosition = rectTransform.anchoredPosition;
		anchoredPosition.y = (0f - TopBarHeight) * (1f - rectTransform.pivot.y);
		rectTransform.anchoredPosition = anchoredPosition;
		Vector2 sizeDelta = rectTransform.sizeDelta;
		sizeDelta.y = TopBarHeight;
		rectTransform.sizeDelta = sizeDelta;
		SetFixedWidthFromLeft(m_btnLocations.transform as RectTransform, 0f, TabWidth);
		SetFixedWidthFromLeft(m_btnShop.transform as RectTransform, TabStep, TabWidth);
		SetFixedWidthFromLeft(m_btnDiary.transform as RectTransform, TabStep * 2f, TabWidth);
		RectTransform rectTransform2 = rectTransform.Find("BtnExit") as RectTransform;
		if (rectTransform2 != null)
		{
			Vector2 anchorMin2 = rectTransform2.anchorMin;
			Vector2 anchorMax2 = rectTransform2.anchorMax;
			anchorMin2.x = 1f;
			anchorMax2.x = 1f;
			rectTransform2.anchorMin = anchorMin2;
			rectTransform2.anchorMax = anchorMax2;
			Vector2 anchoredPosition2 = rectTransform2.anchoredPosition;
			anchoredPosition2.x = (0f - ExitButtonWidth) * (1f - rectTransform2.pivot.x);
			rectTransform2.anchoredPosition = anchoredPosition2;
			Vector2 sizeDelta2 = rectTransform2.sizeDelta;
			sizeDelta2.x = ExitButtonWidth;
			rectTransform2.sizeDelta = sizeDelta2;
		}
	}

	private static void SetFixedWidthFromLeft(RectTransform i_rectTransform, float i_left, float i_width)
	{
		Vector2 anchorMin = i_rectTransform.anchorMin;
		Vector2 anchorMax = i_rectTransform.anchorMax;
		anchorMin.x = 0f;
		anchorMax.x = 0f;
		i_rectTransform.anchorMin = anchorMin;
		i_rectTransform.anchorMax = anchorMax;
		Vector2 anchoredPosition = i_rectTransform.anchoredPosition;
		anchoredPosition.x = i_left + i_width * i_rectTransform.pivot.x;
		i_rectTransform.anchoredPosition = anchoredPosition;
		Vector2 sizeDelta = i_rectTransform.sizeDelta;
		sizeDelta.x = i_width;
		i_rectTransform.sizeDelta = sizeDelta;
	}

	private void Update()
	{
		if (IsOpen() && Input.GetKeyDown(KeyCode.Escape))
		{
			BtnClose();
		}
	}

	public bool IsOpen()
	{
		return m_parent.activeSelf;
	}

	public void Open()
	{
		m_parent.SetActive(value: true);
		StartCoroutine(CoroutineAnimateOverlayFadeOut());
		CommonReferences.Instance.GetManagerAudio().PlayAudioSFX(m_audioStart);
	}

	public void Close()
	{
		StopAllCoroutines();
		m_overlay.gameObject.SetActive(value: false);
		m_parent.SetActive(value: false);
		CommonReferences.Instance.GetManagerAudio().PlayAudioSFX(m_audioClose);
	}

	public void BtnClose()
	{
		((StageHub)CommonReferences.Instance.GetManagerStages().GetStageCurrent()).GetComponentInChildren<ChairHubTerminal>().CloseHubMainMenu();
	}

	public void OpenMenu(Menu i_menuToOpen)
	{
		GetComponentInChildren<ManagerMenus>().OpenMenu(i_menuToOpen);
	}

	public void HandleButtonsTopBar(UnityEngine.UI.Button i_btnPressed)
	{
		m_btnLocations.GetComponent<Image>().sprite = m_sprBtnClosed;
		m_btnShop.GetComponent<Image>().sprite = m_sprBtnClosed;
		m_btnDiary.GetComponent<Image>().sprite = m_sprBtnClosed;
		i_btnPressed.GetComponent<Image>().sprite = m_sprBtnOpen;
	}

	private IEnumerator CoroutineAnimateOverlayFadeOut()
	{
		m_overlay.gameObject.SetActive(value: true);
		float l_tranparencyFrom = 1f;
		float l_tranparencyTo = 0f;
		float l_timeToMove = 1f;
		float l_timeCurrent = 0f;
		while (l_timeCurrent < l_timeToMove)
		{
			l_timeCurrent += Time.fixedDeltaTime;
			float i_time = l_timeCurrent / l_timeToMove;
			float a = AnimationTools.CalculateOverTime(AnimationTools.Transition.Steep, AnimationTools.Transition.Steep, l_tranparencyFrom, l_tranparencyTo, i_time);
			m_overlay.color = new Color(255,0, 0, a);
			yield return new WaitForFixedUpdate();
		}
		m_overlay.gameObject.SetActive(value: false);
	}
}
