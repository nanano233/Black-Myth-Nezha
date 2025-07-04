using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// 射击型敌人
public class ShooterEnemy : Enemy
{
    [Header("射击敌人属性")]
    public float shootCooldown = 3f;
    public float projectileSpeed = 5f;
    public float windUpTime = 0.5f;

    private float lastShootTime;
    [Header("发射点")]
    public Transform firePoint;

    protected override void Start()
    {
        base.Start();
        enemyType = EnemyType.Shooter;
        retreatDistance = 5f; // 设置撤退距离
    }

    protected override void HandleAttackState(float distance)
    {
        base.HandleAttackState(distance);

        // 如果太近则撤退
        if (distance < retreatDistance)
        {
            ChangeState(EnemyState.Retreat);
            return;
        }

        // 射击冷却
        if (Time.time - lastShootTime >= shootCooldown)
        {
            StartCoroutine(Shoot());
            lastShootTime = Time.time;
        }
    }

    protected override void HandleRetreatState(float distance)
    {
        // 撤退到安全距离
        if (distance < retreatDistance)
        {
            Vector2 direction = (transform.position - player.position).normalized;
            transform.position += (Vector3)direction * moveSpeed * Time.deltaTime;
        }
        else
        {
            ChangeState(EnemyState.Attack);
        }
    }

    private IEnumerator Shoot()
    {
        // 显示攻击预警
        StartCoroutine(ShowAttackIndicator(windUpTime));
        yield return new WaitForSeconds(windUpTime);

        if (projectilePrefab != null && currentState != EnemyState.Dead)
        {
            // 使用敌人自身的发射点
            Vector3 spawnPosition = firePoint != null ? firePoint.position : transform.position;
            
            // 统一使用Vector3进行计算
            Vector3 direction = (player.position - spawnPosition).normalized;
            FlameController flame = EnemyFlamePool.Instance.GetFlame();

            if (flame != null)
            {
                flame.gameObject.SetActive(true);
                flame.Initialize(
                    (Vector2)spawnPosition,  // 显式转换为Vector2
                    (Vector2)direction * projectileSpeed
                );
            }

            // 播放射击动画
            if (animator != null)
                animator.SetTrigger("Shoot");
        }
    }
}