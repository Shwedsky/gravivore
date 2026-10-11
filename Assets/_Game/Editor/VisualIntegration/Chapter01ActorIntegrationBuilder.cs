using System;
using System.IO;
using System.Linq;
using Gravivore.Presentation.Assets;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Gravivore.Editor.VisualIntegration
{
    public static class Chapter01ActorIntegrationBuilder
    {
        public const string Root = "Assets/_Game/Content/ActorProductionV2";
        public const string Evidence = "docs/history/implementation-passes/chapter01-r2-actors-v2-integration";
        public static readonly string[] Actors = { "Scout", "Cutter", "Warden", "ArcDrone", "Carrier", "Magnetar", "Custodian" };
        public static readonly string[] Ids = { "scout-drone", "cutter-unit", "warden", "arc-drone", "carrier" };
        public static string Prefab(string actor) => Root + "/Prefabs/" + actor + "_V2.prefab";
        public static void Build()
        {
            Directory.CreateDirectory(Root + "/Prefabs");
            Directory.CreateDirectory(Evidence + "/verification");
            AssetDatabase.Refresh();
            var profiles = new HostileMotionProfile[Actors.Length];
            var records = new System.Collections.Generic.List<string>();
            for (var i = 0; i < Actors.Length; i++)
            {
                var actor = Actors[i];
                var wrapper = new GameObject(actor + "_V2");
                try
                {
                    if(actor == "Carrier")
                    {
                        var importer=(ModelImporter)AssetImporter.GetAtPath(Root+"/Models/Carrier_V2.fbx");
                        var clips=importer.clipAnimations;
                        foreach(var clip in clips)if(clip.name=="Bank")clip.loopTime=true;
                        importer.clipAnimations=clips;importer.SaveAndReimport();
                    }
                    var socketContainer = new GameObject("Presentation Sockets").transform;
                    socketContainer.SetParent(wrapper.transform, false);
                    var model = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(Root + "/Models/" + actor + "_V2.fbx"), socketContainer, false);
                    model.name = actor + "_V2_Rig";
                    // Authored -Y front imports as -Z. Keep gameplay root +Z.
                    model.transform.localRotation = Quaternion.Euler(0, 180, 0);
                    var animator = model.GetComponent<Animator>();
                    var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(Root + "/" + actor + "_V2.controller");
                    animator.runtimeAnimatorController = controller;
                    animator.applyRootMotion = false;
                    animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
                    var material = AssetDatabase.LoadAssetAtPath<Material>(Root + "/Materials/HostileV2_" +
                        (actor == "ArcDrone" ? "Blue" : actor == "Scout" || actor == "Cutter" || actor == "Custodian" ? "Red" : "Amber") + ".mat");
                    // Retain all authored PBR maps; modest local energy gain against R2 pools.
                    material.SetColor("_BaseColor",Color.white*1.08f);
                    material.globalIlluminationFlags=MaterialGlobalIlluminationFlags.BakedEmissive;
                    material.EnableKeyword("_EMISSION");
                    material.SetColor("_EmissionColor",Color.white*(actor=="ArcDrone"?3.2f:
                        actor=="Scout"||actor=="Cutter"||actor=="Custodian"?2.4f:2.6f));
                    EditorUtility.SetDirty(material);
                    var skins = model.GetComponentsInChildren<SkinnedMeshRenderer>(true).OrderBy(s => s.name).ToArray();
                    foreach (var skin in skins) { skin.sharedMaterials = new[] { material }; skin.updateWhenOffscreen = false; }
                    var lod = model.GetComponent<LODGroup>() ?? model.AddComponent<LODGroup>();
                    lod.SetLODs(new[] { new LOD(.14f, new Renderer[] { skins[0] }), new LOD(.075f, new Renderer[] { skins[1] }), new LOD(.025f, new Renderer[] { skins[2] }) });
                    lod.RecalculateBounds();
                    Transform Node(string name) => model.GetComponentsInChildren<Transform>(true).SingleOrDefault(t => t.name == name);
                    void Alias(string name, string target)
                    {
                        if (Node(name) != null) return;
                        var source = Node(target) ?? throw new InvalidOperationException(actor + " missing socket " + target);
                        var alias = new GameObject(name).transform; alias.SetParent(source, false);
                        records.Add(actor + ": " + name + " -> " + target);
                    }
                    Alias("AttackOrigin", "AttackOrigin_L");
                    Alias("WeaponLeft", Node("AttackOrigin_L") != null ? "AttackOrigin_L" : "AttackOrigin");
                    Alias("WeaponRight", Node("AttackOrigin_R") != null ? "AttackOrigin_R" : "AttackOrigin");
                    Alias("VfxTop", "Core"); Alias("VfxRear", "VisualCenter");
                    Alias("Sensor", Node("HEAD") != null ? "HEAD" : "Core");
                    Alias("DeathVfxOrigin", "HitCenter"); Alias("ProjectileSource", "AttackOrigin");
                    Alias("AttackLineOrigin", actor == "Custodian" ? "BossDischarge_L" : "AttackOrigin");
                    Alias("AttackCircleOrigin", "Core");
                    Alias("AttackConeOrigin", actor == "Custodian" ? "BossDischarge_R" : "AttackOrigin");
                    PresentationPrefabValidation.ValidateOrThrow(wrapper);
                    PrefabUtility.SaveAsPrefabAsset(wrapper, Prefab(actor));
                    profiles[i] = new HostileMotionProfile
                    {
                        Controller = controller,
                        RunCycleDistance = new[] { 1.65f, 1.7f, 1.35f, 2f, 2.2f, 1.4f, 1.4f }[i],
                        Idle = actor == "ArcDrone" ? "Hover" : "Idle",
                        Move = actor == "Carrier" ? "Bank" : "Run",
                        Attack = actor == "Cutter" ? "Cut" : actor == "ArcDrone" ? "Discharge" : "Attack",
                        Block = actor == "Warden" ? "Block" : "Hit",
                        Windup = actor == "Magnetar" ? "Charge" : actor == "Custodian" ? "Telegraph" : "Attack",
                        Release = actor == "Magnetar" ? "Vent" : "Attack",
                        VisualRadius = new[] { .76f, 1.46f, 1.17f, .85f, 1.30f, 2.11f, 2.98f }[i],
                        MaximumWallOffset = new[] { .43f, 1.08f, .71f, .51f, .8f, 1.48f, 2f }[i]
                        ,TurnDegreesPerSecond = new[] { 540f, 360f, 240f, 420f, 150f, 180f, 180f }[i]
                    };
                }
                finally { Object.DestroyImmediate(wrapper); }
            }
            var motion = AssetDatabase.LoadAssetAtPath<HostilePresentationDefinition>(Root + "/HostileMotion.asset");
            if (motion == null) { motion = ScriptableObject.CreateInstance<HostilePresentationDefinition>(); AssetDatabase.CreateAsset(motion, Root + "/HostileMotion.asset"); }
            motion.Profiles = profiles; EditorUtility.SetDirty(motion);
            var catalog = new SerializedObject(AssetDatabase.LoadAssetAtPath<S15VisualCatalog>("Assets/_Game/Content/Definitions/S15_VisualCatalog.asset"));
            for (var i = 0; i < 5; i++)
            {
                var recipe = catalog.FindProperty("_enemies").GetArrayElementAtIndex(i);
                if (recipe.FindPropertyRelative("_id").stringValue != Ids[i]) throw new InvalidOperationException("Archetype identity/order drift");
                recipe.FindPropertyRelative("_presentationPrefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(Prefab(Actors[i]));
                recipe.FindPropertyRelative("_parts").arraySize = 0;
            }
            catalog.FindProperty("_hostilePresentation").objectReferenceValue = motion;
            catalog.ApplyModifiedPropertiesWithoutUndo();
            var definition = new SerializedObject(AssetDatabase.LoadAssetAtPath<Object>("Assets/_Game/Content/Definitions/Chapter01_VisualIntegration.asset"));
            foreach (var pair in new[] { ("_elite", "Magnetar"), ("_boss", "Custodian") })
            {
                var binding = definition.FindProperty(pair.Item1);
                binding.FindPropertyRelative("_prefab").objectReferenceValue = AssetDatabase.LoadAssetAtPath<GameObject>(Prefab(pair.Item2));
                binding.FindPropertyRelative("_localPosition").vector3Value = Vector3.zero;
                binding.FindPropertyRelative("_localEulerAngles").vector3Value = Vector3.zero;
                binding.FindPropertyRelative("_localScale").vector3Value = Vector3.one;
            }
            definition.ApplyModifiedPropertiesWithoutUndo(); AssetDatabase.SaveAssets();
            File.WriteAllLines(Evidence + "/verification/socket-aliases.txt", records);
            Validate(); Debug.Log("CHAPTER01_ACTOR_INTEGRATION_AUTHOR_PASS");
        }
        public static void Validate()
        {
            var dependencies = AssetDatabase.GetDependencies(Chapter01BlueprintWorldBuilder.ScenePath, true);
            foreach (var actor in Actors)
            {
                if (!dependencies.Contains(Root + "/Models/" + actor + "_V2.fbx")) throw new InvalidOperationException(actor + " absent from playable Chapter01");
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Prefab(actor));
                PresentationPrefabValidation.ValidateOrThrow(prefab);
                var sockets = new PresentationSocketSet(prefab.transform);
                foreach (PresentationSocket role in Enum.GetValues(typeof(PresentationSocket)))
                    if (!sockets.TryGet(role, out _)) throw new InvalidOperationException(actor + " absent socket " + role);
            }
            if (dependencies.Any(p => p.Contains("ArtReview/ActorProductionV2"))) throw new InvalidOperationException("Actor review dependency leaked");
            File.WriteAllLines(Evidence + "/verification/production-dependencies.txt", dependencies);
            Debug.Log("CHAPTER01_ACTOR_INTEGRATION_VALIDATION_PASS");
        }
    }
}
