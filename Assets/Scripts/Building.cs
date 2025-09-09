using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Building : MonoBehaviour
{
    [Header("�ǹ� ����")]
    public BuildingTypes BuildingType;
    public string BuildingName = "�ǹ�";

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
                    BuildingName = "������";
                    break;

                case BuildingTypes.Coustomer:
                    mat.color = Color.green;
                    BuildingName = "�� ��";
                    break;

                case BuildingTypes.ChargingStation:
                    mat.color = Color.yellow;
                    BuildingName = "������";
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
                Debug.Log($"{BuildingName} ���� ������ �Ⱦ��߽��ϴ�");
                break;
            case BuildingTypes.Coustomer:
                Debug.Log($"{BuildingName} ���� ��� �Ϸ�");
                driver.CompleteDelivery();
                break;
            case BuildingTypes.ChargingStation:
                Debug.Log($"{BuildingName} ���� ���͸� ���� �߽��ϴ�");
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
            Debug.Log($"{BuildingName} �� �������ϴ�.");
        }
    }
}
