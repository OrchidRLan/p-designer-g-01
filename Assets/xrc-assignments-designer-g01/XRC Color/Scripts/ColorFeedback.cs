namespace xrc_assignments_designer_g01.XRC_Color.Scripts
{
    using UnityEngine;
    using UnityEngine.InputSystem;
    [RequireComponent(typeof(AudioSource))]
    public class ColorFeedback: MonoBehaviour
    {
        [SerializeField] private AudioClip applyColorSound;
        [SerializeField] private float volume = 1.0f;
        
        private AudioSource audioSource;

        public void Awake()
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
}