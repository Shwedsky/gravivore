"""Original deterministic industrial ambient score; no external samples.
72-second periodic exploration bed + phase-matched restrained combat layer.
32 kHz stereo PCM masters, band-limited machinery/noise and slow harmonic drift.
All composition/synthesis code and resulting recordings are project-owned.
"""
from pathlib import Path
import numpy as np
import wave, json, hashlib
ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT/'Assets/_Game/Content/VisualSlice/Audio'
OUT.mkdir(exist_ok=True)
SR, SECONDS = 32000, 72
n = SR*SECONDS
t = np.arange(n, dtype=np.float64)/SR
u = t/SECONDS
rng = np.random.default_rng(600037)

def tone(hz, phase=0):
    # Integer cycle counts guarantee the oscillator's loop seam.
    return np.sin(2*np.pi*round(hz*SECONDS)*u+phase)

def air(low, high):
    # Periodic filtered noise: FFT removes hiss/harsh treble at source.
    spec = np.fft.rfft(rng.standard_normal(n))
    f = np.fft.rfftfreq(n, 1/SR)
    spec *= np.exp(-(f/high)**4)*(1-np.exp(-(f/low)**4))
    a = np.fft.irfft(spec,n)
    return a/max(.001, np.std(a))

def stereo(center, detail):
    pan = .25*np.sin(2*np.pi*u*3)
    return np.stack([center+detail*(1-pan),center+detail*(1+pan)],axis=1)

bed = .075*tone(36.7)+.045*tone(55,.7)+.035*tone(73.4,1.2)
bed += (.026+.020*np.sin(2*np.pi*u-1))*tone(110,.4)
bed += (.025+.020*np.sin(2*np.pi*u*2+1.2))*tone(146.8,1.3)
bed += .030*tone(220)*(.55+.45*(.5+.5*np.sin(2*np.pi*u*3)))
bed += .022*tone(293.6,1)*(.60+.40*(.5+.5*np.sin(2*np.pi*u*2+.7)))
bed += .014*tone(440,.8)*(.5+.5*np.sin(2*np.pi*u))
wind = air(45,850)*(.011+.007*np.sin(2*np.pi*u*2))
# Slow, distant metal resonances. Eight-to-eighteen second swells, no drum grid.
detail = .010*tone(293.6)*(.5+.5*np.sin(2*np.pi*u*4))**5
detail += .008*tone(440,1.1)*(.5+.5*np.sin(2*np.pi*u*5+.8))**8
explore = stereo(bed+wind,detail)
combat = .064*tone(55,.7)+.048*tone(82.4,1.2)
combat += .026*tone(164.8)*(.65+.35*np.sin(2*np.pi*u*12))
combat += .026*tone(247.2,.5)*(.75+.25*np.sin(2*np.pi*u*6))
combat += .021*tone(110,.4)*(.5+.5*np.sin(2*np.pi*u*18))**3
combat += .008*air(100,1800)*(.5+.5*np.sin(2*np.pi*u*9))**6
combat = stereo(combat,.011*tone(329.6)*(.5+.5*np.sin(2*np.pi*u*6))**4)
report = {}
for name, data in [('Containment_Exploration',explore),('Containment_CombatLayer',combat)]:
    data = np.tanh(data*1.45)*.82
    pcm = (np.clip(data,-.95,.95)*32767).astype('<i2')
    path = OUT/(name+'.wav')
    with wave.open(str(path),'wb') as w:
        w.setnchannels(2); w.setsampwidth(2); w.setframerate(SR); w.writeframes(pcm.tobytes())
    report[name] = {'duration':SECONDS,'sample_rate':SR,'channels':2,
        'peak':float(np.max(np.abs(data))),'rms':float(np.sqrt(np.mean(data**2))),
        'seam_delta':float(np.max(np.abs(data[-1]-data[0]))),
        'sha256':hashlib.sha256(path.read_bytes()).hexdigest(), 'source':'Original procedural composition; no samples'}
(ROOT/'docs/device-correction/music_masters.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
