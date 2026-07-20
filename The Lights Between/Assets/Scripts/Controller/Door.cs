using UnityEngine;
using UnityEngine.Audio;

public class Door : MonoBehaviour, IInteractable
{
    public bool toggle;
    public bool isDouble;

    [SerializeField] private GameObject doorPair;
    public string promptMessage;
    [SerializeField] private Animator animator;

    [Header("SFX")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip doorOpenSFX;
    [SerializeField] private AudioClip doorCloseSFX;
    [SerializeField] private AudioMixerGroup sfxMixerGroup;

    private void Start()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.outputAudioMixerGroup = sfxMixerGroup;

        promptMessage = toggle ? "Close" : "Open";
    }

    public void Interact()
    {
        toggle = !toggle;

        if (toggle)
        {
            animator.ResetTrigger("close");
            PlayDoorSFX(doorOpenSFX);

            animator.SetTrigger("open");
            promptMessage = "Close";

            Outline outline = GetComponent<Outline>();

            if (outline != null && outline.enabled)
                outline.enabled = false;
        }
        else
        {
            animator.ResetTrigger("open");
            PlayDoorSFX(doorCloseSFX);

            animator.SetTrigger("close");
            promptMessage = "Open";
        }

        if (isDouble && doorPair != null)
        {
            Door otherDoor = doorPair.GetComponent<Door>();

            if (otherDoor != null && otherDoor.toggle != toggle)
            {
                otherDoor.toggle = toggle;
                otherDoor.promptMessage = toggle ? "Close" : "Open";
            }
        }

        Debug.Log(toggle ? "Door opens" : "Door closes");
    }

    private void PlayDoorSFX(AudioClip clip)
    {
        if (audioSource == null || clip == null) return;

        audioSource.PlayOneShot(clip);
    }
}