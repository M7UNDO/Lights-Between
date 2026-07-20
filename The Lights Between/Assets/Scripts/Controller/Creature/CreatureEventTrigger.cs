using UnityEngine;

public enum CreatureTriggerAction
{
    PlayManualScare,
    ActivateCurrentScareResponseTriggers,
    DisappearCreature,
    DisappearCreatureAndDisable,
    StartChaseFromCurrentPosition,
    StopChaseAndHide,
    TriggerPowerCut
}

public class CreatureEventTrigger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CreatureEventManager creatureEventManager;

    [Header("Trigger Settings")]
    [SerializeField] private CreatureTriggerAction triggerAction;
    [SerializeField] private int manualScareIndex;
    [SerializeField] private bool onlyTriggerOnce = true;

    private bool hasTriggered;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(gameObject.name + " was entered by " + other.name);

        if (hasTriggered && onlyTriggerOnce) return;

        bool isPlayer =
            other.CompareTag("Player") ||
            other.GetComponentInParent<PlayerInputHandler>() != null;

        if (!isPlayer) return;

        if (creatureEventManager == null)
        {
            Debug.LogWarning("CreatureEventManager is missing on " + gameObject.name);
            return;
        }

        if (triggerAction == CreatureTriggerAction.PlayManualScare)
        {
            creatureEventManager.PlayManualScareByIndex(manualScareIndex);
        }

        if (triggerAction == CreatureTriggerAction.ActivateCurrentScareResponseTriggers)
        {
            creatureEventManager.ActivateCurrentScareResponseTriggers();
        }

        if (triggerAction == CreatureTriggerAction.DisappearCreature)
        {
            creatureEventManager.DisappearCreature();
        }

        if (triggerAction == CreatureTriggerAction.DisappearCreatureAndDisable)
        {
            creatureEventManager.DisappearCreatureAndDisable();
        }

        if (triggerAction == CreatureTriggerAction.StartChaseFromCurrentPosition)
        {
            creatureEventManager.StartCreatureChaseFromCurrentPosition();
        }

        if (triggerAction == CreatureTriggerAction.StopChaseAndHide)
        {
            creatureEventManager.StopCreatureChaseAndHide();
        }

        if (triggerAction == CreatureTriggerAction.TriggerPowerCut)
        {
            creatureEventManager.TriggerPowerCut();
        }

        hasTriggered = true;

        Debug.Log("Creature trigger activated: " + triggerAction + " on " + gameObject.name);
    }
}