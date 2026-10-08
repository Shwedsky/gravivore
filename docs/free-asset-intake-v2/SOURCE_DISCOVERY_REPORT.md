# Source discovery report

Baseline: merged PR #66, `a6541ef0c78f1489354bb4a5777de5e5b8847ce5`. Same V1 worktree; V2 branch `art/free-asset-intake-v2`.

Discovered **26 asset archives/packages plus one owner concept board**, 5,000,627,772 input bytes. Original snapshot includes SHA-256 for all 27 files. The planned intake contains placeholders; actual downloads are in worktree-root `98_unclassified/` and `00_reference/`. Both roots are included in discovery and explicitly ignored. No sorting is required from the owner.

Concept image is PRESENT and visually inspected, including its top-down gameplay panel. G-0 stays the approved bipedal mech. Enemy text authority: `docs/visual-production-v2/ENEMY_VISUAL_TARGETS_APPROVED_V1.md`. Scoring is a technical-art recommendation, not owner acceptance or device performance.

26 downloaded art sources are identified; the reference is also FOUND. `pipes-armored` is the single missing non-Meshy source. All 11 expected Meshy sources are SKIPPED_SUBSCRIPTION. No duplicate owner downloads found; embedded pipeline/source archives are recorded as same-source DUPLICATE variants and excluded from Unity.

Important corrections: Striker is MSGDI Asset Store id124342, not the V1 Sketchfab license. RTS is the free v1 id112251; header publisher Vdr0id has id15286, matching current Dmitrii Kutsenko. Kenney UI archive name differs, but included license identifies Sci-fi 2.0. TechLab identity is matched, while exact attribution-license version and duplicate texture mapping remain open. Tiago whole-pack clearance is limited by separately sourced Icons8 icons.

## reference-concept-board — FOUND
- Payload: `00_reference\01_gravivore_concept_board\Концепт GRAVIVORE_ Мехи и окружение.png`
- Match confidence: HIGH; SHA256 `98042c70fccf85b4bd06818661efe080263a982a68537d6d9301da0882a2ced5`
- Owner supplied image at 00_reference/01_gravivore_concept_board
- Image visually inspected: named GRAVIVORE, labelled enemies, environment modules and top-down gameplay frame; bipedal G-0 override applies

## mechs-quaternius — FOUND
- Payload: `98_unclassified\Animated Mech Pack - March 2021-20261008T180817Z-1-001.zip`
- Match confidence: HIGH; SHA256 `882d34a43ce1047f7a872b0d627f8b8247c592076013ebbcfd59474014a750c7`
- Archive identity: Animated Mech Pack - March 2021-20261008T180817Z-1-001.zip
- Members: Animated Mech Pack - March 2021\Flat Colors\Blends\George.blend, Animated Mech Pack - March 2021\Flat Colors\Blends\Leela.blend, Animated Mech Pack - March 2021\Flat Colors\Blends\Mike.blend
- Included Animated Mech Pack - March 2021\License.txt

## vfx-impact — FOUND
- Payload: `98_unclassified\Cartoon VFX by Wallcoeur\Particle Systems\VFX - Impact and Hit - Light Version.unitypackage`
- Match confidence: HIGH; SHA256 `26d91eb96d33a7dee1f89c8980ebcd8bc70c0bfbb8c7f3804da754e326028886`
- Unity gzip FEXTRA id=335800; publisher=Cartoon VFX by Wallcoeur; title=VFX - Impact and Hit - Free; version=1.0
- Reconstructed Unity Assets paths, 1 FBX / 18 prefabs

## env-creepycat-starter — FOUND
- Payload: `98_unclassified\Creepy Cat\3D ModelsEnvironments\3D Scifi Kit Starter Kit.unitypackage`
- Match confidence: HIGH; SHA256 `f2517404e37861daddb70fce041e0e51a192196fa16196812243057f2e41418d`
- Unity gzip FEXTRA id=92152; publisher=Creepy Cat; title=3D Scifi Kit Starter Kit; version=3.5
- Reconstructed Unity Assets paths, 125 FBX / 136 prefabs

## env-dmitrii-industrial — FOUND
- Payload: `98_unclassified\Dmitrii Kutsenko\3D ModelsEnvironmentsIndustrial\RPGFPS Game Assets for PCMobile Industrial Set v20.unitypackage`
- Match confidence: HIGH; SHA256 `9490565d585cd54404cc2c8e8329b7bc7a20509cac87f8e9925cc4ffb76b41d0`
- Unity gzip FEXTRA id=86679; publisher=Dmitrii Kutsenko; title=RPG/FPS Game Assets for PC/Mobile (Industrial Set v2.0); version=2.0
- Reconstructed Unity Assets paths, 36 FBX / 134 prefabs

