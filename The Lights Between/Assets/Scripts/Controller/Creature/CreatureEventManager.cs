using UnityEngine;

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

    [Header("Creature Position")]
    public Transform creaturePoint;

    [Header("Viewing Trigger")]
    public GameObject viewingTriggerToActivate;

    [Header("Response Triggers")]
    public GameObject[] responseTriggersToActivateAfterViewing;

    [Header("Creature Behaviour Rules")]
    public bool creatureCanReactToLight = true;
    public bool endScareWhenCreatureDisappears = true;

    [Header("Optional Objects")]
    public GameObject[] objectsToActivate;
    public GameObject[] objectsToDeactivate;

    [Header("Optional Audio")]
    public AudioSource audioSource;
    public AudioClip scareSFX;

    [Header("Optional Generator Power Cut")]
    public bool turnGeneratorOff;
}

public class CreatureEventManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CreatureAI creatureAI;
    [SerializeField] private GeneratorPowerSystem generatorPowerSystem;

    [Header("Generator Start Scares")]
    [SerializeField] private CreatureScareEvent[] generatorStartScares;

    [Header("Power Cut Scares")]
    [SerializeField] private CreatureScareEvent[] powerCutScares;

    [Header("Manual Scares")]
    [SerializeField] private CreatureScareEvent[] manualScares;

    [Header("Debug")]
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
        Debug.Log("Generator started event reached CreatureEventManager.");

        generatorStartCount++;

        int scareIndex = generatorStartCount - 1;

        if (generatorStartScares == null)
        {
            Debug.LogWarning("Generator start scares array is missing.");
            return;
        }

        if (scareIndex < 0 || scareIndex >= generatorStartScares.Length)
        {
            Debug.Log("No generator start scare assigned for start count: " + generatorStartCount);
            return;
        }

        PlayScare(generatorStartScares[scareIndex]);
    }

    public void TriggerPowerCut()
    {
        powerCutCount++;

        int scareIndex = powerCutCount - 1;

        if (powerCutScares != null && scareIndex >= 0 && scareIndex < powerCutScares.Length)
        {
            PlayScare(powerCutScares[scareIndex]);
        }

        if (generatorPowerSystem != null)
        {
            generatorPowerSystem.TurnGeneratorOff();
        }
    }

    public void PlayManualScareByIndex(int index)
    {
        if (manualScares == null)
        {
            Debug.LogWarning("Manual scares array is missing.");
            return;
        }

        if (index < 0 || index >= manualScares.Length)
        {
            Debug.LogWarning("Manual scare index is out of range: " + index);
            return;
        }

        PlayScare(manualScares[index]);
    }

    public void ActivateCurrentScareResponseTriggers()
    {
        if (activeScareEvent == null)
        {
            Debug.LogWarning("No active scare event. Cannot activate response triggers.");
            return;
        }

        ActivateObject(activeScareEvent.viewingTriggerToActivate, false);
        ActivateObjects(activeScareEvent.responseTriggersToActivateAfterViewing);

        Debug.Log("Activated response triggers for scare: " + activeScareEvent.scareName);
    }

    public void DisappearCreature()
    {
        if (creatureAI == null)
        {
            Debug.LogWarning("CreatureAI is missing.");
            return;
        }

        creatureAI.Disappear();
    }

    public void DisappearCreatureAndDisable()
    {
        if (creatureAI == null)
        {
            Debug.LogWarning("CreatureAI is missing.");
            return;
        }

        creatureAI.DisappearAndDisable();
    }

    public void StartCreatureChaseFromCurrentPosition()
    {
        if (creatureAI == null)
        {
            Debug.LogWarning("CreatureAI is missing.");
            return;
        }

        creatureAI.StartChaseFromCurrentPosition();
    }

    public void StopCreatureChaseAndHide()
    {
        if (creatureAI == null)
        {
            Debug.LogWarning("CreatureAI is missing.");
            return;
        }

        creatureAI.StopChaseAndHide();
    }

    private void PlayScare(CreatureScareEvent scareEvent)
    {
        if (scareEvent == null)
        {
            Debug.LogWarning("Scare event is missing.");
            return;
        }

        if (scareEvent.hasPlayed && scareEvent.disableAfterPlaying)
        {
            Debug.Log("Scare already played and is disabled: " + scareEvent.scareName);
            return;
        }

        activeScareEvent = scareEvent;

        ActivateObjects(scareEvent.objectsToActivate);
        DeactivateObjects(scareEvent.objectsToDeactivate);

        ActivateObject(scareEvent.viewingTriggerToActivate, true);

        PlayScareAudio(scareEvent);

        if (scareEvent.turnGeneratorOff && generatorPowerSystem != null)
        {
            generatorPowerSystem.TurnGeneratorOff();
        }

        if (creatureAI != null)
        {
            creatureAI.SetLightReaction(scareEvent.creatureCanReactToLight);

            if (scareEvent.scareType == CreatureScareType.Reveal)
            {
                creatureAI.RevealAtPoint(scareEvent.creaturePoint);
            }

            if (scareEvent.scareType == CreatureScareType.Chase)
            {
                creatureAI.StartChaseFromPoint(scareEvent.creaturePoint);
            }
        }

        scareEvent.hasPlayed = true;

        Debug.Log("Played Creature Scare: " + scareEvent.scareName);
    }

    private void HandleCreatureDisappeared()
    {
        if (activeScareEvent == null) return;
        if (!activeScareEvent.endScareWhenCreatureDisappears) return;

        EndActiveScare();
    }

    private void EndActiveScare()
    {
        if (activeScareEvent == null) return;

        ActivateObject(activeScareEvent.viewingTriggerToActivate, false);
        DeactivateObjects(activeScareEvent.responseTriggersToActivateAfterViewing);
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

    private void PlayScareAudio(CreatureScareEvent scareEvent)
    {
        if (scareEvent.audioSource != null && scareEvent.scareSFX != null)
        {
            scareEvent.audioSource.PlayOneShot(scareEvent.scareSFX);
        }
    }
}