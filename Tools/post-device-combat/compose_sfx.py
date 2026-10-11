"""Original G-0 mechanical SFX: deterministic noise, filters and resonators; no samples."""
import hashlib
import json
import math
from pathlib import Path
import random
import re
import struct
import wave

ROOT = Path(__file__).resolve().parents[2]
RATE = 48000

def synth(kind, duration, seed):
    rng = random.Random(seed)
    low = mid = 0.0
    samples = []
    for i in range(int(duration * RATE)):
        t = i / RATE
        noise = rng.uniform(-1, 1)
        low += (noise-low) * (1-math.exp(-2*math.pi*180/RATE))
        mid += (noise-mid) * (1-math.exp(-2*math.pi*1200/RATE))
        texture = mid-low
        if kind == "charge":
            envelope = math.sin(math.pi*t/duration)**1.3
            signal = envelope*(.9*texture+.16*math.sin(2*math.pi*94*t)+.09*math.sin(2*math.pi*211*t))
        elif kind == "release":
            envelope = (1-math.exp(-t*900))*math.exp(-t*21)
            signal = envelope*(1.8*texture+.48*math.sin(2*math.pi*112*t)+.22*math.sin(2*math.pi*237*t))
            signal += .13*math.sin(2*math.pi*587*t)*math.exp(-t*90)
        elif kind == "impact":
            envelope = (1-math.exp(-t*800))*math.exp(-t*25)
            signal = envelope*(1.3*texture+.55*math.sin(2*math.pi*83*t)+.25*math.sin(2*math.pi*174*t))
        elif kind == "servo":
            envelope = math.sin(math.pi*t/duration)**2
            signal = envelope*(1.4*texture+.12*math.sin(2*math.pi*162*t)+.08*math.sin(2*math.pi*321*t))
        else:
            strike = (1-math.exp(-t*1200))*math.exp(-t*32)
            actuator = math.sin(math.pi*t/duration)**2*math.exp(-t*12)
            signal = strike*(.6*math.sin(2*math.pi*(116+seed%9)*t)+.27*math.sin(2*math.pi*287*t)+1.4*texture)
            signal += actuator*.6*texture
        fade = min(1,t/.003,(duration-t)/.018)
        samples.append(math.tanh(signal*1.7)*max(0,fade))
    peak = max(abs(x) for x in samples)
    return [x*.78/peak for x in samples]

def main():
    clip_root = ROOT / "Assets/_Game/Content/Presentation/Phase6B/Audio/Clips"
    jobs = [(ROOT / "Assets/_Game/Content/Audio/PlayerFeel/Step.wav", "step", .22, 17)]
    for i in range(1,4): jobs.append((clip_root / f"P0_PlayerStep_{i:02}.wav", "step", .22, 17+i))
    for i in range(1,3): jobs.append((clip_root / f"P0_Servo_{i:02}.wav", "servo", .18, 30+i))
    for file, kind, length, seed in [("P0_LashCharge.wav","charge",.16,41),
                                   ("P0_LashRelease.wav","release",.24,42),
                                   ("P0_LashImpact.wav","impact",.26,43)]:
        jobs.append((clip_root / file,kind,length,seed))
    evidence = []
    for path, kind, duration, seed in jobs:
        data = synth(kind,duration,seed)
        with wave.open(str(path),"wb") as wav:
            wav.setnchannels(1); wav.setsampwidth(2); wav.setframerate(RATE)
            wav.writeframes(struct.pack("<"+"h"*len(data),*(round(x*32767) for x in data)))
        evidence.append(dict(path=path.relative_to(ROOT).as_posix(),kind=kind,duration=duration,
                             sample_rate=RATE,peak=max(abs(x) for x in data),
                             rms=math.sqrt(sum(x*x for x in data)/len(data)),
                             sha256=hashlib.sha256(path.read_bytes()).hexdigest()))
    output=ROOT/"docs/history/visual-stages/chapter01-visual-passes/post-device-combat-readability/sfx_masters.json"
    output.write_text(json.dumps(dict(authorship="Project-owned synthesis; no sampled audio",clips=evidence),indent=2))
    bank=ROOT/"Assets/_Game/Content/Presentation/Phase6B/Audio/Phase6B_AudioBank.asset"
    source=bank.read_text()
    for cue, volume in [(2,.27),(3,.48),(4,.37)]:
        source=re.sub(rf'(  - _cue: {cue}\n(?:(?!  - _cue:).)*?_volume: )[^\n]+',
                      lambda match:match.group(1)+str(volume),source,flags=re.S)
    bank.write_text(source)
    print(f"Generated {len(evidence)} original mechanical SFX masters.")

if __name__ == "__main__": main()
