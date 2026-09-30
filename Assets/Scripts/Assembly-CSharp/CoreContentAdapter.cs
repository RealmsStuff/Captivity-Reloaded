using CaptivityReloaded.Modding;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;
using UIButton = UnityEngine.UI.Button;
using Object = UnityEngine.Object;

public static class CoreContentAdapter
{
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	private static void BindCoreContent()
	{
		Library[] libraries = Resources.FindObjectsOfTypeAll<Library>();
		ManagerStages[] stageManagers = Resources.FindObjectsOfTypeAll<ManagerStages>();
		ManagerChallenge[] challengeManagers = Resources.FindObjectsOfTypeAll<ManagerChallenge>();
		Library library = FindSceneObject(libraries);
		ManagerStages stageManager = FindSceneObject(stageManagers);
		ManagerChallenge challengeManager = FindSceneObject(challengeManagers);
		if (library == null || stageManager == null) return;

		foreach (CoreContentCatalogEntry entry in ModLoaderRuntime.CoreContentCatalog)
		{
			Object runtimeAsset = Resolve(entry, library, stageManager, challengeManager);
			if (runtimeAsset == null)
			{
				string selector = entry.LegacyId.HasValue ? "legacy ID " + entry.LegacyId.Value : "legacy name '" + entry.LegacyName + "'";
				ModLoaderRuntime.LastReport.Add(ValidationSeverity.Warning, "adapter.core-missing", "Core " + entry.Category + " " + selector + " could not be resolved.", "core/catalog.json");
				continue;
			}
			ApplyPackagedCoreDefinition(entry.Id, runtimeAsset);

			BindRuntimeIdentity(runtimeAsset, entry.Id, entry.Category);
			ModLoaderRuntime.Registry.BindRuntimeAsset(entry.Id, runtimeAsset, ModLoaderRuntime.LastReport);
		}

		Debug.Log("[ModLoader] Bound " + ModLoaderRuntime.Registry.Count + " packaged Core gameplay entries.");
		CoreAssetSlotBinder.BindAfterSceneObjectsStarted();
		// Keep the shipped garment prefabs as the authoritative Core presentation.
		// The loose Core clothing catalog remains loaded for documentation, authoring,
		// and migration validation, but reconstruction is not visually lossless yet.
		// Replacing all 98 garments here caused broken pivots, masks, and piece ordering
		// even when no external mods were enabled.
		ExternalFactoryRunner.GetOrAdd<ModValidationPresenter>();
		ExternalFactoryRunner.GetOrAdd<ModManagerPresenter>();
		ExternalClothingFactory.Schedule(library.Clothes);
		ExternalPlayerAttachmentFactory.Schedule();
		ExternalWeaponFactory.Schedule(library.Guns);
		ExternalUsableFactory.Schedule(library.Usables);
		ExternalEnemyFactory.Schedule(library.Actors, stageManager);
		ExternalStageFactory.Schedule(stageManager);
		ExternalChallengeFactory.Schedule(challengeManager);
		ExternalRuleProfileFactory.Schedule();
	}

	private static void ApplyPackagedCoreDefinition(ContentId i_id, Object i_runtimeAsset)
	{
		if (i_runtimeAsset is Gun gun)
		{
			foreach (CoreWeaponDefinition definition in ModLoaderRuntime.CoreWeaponDefinitions)
			{
				if (definition.Id != i_id) continue;
				gun.ConfigureModItem(definition.DisplayName, definition.Description);
				gun.ConfigureModWeaponStats(definition.Stats);
				return;
			}
		}
		if (i_runtimeAsset is NPC npc)
		{
			foreach (CoreEnemyDefinition definition in ModLoaderRuntime.CoreEnemyDefinitions)
			{
				if (definition.Id != i_id) continue;
				npc.ConfigureCoreEnemy(definition.DisplayName, definition.Description, definition.Stats, definition.Behavior);
				CoreAssetSlotBinder.RegisterCoreEnemySlots(npc, definition.Id);
				return;
			}
		}
		if (i_runtimeAsset is Stage stage)
		{
			foreach (CoreStageDefinition definition in ModLoaderRuntime.CoreStageDefinitions)
			{
				if (definition.Id != i_id) continue;
				stage.ConfigureCoreStage(definition.DisplayName, definition.Description);
				ManagerWave managerWave = stage.GetManagerWave();
				if (managerWave != null && definition.FirstWaveEnemyCount.HasValue)
					managerWave.ConfigureCoreWave(definition.FirstWaveEnemyCount.Value);
				return;
			}
		}
		if (i_runtimeAsset is Clothing clothing)
		{
			foreach (CoreClothingDefinition definition in ModLoaderRuntime.CoreClothingDefinitions)
			{
				if (definition.Id != i_id) continue;
				clothing.ConfigureModClothing(definition.Category, null, null);
				CoreAssetSlotBinder.RegisterCoreClothingSlots(clothing, definition);
				return;
			}
		}
		if (i_runtimeAsset is Item item)
		{
			foreach (CoreItemDefinition definition in ModLoaderRuntime.CoreItemDefinitions)
			{
				if (definition.Id != i_id) continue;
				item.ConfigureModItem(definition.DisplayName, definition.Description);
				if (item is Usable usable) usable.ConfigureModUsable(definition.Stats, definition.EffectMode, definition.Effects);
				else if (item is PickUpable pickUpable) pickUpable.ConfigureModEconomy(definition.Stats.Weight, definition.Stats.Value);
				return;
			}
		}
	}

	private static void BindRuntimeIdentity(Object i_runtimeAsset, ContentId i_id, ContentCategory i_category)
	{
		GameObject target = i_runtimeAsset is Component component ? component.gameObject : i_runtimeAsset as GameObject;
		if (target == null) return;
		RuntimeContentIdentity identity = target.GetComponent<RuntimeContentIdentity>();
		if (identity == null) identity = target.AddComponent<RuntimeContentIdentity>();
		identity.Configure(i_id, i_category);
	}

	private static T FindSceneObject<T>(T[] i_objects) where T : Component
	{
		foreach (T item in i_objects)
		{
			if (item != null && item.gameObject.scene.IsValid()) return item;
		}
		return null;
	}

	private static Object Resolve(CoreContentCatalogEntry i_entry, Library i_library, ManagerStages i_stageManager, ManagerChallenge i_challengeManager)
	{
		if (i_entry.Category == ContentCategory.Enemy)
		{
			if (i_library.Actors == null) return null;
			return i_entry.LegacyId.HasValue
				? i_library.Actors.GetNpc(i_entry.LegacyId.Value)
				: i_library.Actors.GetNpc(i_entry.LegacyName);
		}
		if (i_entry.Category == ContentCategory.Stage)
		{
			foreach (Stage stage in i_stageManager.GetAllStages())
			{
				if (i_entry.LegacyId.HasValue && stage.GetId() == i_entry.LegacyId.Value) return stage;
			}
		}
		if (i_entry.Category == ContentCategory.Clothing)
		{
			return i_library.Clothes == null || !i_entry.LegacyId.HasValue ? null : i_library.Clothes.GetClothing(i_entry.LegacyId.Value);
		}
		if (i_entry.Category == ContentCategory.Challenge && i_entry.LegacyId.HasValue && i_challengeManager != null)
		{
			foreach (Challenge challenge in i_challengeManager.GetAllChallenges())
			{
				if (challenge.GetId() == i_entry.LegacyId.Value) return challenge;
			}
		}
		if (i_entry.Category == ContentCategory.Item)
		{
			Gun gun = i_library.Guns == null ? null : i_library.Guns.GetGun(i_entry.LegacyName);
			if (gun != null) return gun;
			if (i_library.Usables != null)
			{
				foreach (GameObject usableObject in i_library.Usables.GetAllUsables())
				{
					Usable usable = usableObject.GetComponent<Usable>();
					if (usable != null && usable.GetName() == i_entry.LegacyName) return usable;
				}
			}
			if (i_library.Items != null)
			{
				AmmoBox ammoBox = i_library.Items.GetAmmoBoxTemplate();
				if (ammoBox != null && ammoBox.GetName() == i_entry.LegacyName) return ammoBox;
			}
		}
		return null;
	}
}

public sealed class ModValidationPresenter : MonoBehaviour
{
	private const float ToastInterval = 0.75f;
	private const float ToastDuration = 7f;
	private readonly Queue<ValidationIssue> m_pending = new Queue<ValidationIssue>();
	private readonly HashSet<string> m_seen = new HashSet<string>();
	private int m_issueIndex;
	private float m_nextToastTime;

	private void Update()
	{
		CollectNewErrors();
		if (m_pending.Count == 0 || Time.unscaledTime < m_nextToastTime || !IsGameplayHudActive()) return;
		StatusPlayerHud hud = FindSceneStatusHud();
		if (hud == null) return;
		ValidationIssue issue = m_pending.Dequeue();
		string source = string.IsNullOrEmpty(issue.Source) ? string.Empty : " (" + System.IO.Path.GetFileName(issue.Source) + ")";
		hud.CreateAndAddStatus("Mod error: " + issue.Code, issue.Message + source, StatusPlayerHudItemColor.Combat, ToastDuration);
		m_nextToastTime = Time.unscaledTime + ToastInterval;
	}

	private void CollectNewErrors()
	{
		IReadOnlyList<ValidationIssue> issues = ModLoaderRuntime.LastReport.Issues;
		while (m_issueIndex < issues.Count)
		{
			ValidationIssue issue = issues[m_issueIndex++];
			if (issue.Severity != ValidationSeverity.Error) continue;
			string key = issue.Code + "\n" + issue.Source + "\n" + issue.Message;
			if (m_seen.Add(key)) m_pending.Enqueue(issue);
		}
	}

	private static bool IsGameplayHudActive()
	{
		if (CommonReferences.Instance == null || CommonReferences.Instance.GetManagerStages() == null) return false;
		Stage stage = CommonReferences.Instance.GetManagerStages().GetStageCurrent();
		return stage != null && !(stage is StageHub) && stage.gameObject.activeInHierarchy;
	}

	private static StatusPlayerHud FindSceneStatusHud()
	{
		foreach (StatusPlayerHud hud in Resources.FindObjectsOfTypeAll<StatusPlayerHud>())
			if (hud != null && hud.gameObject.scene.IsValid()) return hud;
		return null;
	}
}

