using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Gravivore.Core;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Gravivore.Editor.Build
{
    public enum AndroidBuildFlavor { Dev, Candidate }

    [Serializable]
    public sealed class AndroidBuildMetadata
    {
        public string flavor;
        public string version;
        public int versionCode;
        public string buildTimestampUtc;
        public string gitCommitSha;
        public string unityVersion;
        public bool developmentBuild;
        public string apkFilename;
    }

    public static class AndroidBuild
    {
        private const string OutputDirectory = "Builds/Android";
        private const string VersionArg = "-Version";
        private const string VersionCodeArg = "-VersionCode";
        private const string CleanArg = "-Clean";

        public static readonly string[] BuildScenes =
        {
            "Assets/_Game/Content/Scenes/Bootstrap.unity",
            "Assets/_Game/Content/Scenes/Chapter01_ScrapExclusion.unity"
        };

        public static void BuildDev() => Build(AndroidBuildFlavor.Dev);
        public static void BuildCandidate() => Build(AndroidBuildFlavor.Candidate);

        private static void Build(AndroidBuildFlavor flavor)
        {
            if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android))
                throw new InvalidOperationException("Unable to activate Android. Install Android Build Support for this Unity Editor.");

            ProjectConfigurator.ConfigureOrThrow();
            ProjectValidator.ValidateOrThrow();
            var version = ResolveVersion(ReadCommandLineValue(VersionArg));
            var versionCode = ResolveVersionCode(ReadCommandLineValue(VersionCodeArg));
            var outputPath = GetOutputPath(flavor, version, versionCode);

            if (HasCommandLineFlag(CleanArg) && Directory.Exists(OutputDirectory)) Directory.Delete(OutputDirectory, true);
            Directory.CreateDirectory(OutputDirectory);
            PlayerSettings.bundleVersion = version;
            PlayerSettings.Android.bundleVersionCode = versionCode;

            var options = GetBuildOptions(flavor);
            ValidateFlavorOptions(flavor, options);
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = BuildScenes,
                locationPathName = outputPath,
                target = BuildTarget.Android,
                options = options
            });
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException($"Android {flavor} build failed: {report.summary.result} ({report.summary.totalErrors} errors).");

            var metadataPath = GetMetadataPath(outputPath);
            File.WriteAllText(metadataPath, CreateMetadataJson(flavor, version, versionCode, outputPath, DateTime.UtcNow, TryGetGitCommitSha()));
            UnityEngine.Debug.Log($"Android {flavor} build written to {outputPath}");
            UnityEngine.Debug.Log($"Android build metadata written to {metadataPath}");
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

        public static AndroidBuildFlavor ParseFlavor(string value)
        {
            if (Enum.TryParse(value, true, out AndroidBuildFlavor flavor) && Enum.IsDefined(typeof(AndroidBuildFlavor), flavor)) return flavor;
            throw new ArgumentException("Android build flavor must be Dev or Candidate.", nameof(value));
        }

        public static BuildOptions GetBuildOptions(AndroidBuildFlavor flavor)
        {
            switch (flavor)
            {
                case AndroidBuildFlavor.Dev: return BuildOptions.Development | BuildOptions.AllowDebugging;
                case AndroidBuildFlavor.Candidate: return BuildOptions.None;
                default: throw new ArgumentOutOfRangeException(nameof(flavor));
            }
        }

        public static string GetOutputPath(AndroidBuildFlavor flavor, string version, int versionCode)
        {
            version = ResolveVersion(version);
            versionCode = ResolveVersionCode(versionCode.ToString(CultureInfo.InvariantCulture));
            var label = flavor == AndroidBuildFlavor.Dev ? "dev" : flavor == AndroidBuildFlavor.Candidate ? "candidate" : throw new ArgumentOutOfRangeException(nameof(flavor));
            return Path.Combine(OutputDirectory, $"gravivore-{label}-{version}+{versionCode}.apk").Replace('\\', '/');
        }

        public static string GetMetadataPath(string apkPath) => Path.ChangeExtension(apkPath, ".build.json").Replace('\\', '/');

        public static string ResolveVersion(string overrideValue)
        {
            var value = string.IsNullOrWhiteSpace(overrideValue) ? GravivoreVersion.AppVersion : overrideValue;
            if (!Version.TryParse(value, out var parsed) || parsed.Build < 0)
                throw new ArgumentException("Version must contain at least three non-negative numeric components.", nameof(overrideValue));
            return value;
        }

        public static int ResolveVersionCode(string overrideValue)
        {
            if (string.IsNullOrWhiteSpace(overrideValue)) return GravivoreVersion.AndroidVersionCode;
            if (!int.TryParse(overrideValue, NumberStyles.None, CultureInfo.InvariantCulture, out var value) || value <= 0)
                throw new ArgumentException("VersionCode must be a positive integer.", nameof(overrideValue));
            return value;
        }

        public static string CreateMetadataJson(AndroidBuildFlavor flavor, string version, int versionCode, string apkPath, DateTime timestampUtc, string gitSha)
        {
            var metadata = new AndroidBuildMetadata
            {
                flavor = flavor.ToString(), version = ResolveVersion(version), versionCode = ResolveVersionCode(versionCode.ToString(CultureInfo.InvariantCulture)),
                buildTimestampUtc = timestampUtc.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture),
                gitCommitSha = string.IsNullOrWhiteSpace(gitSha) ? null : gitSha,
                unityVersion = Application.unityVersion,
                developmentBuild = (GetBuildOptions(flavor) & BuildOptions.Development) != 0,
                apkFilename = Path.GetFileName(apkPath)
            };
            return JsonUtility.ToJson(metadata, true);
        }

        private static void ValidateFlavorOptions(AndroidBuildFlavor flavor, BuildOptions options)
        {
            if (flavor == AndroidBuildFlavor.Candidate && (options & (BuildOptions.Development | BuildOptions.AllowDebugging)) != 0)
                throw new InvalidOperationException("Candidate builds cannot enable development or debugging options.");
        }

        private static string TryGetGitCommitSha()
        {
            try
            {
                using var process = Process.Start(new ProcessStartInfo { FileName = "git", Arguments = "rev-parse HEAD", WorkingDirectory = Directory.GetCurrentDirectory(), UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true });
                if (process == null) return null;
                var output = process.StandardOutput.ReadToEnd().Trim();
                process.WaitForExit(5000);
                return process.ExitCode == 0 && output.Length == 40 ? output : null;
            }
            catch { return null; }
        }

        private static string ReadCommandLineValue(string name)
        {
            var args = Environment.GetCommandLineArgs();
            for (var i = 0; i < args.Length - 1; i++) if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase)) return args[i + 1];
            return null;
        }

        private static bool HasCommandLineFlag(string flag)
        {
            foreach (var arg in Environment.GetCommandLineArgs()) if (string.Equals(arg, flag, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }
    }
}
