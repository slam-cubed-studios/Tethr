// Copyright (c) 2026, TheMGLegends. All rights reserved.

using System;

namespace Tethr.EntitySpawner
{
    /// <summary>
    /// Interface for entities that are spawned by the <see cref="BaseEntitySpawnArea"/>. Users should implement this interface on 
    /// any entity prefabs that are spawned by the spawn area and broadcast the OnDespawned event to ensure the spawn area can track
    /// how many entities are currently active.
    /// </summary>
    public interface ISpawnedEntity
    {
        public event Action<ISpawnedEntity> OnDespawned;
    }
}
