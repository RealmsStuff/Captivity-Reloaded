using CaptivityReloaded.Modding;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UIButton = UnityEngine.UI.Button;

public class ScreenTitle : Screen
{
	private const string PatchNotesPanelName = "PatchNotesPanel";
	private const string TitleOverlayRootName = "TitleOverlayRoot";
	private const string UpdateBannerName = "ReleaseUpdateBanner";
	private const string ReleaseChannelUrl = "https://raw.githubusercontent.com/RealmsStuff/Captivity-Reloaded/main/release-channel-v1.json";
	private const string ReleaseChannelCacheKey = "ReleaseUpdate.Channel.V1";
	private const string ReleaseChannelCheckedTicksKey = "ReleaseUpdate.CheckedTicks.V1";
	private const long ReleaseCheckIntervalTicks = TimeSpan.TicksPerHour * 24L;

	private static readonly Color PatchNotesBorderColor = new Color(0.82f, 0f, 0.08f, 1f);

	private static readonly Color PatchNotesBackgroundColor = new Color(0.12f, 0f, 0.015f, 0.9f);

	[SerializeField]
	private GameObject m_parentLoading;

	[SerializeField]
	private GameObject m_parentTitle;

	[SerializeField]
	private Image m_imgBlackOverlay;

	[SerializeField]
	private AudioClip m_audioScreenMenu;

	[SerializeField]
	private Text m_txtVersion;

	private bool m_releaseCheckStarted;

	private GameObject m_releaseUpdateBanner;

	private void Start()
	{
	}

	public override void Open()
	{
		base.Open();
		EnsurePatchNotesPanel();
		CommonReferences.Instance.GetManagerAudio().PlayAudioMusic(m_audioScreenMenu);
		StartCoroutine(CoroutineAnimateFadeIn());
		ManagerDB.Initialize();
		CheckPrefs();
		CommonReferences.Instance.GetManagerAudio().SetVolumesToSaved();
		m_parentTitle.SetActive(value: true);
		m_parentLoading.SetActive(value: false);
		string version = Application.version;
		if (version.EndsWith(".0")) version = version.Substring(0, version.Length - 2);
		m_txtVersion.text = "version " + version;
		if (!m_releaseCheckStarted)
		{
			m_releaseCheckStarted = true;
			StartCoroutine(CheckForReleaseUpdate());
		}
	}

	private IEnumerator CheckForReleaseUpdate()
	{
		string cached = PlayerPrefs.GetString(ReleaseChannelCacheKey, string.Empty);
		TryShowReleaseUpdate(cached);
		long now = DateTime.UtcNow.Ticks;
		if (long.TryParse(PlayerPrefs.GetString(ReleaseChannelCheckedTicksKey, string.Empty), out long checkedTicks)
			&& checkedTicks > 0L && now >= checkedTicks && now - checkedTicks < ReleaseCheckIntervalTicks) yield break;

		string dailyUrl = ReleaseChannelUrl + "?day=" + DateTime.UtcNow.ToString("yyyyMMdd");
		using (UnityWebRequest request = UnityWebRequest.Get(dailyUrl))
		{
			request.timeout = 5;
			yield return request.SendWebRequest();
			if (request.result != UnityWebRequest.Result.Success) yield break;
			string json = request.downloadHandler.text;
			if (!GameReleaseUpdate.TryParse(json, out _, out string error))
			{
				Debug.LogWarning("[ReleaseUpdate] Rejected release channel: " + error);
				yield break;
			}
			PlayerPrefs.SetString(ReleaseChannelCacheKey, json);
			PlayerPrefs.SetString(ReleaseChannelCheckedTicksKey, now.ToString());
			PlayerPrefs.Save();
			TryShowReleaseUpdate(json);
		}
	}

	private void TryShowReleaseUpdate(string i_json)
	{
		if (!GameReleaseUpdate.TryParse(i_json, out GameReleaseInfo release, out _)
			|| !GameReleaseUpdate.IsNewerThanInstalled(release, Application.version)) return;
		EnsureReleaseUpdateBanner(release);
	}

