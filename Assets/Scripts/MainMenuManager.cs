using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement; // Sahneler arası geçiş için şart

public class MainMenuManager : MonoBehaviour
{
    [Header("Animasyon Ayarları")]
    public RectTransform buttonsContainer; // Butonları tutan kutumuz
    public CanvasGroup containerCanvasGroup; // Saydamlık için
    public float slideDuration = 1.2f; // Animasyonun süresi
    public float slideOffset = -600f; // Butonların ne kadar aşağıdan geleceği
    public AnimationCurve slideCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // Kayma hissiyatı

    void Start()
    {
        if (buttonsContainer != null)
        {
            // 1. Oyun başladığında kutuyu aşağıya al ve tamamen görünmez (şeffaf) yap
            buttonsContainer.anchoredPosition += new Vector2(0, slideOffset);
            if (containerCanvasGroup != null) containerCanvasGroup.alpha = 0f;

            // 2. Animasyonu başlat
            StartCoroutine(SlideInRoutine());
        }
    }

    private IEnumerator SlideInRoutine()
    {
        Vector2 startPos = buttonsContainer.anchoredPosition;
        // Orijinal yerine dönmesi için ayarladığımız offset'i geri çıkarıyoruz
        Vector2 endPos = startPos - new Vector2(0, slideOffset); 
        float elapsed = 0f;

        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / slideDuration;
            
            // AnimationCurve sayesinde dümdüz değil, yavaşlayarak oturan tatlı bir kayma olur
            float curveT = slideCurve.Evaluate(t); 

            // Pozisyonu ve şeffaflığı (Alpha) yumuşakça değiştir
            buttonsContainer.anchoredPosition = Vector2.Lerp(startPos, endPos, curveT);
            if (containerCanvasGroup != null) containerCanvasGroup.alpha = Mathf.Lerp(0f, 1f, curveT);

            yield return null;
        }

        // Animasyon bitince tam yerine oturduğundan emin ol
        buttonsContainer.anchoredPosition = endPos;
        if (containerCanvasGroup != null) containerCanvasGroup.alpha = 1f;
    }

    // PLAY butonuna tıklandığında çalışacak fonksiyon
    public void PlayGame()
    {
        // Ekran görüntülerinde oyun sahnenin adının "SampleScene" olduğunu gördüm.
        // Eğer değiştirdiysen burayı da oyun sahnenin adıyla değiştirmelisin.
        SceneManager.LoadScene("SampleScene"); 
    }

    // QUIT butonuna tıklandığında çalışacak fonksiyon
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Oyun kapatıldı!"); // Unity Editor'de çalışırken kapandığını anlaman için
    }
}