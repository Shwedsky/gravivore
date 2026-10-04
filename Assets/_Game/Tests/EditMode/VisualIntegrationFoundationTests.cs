using System;
using System.Collections.Generic;
using System.Reflection;
using Gravivore.Editor.VisualIntegration;
using Gravivore.Presentation.Assets;
using Gravivore.Presentation.Evolution;
using Gravivore.Presentation.World;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Gravivore.Tests.EditMode
{
    public sealed class VisualIntegrationFoundationTests
    {
        private readonly List<Object> _owned = new List<Object>();
        [TearDown] public void Cleanup()
        { foreach (var obj in _owned) if (obj != null) Object.DestroyImmediate(obj); _owned.Clear(); }
        private GameObject Model()
        {
            var obj = GameObject.CreatePrimitive(PrimitiveType.Cube); Object.DestroyImmediate(obj.GetComponent<Collider>());
            _owned.Add(obj); return obj;
        }
        private static void Set(object target, string field, object value) =>
            target.GetType().GetField(field,BindingFlags.NonPublic | BindingFlags.Instance).SetValue(target,value);

        [Test] public void FoundationValidatorChecksActualSceneBindingsAndExcludedReviewScene() =>
            Assert.DoesNotThrow(VisualIntegrationValidator.ValidateOrThrow);

        [Test] public void OptionalSlotsRetainPlayerFallbacksAndEmptyEncounterBindings()
        {
            var evolution = AssetDatabase.LoadAssetAtPath<EvolutionDefinition>("Assets/_Game/Content/Definitions/S07_Evolution.asset").Catalog;
            Assert.That(evolution.TierOverrides,Has.Length.EqualTo(3));
            foreach (var slot in evolution.TierOverrides) Assert.IsFalse(slot.HasPrefab);
            foreach (var prefab in evolution.TierPrefabs) Assert.DoesNotThrow(() => PresentationPrefabValidation.ValidateOrThrow(prefab));
            var encounters = AssetDatabase.LoadAssetAtPath<ChapterVisualIntegrationDefinition>(VisualIntegrationFoundationBuilder.DefinitionPath);
            Assert.IsFalse(encounters.Elite.HasPrefab); Assert.IsFalse(encounters.Boss.HasPrefab); Assert.IsFalse(encounters.RepairHub.HasPrefab);
        }

        [TestCase("collider")] [TestCase("rigidbody")] [TestCase("script")]
        [TestCase("camera")] [TestCase("light")]
        public void UnsafePrefabIsRejectedBeforeInstantiation(string kind)
        {
            var obj = Model();
            switch (kind)
            {
                case "collider": obj.AddComponent<BoxCollider>(); break;
                case "rigidbody": obj.AddComponent<Rigidbody>(); break;
                case "script": obj.AddComponent<CharacterVisualBinding>(); break;
                case "camera": obj.AddComponent<UnityEngine.Camera>(); break;
                case "light": obj.AddComponent<Light>(); break;
            }
            var binding = new PresentationModelBinding(); Set(binding,"_prefab",obj);
            var parent = new GameObject("Authority parent"); _owned.Add(parent);
            Assert.Throws<InvalidOperationException>(() => binding.InstantiateUnder(parent.transform));
            Assert.That(parent.transform.childCount,Is.Zero);
        }

        [Test] public void PureSkinnedGeometryAndBonesAreAcceptedWithoutPlaybackAuthority()
        {
            var obj = new GameObject("Skin-only candidate"); _owned.Add(obj);
            var skin = obj.AddComponent<SkinnedMeshRenderer>();
            var bone = new GameObject("Bone").transform; bone.SetParent(obj.transform,false); skin.bones = new[] { bone };
            Assert.DoesNotThrow(() => PresentationPrefabValidation.ValidateOrThrow(obj));
            var report = ArtIntakeTool.Inspect(obj,ArtCandidateRole.Player);
            Assert.That(report.skinnedRenderers,Is.EqualTo(1)); Assert.IsTrue(report.rigPresent);
            Assert.That(report.bones,Is.EqualTo(1)); Assert.That(report.missingMeshes,Is.EqualTo(1));
        }

        [Test] public void OffsetsAndScaleAffectOnlyInstantiatedVisualChild()
        {
            var obj = Model(); var parent = new GameObject("Authority",typeof(CharacterController)); _owned.Add(parent);
            parent.transform.position = new Vector3(2,0,3); var collider = parent.GetComponent<CharacterController>();
            collider.radius = .65f; collider.height = 2.4f; collider.center = new Vector3(0,1.2f,0);
            var binding = new PresentationModelBinding(); Set(binding,"_prefab",obj);
            Set(binding,"_localPosition",new Vector3(3,4,5)); Set(binding,"_localEulerAngles",new Vector3(0,25,0));
            Set(binding,"_localScale",new Vector3(7,9,11));
            var visual = binding.InstantiateUnder(parent.transform);
            Assert.That(visual.transform.localPosition,Is.EqualTo(binding.LocalPosition));
            Assert.That(Quaternion.Angle(visual.transform.localRotation,binding.LocalRotation),Is.LessThan(.001f));
            Assert.That(visual.transform.localScale,Is.EqualTo(binding.LocalScale));
            Assert.That(parent.transform.position,Is.EqualTo(new Vector3(2,0,3)));
            Assert.That(parent.transform.localScale,Is.EqualTo(Vector3.one));
            Assert.That(collider.radius,Is.EqualTo(.65f)); Assert.That(collider.height,Is.EqualTo(2.4f));
            Assert.That(collider.center,Is.EqualTo(new Vector3(0,1.2f,0)));
        }

        [TestCase(0)] [TestCase(-1)] [TestCase(float.NaN)] [TestCase(float.PositiveInfinity)]
        public void InvalidScaleCannotEnterAnyModelBinding(float scale) =>
            Assert.Throws<InvalidOperationException>(() => PresentationModelBinding.ValidateTransform(
                Vector3.zero,Vector3.zero,new Vector3(scale,1,1)));

        [Test] public void OptionalSocketsIgnoreImportedBoneNamesAndResolveDedicatedContainer()
        {
            var obj = Model(); var boneRoot = new GameObject("ImportedBones").transform; boneRoot.SetParent(obj.transform,false);
            new GameObject("Core").transform.SetParent(boneRoot,false); new GameObject("Core").transform.SetParent(boneRoot,false);
            var missing = new PresentationSocketSet(obj.transform);
            Assert.IsFalse(missing.TryGet(PresentationSocket.Core,out _));
            Assert.That(missing.GetOr(PresentationSocket.Root,null),Is.EqualTo(obj.transform));
            var container = new GameObject("Presentation Sockets").transform; container.SetParent(obj.transform,false);
            var origin = new GameObject("AttackOrigin").transform; origin.SetParent(container,false);
            Assert.That(new PresentationSocketSet(obj.transform).GetOr(PresentationSocket.AttackOrigin,null),Is.EqualTo(origin));
            new GameObject("AttackOrigin").transform.SetParent(container,false);
            Assert.Throws<InvalidOperationException>(() => new PresentationSocketSet(obj.transform));
        }

        [Test] public void WholeEnemyAndLandmarkRecipesNeedNoLegacyKitbashPartsAndPoolBindingResets()
        {
            var catalog = Object.Instantiate(AssetDatabase.LoadAssetAtPath<S15VisualCatalog>(Gravivore.Editor.S15AssetConfigurator.CatalogPath));
            _owned.Add(catalog); var candidate = Model();
            foreach (var id in new[] { "scout-drone","cutter-unit","warden","arc-drone","carrier" })
            { catalog.TryGetEnemy(id,out var recipe); Set(recipe,"_presentationPrefab",candidate); Set(recipe,"_parts",Array.Empty<S15VisualPart>()); }
            foreach (var id in new[] { "relay-yard","cutting-floor","shield-dump","capacitor-field","hauler-graveyard" })
            { var recipe = catalog.GetLandmark(id); Set(recipe,"_presentationPrefab",candidate); Set(recipe,"_parts",Array.Empty<S15VisualPart>()); }
            Assert.DoesNotThrow(catalog.ValidateOrThrow);
            var parent = new GameObject("Pool authority"); _owned.Add(parent);
            var state = new S15EnemyVisualState(parent.transform,catalog);
            state.Apply("carrier"); Assert.IsNotNull(parent.GetComponent<CharacterVisualBinding>().ActiveModel);
            state.Reset(); Assert.IsNull(parent.GetComponent<CharacterVisualBinding>().ActiveModel);
            state.Apply("scout-drone"); Assert.That(state.ActiveId,Is.EqualTo("scout-drone"));
            Assert.That(parent.transform.localScale,Is.EqualTo(Vector3.one));
        }

        [Test] public void IntakeReportsGeometryMaterialsTexturesBoundsAndWarningsWithoutBudgetRejection()
        {
            var obj = Model(); var renderer = obj.GetComponent<MeshRenderer>();
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit")); _owned.Add(material);
            var texture = new Texture2D(4,8); _owned.Add(texture); material.SetTexture("_BaseMap",texture);
            renderer.sharedMaterials = new[] { material,material };
            for (var i = 0; i < 40; i++)
            {
                var child = new GameObject("Repeated mesh " + i,typeof(MeshFilter),typeof(MeshRenderer));
                child.transform.SetParent(obj.transform,false);
                child.GetComponent<MeshFilter>().sharedMesh = obj.GetComponent<MeshFilter>().sharedMesh;
                child.GetComponent<MeshRenderer>().sharedMaterials = renderer.sharedMaterials;
            }
            var report = ArtIntakeTool.Inspect(obj,ArtCandidateRole.Player);
            Assert.That(report.meshCount,Is.EqualTo(1)); Assert.That(report.rendererCount,Is.EqualTo(41));
            Assert.That(report.triangles,Is.EqualTo(41 * 12)); Assert.That(report.materialSlots,Is.EqualTo(82));
            Assert.That(report.uniqueMaterials,Is.EqualTo(1)); Assert.That(report.textureCount,Is.EqualTo(1));
            Assert.That(report.textures[0].width,Is.EqualTo(4)); Assert.That(report.textures[0].height,Is.EqualTo(8));
            Assert.That(report.boundsSize.sqrMagnitude,Is.GreaterThan(0));
            Assert.IsTrue(report.runtimeSafe); Assert.That(report.warnings,Has.Some.Contains("Renderer guardrail"));
            Assert.DoesNotThrow(() => PresentationPrefabValidation.ValidateOrThrow(obj));
        }
    }
}