	private void EnsureReleaseUpdateBanner(GameReleaseInfo i_release)
	{
		if (m_parentTitle == null || m_releaseUpdateBanner != null || i_release == null) return;
		Font font = m_txtVersion != null ? m_txtVersion.font : Resources.GetBuiltinResource<Font>("Arial.ttf");
		RectTransform overlayRoot = EnsureTitleOverlayRoot();
		m_releaseUpdateBanner = CreateUiObject(UpdateBannerName, overlayRoot);
		RectTransform bannerRect = m_releaseUpdateBanner.GetComponent<RectTransform>();
		bannerRect.anchorMin = bannerRect.anchorMax = bannerRect.pivot = new Vector2(0.5f, 0.5f);
		// Keep the compact banner centered beneath the title-screen logo artwork.
		bannerRect.anchoredPosition = new Vector2(-505f, -300f);
		bannerRect.sizeDelta = new Vector2(540f, 78f);
		Image border = m_releaseUpdateBanner.AddComponent<Image>();
		border.color = PatchNotesBorderColor;

		GameObject background = CreateUiObject("Background", m_releaseUpdateBanner.transform);
		Stretch(background.GetComponent<RectTransform>(), 2f, 2f, 2f, 2f);
		Image backgroundImage = background.AddComponent<Image>();
		backgroundImage.color = PatchNotesBackgroundColor;
		backgroundImage.raycastTarget = false;

		GameObject label = CreateUiObject("Label", m_releaseUpdateBanner.transform);
		RectTransform labelRect = label.GetComponent<RectTransform>();
		labelRect.anchorMin = Vector2.zero; labelRect.anchorMax = Vector2.one;
		labelRect.offsetMin = new Vector2(18f, 5f); labelRect.offsetMax = new Vector2(-192f, -5f);
		Text labelText = label.AddComponent<Text>();
		ConfigureText(labelText, font, 22, TextAnchor.MiddleLeft, Color.white);
		labelText.resizeTextForBestFit = true; labelText.resizeTextMinSize = 16; labelText.resizeTextMaxSize = 22;
		labelText.text = "UPDATE " + i_release.version + " AVAILABLE" + (i_release.prerelease ? " (TEST)" : string.Empty);

		CreateUpdateButton("View", m_releaseUpdateBanner.transform, "VIEW", font, 356f, 9f, 82f, 60f,
			delegate { Application.OpenURL(i_release.releaseUrl); }, 18);
		CreateUpdateButton("Later", m_releaseUpdateBanner.transform, "LATER", font, 446f, 9f, 86f, 60f,
			delegate { m_releaseUpdateBanner.SetActive(false); }, 18);
		m_releaseUpdateBanner.transform.SetAsLastSibling();
	}

	private static void CreateUpdateButton(string i_name, Transform i_parent, string i_label, Font i_font,
		float i_x, float i_y, float i_width, float i_height, UnityEngine.Events.UnityAction i_action, int i_fontSize = 16)
	{
		GameObject root = CreateUiObject(i_name, i_parent);
		RectTransform rect = root.GetComponent<RectTransform>();
		rect.anchorMin = rect.anchorMax = rect.pivot = Vector2.zero;
		rect.anchoredPosition = new Vector2(i_x, i_y); rect.sizeDelta = new Vector2(i_width, i_height);
		Image image = root.AddComponent<Image>();
		image.color = new Color(0.3f, 0.3f, 0.33f, 1f);
		UIButton button = root.AddComponent<UIButton>();
		button.targetGraphic = image;
		button.onClick.AddListener(i_action);
		GameObject label = CreateUiObject("Text", root.transform);
		Stretch(label.GetComponent<RectTransform>(), 3f, 3f, 3f, 3f);
		Text text = label.AddComponent<Text>();
		ConfigureText(text, i_font, i_fontSize, TextAnchor.MiddleCenter, Color.white);
		text.resizeTextForBestFit = true; text.resizeTextMinSize = 10; text.resizeTextMaxSize = i_fontSize;
		text.text = i_label;
	}

