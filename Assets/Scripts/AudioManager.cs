using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    
    [Header("音效配置")]
    public AudioClip shootSFX;
    private AudioSource audioSource;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlaySFX(string sfxName)
    {
        // 根据项目需要可扩展为使用音效名称匹配资源
        if (shootSFX != null && sfxName == "Shoot")
        {
            audioSource.PlayOneShot(shootSFX);
        }
    }
}