public sealed class ModManagerPresenter : MonoBehaviour
{
	private const int RowsPerPage = 9;
	private const string CatalogUrl = "https://raw.githubusercontent.com/RealmsStuff/CR-Mods/main/catalog-v1.json";
	private static readonly string[] TypeFilterLabels = { "ALL TYPES", "STAGES", "CHARACTERS", "ITEMS", "GAMEPLAY", "OTHER" };
	private static readonly string[] StatusFilterLabels = { "ALL MODS", "UPDATES", "INSTALLED", "NOT INSTALLED" };
	private readonly List<UIButton> m_rows = new List<UIButton>();
	private readonly List<Image> m_updateIcons = new List<Image>();
	private List<ModPackStatus> m_statuses;
	private readonly List<ModCatalogPack> m_catalogPacks = new List<ModCatalogPack>();
	private readonly List<ModCatalogPack> m_filteredCatalogPacks = new List<ModCatalogPack>();
	private readonly List<LocalCapmodArchive> m_localArchives = new List<LocalCapmodArchive>();
	private readonly HashSet<string> m_newInstallIds = new HashSet<string>(StringComparer.Ordinal);
	private readonly HashSet<string> m_changedIds = new HashSet<string>(StringComparer.Ordinal);
	private readonly HashSet<string> m_removedIds = new HashSet<string>(StringComparer.Ordinal);
	private UIButton m_openButton;
	private ScreenTitle m_screenTitle;
	private GameObject m_panel;
	private Text m_detailText;
	private ScrollRect m_detailScroll;
	private Image m_previewImage;
	private Texture2D m_previewTexture;
	private Sprite m_previewSprite;
	private int m_previewStatusIndex = -1;
	private string m_previewCatalogId;
	private int m_previewGeneration;
	private UnityWebRequest m_catalogPreviewRequest;
	private int m_previewImageIndex;
	private float m_nextPreviewTime;
	private Text m_pageText;
	private UIButton m_browseButton;
	private UIButton m_localButton;
	private InputField m_searchInput;
	private UIButton m_typeFilterButton;
	private UIButton m_statusFilterButton;
	private int m_typeFilter;
	private int m_statusFilter;
	private UIButton m_toggleButton;
	private UIButton m_rollbackButton;
	private UIButton m_uninstallButton;
	private string m_confirmRemoveId;
	private string m_confirmModifiedId;
	private UIButton m_previousButton;
	private UIButton m_nextButton;
	private UIButton m_closeButton;
	private Font m_font;
	private int m_page;
	private int m_selectedIndex = -1;
	private bool m_pendingRestart;
	private bool m_browsing;
	private bool m_localBrowsing;
	private bool m_catalogBusy;
	private bool m_installBusy;
	private string m_installMessage = string.Empty;
	private string m_catalogMessage = string.Empty;

	private void Update()
	{
		if (m_openButton == null) TryInstall();
		if (m_panel != null && m_panel.activeSelf && Input.GetKeyDown(KeyCode.Escape)) Close();
		if (m_panel != null && m_panel.activeSelf && Time.unscaledTime >= m_nextPreviewTime)
		{
			if (m_browsing && m_previewCatalogId != null)
			{
				ModCatalogPack pack = m_catalogPacks.Find(item => item.Id == m_previewCatalogId);
				if (pack != null) ShowCatalogPreview(pack, m_previewImageIndex + 1);
			}
			else if (!m_browsing && m_previewStatusIndex >= 0) ShowPreview(m_previewImageIndex + 1);
		}
	}

	private void OnDestroy() { ClearPreview(); }

	private void TryInstall()
	{
		ScreenTitle title = FindSceneTitle();
		if (title == null) return;
		if (m_panel == null)
		{
			m_rows.Clear(); m_updateIcons.Clear();
			m_toggleButton = null; m_rollbackButton = null; m_uninstallButton = null;
			m_previousButton = null; m_nextButton = null; m_closeButton = null;
			m_detailText = null; m_detailScroll = null; m_pageText = null; m_previewImage = null; m_browseButton = null; m_localButton = null;
			m_searchInput = null; m_typeFilterButton = null; m_statusFilterButton = null;
			ClearPreview(); m_previewStatusIndex = -1;
		}
		UIButton template = null;
		foreach (UIButton button in title.GetComponentsInChildren<UIButton>(true))
			if (button.name == "BtnOptions") { template = button; break; }
		if (template == null) return;
		m_font = template.GetComponentInChildren<Text>(true)?.font;
		m_openButton = Instantiate(template, template.transform.parent);
		m_openButton.name = "BtnMods";
		m_openButton.transform.SetSiblingIndex(Mathf.Min(template.transform.GetSiblingIndex() + 1, template.transform.parent.childCount - 1));
		m_openButton.onClick = new UIButton.ButtonClickedEvent();
		m_openButton.onClick.AddListener(Open);
		SetButtonText(m_openButton, "Mods");
		SetPurpleButtonColors(m_openButton);
		Text modsText = m_openButton.GetComponentInChildren<Text>(true);
		if (modsText != null) modsText.color = new Color(0.88f, 0.7f, 1f, 1f);
		m_screenTitle = title;
		LayoutRebuilder.ForceRebuildLayoutImmediate(template.transform.parent as RectTransform);
		Canvas canvas = template.GetComponentInParent<Canvas>();
		if (canvas == null)
		{
			ModLoaderRuntime.LastReport.Add(ValidationSeverity.Error, "ui.mods-canvas-missing",
				"Cannot open the Mods screen because the title buttons have no Canvas.");
			Destroy(m_openButton.gameObject);
			m_openButton = null;
			return;
		}
		UIButton optionsPageButton = null;
		foreach (UIButton button in title.GetComponentsInChildren<UIButton>(true))
			if (button.name == "btn_ok") { optionsPageButton = button; break; }
		ManagerOptions optionsManager = title.GetComponentInChildren<ManagerOptions>(true);
		BuildPanel(canvas.transform, optionsPageButton == null ? template : optionsPageButton,
			optionsManager == null ? null : optionsManager.GetOptionsInnerTemplate());
	}

