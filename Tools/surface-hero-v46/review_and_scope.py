"""Factual V45 / V46 boards and exact frozen runtime/content checks."""
import hashlib,json,subprocess
from pathlib import Path
from PIL import Image,ImageDraw,ImageFont,ImageOps
ROOT=Path(__file__).resolve().parents[2];OUT=ROOT/'docs/history/implementation-passes/chapter01-visual-replacement-v3/v46';BASE='ce92f5744b0cd8f4064150c827828992f4dc1c85'
def git(*args):return subprocess.check_output(['git',*args],cwd=ROOT)
def canonical(b):return b'\n'.join(l.rstrip() for l in b.replace(b'\r\n',b'\n').splitlines())
preserved=[]
for prefix in ('Assets/_Game/Runtime','Packages','Assets/_Game/Content/Definitions','Assets/_Game/Content/VisualReplacementV3/Models','Assets/_Game/Content/Chapter01V3/Prefabs','Assets/_Game/Content/VisualSlice/Prefabs','Assets/_Game/Content/VisualReplacementV3/Prefabs','Assets/_Game/Content/VisualReplacementV3/UiSkin','Assets/_Game/Content/ConceptCorrectiveV45/EnemyAmbientMotion.asset'):
    for p in git('ls-tree','-r','--name-only',BASE,prefix).decode().splitlines():
        assert canonical(git('show',BASE+':'+p))==canonical((ROOT/p).read_bytes()),'Frozen content changed: '+p
        preserved.append(p)
receipt={'validated':True,'baseline':BASE,'preservedFiles':len(preserved),'runtimePackagesBalanceWorldSpawnsPatrolSavesProgressionUiActorMeshesWeaponMeshesAndPrefabsUnchanged':True,'authorizedChanges':'Presentation scene overrides, shared PBR materials, original Blender machinery, editor audits and tests','files':preserved}
(OUT/'verification/scope_audit.json').write_text(json.dumps(receipt,indent=2))
font=ImageFont.truetype('C:/Windows/Fonts/segoeui.ttf',22);small=ImageFont.truetype('C:/Windows/Fonts/segoeui.ttf',18)
def img(phase,name):return OUT/'internal'/phase/(name+'.png')
def board(name,rows,title):
    w=720;h=760;sheet=Image.new('RGB',(w*len(rows[0]),60+h*len(rows)),(12,18,23));d=ImageDraw.Draw(sheet);d.text((18,12),title,font=font,fill=(203,218,224))
    for y,row in enumerate(rows):
        for x,(label,path) in enumerate(row):
            im=Image.open(path).convert('RGB');pic=ImageOps.contain(im,(w-20,h-52),Image.Resampling.LANCZOS)
            d.text((x*w+14,68+y*h),label,font=small,fill=(170,198,212));sheet.paste(pic,(x*w+(w-pic.width)//2,102+y*h))
    sheet.save(OUT/'internal'/(name+'.jpg'),quality=94)
board('surface_quality_board',[[('V45 / same corridor machinery camera',img('before','corridor_machine')),('V46 / sealed pressure vessel at corridor',img('after','corridor_machine')),('V46 / neutral lighting; authored capsule',img('neutral','PressureVessel'))]],'SURFACE QUALITY / textured plates, recesses, brushed response, bevels and wear')
board('spawn_dock_board',[[('V45 / capacitor origin',img('before','dock_capacitor-field')),('V46 / same capacitor origin',img('after','dock_capacitor-field')),('V46 / industrial service dock',img('neutral','SpecialDock'))],[('V46 / light service dock',img('neutral','LightDock')),('V46 / heavy service dock',img('neutral','HeavyDock')),('V46 / ordinary origin and patrol apron',img('after','sector_hauler-graveyard'))]],'SPAWN DOCKS / backplate, paired chargers, floor cradle, feeds and open patrol entrance')
board('hero_machinery_board',[[('V46 / pressure vessel',img('neutral','PressureVessel')),('V46 / contained energy cylinder',img('neutral','EnergyCylinder')),('V46 / articulated curved machine',img('neutral','ArcMachine'))],[('V46 / layered gate module',img('neutral','GateModule')),('V45 / production corridor',img('before','04_service_corridor')),('V46 / same production corridor',img('after','04_service_corridor'))]],'HERO MACHINERY / original editable Blender sources / neutral and production cameras')
board('actor_surface_board',[[('V45 / Magnetar close camera',img('before','magnetar_surface')),('V46 / same Magnetar close camera',img('after','magnetar_surface'))],[('V45 / Custodian close camera',img('before','custodian_surface')),('V46 / same Custodian close camera',img('after','custodian_surface'))]],'ACTOR SURFACES / accepted meshes, bones, clips and silhouette retained')
board('intersection_cleanup_board',[[('V45 / corridor stack and supports',img('before','corridor_machine')),('V46 / same corridor; service clearances',img('after','corridor_machine'))],[('V45 / Custodian production camera',img('before','06_custodian_arena')),('V46 / same camera; duplicate cladding removed',img('after','06_custodian_arena'))]],'INTERSECTION CLEANUP / art changes inside frozen V45 collision proxies')
print(json.dumps({'scopeAudit':True,'preservedFiles':len(preserved),'comparisonBoards':5}))
