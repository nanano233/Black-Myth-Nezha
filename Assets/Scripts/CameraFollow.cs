using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Camerafollow : MonoBehaviour
{
    public Transform target;      // 跟随的目标
    public float smoothSpeed = 0.125f; // 平滑跟随速度
    public Vector2 minBounds;     // 最小边界坐标（X,Y）
    public Vector2 maxBounds;     // 最大边界坐标（X,Y）

    private Vector3 velocity = Vector3.zero;
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        if (target == null)
        {
            Debug.LogWarning("Camera target not assigned!");
            return;
        }

        if (transform.position != target.position)
        {
            // 计算目标位置时增加偏移量（根据角色实际中心点调整）
            Vector3 targetPosition = target.position;
            targetPosition.z = transform.position.z; // 保持相机的Z轴位置不变
            targetPosition.x = Mathf.Clamp(targetPosition.x, minBounds.x, maxBounds.x);
            targetPosition.y = Mathf.Clamp(targetPosition.y, minBounds.y, maxBounds.y);

            // 平滑移动
            transform.position = Vector3.SmoothDamp(transform.position,
                targetPosition, ref velocity, smoothSpeed);
        }
    }
    
    public void SetBounds(Vector2 min, Vector2 max)
    {
        minBounds = min;
        maxBounds = max;
    }
}
