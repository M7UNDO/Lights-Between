using UnityEngine;

[RequireComponent(typeof(NarrativeInspectableItem))]
public class GramophoneController : MonoBehaviour, IInteractable
{
    [Header("Audio Settings")]
    [SerializeField] private AudioSource gramophoneAudioSource;
    [SerializeField] private AudioClip musicClip;

    [Header("Prompt Settings")]
    [SerializeField] private string stopMusicPrompt = "Stop Gramophone";

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
        }
    }

    public void StopMusic()
    {
        if (gramophoneAudioSource != null)
        {
            gramophoneAudioSource.Stop();
        }
        isPlayingMusic = false;
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