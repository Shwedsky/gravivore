using System;
using System.IO;
using Gravivore.Core;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Gravivore.Editor.Build
{
    public static class AndroidBuild
    {
        private const string OutputDirectory = "Builds/Android";
        private const string BuildMethodVersionArg = "-Version";
        private const string BuildMethodCleanArg = "-Clean";

        public static readonly string[] BuildScenes =
        {
            "Assets/_Game/Content/Scenes/Bootstrap.unity",
            "Assets/_Game/Content/Scenes/Chapter01_ScrapExclusion.unity"
        };

        public static void BuildDev()
        {
            if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android))
            {
                throw new InvalidOperationException(
                    "Unable to activate the Android build target. Install Android Build Support for this Unity Editor.");
            }

            ProjectConfigurator.ConfigureOrThrow();
            ProjectValidator.ValidateOrThrow();

            var version = ReadCommandLineValue(BuildMethodVersionArg, GravivoreVersion.AppVersion);
            var outputPath = GetOutputPath(version, GravivoreVersion.AndroidVersionCode);

            if (HasCommandLineFlag(BuildMethodCleanArg) && Directory.Exists(OutputDirectory))
            {
                Directory.Delete(OutputDirectory, true);
            }

            Directory.CreateDirectory(OutputDirectory);
            PlayerSettings.bundleVersion = version;
            PlayerSettings.Android.bundleVersionCode = GravivoreVersion.AndroidVersionCode;

            var options = new BuildPlayerOptions
            {
                scenes = BuildScenes,
                locationPathName = outputPath,
                target = BuildTarget.Android,
                options = BuildOptions.Development | BuildOptions.AllowDebugging
            };

            var report = BuildPipeline.BuildPlayer(options);
            var summary = report.summary;

            if (summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException($"Android development build failed: {summary.result} ({summary.totalErrors} errors).");
            }

            Debug.Log($"Android development build written to {outputPath}");
        }

        public static void ApplyAndroidPlayerSettings()
        {
            PlayerSettings.companyName = GravivoreVersion.CompanyName;
            PlayerSettings.productName = GravivoreVersion.ProductName;
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, GravivoreVersion.AndroidPackageId);
            PlayerSettings.bundleVersion = GravivoreVersion.AppVersion;
            PlayerSettings.Android.bundleVersionCode = GravivoreVersion.AndroidVersionCode;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
            PlayerSettings.Android.forceInternetPermission = false;
            PlayerSettings.Android.forceSDCardPermission = false;
        }

        public static string GetOutputPath(string version, int versionCode)
        {
            return Path.Combine(OutputDirectory, $"gravivore-dev-{version}+{versionCode}.apk").Replace('\\', '/');
        }

        private static string ReadCommandLineValue(string argName, string fallback)
        {
            var args = Environment.GetCommandLineArgs();
            for (var i = 0; i < args.Length - 1; i++)
            {
                if (string.Equals(args[i], argName, StringComparison.OrdinalIgnoreCase))
                {
                    return args[i + 1];
                }
            }

            return fallback;
        }

        private static bool HasCommandLineFlag(string flag)
        {
            foreach (var arg in Environment.GetCommandLineArgs())
            {
                if (string.Equals(arg, flag, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
