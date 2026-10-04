using System;
using System.Collections.Generic;
using System.IO;
using Gravivore.Presentation.Assets;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Gravivore.Editor.VisualIntegration
{
    public enum ArtCandidateRole { Player, Ordinary, Elite, Boss, Environment }

    [Serializable] public sealed class ArtTextureInfo { public string path, name; public int width, height; }
    [Serializable] public sealed class ArtIntakeReport
    {
        public string sourcePath, role;
        public int meshCount, rendererCount, materialSlots, uniqueMaterials, textureCount;
        public long triangles;
        public int staticRenderers, skinnedRenderers, bones, colliders, monoBehaviours, missingScripts, lights, cameras, lodGroups;
        public int missingMeshes, missingMaterials, brokenTextureReferences;
        public bool rigPresent, runtimeSafe;
        public string runtimeSafetyReason;
        public Vector3 rootScale, boundsCenter, boundsSize;
        public string[] animationClips, shaders, incompatibleShaders, unassignedTextureProperties, warnings;
        public ArtTextureInfo[] textures;
    }

    public static class ArtIntakeTool
    {
        [MenuItem("Gravivore/Art Intake/Report Selected Candidate")]
        public static void ReportSelected()
        {
            var source = Selection.activeObject as GameObject;
            if (source == null) throw new InvalidOperationException("Select a prefab/model GameObject in Project.");
            ArtIntakeReportWindow.Open(source);
        }

        public static ArtIntakeReport Inspect(GameObject source, ArtCandidateRole role)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            var report = new ArtIntakeReport { sourcePath = AssetDatabase.GetAssetPath(source), role = role.ToString(),
                rootScale = source.transform.localScale };
            var meshes = new HashSet<Mesh>();
            var materials = new HashSet<Material>();
            var textures = new HashSet<Texture>();
            var shaderNames = new HashSet<string>();
            var incompatible = new HashSet<string>();
            var emptyTextureSlots = new HashSet<string>();
            var clips = new HashSet<string>();
            var warnings = new List<string>();
            var renderers = source.GetComponentsInChildren<Renderer>(true);
            report.rendererCount = renderers.Length;
            var haveBounds = false; var bounds = new Bounds();
            foreach (var renderer in renderers)
            {
                Mesh mesh = null;
                if (renderer is SkinnedMeshRenderer skin)
                { report.skinnedRenderers++; report.bones += skin.bones.Length; mesh = skin.sharedMesh; }
                else if (renderer is MeshRenderer)
                { report.staticRenderers++; mesh = renderer.GetComponent<MeshFilter>()?.sharedMesh; }
                if (mesh != null)
                {
                    meshes.Add(mesh);
                    for (var i = 0; i < mesh.subMeshCount; i++)
                        if (mesh.GetTopology(i) == MeshTopology.Triangles) report.triangles += mesh.GetIndexCount(i) / 3;
                }
                else if (renderer is MeshRenderer || renderer is SkinnedMeshRenderer) report.missingMeshes++;
                if (!haveBounds) { bounds = renderer.bounds; haveBounds = true; }
                else bounds.Encapsulate(renderer.bounds);
                report.materialSlots += renderer.sharedMaterials.Length;
                foreach (var material in renderer.sharedMaterials)
                    if (material == null) report.missingMaterials++; else materials.Add(material);
            }
            foreach (var material in materials)
            {
                var shader = material.shader;
                if (shader == null) { incompatible.Add("<missing shader>"); continue; }
                shaderNames.Add(shader.name);
                if (material.GetTag("RenderPipeline", false, "") != "UniversalPipeline" &&
                    !shader.name.StartsWith("Universal Render Pipeline/", StringComparison.Ordinal))
                    incompatible.Add(shader.name);
                for (var i = 0; i < ShaderUtil.GetPropertyCount(shader); i++)
                {
                    if (ShaderUtil.GetPropertyType(shader, i) != ShaderUtil.ShaderPropertyType.TexEnv) continue;
                    var property = ShaderUtil.GetPropertyName(shader, i);
                    var texture = material.GetTexture(property);
                    if (texture != null) textures.Add(texture);
                    else emptyTextureSlots.Add(material.name + "." + property);
                }
                var savedTextures = new SerializedObject(material).FindProperty("m_SavedProperties.m_TexEnvs");
                for (var i = 0; savedTextures != null && i < savedTextures.arraySize; i++)
                {
                    var reference = savedTextures.GetArrayElementAtIndex(i).FindPropertyRelative("second.m_Texture");
                    if (reference != null && reference.objectReferenceValue == null && reference.objectReferenceInstanceIDValue != 0)
                        report.brokenTextureReferences++;
                }
            }
            foreach (var component in source.GetComponentsInChildren<Component>(true))
            {
                if (component == null) { report.missingScripts++; continue; }
                if (component is MonoBehaviour) report.monoBehaviours++;
                if (component is Collider) report.colliders++;
                if (component is Light) report.lights++;
                if (component is UnityEngine.Camera) report.cameras++;
                if (component is LODGroup) report.lodGroups++;
                var name = component.GetType().FullName;
                if (name == "UnityEngine.Animator" || name == "UnityEngine.Animation") report.rigPresent = true;
            }
            report.rigPresent |= report.bones > 0;
            foreach (var clip in AnimationUtility.GetAnimationClips(source)) if (clip != null) clips.Add(clip.name);
            if (!string.IsNullOrEmpty(report.sourcePath))
                foreach (var dependency in AssetDatabase.GetDependencies(report.sourcePath))
                    foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(dependency))
                        if (asset is AnimationClip clip) clips.Add(clip.name);
            report.meshCount = meshes.Count; report.uniqueMaterials = materials.Count; report.textureCount = textures.Count;
            report.boundsCenter = bounds.center; report.boundsSize = bounds.size;
            report.animationClips = Sorted(clips); report.shaders = Sorted(shaderNames);
            report.incompatibleShaders = Sorted(incompatible); report.unassignedTextureProperties = Sorted(emptyTextureSlots);
            var textureList = new List<ArtTextureInfo>();
            foreach (var texture in textures)
                textureList.Add(new ArtTextureInfo { path = AssetDatabase.GetAssetPath(texture), name = texture.name,
                    width = texture.width, height = texture.height });
            textureList.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
            report.textures = textureList.ToArray();
            try { PresentationPrefabValidation.ValidateOrThrow(source); report.runtimeSafe = true; }
            catch (InvalidOperationException exception) { report.runtimeSafetyReason = exception.Message; }
            var triangleBudget = role == ArtCandidateRole.Player ? 50000 : role == ArtCandidateRole.Ordinary ? 25000 :
                role == ArtCandidateRole.Elite ? 60000 : role == ArtCandidateRole.Boss ? 120000 : long.MaxValue;
            var rendererBudget = role == ArtCandidateRole.Player ? 40 : role == ArtCandidateRole.Ordinary ? 20 :
                role == ArtCandidateRole.Elite ? 30 : role == ArtCandidateRole.Boss ? 40 : int.MaxValue;
            if (report.triangles > triangleBudget) warnings.Add("Triangle guardrail exceeded; review visible LODs and device cost.");
            if (report.rendererCount > rendererBudget) warnings.Add("Renderer guardrail exceeded; review batching/materials.");
            if (role == ArtCandidateRole.Player && report.uniqueMaterials > 6 ||
                role == ArtCandidateRole.Ordinary && report.uniqueMaterials > 4)
                warnings.Add("Primary material guardrail exceeded; review shared materials and draw calls.");
            foreach (var texture in textures)
                if (texture.width > 2048 || texture.height > 2048) warnings.Add("Texture above 2k: " + texture.name);
            if (report.skinnedRenderers > 0) warnings.Add("Rig/skin cost and bounds need animated/device review.");
            if (report.lodGroups > 0) warnings.Add("Triangle total includes all LODs; evaluate the visible level separately.");
            if (incompatible.Count > 0) warnings.Add("Shader compatibility needs URP material conversion/review.");
            if (report.missingMaterials + report.missingMeshes + report.brokenTextureReferences > 0)
                warnings.Add("Missing mesh/material or broken texture reference requires repair.");
            report.warnings = warnings.ToArray();
            return report; // Budgets produce warnings, never automatic candidate rejection.
        }

        private static string[] Sorted(HashSet<string> values)
        { var result = new string[values.Count]; values.CopyTo(result); Array.Sort(result, StringComparer.Ordinal); return result; }
    }

    public sealed class ArtIntakeReportWindow : EditorWindow
    {
        private GameObject _source;
        private ArtCandidateRole _role = ArtCandidateRole.Environment;
        private string _json;
        private Vector2 _scroll;
        public static void Open(GameObject source)
        { var window = GetWindow<ArtIntakeReportWindow>("Art Intake"); window._source = source; window.Show(); }
        private void OnGUI()
        {
            _source = (GameObject)EditorGUILayout.ObjectField("Candidate",_source,typeof(GameObject),true);
            _role = (ArtCandidateRole)EditorGUILayout.EnumPopup("Budget role",_role);
            using (new EditorGUI.DisabledScope(_source == null))
                if (GUILayout.Button("Inspect candidate")) _json = JsonUtility.ToJson(ArtIntakeTool.Inspect(_source,_role),true);
            using (new EditorGUI.DisabledScope(string.IsNullOrEmpty(_json)))
                if (GUILayout.Button("Save report JSON"))
                {
                    var path = EditorUtility.SaveFilePanel("Save art intake report","docs",_source.name + "-intake.json","json");
                    if (!string.IsNullOrEmpty(path)) { File.WriteAllText(path,_json + "\n"); Debug.Log("Art intake report written: " + path); }
                }
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            EditorGUILayout.TextArea(_json ?? "Select a candidate and inspect. Budget warnings do not reject assets.");
            EditorGUILayout.EndScrollView();
        }
    }

    [CustomEditor(typeof(ArtReviewSlot))]
    public sealed class ArtReviewSlotEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            var slot = (ArtReviewSlot)target;
            if (GUILayout.Button("Refresh safe candidate / fallback")) { slot.RefreshPreview(); EditorUtility.SetDirty(slot); }
            if (GUILayout.Button("Focus gameplay camera")) Focus(slot, "Gameplay Camera Option");
            if (GUILayout.Button("Focus close camera")) Focus(slot, "Close Review Camera");
        }
        private static void Focus(ArtReviewSlot slot, string name)
        {
            foreach (var root in slot.gameObject.scene.GetRootGameObjects())
                foreach (var camera in root.GetComponentsInChildren<UnityEngine.Camera>(true))
                {
                    camera.enabled = camera.name == name;
                    if (!camera.enabled) continue;
                    var settings = AssetDatabase.LoadAssetAtPath<Gravivore.Presentation.Camera.CameraFollowSettings>(
                        "Assets/_Game/Content/Definitions/S01_CameraFollowSettings.asset");
                    var gameplay = camera.name == "Gameplay Camera Option";
                    var delta = gameplay ? settings.Offset : new Vector3(2.4f,1.8f,3.2f);
                    camera.transform.position = slot.transform.position + delta;
                    camera.transform.LookAt(slot.transform.position + Vector3.up * (gameplay ? settings.LookAtHeight : .9f));
                    if (gameplay) camera.fieldOfView = settings.FieldOfView;
                    EditorUtility.SetDirty(camera);
                }
        }
    }
}
