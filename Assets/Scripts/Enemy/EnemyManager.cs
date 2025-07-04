using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// 敌人管理器
public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance;

    [Header("敌人生成")]
    public GameObject basicEnemyPrefab;
    public GameObject shooterEnemyPrefab;
    public GameObject bossEnemyPrefab;

    public int maxEnemiesPerRoom = 8;
    public int minEnemiesPerRoom = 3;

    private List<Enemy> activeEnemies = new List<Enemy>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
        
    }
    // 修改敌人生成方法
    public void SpawnEnemiesForRoom(Room room)
    {
        if (room == null || room.isStartingRoom) return;

        // 使用房间预设的生成点
        if (room.enemySpawnPoints.Length == 0)
        {
            Debug.LogError($"房间 {room.name} 缺少敌人生成点！");
            return;
        }

        int enemyCount = Mathf.Clamp(Random.Range(minEnemiesPerRoom, maxEnemiesPerRoom + 1),
            0, room.enemySpawnPoints.Length);

        for (int i = 0; i < enemyCount; i++)
        {
            // 循环使用生成点
            Transform spawnPoint = room.enemySpawnPoints[i % room.enemySpawnPoints.Length];
            // 添加变量声明
            GameObject enemyObj = Instantiate(Random.value > 0.3f ? basicEnemyPrefab : shooterEnemyPrefab,
                        spawnPoint.position,
                        Quaternion.identity);
            // 新增事件绑定
            if (enemyObj.TryGetComponent<Enemy>(out var enemy))
            {
                enemy.OnDeath += room.HandleEnemyDeath;
                enemy.OnDeath += () => RemoveEnemy(enemy); // 保持原有逻辑
            }
        }

        room.aliveEnemies = enemyCount;
    }


    // 新增敌人类型权重计算
    private EnemyType GetWeightedEnemyType()
    {
        float rand = Random.value;
        return rand < 0.7f ? EnemyType.Basic :
               rand < 0.9f ? EnemyType.Shooter :
               EnemyType.Boss;
    }

    public void SpawnEnemy(EnemyType type, Vector2 position, Room parentRoom = null)
    {
        GameObject prefab = GetEnemyPrefab(type);
        if (prefab == null) return;

        GameObject enemyObj = Instantiate(prefab, position, Quaternion.identity, parentRoom?.transform);
        if (enemyObj.TryGetComponent<Enemy>(out var enemy))
        {
            activeEnemies.Add(enemy);

            // 绑定敌人死亡事件到房间
            enemy.OnDeath += () =>
            {
                if (parentRoom != null) parentRoom.HandleEnemyDeath();
                RemoveEnemy(enemy);
            };
        }
    }

    // 新增通过字典管理预制件
    private Dictionary<EnemyType, GameObject> _enemyPrefabMap;
    private GameObject GetEnemyPrefab(EnemyType type)
    {
        if (_enemyPrefabMap == null)
        {
            _enemyPrefabMap = new Dictionary<EnemyType, GameObject>
        {
            { EnemyType.Basic, basicEnemyPrefab },
            { EnemyType.Shooter, shooterEnemyPrefab },
            { EnemyType.Boss, bossEnemyPrefab }
        };
        }
        return _enemyPrefabMap.TryGetValue(type, out var prefab) ? prefab : null;
    }

    private void RemoveEnemy(Enemy enemy)
    {
        if (activeEnemies.Contains(enemy))
        {
            activeEnemies.Remove(enemy);
        }
    }

    public void ClearAllEnemies()
    {
        foreach (var enemy in activeEnemies)
        {
            if (enemy != null)
                Destroy(enemy.gameObject);
        }
        activeEnemies.Clear();
    }
}
