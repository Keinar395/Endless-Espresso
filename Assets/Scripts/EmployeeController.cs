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
    private Transform currentTargetMachine;

    [Header("Current Stats (Anlık Değişenler)")]
    public float moveSpeed; // Bunu sildiğimiz için hata veriyordu, geri ekledik
    public float orderTakeTime;
    public float brewTime;

    [Header("Base Stats (Sabit Temel Değerler)")]
    public float baseMoveSpeed = 5f;
    public float baseOrderTime = 2f;
    public float baseBrewTime = 3f;
    public float rotationSpeed = 360f; // Derece/saniye cinsinden rotasyon hızı

    void Awake()
    {
        // Oyun başladığında anlık süreleri temel sürelere eşitliyoruz
        moveSpeed = baseMoveSpeed;
        orderTakeTime = baseOrderTime;
        brewTime = baseBrewTime;
    }

    // Bir önceki adımda yazdığımız bu fonksiyon aynen kalıyor
    public void UpdateWorkStats(float multiplier)
    {
        moveSpeed = baseMoveSpeed * multiplier;
        orderTakeTime = baseOrderTime / multiplier;
        brewTime = baseBrewTime / multiplier;
    }

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
        if (CustomerManager.Instance.IsCustomerReadyToOrder())
        {
            // Eski 'GetFirstCustomer' yerine yeni yazdığımız fonksiyonu çağırıyoruz
            assignedCustomer = CustomerManager.Instance.GetAvailableCustomer();
            
            // Eğer boşta bir müşteri bulabildiysek
            if (assignedCustomer != null)
            {
                // DİĞER ÇALIŞANLAR ALMASIN DİYE MÜŞTERİYİ KENDİMİZE MÜHÜRLÜYORUZ
                assignedCustomer.isBeingServed = true; 
                
                currentState = EmployeeState.MovingToCustomer;
            }
        }
        break;
                
            case EmployeeState.MovingToCustomer:
                MoveToTarget(assignedCustomer.transform.position, EmployeeState.TakingOrder);
                break;
                
            case EmployeeState.TakingOrder:
                // Siparişi alma süreci bittiğinde makineye gitmeden önce bir makine seç!
                timer += Time.deltaTime;
                if (timer >= orderTakeTime)
                {
                    timer = 0f;
                    // YENİ EKLENEN KISIM: Yöneticiden rastgele bir makine iste
                    currentTargetMachine = UpgradeManager.Instance.GetRandomMachine();
                    currentState = EmployeeState.MovingToMachine;
                }
                break;
                
            case EmployeeState.MovingToMachine:
                // Artık sabit makineye değil, kendi seçtiği makineye gidiyor
                MoveToTarget(currentTargetMachine.position, EmployeeState.BrewingCoffee);
                break;
                
            case EmployeeState.BrewingCoffee:
                ProcessTask(brewTime, EmployeeState.MovingToDeliver);
                break;

            case EmployeeState.MovingToDeliver:
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