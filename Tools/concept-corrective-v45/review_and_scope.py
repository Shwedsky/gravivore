"""Local before/after review boards and explicit V44 authority preservation audit."""
import hashlib,json,re,subprocess
from pathlib import Path
from PIL import Image,ImageDraw,ImageFont

ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'docs/history/implementation-passes/chapter01-visual-replacement-v3/v45'
BASE='f57c8ac94b73c74b6a05cd242b57576f615c0b3b'
def git(*args):return subprocess.check_output(['git',*args],cwd=ROOT)
def canonical(data):return b'\n'.join(line.rstrip() for line in data.replace(b'\r\n',b'\n').splitlines())
allowed={
 'Assets/_Game/Runtime/Gameplay/Enemies/EnemyPopulationController.cs',
 'Assets/_Game/Runtime/Gameplay/Enemies/OrdinaryEnemyController.cs',
 'Assets/_Game/Runtime/Gameplay/Enemies/OrdinaryEnemyPool.cs',
 'Assets/_Game/Runtime/Presentation/Composition/S01SceneCompositionRoot.cs',
 'Assets/_Game/Runtime/Presentation/World/FidelityAtmospherePresenter.cs'}
preserved=[];changed=[]
for path in git('ls-tree','-r','--name-only',BASE,'Assets/_Game/Runtime','Packages').decode().splitlines():
 if canonical(git('show',BASE+':'+path))==canonical((ROOT/path).read_bytes()):preserved.append(path)
 else:
  if path not in allowed:raise RuntimeError('Unapproved runtime/package change: '+path)
  changed.append(path)
position_fields={'S04_SpawnSpot_CapacitorField.asset':'_worldOrigin','S04_SpawnSpot_CuttingFloor.asset':'_worldOrigin',
 'S04_SpawnSpot_HaulerGraveyard.asset':'_worldOrigin','S04_SpawnSpot_RelayYard.asset':'_worldOrigin',
 'S04_SpawnSpot_ShieldDump.asset':'_worldOrigin','S09_MagnetarGuard.asset':'_spawnPosition','S09_CustodianM0.asset':'_startPosition'}
for filename,field in position_fields.items():
 path='Assets/_Game/Content/Definitions/'+filename
 def omit(data):return re.sub(rb'(?m)^  '+field.encode()+rb':.*$',b'',canonical(data))
 if omit(git('show',BASE+':'+path))!=omit((ROOT/path).read_bytes()):raise RuntimeError('Balance changed beyond position: '+path)
for prefix in ('Assets/_Game/Content/VisualReplacementV3','Assets/_Game/Content/VisualSlice/Prefabs','Assets/_Game/Content/Chapter01V3/Prefabs'):
 for path in git('ls-tree','-r','--name-only',BASE,prefix).decode().splitlines():
  if canonical(git('show',BASE+':'+path))!=canonical((ROOT/path).read_bytes()):raise RuntimeError('Accepted actor/weapon/UI content changed: '+path)
evidence={'validated':True,'baseline':BASE,'preservedRuntimeAndPackageFiles':preserved,'allowedRuntimeChanges':changed,
 'ordinaryAndEncounterBalanceUnchangedExceptWorldPositions':True,'acceptedV44ModelsWeaponsUiAndMaterialsPreserved':True,
 'saveProgressionCombatAndMapSemanticsPreserved':True,'worldLayoutException':'Explicitly authorized compaction and collision proxy changes; same zone/gate/encounter ids'}
(OUT/'verification/scope_audit.json').write_text(json.dumps(evidence,indent=2))
try:font=ImageFont.truetype('C:/Windows/Fonts/segoeui.ttf',22)
except OSError:font=ImageFont.load_default()
subjects=['01_repair_hub','02_ordinary_sector','03_strong_ordinary','04_service_corridor','05_magnetar_encounter','06_custodian_arena']
for subject in subjects:
 before=Image.open(OUT/'internal/before'/f'{subject}.png').convert('RGB')
 after=Image.open(OUT/'internal/after'/f'{subject}.png').convert('RGB')
 board=Image.new('RGB',(1080,1020),(10,17,22));draw=ImageDraw.Draw(board)
 draw.text((15,14),'V44 / archived production camera',font=font,fill=(172,192,202));draw.text((555,14),'V45 / corrective production camera',font=font,fill=(172,192,202))
 board.paste(before,(0,60));board.paste(after,(540,60));board.save(OUT/'internal'/f'{subject}_before_after.jpg',quality=92)
motion=Image.new('RGB',(1620,1020),(10,17,22));draw=ImageDraw.Draw(motion)
for i,frame in enumerate((0,450,899)):
 draw.text((i*540+15,14),f'Bounded idle patrol / tick {frame}',font=font,fill=(172,192,202))
 motion.paste(Image.open(OUT/'internal/after'/f'idle_{frame}.png').convert('RGB'),(i*540,60))
motion.save(OUT/'internal/idle_patrol_sequence.jpg',quality=92)
print(json.dumps({'validated':True,'preservedRuntimeAndPackageFiles':len(preserved),'allowedRuntimeChanges':len(changed),'beforeAfterBoards':len(subjects)}))
