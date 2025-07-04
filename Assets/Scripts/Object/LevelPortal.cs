using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelPortal : MonoBehaviour
{
    [SerializeField] private float loadDelay = 1f;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // 增加层数
            Room.currentFloor++;
            // 重新加载场景
            StartCoroutine(LoadNextLevel());
        }
    }

    private IEnumerator LoadNextLevel()
    {
        PlayerControl.Instance.SavePlayerData();
        
        // 禁用玩家碰撞体和渲染，而不是整个对象
        var player = PlayerControl.Instance;
        player.GetComponent<Collider2D>().enabled = false;
        player.GetComponent<SpriteRenderer>().enabled = false;
        player.enabled = false; // 禁用脚本

        yield return new WaitForSeconds(loadDelay);
        
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
    }
    // 新增场景加载完成后的初始化
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 重置对象池（关键修复）
        ResetFlamePool();
        
        // 启动房间初始化协程
        StartCoroutine(InitializeAfterRoomGeneration());
    }


    private IEnumerator InitializeAfterRoomGeneration()
    {
        // 确保房间生成完成且当前房间有效
        yield return new WaitUntil(() => 
            RoomManager.Instance != null && 
            RoomManager.Instance.generationComplete && 
            RoomManager.Instance.currentRoom != null);
        
        // 确保玩家组件存在
        if (PlayerControl.Instance == null)
        {
            Debug.LogError("PlayerControl 实例丢失！");
            yield break;
        }

        // 获取玩家并重置状态
        PlayerControl player = PlayerControl.Instance;
        player.enabled = true;
        
        // 重置玩家输入状态（关键修复）
        player.ResetInputState();
        
        // 在起始房间中心生成玩家
        Vector3 spawnPos = RoomManager.Instance.currentRoom.transform.position;
        player.TeleportTo(spawnPos);
        
        // 启用玩家组件
        player.GetComponent<Collider2D>().enabled = true;
        player.GetComponent<SpriteRenderer>().enabled = true;
        
        Debug.Log($"玩家初始化在房间: {RoomManager.Instance.currentRoom.name}, 位置: {spawnPos}");
    }
    
    
    // 新增重置对象池方法
    private void ResetFlamePool()
    {
        if (FlamePool.Instance != null)
        {
            // 销毁所有现有子弹
            foreach (var flame in FlamePool.Instance.GetAllFlames().ToArray())
            {
                if (flame != null) Destroy(flame.gameObject);
            }

            // 重新初始化对象池
            FlamePool.Instance.InitializePool();
            Debug.Log("子弹对象池已重置");
        }
    }


    private IEnumerator DelayedPlayerActivation()
    {
        // 确保完全加载场景
        yield return new WaitUntil(() => SceneManager.GetActiveScene().isLoaded);

        // 先激活对象再初始化
        PlayerControl.Instance.gameObject.SetActive(true);
        PlayerControl.Instance.Initialize();

        // 使用双重传送确保位置同步
        PlayerControl.Instance.TeleportTo(Vector3.zero);
        PlayerControl.Instance.transform.position = Vector3.zero;

        // 强制刷新物理系统
        if (PlayerControl.Instance.rb != null)
        {
            PlayerControl.Instance.rb.velocity = Vector2.zero;
            PlayerControl.Instance.rb.MovePosition(Vector3.zero);
            PlayerControl.Instance.rb.Sleep();
            PlayerControl.Instance.rb.WakeUp();
        }

        // 新增渲染器强制启用
        var renderer = PlayerControl.Instance.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.enabled = true;
            renderer.forceRenderingOff = false;
        }
    }


}