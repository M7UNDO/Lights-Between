using System.Collections;
using TMPro;
using UnityEngine;

public class FlashlightScript : MonoBehaviour, IToolPower
{
    [Header("References")]
    [SerializeField] private GameObject torchLightPrefab;
    [SerializeField] private AudioSource torchClickSFX;
    [SerializeField] private GameObject playerLightZonePrefab;

    [Header("Visuals")]
    [SerializeField] private Texture lightCookie;
    [SerializeField] private Renderer glassRenderer;

    [Header("Drain Settings")]
    [SerializeField] private float drainRate = 5f;

    [Header("First Equip Prompt Settings")]
    [SerializeField] private float promptDuration = 4f;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private string keyboardTogglePrompt = "[Left Click] to toggle On/Off";
    [SerializeField] private string xboxTogglePrompt = "Press [RB] to toggle On/Off";
    [SerializeField] private string playStationTogglePrompt = "Press [RB] to toggle On/Off";

    private ToolClass toolData;
    private float currentPower;
    private float maxPower;

    private Transform flashlightAnchor;
    private Light torchLight;
    private GameObject spawnedLightObject;
    private GameObject spawnedPlayerLightZone;
    public GameObject torchLightZone;

    private bool isOn;
    private bool isEquipped;
    private Color emissionStart;

    private static bool hasShownTorchPromptOnce = false;

    private Coroutine promptCoroutine;
    private InputDeviceType currentDevice = InputDeviceType.KeyboardMouse;
    private TMP_Text activePromptText;
    private CanvasGroup activePromptCanvasGroup;

    public float CurrentPower => currentPower;
    public float MaxPower => maxPower;
    public bool UsesPower => toolData != null && toolData.usesPower;

    public void Initialise(ToolClass tool)
    {
        toolData = tool;

        maxPower = tool.maxPower;
        currentPower = maxPower;

        isEquipped = true;
        isOn = true;

        SetupFlashlightAnchor();
        SpawnTorchLight();
        SpawnPlayerLightZone();

        if (glassRenderer != null)
        {
            emissionStart = glassRenderer.material.GetColor("_EmissionColor");
        }

        TurnOnLightState();

        if (!hasShownTorchPromptOnce)
        {
            hasShownTorchPromptOnce = true;
            ShowFirstTimePrompt();
        }
    }

    private void SetupFlashlightAnchor()
    {
        GameObject anchorObject = GameObject.Find("PlayerCameraRoot");

        if (anchorObject != null)
        {
            flashlightAnchor = anchorObject.transform;
        }
        else
        {
            Debug.LogWarning("PlayerCameraRoot was not found. Torch light will stay on prefab.");
        }
    }

    private void SpawnTorchLight()
    {
        if (torchLightPrefab == null || flashlightAnchor == null) return;

        spawnedLightObject = Instantiate(torchLightPrefab, flashlightAnchor);
        spawnedLightObject.transform.localPosition = Vector3.zero;
        spawnedLightObject.transform.localRotation = Quaternion.identity;

        torchLight = spawnedLightObject.GetComponentInChildren<Light>();

        if (torchLight == null)
        {
            Debug.LogWarning("Torch Light Prefab does not contain a Light component.");
            return;
        }

        if (lightCookie != null)
        {
            torchLight.cookie = lightCookie;
        }
    }

    private void SpawnPlayerLightZone()
    {
        if (playerLightZonePrefab == null || flashlightAnchor == null) return;

        Transform playerTransform = flashlightAnchor.root;

        spawnedPlayerLightZone = Instantiate(playerLightZonePrefab, playerTransform);
        spawnedPlayerLightZone.transform.localPosition = new Vector3(0f, 1f, 0f);
        spawnedPlayerLightZone.transform.localRotation = Quaternion.identity;
    }

