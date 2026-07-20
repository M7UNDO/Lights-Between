using UnityEngine;

public class StartingObjective : MonoBehaviour
{
    [SerializeField] private string startingObjective = "Investigate the light source";

    private void Start()
    {
        if (ObjectiveManager.Instance != null)
        {
            ObjectiveManager.Instance.SetObjective(startingObjective);
        }
    }
}