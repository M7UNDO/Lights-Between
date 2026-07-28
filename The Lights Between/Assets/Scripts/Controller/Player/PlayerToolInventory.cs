using System.Collections.Generic;
using UnityEngine;

public class PlayerToolInventory : MonoBehaviour
{
    [SerializeField] private List<ToolClass> acquiredTools = new List<ToolClass>();

    public List<ToolClass> AcquiredTools => acquiredTools;

    public void AddTool(ToolClass tool)
    {
        if (tool == null) return;

        if (!acquiredTools.Contains(tool))
        {
            acquiredTools.Add(tool);
            Debug.Log("Acquired tool: " + tool.toolName);
        }
    }

    public void RemoveTool(ToolClass tool)
    {
        if (tool == null) return;

        if (acquiredTools.Contains(tool))
        {
            acquiredTools.Remove(tool);
            Debug.Log("Removed tool: " + tool.toolName);
        }
    }

    public bool HasTool(ToolClass tool)
    {
        return acquiredTools.Contains(tool);
    }
}