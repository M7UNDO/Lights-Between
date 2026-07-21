using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(AudioSource))]
public class DoorScript : MonoBehaviour, IInteractable
{
    public bool open;
    public float smooth = 1.0f;
    public float DoorOpenAngle = -90.0f;
    public float DoorCloseAngle = 0.0f;
    public AudioSource asource;
    public AudioClip openDoor, closeDoor;
    public string promptMessage;

    public bool toggle;
    private Coroutine rotationCoroutine;
    private NavMeshObstacle navObstacle;

    void Start()
    {
        asource = GetComponent<AudioSource>();
        navObstacle = GetComponent<NavMeshObstacle>();
        if (navObstacle == null)
        {
            navObstacle = GetComponentInChildren<NavMeshObstacle>();
        }

        promptMessage = toggle ? "Close" : "Open";
        UpdateNavObstacleState();
    }

    public void Interact()
    {
        toggle = !toggle;
        promptMessage = toggle ? "Close" : "Open";

        OpenDoor();

        float targetAngle = toggle ? DoorOpenAngle : DoorCloseAngle;

        if (rotationCoroutine != null)
        {
            StopCoroutine(rotationCoroutine);
        }
        rotationCoroutine = StartCoroutine(AnimateRotation(targetAngle));
    }

    public void ForceOpen()
    {
        if (open) return;

        toggle = true;
        open = true;
        promptMessage = "Close";

        UpdateNavObstacleState();

        if (asource != null && openDoor != null)
        {
            asource.clip = openDoor;
            asource.Play();
        }

        if (rotationCoroutine != null)
        {
            StopCoroutine(rotationCoroutine);
        }
        rotationCoroutine = StartCoroutine(AnimateRotation(DoorOpenAngle));
    }

    private IEnumerator AnimateRotation(float targetAngle)
    {
        Quaternion targetRotation = Quaternion.Euler(0, targetAngle, 0);

        while (Quaternion.Angle(transform.localRotation, targetRotation) > 0.1f)
        {
            transform.localRotation = Quaternion.Slerp(
                transform.localRotation,
                targetRotation,
                Time.deltaTime * 5f * smooth
            );
            yield return null;
        }

        transform.localRotation = targetRotation;
    }

    public void OpenDoor()
    {
        open = !open;
        UpdateNavObstacleState();

        if (asource != null)
        {
            asource.clip = open ? openDoor : closeDoor;
            asource.Play();
        }
    }

    private void UpdateNavObstacleState()
    {
        if (navObstacle != null)
        {
            navObstacle.carving = !open;
            navObstacle.enabled = !open;
        }
    }
}