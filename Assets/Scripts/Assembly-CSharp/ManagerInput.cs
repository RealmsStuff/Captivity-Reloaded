using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DefaultExecutionOrder(-1000)]
public class ManagerInput : MonoBehaviour
{
	[SerializeField]
	private KeyCode m_keyDefaultLeft;

	[SerializeField]
	private KeyCode m_keyDefaultRight;

	[SerializeField]
	private KeyCode m_keyDefaultJump;

	[SerializeField]
	private KeyCode m_keyDefaultWalk;

	[SerializeField]
	private KeyCode m_keyDefaultCrouch;

	[SerializeField]
	private KeyCode m_keyDefaultDash;

	[SerializeField]
	private KeyCode m_keyDefaultFire;

	[SerializeField]
	private KeyCode m_keyDefaultReload;

	[SerializeField]
	private KeyCode m_keyDefaultUse;

	[SerializeField]
	private KeyCode m_keyDefaultPickUp;

	[SerializeField]
	private KeyCode m_keyDefaultEquipPrevious;

	[SerializeField]
	private KeyCode m_keyDefaultDropWeapon;

	[SerializeField]
	private KeyCode m_keyDefaultDrugSelection;

	[SerializeField]
	private KeyCode m_keyDefaultExpose;

	private List<InputButtonXGame> m_buttonsDefault = new List<InputButtonXGame>();

	private List<InputButtonXGame> m_buttons = new List<InputButtonXGame>();

	private Vector2 m_lastControllerAim = Vector2.right;

	private bool m_isControllerLastUsed;

	private float m_aimAssistStrength;

	private int m_aimAssistFrame = -1;

	private Vector3 m_aimAssistTarget;

	private bool m_hasAimAssistTarget;

	private bool m_isControllerInputActiveThisFrame;

	private int m_ignoreMouseMovementFrames;

	private bool m_isControllerUIMode;

	private Vector2 m_lastUINavigation;

	private float m_nextUISubmitTime;

	private bool m_isUISubmitReady = true;

	private bool m_isUISubmitPending;

	private GameObject m_controllerSubmitTarget;

	private float m_nextUINavigationTime;

	private Outline m_controllerSelectionOutline;

	private GameObject m_controllerSelectionMarker;

	private Selectable m_controllerSelection;

	private Dropdown m_controllerOpenDropdown;

	private int m_controllerDropdownIndex;

	private void Update()
	{
		m_isControllerInputActiveThisFrame = false;
		if (m_ignoreMouseMovementFrames > 0)
		{
			m_ignoreMouseMovementFrames--;
		}
		Gamepad gamepad = Gamepad.current;
		if (gamepad != null)
		{
			Vector2 vector = gamepad.rightStick.ReadValue();
			if (vector.sqrMagnitude <= 0.04f)
			{
				vector = gamepad.leftStick.ReadValue();
			}
			if (vector.sqrMagnitude > 0.04f)
			{
				m_lastControllerAim = vector.normalized;
				m_isControllerInputActiveThisFrame = true;
			}
			if (gamepad.leftStick.ReadValue().sqrMagnitude > 0.04f || gamepad.dpad.ReadValue().sqrMagnitude > 0.04f || gamepad.buttonSouth.isPressed || gamepad.buttonSouth.wasReleasedThisFrame || gamepad.buttonEast.isPressed || gamepad.buttonEast.wasReleasedThisFrame || gamepad.buttonWest.isPressed || gamepad.buttonWest.wasReleasedThisFrame || gamepad.buttonNorth.isPressed || gamepad.buttonNorth.wasReleasedThisFrame || gamepad.leftShoulder.isPressed || gamepad.rightShoulder.isPressed || gamepad.leftStickButton.isPressed || gamepad.rightStickButton.isPressed || gamepad.leftTrigger.isPressed || gamepad.rightTrigger.isPressed || gamepad.startButton.isPressed || gamepad.selectButton.isPressed)
			{
				m_isControllerInputActiveThisFrame = true;
			}
			if (m_isControllerInputActiveThisFrame)
			{
				m_isControllerLastUsed = true;
			}
		}
		Mouse mouse = Mouse.current;
		bool flag = mouse != null && (mouse.leftButton.wasPressedThisFrame || mouse.rightButton.wasPressedThisFrame || (m_ignoreMouseMovementFrames <= 0 && mouse.delta.ReadValue().sqrMagnitude > 0.01f));
		if (flag)
		{
			m_isControllerLastUsed = false;
			m_controllerOpenDropdown = null;
			ClearControllerSelection();
		}
		Keyboard keyboard = Keyboard.current;
		if (keyboard != null && !m_isControllerInputActiveThisFrame && m_ignoreMouseMovementFrames <= 0 && keyboard.anyKey.wasPressedThisFrame)
		{
			m_isControllerLastUsed = false;
			m_controllerOpenDropdown = null;
			ClearControllerSelection();
		}
		UpdateUIInputModuleMode();
		UpdateControllerMouseAim();
		UpdateControllerUISelection(gamepad);
	}

