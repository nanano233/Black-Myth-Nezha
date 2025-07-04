using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// 基础敌人 - 近战类型
public class BasicEnemy : Enemy
{
    [Header("基础敌人属性")]
    public float attackCooldown = 2f;
    private float lastAttackTime;

    protected override void Start()
    {
        base.Start();
        enemyType = EnemyType.Basic;
    }

    protected override void HandleAttackState(float distance)
    {
        base.HandleAttackState(distance);

        // 攻击冷却
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            Attack();
            lastAttackTime = Time.time;
        }
    }

    private void Attack()
    {
        animator.SetTrigger("Attack");

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                // 添加空检查和安全获取组件
                PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();

                if (playerHealth != null)
                {

                    playerHealth.TakeDamage();
                }
                else
                {
                    Debug.LogWarning($"攻击到玩家对象但未找到PlayerHealth组件：{hit.name}");
                }
            }
        }
    }


}
