using System;
using Gravivore.Presentation.AudioVfx;
using UnityEngine;

namespace Gravivore.Presentation.World
{
    /// <summary>One reused spark burst, only near an authored service fault. No per-frame allocation.</summary>
    public sealed class FidelityAtmospherePresenter : MonoBehaviour
    {
        private Transform _player;
        private Transform[] _faults;
        private ConceptFidelityDefinition _settings;
        private Phase6BVfxPool _pool;
        private float _remaining;
        private int _cursor;
        private Transform[] _sources;
        private Color[] _sourceColors;
        private Light[] _lights;
        private ParticleSystem[] _mist;
        public int AtmosphericParticleCapacity=>32;
        public int LightCount=>_lights?.Length??0;
        public int Capacity=>_pool!=null?_pool.CreatedInstanceCount:0;
        public void Initialize(ChapterVisualEnvironment environment,Transform player,Phase6BProductionDefinition effects)
        {
            if(_pool!=null)throw new InvalidOperationException("Atmosphere is already initialized.");
            _settings=environment.Definition.Fidelity;_player=player;
            var faults=new System.Collections.Generic.List<Transform>();
            foreach(var node in environment.Floor.GetComponentsInChildren<Transform>(true))
                if(node.name=="Fidelity service fault")faults.Add(node);
            _faults=faults.ToArray();
            var sources=new System.Collections.Generic.List<Transform>();
            foreach(var node in environment.Floor.GetComponentsInChildren<Transform>(true))
                if(node.name.StartsWith("Fidelity light ",StringComparison.Ordinal))sources.Add(node);
            _sources=sources.ToArray();_lights=new Light[_settings.LocalLightCount];
            _sourceColors=new Color[_sources.Length];
            for(var i=0;i<_sources.Length;i++)
                _sourceColors[i]=_sources[i].name.EndsWith("red",StringComparison.Ordinal)?new Color(1,.16f,.05f):
                    _sources[i].name.EndsWith("cyan",StringComparison.Ordinal)?new Color(.10f,.8f,1):
                    _sources[i].name.EndsWith("white",StringComparison.Ordinal)?new Color(.8f,.9f,1):new Color(1,.48f,.10f);
            for(var i=0;i<_lights.Length;i++)
            {
                var obj=new GameObject("Reused internal energy response "+i,typeof(Light));obj.transform.SetParent(transform,false);
                var light=obj.GetComponent<Light>();light.type=LightType.Point;light.shadows=LightShadows.None;
                light.range=_settings.LocalLightRange;light.intensity=_settings.LocalLightIntensity;light.enabled=false;_lights[i]=light;
            }
            var repair=effects.CreateRepairBindings();
            _pool=gameObject.AddComponent<Phase6BVfxPool>();
            _pool.Initialize(new[]{repair[1]});_remaining=_settings.AtmosphereInterval;
            // Two prebuilt reused systems: four steam puffs and eight drifting motes per nearby fault.
            var material=_pool.GetComponentInChildren<ParticleSystemRenderer>(true).sharedMaterial;
            _mist=new ParticleSystem[2];
            for(var i=0;i<_mist.Length;i++)
            {
                var obj=new GameObject(i==0?"Reused service steam":"Reused drifting dust",typeof(ParticleSystem));obj.transform.SetParent(transform,false);
                var particles=obj.GetComponent<ParticleSystem>();particles.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
                var main=particles.main;main.playOnAwake=false;main.loop=false;main.simulationSpace=ParticleSystemSimulationSpace.World;
                main.maxParticles=i==0?8:24;main.startLifetime=i==0?2.2f:3f;main.startSize=i==0?.65f:.045f;main.startSpeed=i==0?.35f:.12f;
                main.startColor=i==0?new Color(.55f,.66f,.7f,.12f):new Color(.6f,.72f,.78f,.35f);
                var emission=particles.emission;emission.enabled=false;var shape=particles.shape;shape.shapeType=ParticleSystemShapeType.Cone;shape.angle=20;shape.radius=i==0?.2f:1.5f;
                obj.transform.localRotation=Quaternion.Euler(-90,0,0);
                var color=particles.colorOverLifetime;color.enabled=true;
                var gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},new[]{new GradientAlphaKey(0,0),new GradientAlphaKey(1,.15f),new GradientAlphaKey(0,1)});color.color=gradient;
                var renderer=particles.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=material;renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;renderer.receiveShadows=false;
                _mist[i]=particles;
            }
        }
        private void Update()
        {
            if(_pool==null||_faults.Length==0)return;
            var first=-1;var second=-1;var firstDistance=float.MaxValue;var secondDistance=float.MaxValue;
            for(var i=0;i<_sources.Length;i++)
            {
                var distance=(_sources[i].position-_player.position).sqrMagnitude;
                if(distance<firstDistance){second=first;secondDistance=firstDistance;first=i;firstDistance=distance;}
                else if(distance<secondDistance){second=i;secondDistance=distance;}
            }
            for(var i=0;i<_lights.Length;i++)
            {
                var index=i==0?first:second;var distance=i==0?firstDistance:secondDistance;var light=_lights[i];
                light.enabled=index>=0&&distance<_settings.AtmosphereRange*_settings.AtmosphereRange;
                if(!light.enabled)continue;
                var source=_sources[index];light.transform.position=source.position;
                light.color=_sourceColors[index];
                light.intensity=_settings.LocalLightIntensity*(.97f+.03f*Mathf.Sin(Time.time*4.1f+index));
            }
            _remaining-=Time.deltaTime;if(_remaining>0)return;
            _remaining=_settings.AtmosphereInterval;
            for(var i=0;i<_faults.Length;i++)
            {
                var fault=_faults[_cursor];_cursor=(_cursor+1)%_faults.Length;
                if((fault.position-_player.position).sqrMagnitude>_settings.AtmosphereRange*_settings.AtmosphereRange)continue;
                _pool.TryPlay(Phase6BVfxCue.WeldingSparks,fault.position,fault.position,.24f);
                _mist[0].transform.position=fault.position;_mist[0].Emit(4);
                _mist[1].transform.position=fault.position+Vector3.up*1.5f;_mist[1].Emit(8);break;
            }
        }
        private void OnDisable(){_pool?.StopAll();if(_lights!=null)foreach(var light in _lights)light.enabled=false;if(_mist!=null)foreach(var particles in _mist)particles.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);}
    }
}
