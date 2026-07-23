using UnityEngine;
using UnityEngine.Events;

public enum CreatureScareType
{
    None,
    Reveal,
    Chase
}

[System.Serializable]
public class CreatureScareEvent
{
    [Header("Scare Identity")]
    public string scareName;
    public bool hasPlayed;
    public bool disableAfterPlaying = true;

    [Header("Scare Type")]
    public CreatureScareType scareType;
    public Transform creaturePoint;

    [Header("Progression Triggers")]
    public GameObject viewingTriggerToActivate;
    public GameObject[] responseTriggersToActivateAfterViewing;

    [Header("Creature Behaviour Rules")]
    public bool creatureCanReactToLight = true;
    public bool endScareWhenCreatureDisappears = true;

    [Header("Custom Scene Events")]
    public UnityEvent onScareStart;
    public UnityEvent onScareEnd;
}

public class CreatureEventManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CreatureAI creatureAI;
    [SerializeField] private GeneratorPowerSystem generatorPowerSystem;
    [SerializeField] private ElectricBoxBreaker electricBoxBreaker;

    [Header("Scare Collections")]
    [SerializeField] private CreatureScareEvent[] generatorStartScares;
    [SerializeField] private CreatureScareEvent[] powerCutScares;
    [SerializeField] private CreatureScareEvent[] manualScares;

    [Header("Debug State")]
    [SerializeField] private int generatorStartCount;
    [SerializeField] private int powerCutCount;
    [SerializeField] private CreatureScareEvent activeScareEvent;

    private void OnEnable()
    {
        if (creatureAI != null)
        {
            creatureAI.OnCreatureDisappeared += HandleCreatureDisappeared;
        }
    }

    private void OnDisable()
    {
        if (creatureAI != null)
        {
            creatureAI.OnCreatureDisappeared -= HandleCreatureDisappeared;
        }
    }

    public void OnGeneratorStarted()
    {
        generatorStartCount++;
        int scareIndex = generatorStartCount - 1;

        if (generatorStartScares == null || scareIndex < 0 || scareIndex >= generatorStartScares.Length) return;

        PlayScare(generatorStartScares[scareIndex]);
    }

    public void TriggerPowerCut()
    {
        bool isPowerActive = false;

        if (electricBoxBreaker != null)
        {
            isPowerActive = electricBoxBreaker.IsGridActive;
        }
        else if (generatorPowerSystem != null)
        {
            isPowerActive = generatorPowerSystem.IsPowerOn;
        }

        if (!isPowerActive) return;

        powerCutCount++;
        int scareIndex = powerCutCount - 1;

        if (powerCutScares != null && scareIndex >= 0 && scareIndex < powerCutScares.Length)
        {
            PlayScare(powerCutScares[scareIndex]);
        }

        if (electricBoxBreaker != null)
        {
            electricBoxBreaker.TripBreaker();
        }

        if (generatorPowerSystem != null)
        {
            generatorPowerSystem.TurnGeneratorOff();
        }
    }

    public void PlayManualScareByIndex(int index)
    {
        if (manualScares == null || index < 0 || index >= manualScares.Length) return;

        PlayScare(manualScares[index]);
    }

    public void ActivateCurrentScareResponseTriggers()
    {
        if (activeScareEvent == null) return;

        ActivateObject(activeScareEvent.viewingTriggerToActivate, false);
        ActivateObjects(activeScareEvent.responseTriggersToActivateAfterViewing);
    }

    public void DisappearCreature()
    {
        if (creatureAI != null)
        {
            creatureAI.Disappear();
        }
    }

    public void DisappearCreatureAndDisable()
    {
        if (creatureAI != null)
        {
            creatureAI.DisappearAndDisable();
        }
    }

    public void StartCreatureChaseFromCurrentPosition()
    {
        if (creatureAI != null)
        {
            creatureAI.StartChaseFromCurrentPosition();
        }
    }

    public void StopCreatureChaseAndHide()
    {
        if (creatureAI != null)
        {
            creatureAI.StopChaseAndHide();
        }
    }

    private void PlayScare(CreatureScareEvent scareEvent)
    {
        if (scareEvent == null) return;
        if (scareEvent.hasPlayed && scareEvent.disableAfterPlaying) return;

        activeScareEvent = scareEvent;

        ActivateObject(scareEvent.viewingTriggerToActivate, true);

        if (creatureAI != null && scareEvent.scareType != CreatureScareType.None)
        {
            creatureAI.SetLightReaction(scareEvent.creatureCanReactToLight);

            if (scareEvent.scareType == CreatureScareType.Reveal)
            {
                creatureAI.RevealAtPoint(scareEvent.creaturePoint);
            }
            else if (scareEvent.scareType == CreatureScareType.Chase)
            {
                creatureAI.StartChaseFromPoint(scareEvent.creaturePoint);
            }
        }

        scareEvent.onScareStart?.Invoke();
        scareEvent.hasPlayed = true;
    }

    private void HandleCreatureDisappeared()
    {
        if (activeScareEvent == null || !activeScareEvent.endScareWhenCreatureDisappears) return;
        EndActiveScare();
    }

    private void EndActiveScare()
    {
        if (activeScareEvent == null) return;

        ActivateObject(activeScareEvent.viewingTriggerToActivate, false);
        DeactivateObjects(activeScareEvent.responseTriggersToActivateAfterViewing);

        activeScareEvent.onScareEnd?.Invoke();
        activeScareEvent = null;
    }

    private void ActivateObjects(GameObject[] objects)
    {
        if (objects == null) return;
        foreach (var obj in objects)
        {
            ActivateObject(obj, true);
        }
    }

    private void DeactivateObjects(GameObject[] objects)
    {
        if (objects == null) return;
        foreach (var obj in objects)
        {
            ActivateObject(obj, false);
        }
    }

    private void ActivateObject(GameObject obj, bool state)
    {
        if (obj != null)
        {
            obj.SetActive(state);
        }
    }
}