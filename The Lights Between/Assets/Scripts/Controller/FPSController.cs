using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using Unity.Cinemachine;
using UnityEngine.UI;

public class FPSController : MonoBehaviour
{
    [Header("Player Movement")]
    [Space(5)]
    public float MoveSpeed = 4.0f;
    public float SprintSpeed = 6.0f;
    public float ControllerSensitivity = 1.0f;
    public float MouseSensitivity = 1.0f;
    public float SpeedChangeRate = 10.0f;

    [Header("Movement SFX")]
    [Space(5)]
    public bool playingFootsteps = false;
    [SerializeField] private float walkFootstepSpeed = 0.5f;
    [SerializeField] private float sprintFootstepSpeed = 0.3f;
    [SerializeField] private float currentFootstepSpeed;

    [Header("Walk Footstep Clips")]
    [SerializeField] private AudioClip[] woodWalkFootstepClips;
    [SerializeField] private AudioClip[] tileWalkFootstepClips;
    [SerializeField] private AudioClip[] carpetWalkFootstepClips;
    [SerializeField] private AudioClip[] defaultWalkFootstepClips;

    [Header("Run Footstep Clips")]
    [SerializeField] private AudioClip[] woodRunFootstepClips;
    [SerializeField] private AudioClip[] tileRunFootstepClips;
    [SerializeField] private AudioClip[] carpetRunFootstepClips;
    [SerializeField] private AudioClip[] defaultRunFootstepClips;

    [SerializeField] private AudioSource stepSFX;
    [SerializeField] private float groundRayDistance = 1.5f;

    [Header("Interaction Settings")]
    [Space(5)]
    [SerializeField] public float interactionRange = 5f;
    public LayerMask interactionLayer;

    public bool isInteractable = false;
    public Image Crosshair;
    public Image InteractableCrosshair;
    [SerializeField] private Outline objectOutline;
    public Canvas hudCanvas;

    public bool isInteracting;
    public GameObject interactPrompt;
    [SerializeField] private TextMeshProUGUI interactText;
    private IInteractable currentInteractable;

    [Header("Jump Settings")]
    [Space(10)]
    public float JumpHeight = 1.2f;
    public float Gravity = -15.0f;

    [Space(10)]
    public float JumpTimeout = 0.1f;
    public float FallTimeout = 0.15f;

    [Header("Ground Check")]
    [Space(10)]
    public bool Grounded = true;
    public float GroundedOffset = -0.14f;
    public float GroundedRadius = 0.5f;
    public LayerMask GroundLayers;

    [Header("Camera Movement Noise")]
    [SerializeField] private CinemachineBasicMultiChannelPerlin cameraNoise;
    [SerializeField] private float walkNoiseAmplitude = 0.15f;
    [SerializeField] private float walkNoiseFrequency = 0.25f;
    [SerializeField] private float sprintNoiseAmplitude = 0.35f;
    [SerializeField] private float sprintNoiseFrequency = 0.8f;
    [SerializeField] private float noiseBlendSpeed = 8f;

    [Header("Cinemachine Settings")]
    public GameObject CinemachineCameraTarget;
    public float TopClamp = 90.0f;
    public float BottomClamp = -90.0f;
    private float _cinemachineTargetPitch;

    private float _speed;
    private float _rotationVelocity;
    private float _verticalVelocity;
    private float _terminalVelocity = 53.0f;

    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;

    private PlayerInput _playerInput;
    private PlayerInputHandler _input;

    private CharacterController _controller;
    private GameObject _mainCamera;

    private const float _threshold = 0.01f;

    public bool isInspecting;
    public bool isInMiniGame;
    public bool isJumpscareActive;

    private float _footstepTimer;

    private bool IsCurrentDeviceMouse
    {
        get
        {
            return _playerInput.currentControlScheme == "KeyboardMouse";
        }
    }

    private void Awake()
    {
        _input = GetComponent<PlayerInputHandler>();
        if (_mainCamera == null)
        {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }
    }

    private void Start()
    {
        _controller = GetComponent<CharacterController>();
        _playerInput = GetComponent<PlayerInput>();

        SetHUD();

        _jumpTimeoutDelta = JumpTimeout;
        _fallTimeoutDelta = FallTimeout;
    }

