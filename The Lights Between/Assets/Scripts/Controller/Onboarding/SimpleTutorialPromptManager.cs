using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class SimpleTutorialPromptManager : MonoBehaviour
{
    public static bool IsTutorialActive { get; private set; }
    public static bool HasCompletedTutorialOnce { get; private set; }

    public static event Action OnTutorialStarted;
    public static event Action OnTutorialEnded;

    [Header("UI References")]
    public CanvasGroup tutorialCanvasGroup;
    public TMP_Text tutorialText;

    [Header("Timing")]
    [SerializeField] private float firstPromptDelay = 1f;
    [SerializeField] private float promptDuration = 3.5f;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float gapBetweenPrompts = 0.3f;

    [Header("Keyboard Prompts (Text)")]
    [TextArea]
    [SerializeField] private string keyboardMoveLookPrompt = "[WASD] Move\n[Mouse] Look";
    [TextArea]
    [SerializeField] private string keyboardInteractPrompt = "[E] Interact";
    [TextArea]
    [SerializeField] private string keyboardWheelPrompt = "Hold [TAB] + Move [Mouse]\nRelease [TAB] to equip item";

    [Header("Xbox Prompts (Sprites)")]
    [TextArea]
    [SerializeField] private string xboxMoveLookPrompt = "<sprite name=\"Left Stick\"> Move\n<sprite name=\"Right Stick\"> Look";
    [TextArea]
    [SerializeField] private string xboxInteractPrompt = "<sprite name=\"X\"> Interact";
    [TextArea]
    [SerializeField] private string xboxWheelPrompt = "Hold <sprite name=\"Left Bumper\"> + Move <sprite name=\"Right Stick\">\nRelease <sprite name=\"Left Bumper\"> to equip item";

    [Header("PlayStation Prompts (Sprites)")]
    [TextArea]
    [SerializeField] private string playStationMoveLookPrompt = "<sprite name=\"_Left Stick\"> Move\n<sprite name=\"_Right Stick\"> Look";
    [TextArea]
    [SerializeField] private string playStationInteractPrompt = "<sprite name=\"Square\"> Interact";
    [TextArea]
    [SerializeField] private string playStationWheelPrompt = "Hold <sprite name=\"L1\"> + Move <sprite name=\"_Right Stick\">\nRelease <sprite name=\"L1\"> to equip item";
    [Header("Tutorial Events")]
    [SerializeField] private UnityEvent onTutorialStart;
    [SerializeField] private UnityEvent onTutorialComplete;

    private InputDeviceType currentDevice = InputDeviceType.KeyboardMouse;
    private Coroutine fadeRoutine;

    private enum TutorialPromptType
    {
        None,
        MoveLook,
        Interact,
        Wheel
    }

    private TutorialPromptType currentPromptType = TutorialPromptType.None;

    public static void ResetTutorialSession()
    {
        HasCompletedTutorialOnce = false;
        IsTutorialActive = false;
    }

    private void Awake()
    {
        if (tutorialCanvasGroup != null)
        {
            tutorialCanvasGroup.alpha = 0f;
        }

        if (tutorialText != null)
        {
            tutorialText.text = "";
        }
    }

    private void OnEnable()
    {
        SubscribeToInputDetector();
    }

    private void OnDisable()
    {
        UnsubscribeFromInputDetector();
    }

    private void Start()
    {
        if (HasCompletedTutorialOnce)
        {
            IsTutorialActive = false;
            onTutorialComplete?.Invoke();
            return;
        }

        SubscribeToInputDetector();
        StartCoroutine(PlayTutorialSequence());
    }

    private void SubscribeToInputDetector()
    {
        if (InputDeviceDetector.Instance != null)
        {
            InputDeviceDetector.Instance.OnDeviceChanged -= HandleDeviceChanged;
            InputDeviceDetector.Instance.OnDeviceChanged += HandleDeviceChanged;
            currentDevice = InputDeviceDetector.Instance.CurrentDevice;
        }
    }

    private void UnsubscribeFromInputDetector()
    {
        if (InputDeviceDetector.Instance != null)
        {
            InputDeviceDetector.Instance.OnDeviceChanged -= HandleDeviceChanged;
        }
    }

    private IEnumerator PlayTutorialSequence()
    {
        IsTutorialActive = true;
        OnTutorialStarted?.Invoke();
        onTutorialStart?.Invoke();

        yield return new WaitForSeconds(firstPromptDelay);

        yield return StartCoroutine(ShowPromptRoutine(TutorialPromptType.MoveLook));

        yield return new WaitForSeconds(gapBetweenPrompts);

        yield return StartCoroutine(ShowPromptRoutine(TutorialPromptType.Interact));

        yield return new WaitForSeconds(gapBetweenPrompts);

        yield return StartCoroutine(ShowPromptRoutine(TutorialPromptType.Wheel));

        currentPromptType = TutorialPromptType.None;
        HidePrompt();

        yield return new WaitForSeconds(fadeDuration);

        IsTutorialActive = false;
        HasCompletedTutorialOnce = true;
        OnTutorialEnded?.Invoke();
        onTutorialComplete?.Invoke();
    }

    private IEnumerator ShowPromptRoutine(TutorialPromptType promptType)
    {
        currentPromptType = promptType;

        ShowPrompt(GetPromptText(promptType));

        yield return new WaitForSeconds(promptDuration);

        HidePrompt();

        yield return new WaitForSeconds(fadeDuration);
    }

    private void ShowPrompt(string message)
    {
        if (tutorialText != null)
        {
            tutorialText.text = message;
        }

        FadePrompt(1f);
    }

    private void HidePrompt()
    {
        FadePrompt(0f);
    }

    private void FadePrompt(float targetAlpha)
    {
        if (tutorialCanvasGroup == null) return;

        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }

        fadeRoutine = StartCoroutine(FadeCanvasGroup(targetAlpha));
    }

    private IEnumerator FadeCanvasGroup(float targetAlpha)
    {
        float startAlpha = tutorialCanvasGroup.alpha;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;

            float t = timer / fadeDuration;
            tutorialCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);

            yield return null;
        }

        tutorialCanvasGroup.alpha = targetAlpha;
    }

    private void HandleDeviceChanged(InputDeviceType newDevice)
    {
        currentDevice = newDevice;

        if (tutorialText == null) return;
        if (currentPromptType == TutorialPromptType.None) return;

        tutorialText.text = GetPromptText(currentPromptType);
    }

    private string GetPromptText(TutorialPromptType promptType)
    {
        switch (promptType)
        {
            case TutorialPromptType.MoveLook:
                return GetMoveLookPrompt();

            case TutorialPromptType.Interact:
                return GetInteractPrompt();

            case TutorialPromptType.Wheel:
                return GetWheelPrompt();

            default:
                return string.Empty;
        }
    }

    private string GetMoveLookPrompt()
    {
        switch (currentDevice)
        {
            case InputDeviceType.Xbox:
                return xboxMoveLookPrompt;

            case InputDeviceType.PlayStation:
                return playStationMoveLookPrompt;

            default:
                return keyboardMoveLookPrompt;
        }
    }

    private string GetInteractPrompt()
    {
        switch (currentDevice)
        {
            case InputDeviceType.Xbox:
                return xboxInteractPrompt;

            case InputDeviceType.PlayStation:
                return playStationInteractPrompt;

            default:
                return keyboardInteractPrompt;
        }
    }

    private string GetWheelPrompt()
    {
        switch (currentDevice)
        {
            case InputDeviceType.Xbox:
                return xboxWheelPrompt;

            case InputDeviceType.PlayStation:
                return playStationWheelPrompt;

            default:
                return keyboardWheelPrompt;
        }
    }
}