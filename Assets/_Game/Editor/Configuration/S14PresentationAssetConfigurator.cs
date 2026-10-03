using System;
using System.IO;
using Gravivore.Presentation.Composition;
using Gravivore.Presentation.Feedback;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Gravivore.Editor
{
    public static class S14PresentationAssetConfigurator
    {
        public const string DefinitionPath = "Assets/_Game/Content/Definitions/S14_Presentation.asset";
        public const string AudioDirectory = "Assets/_Game/Content/Audio";
        public const string ChapterScenePath = "Assets/_Game/Content/Scenes/Chapter01_ScrapExclusion.unity";

        private static readonly string[] CueNames =
        {
            "LashWindup", "LashImpact", "Hit", "Death", "Assimilation", "Evolution", "Telegraph", "BossImpact"
        };

        private static readonly float[] Frequencies = { 330f, 165f, 440f, 110f, 550f, 660f, 220f, 82.5f };

        [MenuItem("Gravivore/Configuration/Create S14 Presentation Assets")]
        public static void ConfigureMenu()
        {
            ConfigureOrThrow();
            Debug.Log("GRAVIVORE S14 presentation assets configured.");
        }

        public static void ConfigureOrThrow()
        {
            Directory.CreateDirectory(AudioDirectory);
            var clips = new AudioClip[CueNames.Length];
            for (var i = 0; i < CueNames.Length; i++)
            {
                var path = $"{AudioDirectory}/S14_{CueNames[i]}.wav";
                if (!File.Exists(path)) File.WriteAllBytes(path, CreateTone(Frequencies[i], i == 5 ? 0.22f : 0.1f));
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
                clips[i] = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                if (clips[i] == null) throw new InvalidOperationException($"Failed to import generated audio cue: {path}");
            }

            var definition = AssetDatabase.LoadAssetAtPath<S14PresentationDefinition>(DefinitionPath);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<S14PresentationDefinition>();
                definition.name = "S14_Presentation";
                AssetDatabase.CreateAsset(definition, DefinitionPath);
            }

            var serialized = new SerializedObject(definition);
            var properties = new[]
            {
                "_lashWindupClip", "_lashImpactClip", "_hitClip", "_deathClip", "_assimilationClip",
                "_evolutionClip", "_telegraphClip", "_bossImpactClip"
            };
            for (var i = 0; i < properties.Length; i++) serialized.FindProperty(properties[i]).objectReferenceValue = clips[i];
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
            PlayerFeelAssetConfigurator.BindSoundPalette(definition);

            var scene = EditorSceneManager.OpenScene(ChapterScenePath, OpenSceneMode.Single);
            S01SceneCompositionRoot root = null;
            foreach (var rootObject in scene.GetRootGameObjects())
            {
                root = rootObject.GetComponentInChildren<S01SceneCompositionRoot>(true);
                if (root != null) break;
            }
            if (root == null) throw new InvalidOperationException("Chapter scene requires S01SceneCompositionRoot.");
            var rootSerialized = new SerializedObject(root);
            rootSerialized.FindProperty("_s14PresentationDefinition").objectReferenceValue = definition;
            rootSerialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(root);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Failed to save S14 scene reference.");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static byte[] CreateTone(float frequency, float duration)
        {
            const int sampleRate = 22050;
            const short channels = 1;
            const short bitsPerSample = 16;
            var sampleCount = Mathf.CeilToInt(sampleRate * duration);
            var dataSize = sampleCount * 2;
            var bytes = new byte[44 + dataSize];
            WriteAscii(bytes, 0, "RIFF");
            WriteInt(bytes, 4, 36 + dataSize);
            WriteAscii(bytes, 8, "WAVEfmt ");
            WriteInt(bytes, 16, 16);
            WriteShort(bytes, 20, 1);
            WriteShort(bytes, 22, channels);
            WriteInt(bytes, 24, sampleRate);
            WriteInt(bytes, 28, sampleRate * channels * bitsPerSample / 8);
            WriteShort(bytes, 32, (short)(channels * bitsPerSample / 8));
            WriteShort(bytes, 34, bitsPerSample);
            WriteAscii(bytes, 36, "data");
            WriteInt(bytes, 40, dataSize);
            for (var i = 0; i < sampleCount; i++)
            {
                var envelope = 1f - (i / (float)sampleCount);
                var sample = (short)(Math.Sin(2d * Math.PI * frequency * i / sampleRate) * envelope * 7000d);
                WriteShort(bytes, 44 + (i * 2), sample);
            }
            return bytes;
        }

        private static void WriteAscii(byte[] bytes, int offset, string value)
        {
            for (var i = 0; i < value.Length; i++) bytes[offset + i] = (byte)value[i];
        }

        private static void WriteInt(byte[] bytes, int offset, int value) => Array.Copy(BitConverter.GetBytes(value), 0, bytes, offset, 4);
        private static void WriteShort(byte[] bytes, int offset, short value) => Array.Copy(BitConverter.GetBytes(value), 0, bytes, offset, 2);
    }
}
