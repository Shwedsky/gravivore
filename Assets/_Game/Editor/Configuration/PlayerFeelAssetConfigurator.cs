using System;
using System.IO;
using Gravivore.Presentation.Feedback;
using UnityEditor;
using UnityEngine;

namespace Gravivore.Editor
{
    public static class PlayerFeelAssetConfigurator
    {
        public static void Configure()
        {
            BuildPalette();
            var definition = AssetDatabase.LoadAssetAtPath<S14PresentationDefinition>(S14PresentationAssetConfigurator.DefinitionPath);
            BindSoundPalette(definition);
            var serialized = new SerializedObject(definition);
            serialized.FindProperty("_lashWindupDuration").floatValue = .15f;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
            AssetDatabase.SaveAssets();
            Debug.Log("PLAYER FEEL audio palette and presentation timing configured.");
        }

        private static void BuildPalette()
        {
            const string sources = "Assets/ThirdParty/Kenney/CombatAudio/";
            const string output = "Assets/_Game/Content/Audio/PlayerFeel/";
            Directory.CreateDirectory(output);
            var names = new[] { "Step", "Charge", "Release", "Impact", "EnemyHit", "EnemyDeath", "PlayerHit", "PlayerDeath" };
            var inputs = new[] { "impactMetal_light_000", "doorClose_001", "thrusterFire_000", "impactMetal_000",
                "impactMetal_medium_000", "explosionCrunch_000", "impactPlate_heavy_000", "impactMetal_heavy_000" };
            var durations = new[] { .09f, .15f, .18f, .12f, .12f, .32f, .18f, .4f };
            for (var n = 0; n < names.Length; n++)
            {
                var sourcePath = sources + inputs[n] + ".ogg";
                AssetDatabase.ImportAsset(sourcePath, ImportAssetOptions.ForceSynchronousImport);
                var importer = (AudioImporter)AssetImporter.GetAtPath(sourcePath);
                var sampleSettings = importer.defaultSampleSettings;
                sampleSettings.loadType = AudioClipLoadType.DecompressOnLoad;
                sampleSettings.compressionFormat = AudioCompressionFormat.PCM;
                importer.defaultSampleSettings = sampleSettings; importer.forceToMono = true; importer.SaveAndReimport();
                var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(sourcePath);
                var samples = new float[clip.samples * clip.channels];
                clip.LoadAudioData();
                if (!clip.GetData(samples, 0)) throw new InvalidOperationException("Cannot decode source: " + sourcePath);
                var onset = 0;
                while (onset < samples.Length - 1 && Mathf.Abs(samples[onset]) < .02f) onset++;
                var count = Mathf.Min(samples.Length - onset, Mathf.CeilToInt(durations[n] * clip.frequency));
                var peak = .01f;
                for (var i = 0; i < count; i++) peak = Mathf.Max(peak, Mathf.Abs(samples[onset + i]));
                using (var writer = new BinaryWriter(File.Create(output + names[n] + ".wav")))
                {
                    writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF")); writer.Write(36 + count * 2);
                    writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt ")); writer.Write(16);
                    writer.Write((short)1); writer.Write((short)1); writer.Write(clip.frequency); writer.Write(clip.frequency * 2);
                    writer.Write((short)2); writer.Write((short)16); writer.Write(System.Text.Encoding.ASCII.GetBytes("data")); writer.Write(count * 2);
                    for (var i = 0; i < count; i++)
                    {
                        var fade = Mathf.Min(1f, i / (clip.frequency * .003f), (count - 1 - i) / (clip.frequency * .018f));
                        writer.Write((short)(samples[onset + i] / peak * .55f * fade * short.MaxValue));
                    }
                }
            }
            AssetDatabase.Refresh();
        }

        public static void BindSoundPalette(S14PresentationDefinition definition)
        {
            const string folder = "Assets/_Game/Content/Audio/PlayerFeel/";
            if (!File.Exists(folder + "Charge.wav")) return; // Legacy bootstrap without Phase 2 assets.
            var fields = new[] { "_stepClip", "_lashWindupClip", "_releaseClip", "_lashImpactClip",
                "_hitClip", "_deathClip", "_playerHitClip", "_playerDeathClip" };
            var files = new[] { "Step", "Charge", "Release", "Impact", "EnemyHit", "EnemyDeath", "PlayerHit", "PlayerDeath" };
            var serialized = new SerializedObject(definition);
            for (var i = 0; i < files.Length; i++)
            {
                var path = folder + files[i] + ".wav";
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
                var importer = (AudioImporter)AssetImporter.GetAtPath(path);
                var sample = importer.defaultSampleSettings;
                sample.loadType = AudioClipLoadType.DecompressOnLoad;
                sample.compressionFormat = AudioCompressionFormat.PCM;
                importer.defaultSampleSettings = sample; importer.forceToMono = true;
                importer.SaveAndReimport();
                serialized.FindProperty(fields[i]).objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>(path)
                    ?? throw new InvalidOperationException("Missing combat sound: " + path);
            }
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(definition);
        }
    }
}
