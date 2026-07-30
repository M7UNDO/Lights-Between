using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class InputTextPrompt : MonoBehaviour
{
    [SerializeField] private TMP_Text textComponent;

    [Header("Format Settings")]
    [TextArea]
    [SerializeField] private string formatTemplate = "Press {0} to Start";

    [Header("Sprite Names (TMP Sprite Asset)")]
    [SerializeField] private string keyboardSpriteName = "KB_Space";
    [SerializeField] private string xboxSpriteName = "Xbox_A";
    [SerializeField] private string playstationSpriteName = "PS_Cross";

    private void Awake()
    {
        if (textComponent == null)
            textComponent = GetComponent<TMP_Text>();
    }

    private void Start()
    {
        if (InputDeviceDetector.Instance != null)
        {
            UpdatePromptText(InputDeviceDetector.Instance.CurrentDevice);
            InputDeviceDetector.Instance.OnDeviceChanged += UpdatePromptText;
        }
    }

    private void OnDestroy()
    {
        if (InputDeviceDetector.Instance != null)
            InputDeviceDetector.Instance.OnDeviceChanged -= UpdatePromptText;
    }

    private void UpdatePromptText(InputDeviceType deviceType)
    {
        string spriteName = GetSpriteNameForDevice(deviceType);
        string spriteTag = string.IsNullOrEmpty(spriteName) ? string.Empty : $"<sprite name=\"{spriteName}\">";

        textComponent.text = string.Format(formatTemplate, spriteTag);
    }

    private string GetSpriteNameForDevice(InputDeviceType deviceType)
    {
        switch (deviceType)
        {
            case InputDeviceType.KeyboardMouse:
                return keyboardSpriteName;

            case InputDeviceType.Xbox:
                return xboxSpriteName;

            case InputDeviceType.PlayStation:
                return playstationSpriteName;

            default:
                return string.Empty;
        }
    }
}