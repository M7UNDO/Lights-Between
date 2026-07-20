using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class IntroDialogueController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference skipAction;

    [Header("Text References")]
    [SerializeField] private TMP_Text dateText;
    [SerializeField] private TMP_Text locationText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private TMP_Text ellipsisText;

    [Header("Dialogue")]
    [TextArea(2, 4)]
    [SerializeField] private string dateLine = "11 August 2004\n11:13 AM";
    [TextArea(2, 4)]
    [SerializeField] private string locationLine = "Morrow Family Farm";
    [TextArea(2, 4)]
    [SerializeField] private string[] playerThoughts;

    [Header("Typing Settings")]
    [SerializeField] private float typeSpeed = 0.04f;
    [SerializeField] private float dateHoldTime = 5f;
    [SerializeField] private float locationHoldTime = 3f;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float ellipsisFlashSpeed = 0.5f;

    [Header("Scene Transition")]
    [SerializeField] private LevelLoader levelLoader;
    [SerializeField] private int levelSceneIndex = 1;

    [Header("Canvas Groups")]
    [SerializeField] private CanvasGroup dateCanvasGroup;
    [SerializeField] private CanvasGroup locationCanvasGroup;
    [SerializeField] private CanvasGroup dialogueCanvasGroup;

    private int currentThoughtIndex;
    private bool isTyping;
    private bool canAdvance;
    private Coroutine ellipsisFlashCoroutine;
    private WaitForSeconds typeSpeedWait;

    private void Awake()
    {
        dateCanvasGroup = GetOrAddCanvasGroup(dateText, dateCanvasGroup);
        locationCanvasGroup = GetOrAddCanvasGroup(locationText, locationCanvasGroup);
        dialogueCanvasGroup = GetOrAddCanvasGroup(dialogueText, dialogueCanvasGroup);

        typeSpeedWait = new WaitForSeconds(typeSpeed);
    }

    private void OnEnable()
    {
        if (skipAction != null)
        {
            skipAction.action.Enable();
            skipAction.action.performed += OnSkipPerformed;
        }
    }

    private void OnDisable()
    {
        if (skipAction != null)
        {
            skipAction.action.performed -= OnSkipPerformed;
            skipAction.action.Disable();
        }
    }

    private void Start()
    {
        ClearText(dateText);
        ClearText(locationText);
        ClearText(dialogueText);

        if (ellipsisText != null)
        {
            ellipsisText.gameObject.SetActive(false);
        }

        StartCoroutine(PlayIntroSequence());
    }

    private CanvasGroup GetOrAddCanvasGroup(TMP_Text textComponent, CanvasGroup existingGroup)
    {
        if (existingGroup != null) return existingGroup;
        if (textComponent == null) return null;
        if (textComponent.TryGetComponent<CanvasGroup>(out var group)) return group;
        return textComponent.gameObject.AddComponent<CanvasGroup>();
    }

    private void ClearText(TMP_Text textComponent)
    {
        if (textComponent != null)
        {
            textComponent.text = string.Empty;
            textComponent.maxVisibleCharacters = 0;
        }
    }

    private void OnSkipPerformed(InputAction.CallbackContext context)
    {
        if (!canAdvance || isTyping) return;
        ShowNextThought();
    }

    private IEnumerator PlayIntroSequence()
    {
        ResetCanvasGroup(dateCanvasGroup);
        ResetCanvasGroup(locationCanvasGroup);
        ResetCanvasGroup(dialogueCanvasGroup);

        dateText.gameObject.SetActive(true);
        locationText.gameObject.SetActive(false);

        yield return StartCoroutine(TypeText(dateText, dateLine));
        yield return new WaitForSeconds(dateHoldTime);
        yield return StartCoroutine(FadeCanvasGroup(dateCanvasGroup, 1f, 0f));

        dateText.gameObject.SetActive(false);
        locationText.gameObject.SetActive(true);
        locationCanvasGroup.alpha = 1f;

        yield return StartCoroutine(TypeText(locationText, locationLine));
        yield return new WaitForSeconds(locationHoldTime);
        yield return StartCoroutine(FadeCanvasGroup(locationCanvasGroup, 1f, 0f));

        locationText.gameObject.SetActive(false);

        canAdvance = true;
        ShowNextThought();
    }

    private void ResetCanvasGroup(CanvasGroup group)
    {
        if (group != null) group.alpha = 1f;
    }

    private void ShowNextThought()
    {
        if (currentThoughtIndex >= playerThoughts.Length)
        {
            StartCoroutine(LoadNextScene());
            return;
        }

        StartCoroutine(TypeThought(playerThoughts[currentThoughtIndex]));
        currentThoughtIndex++;
    }

    private IEnumerator TypeThought(string thought)
    {
        canAdvance = false;

        if (ellipsisFlashCoroutine != null)
        {
            StopCoroutine(ellipsisFlashCoroutine);
        }

        ellipsisText.gameObject.SetActive(false);
        yield return StartCoroutine(TypeText(dialogueText, thought));

        ellipsisText.gameObject.SetActive(true);
        ellipsisFlashCoroutine = StartCoroutine(FlashEllipsis());

        canAdvance = true;
    }

    private IEnumerator FlashEllipsis()
    {
        var wait = new WaitForSeconds(ellipsisFlashSpeed);

        while (true)
        {
            ellipsisText.enabled = true;
            yield return wait;

            ellipsisText.enabled = false;
            yield return wait;
        }
    }

    private IEnumerator TypeText(TMP_Text targetText, string fullText)
    {
        isTyping = true;

        targetText.text = fullText;
        targetText.maxVisibleCharacters = 0;
        targetText.ForceMeshUpdate();

        int totalCharacters = targetText.textInfo.characterCount;

        for (int i = 0; i <= totalCharacters; i++)
        {
            targetText.maxVisibleCharacters = i;
            yield return typeSpeedWait;
        }

        isTyping = false;
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float startAlpha, float endAlpha)
    {
        if (canvasGroup == null) yield break;

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = timer / fadeDuration;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            yield return null;
        }

        canvasGroup.alpha = endAlpha;
    }

    private IEnumerator LoadNextScene()
    {
        canAdvance = false;

        if (ellipsisFlashCoroutine != null)
        {
            StopCoroutine(ellipsisFlashCoroutine);
        }

        ellipsisText.gameObject.SetActive(false);

        yield return StartCoroutine(FadeCanvasGroup(dialogueCanvasGroup, 1f, 0f));

        if (levelLoader != null)
        {
            levelLoader.LoadLevelInt(levelSceneIndex);
        }
    }
}