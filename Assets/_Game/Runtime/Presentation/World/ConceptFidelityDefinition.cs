using System;
using Gravivore.Presentation.Assets;
using UnityEngine;

namespace Gravivore.Presentation.World
{
    [CreateAssetMenu(menuName="Gravivore/Presentation/Concept Fidelity V2")]
    public sealed class ConceptFidelityDefinition : ScriptableObject
    {
        [SerializeField] private GameObject _pedestal, _upper, _forearm, _joint, _tool, _scanner;
        [SerializeField] private Vector3 _leftMount = new Vector3(-2.55f,.08f,-.45f);
        [SerializeField] private Vector3 _rightMount = new Vector3(2.55f,.08f,-.45f);
        [SerializeField, Min(.1f)] private float _engagementSpeed=2.5f;
        [SerializeField, Min(.1f)] private float _shoulderHeight=1.25f;
        [SerializeField, Min(.1f)] private float _atmosphereInterval=7f;
        [SerializeField, Min(.1f)] private float _atmosphereRange=14f;
        [SerializeField] private bool _mobileBloom=true;
        [SerializeField, Range(0,2)] private int _localLightCount=2;
        [SerializeField, Min(.1f)] private float _localLightRange=6;
        [SerializeField, Min(0)] private float _localLightIntensity=2.2f;
        public GameObject Pedestal=>_pedestal;
        public GameObject Upper=>_upper;
        public GameObject Forearm=>_forearm;
        public GameObject Joint=>_joint;
        public GameObject Tool=>_tool;
        public GameObject Scanner=>_scanner;
        public Vector3 Mount(int index)=>index==0?_leftMount:_rightMount;
        public float EngagementSpeed=>_engagementSpeed;
        public float ShoulderHeight=>_shoulderHeight;
        public float AtmosphereInterval=>_atmosphereInterval;
        public float AtmosphereRange=>_atmosphereRange;
        public bool MobileBloom=>_mobileBloom;
        public int LocalLightCount=>_localLightCount;
        public float LocalLightRange=>_localLightRange;
        public float LocalLightIntensity=>_localLightIntensity;
        public void ValidateOrThrow()
        {
            foreach(var prefab in new[]{_pedestal,_upper,_forearm,_joint,_tool,_scanner})
            {
                if(prefab==null)throw new InvalidOperationException("Authored repair machine kit is incomplete.");
                PresentationPrefabValidation.ValidateOrThrow(prefab);
            }
            PresentationModelBinding.ValidateTransform(_leftMount,Vector3.zero,Vector3.one);
            PresentationModelBinding.ValidateTransform(_rightMount,Vector3.zero,Vector3.one);
            foreach(var value in new[]{_engagementSpeed,_shoulderHeight,_atmosphereInterval,_atmosphereRange})
                if(float.IsNaN(value)||float.IsInfinity(value)||value<=0)
                    throw new InvalidOperationException("Fidelity presentation settings must be finite and positive.");
            if(_localLightCount<0||_localLightCount>2||!float.IsFinite(_localLightRange)||_localLightRange<=0||
                !float.IsFinite(_localLightIntensity)||_localLightIntensity<0)
                throw new InvalidOperationException("Local energy response is bounded to two unshadowed lights.");
        }
    }
}
