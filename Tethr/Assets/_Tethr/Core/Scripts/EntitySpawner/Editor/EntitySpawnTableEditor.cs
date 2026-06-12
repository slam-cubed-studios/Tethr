// Copyright (c) 2026, TheMGLegends. All rights reserved.

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Tethr.EntitySpawner
{
    /// <summary>
    /// Custom editor for the <see cref="EntitySpawnTable"/> ScriptableObject. It provides a custom inspector GUI that
    /// clamps the spawn chances of the entities in the spawn table to ensure that their total does not exceed 100%.
    /// </summary>
    [CustomEditor(typeof(EntitySpawnTable))]
    public class EntitySpawnTableEditor : Editor
    {
        private SerializedProperty entitiesProperty;

        private readonly List<int> previousSpawnChances = new();

        private void OnEnable()
        {
            entitiesProperty = serializedObject.FindProperty(nameof(EntitySpawnTable.entities));
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(entitiesProperty, true);

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                ClampSpawnChances();
            }
        }

        private void ClampSpawnChances()
        {
            EntitySpawnTable table = (EntitySpawnTable)target;
            if (table == null)
            {
                return;
            }

            List<EntitySpawnData> entities = table.entities;
            if (!AreListsEqual(entities))
            {
                RebuildPreviousSpawnChances(entities);
                return;
            }

            int remainingSpawnChance = CalculateRemainingSpawnChance(entities, out int changingIndex);
            if (IsIndexValid(changingIndex, entities.Count))
            {
                int newSpawnChance = entities[changingIndex].SpawnChance;
                if (IsSpawnChanceIncreasing(newSpawnChance, previousSpawnChances[changingIndex]))
                {
                    entities[changingIndex].SpawnChance = Mathf.Clamp(newSpawnChance, 0, remainingSpawnChance);
                }
            }

            UpdatePreviousSpawnChances(entities);
        }

        private bool AreListsEqual(List<EntitySpawnData> entities)
        {
            if (previousSpawnChances.Count != entities.Count)
            {
                // INFO: If the entities list has increased in size, we need to set the default spawn chance to 0
                //       for the new entity to prevent issues with spawn chance clamping
                if (Mathf.Sign(entities.Count - previousSpawnChances.Count) > 0.0f)
                {
                    entities[^1].SpawnChance = 0;
                }

                return false;
            }

            return true;
        }

        private void RebuildPreviousSpawnChances(List<EntitySpawnData> entities)
        {
            previousSpawnChances.Clear();
            foreach (EntitySpawnData entity in entities)
            {
                previousSpawnChances.Add(entity.SpawnChance);
            }
        }

        private int CalculateRemainingSpawnChance(List<EntitySpawnData> entities, out int changingIndex)
        {
            changingIndex = -1;
            int remainingSpawnChance = 100;
            for (int i = 0; i < entities.Count; i++)
            {
                if (previousSpawnChances[i] != entities[i].SpawnChance)
                {
                    changingIndex = i;
                    continue;
                }

                remainingSpawnChance -= entities[i].SpawnChance;
            }

            return remainingSpawnChance;
        }

        private bool IsIndexValid(int index, int count)
        {
            return index >= 0 && index < count;
        }

        private bool IsSpawnChanceIncreasing(float newSpawnChance, float previousSpawnChance)
        {
            return Mathf.Sign(newSpawnChance - previousSpawnChance) > 0.0f;
        }

        private void UpdatePreviousSpawnChances(List<EntitySpawnData> entities)
        {
            for (int i = 0; i < entities.Count; i++)
            {
                previousSpawnChances[i] = entities[i].SpawnChance;
            }
        }
    }
}
