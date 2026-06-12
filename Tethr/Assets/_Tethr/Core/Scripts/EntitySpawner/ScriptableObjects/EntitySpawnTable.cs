// Copyright (c) 2026, TheMGLegends. All rights reserved.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tethr.EntitySpawner
{
    /// <summary>
    /// Represents configuration data for spawning a specific entity, including the prefab to spawn and its associated 
    /// spawn chance.
    /// </summary>
    [Serializable]
    public class EntitySpawnData
    {
        public GameObject Prefab;
        [Range(0, 100)] public int SpawnChance;
    }

    /// <summary>
    /// Represents a spawn table that contains a list of entities to spawn, each with its own prefab and spawn chance.
    /// </summary>
    [CreateAssetMenu(fileName = "NewEntitySpawnTable", menuName = "Tethr/Entity Spawn Table")]
    public class EntitySpawnTable : ScriptableObject
    {
        public List<EntitySpawnData> entities;

        public GameObject GetRandomEntityPrefab()
        {
            if (entities == null || entities.Count == 0)
            {
                return null;
            }

            int totalSpawnChance = GetTotalSpawnChance();
            if (totalSpawnChance <= 0)
            {
                Message.LogWarning($"Total spawn chance is zero or negative in {name}. No entities will be spawned.");
                return null;
            }

            int randomValue = UnityEngine.Random.Range(0, totalSpawnChance);

            int cumulativeChance = 0;
            foreach (EntitySpawnData data in entities)
            {
                cumulativeChance += data.SpawnChance;
                if (randomValue < cumulativeChance)
                {
                    return data.Prefab;
                }
            }

            return null;
        }

        private int GetTotalSpawnChance()
        {
            int totalSpawnChance = 0;
            foreach (EntitySpawnData data in entities)
            {
                totalSpawnChance += data.SpawnChance;
            }

            return totalSpawnChance;
        }
    }
}

