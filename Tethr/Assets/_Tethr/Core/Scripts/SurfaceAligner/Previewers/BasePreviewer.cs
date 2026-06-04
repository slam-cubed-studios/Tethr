// Copyright (c) 2026, TheMGLegends. All rights reserved.

#if UNITY_EDITOR
using UnityEngine;

namespace Tethr.SurfaceAligner
{
    [ExecuteInEditMode]
    public abstract class BasePreviewer : MonoBehaviour
    {
        protected bool isActive = false;

        public bool IsActive() => isActive;

        public void Deactivate()
        {
            isActive = false;
            gameObject.SetActive(false);
        }
    }
}
#endif
