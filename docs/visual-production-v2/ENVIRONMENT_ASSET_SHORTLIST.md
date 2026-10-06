# Environment Asset Shortlist — First Visual Slice

Research date: 2026-10-06. Target: one authored ~15×20 m industrial combat space containing floor kit, walls/barriers, one gate, one hero reactor, 4–6 supporting props, conduits/pipes/tanks/machinery/columns/decal support, with mobile-friendly final outputs.

## Primary recommendation

Use **one coherent structural family + one restrained props donor family + custom hero reactor/gate treatment**. Avoid a collage of unrelated sci-fi packs.

## Structural candidates

### 1. Quaternius — Modular Sci-Fi MEGAKIT
- Source: Quaternius
- URL: https://quaternius.com/packs/modularscifimegakit.html
- License: CC0; commercial use permitted; attribution not required.
- Formats: FBX, OBJ, glTF; source tier also offers BLEND and prebuilt Unity URP/Unreal/Godot projects.
- Contents: 270+ modular environment models; grid-based; source version includes custom optimized collisions.
- Visual fit: **B as geometry donor, C+ raw**. Strong coherence and permissive licensing, but its stylized/clean baseline needs GRAVIVORE rematerialing and selective detail augmentation.
- Mobile suitability: high after selecting only required pieces.
- Verdict: **BUY/DOWNLOAD NOW for geometry donor and blockout-to-production transition.**

### 2. OZEA Studio — A-001+ Low Poly Sci-Fi Modular Environment Pack
- Source: Fab / OZEA ecosystem.
- URL: https://www.fab.com/listings/0215a9a9-63fe-4a9f-a147-b66fd17af533
- Contents surfaced: 20 structural pieces including door frame, wall variants, floor tiles, slopes and railings.
- Visual fit: **B- donor**. Cohesive and efficient, but too clean/low-detail as a final untouched environment.
- License/pricing: Fab listing requires selecting a current license; exact purchased license and price must be verified before procurement.
- Verdict: **PROMISING — REVIEW LICENSE/SOURCE FILES FIRST.**

### 3. SciFi Industrial Modular Kit — Corridors, Cargo Bay & Procedural AUTO Generator
- Source: Fab
- URL: https://www.fab.com/listings/29dff121-c082-40e4-b125-df27f0273a72
- Features: used-future painted steel, hazard stripes, vents, exposed pipes, modular 4 m grid, walls, doors, cargo doors, pillars, railings, catwalk, floors, ceilings, ramps/stairs, modular pipes.
- Visual fit: **A- visually** based on listing description; closest surfaced family to desired worn industrial language.
- Risk: currently surfaced listing is Unreal-focused and exact export/source availability plus license/price must be checked before buying.
- Verdict: **PROMISING — REVIEW SOURCE FILES FIRST**, potentially the best paid environment family if raw mesh access is suitable for Unity/Blender.

### 4. Industrial Modular SciFi Environment Kit — XRdev72
- Source: Fab
- URL: https://www.fab.com/listings/9f96b5e2-969f-460c-892f-b688e62d8f53
- Strong industrial detail, PBR, 25 cm modular grid, pipes/panels/wires/vents.
- Visual fit: **B+**, but current listing shows Unreal Engine format; source portability and actual purchased Fab license must be verified.
- Verdict: **REVIEW FIRST**, not immediate buy for a Unity project.

### 5. Sci-Fi Modular Corridor Environment Pack — zenrup
- Source: Fab
- URL: https://www.fab.com/listings/a91a52e7-2233-49a9-9d2a-82a18bcb3823
- 27 meshes / 28 materials / 105 textures; 591,858 triangles across pack; clean topology/real-time claim.
- Visual fit: **C+** because the listing describes clean corridor sci-fi; likely needs heavy grime/rematerial and may be too dense for a small mobile slice if used naively.
- Verdict: **DONOR ONLY / lower priority**.

## Supporting props candidates

