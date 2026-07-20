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

    public float CurrentPower => currentPower;
    public float MaxPower => maxPower;
    public bool UsesPower => toolData != null && toolData.usesPower;

    public void Initialise(ToolClass tool)
    {
        toolData = tool;

        maxPower = tool.maxPower;
        currentPower = maxPower;

        isEquipped = true;
        isOn = false;

        SetupFlashlightAnchor();
        SpawnTorchLight();
        SpawnPlayerLightZone();

        if (glassRenderer != null)
        {
            emissionStart = glassRenderer.material.GetColor("_EmissionColor");
            OffEmission();
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

        torchLight.enabled = false;

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

        spawnedPlayerLightZone.SetActive(false);
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

        torchLight.enabled = isOn;

        if (torchLightZone != null)
        {
            torchLightZone.SetActive(isOn);
        }

        if (spawnedPlayerLightZone != null)
        {
            spawnedPlayerLightZone.SetActive(isOn);
        }

        if (!isOn)
        {
            OffEmission();
        }
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
    }

    private void OnEnable()
    {
        if (isOn && currentPower > 0f)
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
    }

    private void OnDisable()
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

    private void OnDestroy()
    {
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