    private void Update()
    {
        if (SimpleTutorialPromptManager.IsTutorialActive || isInspecting || isInMiniGame || isJumpscareActive || GameLoopManager.IsGameEnded)
        {
            StopFootsteps();

            if (hudCanvas != null && hudCanvas.gameObject.activeSelf)
            {
                hudCanvas.gameObject.SetActive(false);
            }

            SetHUD();
            return;
        }

        JumpAndGravity();
        GroundedCheck();
        Move();
        HandleCameraNoise();

        DetectInteractable();
        UpdateCrosshair();
        Interaction();
        HandleFootsteps();
    }

    private void LateUpdate()
    {
        if (SimpleTutorialPromptManager.IsTutorialActive || isInspecting || isInMiniGame || isJumpscareActive || PauseScript.IsPaused || GameLoopManager.IsGameEnded) return;
        CameraRotation();
    }

    private void GroundedCheck()
    {
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
        Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
    }

    public void SetHUD()
    {
        if (SimpleTutorialPromptManager.IsTutorialActive)
        {
            if(hudCanvas.gameObject != null)
            {
                hudCanvas.gameObject.SetActive(!SimpleTutorialPromptManager.IsTutorialActive);
            }
        }
    }

    private void CameraRotation()
    {
        if (_input.look.sqrMagnitude >= _threshold)
        {
            float sensitivty = IsCurrentDeviceMouse ? MouseSensitivity : ControllerSensitivity;
            float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

            _cinemachineTargetPitch += _input.look.y * sensitivty * deltaTimeMultiplier;
            _rotationVelocity = _input.look.x * sensitivty * deltaTimeMultiplier;

            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);
            transform.Rotate(Vector3.up * _rotationVelocity);
        }
    }

    public void SetInspectionMode(bool inspecting)
    {
        isInspecting = inspecting;

        ClearInteractionState();
        UpdateCrosshair();

        if (_input != null)
        {
            _input.move = Vector2.zero;
            _input.jump = false;
            _input.sprint = false;
            _input.ClearLookInput();

            _input.cursorInputForLook = true;
            _input.blockLookInput = false;
        }
    }

    public void SetMiniGameMode(bool active)
    {
        isInMiniGame = active;
        if (hudCanvas != null) hudCanvas.gameObject.SetActive(!active);

        if (_input != null)
        {
            _input.move = Vector2.zero;
            _input.look = Vector2.zero;
            _input.jump = false;
            _input.sprint = false;
            _input.interact = false;
            _input.wheel = false;
            _input.ClearLookInput();

            _input.cursorInputForLook = !active;
            _input.blockLookInput = active;
        }

        if (active)
        {
            StopFootsteps();
            ClearInteractionState();
        }
    }

    public void SetJumpscareMode(bool active)
    {
        isJumpscareActive = active;
        if (hudCanvas != null) hudCanvas.gameObject.SetActive(!active);

        if (_input != null)
        {
            _input.move = Vector2.zero;
            _input.look = Vector2.zero;
            _input.jump = false;
            _input.sprint = false;
            _input.interact = false;
            _input.wheel = false;
            _input.ClearLookInput();

            _input.cursorInputForLook = false;
            _input.blockLookInput = true;
        }

        if (active)
        {
            StopFootsteps();
            ClearInteractionState();
            UpdateCrosshair();
        }
    }

    public void ForceLookAt(Vector3 targetPosition)
    {
        Vector3 forwardDir = targetPosition - transform.position;
        forwardDir.y = 0f;

        if (forwardDir.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(forwardDir);
        }

        if (_mainCamera != null)
        {
            Vector3 camToTarget = targetPosition - _mainCamera.transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(camToTarget);

            float pitch = targetRotation.eulerAngles.x;
            if (pitch > 180f)
            {
                pitch -= 360f;
            }

            _cinemachineTargetPitch = Mathf.Clamp(pitch, BottomClamp, TopClamp);
            CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);
        }
    }

    private void Move()
    {
        float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;

        if (_input.move == Vector2.zero) targetSpeed = 0.0f;

        float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

        float speedOffset = 0.1f;
        float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

        if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset)
        {
            _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);
            _speed = Mathf.Round(_speed * 1000f) / 1000f;
        }
        else
        {
            _speed = targetSpeed;
        }

        Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;
        if (_input.move != Vector2.zero)
        {
            inputDirection = transform.right * _input.move.x + transform.forward * _input.move.y;
        }

        _controller.Move(inputDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
    }

    private void HandleFootsteps()
    {
        bool isMoving = _input.move.sqrMagnitude > 0.01f;
        bool isSprinting = _input.sprint && isMoving;
        bool shouldPlayFootsteps = isMoving && Grounded;

        currentFootstepSpeed = isSprinting ? sprintFootstepSpeed : walkFootstepSpeed;

        if (shouldPlayFootsteps)
        {
            playingFootsteps = true;
            _footstepTimer -= Time.deltaTime;

            if (_footstepTimer <= 0f)
            {
                PlayFootstepsSFX();
                _footstepTimer = currentFootstepSpeed;
            }
        }
        else
        {
            playingFootsteps = false;

            if (!isMoving)
            {
                _footstepTimer = 0f;
            }
        }
    }

    public void StopFootsteps()
    {
        playingFootsteps = false;
        _footstepTimer = 0f;
    }

    public void PlayFootstepsSFX()
    {
        if (stepSFX == null) return;

        bool isSprinting = _input.sprint && _input.move.sqrMagnitude > 0.01f;

        AudioClip[] chosenClips = GetFootstepClips(isSprinting);

        if (chosenClips == null || chosenClips.Length == 0)
            return;

        int chosenStep = Random.Range(0, chosenClips.Length);

        stepSFX.pitch = Random.Range(0.9f, 1.15f);
        stepSFX.PlayOneShot(chosenClips[chosenStep]);
    }

    private AudioClip[] GetFootstepClips(bool isSprinting)
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, groundRayDistance, GroundLayers))
        {
            if (hit.collider.CompareTag("Wood"))
            {
                return isSprinting ? woodRunFootstepClips : woodWalkFootstepClips;
            }

            if (hit.collider.CompareTag("Tile"))
            {
                return isSprinting ? tileRunFootstepClips : tileWalkFootstepClips;
            }

            if (hit.collider.CompareTag("Carpet"))
            {
                return isSprinting ? carpetRunFootstepClips : carpetWalkFootstepClips;
            }
        }

        return isSprinting ? defaultRunFootstepClips : defaultWalkFootstepClips;
    }

    private void HandleCameraNoise()
    {
        if (cameraNoise == null) return;
        bool isMoving = _input.move.sqrMagnitude > 0.01f && Grounded;
        bool isSprinting = _input.sprint && isMoving;
        float targetAmplitude = isMoving ? (isSprinting ? sprintNoiseAmplitude : walkNoiseAmplitude) : 0f;
        float targetFrequency = isMoving ? (isSprinting ? sprintNoiseFrequency : walkNoiseFrequency) : 0f;

        cameraNoise.AmplitudeGain = Mathf.Lerp(cameraNoise.AmplitudeGain, targetAmplitude, Time.deltaTime * noiseBlendSpeed);
        cameraNoise.FrequencyGain = Mathf.Lerp(cameraNoise.FrequencyGain, targetFrequency, Time.deltaTime * noiseBlendSpeed);
    }

    private void DetectInteractable()
    {
        if (objectOutline != null)
        {
            objectOutline.enabled = false;
            objectOutline = null;
        }

        currentInteractable = null;
        isInteractable = false;

        Ray ray = new Ray(_mainCamera.transform.position, _mainCamera.transform.forward);
        Debug.DrawRay(ray.origin, ray.direction * interactionRange, Color.red);

        if (Physics.Raycast(ray, out RaycastHit hit, interactionRange, interactionLayer))
        {
            bool hasInteractable = hit.collider.TryGetComponent<IInteractable>(out var interactable);
            bool hasPower = hit.collider.TryGetComponent<IToolPower>(out var powerItem);

            if (hasInteractable || hasPower)
            {
                if (hasInteractable)
                {
                    currentInteractable = interactable;
                    isInteractable = true;
                }

                if (interactText != null)
                {
                    string basePrompt = "";

                    if (hit.collider.TryGetComponent<Door>(out Door door))
                    {
                        basePrompt = door.promptMessage;
                    }
                    else if (hit.collider.TryGetComponent<DoorScript>(out DoorScript doorScript))
                    {
                        basePrompt = doorScript.promptMessage;
                    }
                    else if (hit.collider.TryGetComponent<ToolPickup>(out ToolPickup pickup))
                    {
                        basePrompt = "Pick up";
                    }
                    else if (hit.collider.TryGetComponent<GeneratorPowerSystem>(out GeneratorPowerSystem generator))
                    {
                        basePrompt = generator.promptMessage;
                    }
                    else if (hit.collider.TryGetComponent<NarrativeInspectableItem>(out NarrativeInspectableItem item))
                    {
                        basePrompt = item.InteractionPrompt;
                    }
                    else if (hit.collider.TryGetComponent<ParaffinLampScript>(out ParaffinLampScript paraffinLamp))
                    {
                        basePrompt = "Paraffin Lamp";
                    }
                    else if (hasInteractable)
                    {
                        basePrompt = "Interact";
                    }

                    if (hasPower && powerItem.UsesPower)
                    {
                        int powerPercent = Mathf.FloorToInt((powerItem.CurrentPower / powerItem.MaxPower) * 100f);

                        if (!string.IsNullOrEmpty(basePrompt))
                        {
                            basePrompt = $"{basePrompt} {powerPercent}%";
                        }
                        else
                        {
                            basePrompt = $"Paraffin Lamp {powerPercent}%";
                        }
                    }

                    interactText.text = basePrompt;
                }

                objectOutline = hit.collider.GetComponent<Outline>() ?? hit.collider.GetComponentInChildren<Outline>();

                if (objectOutline != null)
                {
                    bool showCrosshairs = !isInspecting && !isJumpscareActive;
                    objectOutline.enabled = showCrosshairs;
                }
            }
        }
    }

    private void UpdateCrosshair()
    {
        bool showCrosshairs = !isInspecting && !isJumpscareActive;

        bool hasHoverInfo = isInteractable || (objectOutline != null);

        if (Crosshair != null && InteractableCrosshair != null)
        {
            Crosshair.gameObject.SetActive(showCrosshairs && !hasHoverInfo);
            InteractableCrosshair.gameObject.SetActive(showCrosshairs && hasHoverInfo);
        }

        if (interactPrompt != null)
        {
            GameObject input = interactPrompt.transform.GetChild(0).gameObject;

            interactPrompt.SetActive(showCrosshairs && hasHoverInfo);

            if (input != null)
            {
                input.SetActive(showCrosshairs && isInteractable);
            }
        }

        if (objectOutline != null)
        {
            objectOutline.enabled = showCrosshairs && hasHoverInfo;
        }
    }

    public void Interaction()
    {
        if (currentInteractable != null)
        {
            if (_input.interact)
            {
                if (objectOutline != null)
                {
                    objectOutline.enabled = false;
                }

                currentInteractable.Interact();

                if (currentInteractable is not GeneratorPowerSystem)
                {
                    _input.interact = false;
                }
            }
        }
    }

    public void ClearInteractionState()
    {
        GameObject input = interactPrompt.transform.GetChild(0).gameObject;
        currentInteractable = null;
        isInteractable = false;

        if (interactPrompt != null)
        {
            interactPrompt.SetActive(false);
            input.SetActive(false);
        }

        if (objectOutline != null)
        {
            objectOutline.enabled = false;
            objectOutline = null;
        }
    }

    private void JumpAndGravity()
    {
        if (Grounded)
        {
            _fallTimeoutDelta = FallTimeout;

            if (_verticalVelocity < 0.0f)
            {
                _verticalVelocity = -2f;
            }

            if (_input.jump && _jumpTimeoutDelta <= 0.0f)
            {
                _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
            }

            if (_jumpTimeoutDelta >= 0.0f)
            {
                _jumpTimeoutDelta -= Time.deltaTime;
            }
        }
        else
        {
            _jumpTimeoutDelta = JumpTimeout;

            if (_fallTimeoutDelta >= 0.0f)
            {
                _fallTimeoutDelta -= Time.deltaTime;
            }

            _input.jump = false;
        }

        if (_verticalVelocity < _terminalVelocity)
        {
            _verticalVelocity += Gravity * Time.deltaTime;
        }
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }
}