    private void ShowFirstTimePrompt()
    {
        SimpleTutorialPromptManager promptManager = FindFirstObjectByType<SimpleTutorialPromptManager>();
        if (promptManager != null)
        {
            activePromptText = promptManager.tutorialText;
            activePromptCanvasGroup = promptManager.tutorialCanvasGroup;
        }

        if (InputDeviceDetector.Instance != null)
        {
            currentDevice = InputDeviceDetector.Instance.CurrentDevice;
            InputDeviceDetector.Instance.OnDeviceChanged += HandleDeviceChanged;
        }

        if (promptCoroutine != null)
        {
            StopCoroutine(promptCoroutine);
        }

        promptCoroutine = StartCoroutine(DisplayPromptRoutine());
    }

    private IEnumerator DisplayPromptRoutine()
    {
        if (activePromptText != null && activePromptCanvasGroup != null)
        {
            activePromptText.text = GetDevicePromptString();

            float timer = 0f;
            float startAlpha = activePromptCanvasGroup.alpha;

            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                activePromptCanvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, timer / fadeDuration);
                yield return null;
            }

            activePromptCanvasGroup.alpha = 1f;

            yield return new WaitForSeconds(promptDuration);

            timer = 0f;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                activePromptCanvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
                yield return null;
            }

            activePromptCanvasGroup.alpha = 0f;
            activePromptText.text = "";
        }

        UnsubscribeDeviceDetector();
    }

    private void HandleDeviceChanged(InputDeviceType newDevice)
    {
        currentDevice = newDevice;
        if (activePromptText != null)
        {
            activePromptText.text = GetDevicePromptString();
        }
    }

    private string GetDevicePromptString()
    {
        if (currentDevice == InputDeviceType.Xbox)
        {
            return xboxTogglePrompt;
        }

        if (currentDevice == InputDeviceType.PlayStation)
        {
            return playStationTogglePrompt;
        }

        return keyboardTogglePrompt;
    }

    private void UnsubscribeDeviceDetector()
    {
        if (InputDeviceDetector.Instance != null)
        {
            InputDeviceDetector.Instance.OnDeviceChanged -= HandleDeviceChanged;
        }
    }

    public void Toggle()
    {
        if (!isEquipped) return;
        if (currentPower <= 0f) return;
        if (torchLight == null) return;

        isOn = !isOn;

        if (torchClickSFX != null)
        {
            torchClickSFX.Play();
        }

        if (isOn)
        {
            TurnOnLightState();
        }
        else
        {
            TurnOffLightState();
        }
    }

    private void TurnOnLightState()
    {
        if (torchLight != null)
        {
            torchLight.enabled = true;
        }

        if (torchLightZone != null)
        {
            torchLightZone.SetActive(true);
        }

        if (spawnedPlayerLightZone != null)
        {
            spawnedPlayerLightZone.SetActive(true);
        }

        UpdateEmission();
    }

    private void TurnOffLightState()
    {
        if (torchLight != null)
        {
            torchLight.enabled = false;
        }

        if (torchLightZone != null)
        {
            torchLightZone.SetActive(false);
        }

        if (spawnedPlayerLightZone != null)
        {
            spawnedPlayerLightZone.SetActive(false);
        }

        OffEmission();
    }

    private void Update()
    {
        if (!isEquipped) return;
        if (!isOn) return;

        currentPower -= drainRate * Time.deltaTime;
        currentPower = Mathf.Clamp(currentPower, 0f, maxPower);

        UpdateEmission();

        if (currentPower <= 0f)
        {
            isOn = false;
            TurnOffLightState();
        }
    }

    private void OnEnable()
    {
        if (isOn && currentPower > 0f)
        {
            TurnOnLightState();
        }
    }

    private void OnDisable()
    {
        TurnOffLightState();
        UnsubscribeDeviceDetector();
    }

    private void OnDestroy()
    {
        UnsubscribeDeviceDetector();

        if (spawnedLightObject != null)
        {
            Destroy(spawnedLightObject);
        }

        if (spawnedPlayerLightZone != null)
        {
            Destroy(spawnedPlayerLightZone);
        }
    }

    private void UpdateEmission()
    {
        if (glassRenderer == null) return;

        float intensity = currentPower / maxPower;
        glassRenderer.material.SetColor("_EmissionColor", emissionStart * intensity);
    }

    private void OffEmission()
    {
        if (glassRenderer == null) return;

        glassRenderer.material.SetColor("_EmissionColor", Color.black);
    }
}