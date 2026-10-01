using UnityEngine;

public class Machine : MonoBehaviour
{
    public bool isOccupied = false;
    private AudioSource audioSource;

    [Header("Görseller")]
    public Sprite normalSprite;
    public Sprite darkSprite;
    private SpriteRenderer spriteRenderer;

    void Start() // Awake değil, Start veya Awake içine ekleyebilirsin
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // UpgradeManager bu fonksiyonu çağıracak
    public void EvolveToDarkShift()
    {
        // YENİ: Eğer Start() fonksiyonu henüz yetişemediyse ve spriteRenderer boşsa, hemen şimdi bul!
        if (spriteRenderer == null) 
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (darkSprite != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = darkSprite;
        }
    }

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