using UnityEngine;

public class MoveSpeedItem : MonoBehaviour
{
    [SerializeField] private float speedBoost = 0.3f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && PlayerControl.Instance != null)
        {
            PlayerControl.Instance.AddMoveSpeedMultiplier(speedBoost);
            Destroy(gameObject);
            Debug.Log($"移动速度已提升，当前速度：{PlayerControl.Instance.currentMoveSpeed}");
        }
    }
}