## weapons-rts-assets — FOUND
- Payload: `98_unclassified\Dmitrii Kutsenko\3D ModelsEnvironmentsSci-Fi\RTS Sci-Fi Game Assets v1.unitypackage`
- Match confidence: HIGH; SHA256 `a46e4a842dc4982ed9f35b4dbe0555265bd4d2076c3e0a1ccd829127f476720c`
- Unity gzip FEXTRA id=112251; publisher=Vdr0id; title=RTS Sci-Fi Game Assets v1; version=1.0
- Reconstructed Unity Assets paths, 9 FBX / 11 prefabs

## ui-exe — FOUND
- Payload: `98_unclassified\EXE - Mini Scifi UI Pack.zip`
- Match confidence: HIGH; SHA256 `8c1e949765faa5d175fcfc07ad2e5a013d960d1f59382fe70994cb3967c5f051`
- Archive identity: EXE - Mini Scifi UI Pack.zip
- Content: {'.jpg': 2, '.pdf': 1, '.ttf': 19, '.txt': 3, '.png': 18}
- Included EXE - Mini Scifi UI Pack\crediting_guide.pdf

## weapons-tower-defence — FOUND
- Payload: `98_unclassified\Firadzo Assets\3D ModelsEnvironmentsSci-Fi\Tower Defence Sci-Fi Turret FREE.unitypackage`
- Match confidence: HIGH; SHA256 `85092593d4f9f71d54219e9851bff43040f9bf1c02bdf3161ddfffda2aca794f`
- Unity gzip FEXTRA id=246331; publisher=Firadzo Assets; title=Tower Defence Sci-Fi Turret; version=1.0
- Reconstructed Unity Assets paths, 1 FBX / 3 prefabs

## vfx-fog — FOUND
- Payload: `98_unclassified\Game Seed Assets\Particle Systems\Fog Particles.unitypackage`
- Match confidence: HIGH; SHA256 `f46c130d15ac4b41f4075d3057db71cbb9c23bd9ab6893a21815bba81a0e923e`
- Unity gzip FEXTRA id=351840; publisher=Game Seed Assets; title=Fog Particles; version=1.0.0
- Reconstructed Unity Assets paths, 0 FBX / 6 prefabs

## vfx-magic — FOUND
- Payload: `98_unclassified\Hovl Studio\Particle SystemsMagic\Magic Effects FREE.unitypackage`
- Match confidence: HIGH; SHA256 `2228de7ba7f19934be8b58c96e1d8ce50f20ac51777fcdaa7b4478ea64d0b44d`
- Unity gzip FEXTRA id=247933; publisher=Hovl Studio; title=Magic Effects FREE; version=1.6
- Reconstructed Unity Assets paths, 4 FBX / 76 prefabs

## env-karboosx-modular — FOUND
- Payload: `98_unclassified\karboosx\3D ModelsEnvironmentsSci-Fi\Sci-Fi Styled Modular Pack.unitypackage`
- Match confidence: HIGH; SHA256 `e92e040b6118f7f60071b14c9f80715c7a6f6671f5e1f5b17ea813bc940294a9`
- Unity gzip FEXTRA id=82913; publisher=karboosx; title=Sci-Fi Styled Modular Pack; version=1.1
- Reconstructed Unity Assets paths, 202 FBX / 152 prefabs

## env-kenney-space — FOUND
- Payload: `98_unclassified\kenney_modular-space-kit_1.0.zip`
- Match confidence: HIGH; SHA256 `f394f7fd9eaf29c9de7e090e55b69926f699841af33b0b116f5cc0088de8a4dc`
- Archive identity: kenney_modular-space-kit_1.0.zip
- Members: Models\FBX format\cables.fbx, Models\FBX format\corridor-corner.fbx, Models\FBX format\corridor-end.fbx
- Included License.txt

## env-kenney-station — FOUND
- Payload: `98_unclassified\kenney_space-station-kit.zip`
- Match confidence: HIGH; SHA256 `215e79bd5415cff93665183390f0343ed9acf87780306331013b78520170c6d8`
- Archive identity: kenney_space-station-kit.zip
- Members: Models\FBX format\balcony-floor-center.fbx, Models\FBX format\balcony-floor-corner.fbx, Models\FBX format\balcony-floor.fbx
- Included License.txt

## ui-kenney — FOUND
- Payload: `98_unclassified\kenney_ui-pack-space-expansion.zip`
- Match confidence: HIGH; SHA256 `4ae5a4949b71ba6c08bfb4d4708b3880915782f7deae7bc5872e1d56f0a668af`
- Archive identity: kenney_ui-pack-space-expansion.zip
- Content: {'.ttf': 2, '.txt': 1, '.png': 742, '.svg': 370, '.url': 2}
- Included License.txt

