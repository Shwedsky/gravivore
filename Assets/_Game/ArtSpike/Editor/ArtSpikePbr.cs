using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Gravivore.ArtSpike.Editor
{
    public static class ArtSpikePbr
    {
        public const string Source = ArtSpikeBuilder.Root + "/Imported/PolyHaven/BlueMetalPlate";
        public const string Derived = ArtSpikeBuilder.Root + "/Proxy/Textures";
        public const string Diffuse = Source + "/blue_metal_plate_diff_1k.jpg";
        public const string Normal = Source + "/blue_metal_plate_nor_gl_1k.png";
        public const string Arm = Source + "/blue_metal_plate_arm_1k.png";
        public const string MetalSmoothness = Derived + "/BlueMetal_MetalSmoothness.png";
        public const string Occlusion = Derived + "/BlueMetal_Occlusion.png";

        public static Color32 PackMetalSmoothness(Color32 arm) => new Color32(arm.b, 0, 0, (byte)(255 - arm.g));

        public static void Build()
        {
            Directory.CreateDirectory(Derived);
            AssetDatabase.Refresh();
            foreach (var path in new[] { Diffuse, Normal, Arm }) AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            // Decode the unchanged source file in linear space, only during editor authoring.
            var source = new Texture2D(2, 2, TextureFormat.RGBA32, false, true);
            Texture2D mask = null, ao = null;
            try
            {
                if (!source.LoadImage(File.ReadAllBytes(Arm))) throw new InvalidDataException("Cannot decode CC0 ARM source.");
                mask = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false, true);
                ao = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false, true);
                var input = source.GetPixels32(); var packed = new Color32[input.Length]; var ambient = new Color32[input.Length];
                for (var i = 0; i < input.Length; i++)
                {
                    packed[i] = PackMetalSmoothness(input[i]);
                    ambient[i] = new Color32(input[i].r, input[i].r, input[i].r, 255);
                }
                mask.SetPixels32(packed); mask.Apply(); ao.SetPixels32(ambient); ao.Apply();
                File.WriteAllBytes(MetalSmoothness, mask.EncodeToPNG());
                File.WriteAllBytes(Occlusion, ao.EncodeToPNG());
            }
            finally
            {
                Object.DestroyImmediate(source);
                if (mask != null) Object.DestroyImmediate(mask);
                if (ao != null) Object.DestroyImmediate(ao);
            }
            AssetDatabase.ImportAsset(MetalSmoothness, ImportAssetOptions.ForceUpdate);
            AssetDatabase.ImportAsset(Occlusion, ImportAssetOptions.ForceUpdate);
        }

        internal static void Apply(Material material)
        {
            material.SetTexture("_BaseMap", AssetDatabase.LoadAssetAtPath<Texture2D>(Diffuse));
            material.SetTexture("_BumpMap", AssetDatabase.LoadAssetAtPath<Texture2D>(Normal));
            material.SetTexture("_MetallicGlossMap", AssetDatabase.LoadAssetAtPath<Texture2D>(MetalSmoothness));
            material.SetTexture("_OcclusionMap", AssetDatabase.LoadAssetAtPath<Texture2D>(Occlusion));
            material.SetFloat("_BumpScale", .45f); material.SetFloat("_OcclusionStrength", .6f);
            material.SetFloat("_Smoothness", .85f); material.SetFloat("_SmoothnessTextureChannel", 0);
            material.EnableKeyword("_NORMALMAP"); material.EnableKeyword("_METALLICSPECGLOSSMAP"); material.EnableKeyword("_OCCLUSIONMAP");
            material.DisableKeyword("_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A");
            EditorUtility.SetDirty(material);
        }
    }

    public sealed class ArtSpikeTextureImporter : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(ArtSpikePbr.Source + "/", StringComparison.Ordinal) &&
                !assetPath.StartsWith(ArtSpikePbr.Derived + "/", StringComparison.Ordinal)) return;
            var importer = (TextureImporter)assetImporter;
            importer.textureType = assetPath == ArtSpikePbr.Normal ? TextureImporterType.NormalMap : TextureImporterType.Default;
            importer.sRGBTexture = assetPath == ArtSpikePbr.Diffuse;
            importer.maxTextureSize = 1024; importer.mipmapEnabled = true;
            importer.isReadable = false; importer.wrapMode = TextureWrapMode.Repeat;
            importer.alphaSource = TextureImporterAlphaSource.FromInput; importer.alphaIsTransparency = false;
            importer.filterMode = FilterMode.Trilinear; importer.textureCompression = TextureImporterCompression.Compressed;
            importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings
            { name = "Android", overridden = true, maxTextureSize = 1024, format = TextureImporterFormat.ASTC_6x6 });
        }
    }
}
