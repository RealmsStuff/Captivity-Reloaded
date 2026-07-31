using UnityEngine;
using UnityEngine.EventSystems;

public class ClassicButton : MonoBehaviour, IPointerDownHandler
{
    // Added "Struggle" to the enum
    public enum MobileButtonType { Jump, Interact, Escape, Reload, Dash, Struggle, Wave }

    [Header("References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private MobileButtonType buttonType;

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
        }
    }
}