using TMPro;
using UnityEngine;

public class EquippedToolStatusUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ToolEquipmentManager equipmentManager;
    [SerializeField] private TextMeshProUGUI equippedToolText;

    private void Update()
    {
        if (equipmentManager == null || equippedToolText == null) return;

        ToolClass currentTool = equipmentManager.CurrentTool;

        if (currentTool == null)
        {
            equippedToolText.text = "No Tool Equipped";
            return;
        }

        IToolPower toolPower = null;

        if (equipmentManager.CurrentToolObject != null)
        {
            toolPower = equipmentManager.CurrentToolObject.GetComponent<IToolPower>();

            if (toolPower == null)
            {
                toolPower = equipmentManager.CurrentToolObject.GetComponentInChildren<IToolPower>();
            }
        }

        if (toolPower != null && toolPower.UsesPower && toolPower.MaxPower > 0f)
        {
            float percentage = (toolPower.CurrentPower / toolPower.MaxPower) * 100f;
            equippedToolText.text = currentTool.toolName + " - " + Mathf.RoundToInt(percentage) + "%";
        }
        else if (currentTool.usesPower && currentTool.maxPower > 0f)
        {
            equippedToolText.text = currentTool.toolName + " - 100%";
        }
        else
        {
            equippedToolText.text = currentTool.toolName;
        }
    }
}