	private void BuildPanel(Transform i_parent, UIButton i_template, Transform i_optionsInner)
	{
		m_panel = CreateUiObject("ModManager", i_parent);
		RectTransform panelRect = m_panel.GetComponent<RectTransform>();
		Stretch(panelRect);
		Image blocker = m_panel.AddComponent<Image>();
		blocker.color = Color.clear;
		blocker.raycastTarget = true;
		CanvasGroup canvasGroup = m_panel.AddComponent<CanvasGroup>();
		canvasGroup.interactable = true; canvasGroup.blocksRaycasts = true;

		GameObject frame = CreateUiObject("Frame", m_panel.transform);
		RectTransform frameRect = frame.GetComponent<RectTransform>();
		frameRect.anchorMin = frameRect.anchorMax = frameRect.pivot = new Vector2(0.5f, 0.5f);
		frameRect.sizeDelta = new Vector2(1280f, 840f);
		GameObject inner = CreateUiObject("Inner", frame.transform);
		RectTransform innerRect = inner.GetComponent<RectTransform>(); Stretch(innerRect);
		CloneOptionsChrome(i_optionsInner, inner.transform);

		Text listHeading = CreateText("ListHeading", inner.transform, "INSTALLED MODS", 26, TextAnchor.MiddleCenter, Color.white);
		SetFixedRect(listHeading.rectTransform, 28f, 724f, 570f, 44f, new Vector2(0f, 0f));
		m_searchInput = CreateSearchInput(inner.transform);
		SetFixedRect(m_searchInput.GetComponent<RectTransform>(), 28f, 776f, 430f, 44f, new Vector2(0f, 0f));
		m_searchInput.onValueChanged.AddListener(OnSearchChanged);
		m_typeFilterButton = CreateButton(i_template, inner.transform, "TypeFilter", TypeFilterLabels[0], CycleTypeFilter);
		SetNeutralButtonColors(m_typeFilterButton); SetButtonFontSize(m_typeFilterButton, 18);
		SetFixedRect(m_typeFilterButton.GetComponent<RectTransform>(), 470f, 776f, 220f, 44f, new Vector2(0f, 0f));
		m_statusFilterButton = CreateButton(i_template, inner.transform, "StatusFilter", StatusFilterLabels[0], CycleStatusFilter);
		SetNeutralButtonColors(m_statusFilterButton); SetButtonFontSize(m_statusFilterButton, 18);
		SetFixedRect(m_statusFilterButton.GetComponent<RectTransform>(), 702f, 776f, 220f, 44f, new Vector2(0f, 0f));
		m_browseButton = CreateButton(i_template, inner.transform, "Browse", "BROWSE", ToggleBrowse);
		SetNeutralButtonColors(m_browseButton); SetButtonFontSize(m_browseButton, 22);
		SetFixedRect(m_browseButton.GetComponent<RectTransform>(), 1034f, 776f, 218f, 44f, new Vector2(0f, 0f));
		m_browseButton.interactable = true;
		m_localButton = CreateButton(i_template, inner.transform, "LocalArchives", "LOCAL FILES", ToggleLocalArchives);
		SetNeutralButtonColors(m_localButton); SetButtonFontSize(m_localButton, 18);
		SetFixedRect(m_localButton.GetComponent<RectTransform>(), 702f, 724f, 220f, 44f, new Vector2(0f, 0f));

		GameObject list = CreateUiObject("PackList", inner.transform);
		RectTransform listRect = list.GetComponent<RectTransform>();
		SetRect(listRect, 28f, 90f, 598f, -116f, 0f, 0f, 0f, 1f);
		for (int index = 0; index < RowsPerPage; index++)
		{
			int visibleRow = index;
			UIButton row = CreateButton(i_template, list.transform, "Pack" + index, string.Empty, delegate { SelectRow(visibleRow); });
			SetButtonFontSize(row, 23);
			SetNeutralButtonColors(row);
			Text rowLabel = row.GetComponentInChildren<Text>(true);
			if (rowLabel != null)
			{
				rowLabel.supportRichText = false;
				rowLabel.rectTransform.offsetMax = new Vector2(-54f, rowLabel.rectTransform.offsetMax.y);
			}
			RectTransform rect = row.GetComponent<RectTransform>();
			rect.anchorMin = new Vector2(0f, 1f); rect.anchorMax = new Vector2(1f, 1f); rect.pivot = new Vector2(0.5f, 1f);
			rect.anchoredPosition = new Vector2(0f, -index * 70f); rect.sizeDelta = new Vector2(0f, 58f);
			m_rows.Add(row);
			Image updateIcon = InputGlyphLibrary.GetOrCreateImage(row.transform, "UpdateIcon");
			updateIcon.sprite = InputGlyphLibrary.GetMobileIcon(ClassicButton.MobileButtonType.Reload);
			updateIcon.color = Color.white;
			updateIcon.preserveAspect = true;
			updateIcon.raycastTarget = false;
			RectTransform updateRect = updateIcon.rectTransform;
			updateRect.anchorMin = updateRect.anchorMax = updateRect.pivot = new Vector2(1f, 0.5f);
			updateRect.anchoredPosition = new Vector2(-16f, 0f);
			updateRect.sizeDelta = new Vector2(32f, 32f);
			updateIcon.gameObject.SetActive(false);
			m_updateIcons.Add(updateIcon);
		}

		GameObject previewBox = CreateUiObject("PreviewBox", inner.transform);
		Image previewBackground = previewBox.AddComponent<Image>();
		previewBackground.color = new Color(0.07f, 0.07f, 0.09f, 1f);
		previewBackground.raycastTarget = false;
		SetRect(previewBox.GetComponent<RectTransform>(), 646f, 445f, -38f, -116f, 0f, 0f, 1f, 1f);
		GameObject preview = CreateUiObject("PreviewImage", previewBox.transform);
		m_previewImage = preview.AddComponent<Image>();
		m_previewImage.preserveAspect = true;
		m_previewImage.raycastTarget = false;
		m_previewImage.enabled = false;
		Stretch(preview.GetComponent<RectTransform>(), 6f);

		GameObject detailViewport = CreateUiObject("DetailsViewport", inner.transform);
		SetRect(detailViewport.GetComponent<RectTransform>(), 646f, 168f, -38f, -405f, 0f, 0f, 1f, 1f);
		Image detailMaskImage = detailViewport.AddComponent<Image>();
		detailMaskImage.color = Color.clear;
		detailViewport.AddComponent<RectMask2D>();
		m_detailScroll = detailViewport.AddComponent<ScrollRect>();
		m_detailScroll.horizontal = false; m_detailScroll.vertical = true;
		m_detailScroll.movementType = ScrollRect.MovementType.Clamped;
		m_detailText = CreateText("Details", detailViewport.transform, string.Empty, 21, TextAnchor.UpperLeft, Color.white);
		m_detailText.supportRichText = false;
		m_detailText.horizontalOverflow = HorizontalWrapMode.Wrap; m_detailText.verticalOverflow = VerticalWrapMode.Overflow;
		RectTransform detailContent = m_detailText.rectTransform;
		detailContent.anchorMin = new Vector2(0f, 1f); detailContent.anchorMax = new Vector2(1f, 1f);
		detailContent.pivot = new Vector2(0.5f, 1f); detailContent.anchoredPosition = new Vector2(-12f, 0f);
		detailContent.sizeDelta = new Vector2(-24f, 0f);
		ContentSizeFitter detailFitter = m_detailText.gameObject.AddComponent<ContentSizeFitter>();
		detailFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
		m_detailScroll.content = detailContent;
		m_detailScroll.viewport = detailViewport.GetComponent<RectTransform>();
		GameObject detailBar = CreateUiObject("DetailsScrollbar", detailViewport.transform);
		RectTransform detailBarRect = detailBar.GetComponent<RectTransform>();
		detailBarRect.anchorMin = new Vector2(1f, 0f); detailBarRect.anchorMax = new Vector2(1f, 1f);
		detailBarRect.pivot = new Vector2(1f, 0.5f); detailBarRect.anchoredPosition = new Vector2(-2f, 0f);
		detailBarRect.sizeDelta = new Vector2(16f, -4f);
		Image detailBarBackground = detailBar.AddComponent<Image>();
		detailBarBackground.color = new Color(0.18f, 0.02f, 0.02f, 0.9f);
		Scrollbar detailScrollbar = detailBar.AddComponent<Scrollbar>();
		detailScrollbar.direction = Scrollbar.Direction.BottomToTop;
		GameObject detailHandle = CreateUiObject("Handle", detailBar.transform);
		Stretch(detailHandle.GetComponent<RectTransform>(), 2f);
		Image detailHandleImage = detailHandle.AddComponent<Image>();
		detailHandleImage.color = new Color(0.85f, 0.08f, 0.08f, 1f);
		detailScrollbar.handleRect = detailHandle.GetComponent<RectTransform>();
		detailScrollbar.targetGraphic = detailHandleImage;
		m_detailScroll.verticalScrollbar = detailScrollbar;

		m_previousButton = CreateButton(i_template, inner.transform, "Previous", "<", delegate { ChangePage(-1); });
		SetNeutralButtonColors(m_previousButton); SetButtonFontSize(m_previousButton, 22);
		SetFixedRect(m_previousButton.GetComponent<RectTransform>(), 28f, 18f, 90f, 58f, new Vector2(0f, 0f));
		m_nextButton = CreateButton(i_template, inner.transform, "Next", ">", delegate { ChangePage(1); });
		SetNeutralButtonColors(m_nextButton); SetButtonFontSize(m_nextButton, 22);
		SetFixedRect(m_nextButton.GetComponent<RectTransform>(), 508f, 18f, 90f, 58f, new Vector2(0f, 0f));
		m_pageText = CreateText("Page", inner.transform, string.Empty, 21, TextAnchor.MiddleCenter, Color.white);
		SetFixedRect(m_pageText.rectTransform, 128f, 18f, 370f, 58f, new Vector2(0f, 0f));

		m_toggleButton = CreateButton(i_template, inner.transform, "Toggle", "", ToggleSelected);
		SetNeutralButtonColors(m_toggleButton); SetButtonFontSize(m_toggleButton, 20);
		SetFixedRect(m_toggleButton.GetComponent<RectTransform>(), 646f, 18f, 370f, 58f, new Vector2(0f, 0f));
		m_rollbackButton = CreateButton(i_template, inner.transform, "Rollback", "ROLL BACK", RollbackSelected);
		SetNeutralButtonColors(m_rollbackButton); SetButtonFontSize(m_rollbackButton, 18);
		SetFixedRect(m_rollbackButton.GetComponent<RectTransform>(), 646f, 91f, 180f, 58f, new Vector2(0f, 0f));
		m_uninstallButton = CreateButton(i_template, inner.transform, "Uninstall", "UNINSTALL", UninstallSelected);
		SetNeutralButtonColors(m_uninstallButton); SetButtonFontSize(m_uninstallButton, 18);
		SetFixedRect(m_uninstallButton.GetComponent<RectTransform>(), 836f, 91f, 180f, 58f, new Vector2(0f, 0f));
		m_closeButton = CreateButton(i_template, inner.transform, "Close", "CLOSE", Close);
		SetNeutralButtonColors(m_closeButton); SetButtonFontSize(m_closeButton, 22);
		SetFixedRect(m_closeButton.GetComponent<RectTransform>(), 1034f, 18f, 218f, 58f, new Vector2(0f, 0f));
		m_panel.SetActive(false);
	}

	private static void CloneOptionsChrome(Transform i_optionsInner, Transform i_parent)
	{
		if (i_optionsInner == null) return;
		Transform backgroundTemplate = i_optionsInner.Find("img_bg");
		if (backgroundTemplate != null)
		{
			GameObject background = Instantiate(backgroundTemplate.gameObject, i_parent);
			background.name = "img_bg";
			background.transform.SetAsFirstSibling();
			foreach (Graphic graphic in background.GetComponentsInChildren<Graphic>(true)) graphic.raycastTarget = false;
		}
		Transform topTemplate = i_optionsInner.Find("Top");
		if (topTemplate == null) return;
		GameObject top = Instantiate(topTemplate.gameObject, i_parent);
		top.name = "Top";
		Text heading = top.GetComponentInChildren<Text>(true);
		if (heading != null)
		{
			heading.text = "Mods";
			heading.raycastTarget = false;
		}
	}

	private void Open()
	{
		m_statuses = new List<ModPackStatus>(ModLoaderRuntime.PackStatuses);
		m_removedIds.Clear();
		foreach (string id in ModInstallRecovery.RemovedIds(ModStoragePaths.GetModsDirectory()))
		{
			m_removedIds.Add(id);
			if (!m_statuses.Exists(status => status.Id == id))
				m_statuses.Add(new ModPackStatus(id, id + " (removed)",
					ModInstallRecovery.RemovedVersion(ModStoragePaths.GetModsDirectory(), id), string.Empty,
					ModPackState.Disabled, "Saved in recovery storage."));
		}
		m_statuses.Sort((left, right) => { int state = left.State.CompareTo(right.State); return state != 0 ? state : string.CompareOrdinal(left.DisplayName, right.DisplayName); });
		m_browsing = false;
		m_localBrowsing = false;
		m_typeFilter = 0;
		m_statusFilter = 0;
		if (m_searchInput != null) m_searchInput.SetTextWithoutNotify(string.Empty);
		ApplyCatalogFilters();
		m_confirmRemoveId = null;
		m_confirmModifiedId = null;
		m_page = 0; m_selectedIndex = m_statuses.Count == 0 ? -1 : 0;
		m_previewStatusIndex = -1;
		ClearPreview();
		if (m_screenTitle != null) m_screenTitle.SetTitleMenuVisible(false);
		m_panel.SetActive(true);
		m_panel.transform.SetAsLastSibling();
		Refresh();
		SelectFirstVisible();
	}

	private void ToggleBrowse()
	{
		if (m_catalogBusy) return;
		m_browsing = !m_browsing;
		m_localBrowsing = false;
		m_confirmRemoveId = null;
		m_confirmModifiedId = null;
		m_page = 0;
		m_selectedIndex = ItemCount == 0 ? -1 : 0;
		ClearPreview(); m_previewStatusIndex = -1;
		if (m_browsing && m_catalogPacks.Count == 0) StartCoroutine(LoadCatalog());
		else { Refresh(); SelectFirstVisible(); }
	}

	private void ToggleLocalArchives()
	{
		if (m_installBusy) return;
		m_localBrowsing = !m_localBrowsing;
		m_browsing = false;
		m_localArchives.Clear();
		if (m_localBrowsing)
		{
			try
			{
				foreach (string path in LocalCapmodInstaller.FindArchives(ModStoragePaths.GetModsDirectory()))
					m_localArchives.Add(LocalCapmodInstaller.Inspect(path));
			}
			catch (Exception exception) { m_installMessage = "Could not list local archives: " + exception.Message; }
		}
		m_confirmModifiedId = null;
		m_page = 0;
		m_selectedIndex = ItemCount == 0 ? -1 : 0;
		ClearPreview(); m_previewStatusIndex = -1;
		Refresh(); SelectFirstVisible();
	}

