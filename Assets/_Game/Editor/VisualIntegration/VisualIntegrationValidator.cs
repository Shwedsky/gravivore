using System;
using Gravivore.Gameplay.Enemies;
using Gravivore.Presentation.Assets;
using Gravivore.Presentation.Composition;
using Gravivore.Presentation.Evolution;
using Gravivore.Presentation.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Gravivore.Editor.VisualIntegration
{
    public static class VisualIntegrationValidator
    {
        public static void ValidateOrThrow()
        {
            var definition = AssetDatabase.LoadAssetAtPath<ChapterVisualIntegrationDefinition>(VisualIntegrationFoundationBuilder.DefinitionPath);
            if (definition == null) throw new InvalidOperationException("Chapter visual integration definition is missing.");
            definition.ValidateOrThrow();
            var evolution = AssetDatabase.LoadAssetAtPath<EvolutionDefinition>("Assets/_Game/Content/Definitions/S07_Evolution.asset");
            if (evolution.Catalog.TierOverrides.Length != 3) throw new InvalidOperationException("Three player intake slots are required.");
            var hub = AssetDatabase.LoadAssetAtPath<GameObject>(VisualIntegrationFoundationBuilder.HubPath);
            if (hub == null) throw new InvalidOperationException("Repair hub anchor prefab is missing.");
            foreach (var anchor in VisualIntegrationFoundationBuilder.HubAnchors)
                if (hub.transform.Find(anchor) == null) throw new InvalidOperationException("Missing repair hub anchor: " + anchor);
            foreach (var component in hub.GetComponentsInChildren<Component>(true))
                if (!(component is Transform)) throw new InvalidOperationException("Repair hub anchor prefab must contain only transforms.");
            foreach (var scene in EditorBuildSettings.scenes)
                if (scene.path == VisualIntegrationFoundationBuilder.ReviewPath)
                    throw new InvalidOperationException("Art review scene must stay outside Android/gameplay build scenes.");
            ValidateScene(VisualIntegrationFoundationBuilder.ChapterPath, scene =>
            {
                S01SceneCompositionRoot root = null;
                foreach (var obj in scene.GetRootGameObjects())
                    if (obj.TryGetComponent(out S01SceneCompositionRoot candidate)) root = candidate;
                if (root == null || root.VisualEnvironment == null) throw new InvalidOperationException("Chapter visual layer is not injected.");
                var environment = root.VisualEnvironment; environment.ValidateOrThrow();
                if (environment.RegionCount != 11) throw new InvalidOperationException("Exactly 11 foundation regions are required.");
                var serializedRoot = new SerializedObject(root);
                var spawn = serializedRoot.FindProperty("_playerSpawn").vector3Value;
                if (environment.GetRegion("repair-hub").Root.position != spawn ||
                    environment.GetRegion("repair-hub").Root.Find("PlayerDockPoint").position != spawn)
                    throw new InvalidOperationException("Repair hub must stay at the current player spawn.");
                var spots = serializedRoot.FindProperty("_spawnSpotDefinitions");
                for (var i = 0; i < spots.arraySize; i++)
                {
                    var config = ((SpawnSpotDefinition)spots.GetArrayElementAtIndex(i).objectReferenceValue).CreateRuntimeConfiguration();
                    var region = environment.GetRegion(config.Id);
                    if (region.Root.position != config.WorldOrigin || region.EnemyId != config.Enemy.Id || region.Landmark == null)
                        throw new InvalidOperationException("Visual spot metadata must match the unchanged gameplay spot: " + config.Id);
                }
                foreach (var id in new[] { "start-region", "elite-approach", "elite-arena", "boss-approach", "boss-arena" })
                    environment.GetRegion(id);
                if (root.GetComponentsInChildren<ArtReviewSlot>(true).Length != 0)
                    throw new InvalidOperationException("Review helpers must not enter gameplay scenes.");
            });
            ValidateScene(VisualIntegrationFoundationBuilder.ReviewPath, scene =>
            {
                var slots = 0;
                foreach (var root in scene.GetRootGameObjects())
                {
                    foreach (var component in root.GetComponentsInChildren<Component>(true))
                    {
                        if (component == null || component is Collider || component is Rigidbody ||
                            component is MonoBehaviour && !(component is ArtReviewSlot))
                            throw new InvalidOperationException("Review scene contains missing scripts or gameplay authority.");
                        if (component is ArtReviewSlot slot)
                        { slots++; slot.Candidate.ValidateOrThrow(); PresentationPrefabValidation.ValidateOrThrow(slot.Fallback); }
                    }
                }
                if (slots != 4) throw new InvalidOperationException("Review scene needs four standardized character slots.");
            });
        }
        private static void ValidateScene(string path, Action<Scene> validate)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null) throw new InvalidOperationException("Integration scene missing: " + path);
            var scene = SceneManager.GetSceneByPath(path);
            var opened = !scene.IsValid() || !scene.isLoaded;
            if (opened) scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            try { validate(scene); }
            finally { if (opened) EditorSceneManager.CloseScene(scene,true); }
        }
    }
}
