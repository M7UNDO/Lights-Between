using UnityEngine;

[RequireComponent(typeof(NarrativeInspectableItem))]
public class GramophoneController : MonoBehaviour, IInteractable
{
    [Header("Audio Settings")]
    [SerializeField] private AudioSource gramophoneAudioSource;
    [SerializeField] private AudioClip musicClip;

    [Header("Prompt Settings")]
    [SerializeField] private string stopMusicPrompt = "Stop Gramophone";

    [Header("Visual & Animation Settings")]
    [SerializeField] private Animator gramophoneAnimator;
    [SerializeField] private Light gramophoneLight;
    [SerializeField] private string playTriggerName = "play";
    [SerializeField] private string offTriggerName = "off";

    private NarrativeInspectableItem narrativeItem;
    private bool isPlayingMusic;

    public bool IsPlayingMusic => isPlayingMusic;

    private void Awake()
    {
        narrativeItem = GetComponent<NarrativeInspectableItem>();
        if (gramophoneAudioSource == null)
        {
            gramophoneAudioSource = GetComponent<AudioSource>();
        }

        if (gramophoneAnimator == null)
        {
            gramophoneAnimator = GetComponent<Animator>();
        }

        if (gramophoneLight != null)
        {
            gramophoneLight.enabled = false;
        }
    }

    public string InteractionPrompt => isPlayingMusic ? stopMusicPrompt : (narrativeItem != null ? narrativeItem.InteractionPrompt : string.Empty);

    public void PlayMusic()
    {
        if (gramophoneAudioSource != null && musicClip != null)
        {
            gramophoneAudioSource.clip = musicClip;
            gramophoneAudioSource.loop = true;
            gramophoneAudioSource.Play();
            isPlayingMusic = true;

            if (gramophoneAnimator != null)
            {
                gramophoneAnimator.ResetTrigger(offTriggerName);
                gramophoneAnimator.SetTrigger(playTriggerName);
            }

            if (gramophoneLight != null)
            {
                gramophoneLight.enabled = true;
            }
        }
    }

    public void StopMusic()
    {
        if (gramophoneAudioSource != null)
        {
            gramophoneAudioSource.Stop();
        }
        isPlayingMusic = false;

        if (gramophoneAnimator != null)
        {
            gramophoneAnimator.ResetTrigger(playTriggerName);
            gramophoneAnimator.SetTrigger(offTriggerName);
        }

        if (gramophoneLight != null)
        {
            gramophoneLight.enabled = false;
        }
    }

    public void Interact()
    {
        if (isPlayingMusic)
        {
            StopMusic();
            return;
        }

        if (narrativeItem != null)
        {
            narrativeItem.Interact();
        }
    }
}