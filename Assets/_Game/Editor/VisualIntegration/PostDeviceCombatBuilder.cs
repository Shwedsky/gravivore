using Gravivore.Presentation.Combat;
using Gravivore.Presentation.Composition;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Gravivore.Editor.VisualIntegration
{
    public static class PostDeviceCombatBuilder
    {
        public const string SettingsPath = "Assets/_Game/Content/VisualSlice/PostDevicePresentation.asset";
        public static void Apply()
        {
            var settings = AssetDatabase.LoadAssetAtPath<PostDevicePresentationDefinition>(SettingsPath);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<PostDevicePresentationDefinition>();
                AssetDatabase.CreateAsset(settings,SettingsPath);
            }
            settings.ValidateOrThrow();
            var environment=PrefabUtility.LoadPrefabContents(FirstVisualSliceBuilder.Prefab("Environment_Slice"));
            try
            {
                foreach(Transform prop in environment.transform)
                    if(prop.name=="Maintenance_Station" && Mathf.Abs(prop.localPosition.x-6.1f)<.01f)
                    { var p=prop.localPosition; p.z=66; prop.localPosition=p; PrefabUtility.RecordPrefabInstancePropertyModifications(prop); }
                PrefabUtility.SaveAsPrefabAsset(environment,FirstVisualSliceBuilder.Prefab("Environment_Slice"));
            }
            finally { PrefabUtility.UnloadPrefabContents(environment); }
            var scene = EditorSceneManager.OpenScene(FirstVisualSliceBuilder.ScenePath);
            S01SceneCompositionRoot root = null;
            foreach(var obj in scene.GetRootGameObjects()) if(obj.TryGetComponent(out S01SceneCompositionRoot candidate)) root=candidate;
            // Opening a scene unloads unreferenced assets, including a freshly-created
            // settings object. Reload after the scene is open before assigning its reference.
            settings=AssetDatabase.LoadAssetAtPath<PostDevicePresentationDefinition>(SettingsPath);
            if(settings==null) throw new System.InvalidOperationException("Post-device settings failed to reload.");
            var serialized = new SerializedObject(root);
            serialized.FindProperty("_postDevicePresentation").objectReferenceValue=settings;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            var env=new SerializedObject(root.VisualEnvironment); var obstacles=env.FindProperty("_sliceObstacles");
            for(var i=0;i<obstacles.arraySize;i++)
            {
                var item=obstacles.GetArrayElementAtIndex(i);
                if(item.FindPropertyRelative("_name").stringValue=="Slice Maintenance station")
                    item.FindPropertyRelative("_center").vector3Value=new Vector3(6.1f,1,66);
            }
            env.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets(); Debug.Log("POST_DEVICE_PRESENTATION_INTEGRATED");
        }
    }
}
