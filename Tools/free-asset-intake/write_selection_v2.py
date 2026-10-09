"""Format explicit human-review candidate judgments, enforcing license gates."""
import csv
import json
from collections import Counter
from intake_v2 import ROOT, write_json

def write():
    docs=ROOT/'docs/free-asset-intake-v2'
    data=json.loads((ROOT/'Tools/free-asset-intake/selection_v2.json').read_text(encoding='utf-8'))
    with (docs/'ASSET_PROVENANCE_RESOLVED.csv').open(encoding='utf-8',newline='') as f:provenance={r['sourceId']:r for r in csv.DictReader(f)}
    resolution=json.loads((docs/'SOURCE_RESOLUTION.json').read_text(encoding='utf-8'))
    assert {x['sourceId'] for x in data['sources']}=={x['sourceId'] for x in resolution['discoveredPayloads'] if x['sourceId']!='reference-concept-board'}
    weights=data['weights'];assert abs(sum(weights)-1)<1e-8
    counts=Counter(x['decision'] for x in data['sources'])
    lines=['# Asset selection matrix — V2','', 'Decision scope: 26 actual art sources. The owner reference, one missing pipe source, 11 subscription-skipped Meshy sources and embedded duplicate formats are excluded from selection counts.', '', 'Visual authority: local owner concept board inspected; approved bipedal G-0 override and enemy target document remain binding. Scores are technical-art judgment from actual local model/UI evidence plus static VFX audit, not an Android performance benchmark or proof of A quality.', '', 'Score order: concept fidelity / mobile technical fit / top-down readability / integration ease / license-provenance / useful distinct coverage. Each score is 0–5; higher integration ease means less adaptation. Weights: 35 / 20 / 15 / 15 / 10 / 5 percent. Weighted total = Σ(score × weight), maximum 5. Equivalent /100 = total ×20. Quantity alone earns no credit.', '', f"Source decisions: USE {counts['USE']}; DONOR {counts['DONOR']}; REJECT {counts['REJECT']}; UNRESOLVED {counts['UNRESOLVED']}.", '', 'USE is a license-compatible selected visible family, subject to the named limited material/scale adaptation. DONOR requires redesign/rebuild before visible production use. VFX scores are provisional for animation/overdraw until V3 runtime review; no VFX stock prefab is USE. Item restrictions below are binding: a pack decision never authorizes every member.', '']
    for s in data['sources']:
        p=provenance[s['sourceId']];assert len(s['scores'])==6 and all(0<=n<=5 for n in s['scores'])
        if s['decision']=='USE':assert p['productionUseAllowed']=='True' and p['license']!='UNKNOWN_REQUIRES_REVIEW'
        total=round(sum(n*w for n,w in zip(s['scores'],weights)),2);s['weightedScore']=total
        lines += [f"## {s['sourceId']} — {s['decision']}", '', f"Scores: {' / '.join(map(str,s['scores']))}; weighted **{total:.2f}/5 ({total*20:.1f}/100)**. License: {p['license']}.", '', 'Items: '+s['items'], '', 'Judgment: '+s['why'], '']
    lines += ['## Required production adaptations','', 'One shared cold-metal material vocabulary; restrained cyan player/service identity and orange/red hostile/industrial energy. Keep attack lanes and floor contrast legible. Use denser layered edge dressing rather than more center-screen clutter. Palette-only donor atlases do not replace custom damage/wear authoring.', '', 'Asset Store sources remain owner-local/private even after adaptation; V3 must use a reproducible local intake or private artifact path. Public review PNGs are annotated, flattened comparisons only. EXE CC BY attribution must ship with any reused graphics. Unknown licenses never pass the USE gate.', '', 'No candidate authorizes replacing G-0, changing gameplay geometry/collision, enemy telegraphs, map layout, equipment mechanics or source/travel/impact timing.']
    (docs/'ASSET_SELECTION_MATRIX.md').write_text('\n'.join(lines)+'\n',encoding='utf-8')
    write_json(docs/'SELECTION_SUMMARY.json',{'scope':'26 discovered art sources; reference/missing/skipped/duplicates excluded','counts':dict(counts),'sources':data['sources']})
    print('SELECTION_READY',dict(counts))

if __name__=='__main__':write()