## weapons-quaternius — FOUND
- Payload: `98_unclassified\Modular Sci Fi Guns - Nov 2021-20261008T181022Z-1-001.zip`
- Match confidence: HIGH; SHA256 `75f1d86858a4f2d27a54f9901f8264203a6812976b943c956a28161deb2e9847`
- Archive identity: Modular Sci Fi Guns - Nov 2021-20261008T181022Z-1-001.zip
- Members: Modular Sci Fi Guns - Nov 2021\Guns\Blends\AR_1.blend, Modular Sci Fi Guns - Nov 2021\Guns\Blends\AR_2.blend, Modular Sci Fi Guns - Nov 2021\Guns\Blends\AR_3.blend
- Included Modular Sci Fi Guns - Nov 2021\License.txt

## env-quaternius-megakit — FOUND
- Payload: `98_unclassified\Modular SciFi MegaKit[Standard].zip`
- Match confidence: HIGH; SHA256 `6fae60cf5189e44dff0bd91097f094a765acc6d57d64a85a0cc0dd56e03035e3`
- Archive identity: Modular SciFi MegaKit[Standard].zip
- Members: Modular SciFi MegaKit[Standard]\FBX\Aliens\Alien_Cyclop.fbx, Modular SciFi MegaKit[Standard]\FBX\Aliens\Alien_Oculichrysalis.fbx, Modular SciFi MegaKit[Standard]\FBX\Aliens\Alien_Scolitex.fbx
- Included Modular SciFi MegaKit[Standard]\License_Standard.txt

## env-molten-maps — FOUND
- Payload: `98_unclassified\Molten Maps SciFi Asset Pack.zip`
- Match confidence: HIGH; SHA256 `3c8b4560da0b68848447355feb20d6e313c8a5078a449b70501cc86a56e65874`
- Archive identity: Molten Maps SciFi Asset Pack.zip
- Members: Molten Maps SciFi Asset Pack\Assets\fbx\3D Chess Board.fbx, Molten Maps SciFi Asset Pack\Assets\fbx\Air Conditioner.fbx, Molten Maps SciFi Asset Pack\Assets\fbx\Battery Blue.fbx
- Included Molten Maps SciFi Asset Pack\License.txt

## mechs-medium-striker — FOUND
- Payload: `98_unclassified\MSGDI\3D ModelsCharactersRobots\Medium Mech Striker.unitypackage`
- Match confidence: HIGH; SHA256 `aecd6560cc41382e2fdaef6137f72e8a5ee79112ccebb9973ab74f7748a6789b`
- Unity gzip FEXTRA id=124342; publisher=MSGDI; title=Medium Mech Striker; version=1.1
- Reconstructed Unity Assets paths, 26 FBX / 51 prefabs

## env-seed-hunter — FOUND
- Payload: `98_unclassified\POLYGONAUTIC\3D ModelsEnvironments\Seed Hunter.unitypackage`
- Match confidence: HIGH; SHA256 `8fa87270832fb402b87a96454a5939f6995563e0778d40ba936182f2f0e2be6b`
- Unity gzip FEXTRA id=143414; publisher=POLYGONAUTIC; title=Seed Hunter; version=1.3
- Reconstructed Unity Assets paths, 84 FBX / 71 prefabs

## env-quaternius-essentials — FOUND
- Payload: `98_unclassified\Sci-Fi Essentials Kit[Standard].zip`
- Match confidence: HIGH; SHA256 `a08346d538aa39fbea9fa492e03620d1860fc6214eedd62a4f5db373ac6fca01`
- Archive identity: Sci-Fi Essentials Kit[Standard].zip
- Members: FBX\Enemy_EyeDrone.fbx, FBX\Enemy_QuadShell.fbx, FBX\Enemy_Trilobite.fbx
- Included License_Standard.txt

## env-sickhead-construction — FOUND
- Payload: `98_unclassified\Sickhead Games\3D ModelsEnvironmentsSci-Fi\Sci-Fi Construction Kit Modular.unitypackage`
- Match confidence: HIGH; SHA256 `4a3248c29518a647a0944508757a68b03b16364a885e867db899cdf4e8f52ac9`
- Unity gzip FEXTRA id=159280; publisher=Sickhead Games; title=Sci-Fi Construction Kit (Modular); version=1.1.0
- Reconstructed Unity Assets paths, 83 FBX / 98 prefabs

