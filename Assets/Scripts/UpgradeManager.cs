using UnityEngine;
using TMPro; // UI Textleri için

public class UpgradeManager : MonoBehaviour
{
    [Header("References")]
    public EmployeeController employee; // Hızını artıracağımız çalışan
    
    [Header("Move Speed Upgrade")]
    public double moveSpeedCost = 15; // İlk yükseltme bedeli
    public float moveSpeedIncrease = 1f; // Her yükseltmede artacak hız miktarı
    public float moveSpeedCostMultiplier = 1.5f; // Sonraki yükseltme ne kadar pahalı olacak?
    public TextMeshProUGUI moveSpeedCostText; // Butonun üstündeki fiyat yazısı

    [Header("Coffee Price Upgrade")]
    public double priceCost = 25;
    public double priceIncrease = 5;
    public float priceCostMultiplier = 1.6f;
    public TextMeshProUGUI priceCostText;

    void Start()
    {
        UpdateUI();
    }

    // Yürüme Hızı Butonuna basıldığında bu çalışacak
    public void UpgradeMoveSpeed()
    {
        // MoneyManager'daki SpendMoney true dönerse (paramız yetiyorsa) işlemi yap
        if (MoneyManager.Instance.SpendMoney(moveSpeedCost))
        {
            // 1. Özelliği geliştir
            employee.moveSpeed += moveSpeedIncrease;
            
            // 2. Yeni maliyeti hesapla (eski maliyet * çarpan)
            moveSpeedCost = System.Math.Round(moveSpeedCost * moveSpeedCostMultiplier);
            
            // 3. Arayüzü güncelle
            UpdateUI();
            
            Debug.Log("Yürüme hızı arttı! Yeni hız: " + employee.moveSpeed);
        }
        else
        {
            Debug.Log("Para yetersiz!");
        }
    }

    // Fiyat Artırma Butonuna basıldığında bu çalışacak
    public void UpgradeCoffeePrice()
    {
        if (MoneyManager.Instance.SpendMoney(priceCost))
        {
            MoneyManager.Instance.coffeePrice += priceIncrease;
            priceCost = System.Math.Round(priceCost * priceCostMultiplier);
            UpdateUI();
            
            Debug.Log("Kahve fiyatı arttı! Yeni fiyat: " + MoneyManager.Instance.coffeePrice);
        }
    }

    // Butonların üzerindeki fiyatları güncelleyen fonksiyon
    private void UpdateUI()
    {
        if (moveSpeedCostText != null)
            moveSpeedCostText.text = "$" + moveSpeedCost.ToString("F0");

        if (priceCostText != null)
            priceCostText.text = "$" + priceCost.ToString("F0");
    }
}