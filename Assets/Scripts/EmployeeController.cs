using UnityEngine;

public class EmployeeController : MonoBehaviour
{
    public enum EmployeeState
    {
        Idle,
        MovingToCustomer,
        TakingOrder,
        MovingToMachine,
        BrewingCoffee,
        MovingToDeliver
    }

    [Header("State")]
    public EmployeeState currentState;

    [Header("Current Stats (Anlık Değişenler)")]
    public float moveSpeed;
    public float orderTakeTime;
    public float brewTime;
    public float rotationSpeed = 720f;

    [Header("Base Stats (Sabit Temel Değerler)")]
    public float baseMoveSpeed = 5f;
    public float baseOrderTime = 1f;
    public float baseBrewTime = 2f;

    [Header("Current Task")]
    private Customer assignedCustomer;
    private Machine currentMachine;

    private float timer = 0f;

    void Awake()
    {
        // Başlangıçta hızları temele eşitliyoruz
        moveSpeed = baseMoveSpeed;
        orderTakeTime = baseOrderTime;
        brewTime = baseBrewTime;
    }

    void Start()
    {
        currentState = EmployeeState.Idle;
    }

    // UpgradeManager tarafından hızlandırıldığımızda çağrılır
    public void UpdateWorkStats(float multiplier)
    {
        moveSpeed = baseMoveSpeed * multiplier;
        
        // GÜVENLİK SINIRI: Süreler ne kadar kısalırsa kısalsın 0.05 saniyenin altına düşmesin (Bug olmaması için)
        orderTakeTime = Mathf.Max(baseOrderTime / multiplier, 0.05f);
        brewTime = Mathf.Max(baseBrewTime / multiplier, 0.05f);
        
        // BONUS: Yürüme hızı arttıkça, dönüş (yönelme) hızı da artsın ki robot gibi donuk kalmasınlar
        rotationSpeed = 720f * multiplier; 
    }

    void Update()
    {
        // GÜVENLİK: Eğer atanmış müşteri bir şekilde yok olursa (Sahneden silinirse vb.)
        if (currentState != EmployeeState.Idle && assignedCustomer == null)
        {
            // Elinde kilitlediği bir makine varsa serbest bırak
            if (currentMachine != null) currentMachine.isOccupied = false;
            
            currentState = EmployeeState.Idle;
            timer = 0f;
            return;
        }

        switch (currentState)
        {
            case EmployeeState.Idle:
                Customer potentialCustomer = CustomerManager.Instance.TryGetAvailableCustomer();
    
                if (potentialCustomer != null)
                {
                    assignedCustomer = potentialCustomer;
                    assignedCustomer.isBeingServed = true; // Hemen mühürle
                    currentState = EmployeeState.MovingToCustomer;
                }
                break;

            case EmployeeState.MovingToCustomer:
                MoveToTarget(assignedCustomer.transform.position, EmployeeState.TakingOrder);
                break;

            case EmployeeState.TakingOrder:
                timer += Time.deltaTime;
                // Sipariş alma süresi dolduysa
                if (timer >= orderTakeTime)
                {
                    // Boş makine bulmaya çalış
                    currentMachine = UpgradeManager.Instance.GetAvailableMachine();
                    
                    if (currentMachine != null)
                    {
                        currentMachine.isOccupied = true; // Makineyi rezerve et
                        timer = 0f; // SADECE makine bulursak sıfırlıyoruz!
                        currentState = EmployeeState.MovingToMachine;
                    }
                }
                break;

            case EmployeeState.MovingToMachine:
                MoveToTarget(currentMachine.transform.position, EmployeeState.BrewingCoffee);
                break;

            case EmployeeState.BrewingCoffee:
                timer += Time.deltaTime;
                if (timer >= brewTime)
                {
                    timer = 0f;
                    if (currentMachine != null) currentMachine.isOccupied = false;
                    currentMachine = null;
                    currentState = EmployeeState.MovingToDeliver;
                }
                break;

            case EmployeeState.MovingToDeliver:
                MoveToTarget(assignedCustomer.transform.position, EmployeeState.Idle);
                break;
        }
    }

    private void MoveToTarget(Vector3 target, EmployeeState nextState)
    {
        // --- Rotasyon Mantığı ---
        Vector2 direction = target - transform.position;
        if (direction.sqrMagnitude > 0.001f)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0, 0, angle - 90f), rotationSpeed * Time.deltaTime);
        }

        // --- Hareket Mantığı ---
        transform.position = Vector2.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);

        // --- Mesafe ve Varış Kontrolleri ---
        float distance = Vector2.Distance(transform.position, target);

        if (currentState == EmployeeState.MovingToDeliver)
        {
            if (distance < 2f)
            {
                MoneyManager.Instance.AddMoney(MoneyManager.Instance.coffeePrice);
                CustomerManager.Instance.ServeSpecificCustomer(assignedCustomer);
                assignedCustomer = null; 
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
}