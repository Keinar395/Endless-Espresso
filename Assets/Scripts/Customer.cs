using UnityEngine;

public class Customer : MonoBehaviour
{
    public Vector3 targetPosition; 
    public float speed = 4f;
    public bool isAtCounter = false;

    [Header("Görseller")]
    public Sprite normalSprite;
    public Sprite darkSprite;
    private SpriteRenderer spriteRenderer;
    
    // YENİ EKLENEN DEĞİŞKEN: Başka bir çalışan bu müşteriyi aldı mı?
    public bool isBeingServed = false; 
    
    private bool isGoingToExit = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // Hedef koordinata yürü
        transform.position = Vector2.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        // Hedefe ulaşıldıysa
        if (Vector2.Distance(transform.position, targetPosition) < 0.1f)
        {
            if (!isGoingToExit)
            {
                isAtCounter = true; // Tezgahtaki rastgele yerine ulaştı
            }
            else
            {
                Destroy(gameObject); // Çıkışa ulaştıysa yok ol
            }
        }
    }

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

    public void GoToExit(Transform exitPoint)
    {
        targetPosition = exitPoint.position;
        isAtCounter = false;
        isGoingToExit = true;
    }
}