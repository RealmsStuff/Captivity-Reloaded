using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MenuLocations : Menu
{
	[SerializeField]
	private StageMenuItem m_stageMenuItemDefault;

	[SerializeField]
	private Text m_txtTitleLvl;

	[SerializeField]
	private Text m_txtDescriptionLvl;

	[SerializeField]
	private Text m_txtHighscore;

	[SerializeField]
	private UnityEngine.UI.Button m_btnGo;

	private Stage m_stageSelected;
	private ManagerStages m_managerStages;
	private RectTransform m_stageContent;
	private RectTransform m_stageViewport;
	private ContentSizeFitter m_stageContentFitter;
	private VerticalLayoutGroup m_stageLayout;
	private ScrollRect m_stageScroll;

	private void Start()
	{
		m_btnGo.interactable = false;
		m_stageMenuItemDefault.gameObject.SetActive(value: false);
		m_managerStages = CommonReferences.Instance.GetManagerStages();
		m_managerStages.OnRuntimeStageAdded += AddStageMenuItem;
		BuildStageMenuItems();
		ConfigureStageScrollView();
	}

	private void ConfigureStageScrollView()
	{
		m_stageContent = m_stageMenuItemDefault.transform.parent as RectTransform;
		m_stageViewport = m_stageContent == null ? null : m_stageContent.parent as RectTransform;
		if (m_stageContent == null || m_stageViewport == null) return;
		m_stageViewport.anchorMin = Vector2.zero;
		m_stageViewport.anchorMax = new Vector2(0.5f, 1f);
		m_stageViewport.pivot = new Vector2(0.5f, 0.5f);
		m_stageViewport.offsetMin = new Vector2(20f, 20f);
		m_stageViewport.offsetMax = new Vector2(-20f, -20f);
		m_stageContentFitter = m_stageContent.GetComponent<ContentSizeFitter>() ?? m_stageContent.gameObject.AddComponent<ContentSizeFitter>();
		m_stageContentFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
		m_stageLayout = m_stageContent.GetComponent<VerticalLayoutGroup>();
		if (m_stageViewport.GetComponent<RectMask2D>() == null) m_stageViewport.gameObject.AddComponent<RectMask2D>();
		Image viewportImage = m_stageViewport.GetComponent<Image>() ?? m_stageViewport.gameObject.AddComponent<Image>();
		viewportImage.color = new Color(0f, 0f, 0f, 0.001f);
		m_stageScroll = m_stageViewport.GetComponent<ScrollRect>() ?? m_stageViewport.gameObject.AddComponent<ScrollRect>();
		m_stageScroll.content = m_stageContent;
		m_stageScroll.viewport = m_stageViewport;
		m_stageScroll.horizontal = false;
		m_stageScroll.movementType = ScrollRect.MovementType.Clamped;
		m_stageScroll.scrollSensitivity = 42f;
		m_stageScroll.verticalScrollbar = CreateVerticalScrollbar(m_stageViewport);
		m_stageScroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
		RefreshStageLayout();
	}

	private void RefreshStageLayout()
	{
		if (m_stageContent == null || m_stageViewport == null || m_stageContentFitter == null || m_stageLayout == null || m_stageScroll == null) return;
		m_stageContentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
		m_stageContent.anchorMin = new Vector2(0f, 1f);
		m_stageContent.anchorMax = Vector2.one;
		m_stageContent.pivot = new Vector2(0.5f, 1f);
		m_stageContent.anchoredPosition = Vector2.zero;
		m_stageContent.sizeDelta = new Vector2(-20f, 0f);
		Canvas.ForceUpdateCanvases();
		LayoutRebuilder.ForceRebuildLayoutImmediate(m_stageContent);
		bool overflow = LayoutUtility.GetPreferredHeight(m_stageContent) > m_stageViewport.rect.height;
		m_stageLayout.childAlignment = overflow ? TextAnchor.UpperCenter : TextAnchor.MiddleCenter;
		m_stageScroll.vertical = overflow;
		// Keep the red right-side bar visible as part of the Locations layout even
		// when the centered list is short enough that scrolling is unnecessary.
		m_stageScroll.verticalScrollbar.gameObject.SetActive(true);
		if (!overflow)
		{
			m_stageContentFitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;
			m_stageContent.anchorMin = Vector2.zero;
			m_stageContent.anchorMax = Vector2.one;
			m_stageContent.pivot = new Vector2(0.5f, 0.5f);
			m_stageContent.anchoredPosition = Vector2.zero;
			m_stageContent.sizeDelta = new Vector2(-20f, 0f);
		}
		LayoutRebuilder.ForceRebuildLayoutImmediate(m_stageContent);
		m_stageScroll.verticalNormalizedPosition = 1f;
	}

	private static Scrollbar CreateVerticalScrollbar(RectTransform i_parent)
	{
		Transform existing = i_parent.Find("LocationScrollbar");
		if (existing != null) return existing.GetComponent<Scrollbar>();
		GameObject barObject = new GameObject("LocationScrollbar", typeof(RectTransform), typeof(Image), typeof(Scrollbar));
		RectTransform bar = barObject.GetComponent<RectTransform>();
		bar.SetParent(i_parent, false);
		bar.anchorMin = new Vector2(1f, 0f);
		bar.anchorMax = Vector2.one;
		bar.pivot = new Vector2(1f, 0.5f);
		bar.anchoredPosition = new Vector2(-4f, 0f);
		bar.sizeDelta = new Vector2(10f, -12f);
		barObject.GetComponent<Image>().color = new Color(0.08f, 0.02f, 0.02f, 0.8f);
		GameObject slidingObject = new GameObject("SlidingArea", typeof(RectTransform));
		RectTransform sliding = slidingObject.GetComponent<RectTransform>();
		sliding.SetParent(bar, false);
		sliding.anchorMin = Vector2.zero;
		sliding.anchorMax = Vector2.one;
		sliding.offsetMin = new Vector2(2f, 2f);
		sliding.offsetMax = new Vector2(-2f, -2f);
		GameObject handleObject = new GameObject("Handle", typeof(RectTransform), typeof(Image));
		RectTransform handle = handleObject.GetComponent<RectTransform>();
		handle.SetParent(sliding, false);
		handle.anchorMin = Vector2.zero;
		handle.anchorMax = Vector2.one;
		handle.offsetMin = Vector2.zero;
		handle.offsetMax = Vector2.zero;
		Image handleImage = handleObject.GetComponent<Image>();
		handleImage.color = new Color(0.75f, 0.05f, 0.05f, 1f);
		Scrollbar scrollbar = barObject.GetComponent<Scrollbar>();
		scrollbar.handleRect = handle;
		scrollbar.targetGraphic = handleImage;
		scrollbar.direction = Scrollbar.Direction.BottomToTop;
		scrollbar.navigation = new Navigation { mode = Navigation.Mode.None };
		bar.SetAsLastSibling();
		return scrollbar;
	}

	private void OnDestroy()
	{
		if (m_managerStages != null) m_managerStages.OnRuntimeStageAdded -= AddStageMenuItem;
	}

	private void BuildStageMenuItems()
	{
		foreach (Stage allStage in CommonReferences.Instance.GetManagerStages().GetAllStages())
		{
			AddStageMenuItem(allStage);
		}
	}

	private void AddStageMenuItem(Stage i_stage)
	{
		if (i_stage == null || i_stage is StageHub || i_stage.GetIsRuntimeTemplate()) return;
		StageMenuItem stageMenuItem = Object.Instantiate(m_stageMenuItemDefault, m_stageMenuItemDefault.transform.parent);
		stageMenuItem.SetStage(i_stage);
		stageMenuItem.gameObject.SetActive(value: true);
		LayoutRebuilder.ForceRebuildLayoutImmediate(m_stageMenuItemDefault.transform.parent as RectTransform);
		RefreshStageLayout();
	}

	public void Go()
	{
		CommonReferences.Instance.GetManagerScreens().GetScreenGame().OpenStage(m_stageSelected);
		CommonReferences.Instance.GetManagerHud().CloseHubMainMenu();
	}

	public void SetLevelSelected(Stage i_stage)
	{
		m_stageSelected = i_stage;
		m_txtTitleLvl.text = m_stageSelected.GetName();
		m_txtDescriptionLvl.text = m_stageSelected.GetDescription();
		int highscore = m_stageSelected.GetHighscore();
		if (highscore != 0)
		{
			m_txtHighscore.text = "Highscore: Wave " + highscore;
		}
		else
		{
			m_txtHighscore.text = "";
		}
		StopAllCoroutines();
		StartCoroutine(CoroutineTypeWriterEffect(m_stageSelected.GetDescription()));
		m_btnGo.interactable = true;
	}

	private IEnumerator CoroutineTypeWriterEffect(string i_text)
	{
		m_txtDescriptionLvl.text = "";
		for (int l_index = 0; l_index < i_text.Length; l_index++)
		{
			m_txtDescriptionLvl.text += i_text[l_index];
			yield return new WaitForEndOfFrame();
		}
	}
}
