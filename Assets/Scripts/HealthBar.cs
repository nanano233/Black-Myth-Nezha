using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HealthBar : MonoBehaviour
{
    [System.Serializable]
    public class HeartPiece
    {
        public RectTransform container;
        public Image filledImage;
        public Image outlineImage;
    }

    [Header("玩家引用")]
    public PlayerHealth playerHealth; // 新增玩家健康引用
    [Header("Settings")]
    public int maxHealth = 6;          // 最大血量（以半心为单位）
    public int currentHealth = 6;      // 当前血量（以半心为单位）
    public Color healthColor = new Color(0.9f, 0.2f, 0.2f, 1f); // 健康时颜色
    public Color lowHealthColor = new Color(0.9f, 0.5f, 0.2f, 1f); // 低血量时颜色
    public float lowHealthThreshold = 0.3f; // 低血量阈值
    public float heartSpacing = 10f;   // 心形之间的间距

    [Header("References")]
    public GameObject heartPiecePrefab; // 半心形预制体
    public Transform heartsContainer;  // 心形容器

    private List<HeartPiece> heartPieces = new List<HeartPiece>();
    private bool initialized = false;

    void Start()
    {
        // 在Start方法开头添加
        if (playerHealth == null)
        {
            playerHealth = FindObjectOfType<PlayerHealth>();
        }
        
        // 立即初始化血条
        InitializeHealthBar();
        
        // 注册事件监听
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged += HandleHealthChanged;
            // 手动触发一次更新
            HandleHealthChanged(playerHealth.currentHealth, playerHealth.maxHealth);
        }

    }


    public void InitializeHealthBar()
    {
        if (initialized) return;

        // 添加玩家血量同步
        if (playerHealth != null)
        {
            maxHealth = playerHealth.maxHealth ;
            currentHealth = playerHealth.currentHealth ;
        }

        // 清除现有心形
        foreach (Transform child in heartsContainer)
        {
            Destroy(child.gameObject);
        }
        heartPieces.Clear();

        // 创建心形单元
        for (int i = 0; i < maxHealth; i++)
        {
            GameObject heartObj = Instantiate(heartPiecePrefab, heartsContainer);
            HeartPiece piece = new HeartPiece
            {
                container = heartObj.GetComponent<RectTransform>(),
                filledImage = heartObj.transform.Find("Filled").GetComponent<Image>(),
                outlineImage = heartObj.transform.Find("Outline").GetComponent<Image>()
            };

            // 设置心形方向（左右交替）
            bool isLeftPiece = (i % 2 == 0);
            piece.filledImage.transform.localScale = new Vector3(isLeftPiece ? 1 : -1, 1, 1);

            heartPieces.Add(piece);
            piece.outlineImage.gameObject.SetActive(true);
        }

        UpdateHealthDisplay();
        initialized = true;
    }

    public void SetHealth(int health)
    {
        currentHealth = Mathf.Clamp(health, 0, maxHealth);
        UpdateHealthDisplay();
    }

    public void SetMaxHealth(int max)
    {
        maxHealth = Mathf.Max(max, 1);
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        
        // 强制重新初始化血条
        initialized = false;
        InitializeHealthBar();
        
        Debug.Log($"最大血量更新: {maxHealth}");
    }

    private void UpdateHealthDisplay()
    {
        if (!initialized) return;

        // 计算完整心的数量和剩余半心
        int fullHearts = currentHealth / 2;
        int halfHeart = currentHealth % 2;
        int maxFullHearts = maxHealth / 2;
        // 获取预制件原始宽度（在编辑器中预设的值）
        float unitWidth = heartPiecePrefab.GetComponent<RectTransform>().rect.width;

        // 更新每个心形单元
        for (int i = 0; i < heartPieces.Count; i++)
        {
            HeartPiece piece = heartPieces[i];
            bool isLeftPiece = (i % 2 == 0);
            int heartIndex = i / 2;

            // 获取半心实际宽度（消除缩放影响）
            float heartWidth = piece.container.rect.width * Mathf.Abs(piece.filledImage.transform.localScale.x);

            // 修正位置计算（使用预制件标准宽度）
            float offset = heartIndex * (unitWidth *0.6f + heartSpacing);
            piece.container.anchoredPosition = new Vector2(offset, 0);
            // 设置填充状态
            bool shouldShowFilled = (heartIndex < fullHearts) || 
                                (heartIndex == fullHearts && isLeftPiece && halfHeart > 0);

            // 始终显示轮廓但透明化填充区域
            piece.outlineImage.gameObject.SetActive(true);
            piece.filledImage.gameObject.SetActive(shouldShowFilled);

            // 设置颜色
            piece.filledImage.color = healthColor;
        }
    }

    void OnDestroy()
    {
        // 移除事件监听
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged -= HandleHealthChanged;
        }
    }
    private void HandleHealthChanged(int current, int max)
    {
        // 先更新最大血量再设置当前血量
        SetMaxHealth(max);
        SetHealth(current);
        
        // 添加调试信息
        Debug.Log($"血量变化: {current}/{max}");
    }

}