using UnityEditor;

namespace Gravivore.Editor
{
    public sealed class S15ModelImportPostprocessor : AssetPostprocessor
    {
        private const string ModelRoot = "Assets/ThirdParty/KenneyFactoryKit/Models/";

        private void OnPreprocessModel()
        {
            if (!assetPath.StartsWith(ModelRoot, System.StringComparison.Ordinal)) return;
            var importer = (ModelImporter)assetImporter;
            importer.globalScale = 1f;
            importer.importAnimation = false;
            importer.animationType = ModelImporterAnimationType.None;
            importer.importBlendShapes = false;
            importer.importCameras = false;
            importer.importLights = false;
            importer.importVisibility = false;
            importer.isReadable = false;
            importer.meshCompression = ModelImporterMeshCompression.Medium;
            importer.optimizeMeshPolygons = true;
            importer.optimizeMeshVertices = true;
            importer.importTangents = ModelImporterTangents.None;
            importer.materialImportMode = ModelImporterMaterialImportMode.None;
        }
    }
}
