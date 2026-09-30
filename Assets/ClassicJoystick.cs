using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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

	private void Start()
	{
		RefreshKenneyStyle();
		if (!isLeftJoystick || transform.parent == null || transform.parent.Find("Right Aim Joystick") != null) return;
		GameObject aimObject = Instantiate(gameObject, transform.parent);
		aimObject.name = "Right Aim Joystick";
		ClassicJoystick aimJoystick = aimObject.GetComponent<ClassicJoystick>();
		aimJoystick.isLeftJoystick = false;
		RectTransform aimRect = aimObject.GetComponent<RectTransform>();
		if (aimRect != null) aimRect.anchoredPosition = new Vector2(380f, -211f);
	}

	public void RefreshKenneyStyle()
	{
		InputGlyphLibrary.ApplyMobileLayout(transform as RectTransform, isLeftJoystick ? "Left Joystick Background" : "Right Aim Joystick");
		if (background != null)
		{
			Image backgroundImage = background.GetComponent<Image>();
			if (backgroundImage != null)
			{
				backgroundImage.sprite = isLeftJoystick && playerController != null && playerController.GetUseMobileDPad()
					? InputGlyphLibrary.GetMobileControlSprite("dpad")
					: InputGlyphLibrary.GetMobileControlSprite("joystick_circle_pad_a");
				backgroundImage.preserveAspect = true;
			}
		}
		if (handle != null)
		{
			Image handleImage = handle.GetComponent<Image>();
			if (handleImage != null)
			{
				handleImage.sprite = InputGlyphLibrary.GetMobileControlSprite("joystick_circle_nub_a");
				handleImage.preserveAspect = true;
				handleImage.enabled = !(isLeftJoystick && playerController != null && playerController.GetUseMobileDPad());
			}
		}
	}

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
			if (isLeftJoystick && playerController != null && playerController.GetUseMobileDPad())
			{
				inputVector = SnapToDPad(inputVector);
			}

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

		if (isLeftJoystick) playerController.SetLeftJoystick(direction);
		else playerController.SetRightJoystick(direction);
    }

	private static Vector2 SnapToDPad(Vector2 i_direction)
	{
		if (i_direction.sqrMagnitude < 0.04f) return Vector2.zero;
		return Mathf.Abs(i_direction.x) >= Mathf.Abs(i_direction.y)
			? new Vector2(Mathf.Sign(i_direction.x), 0f)
			: new Vector2(0f, Mathf.Sign(i_direction.y));
	}
}
