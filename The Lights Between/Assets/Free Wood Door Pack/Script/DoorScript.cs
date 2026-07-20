using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    void Start()
    {
        asource = GetComponent<AudioSource>();
        promptMessage = toggle ? "Close" : "Open";
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
        if (asource != null)
        {
            asource.clip = open ? openDoor : closeDoor;
            asource.Play();
        }
    }
}