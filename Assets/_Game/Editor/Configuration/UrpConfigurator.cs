using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Gravivore.Editor
{
    public static class UrpConfigurator
    {
        public const string UrpAssetPath = "Assets/_Game/Content/Settings/Gravivore_URP.asset";
        public const string RendererDataPath = "Assets/_Game/Content/Settings/Gravivore_URP_Renderer.asset";

        public static void ConfigureUrp()
        {
            var pipelineAsset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(UrpAssetPath);
            var rendererData = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(RendererDataPath);

            if (pipelineAsset == null && rendererData == null)
            {
                CreateUrpAssets(out pipelineAsset, out rendererData);
            }
            else if (pipelineAsset == null || rendererData == null)
            {
                throw new InvalidDataException(
                    $"URP configuration is incomplete. Expected both {UrpAssetPath} and {RendererDataPath}.");
            }

            if (!ReferencesRenderer(pipelineAsset, rendererData))
            {
                throw new InvalidDataException(
                    $"{UrpAssetPath} does not reference the canonical renderer data at {RendererDataPath}.");
            }

            GraphicsSettings.defaultRenderPipeline = pipelineAsset;
            QualitySettings.renderPipeline = pipelineAsset;
            AssetDatabase.SaveAssets();
        }

        public static bool ReferencesRenderer(
            UniversalRenderPipelineAsset pipelineAsset,
            UniversalRendererData rendererData)
        {
            if (pipelineAsset == null || rendererData == null)
            {
                return false;
            }

            var rendererDataList = pipelineAsset.rendererDataList;
            return rendererDataList.Length > 0 && rendererDataList[0] == rendererData;
        }

        private static void CreateUrpAssets(
            out UniversalRenderPipelineAsset pipelineAsset,
            out UniversalRendererData rendererData)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(UrpAssetPath) ?? "Assets/_Game/Content/Settings");

            rendererData = ScriptableObject.CreateInstance<UniversalRendererData>();
            rendererData.name = "Gravivore_URP_Renderer";
            AssetDatabase.CreateAsset(rendererData, RendererDataPath);

            pipelineAsset = UniversalRenderPipelineAsset.Create(rendererData);
            if (pipelineAsset == null)
            {
                AssetDatabase.DeleteAsset(RendererDataPath);
                throw new InvalidDataException("URP failed to create a pipeline asset with renderer data.");
            }

            pipelineAsset.name = "Gravivore_URP";
            AssetDatabase.CreateAsset(pipelineAsset, UrpAssetPath);
        }
    }
}
