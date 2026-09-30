using UnityEngine;

public class Machine : MonoBehaviour
{
    public bool isOccupied = false;
    private AudioSource audioSource;

    void Awake()
    {
        // Makine doğduğunda kendine özel bir hoparlör ekler
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true; // Çalışma sesi sürekli döngüde olacak
        audioSource.playOnAwake = false;
        audioSource.volume = 0.5f; // Çok kalabalık olunca kafa şişirmesin diye kısık
    }

    public void StartBrewing(AudioClip loopClip)
    {
        if (loopClip != null)
        {
            audioSource.clip = loopClip;
            audioSource.Play();
        }
    }

    public void StopBrewing()
    {
        audioSource.Stop();
    }
}