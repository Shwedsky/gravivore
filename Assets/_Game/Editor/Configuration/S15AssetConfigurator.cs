using System;
using Gravivore.Presentation.Assets;
using Gravivore.Presentation.Composition;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Gravivore.Editor
{
    public static class S15AssetConfigurator
    {
        public const string CatalogPath = "Assets/_Game/Content/Definitions/S15_VisualCatalog.asset";
        public const string BodyMaterialPath = "Assets/_Game/Content/Materials/S15_FactoryBody.mat";
        public const string AccentMaterialPath = "Assets/_Game/Content/Materials/S15_EnergyAccent.mat";
        public const string DarkMaterialPath = "Assets/_Game/Content/Materials/S15_MachineryDark.mat";
        public const string ChapterScenePath = "Assets/_Game/Content/Scenes/Chapter01_ScrapExclusion.unity";
        public const string ModelRoot = "Assets/ThirdParty/KenneyFactoryKit/Models/";

        [MenuItem("Gravivore/Configuration/Create S15 Asset Integration")]
        public static void ConfigureMenu()
        {
            ConfigureOrThrow();
            Debug.Log("GRAVIVORE S15 asset integration configured.");
        }

        public static void ConfigureOrThrow()
        {
            var body = CreateMaterial(BodyMaterialPath, new Color(0.34f, 0.4f, 0.42f, 1f), 0.72f, 0.15f);
            var accent = CreateMaterial(AccentMaterialPath, new Color(0.08f, 0.82f, 0.68f, 1f), 0.35f, 0.05f);
            var dark = CreateMaterial(DarkMaterialPath, new Color(0.075f, 0.09f, 0.105f, 1f), 0.58f, 0.45f);

            var catalog = AssetDatabase.LoadAssetAtPath<S15VisualCatalog>(CatalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<S15VisualCatalog>();
                catalog.name = "S15_VisualCatalog";
                AssetDatabase.CreateAsset(catalog, CatalogPath);
            }

            var serialized = new SerializedObject(catalog);
            serialized.FindProperty("_bodyMaterial").objectReferenceValue = body;
            serialized.FindProperty("_accentMaterial").objectReferenceValue = accent;
            serialized.FindProperty("_darkMaterial").objectReferenceValue = dark;
            SetRecipe(serialized.FindProperty("_player"), "player-techno-organism",
                Part("machine-connection-hole.fbx", new Vector3(0f, 0.72f, 0f), Vector3.zero, new Vector3(0.7f, 0.7f, 0.7f), 0),
                Part("cog-a.fbx", new Vector3(0f, 0.76f, -0.32f), new Vector3(90f, 0f, 0f), new Vector3(0.42f, 0.42f, 0.42f), 1),
                Part("piston-round.fbx", new Vector3(-0.38f, 0.58f, 0f), new Vector3(0f, 0f, 90f), new Vector3(0.55f, 0.55f, 0.55f), 2),
                Part("piston-round.fbx", new Vector3(0.38f, 0.58f, 0f), new Vector3(0f, 0f, -90f), new Vector3(0.55f, 0.55f, 0.55f), 2));

            SetRecipes(serialized.FindProperty("_enemies"), new[]
            {
                Recipe("scout-drone", Part("hopper-round.fbx", new Vector3(0f, 0.68f, 0f), Vector3.zero, Scale(0.58f), 0), Part("cone.fbx", new Vector3(0f, 1.14f, 0f), Vector3.zero, Scale(0.35f), 1)),
                Recipe("cutter-unit", Part("machine-connection-hole.fbx", new Vector3(0f, 0.72f, 0f), Vector3.zero, Scale(0.62f), 0), Part("robot-arm-a.fbx", new Vector3(0.22f, 0.65f, 0f), new Vector3(0f, 90f, 0f), Scale(0.5f), 1)),
                Recipe("warden", Part("box-large.fbx", new Vector3(0f, 0.72f, 0f), Vector3.zero, new Vector3(0.58f, 0.78f, 0.58f), 2), Part("structure-wall.fbx", new Vector3(0f, 0.65f, -0.24f), Vector3.zero, new Vector3(0.36f, 0.5f, 0.36f), 0)),
                Recipe("arc-drone", Part("cog-a.fbx", new Vector3(0f, 0.75f, 0f), new Vector3(90f, 0f, 0f), Scale(0.76f), 1), Part("cog-b.fbx", new Vector3(0f, 0.75f, 0f), Vector3.zero, Scale(0.48f), 2)),
                Recipe("carrier", Part("hopper-round.fbx", new Vector3(0f, 0.72f, 0f), Vector3.zero, new Vector3(0.9f, 0.62f, 0.9f), 0), Part("box-large.fbx", new Vector3(0f, 1.03f, 0f), Vector3.zero, new Vector3(0.55f, 0.3f, 0.55f), 2), Part("piston-round.fbx", new Vector3(0f, 0.42f, 0.42f), new Vector3(90f, 0f, 0f), Scale(0.5f), 1))
            });
            SetRecipes(serialized.FindProperty("_landmarks"), new[]
            {
                Recipe("relay-yard", Part("machine-fortified.fbx", Vector3.zero, Vector3.zero, Scale(1.45f), 0)),
                Recipe("cutting-floor", Part("crane-magnet.fbx", Vector3.zero, Vector3.zero, Scale(1.35f), 1)),
                Recipe("shield-dump", Part("structure-yellow-medium.fbx", Vector3.zero, Vector3.zero, Scale(1.3f), 0), Part("structure-wall.fbx", new Vector3(0f, 0f, 0.4f), Vector3.zero, Scale(1.3f), 2)),
                Recipe("capacitor-field", Part("pipe-large-long.fbx", Vector3.zero, new Vector3(0f, 0f, 90f), Scale(1.5f), 1)),
                Recipe("hauler-graveyard", Part("box-large.fbx", Vector3.zero, new Vector3(0f, 25f, 8f), Scale(1.5f), 2), Part("door-wide-closed.fbx", new Vector3(0f, 0f, -0.45f), Vector3.zero, Scale(1.15f), 0))
            });
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(catalog);
            catalog.ValidateOrThrow();

            var scene = EditorSceneManager.OpenScene(ChapterScenePath, OpenSceneMode.Single);
            S01SceneCompositionRoot root = null;
            foreach (var rootObject in scene.GetRootGameObjects())
            {
                root = rootObject.GetComponentInChildren<S01SceneCompositionRoot>(true);
                if (root != null) break;
            }
            if (root == null) throw new InvalidOperationException("Chapter scene requires S01SceneCompositionRoot.");
            var rootSerialized = new SerializedObject(root);
            rootSerialized.FindProperty("_s15VisualCatalog").objectReferenceValue = catalog;
            rootSerialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(root);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Failed to save S15 scene reference.");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static Material CreateMaterial(string path, Color color, float smoothness, float metallic)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) throw new InvalidOperationException("URP Lit shader is unavailable in the Editor.");
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader) { name = System.IO.Path.GetFileNameWithoutExtension(path) };
                AssetDatabase.CreateAsset(material, path);
            }
            material.shader = shader;
            material.color = color;
            material.SetFloat("_Smoothness", smoothness);
            material.SetFloat("_Metallic", metallic);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Vector3 Scale(float value) => new Vector3(value, value, value);
        private static RecipeData Recipe(string id, params PartData[] parts) => new RecipeData(id, parts);
        private static PartData Part(string model, Vector3 position, Vector3 rotation, Vector3 scale, int materialRole) => new PartData(model, position, rotation, scale, materialRole);

        private static void SetRecipes(SerializedProperty property, RecipeData[] recipes)
        {
            property.arraySize = recipes.Length;
            for (var i = 0; i < recipes.Length; i++) SetRecipe(property.GetArrayElementAtIndex(i), recipes[i].Id, recipes[i].Parts);
        }

        private static void SetRecipe(SerializedProperty property, string id, params PartData[] parts)
        {
            property.FindPropertyRelative("_id").stringValue = id;
            var serializedParts = property.FindPropertyRelative("_parts");
            serializedParts.arraySize = parts.Length;
            for (var i = 0; i < parts.Length; i++)
            {
                var target = serializedParts.GetArrayElementAtIndex(i);
                var source = AssetDatabase.LoadAssetAtPath<GameObject>(ModelRoot + parts[i].Model);
                if (source == null) throw new InvalidOperationException($"Missing imported S15 model: {parts[i].Model}");
                target.FindPropertyRelative("_sourceModel").objectReferenceValue = source;
                target.FindPropertyRelative("_localPosition").vector3Value = parts[i].Position;
                target.FindPropertyRelative("_localEulerAngles").vector3Value = parts[i].Rotation;
                target.FindPropertyRelative("_localScale").vector3Value = parts[i].Scale;
                target.FindPropertyRelative("_materialRole").enumValueIndex = parts[i].MaterialRole;
            }
        }

        private readonly struct RecipeData
        {
            public RecipeData(string id, PartData[] parts) { Id = id; Parts = parts; }
            public string Id { get; }
            public PartData[] Parts { get; }
        }

        private readonly struct PartData
        {
            public PartData(string model, Vector3 position, Vector3 rotation, Vector3 scale, int materialRole)
            { Model = model; Position = position; Rotation = rotation; Scale = scale; MaterialRole = materialRole; }
            public string Model { get; }
            public Vector3 Position { get; }
            public Vector3 Rotation { get; }
            public Vector3 Scale { get; }
            public int MaterialRole { get; }
        }
    }
}
