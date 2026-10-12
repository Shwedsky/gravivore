"""Reuse the proven real-camera capture/controller fixtures for a new scope."""
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
p=ROOT/'Assets/_Game/Tests/PlayMode/CapacitorFieldProofSmokeTests.cs'
s=p.read_text(encoding='utf-8').replace('CapacitorFieldProofSmokeTests','ConnectedConceptSectorsSmokeTests').replace('implementation-passes/capacitor-field-proof','implementation-passes/connected-concept-sectors')
s=s.replace('private SpawnSpotRuntime Arc => Enumerable.Range(0,_scene.Root.EnemyPopulation.SpotCount).Select(_scene.Root.EnemyPopulation.GetSpot).Single(s=>s.Id=="capacitor-field");','private SpawnSpotRuntime Spot(string id) => Enumerable.Range(0,_scene.Root.EnemyPopulation.SpotCount).Select(_scene.Root.EnemyPopulation.GetSpot).Single(s=>s.Id==id);')
s=s.replace('new Vector3(-9,0,10)};', 'new Vector3(-9,0,10),new Vector3(8,0,10),new Vector3(15,0,10),new Vector3(15,0,12.8f),new Vector3(18.2f,0,8),new Vector3(9,0,10),new Vector3(-5.8f,0,10),new Vector3(0,0,10),new Vector3(5.8f,0,10)};')
s=s.replace('"05_return_exit"};','"05_return_exit","06_cutting_entry","07_cutting_combat","08_cutting_approach","09_cutting_edge","10_cutting_exit","11_transfer_west","12_transfer_junction","13_transfer_east"};')
s=s.replace('FiveNormalGameplayPositionsAndPostProcessingActuallyChangesPixels','BothSectorsAndTransferAtOneProductionCameraWithActualPostProcessing')
s=s.replace('var spot=Arc;', 'foreach(var spotId in new[]{"capacitor-field","cutting-floor"})\n            {\n            var spot=Spot(spotId);')
s=s.replace('ArcAttacksRewardsDeathRespawnPlayerDeathRepairAndSaveRemainFunctional','ArcAndCutterAttacksRewardsDeathRespawnRepairAndSaveRemainFunctional')
s=s.replace('Arc actual attack/source;', '"+spotId+" actual attack/source;')
s=s.replace('File.WriteAllText(Output+"/verification/functional-flow.txt",','File.AppendAllText(Output+"/verification/functional-flow.txt",')
s=s.replace('production balance untouched.");\n        }', 'production balance untouched.\\n");\n            }\n        }')
# A physical path through the machine approaches, all portals and the central junction.
route='new[]{new Vector3(-15,0,10),new Vector3(-15,0,12.9f),new Vector3(-15,0,10),new Vector3(-8,0,10),new Vector3(-4,0,10),new Vector3(0,0,10),new Vector3(4,0,10),new Vector3(8,0,10),new Vector3(15,0,10),new Vector3(15,0,12.8f),new Vector3(18.2f,0,8),new Vector3(15,0,10),new Vector3(8,0,10),new Vector3(0,0,10),new Vector3(-8,0,10),new Vector3(-15,0,10)}'
import re
s=re.sub(r'var route=new\[\]\{[^;]+\};', 'var route='+route+';',s)
s=s.replace('ActualPlayerLocomotionTraversesEntryApproachEdgesAndExit','ActualPlayerLocomotionTraversesBothInstallationsAndAllTransferPortals')
s=s.replace('GRAVIVORE_CAPACITOR_VIDEO','GRAVIVORE_CONNECTED_VIDEO').replace('if(index==2)wait=12;','if(index==2||index==8)wait=12;')
s=s.replace('Time.time-started<55','Time.time-started<80')
s=s.replace('MeasuredEditorCombatObservationWithoutReadback','MeasuredEditorCombatInBothFunctionalPlacesWithoutReadback')
s=s.replace('yield return Load();var root=_scene.Root;Move(Positions[1]);root.PlayerHealth.SetDevelopmentGodMode(true);','yield return Load();var root=_scene.Root;root.PlayerHealth.SetDevelopmentGodMode(true);\n            foreach(var region in new[]{"capacitor-field","cutting-floor"})\n            {\n            Move(region=="capacitor-field"?Positions[1]:Positions[6]);')
s=s.replace('GetRegion("capacitor-field").Root.GetComponentsInChildren<MeshRenderer>();','GetRegion(region).Root.GetComponentsInChildren<MeshRenderer>();')
s=s.replace('Output+"/verification/performance-editor.json"','Output+"/verification/performance-"+region+"-editor.json"')
s=s.replace('performance.\"},true));\n            }\n        }','performance.\"},true));\n            }\n            }\n        }') if False else s
needle='Does not establish Android FPS, thermal behaviour or zero-allocation runtime."},true));\n            }\n        }'
s=s.replace(needle,'Does not establish Android FPS, thermal behaviour or zero-allocation runtime."},true));\n            }\n            }\n        }')
(ROOT/'Assets/_Game/Tests/PlayMode/ConnectedConceptSectorsSmokeTests.cs').write_text(s,encoding='utf-8')
print('CONNECTED_CONTROLLER_CAMERA_FIXTURES_READY')
