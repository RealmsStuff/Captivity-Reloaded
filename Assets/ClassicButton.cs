using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ClassicButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    // Expose intentionally occupies the old Struggle serialized slot.
    public enum MobileButtonType { Jump, Interact, Escape, Reload, Dash, Expose, Wave, NextWeapon, NextMedicine, SelfPleasure }

	private static bool s_cycleButtonsCreated;

    [Header("References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private MobileButtonType buttonType;

    private void Awake()
    {
        if (playerController == null)
        {
            playerController = FindObjectOfType<PlayerController>();
        }
    }

	private void Start()
	{
		RefreshKenneyStyle();
		if (buttonType == MobileButtonType.Reload && !s_cycleButtonsCreated)
		{
			s_cycleButtonsCreated = true;
			CreateCycleButton(MobileButtonType.NextWeapon, "Next Weapon", new Vector2(394f, 220f));
			CreateCycleButton(MobileButtonType.NextMedicine, "Next Medicine", new Vector2(612f, 220f));
			if (ExternalRuleProfileFactory.IsSelfPleasureEnabled())
				CreateCycleButton(MobileButtonType.SelfPleasure, "Pleasure", new Vector2(830f, 220f));
		}
	}

	public void RefreshKenneyStyle()
	{
		InputGlyphLibrary.ApplyMobileLayout(transform as RectTransform, buttonType.ToString());
		bool showBackground = InputGlyphLibrary.ShouldShowMobileButtonBackground(buttonType);
		Image background = GetComponent<Image>();
		if (background != null)
		{
			background.sprite = showBackground ? InputGlyphLibrary.GetMobileControlSprite("button_circle") : null;
			background.type = Image.Type.Simple;
			background.preserveAspect = showBackground;
			background.color = showBackground ? Color.white : new Color(1f, 1f, 1f, 0f);
		}
		Image icon = InputGlyphLibrary.GetOrCreateImage(transform, "KenneyActionIcon");
		icon.sprite = InputGlyphLibrary.GetMobileIcon(buttonType);
		RectTransform iconRect = icon.rectTransform;
		iconRect.anchorMin = iconRect.anchorMax = new Vector2(0.5f, 0.5f);
		iconRect.pivot = new Vector2(0.5f, 0.5f);
		iconRect.anchoredPosition = Vector2.zero;
		float iconSize = showBackground ? 92f : 112f;
		iconRect.sizeDelta = new Vector2(iconSize, iconSize);
		iconRect.localScale = buttonType == MobileButtonType.Dash ? new Vector3(-1f, 1f, 1f) : Vector3.one;
		Text label = GetComponentInChildren<Text>(true);
		if (label != null)
		{
			label.enabled = false;
		}
	}

	private void CreateCycleButton(MobileButtonType i_buttonType, string i_label, Vector2 i_position)
	{
		GameObject gameObject = Instantiate(base.gameObject, base.transform.parent);
		gameObject.name = i_label.Replace(" ", "");
		ClassicButton component = gameObject.GetComponent<ClassicButton>();
		component.buttonType = i_buttonType;
		component.playerController = playerController;
		RectTransform component2 = gameObject.GetComponent<RectTransform>();
		component2.anchoredPosition = i_position;
		component2.sizeDelta = new Vector2(180f, 120f);
		Text componentInChildren = gameObject.GetComponentInChildren<Text>();
		if (componentInChildren != null)
		{
			componentInChildren.text = i_label;
			componentInChildren.enabled = false;
		}
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		Image background = GetComponent<Image>();
		if (background != null && InputGlyphLibrary.ShouldShowMobileButtonBackground(buttonType))
			background.color = new Color(1f, 0.72f, 0.16f, 1f);
        if (playerController == null) return;

        switch (buttonType)
        {
            case MobileButtonType.Jump:
                playerController.TriggerMobileJump();
                break;
            case MobileButtonType.Interact:
                playerController.TriggerMobileInteract();
                break;
            case MobileButtonType.Escape:
                playerController.TriggerMobileEscape();
                break;
            case MobileButtonType.Reload:
                playerController.TriggerMobileReload();
                break;
            case MobileButtonType.Dash:
                playerController.TriggerMobileDash();
                break;
			case MobileButtonType.Expose:
				playerController.TriggerMobileExpose();
                break;
            case MobileButtonType.Wave:
                playerController.TriggerMobileWave();
                break;
			case MobileButtonType.NextWeapon:
				playerController.TriggerMobileNextWeapon();
				break;
			case MobileButtonType.NextMedicine:
				playerController.TriggerMobileNextMedicine();
				break;
			case MobileButtonType.SelfPleasure:
				playerController.TriggerMobileSelfPleasure();
				break;
        }
    }

	public void OnPointerUp(PointerEventData eventData)
	{
		Image background = GetComponent<Image>();
		if (background != null && InputGlyphLibrary.ShouldShowMobileButtonBackground(buttonType)) background.color = Color.white;
		if (playerController != null && buttonType == MobileButtonType.Expose) playerController.ReleaseMobileExpose();
	}
}
