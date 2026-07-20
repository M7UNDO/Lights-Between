using UnityEngine;

public class ObjectiveToolPickupListener : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerToolInventory inventory;
    [SerializeField] private ToolClass requiredTool;

    [Header("Objective Logic")]
    [SerializeField] private string objectiveToComplete;
    [SerializeField] private string nextObjectiveToStart;

    [Header("Settings")]
    [SerializeField] private bool onlyCompleteOnce = true;

    private bool hasCompleted;

    private void Update()
    {
        if (hasCompleted && onlyCompleteOnce) return;
        if (inventory == null || requiredTool == null) return;

        if (inventory.HasTool(requiredTool))
        {
            CompleteObjective();
        }
    }

    private void CompleteObjective()
    {
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

        hasCompleted = true;

        Debug.Log("Tool objective completed: " + objectiveToComplete);
    }
}