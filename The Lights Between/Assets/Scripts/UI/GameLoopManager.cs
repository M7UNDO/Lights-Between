using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class GameLoopManager : MonoBehaviour
{
    public static GameLoopManager Instance { get; private set; }
    public static bool IsGameEnded => Instance != null && !Instance.isGameActive;

    [Header("UI Elements")]
    [SerializeField] private GameObject playerHUD;

    [Header("Time System")]
    [SerializeField] private TextMeshProUGUI clockText;
    [SerializeField] private float realMinutesPerGameHour = 3f;

    [Header("Narrative Tracking")]
    [SerializeField] private List<NarrativeInspectableItem> requiredNarrativeItems;
    [SerializeField] private Image[] narrativeItemDots;

    [Header("UI Panels")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject winFirstSelected;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject gameOverFirstSelected;

    private HashSet<NarrativeInspectableItem> viewedItems = new HashSet<NarrativeInspectableItem>();
    private float currentTimeInHours;
    private bool isGameActive;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        currentTimeInHours = 0f;
        isGameActive = true;

        if (playerHUD != null)
        {
            playerHUD.SetActive(!SimpleTutorialPromptManager.IsTutorialActive);
        }

        if (winPanel != null) winPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        InitializeDotsUI();
        UpdateClockUI();
    }

    private void InitializeDotsUI()
    {
        if (narrativeItemDots == null) return;

        for (int i = 0; i < narrativeItemDots.Length; i++)
        {
            if (narrativeItemDots[i] != null)
            {
                narrativeItemDots[i].fillCenter = false;
            }
        }
    }

    private void Update()
    {
        if (!isGameActive) return;

        if (SimpleTutorialPromptManager.IsTutorialActive)
        {
            if (playerHUD != null && playerHUD.activeSelf)
            {
                playerHUD.SetActive(false);
            }
            return;
        }

        if (playerHUD != null && !playerHUD.activeSelf)
        {
            playerHUD.SetActive(true);
        }

        float realSecondsPerGameHour = realMinutesPerGameHour * 60f;
        float timeSpeedMultiplier = 1f / realSecondsPerGameHour;

        currentTimeInHours += timeSpeedMultiplier * Time.deltaTime;

        UpdateClockUI();

        if (currentTimeInHours >= 6f)
        {
            currentTimeInHours = 6f;
            UpdateClockUI();
            EvaluateTimeUp();
        }
    }

    private void UpdateClockUI()
    {
        if (clockText == null) return;

        int rawHours = Mathf.FloorToInt(currentTimeInHours);
        int displayHours = rawHours == 0 ? 12 : rawHours;
        int minutes = Mathf.FloorToInt((currentTimeInHours - rawHours) * 60f);

        clockText.text = string.Format("{0:00}:{1:00}AM", displayHours, minutes);
    }

    public void RegisterItemInspected(NarrativeInspectableItem item)
    {
        if (!isGameActive) return;

        if (requiredNarrativeItems.Contains(item))
        {
            if (viewedItems.Add(item))
            {
                int nextDotIndex = viewedItems.Count - 1;
                UpdateDotUI(nextDotIndex);
            }
        }
    }

    private void UpdateDotUI(int index)
    {
        if (narrativeItemDots == null || index < 0 || index >= narrativeItemDots.Length) return;

        Image dot = narrativeItemDots[index];
        if (dot != null)
        {
            dot.fillCenter = true;
        }
    }

    private void EvaluateTimeUp()
    {
        if (!isGameActive) return;

        isGameActive = false;

        if (viewedItems.Count >= requiredNarrativeItems.Count)
        {
            ShowPanel(winPanel, winFirstSelected);
        }
        else
        {
            ShowPanel(gameOverPanel, gameOverFirstSelected);
        }
    }

    public void TriggerPlayerDeath()
    {
        if (!isGameActive) return;

        isGameActive = false;
        StartCoroutine(DeathSequenceCoroutine());
    }

    private IEnumerator DeathSequenceCoroutine()
    {
        yield return new WaitForSeconds(5f);
        ShowPanel(gameOverPanel, gameOverFirstSelected);
    }

    private void ShowPanel(GameObject panel, GameObject firstSelected)
    {
        Time.timeScale = 0f;

        if (panel != null)
        {
            panel.SetActive(true);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SetSelected(firstSelected);
    }

    private void SetSelected(GameObject obj)
    {
        if (obj == null || EventSystem.current == null) return;
        StartCoroutine(SetSelectedNextFrame(obj));
    }

    private IEnumerator SetSelectedNextFrame(GameObject obj)
    {
        yield return null;
        EventSystem.current.SetSelectedGameObject(null);
        yield return null;
        EventSystem.current.SetSelectedGameObject(obj);
    }
}