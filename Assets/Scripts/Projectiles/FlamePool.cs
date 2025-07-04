using System.Collections.Generic;
using UnityEngine;

public class FlamePool : MonoBehaviour
{
    public static FlamePool Instance;

    public GameObject flamePrefab;
    public int initialPoolSize = 20;

    private Queue<FlameController> availableFlames = new Queue<FlameController>();
    private List<FlameController> allFlames = new List<FlameController>();

    void Awake()
    {
        // 加强单例保护
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitializePool();
    }


    public void InitializePool()
    {
        // 销毁所有现有子弹
        foreach (var flame in allFlames.ToArray())
        {
            if (flame != null && flame.gameObject != null)
            {
                Destroy(flame.gameObject);
            }
        }
        
        // 重新初始化集合
        availableFlames = new Queue<FlameController>();
        allFlames = new List<FlameController>();
        
        // 创建新子弹
        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject flame = Instantiate(flamePrefab);
            FlameController fc = flame.GetComponent<FlameController>();
            fc.gameObject.tag = "PlayerProjectile";
            flame.SetActive(false);
            availableFlames.Enqueue(fc);
            allFlames.Add(fc);
        }
        
        Debug.Log($"子弹对象池已初始化，大小: {initialPoolSize}");
    }
    
    // 新增获取所有子弹的方法
    public List<FlameController> GetAllFlames()
    {
        return new List<FlameController>(allFlames);
    }

    void CreateNewFlame()
    {
        GameObject flameObj = Instantiate(flamePrefab);
        flameObj.SetActive(false);
        FlameController flame = flameObj.GetComponent<FlameController>();
        availableFlames.Enqueue(flame);
        allFlames.Add(flame);
    }

    public FlameController GetFlame()
    {
        if (availableFlames.Count == 0)
        {
            CreateNewFlame();
        }

        return availableFlames.Dequeue();
    }

    public void ReturnFlame(FlameController flame)
    {
        flame.gameObject.SetActive(false);
        availableFlames.Enqueue(flame);
    }

    // 更新所有子弹的属性（当玩家获得新道具时调用）
    public void UpdateAllFlameProperties(ProjectileProperties newProperties)
    {
        foreach (var flame in allFlames)
        {
            flame.properties = newProperties;
        }
    }

}