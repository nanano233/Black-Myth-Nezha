using UnityEngine;
//子弹
public class FlameController : MonoBehaviour
{
    public ProjectileProperties properties;
    public GameObject impactEffect;
    
    private Vector2 startPosition;
    private Vector2 direction;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(Vector2 startPos, Vector2 dir)
    {
        startPosition = startPos;
        // 添加方向归一化
        direction = dir.normalized;
        transform.position = startPos;


        // 添加范围验证
        if (properties.currentRange <= 0)
        {
            Debug.LogWarning($"无效子弹射程: {properties.currentRange}，使用默认值10");
            properties.currentRange = 10f;
        }
    
        rb.velocity = direction * properties.currentSpeed;
    }

    void Update()
    {
        // 超出射程自动销毁
        if (Vector2.Distance(startPosition, transform.position) >= properties.currentRange)
        {
            ReturnToPool();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 根据子弹来源区分处理逻辑
        if (CompareTag("PlayerProjectile"))
        {
            if (other.CompareTag("Enemy") && other.TryGetComponent<Enemy>(out var enemy))
            {
                // 玩家子弹伤害敌人
                float finalDamage = PlayerControl.Instance.GetPlayerDamage() * properties.damageMultiplier;
                enemy.TakeDamage(finalDamage);
                CreateImpactEffect();
                ReturnToPool();
            }
        }
        else if (CompareTag("EnemyProjectile"))
        {
            // 敌人子弹逻辑...
            if (other.CompareTag("Player"))
            {
                PlayerHealth health = other.GetComponent<PlayerHealth>();
                if (health != null)
                {
                    health.TakeDamage();
                    ReturnToPool();
                }
            }
        
        }

        // 通用碰撞处理（墙壁等）
        if (other.CompareTag("Wall") || other.CompareTag("North") ||
            other.CompareTag("South") || other.CompareTag("East") ||
            other.CompareTag("West"))
        {
            CreateImpactEffect();
            ReturnToPool();
        }
    }

    void CreateImpactEffect()
    {
        if (impactEffect)
        {
            GameObject effect = Instantiate(impactEffect, transform.position, Quaternion.identity);
            Destroy(effect, 0.5f);
        }
    }

    void ReturnToPool()
    {
        gameObject.SetActive(false);
        if (CompareTag("PlayerProjectile") && FlamePool.Instance != null)
        {
            FlamePool.Instance.ReturnFlame(this);
        }
        else if (CompareTag("EnemyProjectile") && EnemyFlamePool.Instance != null)
        {
            EnemyFlamePool.Instance.ReturnFlame(this);
        }
    }

}