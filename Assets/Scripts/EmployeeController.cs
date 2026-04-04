using UnityEngine;

public class EmployeeController : MonoBehaviour
{
    public enum EmployeeState
    {
        Idle, // YENİ: Boşta bekleme durumu
        MovingToCustomer,
        TakingOrder,
        MovingToMachine,
        BrewingCoffee,
        MovingToDeliver
    }

    [Header("State")]
    public EmployeeState currentState;

    [Header("References")]
    public Transform customerPoint;
    public Transform machinePoint;

    [Header("Movement Stats")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 720f;

    [Header("Work Stats")]
    public float orderTakeTime = 2f;
    public float brewTime = 3f;

    [Header("Current Task")]
    private Customer assignedCustomer;

    private float timer = 0f;

    void Start()
    {
        currentState = EmployeeState.Idle; // Artık oyun başladığında bekliyor
    }

    void Update()
    {
        switch (currentState)
        {
            case EmployeeState.Idle:
                // Müşteri hazır mı diye bak
                if (CustomerManager.Instance.IsCustomerReadyToOrder())
                {
                    // Sıradaki müşteriyi "rezerve" et ve değişkene ata
                    assignedCustomer = CustomerManager.Instance.GetFirstCustomer();
                    currentState = EmployeeState.MovingToCustomer;
                }
                break;
                
            case EmployeeState.MovingToCustomer:
                // DÜZELTME 1: Artık rezerve ettiğimiz müşterinin direkt kendi pozisyonuna gidiyoruz
                MoveToTarget(assignedCustomer.transform.position, EmployeeState.TakingOrder);
                break;
                
            case EmployeeState.TakingOrder:
                ProcessTask(orderTakeTime, EmployeeState.MovingToMachine);
                break;
                
            case EmployeeState.MovingToMachine:
                MoveToTarget(machinePoint.position, EmployeeState.BrewingCoffee);
                break;
                
            case EmployeeState.BrewingCoffee:
                // Kahve bitince teslimata geçiyoruz
                ProcessTask(brewTime, EmployeeState.MovingToDeliver);
                break;

            case EmployeeState.MovingToDeliver:
                // DÜZELTME 2: Eski sabit 'customerPoint' yerine rezerve ettiğimiz müşteriye gidiyoruz.
                // DÜZELTME 3: İşi bitince 'TakingOrder' değil, 'Idle' (boşta bekleme) durumuna geçmesini söylüyoruz.
                MoveToTarget(assignedCustomer.transform.position, EmployeeState.Idle);
                break;
        }
    }

    private void MoveToTarget(Vector3 target, EmployeeState nextState)
    {
        // Rotasyon Mantığı
        Vector2 direction = target - transform.position;
        if (direction.sqrMagnitude > 0.001f)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0, 0, angle - 90f), rotationSpeed * Time.deltaTime);
        }

        // Hareket Mantığı
        transform.position = Vector2.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);

        // Mesafe Kontrolleri (Senin mantığınla güncellendi)
        float distance = Vector2.Distance(transform.position, target);

        if (currentState == EmployeeState.MovingToDeliver)
        {
            if (distance < 2f)
            {
                MoneyManager.Instance.AddMoney(MoneyManager.Instance.coffeePrice);
                
                // Müşteri yöneticisine "bu spesifik müşteriyi" gönder diyoruz
                CustomerManager.Instance.ServeSpecificCustomer(assignedCustomer);
                
                assignedCustomer = null; // İş bitti, hafızayı boşalt
                timer = 0f;
                currentState = nextState;
            }
        }
        else if (currentState == EmployeeState.MovingToCustomer)
        {
            if (distance < 2f)
            {
                timer = 0f;
                currentState = nextState;
            }
        }
        else if (currentState == EmployeeState.MovingToMachine)
        {
            if (distance < 1f)
            {
                timer = 0f;
                currentState = nextState;
            }
        }
    }

    private void ProcessTask(float duration, EmployeeState nextState)
    {
        timer += Time.deltaTime;
        if (timer >= duration)
        {
            timer = 0f;
            currentState = nextState;
        }
    }
}