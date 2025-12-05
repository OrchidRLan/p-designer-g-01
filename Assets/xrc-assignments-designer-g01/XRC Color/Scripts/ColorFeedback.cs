using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ColorFeedback : MonoBehaviour
{
    [Header("Audio Feedback")]
    [Tooltip("Sound to play when color is applied (short 'ding' sound)")]
    [SerializeField] private AudioClip applyColorSound;
    
    [Tooltip("Volume for apply sound (0-1)")]
    [SerializeField] private float volume = 1.0f;
    
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; 
        audioSource.volume = volume;
    }
    
    public void PlayApplySound()
    {
        audioSource.PlayOneShot(applyColorSound, volume);
        
    }

    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        if (audioSource != null)
        {
            audioSource.volume = volume;
        }
    }
}
