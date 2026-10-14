Shader "GRAVIVORE/EnergyChamberRuntimeLit"
{
    Properties
    {
        _BaseMap("Albedo / wear",2D)="white"{}
        _BaseColor("Surface tint",Color)=(1,1,1,1)
        _MetallicGlossMap("Metallic (R), smoothness (A)",2D)="white"{}
        _Metallic("Fallback metallic",Range(0,1))=.8
        _Smoothness("Smoothness multiplier",Range(0,1))=1
        [Normal]_BumpMap("Surface normal",2D)="bump"{}
        _BumpScale("Normal strength",Range(0,2))=.65
        _OcclusionMap("Occlusion",2D)="white"{}
        _OcclusionStrength("Occlusion strength",Range(0,1))=1
        _EmissionMap("Authored energy mask",2D)="white"{}
        [HDR]_EmissionColor("Primary energy",Color)=(0,0,0,1)
        _ChamberOrigin("Installation world origin",Vector)=(11,.08,50.65,0)
        _SpillStrength("Local surface spill",Range(0,2))=1
        _PracticalStrength("Selective practical sources",Range(0,2))=1
        _EnergyRole("Energy role: 0 static, 1 core, 2 node",Float)=0
        _SurfaceClass("Casing upper-machined zone",Float)=0
        _OperationStrength("Slow operational amplitude",Range(0,.2))=.12
        _EmissionScale("Emission / spill diagnostic scale",Range(0,1))=1
        _PreviewTime("Deterministic review time (-1 = live)",Float)=-1
        [HideInInspector]_SpecColor("Specular",Color)=(.2,.2,.2,1)
        [HideInInspector]_Cutoff("Cutoff",Float)=.5
        [HideInInspector]_Surface("Surface",Float)=0
        [HideInInspector]_Cull("Cull",Float)=2
    }
    SubShader
    {
        Tags{"RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry"}
        LOD 300
        HLSLINCLUDE
        #include "EnergyChamberSurface.hlsl"
        ENDHLSL
        Pass
        {
            Name "ForwardLit"
            Tags{"LightMode"="UniversalForward"}
            ZWrite On
            Cull [_Cull]
            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex LitPassVertex
            #pragma fragment ECFragment
            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local_fragment _METALLICSPECGLOSSMAP
            #pragma shader_feature_local_fragment _EMISSION
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_ATLAS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #pragma multi_compile _ EVALUATE_SH_MIXED EVALUATE_SH_VERTEX
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitForwardPass.hlsl"
            half4 ECFragment(Varyings input):SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                SurfaceData surface;
                InitializeStandardLitSurfaceData(input.uv,surface);
                InputData data;
                InitializeInputData(input,surface.normalTS,data);
                InitializeBakedGIData(input,data);
                ApplyEnergyChamberPresentation(input.positionWS,data.normalWS,surface);
                half4 color=UniversalFragmentPBR(data,surface);
                color.rgb=MixFog(color.rgb,data.fogCoord);color.a=1;
                return color;
            }
            ENDHLSL
        }
        Pass
        {
            Name "ShadowCaster"
            Tags{"LightMode"="ShadowCaster"}
            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull [_Cull]
            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            #pragma multi_compile_instancing
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }
        Pass
        {
            Name "DepthOnly"
            Tags{"LightMode"="DepthOnly"}
            ZWrite On
            ColorMask R
            Cull [_Cull]
            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
            ENDHLSL
        }
        Pass
        {
            Name "DepthNormals"
            Tags{"LightMode"="DepthNormals"}
            ZWrite On
            Cull [_Cull]
            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex DepthNormalsVertex
            #pragma fragment DepthNormalsFragment
            #pragma shader_feature_local _NORMALMAP
            #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthNormalsPass.hlsl"
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
