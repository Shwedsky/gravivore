using System;
using System.Collections.Generic;
using System.IO;
using Gravivore.Core;
using Gravivore.Gameplay.Combat;
using Gravivore.Gameplay.Enemies;
using Gravivore.Gameplay.Encounters;
using Gravivore.Gameplay.Equipment;
using Gravivore.Gameplay.Player;
using Gravivore.Gameplay.Progression;
using Gravivore.Gameplay.Quests;
using Gravivore.Gameplay.World;
using Gravivore.Persistence.Profile;
using Gravivore.Presentation.Evolution;
using Gravivore.Presentation.Quests;
using Gravivore.Presentation.World;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Gravivore.Editor
{
    public static class ProjectValidator
    {
        private static readonly string[] RequiredPaths =
        {
            "Assets/_Game/Runtime/Core/Gravivore.Core.asmdef",
            "Assets/_Game/Runtime/Gameplay/Gravivore.Gameplay.asmdef",
            "Assets/_Game/Runtime/Persistence/Gravivore.Persistence.asmdef",
            "Assets/_Game/Runtime/Platform/Gravivore.Platform.asmdef",
            "Assets/_Game/Runtime/Presentation/Gravivore.Presentation.asmdef",
            "Assets/_Game/Content/Scenes/Bootstrap.unity",
            "Assets/_Game/Content/Scenes/Chapter01_ScrapExclusion.unity",
            "Assets/_Game/Content/Definitions/S01_PlayerMovementSettings.asset",
            "Assets/_Game/Content/Definitions/S01_FloatingJoystickSettings.asset",
            "Assets/_Game/Content/Definitions/S01_CameraFollowSettings.asset",
            "Assets/_Game/Content/Definitions/S02_PowerCurve.asset",
            "Assets/_Game/Content/Definitions/S02_HullCurve.asset",
            "Assets/_Game/Content/Definitions/S02_ArmorCurve.asset",
            "Assets/_Game/Content/Definitions/S02_FluxCurve.asset",
            "Assets/_Game/Content/Definitions/S02_MobilityCurve.asset",
            "Assets/_Game/Content/Definitions/S02_PlayerStats.asset",
            "Assets/_Game/Content/Definitions/S03_GravityAttackSettings.asset",
            "Assets/_Game/Content/Definitions/S04_Enemy_ScoutDrone.asset",
            "Assets/_Game/Content/Definitions/S04_Enemy_CutterUnit.asset",
            "Assets/_Game/Content/Definitions/S04_Enemy_Warden.asset",
            "Assets/_Game/Content/Definitions/S04_Enemy_ArcDrone.asset",
            "Assets/_Game/Content/Definitions/S04_Enemy_Carrier.asset",
            "Assets/_Game/Content/Definitions/S04_SpawnSpot_RelayYard.asset",
            "Assets/_Game/Content/Definitions/S04_SpawnSpot_CuttingFloor.asset",
            "Assets/_Game/Content/Definitions/S04_SpawnSpot_ShieldDump.asset",
            "Assets/_Game/Content/Definitions/S04_SpawnSpot_CapacitorField.asset",
            "Assets/_Game/Content/Definitions/S04_SpawnSpot_HaulerGraveyard.asset",
            "Assets/_Game/Content/Definitions/S06_ProgressionThresholds.asset",
            "Assets/_Game/Content/Definitions/S06_CoreReward_ScoutDrone.asset",
            "Assets/_Game/Content/Definitions/S06_CoreReward_CutterUnit.asset",
            "Assets/_Game/Content/Definitions/S06_CoreReward_Warden.asset",
            "Assets/_Game/Content/Definitions/S06_CoreReward_ArcDrone.asset",
            "Assets/_Game/Content/Definitions/S06_CoreReward_Carrier.asset",
            "Assets/_Game/Content/Definitions/S06_PlayerProgression.asset",
            "Assets/_Game/Content/Definitions/S07_Evolution.asset",
            "Assets/_Game/Content/Definitions/S08_Chapter01World.asset",
            "Assets/_Game/Content/Definitions/S09_MagnetarGuard.asset",
            "Assets/_Game/Content/Definitions/S09_CustodianM0.asset",
            "Assets/_Game/Content/Definitions/S10_Equipment_GraviticFang.asset",
            "Assets/_Game/Content/Definitions/S10_Equipment_PulseCapacitor.asset",
            "Assets/_Game/Content/Definitions/S10_Equipment_LayeredCarapace.asset",
            "Assets/_Game/Content/Definitions/S10_Equipment_ImpactFrame.asset",
            "Assets/_Game/Content/Definitions/S10_Equipment_VectorFins.asset",
            "Assets/_Game/Content/Definitions/S10_Equipment_FluxVanes.asset",
            "Assets/_Game/Content/Definitions/S10_EquipmentCatalog.asset",
            "Assets/_Game/Content/Definitions/S11_Chapter01OnboardingQuest.asset",
            "Assets/_Game/Content/Definitions/S11_OnboardingPresentation.asset",
            "Assets/_Game/Content/Definitions/S12_SaveOffline.asset",
            UrpConfigurator.UrpAssetPath,
            UrpConfigurator.RendererDataPath,
            "build-android.ps1"
        };

        [MenuItem("Gravivore/Validation/Validate Project")]
        public static void ValidateProjectMenu()
        {
            ValidateOrThrow();
            Debug.Log("GRAVIVORE project validation passed.");
        }

        public static void ValidateOrThrow()
        {
            foreach (var path in RequiredPaths)
            {
                if (!File.Exists(path) && !Directory.Exists(path))
                {
                    throw new FileNotFoundException($"Required bootstrap file is missing: {path}");
                }
            }

            ValidateEditorBuildSettings();
            ValidateUrpConfiguration();
            ValidateAndroidPlayerSettings();
            ValidateVersion();
            ValidatePlayerStats();
            ValidateGravityAttack();
            ValidateEnemySpawnSpots();
            ValidateProgression();
            ValidateEvolution();
            ValidateChapter01World();
            ValidateEliteAndBossEncounters();
            ValidateEquipment();
            ValidateQuests();
            ValidateSaveOffline();
        }

        private static void ValidateSaveOffline()
        {
            const string definitionPath = "Assets/_Game/Content/Definitions/S12_SaveOffline.asset";
            const string scenePath = "Assets/_Game/Content/Scenes/Chapter01_ScrapExclusion.unity";
            var definition = AssetDatabase.LoadAssetAtPath<SaveOfflineDefinition>(definitionPath);
            if (definition == null)
            {
                throw new InvalidOperationException("Canonical S12 save/offline definition is required.");
            }

            definition.ValidateOrThrow();
            var configuration = definition.Configuration;
            if (configuration.CurrentSchemaVersion <= 0 ||
                configuration.OfflineReward.MaximumEligibleDuration != TimeSpan.FromHours(2) ||
                configuration.MinimumResumeAbsence != TimeSpan.FromSeconds(60) ||
                configuration.OfflineReward.Efficiency <= 0d ||
                configuration.OfflineReward.Efficiency > 1d ||
                double.IsNaN(configuration.OfflineReward.ActiveBaselineUnitsPerHour) ||
                double.IsInfinity(configuration.OfflineReward.ActiveBaselineUnitsPerHour) ||
                configuration.OfflineReward.ActiveBaselineUnitsPerHour < 0d ||
                float.IsNaN(configuration.AutosaveDelaySeconds) ||
                float.IsInfinity(configuration.AutosaveDelaySeconds) ||
                configuration.AutosaveDelaySeconds < 0.1f ||
                configuration.AutosaveDelaySeconds > 10f)
            {
                throw new InvalidOperationException("Canonical S12 save/offline values are invalid.");
            }

            var dependencies = AssetDatabase.GetDependencies(scenePath, true);
            if (Array.IndexOf(dependencies, definitionPath) < 0)
            {
                throw new InvalidOperationException("The canonical chapter scene must reference the S12 save/offline definition.");
            }
        }

        private static void ValidateQuests()
        {
            const string questPath = "Assets/_Game/Content/Definitions/S11_Chapter01OnboardingQuest.asset";
            const string presentationPath = "Assets/_Game/Content/Definitions/S11_OnboardingPresentation.asset";
            const string worldPath = "Assets/_Game/Content/Definitions/S08_Chapter01World.asset";
            const string scenePath = "Assets/_Game/Content/Scenes/Chapter01_ScrapExclusion.unity";
            var questDefinition = AssetDatabase.LoadAssetAtPath<QuestDefinition>(questPath);
            var presentation = AssetDatabase.LoadAssetAtPath<QuestOnboardingDefinition>(presentationPath);
            var worldDefinition = AssetDatabase.LoadAssetAtPath<Chapter01WorldDefinition>(worldPath);
            if (questDefinition == null || presentation == null || worldDefinition == null)
            {
                throw new InvalidOperationException("Canonical S11 quest, onboarding presentation, and S08 world definitions are required.");
            }

            presentation.ValidateOrThrow();
            var catalog = questDefinition.Catalog;
            if (!string.Equals(catalog.QuestId, "chapter01-onboarding", StringComparison.Ordinal))
            {
                throw new InvalidOperationException("Canonical S11 quest id is invalid.");
            }

            var expectedSpotObjectives = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                { "intro-relay-yard", "relay-yard|scout-drone" },
                { "intro-cutting-floor", "cutting-floor|cutter-unit" },
                { "intro-shield-dump", "shield-dump|warden" },
                { "intro-capacitor-field", "capacitor-field|arc-drone" },
                { "intro-hauler-graveyard", "hauler-graveyard|carrier" }
            };
            var foundSpotObjectives = new HashSet<string>(StringComparer.Ordinal);
            var hasMovement = false;
            var hasElite = false;
            var hasBoss = false;
            for (var i = 0; i < catalog.ObjectiveCount; i++)
            {
                var objective = catalog.GetObjective(i);
                if (objective.Type == QuestObjectiveType.MovementPerformed) hasMovement = true;
                if (objective.Type == QuestObjectiveType.EliteDefeated &&
                    string.Equals(objective.EncounterId, "magnetar-guard", StringComparison.Ordinal)) hasElite = true;
                if (objective.Type == QuestObjectiveType.BossDefeated &&
                    string.Equals(objective.EncounterId, "custodian-m0", StringComparison.Ordinal)) hasBoss = true;
                if (expectedSpotObjectives.TryGetValue(objective.Id, out var expected))
                {
                    var actual = objective.SpotId + "|" + objective.EnemyId;
                    if (!string.Equals(actual, expected, StringComparison.Ordinal) ||
                        objective.TargetType != QuestTargetType.FarmingZone ||
                        !string.Equals(objective.TargetId, objective.SpotId, StringComparison.Ordinal))
                    {
                        throw new InvalidOperationException($"Canonical spot objective {objective.Id} has invalid mapping.");
                    }

                    foundSpotObjectives.Add(objective.Id);
                }
            }

            if (!hasMovement || !hasElite || !hasBoss || foundSpotObjectives.Count != 5)
            {
                throw new InvalidOperationException("S11 requires movement, five spot, elite, and boss objectives.");
            }

            var world = worldDefinition.Configuration;
            if (world.EliteRequirement.RequiredObjectiveCount != expectedSpotObjectives.Count)
            {
                throw new InvalidOperationException("Elite requirement must reference exactly the five S11 introductory spot objectives.");
            }

            for (var i = 0; i < world.EliteRequirement.RequiredObjectiveCount; i++)
            {
                var objectiveId = world.EliteRequirement.GetRequiredObjectiveId(i);
                if (!expectedSpotObjectives.ContainsKey(objectiveId))
                {
                    throw new InvalidOperationException("Elite requirement references an unexpected S11 objective id.");
                }
            }

            var dependencies = AssetDatabase.GetDependencies(scenePath, true);
            if (Array.IndexOf(dependencies, questPath) < 0 || Array.IndexOf(dependencies, presentationPath) < 0)
            {
                throw new InvalidOperationException("The canonical chapter scene must reference the S11 quest and presentation definitions.");
            }
        }

        private static void ValidateEquipment()
        {
            const string catalogPath = "Assets/_Game/Content/Definitions/S10_EquipmentCatalog.asset";
            const string scenePath = "Assets/_Game/Content/Scenes/Chapter01_ScrapExclusion.unity";
            var definition = AssetDatabase.LoadAssetAtPath<EquipmentCatalogDefinition>(catalogPath);
            if (definition == null)
            {
                throw new InvalidOperationException($"A valid equipment catalog is required at {catalogPath}.");
            }

            definition.ValidateOrThrow();
            var catalog = definition.Catalog;
            if (catalog.Count < 4 || catalog.Count > 6 || Enum.GetValues(typeof(EquipmentSlot)).Length > 3)
            {
                throw new InvalidOperationException("The S10 catalog requires four to six canonical items and at most three slot types.");
            }

            var expectedIds = new HashSet<string>(StringComparer.Ordinal)
            {
                "gravitic-fang",
                "pulse-capacitor",
                "layered-carapace",
                "impact-frame",
                "vector-fins",
                "flux-vanes"
            };
            var representedSlots = new HashSet<EquipmentSlot>();
            for (var i = 0; i < catalog.Count; i++)
            {
                var item = catalog.GetAt(i);
                if (!expectedIds.Remove(item.Id))
                {
                    throw new InvalidOperationException($"Unexpected or duplicate canonical equipment id: {item.Id}.");
                }

                representedSlots.Add(item.Slot);
            }

            if (expectedIds.Count != 0 || representedSlots.Count != 3)
            {
                throw new InvalidOperationException("Canonical equipment must contain six known items across exactly three slots.");
            }

            var dependencies = AssetDatabase.GetDependencies(scenePath, true);
            if (Array.IndexOf(dependencies, catalogPath) < 0)
            {
                throw new InvalidOperationException("The canonical chapter scene must reference the S10 equipment catalog.");
            }
        }

        private static void ValidateEliteAndBossEncounters()
        {
            const string elitePath = "Assets/_Game/Content/Definitions/S09_MagnetarGuard.asset";
            const string bossPath = "Assets/_Game/Content/Definitions/S09_CustodianM0.asset";
            const string worldPath = "Assets/_Game/Content/Definitions/S08_Chapter01World.asset";
            const string scenePath = "Assets/_Game/Content/Scenes/Chapter01_ScrapExclusion.unity";
            var eliteDefinition = AssetDatabase.LoadAssetAtPath<MagnetarGuardDefinition>(elitePath);
            var bossDefinition = AssetDatabase.LoadAssetAtPath<CustodianBossDefinition>(bossPath);
            var worldDefinition = AssetDatabase.LoadAssetAtPath<Chapter01WorldDefinition>(worldPath);
            if (eliteDefinition == null || bossDefinition == null || worldDefinition == null)
            {
                throw new InvalidOperationException("Canonical S08/S09 encounter definitions are required.");
            }

            eliteDefinition.ValidateOrThrow();
            var world = worldDefinition.Configuration;
            bossDefinition.ValidateOrThrow(world);
            var elite = eliteDefinition.Configuration;
            var boss = bossDefinition.CreateConfiguration(world);
            if (!string.Equals(elite.Id, "magnetar-guard", StringComparison.Ordinal) ||
                !string.Equals(elite.Id, world.EliteEnemyId, StringComparison.Ordinal) ||
                elite.DisplacementClass == DisplacementClass.Standard ||
                elite.DisplacementClass == DisplacementClass.Boss)
            {
                throw new InvalidOperationException("Magnetar Guard id and reduced-displacement class must match the S08 world contract.");
            }

            var eliteClearance = elite.CollisionRadius;
            if (!world.Bounds.Contains(elite.SpawnPosition, eliteClearance) ||
                elite.SpawnPosition.z <= world.EliteGate.Position.z + world.EliteGate.Size.z * 0.5f ||
                elite.SpawnPosition.z >= world.BossGate.Position.z - world.BossGate.Size.z * 0.5f ||
                Mathf.Abs(elite.SpawnPosition.x - world.EliteGate.Position.x) + eliteClearance > world.EliteGate.Size.x * 0.5f)
            {
                throw new InvalidOperationException("Magnetar Guard must fit between the elite and boss gates in the canonical encounter lane.");
            }

            if (!string.Equals(boss.Id, "custodian-m0", StringComparison.Ordinal) ||
                boss.DisplacementClass != DisplacementClass.Boss ||
                !world.Bounds.Contains(boss.StartPosition, boss.CollisionRadius) ||
                Vector3.Distance(boss.StartPosition, world.BossArenaCenter) > world.BossArenaRadius - boss.CollisionRadius ||
                boss.AttackSequenceCount != 3)
            {
                throw new InvalidOperationException("Custodian M-0 id, immunity, sequence, start position, or arena placement is invalid.");
            }

            var attacks = new[] { BossAttackType.CirclePulse, BossAttackType.ConeSweep, BossAttackType.LineCharge };
            for (var i = 0; i < attacks.Length; i++)
            {
                var attack = boss.GetAttack(attacks[i]);
                if (attack.TelegraphDuration < CustodianBossConfiguration.MinimumTelegraphDuration ||
                    attack.TelegraphDuration > CustodianBossConfiguration.MaximumTelegraphDuration ||
                    float.IsNaN(attack.Damage) || float.IsInfinity(attack.Damage) || attack.Damage <= 0f)
                {
                    throw new InvalidOperationException($"Boss attack {attacks[i]} has invalid damage or warning timing.");
                }

                if (boss.GetAttackAtSequenceIndex(i) != attacks[i])
                {
                    throw new InvalidOperationException("The canonical deterministic boss sequence must include all three attacks in design order.");
                }
            }

            var dependencies = AssetDatabase.GetDependencies(scenePath, true);
            if (Array.IndexOf(dependencies, elitePath) < 0 || Array.IndexOf(dependencies, bossPath) < 0)
            {
                throw new InvalidOperationException("The canonical chapter scene must reference both S09 encounter definitions.");
            }
        }

        private static void ValidateChapter01World()
        {
            const string definitionPath = "Assets/_Game/Content/Definitions/S08_Chapter01World.asset";
            const string scenePath = "Assets/_Game/Content/Scenes/Chapter01_ScrapExclusion.unity";
            var definition = AssetDatabase.LoadAssetAtPath<Chapter01WorldDefinition>(definitionPath);
            if (definition == null)
            {
                throw new InvalidOperationException($"A valid Chapter 01 world definition is required at {definitionPath}.");
            }

            var configuration = definition.Configuration;
            var expected = new Dictionary<string, Vector3>(StringComparer.Ordinal)
            {
                { "relay-yard", new Vector3(-8f, 0f, 6f) },
                { "cutting-floor", new Vector3(0f, 0f, 10f) },
                { "shield-dump", new Vector3(8f, 0f, 6f) },
                { "capacitor-field", new Vector3(-7f, 0f, -7f) },
                { "hauler-graveyard", new Vector3(7f, 0f, -7f) }
            };
            var spawnConfigurations = new Dictionary<string, SpawnSpotRuntimeConfiguration>(StringComparer.Ordinal);
            var spawnPaths = new[]
            {
                "Assets/_Game/Content/Definitions/S04_SpawnSpot_RelayYard.asset",
                "Assets/_Game/Content/Definitions/S04_SpawnSpot_CuttingFloor.asset",
                "Assets/_Game/Content/Definitions/S04_SpawnSpot_ShieldDump.asset",
                "Assets/_Game/Content/Definitions/S04_SpawnSpot_CapacitorField.asset",
                "Assets/_Game/Content/Definitions/S04_SpawnSpot_HaulerGraveyard.asset"
            };
            for (var i = 0; i < spawnPaths.Length; i++)
            {
                var spawn = AssetDatabase.LoadAssetAtPath<SpawnSpotDefinition>(spawnPaths[i]);
                var spawnConfiguration = spawn.CreateRuntimeConfiguration();
                spawnConfigurations.Add(spawn.Id, spawnConfiguration);
            }
            if (configuration.ZoneCount != expected.Count || configuration.EliteRequirement.RequiredObjectiveCount != expected.Count)
            {
                throw new InvalidOperationException("Chapter 01 requires five canonical zones and five quest-objective requirements.");
            }

            var zoneColors = new HashSet<Color>();
            for (var i = 0; i < configuration.ZoneCount; i++)
            {
                var zone = configuration.GetZone(i);
                if (!expected.TryGetValue(zone.Id, out var expectedCenter) ||
                    Vector3.Distance(zone.Center, expectedCenter) > 0.01f ||
                    !spawnConfigurations.TryGetValue(zone.Id, out var spawnConfiguration) ||
                    Vector3.Distance(zone.Center, spawnConfiguration.WorldOrigin) > 0.01f)
                {
                    throw new InvalidOperationException($"World zone {zone.Id} does not match its S04 spawn origin.");
                }

                if (Vector3.Distance(zone.Center, zone.LandmarkPosition) < 2.5f)
                {
                    throw new InvalidOperationException($"World landmark {zone.Id} overlaps its spawn anchors.");
                }

                if (!configuration.Bounds.Contains(zone.Center))
                {
                    throw new InvalidOperationException($"World zone {zone.Id} is outside the movement ground.");
                }

                for (var anchorIndex = 0; anchorIndex < spawnConfiguration.AnchorOffsets.Length; anchorIndex++)
                {
                    if (!configuration.Bounds.Contains(spawnConfiguration.GetAnchorWorldPosition(anchorIndex), 0.1f))
                    {
                        throw new InvalidOperationException(
                            $"Spawn anchor {anchorIndex} in {zone.Id} intersects or exceeds the world boundaries.");
                    }
                }

                if (!zoneColors.Add(zone.Color))
                {
                    throw new InvalidOperationException("Each canonical zone requires a distinct visual landmark color.");
                }
            }

            var expectedObjectiveIds = new HashSet<string>(StringComparer.Ordinal)
            {
                "intro-relay-yard",
                "intro-cutting-floor",
                "intro-shield-dump",
                "intro-capacitor-field",
                "intro-hauler-graveyard"
            };
            for (var i = 0; i < configuration.EliteRequirement.RequiredObjectiveCount; i++)
            {
                if (!expectedObjectiveIds.Remove(configuration.EliteRequirement.GetRequiredObjectiveId(i)))
                {
                    throw new InvalidOperationException("Elite gate requirements must use the five canonical S11 spot objective ids.");
                }
            }

            if (configuration.EliteRequirement.MinimumAssimilationScore < 1 ||
                string.Equals(configuration.EliteGate.Id, configuration.BossGate.Id, StringComparison.Ordinal) ||
                configuration.EliteGate.Position.z >= configuration.BossGate.Position.z ||
                configuration.BossGate.Position.z >= configuration.BossArenaCenter.z ||
                !configuration.Bounds.ContainsRectangle(
                    configuration.EliteGate.Position,
                    new Vector2(configuration.EliteGate.Size.x, configuration.EliteGate.Size.z)) ||
                !configuration.Bounds.ContainsRectangle(
                    configuration.BossGate.Position,
                    new Vector2(configuration.BossGate.Size.x, configuration.BossGate.Size.z)) ||
                !configuration.Bounds.ContainsCircle(configuration.BossArenaCenter, configuration.BossArenaRadius) ||
                configuration.Boundary.Height < configuration.EliteGate.Size.y ||
                configuration.Boundary.Height < configuration.BossGate.Size.y)
            {
                throw new InvalidOperationException("World bounds, gate geometry, or elite gate requirement are invalid.");
            }

            var dependencies = AssetDatabase.GetDependencies(scenePath, true);
            if (Array.IndexOf(dependencies, definitionPath) < 0)
            {
                throw new InvalidOperationException("The canonical chapter scene must reference the canonical world definition.");
            }
        }

        private static void ValidateEvolution()
        {
            const string definitionPath = "Assets/_Game/Content/Definitions/S07_Evolution.asset";
            const string scenePath = "Assets/_Game/Content/Scenes/Chapter01_ScrapExclusion.unity";
            var definition = AssetDatabase.LoadAssetAtPath<EvolutionDefinition>(definitionPath);
            if (definition == null)
            {
                throw new InvalidOperationException($"A valid evolution definition is required at {definitionPath}.");
            }

            definition.ValidateOrThrow();
            var dependencies = AssetDatabase.GetDependencies(scenePath, true);
            var found = false;
            for (var i = 0; i < dependencies.Length; i++)
            {
                if (string.Equals(dependencies[i], definitionPath, StringComparison.Ordinal))
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                throw new InvalidOperationException("The canonical chapter scene must reference the canonical evolution definition.");
            }
        }

        private static void ValidateProgression()
        {
            const string path = "Assets/_Game/Content/Definitions/S06_PlayerProgression.asset";
            var definition = AssetDatabase.LoadAssetAtPath<PlayerProgressionDefinition>(path);
            if (definition == null)
            {
                throw new InvalidOperationException($"A valid player progression definition is required at {path}.");
            }

            var configuration = definition.Configuration;
            var expectedRoutes = new[]
            {
                "scout-drone",
                "cutter-unit",
                "warden",
                "arc-drone",
                "carrier"
            };
            if (configuration.RewardCount != expectedRoutes.Length)
            {
                throw new InvalidOperationException("The vertical slice requires exactly five ordinary-enemy reward routes.");
            }

            var routedStats = new HashSet<PlayerStatType>();
            for (var i = 0; i < expectedRoutes.Length; i++)
            {
                if (!configuration.TryGetReward(expectedRoutes[i], out var reward))
                {
                    throw new InvalidOperationException($"Missing progression reward route for {expectedRoutes[i]}.");
                }

                if (!routedStats.Add(reward.Stat))
                {
                    throw new InvalidOperationException($"Multiple ordinary enemy routes reward {reward.Stat}.");
                }
            }

            if (routedStats.Count != 5)
            {
                throw new InvalidOperationException("The five ordinary enemies must route to all five player stats.");
            }
        }

        private static void ValidateEnemySpawnSpots()
        {
            var spotPaths = new[]
            {
                "Assets/_Game/Content/Definitions/S04_SpawnSpot_RelayYard.asset",
                "Assets/_Game/Content/Definitions/S04_SpawnSpot_CuttingFloor.asset",
                "Assets/_Game/Content/Definitions/S04_SpawnSpot_ShieldDump.asset",
                "Assets/_Game/Content/Definitions/S04_SpawnSpot_CapacitorField.asset",
                "Assets/_Game/Content/Definitions/S04_SpawnSpot_HaulerGraveyard.asset"
            };
            var ids = new HashSet<string>(StringComparer.Ordinal);
            var enemyIds = new HashSet<string>(StringComparer.Ordinal);
            for (var i = 0; i < spotPaths.Length; i++)
            {
                var definition = AssetDatabase.LoadAssetAtPath<SpawnSpotDefinition>(spotPaths[i]);
                if (definition == null)
                {
                    throw new InvalidOperationException($"A valid spawn spot definition is required at {spotPaths[i]}.");
                }

                var configuration = definition.CreateRuntimeConfiguration();
                if (!ids.Add(definition.Id))
                {
                    throw new InvalidOperationException($"Duplicate spawn spot id: {definition.Id}.");
                }

                if (!enemyIds.Add(configuration.Enemy.Id))
                {
                    throw new InvalidOperationException(
                        $"Each vertical-slice spot requires a distinct ordinary enemy definition: {configuration.Enemy.Id}.");
                }
            }
        }

        private static void ValidateGravityAttack()
        {
            const string attackSettingsPath = "Assets/_Game/Content/Definitions/S03_GravityAttackSettings.asset";
            var settings = AssetDatabase.LoadAssetAtPath<GravityAttackSettings>(attackSettingsPath);
            if (settings == null)
            {
                throw new InvalidOperationException(
                    $"A valid gravity attack definition is required at {attackSettingsPath}.");
            }

            settings.ValidateOrThrow();
        }

        private static void ValidatePlayerStats()
        {
            const string playerStatsPath = "Assets/_Game/Content/Definitions/S02_PlayerStats.asset";
            var definition = AssetDatabase.LoadAssetAtPath<PlayerStatsDefinition>(playerStatsPath);
            if (definition == null)
            {
                throw new InvalidOperationException($"A valid player stats definition is required at {playerStatsPath}.");
            }

            definition.ValidateOrThrow();
        }

        private static void ValidateUrpConfiguration()
        {
            var pipelineAsset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(UrpConfigurator.UrpAssetPath);
            if (pipelineAsset == null)
            {
                throw new InvalidOperationException($"A valid URP pipeline asset is required at {UrpConfigurator.UrpAssetPath}.");
            }

            var rendererData = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(UrpConfigurator.RendererDataPath);
            if (rendererData == null)
            {
                throw new InvalidOperationException($"A valid Universal Renderer Data asset is required at {UrpConfigurator.RendererDataPath}.");
            }

            if (!UrpConfigurator.ReferencesRenderer(pipelineAsset, rendererData))
            {
                throw new InvalidOperationException("The URP pipeline asset must reference the canonical Universal Renderer Data asset.");
            }

            if (GraphicsSettings.defaultRenderPipeline != pipelineAsset)
            {
                throw new InvalidOperationException("Graphics settings must use the canonical URP pipeline asset.");
            }

            if (QualitySettings.renderPipeline != pipelineAsset)
            {
                throw new InvalidOperationException("Quality settings must use the canonical URP pipeline asset.");
            }
        }

        private static void ValidateEditorBuildSettings()
        {
            var configuredScenes = EditorBuildSettings.scenes;
            foreach (var requiredScene in Build.AndroidBuild.BuildScenes)
            {
                var found = false;
                foreach (var scene in configuredScenes)
                {
                    if (scene.enabled && string.Equals(scene.path, requiredScene, StringComparison.Ordinal))
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    throw new InvalidOperationException($"Scene is not enabled in build settings: {requiredScene}");
                }
            }
        }

        private static void ValidateAndroidPlayerSettings()
        {
            if (PlayerSettings.defaultInterfaceOrientation != UIOrientation.Portrait)
            {
                throw new InvalidOperationException("Android orientation must be portrait.");
            }

            if (PlayerSettings.Android.targetArchitectures != AndroidArchitecture.ARM64)
            {
                throw new InvalidOperationException("Android target architecture must be ARM64 only.");
            }

            if (PlayerSettings.Android.minSdkVersion != AndroidSdkVersions.AndroidApiLevel26)
            {
                throw new InvalidOperationException("Android minimum API must be 26.");
            }

            if (PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android) != ScriptingImplementation.IL2CPP)
            {
                throw new InvalidOperationException("Android scripting backend must be IL2CPP.");
            }

            var packageId = PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android);
            if (packageId != GravivoreVersion.AndroidPackageId)
            {
                throw new InvalidOperationException($"Android package id must be {GravivoreVersion.AndroidPackageId}.");
            }
        }

        private static void ValidateVersion()
        {
            if (string.IsNullOrWhiteSpace(GravivoreVersion.AppVersion))
            {
                throw new InvalidOperationException("App version must be configured.");
            }

            if (GravivoreVersion.AndroidVersionCode < 1)
            {
                throw new InvalidOperationException("Android version code must be positive.");
            }
        }
    }
}
