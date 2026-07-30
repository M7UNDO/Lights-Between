using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public static bool isGamePaused { get; private set; } = false;
    private bool toggle;

    [Header("Pause UI")]

    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject settingsPanel;

    
    public void TogglePause()
    {
        toggle = !toggle;

        if (toggle)
        {
            isGamePaused = true;
        }
        else
        {
            isGamePaused = false;
        }
    }

    public void Back()
    {
        if(settingsPanel != null)
        {

        }
    }
}
