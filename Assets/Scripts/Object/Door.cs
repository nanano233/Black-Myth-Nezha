using UnityEngine;

public class Door : MonoBehaviour
{
    // 新增状态枚举
    public enum DoorState { Unconnected, Locked, Unlocked }

    [Header("精灵设置")]
    public Sprite lockedSprite;
    public Sprite unlockedSprite;
    [Header("Boss房特殊精灵")]  // 新增Boss房专用精灵
    public Sprite bossLockedSprite; 
    public Sprite bossUnlockedSprite;

    [Header("门状态")]
    public DoorState doorState = DoorState.Unconnected;
    [Header("碰撞体设置")]
    [Tooltip("墙壁碰撞体（非触发器）")] 
    [SerializeField] private BoxCollider2D wallCollider; // 需要手动分配
    [Tooltip("传送触发器（需勾选IsTrigger）")]
    [SerializeField] private BoxCollider2D triggerCollider; // 需要手动分配
    
    [Header("连接房间")]
    public Room connectedRoom; // 修改访问权限为public
    [Header("所属房间")]
    public Room parentRoom;
    [Header("引用")]
    [SerializeField] private SpriteRenderer doorSprite; // 修改为SerializeField并添加自动获取

    [Header("颜色设置")]
    public Color lockedColor = Color.red;
    public Color unlockedColor = Color.green;

    void Awake()
    {
        // 自动获取SpriteRenderer组件（如果未分配）
        if (doorSprite == null)
        {
            doorSprite = GetComponentInChildren<SpriteRenderer>();
            if (doorSprite == null)
            {
                Debug.LogError($"门 {name} 缺少SpriteRenderer组件");
            }
        }
    }
    // 添加验证方法
    public void ValidateDoorOrientation()
    {
        // 检查是否有有效的方向标签
        if (!CompareTag("North") && !CompareTag("South") &&
            !CompareTag("East") && !CompareTag("West"))
        {
            Debug.LogError($"门 {name} 缺少方向标签！请添加North/South/East/West标签");

            // 尝试根据位置自动分配标签
            Vector3 relativePos = transform.position - transform.parent.position;
            if (relativePos.y > 0) tag = "North";
            else if (relativePos.y < 0) tag = "South";
            else if (relativePos.x > 0) tag = "East";
            else if (relativePos.x < 0) tag = "West";

            Debug.LogWarning($"已根据位置自动分配标签: {tag}");
        }
    }

    // 在Start方法中调用验证
    void Start()
    {
        ValidateDoorOrientation();
        UpdateDoorAppearance();
        Debug.Log($"门 {name} 初始化完成，方向: {tag}");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 增加空引用检查和安全条件
        if (other.CompareTag("Player") &&
            doorState == DoorState.Unlocked &&
            connectedRoom != null &&
            RoomManager.Instance != null) // 新增实例检查
        {
            RoomManager.Instance.EnterRoom(connectedRoom, this);
        }
        // else if (connectedRoom == null)
        // {
        //     Debug.LogWarning($"门 {name} 未连接有效房间");
        // }
    
    }

    public void Lock()
    {
        doorState = DoorState.Locked;  // 修复：使用新的枚举状态
        UpdateDoorAppearance();
    }

    public void Unlock()
    {
        doorState = DoorState.Unlocked; // 修复：使用新的枚举状态
        UpdateDoorAppearance();
    }

    public void UpdateDoorAppearance()
    {
        if (doorSprite != null)
        {
            // 未连接状态处理
            if (doorState == DoorState.Unconnected)
            {
                doorSprite.enabled = false;
                if (wallCollider != null) wallCollider.enabled = true;
                if (triggerCollider != null) triggerCollider.enabled = false;
                return;
            }
            // 新增精灵切换逻辑
            doorSprite.enabled = doorState != DoorState.Unconnected;
            // 根据房间类型选择精灵
            bool isBossDoor = (parentRoom != null && parentRoom.isBossRoom) || 
                            (connectedRoom != null && connectedRoom.isBossRoom);
            
            doorSprite.sprite = doorState switch
            {
                DoorState.Locked => isBossDoor ? bossLockedSprite : lockedSprite,
                DoorState.Unlocked => isBossDoor ? bossUnlockedSprite : unlockedSprite,
                _ => null
            };
        }

        // 控制碰撞体状态
        if (wallCollider != null)
        {
            wallCollider.enabled = doorState != DoorState.Unlocked;
        }
        if (triggerCollider != null)
        {
            triggerCollider.enabled = doorState == DoorState.Unlocked;
        }
    }

    public void Initialize()
    {
        // 重置连接状态
        if (connectedRoom == null)
            doorState = DoorState.Unconnected;
        UpdateDoorAppearance();
    }
    
    public void ConnectRoom(Room room)
    {
        connectedRoom = room;
        
        // 新增空检查和安全解锁逻辑
        if (room != null)
        {
            doorState = DoorState.Unlocked;
            Debug.Log($"门 {name} 已连接到房间: {room.name}");
        }
        else
        {
            doorState = DoorState.Unconnected;
            Debug.LogWarning($"门 {name} 收到空房间连接");
        }
        
        UpdateDoorAppearance();
    }
}