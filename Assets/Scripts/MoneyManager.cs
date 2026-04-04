using UnityEngine;
using TMPro; // TextMeshPro (UI) kütüphanesini kullanacağız

public class MoneyManager : MonoBehaviour
{
    // Singleton yapısı: Bu scripte diğer tüm scriptlerden kolayca ulaşmamızı sağlar
    public static MoneyManager Instance;

    [Header("Economy Stats")]
    // Idle oyunlarında sayılar çok büyüyeceği için int yerine double kullanıyoruz
    public double currentMoney = 0; 
    public double coffeePrice = 10; // Ürünün başlangıç satış fiyatı

    [Header("UI Elements")]
    public TextMeshProUGUI moneyText;

    void Awake()
    {
        // Klasik Singleton kurulumu
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        UpdateUI();
    }

    // Para kazanıldığında çağrılacak fonksiyon
    public void AddMoney(double amount)
    {
        currentMoney += amount;
        UpdateUI();
    }

    // İleride upgrade (yükseltme) yaparken kullanılacak fonksiyon
    public bool SpendMoney(double amount)
    {
        if (currentMoney >= amount)
        {
            currentMoney -= amount;
            UpdateUI();
            return true; // Satın alma başarılı
        }
        return false; // Yeterli para yok
    }

    // Ekrandaki yazıyı güncelleyen fonksiyon
    private void UpdateUI()
    {
        if (moneyText != null)
        {
            // Şimdilik sadece tam sayı olarak yazdırıyoruz (F0). 
            // İleride buraya 1K, 1M, 1B gibi kısaltma formatları ekleyeceğiz.
            moneyText.text = "$" + currentMoney.ToString("F0"); 
        }
    }
}