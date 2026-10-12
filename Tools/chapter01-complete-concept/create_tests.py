from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
original=(ROOT/'Assets/_Game/Tests/PlayMode/ConnectedConceptSectorsSmokeTests.cs').read_text(encoding='utf-8')
imports=original[:original.index('    public sealed class')]
helpers=original[original.index('        private void Move('):original.index('        private sealed class RouteInput')]
body=r'''
    public sealed class Chapter01CompleteConceptSmokeTests
    {
        private const string Output="docs/history/implementation-passes/chapter01-complete-concept";
        private CanonicalSceneTestScope _scene;
        [UnityTearDown] public IEnumerator Cleanup(){if(_scene!=null)yield return _scene.Cleanup();}
        private IEnumerator Load(){_scene=new CanonicalSceneTestScope();yield return _scene.Load();Assert.NotNull(_scene.Root.VisualEnvironment.Floor.Find("Chapter 01 connected industrial service spine"));}
        private static readonly string[] Ids={"repair-hub","relay-yard","capacitor-field","cutting-floor","shield-dump","hauler-graveyard","elite-arena","boss-arena"};
        private static readonly Vector3[] Positions={
            new Vector3(0,0,-34.5f),new Vector3(0,0,-30),new Vector3(0,0,-27),
            new Vector3(0,0,-14),new Vector3(0,0,-10),new Vector3(0,0,-6.3f),
            new Vector3(-8,0,10),new Vector3(-15,0,10),new Vector3(-15,0,12.9f),
            new Vector3(8,0,10),new Vector3(15,0,10),new Vector3(15,0,12.8f),
            new Vector3(-8,0,32),new Vector3(-15,0,32),new Vector3(-15,0,34.8f),
            new Vector3(8,0,32),new Vector3(15,0,32),new Vector3(15,0,35.5f),
            new Vector3(0,0,48),new Vector3(0,0,51.5f),new Vector3(-5,0,53.8f),
            new Vector3(0,0,70),new Vector3(0,0,73),new Vector3(6.5f,0,80)};

        [UnityTest] public IEnumerator EightFunctionalPlacesAtEntryCentreAndMachineWithUiCombatAndSilhouetteEvidence()
        {
            yield return Load();var root=_scene.Root;root.PlayerHealth.SetDevelopmentGodMode(true);
            root.PlayerObject.GetComponent<PlayerLocomotion>().enabled=false;
            var records=new List<Visibility>();
            for(var i=0;i<Positions.Length;i++)
            {
                if(i==18){root.WorldUnlocks.PrepareEliteEncounterForDevelopment();root.Chapter1Encounters.Tick();}
                if(i==21)root.MagnetarGuard.ApplyDamage(new DamageRequest(100000,DamageType.Gravity));
                Assert.IsTrue(root.WorldPresenter.Layout.IsClear(Positions[i],.65f),"Review position obstructed: "+i);
                Move(Positions[i]);yield return new WaitForSeconds(1);
                UnityEngine.Camera.main.GetComponent<PortraitFollowCamera>().SnapToTarget();
                var name=(i/3+1).ToString("D2")+"_"+Ids[i/3]+"_"+new[]{"entry","centre","machine"}[i%3];
                Capture(name,true);Capture(name,false);records.Add(MeasureVisibility(name));
            }
            foreach(var z in new[]{-19.5f,3f,21f,32f,43f,65})
            {Move(new Vector3(0,0,z));yield return null;Capture("link_"+z.ToString(System.Globalization.CultureInfo.InvariantCulture),true);Capture("link_"+z.ToString(System.Globalization.CultureInfo.InvariantCulture),false);}
            root.MapIntegration.MapPresenter.OpenExpanded();Capture("chapter_minimap",true);root.MapIntegration.MapPresenter.CloseExpanded();
            Directory.CreateDirectory(Output+"/verification");File.WriteAllText(Output+"/verification/player-silhouette.json",JsonUtility.ToJson(new VisibilityReport{samples=records.ToArray()},true));
            foreach(var sample in records)Assert.That(sample.fraction,Is.GreaterThanOrEqualTo(.72f),sample.position+" architecture hides the player silhouette");
        }
        private Visibility MeasureVisibility(string position)
        {
            var root=_scene.Root;var camera=UnityEngine.Camera.main;var post=camera.GetUniversalAdditionalCameraData();var pp=post.renderPostProcessing;
            var flags=camera.clearFlags;var bg=camera.backgroundColor;
            var renderers=Object.FindObjectsByType<Renderer>(FindObjectsInactive.Exclude,FindObjectsSortMode.None);
            var states=renderers.Select(r=>r.enabled).ToArray();var materials=renderers.Select(r=>r.sharedMaterials).ToArray();
            var white=new Material(Shader.Find("Universal Render Pipeline/Unlit"));white.SetColor("_BaseColor",Color.white);
            var black=new Material(Shader.Find("Universal Render Pipeline/Unlit"));black.SetColor("_BaseColor",Color.black);
            var player=renderers.Select(r=>r.transform.IsChildOf(root.PlayerObject.transform)&&(r is MeshRenderer||r is SkinnedMeshRenderer)).ToArray();
            var env=renderers.Select(r=>r.transform.IsChildOf(root.VisualEnvironment.Floor)).ToArray();
            int Count(byte[] png){var texture=new Texture2D(2,2);try{texture.LoadImage(png);return texture.GetPixels32().Count(p=>p.r>220&&p.g>220&&p.b>220);}finally{Object.Destroy(texture);}}
            try
            {
                post.renderPostProcessing=false;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.black;
                for(var i=0;i<renderers.Length;i++){renderers[i].sharedMaterials=Enumerable.Repeat(player[i]?white:black,materials[i].Length).ToArray();renderers[i].enabled=states[i]&&player[i];}
                var full=Count(Capture("silhouette_"+position+"_uncovered",false));
                for(var i=0;i<renderers.Length;i++)renderers[i].enabled=states[i]&&(player[i]||env[i]);
                var visible=Count(Capture("silhouette_"+position+"_architecture",false));
                Assert.That(full,Is.GreaterThan(150),"Player must occupy readable portrait pixels");
                return new Visibility{position=position,uncoveredPixels=full,visiblePixels=visible,fraction=(float)visible/full};
            }
            finally
            {
                for(var i=0;i<renderers.Length;i++){renderers[i].sharedMaterials=materials[i];renderers[i].enabled=states[i];}
                post.renderPostProcessing=pp;camera.clearFlags=flags;camera.backgroundColor=bg;Object.Destroy(white);Object.Destroy(black);
            }
        }
        [UnityTest] public IEnumerator ActualLocomotionTraversesWholeChapterAndMachineApproachesWithOneMinimapAuthority()
        {
            yield return Load();var root=_scene.Root;root.PlayerHealth.SetDevelopmentGodMode(true);
            root.WorldUnlocks.PrepareEliteEncounterForDevelopment();root.Chapter1Encounters.Tick();root.MagnetarGuard.ApplyDamage(new DamageRequest(100000,DamageType.Gravity));
            root.PlayerObject.GetComponent<GravityAttackController>().enabled=false;
            var locomotion=root.PlayerObject.GetComponent<PlayerLocomotion>();locomotion.enabled=false;
            var input=new RouteInput();locomotion.Initialize(input,UnityEngine.Camera.main.transform,root.PlayerStats,540);
            var body=root.PlayerObject.GetComponent<CharacterController>();
            foreach(var occupant in root.GetComponentsInChildren<CharacterController>(true))if(occupant!=body)occupant.enabled=false;
            foreach(var enemy in root.GetComponentsInChildren<OrdinaryEnemyController>(true))enemy.enabled=false;
            root.MagnetarGuard.enabled=false;root.CustodianBoss.enabled=false;
            var layout=root.WorldPresenter.Layout;var route=Enumerable.Range(0,layout.RoutePointCount).Select(layout.GetRoutePoint).ToArray();
            var records=new List<string>();Move(route[0]);
            for(var i=1;i<route.Length;i++)
            {
                for(var j=0;j<650;j++){var delta=route[i]-body.transform.position;delta.y=0;if(delta.magnitude<.14f)break;Steer(input,delta);locomotion.Step(.035f);Physics.SyncTransforms();if(j%25==0)yield return null;}
                Assert.That(Vector3.Distance(body.transform.position,route[i]),Is.LessThan(.2f),"Chapter segment "+i+" from "+body.transform.position);
                Assert.That(body.transform.position.y,Is.InRange(-.1f,.15f));records.Add("actual PlayerLocomotion segment "+i+" PASS "+route[i]);
            }
            for(var i=0;i<Positions.Length;i+=3)
            {
                Move(Positions[i+1]);var target=Positions[i+2];
                for(var j=0;j<300;j++){var delta=target-body.transform.position;delta.y=0;if(delta.magnitude<.14f)break;Steer(input,delta);locomotion.Step(.035f);Physics.SyncTransforms();if(j%25==0)yield return null;}
                Assert.That(Vector3.Distance(body.transform.position,target),Is.LessThan(.2f),"Machine approach "+Ids[i/3]);records.Add(Ids[i/3]+" machine approach PASS");
            }
            input.Movement=Vector2.zero;var topology=root.MapIntegration.MapPresenter.Topology;
            Assert.That(topology.SurfaceCount,Is.EqualTo(layout.SurfaceCount));Assert.That(topology.BlockerCount,Is.EqualTo(root.WorldPresenter.GameplayRoot.GetComponentsInChildren<BoxCollider>(true).Count(c=>c.gameObject.layer==LayerMask.NameToLayer("HardBlocker"))));
            Directory.CreateDirectory(Output+"/verification");File.WriteAllLines(Output+"/verification/chapter-traversal.txt",records);
        }
        [UnityTest] public IEnumerator FirstAndRepeatedEncountersKeepRewardsCooldownAndReloadAcrossTheNewArchitecture()
        {
            yield return Load();var root=_scene.Root;root.PlayerHealth.SetDevelopmentGodMode(true);root.PlayerObject.GetComponent<PlayerLocomotion>().enabled=false;root.PlayerObject.GetComponent<GravityAttackController>().enabled=false;
            root.WorldUnlocks.PrepareEliteEncounterForDevelopment();root.Chapter1Encounters.Tick();
            Move(new Vector3(0,0,51.5f));yield return new WaitForSeconds(5);
            Assert.That(root.Phase6BCombat.EliteAttackCount,Is.GreaterThan(0));
            root.MagnetarGuard.ApplyDamage(new DamageRequest(100000,DamageType.Gravity));Assert.IsTrue(root.MapMarkers.GetMarker(10).FirstClearCompleted);
            _scene.AdvanceTime(TimeSpan.FromMinutes(15));root.Chapter1Encounters.Tick();Assert.IsTrue(root.MagnetarGuard.IsAlive);
            root.MagnetarGuard.ApplyDamage(new DamageRequest(100000,DamageType.Gravity));Assert.That(root.MapMarkers.GetMarker(10).PremiumRewardsRemaining,Is.EqualTo(2));
            var completions=0;root.BossCompletion.Defeated+=_=>completions++;Move(new Vector3(0,0,73));
            yield return new WaitForSeconds(8);Assert.IsTrue(root.CustodianBoss.CanBeTargeted);Capture("boss_actual_encounter",true);Capture("boss_actual_encounter",false);
            root.CustodianBoss.ApplyDamage(new DamageRequest(100000,DamageType.Gravity));Assert.That(completions,Is.EqualTo(1));
            _scene.AdvanceTime(TimeSpan.FromMinutes(30));root.Chapter1Encounters.Tick();root.CustodianBoss.Tick(0);
            root.CustodianBoss.ApplyDamage(new DamageRequest(100000,DamageType.Gravity));Assert.That(completions,Is.EqualTo(1));Assert.IsTrue(root.FlushNow());
            var score=root.Progression.State.TotalAssimilationScore;var profile=root.ProfileId;
            yield return _scene.Load();root=_scene.Root;
            Assert.That(root.ProfileId,Is.EqualTo(profile));Assert.That(root.Progression.State.TotalAssimilationScore,Is.EqualTo(score));
            Assert.IsTrue(root.BossCompletion.IsDefeated);Assert.That(root.MapMarkers.GetMarker(11).PremiumRewardsRemaining,Is.EqualTo(1));Assert.That(root.MapMarkers.GetMarker(11).RemainingSeconds,Is.EqualTo(1800));
            Directory.CreateDirectory(Output+"/verification");File.WriteAllText(Output+"/verification/encounters-save.txt","PASS: real elite attack and boss encounter in rebuilt architecture; first and repeated defeats; single chapter completion; reward entitlements; cooldown; profile/score preserved after scene reload. Lethal damage and clock advance are isolated test fixtures, production balance unchanged.\n");
        }
        [UnityTest,Timeout(360000)] public IEnumerator ContinuousChapterTraversalWithOrdinaryCombatAndActualEliteBossDefeats()
        {
            if(Environment.GetEnvironmentVariable("GRAVIVORE_CHAPTER_VIDEO")!="1")Assert.Ignore("Opt-in full chapter recording.");
            yield return Load();var root=_scene.Root;root.PlayerHealth.SetDevelopmentGodMode(true);
            // A review checkpoint avoids replaying the 30–45 minute progression grind.
            // Damage, targeting, movement, enemies, telegraphs and rewards use production logic.
            root.PlayerStats.SetLevel(PlayerStatType.Power,Math.Min(25,root.PlayerStats.GetMaximumLevel(PlayerStatType.Power)));
            root.WorldUnlocks.PrepareEliteEncounterForDevelopment();root.Chapter1Encounters.Tick();
            var locomotion=root.PlayerObject.GetComponent<PlayerLocomotion>();locomotion.enabled=false;
            var input=new RouteInput();locomotion.Initialize(input,UnityEngine.Camera.main.transform,root.PlayerStats,540);
            var route=new[]{new Vector3(0,0,-30),new Vector3(0,0,-27),new Vector3(0,0,-10),new Vector3(0,0,-6.3f),new Vector3(0,0,10),new Vector3(-15,0,10),new Vector3(-15,0,12.9f),new Vector3(-15,0,10),new Vector3(0,0,10),new Vector3(15,0,10),new Vector3(15,0,12.8f),new Vector3(15,0,10),new Vector3(0,0,10),new Vector3(0,0,32),new Vector3(-15,0,32),new Vector3(-15,0,34.8f),new Vector3(-15,0,32),new Vector3(0,0,32),new Vector3(15,0,32),new Vector3(15,0,35.5f),new Vector3(15,0,32),new Vector3(0,0,32),new Vector3(0,0,51.5f),new Vector3(-5,0,53.8f),new Vector3(0,0,51.5f),new Vector3(2,0,54),new Vector3(2,0,63),new Vector3(0,0,67),new Vector3(0,0,70),new Vector3(0,0,73),new Vector3(6.5f,0,80),new Vector3(0,0,73)};
            Move(route[0]);var index=1;var frame=0;var started=Time.time;var wait=0f;var nextCapture=0f;
            var attacks=root.GetComponent<AttackCausalityPresenter>().SourceCount;var deaths=0;var maxEnemies=0;
            foreach(var enemy in root.GetComponentsInChildren<OrdinaryEnemyController>(true))enemy.Died+=_=>deaths++;
            Directory.CreateDirectory(Output+"/video-frames");
            while(Time.time-started<300&&(index<route.Length||wait>0))
            {
                if(root.ChapterCompletion.IsVisible){Capture("frame_"+(frame++).ToString("D5"),true,true);root.ChapterCompletion.ContinueExploring();}
                var delta=index<route.Length?route[index]-root.PlayerObject.transform.position:Vector3.zero;delta.y=0;
                var holdElite=index==23&&root.MagnetarGuard.IsAlive;
                var holdBoss=index<route.Length&&route[index]==new Vector3(6.5f,0,80)&&root.CustodianBoss.IsAlive;
                if(wait>0){wait-=Time.deltaTime;input.Movement=Vector2.zero;}
                else if(delta.magnitude<.24f)
                {
                    input.Movement=Vector2.zero;
                    if(!holdElite&&!holdBoss){if(new[]{2,5,9,14,18}.Contains(index))wait=5;index++;}
                }
                else {Steer(input,delta);locomotion.Step(Mathf.Min(Time.deltaTime,.1f));}
                maxEnemies=Math.Max(maxEnemies,root.EnemyPopulation.LiveEnemyCount);
                if(Time.time-started>=nextCapture){Capture("frame_"+(frame++).ToString("D5"),true,true);nextCapture+=.1f;}
                yield return null;
            }
            input.Movement=Vector2.zero;
            Assert.That(index,Is.GreaterThanOrEqualTo(route.Length));Assert.IsFalse(root.MagnetarGuard.IsAlive);Assert.IsTrue(root.BossCompletion.IsDefeated);
            File.WriteAllText(Output+"/verification/video.json",JsonUtility.ToJson(new Video{frames=frame,seconds=Time.time-started,attacks=root.GetComponent<AttackCausalityPresenter>().SourceCount-attacks,deaths=deaths,maxEnemies=maxEnemies,
                method="Single uncut production camera run through all eight sectors. Real locomotion, targeting, AI and attacks; elite/boss die to actual player attacks. Isolated review checkpoint: Power level 25, development gate prerequisites, god mode. Production ContinueExploring closes the completion modal after the actual boss defeat. No lethal damage injection or teleport after the initial spawn; silent 10fps capture, Editor evidence, not Android performance."},true));
        }
        [UnityTest] public IEnumerator AllEightCombatCameraLocationsHaveMeasuredEditorFrameEvidenceWithoutReadback()
        {
            yield return Load();var root=_scene.Root;root.PlayerHealth.SetDevelopmentGodMode(true);root.PlayerObject.GetComponent<PlayerLocomotion>().enabled=false;
            for(var sector=0;sector<8;sector++)
            {
                if(sector==6){root.WorldUnlocks.PrepareEliteEncounterForDevelopment();root.Chapter1Encounters.Tick();}
                if(sector==7)root.MagnetarGuard.ApplyDamage(new DamageRequest(100000,DamageType.Gravity));Move(Positions[sector*3+1]);
                using(var cpu=ProfilerRecorder.StartNew(ProfilerCategory.Internal,"Main Thread"))using(var gc=ProfilerRecorder.StartNew(ProfilerCategory.Memory,"GC Allocated In Frame"))
                {
                    double sum=0,max=0,cpuTotal=0;long gcTotal=0,gcMax=0;var live=0;
                    for(var i=0;i<240;i++){yield return null;var ms=Time.unscaledDeltaTime*1000d;sum+=ms;max=Math.Max(max,ms);cpuTotal+=cpu.LastValue/1000000d;gcTotal+=gc.LastValue;gcMax=Math.Max(gcMax,gc.LastValue);live=Math.Max(live,root.EnemyPopulation.LiveEnemyCount);}
                    Directory.CreateDirectory(Output+"/verification");File.WriteAllText(Output+"/verification/performance-"+Ids[sector]+"-editor.json",JsonUtility.ToJson(new Performance{frames=240,meanFrameMs=sum/240,maxFrameMs=max,meanMainThreadMs=cpuTotal/240,gcBytes=gcTotal,gcMaxFrameBytes=gcMax,maxLiveEnemies=live,cpuValid=cpu.Valid,gcValid=gc.Valid,method="Editor 240-frame observation, active combat, no screenshot readback. Includes Editor/test overhead; does not establish Android FPS, thermal behaviour or zero managed allocations."},true));
                }
            }
        }
'''
tail=r'''
        private sealed class RouteInput:IMovementInput{public Vector2 Movement{get;set;}}
        [Serializable] private sealed class Visibility{public string position;public int uncoveredPixels,visiblePixels;public float fraction;}
        [Serializable] private sealed class VisibilityReport{public Visibility[] samples;}
        [Serializable] private sealed class Video{public int frames,attacks,deaths,maxEnemies;public float seconds;public string method;}
        [Serializable] private sealed class Performance{public int frames,maxLiveEnemies;public long gcBytes,gcMaxFrameBytes;public double meanFrameMs,maxFrameMs,meanMainThreadMs;public bool cpuValid,gcValid;public string method;}
    }
}
'''
(ROOT/'Assets/_Game/Tests/PlayMode/Chapter01CompleteConceptSmokeTests.cs').write_text(imports+body+helpers+tail,encoding='utf-8')
print('CHAPTER_TESTS_WRITTEN')
