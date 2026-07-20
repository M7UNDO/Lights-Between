using UnityEngine;

public class CursorLockOnStart : MonoBehaviour
{
    [SerializeField] private bool lockCursorOnStart = true;

    private void Start()
    {
        if (lockCursorOnStart)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}