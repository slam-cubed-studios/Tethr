// Copyright (c) 2026, TheMGLegends. All rights reserved.

#if UNITY_EDITOR
using UnityEngine;

namespace Tethr.SurfaceAligner
{
    /// <summary>
    /// 2D version of the <see cref="BasePreviewer"/>. It uses a sprite renderer to render a preview of how the object will be
    /// aligned to the surface.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class Previewer2D : BasePreviewer
    {
        private SpriteRenderer spriteRenderer;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            gameObject.hideFlags = HideFlags.HideAndDontSave;
            gameObject.SetActive(false);
        }

        public void Activate(Vector3 hitNormal, Vector3 position, Vector3 scale, float alpha, SpriteRenderer renderer)
        {
            transform.up = hitNormal;
            transform.localScale = scale;
            transform.position = position;
            SetSpriteRendererProperties(renderer, new Color(renderer.color.r, renderer.color.g, renderer.color.b, alpha));

            gameObject.SetActive(true);
            isActive = true;

        }

        private void SetSpriteRendererProperties(SpriteRenderer renderer, Color colour)
        {
            if (renderer == null)
            {
                return;
            }

            spriteRenderer.sprite = renderer.sprite;
            spriteRenderer.color = colour;
            spriteRenderer.flipX = renderer.flipX;
            spriteRenderer.flipY = renderer.flipY;
            spriteRenderer.drawMode = renderer.drawMode;
            spriteRenderer.sortingOrder = renderer.sortingOrder;
        }
    }
}
#endif
