#ifndef GRAVIVORE_ENERGY_CHAMBER_SURFACE
#define GRAVIVORE_ENERGY_CHAMBER_SURFACE
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/DebugMipmapStreamingMacros.hlsl"

// Identical layout in forward, shadow and depth passes: SRP Batcher compatible.
CBUFFER_START(UnityPerMaterial)
float4 _BaseMap_ST;
float4 _BaseMap_TexelSize;
half4 _BaseColor;
half4 _SpecColor;
half4 _EmissionColor;
float4 _ChamberOrigin;
half _Cutoff;
half _Smoothness;
half _Metallic;
half _BumpScale;
half _OcclusionStrength;
half _Surface;
half _SpillStrength;
half _PracticalStrength;
half _EnergyRole;
half _SurfaceClass;
half _OperationStrength;
half _EmissionScale;
float _PreviewTime;
UNITY_TEXTURE_STREAMING_DEBUG_VARS;
CBUFFER_END
TEXTURE2D(_MetallicGlossMap); SAMPLER(sampler_MetallicGlossMap);
TEXTURE2D(_OcclusionMap); SAMPLER(sampler_OcclusionMap);

void InitializeStandardLitSurfaceData(float2 uv,out SurfaceData s)
{
    s=(SurfaceData)0;
    half4 base=SampleAlbedoAlpha(uv,TEXTURE2D_ARGS(_BaseMap,sampler_BaseMap));
    s.albedo=base.rgb*_BaseColor.rgb;
    s.alpha=1;
    s.specular=_SpecColor.rgb;
#ifdef _METALLICSPECGLOSSMAP
    half4 metal=SAMPLE_TEXTURE2D(_MetallicGlossMap,sampler_MetallicGlossMap,uv);
    s.metallic=metal.r;s.smoothness=metal.a*_Smoothness;
#else
    s.metallic=_Metallic;s.smoothness=_Smoothness;
#endif
    s.normalTS=SampleNormal(uv,TEXTURE2D_ARGS(_BumpMap,sampler_BumpMap),_BumpScale);
    s.occlusion=lerp(1,SAMPLE_TEXTURE2D(_OcclusionMap,sampler_OcclusionMap,uv).g,_OcclusionStrength);
    s.emission=SampleEmission(uv,_EmissionColor.rgb,TEXTURE2D_ARGS(_EmissionMap,sampler_EmissionMap));
}

half ECSoftBand(float coordinate,float center,float width)
{
    half b=saturate(1-abs(coordinate-center)/width);return b*b;
}
half ECIndicatorBand(float coordinate,float center,float width)
{
    return 1-smoothstep(width*.55,width,abs(coordinate-center));
}

