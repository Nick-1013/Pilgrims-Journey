using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Player Sounds")]
    public AudioClip walkClip;
    public AudioClip attackClip;
    public AudioClip hurtClip;

    private AudioSource audioSource;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        audioSource = GetComponent<AudioSource>();
    }

    public void PlayWalk()
    {
        if (!audioSource.isPlaying)
            audioSource.PlayOneShot(walkClip);
    }

    public void PlayAttack()
    {
        audioSource.PlayOneShot(attackClip);
    }

    public void PlayHurt()
    {
        audioSource.PlayOneShot(hurtClip);
    }
}