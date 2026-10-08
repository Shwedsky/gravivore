#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Gravivore.Presentation.Development
{
    /// <summary>Once per chapter composition, never a per-frame shader scan.</summary>
    public sealed class RenderingBootDiagnostics : MonoBehaviour
    {
        public bool HasLogged { get; private set; }
        public int MaterialCount { get; private set; }
        public void Initialize(Transform chapter, UnityEngine.Camera camera)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (HasLogged) return;
            HasLogged = true;
            var pipeline = GraphicsSettings.currentRenderPipeline;
            Debug.Log($"RENDER_BOOT pipeline={pipeline?.name ?? "null"}; api={SystemInfo.graphicsDeviceType}; device={SystemInfo.graphicsDeviceName}; version={SystemInfo.graphicsDeviceVersion}; colorSpace={QualitySettings.activeColorSpace}; cameraHDR={camera.allowHDR}; HDRFormat={SystemInfo.GetGraphicsFormat(UnityEngine.Experimental.Rendering.DefaultFormat.HDR)}");
            var materials = new HashSet<Material>();
            foreach (var renderer in chapter.GetComponentsInChildren<Renderer>(true))
                foreach (var material in renderer.sharedMaterials) Record(material, renderer.name, materials);
            foreach (var graphic in chapter.GetComponentsInChildren<Graphic>(true))
                Record(graphic.material, graphic.name, materials);
            Record(Graphic.defaultGraphicMaterial, "runtime uGUI default", materials);
            MaterialCount = materials.Count;
#endif
        }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private static void Record(Material material, string owner, HashSet<Material> seen)
        {
            if (material == null) { Debug.LogError($"MAGENTA_GUARD missing material on {owner}"); return; }
            if (!seen.Add(material)) return;
            var shader = material.shader;
            var supported = shader != null && shader.name != "Hidden/InternalErrorShader" && shader.isSupported;
            var detail = $"material={material.name}; shader={shader?.name ?? "null"}; supported={supported}; queue={material.renderQueue}; keywords=[{string.Join(",", material.shaderKeywords)}]; owner={owner}";
            if (!supported && SystemInfo.graphicsDeviceType != GraphicsDeviceType.Null) Debug.LogError("MAGENTA_GUARD " + detail);
            else Debug.Log("RENDER_BOOT " + detail);
        }
#endif
    }
}
#endif
