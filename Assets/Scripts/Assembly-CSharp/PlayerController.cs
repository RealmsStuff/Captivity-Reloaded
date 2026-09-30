using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    private Player m_player;
    private Inventory m_inventory;
    private ManagerInput m_managerInput;

    private Notification m_pickUpPrompt;
    private bool m_isForceIgnoreInput;

    [Header("Mobile Settings")]
    [SerializeField] private bool m_useMobileControls = false;

    // Direct inputs from the UI Canvas
    private Vector2 m_leftJoystickInput;
	private Vector2 m_mobileAimInput = Vector2.right;
	private Vector2 m_lastMobileAim = Vector2.right;
    private bool m_mobileJumpPressed;
    private bool m_mobileInteractPressed;
    private bool m_mobileReloadPressed; // Declared
    private bool m_mobileDashPressed;   // Declared
    private bool m_mobileFirePressed;   // For semi-automatic (one shot per tap)
    private bool m_mobileFireHeld;      // For automatic (shoots while held)
	private bool m_mobileFireReleased;
	private bool m_mobileAltFirePressed;
    private bool m_mobileExposeHeld;
    private bool m_mobileWavePressed;
	private bool m_mobileSelfPleasurePressed;

    public void Awake()
    {
        m_isForceIgnoreInput = false;
        LoadInventory();
        m_managerInput = CommonReferences.Instance.GetManagerInput();

        // Automatically detect mobile platforms, or defer to the inspector toggle for testing.
#if UNITY_ANDROID || UNITY_IOS
        m_useMobileControls = true;
#endif
		if (PlayerPrefs.HasKey("ForceMobileControls"))
		{
			PlayerPrefs.DeleteKey("ForceMobileControls");
			PlayerPrefs.Save();
		}
    }

    public bool GetIsMobileControlsEnabled()
    {
        return IsUsingMobileInput();
    }

    private bool IsUsingMobileInput()
    {
		return m_useMobileControls && Gamepad.current == null;
    }

    private void Update()
    {
		HandlePickUpPrompt();
		HandleSelfPleasureInput();
		if (m_player.IsSelfPleasuring()) return;
        if (!m_isForceIgnoreInput && m_player.GetStatePlayerCurrent() != StatePlayer.BeingRaped && !m_player.IsDead() && !CommonReferences.Instance.GetManagerScreens().GetScreenGame().IsPaused() && !CommonReferences.Instance.GetManagerHud().GetVendorHud().GetIsOpen() && !CommonReferences.Instance.GetManagerHud().GetWardrobeHud().IsShowing() && !CommonReferences.Instance.GetManagerHud().GetHubMainMenu().IsOpen())
        {
            HandleInput();
        }

        // Automatic Facing Direction
        if (!m_isForceIgnoreInput && m_player.GetIsCanSwitchFacingSide() && !m_player.GetIsBeingRaped())
        {
            if (IsUsingMobileInput())
            {
				// Mobile movement and aim are independent. Facing follows the aim
				// stick, never the movement stick or a touched UI button.
                m_player.FaceSideAim();
            }
            else
            {
                m_player.FaceSideAim();
            }
        }
    }

    private void LateUpdate()
    {
        // Reset single-frame mobile presses AFTER all other scripts have read them
        m_mobileJumpPressed = false;
        m_mobileInteractPressed = false;
        m_mobileReloadPressed = false;
        m_mobileDashPressed = false;
        m_mobileFirePressed = false;
		m_mobileFireReleased = false;
		m_mobileAltFirePressed = false;
        m_mobileWavePressed = false;
		m_mobileSelfPleasurePressed = false;
    }

    // Direct inputs from the mobile UI Joystick
    public void SetLeftJoystick(Vector2 i_value) => m_leftJoystickInput = i_value;
	public void SetRightJoystick(Vector2 i_value)
	{
		m_mobileAimInput = Vector2.ClampMagnitude(i_value, 1f);
		if (m_mobileAimInput.sqrMagnitude > 0.04f)
		{
			m_lastMobileAim = m_mobileAimInput.normalized;
		}
	}

	public Vector2 GetMobileAimDirection()
	{
		if (m_mobileAimInput.sqrMagnitude > 0.04f) return m_mobileAimInput.normalized;
		if (m_lastMobileAim.sqrMagnitude > 0.04f) return m_lastMobileAim.normalized;
		return m_player != null && m_player.GetIsFacingLeft() ? Vector2.left : Vector2.right;
	}

	public Vector2 GetMobileAimInput()
	{
		return m_mobileAimInput;
	}

	public bool GetUseMobileDPad()
	{
		return PlayerPrefs.GetInt("MobileUseDPad", 0) == 1;
	}

	public Vector3 GetMobileAimScreenPosition(Vector3 i_worldOrigin)
	{
		Camera camera = CommonReferences.Instance.GetManagerCamerasXGame().GetCameraXGameCurrent().GetCameraUnity();
		if (camera == null) return Input.mousePosition;
		return camera.WorldToScreenPoint(i_worldOrigin) + (Vector3)(GetMobileAimDirection() * 500f);
	}

	public Vector3 GetMobileAimWorldPosition(Vector3 i_worldOrigin)
	{
		if (m_managerInput != null && IsUsingMobileInput())
		{
			return m_managerInput.GetAimWorldPosition(i_worldOrigin);
		}
		Camera camera = CommonReferences.Instance.GetManagerCamerasXGame().GetCameraXGameCurrent().GetCameraUnity();
		if (camera == null) return i_worldOrigin + (Vector3)(GetMobileAimDirection() * 10f);
		Vector3 screen = GetMobileAimScreenPosition(i_worldOrigin);
		return camera.ScreenToWorldPoint(new Vector3(screen.x, screen.y, 10f));
	}

    public void TriggerMobileExpose() => m_mobileExposeHeld = true;
    public void ReleaseMobileExpose() => m_mobileExposeHeld = false;

    public void TriggerMobileWave()
    {
        m_mobileWavePressed = true;
    }

    public bool GetIsStrugglePressed()
    {
        return m_mobileJumpPressed;
    }

    public bool GetIsMobileJumpPressed() => m_mobileJumpPressed;

    public bool ShouldShowMobileControls()
    {
        if (!IsUsingMobileInput() || CommonReferences.Instance == null) return false;
        ManagerHud hud = CommonReferences.Instance.GetManagerHud();
        ScreenGame game = CommonReferences.Instance.GetManagerScreens().GetScreenGame();
        if (hud == null || game == null || game.IsPaused()) return false;
        return !hud.GetVendorHud().GetIsOpen()
            && !hud.GetWardrobeHud().IsShowing()
            && !hud.GetHubMainMenu().IsOpen()
            && !hud.GetManagerEquippablesHud().GetIsShowing()
            && !hud.GetKeypadHud().IsShowing();
    }

    public void CancelMobileHeldInputs()
    {
        m_leftJoystickInput = Vector2.zero;
        m_mobileAimInput = Vector2.zero;
        m_mobileFireHeld = false;
        m_mobileExposeHeld = false;
    }

    public bool GetIsWavePressed()
    {
        return m_mobileWavePressed;
    }
    public void TriggerMobileFireDown()
    {
        m_mobileFirePressed = true;
        m_mobileFireHeld = true;
    }

    public void TriggerMobileFireUp()
    {
        m_mobileFireHeld = false;
		m_mobileFireReleased = true;
    }
	public void TriggerMobileAlternateFire() => m_mobileAltFirePressed = true;
	public void TriggerMobileSelfPleasure() => m_mobileSelfPleasurePressed = true;

    // Direct inputs from the mobile UI Buttons
    public void TriggerMobileJump() => m_mobileJumpPressed = true;
    public void TriggerMobileInteract() => m_mobileInteractPressed = true;
    public void TriggerMobileEscape()
    {
        CommonReferences.Instance.GetManagerScreens().GetScreenGame().HandleEscapeInput();
    }
    public void TriggerMobileReload() => m_mobileReloadPressed = true; // Implemented
    public void TriggerMobileDash() => m_mobileDashPressed = true;     // Implemented
	public void TriggerMobileNextWeapon()
	{
		if (m_player != null)
		{
			m_player.EquipNextWeapon(i_onlyUsables: false);
		}
	}
	public void TriggerMobileNextMedicine()
	{
		if (m_player != null)
		{
			m_player.EquipNextWeapon(i_onlyUsables: true);
		}
	}

	private void HandleSelfPleasureInput()
	{
		bool menuOpen = CommonReferences.Instance.GetManagerScreens().GetScreenGame().IsPaused()
			|| CommonReferences.Instance.GetManagerHud().GetVendorHud().GetIsOpen()
			|| CommonReferences.Instance.GetManagerHud().GetWardrobeHud().IsShowing()
			|| CommonReferences.Instance.GetManagerHud().GetHubMainMenu().IsOpen();
		if (m_player.IsSelfPleasuring() && menuOpen)
		{
			m_player.StopSelfPleasure();
			return;
		}
		bool pressed = IsUsingMobileInput() ? m_mobileSelfPleasurePressed : m_managerInput.IsButtonDown(InputButton.SelfPleasure);
		if (!pressed || menuOpen || m_isForceIgnoreInput) return;
		if (m_player.IsSelfPleasuring()) m_player.StopSelfPleasure();
		else m_player.TryStartSelfPleasure();
	}

    private void ApplyMobileFacing()
    {
        Vector3 localScale = m_player.transform.localScale;

        if (m_leftJoystickInput.x < -0.2f && localScale.x > 0) // Moving Left
        {
            localScale.x = -Mathf.Abs(localScale.x);
            m_player.transform.localScale = localScale;
        }
        else if (m_leftJoystickInput.x > 0.2f && localScale.x < 0) // Moving Right
        {
            localScale.x = Mathf.Abs(localScale.x);
            m_player.transform.localScale = localScale;
        }
    }

    public void SetPlayer(Player l_player) => m_player = l_player;
    public Player GetPlayer() => m_player;

    private void LoadInventory()
    {
        m_inventory = new Inventory();
        m_inventory.LoadFromSave();
    }

    public void RemovePickupAbleFromInventory(PickUpable i_pickUpable) => GetInventory().RemovePickUpable(i_pickUpable);

    public bool GetIsHasPickUpable(PickUpable i_pickUpable)
    {
        foreach (PickUpable allPickUpable in CommonReferences.Instance.GetPlayerController().GetInventory().GetAllPickUpables())
        {
            if (i_pickUpable == allPickUpable) return true;
        }
        return false;
    }

    public void GainMoney(int i_amount)
    {
        m_inventory.AddMoney(i_amount);
        CommonReferences.Instance.GetManagerHud().GetMoneyHud().GainMoney(i_amount);
    }

    public void LoseMoney(int i_amount)
    {
        m_inventory.DepleteMoney(i_amount);
        CommonReferences.Instance.GetManagerHud().GetMoneyHud().LoseMoney(i_amount);
    }

    public void ResetInventory() => m_inventory.Reset();
    public Inventory GetInventory() => m_inventory;

    private void HandleInput()
    {
        // Handle Default Gun Shooting
        if ((bool)m_player.GetEquippableEquipped() && !m_player.GetIsUsingUsable() && m_player.GetIsThinking() && m_player.GetIsCanAttack() && !m_player.GetIsEquipping() && !CommonReferences.Instance.GetManagerHud().GetManagerEquippablesHud().GetIsShowing() && !m_player.IsExposing())
        {
            if (m_player.GetEquippableEquipped() is Gun)
            {
                Gun gun = (Gun)m_player.GetEquippableEquipped();
				if (gun.HasThrowableAttack())
				{
					bool throwDown = IsUsingMobileInput() ? m_mobileFirePressed : m_managerInput.IsButtonDown(InputButton.Fire);
					bool throwUp = IsUsingMobileInput() ? m_mobileFireReleased : m_managerInput.IsButtonUp(InputButton.Fire);
					if (throwDown) gun.BeginThrowableAim();
					if (throwUp && gun.IsAimingThrowable()) m_player.ReleaseThrowableGun(gun);
				}
				else if (gun.HasChargeAttack())
				{
					bool chargeDown = IsUsingMobileInput() ? m_mobileFirePressed : m_managerInput.IsButtonDown(InputButton.Fire);
					bool chargeUp = IsUsingMobileInput() ? m_mobileFireReleased : m_managerInput.IsButtonUp(InputButton.Fire);
					if (chargeDown) gun.BeginCharge();
					if (chargeUp && gun.IsCharging()) m_player.ReleaseChargedGun(gun);
				}
				else
				{

                bool wantsFire = false;

                if (IsUsingMobileInput())
                {
                    // If semi-automatic, look for the tap. If automatic, look for the hold.
                    wantsFire = gun.GetIsSemiFire() ? m_mobileFirePressed : m_mobileFireHeld;
                }
                else
                {
                    wantsFire = gun.GetIsSemiFire() ? m_managerInput.IsButtonDown(InputButton.Fire) : m_managerInput.IsButton(InputButton.Fire);
                }

                if (wantsFire)
                {
                    if (m_player.GetIsReloading() && gun.IsReloadCancelable() && gun.GetAmmoMagazineLeft() > 0)
                    {
                        m_player.InterruptReload();
                    }
					if (!m_player.GetIsReloading())
					{
						m_player.UseEquippedEquippable(i_isAltFire: false);
						if (ExternalRuleProfileFactory.ShouldAutoReloadOnEmpty() && gun != null && gun.GetAmmoMagazineLeft() < 1
							&& (gun.GetAmmoLeft() > 0 || gun.GetIsAmmoInfinite()) && !m_player.GetIsReloading()) m_player.Reload();
					}
                }
				}
				bool wantsAlternate = IsUsingMobileInput() ? m_mobileAltFirePressed : m_managerInput.IsButtonDown(InputButton.AlternateFire);
				if (gun.HasAlternateAttack() && wantsAlternate && !m_player.GetIsReloading())
					m_player.UseEquippedEquippable(i_isAltFire: true);
            }
            else
            {
                // Non-gun weapon attacking (melee, etc.)
                bool wantsMelee = IsUsingMobileInput() ? m_mobileFirePressed : m_managerInput.IsButton(InputButton.Fire);
                if (wantsMelee)
                {
                    m_player.UseEquippedEquippable(i_isAltFire: false);
                }
            }
        }

        bool wantsReload = IsUsingMobileInput() ? m_mobileReloadPressed : m_managerInput.IsButton(InputButton.Reload);

        if (wantsReload && m_player.GetEquippableEquipped() is Gun && m_player.GetStatePlayerCurrent() != StatePlayer.Grappling && !m_player.GetIsEquipping() && !m_player.GetIsReloading() && !m_player.IsExposing())
        {
            Gun gun2 = (Gun)m_player.GetEquippableEquipped();
            if (gun2.GetAmmoMagazineLeft() < gun2.GetAmmoMagazineMax() && m_player.GetIsCanAttack() && !m_player.GetIsReloading() && (gun2.GetAmmoLeft() > 0 || gun2.GetIsAmmoInfinite()))
            {
                m_player.Reload();
            }
        }

        if (m_managerInput.IsButtonDown(InputButton.PickUp) || (IsUsingMobileInput() && m_mobileInteractPressed))
        {
            m_player.PickUpTry();
        }

        // 3. Handle Interact Button
        bool wantsInteract = IsUsingMobileInput() ? m_mobileInteractPressed : m_managerInput.IsButtonDown(InputButton.Use);
        if (wantsInteract)
        {
            bool flag = true;
            if (m_player.GetIsReloading() && (bool)m_player.GetEquippableEquipped())
            {
                if (((Gun)m_player.GetEquippableEquipped()).IsReloadCancelable())
                {
                    m_player.InterruptReload();
                    flag = true;
                }
                else
                {
                    flag = false;
                }
            }
            if (flag)
            {
                m_player.InteractTry();
            }
        }

        if (m_managerInput.IsButtonDown(InputButton.DropWeapon) && !m_player.GetIsReloading() && !m_player.GetIsEquipping() && !m_player.IsExposing())
        {
            m_player.DropEquippedEquippable();
        }

        bool flag2 = true;
        if (m_player.GetIsReloading())
        {
            Gun gun3 = (Gun)m_player.GetEquippableEquipped();
            if ((bool)gun3 && !gun3.IsReloadCancelable())
            {
                flag2 = false;
            }
        }
        if (flag2 && !m_player.GetIsUsingUsable() && !m_player.GetIsAttacking() && !m_player.IsExposing())
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) ShowWeaponsHud(WeaponType.Pistol);
            if (Input.GetKeyDown(KeyCode.Alpha2)) ShowWeaponsHud(WeaponType.Smg);
            if (Input.GetKeyDown(KeyCode.Alpha3)) ShowWeaponsHud(WeaponType.Shotgun);
            if (Input.GetKeyDown(KeyCode.Alpha4)) ShowWeaponsHud(WeaponType.Rifle);
            if (Input.GetKeyDown(KeyCode.Alpha5)) ShowWeaponsHud(WeaponType.Special);

			if (m_managerInput.IsControllerButtonDown(InputButton.DrugSelection))
			{
				m_player.EquipNextWeapon(i_onlyUsables: true);
			}
			else if (m_managerInput.IsControllerPreviousDrugPressed())
			{
				m_player.EquipNextWeapon(i_onlyUsables: true, i_backwards: true);
			}
			else if (m_managerInput.IsButtonDown(InputButton.DrugSelection))
			{
				ShowWeaponsHud(WeaponType.Usable);
			}
			if (m_managerInput.IsControllerButtonDown(InputButton.EquipPrevious))
			{
				m_player.EquipNextWeapon(i_onlyUsables: false);
			}
			else if (m_managerInput.IsControllerPreviousWeaponPressed())
			{
				m_player.EquipNextWeapon(i_onlyUsables: false, i_backwards: true);
			}
			else if (m_managerInput.IsButtonDown(InputButton.EquipPrevious))
			{
				m_player.EquipPreviousWeapon();
			}
        }
        HandleInputMovement();
    }

	private void HandlePickUpPrompt()
	{
		ScreenGame screenGame = CommonReferences.Instance.GetManagerScreens().GetScreenGame();
		ManagerHud managerHud = CommonReferences.Instance.GetManagerHud();
		bool flag = screenGame.gameObject.activeInHierarchy && !screenGame.IsPaused() && !m_player.IsDead() && !managerHud.GetVendorHud().GetIsOpen() && !managerHud.GetWardrobeHud().IsShowing() && !managerHud.GetHubMainMenu().IsOpen();
		Weapon weapon = null;
		if (flag && CommonReferences.Instance.GetManagerStages().GetStageCurrent() != null)
		{
			float num = m_player.GetRangePickUp();
			foreach (Item allItem in CommonReferences.Instance.GetManagerStages().GetStageCurrent().GetAllItems())
			{
				Weapon weapon2 = allItem as Weapon;
				if (weapon2 != null && weapon2.GetIsPickUpable() && !weapon2.IsPickedUp() && weapon2.isActiveAndEnabled)
				{
					float num2 = Vector2.Distance(weapon2.transform.position, m_player.transform.position);
					if (num2 <= num)
					{
						num = num2;
						weapon = weapon2;
					}
				}
			}
		}
		if (weapon == null)
		{
			DestroyPickUpPrompt();
			return;
		}
		string text = "Press " + m_managerInput.GetPromptBindingName(InputButton.PickUp) + " to pick up " + weapon.GetName();
		if (m_pickUpPrompt == null)
		{
			m_pickUpPrompt = managerHud.GetManagerNotification().CreateNotification(text, ColorTextNotification.Equippable, i_isContinues: true);
			m_pickUpPrompt.SetPromptInput(InputButton.PickUp);
		}
		else
		{
			m_pickUpPrompt.SetText(text);
		}
	}

	private void DestroyPickUpPrompt()
	{
		if (m_pickUpPrompt != null)
		{
			CommonReferences.Instance.GetManagerHud().GetManagerNotification().DestroyNotification(m_pickUpPrompt);
			m_pickUpPrompt = null;
		}
	}

    private void ShowWeaponsHud(WeaponType i_weaponType)
    {
        if (!m_player.IsExposing() && m_player.GetStateActorCurrent() != StateActor.Climbing && !CommonReferences.Instance.GetManagerHud().GetManagerEquippablesHud().GetIsShowing())
        {
            CommonReferences.Instance.GetManagerHud().GetManagerEquippablesHud().Show(i_weaponType);
        }
    }

    private void HandleInputMovement()
    {
        if (m_player.GetStatePlayerCurrent() != StatePlayer.Dashing)
        {
            // Crouch check
            bool isCrouchPressed = IsUsingMobileInput() ? (m_leftJoystickInput.y < -0.5f) : m_managerInput.IsButton(InputButton.Crouch);
            if (isCrouchPressed)
            {
                m_player.SetIsCrouching(i_isCrouching: true);
            }
            else
            {
                m_player.SetIsCrouching(i_isCrouching: false);
            }

            HandleInputExpose();
            HandleInputWalk();
            HandleInputDash();
            HandleInputJump();
        }
        HandleInputSprint();
    }

    private void HandleInputWalk()
    {
        if (IsUsingMobileInput())
        {
            if (m_leftJoystickInput.x < -0.2f)
            {
                m_player.MoveHorizontal(i_left: true);
            }
            else if (m_leftJoystickInput.x > 0.2f)
            {
                m_player.MoveHorizontal(i_left: false);
            }
        }
        else
        {
            if (m_managerInput.IsButton(InputButton.MoveLeft)) m_player.MoveHorizontal(i_left: true);
            if (m_managerInput.IsButton(InputButton.MoveRight)) m_player.MoveHorizontal(i_left: false);
        }
    }

    private void HandleInputJump()
    {
        bool wantsJump = IsUsingMobileInput() ? m_mobileJumpPressed : m_managerInput.IsButtonDown(InputButton.Jump);
        if (wantsJump && m_player.GetStateActorCurrent() != StateActor.Jumping && !m_player.IsCrouching())
        {
            m_player.Jump();
        }
    }

    private void HandleInputDash()
    {
        bool wantsDash = IsUsingMobileInput() ? m_mobileDashPressed : m_managerInput.IsButtonDown(InputButton.Dash);
        if (wantsDash)
        {
            bool isMovingLeft = IsUsingMobileInput() ? (m_leftJoystickInput.x < -0.2f) : m_managerInput.IsButton(InputButton.MoveLeft);
            bool isMovingRight = IsUsingMobileInput() ? (m_leftJoystickInput.x > 0.2f) : m_managerInput.IsButton(InputButton.MoveRight);

            if (isMovingLeft) m_player.Dash(i_left: true);
            else if (isMovingRight) m_player.Dash(i_left: false);
            else if (m_player.GetIsFacingLeft()) m_player.Dash(i_left: true);
            else m_player.Dash(i_left: false);
        }
    }

    private void HandleInputSprint()
    {
        bool isMovingLeft = IsUsingMobileInput() ? (m_leftJoystickInput.x < -0.2f) : m_managerInput.IsButton(InputButton.MoveLeft);
        bool isMovingRight = IsUsingMobileInput() ? (m_leftJoystickInput.x > 0.2f) : m_managerInput.IsButton(InputButton.MoveRight);
        bool isWalkPressed = IsUsingMobileInput() ? false : m_managerInput.IsButton(InputButton.Walk);

        if (m_player.GetIsSprinting() && !isWalkPressed)
        {
            if (m_player.GetIsFacingLeft())
            {
                if (isMovingLeft)
                {
                    m_player.SetIsSprinting(i_isSprinting: true);
                    return;
                }
            }
            else if (isMovingRight)
            {
                m_player.SetIsSprinting(i_isSprinting: true);
                return;
            }
        }
        if (!isWalkPressed)
        {
            if (m_player.GetIsFacingLeft())
            {
                if (isMovingLeft)
                {
                    m_player.SetIsSprinting(i_isSprinting: true);
                    return;
                }
            }
            else if (isMovingRight)
            {
                m_player.SetIsSprinting(i_isSprinting: true);
                return;
            }
        }
        m_player.SetIsSprinting(i_isSprinting: false);
    }

    private void HandleInputExpose()
    {
        if (m_player.GetStatePlayerCurrent() == StatePlayer.Dashing || m_player.IsCrouching())
        {
            m_player.SetIsExposing(i_isExposing: false);
        }
		else if (((IsUsingMobileInput() && m_mobileExposeHeld) || (!IsUsingMobileInput() && m_managerInput.IsButton(InputButton.Expose)))
			&& (!(m_player.GetEquippableEquipped() is Gun equippedGun) || !equippedGun.HasAlternateAttack()))
        {
            m_player.SetIsExposing(i_isExposing: true);
        }
        else
        {
            m_player.SetIsExposing(i_isExposing: false);
        }
    }

    public void SetIsForceIgnoreInput(bool i_isForceIgnoreInput)
    {
        m_isForceIgnoreInput = m_player.IsDead() ? true : i_isForceIgnoreInput;
    }

    public bool GetIsForceIgnoreInput() => m_isForceIgnoreInput;
}