	private IEnumerator LoadCatalog()
	{
		m_catalogBusy = true;
		m_catalogMessage = "Loading catalog...";
		Refresh();
		string json = null;
		string source = CatalogUrl;
		using (UnityWebRequest request = UnityWebRequest.Get(CatalogUrl))
		{
			request.timeout = 10;
			yield return request.SendWebRequest();
			if (request.result == UnityWebRequest.Result.Success) json = request.downloadHandler.text;
			else m_catalogMessage = "Online catalog unavailable: " + request.error;
		}
		ModCatalogParseResult parsed = json == null ? null : ModCatalogParser.Parse(json, source);
		if (parsed != null && parsed.Report.IsValid)
		{
			SetCatalog(parsed.Catalog);
			TryWriteCatalogCache(json);
			m_catalogMessage = m_catalogPacks.Count == 0 ? "The catalog currently has no listings." : string.Empty;
		}
		else
		{
			if (parsed != null) m_catalogMessage = FirstCatalogError(parsed.Report);
			if (!TryLoadCatalogCache()) TryLoadBundledCatalog();
		}
		m_catalogBusy = false;
		m_selectedIndex = ItemCount == 0 ? -1 : 0;
		Refresh(); SelectFirstVisible();
	}

	private IEnumerator InstallSelected()
	{
		ModCatalogPack pack = m_filteredCatalogPacks[m_selectedIndex];
		ModCatalogVersion version = LatestVersion(pack);
		string mods = ModStoragePaths.GetModsDirectory();
		if (version == null || m_changedIds.Contains(pack.Id)
			|| (IsInstalled(pack.Id) && !HasUpdate(pack, version)) || !IsGameVersionCompatible(version)
			|| version.ModApiVersion != ModManifestParser.SupportedModApiVersion || string.IsNullOrEmpty(mods)
			|| version.SizeBytes > ModCatalogPlatformPolicy.MaximumArchiveBytes(Application.platform))
		{
			m_installMessage = "This version is not compatible with this game or mod storage is unavailable.";
			RefreshDetails();
			yield break;
		}
		ModInstallPlan plan = PlanSelected(pack, version);
		if (!plan.Report.IsValid)
		{
			m_installMessage = FirstInstallError(plan.Report);
			RefreshDetails();
			yield break;
		}
		long totalBytes = 0;
		foreach (ModInstallChoice choice in plan.Downloads)
		{
			if (m_changedIds.Contains(choice.Pack.Id))
			{
				m_installMessage = "Restart before changing '" + choice.Pack.Id + "' again.";
				RefreshDetails(); yield break;
			}
			totalBytes += choice.Version.SizeBytes;
		}
		if (plan.Downloads.Count > 16 || totalBytes > ModCatalogPlatformPolicy.MaximumBatchBytes(Application.platform))
		{
			m_installMessage = "Dependency batch exceeds the download limit (16 packs / 512 MiB, or 64 MiB on WebGL).";
			RefreshDetails(); yield break;
		}
		HashSet<string> preserveModified = ModifiedUpdateIds(plan);
		if (preserveModified.Count > 0 && m_confirmModifiedId != pack.Id)
		{
			m_confirmModifiedId = pack.Id;
			m_installMessage = "LOCAL FILE WARNING: " + string.Join(", ", preserveModified)
				+ " differs from a tracked catalog release or has no tracking record. Press CONFIRM UPDATE to preserve a copy and continue.";
			RefreshDetails();
			yield break;
		}
		m_confirmModifiedId = null;
		m_installBusy = true;
		m_installMessage = "Preparing " + plan.Downloads.Count + " release" + (plan.Downloads.Count == 1 ? "" : "s") + "...";
		Refresh();
		string folder = null;
		try
		{
			folder = System.IO.Path.Combine(Application.persistentDataPath, "ModDownloads", Guid.NewGuid().ToString("N"));
			System.IO.Directory.CreateDirectory(folder);
		}
		catch (Exception exception) { m_installMessage = "Could not prepare download: " + exception.Message; }
		Dictionary<string, string> archives = new Dictionary<string, string>(StringComparer.Ordinal);
		bool failed = folder == null;
		if (!failed)
		{
			bool webGl = ModCatalogPlatformPolicy.UsesMemoryDownload(Application.platform);
			for (int index = 0; index < plan.Downloads.Count; index++)
			{
				ModInstallChoice choice = plan.Downloads[index];
				string archive = System.IO.Path.Combine(folder, Guid.NewGuid().ToString("N") + ".zip");
				using (UnityWebRequest request = webGl ? UnityWebRequest.Get(choice.Version.Download)
					: new UnityWebRequest(choice.Version.Download, "GET"))
				{
					if (!webGl) request.downloadHandler = new DownloadHandlerFile(archive);
					request.timeout = 30;
					UnityWebRequestAsyncOperation operation = request.SendWebRequest();
					bool oversized = false;
					while (!operation.isDone)
					{
						if (request.downloadedBytes > (ulong)choice.Version.SizeBytes)
						{ oversized = true; request.Abort(); }
						m_installMessage = "Downloading " + (index + 1) + "/" + plan.Downloads.Count + " " + choice.Pack.DisplayName
							+ "  " + (request.downloadedBytes / 1024UL) + " / " + (choice.Version.SizeBytes / 1024L) + " KiB...";
						RefreshDetails();
						yield return null;
					}
					if (oversized || request.result != UnityWebRequest.Result.Success
						|| request.downloadedBytes != (ulong)choice.Version.SizeBytes)
					{
						m_installMessage = oversized ? "Download exceeded its catalog size limit."
							: "Download failed or size differed from catalog for '" + choice.Pack.Id + "': " + request.error;
						failed = true;
						break;
					}
					try
					{
						if (webGl) System.IO.File.WriteAllBytes(archive, request.downloadHandler.data);
						ValidationReport preflight = ModArchiveValidator.Validate(archive, choice.Pack, choice.Version);
						if (!preflight.IsValid)
						{ m_installMessage = FirstInstallError(preflight); failed = true; break; }
						archives.Add(choice.Pack.Id, archive);
					}
					catch (Exception exception) { m_installMessage = "Download validation failed: " + exception.Message; failed = true; break; }
				}
			}
			if (!failed)
			{
				m_installMessage = "Validating all mods before installation...";
				RefreshDetails();
				ValidationReport installed = ModInstallTransaction.Apply(mods, plan, archives,
					ModLoaderRuntime.LoadedPacks, preserveModified);
				if (installed.IsValid)
				{
					foreach (ModInstallChoice choice in plan.Downloads)
					{
						if (choice.IsUpdate) m_changedIds.Add(choice.Pack.Id);
						else
						{
							m_newInstallIds.Add(choice.Pack.Id);
							ModEnableState.SetEnabled(choice.Pack.Id, true);
						}
					}
					m_pendingRestart = true;
					m_installMessage = "Installed " + plan.Downloads.Count + " release" + (plan.Downloads.Count == 1 ? "" : "s")
						+ " together. Restart to activate; previous versions remain available for rollback.";
				}
				else m_installMessage = FirstInstallError(installed);
			}
			try { if (System.IO.Directory.Exists(folder)) System.IO.Directory.Delete(folder, true); }
			catch (Exception exception) { Debug.LogWarning("[ModLoader] Could not remove temporary downloads: " + exception.Message); }
		}
		m_installBusy = false;
		ApplyCatalogFilters();
		Refresh();
	}

	private HashSet<string> ModifiedUpdateIds(ModInstallPlan i_plan)
	{
		HashSet<string> result = new HashSet<string>(StringComparer.Ordinal);
		if (i_plan == null || !i_plan.Report.IsValid) return result;
		string mods = ModStoragePaths.GetModsDirectory();
		foreach (ModInstallChoice choice in i_plan.Downloads)
			if (choice.IsUpdate && ModInstallIntegrity.Check(mods, choice.Pack.Id).NeedsConfirmation)
				result.Add(choice.Pack.Id);
		return result;
	}

	private ModInstallPlan PlanSelected(ModCatalogPack i_pack, ModCatalogVersion i_version)
	{
		ModInstallPlan plan = ModInstallPlanner.Plan(new ModCatalogDocument { SchemaVersion = 1, Packs = m_catalogPacks },
			i_pack, i_version, m_statuses == null ? new List<ModPackStatus>()
				: m_statuses.FindAll(status => !m_removedIds.Contains(status.Id)),
				candidate => candidate != null && candidate.ModApiVersion == ModManifestParser.SupportedModApiVersion
					&& IsGameVersionCompatible(candidate)
					&& candidate.SizeBytes <= ModCatalogPlatformPolicy.MaximumArchiveBytes(Application.platform));
		long bytes = 0;
		foreach (ModInstallChoice choice in plan.Downloads) bytes += choice.Version.SizeBytes;
		if (plan.Report.IsValid && (plan.Downloads.Count > 16
			|| bytes > ModCatalogPlatformPolicy.MaximumBatchBytes(Application.platform)))
			plan.Report.Add(ValidationSeverity.Error, "plan.size", "Dependency batch exceeds 16 packs or the platform download limit.");
		return plan;
	}

	private static bool IsGameVersionCompatible(ModCatalogVersion i_version)
	{
		if (i_version == null || string.IsNullOrEmpty(i_version.GameVersion)) return false;
		if (i_version.GameVersion == "*") return true;
		string requiredText = i_version.GameVersion.StartsWith(">=", StringComparison.Ordinal)
			? i_version.GameVersion.Substring(2) : i_version.GameVersion;
		if (!SemanticVersion.TryParse(requiredText, out SemanticVersion required)
			|| !SemanticVersion.TryParse(Application.version, out SemanticVersion current)) return false;
		return i_version.GameVersion.StartsWith(">=", StringComparison.Ordinal)
			? current.CompareTo(required) >= 0 : current.CompareTo(required) == 0;
	}

	private static string FirstInstallError(ValidationReport i_report)
	{
		foreach (ValidationIssue issue in i_report.Issues)
			if (issue.Severity == ValidationSeverity.Error) return "Install rejected: " + issue.Message;
		return "Install rejected.";
	}

	private void SetCatalog(ModCatalogDocument i_catalog)
	{
		m_catalogPacks.Clear();
		if (i_catalog?.Packs != null) m_catalogPacks.AddRange(i_catalog.Packs);
		m_catalogPacks.Sort((left, right) => string.Compare(left.DisplayName, right.DisplayName, StringComparison.OrdinalIgnoreCase));
		ApplyCatalogFilters();
	}

	private void OnSearchChanged(string i_value)
	{
		RefreshCatalogFilterResults();
	}

	private void CycleTypeFilter()
	{
		m_typeFilter = (m_typeFilter + 1) % TypeFilterLabels.Length;
		RefreshCatalogFilterResults();
	}

	private void CycleStatusFilter()
	{
		m_statusFilter = (m_statusFilter + 1) % StatusFilterLabels.Length;
		RefreshCatalogFilterResults();
	}

