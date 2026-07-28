using UnityEngine;

public class TestObject : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        Debug.Log("Interacted with Test Object");
    }
}
