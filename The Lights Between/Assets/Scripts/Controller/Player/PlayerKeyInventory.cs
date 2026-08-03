using System.Collections.Generic;
using UnityEngine;

public class PlayerKeyInventory : MonoBehaviour, IKeyInventory
{
    [SerializeField] private List<string> keys = new List<string>();

    public bool HasKey(string keyID)
    {
        return string.IsNullOrEmpty(keyID) || keys.Contains(keyID);
    }

    public bool ConsumeKey(string keyID)
    {
        if (HasKey(keyID))
        {
            keys.Remove(keyID);
            return true;
        }
        return false;
    }

    public void AddKey(string keyID)
    {
        if (!keys.Contains(keyID))
        {
            keys.Add(keyID);
        }
    }
}