// Local, opaque surface illumination, authored for the frozen installation.
// No Light objects, extra render passes, geometry, particles or CPU presenters.
void ApplyEnergyChamberPresentation(float3 worldPosition,half3 normalWS,inout SurfaceData s)
{
    float3 p=worldPosition-_ChamberOrigin.xyz;
    float t=_PreviewTime>=0?_PreviewTime:_Time.y;
    float radial=length(p.xz);
    bool xNode=abs(p.x)>abs(p.z);
    float2 node=xNode?float2(p.x>=0?4.4:-4.4,0):float2(0,p.z>=0?4.4:-4.4);
    float phase=xNode?(p.x>=0?.7:3.4):(p.z>=0?1.9:5.1);
    float3 nodeDelta=float3(p.x-node.x,p.y-clamp(p.y,1.45,3.5),p.z-node.y);
    half opening=ECSoftBand(abs(p.x)/max(radial,.05),.707,.34);
    half core=ECSoftBand(radial,1.42,1.02)*ECSoftBand(p.y,3.65,2.6)*(.25+.75*opening);
    half base=ECSoftBand(radial,2.18,.9)*ECSoftBand(p.y,1.4,.8);
    half cold=pow(saturate(1-length(nodeDelta)/1.65),2);
    // The accepted upper segmented plates are machined metal, while their
    // lower casing uses the colder painted-steel family on the same mesh.
    half cap=_SurfaceClass*step(5.55,p.y)*saturate((1.8-radial)*4);
    s.albedo=lerp(s.albedo,s.albedo*1.7+half3(.055,.072,.087),cap);
    s.metallic=lerp(s.metallic,.78,cap);s.smoothness=lerp(s.smoothness,.57,cap);
    half coreBreath=1+_OperationStrength*(.65*sin(t*.31)+.35*sin(t*.137+1.4));
    half nodeBreath=1+_OperationStrength*(.65*sin(t*(.24+phase*.009)+phase)+.35*sin(t*.119+phase*1.6));
    half emissionBreath=_EnergyRole<.5?1:(_EnergyRole<1.5?coreBreath:nodeBreath);
    s.emission*=emissionBreath;

    // Warm/cool response falls off on the nearest existing casing/frame/pipe.
    // Occlusion and underlying material remain visible through the spill.
    half3 warm=half3(1,.24,.035)*(core*.46+base*.27)*coreBreath;
    half3 cool=half3(.025,.39,.69)*cold*.44*nodeBreath;
    half3 incident=(warm+cool)*_SpillStrength*s.occlusion;
    s.emission+=incident*(s.albedo*1.4+half3(.09,.09,.09))*(.65+.35*abs(normalWS.y));

    // Selective small practical sources on existing service bands and hardware.
    // Short sectors, never an all-edge neon outline.
    float axial=max(abs(p.x),abs(p.z));float crossAxis=min(abs(p.x),abs(p.z));
    half sector=ECIndicatorBand(crossAxis,.42,.22);
    half plinthSlot=ECIndicatorBand(p.y,1.10,.055)*ECIndicatorBand(radial,2.63,.17)*sector;
    half lowerSlot=ECIndicatorBand(p.y,.34,.045)*ECIndicatorBand(radial,2.74,.12)*ECIndicatorBand(crossAxis,.5,.30);
    half collarSlot=ECIndicatorBand(p.y,2.08,.04)*ECIndicatorBand(radial,1.72,.24)*ECIndicatorBand(crossAxis,.38,.16);
    half upperIndicator=ECIndicatorBand(p.y,4.46,.035)*ECIndicatorBand(radial,1.50,.20)*ECIndicatorBand(crossAxis,.56,.09);
    half nodeIndicator=ECIndicatorBand(p.y,.929,.030)*ECIndicatorBand(axial,3.0,.038)*ECIndicatorBand(crossAxis,0,.085);
    float localSide=xNode?p.z*sign(p.x):-p.x*sign(p.z);
    half serviceIndicator=ECIndicatorBand(localSide,-1.043,.023)*ECIndicatorBand(p.y,.40,.036)*ECIndicatorBand(axial,4.9,.34);
    half conduit=ECSoftBand(crossAxis,0,.07)*ECSoftBand(p.y,.95,.10)*saturate((axial-2.35)*4)*saturate((4.05-axial)*4);
    conduit+=ECSoftBand(crossAxis,.21,.060)*ECSoftBand(p.y,1.60,.17)*saturate((axial-2.5)*4)*saturate((3.4-axial)*4);
    // A soft energy crest travels about .11 m/s; never switches off.
    half travel=.72+.28*pow(.5+.5*sin(axial*3.1-t*.34+phase),4);
    half practicalWarm=(plinthSlot+lowerSlot+collarSlot+upperIndicator+serviceIndicator)*(.93+.07*sin(t*.23+axial));
    half practicalCold=nodeIndicator*(.93+.07*sin(t*.19+phase));
    s.emission+=_PracticalStrength*(half3(3.4,.68,.055)*(practicalWarm+conduit*.42*travel)+half3(.045,1.0,1.9)*practicalCold);
    s.emission*=_EmissionScale;
}
#endif