	private void Awake()
	{
		m_aimAssistStrength = PlayerPrefs.HasKey("ControllerAimAssist") ? PlayerPrefs.GetFloat("ControllerAimAssist") : 0.35f;
		m_buttonsDefault.Add(new InputButtonXGame(InputButton.MoveLeft, m_keyDefaultLeft));
		m_buttonsDefault.Add(new InputButtonXGame(InputButton.MoveRight, m_keyDefaultRight));
		m_buttonsDefault.Add(new InputButtonXGame(InputButton.Jump, m_keyDefaultJump));
		m_buttonsDefault.Add(new InputButtonXGame(InputButton.Walk, m_keyDefaultWalk));
		m_buttonsDefault.Add(new InputButtonXGame(InputButton.Crouch, m_keyDefaultCrouch));
		m_buttonsDefault.Add(new InputButtonXGame(InputButton.Dash, m_keyDefaultDash));
		m_buttonsDefault.Add(new InputButtonXGame(InputButton.Fire, m_keyDefaultFire));
		m_buttonsDefault.Add(new InputButtonXGame(InputButton.Reload, m_keyDefaultReload));
		m_buttonsDefault.Add(new InputButtonXGame(InputButton.Use, m_keyDefaultUse));
		m_buttonsDefault.Add(new InputButtonXGame(InputButton.PickUp, m_keyDefaultPickUp));
		m_buttonsDefault.Add(new InputButtonXGame(InputButton.EquipPrevious, m_keyDefaultEquipPrevious));
		m_buttonsDefault.Add(new InputButtonXGame(InputButton.DropWeapon, m_keyDefaultDropWeapon));
		m_buttonsDefault.Add(new InputButtonXGame(InputButton.DrugSelection, m_keyDefaultDrugSelection));
		m_buttonsDefault.Add(new InputButtonXGame(InputButton.Expose, m_keyDefaultExpose));
	}

	public bool IsButton(InputButton i_inputButton)
	{
		if (IsControllerButton(i_inputButton, ButtonPhase.Held))
		{
			return true;
		}
		for (int i = 0; i < m_buttons.Count; i++)
		{
			if (m_buttons[i].GetInputButton() == i_inputButton)
			{
				if (Input.GetKey(m_buttons[i].GetKeyCode()))
				{
					return true;
				}
				return false;
			}
		}
		return false;
	}

	public bool IsButtonDown(InputButton i_inputButton)
	{
		if (IsControllerButton(i_inputButton, ButtonPhase.Down))
		{
			return true;
		}
		for (int i = 0; i < m_buttons.Count; i++)
		{
			if (m_buttons[i].GetInputButton() == i_inputButton)
			{
				if (Input.GetKeyDown(m_buttons[i].GetKeyCode()))
				{
					return true;
				}
				return false;
			}
		}
		return false;
	}

	public bool IsButtonUp(InputButton i_inputButton)
	{
		if (IsControllerButton(i_inputButton, ButtonPhase.Up))
		{
			return true;
		}
		for (int i = 0; i < m_buttons.Count; i++)
		{
			if (m_buttons[i].GetInputButton() == i_inputButton)
			{
				if (Input.GetKeyUp(m_buttons[i].GetKeyCode()))
				{
					return true;
				}
				return false;
			}
		}
		return false;
	}

	public List<KeyCode> GetAnyKey()
	{
		List<KeyCode> list = new List<KeyCode>();
		foreach (KeyCode value in Enum.GetValues(typeof(KeyCode)))
		{
			if (Input.GetKey(value))
			{
				list.Add(value);
			}
		}
		return list;
	}

	public KeyCode GetKeyAssignedToButton(InputButton i_inputButton)
	{
		for (int i = 0; i < m_buttons.Count; i++)
		{
			if (m_buttons[i].GetInputButton() == i_inputButton)
			{
				return m_buttons[i].GetKeyCode();
			}
		}
		return KeyCode.Alpha0;
	}