	private void EnsurePatchNotesPanel()
	{
		if (m_parentTitle == null)
		{
			return;
		}

		RectTransform overlayRoot = EnsureTitleOverlayRoot();
		if (overlayRoot.Find(PatchNotesPanelName) != null) return;
		Font font = (m_txtVersion != null) ? m_txtVersion.font : Resources.GetBuiltinResource<Font>("Arial.ttf");
		GameObject panel = CreateUiObject(PatchNotesPanelName, overlayRoot);
		RectTransform panelRect = panel.GetComponent<RectTransform>();
		panelRect.anchorMin = panelRect.anchorMax = panelRect.pivot = new Vector2(0.5f, 0.5f);
		panelRect.anchoredPosition = new Vector2(528f, -81f);
		panelRect.sizeDelta = new Vector2(557f, 316f);
		Image borderImage = panel.AddComponent<Image>();
		borderImage.color = PatchNotesBorderColor;
		borderImage.raycastTarget = false;

		GameObject background = CreateUiObject("Background", panel.transform);
		Stretch(background.GetComponent<RectTransform>(), 3f, 3f, 3f, 3f);
		Image backgroundImage = background.AddComponent<Image>();
		backgroundImage.color = PatchNotesBackgroundColor;
		backgroundImage.raycastTarget = false;

		GameObject header = CreateUiObject("Header", background.transform);
		RectTransform headerRect = header.GetComponent<RectTransform>();
		headerRect.anchorMin = new Vector2(0f, 0.82f);
		headerRect.anchorMax = Vector2.one;
		headerRect.offsetMin = new Vector2(18f, 0f);
		headerRect.offsetMax = new Vector2(-18f, 0f);
		Text headerText = header.AddComponent<Text>();
		ConfigureText(headerText, font, 32, TextAnchor.MiddleCenter, PatchNotesBorderColor);
		headerText.fontStyle = FontStyle.Bold;
		headerText.text = "Patch Notes";

		GameObject divider = CreateUiObject("Divider", background.transform);
		RectTransform dividerRect = divider.GetComponent<RectTransform>();
		dividerRect.anchorMin = new Vector2(0f, 0.82f);
		dividerRect.anchorMax = new Vector2(1f, 0.82f);
		dividerRect.anchoredPosition = Vector2.zero;
		dividerRect.sizeDelta = new Vector2(0f, 3f);
		Image dividerImage = divider.AddComponent<Image>();
		dividerImage.color = PatchNotesBorderColor;
		dividerImage.raycastTarget = false;

		GameObject body = CreateUiObject("Body", background.transform);
		RectTransform bodyRect = body.GetComponent<RectTransform>();
		bodyRect.anchorMin = Vector2.zero;
		bodyRect.anchorMax = new Vector2(1f, 0.82f);
		bodyRect.offsetMin = new Vector2(26f, 20f);
		bodyRect.offsetMax = new Vector2(-26f, -18f);
		Text bodyText = body.AddComponent<Text>();
		ConfigureText(bodyText, font, 22, TextAnchor.UpperLeft, Color.white);
		bodyText.resizeTextForBestFit = true;
		bodyText.resizeTextMinSize = 15;
		bodyText.resizeTextMaxSize = 22;
		bodyText.lineSpacing = 1.15f;
		bodyText.text = "Version 2.2.2\n\n- Added the native Randomizer game mode\n- Added keyboard-only assisted aiming and quick turning\n- Randomizer loadouts stay locked throughout each run\n- Improved the location menu layout and scrollbars\n- Removed Schockgewehr from weapon vendors";
	}

	private RectTransform EnsureTitleOverlayRoot()
	{
		Transform layoutParent = m_parentTitle.transform.Find("MiddleArea") ?? m_parentTitle.transform;
		Transform existing = layoutParent.Find(TitleOverlayRootName);
		if (existing != null) return existing as RectTransform;
		GameObject root = CreateUiObject(TitleOverlayRootName, layoutParent);
		RectTransform rect = root.GetComponent<RectTransform>();
		rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
		rect.anchoredPosition = Vector2.zero;
		rect.sizeDelta = new Vector2(1920f, 1080f);
		return rect;
	}

	private static GameObject CreateUiObject(string name, Transform parent)
	{
		GameObject gameObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
		gameObject.layer = parent.gameObject.layer;
		gameObject.transform.SetParent(parent, worldPositionStays: false);
		return gameObject;
	}

	private static void Stretch(RectTransform rect, float left, float bottom, float right, float top)
	{
		rect.anchorMin = Vector2.zero;
		rect.anchorMax = Vector2.one;
		rect.offsetMin = new Vector2(left, bottom);
		rect.offsetMax = new Vector2(-right, -top);
	}

