// Copyright (c) 2026, TheMGLegends. All rights reserved.

using EditorAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tethr.EntitySpawner
{
    /// <summary>
    /// Specifies the respawn behaviour type.
    /// </summary>
    public enum RespawnMode
    {
        None,
        AfterDelay,
        WhenEmpty
    }

    /// <summary>
    /// Specifies the spawn area shape type.
    /// </summary>
    public enum SpawnAreaShape
    {
        Box,
        Sphere
    }

    public abstract class BaseEntitySpawnArea : MonoBehaviour
    {
        /// <summary>
        /// Specifies the type of activation that triggers the spawn area to spawn entities.
        /// </summary>
        private enum ActivationType
        {
            Awake,
            Trigger,
            Manual
        }

        [Header("Debug Settings")]
        [SerializeField] private DebugDrawType debugDrawType = DebugDrawType.WhenSelected;
        [SerializeField] protected Color spawnAreaColour = Color.cyan;
        [SerializeField, Range(0.0f, 1.0f)] protected float alpha = 0.1f;

        [Header("Entity Spawn Table")]
        [SerializeField] private EntitySpawnTable spawnTable;

        [Header("Entity Spawn Rules")]
        [SerializeField] private ActivationType activationType = ActivationType.Awake;
        [SerializeField, Min(0)] private int maxEntities = 10;
        [SerializeField, Min(0)] private int spawnAmount = 5;
        [SerializeField, Min(0.0f)] private float spawnInterval = 1.0f;

        [Space(10.0f)]

        [SerializeField] protected SpawnAreaShape spawnAreaShape = SpawnAreaShape.Box;

        [ShowField(nameof(spawnAreaShape), SpawnAreaShape.Box)]
        [SerializeField, Min(0.0f)] protected Vector3 size = Vector3.one;

        [ShowField(nameof(spawnAreaShape), SpawnAreaShape.Sphere)]
        [SerializeField, Min(0.0f)] protected float radius = 1.0f;

        [Space(10.0f)]

        [SerializeField] private RespawnMode respawnMode = RespawnMode.None;

        [ShowField(nameof(respawnMode), RespawnMode.AfterDelay)]
        [SerializeField, Min(0.0f)] private float respawnDelay = 5.0f;

        private bool isSpawning = false;
        private Coroutine delayedSpawnCoroutine;
        private readonly HashSet<GameObject> spawnedEntities = new();

        private void OnValidate()
        {
            spawnAmount = Mathf.Clamp(spawnAmount, 0, maxEntities);
        }

        private void OnDisable()
        {
            foreach (GameObject entity in spawnedEntities)
            {
                if (entity != null)
                {
                    if (entity.TryGetComponent(out ISpawnedEntity spawnedEntity))
                    {
                        spawnedEntity.OnDespawned -= OnEntityDespawned;
                    }
                }
            }

            if (delayedSpawnCoroutine != null)
            {
                StopCoroutine(delayedSpawnCoroutine);
                delayedSpawnCoroutine = null;
            }

            CancelInvoke(nameof(Spawn));
        }

        private void Start()
        {
            if (activationType == ActivationType.Awake)
            {
                Spawn();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (activationType == ActivationType.Trigger && !isSpawning)
            {
                Spawn();
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (activationType == ActivationType.Trigger && !isSpawning)
            {
                Spawn();
            }
        }

        private void OnDrawGizmos()
        {
            if (debugDrawType != DebugDrawType.Always)
            {
                return;
            }

            DrawSpawnAreaGizmos();
        }

        private void OnDrawGizmosSelected()
        {
            if (debugDrawType != DebugDrawType.WhenSelected)
            {
                return;
            }

            DrawSpawnAreaGizmos();
        }

        /// <summary>
        /// Spawns entities based on the assigned EntitySpawnTable and spawn rules.
        /// </summary>
        public void Spawn()
        {
            if (spawnTable == null)
            {
                Message.LogWarning("No EntitySpawnTable assigned to EntitySpawnArea. Cannot spawn entities.");
                return;
            }

            if (spawnInterval > 0.0f)
            {
                if (isSpawning)
                {
                    return;
                }

                delayedSpawnCoroutine = StartCoroutine(SpawnDelayed());
                return;
            }

            Vector3 transformPosition = transform.position;
            for (int i = 0; i < spawnAmount; i++)
            {
                if (spawnedEntities.Count >= maxEntities)
                {
                    Message.Log("Reached max entity limit. Stopping spawn.");
                    break;
                }

                Vector3 spawnPosition = transformPosition + GetRandomPositionInArea();
                InstantiateEntity(spawnPosition);
            }

            if (respawnMode == RespawnMode.AfterDelay)
            {
                CancelInvoke(nameof(Spawn));
                Invoke(nameof(Spawn), respawnDelay);
            }
        }

        private IEnumerator SpawnDelayed()
        {
            isSpawning = true;
            Vector3 transformPosition = transform.position;
            WaitForSeconds delay = new(spawnInterval);
            for (int i = 0; i < spawnAmount; i++)
            {
                if (spawnedEntities.Count >= maxEntities)
                {
                    Message.Log("Reached max entity limit. Stopping spawn.");
                    break;
                }

                Vector3 spawnPosition = transformPosition + GetRandomPositionInArea();
                InstantiateEntity(spawnPosition);

                yield return delay;
            }

            isSpawning = false;
            delayedSpawnCoroutine = null;

            if (respawnMode == RespawnMode.AfterDelay)
            {
                CancelInvoke(nameof(Spawn));
                Invoke(nameof(Spawn), respawnDelay);
            }
        }

        internal abstract Vector3 GetRandomPositionInArea();

        internal abstract void DrawSpawnAreaGizmos();

        private void InstantiateEntity(Vector3 spawnPosition)
        {
            if (spawnTable == null)
            {
                return;
            }

            GameObject prefab = spawnTable.GetRandomEntityPrefab();
            if (prefab == null)
            {
                Message.LogWarning("EntitySpawnTable returned a null prefab. Cannot instantiate entity.");
                return;
            }

            GameObject gameObject = Instantiate(prefab, spawnPosition, Quaternion.identity);
            if (gameObject != null)
            {
                spawnedEntities.Add(gameObject);
                if (gameObject.TryGetComponent(out ISpawnedEntity spawnedEntity))
                {
                    spawnedEntity.OnDespawned += OnEntityDespawned;
                }
            }
        }

        private void OnEntityDespawned(ISpawnedEntity spawnedEntity)
        {
            if (spawnedEntity == null || spawnedEntity is not MonoBehaviour mono)
            {
                return;
            }

            GameObject gameObject = mono.gameObject;
            if (gameObject != null)
            {
                spawnedEntity.OnDespawned -= OnEntityDespawned;
                spawnedEntities.Remove(gameObject);
            }

            if (respawnMode == RespawnMode.WhenEmpty && spawnedEntities.Count <= 0)
            {
                Spawn();
            }
        }
    }
}