	public List<InputButtonXGame> GetButtonsDefault()
	{
		return m_buttonsDefault;
	}

	public void SetButtonsToDefault()
	{
		m_buttons.Clear();
		for (int i = 0; i < m_buttonsDefault.Count; i++)
		{
			m_buttons.Add(m_buttonsDefault[i]);
		}
	}

	public void SetButtonsToSavedButtons()
	{
		m_buttons.Clear();
		List<InputButtonXGame> inputButtons = ManagerDB.GetInputButtons();
		for (int i = 0; i < inputButtons.Count; i++)
		{
			m_buttons.Add(inputButtons[i]);
		}
	}

	public List<InputButtonXGame> GetButtons()
	{
		return m_buttons;
	}

	public bool IsPausePressed()
	{
		return Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame;
	}

	public bool IsControllerAiming()
	{
		Gamepad gamepad = Gamepad.current;
		return gamepad != null && (m_isControllerLastUsed || gamepad.leftStick.ReadValue().sqrMagnitude > 0.04f || gamepad.rightStick.ReadValue().sqrMagnitude > 0.04f || gamepad.buttonSouth.wasPressedThisFrame || gamepad.buttonEast.wasPressedThisFrame || gamepad.buttonWest.wasPressedThisFrame || gamepad.buttonNorth.wasPressedThisFrame || gamepad.leftShoulder.wasPressedThisFrame || gamepad.rightShoulder.wasPressedThisFrame || gamepad.leftTrigger.wasPressedThisFrame || gamepad.rightTrigger.wasPressedThisFrame);
	}

	public bool IsControllerLastUsed()
	{
		return m_isControllerLastUsed && Gamepad.current != null;
	}

	public bool IsControllerButtonDown(InputButton i_inputButton)
	{
		return IsControllerButton(i_inputButton, ButtonPhase.Down);
	}

	public bool IsControllerPreviousWeaponPressed()
	{
		return Gamepad.current != null && Gamepad.current.leftShoulder.wasPressedThisFrame;
	}

	public bool IsControllerPreviousDrugPressed()
	{
		return Gamepad.current != null && Gamepad.current.dpad.down.wasPressedThisFrame;
	}

	public void BlockControllerSubmitUntilRelease()
	{
		m_isUISubmitReady = false;
		m_isUISubmitPending = false;
		m_controllerSubmitTarget = null;
		m_nextUISubmitTime = Time.unscaledTime + 0.3f;
	}

	public Vector3 GetAimScreenPosition(Vector3 i_worldOrigin)
	{
		if (!IsControllerAiming() || Camera.main == null)
		{
			return Input.mousePosition;
		}
		Vector3 vector = Camera.main.WorldToScreenPoint(i_worldOrigin) + (Vector3)(m_lastControllerAim * 500f);
		return ApplyAimAssist(vector);
	}

