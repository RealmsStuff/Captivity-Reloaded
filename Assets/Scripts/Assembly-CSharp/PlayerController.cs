using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    private Player m_player;
    private Inventory m_inventory;
    private ManagerInput m_managerInput;
    private bool m_isForceIgnoreInput;

    [Header("Mobile Settings")]
    [SerializeField] private bool m_useMobileControls = false;

    // Direct inputs from the UI Canvas
    private Vector2 m_leftJoystickInput;
    private bool m_mobileJumpPressed;
    private bool m_mobileInteractPressed;
    private bool m_mobileEscapePressed;
    private bool m_mobileReloadPressed; // Declared
    private bool m_mobileDashPressed;   // Declared
    private bool m_mobileFirePressed;   // For semi-automatic (one shot per tap)
    private bool m_mobileFireHeld;      // For automatic (shoots while held)
    private bool m_mobileStrugglePressed; // Set to true on tap
    private bool m_mobileWavePressed;

    public void Awake()
    {
        m_isForceIgnoreInput = false;
        LoadInventory();
        m_managerInput = CommonReferences.Instance.GetManagerInput();

        // Automatically detect mobile platforms, or defer to inspector toggle
        //#if UNITY_ANDROID && !UNITY_EDITOR
                m_useMobileControls = true;
        //#endif
    }

    public bool GetIsMobileControlsEnabled()
    {
        return m_useMobileControls;
    }

    private void Update()
    {
        if (!m_isForceIgnoreInput && m_player.GetStatePlayerCurrent() != StatePlayer.BeingRaped && !m_player.IsDead() && !CommonReferences.Instance.GetManagerScreens().GetScreenGame().IsPaused())
        {
            HandleInput();
        }

        // Automatic Facing Direction
        if (!m_isForceIgnoreInput && m_player.GetIsCanSwitchFacingSide() && !m_player.GetIsBeingRaped())
        {
            if (m_useMobileControls)
            {
                // 1. Face the direction the left joystick is pushed
                ApplyMobileFacing();

                // 2. Let the player aim towards the new virtual mouse position
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
        m_mobileEscapePressed = false;
        m_mobileReloadPressed = false;
        m_mobileDashPressed = false;
        m_mobileFirePressed = false;
        m_mobileStrugglePressed = false;
        m_mobileWavePressed = false;
    }

    // Direct inputs from the mobile UI Joystick
    public void SetLeftJoystick(Vector2 i_value) => m_leftJoystickInput = i_value;

    public void TriggerMobileStruggle()
    {
        m_mobileStrugglePressed = true;
    }

    public void TriggerMobileWave()
    {
        m_mobileWavePressed = true;
    }

    public bool GetIsStrugglePressed()
    {
        return m_mobileStrugglePressed;
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
    }

    // Direct inputs from the mobile UI Buttons
    public void TriggerMobileJump() => m_mobileJumpPressed = true;
    public void TriggerMobileInteract() => m_mobileInteractPressed = true;
    public void TriggerMobileEscape() => m_mobileEscapePressed = true;
    public void TriggerMobileReload() => m_mobileReloadPressed = true; // Implemented
    public void TriggerMobileDash() => m_mobileDashPressed = true;     // Implemented

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
        // 1. Handle Escape Button (Pause / Back / Escape Struggle)
        if (m_useMobileControls && m_mobileEscapePressed)
        {
            CommonReferences.Instance.GetManagerHud().OpenPauseMenu();
        }

        // 2. Handle Default Gun Shooting
        if ((bool)m_player.GetEquippableEquipped() && !m_player.GetIsUsingUsable() && m_player.GetIsThinking() && m_player.GetIsCanAttack() && !m_player.GetIsEquipping() && !CommonReferences.Instance.GetManagerHud().GetManagerEquippablesHud().GetIsShowing() && !m_player.IsExposing())
        {
            if (m_player.GetEquippableEquipped() is Gun)
            {
                Gun gun = (Gun)m_player.GetEquippableEquipped();

                bool wantsFire = false;

                if (m_useMobileControls)
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
                    }
                }
            }
            else
            {
                // Non-gun weapon attacking (melee, etc.)
                bool wantsMelee = m_useMobileControls ? m_mobileFirePressed : m_managerInput.IsButton(InputButton.Fire);
                if (wantsMelee)
                {
                    m_player.UseEquippedEquippable(i_isAltFire: false);
                }
            }
        }

        bool wantsReload = m_useMobileControls ? m_mobileReloadPressed : m_managerInput.IsButton(InputButton.Reload);

        if (wantsReload && m_player.GetEquippableEquipped() is Gun && m_player.GetStatePlayerCurrent() != StatePlayer.Grappling && !m_player.GetIsEquipping() && !m_player.GetIsReloading() && !m_player.IsExposing())
        {
            Gun gun2 = (Gun)m_player.GetEquippableEquipped();
            if (gun2.GetAmmoMagazineLeft() < gun2.GetAmmoMagazineMax() && m_player.GetIsCanAttack() && !m_player.GetIsReloading() && (gun2.GetAmmoLeft() > 0 || gun2.GetIsAmmoInfinite()))
            {
                m_player.Reload();
            }
        }

        if (m_managerInput.IsButtonDown(InputButton.PickUp))
        {
            m_player.PickUpTry();
        }

        // 3. Handle Interact Button
        bool wantsInteract = m_useMobileControls ? m_mobileInteractPressed : m_managerInput.IsButtonDown(InputButton.Use);
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

            if (m_managerInput.IsButtonDown(InputButton.DrugSelection)) ShowWeaponsHud(WeaponType.Usable);
            if (m_managerInput.IsButtonDown(InputButton.EquipPrevious)) m_player.EquipPreviousWeapon();
        }
        HandleInputMovement();
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
            bool isCrouchPressed = m_useMobileControls ? (m_leftJoystickInput.y < -0.5f) : m_managerInput.IsButton(InputButton.Crouch);
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
        if (m_useMobileControls)
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
        bool wantsJump = m_useMobileControls ? m_mobileJumpPressed : m_managerInput.IsButtonDown(InputButton.Jump);
        if (wantsJump && m_player.GetStateActorCurrent() != StateActor.Jumping && !m_player.IsCrouching())
        {
            m_player.Jump();
        }
    }

    private void HandleInputDash()
    {
        bool wantsDash = m_useMobileControls ? m_mobileDashPressed : m_managerInput.IsButtonDown(InputButton.Dash);
        if (wantsDash)
        {
            bool isMovingLeft = m_useMobileControls ? (m_leftJoystickInput.x < -0.2f) : m_managerInput.IsButton(InputButton.MoveLeft);
            bool isMovingRight = m_useMobileControls ? (m_leftJoystickInput.x > 0.2f) : m_managerInput.IsButton(InputButton.MoveRight);

            if (isMovingLeft) m_player.Dash(i_left: true);
            else if (isMovingRight) m_player.Dash(i_left: false);
            else if (m_player.GetIsFacingLeft()) m_player.Dash(i_left: true);
            else m_player.Dash(i_left: false);
        }
    }

    private void HandleInputSprint()
    {
        bool isMovingLeft = m_useMobileControls ? (m_leftJoystickInput.x < -0.2f) : m_managerInput.IsButton(InputButton.MoveLeft);
        bool isMovingRight = m_useMobileControls ? (m_leftJoystickInput.x > 0.2f) : m_managerInput.IsButton(InputButton.MoveRight);
        bool isWalkPressed = m_useMobileControls ? false : m_managerInput.IsButton(InputButton.Walk);

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
        else if (!m_useMobileControls && m_managerInput.IsButton(InputButton.Expose))
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