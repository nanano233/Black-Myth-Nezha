using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RoomManager : MonoBehaviour
{
    // 方向枚举
    private enum Direction { North, East, South, West }
    public static RoomManager Instance;

    [Header("房间设置")]
    public GameObject roomPrefab;
    [Tooltip("最小房间数量")]
    public int minRooms = 5;
    public int maxRooms = 10;
    public float roomWidth = 40f;
    public float roomHeight = 30f;

    [Header("当前状态")]
    public List<Room> allRooms = new List<Room>();
    public Room currentRoom;

    [Header("生成参数")]
    [Range(0.1f, 1f)] public float branchChance = 0.35f;
    public int maxGenerationDepth = 5;
    // 添加生成完成标志
    [HideInInspector] public bool generationComplete = false;

    private HashSet<Vector2Int> occupiedPositions = new HashSet<Vector2Int>();
    private Queue<Room> generationQueue = new Queue<Room>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // 清空现有房间（新增销毁逻辑）
        foreach (var room in allRooms.ToArray())
        {
            if (room != null && room.gameObject != null)
                Destroy(room.gameObject);
        }
        allRooms.Clear();
        GenerateRooms();
        // 添加空检查和安全进入方式
        if (allRooms[0] != null)
        {
            EnterRoom(allRooms[0], null); // 明确传递null表示初始进入
        }
        else
        {
            Debug.LogError("起始房间生成失败");
        }

        // 确保起始房间有效
        if (allRooms.Count > 0 && allRooms[0] != null)
        {
            currentRoom = allRooms[0];
            currentRoom.ActivateRoom();

            // 添加摄像机初始化
            if (CameraController.Instance != null)
            {
                CameraController.Instance.SetBounds(
                    currentRoom.cameraMinBounds,
                    currentRoom.cameraMaxBounds
                );
                CameraController.Instance.MoveTo(currentRoom.transform.position);
            }

            // 添加玩家位置日志
            Debug.Log($"起始房间位置: {currentRoom.transform.position}");
        }
        else
        {
            Debug.LogError("无法找到有效的起始房间！");
        }
        generationComplete = true;
        Debug.Log("房间生成完成");
    }

    void GenerateRooms()
    {
        // 清空生成队列和位置记录
        generationQueue.Clear();
        occupiedPositions.Clear();

        // 创建起始房间
        Vector2Int startGridPos = Vector2Int.zero;
        Room startRoom = CreateRoom(GetWorldPosition(startGridPos));
        startRoom.isStartingRoom = true;
        occupiedPositions.Add(startGridPos);
        generationQueue.Enqueue(startRoom);

        // 使用DFS算法生成房间
        int currentDepth = 0;
        while (generationQueue.Count > 0 && allRooms.Count < maxRooms && currentDepth < maxGenerationDepth)
        {
            Room currentRoom = generationQueue.Dequeue();

            // 尝试在各个方向生成新房间
            foreach (Direction direction in System.Enum.GetValues(typeof(Direction)))
            {
                if (Random.value > branchChance) continue;

                Vector2Int newGridPos = GetNewGridPosition(currentRoom.transform.position, direction);
                if (!occupiedPositions.Contains(newGridPos))
                {
                    Room newRoom = CreateRoom(GetWorldPosition(newGridPos));
                    ConnectRooms(currentRoom, newRoom, direction);
                    generationQueue.Enqueue(newRoom);
                    occupiedPositions.Add(newGridPos);
                }
            }
            currentDepth++;
        }

        // 确保达到最小房间数
        int requiredRooms = Mathf.Max(minRooms, 1); // 至少1个房间
        while (allRooms.Count < requiredRooms)
        {
            Room randomRoom = allRooms[Random.Range(0, allRooms.Count)];

            // 尝试所有可能的方向
            foreach (Direction dir in System.Enum.GetValues(typeof(Direction)))
            {
                Vector2Int newPos = GetNewGridPosition(randomRoom.transform.position, dir);
                if (!occupiedPositions.Contains(newPos))
                {
                    Room newRoom = CreateRoom(GetWorldPosition(newPos));
                    ConnectRooms(randomRoom, newRoom, dir);
                    occupiedPositions.Add(newPos);
                    break; // 找到一个有效位置后跳出循环
                }
            }
        }

        // 设置最后一个房间为Boss房
        if (allRooms.Count > 1)
        {
            Room farthestRoom = FindFarthestRoom(startRoom);
            farthestRoom.isBossRoom = true;
        }

        // 修正起始房间门状态
        foreach (Door door in startRoom.doors)
        {
            // 仅解锁实际连接的门
            if (door.connectedRoom != null)
            {
                door.Unlock();
                Debug.Log($"解锁起始房间门：{door.name}，连接房间：{door.connectedRoom.name}");
            }
            else
            {
                // 明确设置未连接状态
                door.doorState = Door.DoorState.Unconnected;
                door.UpdateDoorAppearance();
            }
        }

        // 确保所有门状态正确
        foreach (Room room in allRooms)
        {
            foreach (Door door in room.doors)
            {
                if (door.connectedRoom == null)
                {
                    door.doorState = Door.DoorState.Unconnected;
                    door.UpdateDoorAppearance();
                }
            }
        }
    }

    // 精确计算网格位置
    Vector2Int GetNewGridPosition(Vector2 currentPos, Direction direction)
    {
        Vector2Int gridPos = new Vector2Int(
            Mathf.RoundToInt(currentPos.x / roomWidth),
            Mathf.RoundToInt(currentPos.y / roomHeight)
        );

        switch (direction)
        {
            case Direction.North: return gridPos + Vector2Int.up;
            case Direction.East: return gridPos + Vector2Int.right;
            case Direction.South: return gridPos + Vector2Int.down;
            case Direction.West: return gridPos + Vector2Int.left;
            default: return gridPos;
        }
    }

    Vector2 GetWorldPosition(Vector2Int gridPos)
    {
        return new Vector2(gridPos.x * roomWidth, gridPos.y * roomHeight);
    }

    // 修改后的连接房间方法（使用新的门配对逻辑）
    void ConnectRooms(Room roomA, Room roomB, Direction direction)
    {
        // 新增自连接检查
        if (roomA == roomB)
        {
            Debug.LogError($"房间自连接尝试：{roomA.name}");
            return;
        }
        // 获取两个房间的对应门
        Door doorA = GetDoorForDirection(roomA, direction);
        Door doorB = GetDoorForDirection(roomB, GetOppositeDirection(direction));

        if (doorA != null && doorB != null)
        {


            // 新增门归属验证
            if (doorA.transform.IsChildOf(roomA.transform) && doorB.transform.IsChildOf(roomB.transform))
            {
                // ... 保持原有连接逻辑 ...
                // 新增位置验证逻辑
                Vector2 expectedPos = GetWorldPosition(GetNewGridPosition(roomA.transform.position, direction));
                if (Vector2.Distance(roomB.transform.position, expectedPos) > 0.1f)
                {
                    Debug.LogError($"房间位置不匹配！预期：{expectedPos}，实际：{roomB.transform.position}");
                }

                // 替换直接赋值的方式，改用ConnectRoom方法
                doorA.ConnectRoom(roomB);
                doorB.ConnectRoom(roomA);

                // 激活门对象
                doorA.gameObject.SetActive(true);
                doorB.gameObject.SetActive(true);

                // 解锁起始房间的门
                if (roomA.isStartingRoom)
                {
                    doorA.Unlock();
                    Debug.Log($"解锁起始房间门：{doorA.name}");
                }
            }
            else
            {
                Debug.LogError($"门归属异常：{doorA.name} 或 {doorB.name} 不属于对应房间");
            }
        }
        else
        {
            Debug.LogWarning($"房间连接失败：{roomA.name} 或 {roomB.name} 缺少对应方向的门");
        }
    }


    // 修改GetDoorForDirection方法使用标签
    Door GetDoorForDirection(Room room, Direction dir)
    {
        string tag = dir.ToString(); // 将方向枚举转换为字符串标签
        foreach (Door door in room.doors)
        {
            if (door.CompareTag(tag))
                return door;
        }
        return null;
    }

    Direction GetOppositeDirection(Direction dir)
    {
        return dir switch
        {
            Direction.North => Direction.South,
            Direction.East => Direction.West,
            Direction.South => Direction.North,
            Direction.West => Direction.East,
            _ => Direction.North
        };
    }

    Room FindFarthestRoom(Room startRoom)
    {
        Room farthest = startRoom;
        float maxDistance = 0;

        foreach (Room room in allRooms)
        {
            float distance = Vector2.Distance(startRoom.transform.position, room.transform.position);
            if (distance > maxDistance)
            {
                maxDistance = distance;
                farthest = room;
            }
        }
        return farthest;
    }

    Room CreateRoom(Vector2 position)
    {
        // 计算精确对齐的网格位置
        Vector2Int gridPos = new Vector2Int(
            Mathf.RoundToInt(position.x / roomWidth),
            Mathf.RoundToInt(position.y / roomHeight)
        );

        Vector2 alignedPos = new Vector2(
            gridPos.x * roomWidth,
            gridPos.y * roomHeight
        );

        GameObject roomObj = Instantiate(roomPrefab, alignedPos, Quaternion.identity);
        Room room = roomObj.GetComponent<Room>();
        if (room == null) room = roomObj.AddComponent<Room>();

        // 确保组件初始化顺序正确
        if (room != null)
        {
            // 先设置房间属性
            room.cameraMinBounds = new Vector2(
                position.x - roomWidth * 0.07f,
                position.y - roomHeight * 0.12f
            );
            room.cameraMaxBounds = new Vector2(
                position.x + roomWidth * 0.07f,
                position.y + roomHeight * 0.12f
            );

            // 后初始化房间
            room.Initialize(gridPos);
            allRooms.Add(room);
        }
        return room;
    }

    public void EnterRoom(Room newRoom, Door enteredDoor = null)
    {
        // 添加参数验证
        if (newRoom == null)
        {
            Debug.LogError("无效的房间进入请求");
            return;
        }

        // 新增初始进入的特殊处理
        if (enteredDoor == null && currentRoom == null)
        {
            Debug.Log($"初始进入房间: {newRoom.name}");
            currentRoom = newRoom;
            currentRoom.ActivateRoom();
            CameraController.Instance.SetBounds(
                newRoom.cameraMinBounds,
                newRoom.cameraMaxBounds
            );
            CameraController.Instance.MoveTo(newRoom.transform.position);
            return;
        }

        // 新增传送保护
        if (currentRoom == newRoom)
        {
            Debug.LogWarning($"尝试进入当前所在房间：{newRoom.name}");
            return;
        }
        // 关闭当前房间
        if (currentRoom != null)
        {
            currentRoom.DeactivateRoom();
        }

        // 设置新房间
        currentRoom = newRoom;
        currentRoom.ActivateRoom();

        // 新房间首次进入时锁门
        if (!newRoom.isVisited)
        {
            // 起始房间不锁门
            if (!newRoom.isStartingRoom)
            {
                if (EnemyManager.Instance != null)
                {
                    EnemyManager.Instance.SpawnEnemiesForRoom(newRoom);
                }
                else
                {
                    Debug.LogError("EnemyManager实例未找到！");
                }
                newRoom.LockAllDoors(enteredDoor);
            }
            newRoom.isVisited = true;
        }
        else
        {
            // 解锁所有已连接的门（新增）
            foreach (Door door in newRoom.doors)
            {
                if (door.connectedRoom != null)
                {
                    door.Unlock();
                }
            }
        }

        // 更新摄像机边界
        CameraController.Instance.SetBounds(
            newRoom.cameraMinBounds,
            newRoom.cameraMaxBounds
        );
        CameraController.Instance.MoveTo(newRoom.transform.position);

        // 调整玩家位置（如果通过门进入）
        if (enteredDoor != null && PlayerControl.Instance != null)
        {
            if (enteredDoor.connectedRoom == null)
            {
                Debug.LogError($"传入门 {enteredDoor.name} 未连接任何房间");
                return;
            }
            // **修改此处：从门的祖父级获取Room组件**
            Room enteredRoom = enteredDoor.transform.parent?.parent?.GetComponent<Room>();
            if (enteredRoom == null)
            {
                Debug.LogError($"传入门 {enteredDoor.name} 没有有效的父级房间，完整路径：{GetHierarchyPath(enteredDoor.transform)}");
                return;
            }

            Vector3 spawnPosition = GetConnectedDoorPosition(newRoom, enteredDoor);
            PlayerControl.Instance.TeleportTo(spawnPosition);
        }
    }

    // 新增辅助方法：获取对象层级路径
    string GetHierarchyPath(Transform obj)
    {
        if (obj == null) return "null";
        string path = obj.name;
        while (obj.parent != null)
        {
            obj = obj.parent;
            path = obj.name + "/" + path;
        }
        return path;
    }

    // 完全重写传送位置计算方法
    Vector3 GetConnectedDoorPosition(Room room, Door enteredDoor)
    {
        // 1. 获取进入门的方向
        Vector3 enteredDir = GetDoorWorldDirection(enteredDoor);

        if (enteredDir == Vector3.zero)
        {
            Debug.LogWarning($"进入门 {enteredDoor.name} 方向无效，使用房间中心位置");
            return room.transform.position;
        }

        // 2. 计算新房间需要的门方向（相反方向）
        Vector3 neededDir = -enteredDir;
        string neededTag = neededDir switch
        {
            Vector3 v when v == Vector3.up => "North",
            Vector3 v when v == Vector3.down => "South",
            Vector3 v when v == Vector3.right => "East",
            Vector3 v when v == Vector3.left => "West",
            _ => ""
        };

        // 3. 查找匹配的门
        foreach (Door door in room.doors)
        {
            if (door.CompareTag(neededTag))
            {
                // 4. 计算精确传送位置（门位置+方向偏移）
                return door.transform.position - (neededDir * 0.2f);
            }
        }

        // 5. 备用方案：房间中心
        Debug.LogWarning($"在房间 {room.name} 中未找到 {neededTag} 方向的门，使用房间中心位置");
        return room.transform.position;
    }

    // 新增：获取门的世界空间方向
    Vector3 GetDoorWorldDirection(Door door)
    {
        if (door.CompareTag("North")) return Vector3.up;
        if (door.CompareTag("South")) return Vector3.down;
        if (door.CompareTag("East")) return Vector3.right;
        if (door.CompareTag("West")) return Vector3.left;

        Debug.LogError($"门 {door.name} 缺少有效的方向标签");
        return Vector3.zero;
    }

    public void ClearAllEnemies()
    {
        // 优化后的敌人清理逻辑
        Enemy[] allEnemies = FindObjectsOfType<Enemy>();
        foreach (Enemy enemy in allEnemies)
        {
            Destroy(enemy.gameObject);
        }

        // 更新所有房间状态
        foreach (Room room in allRooms)
        {
            room.aliveEnemies = 0; // 现在可以正常访问
            foreach (Door door in room.doors)
            {
                if (door.doorState == Door.DoorState.Locked)
                {
                    door.Unlock();
                }
            }
        }
    }

    public void ReportRoomCleared(Room clearedRoom)
    {
        // 添加房间类型检查
        if (clearedRoom.isStartingRoom)
        {
            Debug.Log($"特殊房间 {clearedRoom.name} 不生成道具");
            return;
        }

        clearedRoom.isCleared = true;
        
        if (clearedRoom.TryGetComponent<ItemSpawner>(out var spawner))
        {
            spawner.SpawnRandomItem();
            Debug.Log($"在房间 {clearedRoom.name} 生成道具");
        }
    }


    public void CheckRoomCleared()
    {
        // 修改为仅检测当前房间
        if (currentRoom != null && !currentRoom.isCleared && currentRoom.aliveEnemies <= 0)
        {
            currentRoom.UnlockDoors();
            ReportRoomCleared(currentRoom);
            
            Debug.Log($"当前房间 {currentRoom.name} 已清除");
        }
    }

}