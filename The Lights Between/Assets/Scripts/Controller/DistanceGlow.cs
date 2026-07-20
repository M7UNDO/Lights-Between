using UnityEngine;

public class DistanceGlow : MonoBehaviour
{
    [Header("Activation")]
    public bool isGlowEnabled = true;
    [SerializeField] private float glowRadius = 7f;

    [Header("Glow Settings")]
    [SerializeField] private Renderer objectRenderer;
    [SerializeField] private Color glowColor = Color.yellow;
    [SerializeField] private float flashSpeed = 2f;
    [SerializeField] private float minIntensity = 0.0f;
    [SerializeField] private float maxIntensity = 2.0f;

    private Transform playerTransform;
    private MaterialPropertyBlock propertyBlock;
    private int emissionColorId;
    private Color defaultColor;

    private void Awake()
    {
        if (objectRenderer == null)
        {
            objectRenderer = GetComponentInChildren<Renderer>();
        }

        propertyBlock = new MaterialPropertyBlock();
        emissionColorId = Shader.PropertyToID("_EmissionColor");
        defaultColor = Color.black;

        if (objectRenderer != null)
        {
            foreach (var mat in objectRenderer.sharedMaterials)
            {
                mat.EnableKeyword("_EMISSION");
            }
        }

        FPSController player = FindFirstObjectByType<FPSController>();
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    private void Update()
    {
        if (objectRenderer == null || !isGlowEnabled || playerTransform == null)
        {
            ResetGlow();
            return;
        }

        float distance = Vector3.Distance(transform.position, playerTransform.position);

        if (distance <= glowRadius)
        {
            float pingPong = Mathf.PingPong(Time.time * flashSpeed, 1f);
            float currentIntensity = Mathf.Lerp(minIntensity, maxIntensity, pingPong);
            Color finalColor = glowColor * currentIntensity;

            objectRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(emissionColorId, finalColor);
            objectRenderer.SetPropertyBlock(propertyBlock);
        }
        else
        {
            ResetGlow();
        }
    }

    private void ResetGlow()
    {
        if (objectRenderer == null) return;

        objectRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetColor(emissionColorId, defaultColor);
        objectRenderer.SetPropertyBlock(propertyBlock);
    }
}