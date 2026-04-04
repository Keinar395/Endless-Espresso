using UnityEngine;

public class EmployeeController : MonoBehaviour
{
    public enum EmployeeState
    {
        MovingToCustomer, // İlk siparişi almaya giderken
        TakingOrder,      // Sipariş alma süreci
        MovingToMachine,  // Makineye giderken
        BrewingCoffee,    // Kahve yapma süreci
        MovingToDeliver   // Kahveyi müşteriye geri götürürken (Yeni Durum)
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

    private float timer = 0f;

    void Start()
    {
        currentState = EmployeeState.MovingToCustomer;
    }

    void Update()
    {
        switch (currentState)
        {
            case EmployeeState.MovingToCustomer:
                MoveToTarget(customerPoint.position, EmployeeState.TakingOrder);
                break;
                
            case EmployeeState.TakingOrder:
                ProcessTask(orderTakeTime, EmployeeState.MovingToMachine);
                break;
                
            case EmployeeState.MovingToMachine:
                MoveToTarget(machinePoint.position, EmployeeState.BrewingCoffee);
                break;
                
            case EmployeeState.BrewingCoffee:
                // Kahve bitince parayı hemen vermiyoruz, teslimata geçiyoruz
                ProcessTask(brewTime, EmployeeState.MovingToDeliver);
                break;

            case EmployeeState.MovingToDeliver:
                // Tekrar müşteri noktasına gidiyoruz
                MoveToTarget(customerPoint.position, EmployeeState.TakingOrder);
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
            // Kahveyi teslim ettiğimiz an parayı kazanıyoruz
            if (distance < 2f)
            {
                MoneyManager.Instance.AddMoney(MoneyManager.Instance.coffeePrice);
                timer = 0f;
                currentState = nextState; // Tekrar TakingOrder'a döner
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