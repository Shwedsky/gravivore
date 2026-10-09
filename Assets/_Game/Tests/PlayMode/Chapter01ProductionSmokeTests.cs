using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Gravivore.Gameplay.Combat;
using Gravivore.Gameplay.Enemies;
using Gravivore.Gameplay.Player;
using Gravivore.Gameplay.Progression;
using Gravivore.Presentation.Assets;
using Gravivore.Presentation.Camera;
using Gravivore.Presentation.Composition;
using Gravivore.Presentation.Feedback;
using Gravivore.Presentation.Map;
using Gravivore.Presentation.Player;
using Gravivore.Presentation.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils;
using UnityEngine.UI;
using Object=UnityEngine.Object;

namespace Gravivore.Tests.PlayMode
{
    public sealed class Chapter01ProductionSmokeTests
    {
        private CanonicalSceneTestScope _scene;
        [UnityTearDown] public IEnumerator Cleanup() {if(_scene!=null)yield return _scene.Cleanup();}
        private IEnumerator Load()
        {_scene=new CanonicalSceneTestScope();yield return _scene.Load();Control(_scene.Root);}
        private static void Control(S01SceneCompositionRoot root)
        {
            root.EnemyPopulation.enabled=false;root.MagnetarGuard.enabled=false;root.CustodianBoss.enabled=false;
            root.PlayerObject.GetComponent<GravityAttackController>().enabled=false;root.PlayerObject.GetComponent<PlayerLocomotion>().enabled=false;
            foreach(var enemy in root.GetComponentsInChildren<OrdinaryEnemyController>(true))enemy.enabled=false;
        }
        private static T Field<T>(object owner,string name)=>(T)owner.GetType().GetField(name,BindingFlags.NonPublic|BindingFlags.Instance).GetValue(owner);
        private static void SetField(object owner,string name,object value)=>owner.GetType().GetField(name,BindingFlags.NonPublic|BindingFlags.Instance).SetValue(owner,value);
        private static void Move(S01SceneCompositionRoot root,Vector3 point)
        {var body=root.PlayerObject.GetComponent<CharacterController>();body.enabled=false;body.transform.position=point;body.enabled=true;Physics.SyncTransforms();}

