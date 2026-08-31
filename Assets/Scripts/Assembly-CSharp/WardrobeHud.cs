using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WardrobeHud : MonoBehaviour
{
	[SerializeField]
	private AudioClip m_audioOpen;

	[SerializeField]
	private AudioClip m_audioClose;

	[SerializeField]
	private AudioClip m_audioSelectClothing;

	[SerializeField]
	private AudioClip m_audioUnselectClothing;

	[SerializeField]
	private GameObject m_parent;

	[SerializeField]
	private GameObject m_tabDefault;

	[SerializeField]
	private GameObject m_tabMenuDefault;

	[SerializeField]
	private ClothingHudItem m_clothingItemDefault;

	private SkeletonPlayer m_skeletonShowcase;

	private Material m_showcaseMaterial;

	private List<Clothing> m_clothes = new List<Clothing>();

	private List<GameObject> m_tabs = new List<GameObject>();

	private List<GameObject> m_tabMenus = new List<GameObject>();

	private List<ClothingHudItem> m_clothingItems = new List<ClothingHudItem>();

	private List<ClothingHudItem> m_clothingItemsSelected = new List<ClothingHudItem>();

	private SkinColor m_skinColorSelected;

	private EyeColor m_eyeColorSelected;

	private RectTransform m_windowRect;

	private RectTransform m_windowContainerRect;

	private Vector2 m_windowContainerSize = new Vector2(-1f, -1f);

	private ClothingHudItem m_clothingItemLastClicked;

	private float m_clothingItemNextClickTime;

	private void LateUpdate()
	{
		if (IsShowing())
		{
			ConfigureWindow();
		}
		bool flag = CommonReferences.Instance.GetManagerInput().IsControllerLastUsed() && UnityEngine.InputSystem.Gamepad.current != null && UnityEngine.InputSystem.Gamepad.current.buttonEast.wasPressedThisFrame;
		if (IsShowing() && (Input.GetKeyDown(KeyCode.Escape) || flag))
		{
			Hide();
		}
	}

	private void BuildHudInterface()
	{
		m_tabDefault.SetActive(value: false);
		m_tabMenuDefault.SetActive(value: false);
		m_clothingItemDefault.gameObject.SetActive(value: false);
		string[] names = Enum.GetNames(typeof(ClothingCategory));
		ConfigureWindow();
		ConfigureHeader();
		ConfigureTabRow();
		ConfigureActionButtons();
		for (int i = 0; i < names.Length; i++)
		{
			string text = names[i];
			GameObject gameObject = UnityEngine.Object.Instantiate(m_tabDefault, m_tabDefault.transform.parent);
			gameObject.name = "tab" + text;
			gameObject.GetComponentInChildren<Text>().text = text;
			m_tabs.Add(gameObject);
			gameObject.SetActive(value: true);
			LayoutTab(gameObject, i, names.Length);
			WireTabButton(gameObject);
			GameObject gameObject2 = UnityEngine.Object.Instantiate(m_tabMenuDefault, m_tabMenuDefault.transform.parent);
			gameObject2.name = "tabMenu" + text;
			m_tabMenus.Add(gameObject2);
			gameObject2.SetActive(value: false);
		}
		RetrieveAllClothes();
		string text2 = ClothingCategory.Hair.ToString();
		foreach (Clothing cloth in m_clothes)
		{
			if (cloth.GetCatergoryClothing() != ClothingCategory.Hair)
			{
				text2 = cloth.GetCatergoryClothing().ToString();
				break;
			}
		}
		OpenTab(text2);
		CreateClothingItems();
		CreatePlayerShowcase();
		AddAlreadyEquippedClothes();
		ConfigureShowcaseRendering();
	}

	private void ConfigureWindow()
	{
		if (m_windowRect == null)
		{
			RectTransform[] componentsInChildren = m_parent.GetComponentsInChildren<RectTransform>(includeInactive: true);
			foreach (RectTransform rectTransform in componentsInChildren)
			{
				if (rectTransform.gameObject.name == "img_topBar")
				{
					m_windowRect = rectTransform.parent as RectTransform;
					m_windowContainerRect = (m_windowRect != null) ? (m_windowRect.parent as RectTransform) : null;
					break;
				}
			}
		}
		if (m_windowRect == null || m_windowContainerRect == null)
		{
			return;
		}
		Vector2 size = m_windowContainerRect.rect.size;
		if (m_windowContainerSize == size)
		{
			return;
		}
		float num = Mathf.Max(size.x - 64f, 1f) / 1280f;
		float num2 = Mathf.Max(size.y - 64f, 1f) / 720f;
		float num3 = Mathf.Min(1f, Mathf.Min(num, num2));
		m_windowRect.anchorMin = new Vector2(0.5f, 0.5f);
		m_windowRect.anchorMax = new Vector2(0.5f, 0.5f);
		m_windowRect.pivot = new Vector2(0.5f, 0.5f);
		m_windowRect.anchoredPosition = Vector2.zero;
		m_windowRect.sizeDelta = new Vector2(1280f, 720f);
		m_windowRect.localScale = new Vector3(num3, num3, 1f);
		m_windowContainerSize = size;
	}

	private void ConfigureHeader()
	{
		RectTransform[] componentsInChildren = m_parent.GetComponentsInChildren<RectTransform>(includeInactive: true);
		foreach (RectTransform rectTransform in componentsInChildren)
		{
			if (rectTransform.gameObject.name != "img_topBar")
			{
				continue;
			}
			rectTransform.anchorMin = new Vector2(0f, 1f);
			rectTransform.anchorMax = new Vector2(1f, 1f);
			rectTransform.pivot = new Vector2(0.5f, 0.5f);
			rectTransform.anchoredPosition = new Vector2(0f, -34f);
			rectTransform.sizeDelta = new Vector2(0f, 68f);
			rectTransform.localScale = Vector3.one;
			rectTransform.SetAsLastSibling();

			Text componentInChildren = rectTransform.GetComponentInChildren<Text>(includeInactive: true);
			if (componentInChildren != null)
			{
				RectTransform rectTransform2 = componentInChildren.rectTransform;
				rectTransform2.anchorMin = Vector2.zero;
				rectTransform2.anchorMax = Vector2.one;
				rectTransform2.pivot = new Vector2(0.5f, 0.5f);
				rectTransform2.offsetMin = new Vector2(16f, 0f);
				rectTransform2.offsetMax = new Vector2(-84f, 0f);
				rectTransform2.localScale = Vector3.one;
			}

			UnityEngine.UI.Button componentInChildren2 = rectTransform.GetComponentInChildren<UnityEngine.UI.Button>(includeInactive: true);
			if (componentInChildren2 != null)
			{
				RectTransform component = componentInChildren2.GetComponent<RectTransform>();
				component.anchorMin = new Vector2(1f, 0.5f);
				component.anchorMax = new Vector2(1f, 0.5f);
				component.pivot = new Vector2(0.5f, 0.5f);
				component.anchoredPosition = new Vector2(-34f, 0f);
				component.sizeDelta = new Vector2(68f, 68f);
				component.localScale = Vector3.one;
			}
			break;
		}
	}

	private void ConfigureTabRow()
	{
		RectTransform rectTransform = m_tabDefault.transform.parent as RectTransform;
		if (rectTransform == null)
		{
			return;
		}
		HorizontalLayoutGroup component = rectTransform.GetComponent<HorizontalLayoutGroup>();
		if (component != null)
		{
			component.enabled = false;
		}
		RectTransform rectTransform2 = rectTransform.parent as RectTransform;
		if (rectTransform2 != null)
		{
			rectTransform2.SetAsLastSibling();
		}
		rectTransform.anchorMin = new Vector2(0f, 1f);
		rectTransform.anchorMax = new Vector2(1f, 1f);
		rectTransform.pivot = new Vector2(0.5f, 1f);
		rectTransform.anchoredPosition = Vector2.zero;
		rectTransform.sizeDelta = new Vector2(0f, 48f);
	}

	private void ConfigureActionButtons()
	{
		UnityEngine.UI.Button[] componentsInChildren = m_parent.GetComponentsInChildren<UnityEngine.UI.Button>(includeInactive: true);
		foreach (UnityEngine.UI.Button button in componentsInChildren)
		{
			Text componentInChildren = button.GetComponentInChildren<Text>(includeInactive: true);
			if (componentInChildren != null && componentInChildren.text == "Cancel")
			{
				button.gameObject.SetActive(value: false);
				continue;
			}
			if (button.gameObject.name != "btn_ok")
			{
				continue;
			}
			RectTransform component = button.GetComponent<RectTransform>();
			if (component != null)
			{
				component.anchorMin = new Vector2(1f, 0f);
				component.anchorMax = new Vector2(1f, 0f);
				component.pivot = new Vector2(1f, 0f);
				component.anchoredPosition = new Vector2(-8f, 8f);
				component.sizeDelta = new Vector2(140f, 48f);
				component.localScale = Vector3.one;
				component.SetAsLastSibling();
			}
			button.onClick = new UnityEngine.UI.Button.ButtonClickedEvent();
			button.onClick.AddListener(AcceptAndClose);
			button.gameObject.SetActive(value: true);
		}
	}

	private void LayoutTab(GameObject i_tab, int i_index, int i_tabCount)
	{
		RectTransform component = i_tab.GetComponent<RectTransform>();
		if (component == null || i_tabCount <= 0)
		{
			return;
		}
		float num = (float)i_index / (float)i_tabCount;
		float num2 = (float)(i_index + 1) / (float)i_tabCount;
		component.anchorMin = new Vector2(num, 0f);
		component.anchorMax = new Vector2(num2, 1f);
		component.pivot = new Vector2(0.5f, 0.5f);
		component.anchoredPosition = Vector2.zero;
		component.sizeDelta = Vector2.zero;
	}

	private void WireTabButton(GameObject i_tab)
	{
		Text tabText = i_tab.GetComponentInChildren<Text>();
		UnityEngine.UI.Button tabButton = i_tab.GetComponentInChildren<UnityEngine.UI.Button>();
		if (tabText != null && tabButton != null)
		{
			tabButton.onClick.AddListener(delegate
			{
				OpenTab(tabText.text);
			});
		}
	}

	private void OpenTab(string i_categoryName)
	{
		GameObject tabMenu = GetTabMenu(i_categoryName);
		if (tabMenu == null)
		{
			return;
		}
		HideAllTabMenus();
		tabMenu.SetActive(value: true);
		foreach (GameObject tab in m_tabs)
		{
			Text tabText = tab.GetComponentInChildren<Text>();
			UnityEngine.UI.Image image = tab.GetComponentInChildren<UnityEngine.UI.Image>();
			if (tabText != null && image != null)
			{
				image.color = ((tabText.text == i_categoryName) ? new Color(0.65f, 1f, 0.65f, 1f) : Color.white);
			}
		}
	}

	private void RetrieveAllClothes()
	{
		m_clothes.AddRange(ManagerDB.GetUnlockedClothes());
	}

	private void CreateClothingItems()
	{
		foreach (Clothing item in m_clothes)
		{
			ClothingHudItem clothingHudItem = UnityEngine.Object.Instantiate(m_clothingItemDefault, m_clothingItemDefault.transform);
			clothingHudItem.SetClothing(item);
			string i_categoryName = item.GetCatergoryClothing().ToString();
			clothingHudItem.transform.SetParent(GetTabMenu(i_categoryName).transform);
			m_clothingItems.Add(clothingHudItem);
			WireClothingButton(clothingHudItem);
			clothingHudItem.gameObject.SetActive(value: true);
		}
	}

	private void WireClothingButton(ClothingHudItem i_clothingItem)
	{
		UnityEngine.UI.Button componentInChildren = i_clothingItem.GetComponentInChildren<UnityEngine.UI.Button>();
		if (componentInChildren == null)
		{
			return;
		}
		componentInChildren.onClick = new UnityEngine.UI.Button.ButtonClickedEvent();
		componentInChildren.onClick.AddListener(delegate
		{
			ClickClothingItem(i_clothingItem);
		});
	}

	private void CreatePlayerShowcase()
	{
		StageHub stageHub = (StageHub)CommonReferences.Instance.GetManagerStages().GetStageCurrent();
		m_skeletonShowcase = UnityEngine.Object.Instantiate(CommonReferences.Instance.GetPlayer().GetSkeletonPlayer(), stageHub.GetParentPlayerShowcaseWardrobe());
		m_skeletonShowcase.transform.localPosition = Vector3.zero;
		m_skeletonShowcase.UpdateClothesEquippedAlready();
		m_skeletonShowcase.RemoveAllClothing();
		m_skinColorSelected = CommonReferences.Instance.GetPlayer().GetSkeletonPlayer().GetSkinColor();
		m_skeletonShowcase.SetSkinColor(m_skinColorSelected);
		m_eyeColorSelected = CommonReferences.Instance.GetPlayer().GetSkeletonPlayer().GetEyeColor();
		m_skeletonShowcase.SetEyeColor(CommonReferences.Instance.GetPlayer().GetSkeletonPlayer().GetEyeColor());
		if (CommonReferences.Instance.GetPlayer().GetIsFacingLeft())
		{
			m_skeletonShowcase.transform.localScale = new Vector3(-1f, 1f, 1f);
		}
	}

	private void ConfigureShowcaseRendering()
	{
		if (m_skeletonShowcase == null)
		{
			return;
		}
		if (m_showcaseMaterial == null)
		{
			Shader shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
			if (shader == null)
			{
				shader = Shader.Find("Sprites/Default");
			}
			if (shader != null)
			{
				m_showcaseMaterial = new Material(shader);
				m_showcaseMaterial.name = "Wardrobe Preview Material";
			}
		}
		foreach (Transform transform in m_skeletonShowcase.GetComponentsInChildren<Transform>(includeInactive: true))
		{
			transform.gameObject.layer = 9;
		}
		if (m_showcaseMaterial != null)
		{
			foreach (SpriteRenderer spriteRenderer in m_skeletonShowcase.GetComponentsInChildren<SpriteRenderer>(includeInactive: true))
			{
				spriteRenderer.sharedMaterial = m_showcaseMaterial;
			}
		}
	}

	private void AddAlreadyEquippedClothes()
	{
		foreach (Clothing item in CommonReferences.Instance.GetPlayer().GetSkeletonPlayer().GetClothesEquipped())
		{
			SelectClothingItem(GetClothingItemAttachedToClothing(item));
		}
	}

	public void ClickClothingItem(ClothingHudItem i_clothingItemSelected)
	{
		if (i_clothingItemSelected == null || (m_clothingItemLastClicked == i_clothingItemSelected && Time.unscaledTime < m_clothingItemNextClickTime))
		{
			return;
		}
		m_clothingItemLastClicked = i_clothingItemSelected;
		m_clothingItemNextClickTime = Time.unscaledTime + 0.25f;
		if (m_clothingItemsSelected.Contains(i_clothingItemSelected))
		{
			UnSelectClothingItem(i_clothingItemSelected);
			CommonReferences.Instance.GetManagerAudio().PlayAudioSFX(m_audioUnselectClothing);
		}
		else
		{
			SelectClothingItem(i_clothingItemSelected);
			CommonReferences.Instance.GetManagerAudio().PlayAudioSFX(m_audioSelectClothing);
		}
	}

	private void SelectClothingItem(ClothingHudItem i_clothingItem)
	{
		m_clothingItemsSelected.Add(i_clothingItem);
		m_skeletonShowcase.EquipClothing(i_clothingItem.GetClothing());
		ConfigureShowcaseRendering();
		UnEquipIncompatibleClothes(i_clothingItem);
		i_clothingItem.SetIsSelected(i_isSelected: true);
	}

	private void UnSelectClothingItem(ClothingHudItem i_clothingItem)
	{
		m_skeletonShowcase.RemoveClothing(i_clothingItem.GetClothing());
		m_clothingItemsSelected.Remove(i_clothingItem);
		i_clothingItem.SetIsSelected(i_isSelected: false);
	}

	private void UnEquipIncompatibleClothes(ClothingHudItem i_clothingItemSelected)
	{
		List<ClothingHudItem> list = new List<ClothingHudItem>();
		foreach (ClothingHudItem item in m_clothingItemsSelected)
		{
			list.Add(item);
		}
		foreach (ClothingHudItem item2 in list)
		{
			if (item2.GetClothing().GetId() != i_clothingItemSelected.GetClothing().GetId() && !item2.GetClothing().IsCompatibleWithClothing(i_clothingItemSelected.GetClothing()))
			{
				UnSelectClothingItem(item2);
			}
		}
	}

	public void ClickTab(Text i_textButtonTabClicked)
	{
		OpenTab(i_textButtonTabClicked.text);
	}

	private void OpenTabMenu(GameObject i_tabMenu)
	{
		i_tabMenu.SetActive(value: true);
	}

	private GameObject GetTabMenu(string i_categoryName)
	{
		foreach (GameObject tabMenu in m_tabMenus)
		{
			if (tabMenu.name == "tabMenu" + i_categoryName)
			{
				return tabMenu;
			}
		}
		return null;
	}

	private void HideAllTabMenus()
	{
		foreach (GameObject tabMenu in m_tabMenus)
		{
			tabMenu.SetActive(value: false);
		}
	}

	private void ClearAllData()
	{
		m_clothingItemLastClicked = null;
		m_clothingItemNextClickTime = 0f;
		m_clothes.Clear();
		foreach (GameObject tab in m_tabs)
		{
			UnityEngine.Object.Destroy(tab);
		}
		m_tabs.Clear();
		foreach (GameObject tabMenu in m_tabMenus)
		{
			UnityEngine.Object.Destroy(tabMenu);
		}
		m_tabMenus.Clear();
		foreach (ClothingHudItem clothingItem in m_clothingItems)
		{
			UnityEngine.Object.Destroy(clothingItem.gameObject);
		}
		m_clothingItems.Clear();
		if ((bool)m_skeletonShowcase)
		{
			UnityEngine.Object.Destroy(m_skeletonShowcase.gameObject);
		}
		if ((bool)m_showcaseMaterial)
		{
			UnityEngine.Object.Destroy(m_showcaseMaterial);
			m_showcaseMaterial = null;
		}
		m_clothingItemsSelected.Clear();
	}

	public void AcceptAndClose()
	{
		CommonReferences.Instance.GetPlayer().GetSkeletonPlayer().RemoveAllClothing();
		foreach (Clothing item in GetClothesEquippedShowcase())
		{
			CommonReferences.Instance.GetPlayer().GetSkeletonPlayer().EquipClothing(item);
		}
		ManagerDB.EquipClothes(GetClothesEquippedShowcase());
		CommonReferences.Instance.GetPlayer().GetSkeletonPlayer().SetSkinColor(m_skinColorSelected);
		ManagerDB.SetSkinColor(m_skinColorSelected);
		CommonReferences.Instance.GetPlayer().GetSkeletonPlayer().SetEyeColor(m_eyeColorSelected);
		ManagerDB.SetEyeColor(m_eyeColorSelected);
		Hide();
	}

	private ClothingHudItem GetClothingItemAttachedToClothing(Clothing i_clothing)
	{
		foreach (ClothingHudItem clothingItem in m_clothingItems)
		{
			if (clothingItem.GetClothing().GetId() == i_clothing.GetId())
			{
				return clothingItem;
			}
		}
		return null;
	}

	private List<Clothing> GetClothesEquippedShowcase()
	{
		List<Clothing> list = new List<Clothing>();
		foreach (ClothingHudItem item in m_clothingItemsSelected)
		{
			list.Add(item.GetClothing());
		}
		return list;
	}

	public void SetSkinColor(Text i_textBtn)
	{
		switch (i_textBtn.text)
		{
		case "Pale":
			m_skeletonShowcase.SetSkinColor(SkinColor.Pale);
			m_skinColorSelected = SkinColor.Pale;
			break;
		case "White":
			m_skeletonShowcase.SetSkinColor(SkinColor.White);
			m_skinColorSelected = SkinColor.White;
			break;
		case "Tan":
			m_skeletonShowcase.SetSkinColor(SkinColor.Tan);
			m_skinColorSelected = SkinColor.Tan;
			break;
		case "Black":
			m_skeletonShowcase.SetSkinColor(SkinColor.Black);
			m_skinColorSelected = SkinColor.Black;
			break;
		}
	}

	public void SetEyeColor(Text i_textBtn)
	{
		switch (i_textBtn.text)
		{
		case "Blue":
			m_skeletonShowcase.SetEyeColor(EyeColor.Blue);
			m_eyeColorSelected = EyeColor.Blue;
			break;
		case "Brown":
			m_skeletonShowcase.SetEyeColor(EyeColor.Brown);
			m_eyeColorSelected = EyeColor.Brown;
			break;
		case "Green":
			m_skeletonShowcase.SetEyeColor(EyeColor.Green);
			m_eyeColorSelected = EyeColor.Green;
			break;
		case "Yellow":
			m_skeletonShowcase.SetEyeColor(EyeColor.Yellow);
			m_eyeColorSelected = EyeColor.Yellow;
			break;
		}
	}

	public void Show()
	{
		CommonReferences.Instance.GetPlayerController().SetIsForceIgnoreInput(i_isForceIgnoreInput: true);
		m_parent.SetActive(value: true);
		ClearAllData();
		BuildHudInterface();
		CommonReferences.Instance.GetManagerAudio().PlayAudioSFX(m_audioOpen);
	}

	public void Hide()
	{
		CommonReferences.Instance.GetPlayerController().SetIsForceIgnoreInput(i_isForceIgnoreInput: false);
		ClearAllData();
		m_parent.SetActive(value: false);
		CommonReferences.Instance.GetManagerAudio().PlayAudioSFX(m_audioClose);
	}

	public bool IsShowing()
	{
		return m_parent.activeSelf;
	}
}
