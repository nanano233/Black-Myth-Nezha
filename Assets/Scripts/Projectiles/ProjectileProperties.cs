using UnityEngine;

[System.Serializable]
public class ProjectileProperties
{
    [Header("弹道属性")]
    public float baseSpeed = 10f;
    public float baseRange = 10f;
    public float baseFireRate = 0.5f;
    public float damageMultiplier = 1f; // 伤害倍率
    
    [Header("当前属性")]
    public float currentSpeed;
    public float currentRange;
    public float currentFireRate;
    
    public void ResetToBase()
    {
        currentSpeed = baseSpeed;
        currentRange = baseRange;
        currentFireRate = baseFireRate;
        damageMultiplier = 1f;
    }
}