        [UnityTest] public IEnumerator CustodianActualHudFullHalfLowResetRepeatDeathAndNoOrdinaryPlate()
        {
            yield return Load();var root=_scene.Root;
            root.WorldUnlocks.PrepareEliteEncounterForDevelopment();root.Chapter1Encounters.Tick();
            root.MagnetarGuard.ApplyDamage(new DamageRequest(100000,DamageType.Gravity));
            Move(root,root.CustodianBoss.transform.position+Vector3.back*4.5f);
            var boss=root.CustodianBoss;boss.Tick(0);
            var hud=root.BossHealthHud;Assert.IsTrue(hud.IsVisible);Assert.That(hud.FillAmount,Is.EqualTo(1));
            Canvas.ForceUpdateCanvases();Assert.That(hud.FillRect.rect.width,Is.GreaterThan(100));
            Assert.That(hud.FillRect.GetComponent<Image>().color.a,Is.EqualTo(1));
            var config=Field<Gravivore.Gameplay.Encounters.CustodianBossConfiguration>(boss,"_configuration");
            boss.ApplyDamage(new DamageRequest(boss.MaximumHitPoints*.5f*(1+config.Armor/100f),DamageType.Gravity));
            Assert.That(hud.FillAmount,Is.EqualTo(.5f).Within(.001f));
            boss.ApplyDamage(new DamageRequest(boss.MaximumHitPoints*.45f*(1+config.Armor/100f),DamageType.Gravity));
            Assert.That(hud.FillAmount,Is.EqualTo(.05f).Within(.001f));
            Assert.IsTrue(boss.ResetEncounter());Assert.IsFalse(hud.IsVisible);Assert.That(hud.FillAmount,Is.EqualTo(1));
            boss.Tick(0);Assert.IsTrue(hud.IsVisible);
            boss.ApplyDamage(new DamageRequest(100000,DamageType.Gravity));Assert.IsFalse(hud.IsVisible);
            root.ChapterCompletion.ContinueExploring();
            _scene.AdvanceTime(TimeSpan.FromMinutes(30));root.Chapter1Encounters.Tick();boss.Tick(0);
            Assert.IsTrue(root.BossCompletion.IsDefeated);Assert.IsTrue(hud.IsVisible);Assert.That(hud.FillAmount,Is.EqualTo(1));
            var actors=(Array)root.CombatReadability.GetType().GetField("_actors",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(root.CombatReadability);
            foreach(var actor in actors)Assert.That(actor.GetType().GetField("Root").GetValue(actor),Is.Not.SameAs(boss.transform));
            Capture(root,"boss_full");boss.ApplyDamage(new DamageRequest(100000,DamageType.Gravity));Assert.IsFalse(hud.IsVisible);
        }

        [UnityTest] public IEnumerator CharacteristicsLiveRewardLevelEquipmentAndSaveReloadUseAuthoritativeState()
        {
            yield return Load();var root=_scene.Root;var ui=root.PauseMenu.Characteristics;
            root.PauseMenu.Open();ui.Open();Assert.IsTrue(ui.IsVisible);
            var version=ui.Revision;var spot=root.EnemyPopulation.GetSpot(0);var enemy=spot.GetLiveEnemy(0);
            root.Progression.TryPreview(spot.EnemyId,spot.RewardMultiplier,out var preview);
            enemy.ApplyDamage(new DamageRequest(100000,DamageType.Gravity));Assert.That(ui.Revision,Is.GreaterThan(version));
            foreach(PlayerStatType stat in Enum.GetValues(typeof(PlayerStatType)))
            {var read=ui.Model.Read(stat);Assert.That(read.Level,Is.EqualTo(root.PlayerStats.BaseLevels.GetLevel(stat)));
                Assert.That(read.Experience,Is.EqualTo(root.Progression.State.GetStatExperience(stat)));
                Assert.That(read.Required,Is.EqualTo(root.Progression.Configuration.ThresholdCurve.Evaluate(read.Level)));
                Assert.IsTrue(read.Current.HasSameValues(root.PlayerStats.DerivedStats));Assert.IsTrue(read.Next.HasSameValues(root.PlayerStats.PreviewLevel(stat,read.Level+1)));}
            Assert.That(root.CombatReadability.LastGrantedReward.StatExperience,Is.EqualTo(preview.StatExperience));
            var power=root.PlayerStats.BaseLevels.Power;root.PlayerStats.SetLevel(PlayerStatType.Power,power+1);
            StringAssert.Contains("↑",root.PlayerStatsHud.RecentChangeText);
            var item=root.EquipmentCatalog.GetAt(0);version=ui.Revision;root.Equipment.GrantEquipment(item.Id);root.Equipment.Equip(item.Id,item.Slot);
            Assert.That(ui.Revision,Is.GreaterThan(version));Assert.IsTrue(ui.Model.Read(PlayerStatType.Power).Current.HasSameValues(root.PlayerStats.DerivedStats));
            ui.Select(PlayerStatType.Flux);AssertRussianAndFits(ui.Panel);ui.Close();ui.Open();Canvas.ForceUpdateCanvases();AssertRussianAndFits(ui.Panel);
            Capture(root,"characteristics");ui.Close();root.PauseMenu.Resume();
            Assert.IsTrue(root.FlushNow());var xp=root.Progression.State.GetStatExperience(preview.Stat);var values=root.PlayerStats.DerivedStats;
            yield return _scene.Load();root=_scene.Root;Control(root);ui=root.PauseMenu.Characteristics;
            Assert.That(ui.Model.Read(preview.Stat).Experience,Is.EqualTo(xp));Assert.IsTrue(ui.Model.Read(preview.Stat).Current.HasSameValues(values));
            root.PauseMenu.Open();ui.Open();AssertRussianAndFits(ui.Panel);ui.Close();root.PauseMenu.Resume();
        }
        private static void AssertRussianAndFits(RectTransform panel)
        {
            Canvas.ForceUpdateCanvases();
            foreach(var text in panel.GetComponentsInChildren<Text>())
            {Assert.IsFalse(Regex.IsMatch(text.text,"[A-Za-z]"),text.name+": "+text.text);
                Assert.That(text.preferredHeight,Is.LessThanOrEqualTo(text.rectTransform.rect.height+1),text.name+" needs enough vertical space.");}
        }

        [UnityTest] public IEnumerator CompleteWorldLockedAndUnlockedCapsuleRoutesAndActualControllerTraversal()
        {
            yield return Load();var root=_scene.Root;var body=root.PlayerObject.GetComponent<CharacterController>();
            foreach(var controller in root.GetComponentsInChildren<CharacterController>(true))if(controller!=body)controller.enabled=false;
            Physics.SyncTransforms();var start=body.transform.position;
            var locked=new RouteGrid(root,start);
            foreach(var definition in Field<SpawnSpotDefinition[]>(root,"_spawnSpotDefinitions"))
            {var config=definition.CreateRuntimeConfiguration();Assert.IsTrue(locked.Reaches(config.WorldOrigin),config.Id);
                foreach(var anchor in config.AnchorOffsets)Assert.IsTrue(locked.Reaches(config.WorldOrigin+anchor),config.Id+" spawn "+anchor);}
            var world=root.WorldPresenter.Configuration;
            foreach(var spot in root.StrongSpots)Assert.That(locked.Reaches(spot.Position),Is.EqualTo(spot.Position.z<world.EliteGate.Position.z),"Locked: "+spot.Id);
            Assert.IsTrue(locked.Reaches(world.EliteGate.Position+Vector3.back*2));Assert.IsFalse(locked.Reaches(root.MagnetarGuard.transform.position));
            Assert.IsFalse(locked.Reaches(world.BossGate.Position+Vector3.back*2));Assert.IsFalse(locked.Reaches(root.CustodianBoss.transform.position));
            root.WorldUnlocks.PrepareEliteEncounterForDevelopment();root.Chapter1Encounters.Tick();
            root.MagnetarGuard.ApplyDamage(new DamageRequest(100000,DamageType.Gravity));Physics.SyncTransforms();
            var opened=new RouteGrid(root,start);var targets=new List<(string id,Vector3 point)>();
            for(var i=0;i<root.WorldPresenter.ZoneCount;i++)targets.Add(("zone "+i,root.WorldPresenter.GetZoneCenter(i)));
            foreach(var definition in Field<SpawnSpotDefinition[]>(root,"_spawnSpotDefinitions"))
            {var config=definition.CreateRuntimeConfiguration();foreach(var anchor in config.AnchorOffsets)targets.Add((config.Id+" anchor "+anchor,config.WorldOrigin+anchor));}
            foreach(var spot in root.StrongSpots)targets.Add((spot.Id,spot.Position));
            targets.Add(("elite approach",world.EliteGate.Position+Vector3.back*2));targets.Add(("Magnetar",root.MagnetarGuard.transform.position));
            targets.Add(("boss gate approach",world.BossGate.Position+Vector3.back*2));targets.Add(("Custodian arena",root.CustodianBoss.transform.position));
            foreach(var target in targets)
            {
                Assert.IsTrue(opened.Reaches(target.point),target.id+" must have a capsule route.");
                Move(root,start);foreach(var point in opened.Path(target.point))Walk(body,point,target.id);Walk(body,target.point,target.id);
                Assert.That(Vector3.Distance(body.transform.position,target.point),Is.LessThan(.2f),target.id+" actual controller");
            }
            Directory.CreateDirectory("docs/history/visual-stages/chapter01-visual-passes/chapter01-production/verification");
            File.WriteAllLines("docs/history/visual-stages/chapter01-visual-passes/chapter01-production/verification/reachability.txt",targets.Select(t=>t.id+" => "+t.point+" capsule + CharacterController PASS"));
        }
        private static void Walk(CharacterController body,Vector3 point,string label)
        {for(var i=0;i<40;i++){var delta=point-body.transform.position;delta.y=0;if(delta.magnitude<.08f)return;body.Move(Vector3.ClampMagnitude(delta,.16f));Physics.SyncTransforms();}
            Assert.Fail(label+" controller blocked at "+body.transform.position+" towards "+point);}
        internal sealed class RouteGrid
        {
            private const float Spacing=.5f;private readonly int _width,_height;private readonly Vector3 _origin;private readonly int[] _parents;private readonly int _start;
            public RouteGrid(S01SceneCompositionRoot root,Vector3 start)
            {
                var bounds=root.WorldPresenter.Bounds;_width=Mathf.RoundToInt(bounds.Size.x/Spacing)-1;_height=Mathf.RoundToInt(bounds.Size.y/Spacing)-1;
                _origin=new Vector3(bounds.MinX+Spacing,0,bounds.MinZ+Spacing);_parents=Enumerable.Repeat(-1,_width*_height).ToArray();
                _start=Cell(start);_parents[_start]=_start;var queue=new Queue<int>();queue.Enqueue(_start);
                var radius=root.PlayerObject.GetComponent<CharacterController>().radius+.06f;var mask=LayerMask.GetMask("HardBlocker");
                while(queue.Count>0)
                {
                    var current=queue.Dequeue();var x=current%_width;var z=current/_width;
                    foreach(var offset in new[]{-1,1,-_width,_width})
                    {
                        var next=current+offset;if(next<0||next>=_parents.Length||offset==-1&&x==0||offset==1&&x==_width-1||offset==-_width&&z==0||offset==_width&&z==_height-1||_parents[next]!=-1)continue;
                        var a=Point(current);var b=Point(next);
                        if(Physics.CheckCapsule(b+Vector3.up*.5f,b+Vector3.up*.9f,radius,mask,QueryTriggerInteraction.Ignore)||
                            Physics.CapsuleCast(a+Vector3.up*.5f,a+Vector3.up*.9f,radius,(b-a).normalized,Spacing,mask,QueryTriggerInteraction.Ignore))continue;
                        _parents[next]=current;queue.Enqueue(next);
                    }
                }
            }
            private int Cell(Vector3 p)=>Mathf.RoundToInt((p.z-_origin.z)/Spacing)*_width+Mathf.RoundToInt((p.x-_origin.x)/Spacing);
            private Vector3 Point(int i)=>_origin+new Vector3(i%_width*Spacing,0,i/_width*Spacing);
            public bool Reaches(Vector3 point){var i=Cell(point);return i>=0&&i<_parents.Length&&_parents[i]!=-1;}
            public List<Vector3> Path(Vector3 point){var list=new List<Vector3>();var i=Cell(point);while(i!=_start){list.Add(Point(i));i=_parents[i];}list.Reverse();return list;}
        }

        [UnityTest] public IEnumerator ProductionArtIsLiveSolidPropsHaveMatchingGameplayProxiesAndFullMapUsesAuthority()
        {
            yield return Load();var root=_scene.Root;var env=root.VisualEnvironment;Assert.IsTrue(env.FullChapterProduction);
            Physics.SyncTransforms();
            if(env.BlueprintWorldOnly)
            {
                foreach(var id in new[]{"repair-hub","relay-yard","capacitor-field","cutting-floor","shield-dump","hauler-graveyard","elite-arena","boss-arena"})
                    Assert.IsNotEmpty(env.GetRegion(id).Root.GetComponentsInChildren<Renderer>(),id);
                var layout=root.WorldPresenter.Layout;Assert.NotNull(layout);
                Assert.That(root.WorldPresenter.EnvironmentBlockerCount,Is.EqualTo(layout.BlockerCount));
                for(var i=0;i<layout.BlockerCount;i++)
                {
                    var volume=layout.GetBlocker(i);var proxy=root.WorldPresenter.GetEnvironmentBlocker(i);
                    Assert.That(Vector3.Distance(proxy.bounds.center,volume.Center),Is.LessThan(.001f),volume.Id);
                    Assert.That(Vector3.Distance(proxy.bounds.size,volume.Size),Is.LessThan(.001f),volume.Id);
                    Assert.IsNull(proxy.GetComponent<Renderer>(),volume.Id);
                }
                Assert.IsEmpty(env.GetComponentsInChildren<Collider>(true));
                Assert.IsNull(env.Floor.Find("Chapter 01 Full Production"));
            }
            else
            {
            var production=env.Floor.Find("Chapter 01 Full Production");Assert.IsNotNull(production);
            foreach(var id in new[]{"relay-yard","cutting-floor","shield-dump","capacitor-field","hauler-graveyard"})Assert.IsNotNull(production.Find(id));
            Assert.IsNotNull(production.Find("Service corridors"));Assert.IsNotNull(production.Find("Custodian containment complex"));
            var walls=env.Floor.GetComponentsInChildren<Renderer>().Where(r=>r.name.StartsWith("Bulkhead_Module",StringComparison.Ordinal)).ToArray();
            bool Covered(Vector3 p)=>walls.Any(r=>{var bounds=r.bounds;bounds.Expand(.12f);return bounds.Contains(p);});
            var world=root.WorldPresenter.Configuration;var bounds=world.Bounds;
            foreach(var side in new[]{-1,1})
            {
                foreach(var z in new[]{world.EliteGate.Position.z,world.BossGate.Position.z})for(var x=3.5f;x<=bounds.MaxX-.5f;x+=.5f)
                    Assert.IsTrue(Covered(new Vector3(side*x,1.3f,z)),"Opaque gate flank visual at "+side*x+", "+z);
                for(var z=bounds.MinZ+4.5f;z<=bounds.MaxZ-.5f;z+=.5f)
                    Assert.IsTrue(Covered(new Vector3(side*(bounds.MaxX-.5f),1.3f,z)),"Opaque perimeter visual at "+side+", "+z);
            }
            for(var x=bounds.MinX+.5f;x<=bounds.MaxX-.5f;x+=.5f)Assert.IsTrue(Covered(new Vector3(x,1.3f,bounds.MaxZ)),"North perimeter visual at "+x);
            }
            foreach(var enemy in root.EnemyPopulation.GetComponentsInChildren<OrdinaryEnemyController>(true))
            {var binding=enemy.GetComponent<CharacterVisualBinding>();if(binding.ActiveModel==null)continue;
                Assert.IsNotNull(binding.ActiveModel.GetComponentInChildren<Animator>(),enemy.LifeId.ToString());}
            StringAssert.StartsWith("Custodian_V3",root.CustodianBoss.GetComponent<CharacterVisualBinding>().ActiveModel.name);
            for(var i=0;i<env.SliceObstacleCount;i++)
            {var obstacle=env.GetSliceObstacle(i);if(!obstacle.Name.StartsWith("Chapter01 "))continue;
                var proxy=Enumerable.Range(0,root.WorldPresenter.EnvironmentBlockerCount).Select(root.WorldPresenter.GetEnvironmentBlocker).Single(c=>c.name==obstacle.Name);
                Assert.IsTrue(proxy.enabled && proxy.gameObject.activeInHierarchy,obstacle.Name);
                Assert.That(Vector3.Distance(proxy.bounds.center,obstacle.Center),Is.LessThan(.001f),obstacle.Name);
                Assert.That(Vector3.Distance(proxy.bounds.size,obstacle.Size),Is.LessThan(.001f),obstacle.Name);}
            var map=root.MapIntegration.MapPresenter;map.OpenExpanded();map.RefreshNow();
            for(var i=0;i<root.MapMarkers.Count;i++){var marker=root.MapMarkers.GetMarker(i);Assert.That(map.GetCachedViewIdentity(marker.Id,true),Is.Not.Zero,marker.Id);
                if(marker.Visible && marker.Kind!=MapMarkerKind.Player)Assert.IsTrue(map.SelectMarker(marker.Id),marker.Id);}
            map.ClearSelection();Capture(root,"full_map");map.CloseExpanded();
            foreach(var pair in new[]{("relay",new Vector3(-26,0,20)),("cutting",new Vector3(0,0,40)),("shield",new Vector3(26,0,20)),("capacitor",new Vector3(-20,0,-12)),("hauler",new Vector3(20,0,-12)),("traversal",new Vector3(0,0,10)),("boss_approach",new Vector3(0,0,84))})
            {Move(root,pair.Item2);Capture(root,pair.Item1);}
        }
        [UnityTest] public IEnumerator SustainedRunStepsAreQuieterAndLessFrequentThanV38WithIndependentCombatGain()
        {
            yield return Load();var root=_scene.Root;var motion=root.PlayerObject.GetComponent<MechMotionPresenter>();motion.enabled=false;
            var tuned=Field<S14PresentationDefinition>(motion,"_settings");var baseline=Object.Instantiate(tuned);
            SetField(baseline,"_stepMinimumInterval",.28f);SetField(baseline,"_stepVolume",.24f);
            var audio=root.AudioPresenter;var muted=audio.IsMuted;var volume=audio.Volume;var position=root.PlayerObject.transform.position;
            try
            {
                audio.SetMuted(false);audio.SetVolume(.8f);
                var counts=new List<string>();
                foreach(var speed in new[]{4.5f,6f,7.5f})
                {
                    var oldCount=RunSteps(motion,root.PlayerObject.transform,baseline,1.8f,speed);
                    var newCount=RunSteps(motion,root.PlayerObject.transform,tuned,2.25f,speed);
                    Assert.That((float)newCount/oldCount,Is.InRange(.70f,.85f),"Sustained run at "+speed+" m/s");
                    counts.Add(speed+" m/s: v38="+oldCount+", production="+newCount+", ratio="+((float)newCount/oldCount));
                }
                var sources=Field<AudioSource[]>(audio,"_sources");
                audio.Play(S14AudioCue.Step);Assert.That(sources.Last().volume,Is.EqualTo(.8f*tuned.StepVolume).Within(.0001f));
                audio.Play(S14AudioCue.Telegraph);Assert.That(sources.Any(s=>Mathf.Abs(s.volume-.8f*.65f)<.0001f),Is.True);
                audio.Play(S14AudioCue.LashImpact);Assert.That(sources.Any(s=>Mathf.Abs(s.volume-.8f*.45f)<.0001f),Is.True);
                audio.SetVolume(.5f);Assert.That(sources.Last().volume,Is.EqualTo(.5f*tuned.StepVolume).Within(.0001f));
                Assert.That(sources.Length,Is.EqualTo(4));Directory.CreateDirectory("docs/history/visual-stages/chapter01-visual-passes/chapter01-production/verification");
                File.WriteAllLines("docs/history/visual-stages/chapter01-visual-passes/chapter01-production/verification/footsteps.txt",counts);
            }
            finally{SetField(motion,"_settings",tuned);motion.ConfigureStride(2.25f);root.PlayerObject.transform.position=position;audio.SetMuted(muted);audio.SetVolume(volume);Object.Destroy(baseline);}
        }
        private static int RunSteps(MechMotionPresenter motion,Transform authority,S14PresentationDefinition definition,float stride,float speed)
        {
            SetField(motion,"_settings",definition);motion.ConfigureStride(stride);SetField(motion,"_phase",0f);SetField(motion,"_time",0f);
            SetField(motion,"_stepIndex",0);SetField(motion,"_nextStepAudioTime",0f);SetField(motion,"_previousPosition",authority.position);
            var before=motion.FootstepCueCount;
            for(var i=0;i<3600;i++){authority.position+=Vector3.forward*(speed/60f);motion.Tick(1f/60f);}
            return motion.FootstepCueCount-before;
        }
        private static void Capture(S01SceneCompositionRoot root,string name)
        {
            root.MapIntegration.MapPresenter.RefreshNow();
            var camera=UnityEngine.Camera.main;camera.GetComponent<PortraitFollowCamera>().SnapToTarget();
            root.CombatReadability.Tick(.016f);
            var canvas=root.GetComponentInChildren<Canvas>();var mode=canvas.renderMode;var world=canvas.worldCamera;var prior=camera.targetTexture;var active=RenderTexture.active;
            var render=new RenderTexture(540,960,24);var texture=new Texture2D(540,960,TextureFormat.RGB24,false);
            try{canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;camera.targetTexture=render;Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=render;
                texture.ReadPixels(new Rect(0,0,540,960),0,0);texture.Apply();Directory.CreateDirectory("docs/history/visual-stages/chapter01-visual-passes/chapter01-production/internal");File.WriteAllBytes("docs/history/visual-stages/chapter01-visual-passes/chapter01-production/internal/"+name+".png",texture.EncodeToPNG());}
            finally{canvas.renderMode=mode;canvas.worldCamera=world;camera.targetTexture=prior;RenderTexture.active=active;render.Release();Object.Destroy(render);Object.Destroy(texture);}
        }
    }
}
