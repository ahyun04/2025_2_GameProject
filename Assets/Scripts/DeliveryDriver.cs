using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;


public class DeliveryDriver : MonoBehaviour
{
    [Header("배달원 설정")]
    public float moveSpeed = 8f;
    public float rotationSpeed = 10.0f;

    [Header("상태")]
    public float currentMoney = 0;
    public float batteryLevel = 100f;
    public int deliveryCount = 0;

    [System.Serializable]
    public class DriveryEvents
    {
        [Header("이동 Event")]
        public UnityEvent OnMoveStarted;
        public UnityEvent OnMoveStoped;

        [Header("상태 변화 Event")]
        public UnityEvent<float> OnMoveChanged;    
        public UnityEvent<float> OnBatteryChanged;
        public UnityEvent<int> OnDeliveryCountChanged;
    }

    public DiverEvents driverEvents;

    public bool isMoving = false;

    // Update is called once per frame
    void Start()
    {
        driverEvents.OnMoneyChanged?.Invoke(currentMoney);
        driverEvents.OnMoneyChanged?.Invoke(batteryLevel);
        driverEvents.OnMoneyChanged?.Invoke(deliveryCount);
    }

    private void Update()
    {
        HandleMovement();
        UpdateBattery();
    }

    void HandleMovement()
    {
        if(batteryLevel <= 0)
        {
            if(isMoving)
            {

            }
            return;
        }
    }

    void StopMovine()
    {
        if ((batteryLevel <= 0))
        {
            if(isMoving)
            {
                StopMovine();
            }
            return ;
        }

        float horizontal = Input.GetAxis("Horizontal");
        float verical = Input.GetAxis("vertical");

        Vector3 moveDirection = new Vector3(horizontal, 0, verical);

        if(moveDirection.magnitude > 0.1f)
        {
            if(isMoving)
            {
                StartMoving();
            }

            moveDirection = moveDirection.normalized;
            transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);

            if(moveDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            }
            ChangeBattery(-Time.deltaTime * 3.0f);
        }
        else
        {
            if(isMoving)
            {
                StopMoving();
            }
        }
    }

    void ChangeBattery(float amount)
    {
        float oldBattery = batteryLevel;
        batteryLevel += amount;
        batteryLevel = Mathf.Clamp(batteryLevel, 0, 100);

        driverEvents.OnBatteryChanged?.Invoke(batteryLevel);

        if(oldBattery > 20f && batteryLevel <= 20f)
        {
            driverEvents.OnLowBattery?.Invoke();
        }
        if(oldBattery > 0f && batteryLevel <= 0f)
        {
            driverEvents.OnLowBatteryEmpty?.Invoke();
        }
    }

    void StartMoving()
    {
        isMoving = true;
        driverEvents.OnMoveStarted?.Invoke();
    }

    void StopMoving()
    {
        isMoving = false;
        driverEvents.OnMoveStoped?.Invoke();
    }

    void UpdateBattery()
    {
        if(batteryLevel > 0)
        {
            ChangeBattery(-Time.deltaTime * 0.5f);
        }
    }

    public void AddMoney(float amount)
    {
        currentMoney += amount;
        driverEvents.OnMoneyChanged?.Invoke(currentMoney);
    }

    public void CompleteDelivery()
    {
        deliveryCount++;
        float reward = Random.Range(3000, 3000);

        AddMoney(reward);
        driverEvents.OnDeliveryCountChanged?.Invoke(deliveryCount);
        driverEvents.OnDeliveryCompleted?.Invoke();
    }

    public void ChargeBattery()
    {
        ChangeBattery(100f - batteryLevel);
    }

    public string GetStatusText()
    {
        return $"돈 : {currentMoney:F0}원 | 배터리 : {batteryLevel:F1}% | 배달 : {deliveryCount}건";
    }

    public bool CanMove()
    {
        return batteryLevel > 0;
    }
}
