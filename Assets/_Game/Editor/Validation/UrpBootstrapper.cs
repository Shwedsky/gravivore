using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Gravivore.Editor
{
    public static class UrpBootstrapper
    {
        private const string UniversalPipelineAssetTypeName =
            "UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset, Unity.RenderPipelines.Universal.Runtime";

        private const string UrpAssetPath = "Assets/_Game/Content/Settings/Gravivore_URP.asset";

        public static void EnsureUrpConfigured()
        {
            var pipelineAsset = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>(UrpAssetPath);
            if (pipelineAsset == null)
            {
                pipelineAsset = CreateUrpAsset();
            }

            GraphicsSettings.defaultRenderPipeline = pipelineAsset;
            QualitySettings.renderPipeline = pipelineAsset;
            AssetDatabase.SaveAssets();
        }

        private static RenderPipelineAsset CreateUrpAsset()
        {
            var pipelineAssetType = Type.GetType(UniversalPipelineAssetTypeName);
            if (pipelineAssetType == null)
            {
                throw new InvalidOperationException("Universal Render Pipeline package is not available. Check Packages/manifest.json.");
            }

            Directory.CreateDirectory(Path.GetDirectoryName(UrpAssetPath) ?? "Assets/_Game/Content/Settings");

            var asset = ScriptableObject.CreateInstance(pipelineAssetType) as RenderPipelineAsset;
            if (asset == null)
            {
                throw new InvalidOperationException("Failed to create a Universal Render Pipeline asset.");
            }

            AssetDatabase.CreateAsset(asset, UrpAssetPath);
            AssetDatabase.ImportAsset(UrpAssetPath);
            return asset;
        }
    }
}
