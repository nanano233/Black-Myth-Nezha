using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossEnemy : Enemy
{
    [Header("Boss属性")]
    public int phase = 1;
    public int maxPhase = 2;
    public float phaseTransitionHealth = 0.5f;

    [Header("近战属性")]
    public float attackCooldown = 2f;
    public float chargeSpeed = 8f;
    public float chargeCooldown = 5f;
    public float meleeDamageRange = 2f;
    
    [Header("远程属性")] 
    public float projectileSpeed = 5f;
    public int burstCount = 3;
    public float burstInterval = 0.3f;

    private float lastChargeTime;
    private bool isPhaseTransitioning;
    private float lastAttackTime;

    protected override void Start()
    {
        base.Start();
        enemyType = EnemyType.Boss;
        attackRange = 5f;
        chaseRange = 15f;
    }

    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);

        if (!isPhaseTransitioning && phase < maxPhase &&
            (float)currentHealth / maxHealth <= (1 - phase * phaseTransitionHealth))
        {
            StartCoroutine(PhaseTransition());
        }
    }

    protected override void HandleAttackState(float distance)
    {
        base.HandleAttackState(distance);

        switch (phase)
        {
            case 1: // 第一阶段近战
                HandleMeleePhase();
                break;
            case 2: // 第二阶段远程
                HandleRangedPhase();
                break;
        }
    }

    private void HandleMeleePhase()
    {
        // 冲锋攻击
        if (Time.time - lastChargeTime >= chargeCooldown)
        {
            StartCoroutine(ChargeAttack());
            lastChargeTime = Time.time;
        }
    }

    private void HandleRangedPhase()
    {
        // 远程射击
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            StartCoroutine(ShootProjectiles());
            lastAttackTime = Time.time;
        }
    }

    private IEnumerator ChargeAttack()
    {
        Vector2 chargeDirection = (player.position - transform.position).normalized;
        float originalSpeed = moveSpeed;
        
        // 冲锋阶段
        moveSpeed = chargeSpeed;
        yield return new WaitForSeconds(0.8f);
        moveSpeed = originalSpeed;

        // 碰撞检测
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, meleeDamageRange);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                hit.GetComponent<PlayerHealth>().TakeDamage();
            }
        }
    }

    private IEnumerator ShootProjectiles()
    {
        Vector2 shootDirection = (player.position - transform.position).normalized;
        
        for (int i = 0; i < burstCount; i++)
        {
            GameObject projectile = Instantiate(
                projectilePrefab,
                transform.position,
                Quaternion.identity
            );

            if (projectile.TryGetComponent<FlameController>(out var flame))
            {
                flame.Initialize(transform.position, shootDirection * projectileSpeed);
                flame.properties.damageMultiplier = damage / PlayerControl.Instance.GetPlayerDamage();
            }
            
            yield return new WaitForSeconds(burstInterval);
        }
    }

    private IEnumerator PhaseTransition()
    {
        isPhaseTransitioning = true;
        phase++;

        Debug.Log($"Boss进入第{phase}阶段！");
        
        // 第二阶段增强
        if (phase == 2)
        {
            moveSpeed *= 0.8f;
            attackCooldown = 3f;
            attackRange = 10f;
        }

        // 无敌时间和动画
        spriteRenderer.color = Color.blue;
        yield return new WaitForSeconds(2f);
        spriteRenderer.color = originalColor;

        isPhaseTransitioning = false;
    }

    protected override void Die()
    {
        //RoomManager.Instance.SpawnNextLevelPortal();
        base.Die();
    }
}