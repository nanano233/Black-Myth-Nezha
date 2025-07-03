using UnityEngine;
using System.Collections.Generic;

public class Room : MonoBehaviour
{
    // 摄像机边界
    [Header("摄像机边界")]
    public Vector2 cameraMinBounds;
    public Vector2 cameraMaxBounds;

    [Header("房间属性")]
    public bool isStartingRoom = false;
    public bool isBossRoom = false;
    public bool isCleared = false;

    [Header("引用")]
    public Door[] doors;
    public Transform[] enemySpawnPoints;
    public GameObject enemyPrefab; // 新增敌人预制件引用
    // 新增网格坐标属性
    [HideInInspector] public Vector2Int gridPosition;

    // 新增属性
    public bool isVisited;
    [HideInInspector] public int aliveEnemies; // 添加HideInInspector特性保持编辑器整洁


    void Start()
    {
        // 自动获取所有门（强制检查子对象）
        if (doors.Length == 0)
        {
            doors = GetComponentsInChildren<Door>(true); // 包含隐藏对象
            Debug.Log($"{name} 自动找到 {doors.Length} 个门");
        }

        // 自动获取生成点
        if (enemySpawnPoints.Length == 0 && transform.Find("SpawnPoints") != null)
        {
            Transform spawnParent = transform.Find("SpawnPoints");
            enemySpawnPoints = new Transform[spawnParent.childCount];
            for (int i = 0; i < spawnParent.childCount; i++)
            {
                enemySpawnPoints[i] = spawnParent.GetChild(i);
            }
        }

    }

    public void Initialize(Vector2Int gridPos)
    {
        gridPosition = gridPos;
        
        // 确保所有门初始化为未连接状态
        foreach (Door door in doors)
        {
            if (door != null)
            {
                door.doorState = Door.DoorState.Unconnected;
                door.Initialize();
            }
        }
        
        // 特殊处理起始房间
        if (isStartingRoom)
        {
            // 起始房间自动解锁所有连接的门
            foreach (Door door in doors)
            {
                if (door.connectedRoom != null)
                {
                    door.Unlock();
                }
            }
        }
    }

    // 修改后的房间激活控制方法
    public void ActivateRoom()
    {
        // 保持房间对象始终激活
        gameObject.SetActive(true);

        // 激活所有连接的门
        foreach (Door door in doors)
        {
            if (door.connectedRoom != null)
            {
                door.gameObject.SetActive(true);
                Debug.Log($"{name} 激活门：{door.name}");
            }
        }

        // 禁用敌人生成点（保持原房间可见）
        if (enemySpawnPoints != null)
        {
            foreach (Transform spawnPoint in enemySpawnPoints)
            {
                spawnPoint.gameObject.SetActive(false);
            }
        }
    }
    public void DeactivateRoom()
    {
        // 只关闭门，保持房间可见
        foreach (Door door in doors)
        {
            door.gameObject.SetActive(false);
        }

        // 禁用敌人生成点（如果存在）
        if (enemySpawnPoints != null)
        {
            foreach (Transform spawnPoint in enemySpawnPoints)
            {
                if (spawnPoint != null)
                    spawnPoint.gameObject.SetActive(false);
            }
        }

        Debug.Log($"{name} 房间已停用（保持可见）");
    }

    void GenerateEnemies()
    {
        if (isBossRoom)
        {
            // 生成Boss
            Debug.Log("生成Boss敌人");
        }
        else
        {
            // 生成普通敌人
            foreach (Transform spawnPoint in enemySpawnPoints)
            {
                if (Random.value > 0.7f) // 70%几率生成敌人
                {
                    // 实际应使用对象池生成敌人
                    Debug.Log($"在 {spawnPoint.position} 生成敌人");
                }
            }
        }
    }

    public void UnlockDoors()
    {
        isCleared = true;
        foreach (Door door in doors)
        {
            // 新增连接状态检查
            if (door.connectedRoom != null)
            {
                door.Unlock();
            }
        }
    }
    public void CheckEnemiesCleared()
    {
        // 实际应检查所有敌人是否被击败
        // 这里简化处理
        UnlockDoors();
        Debug.Log("所有敌人被击败，房间已清除！");
    }

    // 注意：移除了OnTriggerEnter2D方法，避免与Door的触发冲突

    public Vector3 GetConnectedDoorPosition(Door door)
    {
        int doorIndex = System.Array.IndexOf(doors, door);
        if (doorIndex != -1 && doors[doorIndex].connectedRoom != null)
        {
            return doors[doorIndex].transform.position;
        }
        return transform.position;
    }

    // 添加敌人计数

    public void SpawnEnemies()
    {
        if (enemyPrefab == null)
        {
            Debug.LogWarning("无法生成敌人，enemyPrefab未赋值");
            return;
        }
        aliveEnemies = enemySpawnPoints.Length;
        foreach (Transform spawnPoint in enemySpawnPoints)
        {
            GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
            enemy.GetComponent<Enemy>().OnDeath += HandleEnemyDeath;
        }
    }

    public void HandleEnemyDeath()
    {
        if (--aliveEnemies <= 0)
        {
            foreach (Door door in doors)
            {
                // 添加连接状态检查
                if (door.connectedRoom != null && door.doorState == Door.DoorState.Locked)
                {
                    door.Unlock();
                }
            }
        }
    }
    
    public void LockAllDoors(Door exceptDoor = null)
    {
        foreach (Door door in doors)
        {
            // 跳过未连接的门
            if (door.connectedRoom == null) continue;
            
            if (door != exceptDoor)
            {
                door.Lock();
            }
            else
            {
                door.Unlock();
            }
        }
    }
}