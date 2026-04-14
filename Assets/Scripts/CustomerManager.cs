using System.Collections.Generic;
using UnityEngine;

public class CustomerManager : MonoBehaviour
{
    public static CustomerManager Instance;

    [Header("References")]
    public GameObject customerPrefab;
    public Transform spawnPoint;
    public Transform counterPoint; // Tezgahın merkez noktası
    public Transform exitPoint;

    [Header("Spawning & Positioning")]
    public float spawnInterval = 3f; // Kaç saniyede bir müşteri gelsin?
    public int maxCustomers = 5;     // Tezgahta aynı anda en fazla kaç kişi bekleyebilir?
    [Tooltip("Tezgahın merkezinden sağa ve sola ne kadar rastgele sapabileceği.")]
    public float randomOffsetRange = 10f; 

    private float timer = 0f;

    // Bekleyen müşterileri sırayla tutacağımız liste
    public List<Customer> waitingCustomers = new List<Customer>();

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        timer += Time.deltaTime;
        
        // Yeterli süre geçtiyse ve dükkan tam dolu değilse yeni müşteri yarat
        if (timer >= spawnInterval && waitingCustomers.Count < maxCustomers)
        {
            SpawnCustomer();
            timer = 0f;
        }
    }

    void SpawnCustomer()
    {
        GameObject go = Instantiate(customerPrefab, spawnPoint.position, Quaternion.identity);
        Customer newCustomer = go.GetComponent<Customer>();
        
        // Tezgah düzleminde X ekseninde (sağa/sola) rastgele bir sapma yaratıyoruz.
        // EĞER senin tezgahın yukarıdan aşağıya (Y ekseni) duruyorsa burayı (0, Random.Range, 0) yapmalısın.
        Vector3 randomOffset = new Vector3(Random.Range(-randomOffsetRange, randomOffsetRange), 0, 0);
        
        // Müşterinin gideceği yeri tezgahın merkezi + rastgele sapma olarak belirliyoruz
        newCustomer.targetPosition = counterPoint.position + randomOffset;
        
        // Müşteriyi listeye ekle
        waitingCustomers.Add(newCustomer);
    }

    public bool IsCustomerReadyToOrder()
    {
        // Listeyi gez: Tezgahta olan VE henüz hizmet edilmeyen biri var mı?
        foreach (Customer c in waitingCustomers)
        {
            if (c.isAtCounter && !c.isBeingServed)
                return true;
        }
        return false;
    }

    // Eski GetFirstCustomer() fonksiyonunun adını ve içini değiştiriyoruz
    public Customer TryGetAvailableCustomer()
    {
        foreach (Customer c in waitingCustomers)
        {
            if (c.isAtCounter && !c.isBeingServed)
            {
                return c; // Bulduğumuz an geri gönderiyoruz
            }
        }
        return null; // Kimse yoksa null döner
}

    public void ServeSpecificCustomer(Customer customerToServe)
    {
        if (waitingCustomers.Contains(customerToServe))
        {
            customerToServe.GoToExit(exitPoint);
            waitingCustomers.Remove(customerToServe);
        }
    }

    // Çalışanın, sıradaki müşterinin nerede durduğunu öğrenmesi için yeni fonksiyon
    public Vector3 GetFirstCustomerPosition()
    {
        return waitingCustomers[0].transform.position;
    }

    public void ServeCustomer()
    {
        if (waitingCustomers.Count > 0)
        {
            Customer servedCustomer = waitingCustomers[0];
            servedCustomer.GoToExit(exitPoint); // İşi biteni çıkışa yolla
            waitingCustomers.RemoveAt(0);       // Listeden sil ki sıradaki müşteriye geçilsin
        }
    }
}