	private void RefreshCatalogFilterResults()
	{
		m_confirmModifiedId = null;
		m_installMessage = string.Empty;
		ApplyCatalogFilters();
		m_page = 0;
		m_selectedIndex = m_filteredCatalogPacks.Count == 0 ? -1 : 0;
		Refresh();
	}

	private void ApplyCatalogFilters()
	{
		m_filteredCatalogPacks.Clear();
		string query = m_searchInput == null ? string.Empty : m_searchInput.text.Trim();
		foreach (ModCatalogPack pack in m_catalogPacks)
		{
			if (!MatchesSearch(pack, query) || !MatchesTypeFilter(pack)) continue;
			bool installed = IsInstalled(pack.Id);
			if (m_statusFilter == 1 && !UpdateAvailable(pack)) continue;
			if (m_statusFilter == 2 && !installed) continue;
			if (m_statusFilter == 3 && installed) continue;
			m_filteredCatalogPacks.Add(pack);
		}
		if (m_browsing)
		{
			int pages = Mathf.Max(1, Mathf.CeilToInt((float)m_filteredCatalogPacks.Count / RowsPerPage));
			m_page = Mathf.Clamp(m_page, 0, pages - 1);
			m_selectedIndex = m_filteredCatalogPacks.Count == 0 ? -1
				: Mathf.Clamp(m_selectedIndex, 0, m_filteredCatalogPacks.Count - 1);
		}
	}

	private static bool MatchesSearch(ModCatalogPack i_pack, string i_query)
	{
		if (string.IsNullOrEmpty(i_query)) return true;
		if (ContainsIgnoreCase(i_pack.DisplayName, i_query) || ContainsIgnoreCase(i_pack.Id, i_query)
			|| ContainsIgnoreCase(i_pack.Summary, i_query)) return true;
		if (i_pack.Authors != null)
			foreach (string author in i_pack.Authors) if (ContainsIgnoreCase(author, i_query)) return true;
		if (i_pack.Tags != null)
			foreach (string tag in i_pack.Tags) if (ContainsIgnoreCase(tag, i_query)) return true;
		return false;
	}

	private bool MatchesTypeFilter(ModCatalogPack i_pack)
	{
		if (m_typeFilter == 0) return true;
		bool stage = HasAnyTag(i_pack, "stage", "map");
		bool character = HasAnyTag(i_pack, "enemy", "player", "clothing", "character", "actor");
		bool item = HasAnyTag(i_pack, "weapon", "item", "usable", "vendor");
		bool gameplay = HasAnyTag(i_pack, "difficulty", "game-mode", "gameplay", "rule-profile", "challenge", "mode");
		switch (m_typeFilter)
		{
		case 1: return stage;
		case 2: return character;
		case 3: return item;
		case 4: return gameplay;
		default: return !stage && !character && !item && !gameplay;
		}
	}

	private static bool HasAnyTag(ModCatalogPack i_pack, params string[] i_expected)
	{
		if (i_pack.Tags == null) return false;
		foreach (string tag in i_pack.Tags)
			foreach (string expected in i_expected)
				if (string.Equals(tag, expected, StringComparison.OrdinalIgnoreCase)) return true;
		return false;
	}

	private static bool ContainsIgnoreCase(string i_value, string i_query)
	{
		return !string.IsNullOrEmpty(i_value) && i_value.IndexOf(i_query, StringComparison.OrdinalIgnoreCase) >= 0;
	}

	private bool TryLoadCatalogCache()
	{
		if (!ModCatalogCache.TryRead(CatalogCachePath, out ModCatalogDocument cached, out ValidationReport report))
		{
			if (report.Issues.Count > 0) Debug.LogWarning("[ModLoader] Could not read catalog cache: " + report.Issues[0].Message);
			return false;
		}
		SetCatalog(cached);
		m_catalogMessage = "Showing the last downloaded catalog (offline).";
		return true;
	}

	private void TryLoadBundledCatalog()
	{
		TextAsset bundled = Resources.Load<TextAsset>("Modding/Catalog/catalog-v1");
		ModCatalogParseResult result = bundled == null ? null : ModCatalogParser.Parse(bundled.text, "bundled/catalog-v1.json");
		if (result != null && result.Report.IsValid)
		{
			SetCatalog(result.Catalog);
			m_catalogMessage = m_catalogPacks.Count == 0 ? "Online catalog unavailable. No bundled listings are available." : "Showing bundled catalog listings.";
		}
	}

	private void TryWriteCatalogCache(string i_json)
	{
		ValidationReport report = ModCatalogCache.WriteValidated(CatalogCachePath, i_json);
		if (!report.IsValid && report.Issues.Count > 0)
			Debug.LogWarning("[ModLoader] Could not write catalog cache: " + report.Issues[0].Message);
	}

	private static string CatalogCachePath => System.IO.Path.Combine(Application.persistentDataPath, "ModCatalog", "catalog-v1.json");
	private static string FirstCatalogError(ValidationReport i_report)
	{
		foreach (ValidationIssue issue in i_report.Issues) if (issue.Severity == ValidationSeverity.Error) return "Catalog rejected: " + issue.Message;
		return "Catalog rejected.";
	}
	private int ItemCount => m_localBrowsing ? m_localArchives.Count
		: m_browsing ? m_filteredCatalogPacks.Count : (m_statuses == null ? 0 : m_statuses.Count);

	private void Close()
	{
		if (m_panel == null || m_installBusy) return;
		m_panel.SetActive(false);
		m_confirmRemoveId = null;
		m_confirmModifiedId = null;
		ClearPreview(); m_previewStatusIndex = -1;
		if (m_screenTitle != null) m_screenTitle.SetTitleMenuVisible(true);
		if (EventSystem.current != null && m_openButton != null) EventSystem.current.SetSelectedGameObject(m_openButton.gameObject);
	}

	private void SelectRow(int i_visibleRow)
	{
		if (m_installBusy) return;
		m_installMessage = string.Empty;
		int index = m_page * RowsPerPage + i_visibleRow;
		if (index < 0 || index >= ItemCount) return;
		m_selectedIndex = index;
		m_confirmRemoveId = null;
		m_confirmModifiedId = null;
		if (m_detailScroll != null) m_detailScroll.verticalNormalizedPosition = 1f;
		if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(m_rows[i_visibleRow].gameObject);
		RefreshDetails();
	}

	private void ToggleSelected()
	{
		if (m_localBrowsing) { InstallLocalSelected(); return; }
		if (m_browsing)
		{
			if (!m_installBusy && m_selectedIndex >= 0 && m_selectedIndex < m_filteredCatalogPacks.Count)
				StartCoroutine(InstallSelected());
			return;
		}
		if (m_selectedIndex < 0 || m_selectedIndex >= m_statuses.Count) return;
		ModPackStatus status = m_statuses[m_selectedIndex];
		if (m_changedIds.Contains(status.Id)) return;
		if (!status.IsUserConfigurable) return;
		if (status.State == ModPackState.Conflicting)
		{
			ModEnableState.SetEnabled(status.Id, true);
			foreach (ModPackStatus other in m_statuses)
				if (other.IsUserConfigurable && other.Id != status.Id && PacksConflict(status.Pack, other.Pack))
					ModEnableState.SetEnabled(other.Id, false);
		}
		else ModEnableState.SetEnabled(status.Id, !ModEnableState.IsEnabled(status.Id));
		m_pendingRestart = true;
		RefreshDetails();
	}

	private void InstallLocalSelected()
	{
		if (m_installBusy || m_selectedIndex < 0 || m_selectedIndex >= m_localArchives.Count) return;
		LocalCapmodArchive archive = m_localArchives[m_selectedIndex];
		if (!archive.Report.IsValid || archive.Manifest == null) { m_installMessage = FirstInstallError(archive.Report); RefreshDetails(); return; }
		if (m_changedIds.Contains(archive.Manifest.Id))
		{ m_installMessage = "Restart before changing this mod again."; RefreshDetails(); return; }
		if (archive.Manifest.ModApiVersion != ModManifestParser.SupportedModApiVersion)
		{ m_installMessage = "This mod uses an incompatible Mod API version."; RefreshDetails(); return; }
		string mods = ModStoragePaths.GetModsDirectory();
		bool replacing = System.IO.Directory.Exists(System.IO.Path.Combine(mods, archive.Manifest.Id));
		if (replacing && m_confirmModifiedId != archive.Manifest.Id)
		{
			m_confirmModifiedId = archive.Manifest.Id;
			m_installMessage = "Press CONFIRM REPLACE to back up the installed version, then install this .capmod. Restart required.";
			RefreshDetails(); return;
		}
		m_confirmModifiedId = null;
		m_installBusy = true;
		m_installMessage = "Validating and staging local .capmod...";
		RefreshDetails();
		ValidationReport report = LocalCapmodInstaller.Install(archive, mods, replacing, ModLoaderRuntime.LoadedPacks);
		m_installBusy = false;
		if (report.IsValid)
		{
			m_changedIds.Add(archive.Manifest.Id);
			if (!replacing) { m_newInstallIds.Add(archive.Manifest.Id); ModEnableState.SetEnabled(archive.Manifest.Id, true); }
			m_pendingRestart = true;
			m_localArchives.RemoveAt(m_selectedIndex);
			m_selectedIndex = Mathf.Min(m_selectedIndex, m_localArchives.Count - 1);
			m_installMessage = "Installed. Restart to activate; the previous version remains available for rollback.";
		}
		else m_installMessage = FirstInstallError(report);
		Refresh();
	}

	private void RollbackSelected()
	{
		if (m_browsing || m_installBusy || m_selectedIndex < 0 || m_selectedIndex >= m_statuses.Count) return;
		ModPackStatus status = m_statuses[m_selectedIndex];
		if (m_changedIds.Contains(status.Id)) return;
		ValidationReport report = ModInstallRecovery.Rollback(ModStoragePaths.GetModsDirectory(), status.Id);
		m_installMessage = report.IsValid ? "Rolled back. Restart to activate the previous version." : FirstInstallError(report);
		if (report.IsValid) { m_changedIds.Add(status.Id); m_pendingRestart = true; }
		ApplyCatalogFilters();
		RefreshDetails();
	}

	private void RestoreSelected()
	{
		if (m_installBusy || m_selectedIndex < 0 || m_selectedIndex >= ItemCount) return;
		string id = m_browsing ? m_filteredCatalogPacks[m_selectedIndex].Id : m_statuses[m_selectedIndex].Id;
		if (IsInstalled(id) || !m_removedIds.Contains(id)) return;
		ValidationReport report = ModInstallRecovery.Restore(ModStoragePaths.GetModsDirectory(), id);
		m_installMessage = report.IsValid ? "Restored the removed copy. Restart to activate it." : FirstInstallError(report);
		if (report.IsValid) { m_removedIds.Remove(id); m_newInstallIds.Add(id); m_changedIds.Add(id); m_pendingRestart = true; }
		ApplyCatalogFilters();
		Refresh();
	}

