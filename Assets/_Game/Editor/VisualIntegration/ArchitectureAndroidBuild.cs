using System;
using System.IO;
using UnityEditor;
using UnityEditor.Android;
using UnityEngine;

namespace Gravivore.Editor.VisualIntegration
{
    /// <summary>Build-only SDK selection; restores the previous functional SDK preference.</summary>
    public static class ArchitectureAndroidBuild
    {
        public static void BuildWithInstalledSdk()
        {
            var mirror = Environment.GetEnvironmentVariable("GRAVIVORE_ARCHITECTURE_OFFLINE_SDK");
            if (string.IsNullOrWhiteSpace(mirror) || !Directory.Exists(mirror))
                throw new InvalidDataException("A genuine installed SDK mirror is required.");
            var previous = AndroidExternalToolsSettings.sdkRootPath;
            var embedded = Path.Combine(EditorApplication.applicationContentsPath, "PlaybackEngines/AndroidPlayer/SDK");
            var restore = !string.IsNullOrEmpty(previous) &&
                Path.GetFullPath(previous).TrimEnd('/', '\\').Equals(Path.GetFullPath(embedded).TrimEnd('/', '\\'), StringComparison.OrdinalIgnoreCase)
                ? string.Empty : previous;
            try
            {
                AndroidExternalToolsSettings.sdkRootPath = mirror;
                Build.AndroidBuild.BuildDev();
            }
            finally
            {
                AndroidExternalToolsSettings.sdkRootPath = restore;
                File.WriteAllText(Chapter01ArchitectureProduction.Evidence + "/verification/sdk-preference-restoration.json",
                    JsonUtility.ToJson(new SdkReceipt { before = previous, mirror = mirror, after = AndroidExternalToolsSettings.sdkRootPath }, true));
            }
        }
        [Serializable] private sealed class SdkReceipt { public string before, mirror, after; }
    }
}
