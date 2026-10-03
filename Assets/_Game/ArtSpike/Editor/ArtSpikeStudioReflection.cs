using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Gravivore.ArtSpike.Editor
{
    // A small static reflection environment reveals beveled metal planes without extra live lights.
    public static class ArtSpikeStudioReflection
    {
        public static void Apply()
        {
            const string path = ArtSpikeBuilder.Root + "/Materials/ArtSpike_StudioReflection.cubemap";
            var cube = AssetDatabase.LoadAssetAtPath<Cubemap>(path);
            if (cube == null)
            {
                cube = new Cubemap(32, TextureFormat.RGBAHalf, true) { name = "ArtSpike_StudioReflection" };
                AssetDatabase.CreateAsset(cube, path);
            }
            for (var face = 0; face < 6; face++)
            {
                var pixels = new Color[32 * 32];
                for (var y = 0; y < 32; y++)
                    for (var x = 0; x < 32; x++)
                    {
                        var u = (x + .5f) / 16f - 1f;
                        var v = (y + .5f) / 16f - 1f;
                        Vector3 direction;
                        switch ((CubemapFace)face)
                        {
                            case CubemapFace.PositiveX: direction = new Vector3(1, -v, -u); break;
                            case CubemapFace.NegativeX: direction = new Vector3(-1, -v, u); break;
                            case CubemapFace.PositiveY: direction = new Vector3(u, 1, v); break;
                            case CubemapFace.NegativeY: direction = new Vector3(u, -1, -v); break;
                            case CubemapFace.PositiveZ: direction = new Vector3(u, -v, 1); break;
                            default: direction = new Vector3(-u, -v, -1); break;
                        }
                        direction.Normalize();
                        var ceiling = Mathf.SmoothStep(0, 1, direction.y * .5f + .5f);
                        var key = Mathf.Pow(Mathf.Max(0, Vector3.Dot(direction, new Vector3(-.5f, .75f, .4f).normalized)), 14);
                        var fill = Mathf.Pow(Mathf.Max(0, Vector3.Dot(direction, new Vector3(.8f, .3f, -.3f).normalized)), 20);
                        pixels[y * 32 + x] = Color.Lerp(new Color(.035f, .045f, .055f),
                            new Color(.22f, .28f, .34f), ceiling) +
                            new Color(.45f, .50f, .55f) * key + new Color(.15f, .22f, .28f) * fill;
                    }
                cube.SetPixels(pixels, (CubemapFace)face);
            }
            cube.Apply(true, false);
            EditorUtility.SetDirty(cube);
            RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;
            RenderSettings.customReflectionTexture = cube;
            RenderSettings.reflectionIntensity = .60f;
        }
    }
}
