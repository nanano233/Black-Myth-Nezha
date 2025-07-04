using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

// 敌人类型枚举
public enum EnemyType
{
    Basic,
    Shooter,
    Boss
}

// 敌人状态枚举
public enum EnemyState
{
    Idle,
    Chase,
    Attack,
    Retreat,
    Dead
}

// 敌人基类
public abstract class Enemy : MonoBehaviour
{
    [Header("基本属性")]
    public EnemyType enemyType;
    public float maxHealth = 10f;  // 改为float
    public int damage = 1;
    public float moveSpeed = 2f;
    public float attackRange = 3f;
    public float chaseRange = 8f;
    public float retreatDistance = 4f;
    
    [Header("状态")]
    public EnemyState currentState = EnemyState.Idle;
    
    [Header("视觉反馈")]
    public Color hitColor = Color.red;
    public GameObject deathEffect;
    public GameObject projectilePrefab;
    public GameObject attackIndicator;
    
    protected float currentHealth;  // 改为float
    protected Transform player;
    protected SpriteRenderer spriteRenderer;
    protected Color originalColor;
    protected Animator animator;
    
    [Header("掉落物")]
    public GameObject[] dropItems; // 红心、灵石、灵气等
    
    // 修改事件类型为C#标准事件
    public event System.Action OnDeath;  // 替换原来的UnityEvent
    // 添加命中事件
    public event System.Action OnHit; 

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        originalColor = spriteRenderer.color;

        if (attackIndicator != null)
            attackIndicator.SetActive(false);
    }

    protected virtual void Update()
    {
        if (currentState == EnemyState.Dead) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        switch (currentState)
        {
            case EnemyState.Idle:
                HandleIdleState(distanceToPlayer);
                break;
            case EnemyState.Chase:
                HandleChaseState(distanceToPlayer);
                break;
            case EnemyState.Attack:
                HandleAttackState(distanceToPlayer);
                break;
            case EnemyState.Retreat:
                HandleRetreatState(distanceToPlayer);
                break;
        }
        
        // 新增：根据移动方向翻转图片
        if (player.position.x > transform.position.x)
        {
            spriteRenderer.flipX = true;
        }
        else
        {
            spriteRenderer.flipX = false;
        }
    }
    
    protected virtual void HandleIdleState(float distance)
    {
        if (distance <= chaseRange)
        {
            ChangeState(EnemyState.Chase);
        }
    }
    
    protected virtual void HandleChaseState(float distance)
    {
        // 追逐玩家
        Vector2 direction = (player.position - transform.position).normalized;
        transform.position += (Vector3)direction * moveSpeed * Time.deltaTime;
        
        // 在攻击范围内转为攻击状态
        if (distance <= attackRange)
        {
            ChangeState(EnemyState.Attack);
        }
        // 超出追逐范围转为闲置状态
        else if (distance > chaseRange)
        {
            ChangeState(EnemyState.Idle);
        }
    }
    
    protected virtual void HandleAttackState(float distance)
    {
        // 超出攻击范围转为追逐状态
        if (distance > attackRange)
        {
            ChangeState(EnemyState.Chase);
            return;
        }
        
        // 攻击逻辑在子类中实现
    }
    
    protected virtual void HandleRetreatState(float distance)
    {
        // 撤退逻辑在子类中实现
    }
    
    // 修改后的TakeDamage方法
    public virtual void TakeDamage(float damage)
    {
        if (currentState == EnemyState.Dead) return;
        
        currentHealth -= damage;
        OnHit?.Invoke();
        StartCoroutine(FlashHitEffect());
        
        if (currentHealth <= 0f)
        {
            Die();
        }
    }
    
    protected virtual void Die()
    {
        currentState = EnemyState.Dead;
        OnDeath?.Invoke();  // 触发C#事件

        // 死亡特效
        if (deathEffect != null)
            Instantiate(deathEffect, transform.position, Quaternion.identity);

        // 掉落物品
        DropItem();

        // 销毁敌人
        Destroy(gameObject, 0.1f);
    }
    
    protected virtual void DropItem()
    {
        if (dropItems.Length > 0 && Random.value < 0.3f)
        {
            int index = Random.Range(0, dropItems.Length);
            Instantiate(dropItems[index], transform.position, Quaternion.identity);
        }
    }
    
    protected IEnumerator FlashHitEffect()
    {
        spriteRenderer.color = hitColor;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
    }
    
    protected void ChangeState(EnemyState newState)
    {
        currentState = newState;
        
        // 状态切换动画
        if (animator != null)
        {
            animator.SetInteger("State", (int)newState);
        }
    }
    
    // 显示攻击指示器
    protected IEnumerator ShowAttackIndicator(float duration)
    {
        if (attackIndicator != null)
        {
            attackIndicator.SetActive(true);
            yield return new WaitForSeconds(duration);
            attackIndicator.SetActive(false);
        }
    }
    
    protected virtual void OnDrawGizmosSelected()
    {
        // 显示追逐范围
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        
        // 显示攻击范围
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
