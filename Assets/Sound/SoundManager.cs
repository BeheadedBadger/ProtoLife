using UnityEngine;

public enum soundType 
{ 
    Fail,
    ShortFail,
    VeryShortFail,
    Success,
    ShortSuccess,
    VeryShortSuccess,
    OpenMysteryBox
}

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    [SerializeField] AudioClip[] audioList;
    private static SoundManager instance;
    private AudioSource audioSource;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public static void PlaySound(soundType sound)
    {
        instance.audioSource.PlayOneShot(instance.audioList[(int)sound]);
    }
}