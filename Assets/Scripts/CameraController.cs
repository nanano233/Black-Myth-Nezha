using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance;
    
    [Header("跟随设置")]
    public Transform target;
    public float smoothSpeed = 0.1f;
    public Vector3 offset = new Vector3(0, 0, -10);
    
    [Header("边界限制")]
    public Vector2 minBounds;
    public Vector2 maxBounds;
    
    private Vector3 velocity = Vector3.zero;
    private Camera cam;
    
    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void FixedUpdate()
    {
        // 添加空引用保护
        if (target != null && target.gameObject.activeInHierarchy)
        {
            // 计算目标位置时增加偏移量（根据角色实际中心点调整）
            Vector3 targetPosition = target.position;
            targetPosition.z = transform.position.z; // 保持相机的Z轴位置不变
            targetPosition.x = Mathf.Clamp(targetPosition.x, minBounds.x, maxBounds.x);
            targetPosition.y = Mathf.Clamp(targetPosition.y, minBounds.y, maxBounds.y);

            // 平滑移动
            transform.position = Vector3.SmoothDamp(transform.position,
                targetPosition, ref velocity, smoothSpeed);
        }
    }
    
    public void MoveTo(Vector3 position)
    {
        // 移除强制调用FixedUpdate
        Vector3 targetPosition = new Vector3(
            Mathf.Clamp(position.x, minBounds.x, maxBounds.x),
            Mathf.Clamp(position.y, minBounds.y, maxBounds.y),
            offset.z
        );
        transform.position = targetPosition;
        
        // 重置速度参数
        velocity = Vector3.zero;
    }
    
    // 保留原CameraFollow的边界设置方法
    public void SetBounds(Vector2 min, Vector2 max)
    {
        minBounds = min;
        maxBounds = max;
    }
}