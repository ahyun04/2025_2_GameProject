using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeFactory : MonoBehaviour
{

    [Header("프리팹과 위치")]
    public GameObject cubePrefab;
    public Transform queuePoint;
    public Transform woodStoreage;
    public Transform metalStorage;
    public Transform asseblyArea;

    private Queue<GameObject>materialQueue = new Queue<GameObject>();
    private Stack<GameObject> woodWareHouse = new Stack<GameObject>();
    private Stack<GameObject> metalWarHouse = new Stack<GameObject>();
    private Stack<string> assemblyStack = new Stack<string>();
    private List<WorkRequest> requestList = new List<WorkRequest>();
    private Dictionary<ProductType, int> products = new Dictionary<ProductType, int>();

    public int money = 500;
    public int score = 0;

    private float lastMaterialTime;  
    private float lastOrderTime;

    void Start()
    {
        products[ProductType.Chair] = 0;

        assemblyStack.Push("포장");
        assemblyStack.Push("조립");
        assemblyStack.Push("준비");
    }

    void Update()
    {
        HandleInput();
        UpdateVisuals();
        AutoEvent();
    }

    void AddMaterial()
    {
        ResourceType randomType = (Random.value > 0.5f) ? ResourceType.Wood : ResourceType.Metal;

        GameObject newCube = Instantiate(cubePrefab);
        ResourceCube cubeComponent = newCube.AddComponent<ResourceCube>();
        cubeComponent.Initaliz(randomType);

        materialQueue.Enqueue(newCube);

        Debug.Log($"{randomType} 원료 도착, 큐 대기 : {materialQueue.Count} 개");
    }

    void ProcessQueue()
    {
        if(materialQueue.Count == 0)
        {
            Debug.Log("큐가 비어있습니다");
            return;
        }

        GameObject cube = materialQueue.Dequeue();
        ResourceCube resource = cube.GetComponent<ResourceCube>();

        if(resource.type == ResourceType.Wood)
        {
            woodWareHouse.Push(cube);
            Debug.Log($"나무 창고 입고! 창고: {woodWareHouse.Count} 개");
        }
        else if(resource.type == ResourceType.Metal)
        {
            metalWarHouse.Push(cube);
            Debug.Log($"금속 창고 입고! 창고 : {metalWarHouse.Count} 개");
        }
    }

    void ProcessAssembly()
    {
        if(woodWareHouse.Count == 0 || metalWarHouse.Count == 0)
        {
            Debug.Log("조립할 재료가 부족합니다.");
            return;
        }

        if(assemblyStack.Count == 0)
        {
            Debug.Log("조립 작업이 없습니다.");
            return;
        }

        string work = assemblyStack.Pop();

        GameObject wood = woodWareHouse.Pop();
        GameObject metal = metalWarHouse.Pop();
        Destroy( wood );
        Destroy( metal );

        if(assemblyStack.Count == 0)
        {
            products[ProductType.Chair]++;
            score += 100;

            assemblyStack.Push("포장");
            assemblyStack.Push("조립");
            assemblyStack.Push("준비");

            Debug.Log($"의자 완성! 총 의자 : {products[ProductType.Chair]} 개");
        }
    }

    void AddRequest()
    {
        int quantity = Random.Range(1, 4);
        int reward = quantity * 200;

        WorkRequest newRequest = new WorkRequest(ProductType.Chair, quantity, reward);

        requestList.Add(newRequest);

        Debug.Log("새 요청서 도착");
    }

    void ProcessRequests()
    {
        if(requestList.Count == 0)
        {
            Debug.Log("처리할 요청서가 없습니다.");
            return;
        }

        WorkRequest firestRequest = requestList[0];

        if (products[firestRequest.productType] >= firestRequest.quantity)
        {
            products[firestRequest.productType] -= firestRequest.quantity;
            money += firestRequest.reward;
            score += firestRequest.reward;

            requestList.RemoveAt(0);
        }
        else
        {
            int available = products[firestRequest.productType];
            int needed = firestRequest.quantity - available;
            Debug.Log($"재고 부족 {needed}대 더 필요 (현재 : {available} 개");
        }
    }

    void UpdateVisuals()
    {
        UpdateQueueVisual();
        UpdateWareHouseVisual();
    }

    void UpdateQueueVisual()
    {
        if (queuePoint == null) return;

        GameObject[] queueArray = materialQueue.ToArray();
        for(int i = 0; i < queueArray.Length; i++)
        {
            Vector3 position = queuePoint.position + Vector3.right * (i*1.2f);
            queueArray[i].transform.position = position;
        }
    }

    void UpdateWareHouseVisual()
    {
        UpdateStackVisual(woodWareHouse.ToArray(), woodStoreage);
        UpdateStackVisual(metalWarHouse.ToArray(), metalStorage);
    }

    void UpdateStackVisual(GameObject[] stackArray, Transform basePoint)
    {
        if (basePoint == null) return;

        for(int i = 0; i < stackArray.Length; i++)
        {
            Vector3 position = basePoint.position + Vector3.up * (i * 1.1f);
            stackArray[stackArray.Length -1-i].transform.position = position;
        }
    }

    void OnGUI()
    {
        GUI.Label(new Rect(10, 20, 200, 20), $"돈 : {money}원 | 점수 : {score}점");

        GUI.Label(new Rect(10, 40, 250, 20), $"원료 큐 (Queue) : {materialQueue.Count} 개 대기");
        GUI.Label(new Rect(10, 60, 250, 20), $"나무 창고 (Stack) : {woodWareHouse.Count} 개 대기");
        GUI.Label(new Rect(10, 80, 250, 20), $"금속 창고 (Stack) : {metalWarHouse.Count} 개 대기");
        GUI.Label(new Rect(10, 100, 250, 20), $"조립 스택 (Stack) : {assemblyStack.Count} 개 대기");
        GUI.Label(new Rect(10, 120, 250, 20), $"요청서 (Dict) : {products[ProductType.Chair]} 개 대기");
        GUI.Label(new Rect(10, 140, 250, 20), $"요청서 (List) : {requestList.Count} 개 대기");

        GUI.Label(new Rect(10, 170, 200, 20), "=====요청서 목록=====");
        for(int i = 0; i<requestList.Count && i<3;i++)
        {
            WorkRequest request = requestList[i];
            GUI.Label(new Rect(10, 190 + i * 20, 300, 20), $"{i} 의자 {request.quantity} 개 -> {request.reward}원");
        }

        GUI.Label(new Rect(300, 40, 150, 20), "=====조작법=====");
        GUI.Label(new Rect(300, 60, 150, 20), "1키 : 원료 큐 추가");
        GUI.Label(new Rect(300, 80, 150, 20), "Q키 : 큐 -> 창고");
        GUI.Label(new Rect(300, 100, 150, 20), "A키 : 조립 (스택)");
        GUI.Label(new Rect(300, 120, 150, 20), "S키 : 요청 처리");
        GUI.Label(new Rect(300, 140, 150, 20), "R키 : 요청서 추가");
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) AddMaterial();
        if(Input.GetKeyDown(KeyCode.Q)) ProcessQueue();
        if (Input.GetKeyDown(KeyCode.A)) ProcessAssembly();
        if(Input.GetKeyDown(KeyCode.S)) ProcessRequests();
        if(Input.GetKeyDown(KeyCode.R)) AddRequest();
    }

    void AutoEvent()
    {
        if(Time.time - lastMaterialTime > 3f)
        {
            AddMaterial();
            lastMaterialTime = Time.time;
        }

        if(Time.time - lastOrderTime > 10f)
        {
            AddRequest();
            lastOrderTime = Time.time;
        }
    }
}
