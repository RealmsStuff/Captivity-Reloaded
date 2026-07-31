using UnityEngine;
using UnityEngine.EventSystems;

public class ClassicJoystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    [Header("References")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private RectTransform background;
    [SerializeField] private RectTransform handle;

    [Header("Settings")]
    [SerializeField] private float dragRange = 100f;
    [SerializeField] private bool isLeftJoystick = true;

    private Vector2 inputVector;

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 position;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(background, eventData.position, eventData.pressEventCamera, out position))
        {
            // Normalize touch position based on background boundaries
            float width = background.rect.width / 2;
            float height = background.rect.height / 2;

            position.x = position.x / width;
            position.y = position.y / height;

            inputVector = new Vector2(position.x, position.y);
            inputVector = (inputVector.magnitude > 1.0f) ? inputVector.normalized : inputVector;

            // Update the physical position of the Joystick knob
            handle.anchoredPosition = new Vector2(
                inputVector.x * (background.rect.width / 2 * (dragRange / 100f)),
                inputVector.y * (background.rect.height / 2 * (dragRange / 100f))
            );

            // Pass the coordinates to PlayerController
            SendInputToController(inputVector);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Return joystick handle to center and reset input values on touch release
        inputVector = Vector2.zero;
        handle.anchoredPosition = Vector2.zero;
        SendInputToController(Vector2.zero);
    }

    private void SendInputToController(Vector2 direction)
    {
        if (playerController == null) return;

        playerController.SetLeftJoystick(direction);
    }
}