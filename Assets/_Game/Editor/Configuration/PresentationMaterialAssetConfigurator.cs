using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Gravivore.Presentation.Composition;

namespace Gravivore.Editor
{
    public static class PresentationMaterialAssetConfigurator
    {
        public const string LitMaterialPath = "Assets/_Game/Content/Materials/Gravivore_Lit.mat";
        public const string UnlitMaterialPath = "Assets/_Game/Content/Materials/Gravivore_Unlit.mat";
        public const string PalettePath = "Assets/_Game/Content/Materials/PresentationMaterialPalette.asset";
        public const string ChapterScenePath = "Assets/_Game/Content/Scenes/Chapter01_ScrapExclusion.unity";

        [MenuItem("Gravivore/Configuration/Create Presentation Materials")]
        public static void ConfigureMenu()
        {
            ConfigureOrThrow();
            Debug.Log("GRAVIVORE presentation materials configured.");
        }

        public static void ConfigureOrThrow()
        {
            var lit = CreateOrUpdateMaterial(LitMaterialPath, "Universal Render Pipeline/Lit");
            var unlit = CreateOrUpdateMaterial(UnlitMaterialPath, "Universal Render Pipeline/Unlit");
            var palette = AssetDatabase.LoadAssetAtPath<PresentationMaterialPalette>(PalettePath);
            if (palette == null)
            {
                palette = ScriptableObject.CreateInstance<PresentationMaterialPalette>();
                palette.name = "PresentationMaterialPalette";
                AssetDatabase.CreateAsset(palette, PalettePath);
            }

            var paletteObject = new SerializedObject(palette);
            paletteObject.FindProperty("_litMaterial").objectReferenceValue = lit;
            paletteObject.FindProperty("_unlitMaterial").objectReferenceValue = unlit;
            paletteObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(palette);

            var scene = EditorSceneManager.OpenScene(ChapterScenePath, OpenSceneMode.Single);
            S01SceneCompositionRoot compositionRoot = null;
            foreach (var rootObject in scene.GetRootGameObjects())
            {
                compositionRoot = rootObject.GetComponentInChildren<S01SceneCompositionRoot>(true);
                if (compositionRoot != null) break;
            }

            if (compositionRoot == null)
            {
                throw new InvalidOperationException($"Scene {ChapterScenePath} requires an S01SceneCompositionRoot.");
            }

            var rootObjectSerialized = new SerializedObject(compositionRoot);
            rootObjectSerialized.FindProperty("_materialPalette").objectReferenceValue = palette;
            rootObjectSerialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(compositionRoot);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene))
            {
                throw new InvalidOperationException($"Failed to save presentation material reference in {ChapterScenePath}.");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static Material CreateOrUpdateMaterial(string path, string shaderName)
        {
            var shader = Shader.Find(shaderName);
            if (shader == null)
            {
                throw new InvalidOperationException($"Required shader is unavailable in the Editor: {shaderName}.");
            }

            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader) { name = System.IO.Path.GetFileNameWithoutExtension(path) };
                AssetDatabase.CreateAsset(material, path);
            }
            else
            {
                material.shader = shader;
            }

            material.color = Color.white;
            EditorUtility.SetDirty(material);
            return material;
        }
    }
}
