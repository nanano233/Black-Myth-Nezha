using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    [Header("射击配置")]
    public Transform firePoint;
    public ProjectileProperties projectileProperties;

    [Header("对象池")]
    public FlamePool flamePool;

    public static PlayerShooting Instance; 

    private float nextFireTime = 0f;
    private Vector2 lastDirection = Vector2.right;
    [Header("基础属性")]
    public ProjectileProperties baseProjectileProperties; // 新增基础属性引用


    void Awake()
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
        // 确保对象池存在
        if (FlamePool.Instance == null)
        {
            gameObject.AddComponent<FlamePool>();
            FlamePool.Instance.InitializePool();
        }
        else
        {
            flamePool = FlamePool.Instance;
        }

        // 确保属性初始化
        if (projectileProperties == null)
        {
            projectileProperties = new ProjectileProperties();
            projectileProperties.ResetToBase();
        }
        else
        {
            flamePool = FlamePool.Instance;
        }
        // 修改初始化方式
        if (baseProjectileProperties != null)
        {
            UpdateProjectileProperties(baseProjectileProperties);
        }
        else
        {
            Debug.LogError("未设置基础弹道属性");
        }
    
    }

    void Update()
    {
        HandleAiming();

        if (Input.GetButton("Fire1") && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + projectileProperties.currentFireRate;
        }
    }

    void HandleAiming()
    {
        // 优先使用键盘输入方向
        if (PlayerControl.Instance != null &&
            (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow) ||
             Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow))
        )
        {
            lastDirection = PlayerControl.Instance.FaceDirection;
        }
        else // 备用鼠标瞄准
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 direction = (mousePos - (Vector2)transform.position).normalized;
            lastDirection = direction;
        }

        // 在方向基础上添加垂直偏移（假设角色高度为1单位）
        firePoint.localPosition = (lastDirection * 0.5f) + Vector2.up * 2.7f;
    
    }

    public void Shoot()
    {
        // 添加冷却时间检查
        if (Time.time < nextFireTime) return;
        // 添加空引用检查
        if (flamePool == null)
        {
            Debug.LogError("子弹池未初始化！当前对象池状态：" + (FlamePool.Instance != null ? "已存在" : "未创建"));
            return;
        }
        FlameController flame = flamePool.GetFlame();
        if (flame != null)
        {
            flame.properties = projectileProperties; // 确保属性传递
            flame.gameObject.SetActive(true);
            flame.Initialize(firePoint.position, lastDirection);
            nextFireTime = Time.time + projectileProperties.currentFireRate; // 更新下次射击时间
        }
        else
        {
            Debug.LogWarning("获取子弹实例失败");
        }

        // 播放射击音效
        // AudioManager.Instance.PlaySFX("Shoot");
    }

    public void UpdateProjectileProperties(ProjectileProperties newProperties)
    {
        // 改为直接更新当前实例的属性，而不是创建新实例
        projectileProperties.baseSpeed = newProperties.baseSpeed;
        projectileProperties.baseRange = newProperties.baseRange;
        projectileProperties.baseFireRate = newProperties.baseFireRate;
        projectileProperties.damageMultiplier = newProperties.damageMultiplier;

        // 重置当前值
        projectileProperties.ResetToBase();

        // 更新对象池
        flamePool.UpdateAllFlameProperties(projectileProperties);
    }

    public void UpdateFireDirection(Vector2 newDirection)
    {
        lastDirection = newDirection;
        // 保持相同的垂直偏移计算
        firePoint.localPosition = (lastDirection * 0.5f) + Vector2.up * 2.7f;
    
    }
}