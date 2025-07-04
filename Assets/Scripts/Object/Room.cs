using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Room : MonoBehaviour
{
    [Header("关卡传送")]
    public GameObject levelPortalPrefab; // 需要拖入预制件
    public static int currentFloor = 1; // 当前层数
    // 摄像机边界
    [Header("摄像机边界")]
    public Vector2 cameraMinBounds;
    public Vector2 cameraMaxBounds;

    [Header("房间属性")]
    public bool isStartingRoom = false;
    public bool isBossRoom = false;
    public bool isCleared = false;
    // 添加房间类型枚举
    public enum RoomType { Normal, Boss }
    public RoomType roomType;

    [Header("生成设置")]
    [Tooltip("敌人生成冷却时间")] 
    public float spawnInterval = 0.5f;
    [Tooltip("Boss敌人预制件")] 
    public GameObject bossEnemyPrefab;

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
        // 在Start方法开头添加类型验证
        if (isBossRoom && roomType != RoomType.Boss)
        {
            roomType = RoomType.Boss;
            Debug.LogWarning($"{name} 房间类型自动修正为Boss");
        }

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
        foreach (Door door in doors)
        {
            door.parentRoom = this; // 设置门的父房间
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
        if (roomType == RoomType.Boss)
        {
            if (bossEnemyPrefab != null)
            {
                StartCoroutine(SpawnBossWithDelay());
            }
            else
            {
                Debug.LogError("Boss房间未设置bossEnemyPrefab");
            }
        }
        else
        {
            EnemyManager.Instance.SpawnEnemiesForRoom(this);
        }
    }
    private IEnumerator SpawnBossWithDelay()
    {
        yield return new WaitForSeconds(spawnInterval);
        GameObject boss = Instantiate(bossEnemyPrefab, GetRoomCenter(), Quaternion.identity);
        boss.GetComponent<Enemy>().OnDeath += HandleEnemyDeath;
        aliveEnemies = 1;
    }

    // 添加房间中心位置获取方法
    public Vector3 GetRoomCenter()
    {
        return transform.position + new Vector3(
            (cameraMaxBounds.x - cameraMinBounds.x) / 2,
            (cameraMaxBounds.y - cameraMinBounds.y) / 2,
            0
        );
    }

    public Vector2 GetRandomPositionInRoom()
    {
        // 修正坐标计算，使用相对房间中心的随机范围
        float roomWidth = cameraMaxBounds.x - cameraMinBounds.x;
        float roomHeight = cameraMaxBounds.y - cameraMinBounds.y;
        
        return (Vector2)transform.position + new Vector2(
            Random.Range(-roomWidth * 0.4f, roomWidth * 0.4f),
            Random.Range(-roomHeight * 0.4f, roomHeight * 0.4f)
        );
    }
    public bool IsPositionInRoom(Vector2 position)
    {
        Vector2 localPos = position - (Vector2)transform.position;
        return localPos.x >= cameraMinBounds.x && localPos.x <= cameraMaxBounds.x &&
               localPos.y >= cameraMinBounds.y && localPos.y <= cameraMaxBounds.y;
    }

    public void HandleEnemyDeath()
    {
        aliveEnemies = Mathf.Max(aliveEnemies - 1, 0);

        if (aliveEnemies <= 0)
        {
            isCleared = true;
            // 添加房间状态更新

            // 新增Boss房特殊逻辑
            if (isBossRoom)
            {
                // 生成传送门
                if (levelPortalPrefab != null)
                {
                    Instantiate(levelPortalPrefab, GetRoomCenter(), Quaternion.identity);
                }
                else
                {
                    Debug.LogWarning("未分配关卡传送门预制件");
                }
            }

            RoomManager.Instance.CheckRoomCleared();
            foreach (Door door in doors)
            {
                if (door.connectedRoom != null)
                {
                    door.Unlock();
                }
            }
            RoomManager.Instance.ReportRoomCleared(this);
        }
    }

    // 新增调试绘制
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0.5f, 0, 0.3f);
        Gizmos.color = Color.magenta;
        Vector3 center = new Vector3(
            transform.position.x + (cameraMinBounds.x + cameraMaxBounds.x) / 2,
            transform.position.y + (cameraMinBounds.y + cameraMaxBounds.y) / 2,
            0
        );
        Vector3 size = new Vector3(
            cameraMaxBounds.x - cameraMinBounds.x,
            cameraMaxBounds.y - cameraMinBounds.y,
            0.1f
        );
        Gizmos.DrawWireCube(center, size);
    
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