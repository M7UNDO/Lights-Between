using System.Collections;
using TMPro;
using UnityEngine;

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager Instance;

    [System.Serializable]
    public class Objective
    {
        public string objectiveID;

        [TextArea(2, 4)]
        public string objectiveText;

        public bool completed;
    }

    [Header("Objectives")]
    [SerializeField] private Objective[] objectives;

    [Header("UI References")]
    [SerializeField] private CanvasGroup objectiveCanvasGroup;
    [SerializeField] private TMP_Text objectiveText;

    [Header("Timing")]
    [SerializeField] private float firstObjectiveDelay = 4f;
    [SerializeField] private float objectiveDisplayTime = 5f;
    [SerializeField] private float fadeDuration = 0.5f;

    [Header("First Objective")]
    [SerializeField] private string firstObjectiveID = "InvestigateLight";

    private Coroutine displayRoutine;
    private Coroutine fadeRoutine;
    private Objective currentObjective;
    private bool objectiveSequenceStarted;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (objectiveCanvasGroup != null)
        {
            objectiveCanvasGroup.alpha = 0f;
        }

        if (objectiveText != null)
        {
            objectiveText.text = "";
        }
    }

    public void StartObjectiveSequence()
    {
        if (objectiveSequenceStarted) return;

        objectiveSequenceStarted = true;
        StartCoroutine(StartFirstObjectiveAfterDelay());
    }

    private IEnumerator StartFirstObjectiveAfterDelay()
    {
        yield return new WaitForSeconds(firstObjectiveDelay);

        SetObjective(firstObjectiveID);
    }

    public void SetFirstObjective(string objectiveID)
    {
        Objective objective = GetObjectiveByID(objectiveID);

        if (objective == null)
        {
            Debug.LogWarning("Objective not found: " + objectiveID);
            return;
        }

        if (objective.completed)
        {
            return;
        }

        currentObjective = objective;
    }
    public void SetObjective(string objectiveID)
    {
        Objective objective = GetObjectiveByID(objectiveID);

        if (objective == null)
        {
            Debug.LogWarning("Objective not found: " + objectiveID);
            return;
        }

        if (objective.completed)
        {
            return;
        }

        currentObjective = objective;

        ShowObjective(objective.objectiveText);

        //Debug.Log("Objective started: " + objective.objectiveID);
    }

    public void CompleteObjective(string objectiveID)
    {
        Objective objective = GetObjectiveByID(objectiveID);

        if (objective == null)
        {
            Debug.LogWarning("Objective not found: " + objectiveID);
            return;
        }

        if (objective.completed)
        {
            return;
        }

        objective.completed = true;

        if (currentObjective == objective)
        {
            HideObjective();
            currentObjective = null;
        }

        Debug.Log("Objective completed: " + objective.objectiveID);
    }

    public void CompleteObjectiveAndStartNext(string completedObjectiveID, string nextObjectiveID)
    {
        CompleteObjective(completedObjectiveID);

        if (!string.IsNullOrEmpty(nextObjectiveID))
        {
            SetObjective(nextObjectiveID);
        }
    }

    private Objective GetObjectiveByID(string objectiveID)
    {
        if (objectives == null) return null;

        foreach (Objective objective in objectives)
        {
            if (objective != null && objective.objectiveID == objectiveID)
            {
                return objective;
            }
        }

        return null;
    }

    private void ShowObjective(string message)
    {
        if (objectiveText != null)
        {
            objectiveText.text = message;
        }

        if (displayRoutine != null)
        {
            StopCoroutine(displayRoutine);
        }

        displayRoutine = StartCoroutine(DisplayObjectiveRoutine());
    }

    private IEnumerator DisplayObjectiveRoutine()
    {
        FadeObjective(1f);

        yield return new WaitForSeconds(objectiveDisplayTime);

        FadeObjective(0f);
    }

    private void HideObjective()
    {
        if (displayRoutine != null)
        {
            StopCoroutine(displayRoutine);
            displayRoutine = null;
        }

        FadeObjective(0f);
    }

    private void FadeObjective(float targetAlpha)
    {
        if (objectiveCanvasGroup == null) return;

        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }

        fadeRoutine = StartCoroutine(FadeCanvasGroup(targetAlpha));
    }

    private IEnumerator FadeCanvasGroup(float targetAlpha)
    {
        float startAlpha = objectiveCanvasGroup.alpha;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;

            float t = timer / fadeDuration;
            objectiveCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);

            yield return null;
        }

        objectiveCanvasGroup.alpha = targetAlpha;
    }
}