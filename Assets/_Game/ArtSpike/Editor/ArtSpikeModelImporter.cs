using System;
using UnityEditor;

namespace Gravivore.ArtSpike.Editor
{
    // Only the isolated donor subset is affected; the original FBX bytes stay untouched.
    public sealed class ArtSpikeModelImporter : AssetPostprocessor
    {
        private void OnPreprocessModel()
        {
            if (!assetPath.StartsWith("Assets/_Game/ArtSpike/Imported/", StringComparison.Ordinal)) return;
            var importer = (ModelImporter)assetImporter;
            importer.globalScale = 1f;
            importer.importAnimation = false;
            importer.animationType = ModelImporterAnimationType.None;
            importer.importBlendShapes = false;
            importer.importCameras = false;
            importer.importLights = false;
            importer.importVisibility = false;
            importer.isReadable = false;
            // Preserve donor geometry for this proof; no destructive compression or mesh editing.
            importer.meshCompression = ModelImporterMeshCompression.Off;
            importer.optimizeMeshPolygons = true;
            importer.optimizeMeshVertices = true;
            importer.importTangents = ModelImporterTangents.None;
            importer.materialImportMode = ModelImporterMaterialImportMode.None;
        }
    }
}
