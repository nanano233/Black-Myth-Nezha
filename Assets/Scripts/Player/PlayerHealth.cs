using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// 玩家健康类

public class PlayerHealth : MonoBehaviour
{
    public int damage = 1; // 玩家受到伤害的值
    public int maxHealth = 6;
    private int currentHealth;

    // 新增受击状态相关参数
    public float invincibleDuration = 1f;
    private bool isInvincible = false;
    private Animator animator;
    private SpriteRenderer spriteRenderer; // 新增渲染器引用

    private void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>(); // 获取渲染器组件
    }

    public void TakeDamage()
    {
        if (isInvincible) return;
        
        currentHealth -= damage;
        // 移除动画触发，改为闪烁效果
        StartCoroutine(InvincibilityFrame());
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // 新增无敌时间协程
    private IEnumerator InvincibilityFrame()
    {
        isInvincible = true;
        float endTime = Time.time + invincibleDuration;
        
        // 闪烁逻辑
        while (Time.time < endTime)
        {
            spriteRenderer.color = new Color(1, 1, 1, 0.5f); // 半透明
            yield return new WaitForSeconds(0.05f);
            spriteRenderer.color = Color.white; // 恢复
            yield return new WaitForSeconds(0.05f);
        }
        
        isInvincible = false;
    }
    private void Die()
    {
        // 玩家死亡逻辑
        Debug.Log("玩家死亡");

        // 禁用玩家控制
        if (TryGetComponent<PlayerControl>(out var control))
        {
            control.enabled = false;
        }

        // 停止物理运动
        if (TryGetComponent<Rigidbody2D>(out var rb))
        {
            rb.velocity = Vector2.zero;
            rb.simulated = false;
        }

        // 改为通过GameManager处理
        GameManager.Instance?.HandlePlayerDeath();
    
    }

}