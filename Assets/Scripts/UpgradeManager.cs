using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public struct ProductTier
{
    public string productName; 
    public double basePrice;   
    public double evolutionCost; 
    
    [Header("Görsel & Ses Ayarları")]
    public Sprite tierBackgroundSprite; 
    public AudioClip customEvolveSound; 
    
    [Header("Dark Shift")]
    public bool isDarkShift; 
}

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

    [Header("Evolution & Visual System")]
    public int currentProductTier = 0; 
    public TextMeshProUGUI productNameText; 
    public TextMeshProUGUI evolutionCostText;
    public ProductTier[] productTiers; 
    
    public Camera mainCamera;
    public SpriteRenderer backgroundRenderer; 
    public bool isDarkShiftActive = false; 

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
            if(productTiers[0].tierBackgroundSprite != null && backgroundRenderer != null)
                backgroundRenderer.sprite = productTiers[0].tierBackgroundSprite;
        }

        CustomerManager.Instance.UpdateCustomerFlow(globalSpeedMultiplier);
        UpdateUI();
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
                
                AudioClip tierSound = productTiers[currentProductTier].customEvolveSound;
                if (tierSound != null) AudioManager.Instance.PlaySFX(tierSound);
                else AudioManager.Instance.PlaySFX(AudioManager.Instance.evolveClip); 

                StartCoroutine(CameraShake(0.5f, 0.2f));

                if (productTiers[currentProductTier].tierBackgroundSprite != null && backgroundRenderer != null)
                {
                    backgroundRenderer.sprite = productTiers[currentProductTier].tierBackgroundSprite;
                }

                if (productTiers[currentProductTier].isDarkShift && !isDarkShiftActive)
                {
                    TriggerDarkShift();
                }

                UpdateUI();
            }
            else 
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.cannotBuyClip); 
            }
        }
    }
    
    private IEnumerator CameraShake(float duration, float magnitude)
    {
        if(mainCamera == null) yield break;
        
        Vector3 originalPos = mainCamera.transform.localPosition;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            mainCamera.transform.localPosition = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);
            elapsed += Time.deltaTime;
            yield return null;
        }
        mainCamera.transform.localPosition = originalPos;
    }

    private void TriggerDarkShift()
    {
        isDarkShiftActive = true;
        
        foreach (Machine m in activeMachines) m.EvolveToDarkShift();
        foreach (EmployeeController emp in activeEmployees) emp.EvolveToDarkShift();
        if(CustomerManager.Instance != null) CustomerManager.Instance.ActivateDarkShift();
    }

    public Machine GetAvailableMachine()
    {
        foreach (Machine m in activeMachines)
        {
            if (!m.isOccupied) return m;
        }
        return null; 
    }

    public void BuyNewMachine()
    {
        if (MoneyManager.Instance.SpendMoney(machineCost))
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.upgradeClip); 
            StartCoroutine(CameraShake(0.1f, 0.05f)); 

            Machine lastMachine = activeMachines[activeMachines.Count - 1];
            Vector3 newPosition = lastMachine.transform.position + new Vector3(machineSpacingOffset, 0, 0);

            GameObject newMachine = Instantiate(machinePrefab, newPosition, Quaternion.identity);
            Machine machineScript = newMachine.GetComponent<Machine>();
            machineScript.isOccupied = false; 
            
            if(isDarkShiftActive) machineScript.EvolveToDarkShift();
            
            activeMachines.Add(machineScript);
            machineCost = System.Math.Round(machineCost * machineCostMultiplier);
            UpdateUI();
        }
        else AudioManager.Instance.PlaySFX(AudioManager.Instance.cannotBuyClip); 
    }

    public void HireEmployee()
    {
        if (MoneyManager.Instance.SpendMoney(hireCost))
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.upgradeClip); 
            StartCoroutine(CameraShake(0.1f, 0.05f)); 

            GameObject newEmp = Instantiate(employeePrefab, employeeSpawnPoint.position, Quaternion.identity);
            EmployeeController empScript = newEmp.GetComponent<EmployeeController>();
            
            empScript.UpdateWorkStats(globalSpeedMultiplier); 
            empScript.currentState = EmployeeController.EmployeeState.Idle; 
            
            if(isDarkShiftActive) empScript.EvolveToDarkShift();

            activeEmployees.Add(empScript);
            hireCost = System.Math.Round(hireCost * hireCostMultiplier);
            UpdateUI();
        }
        else AudioManager.Instance.PlaySFX(AudioManager.Instance.cannotBuyClip); 
    }

    public void UpgradeMoveSpeed()
    {
        if (MoneyManager.Instance.SpendMoney(moveSpeedCost))
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.upgradeClip); 
            globalSpeedMultiplier += speedStep;

            foreach (EmployeeController emp in activeEmployees) emp.UpdateWorkStats(globalSpeedMultiplier);
            CustomerManager.Instance.UpdateCustomerFlow(globalSpeedMultiplier);

            moveSpeedCost = System.Math.Round(moveSpeedCost * moveSpeedCostMultiplier);
            UpdateUI();
        }
        else AudioManager.Instance.PlaySFX(AudioManager.Instance.cannotBuyClip); 
    }

    private void UpdateUI()
    {
        if (moveSpeedCostText != null) moveSpeedCostText.text ="Increase Speed: $" + NumberFormatter.Format(moveSpeedCost);
        if (hireCostText != null) hireCostText.text ="Hire Employee: $" + NumberFormatter.Format(hireCost);
        if (machineCostText != null) machineCostText.text = "Buy Machine: $" + NumberFormatter.Format(machineCost); 

        if (productTiers.Length > 0 && currentProductTier < productTiers.Length)
        {
            if (productNameText != null) productNameText.text = productTiers[currentProductTier].productName;

            if (evolutionCostText != null)
            {
                if (currentProductTier >= productTiers.Length - 1) evolutionCostText.text = "MAX";
                else evolutionCostText.text = "$" + NumberFormatter.Format(productTiers[currentProductTier].evolutionCost);
            }
        }
    }

    public void CheatAddMoney()
    {
        MoneyManager.Instance.AddMoney(1000000f);
        UpdateUI();
    }

    
}