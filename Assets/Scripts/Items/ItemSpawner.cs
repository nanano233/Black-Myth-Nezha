using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    public GameObject[] itemPrefabs; // 三个道具预制体
    
    public void SpawnRandomItem()
    {
        if (itemPrefabs.Length == 0) return;
        
        int index = Random.Range(0, itemPrefabs.Length);
        Vector3 spawnPos = transform.position + Vector3.up * 2f;
        Instantiate(itemPrefabs[index], spawnPos, Quaternion.identity);
    }
}