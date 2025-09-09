using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Building : MonoBehaviour
{
    [Header("건물 정보")]
    public BuildingTypes BuildingType;
    public string BuildingName = "건물";

    [System.Serializable]
    public class BuildingEvents
    {
        public UnityEvent<string> OnDriverEntered;
        public UnityEvent<string> OnDriverExited;
        public UnityEvent<BuildingTypes> OnServiceUsed;
    }

    public BuildingEvents buildingEvents;
    // Start is called before the first frame update
    void Start()
    {
        SetupBuilding();
    }

    void SetupBuilding()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            Material mat = renderer.material;
            switch (BuildingType)
            {
                case BuildingTypes.Rostaurant:
                    mat.color = Color.red;
                    BuildingName = "음식점";
                    break;

                case BuildingTypes.Coustomer:
                    mat.color = Color.green;
                    BuildingName = "고객 집";
                    break;

                case BuildingTypes.ChargingStation:
                    mat.color = Color.yellow;
                    BuildingName = "충전소";
                    break;
            }
        }

        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }

    }
    void HandleDriverService(DeliveryDriver driver)
    {
        switch (BuildingType)
        {
            case BuildingTypes.Rostaurant:
                Debug.Log($"{BuildingName} 에서 음식을 픽업했습니다");
                break;
            case BuildingTypes.Coustomer:
                Debug.Log($"{BuildingName} 에서 배달 완료");
                driver.CompleteDelivery();
                break;
            case BuildingTypes.ChargingStation:
                Debug.Log($"{BuildingName} 에서 배터리 충전 했습니다");
                driver.ChargeBattery();
                break;
        }
    }
    void OnTriggerEnter(Collider other)
    {
        DeliveryDriver driver = other.GetComponent<DeliveryDriver>();
        if (driver != null)
        {
            buildingEvents.OnDriverEntered?.Invoke(BuildingName);
            HandleDriverService(driver);
        }
    }

    void OnTriggerExit(Collider other)
    {
        DeliveryDriver driver = other.GetComponent<DeliveryDriver>();
        if (driver != null)
        {
            buildingEvents.OnDriverExited?.Invoke(BuildingName);
            Debug.Log($"{BuildingName} 을 떠났습니다.");
        }
    }
}
