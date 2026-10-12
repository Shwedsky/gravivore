# GRAVIVORE — Actor and Combat Presentation in the Concept World

Статус: изготовление и регрессия завершены; Android APK готовится из отдельного зафиксированного source commit.
Ветка: `codex/actors-combat-concept-world`. Исходная поставка главы:
`7dcd6652c0c7232aaaed5e134edbfc5ea1c3c7e2`, APK 53 / build source `3b1396e`.
Merge не выполняется.

## Конкретные недостатки и изготовление

Сначала сняты существующие модели в Capacitor Field и Cutting Floor с
четырьмя поворотами, locomotion/attack/death и выключенной эмиссией; отдельно
Magnetar и Custodian в новых аренах. Обнаружены: слишком тёмные механизмы,
слабая отделённость красных оболочек, общий цвет/тип следа обычных атак,
несогласованный габарит стен и idle/attack-ноги при фактическом перемещении.

Ранняя камера evidence-helper читала линейный ARGBHalf напрямую в PNG.
Эти диагностические кадры темнее экранного результата. Итоговый захват
использует ARGB32 sRGB при прежних HDR/URP/ACES/Bloom и игровой камере.
Сопоставление «до» повторяет поверхности и LOD APK 53 в памяти, при той же
камере и неизменной геометрии; исходные PNG/параметры сохранены в Tools/baseline.
Изображения не ретушируются.

Семья заново не изготавливается. Сохранены семь FBX, anatomy, skinning,
in-place clips, сокеты и относительные масштабы. Уточнены графит и сервисный
металл общего trim, существующие карты G-0, отдельные красные окрашенные
оболочки Cutter/Custodian. Механизмы остаются графитовыми. Новый opaque URP
metal BRDF использует прежние base/normal/metal/smoothness/AO/contained-energy
карты, общий reflection environment и реальные источники света. Дешёвое
верхнее/нижнее industrial irradiance задаётся только материалам движущихся
машин; света и камер не добавлено, окружение не переосвещается.

## Движение, атаки и пространство

Для G-0, Magnetar и Custodian существующий Run-клип управляет только ногами/
опорами через Generic mask и нормализованную пройденную дистанцию. Core,
оружие, charge/vent и специальные позы сохраняют основной слой. Клип,
скорость мира и damage commit не меняются. Mask выключен у death/reset.

Wall-only swept envelope учитывает полный аппарат и инструменты до движения
CharacterController. Прежние combat sensors/radii, attack ranges, damage,
intervals, reward IDs и save schema сохраняются. Финальное косметическое
смещение от стены ограничено 0.08 m; существующий краткий gravity pull trail
остаётся отдельным эффектом перемещения. G-0 также имеет wall envelope 1.2 m.
Ближайшая проверяемая позиция у Capacitor перемещена с Z=12.9 на Z=12.1,
чтобы её можно было достичь новым полным габаритом. World meshes, blockers,
route, camera и миникарта не перестроены.

Новая проверка полного корпуса выявила неверную диагональную постановку
около сервиса Custodian: она проходила через reactor footprint. Проверка
расширена на реальные грани и достижимые подходы с полным envelope; отсутствие
подхода не выдавалось за успешную проверку. Установку менять не потребовалось.

Source → travel → impact получают цвет соответствующей роли. Arc Drone
сохраняет синий, Magnetar — amber, Custodian — red. Механические удары
используют короткий дуговой след, парные инструменты чередуют реальные
weapon sockets. Carrier не получает нового оружия/ability. Для Custodian
charge и release используют источники line/circle/cone. Пулы сохраняют
фиксированную ёмкость; цвет и источник сбрасываются при новой жизни.

## Стоимость после объединения

Измеряется настоящая отрисовка всей главы `Camera.Render`, 540×960 sRGB,
по 240 frames в восьми местах после прогрева. Draw/triangles counters —
submitted Editor render work, а не сумма всех meshes/LODs. Editor CPU включает
тестовый runner, URP и Editor; его время не является Android FPS.

Первый измеренный проход показал частое использование LOD0 обычных врагов
и до 1.24 млн submitted triangles. Пороги существующих LOD уточнены по этому
результату; основные формы и rig не decimate/rebuild. G-0 сохраняет свой LOD,
элита/босс сохраняют высокую детализацию в обычном близком кадре.
Финальная повторная стоимость сохранена в
`verification/complete-chapter-render-observation.json`: средние 384–573
submitted draw calls, 0.75–1.21 млн submitted triangles, Editor Main Thread
4.32–5.60 ms; active LOD каждого видимого актёра записан в каждом секторе.
Gfx Used Memory 255–287 MiB относится к всей Editor-сессии; средний
GC Allocated In Frame около 11 KiB включает Editor/test/URP. В сцене
учтены 18 включённых источников света, новые источники не добавлялись.

