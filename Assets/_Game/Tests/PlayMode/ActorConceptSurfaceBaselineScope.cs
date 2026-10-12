using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gravivore.Presentation.Assets;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Gravivore.Tests.PlayMode
{
    /// <summary>Original APK53 surfaces/LODs, reapplied only in memory for matched sRGB evidence.</summary>
    internal sealed class ActorConceptSurfaceBaselineScope:IDisposable
    {
        private readonly List<(Material live,Material copy)> _materials=new List<(Material,Material)>();
        private readonly List<(LODGroup live,LOD[] copy)> _lods=new List<(LODGroup,LOD[])>();
        private Texture2D _hostile,_g0;
        private HostilePresentationDefinition _profiles;
        private float[] _offsets;
        public ActorConceptSurfaceBaselineScope(Transform root,bool enabled)
        {
            if(!enabled)return;
            const string folder="Tools/actors-combat-concept-world/baseline/";
            var data=JsonUtility.FromJson<Snapshot>(File.ReadAllText(folder+"original-material-values.json"));
            _hostile=new Texture2D(2,2);_hostile.LoadImage(File.ReadAllBytes(folder+"HostileBase.png"));
            _g0=new Texture2D(2,2);_g0.LoadImage(File.ReadAllBytes(folder+"G0Base.png"));
            foreach(var material in root.GetComponentsInChildren<SkinnedMeshRenderer>(true).SelectMany(s=>s.sharedMaterials).Distinct())
            {
                if(material.shader.name!="Gravivore/Actor Industrial Lit")continue;
                var name=material.name.Replace(" (Instance)","");
                if(name.StartsWith("Cutter_")||name.StartsWith("Custodian_"))name="HostileV2_Red";
                var basis=data.materials.First(m=>m.name==name);_materials.Add((material,new Material(material)));
                material.shader=Shader.Find("Universal Render Pipeline/Lit");
                material.SetTexture("_BaseMap",name.StartsWith("G0_")?_g0:_hostile);
                material.SetColor("_BaseColor",basis.baseColor);material.SetColor("_EmissionColor",basis.emission);
                material.SetFloat("_BumpScale",basis.bumpScale);material.SetFloat("_Smoothness",basis.smoothness);material.SetFloat("_OcclusionStrength",basis.occlusionStrength);
                foreach(var keyword in new[]{"_EMISSION","_NORMALMAP","_METALLICSPECGLOSSMAP","_OCCLUSIONMAP"})material.EnableKeyword(keyword);
            }
            foreach(var group in root.GetComponentsInChildren<LODGroup>(true).Where(l=>l.name.EndsWith("_V2_Rig")))
            {
                var old=group.GetLODs();_lods.Add((group,old));var lods=group.GetLODs();
                lods[0].screenRelativeTransitionHeight=.14f;lods[1].screenRelativeTransitionHeight=.075f;lods[2].screenRelativeTransitionHeight=.025f;group.SetLODs(lods);
            }
            _profiles=Resources.FindObjectsOfTypeAll<S15VisualCatalog>().First(c=>c.HostilePresentation!=null).HostilePresentation;
            _offsets=_profiles.Profiles.Select(p=>p.MaximumWallOffset).ToArray();var original=new[]{.43f,1.08f,.71f,.51f,.8f,1.48f,2f};
            for(var i=0;i<original.Length;i++)_profiles.Profiles[i].MaximumWallOffset=original[i];
        }
        public void Dispose()
        {
            foreach(var pair in _materials){pair.live.shader=pair.copy.shader;pair.live.CopyPropertiesFromMaterial(pair.copy);Object.Destroy(pair.copy);}
            foreach(var pair in _lods)pair.live.SetLODs(pair.copy);
            if(_profiles!=null)for(var i=0;i<_offsets.Length;i++)_profiles.Profiles[i].MaximumWallOffset=_offsets[i];
            if(_hostile!=null)Object.Destroy(_hostile);if(_g0!=null)Object.Destroy(_g0);
        }
        [Serializable]private sealed class Snapshot{public string sourceCommit;public Surface[] materials;}
        [Serializable]private sealed class Surface{public string name;public Color baseColor,emission;public float bumpScale,smoothness,occlusionStrength;}
    }
}
