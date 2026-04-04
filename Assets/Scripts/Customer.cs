using UnityEngine;

public class Customer : MonoBehaviour
{
    // Artık Transform değil, direkt olarak gideceği koordinatı tutuyoruz
    public Vector3 targetPosition; 
    public float speed = 4f;
    public bool isAtCounter = false;
    private bool isGoingToExit = false;

    void Update()
    {
        // Hedef koordinata yürü
        transform.position = Vector2.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        // Hedefe ulaşıldıysa
        if (Vector2.Distance(transform.position, targetPosition) < 0.01f)
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

    public void GoToExit(Transform exitPoint)
    {
        targetPosition = exitPoint.position;
        isAtCounter = false;
        isGoingToExit = true;
    }
}