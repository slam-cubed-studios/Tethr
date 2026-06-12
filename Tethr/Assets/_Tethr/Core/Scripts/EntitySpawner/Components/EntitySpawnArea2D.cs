// Copyright (c) 2026, TheMGLegends. All rights reserved.

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

namespace Tethr.EntitySpawner
{
    /// <summary>
    /// 2D version of the <see cref="BaseEntitySpawnArea"/>. It cotains 2D-specific implementations for getting 
    /// random positions within the spawn area and drawing the spawn area gizmos.
    /// </summary>
    public class EntitySpawnArea2D : BaseEntitySpawnArea
    {
        internal override Vector3 GetRandomPositionInArea()
        {
            return spawnAreaShape switch
            {
                SpawnAreaShape.Box => new Vector2(Random.Range(-size.x / 2, size.x / 2),
                                                  Random.Range(-size.y / 2, size.y / 2)),
                SpawnAreaShape.Sphere => Random.insideUnitCircle * radius,
                _ => Vector2.zero
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
                    Vector3 modifiedSize = new(size.x, size.y, 0f);

                    Handles.color = spawnAreaColour;
                    Handles.DrawWireCube(transformPosition, modifiedSize);

                    Gizmos.color = alphaColour;
                    Gizmos.DrawCube(transformPosition, modifiedSize);
                    break;
                case SpawnAreaShape.Sphere:
                    Handles.color = spawnAreaColour;
                    Handles.DrawWireDisc(transformPosition, Vector3.forward, radius, 2.0f);

                    Handles.color = alphaColour;
                    Handles.DrawSolidDisc(transformPosition, Vector3.forward, radius);
                    break;
            }

            Handles.color = previousHandlesColour;
            Gizmos.color = previousGizmosColour;
#endif
        }
    }
}
