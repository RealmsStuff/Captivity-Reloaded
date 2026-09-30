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
		RectTransform content = m_stageMenuItemDefault.transform.parent as RectTransform;
		RectTransform viewport = content == null ? null : content.parent as RectTransform;
		if (content == null || viewport == null) return;
		ContentSizeFitter fitter = content.GetComponent<ContentSizeFitter>() ?? content.gameObject.AddComponent<ContentSizeFitter>();
		fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
		fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
		content.anchorMin = new Vector2(0f, 1f);
		content.anchorMax = new Vector2(1f, 1f);
		content.pivot = new Vector2(0.5f, 1f);
		content.anchoredPosition = Vector2.zero;
		content.sizeDelta = new Vector2(-20f, 0f);
		VerticalLayoutGroup layout = content.GetComponent<VerticalLayoutGroup>();
		if (layout != null) layout.childAlignment = TextAnchor.UpperCenter;
		if (viewport.GetComponent<RectMask2D>() == null) viewport.gameObject.AddComponent<RectMask2D>();
		Image viewportImage = viewport.GetComponent<Image>() ?? viewport.gameObject.AddComponent<Image>();
		viewportImage.color = new Color(0f, 0f, 0f, 0.001f);
		ScrollRect scroll = viewport.GetComponent<ScrollRect>() ?? viewport.gameObject.AddComponent<ScrollRect>();
		scroll.content = content;
		scroll.viewport = viewport;
		scroll.horizontal = false;
		scroll.vertical = true;
		scroll.movementType = ScrollRect.MovementType.Clamped;
		scroll.scrollSensitivity = 42f;
		scroll.verticalScrollbar = CreateVerticalScrollbar(viewport);
		scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
		Canvas.ForceUpdateCanvases();
		LayoutRebuilder.ForceRebuildLayoutImmediate(content);
		scroll.verticalNormalizedPosition = 1f;
	}

	private static Scrollbar CreateVerticalScrollbar(RectTransform i_parent)
	{
		Transform existing = i_parent.Find("LocationScrollbar");
		if (existing != null) return existing.GetComponent<Scrollbar>();
		GameObject barObject = new GameObject("LocationScrollbar", typeof(RectTransform), typeof(Image), typeof(Scrollbar));
		RectTransform bar = barObject.GetComponent<RectTransform>();
		bar.SetParent(i_parent, false);
		bar.anchorMin = new Vector2(0.5f, 0f);
		bar.anchorMax = new Vector2(0.5f, 1f);
		bar.pivot = new Vector2(0.5f, 0.5f);
		bar.anchoredPosition = new Vector2(296f, 0f);
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
