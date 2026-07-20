using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenuUI : MonoBehaviour
{
    // Globally accessible flag that other scripts (like the FPS Controller) can read
    public static bool IsPaused { get; private set; }

    [Header("Input Options")]
    [Tooltip("The same Escape/Pause input action asset reference used across your UI layout.")]
    [SerializeField] private InputActionReference pauseAndBackInput;

    [Header("Pause Panels")]
    [SerializeField] private GameObject pauseCanvas;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("First Selected UI Items")]
    [SerializeField] private GameObject pauseFirstSelected;
    [SerializeField] private GameObject settingsFirstSelected;

    [Header("Scene Settings")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private GameObject lastSelectedBeforeSubMenu;
    private Coroutine selectCoroutine;

    private void Start()
    {
        // Ensure everything starts clean, running, and menus are hidden
        ForceResume();
    }

    private void OnEnable()
    {
        if (pauseAndBackInput != null)
        {
            pauseAndBackInput.action.Enable();
            pauseAndBackInput.action.performed += OnPauseOrBackPressed;
        }
    }

    private void OnDisable()
    {
        if (pauseAndBackInput != null)
        {
            pauseAndBackInput.action.performed -= OnPauseOrBackPressed;
            pauseAndBackInput.action.Disable();
        }
    }

    /// <summary>
    /// Handles the unified Escape button logic smoothly by peeling back UI layers one by one.
    /// </summary>
    private void OnPauseOrBackPressed(InputAction.CallbackContext context)
    {
        // 1. If we are in the settings panel, step back to the pause panel and STOP.
        if (settingsPanel != null && settingsPanel.activeSelf)
        {
            CloseSettings();
            return;
        }

        // 2. If we are already paused on the main pause screen, resume the game.
        if (IsPaused)
        {
            ResumeGame();
        }
        // 3. If the game is running normally, open the pause menu.
        else
        {
            PauseGame();
        }
    }

    #region Pause Core Flow

    public void PauseGame()
    {
        IsPaused = true;
        Time.timeScale = 0f; // Freeze game physics and animations

        if (pauseCanvas != null) pauseCanvas.SetActive(true);
        if (pausePanel != null) pausePanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);

        // UI Mouse/Controller focus management
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        SetSelected(pauseFirstSelected);
    }

    public void ResumeGame()
    {
        ForceResume();
    }

    private void ForceResume()
    {
        IsPaused = false;
        Time.timeScale = 1f; // Restore game simulation speed

        if (pauseCanvas != null) pauseCanvas.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    #endregion

    #region Settings Menu Navigation

    public void OpenSettings()
    {
        RememberCurrentSelection();

        if (pausePanel != null) pausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);

        SetSelected(settingsFirstSelected);
    }

    public void CloseSettings()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(true);

        RestorePreviousSelection();
    }

    #endregion

    #region Scene Management

    public void RestartLevel()
    {
        ForceResume();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadMainMenu()
    {
        ForceResume();
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    #endregion

    #region Selection Management

    private void RememberCurrentSelection()
    {
        if (EventSystem.current == null) return;

        GameObject current = EventSystem.current.currentSelectedGameObject;
        lastSelectedBeforeSubMenu = (current != null) ? current : pauseFirstSelected;
    }

    private void RestorePreviousSelection()
    {
        SetSelected(lastSelectedBeforeSubMenu != null ? lastSelectedBeforeSubMenu : pauseFirstSelected);
    }

    private void SetSelected(GameObject obj)
    {
        if (EventSystem.current == null) return;

        if (selectCoroutine != null)
        {
            StopCoroutine(selectCoroutine);
        }
        selectCoroutine = StartCoroutine(SetSelectedNextFrame(obj));
    }

    private IEnumerator SetSelectedNextFrame(GameObject obj)
    {
        yield return null;
        EventSystem.current.SetSelectedGameObject(null);
        yield return null;
        if (obj != null)
        {
            EventSystem.current.SetSelectedGameObject(obj);
        }
    }

    #endregion
}