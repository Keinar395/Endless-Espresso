using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance;

    [Header("Global Employee Stats")]
    public float currentGlobalMoveSpeed = 5f; 
    public List<EmployeeController> activeEmployees = new List<EmployeeController>(); 
    
    [Header("Hire Employee Upgrade")]
    public double hireCost = 50;
    public float hireCostMultiplier = 2.5f;
    public TextMeshProUGUI hireCostText;
    public GameObject employeePrefab;
    public Transform employeeSpawnPoint; 

    [Header("Move Speed Upgrade")]
    public double moveSpeedCost = 15;
    public float moveSpeedCostMultiplier = 1.5f;
    public TextMeshProUGUI moveSpeedCostText;
    public float globalSpeedMultiplier = 1f; 
    public float speedStep = 0.1f; // Her tıkta hızı %10 artırır

    [Header("Machine Upgrade (Yatay Genişleme)")]
    public double machineCost = 30; // Makine alma bedeli
    public float machineCostMultiplier = 1.8f;
    public TextMeshProUGUI machineCostText;
    public GameObject machinePrefab; // Makinenin kendisi (Kare)
   public List<Machine> activeMachines = new List<Machine>(); // Sahnede var olan tüm makineler
    [Tooltip("Yeni makine eskisinin ne kadar uzağına yerleşecek?")]
    public float machineSpacingOffset = 1f; // X ekseninde sağa doğru kayma miktarı

    [Header("Evolution (Dark Shift) System")]
    public int currentProductTier = 0; 
    public TextMeshProUGUI productNameText; 
    public TextMeshProUGUI evolutionCostText;
    public ProductTier[] productTiers; 

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        EmployeeController firstEmp = Object.FindFirstObjectByType<EmployeeController>();
        if (firstEmp != null)
        {
            activeEmployees.Add(firstEmp);
            firstEmp.UpdateWorkStats(globalSpeedMultiplier); // DOĞRUSU BU
        }

        if (productTiers.Length > 0)
        {
            MoneyManager.Instance.coffeePrice = productTiers[0].basePrice;
        }

        UpdateUI();
    }

    // --- YENİ EKLENEN MAKİNE SATIN ALMA FONKSİYONU ---
    public void BuyNewMachine()
    {
        if (MoneyManager.Instance.SpendMoney(machineCost))
        {
            // DÜZELTME 1: Artık listedeki eleman Transform değil, Machine tipinde
            Machine lastMachine = activeMachines[activeMachines.Count - 1];
            
            // lastMachine.transform.position diyerek pozisyonunu alıyoruz
            Vector3 newPosition = lastMachine.transform.position + new Vector3(machineSpacingOffset, 0, 0);

            GameObject newMachine = Instantiate(machinePrefab, newPosition, Quaternion.identity);
            
            // DÜZELTME 2: GameObject'i direkt listeye atamayız. 
            // İçindeki "Machine" scriptini alıp listeye ekliyoruz.
            activeMachines.Add(newMachine.GetComponent<Machine>());

            machineCost = System.Math.Round(machineCost * machineCostMultiplier);
            UpdateUI();

            Debug.Log("Yeni makine alındı! Toplam makine: " + activeMachines.Count);
        }
    }

    // Çalışanların boş bir makine bulması için kullanılacak fonksiyon
    public Machine GetAvailableMachine()
{
    foreach (Machine m in activeMachines)
    {
        if (!m.isOccupied) return m;
    }
    return null; // Tüm makineler doluysa null döner
}

    // (Eski Fonksiyonlar Aynen Kalıyor)
    public void HireEmployee()
    {
        if (MoneyManager.Instance.SpendMoney(hireCost))
        {
            GameObject newEmp = Instantiate(employeePrefab, employeeSpawnPoint.position, Quaternion.identity);
            EmployeeController empScript = newEmp.GetComponent<EmployeeController>();
            // Yeni çalışan doğduğunda anında güncel hız çarpanını alsın
            empScript.UpdateWorkStats(globalSpeedMultiplier); // DOĞRUSU BU
            activeEmployees.Add(empScript);
            hireCost = System.Math.Round(hireCost * hireCostMultiplier);
            UpdateUI();
        }
    }

    public void UpgradeMoveSpeed()
{
    if (MoneyManager.Instance.SpendMoney(moveSpeedCost))
    {
        // Çarpanı artır (Örn: 1.0 -> 1.1 -> 1.2)
        globalSpeedMultiplier += speedStep;

        // Tüm çalışanlara yeni hızı ve süreleri güncellemesini söyle
        foreach (EmployeeController emp in activeEmployees)
        {
            emp.UpdateWorkStats(globalSpeedMultiplier);
        }

        moveSpeedCost = System.Math.Round(moveSpeedCost * moveSpeedCostMultiplier);
        UpdateUI();
    }
}

    public void EvolveProduct()
    {
        if (currentProductTier < productTiers.Length - 1)
        {
            double cost = productTiers[currentProductTier].evolutionCost;
            if (MoneyManager.Instance.SpendMoney(cost))
            {
                currentProductTier++; 
                MoneyManager.Instance.coffeePrice = productTiers[currentProductTier].basePrice;
                UpdateUI();
                Debug.Log("DÜKKAN EVRİM GEÇİRDİ! Yeni Ürün: " + productTiers[currentProductTier].productName);
            }
        }
    }

    private void UpdateUI()
    {
        if (moveSpeedCostText != null) moveSpeedCostText.text ="Increase Speed:" + "$" + moveSpeedCost.ToString("F0");
        if (hireCostText != null) hireCostText.text ="Hire Employee:" + "$" + hireCost.ToString("F0");
        if (machineCostText != null) machineCostText.text = "Buy Machine:" + "$" + machineCost.ToString("F0"); // Yeni eklendi

        if (productTiers.Length > 0 && currentProductTier < productTiers.Length)
        {
            if (productNameText != null) 
                productNameText.text = productTiers[currentProductTier].productName;

            if (evolutionCostText != null)
            {
                if (currentProductTier >= productTiers.Length - 1)
                    evolutionCostText.text = "MAX";
                else
                    evolutionCostText.text = "$" + productTiers[currentProductTier].evolutionCost.ToString("F0");
            }
        }
    }

    public void CheatAddMoney()
    {
        MoneyManager.Instance.AddMoney(1000000f);
        UpdateUI();
    }
}

[System.Serializable]
public struct ProductTier
{
    public string productName; 
    public double basePrice;   
    public double evolutionCost; 
}