### OZEA Studio — HS-003 Low Poly Sci-Fi Industrial Props Pack
- Source: itch.io
- URL: https://ozea-studio.itch.io/low-poly-sci-fi-industrial-props-pack-hs-003
- Price surfaced: $5 minimum.
- License: commercial/non-commercial use allowed; source redistribution/resale prohibited.
- Formats: BLEND, FBX, OBJ+MTL.
- 12 props including barriers, vent block, data unit, barrel, terminal, wall panels and grate.
- Visual fit: **B- donor**. Useful as low-cost coherent secondary props; requires PBR/GRAVIVORE rematerial because pack uses simple material-color workflow.
- Verdict: **BUY/DOWNLOAD NOW candidate** after final human visual glance.

### Assets.fun — Low Poly Sci-Fi Industrial Props Pack
- Source: itch.io
- URL: https://assetsdotfun.itch.io/low-poly-sci-fi-industrial-props-pack
- Price surfaced: $5 minimum.
- 30 props / 12,524 tris total; FBX, OBJ, GLB, BLEND, STL; eight shared solid-color materials; no PBR textures.
- Visual fit: **C+ donor**. Extremely efficient and editable but visually too simple for final hero area without substantial rematerial/detail pass.
- License terms were not fully surfaced in research result; verify purchase license before use.
- Verdict: **PROMISING — REVIEW LICENSE FIRST**.

### Tefotec — Sci-Fi Industrial Props Free Pack
- URL: https://tefotec.itch.io/sci-fi-industrial-props-free-3d-asset-pack
- Five high-quality game-ready props; free/pay-what-you-want surfaced.
- Exact license text was not surfaced in search result.
- Verdict: **DO NOT USE until license is explicitly verified.**

## Gate

Preferred path: use a modular structural donor for the surrounding frame but author/modify the gate itself so it reads as a real mechanical threshold instead of a rectangle.

- SilFerTech `Sci-Fi Industrial Pack` includes a separated-part animated gate and PBR textures; $9.99 surfaced.
  URL: https://silfertech.itch.io/sci-fi-industrial-pack
- License terms were not clearly surfaced; therefore status is **PROMISING — LICENSE CHECK REQUIRED**, not buy-now.
- Alternative: custom gate built from Quaternius/OZEA structural frame pieces plus custom rails, leaf panels, pistons and hazard/service decals.

## Hero reactor

No surfaced ready-made reactor currently satisfies the requirement strongly enough to recommend shipping it unchanged.

Preferred: **custom/hybrid landmark** using coherent environment donor components for pipes, structural rings, braces and service machinery while authoring the central energy containment assembly and silhouette.

Required form:
- visibly different from ordinary tanks;
- multi-level containment frame, not one cylinder;
- readable central energy/core volume;
- layered supports / conduit routes / service access;
- top-down readable macro silhouette;
- localized emissive only.

Expected LOD0 target: ~20–35k tris, 2–4 materials after consolidation.

## Recommended pack combination for the first slice

### Economy
Quaternius MEGAKIT structural geometry + selected CC0 materials + custom gate/reactor additions.

### Balanced — preferred
Paid used-future industrial structural pack **only after confirming Blender/FBX access and license**, plus OZEA HS-003 secondary props, custom reactor, custom/remixed gate.

### Quality-first
Custom structural hero modules for the visible combat area, with donor pipes/props/industrial machinery from one coherent commercial family; custom reactor and gate.

## Environment Blender work

For all selected donor pieces:
1. normalize scale to project metric grid;
2. rebuild origins/pivots for snapping;
3. remove unseen backfaces/interior detail where safe;
4. merge excessive material slots;
5. remap to shared GRAVIVORE material families;
6. bake unique donor details where needed into consolidated atlases;
7. add authored wear only at edges/contact/service areas;
8. create mobile LODs only where screen-size benefit justifies them;
9. create simple collision proxies separately;
10. check top-down occlusion and keep tall props from hiding combat;
11. add physical blocking only where gameplay expects it; do not ship pass-through solid-looking machinery;
12. export validated FBX meshes with deterministic naming.

## Selection verdict

**Immediate permissive backbone: Quaternius.**
**Best-looking paid lead: used-future SciFi Industrial Modular Kit, pending source/license portability check.**
**Secondary props: OZEA HS-003.**
**Hero reactor: custom/hybrid required.**
