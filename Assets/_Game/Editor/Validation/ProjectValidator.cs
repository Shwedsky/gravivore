using System;
using System.Collections.Generic;
using System.IO;
using Gravivore.Core;
using Gravivore.Gameplay.Combat;
using Gravivore.Gameplay.Enemies;
using Gravivore.Gameplay.Player;
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