	private void UninstallSelected()
	{
		if (m_browsing || m_installBusy || m_selectedIndex < 0 || m_selectedIndex >= m_statuses.Count) return;
		ModPackStatus status = m_statuses[m_selectedIndex];
		if (m_changedIds.Contains(status.Id)) return;
		if (m_confirmRemoveId != status.Id)
		{
			m_confirmRemoveId = status.Id;
			m_installMessage = "Press CONFIRM REMOVE again. A restorable copy will be saved.";
			RefreshDetails();
			return;
		}
		m_confirmRemoveId = null;
		ValidationReport report = ModInstallRecovery.Uninstall(ModStoragePaths.GetModsDirectory(), status.Id);
		m_installMessage = report.IsValid ? "Removed to recovery storage. Restart to unload it." : FirstInstallError(report);
		if (report.IsValid) { m_changedIds.Add(status.Id); m_removedIds.Add(status.Id); m_pendingRestart = true; }
		ApplyCatalogFilters();
		RefreshDetails();
	}

	private void ChangePage(int i_direction)
	{
		if (m_installBusy) return;
		int pages = Mathf.Max(1, Mathf.CeilToInt((float)ItemCount / RowsPerPage));
		m_page = Mathf.Clamp(m_page + i_direction, 0, pages - 1);
		m_selectedIndex = Mathf.Min(m_page * RowsPerPage, ItemCount - 1);
		m_confirmRemoveId = null;
		m_confirmModifiedId = null;
		Refresh(); SelectFirstVisible();
	}

	private void Refresh()
	{
		SetButtonText(m_browseButton, m_browsing ? "INSTALLED" : "BROWSE");
		SetButtonText(m_localButton, m_localBrowsing ? "INSTALLED" : "LOCAL FILES");
		m_localButton.interactable = !m_installBusy && !m_catalogBusy;
		m_localButton.gameObject.SetActive(!m_browsing);
		m_browseButton.interactable = !m_catalogBusy && !m_installBusy;
		m_searchInput.gameObject.SetActive(m_browsing);
		m_typeFilterButton.gameObject.SetActive(m_browsing);
		m_statusFilterButton.gameObject.SetActive(m_browsing);
		SetButtonText(m_typeFilterButton, TypeFilterLabels[m_typeFilter]);
		SetButtonText(m_statusFilterButton, StatusFilterLabels[m_statusFilter]);
		m_searchInput.interactable = !m_catalogBusy && !m_installBusy;
		m_typeFilterButton.interactable = !m_catalogBusy && !m_installBusy;
		m_statusFilterButton.interactable = !m_catalogBusy && !m_installBusy;
		Transform heading = m_panel.transform.Find("Frame/Inner/ListHeading");
		if (heading != null) heading.GetComponent<Text>().text = m_localBrowsing ? "LOCAL .CAPMOD FILES" : m_browsing ? "BROWSE MODS" : "INSTALLED MODS";
		int pages = Mathf.Max(1, Mathf.CeilToInt((float)ItemCount / RowsPerPage));
		m_pageText.text = "PAGE " + (m_page + 1) + " / " + pages;
		m_previousButton.interactable = !m_installBusy && m_page > 0;
		m_nextButton.interactable = !m_installBusy && m_page + 1 < pages;
		for (int row = 0; row < m_rows.Count; row++)
		{
			int index = m_page * RowsPerPage + row;
			bool visible = index < ItemCount;
			m_rows[row].gameObject.SetActive(visible);
			m_updateIcons[row].gameObject.SetActive(false);
			m_rows[row].interactable = visible && !m_installBusy;
			if (visible)
			{
				if (m_localBrowsing)
				{
					LocalCapmodArchive archive = m_localArchives[index];
					SetButtonText(m_rows[row], archive.Manifest?.DisplayName ?? System.IO.Path.GetFileName(archive.Path));
					SetStatusButtonColors(m_rows[row], archive.Report.IsValid ? ModPackState.Loaded : ModPackState.Invalid);
				}
				else if (m_browsing)
				{
					ModCatalogPack pack = m_filteredCatalogPacks[index];
					SetButtonText(m_rows[row], pack.DisplayName);
					SetCatalogButtonColors(m_rows[row], IsInstalled(pack.Id));
					m_updateIcons[row].gameObject.SetActive(UpdateAvailable(pack));
				}
				else
				{
					ModPackStatus status = m_statuses[index];
					SetButtonText(m_rows[row], status.DisplayName);
					SetStatusButtonColors(m_rows[row], status.State);
				}
			}
		}
		RefreshDetails();
	}

	private void RefreshDetails()
	{
		if (m_localBrowsing)
		{
			m_rollbackButton.gameObject.SetActive(false); m_uninstallButton.gameObject.SetActive(false);
			ClearPreview(); m_previewStatusIndex = -1;
			if (m_selectedIndex < 0 || m_selectedIndex >= m_localArchives.Count)
			{
				m_detailText.text = "Drop a .capmod into the Mods folder, then reopen LOCAL FILES."
					+ (m_installMessage.Length == 0 ? string.Empty : "\n\n" + m_installMessage);
				m_toggleButton.gameObject.SetActive(false); return;
			}
			LocalCapmodArchive archive = m_localArchives[m_selectedIndex];
			bool replacing = archive.Manifest != null && IsInstalled(archive.Manifest.Id);
			m_detailText.text = (archive.Manifest?.DisplayName ?? System.IO.Path.GetFileName(archive.Path))
				+ (archive.Manifest == null ? string.Empty : "  v" + archive.Manifest.Version + "\n" + archive.Manifest.Id
					+ "\n\n" + (archive.Manifest.Description ?? "No description provided.")
					+ "\n\nREQUIRED: " + DependencyList(archive.Manifest.Dependencies)
					+ "\nSIZE: " + FormatBytes(archive.Version.SizeBytes))
				+ (replacing ? "\n\nAn installed mod with this ID will be preserved for rollback." : string.Empty)
				+ (archive.Report.IsValid ? string.Empty : "\n\n" + FirstInstallError(archive.Report))
				+ (m_installMessage.Length == 0 ? string.Empty : "\n\n" + m_installMessage);
			m_toggleButton.gameObject.SetActive(true);
			m_toggleButton.interactable = !m_installBusy && archive.Report.IsValid && archive.Manifest != null
				&& archive.Manifest.ModApiVersion == ModManifestParser.SupportedModApiVersion
				&& !m_changedIds.Contains(archive.Manifest.Id);
			SetButtonText(m_toggleButton, replacing
				? m_confirmModifiedId == archive.Manifest?.Id ? "CONFIRM REPLACE" : "REPLACE (BACKUP FIRST)"
				: "INSTALL (RESTART REQUIRED)");
			return;
		}
		if (m_browsing)
		{
			m_toggleButton.gameObject.SetActive(false);
			m_rollbackButton.gameObject.SetActive(false); m_uninstallButton.gameObject.SetActive(false);
			if (m_selectedIndex < 0 || m_selectedIndex >= m_filteredCatalogPacks.Count)
			{
				ClearPreview(); m_previewStatusIndex = -1;
				m_detailText.text = m_catalogBusy || m_catalogPacks.Count == 0
					? m_catalogMessage : "No mods match the current search and filters.";
				return;
			}
			ModCatalogPack catalogPack = m_filteredCatalogPacks[m_selectedIndex];
			if (m_previewCatalogId != catalogPack.Id)
			{
				m_previewStatusIndex = -1;
				ShowCatalogPreview(catalogPack, 0);
			}
			ModCatalogVersion version = LatestVersion(catalogPack);
			bool installed = IsInstalled(catalogPack.Id);
			bool update = installed && HasUpdate(catalogPack, version);
			bool removed = !installed && ModInstallRecovery.HasRemoved(ModStoragePaths.GetModsDirectory(), catalogPack.Id);
			ModInstallPlan plan = m_installBusy || version == null || (installed && !update) ? null : PlanSelected(catalogPack, version);
			HashSet<string> modifiedUpdates = ModifiedUpdateIds(plan);
			string requirements = "\n\nREQUIRED: ";
			if (version?.Dependencies == null || version.Dependencies.Count == 0) requirements += "none";
			else foreach (ModDependency dependency in version.Dependencies)
				requirements += "\n  " + dependency.Id + " " + dependency.Version;
			if (plan != null && plan.Report.IsValid && plan.Downloads.Count > 1)
			{
				requirements += "\n\nWILL DOWNLOAD TOGETHER:";
				foreach (ModInstallChoice choice in plan.Downloads)
					if (choice.Pack.Id != catalogPack.Id) requirements += "\n  " + choice.Pack.Id + " v" + choice.Version.Version
						+ (choice.IsUpdate ? " (update)" : " (new)");
			}
			if (plan != null && plan.Report.IsValid && plan.SatisfiedRequirements.Count > 0)
			{
				requirements += "\n\nALREADY SATISFIED:";
				foreach (string satisfied in plan.SatisfiedRequirements) requirements += "\n  " + satisfied;
			}
			if (plan != null && !plan.Report.IsValid) requirements += "\n\n" + FirstInstallError(plan.Report);
			if (modifiedUpdates.Count > 0)
			{
				requirements += "\n\nLOCAL FILE WARNING:";
				foreach (string id in modifiedUpdates)
				{
					ModIntegrityCheck integrity = ModInstallIntegrity.Check(ModStoragePaths.GetModsDirectory(), id);
					requirements += "\n  " + id + ": " + integrity.Message;
				}
				requirements += "\nA separate modified copy will be preserved before updating.";
			}
			string warnings = catalogPack.ContentWarnings == null || catalogPack.ContentWarnings.Count == 0
				? "none" : string.Join(", ", catalogPack.ContentWarnings);
			string releaseInformation = "\n\nCONTENT WARNINGS: " + warnings
				+ "\nDOWNLOAD: " + (version == null ? "unavailable" : FormatBytes(version.SizeBytes))
				+ "\nSOURCE: " + catalogPack.SourceRepository;
			m_detailText.text = catalogPack.DisplayName + (version == null ? string.Empty : "  v" + version.Version) +
				"\nBY " + string.Join(", ", catalogPack.Authors) + (update ? "\nUPDATE AVAILABLE" : installed ? "\nINSTALLED" : "\nNOT INSTALLED") +
				requirements + "\n\n" + catalogPack.Summary + releaseInformation
				+ (version == null ? string.Empty : "\n\nGAME: " + version.GameVersion + "  MOD API: " + version.ModApiVersion)
				+ (m_installMessage.Length == 0 ? string.Empty : "\n\n" + m_installMessage);
			bool webGlTooLarge = version != null
				&& version.SizeBytes > ModCatalogPlatformPolicy.MaximumArchiveBytes(Application.platform);
			m_toggleButton.gameObject.SetActive(version != null && (!installed || update));
			m_rollbackButton.gameObject.SetActive(removed);
			if (removed)
			{
				m_rollbackButton.onClick.RemoveAllListeners();
				m_rollbackButton.onClick.AddListener(RestoreSelected);
				m_rollbackButton.interactable = !m_installBusy;
				SetButtonText(m_rollbackButton, "RESTORE REMOVED");
			}
			bool compatible = version != null && IsGameVersionCompatible(version)
				&& version.ModApiVersion == ModManifestParser.SupportedModApiVersion;
			m_toggleButton.interactable = !m_installBusy && !removed && !m_changedIds.Contains(catalogPack.Id) && !webGlTooLarge && compatible
				&& plan != null && plan.Report.IsValid;
			SetButtonText(m_toggleButton, m_installBusy ? "DOWNLOADING..." : webGlTooLarge
				? "TOO LARGE FOR WEBGL" : !compatible ? "INCOMPATIBLE VERSION"
				: plan != null && !plan.Report.IsValid ? "REQUIREMENTS BLOCKED"
				: modifiedUpdates.Count > 0 && m_confirmModifiedId == catalogPack.Id ? "CONFIRM UPDATE"
				: modifiedUpdates.Count > 0 ? "UPDATE AVAILABLE"
				: update ? "UPDATE (RESTART REQUIRED)" : "INSTALL (RESTART REQUIRED)");
			return;
		}
		if (m_selectedIndex < 0 || m_selectedIndex >= m_statuses.Count)
		{
			m_detailText.text = "No mod packs were discovered."; m_toggleButton.gameObject.SetActive(false);
			m_rollbackButton.gameObject.SetActive(false); m_uninstallButton.gameObject.SetActive(false);
			ClearPreview(); m_previewStatusIndex = -1; return;
		}
		ModPackStatus status = m_statuses[m_selectedIndex];
		if (m_removedIds.Contains(status.Id))
		{
			m_detailText.text = status.DisplayName + "\n" + status.Id + "  v" + status.Version
				+ "\n\nSaved outside Mods; it will not load after restart."
				+ (m_installMessage.Length == 0 ? string.Empty : "\n\n" + m_installMessage);
			m_toggleButton.gameObject.SetActive(false);
			m_uninstallButton.gameObject.SetActive(false);
			m_rollbackButton.gameObject.SetActive(true);
			m_rollbackButton.onClick.RemoveAllListeners();
			m_rollbackButton.onClick.AddListener(RestoreSelected);
			SetButtonText(m_rollbackButton, "RESTORE REMOVED");
			ClearPreview(); m_previewStatusIndex = -1;
			return;
		}
		if (m_previewStatusIndex != m_selectedIndex)
		{
			m_previewStatusIndex = m_selectedIndex;
			ShowPreview(0);
		}
		string details = status.DisplayName + "\n" + status.Id + (status.Version.Length == 0 ? "" : "  v" + status.Version) +
			"\n\n" + (string.IsNullOrWhiteSpace(status.Pack?.Manifest?.Description) ? "No description provided." : status.Pack.Manifest.Description);
		if (status.State == ModPackState.Invalid || status.State == ModPackState.Conflicting)
			details += "\n\n" + status.Reason;
		if (status.Pack?.Manifest != null)
		{
			details += "\n\nREQUIRED: " + DependencyList(status.Pack.Manifest.Dependencies) +
				"\nOPTIONAL: " + DependencyList(status.Pack.Manifest.OptionalDependencies);
			if (status.Pack.Manifest.Conflicts != null && status.Pack.Manifest.Conflicts.Count > 0) details += "\nCONFLICTS: " + string.Join(", ", status.Pack.Manifest.Conflicts);
		}
		if (m_pendingRestart) details += "\n\nCHANGES ARE SAVED AND WILL APPLY AFTER RESTART.";
		string backupVersion = ModInstallRecovery.BackupVersion(ModStoragePaths.GetModsDirectory(), status.Id);
		if (backupVersion != null) details += "\nPREVIOUS VERSION: v" + backupVersion;
		if (m_installMessage.Length > 0) details += "\n\n" + m_installMessage;
		m_detailText.text = details;
		m_toggleButton.gameObject.SetActive(status.IsUserConfigurable && !m_changedIds.Contains(status.Id));
		m_rollbackButton.gameObject.SetActive(backupVersion != null && !m_changedIds.Contains(status.Id));
		m_rollbackButton.onClick.RemoveAllListeners();
		m_rollbackButton.onClick.AddListener(RollbackSelected);
		SetButtonText(m_rollbackButton, "ROLL BACK");
		m_uninstallButton.gameObject.SetActive(status.Id != "core" && !m_changedIds.Contains(status.Id));
		SetButtonText(m_uninstallButton, m_confirmRemoveId == status.Id ? "CONFIRM REMOVE" : "UNINSTALL");
		if (status.IsUserConfigurable)
			SetButtonText(m_toggleButton, status.State == ModPackState.Conflicting ? "MAKE ACTIVE AFTER RESTART" :
				(ModEnableState.IsEnabled(status.Id) ? "DISABLE AFTER RESTART" : "ENABLE AFTER RESTART"));
	}

