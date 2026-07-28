using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerInputHandler : MonoBehaviour
{
    [Header("Character Input Values")]
    public Vector2 move;
    public Vector2 look;

    public bool jump;
    public bool sprint;
    public bool interact;
    public bool wheel;
    public bool toggleFlashlight;
    public bool cycleFlashlightCookie;
    public bool placementModeOn;
    public bool placementModeOff;
    public bool placeObject;
    public bool cancel;
    public bool miniGameConfirm;

    public Vector2 wheelNavigate;

    [Header("Movement Settings")]
    public bool analogMovement;

    [Header("Mouse Cursor Settings")]
    public bool cursorLocked = true;
    public bool cursorInputForLook = true;
    public bool blockLookInput;

    private PlayerControls playerControls;
    private PlayerInput playerInput;

    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;
    private InputAction sprintAction;
    private InputAction interactAction;
    private InputAction wheelAction;
    private InputAction wheelNavigateAction;
    public InputAction flashlightAction;
    private InputAction cycleFlashlightCookieAction;
    private InputAction enterPlacementMode;
    private InputAction exitPlacementMode;
    private InputAction placeObjectAction;
    private InputAction cancelAction;
    private InputAction miniGameConfirmAction;

    public bool IsUsingMouse
    {
        get
        {
            return playerInput != null &&
                   playerInput.currentControlScheme == "KeyboardMouse";
        }
    }

    private void Awake()
    {

        playerInput = GetComponent<PlayerInput>();
        playerControls = new PlayerControls();
        SetCursorState(cursorLocked);

        moveAction = playerControls.Player.Move;
        lookAction = playerControls.Player.Look;
        jumpAction = playerControls.Player.Jump;
        sprintAction = playerControls.Player.Sprint;
        interactAction = playerControls.Player.Interact;
        wheelAction = playerControls.Player.Wheel;
        wheelNavigateAction = playerControls.Player.WheelNavigate;
        flashlightAction = playerControls.Player.Flashlight;
        cycleFlashlightCookieAction = playerControls.Player.CycleFlashlightCookie;
        enterPlacementMode = playerControls.Player.EnterPlacementMode;
        exitPlacementMode = playerControls.Player.ExitPlacementMode;
        placeObjectAction = playerControls.Player.PlaceObject;
        cancelAction = playerControls.Player.Cancel;
        miniGameConfirmAction = playerControls.Player.MiniGameConfirm;

    }

    private void OnEnable()
    {
        moveAction.performed += OnMovePerformed;
        moveAction.canceled += OnMovePerformed;

        lookAction.performed += OnLookPerformed;
        lookAction.canceled += OnLookCanceled;

        jumpAction.performed += OnJumpPerformed;
        jumpAction.canceled += OnJumpCanceled;

        sprintAction.performed += OnSprintPerformed;
        sprintAction.canceled += OnSprintCanceled;

        interactAction.performed += OnInteractPerformed;
        interactAction.canceled += OnInteractCanceled;

        wheelAction.performed += OnWheelPerformed;
        wheelAction.canceled += OnWheelCanceled;

        wheelNavigateAction.performed += OnWheelNavigatePerformed;
        wheelNavigateAction.canceled += OnWheelNavigateCanceled;

        flashlightAction.performed += OnFlashlightPerformed;

        cycleFlashlightCookieAction.performed += OnCycleFlashlightCookiePerformed;
        cycleFlashlightCookieAction.canceled += OnCycleFlashlightCookieCanceled;

        enterPlacementMode.performed += OnEnterPlacementModePerformed;
        enterPlacementMode.canceled += OnEnterPlacementModeCanceled;

        exitPlacementMode.performed += OnExitPlacementModePerformed;
        exitPlacementMode.canceled += OnExitPlacementModeCanceled;

        placeObjectAction.performed += OnPlaceObjectPerformed;
        placeObjectAction.canceled += OnPlaceObjectCanceled;

        cancelAction.performed += OnCancelPerformed;
        cancelAction.canceled += OnCancelCanceled;

        miniGameConfirmAction.performed += OnMiniGameConfirmPerformed;
        miniGameConfirmAction.canceled += OnMiniGameConfirmCanceled;

        playerControls.Enable();
    }

    

    private void OnDisable()
    {
        moveAction.performed -= OnMovePerformed;
        moveAction.canceled -= OnMovePerformed;

        lookAction.performed -= OnLookPerformed;
        lookAction.canceled -= OnLookCanceled;

        jumpAction.performed -= OnJumpPerformed;
        jumpAction.canceled -= OnJumpCanceled;

        sprintAction.performed -= OnSprintPerformed;
        sprintAction.canceled -= OnSprintCanceled;

        interactAction.performed -= OnInteractPerformed;
        interactAction.canceled -= OnInteractCanceled;

        wheelAction.performed -= OnWheelPerformed;
        wheelAction.canceled -= OnWheelCanceled;

        wheelNavigateAction.performed -= OnWheelNavigatePerformed;
        wheelNavigateAction.canceled -= OnWheelNavigateCanceled;

        flashlightAction.performed -= OnFlashlightPerformed;

        cycleFlashlightCookieAction.performed -= OnCycleFlashlightCookiePerformed;
        cycleFlashlightCookieAction.canceled -= OnCycleFlashlightCookieCanceled;

        enterPlacementMode.performed -= OnEnterPlacementModePerformed;
        enterPlacementMode.canceled -= OnEnterPlacementModeCanceled;

        exitPlacementMode.performed -= OnExitPlacementModePerformed;
        exitPlacementMode.canceled -= OnExitPlacementModeCanceled;

        placeObjectAction.performed -= OnPlaceObjectPerformed;
        placeObjectAction.canceled -= OnPlaceObjectCanceled;

        cancelAction.performed -= OnCancelPerformed;
        cancelAction.canceled -= OnCancelCanceled;

        miniGameConfirmAction.performed -= OnMiniGameConfirmPerformed;
        miniGameConfirmAction.canceled -= OnMiniGameConfirmCanceled;

        playerControls.Disable();
    }

    private void OnMovePerformed(InputAction.CallbackContext ctx)
    {
        move = ctx.ReadValue<Vector2>();
    }

    private void OnLookPerformed(InputAction.CallbackContext ctx)
    {
        if (cursorInputForLook && !blockLookInput)
        {
            look = ctx.ReadValue<Vector2>();
        }
    }

    private void OnLookCanceled(InputAction.CallbackContext ctx)
    {
        look = Vector2.zero;
    }

    private void OnJumpPerformed(InputAction.CallbackContext ctx)
    {
        jump = true;
    }

    private void OnJumpCanceled(InputAction.CallbackContext ctx)
    {
        jump = false;
    }

    private void OnSprintPerformed(InputAction.CallbackContext ctx)
    {
        sprint = true;
    }

    private void OnSprintCanceled(InputAction.CallbackContext ctx)
    {
        sprint = false;
    }

    private void OnInteractPerformed(InputAction.CallbackContext ctx)
    {
        interact = true;
    }

    private void OnInteractCanceled(InputAction.CallbackContext ctx)
    {
        interact = false;
    }

    private void OnCancelPerformed(InputAction.CallbackContext ctx)
    {
        cancel = true;
    }

    private void OnCancelCanceled(InputAction.CallbackContext ctx)
    {
        cancel = false;
    }

    private void OnMiniGameConfirmPerformed(InputAction.CallbackContext ctx)
    {
        miniGameConfirm = true;
    }

    private void OnMiniGameConfirmCanceled(InputAction.CallbackContext ctx)
    {
        miniGameConfirm = false;
    }

    private void OnEnterPlacementModeCanceled(InputAction.CallbackContext context)
    {
        placementModeOn = false; 
    }

    private void OnEnterPlacementModePerformed(InputAction.CallbackContext context)
    {
        placementModeOn = true; 
    }
    private void OnExitPlacementModeCanceled(InputAction.CallbackContext context)
    {
        placementModeOff = false; 
    }

    private void OnExitPlacementModePerformed(InputAction.CallbackContext context)
    {
        placementModeOff = true; 
    }

    private void OnPlaceObjectPerformed(InputAction.CallbackContext context)
    {
        placeObject = true;
    }

    private void OnPlaceObjectCanceled(InputAction.CallbackContext context)
    {
        placeObject = false;
    }


    private void OnWheelPerformed(InputAction.CallbackContext ctx)
    {
        wheel = true;
    }

    private void OnWheelCanceled(InputAction.CallbackContext ctx)
    {
        wheel = false;
    }

    private void OnWheelNavigatePerformed(InputAction.CallbackContext ctx)
    {
        wheelNavigate = ctx.ReadValue<Vector2>();
    }

    private void OnWheelNavigateCanceled(InputAction.CallbackContext ctx)
    {
        wheelNavigate = Vector2.zero;
    }

    private void OnFlashlightPerformed(InputAction.CallbackContext ctx)
    {
        toggleFlashlight = true;
    }

    private void OnCycleFlashlightCookiePerformed(InputAction.CallbackContext ctx)
    {
        cycleFlashlightCookie = true;
    }

    private void OnCycleFlashlightCookieCanceled(InputAction.CallbackContext ctx)
    {
        cycleFlashlightCookie = false;
    }

    public void ClearLookInput()
    {
        look = Vector2.zero;
    }

    public void ResetInputValues()
    {
        move = Vector2.zero;
        look = Vector2.zero;
        jump = false;
        sprint = false;
        interact = false;
        wheel = false;
        toggleFlashlight = false;
        cycleFlashlightCookie = false;
        wheelNavigate = Vector2.zero;
        cancel = false;
        miniGameConfirm = false;
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        SetCursorState(cursorLocked);
    }

    private void SetCursorState(bool newState)
    {
        Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
    }
}