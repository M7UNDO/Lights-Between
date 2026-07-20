using UnityEngine;

public class ObjectiveTrigger : MonoBehaviour
{
    [Header("Objective Logic")]
    [SerializeField] private string objectiveToComplete;
    [SerializeField] private string nextObjectiveToStart;

    [Header("Settings")]
    [SerializeField] private bool onlyTriggerOnce = true;

    private bool hasTriggered;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered && onlyTriggerOnce) return;

        bool isPlayer =
            other.CompareTag("Player") ||
            other.GetComponentInParent<PlayerInputHandler>() != null;

        if (!isPlayer) return;

        if (ObjectiveManager.Instance == null)
        {
            Debug.LogWarning("ObjectiveManager not found.");
            return;
        }

        if (!string.IsNullOrEmpty(objectiveToComplete) && !string.IsNullOrEmpty(nextObjectiveToStart))
        {
            ObjectiveManager.Instance.CompleteObjectiveAndStartNext(objectiveToComplete, nextObjectiveToStart);
        }
        else if (!string.IsNullOrEmpty(objectiveToComplete))
        {
            ObjectiveManager.Instance.CompleteObjective(objectiveToComplete);
        }
        else if (!string.IsNullOrEmpty(nextObjectiveToStart))
        {
            ObjectiveManager.Instance.SetObjective(nextObjectiveToStart);
        }

        hasTriggered = true;

        Debug.Log("Objective trigger activated: " + gameObject.name);
    }
}