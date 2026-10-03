using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Hoparlörler (Audio Sources)")]
    public AudioSource sfxSource;   // Tekil efektler (Para, Buton vb.)
    public AudioSource loopSource;  // Döngüsel sesler (Kahve dökme vb.)
    
    [Header("UI & Sistem Sesleri")]
    public AudioClip clickClip;
    public AudioClip cannotBuyClip;
    public AudioClip upgradeClip;
    public AudioClip evolveClip;

    [Header("Oyun Mekaniği Sesleri")]
    public AudioClip takeOrderClip;
    public AudioClip brewingLoopClip; // Kahve makinesi çalışma sesi
    public AudioClip brewDoneClip;
    public AudioClip moneyClip;

    [Header("Pitch (Ton) Ayarları")]
    [Tooltip("Aynı ses üst üste çalarken ne kadar rastgele incelip kalınlaşsın? (Örn: 0.1)")]

    [Header("Müzik Ayarları")]
    public AudioSource musicSource; // Arka plan müziğini çalan Audio Source bileşeni
    public AudioClip darkShiftMusic;

    public float pitchRandomness = 0.05f; 

    void Awake()
    {
        Instance = this;
    }

    // NORMAL SES ÇALMA (Butonlar vb. için)
    public void PlaySFX(AudioClip clip)
    {
        if (clip != null)
        {
            sfxSource.pitch = 1f; // Tonu normale döndür
            sfxSource.PlayOneShot(clip);
        }
    }

    // RASTGELE TONLU SES ÇALMA (Para, Müşteri, Sipariş vb. çok tekrar edenler için)
    public void PlaySFXRandomPitch(AudioClip clip)
    {
        if (clip != null)
        {
            // Orijinal hızı 1'dir. Bunu 0.85 ile 1.15 arasında rastgele yapıyoruz.
            sfxSource.pitch = Random.Range(1f - pitchRandomness, 1f + pitchRandomness);
            sfxSource.PlayOneShot(clip);
            
            // Çaldıktan sonra hemen normale döndürelim ki diğer sesleri bozmasın
            // (Aslında PlayOneShot o anki pitch'i baz alır, bir sonraki seste tekrar ayarlanır)
        }
    }

    public void SwitchToDarkShiftMusic()
    {
        if (musicSource != null && darkShiftMusic != null)
        {
            musicSource.Stop(); // Normal müziği durdur
            musicSource.clip = darkShiftMusic; // Yeni müziği koy
            musicSource.Play(); // Şeytani müziği başlat
        }
    }
}