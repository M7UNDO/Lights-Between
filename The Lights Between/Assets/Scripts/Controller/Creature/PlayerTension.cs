using UnityEngine;
using UnityEngine.UI;

public class PlayerTensionSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerLightStatus playerLightStatus;
    [SerializeField] private CreatureAI creatureAI;
    [SerializeField] private Image tensionFillBar;

    [Header("Tension Settings")]
    [SerializeField] private float maxTension = 100f;
    [SerializeField] private float tensionIncreaseRate = 5f;
    [SerializeField] private float tensionDecreaseRate = 3f;

    private float currentTension;
    private bool hasTriggeredKill;

    public float CurrentTension => currentTension;
    public float MaxTension => maxTension;

    private void OnEnable()
    {
        if (creatureAI != null)
        {
            creatureAI.OnCreatureDisappeared += ResetTension;
        }
    }

    private void OnDisable()
    {
        if (creatureAI != null)
        {
            creatureAI.OnCreatureDisappeared -= ResetTension;
        }
    }

    private void Update()
    {
        if (playerLightStatus == null || creatureAI == null || hasTriggeredKill) return;

        HandleTensionCalculation();
        UpdateUI();
        CheckTensionThreshold();
    }

    private void HandleTensionCalculation()
    {
        if (!playerLightStatus.isInLight)
        {
            currentTension += tensionIncreaseRate * Time.deltaTime;
        }
        else
        {
            currentTension -= tensionDecreaseRate * Time.deltaTime;
        }

        currentTension = Mathf.Clamp(currentTension, 0f, maxTension);
    }

    private void UpdateUI()
    {
        if (tensionFillBar != null)
        {
            tensionFillBar.fillAmount = currentTension / maxTension;
        }
    }

    private void CheckTensionThreshold()
    {
        if (currentTension >= maxTension)
        {
            hasTriggeredKill = true;
            TriggerCreatureKill();
        }
    }

    private void TriggerCreatureKill()
    {
        creatureAI.StartChaseFromCurrentPosition();
    }

    public void ResetTension()
    {
        currentTension = 0f;
        hasTriggeredKill = false;
        UpdateUI();
    }
}