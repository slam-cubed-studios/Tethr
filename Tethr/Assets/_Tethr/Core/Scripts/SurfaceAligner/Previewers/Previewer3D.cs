// Copyright (c) 2026, TheMGLegends. All rights reserved.

#if UNITY_EDITOR
using UnityEngine;

namespace Tethr.SurfaceAligner
{
    /// <summary>
    /// 3D version of the <see cref="BasePreviewer"/>. It uses a mesh filter & renderer to render a preview of how the object will be 
    /// aligned to the surface.
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class Previewer3D : BasePreviewer
    {
        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;

        private void Awake()
        {
            meshFilter = GetComponent<MeshFilter>();
            meshRenderer = GetComponent<MeshRenderer>();
            gameObject.hideFlags = HideFlags.HideAndDontSave;
            gameObject.SetActive(false);
        }

        public void Activate(Vector3 hitNormal, Vector3 position, Vector3 scale, Mesh mesh, MeshRenderer renderer, Material material)
        {
            if (mesh == null || renderer == null || material == null)
            {
                return;
            }

            transform.up = hitNormal;
            transform.localScale = scale;
            transform.position = position;
            meshFilter.mesh = mesh;
            meshRenderer.material = material;
            gameObject.SetActive(true);
            isActive = true;
        }
    }
}
#endif
