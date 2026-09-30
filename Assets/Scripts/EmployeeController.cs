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
        moveSpeed = baseMoveSpeed;
        orderTakeTime = baseOrderTime;
        brewTime = baseBrewTime;
    }

    void Start()
    {
        currentState = EmployeeState.Idle;
    }

    public void UpdateWorkStats(float multiplier)
    {
        moveSpeed = baseMoveSpeed * multiplier;
        orderTakeTime = Mathf.Max(baseOrderTime / multiplier, 0.05f);
        brewTime = Mathf.Max(baseBrewTime / multiplier, 0.05f);
        rotationSpeed = 720f * multiplier; 
    }

    void Update()
    {
        // GÜVENLİK (Müşteri kaybolursa)
        if (currentState != EmployeeState.Idle && assignedCustomer == null)
        {
            if (currentMachine != null) 
            {
                currentMachine.StopBrewing(); // SESİ KES
                currentMachine.isOccupied = false;
            }
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
                    assignedCustomer.isBeingServed = true; 
                    currentState = EmployeeState.MovingToCustomer;
                }
                break;

            case EmployeeState.MovingToCustomer:
                MoveToTarget(assignedCustomer.transform.position, EmployeeState.TakingOrder);
                break;

            case EmployeeState.TakingOrder:
                timer += Time.deltaTime;
                if (timer >= orderTakeTime)
                {
                    currentMachine = UpgradeManager.Instance.GetAvailableMachine();
                    if (currentMachine != null)
                    {
                        currentMachine.isOccupied = true; 
                        timer = 0f; 
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
                    if (currentMachine != null) 
                    {
                        currentMachine.StopBrewing(); // SESİ DURDUR (Kahve dökülmesi bitti)
                        currentMachine.isOccupied = false;
                    }
                    
                    // BİTİŞ (Beep) SESİNİ ÇAL
                    AudioManager.Instance.PlaySFXRandomPitch(AudioManager.Instance.brewDoneClip);
                    
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
        Vector2 direction = target - transform.position;
        if (direction.sqrMagnitude > 0.001f)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0, 0, angle - 90f), rotationSpeed * Time.deltaTime);
        }

        transform.position = Vector2.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
        float distance = Vector2.Distance(transform.position, target);

        if (currentState == EmployeeState.MovingToDeliver)
        {
            if (distance < 2f)
            {
                // TESLİMAT: PARA KAZANMA SESİ ÇAL
                AudioManager.Instance.PlaySFXRandomPitch(AudioManager.Instance.moneyClip);
                
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
                // MÜŞTERİYE VARILDI: SİPARİŞ ALMA SESİ ÇAL
                AudioManager.Instance.PlaySFXRandomPitch(AudioManager.Instance.takeOrderClip);
                
                timer = 0f;
                currentState = nextState;
            }
        }
        else if (currentState == EmployeeState.MovingToMachine)
        {
            if (distance < 1f)
            {
                // MAKİNEYE VARILDI: ÇALIŞMA (LOOP) SESİNİ BAŞLAT
                currentMachine.StartBrewing(AudioManager.Instance.brewingLoopClip);
                
                timer = 0f;
                currentState = nextState;
            }
        }
    }
}