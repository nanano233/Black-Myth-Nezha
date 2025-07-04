using System.Collections.Generic;
using UnityEngine;

public class EnemyFlamePool : MonoBehaviour
{
    public static EnemyFlamePool Instance;
    public GameObject enemyFlamePrefab;
    public int poolSize = 20;

    private Queue<FlameController> availableFlames = new Queue<FlameController>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializePool();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject flame = Instantiate(enemyFlamePrefab);
            FlameController fc = flame.GetComponent<FlameController>();
            fc.gameObject.tag = "EnemyProjectile";
            availableFlames.Enqueue(fc);
            flame.SetActive(false);
        }
    }

    public FlameController GetFlame()
    {
        if (availableFlames.Count == 0)
            ExpandPool();
        
        return availableFlames.Dequeue();
    }

    public void ReturnFlame(FlameController flame)
    {
        flame.gameObject.SetActive(false);
        availableFlames.Enqueue(flame);
    }

    void ExpandPool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject flame = Instantiate(enemyFlamePrefab);
            FlameController fc = flame.GetComponent<FlameController>();
            availableFlames.Enqueue(fc);
            flame.SetActive(false);
        }
    }
}