	public Vector3 GetAimWorldPosition(Vector3 i_worldOrigin)
	{
		if (!IsControllerAiming())
		{
			return CommonReferences.Instance.GetManagerCamerasXGame().GetCameraXGameCurrent().GetCameraUnity()
				.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10f));
		}
		Camera camera = CommonReferences.Instance.GetManagerCamerasXGame().GetCameraXGameCurrent().GetCameraUnity();
		Vector3 vector = GetAimScreenPosition(i_worldOrigin);
		return camera.ScreenToWorldPoint(new Vector3(vector.x, vector.y, 10f));
	}

	public void SetAimAssistStrength(float i_strength)
	{
		m_aimAssistStrength = Mathf.Clamp01(i_strength);
	}

	private Vector3 ApplyAimAssist(Vector3 i_aimScreenPosition)
	{
		if (m_aimAssistStrength <= 0f || Camera.main == null)
		{
			return i_aimScreenPosition;
		}
		if (m_aimAssistFrame != Time.frameCount)
		{
			m_aimAssistFrame = Time.frameCount;
			m_hasAimAssistTarget = false;
			float num = Mathf.Lerp(40f, 260f, m_aimAssistStrength);
			NPC[] array = UnityEngine.Object.FindObjectsOfType<NPC>();
			for (int i = 0; i < array.Length; i++)
			{
				NPC nPC = array[i];
				if (!nPC.gameObject.activeInHierarchy || nPC.IsDead() || !nPC.GetIsCanBeAttacked())
				{
					continue;
				}
				Vector3 vector = Camera.main.WorldToScreenPoint(GetNPCAimPoint(nPC));
				if (vector.z <= 0f)
				{
					continue;
				}
				float num2 = Vector2.Distance(i_aimScreenPosition, vector);
				if (num2 < num)
				{
					num = num2;
					m_aimAssistTarget = vector;
					m_hasAimAssistTarget = true;
				}
			}
		}
		return m_hasAimAssistTarget ? Vector3.Lerp(i_aimScreenPosition, m_aimAssistTarget, m_aimAssistStrength) : i_aimScreenPosition;
	}

	private static Vector3 GetNPCAimPoint(NPC i_npc)
	{
		if (i_npc is Zombie)
		{
			SkeletonActor skeletonActor = i_npc.GetSkeletonActor();
			Bone bone = skeletonActor != null ? skeletonActor.GetBone("head") : null;
			if (bone != null)
			{
				BodyPart bodyPart = bone.GetBodyPart();
				Collider2D collider2D2 = bodyPart != null ? bodyPart.GetCollider() : null;
				return collider2D2 != null ? collider2D2.bounds.center : bone.transform.position;
			}
		}
		Collider2D[] componentsInChildren = i_npc.GetComponentsInChildren<Collider2D>();
		bool flag = false;
		Bounds bounds = default(Bounds);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Collider2D collider2D = componentsInChildren[i];
			if (!collider2D.enabled || collider2D.isTrigger || !collider2D.gameObject.activeInHierarchy)
			{
				continue;
			}
			if (!flag)
			{
				bounds = collider2D.bounds;
				flag = true;
			}
			else
			{
				bounds.Encapsulate(collider2D.bounds);
			}
		}
		return flag ? bounds.center : i_npc.transform.position;
	}

	private void UpdateControllerMouseAim()
	{
		ScreenGame screenGame = CommonReferences.Instance.GetManagerScreens().GetScreenGame();
		bool flag = m_isControllerInputActiveThisFrame && IsControllerAiming() && screenGame.gameObject.activeInHierarchy && !screenGame.IsPaused() && !IsMenuOpen();
		if (!flag)
		{
			return;
		}
		Player player = CommonReferences.Instance.GetPlayer();
		if (player == null)
		{
			return;
		}
		Mouse mouse = Mouse.current;
		if (mouse == null)
		{
			return;
		}
		Vector3 aimScreenPosition = GetAimScreenPosition(player.transform.position + Vector3.up * 0.5f);
		// Keep the real cursor on-screen so controller aim and pointer feedback agree.
		Vector2 vector = new Vector2(Mathf.Clamp(aimScreenPosition.x, 0f, UnityEngine.Screen.width), Mathf.Clamp(aimScreenPosition.y, 0f, UnityEngine.Screen.height));
		mouse.WarpCursorPosition(vector);
		// The Input System reports cursor warps as mouse delta on following frames.
		// Ignore that synthetic motion so prompts do not flicker back to keyboard bindings.
		m_ignoreMouseMovementFrames = 3;
	}

	private void UpdateControllerUISelection(Gamepad i_gamepad)
	{
		if (i_gamepad == null || !IsControllerLastUsed() || !IsMenuOpen())
		{
			return;
		}
		EventSystem current = EventSystem.current;
		if (current == null)
		{
			return;
		}
		if (HandleControllerDropdown(i_gamepad, current))
		{
			return;
		}
		GameObject currentSelectedGameObject = current.currentSelectedGameObject;
		Selectable selectable = currentSelectedGameObject != null ? currentSelectedGameObject.GetComponent<Selectable>() : null;
		if (selectable == null || !selectable.isActiveAndEnabled || !selectable.gameObject.activeInHierarchy || !selectable.IsInteractable())
		{
			selectable = FindBestMenuSelectable();
			if (selectable == null)
			{
				return;
			}
			current.SetSelectedGameObject(selectable.gameObject);
		}
		Vector2 vector2 = i_gamepad.dpad.ReadValue();
		bool flag3 = vector2.sqrMagnitude >= 0.25f;
		Vector2 vector = flag3 ? vector2 : i_gamepad.leftStick.ReadValue();
		float num = flag3 ? 0.6f : 0.8f;
		bool flag = Mathf.Abs(vector.x) > num || Mathf.Abs(vector.y) > num;
		bool flag4 = Mathf.Abs(vector.x) < 0.2f && Mathf.Abs(vector.y) < 0.2f;
		if (flag4)
		{
			m_lastUINavigation = Vector2.zero;
		}
		bool flag2 = flag && m_lastUINavigation.sqrMagnitude < 0.25f && Time.unscaledTime >= m_nextUINavigationTime;
		if (flag2)
		{
			m_nextUINavigationTime = Time.unscaledTime + 0.25f;
			Selectable selectable2 = null;
			if (Mathf.Abs(vector.x) > Mathf.Abs(vector.y))
			{
				Slider slider = selectable as Slider;
				if (slider != null)
				{
					slider.value += Mathf.Sign(vector.x) * Mathf.Max(1f, (slider.maxValue - slider.minValue) / 10f);
				}
				else
				{
					selectable2 = FindSelectableInDirection(selectable, vector.x > 0f ? Vector2.right : Vector2.left);
				}
			}
			else
			{
				selectable2 = FindSelectableInDirection(selectable, vector.y > 0f ? Vector2.up : Vector2.down);
			}
			if (selectable2 != null && selectable2.isActiveAndEnabled && selectable2.gameObject.activeInHierarchy && selectable2.IsInteractable())
			{
				current.SetSelectedGameObject(selectable2.gameObject);
				selectable = selectable2;
			}
		}
		if (flag)
		{
			m_lastUINavigation = vector;
		}
		if (m_isUISubmitReady && i_gamepad.buttonSouth.wasPressedThisFrame && Time.unscaledTime >= m_nextUISubmitTime)
		{
			m_isUISubmitReady = false;
			m_isUISubmitPending = true;
			m_controllerSubmitTarget = selectable.gameObject;
		}
		if (m_isUISubmitPending && i_gamepad.buttonSouth.wasReleasedThisFrame)
		{
			GameObject gameObject = m_controllerSubmitTarget;
			m_isUISubmitPending = false;
			m_controllerSubmitTarget = null;
			m_nextUISubmitTime = Time.unscaledTime;
			if (gameObject != null && gameObject.activeInHierarchy)
			{
				SimulateControllerClick(current, gameObject);
			}
			m_isUISubmitReady = true;
		}
		else if (!m_isUISubmitPending && !m_isUISubmitReady && !i_gamepad.buttonSouth.isPressed && Time.unscaledTime >= m_nextUISubmitTime)
		{
			m_isUISubmitReady = true;
		}
		if (i_gamepad.buttonEast.wasPressedThisFrame)
		{
			KeypadHud keypadHud = CommonReferences.Instance.GetManagerHud().GetKeypadHud();
			if (keypadHud != null && keypadHud.IsShowing())
			{
				keypadHud.Close();
			}
			else
			{
				ExecuteEvents.Execute(selectable.gameObject, new BaseEventData(current), ExecuteEvents.cancelHandler);
			}
		}
		if (m_controllerOpenDropdown == null)
		{
			MoveControllerCursorTo(selectable);
		}
	}

	private void SimulateControllerClick(EventSystem i_eventSystem, GameObject i_target)
	{
		UnityEngine.UI.Button component = i_target.GetComponent<UnityEngine.UI.Button>();
		if (component != null)
		{
			component.onClick.Invoke();
			return;
		}
		UnityEngine.UI.Dropdown component2 = i_target.GetComponent<UnityEngine.UI.Dropdown>();
		if (component2 != null)
		{
			component2.Show();
			m_controllerOpenDropdown = component2;
			m_controllerDropdownIndex = component2.value;
			SelectControllerDropdownOption(i_eventSystem);
			return;
		}
		UnityEngine.UI.Toggle component3 = i_target.GetComponent<UnityEngine.UI.Toggle>();
		if (component3 != null)
		{
			// Dropdown options are radio-group toggles. Selecting an already active
			// option must leave it on; turning it off does not notify Dropdown.
			component3.isOn = component3.group != null || !component3.isOn;
			return;
		}
		Vector2 vector = Mouse.current != null ? Mouse.current.position.ReadValue() : (Vector2)i_target.transform.position;
		PointerEventData pointerEventData = new PointerEventData(i_eventSystem)
		{
			position = vector,
			button = PointerEventData.InputButton.Left,
			clickCount = 1,
			eligibleForClick = true
		};
		ExecuteEvents.Execute(i_target, pointerEventData, ExecuteEvents.pointerClickHandler);
	}

	private bool HandleControllerDropdown(Gamepad i_gamepad, EventSystem i_eventSystem)
	{
		if (m_controllerOpenDropdown == null)
		{
			return false;
		}
		if (!m_controllerOpenDropdown.isActiveAndEnabled || !m_controllerOpenDropdown.gameObject.activeInHierarchy)
		{
			m_controllerOpenDropdown = null;
			return false;
		}
		Vector2 vector2 = i_gamepad.dpad.ReadValue();
		bool flag3 = vector2.sqrMagnitude >= 0.25f;
		Vector2 vector = flag3 ? vector2 : i_gamepad.leftStick.ReadValue();
		float num = flag3 ? 0.6f : 0.8f;
		bool flag = Mathf.Abs(vector.y) > num;
		if (Mathf.Abs(vector.x) < 0.2f && Mathf.Abs(vector.y) < 0.2f)
		{
			m_lastUINavigation = Vector2.zero;
		}
		if (flag && m_lastUINavigation.sqrMagnitude < 0.25f && Time.unscaledTime >= m_nextUINavigationTime)
		{
			m_nextUINavigationTime = Time.unscaledTime + 0.25f;
			int num2 = vector.y > 0f ? -1 : 1;
			m_controllerDropdownIndex = Mathf.Clamp(m_controllerDropdownIndex + num2, 0, m_controllerOpenDropdown.options.Count - 1);
			SelectControllerDropdownOption(i_eventSystem);
		}
		if (flag)
		{
			m_lastUINavigation = vector;
		}
		if (m_isUISubmitReady && i_gamepad.buttonSouth.wasPressedThisFrame && Time.unscaledTime >= m_nextUISubmitTime)
		{
			m_isUISubmitReady = false;
			m_isUISubmitPending = true;
		}
		if (m_isUISubmitPending && i_gamepad.buttonSouth.wasReleasedThisFrame)
		{
			Dropdown dropdown = m_controllerOpenDropdown;
			m_controllerOpenDropdown = null;
			m_isUISubmitPending = false;
			m_isUISubmitReady = true;
			m_nextUISubmitTime = Time.unscaledTime;
			dropdown.value = m_controllerDropdownIndex;
			dropdown.RefreshShownValue();
			dropdown.Hide();
			i_eventSystem.SetSelectedGameObject(dropdown.gameObject);
			MoveControllerCursorTo(dropdown);
			return true;
		}
		if (i_gamepad.buttonEast.wasPressedThisFrame)
		{
			Dropdown dropdown2 = m_controllerOpenDropdown;
			m_controllerOpenDropdown = null;
			m_isUISubmitPending = false;
			m_isUISubmitReady = !i_gamepad.buttonSouth.isPressed;
			dropdown2.Hide();
			i_eventSystem.SetSelectedGameObject(dropdown2.gameObject);
			MoveControllerCursorTo(dropdown2);
		}
		return true;
	}

	private void SelectControllerDropdownOption(EventSystem i_eventSystem)
	{
		if (m_controllerOpenDropdown == null)
		{
			return;
		}
		string text = "Item " + m_controllerDropdownIndex;
		Toggle[] array = UnityEngine.Object.FindObjectsOfType<Toggle>();
		for (int i = 0; i < array.Length; i++)
		{
			Toggle toggle = array[i];
			if (!toggle.gameObject.activeInHierarchy || !toggle.gameObject.name.StartsWith(text + ":"))
			{
				continue;
			}
			Transform transform = toggle.transform;
			while (transform != null && transform.name != "Dropdown List")
			{
				transform = transform.parent;
			}
			if (transform != null)
			{
				i_eventSystem.SetSelectedGameObject(toggle.gameObject);
				ScrollRect scrollRect = toggle.GetComponentInParent<ScrollRect>();
				if (scrollRect != null && m_controllerOpenDropdown.options.Count > 1)
				{
					Canvas.ForceUpdateCanvases();
					scrollRect.verticalNormalizedPosition = 1f - (float)m_controllerDropdownIndex / (m_controllerOpenDropdown.options.Count - 1);
				}
				MoveControllerCursorTo(toggle);
				return;
			}
		}
	}

	private void MoveControllerCursorTo(Selectable i_selectable)
	{
		if (m_controllerSelection == i_selectable)
		{
			return;
		}
		ClearControllerSelectionOutline();
		m_controllerSelection = i_selectable;
		RectTransform component = i_selectable.GetComponent<RectTransform>();
		Mouse mouse = Mouse.current;
		if (component == null || mouse == null)
		{
			return;
		}
		Canvas canvas = i_selectable.GetComponentInParent<Canvas>();
		Camera camera = canvas == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
		Vector2 vector = RectTransformUtility.WorldToScreenPoint(camera, component.TransformPoint(component.rect.center));
		mouse.WarpCursorPosition(vector);
		m_ignoreMouseMovementFrames = 4;
	}

	private void UpdateUIInputModuleMode()
	{
		bool flag = IsControllerLastUsed() && IsMenuOpen();
		EventSystem current = EventSystem.current;
		if (current != null)
		{
			StandaloneInputModule component = current.GetComponent<StandaloneInputModule>();
			if (component != null)
			{
				component.enabled = true;
			}
			// StandaloneInputModule must remain enabled for physical mouse clicks.
			// Disable only its legacy controller navigation/submit; ManagerInput
			// supplies exactly one deliberate controller event itself.
			current.sendNavigationEvents = !flag;
		}
		if (flag && !m_isControllerUIMode)
		{
			m_lastUINavigation = Vector2.zero;
			m_nextUINavigationTime = Time.unscaledTime + 0.12f;
			m_isUISubmitReady = true;
			m_isUISubmitPending = false;
			m_controllerSubmitTarget = null;
		}
		else if (!flag)
		{
			m_lastUINavigation = Vector2.zero;
			ClearControllerSelectionOutline();
		}
		m_isControllerUIMode = flag;
	}

	private static Selectable FindBestMenuSelectable()
	{
		Selectable[] allSelectablesArray = Selectable.allSelectablesArray;
		Selectable selectable = null;
		float num = float.MaxValue;
		Vector2 vector = new Vector2(UnityEngine.Screen.width * 0.5f, UnityEngine.Screen.height * 0.5f);
		for (int i = 0; i < allSelectablesArray.Length; i++)
		{
			Selectable selectable2 = allSelectablesArray[i];
			if (!IsSelectableAvailable(selectable2))
			{
				continue;
			}
			float num2 = Vector2.SqrMagnitude((Vector2)selectable2.transform.position - vector);
			if (num2 < num)
			{
				num = num2;
				selectable = selectable2;
			}
		}
		return selectable;
	}

	private static Selectable FindSelectableInDirection(Selectable i_current, Vector2 i_direction)
	{
		// Unity creates dropdown options at runtime with explicit navigation links.
		// Prefer those links so the stick remains inside the open list.
		Selectable selectable3;
		if (Mathf.Abs(i_direction.x) > Mathf.Abs(i_direction.y))
		{
			selectable3 = i_direction.x > 0f ? i_current.FindSelectableOnRight() : i_current.FindSelectableOnLeft();
		}
		else
		{
			selectable3 = i_direction.y > 0f ? i_current.FindSelectableOnUp() : i_current.FindSelectableOnDown();
		}
		if (IsSelectableAvailable(selectable3))
		{
			return selectable3;
		}
		Selectable selectable = null;
		float num = float.MaxValue;
		Vector2 vector = i_current.transform.position;
		Selectable[] allSelectablesArray = Selectable.allSelectablesArray;
		for (int i = 0; i < allSelectablesArray.Length; i++)
		{
			Selectable selectable2 = allSelectablesArray[i];
			if (selectable2 == i_current || !IsSelectableAvailable(selectable2))
			{
				continue;
			}
			Vector2 vector2 = (Vector2)selectable2.transform.position - vector;
			float magnitude = vector2.magnitude;
			if (magnitude < 0.01f)
			{
				continue;
			}
			float num2 = Vector2.Dot(vector2 / magnitude, i_direction);
			if (num2 < 0.35f)
			{
				continue;
			}
			float num3 = magnitude * (1f + (1f - num2) * 4f);
			if (num3 < num)
			{
				num = num3;
				selectable = selectable2;
			}
		}
		return selectable;
	}

	private static bool IsSelectableAvailable(Selectable i_selectable)
	{
		return i_selectable != null && i_selectable.isActiveAndEnabled && i_selectable.gameObject.activeInHierarchy && i_selectable.IsInteractable();
	}

	private static bool IsMenuOpen()
	{
		ScreenGame screenGame = CommonReferences.Instance.GetManagerScreens().GetScreenGame();
		if (!screenGame.gameObject.activeInHierarchy || screenGame.IsPaused())
		{
			return true;
		}
		ManagerHud managerHud = CommonReferences.Instance.GetManagerHud();
		return managerHud.GetVendorHud().GetIsOpen() || managerHud.GetWardrobeHud().IsShowing() || managerHud.GetHubMainMenu().IsOpen() || managerHud.GetManagerEquippablesHud().GetIsShowing() || managerHud.GetKeypadHud().IsShowing();
	}

	private static void ClearControllerSelection()
	{
		if (EventSystem.current != null)
		{
			EventSystem.current.SetSelectedGameObject(null);
		}
	}

	private void RefreshControllerSelectionOutline(Selectable i_selectable)
	{
	}

	private void ClearControllerSelectionOutline()
	{
		if (m_controllerSelectionOutline != null)
		{
			UnityEngine.Object.Destroy(m_controllerSelectionOutline);
		}
		if (m_controllerSelectionMarker != null)
		{
			UnityEngine.Object.Destroy(m_controllerSelectionMarker);
		}
		m_controllerSelectionOutline = null;
		m_controllerSelectionMarker = null;
		m_controllerSelection = null;
	}

	public string GetControllerBindingName(InputButton i_inputButton)
	{
		switch (i_inputButton)
		{
		case InputButton.MoveLeft:
		case InputButton.MoveRight: return "Left Stick (Move / Aim)";
		case InputButton.Jump: return "A / Cross";
		case InputButton.Walk: return "Right Stick Click";
		case InputButton.Crouch: return "B / Circle";
		case InputButton.Dash: return "Left Stick Click";
		case InputButton.Fire: return "Right Trigger";
		case InputButton.Reload: return "X / Square";
		case InputButton.Use: return "Y / Triangle";
		case InputButton.PickUp: return "D-Pad Left";
		case InputButton.EquipPrevious: return "Bumpers (Cycle Weapons)";
		case InputButton.DropWeapon: return "D-Pad Right";
		case InputButton.DrugSelection: return "D-Pad Up / Down (Cycle Drugs)";
		case InputButton.Expose: return "Left Trigger";
		default: return "";
		}
	}

	public string GetPromptBindingName(InputButton i_inputButton)
	{
		if (IsControllerLastUsed())
		{
			return GetControllerBindingName(i_inputButton);
		}
		if (CommonReferences.Instance.GetPlayerController().GetIsMobileControlsEnabled())
		{
			switch (i_inputButton)
			{
			case InputButton.Use:
			case InputButton.PickUp: return "Interact button";
			case InputButton.Jump: return "Jump button";
			case InputButton.Fire: return "Fire button";
			case InputButton.Reload: return "Reload button";
			case InputButton.Dash: return "Dash button";
			default: return "Touch control";
			}
		}
		return GetKeyAssignedToButton(i_inputButton).ToString();
	}

	public bool IsWaveSkipPressed()
	{
		return Input.GetKeyDown(KeyCode.Z) || (Gamepad.current != null && Gamepad.current.selectButton.wasPressedThisFrame);
	}

	public string GetWaveSkipBindingName()
	{
		if (IsControllerLastUsed())
		{
			return "View / Share";
		}
		if (CommonReferences.Instance.GetPlayerController().GetIsMobileControlsEnabled())
		{
			return "Skip button";
		}
		return "Z";
	}

	private enum ButtonPhase
	{
		Held,
		Down,
		Up
	}

	private bool IsControllerButton(InputButton i_inputButton, ButtonPhase i_phase)
	{
		Gamepad gamepad = Gamepad.current;
		if (gamepad == null)
		{
			return false;
		}
		UnityEngine.InputSystem.Controls.ButtonControl button = null;
		switch (i_inputButton)
		{
		case InputButton.MoveLeft: return i_phase == ButtonPhase.Held && gamepad.leftStick.ReadValue().x < -0.2f;
		case InputButton.MoveRight: return i_phase == ButtonPhase.Held && gamepad.leftStick.ReadValue().x > 0.2f;
		case InputButton.Jump: button = gamepad.buttonSouth; break;
		case InputButton.Walk: button = gamepad.rightStickButton; break;
		case InputButton.Crouch: button = gamepad.buttonEast; break;
		case InputButton.Dash: button = gamepad.leftStickButton; break;
		case InputButton.Fire: button = gamepad.rightTrigger; break;
		case InputButton.Reload: button = gamepad.buttonWest; break;
		case InputButton.Use: button = gamepad.buttonNorth; break;
		case InputButton.PickUp: button = gamepad.dpad.left; break;
		case InputButton.EquipPrevious: button = gamepad.rightShoulder; break;
		case InputButton.DropWeapon: button = gamepad.dpad.right; break;
		case InputButton.DrugSelection: button = gamepad.dpad.up; break;
		case InputButton.Expose: button = gamepad.leftTrigger; break;
		}
		if (button == null)
		{
			return false;
		}
		switch (i_phase)
		{
		case ButtonPhase.Down: return button.wasPressedThisFrame;
		case ButtonPhase.Up: return button.wasReleasedThisFrame;
		default: return button.isPressed;
		}
	}
}
