using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Gravivore.ArtSpike.Editor
{
    // A pivot/idle proof for the authorized proxy, independent of gameplay and production prefabs.
    public static class ArtSpikeArticulation
    {
        public const string ClipPath = ArtSpikeBuilder.Root + "/Proxy/MechanicalIdle_ArtSpike.anim";

        public static void BuildAndProve()
        {
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(ArtSpikeBuilder.CharacterPaths[2]);
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(ClipPath);
            if (clip == null)
            {
                clip = new AnimationClip { name = "MechanicalIdle_ArtSpike", legacy = true };
                AssetDatabase.CreateAsset(clip, ClipPath);
            }
            clip.ClearCurves(); clip.wrapMode = WrapMode.Loop;
            foreach (var hip in source.GetComponentsInChildren<Transform>(true).Where(t => t.name == "HipPivot"))
            {
                var sign = hip.parent.name.StartsWith("Left", StringComparison.Ordinal) ? -1 : 1;
                clip.SetCurve(AnimationUtility.CalculateTransformPath(hip, source.transform), typeof(Transform),
                    "localEulerAnglesRaw.z", Sweep(0, sign * 3));
            }
            foreach (var weapon in source.GetComponentsInChildren<Transform>(true).Where(t =>
                t.name == "LeftWeaponPivot" || t.name == "RightWeaponPivot"))
            {
                var rest = weapon.name.StartsWith("Left", StringComparison.Ordinal) ? 7f : -7f;
                clip.SetCurve(AnimationUtility.CalculateTransformPath(weapon, source.transform), typeof(Transform),
                    "localEulerAnglesRaw.y", Sweep(rest, 4));
            }
            EditorUtility.SetDirty(clip); AssetDatabase.SaveAssets();
            var instance = Object.Instantiate(source);
            try
            {
                clip.SampleAnimation(instance, .75f);
                var moved = instance.GetComponentsInChildren<Transform>(true).Count(t => t.name == "HipPivot" &&
                    Quaternion.Angle(t.localRotation, Quaternion.Euler(0, t.parent.name.StartsWith("Left") ? 180 : 0, 0)) > 2);
                if (moved != 4) throw new InvalidOperationException("Proxy articulation proof did not move four independent hip pivots.");
                File.WriteAllText("docs/art-spike/ARTICULATION.json",
                    "{\n  \"kind\": \"project-authored pivot idle; not an imported skeleton or gait\",\n  \"clip\": \"" + ClipPath +
                    "\",\n  \"durationSeconds\": 3,\n  \"sampleSeconds\": 0.75,\n  \"independentHipsMoved\": 4,\n  \"weaponPivots\": 2,\n  \"gameplayBinding\": false\n}\n");
            }
            finally { Object.DestroyImmediate(instance); }
        }

        private static AnimationCurve Sweep(float rest, float amplitude) => new AnimationCurve(
            new Keyframe(0, rest), new Keyframe(.75f, rest + amplitude),
            new Keyframe(1.5f, rest), new Keyframe(2.25f, rest - amplitude), new Keyframe(3, rest));

        [MenuItem("Gravivore/Art Spike/Preview Proxy Mechanical Idle")]
        public static void Preview()
        {
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().path != ArtSpikeBuilder.ScenePath)
                throw new InvalidOperationException("Open the isolated ArtSpike comparison scene before starting this preview.");
            var instance = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(ArtSpikeBuilder.CharacterPaths[2]));
            instance.name = "TEMPORARY_ProxyArticulationPreview_DeleteAfterReview";
            instance.hideFlags = HideFlags.DontSave;
            instance.transform.position = new Vector3(0, 0, -3);
            var animation = instance.AddComponent<Animation>();
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(ClipPath);
            animation.AddClip(clip, clip.name); animation.clip = clip;
            // Sampled proof is available outside Play mode. Legacy playback is only this temporary object.
            clip.SampleAnimation(instance, .75f);
            Selection.activeGameObject = instance;
        }
    }
}
