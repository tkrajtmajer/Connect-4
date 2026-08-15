using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance { get; private set; }
    public AudioSource audioSource;

    [Header("Audio Clips")]
    public AudioClip buttonClickClip;
    public AudioClip coinDropClip;
    public AudioClip rotateClip;
    public AudioClip flipClip;
    public AudioClip blowupClip;
    public AudioClip swapClip;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else Instance = this;
    }

    public void PlayClip(AudioClip clip) {
        audioSource.PlayOneShot(clip);
    }
}