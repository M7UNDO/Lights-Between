using UnityEngine;

public class NarrativeInspectableItem : MonoBehaviour, IInteractable
{
    [Header("Item Text")]
    [SerializeField] private string itemName = "Unknown Item";

    [TextArea(3, 8)]
    [SerializeField] private string itemDescription;

    [SerializeField] private string interactionPrompt = "Inspect";

    [Header("Inspection Transform")]
    [SerializeField] private Vector3 inspectionPositionOffset = Vector3.zero;
    [SerializeField] private Vector3 inspectionRotationOffset = Vector3.zero;
    [SerializeField] private Vector3 inspectionScale = Vector3.one;

    private GramophoneController gramophoneController;

    private void Awake()
    {
        gramophoneController = GetComponent<GramophoneController>();
    }

    public string ItemName => itemName;
    public string ItemDescription => itemDescription;

    public string InteractionPrompt
    {
        get
        {
            if (gramophoneController != null && gramophoneController.IsPlayingMusic)
            {
                return gramophoneController.InteractionPrompt;
            }

            return interactionPrompt;
        }
    }

    public Vector3 InspectionPositionOffset => inspectionPositionOffset;
    public Vector3 InspectionRotationOffset => inspectionRotationOffset;
    public Vector3 InspectionScale => inspectionScale;

    public void Interact()
    {
        if (gramophoneController != null && gramophoneController.IsPlayingMusic)
        {
            gramophoneController.Interact();
            return;
        }

        InspectionManager manager = FindFirstObjectByType<InspectionManager>();
        if (manager != null)
        {
            manager.BeginInspection(this);

            if (GameLoopManager.Instance != null)
            {
                GameLoopManager.Instance.RegisterItemInspected(this);
            }
        }
    }
}