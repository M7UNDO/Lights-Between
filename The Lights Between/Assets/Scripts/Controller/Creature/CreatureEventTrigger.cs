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
        if (hasTriggered && onlyTriggerOnce) return;

        bool isPlayer = other.CompareTag("Player") || other.GetComponentInParent<PlayerInputHandler>() != null;

        if (!isPlayer) return;

        if (creatureEventManager == null)
        {
            Debug.LogWarning("CreatureEventManager is missing on " + gameObject.name);
            return;
        }

        ExecuteTriggerAction();
        hasTriggered = true;
    }

    private void ExecuteTriggerAction()
    {
        switch (triggerAction)
        {
            case CreatureTriggerAction.PlayManualScare:
                creatureEventManager.PlayManualScareByIndex(manualScareIndex);
                break;
            case CreatureTriggerAction.ActivateCurrentScareResponseTriggers:
                creatureEventManager.ActivateCurrentScareResponseTriggers();
                break;
            case CreatureTriggerAction.DisappearCreature:
                creatureEventManager.DisappearCreature();
                break;
            case CreatureTriggerAction.DisappearCreatureAndDisable:
                creatureEventManager.DisappearCreatureAndDisable();
                break;
            case CreatureTriggerAction.StartChaseFromCurrentPosition:
                creatureEventManager.StartCreatureChaseFromCurrentPosition();
                break;
            case CreatureTriggerAction.StopChaseAndHide:
                creatureEventManager.StopCreatureChaseAndHide();
                break;
            case CreatureTriggerAction.TriggerPowerCut:
                creatureEventManager.TriggerPowerCut();
                break;
        }
    }
}