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
}