	private bool IsInstalled(string i_id)
	{
		if (m_removedIds.Contains(i_id)) return false;
		if (m_newInstallIds.Contains(i_id)) return true;
		if (m_statuses == null) return false;
		foreach (ModPackStatus status in m_statuses) if (status.Id == i_id) return true;
		return false;
	}
	private bool HasUpdate(ModCatalogPack i_pack, ModCatalogVersion i_version)
	{
		if (i_version == null || !SemanticVersion.TryParse(i_version.Version, out SemanticVersion offered)) return false;
		if (m_statuses == null) return false;
		foreach (ModPackStatus status in m_statuses)
			if (status.Id == i_pack.Id && SemanticVersion.TryParse(status.Version, out SemanticVersion installed))
				return offered.CompareTo(installed) > 0;
		return false;
	}
	private bool UpdateAvailable(ModCatalogPack i_pack)
	{
		return i_pack != null && !m_changedIds.Contains(i_pack.Id) && IsInstalled(i_pack.Id)
			&& HasUpdate(i_pack, LatestVersion(i_pack));
	}
	private static ModCatalogVersion LatestVersion(ModCatalogPack i_pack)
	{
		ModCatalogVersion latest = null; SemanticVersion latestVersion = default;
		ModCatalogVersion latestCompatible = null; SemanticVersion latestCompatibleVersion = default;
		foreach (ModCatalogVersion candidate in i_pack.Versions)
		{
			if (!SemanticVersion.TryParse(candidate.Version, out SemanticVersion parsed)) continue;
			if (latest == null || parsed.CompareTo(latestVersion) > 0) { latest = candidate; latestVersion = parsed; }
			if (candidate.ModApiVersion == ModManifestParser.SupportedModApiVersion
				&& IsGameVersionCompatible(candidate)
				&& candidate.SizeBytes <= ModCatalogPlatformPolicy.MaximumArchiveBytes(Application.platform)
				&& (latestCompatible == null || parsed.CompareTo(latestCompatibleVersion) > 0))
			{ latestCompatible = candidate; latestCompatibleVersion = parsed; }
		}
		return latestCompatible ?? latest;
	}

	private void ShowPreview(int i_index)
	{
		ClearPreview();
		m_nextPreviewTime = float.PositiveInfinity;
		if (m_previewImage == null || m_statuses == null || m_previewStatusIndex < 0 || m_previewStatusIndex >= m_statuses.Count) return;
		ModPack pack = m_statuses[m_previewStatusIndex].Pack;
		List<string> images = pack?.Manifest?.PreviewImages;
		if (images == null || images.Count == 0 || string.IsNullOrEmpty(pack.RootPath)) return;
		m_previewImageIndex = i_index % images.Count;
		if (images.Count > 1) m_nextPreviewTime = Time.unscaledTime + 3f;
		if (!RuntimePngAssetLoader.TryLoad(pack.RootPath, images[m_previewImageIndex], "Mod preview", FilterMode.Bilinear,
			null, "ui.preview-file", "ui.preview-decode", pack.RootPath, out m_previewTexture)) return;
		m_previewSprite = Sprite.Create(m_previewTexture, new Rect(0f, 0f, m_previewTexture.width, m_previewTexture.height), new Vector2(0.5f, 0.5f));
		m_previewImage.sprite = m_previewSprite;
		m_previewImage.enabled = true;
	}

	private void ShowCatalogPreview(ModCatalogPack i_pack, int i_index)
	{
		ClearPreview();
		m_previewStatusIndex = -1;
		m_previewCatalogId = i_pack?.Id;
		m_nextPreviewTime = float.PositiveInfinity;
		if (m_previewImage == null || i_pack?.PreviewImages == null || i_pack.PreviewImages.Count == 0) return;
		m_previewImageIndex = i_index % i_pack.PreviewImages.Count;
		int generation = m_previewGeneration;
		StartCoroutine(LoadCatalogPreview(i_pack.Id, i_pack.PreviewImages[m_previewImageIndex], generation,
			i_pack.PreviewImages.Count > 1));
	}

	private IEnumerator LoadCatalogPreview(string i_packId, string i_url, int i_generation, bool i_cycle)
	{
		using (UnityWebRequest request = UnityWebRequest.Get(i_url))
		{
			m_catalogPreviewRequest = request;
			request.timeout = 10;
			UnityWebRequestAsyncOperation operation = request.SendWebRequest();
			bool oversized = false;
			while (!operation.isDone)
			{
				if (request.downloadedBytes > (ulong)ModCatalogParser.MaxPreviewBytes)
				{
					oversized = true;
					request.Abort();
				}
				yield return null;
			}
			if (m_catalogPreviewRequest == request) m_catalogPreviewRequest = null;
			if (i_generation != m_previewGeneration || m_previewCatalogId != i_packId) yield break;
			if (oversized || request.result != UnityWebRequest.Result.Success)
			{
				ScheduleCatalogPreviewAdvance(i_packId, i_generation, i_cycle);
				yield break;
			}
			byte[] bytes = request.downloadHandler.data;
			if (bytes == null || bytes.Length < 8 || bytes.Length > ModCatalogParser.MaxPreviewBytes
				|| bytes[0] != 0x89 || bytes[1] != 0x50 || bytes[2] != 0x4e || bytes[3] != 0x47)
			{
				ScheduleCatalogPreviewAdvance(i_packId, i_generation, i_cycle);
				yield break;
			}
			Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
			if (!texture.LoadImage(bytes, false) || texture.width > RuntimePngAssetLoader.MaximumPngDimension
				|| texture.height > RuntimePngAssetLoader.MaximumPngDimension
				|| (long)texture.width * texture.height > RuntimePngAssetLoader.MaximumPngPixels)
			{
				Destroy(texture);
				ScheduleCatalogPreviewAdvance(i_packId, i_generation, i_cycle);
				yield break;
			}
			if (i_generation != m_previewGeneration || m_previewCatalogId != i_packId)
			{
				Destroy(texture);
				yield break;
			}
			texture.name = "Catalog preview " + i_packId;
			texture.filterMode = FilterMode.Bilinear;
			m_previewTexture = texture;
			m_previewSprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
			m_previewImage.sprite = m_previewSprite;
			m_previewImage.enabled = true;
			m_nextPreviewTime = i_cycle ? Time.unscaledTime + 3f : float.PositiveInfinity;
		}
	}

