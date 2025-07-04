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
        if (Instance == null)
        {
            Instance = this;
            // 确保预制件已赋值
            InitializePool(); // 确保立即初始化
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void InitializePool()
    {
        availableFlames = new Queue<FlameController>();

        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject flame = Instantiate(flamePrefab);
            FlameController fc = flame.GetComponent<FlameController>();
            fc.gameObject.tag = "PlayerProjectile"; // 明确标记玩家子弹
            availableFlames.Enqueue(fc);
            flame.SetActive(false);
        }
    
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
    public List<FlameController> GetAllFlames()
    {
        return new List<FlameController>(allFlames);
    }

}