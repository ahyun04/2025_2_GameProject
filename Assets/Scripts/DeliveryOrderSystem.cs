using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class DeliveryOrderSystem : MonoBehaviour
{
    [Header("주문 설정")]
    public float ordergenratelnterval = 15f;
    public int maxActiveOrders = 8;

    [Header("게임 상태")]
    public int totalOrdersGenerated = 0;
    public int completedOrder = 0;
    public int expiredOrders = 0;

    private List<DeliveryOrder> currentOrders = new List<DeliveryOrder>();

    private List<Building> restaurants = new List<Building>();
    private List<Building> customers = new List<Building>();

    [System.Serializable]
    public class OrderSystemEvents
    {
        public UnityEvent<DeliveryOrder> OnNewOrderAdded;
        public UnityEvent<DeliveryOrder> OnOrderPickedUp;
        public UnityEvent<DeliveryOrder> OnOrderCompleted;
        public UnityEvent<DeliveryOrder> OnOrderExpired;
    }

    public OrderSystemEvents orderEvents;
    public DeliveryDriver driver;

    void Start()
    {
        driver = FindObjectOfType<DeliveryDriver>();
        FindAllBuliding();

        StartCoroutine(GeneratInitialOrders());
        StartCoroutine(orderGenenrator());
        StartCoroutine(ExpiredOrderChecker());
    }

    void FindAllBuliding()
    {
        Building[] allBulidings = FindObjectsOfType<Building>();

        foreach (Building building in allBulidings)
        {
            if (building.BuildingType == BuildingTypes.Restaurant)
            {
                restaurants.Add(building);
            }
            else if (building.BuildingType == BuildingTypes.Coustomer)
            {
                customers.Add(building);
            }
        }
        Debug.Log($"음식점 {restaurants.Count}개, 고객 {customers.Count} 개 발견");
    }

    void CreatNewOrder()
    {
        if (restaurants.Count == 0 || customers.Count == 0)
        {
            return;
        }

        Building randomRestaurant = restaurants[Random.Range(0, restaurants.Count)];
        Building randomCustomer = customers[Random.Range(0, customers.Count)];

        if(randomRestaurant == randomCustomer)
        {
            randomCustomer = customers[Random.Range(0, customers.Count)];
        }

        float reward = Random.Range(3000f, 8000f);

        DeliveryOrder newOrder = new DeliveryOrder(++totalOrdersGenerated, randomRestaurant, randomCustomer, reward);

        currentOrders.Add(newOrder);
        orderEvents.OnNewOrderAdded?.Invoke(newOrder);
    }

    void PickupOrder(DeliveryOrder order)
    {
        order.state = OrderState.PickedUp;
        orderEvents.OnOrderPickedUp?.Invoke(order);
    }

    void CompleteOrder(DeliveryOrder order)
    {
        order.state = OrderState.Completed;
        completedOrder++;

        if(driver != null)
        {
            driver.AddMoney(order.reward);
        }

        currentOrders.Remove(order);
        orderEvents.OnOrderCompleted?.Invoke(order);
    }

    void ExpireOrder(DeliveryOrder order)
    {
        order.state = OrderState.Expired;
        expiredOrders++;

        currentOrders.Remove(order);
        orderEvents.OnOrderExpired?.Invoke(order);
    }

    public List<DeliveryOrder> GetCurrentOrders()
    {
        return new List<DeliveryOrder>(currentOrders);
    }

    public int GetPickWaitingCount()
    {
        int count = 0;
        foreach(DeliveryOrder order in currentOrders)
        {
            if(order.state == OrderState.WaitingPickup) count++;
        }
        return count;
    }

    public int GetDeliveryWaitingCount()
    {
        int count = 0;
        foreach (DeliveryOrder order in currentOrders)
        {
            if(order.state == OrderState.PickedUp) count++;
        }
        return count;
    }

    DeliveryOrder FindOrderForPickup(Building restaurant)
    {
        foreach(DeliveryOrder order in currentOrders)
        {
            if (order.restaurantBuilding == restaurant && order.state == OrderState.WaitingPickup) return order;
        }
        return null;
    }

    DeliveryOrder FindOrderForDelivery(Building customer)
    {
        foreach(DeliveryOrder order in currentOrders)
        {
            if(order.customerBuilding == customer && order.state == OrderState.PickedUp)
            {
                return order;
            }

        }
        return null;
    }

    public void OnDriverEnteredRestaurant(Building restaurant)
    {
        DeliveryOrder orderRoPickup = FindOrderForPickup(restaurant);

        if (orderRoPickup != null)
        {
            PickupOrder(orderRoPickup);
        }
    }

    public void OnDliverEnteredCustorm(Building customer)
    {
        DeliveryOrder orderToDliver = FindOrderForPickup(customer);

        if(orderToDliver != null)
        {
            CompleteOrder(orderToDliver);
        }
    }

    IEnumerator GeneratInitialOrders()
    {
        yield return new WaitForSeconds(1f);

        for(int i = 0; i < 3; i++)
        {
            CreatNewOrder();
            yield return new WaitForSeconds(0.5f);
        }
    }

    IEnumerator orderGenenrator()
    {
        while(true)
        {
            yield return new WaitForSeconds(ordergenratelnterval);

            if(currentOrders.Count < maxActiveOrders )
            {
                CreatNewOrder();
            }
        }
    }

    IEnumerator ExpiredOrderChecker()
    {
        while(true)
        {
            yield return new WaitForSeconds(5f);
            List<DeliveryOrder> expiredorders = new List<DeliveryOrder>();

            foreach (DeliveryOrder order in currentOrders)
            {
                if(order.IsExpired() && order.state != OrderState.Completed)
                {
                    expiredorders.Add(order);
                }
            }

            foreach(DeliveryOrder expired in  expiredorders)
            {
                ExpireOrder(expired);
            }
        }
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 10, 400, 1300));
        GUILayout.Label("=== 배달 주문 ===");
        GUILayout.Label($"활성 주문 : {currentOrders.Count} 개");
        GUILayout.Label($"픽업 대기 : {GetPickWaitingCount()} 개");
        GUILayout.Label($"배달 대기 : {GetDeliveryWaitingCount()} 개");
        GUILayout.Label($"완료 : {completedOrder} 개 | 만료 : {expiredOrders}");

        GUILayout.Space(10);

        foreach(DeliveryOrder order in currentOrders)
        {
            string status = order.state == OrderState.WaitingPickup ? "픽업 대기" : "배달 대기";
            float timeLeft = order.GetRemainingTime();

            GUILayout.Label($"#{order.orderId} : {order.restaurantName} -> {order.customerName}");
            GUILayout.Label($"{status} | {timeLeft:F0} 초 남음");
        }

        GUILayout.EndArea();
    }
}
