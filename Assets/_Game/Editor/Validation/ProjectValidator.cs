using System;
using System.IO;
using Gravivore.Core;
using UnityEditor;
using UnityEngine;

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
            "Assets/_Game/Content/Settings",
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
            UrpBootstrapper.EnsureUrpConfigured();
            ValidateAndroidPlayerSettings();
            ValidateVersion();
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
            Build.AndroidBuild.ApplyAndroidPlayerSettings();

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