	private static void ConfigureText(Text text, Font font, int fontSize, TextAnchor alignment, Color color)
	{
		text.font = font;
		text.fontSize = fontSize;
		text.alignment = alignment;
		text.color = color;
		text.raycastTarget = false;
		text.horizontalOverflow = HorizontalWrapMode.Wrap;
		text.verticalOverflow = VerticalWrapMode.Truncate;
	}

	private void CheckPrefs()
	{
		if (!PlayerPrefs.HasKey("VolumeMaster"))
		{
			ManagerDB.ResetVolumes();
		}
		if (!PlayerPrefs.HasKey("Difficulty"))
		{
			ManagerDB.SetDifficulty(Difficulty.Normal);
		}
		if (!PlayerPrefs.HasKey("IsReduceGunFlash"))
		{
			ManagerDB.SetIsReduceGunFlash(i_isReduce: false);
		}
		DisplaySettings.EnsureDefaults();
	}

	public void OpenOptions()
	{
		m_parentTitle.SetActive(value: false);
		GetComponentInChildren<ManagerOptions>(includeInactive: true).Open();
	}

	public void CloseOptions()
	{
		m_parentTitle.SetActive(value: true);
		GetComponentInChildren<ManagerOptions>(includeInactive: true).Close();
	}

	public void SetTitleMenuVisible(bool i_isVisible)
	{
		m_parentTitle.SetActive(i_isVisible);
	}

	public void ShowLoading()
	{
		m_parentTitle.SetActive(value: false);
		m_parentLoading.SetActive(value: true);
	}

	private IEnumerator CoroutineAnimateFadeIn()
	{
		float l_tranparencyFrom = 1f;
		float l_tranparencyTo = 0f;
		float l_timeToMove = 0.5f;
		float l_timeCurrent = 0f;
		m_imgBlackOverlay.color = new Color(m_imgBlackOverlay.color.r, m_imgBlackOverlay.color.g, m_imgBlackOverlay.color.b, l_tranparencyFrom);
		m_imgBlackOverlay.gameObject.SetActive(value: true);
		yield return new WaitForSecondsRealtime(0.5f);
		while (l_timeCurrent < l_timeToMove)
		{
			l_timeCurrent += Time.fixedDeltaTime;
			float i_time = l_timeCurrent / l_timeToMove;
			float a = AnimationTools.CalculateOverTime(AnimationTools.Transition.Steep, AnimationTools.Transition.Steep, l_tranparencyFrom, l_tranparencyTo, i_time);
			m_imgBlackOverlay.color = new Color(m_imgBlackOverlay.color.r, m_imgBlackOverlay.color.g, m_imgBlackOverlay.color.b, a);
			yield return new WaitForFixedUpdate();
		}
		m_imgBlackOverlay.gameObject.SetActive(value: false);
	}

	private IEnumerator CoroutineAnimateFadeInScreenGame()
	{
		float l_tranparencyFrom = 0f;
		float l_tranparencyTo = 1f;
		float l_timeToMove = 0.5f;
		float l_timeCurrent = 0f;
		m_imgBlackOverlay.color = new Color(m_imgBlackOverlay.color.r, m_imgBlackOverlay.color.g, m_imgBlackOverlay.color.b, l_tranparencyFrom);
		m_imgBlackOverlay.gameObject.SetActive(value: true);
		while (l_timeCurrent < l_timeToMove)
		{
			l_timeCurrent += Time.fixedDeltaTime;
			float i_time = l_timeCurrent / l_timeToMove;
			float a = AnimationTools.CalculateOverTime(AnimationTools.Transition.Steep, AnimationTools.Transition.Steep, l_tranparencyFrom, l_tranparencyTo, i_time);
			m_imgBlackOverlay.color = new Color(m_imgBlackOverlay.color.r, m_imgBlackOverlay.color.g, m_imgBlackOverlay.color.b, a);
			yield return new WaitForFixedUpdate();
		}
		CommonReferences.Instance.GetManagerScreens().OpenScreenGame();
	}

	public void ExitGame()
	{
		Application.Quit();
	}
}
