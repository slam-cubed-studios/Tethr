// Copyright (c) 2026, TheMGLegends. All rights reserved.

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

namespace Tethr.EntitySpawner
{
    /// <summary>
    /// 3D version of the <see cref="BaseEntitySpawnArea"/>. It cotains 3D-specific implementations for getting 
    /// random positions within the spawn area and drawing the spawn area gizmos.
    /// </summary>
    public class EntitySpawnArea : BaseEntitySpawnArea
    {
        internal override Vector3 GetRandomPositionInArea()
        {
            return spawnAreaShape switch
            {
                SpawnAreaShape.Box => new Vector3(Random.Range(-size.x / 2, size.x / 2),
                                                  Random.Range(-size.y / 2, size.y / 2),
                                                  Random.Range(-size.z / 2, size.z / 2)),
                SpawnAreaShape.Sphere => Random.insideUnitSphere * radius,
                _ => Vector3.zero
            };
        }

        internal override void DrawSpawnAreaGizmos()
        {
#if UNITY_EDITOR
            Color previousHandlesColour = Handles.color;
            Color previousGizmosColour = Gizmos.color;
            Color alphaColour = new(spawnAreaColour.r, spawnAreaColour.g, spawnAreaColour.b, alpha);

            Vector3 transformPosition = transform.position;
            switch (spawnAreaShape)
            {
                case SpawnAreaShape.Box:
                    Gizmos.color = spawnAreaColour;
                    Gizmos.DrawWireCube(transformPosition, size);

                    Gizmos.color = alphaColour;
                    Gizmos.DrawCube(transformPosition, size);
                    break;
                case SpawnAreaShape.Sphere:
                    Gizmos.color = spawnAreaColour;
                    Gizmos.DrawWireSphere(transformPosition, radius);

                    Handles.color = alphaColour;
                    Handles.SphereHandleCap(0, transformPosition, Quaternion.identity, radius * 2, EventType.Repaint);
                    break;
            }

            Handles.color = previousHandlesColour;
            Gizmos.color = previousGizmosColour;
#endif
        }
    }
}
