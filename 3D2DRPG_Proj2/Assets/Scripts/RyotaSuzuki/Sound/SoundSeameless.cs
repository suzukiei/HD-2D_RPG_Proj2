using UnityEngine;

public class SoundSeameless : MonoBehaviour
{
    public AudioSource audioSource;
    private double nextEventTime;
    private AudioSettings audioSettings;
    private double clipLength;

    void Start()
    {
        clipLength = audioSource.clip.length;
        nextEventTime = AudioSettings.dspTime + 0.1f;
        audioSource.PlayScheduled(nextEventTime);
    }

    void Update()
    {
        if (AudioSettings.dspTime > nextEventTime + clipLength)
        {
            nextEventTime += clipLength;
            audioSource.PlayScheduled(nextEventTime);
        }
    }
}
