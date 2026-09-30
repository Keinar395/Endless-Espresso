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
    public float speedStep = 0.1f; 

    [Header("Machine Upgrade")]
    public double machineCost = 30; 
    public float machineCostMultiplier = 1.8f;
    public TextMeshProUGUI machineCostText;
    public GameObject machinePrefab; 
    public List<Machine> activeMachines = new List<Machine>(); 
    public float machineSpacingOffset = 1f; 

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
            firstEmp.UpdateWorkStats(globalSpeedMultiplier); 
        }

        if (productTiers.Length > 0)
        {
            MoneyManager.Instance.coffeePrice = productTiers[0].basePrice;
        }

        CustomerManager.Instance.UpdateCustomerFlow(globalSpeedMultiplier);
        UpdateUI();
    }

    public void BuyNewMachine()
    {
        if (MoneyManager.Instance.SpendMoney(machineCost))
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.upgradeClip); // BAŞARILI
            
            Machine lastMachine = activeMachines[activeMachines.Count - 1];
            Vector3 newPosition = lastMachine.transform.position + new Vector3(machineSpacingOffset, 0, 0);

            GameObject newMachine = Instantiate(machinePrefab, newPosition, Quaternion.identity);
            Machine machineScript = newMachine.GetComponent<Machine>();
            machineScript.isOccupied = false; 
            
            activeMachines.Add(machineScript);

            machineCost = System.Math.Round(machineCost * machineCostMultiplier);
            UpdateUI();
        }
        else 
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.cannotBuyClip); // HATA SESİ
        }
    }

    public Machine GetAvailableMachine()
    {
        foreach (Machine m in activeMachines)
        {
            if (!m.isOccupied) return m;
        }
        return null; 
    }

    public void HireEmployee()
    {
        if (MoneyManager.Instance.SpendMoney(hireCost))
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.upgradeClip); // BAŞARILI

            GameObject newEmp = Instantiate(employeePrefab, employeeSpawnPoint.position, Quaternion.identity);
            EmployeeController empScript = newEmp.GetComponent<EmployeeController>();
            
            empScript.UpdateWorkStats(globalSpeedMultiplier); 
            empScript.currentState = EmployeeController.EmployeeState.Idle; 

            activeEmployees.Add(empScript);
            hireCost = System.Math.Round(hireCost * hireCostMultiplier);
            UpdateUI();
        }
        else 
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.cannotBuyClip); // HATA SESİ
        }
    }

    public void UpgradeMoveSpeed()
    {
        if (MoneyManager.Instance.SpendMoney(moveSpeedCost))
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.upgradeClip); // BAŞARILI

            globalSpeedMultiplier += speedStep;

            foreach (EmployeeController emp in activeEmployees)
            {
                emp.UpdateWorkStats(globalSpeedMultiplier);
            }
            
            CustomerManager.Instance.UpdateCustomerFlow(globalSpeedMultiplier);

            moveSpeedCost = System.Math.Round(moveSpeedCost * moveSpeedCostMultiplier);
            UpdateUI();
        }
        else 
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.cannotBuyClip); // HATA SESİ
        }
    }

    public void EvolveProduct()
    {
        if (currentProductTier < productTiers.Length - 1)
        {
            double cost = productTiers[currentProductTier].evolutionCost;
            if (MoneyManager.Instance.SpendMoney(cost))
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.evolveClip); // EVRİM SESİ

                currentProductTier++; 
                MoneyManager.Instance.coffeePrice = productTiers[currentProductTier].basePrice;
                UpdateUI();
            }
            else 
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.cannotBuyClip); // HATA SESİ
            }
        }
    }

    private void UpdateUI()
    {
        if (moveSpeedCostText != null) moveSpeedCostText.text ="Increase Speed:" + "$" + moveSpeedCost.ToString("F0");
        if (hireCostText != null) hireCostText.text ="Hire Employee:" + "$" + hireCost.ToString("F0");
        if (machineCostText != null) machineCostText.text = "Buy Machine:" + "$" + machineCost.ToString("F0"); 

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