## vfx-black-hole — FOUND
- Payload: `98_unclassified\SOLODREAM CREATION\VFX\Free Asset Black Hole Effect.unitypackage`
- Match confidence: HIGH; SHA256 `2a7389dddb18bb8fd9432ee124c1ad85f2ebfaadba46600462a054600905efe0`
- Unity gzip FEXTRA id=356686; publisher=SOLODREAM CREATION; title=Black Hole Effect; version=1.0.0
- Reconstructed Unity Assets paths, 0 FBX / 2 prefabs

## pipes-techlab — FOUND
- Payload: `98_unclassified\techlab-modular-scifi-pipes.zip`
- Match confidence: MEDIUM; SHA256 `26c97126622b9d11cf5abbbbfce6a98777530e48ad28f80c4ce32629059cc2e5`
- Archive identity: techlab-modular-scifi-pipes.zip
- Members: source\TechPipes_Scene.fbx
- Indexed author listing names TechLab/TooManyDemons and a 20-piece set; sole TechPipes_Scene.fbx plus matching TechLab trimsheets

## ui-tiago — FOUND
- Payload: `98_unclassified\UI Sci-Fi Files.zip`
- Match confidence: HIGH; SHA256 `76324d35334feee24c6169db205bc5c0d9297363282269c9289af3bee2687d51`
- Archive identity: UI Sci-Fi Files.zip
- Content: {'.pdf': 1, '.psd': 2, '.png': 5}
- Official publisher page names exact UI Sci-Fi Files.zip (3.7MB), two PSD dimensions match local 2000x730 / 3175x2082

## weapons-warzone — FOUND
- Payload: `98_unclassified\VDGames\3D ModelsEnvironments\WarZone Sci-Fi Turret pack.unitypackage`
- Match confidence: HIGH; SHA256 `5ed67acc4f8579511219c484bd171c33ae880b3cc4158dd7f15a347c6f01d0d9`
- Unity gzip FEXTRA id=57540; publisher=VDGames; title=WarZone Sci-Fi Turret pack; version=2.0
- Reconstructed Unity Assets paths, 14 FBX / 0 prefabs

## vfx-fire — FOUND
- Payload: `98_unclassified\Vefects\Particle SystemsFire\Free Fire VFX - URP.unitypackage`
- Match confidence: HIGH; SHA256 `dd1a097cd80c25ecf98f0db959b6a03971671cf2c44c7e56e69abfb9e1eb6204`
- Unity gzip FEXTRA id=266226; publisher=Vefects; title=Free Fire VFX - URP; version=1.0.2023.1
- Reconstructed Unity Assets paths, 3 FBX / 18 prefabs

## mechs-combat-drone — FOUND
- Payload: `98_unclassified\VoodooPlay\3D ModelsCharactersRobots\Low poly combat drone.unitypackage`
- Match confidence: HIGH; SHA256 `4ef3f8d00aabd1a016f2542729d611365189584f6eaf50cd853e97a8c1bc5b7b`
- Unity gzip FEXTRA id=82234; publisher=VoodooPlay; title=Low poly combat drone; version=1.0
- Reconstructed Unity Assets paths, 1 FBX / 1 prefabs

## Expected sources without payloads

- `pipes-armored`: **MISSING** — No matching local payload
- `meshy-scout-sentinel-drone`: **SKIPPED_SUBSCRIPTION** — Owner explicitly skipped subscription-only Meshy downloads
- `meshy-scout-armored-hover-drone`: **SKIPPED_SUBSCRIPTION** — Owner explicitly skipped subscription-only Meshy downloads
- `meshy-cutter-quadruped-combat-drone`: **SKIPPED_SUBSCRIPTION** — Owner explicitly skipped subscription-only Meshy downloads
- `meshy-elite-quadruped-military-drone`: **SKIPPED_SUBSCRIPTION** — Owner explicitly skipped subscription-only Meshy downloads
- `meshy-custodian-spider-candidate`: **SKIPPED_SUBSCRIPTION** — Owner explicitly skipped subscription-only Meshy downloads
- `meshy-custodian-railgun-spider-candidate`: **SKIPPED_SUBSCRIPTION** — Owner explicitly skipped subscription-only Meshy downloads
- `meshy-heavy-turret-donor`: **SKIPPED_SUBSCRIPTION** — Owner explicitly skipped subscription-only Meshy downloads
- `meshy-generator-donor`: **SKIPPED_SUBSCRIPTION** — Owner explicitly skipped subscription-only Meshy downloads
- `meshy-scifi-crate-donor`: **SKIPPED_SUBSCRIPTION** — Owner explicitly skipped subscription-only Meshy downloads
- `meshy-weapon-rpk7`: **SKIPPED_SUBSCRIPTION** — Owner explicitly skipped subscription-only Meshy downloads
- `meshy-weapon-heavy-energy-gun`: **SKIPPED_SUBSCRIPTION** — Owner explicitly skipped subscription-only Meshy downloads