GPU Frame Time marker в Editor существует, но возвращает 0; время GPU не
измерено. Render Thread recorder недоступен. Memory/GC — полная Editor
среда, не память APK и не доказательство отсутствия allocations в Android.
Отдельный warmed test проверяет нулевые managed allocations wall queries.

Прозрачность проверяется depth-tested additive raster текущих VFX с alpha
cutoff 0.025 и сохранённой opaque depth. UI исключён. Это фактические
диагностические world fragments в выбранных кадрах, а не Android GPU timing
и не гарантия worst-case overdraw.

## Регрессия и поставка

CompileVerified: exit 0. Один предшествующий CompileFinal завершил компиляцию,
но упал при закрытии Unity; его лог сохранён. ProjectValidator: exit 0,
включая industrial actor material/texture audit.

EditModeFinal: **529/529 PASS**. Полный PlayMode с opt-in video:
169 tests, 166 PASS, 2 FAIL, 1 SKIP; длительность 1116.40 s. Оба сбоя
были устаревшими точками маршрута после введения полного габарита G-0:
угловая точка Capacitor и подход к boss gate вплотную к footprint Magnetar.
После исправления **FinalRouteRetry: 2/2 PASS**, 26.46 s; второй тест
проверяет все маршруты действующим PlayerLocomotion, а не прямым body.Move.
Итого закрытый регресс: **168 PASS, 0 unresolved FAIL, 1 SKIP**. Это
полный прогон плюс фактический повтор двух исправленных проверок, а не
переписанный «зелёный» XML полного прогона. Единственный skip — исторический
opt-in structural-foundation capture, не gameplay/actor test.

Проверены ordinary live attacks/source/palette в холодном и тёплом секторах,
wall clearance во всех поворотах/движении/атаках, смерть/новые life IDs,
восстановление G-0, все специальные атаки и reset Magnetar/Custodian,
первый/повторный encounter, exactly-once completion, rewards/save reload,
мини-карта и проходы. Оба существующих пятиминутных soak включены в полный
прогон. Отдельно warmed 250 wall queries: zero managed allocations PASS.
Сырые XML и ранние неуспешные прогоны сохранены как диагностика.

`actors-combat-continuous.mp4`: **1044 frames, 104.4 s, 540×960, 10 fps**,
31 attack sources, 20 ordinary deaths, максимум 20 ordinary enemies.
Одна непрерывная игровая камера: все восемь секторов, элита и босс погибают
от настоящих атак игрока. Изолированный review checkpoint: Power 25,
development gate prerequisites, god mode; без teleport после начала и
без lethal damage injection. Это запись боя, не fresh-save progression
grind и не замер FPS. MP4 декодирован Blender: проверены first/middle/last.

Android build/signature/resource результаты добавляются после сборки APK 54.

`adb devices -l`: подключённых Android-устройств нет. Остаются
непроверенными actual Android CPU/GPU, submitted draws,
memory, GC, 60 FPS, thermal/15-minute soak, touch, background/resume и
kill/relaunch реального профиля. Editor и compile APK их не заменяют.

## Сохраняющиеся художественные ограничения

Установленные формы ближе к функциональным Actor V2, чем к полному объёму
деталей иллюстрации. Мелкие поверхности и вложенные механизмы упрощены,
задние поверхности и LOD читаются спокойнее. Эмиссия локализована и не
заменяет оболочки. Более высокое сходство с живописным светом/плотностью
concept board не объявляется выполненным по зелёным тестам.

Доказательства и окончательный APK предназначены для owner visual gate;
финальное художественное принятие на Android не подменяется этим отчётом.

## Файлы и предположения

Основные изменения: ActorProductionV2 materials/maps/prefabs/HostileMotion,
G0 production base map и три tier materials, ActorIndustrialLit.shader,
moving-support masks/controllers, ActorWallEnvelope и применение к player/
ordinary/elite/boss, VisualSliceAnimationBridge, AttackCausalityPresenter,
Phase6B VFX pool/instances, S15VisualCatalog и composition injection.
Новые editor build audits и integration tests находятся рядом с этими
системами; точный список сохраняется в `verification/files-changed.txt`.

Использованы существующие project-owned ассеты и клипы, без внешних
загрузок и новых лицензий. Предположение: последний подтверждённый
full-chapter source `7dcd665` — действующая основа; APK 53 и его SHA-256
сохранены. Preservation audit подтверждает отсутствие изменений world
meshes/proxies/layout/camera, FBX, balance definitions/IDs и persistence.
Изменение S15_VisualCatalog — только player wall envelope radius.

Автоматическое продолжение к изготовлению новой семьи не задано.
Оставшийся gate — художественная оценка и профилирование APK на Android.
