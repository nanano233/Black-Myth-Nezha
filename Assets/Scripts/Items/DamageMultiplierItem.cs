using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageMultiplierItem : MonoBehaviour
{
    // 添加碰撞体验证
    void Start()
    {
        Collider2D collider = GetComponent<Collider2D>();
        if (collider == null || !collider.isTrigger)
        {
            Debug.LogError("道具缺少Trigger碰撞体！");
        }
    }
private void OnTriggerEnter2D(Collider2D other)
{
    if (other.CompareTag("Player"))
    {
        // 添加安全访问检查
        if (PlayerControl.Instance != null && PlayerControl.Instance.enabled)
        {
            // 添加空引用保护
            var player = PlayerControl.Instance;
            if (player != null)
            {
                // 更新伤害并刷新子弹属性
                player.AddDamageMultiplier(0.3f);
                player.UpdateProjectileProperties(); // 新增属性刷新调用
                Destroy(gameObject);
                Debug.Log($"伤害倍率已提升，当前：{player.GetDamageMultiplier()}x");
            }
        }
        else
        {
            Debug.LogWarning("玩家控制器不可用");
        }
    }
}

    // 新增可视化调试
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}