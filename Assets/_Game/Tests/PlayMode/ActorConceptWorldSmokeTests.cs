using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gravivore.Gameplay.Combat;
using Gravivore.Gameplay.Enemies;
using Gravivore.Gameplay.Player;
using Gravivore.Presentation.Assets;
using Gravivore.Presentation.Camera;
using Gravivore.Presentation.Combat;
using Gravivore.Presentation.Player;
using Gravivore.Presentation.Evolution;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace Gravivore.Tests.PlayMode
{
    public sealed class ActorConceptWorldSmokeTests
    {
        private const string Output = "docs/history/implementation-passes/actors-combat-concept-world";
        private CanonicalSceneTestScope _scene;
        private static string Stage => Environment.GetEnvironmentVariable("GRAVIVORE_ACTOR_CONCEPT_STAGE") == "before" ? "before" : "after";
        private static readonly string[] Names = { "Scout", "Cutter", "Warden", "ArcDrone", "Carrier" };
        [UnityTearDown] public IEnumerator Cleanup() { if (_scene != null) yield return _scene.Cleanup(); }
        private IEnumerator Load()
        {
            _scene = new CanonicalSceneTestScope(); yield return _scene.Load();
            var root = _scene.Root; root.PlayerHealth.SetDevelopmentGodMode(true);
            root.PlayerObject.GetComponent<PlayerLocomotion>().enabled = false;
            root.PlayerObject.GetComponent<GravityAttackController>().enabled = false;
            root.EnemyPopulation.enabled = false;
            foreach (var enemy in root.GetComponentsInChildren<OrdinaryEnemyController>(true)) enemy.enabled = false;
            root.MagnetarGuard.enabled = false; root.CustodianBoss.enabled = false;
        }
        // Kept as a reproduction helper for the executed opt-in diagnostic runs.
        public IEnumerator DiagnoseMovingSurfaceLighting()
        {
            yield return Load(); var root = _scene.Root;
            var enemy = root.EnemyPopulation.GetSpot(2).GetLiveEnemy(0);
            Move(enemy.transform, new Vector3(15, 0, 9)); Move(root.PlayerObject.transform, new Vector3(15, 0, 6.5f));
            root.GetComponent<VisualSliceAnimationBridge>().enabled = false;
            root.GetComponent<VisualSliceCombatMotion>().enabled = false;
            var binding = enemy.GetComponent<CharacterVisualBinding>();
            var material = binding.ActiveModel.GetComponentInChildren<SkinnedMeshRenderer>().sharedMaterial;
            var normal = material.GetFloat("_BumpScale"); var smooth = material.GetFloat("_Smoothness");
            var prior = RenderSettings.ambientProbe; var data = new List<string>();
            for (var c = 0; c < 3; c++) for (var n = 0; n < 9; n++) data.Add("SH[" + c + "," + n + "]=" + prior[c, n]);
            try
            {
                yield return null; Capture("diagnostic_original");
                material.SetFloat("_BumpScale", 0); material.SetFloat("_Smoothness", .35f);
                yield return null; Capture("diagnostic_no_normal");
                var sh = new SphericalHarmonicsL2(); sh.AddAmbientLight(new Color(.22f, .27f, .34f));
                sh.AddDirectionalLight(new Vector3(-.4f, 1, -.6f).normalized, new Color(.6f, .69f, .8f), .7f);
                RenderSettings.ambientProbe = sh;
                yield return null; Capture("diagnostic_explicit_SH");
                data.Add("material=" + material.name + "; color=" + material.GetColor("_BaseColor") + "; metallic=" + material.GetFloat("_Metallic") + "; texture=" + material.GetTexture("_BaseMap").name);
                var skins = binding.ActiveModel.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                foreach (var skin in skins)
                {
                    data.Add(skin.name + "; uv=" + skin.sharedMesh.uv.Length + "; visible=" + skin.isVisible + "; bounds=" + skin.bounds + "; enabled=" + skin.enabled);
                    data.Add("UV first=" + string.Join(";", skin.sharedMesh.uv.Take(6).Select(v => v.ToString())) + "; range=" + skin.sharedMesh.uv.Min(v => v.x) + "," + skin.sharedMesh.uv.Max(v => v.x) + "," + skin.sharedMesh.uv.Min(v => v.y) + "," + skin.sharedMesh.uv.Max(v => v.y));
                    var block = new MaterialPropertyBlock(); skin.lightProbeUsage = LightProbeUsage.CustomProvided;
                    block.CopySHCoefficientArraysFrom(new[] { sh }); skin.SetPropertyBlock(block);
                }
                yield return null; Capture("diagnostic_actor_SH");
                var testMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit")); testMaterial.SetTexture("_BaseMap", material.GetTexture("_BaseMap"));
                testMaterial.SetColor("_BaseColor", Color.white);
                foreach (var skin in skins) skin.sharedMaterial = testMaterial;
                yield return null; Capture("diagnostic_unlit_atlas");
                testMaterial.SetTexture("_BaseMap", null); testMaterial.SetColor("_BaseColor", Color.white);
                var actorAnimator = binding.ActiveModel.GetComponentInChildren<Animator>(); actorAnimator.cullingMode = AnimatorCullingMode.AlwaysAnimate; actorAnimator.Play("Idle", 0, .2f); actorAnimator.Update(.1f);
                binding.ActiveModel.GetComponentInChildren<LODGroup>().ForceLOD(0);
                yield return null; yield return null; Capture("diagnostic_white_skin");
                var baked = new Mesh(); skins[0].BakeMesh(baked);
                var testObject = new GameObject("Diagnostic CPU skin", typeof(MeshFilter), typeof(MeshRenderer));
                testObject.transform.SetPositionAndRotation(skins[0].transform.position, skins[0].transform.rotation); testObject.transform.localScale = skins[0].transform.lossyScale;
                testObject.GetComponent<MeshFilter>().sharedMesh = baked; testObject.GetComponent<MeshRenderer>().sharedMaterial = testMaterial;
                foreach (var skin in skins) skin.enabled = false;
                yield return null; Capture("diagnostic_white_CPU_mesh");
                foreach (var skin in skins) skin.enabled = true;
                Object.Destroy(testObject); Object.Destroy(baked);
                testMaterial.SetTexture("_BaseMap", material.GetTexture("_BaseMap"));
                yield return null; yield return null; Capture("diagnostic_atlas_LOD0");
                foreach (var skin in skins) skin.sharedMaterial = material;
                Object.Destroy(testMaterial);
            }
            finally { material.SetFloat("_BumpScale", normal); material.SetFloat("_Smoothness", smooth); RenderSettings.ambientProbe = prior; }
            Directory.CreateDirectory(Output + "/verification"); File.WriteAllLines(Output + "/verification/surface-lighting-diagnosis.txt", data);
        }
        [UnityTest, Timeout(180000)] public IEnumerator MatchedColdWarmActorGalleryAndArenaArticulation()
        {
            yield return Load(); var root = _scene.Root;
            using(var baseline=new ActorConceptSurfaceBaselineScope(root.transform,Stage=="before"))
            {
            var bridge = root.GetComponent<VisualSliceAnimationBridge>(); var motion = root.GetComponent<VisualSliceCombatMotion>();
            bridge.enabled = false; motion.enabled = false;
            var records = new List<ActorView>();
            for (var sector = 0; sector < 2; sector++)
            {
                var centre = new Vector3(sector == 0 ? -15 : 15, 0, 9);
                var zone = sector == 0 ? "cold" : "warm";
                Move(root.PlayerObject.transform, centre + Vector3.back * 2.5f);
                for (var i = 0; i < Names.Length; i++)
                {
                    var enemy = root.EnemyPopulation.GetSpot(i).GetLiveEnemy(0);
                    var original = enemy.transform.position; Move(enemy.transform, centre);
                    var binding = enemy.GetComponent<CharacterVisualBinding>();
                    var animator = binding.ActiveModel.GetComponentInChildren<Animator>();
                    animator.cullingMode = AnimatorCullingMode.AlwaysAnimate; animator.speed = 0;
                    var profile = Profile(binding);
                    for (var heading = 0; heading < 4; heading++)
                    {
                        enemy.transform.rotation = Quaternion.Euler(0, heading * 90, 0);
                        binding.ActiveModel.localPosition = Vector3.zero; binding.ActiveModel.localRotation = Quaternion.identity;
                        animator.Play(profile.Idle, 0, .3f); animator.Update(0); yield return null;
                        Capture(zone + "_" + Names[i] + "_turn_" + heading);
                    }
                    foreach (var state in new[] { profile.Move, profile.Attack, profile.Block, "Death" }.Distinct())
                    {
                        animator.Play(state, 0, .5f); animator.Update(0); yield return null;
                        Capture(zone + "_" + Names[i] + "_" + state);
                    }
                    animator.Play(profile.Idle, 0, .3f); animator.Update(0); yield return null;
                    var materials = binding.ActiveModel.GetComponentsInChildren<Renderer>(true).SelectMany(r => r.sharedMaterials).Distinct().ToArray();
                    var emission = materials.Select(m => m.GetColor("_EmissionColor")).ToArray();
                    try { foreach (var m in materials) m.SetColor("_EmissionColor", Color.black); Capture(zone + "_" + Names[i] + "_no_emission"); }
                    finally { for (var m = 0; m < materials.Length; m++) materials[m].SetColor("_EmissionColor", emission[m]); }
                    records.Add(Measure(Names[i], zone, binding, enemy.CollisionRadius));
                    Move(enemy.transform, original); animator.speed = 1;
                }
                Capture(zone + "_G0");
            }
            root.WorldUnlocks.PrepareEliteEncounterForDevelopment(); root.Chapter1Encounters.Tick();
            Move(root.PlayerObject.transform, root.MagnetarGuard.transform.position + Vector3.back * 4);
            root.MagnetarGuard.Tick(0);
            yield return GalleryEncounter("Magnetar", root.MagnetarGuard.transform, new[] { "Idle", "Run", "Attack", "Charge", "Vent", "Hit", "Death" }, records, root.MagnetarGuard.CollisionRadius);
            root.MagnetarGuard.ApplyDamage(new DamageRequest(100000, DamageType.Gravity));
            Move(root.PlayerObject.transform, root.CustodianBoss.transform.position + Vector3.back * 4.5f);
            root.CustodianBoss.Tick(0);
            yield return GalleryEncounter("Custodian", root.CustodianBoss.transform, new[] { "Idle", "Run", "Attack", "Windup", "Telegraph", "AttackLine", "AttackCircle", "AttackCone", "Death" }, records, root.CustodianBoss.CollisionRadius);
            Directory.CreateDirectory(Output + "/verification");
            File.WriteAllText(Output + "/verification/" + Stage + "-actor-envelope.json", JsonUtility.ToJson(new ActorViews { views = records.ToArray() }, true));
            }
        }
        [UnityTest, Timeout(180000)] public IEnumerator LiveAttackSourcesPalettesDeathAndFreshRespawnInColdAndWarmSectors()
        {
            yield return Load(); var root=_scene.Root;
            var bridge=root.GetComponent<VisualSliceAnimationBridge>();var motion=root.GetComponent<VisualSliceCombatMotion>();
            var causality=root.GetComponent<AttackCausalityPresenter>();var records=new List<string>();
            for(var sector=0;sector<2;sector++)
            {
                var centre=new Vector3(sector==0?-15:15,0,9);var zone=sector==0?"cold":"warm";
                for(var i=0;i<5;i++)
                {
                    var spot=root.EnemyPopulation.GetSpot(i);var enemy=spot.GetLiveEnemy(0);var life=enemy.LifeId;
                    Move(enemy.transform,centre);Move(root.PlayerObject.transform,centre+Vector3.back*.95f);
                    var binding=enemy.GetComponent<CharacterVisualBinding>();var before=causality.SourceCount;
                    for(var tick=0;tick<100 && causality.SourceCount==before;tick++)
                    {enemy.Tick(.05f);motion.Tick(.05f);bridge.Tick(.05f);yield return null;}
                    Assert.That(causality.SourceCount,Is.GreaterThan(before),zone+" "+Names[i]+" real attack");
                    var profile=Profile(binding);Assert.That(causality.LastMechanicalTravel,Is.EqualTo(!profile.ElectricalTravel));
                    if(i==3)Assert.That(causality.LastPresentedColor.b,Is.GreaterThan(causality.LastPresentedColor.r*3),"Approved Arc Drone blue");
                    else Assert.That(causality.LastPresentedColor.r,Is.GreaterThan(causality.LastPresentedColor.b*2));
                    var left=binding.GetSocketOr(PresentationSocket.WeaponLeft,enemy.TargetPoint).position;
                    var right=binding.GetSocketOr(PresentationSocket.WeaponRight,enemy.TargetPoint).position;
                    Assert.That(Mathf.Min(Vector3.Distance(causality.LastSource,left),Vector3.Distance(causality.LastSource,right)),Is.LessThan(.75f));
                    Assert.That(binding.ActiveModel.localPosition.magnitude,Is.LessThanOrEqualTo(.081f),"No large cosmetic wall translation");
                    Capture(zone+"_"+Names[i]+"_live_strike");
                    ActorConceptPerformanceSmokeTests.CaptureCoverage(UnityEngine.Camera.main,root.transform,zone+"_"+Names[i]+"_actual_strike");
                    for(var frame=0;frame<12;frame++)yield return null;
                    Capture(zone+"_"+Names[i]+"_live_impact");
                    while(spot.LiveCount>0)spot.GetLiveEnemy(0).ApplyDamage(new DamageRequest(100000,DamageType.Gravity));
                    bridge.Tick();yield return null;Capture(zone+"_"+Names[i]+"_actual_death");
                    Move(root.PlayerObject.transform,new Vector3(0,0,-30));
                    for(var tick=0;tick<12;tick++)root.EnemyPopulation.Tick(60);causality.Tick(0);
                    var fresh=spot.GetLiveEnemy(0);Assert.That(fresh.LifeId,Is.Not.EqualTo(life));
                    Move(fresh.transform,centre);Move(root.PlayerObject.transform,centre+Vector3.back*.95f);
                    before=causality.SourceCount;
                    for(var tick=0;tick<100&&causality.SourceCount==before;tick++){fresh.Tick(.05f);motion.Tick(.05f);bridge.Tick(.05f);yield return null;}
                    Assert.That(causality.SourceCount,Is.GreaterThan(before),"Fresh pooled-life event subscription");
                    Assert.IsFalse(fresh.GetComponent<CharacterVisualBinding>().ActiveModel.GetComponentInChildren<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Death"));
                    Capture(zone+"_"+Names[i]+"_respawn_strike");
                    records.Add(zone+" "+Names[i]+": attack, source, colour, bounded centre, death and fresh-life source PASS");
                }
            }
            Directory.CreateDirectory(Output+"/verification");File.WriteAllLines(Output+"/verification/live-combat.txt",records);
        }
        private IEnumerator GalleryEncounter(string name, Transform actor, string[] states, List<ActorView> records, float radius)
        {
            var binding = actor.GetComponent<CharacterVisualBinding>();
            var animator = binding.ActiveModel.GetComponentInChildren<Animator>(); animator.cullingMode = AnimatorCullingMode.AlwaysAnimate; animator.speed = 0;
            foreach (var state in states) { animator.Play(state, 0, .5f); animator.Update(0); yield return null; Capture(name + "_" + state); }
            records.Add(Measure(name, "arena", binding, radius)); animator.speed = 1;
        }
        [UnityTest,Timeout(90000)] public IEnumerator G0TurningActualLocomotionGravityAttackDeathAndRepairPreserveEvolutionAndCenter()
        {
            yield return Load();var root=_scene.Root;var player=root.PlayerObject;var body=player.GetComponent<CharacterController>();
            var view=player.GetComponent<PlayerEvolutionView>();var binding=player.GetComponent<CharacterVisualBinding>();
            var bridge=root.GetComponent<VisualSliceAnimationBridge>();var cosmetic=player.GetComponent<MechMotionPresenter>();
            var locomotion=player.GetComponent<PlayerLocomotion>();var input=new TestInput();locomotion.Initialize(input,UnityEngine.Camera.main.transform,root.PlayerStats,540);
            var records=new List<ActorView>();
            for(var zone=0;zone<2;zone++)
            {
                var centre=new Vector3(zone==0?-15:15,0,9);var label=zone==0?"cold":"warm";Move(player.transform,centre);
                bridge.enabled=false;cosmetic.enabled=false;
                var animator=binding.ActiveModel.GetComponentInChildren<Animator>();animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
                for(var heading=0;heading<4;heading++)
                {player.transform.rotation=Quaternion.Euler(0,heading*90,0);binding.ActiveModel.localRotation=Quaternion.identity;animator.Play("Idle",0,.25f);animator.Update(0);yield return null;Capture(label+"_G0_turn_"+heading);}
                bridge.enabled=true;cosmetic.enabled=true;
                var before=player.transform.position;input.Movement=Vector2.up;
                for(var tick=0;tick<12;tick++){locomotion.Step(.03f);bridge.Tick(.03f);yield return null;}
                input.Movement=Vector2.zero;Assert.That(Vector3.Distance(before,player.transform.position),Is.GreaterThan(.1f));Capture(label+"_G0_actual_move");
                records.Add(Measure("G0",label,binding,body.radius));
                Move(player.transform,centre);
                var enemy=root.EnemyPopulation.GetSpot(zone==0?3:1).GetLiveEnemy(0);Move(enemy.transform,player.transform.position+Vector3.forward*1.3f);
                enemy.enabled=true;
                foreach(var candidate in root.GetComponentsInChildren<OrdinaryEnemyController>(true))candidate.TargetPoint.GetComponent<Collider>().enabled=candidate==enemy;
                var attack=player.GetComponent<GravityAttackController>();var hp=enemy.CurrentHitPoints;attack.ResetTransientState();attack.Tick(0);attack.Tick(root.PlayerStats.DerivedStats.AttackInterval+.01f);
                bridge.Tick();yield return null;Capture(label+"_G0_actual_gravity");Assert.That(enemy.CurrentHitPoints,Is.LessThan(hp));
                var tier=view.CurrentTier;var power=root.PlayerStats.DerivedStats.BaseDamage;
                root.PlayerHealth.SetDevelopmentGodMode(false);root.PlayerHealth.ApplyDamage(new DamageRequest(100000,DamageType.Gravity));bridge.Tick();yield return null;Capture(label+"_G0_actual_death");
                root.PlayerHealth.Tick(10);bridge.Tick();yield return null;Capture(label+"_G0_repaired_respawn");
                Assert.IsTrue(root.PlayerHealth.IsAlive);Assert.That(view.CurrentTier,Is.EqualTo(tier));Assert.That(root.PlayerStats.DerivedStats.BaseDamage,Is.EqualTo(power));root.PlayerHealth.SetDevelopmentGodMode(true);
            }
            Directory.CreateDirectory(Output+"/verification");File.WriteAllText(Output+"/verification/g0-envelope.json",JsonUtility.ToJson(new ActorViews{views=records.ToArray()},true));
        }
        private static HostileMotionProfile Profile(CharacterVisualBinding binding)
        {
            var catalog = Resources.FindObjectsOfTypeAll<S15VisualCatalog>().First(c => c.HostilePresentation != null);
            return catalog.HostilePresentation.Get(binding.ActiveModel.GetComponentInChildren<Animator>().runtimeAnimatorController);
        }
        [UnityTest] public IEnumerator MovingSupportLayerKeepsG0AndEliteFeetActiveDuringUpperBodyActions()
        {
            yield return Load();var root=_scene.Root;var bridge=root.GetComponent<VisualSliceAnimationBridge>();
            var input=new TestInput{Movement=Vector2.up};var locomotion=root.PlayerObject.GetComponent<PlayerLocomotion>();locomotion.Initialize(input,UnityEngine.Camera.main.transform,root.PlayerStats,540);
            Move(root.PlayerObject.transform,new Vector3(15,0,8));
            var animator=root.PlayerObject.GetComponent<CharacterVisualBinding>().ActiveModel.GetComponentInChildren<Animator>();
            var support=animator.GetLayerIndex("Industrial moving supports");Assert.That(support,Is.GreaterThan(0));
            var target=root.EnemyPopulation.GetSpot(1).GetLiveEnemy(0);Move(target.transform,root.PlayerObject.transform.position+Vector3.forward*1.3f);
            target.enabled=true;
            foreach(var candidate in root.GetComponentsInChildren<OrdinaryEnemyController>(true))candidate.TargetPoint.GetComponent<Collider>().enabled=candidate==target;
            var attack=root.PlayerObject.GetComponent<GravityAttackController>();attack.ResetTransientState();attack.Tick(0);attack.Tick(root.PlayerStats.DerivedStats.AttackInterval+.01f);
            for(var frame=0;frame<12;frame++){locomotion.Step(.025f);bridge.Tick(.025f);animator.Update(.025f);yield return null;}
            Assert.That(animator.GetLayerWeight(support),Is.GreaterThan(.9f));Assert.That(animator.GetFloat("IndustrialGaitPhase"),Is.GreaterThan(.1f));
            root.WorldUnlocks.PrepareEliteEncounterForDevelopment();root.Chapter1Encounters.Tick();Move(root.PlayerObject.transform,root.MagnetarGuard.transform.position+Vector3.back*8);
            var eliteAnimator=root.MagnetarGuard.GetComponent<CharacterVisualBinding>().ActiveModel.GetComponentInChildren<Animator>();
            var prior=root.MagnetarGuard.transform.position;
            var ran=false;
            for(var frame=0;frame<8;frame++){root.MagnetarGuard.Tick(.1f);bridge.Tick(.1f);eliteAnimator.Update(.1f);ran|=eliteAnimator.GetCurrentAnimatorStateInfo(0).IsName("Run");yield return null;}
            Assert.That(Vector3.Distance(prior,root.MagnetarGuard.transform.position),Is.GreaterThan(.1f));
            Assert.IsTrue(eliteAnimator.GetCurrentAnimatorStateInfo(eliteAnimator.GetLayerIndex("Industrial moving supports")).IsName("Distance driven support gait"),"Moving legs must retain Run while upper body is attacking");
            Assert.That(eliteAnimator.GetFloat("IndustrialGaitPhase"),Is.GreaterThan(.1f));
            Assert.That(eliteAnimator.GetLayerWeight(eliteAnimator.GetLayerIndex("Industrial moving supports")),Is.GreaterThan(.9f));
        }
        private static ActorView Measure(string name, string sector, CharacterVisualBinding binding, float radius)
        {
            var skins=binding.ActiveModel.GetComponentsInChildren<LODGroup>(true).SelectMany(g=>g.GetLODs()[0].renderers).OfType<SkinnedMeshRenderer>().Distinct().ToArray();
            var min = Vector3.positiveInfinity; var max = Vector3.negativeInfinity; float envelope = 0;
            foreach(var skin in skins)
            {
                var mesh=new Mesh();skin.BakeMesh(mesh);
                foreach (var v in mesh.vertices) { var p = binding.transform.InverseTransformPoint(skin.transform.TransformPoint(v)); min = Vector3.Min(min, p); max = Vector3.Max(max, p); envelope = Mathf.Max(envelope, new Vector2(p.x, p.z).magnitude); }
                Object.Destroy(mesh);
            }
            var profile = Profile(binding);
            return new ActorView { actor = name, sector = sector, size = max - min, radialEnvelope = envelope, collisionRadius = radius, maximumCosmeticWallOffset = profile?.MaximumWallOffset??0, visualCenterOffset = binding.ActiveModel.localPosition };
        }
        private static void Move(Transform actor, Vector3 position) { var body = actor.GetComponent<CharacterController>(); if (body != null) body.enabled = false; actor.position = position; if (body != null) body.enabled = true; Physics.SyncTransforms(); }
        private void Capture(string name)
        {
            var root = _scene.Root; var camera = UnityEngine.Camera.main; camera.GetComponent<PortraitFollowCamera>().SnapToTarget();
            var canvases = root.GetComponentsInChildren<Canvas>(true); var enabled = canvases.Select(c => c.enabled).ToArray();
            var target = camera.targetTexture; var active = RenderTexture.active;
            var render = new RenderTexture(540, 960, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB); var texture = new Texture2D(540, 960, TextureFormat.RGB24, false);
            try { foreach (var c in canvases) c.enabled = false; camera.targetTexture = render; camera.Render(); RenderTexture.active = render; texture.ReadPixels(new Rect(0, 0, 540, 960), 0, 0); texture.Apply(); Directory.CreateDirectory(Output + "/" + Stage); File.WriteAllBytes(Output + "/" + Stage + "/" + name + ".png", texture.EncodeToPNG()); }
            finally { for (var i = 0; i < canvases.Length; i++) canvases[i].enabled = enabled[i]; camera.targetTexture = target; RenderTexture.active = active; render.Release(); Object.Destroy(render); Object.Destroy(texture); }
        }
        [Serializable] private sealed class ActorViews { public ActorView[] views; }
        private sealed class TestInput:IMovementInput{public Vector2 Movement{get;set;}}
        [Serializable] private sealed class ActorView { public string actor, sector; public Vector3 size, visualCenterOffset; public float radialEnvelope, collisionRadius, maximumCosmeticWallOffset; }
    }
}
