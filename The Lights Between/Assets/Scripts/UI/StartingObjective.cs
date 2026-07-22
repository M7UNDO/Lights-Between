using UnityEngine;

public class StartingObjective : MonoBehaviour
{
    [SerializeField] private string startingObjectiveID = "InvestigateLight";

    private void Start()
    {
        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.SetFirstObjective(startingObjectiveID);
        }
    }
}