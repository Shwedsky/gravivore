using System;
using UnityEditor;
using UnityEngine;

namespace Gravivore.Editor
{
    public static class ProjectConfigurator
    {
        [MenuItem("Gravivore/Configuration/Configure Project")]
        public static void ConfigureProjectMenu()
        {
            ConfigureOrThrow();
            Debug.Log("GRAVIVORE project configuration applied.");
        }

        public static void ConfigureOrThrow()
        {
            UrpConfigurator.ConfigureUrp();
            Build.AndroidBuild.ApplyAndroidPlayerSettings();
            AssetDatabase.SaveAssets();
        }
    }

    internal sealed class ProjectBootstrapAssetPostprocessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths,
            bool didDomainReload)
        {
            if (!didDomainReload)
            {
                return;
            }

            try
            {
                ProjectConfigurator.ConfigureOrThrow();
            }
            catch (Exception exception)
            {
                Debug.LogError($"GRAVIVORE automatic project configuration failed: {exception.Message}");
            }
        }
    }
}
