using System;
using Unity.Mathematics;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [SerializeField] private GameObject obstacleObj;
    [SerializeField] private int spawnCount = 1;
    [SerializeField] private float maxHorizontalSpawn;
    [SerializeField] private float maxVerticalSpawn;


    void Start()
    {
        for (int i = 0; i < spawnCount; i++)
        {
            float randX = UnityEngine.Random.Range(-maxHorizontalSpawn, maxHorizontalSpawn);
            float randY = UnityEngine.Random.Range(-maxVerticalSpawn, maxVerticalSpawn);
            Instantiate(obstacleObj, new Vector2(randX, randY), quaternion.identity);
        }
    }

}
