using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlayerControl : MonoBehaviour
{
    [Header("玩家属性")]
    public float baseDamage = 1f;
    public float currentDamage;
    public float baseMoveSpeed = 4f;
    public float currentMoveSpeed;
    // 在现有属性区域新增
    private float moveSpeedMultiplier = 1f;
    // 在现有属性下新增
    private float damageMultiplier = 1f;
    private List<IDamageModifier> damageModifiers = new List<IDamageModifier>();
    public static PlayerControl Instance; // 新增静态实例

    //动画组件
    private Animator ani;
    //刚体组件
    public Rigidbody2D rb;
    // 添加面朝方向属性（公开获取）
    public Vector2 FaceDirection => new Vector2(lastFaceH, lastFaceV);

    void Awake() // 新增Awake方法
    {
        // 修复重复玩家问题
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 确保玩家在场景加载后可见
        gameObject.SetActive(true);

    }

    void Start()
    {
        //获取动画组件
        ani = GetComponent<Animator>();
        //获取刚体组件
        rb = GetComponent<Rigidbody2D>();

        // 获取PlayerShooting组件
        if (shootingSystem == null)
        {
            shootingSystem = GetComponent<PlayerShooting>();
        }

        // 初始化玩家状态
        currentDamage = baseDamage;
        currentMoveSpeed = baseMoveSpeed;
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
        rb.velocity = moveDir.normalized * currentMoveSpeed;


        // 新增：同步面朝方向到射击系统
        if (shootingSystem != null && (h != 0 || v != 0))
        {
            shootingSystem.UpdateFireDirection(new Vector2(lastFaceH, lastFaceV));
        }
        // 新增：方向键持续射击检测（仅方向键，不含WASD）
        bool isShooting = false;
        foreach (var key in new KeyCode[] {
            KeyCode.LeftArrow, KeyCode.RightArrow,
            KeyCode.DownArrow, KeyCode.UpArrow })
        {
            if (Input.GetKey(key))
            {
                isShooting = true;
                break;
            }
        }

        // 使用面朝方向进行射击
        if (isShooting && shootingSystem != null)
        {
            shootingSystem.UpdateFireDirection(FaceDirection);
            shootingSystem.Shoot();
        }

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
        // 新增位置验证
        if (float.IsNaN(position.x) || float.IsNaN(position.y))
        {
            position = Vector3.zero;
            Debug.LogWarning("检测到非法坐标，已重置");
        }

        // 确保Transform组件存在
        if (transform == null)
        {
            Debug.LogError("Transform组件丢失！");
            return;
        }

        // 直接设置位置
        transform.position = position;

        // 确保刚体存在
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.MovePosition(position);
            rb.WakeUp();
        }

        // 强制重置动画状态
        if (ani != null)
        {
            ani.Play("Idle", 0, 0f);
            ani.SetFloat("Speed", 0f);
        }

        // 强制启用渲染器和碰撞体
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.enabled = true;
            renderer.forceRenderingOff = false;
        }

        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = true;
        }

        // 重置输入状态
        lastDirectionKey = KeyCode.None;
        lastHorizontalKey = KeyCode.None;
        lastVerticalKey = KeyCode.None;
        // 重置输入状态
        ResetInputState();

        Debug.Log($"玩家传送完成: {position}");
    }


    // 添加初始化方法
    public void Initialize()
    {
        // 确保组件获取
        if (ani == null) ani = GetComponent<Animator>();
        if (rb == null) rb = GetComponent<Rigidbody2D>();

        // 强制重置位置
        transform.position = Vector3.zero;

        // 重置状态
        lastFaceH = 0f;
        lastFaceV = -1f;
        lastDirectionKey = KeyCode.None;
        lastHorizontalKey = KeyCode.None;
        lastVerticalKey = KeyCode.None;

        // 确保碰撞体和渲染器启用
        if (TryGetComponent<Collider2D>(out var collider))
        {
            collider.enabled = true;
        }

        if (TryGetComponent<SpriteRenderer>(out var renderer))
        {
            renderer.enabled = true;
        }

        Debug.Log("玩家控制器完全初始化");
    }



    [Header("组件引用")]
    public PlayerShooting shootingSystem;

    private List<IProjectileModifier> activeModifiers = new List<IProjectileModifier>();

    public void AddProjectileModifier(IProjectileModifier modifier)
    {
        activeModifiers.Add(modifier);
        UpdateProjectileProperties();
    }

    public void UpdateProjectileProperties()
    {
        // 创建属性副本
        ProjectileProperties newProperties = new ProjectileProperties();
        newProperties.ResetToBase();

        // 应用所有修改器
        foreach (var modifier in activeModifiers)
        {
            modifier.ApplyEffect(newProperties);
        }

        // 确保射击系统更新属性
        if (shootingSystem != null)
        {
            shootingSystem.UpdateProjectileProperties(new ProjectileProperties()
            {
                damageMultiplier = this.damageMultiplier
            });
        }
    }

    // 新增：获取玩家伤害值
    public float GetPlayerDamage()
    {
        return currentDamage;
    }

    // 新增：添加伤害修改器
    public void AddDamageModifier(IDamageModifier modifier)
    {
        damageModifiers.Add(modifier);
        UpdateDamage();
    }

    // 更新伤害计算
    private void UpdateDamage()
    {
        currentDamage = baseDamage * damageMultiplier;
        foreach (var modifier in damageModifiers)
        {
            currentDamage = modifier.ModifyDamage(currentDamage);
        }
    }
    // 新增保存方法
    public void SavePlayerData()
    {
        PlayerPrefs.SetFloat("MoveSpeedMultiplier", moveSpeedMultiplier);
        PlayerPrefs.SetFloat("DamageMultiplier", damageMultiplier);
        PlayerPrefs.SetFloat("PlayerDamage", currentDamage);
        PlayerPrefs.SetFloat("MoveSpeed", currentMoveSpeed);
        PlayerPrefs.Save();
    }
    // 新增加载方法
    public void LoadPlayerData()
    {
        moveSpeedMultiplier = PlayerPrefs.GetFloat("MoveSpeedMultiplier", 1f);
        currentMoveSpeed = baseMoveSpeed * moveSpeedMultiplier;
        damageMultiplier = PlayerPrefs.GetFloat("DamageMultiplier", 1f);
        currentDamage = PlayerPrefs.GetFloat("PlayerDamage", 1f); // 默认值1
        currentMoveSpeed = PlayerPrefs.GetFloat("MoveSpeed", 4f);
    }

    // 新增输入状态重置方法（关键修复）
    public void ResetInputState()
    {
        lastDirectionKey = KeyCode.None;
        lastHorizontalKey = KeyCode.None;
        lastVerticalKey = KeyCode.None;
        lastFaceH = 0f;
        lastFaceV = -1f;

        // 重置刚体速度和动画状态
        if (rb != null) rb.velocity = Vector2.zero;
        if (ani != null)
        {
            ani.SetFloat("Horizontal", 0);
            ani.SetFloat("Vertical", -1);
            ani.SetFloat("Speed", 0);
            ani.Play("Idle", 0, 0f);
        }

        Debug.Log("玩家输入状态已重置");
    }
    public void AddDamageMultiplier(float multiplier)
    {
        damageMultiplier += multiplier;
        UpdateDamage();
    }


    // 在现有属性下新增
    public float GetDamageMultiplier()
    {
        return damageMultiplier;
    }

    // 在PlayerControl.cs中添加
    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"玩家碰撞到: {other.gameObject.name}");
    }
    
    public void AddMoveSpeedMultiplier(float multiplier)
    {
        moveSpeedMultiplier += multiplier;
        currentMoveSpeed = baseMoveSpeed * moveSpeedMultiplier;
    }
}