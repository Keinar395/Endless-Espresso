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
    public float baseSpawnInterval = 3f; 
    private float currentSpawnInterval;  
    public int maxCustomers = 5;         
    public float randomOffsetRange = 10f; 
    
    [Header("Customer Speed (Müşteri Hızı)")]
    public float baseCustomerSpeed = 4f; 
    private float currentCustomerSpeed;  

    [Header("Floating Text Prefab")]
    public GameObject floatingTextPrefab;

    private float timer = 0f;
    public bool isDarkShiftActive = false;

    public List<Customer> waitingCustomers = new List<Customer>();

    void Awake()
    {
        Instance = this;
        currentSpawnInterval = baseSpawnInterval;
        currentCustomerSpeed = baseCustomerSpeed;
    }

    void Update()
    {
        timer += Time.deltaTime;
        
        if (timer >= currentSpawnInterval && waitingCustomers.Count < maxCustomers)
        {
            SpawnCustomer();
            timer = 0f;
        }
    }

    public void UpdateCustomerFlow(float globalMultiplier)
    {
        float spawnMultiplier = Mathf.Pow(globalMultiplier, 2.5f); 
        currentSpawnInterval = baseSpawnInterval / spawnMultiplier;
        currentSpawnInterval = Mathf.Max(currentSpawnInterval, 0.15f);
        currentCustomerSpeed = baseCustomerSpeed * globalMultiplier;
    }

    void SpawnCustomer()
    {
        GameObject go = Instantiate(customerPrefab, spawnPoint.position, Quaternion.identity);
        Customer newCustomer = go.GetComponent<Customer>();

        // HATA DÜZELTİLDİ: newCustomerScript yerine newCustomer kullanıldı
        if (isDarkShiftActive)
        {
            newCustomer.EvolveToDarkShift();
        }
        
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

    public void SpawnFloatingText(Vector3 spawnPosition, string textValue)
    {
        if (floatingTextPrefab != null)
        {
            Vector3 offsetPosition = spawnPosition + new Vector3(0, 1f, 0);
            GameObject popup = Instantiate(floatingTextPrefab, offsetPosition, Quaternion.identity);
            popup.GetComponent<FloatingText>().Setup(textValue);
        }
    }

    public void ActivateDarkShift()
    {
        isDarkShiftActive = true;
        Customer[] allActiveCustomers = Object.FindObjectsByType<Customer>(FindObjectsSortMode.None);
        foreach (Customer c in allActiveCustomers)
        {
            c.EvolveToDarkShift();
        }
    }
}