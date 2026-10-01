using System.Collections.Generic;
using UnityEngine;

public class CustomerManager : MonoBehaviour
{
    public static CustomerManager Instance;

    [Header("References")]
    public GameObject customerPrefab;
    public Transform spawnPoint;
    public Transform counterPoint; 
    public Transform exitPoint;

    [Header("Spawning & Positioning")]
    public float baseSpawnInterval = 3f; // Başlangıçtaki müşteri gelme süresi
    private float currentSpawnInterval;  // O anki hızlandırılmış süre
    public int maxCustomers = 5;         
    public float randomOffsetRange = 10f; 
    
    [Header("Customer Speed (Müşteri Hızı)")]
    public float baseCustomerSpeed = 4f; // Müşterinin başlangıç yürüme hızı
    private float currentCustomerSpeed;  // Hız çarpanına göre artan yürüme hızı

    [Header("Floating Text Prefab")]
    public GameObject floatingTextPrefab;

    private float timer = 0f;

    public List<Customer> waitingCustomers = new List<Customer>();

    void Awake()
    {
        Instance = this;
        // Oyun başlarken anlık değerleri temellere eşitle
        currentSpawnInterval = baseSpawnInterval;
        currentCustomerSpeed = baseCustomerSpeed;
    }

    void Update()
    {
        timer += Time.deltaTime;
        
        // Yeterli süre geçtiyse ve dükkan tam dolu değilse
        if (timer >= currentSpawnInterval && waitingCustomers.Count < maxCustomers)
        {
            SpawnCustomer();
            timer = 0f;
        }
    }

    // UpgradeManager tarafından çağrılacak yeni fonksiyon!
    public void UpdateCustomerFlow(float globalMultiplier)
    {
        // globalMultiplier oyun başında 1'dir.
        // Hız arttıkça müşterilerin daha da "agresif" gelmesini istiyorsak, çarpanın gücünü artırabiliriz (Örn: 1.2 kuvvetini alarak)
        // Böylece oyun başında çarpan 1 kalır (bozulmaz), ama sonlara doğru çok daha fazla müşteri üretir.
        float spawnMultiplier = Mathf.Pow(globalMultiplier, 2.5f); 

        currentSpawnInterval = baseSpawnInterval / spawnMultiplier;
        
        // KOD HATASININ ÇÖZÜMÜ: Alt sınır koyuyoruz. Müşteriler en fazla 0.15 saniyede bir gelebilsin (Oyun çökmesin diye)
        currentSpawnInterval = Mathf.Max(currentSpawnInterval, 0.15f);

        // Müşteri yürüme hızı normal çarpanla artsın (Oyun başında 5 ile ÇARPMIYORUZ ki tazı gibi koşmasınlar)
        currentCustomerSpeed = baseCustomerSpeed * globalMultiplier;
    }

    void SpawnCustomer()
    {
        GameObject go = Instantiate(customerPrefab, spawnPoint.position, Quaternion.identity);
        Customer newCustomer = go.GetComponent<Customer>();
        
        // YENİ: Müşterinin yürüme hızını dinamik hıza eşitle
        newCustomer.speed = currentCustomerSpeed;
        
        Vector3 randomOffset = new Vector3(Random.Range(-randomOffsetRange, randomOffsetRange), 0, 0);
        newCustomer.targetPosition = counterPoint.position + randomOffset;
        
        waitingCustomers.Add(newCustomer);
    }

    public bool IsCustomerReadyToOrder()
    {
        foreach (Customer c in waitingCustomers)
        {
            if (c.isAtCounter && !c.isBeingServed)
                return true;
        }
        return false;
    }

    public Customer TryGetAvailableCustomer()
    {
        foreach (Customer c in waitingCustomers)
        {
            if (c.isAtCounter && !c.isBeingServed)
            {
                return c; 
            }
        }
        return null; 
    }

    public void ServeSpecificCustomer(Customer customerToServe)
    {
        if (waitingCustomers.Contains(customerToServe))
        {
            customerToServe.GoToExit(exitPoint);
            waitingCustomers.Remove(customerToServe);
        }
    }

    // Yazıyı doğuran fonksiyon
    public void SpawnFloatingText(Vector3 spawnPosition, string textValue)
    {
        if (floatingTextPrefab != null)
        {
            // Müşterinin biraz üzerinden doğsun diye Y eksenine 1f ekliyoruz
            Vector3 offsetPosition = spawnPosition + new Vector3(0, 1f, 0);
            GameObject popup = Instantiate(floatingTextPrefab, offsetPosition, Quaternion.identity);
            
            popup.GetComponent<FloatingText>().Setup(textValue);
        }
    }
}