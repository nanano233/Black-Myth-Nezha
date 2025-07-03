using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayerControl : MonoBehaviour
{
    public static PlayerControl Instance; // 新增静态实例

    //动画组件
    private Animator ani;
    //刚体组件
    private Rigidbody2D rb;

    void Awake() // 新增Awake方法
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        //获取动画组件
        ani = GetComponent<Animator>();
        //获取刚体组件
        rb = GetComponent<Rigidbody2D>();
    }

    // 存储最后的面朝方向（默认朝下）
    private float lastFaceH = 0f;
    private float lastFaceV = -1f;

    // 跟踪最后按下的方向键
    private KeyCode lastDirectionKey = KeyCode.None;

    // 方向键配置字典，包含WASD
    private Dictionary<KeyCode, Vector2> directionMap = new Dictionary<KeyCode, Vector2>()
    {
        { KeyCode.LeftArrow, Vector2.left }, { KeyCode.A, Vector2.left },
        { KeyCode.RightArrow, Vector2.right }, { KeyCode.D, Vector2.right },
        { KeyCode.DownArrow, Vector2.down }, { KeyCode.S, Vector2.down },
        { KeyCode.UpArrow, Vector2.up }, { KeyCode.W, Vector2.up }
    };

    // 跟踪最后按下的水平/垂直移动键
    private KeyCode lastHorizontalKey = KeyCode.None;
    private KeyCode lastVerticalKey = KeyCode.None;

    void Update()
    {
        if (!gameObject.activeSelf) return;
        // 修复移动控制：处理最后按下的WASD键
        float h = 0f;
        float v = 0f;

        // 修复方向检测：仅用方向键控制面朝方向
        bool hasFaceInput = false;
        Vector2 faceDirection = new Vector2(lastFaceH, lastFaceV);

        // 独立检测方向键（上下左右箭头）
        foreach (var key in new KeyCode[] {
            KeyCode.LeftArrow, KeyCode.RightArrow,
            KeyCode.DownArrow, KeyCode.UpArrow })
        {
            if (Input.GetKeyDown(key))
            {
                lastDirectionKey = key;
                hasFaceInput = true;
            }
        }

        // 新增：WASD方向检测（与方向键相同逻辑）
        foreach (var key in new KeyCode[] { KeyCode.A, KeyCode.D, KeyCode.S, KeyCode.W })
        {
            if (Input.GetKeyDown(key))
            {
                // 新增：检查是否有方向键被按住
                bool hasArrowKeyHeld = false;
                foreach (var arrowKey in new KeyCode[] {
                    KeyCode.LeftArrow, KeyCode.RightArrow,
                    KeyCode.DownArrow, KeyCode.UpArrow })
                {
                    if (Input.GetKey(arrowKey))
                    {
                        hasArrowKeyHeld = true;
                        break;
                    }
                }

                // 仅在无方向键输入且没有方向键被按住时记录WASD
                if (!hasFaceInput && !hasArrowKeyHeld)
                {
                    lastDirectionKey = key;
                    hasFaceInput = true;
                }
            }
        }

        // 持续响应最后按下的方向键（包含WASD）
        if (lastDirectionKey != KeyCode.None && Input.GetKey(lastDirectionKey))
        {
            hasFaceInput = directionMap.ContainsKey(lastDirectionKey);
        }

        // 修改后的清除逻辑（同时检测方向键和WASD）
        if (lastDirectionKey != KeyCode.None && !Input.GetKey(lastDirectionKey))
        {
            KeyCode newKey = KeyCode.None;
            // 优先寻找方向键
            foreach (var key in new KeyCode[] {
                KeyCode.LeftArrow, KeyCode.RightArrow,
                KeyCode.DownArrow, KeyCode.UpArrow })
            {
                if (Input.GetKey(key))
                {
                    newKey = key;
                    break; // 方向键优先
                }
            }
            // 没有方向键再找WASD
            if (newKey == KeyCode.None)
            {
                foreach (var key in new KeyCode[] { KeyCode.A, KeyCode.D, KeyCode.S, KeyCode.W })
                {
                    if (Input.GetKey(key))
                    {
                        newKey = key;
                    }
                }
            }
            lastDirectionKey = newKey;
            hasFaceInput = newKey != KeyCode.None;
        }

        // 应用方向输入
        if (hasFaceInput && directionMap.TryGetValue(lastDirectionKey, out Vector2 newDir))
        {
            faceDirection = newDir;
        }

        // 检测水平方向按键事件
        if (Input.GetKeyDown(KeyCode.A)) lastHorizontalKey = KeyCode.A;
        if (Input.GetKeyDown(KeyCode.D)) lastHorizontalKey = KeyCode.D;

        // 检测垂直方向按键事件
        if (Input.GetKeyDown(KeyCode.W)) lastVerticalKey = KeyCode.W;
        if (Input.GetKeyDown(KeyCode.S)) lastVerticalKey = KeyCode.S;

        // 计算水平移动（最后按下优先）
        if (lastHorizontalKey != KeyCode.None && Input.GetKey(lastHorizontalKey))
        {
            h = lastHorizontalKey == KeyCode.A ? -1 : 1;
        }
        else
        {
            if (Input.GetKey(KeyCode.A)) h = -1;
            else if (Input.GetKey(KeyCode.D)) h = 1;
            lastHorizontalKey = KeyCode.None;
        }

        // 计算垂直移动（最后按下优先）
        if (lastVerticalKey != KeyCode.None && Input.GetKey(lastVerticalKey))
        {
            v = lastVerticalKey == KeyCode.S ? -1 : 1;
        }
        else
        {
            if (Input.GetKey(KeyCode.S)) v = -1;
            else if (Input.GetKey(KeyCode.W)) v = 1;
            lastVerticalKey = KeyCode.None;
        }

        // 更新面朝方向逻辑（优先方向键，其次移动方向）
        if (hasFaceInput)
        {
            lastFaceH = faceDirection.x;
            lastFaceV = faceDirection.y;
        }
        else if (h != 0 || v != 0)
        {
            lastFaceH = h;
            lastFaceV = v;
        }
        // 新增：当使用WASD移动时立即更新方向（若没有方向键输入）
        if (!hasFaceInput && (h != 0 || v != 0))
        {
            lastFaceH = h;
            lastFaceV = v;
        }

        // 设置动画参数（始终使用最后记录的方向）
        ani.SetFloat("Horizontal", lastFaceH);
        ani.SetFloat("Vertical", lastFaceV);
        //切换动画状态
        // 设置刚体速度（修复移动计算）
        Vector2 moveDir = new Vector2(h, v);
        ani.SetFloat("Speed", moveDir.magnitude);
        rb.velocity = moveDir.normalized * 4f;


        // 新增清理功能（添加在Update方法末尾）
        if (Input.GetKeyDown(KeyCode.F9))
        {
            RoomManager.Instance.ClearAllEnemies();
            Debug.Log("已清理所有敌人");
        }
    }

    // 添加传送后激活检测
    void OnEnable()
    {
        if (CameraController.Instance != null)
        {
            CameraController.Instance.target = transform;
        }
    }
    
    // 添加传送方法
    public void TeleportTo(Vector3 position)
    {
        // 确保玩家在传送前停止移动
        if (rb != null) 
        {
            rb.velocity = Vector2.zero;
        }
        
        transform.position = position;
        
        // 重置输入状态，防止传送后保持原有方向
        lastDirectionKey = KeyCode.None;
        lastHorizontalKey = KeyCode.None;
        lastVerticalKey = KeyCode.None;
        
        // 重置动画状态
        ani.SetFloat("Speed", 0f);
        
        Debug.Log($"玩家传送至位置: {position}");
    }

    // 添加初始化方法
    public void Initialize()
    {
        // 确保动画组件和刚体组件已正确获取
        if (ani == null) ani = GetComponent<Animator>();
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        
        // 初始化玩家状态
        lastFaceH = 0f;
        lastFaceV = -1f;
        lastDirectionKey = KeyCode.None;
        lastHorizontalKey = KeyCode.None;
        lastVerticalKey = KeyCode.None;
        
        Debug.Log("玩家控制器初始化完成");
    }
}