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

            ToolEquipmentManager equipmentManager = FindFirstObjectByType<ToolEquipmentManager>();
            if (equipmentManager != null && !equipmentManager.HasToolEquipped)
            {
                equipmentManager.EquipTool(tool);
            }

            Destroy(gameObject);
        }
    }
}