	private void ScheduleCatalogPreviewAdvance(string i_packId, int i_generation, bool i_cycle)
	{
		if (i_cycle && i_generation == m_previewGeneration && m_previewCatalogId == i_packId)
			m_nextPreviewTime = Time.unscaledTime + 3f;
	}

	private void ClearPreview()
	{
		if (m_catalogPreviewRequest != null)
		{
			m_catalogPreviewRequest.Abort();
			m_catalogPreviewRequest = null;
		}
		m_previewGeneration++;
		m_previewCatalogId = null;
		m_nextPreviewTime = float.PositiveInfinity;
		if (m_previewImage != null) { m_previewImage.sprite = null; m_previewImage.enabled = false; }
		if (m_previewSprite != null) Destroy(m_previewSprite);
		if (m_previewTexture != null) Destroy(m_previewTexture);
		m_previewSprite = null; m_previewTexture = null;
	}

	private static bool PacksConflict(ModPack i_left, ModPack i_right)
	{
		if (i_left?.Manifest == null || i_right?.Manifest == null) return false;
		return (i_left.Manifest.Conflicts != null && i_left.Manifest.Conflicts.Contains(i_right.Manifest.Id)) ||
			(i_right.Manifest.Conflicts != null && i_right.Manifest.Conflicts.Contains(i_left.Manifest.Id));
	}

	private void SelectFirstVisible()
	{
		for (int row = 0; row < m_rows.Count; row++) if (m_rows[row].gameObject.activeSelf) { SelectRow(row); return; }
		if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(m_closeButton.gameObject);
	}

	private static string DependencyList(List<ModDependency> i_dependencies)
	{
		if (i_dependencies == null || i_dependencies.Count == 0) return "none";
		List<string> values = new List<string>(); foreach (ModDependency dependency in i_dependencies) values.Add(dependency.Id + " " + dependency.Version); return string.Join(", ", values);
	}
	private static string FormatBytes(long i_bytes)
	{
		if (i_bytes < 1024L) return i_bytes + " B";
		if (i_bytes < 1024L * 1024L) return (i_bytes / 1024f).ToString("0.#") + " KiB";
		return (i_bytes / (1024f * 1024f)).ToString("0.##") + " MiB";
	}
	private static void SetStatusButtonColors(UIButton i_button, ModPackState i_state)
	{
		Color color = i_state == ModPackState.Loaded ? Color.green :
			i_state == ModPackState.Disabled ? new Color(0.58f, 0.15f, 0.18f, 1f) : new Color(0.85f, 0.69f, 0.18f, 1f);
		ColorBlock colors = i_button.colors;
		colors.normalColor = color;
		colors.highlightedColor = i_state == ModPackState.Loaded ? new Color(0f, 0.735849f, 0f, 1f) : Color.Lerp(color, Color.white, 0.2f);
		colors.selectedColor = i_state == ModPackState.Loaded ? color : Color.Lerp(color, Color.white, 0.3f);
		colors.pressedColor = i_state == ModPackState.Loaded ? new Color(0f, 0.5849056f, 0f, 1f) : Color.Lerp(color, Color.black, 0.2f);
		i_button.colors = colors;
		Text label = i_button.GetComponentInChildren<Text>(true);
		if (label != null) label.color = i_state == ModPackState.Invalid || i_state == ModPackState.Conflicting ? Color.black : Color.white;
	}
	private static void SetCatalogButtonColors(UIButton i_button, bool i_installed)
	{
		if (!i_installed) { SetNeutralButtonColors(i_button); Text neutral = i_button.GetComponentInChildren<Text>(true); if (neutral != null) neutral.color = Color.white; return; }
		ColorBlock colors = i_button.colors;
		colors.normalColor = Color.green; colors.highlightedColor = new Color(0f, 0.735849f, 0f, 1f);
		colors.selectedColor = Color.green; colors.pressedColor = new Color(0f, 0.5849056f, 0f, 1f);
		i_button.colors = colors; Text label = i_button.GetComponentInChildren<Text>(true); if (label != null) label.color = Color.white;
	}
	private static ScreenTitle FindSceneTitle() { foreach (ScreenTitle title in Resources.FindObjectsOfTypeAll<ScreenTitle>()) if (title.gameObject.scene.IsValid()) return title; return null; }

	private UIButton CreateButton(UIButton i_template, Transform i_parent, string i_name, string i_text, UnityEngine.Events.UnityAction i_action)
	{
		UIButton button = Instantiate(i_template, i_parent); button.name = i_name; button.onClick = new UIButton.ButtonClickedEvent(); button.onClick.AddListener(i_action); SetButtonText(button, i_text); return button;
	}
	private static void SetButtonText(UIButton i_button, string i_text) { Text text = i_button.GetComponentInChildren<Text>(true); if (text != null) text.text = i_text; }
	private static void SetButtonFontSize(UIButton i_button, int i_size) { Text text = i_button.GetComponentInChildren<Text>(true); if (text != null) { text.fontSize = i_size; text.resizeTextForBestFit = true; text.resizeTextMinSize = 16; text.resizeTextMaxSize = i_size; } }
	private static void SetPurpleButtonColors(UIButton i_button)
	{
		ColorBlock colors = i_button.colors;
		colors.normalColor = new Color(0.62f, 0.34f, 0.82f, 1f);
		colors.highlightedColor = new Color(0.76f, 0.5f, 0.94f, 1f);
		colors.selectedColor = new Color(0.72f, 0.44f, 0.92f, 1f);
		colors.pressedColor = new Color(0.45f, 0.2f, 0.65f, 1f);
		i_button.colors = colors;
	}
	private static void SetNeutralButtonColors(UIButton i_button)
	{
		ColorBlock colors = i_button.colors;
		colors.normalColor = new Color(0.78f, 0.78f, 0.82f, 1f);
		colors.highlightedColor = Color.white;
		colors.selectedColor = new Color(0.82f, 0.7f, 0.94f, 1f);
		colors.pressedColor = new Color(0.58f, 0.58f, 0.64f, 1f);
		i_button.colors = colors;
	}
	private Text CreateText(string i_name, Transform i_parent, string i_text, int i_size, TextAnchor i_anchor, Color i_color)
	{
		GameObject gameObject = CreateUiObject(i_name, i_parent); Text text = gameObject.AddComponent<Text>(); text.font = m_font; text.fontSize = i_size; text.alignment = i_anchor; text.color = i_color; text.text = i_text; return text;
	}
	private InputField CreateSearchInput(Transform i_parent)
	{
		GameObject root = CreateUiObject("CatalogSearch", i_parent);
		Image background = root.AddComponent<Image>();
		background.color = new Color(0.18f, 0.02f, 0.025f, 0.96f);
		InputField input = root.AddComponent<InputField>();
		input.targetGraphic = background;
		input.lineType = InputField.LineType.SingleLine;
		input.characterLimit = 80;
		Text value = CreateText("Text", root.transform, string.Empty, 19, TextAnchor.MiddleLeft, Color.white);
		Stretch(value.rectTransform);
		value.rectTransform.offsetMin = new Vector2(14f, 2f);
		value.rectTransform.offsetMax = new Vector2(-14f, -2f);
		value.supportRichText = false;
		Text placeholder = CreateText("Placeholder", root.transform, "SEARCH MODS...", 17, TextAnchor.MiddleLeft,
			new Color(0.68f, 0.68f, 0.72f, 1f));
		Stretch(placeholder.rectTransform);
		placeholder.rectTransform.offsetMin = new Vector2(14f, -2f);
		placeholder.rectTransform.offsetMax = new Vector2(-14f, 2f);
		placeholder.verticalOverflow = VerticalWrapMode.Overflow;
		placeholder.raycastTarget = false;
		input.textComponent = value;
		input.placeholder = placeholder;
		return input;
	}
	private static GameObject CreateUiObject(string i_name, Transform i_parent) { GameObject gameObject = new GameObject(i_name, typeof(RectTransform)); gameObject.layer = i_parent.gameObject.layer; gameObject.transform.SetParent(i_parent, false); return gameObject; }
	private static void Stretch(RectTransform i_rect, float i_inset = 0f) { i_rect.anchorMin = Vector2.zero; i_rect.anchorMax = Vector2.one; i_rect.offsetMin = new Vector2(i_inset, i_inset); i_rect.offsetMax = new Vector2(-i_inset, -i_inset); }
	private static void SetRect(RectTransform i_rect, float i_left, float i_bottom, float i_right, float i_top, float i_minX, float i_minY, float i_maxX, float i_maxY) { i_rect.anchorMin = new Vector2(i_minX, i_minY); i_rect.anchorMax = new Vector2(i_maxX, i_maxY); i_rect.offsetMin = new Vector2(i_left, i_bottom); i_rect.offsetMax = new Vector2(i_right, i_top); }
	private static void SetFixedRect(RectTransform i_rect, float i_x, float i_y, float i_width, float i_height, Vector2 i_anchor) { i_rect.anchorMin = i_anchor; i_rect.anchorMax = i_anchor; i_rect.pivot = i_anchor; i_rect.anchoredPosition = new Vector2(i_x, i_y); i_rect.sizeDelta = new Vector2(i_width, i_height); }
}

public static class ExternalFactoryRunner
{
	private static GameObject m_host;

	public static T GetOrAdd<T>() where T : Component
	{
		if (m_host == null)
		{
			m_host = new GameObject("[ModLoader] Runtime Content Builders");
			Object.DontDestroyOnLoad(m_host);
		}
		T component = m_host.GetComponent<T>();
		return component == null ? m_host.AddComponent<T>() : component;
	}
}
