using UnityEngine;

// 示例道具：伤害提升道具
public class DamageBoostItem : MonoBehaviour, IDamageModifier
{
    public float damageMultiplier = 1.5f;
    
    public float ModifyDamage(float currentDamage)
    {
        return currentDamage * damageMultiplier;
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerControl.Instance.AddDamageModifier(this);
            Destroy(gameObject);
        }
    }
}