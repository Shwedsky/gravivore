using System;
using Gravivore.Editor.Build;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Gravivore.Tests.EditMode
{
    public sealed class AndroidBuildTests
    {
        [Test]
        public void FlavorPackageIds_AreIsolated()
        {
            Assert.AreEqual("com.gravivore.mobile.dev", AndroidBuild.GetApplicationIdentifier(AndroidBuildFlavor.Dev));
            Assert.AreEqual("com.gravivore.mobile", AndroidBuild.GetApplicationIdentifier(AndroidBuildFlavor.Candidate));
            Assert.AreNotEqual(
                AndroidBuild.GetApplicationIdentifier(AndroidBuildFlavor.Dev),
                AndroidBuild.GetApplicationIdentifier(AndroidBuildFlavor.Candidate));
        }

        [Test]
        public void FlavorSwitching_RestoresOriginalPlayerSettingsIdentity()
        {
            var original = AndroidBuild.CaptureScopedPlayerSettings();
            try
            {
                AndroidBuild.ApplyFlavorPlayerSettings(AndroidBuildFlavor.Dev, "1.2.3", 41);
                Assert.AreEqual(
                    "com.gravivore.mobile.dev",
                    PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android));

                AndroidBuild.RestoreScopedPlayerSettings(original);
                Assert.AreEqual(
                    original.ApplicationId,
                    PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android));

                AndroidBuild.ApplyFlavorPlayerSettings(AndroidBuildFlavor.Candidate, "1.2.3", 42);
                Assert.AreEqual(
                    "com.gravivore.mobile",
                    PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android));

                AndroidBuild.RestoreScopedPlayerSettings(original);
                AndroidBuild.ApplyFlavorPlayerSettings(AndroidBuildFlavor.Dev, "1.2.3", 43);
                Assert.AreEqual(
                    "com.gravivore.mobile.dev",
                    PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android));
            }
            finally
            {
                AndroidBuild.RestoreScopedPlayerSettings(original);
            }

            Assert.AreEqual(
                original.ApplicationId,
                PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android));
            Assert.AreEqual(original.BundleVersion, PlayerSettings.bundleVersion);
            Assert.AreEqual(original.VersionCode, PlayerSettings.Android.bundleVersionCode);
        }

        [Test]
        public void DevOptions_EnableDevelopmentAndDebugging()
        {
            var options = AndroidBuild.GetBuildOptions(AndroidBuildFlavor.Dev);
            Assert.AreNotEqual(0, options & BuildOptions.Development);
            Assert.AreNotEqual(0, options & BuildOptions.AllowDebugging);
        }

        [Test]
        public void CandidateOptions_ExcludeDevelopmentAndDebugging()
        {
            var options = AndroidBuild.GetBuildOptions(AndroidBuildFlavor.Candidate);
            Assert.AreEqual(BuildOptions.None, options & BuildOptions.Development);
            Assert.AreEqual(BuildOptions.None, options & BuildOptions.AllowDebugging);
        }

        [Test]
        public void CandidateValidation_RejectsInjectedDevelopmentOrDebugOptions()
        {
            Assert.Throws<InvalidOperationException>(
                () => AndroidBuild.ValidateFlavorOptions(
                    AndroidBuildFlavor.Candidate,
                    BuildOptions.Development));
            Assert.Throws<InvalidOperationException>(
                () => AndroidBuild.ValidateFlavorOptions(
                    AndroidBuildFlavor.Candidate,
                    BuildOptions.AllowDebugging));
        }

        [TestCase(AndroidBuildFlavor.Dev, "Builds/Android/gravivore-dev-1.2.3+42.apk")]
        [TestCase(AndroidBuildFlavor.Candidate, "Builds/Android/gravivore-candidate-1.2.3+42.apk")]
        public void OutputNaming_IsFlavorSpecific(AndroidBuildFlavor flavor, string expected)
        {
            Assert.AreEqual(expected, AndroidBuild.GetOutputPath(flavor, "1.2.3", 42));
        }

        [Test]
        public void FlavorParsing_RejectsUnknownValue()
        {
            Assert.AreEqual(AndroidBuildFlavor.Dev, AndroidBuild.ParseFlavor("dev"));
            Assert.AreEqual(AndroidBuildFlavor.Candidate, AndroidBuild.ParseFlavor("Candidate"));
            Assert.Throws<ArgumentException>(() => AndroidBuild.ParseFlavor("Release"));
        }

        [Test]
        public void VersionOverrides_AreValidatedDeterministically()
        {
            Assert.AreEqual("2.3.4", AndroidBuild.ResolveVersion("2.3.4"));
            Assert.AreEqual(17, AndroidBuild.ResolveVersionCode("17"));
            Assert.AreEqual(17, AndroidBuild.ResolveVersionCode(AndroidBuildFlavor.Candidate, "17"));
            Assert.AreEqual(
                Gravivore.Core.GravivoreVersion.AndroidVersionCode,
                AndroidBuild.ResolveVersionCode(AndroidBuildFlavor.Dev, null));
            Assert.Throws<ArgumentException>(() => AndroidBuild.ResolveVersion("2.3"));
            Assert.Throws<ArgumentException>(() => AndroidBuild.ResolveVersionCode("0"));
            Assert.Throws<ArgumentException>(() => AndroidBuild.ResolveVersionCode("random"));
            Assert.Throws<ArgumentException>(
                () => AndroidBuild.ResolveVersionCode(AndroidBuildFlavor.Candidate, null));
        }

        [Test]
        public void BuildScenes_AreIdenticalAndCanonicalForBothFlavors()
        {
            CollectionAssert.AreEqual(new[]
            {
                "Assets/_Game/Content/Scenes/Bootstrap.unity",
                "Assets/_Game/Content/Scenes/Chapter01_ScrapExclusion.unity"
            }, AndroidBuild.BuildScenes);
        }

        [Test]
        public void AndroidSettings_AreArm64Il2CppApi26Portrait()
        {
            AndroidBuild.ApplyAndroidPlayerSettings();
            Assert.AreEqual(AndroidArchitecture.ARM64, PlayerSettings.Android.targetArchitectures);
            Assert.AreEqual(ScriptingImplementation.IL2CPP, PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android));
            Assert.AreEqual(AndroidSdkVersions.AndroidApiLevel26, PlayerSettings.Android.minSdkVersion);
            Assert.AreEqual(UIOrientation.Portrait, PlayerSettings.defaultInterfaceOrientation);
        }

        [Test]
        public void Metadata_ContainsFlavorVersionCommitAndArtifact()
        {
            var json = AndroidBuild.CreateMetadataJson(
                AndroidBuildFlavor.Candidate, "1.2.3", 42,
                "Builds/Android/gravivore-candidate-1.2.3+42.apk",
                new DateTime(2032, 1, 2, 3, 4, 5, DateTimeKind.Utc),
                "0123456789012345678901234567890123456789");
            var metadata = JsonUtility.FromJson<AndroidBuildMetadata>(json);
            Assert.AreEqual("Candidate", metadata.flavor);
            Assert.AreEqual("1.2.3", metadata.version);
            Assert.AreEqual(42, metadata.versionCode);
            Assert.AreEqual("2032-01-02T03:04:05.0000000Z", metadata.buildTimestampUtc);
            Assert.AreEqual("0123456789012345678901234567890123456789", metadata.gitCommitSha);
            Assert.IsFalse(metadata.developmentBuild);
            Assert.IsFalse(metadata.allowDebugging);
            Assert.AreEqual("com.gravivore.mobile", metadata.applicationId);
            Assert.AreEqual("ARM64", metadata.targetArchitecture);
            Assert.AreEqual("gravivore-candidate-1.2.3+42.apk", metadata.apkFilename);
        }
    }
}
