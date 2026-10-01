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
            Assert.Throws<ArgumentException>(() => AndroidBuild.ResolveVersion("2.3"));
            Assert.Throws<ArgumentException>(() => AndroidBuild.ResolveVersionCode("0"));
            Assert.Throws<ArgumentException>(() => AndroidBuild.ResolveVersionCode("random"));
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
            Assert.AreEqual("gravivore-candidate-1.2.3+42.apk", metadata.apkFilename);
        }
    }
}
