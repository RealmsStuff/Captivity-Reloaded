using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HubMainMenu : MonoBehaviour
{
	private const float DesignFrameWidth = 1872f;

	private const float DesignFrameHeight = 1032f;

	private const float DesignFrameMargin = 48f;

	private const float TopBarHeight = 132.6006f;

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

	private Vector2 m_lastParentSize = new Vector2(-1f, -1f);

	private void Awake()
	{
		NormalizeTopBarLayout();
		m_parent.SetActive(value: false);
	}

	private void OnRectTransformDimensionsChange()
	{
		if (m_btnLocations != null) NormalizeTopBarLayout();
	}

	private void NormalizeTopBarLayout()
	{
		RectTransform topBar = m_btnLocations.transform.parent as RectTransform;
		if (topBar == null) return;
		RectTransform outerBox = topBar.parent as RectTransform;
		RectTransform parent = m_parent == null ? null : m_parent.transform as RectTransform;
		if (outerBox == null || parent == null) return;

		float availableWidth = Mathf.Max(1f, parent.rect.width - DesignFrameMargin);
		float availableHeight = Mathf.Max(1f, parent.rect.height - DesignFrameMargin);
		float frameScale = Mathf.Min(1f, availableWidth / DesignFrameWidth, availableHeight / DesignFrameHeight);
		outerBox.anchorMin = new Vector2(0.5f, 0.5f);
		outerBox.anchorMax = new Vector2(0.5f, 0.5f);
		outerBox.pivot = new Vector2(0.5f, 0.5f);
		outerBox.anchoredPosition = Vector2.zero;
		outerBox.sizeDelta = new Vector2(DesignFrameWidth, DesignFrameHeight);
		outerBox.localScale = new Vector3(frameScale, frameScale, 1f);
		m_lastParentSize = parent.rect.size;

		topBar.anchorMin = new Vector2(0f, 1f);
		topBar.anchorMax = new Vector2(1f, 1f);
		topBar.pivot = new Vector2(0.5f, 0.5f);
		topBar.anchoredPosition = new Vector2(0f, -TopBarHeight * 0.5f);
		topBar.sizeDelta = new Vector2(0f, TopBarHeight);
		RectTransform managerMenus = outerBox.Find("ManagerMenus") as RectTransform;
		if (managerMenus != null)
		{
			managerMenus.anchorMin = Vector2.zero;
			managerMenus.anchorMax = Vector2.one;
			managerMenus.offsetMin = Vector2.zero;
			managerMenus.offsetMax = new Vector2(0f, -TopBarHeight);
		}

		if (topBar.rect.width <= ExitButtonWidth) return;
		float tabWidth = (topBar.rect.width - ExitButtonWidth) / 3f;
		SetFixedWidthFromLeft(m_btnLocations.transform as RectTransform, 0f, tabWidth);
		SetFixedWidthFromLeft(m_btnShop.transform as RectTransform, tabWidth, tabWidth);
		SetFixedWidthFromLeft(m_btnDiary.transform as RectTransform, tabWidth * 2f, tabWidth);
		RectTransform exit = topBar.Find("BtnExit") as RectTransform;
		if (exit != null)
		{
			exit.anchorMin = new Vector2(1f, exit.anchorMin.y);
			exit.anchorMax = new Vector2(1f, exit.anchorMax.y);
			exit.pivot = new Vector2(1f, exit.pivot.y);
			exit.anchoredPosition = new Vector2(0f, exit.anchoredPosition.y);
			exit.sizeDelta = new Vector2(ExitButtonWidth, exit.sizeDelta.y);
		}
	}

	private static void SetFixedWidthFromLeft(RectTransform i_rect, float i_left, float i_width)
	{
		if (i_rect == null) return;
		i_rect.anchorMin = new Vector2(0f, i_rect.anchorMin.y);
		i_rect.anchorMax = new Vector2(0f, i_rect.anchorMax.y);
		i_rect.pivot = new Vector2(0f, i_rect.pivot.y);
		i_rect.anchoredPosition = new Vector2(i_left, i_rect.anchoredPosition.y);
		i_rect.sizeDelta = new Vector2(i_width, i_rect.sizeDelta.y);
	}

	private void Update()
	{
		RectTransform parent = m_parent == null ? null : m_parent.transform as RectTransform;
		if (parent != null && parent.rect.size != m_lastParentSize)
		{
			NormalizeTopBarLayout();
		}
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
		Canvas.ForceUpdateCanvases();
		NormalizeTopBarLayout();
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
