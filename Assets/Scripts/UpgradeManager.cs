using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance;

    [Header("Global Employee Stats")]
    public float currentGlobalMoveSpeed = 5f; // Dükkandaki herkesin ortak hızı
    public List<EmployeeController> activeEmployees = new List<EmployeeController>(); // Sahnedeki tüm çalışanlar
    
    [Header("Hire Employee Upgrade")]
    public double hireCost = 50;
    public float hireCostMultiplier = 2.5f;
    public TextMeshProUGUI hireCostText;
    public GameObject employeePrefab;
    public Transform employeeSpawnPoint; // Yeni işçilerin doğacağı yer

    [Header("Move Speed Upgrade")]
    public double moveSpeedCost = 15;
    public float moveSpeedIncrease = 1f;
    public float moveSpeedCostMultiplier = 1.5f;
    public TextMeshProUGUI moveSpeedCostText;

    [Header("Evolution (Dark Shift) System")]
    public int currentProductTier = 0; // Şu an hangi üründeyiz? (0 = Kahve)
    public TextMeshProUGUI productNameText; // Ekranda sattığımız ürünün adını göstermek için
    public TextMeshProUGUI evolutionCostText;
    
    // Ürünlerin listesi (Inspector'dan doldurulacak)
    public ProductTier[] productTiers; 

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Oyundaki ilk elemanı (sahnedekini) listeye ekle ve hızını ayarla
        EmployeeController firstEmp = Object.FindFirstObjectByType<EmployeeController>();
        if (firstEmp != null)
        {
            activeEmployees.Add(firstEmp);
            firstEmp.moveSpeed = currentGlobalMoveSpeed;
        }

        // Oyun başladığında ilk ürünün fiyatını MoneyManager'a aktar
        if (productTiers.Length > 0)
        {
            MoneyManager.Instance.coffeePrice = productTiers[0].basePrice;
        }

        UpdateUI();
    }

    // 1. ELEMAN İŞE ALMA FONKSİYONU
    public void HireEmployee()
    {
        if (MoneyManager.Instance.SpendMoney(hireCost))
        {
            // Yeni çalışanı yarat
            GameObject newEmp = Instantiate(employeePrefab, employeeSpawnPoint.position, Quaternion.identity);
            EmployeeController empScript = newEmp.GetComponent<EmployeeController>();
            
            // Yeni çalışanın hızını güncel global hıza eşitle ki yavaş kalmasın
            empScript.moveSpeed = currentGlobalMoveSpeed;
            
            // Listeye ekle
            activeEmployees.Add(empScript);

            // Maliyeti artır ve UI güncelle
            hireCost = System.Math.Round(hireCost * hireCostMultiplier);
            UpdateUI();
            
            Debug.Log("Yeni eleman işe alındı! Toplam eleman: " + activeEmployees.Count);
        }
    }

    // 2. HIZ ARTIRMA FONKSİYONU (Sınırsız)
    public void UpgradeMoveSpeed()
    {
        if (MoneyManager.Instance.SpendMoney(moveSpeedCost))
        {
            // Global hızı artır
            currentGlobalMoveSpeed += moveSpeedIncrease;

            // Sahnedeki TÜM çalışanların hızını anında güncelle
            foreach (EmployeeController emp in activeEmployees)
            {
                emp.moveSpeed = currentGlobalMoveSpeed;
            }

            moveSpeedCost = System.Math.Round(moveSpeedCost * moveSpeedCostMultiplier);
            UpdateUI();
        }
    }

    // 3. EVRİM / ÇAĞ ATLAMA FONKSİYONU
    public void EvolveProduct()
    {
        // Eğer hala evrimleşebileceğimiz bir ürün varsa
        if (currentProductTier < productTiers.Length - 1)
        {
            double cost = productTiers[currentProductTier].evolutionCost;

            if (MoneyManager.Instance.SpendMoney(cost))
            {
                currentProductTier++; // Bir sonraki karanlık ürüne geç
                
                // Yeni ürünün devasa satış fiyatını MoneyManager'a bildir
                MoneyManager.Instance.coffeePrice = productTiers[currentProductTier].basePrice;
                
                UpdateUI();

                Debug.Log("DÜKKAN EVRİM GEÇİRDİ! Yeni Ürün: " + productTiers[currentProductTier].productName);
                
                // İLERİSİ İÇİN NOT: Burada tüm dükkanın rengini, makinelerin sprite'larını 
                // değiştirecek bir "DarkShiftEvent" tetikleyebilirsin.
            }
        }
    }

    private void UpdateUI()
    {
        if (moveSpeedCostText != null) moveSpeedCostText.text = "$" + moveSpeedCost.ToString("F0");
        if (hireCostText != null) hireCostText.text = "$" + hireCost.ToString("F0");

        // Evrim UI Güncellemesi
        if (productTiers.Length > 0 && currentProductTier < productTiers.Length)
        {
            if (productNameText != null) 
                productNameText.text = "Satılan: " + productTiers[currentProductTier].productName;

            if (evolutionCostText != null)
            {
                // Eğer son üründeysek (Oyunun sonu/Maksimum seviye)
                if (currentProductTier >= productTiers.Length - 1)
                    evolutionCostText.text = "MAX";
                else
                    evolutionCostText.text = "$" + productTiers[currentProductTier].evolutionCost.ToString("F0");
            }
        }
    }
}

// Ürünlerin özelliklerini Inspector'da liste halinde görebilmemizi sağlayan yapı
[System.Serializable]
public struct ProductTier
{
    public string productName; // Ürünün Adı (Örn: "Espresso", "Kanlı Smoothie")
    public double basePrice;   // Bu ürünün müşteriye satış fiyatı
    public double evolutionCost; // BU ÜRÜNDEN bir sonrakine geçmek için gereken devasa para
}