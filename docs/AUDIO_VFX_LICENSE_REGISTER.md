# GRAVIVORE — Audio / VFX License Register

Status: **source verification register — no third-party binaries committed in Phase 6A**

Checked: **2026-10-04**

## License policy

Selected third-party candidates in this register are CC0. CC0 is compatible with commercial use, modification and redistribution without required attribution. GRAVIVORE still records provenance.

If a later download page, bundled license file or source archive contradicts this register, **do not import the asset** until resolved.

## Register

### Kenney — Sci-fi Sounds
- Author: Kenney
- Official source: https://kenney.nl/assets/sci-fi-sounds
- Exact license: Creative Commons CC0
- Commercial use: yes
- Modification: yes
- Redistribution: yes
- Attribution required: no
- Selected/downloaded files: **not downloaded in Phase 6A**
- Planned use: charge/release/energy source layers
- Planned modification: trim, EQ, pitch, layer
- Quality grade: B
- Import status: pending local binary audition

### Kenney — Digital Audio
- Author: Kenney
- Official source: https://kenney.nl/assets/digital-audio
- Exact license: Creative Commons CC0
- Commercial use: yes
- Modification: yes
- Redistribution: yes
- Attribution required: no
- Selected/downloaded files: not downloaded
- Planned use: synthetic sublayers only
- Quality grade: C/B
- Import status: optional

### Kenney — Interface Sounds
- Author: Kenney
- Official source: https://kenney.nl/assets/interface-sounds
- Exact license: Creative Commons CC0
- Commercial use: yes
- Modification: yes
- Redistribution: yes
- Attribution required: no
- Selected/downloaded files: not downloaded
- Planned use: UI click/confirm/deny
- Quality grade: B
- Import status: pending

### Kenney — Particle Pack
- Author: Kenney
- Official source: https://kenney.nl/assets/particle-pack
- Exact license: Creative Commons CC0
- Commercial use: yes
- Modification: yes
- Redistribution: yes
- Attribution required: no
- Selected/downloaded files: not downloaded
- Planned use: particle sprite source for sparks/flash/smoke where visually suitable
- Quality grade: A/B source
- Import status: pending texture inspection

### Mechanical Sounds
- Author: BMacZero / Brian MacIntosh
- Official source: https://opengameart.org/content/mechanical-sounds
- Exact license: CC0
- Commercial use: yes
- Modification: yes
- Redistribution: yes
- Attribution required: no; source states optional credit
- Candidate filenames: clank1.wav, lightclunk1.wav, lightclunk2.wav, rattle1.wav, squeakyclick1.wav, squeakyclick2.wav, mechanical1.wav, mechanical2.wav
- Downloaded files: none in Phase 6A
- Planned use: footsteps, servo, repair arm, enemy movement layers
- Planned modification: trim/EQ/pitch/layer
- Quality grade: A/B
- Import status: priority audition

### 60 CC0 Sci-Fi SFX
- Author: rubberduck
- Official source: https://opengameart.org/content/60-cc0-sci-fi-sfx
- Exact license: CC0
- Commercial use: yes
- Modification: yes
- Redistribution: yes
- Attribution required: no
- Package filename: 60-sci-fi-sfx.zip
- Downloaded files: none
- Planned use: charge, Arc Drone, Magnetar, system textures
- Planned modification: strict removal of arcade/phaser-like sounds; layer selected dry material
- Quality grade: B
- Import status: priority audition

### 50 CC0 Sci-Fi SFX
- Author: rubberduck
- Official source: https://opengameart.org/content/50-cc0-sci-fi-sfx
- Exact license: CC0
- Commercial use: yes
- Modification: yes
- Redistribution: yes
- Attribution required: no
- Package filename: sci-fi-sfx.zip
- Downloaded files: none
- Planned use: misc/terminal/energy source only
- Planned modification: reject retro subset
- Quality grade: C/B
- Import status: secondary

### Electronic device loop
- Author: qubodup
- Official source: https://opengameart.org/content/electronic-device-loop
- Exact license: CC0
- Commercial use: yes
- Modification: yes
- Redistribution: yes
- Attribution required: no
- Candidate filename: qubodup-edev.flac
- Downloaded files: none
- Planned use: repair-hub diagnostic/machine layer
- Planned modification: EQ, gain reduction, clean loop/short derivative
- Quality grade: B
- Import status: pending

### Steamboat Engine Sound
- Author: Spring Spring
- Official source: https://opengameart.org/content/steamboat-engine-sound
- Exact license: CC0
- Commercial use: yes
- Modification: yes
- Redistribution: yes
- Attribution required: no
- Candidate filename: steamboat_engine.wav
- Downloaded files: none
- Planned use: source layer for Carrier/Custodian/repair machinery
- Planned modification: strong filtering/layering; literal boat identity must disappear
- Quality grade: B
- Import status: optional

### Persistence
- Author: cinameng / James Gargette
- Official source: https://opengameart.org/content/persistence
- Exact license: CC0
- Commercial use: yes
- Modification: yes
- Redistribution: yes
- Attribution required: no
- Candidate filenames: persistence.mp3, persistence-loopshort.mp3
- Downloaded files: none
- Planned use: music direction A audition
- Quality grade: A audition
- Import status: pending device/music review

### Bilwe
- Author: cinameng / James Gargette
- Official source: https://opengameart.org/content/bilwe
- Exact license: CC0
- Commercial use: yes
- Modification: yes
- Redistribution: yes
- Attribution required: no
- Candidate filename: bilwe.mp3
- Downloaded files: none
- Planned use: music direction B audition
- Quality grade: A audition
- Import status: pending

### Searching
- Author: yd
- Official source: https://opengameart.org/content/searching
- Exact license: CC0
- Commercial use: yes
- Modification: yes
- Redistribution: yes
- Attribution required: no
- Candidate filename: Searching.ogg
- Downloaded files: none
- Planned use: music direction C audition / recommended baseline candidate
- Quality grade: A audition
- Import status: pending

### Factory ambiance
- Author: yd
- Official source: https://opengameart.org/content/factory-ambiance
- Exact license: CC0
- Commercial use: yes
- Modification: yes
- Redistribution: yes
- Attribution required: no
- Candidate filename: Factory.ogg
- Downloaded files: none
- Planned use: ambient music alternate
- Quality grade: A/B audition
- Import status: pending

## Explicit rejects / not selected

- CC-BY-SA / other ShareAlike content: reject unless explicitly approved later.
- NC content: reject.
- "free" pages without exact license: reject.
- unofficial mirrors: reject.
- login-only/manual acquisition when autonomous legal provenance cannot be verified: reject.
- recognizable franchise sounds: reject.
- 8-bit/chiptune sources: reject by direction even when CC0.
- cyberpunk club / aggressive metal / heroic orchestral music: reject for Chapter 1 baseline.

## Binary intake checklist

Before a future integration commit, add for every imported file:
- original exact filename;
- SHA-256 if practical;
- downloaded date;
- official URL;
- bundled license file or page snapshot reference;
- format/container;
- sample rate;
- bit depth;
- channels;
- duration;
- peak level / clipping result;
- leading/trailing silence;
- noise/reverb notes;
- derivative filename;
- edit chain;
- final event mapping.

Then update `ThirdPartyNotices.md` with the exact imported subset.
