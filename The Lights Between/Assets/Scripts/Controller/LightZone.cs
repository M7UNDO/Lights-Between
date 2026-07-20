using UnityEngine;
using System.Collections.Generic;

public class LightZone : MonoBehaviour
{
    [Header("Light Zone State")]
    [SerializeField] private bool isActive = true;

    private readonly HashSet<PlayerLightStatus> trackedPlayers = new HashSet<PlayerLightStatus>();
    private readonly HashSet<CreatureLightSensor> trackedCreatures = new HashSet<CreatureLightSensor>();

    public bool IsActive
    {
        get { return isActive; }
    }

    public void SetActive(bool active)
    {
        isActive = active;
        gameObject.SetActive(active);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isActive) return;

        PlayerLightStatus player = other.GetComponent<PlayerLightStatus>();
        if (player != null && trackedPlayers.Add(player))
        {
            player.EnterLight();
            print("Player in Light!");
        }

        CreatureLightSensor creature = other.GetComponent<CreatureLightSensor>();
        if (creature != null && trackedCreatures.Add(creature))
        {
            creature.EnterLight();
            print("Creature in Light!");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerLightStatus player = other.GetComponent<PlayerLightStatus>();
        if (player != null && trackedPlayers.Remove(player))
        {
            player.ExitLight();
            print("Player Exited Light!");
        }

        CreatureLightSensor creature = other.GetComponent<CreatureLightSensor>();
        if (creature != null && trackedCreatures.Remove(creature))
        {
            creature.ExitLight();
            print("Creature Exited Light!");
        }
    }

    private void OnDisable()
    {
        foreach (PlayerLightStatus player in trackedPlayers)
        {
            if (player != null)
            {
                player.ExitLight();
                print("Player Exited Light due to zone deactivation!");
            }
        }

        foreach (CreatureLightSensor creature in trackedCreatures)
        {
            if (creature != null)
            {
                creature.ExitLight();
                print("Creature Exited Light due to zone deactivation!");
            }
        }

        trackedPlayers.Clear();
        trackedCreatures.Clear();
    }
}