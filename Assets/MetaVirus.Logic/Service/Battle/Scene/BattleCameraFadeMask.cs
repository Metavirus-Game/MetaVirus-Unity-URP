using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;

namespace MetaVirus.Logic.Service.Battle.Scene
{
    public class BattleCameraFadeMask : MonoBehaviour
    {
        public Renderer fadeMaskRenderer;
        public float fadeDuration = 0.3f;

        private Tweener _tweener;
        private Material _material;
        private static readonly int Alpha = Shader.PropertyToID("_Alpha");

        private void Start()
        {
            if (fadeMaskRenderer == null)
            {
                fadeMaskRenderer = GetComponentInChildren<MeshRenderer>();
            }

            _material = new Material(fadeMaskRenderer.material);
            fadeMaskRenderer.material = _material;

            fadeMaskRenderer.enabled = false;
        }

        public void FadeTo(float alpha, float from = 0)
        {
            if (_tweener != null)
            {
                _tweener.Kill(true);
            }


            fadeMaskRenderer.enabled = true;
            _tweener = DOTween.To(value => { _material.SetFloat(Alpha, value); }, from, alpha, fadeDuration)
                .OnComplete(() =>
                {
                    if (alpha == 0)
                    {
                        fadeMaskRenderer.enabled = false;
                    }

                    _tweener = null;
                });
        }

        public void Hide()
        {
            var currAlpha = _material.GetFloat(Alpha);
            FadeTo(0, currAlpha);
        }

        public void HideImmediately()
        {
            _tweener?.Kill();
            _material.SetFloat(Alpha, 0);
            fadeMaskRenderer.enabled = false;
        }
    }
}