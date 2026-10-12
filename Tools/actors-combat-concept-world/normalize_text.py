"""Remove generated trailing whitespace from this pass's source and text evidence."""
from pathlib import Path
import subprocess
ROOT=Path(__file__).resolve().parents[2]
paths=[ROOT/'Assets/_Game/ArtReview/G0ProductionV3/G0_Production_V3_Review.controller']
paths+=list((ROOT/'Assets/_Game/Content/ActorProductionV2').rglob('*.controller'))
paths+=list((ROOT/'Assets/_Game/Content/ActorProductionV2').rglob('*.mask'))
paths+=list((ROOT/'Assets/_Game/Content/ActorProductionV2/Materials').glob('*.mat'))
paths+=list((ROOT/'Assets/_Game/Content/VisualSlice/Materials').glob('G0_V3_Tier*.mat'))
paths+=[ROOT/p for p in subprocess.check_output(['git','diff','--cached','--name-only','--diff-filter=A'],cwd=ROOT).decode().splitlines() if p.startswith('Assets/_Game/') and p.endswith('.meta')]
paths+=list((ROOT/'docs/history/implementation-passes/actors-combat-concept-world/verification').glob('*.txt'))
count=0
for path in paths:
    text=path.read_text(encoding='utf-8')
    clean='\n'.join(line.rstrip() for line in text.splitlines()).rstrip()+'\n'
    if clean!=text: path.write_text(clean,encoding='utf-8',newline='\n');count+=1
print('Normalized authored YAML whitespace:',count)
