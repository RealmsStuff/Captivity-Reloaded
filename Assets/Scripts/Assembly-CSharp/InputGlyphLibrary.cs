using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public static class InputGlyphLibrary
{
	private static readonly Dictionary<string, Sprite> s_sprites = new Dictionary<string, Sprite>(StringComparer.Ordinal);
	private const string MobileStyle = "C";

	public static Sprite GetPromptSprite(InputButton i_button)
	{
		if (IsMobile()) return Load("InputGlyphs/Mobile/Icons/" + MobileIcon(i_button));
		ManagerInput input = CommonReferences.Instance == null ? null : CommonReferences.Instance.GetManagerInput();
		if (input == null || !input.IsControllerLastUsed()) return null;
		return Load("InputGlyphs/" + ControllerFamily() + "/" + ControllerKey(i_button));
	}

	public static Sprite GetControllerSprite(InputButton i_button)
	{
		return Load("InputGlyphs/" + ControllerFamily() + "/" + ControllerKey(i_button));
	}

	public static Sprite GetStruggleSprite()
	{
		if (IsMobile()) return Load("InputGlyphs/Mobile/Icons/icon_arrow_rotate");
		ManagerInput input = CommonReferences.Instance == null ? null : CommonReferences.Instance.GetManagerInput();
		return input != null && input.IsControllerLastUsed() ? Load("InputGlyphs/" + ControllerFamily() + "/stick_right") : null;
	}

	public static Sprite GetMobileControlSprite(string i_name)
	{
		return Load("InputGlyphs/Mobile/" + MobileStyle + "/" + i_name);
	}

	public static Sprite GetMobileIcon(ClassicButton.MobileButtonType i_type)
	{
		string name;
		switch (i_type)
		{
		case ClassicButton.MobileButtonType.Jump: name = "icon_jump"; break;
		case ClassicButton.MobileButtonType.Interact: name = "icon_hand"; break;
		case ClassicButton.MobileButtonType.Escape: name = "icon_menu"; break;
		case ClassicButton.MobileButtonType.Reload: name = "icon_arrow_rotate"; break;
		case ClassicButton.MobileButtonType.Dash: name = "icon_burst"; break;
		case ClassicButton.MobileButtonType.Expose: name = "icon_star"; break;
		case ClassicButton.MobileButtonType.Wave: name = "icon_play"; break;
		case ClassicButton.MobileButtonType.NextWeapon: name = "icon_pistol"; break;
		case ClassicButton.MobileButtonType.NextMedicine: name = "icon_plus"; break;
		case ClassicButton.MobileButtonType.SelfPleasure: name = "icon_fire"; break;
		default: name = "icon_hand"; break;
		}
		return Load("InputGlyphs/Mobile/Icons/" + name);
	}

	public static bool ShouldShowMobileButtonBackground(ClassicButton.MobileButtonType i_type)
	{
		return i_type != ClassicButton.MobileButtonType.Escape
			&& i_type != ClassicButton.MobileButtonType.Wave
			&& i_type != ClassicButton.MobileButtonType.NextWeapon
			&& i_type != ClassicButton.MobileButtonType.NextMedicine;
	}

	private static bool IsMobile()
	{
		if (CommonReferences.Instance == null) return false;
		PlayerController controller = CommonReferences.Instance.GetPlayerController();
		return controller != null && controller.GetIsMobileControlsEnabled();
	}

	private static string ControllerFamily()
	{
		Gamepad gamepad = Gamepad.current;
		string identity = gamepad == null ? string.Empty : (gamepad.displayName + " " + gamepad.layout + " " + gamepad.name).ToLowerInvariant();
		if (identity.Contains("dualshock") || identity.Contains("dualsense") || identity.Contains("playstation")) return "PlayStation";
		if (identity.Contains("switch") || identity.Contains("nintendo")) return "Switch";
		if (identity.Contains("steam deck") || identity.Contains("steamdeck")) return "SteamDeck";
		return "Xbox";
	}

	private static string ControllerKey(InputButton i_button)
	{
		switch (i_button)
		{
		case InputButton.MoveLeft:
		case InputButton.MoveRight: return "stick_left";
		case InputButton.Jump: return "south";
		case InputButton.Walk: return "rs";
		case InputButton.Crouch: return "east";
		case InputButton.Dash: return "ls";
		case InputButton.Fire: return "rt";
		case InputButton.Reload: return "west";
		case InputButton.Use: return "north";
		case InputButton.PickUp: return "dpad_left";
		case InputButton.EquipPrevious: return "rb";
		case InputButton.DropWeapon: return "dpad_right";
		case InputButton.DrugSelection: return "dpad_up";
		case InputButton.Expose: return "lt";
		case InputButton.SelfPleasure: return "view";
		default: return "south";
		}
	}

	private static string MobileIcon(InputButton i_button)
	{
		switch (i_button)
		{
		case InputButton.Jump: return "icon_jump";
		case InputButton.Fire: return "icon_target";
		case InputButton.Reload: return "icon_arrow_rotate";
		case InputButton.Dash: return "icon_burst";
		case InputButton.EquipPrevious: return "icon_pistol";
		case InputButton.DrugSelection: return "icon_plus";
		case InputButton.Expose: return "icon_star";
		case InputButton.SelfPleasure: return "icon_fire";
		default: return "icon_hand";
		}
	}

	private static Sprite Load(string i_path)
	{
		if (s_sprites.TryGetValue(i_path, out Sprite cached)) return cached;
		Texture2D texture = Resources.Load<Texture2D>(i_path);
		if (texture == null) return null;
		texture.filterMode = FilterMode.Bilinear;
		Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
		sprite.name = i_path;
		s_sprites[i_path] = sprite;
		return sprite;
	}

	public static Image GetOrCreateImage(Transform i_parent, string i_name)
	{
		Transform existing = i_parent.Find(i_name);
		if (existing != null) return existing.GetComponent<Image>();
		GameObject glyph = new GameObject(i_name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
		glyph.transform.SetParent(i_parent, false);
		Image image = glyph.GetComponent<Image>();
		image.preserveAspect = true;
		image.raycastTarget = false;
		return image;
	}

	public static void StyleMobileCanvas(Transform i_root)
	{
		if (i_root == null) return;
		RectTransform rootRect = i_root as RectTransform;
		if (rootRect != null)
		{
			rootRect.anchorMin = Vector2.zero;
			rootRect.anchorMax = Vector2.one;
			rootRect.offsetMin = Vector2.zero;
			rootRect.offsetMax = Vector2.zero;
		}
		foreach (Transform child in i_root.GetComponentsInChildren<Transform>(true))
		{
			ApplyMobileLayout(child as RectTransform, child.name);
			string iconName = null;
			switch (child.name)
			{
			case "Shoot": iconName = "icon_target"; break;
			case "Jump": iconName = "icon_jump"; break;
			case "Interact": iconName = "icon_hand"; break;
			case "Escape": iconName = "icon_menu"; break;
			case "Reload": iconName = "icon_arrow_rotate"; break;
			case "Dash": iconName = "icon_burst"; break;
			case "Struggle":
			case "Expose":
				iconName = "icon_star";
				child.name = "Expose";
				Text exposeLabel = child.GetComponentInChildren<Text>(true);
				if (exposeLabel != null) exposeLabel.text = "Expose";
				break;
			case "Wave": iconName = "icon_play"; break;
			case "NextWeapon": iconName = "icon_pistol"; break;
			case "NextMedicine": iconName = "icon_plus"; break;
			}
			if (iconName == null) continue;
			Image background = child.GetComponent<Image>();
			if (background == null) continue;
			Text label = child.GetComponentInChildren<Text>(true);
			if (label != null) label.enabled = false;
			ClassicButton button = child.GetComponent<ClassicButton>();
			bool showBackground = button == null || ShouldShowMobileButtonBackground(GetButtonType(child.name));
			background.sprite = showBackground ? GetMobileControlSprite("button_circle") : null;
			background.type = Image.Type.Simple;
			background.preserveAspect = showBackground;
			background.color = showBackground ? Color.white : new Color(1f, 1f, 1f, 0f);
			Image icon = GetOrCreateImage(child, "KenneyActionIcon");
			icon.sprite = Load("InputGlyphs/Mobile/Icons/" + iconName);
			RectTransform iconRect = icon.rectTransform;
			iconRect.anchorMin = iconRect.anchorMax = new Vector2(0.5f, 0.5f);
			iconRect.pivot = new Vector2(0.5f, 0.5f);
			iconRect.anchoredPosition = Vector2.zero;
			float iconSize = showBackground ? 92f : 112f;
			iconRect.sizeDelta = new Vector2(iconSize, iconSize);
			iconRect.localScale = child.name == "Dash" ? new Vector3(-1f, 1f, 1f) : Vector3.one;
		}
	}

	private static ClassicButton.MobileButtonType GetButtonType(string i_name)
	{
		ClassicButton.MobileButtonType type;
		return Enum.TryParse(i_name, true, out type) ? type : ClassicButton.MobileButtonType.Interact;
	}

	public static void ApplyMobileLayout(RectTransform i_rect, string i_name)
	{
		if (i_rect == null) return;
		Vector2 anchor;
		Vector2 size;
		switch (i_name)
		{
		case "Escape": anchor = new Vector2(0.084f, 0.89f); size = new Vector2(150f, 100f); break;
		case "Wave": anchor = new Vector2(0.59f, 0.90f); size = new Vector2(120f, 100f); break;
		case "Expose":
		case "Struggle": anchor = new Vector2(0.835f, 0.83f); size = new Vector2(160f, 130f); break;
		case "Reload": anchor = new Vector2(0.907f, 0.65f); size = new Vector2(160f, 130f); break;
		case "Interact": anchor = new Vector2(0.804f, 0.52f); size = new Vector2(180f, 150f); break;
		case "Dash": anchor = new Vector2(0.666f, 0.445f); size = new Vector2(180f, 145f); break;
		case "Jump": anchor = new Vector2(0.616f, 0.295f); size = new Vector2(180f, 150f); break;
		case "Shoot": anchor = new Vector2(0.667f, 0.60f); size = new Vector2(180f, 150f); break;
		case "NextWeapon": anchor = new Vector2(0.378f, 0.14f); size = new Vector2(150f, 120f); break;
		case "NextMedicine": anchor = new Vector2(0.51f, 0.14f); size = new Vector2(150f, 120f); break;
		case "Pleasure":
		case "SelfPleasure": anchor = new Vector2(0.72f, 0.83f); size = new Vector2(150f, 120f); break;
		case "Left Joystick Background": anchor = new Vector2(0.187f, 0.295f); size = new Vector2(180f, 180f); break;
		case "Right Aim Joystick": anchor = new Vector2(0.831f, 0.22f); size = new Vector2(180f, 180f); break;
		default: return;
		}
		i_rect.anchorMin = anchor;
		i_rect.anchorMax = anchor;
		i_rect.pivot = new Vector2(0.5f, 0.5f);
		i_rect.anchoredPosition = Vector2.zero;
		i_rect.sizeDelta = size;
	}

	public static void RefreshMobileControls()
	{
		foreach (DisableUIIfNotMobile canvas in Resources.FindObjectsOfTypeAll<DisableUIIfNotMobile>())
			if (canvas != null && canvas.gameObject.scene.IsValid()) StyleMobileCanvas(canvas.transform);
		foreach (ClassicButton button in Resources.FindObjectsOfTypeAll<ClassicButton>())
			if (button != null && button.gameObject.scene.IsValid()) button.RefreshKenneyStyle();
		foreach (ClassicJoystick joystick in Resources.FindObjectsOfTypeAll<ClassicJoystick>())
			if (joystick != null && joystick.gameObject.scene.IsValid()) joystick.RefreshKenneyStyle();
	}
}
