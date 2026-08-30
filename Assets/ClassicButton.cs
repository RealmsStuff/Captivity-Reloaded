using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ClassicButton : MonoBehaviour, IPointerDownHandler
{
    // Added "Struggle" to the enum
    public enum MobileButtonType { Jump, Interact, Escape, Reload, Dash, Struggle, Wave, NextWeapon, NextMedicine }

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
		if (buttonType == MobileButtonType.Reload && !s_cycleButtonsCreated)
		{
			s_cycleButtonsCreated = true;
			CreateCycleButton(MobileButtonType.NextWeapon, "Next Weapon", new Vector2(394f, -142f));
			CreateCycleButton(MobileButtonType.NextMedicine, "Next Medicine", new Vector2(394f, -314f));
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
			componentInChildren.resizeTextForBestFit = true;
			componentInChildren.resizeTextMinSize = 12;
			componentInChildren.resizeTextMaxSize = 28;
		}
	}

    public void OnPointerDown(PointerEventData eventData)
    {
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
            case MobileButtonType.Struggle:
                playerController.TriggerMobileStruggle();
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
        }
    }
}
