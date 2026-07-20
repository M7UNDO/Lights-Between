using UnityEngine;

public class ToolPickup : MonoBehaviour, IInteractable
{
    [SerializeField] private ToolClass tool;
    [SerializeField] private string promptMessage = "Pick up";

    public string PromptMessage => promptMessage;

    public void Interact()
    {
        PlayerToolInventory inventory = FindFirstObjectByType<PlayerToolInventory>();

        if (inventory != null)
        {
            inventory.AddTool(tool);
            Destroy(gameObject);
        }
    }
}