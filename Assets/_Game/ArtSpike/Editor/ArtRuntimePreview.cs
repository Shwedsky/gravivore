using System;
using Gravivore.Presentation.Assets;
using Gravivore.Presentation.Evolution;
using UnityEditor;
using UnityEngine;

namespace Gravivore.ArtSpike.Editor
{
    // Deliberately opt-in: ordinary project configuration never enables this ART-branch preview.
    public static class ArtRuntimePreview
    {
        [MenuItem("Gravivore/Art Spike/Bind Temporary Runtime Preview")]
        public static void Bind()
        {
            var definition = AssetDatabase.LoadAssetAtPath<EvolutionDefinition>("Assets/_Game/Content/Definitions/S07_Evolution.asset");
            var serialized = new SerializedObject(definition);
            var forms = serialized.FindProperty("_tierPrefabs");
            forms.arraySize = 3;
            serialized.FindProperty("_attackPresentationOffset").vector3Value = new Vector3(0f, .68f, 1.15f);
            for (var i = 0; i < 3; i++)
                forms.GetArrayElementAtIndex(i).objectReferenceValue = Load(ArtSpikeBuilder.CharacterPaths[i]);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            definition.ValidateOrThrow();
            EditorUtility.SetDirty(definition);

            var catalog = AssetDatabase.LoadAssetAtPath<S15VisualCatalog>(Gravivore.Editor.S15AssetConfigurator.CatalogPath);
            serialized = new SerializedObject(catalog);
            var enemies = serialized.FindProperty("_enemies");
            var found = false;
            for (var i = 0; i < enemies.arraySize; i++)
            {
                var recipe = enemies.GetArrayElementAtIndex(i);
                if (recipe.FindPropertyRelative("_id").stringValue != "cutter-unit") continue;
                recipe.FindPropertyRelative("_presentationPrefab").objectReferenceValue = Load(ArtSpikeBuilder.CharacterPaths[3]);
                found = true;
            }
            if (!found) throw new InvalidOperationException("Cutter runtime recipe is missing.");
            serialized.ApplyModifiedPropertiesWithoutUndo();
            catalog.ValidateOrThrow();
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            Debug.Log("ART V3 runtime preview bound: G-0 Tier0/1/2 and Cutter only; existing gameplay authority preserved.");
        }

        private static GameObject Load(string path) =>
            AssetDatabase.LoadAssetAtPath<GameObject>(path) ??
            throw new InvalidOperationException("Missing ART runtime form: " + path);
    }
}
