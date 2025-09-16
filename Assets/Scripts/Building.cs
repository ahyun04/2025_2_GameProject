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

    private DeliveryOrderSystem orderSystem;
    // Start is called before the first frame update
    void Start()
    {
        SetupBuilding();
        orderSystem = FindAnyObjectByType<DeliveryOrderSystem>();
        CreateNameTag  ();
    }

    void SetupBuilding()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            Material mat = renderer.material;
            switch (BuildingType)
            {
                case BuildingTypes.Restaurant:
                    mat.color = Color.red;
                    break;

                case BuildingTypes.Coustomer:
                    mat.color = Color.green;
                    break;

                case BuildingTypes.ChargingStation:
                    mat.color = Color.yellow;
                    break;
            }
        }
        Collider col = GetComponent<Collider>();
        if (col != null) { col.isTrigger = true; }
    }
    void HandleDriverService(DeliveryDriver driver)
    {
        switch (BuildingType)
        {
            case BuildingTypes.Restaurant:
                if(orderSystem != null)
                {
                    orderSystem.OnDriverEnteredRestaurant(this);
                }
                break;
            case BuildingTypes.Coustomer:
                if (orderSystem != null)
                {
                    orderSystem.OnDliverEnteredCustorm(this);
                }
                else
                {
                    driver.CompleteDelivery();
                }
                break;
            case BuildingTypes.ChargingStation:
                driver.ChargeBattery();
                break;
        }

        buildingEvents.OnServiceUsed?.Invoke(BuildingType);
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

    void CreateNameTag()
    {
        GameObject nameTag = new GameObject("NameTag");
        nameTag.transform.SetParent(transform);
        nameTag.transform.localPosition = Vector3.up * 1.5f;

        TextMesh textMesh = nameTag.AddComponent<TextMesh>();
        textMesh.text = BuildingName;
        textMesh.characterSize = 0.2f;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.color = Color.white;
        textMesh.fontSize = 20;

        nameTag.AddComponent<Bildboard>();
    }
}
