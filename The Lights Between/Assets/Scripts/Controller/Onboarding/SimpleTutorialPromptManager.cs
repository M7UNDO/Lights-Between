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

    [Header("Keyboard Prompts")]
    [SerializeField] private string keyboardMoveLookPrompt = "[WASD] Move\n[Mouse] Look";
    [SerializeField] private string keyboardInteractPrompt = "[E] Interact";
    [SerializeField] private string keyboardWheelPrompt = "Hold [TAB] + Move [Mouse]\nRelease [TAB] to equip item";

    [Header("Xbox Prompts")]
    [SerializeField] private string xboxMoveLookPrompt = "[Left Stick] Move\n[Right Stick] Look";
    [SerializeField] private string xboxInteractPrompt = "[X] Interact";
    [SerializeField] private string xboxWheelPrompt = "Hold [LB] + Move [Right Stick]\nRelease [LB] to equip item";

    [Header("PlayStation Prompts")]
    [SerializeField] private string playStationMoveLookPrompt = "[Left Stick] Move\n[Right Stick] Look";
    [SerializeField] private string playStationInteractPrompt = "[Square] Interact";
    [SerializeField] private string playStationWheelPrompt = "Hold [L1] + Move [Right Stick]\nRelease [L1] to equip item";

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
        if (promptType == TutorialPromptType.MoveLook)
        {
            return GetMoveLookPrompt();
        }

        if (promptType == TutorialPromptType.Interact)
        {
            return GetInteractPrompt();
        }

        if (promptType == TutorialPromptType.Wheel)
        {
            return GetWheelPrompt();
        }

        return "";
    }

    private string GetMoveLookPrompt()
    {
        if (currentDevice == InputDeviceType.Xbox)
        {
            return xboxMoveLookPrompt;
        }

        if (currentDevice == InputDeviceType.PlayStation)
        {
            return playStationMoveLookPrompt;
        }

        return keyboardMoveLookPrompt;
    }

    private string GetInteractPrompt()
    {
        if (currentDevice == InputDeviceType.Xbox)
        {
            return xboxInteractPrompt;
        }

        if (currentDevice == InputDeviceType.PlayStation)
        {
            return playStationInteractPrompt;
        }

        return keyboardInteractPrompt;
    }

    private string GetWheelPrompt()
    {
        if (currentDevice == InputDeviceType.Xbox)
        {
            return xboxWheelPrompt;
        }

        if (currentDevice == InputDeviceType.PlayStation)
        {
            return playStationWheelPrompt;
        }

        return keyboardWheelPrompt;
    }
}