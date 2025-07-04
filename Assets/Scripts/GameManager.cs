using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    [Header("玩家属性")]
    public ProjectileProperties baseProjectileProperties;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 重置玩家属性
        var playerShooting = FindObjectOfType<PlayerShooting>();
        if (playerShooting != null && baseProjectileProperties != null)
        {
            playerShooting.UpdateProjectileProperties(baseProjectileProperties);
        }
    }

    public void HandlePlayerDeath()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}