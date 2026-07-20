using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class SimpleTutorialPromptManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private CanvasGroup tutorialCanvasGroup;
    [SerializeField] private TMP_Text tutorialText;

    [Header("Timing")]
    [SerializeField] private float firstPromptDelay = 1f;
    [SerializeField] private float promptDuration = 3f;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float gapBetweenPrompts = 0.3f;

    [Header("Keyboard Prompts")]
    [SerializeField] private string keyboardMoveLookPrompt = "[WASD] to move\n[Mouse] to look";
    [SerializeField] private string keyboardInteractPrompt = "[E] to interact";
    [SerializeField] private string keyboardWheelPrompt = "Hold [TAB] to open tool wheel";

    [Header("Xbox Prompts")]
    [SerializeField] private string xboxMoveLookPrompt = "[Left Stick] to move\n[Right Stick] to look";
    [SerializeField] private string xboxInteractPrompt = "[X] to interact";
    [SerializeField] private string xboxWheelPrompt = "Hold [LB] to open tool wheel";

    [Header("PlayStation Prompts")]
    [SerializeField] private string playStationMoveLookPrompt = "[Left Stick] to move\n[Right Stick] to look";
    [SerializeField] private string playStationInteractPrompt = "[Square] to interact";
    [SerializeField] private string playStationWheelPrompt = "Hold [L1] to open tool wheel";

    [Header("Tutorial Complete Event")]
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
        if (InputDeviceDetector.Instance != null)
        {
            currentDevice = InputDeviceDetector.Instance.CurrentDevice;
            InputDeviceDetector.Instance.OnDeviceChanged += HandleDeviceChanged;
        }
    }

    private void OnDisable()
    {
        if (InputDeviceDetector.Instance != null)
        {
            InputDeviceDetector.Instance.OnDeviceChanged -= HandleDeviceChanged;
        }
    }

    private void Start()
    {
        StartCoroutine(PlayTutorialSequence());
    }

    private IEnumerator PlayTutorialSequence()
    {
        yield return new WaitForSeconds(firstPromptDelay);

        yield return StartCoroutine(ShowPromptRoutine(TutorialPromptType.MoveLook));

        yield return new WaitForSeconds(gapBetweenPrompts);

        yield return StartCoroutine(ShowPromptRoutine(TutorialPromptType.Interact));

        yield return new WaitForSeconds(gapBetweenPrompts);

        yield return StartCoroutine(ShowPromptRoutine(TutorialPromptType.Wheel));

        currentPromptType = TutorialPromptType.None;
        HidePrompt();

        yield return new WaitForSeconds(fadeDuration);

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