using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace GameEngine.Utils
{
    public class FollowGhostEffect : MonoBehaviour
    {
        [Header("是否激活残影特效")] public bool active = false;

        [Header("生成残影的时间间隔")] public float interval = 0.2f;

        [Header("残影生成后的初始alpha")] public float alpha = 1;

        [Header("每一个残影的存活时间")] public float aliveTime = 1;

        public float timeScale = 1;

        private float _time = 0;

        private SkinnedMeshRenderer[] _skinnedRenderers;
        private MeshRenderer[] _meshRenderers;
        private static readonly int SrcBlend = Shader.PropertyToID("_SrcBlend");
        private static readonly int DstBlend = Shader.PropertyToID("_DstBlend");
        private static readonly int ZWrite = Shader.PropertyToID("_ZWrite");
        private static readonly int Color = Shader.PropertyToID("_BaseColor");

        private readonly List<FollowGhost> _ghosts = new List<FollowGhost>();
        private static readonly int Blend = Shader.PropertyToID("_Blend");
        private static readonly int Surface = Shader.PropertyToID("_Surface");

        private void Awake()
        {
            _skinnedRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
            _meshRenderers = GetComponentsInChildren<MeshRenderer>();
        }

        private float DeltaTime => Time.deltaTime * timeScale;
        private float Interval => interval * timeScale;
        private float AliveTime => aliveTime * timeScale;

        private void Update()
        {
            var dt = DeltaTime;

            if (active)
            {
                if (_skinnedRenderers == null || _skinnedRenderers.Length == 0)
                {
                    active = false;
                    return;
                }


                _time += dt;

                if (_time > Interval)
                {
                    _time = 0;
                    MakeFollowGhost();
                }
            }

            for (var i = _ghosts.Count - 1; i >= 0; i--)
            {
                var g = _ghosts[i];
                if (g.IncAliveTime(dt))
                {
                    //超过存活时间，清理并销毁
                    _ghosts.Remove(g);
                    Destroy(g);
                    continue;
                }

                var m = g.Material;
                if (m.HasProperty("_Color"))
                {
                    var c = m.GetColor(Color);
                    c.a = g.Alpha;
                    m.SetColor(Color, c);
                }

                Graphics.DrawMesh(g.Mesh, g.Matrix, g.Material, gameObject.layer);
            }
        }

        void MakeFollowGhost()
        {
            foreach (var r in _skinnedRenderers)
            {
                var mesh = new Mesh();
                r.BakeMesh(mesh, true);

                var material = new Material(r.material);
                SetMaterialRenderingModeURP(material, RenderingModeUrp.Transparent);

                var fg = new FollowGhost(mesh, material, r.transform.localToWorldMatrix, alpha, AliveTime);
                _ghosts.Add(fg);
            }

            foreach (var r in _meshRenderers)
            {
                var mesh = r.GetComponent<MeshFilter>().mesh;

                var material = new Material(r.material);
                SetMaterialRenderingModeURP(material, RenderingModeUrp.Transparent);

                var fg = new FollowGhost(mesh, material, r.transform.localToWorldMatrix, alpha, AliveTime);
                _ghosts.Add(fg);
            }
        }


        /// <summary>
        /// 设置纹理渲染模式，URP模式下
        /// </summary>
        /// <param name="material"></param>
        /// <param name="renderingMode"></param>
        void SetMaterialRenderingModeURP(Material material, RenderingModeUrp renderingMode)
        {
            if (renderingMode == RenderingModeUrp.Opaque)
            {
                material.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");

                material.SetOverrideTag("RenderType", "Opaque");

                material.SetInt(ZWrite, 1);
                material.SetInt(SrcBlend, (int)BlendMode.One);
                material.SetInt(DstBlend, (int)BlendMode.Zero);

                material.SetFloat(Blend, 0);
                material.SetFloat(Surface, 0);

                material.renderQueue = (int)RenderQueue.Geometry;
            }
            else
            {
                material.DisableKeyword("_ALPHAMODULATE_ON");

                material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                material.EnableKeyword("_ALPHAPREMULTIPLY_ON");

                material.SetOverrideTag("RenderType", "Transparent");

                material.SetInt(ZWrite, 0);
                material.SetInt(SrcBlend, (int)BlendMode.One);
                material.SetInt(DstBlend, (int)BlendMode.OneMinusSrcAlpha);

                material.SetFloat(Blend, 1);
                material.SetFloat(Surface, 1);

                material.renderQueue = (int)RenderQueue.Transparent;
            }
        }

        /// <summary>
        /// 设置纹理渲染模式, 旧管线模式下
        /// </summary>
        void SetMaterialRenderingModeOld(Material material, RenderingMode renderingMode)
        {
            switch (renderingMode)
            {
                case RenderingMode.Opaque:
                    material.SetInt(SrcBlend, (int)BlendMode.One);
                    material.SetInt(DstBlend, (int)BlendMode.Zero);
                    material.SetInt(ZWrite, 1);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.renderQueue = -1;
                    break;
                case RenderingMode.Cutout:
                    material.SetInt(SrcBlend, (int)BlendMode.One);
                    material.SetInt(DstBlend, (int)BlendMode.Zero);
                    material.SetInt(ZWrite, 1);
                    material.EnableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.renderQueue = 2450;
                    break;
                case RenderingMode.Fade:
                    material.SetInt(SrcBlend, (int)BlendMode.SrcAlpha);
                    material.SetInt(DstBlend, (int)BlendMode.OneMinusSrcAlpha);
                    material.SetInt(ZWrite, 0);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.EnableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.renderQueue = 3000;
                    break;
                case RenderingMode.Transparent:
                    material.SetInt(SrcBlend, (int)BlendMode.One);
                    material.SetInt(DstBlend, (int)BlendMode.OneMinusSrcAlpha);
                    material.SetInt(ZWrite, 0);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.renderQueue = 3000;
                    break;
            }
        }
    }

    enum RenderingModeUrp
    {
        Opaque,
        Transparent
    }

    enum RenderingMode
    {
        Opaque,
        Cutout,
        Fade,
        Transparent,
    }

    class FollowGhost : Object
    {
        //残影的Mesh
        public Mesh Mesh { get; }

        //残影的材质
        public Material Material { get; }

        //残影的位置矩阵
        public Matrix4x4 Matrix { get; }

        private float _alpha;

        //残影透明度
        public float Alpha => _alpha * (1 - AliveTime / Duration);

        public float AliveTime { get; private set; } = 0;

        public float Duration { get; }

        public FollowGhost(Mesh mesh, Material material, Matrix4x4 matrix, float alpha, float duration)
        {
            Material = material;
            Mesh = mesh;
            Matrix = matrix;
            _alpha = alpha;
            Duration = duration;
        }

        public bool IncAliveTime(float time)
        {
            AliveTime += time;
            return AliveTime > Duration;
        }
    }
}