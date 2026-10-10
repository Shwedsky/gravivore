"""Keep existing validation callbacks from recreating superseded docs roots."""
from pathlib import Path
import json
ROOT=Path(__file__).resolve().parents[2]
base='docs/history/visual-stages/chapter01-visual-passes'
mapping={f'docs/{name}':f'{base}/{name}' for name in ['chapter01-production','chapter01-v3','device-correction','first-visual-slice','post-device-combat-readability','render-hotfix','concept-fidelity-v2']}
mapping['docs/art-spike']='docs/history/visual-stages/art-spike/evidence'
updated=[]
for folder in ['Assets/_Game/Editor','Assets/_Game/Tests','Tools']:
    for p in (ROOT/folder).rglob('*'):
        if p.suffix not in ('.cs','.py','.ps1') or p==Path(__file__).resolve():continue
        original=p.read_text(encoding='utf-8-sig');lines=[]
        for line in original.splitlines(keepends=True):
            if "git('show'" not in line and "'git','show'" not in line and 'git show ' not in line:
                for old,new in mapping.items():line=line.replace(old,new)
            lines.append(line)
        revised=''.join(lines)
        if revised!=original:p.write_text(revised,encoding='utf-8',newline='');updated.append(p.relative_to(ROOT).as_posix())
(ROOT/'docs/history/implementation-passes/chapter01-visual-replacement-v3/v47/legacy_output_path_refresh.json').write_text(json.dumps({'changedFiles':updated,'purpose':'Validation receipts remain historical; no active authority or runtime art source changed.'},indent=2))
print('UPDATED_VALIDATION_OUTPUT_PATHS',len(updated))
