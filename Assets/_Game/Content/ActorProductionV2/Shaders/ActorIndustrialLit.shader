Shader "Gravivore/Actor Industrial Lit"
{
    Properties
    {
        _BaseMap("Authored base colour", 2D) = "white" {}
        _BaseColor("Surface tint", Color) = (1,1,1,1)
        _BumpMap("Machined normal", 2D) = "bump" {}
        _BumpScale("Relief", Float) = 0.35
        _MetallicGlossMap("Metal / smoothness", 2D) = "white" {}
        _Metallic("Metal fallback", Range(0,1)) = 0
        _Smoothness("Smoothness response", Range(0,1)) = 0.8
        _OcclusionMap("Cavity", 2D) = "white" {}
        _OcclusionStrength("Cavity strength", Range(0,1)) = 0.85
        _EmissionMap("Contained energy", 2D) = "black" {}
        [HDR] _EmissionColor("Energy gain", Color) = (1.5,1.5,1.5,1)
        _ActorSky("Industrial ambient upper irradiance", Vector) = (0.22,0.26,0.32,1)
        _ActorGround("Industrial ambient lower irradiance", Vector) = (0.055,0.065,0.085,1)
        _ActorFill("Diffuse environment fill", Range(0,2)) = 0.65
        _Cull("Cull", Float) = 2
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
        TEXTURE2D(_BumpMap); SAMPLER(sampler_BumpMap);
        TEXTURE2D(_MetallicGlossMap); SAMPLER(sampler_MetallicGlossMap);
        TEXTURE2D(_OcclusionMap); SAMPLER(sampler_OcclusionMap);
        TEXTURE2D(_EmissionMap); SAMPLER(sampler_EmissionMap);
        CBUFFER_START(UnityPerMaterial)
        float4 _BaseMap_ST;
        half4 _BaseColor, _EmissionColor, _ActorSky, _ActorGround;
        half _BumpScale, _Metallic, _Smoothness, _OcclusionStrength, _ActorFill, _Cull;
        CBUFFER_END
        ENDHLSL
        Pass
        {
            Name "IndustrialForward"
            Tags { "LightMode"="UniversalForward" }
            ZWrite On Cull [_Cull]
            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma shader_feature_local_fragment _EMISSION
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            struct Attributes { float4 positionOS:POSITION; half3 normalOS:NORMAL; half4 tangentOS:TANGENT; float2 uv:TEXCOORD0; UNITY_VERTEX_INPUT_INSTANCE_ID };
            struct Varyings { float4 positionCS:SV_POSITION; float3 positionWS:TEXCOORD0; half3 normalWS:TEXCOORD1; half4 tangentWS:TEXCOORD2; float2 uv:TEXCOORD3; half4 fogAndVertexLight:TEXCOORD4; UNITY_VERTEX_INPUT_INSTANCE_ID UNITY_VERTEX_OUTPUT_STEREO };
            Varyings Vert(Attributes input)
            {
                Varyings o; UNITY_SETUP_INSTANCE_ID(input); UNITY_TRANSFER_INSTANCE_ID(input,o); UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                VertexPositionInputs p=GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs n=GetVertexNormalInputs(input.normalOS,input.tangentOS);
                o.positionCS=p.positionCS; o.positionWS=p.positionWS; o.normalWS=n.normalWS;
                o.tangentWS=half4(n.tangentWS,input.tangentOS.w*GetOddNegativeScale());
                o.uv=TRANSFORM_TEX(input.uv,_BaseMap);
                o.fogAndVertexLight=half4(ComputeFogFactor(p.positionCS.z),VertexLighting(p.positionWS,n.normalWS));
                return o;
            }
            half4 Frag(Varyings input):SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input); UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                half3 bitangent=input.tangentWS.w*cross(input.normalWS,input.tangentWS.xyz);
                half3 n=UnpackNormalScale(SAMPLE_TEXTURE2D(_BumpMap,sampler_BumpMap,input.uv),_BumpScale);
                half3 normalWS=NormalizeNormalPerPixel(TransformTangentToWorld(n,half3x3(input.tangentWS.xyz,bitangent,input.normalWS)));
                half4 packed=SAMPLE_TEXTURE2D(_MetallicGlossMap,sampler_MetallicGlossMap,input.uv);
                SurfaceData surface=(SurfaceData)0;
                surface.albedo=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,input.uv).rgb*_BaseColor.rgb;
                surface.metallic=packed.r; surface.smoothness=packed.a*_Smoothness; surface.normalTS=n;
                surface.occlusion=lerp(1,SAMPLE_TEXTURE2D(_OcclusionMap,sampler_OcclusionMap,input.uv).g,_OcclusionStrength);
                #if defined(_EMISSION)
                surface.emission=SAMPLE_TEXTURE2D(_EmissionMap,sampler_EmissionMap,input.uv).rgb*_EmissionColor.rgb;
                #endif
                surface.alpha=1;
                InputData lighting=(InputData)0;
                lighting.positionWS=input.positionWS; lighting.normalWS=normalWS;
                lighting.viewDirectionWS=GetWorldSpaceNormalizeViewDir(input.positionWS);
                lighting.shadowCoord=TransformWorldToShadowCoord(input.positionWS);
                lighting.vertexLighting=input.fogAndVertexLight.yzw;
                // Moving machines have no baked lightmap. The same industrial upper/lower
                // irradiance is supplied in material data; real lights and the shared
                // reflection environment still use the complete URP metal BRDF.
                lighting.bakedGI=SampleSH(normalWS)+lerp(_ActorGround.rgb,_ActorSky.rgb,saturate(normalWS.y*.5+.5))*_ActorFill;
                lighting.normalizedScreenSpaceUV=GetNormalizedScreenSpaceUV(input.positionCS);
                lighting.shadowMask=half4(1,1,1,1);
                half4 color=UniversalFragmentPBR(lighting,surface);
                color.rgb=MixFog(color.rgb,input.fogAndVertexLight.x); return color;
            }
            ENDHLSL
        }
        Pass
        {
            Name "IndustrialShadow" Tags { "LightMode"="ShadowCaster" }
            ZWrite On ZTest LEqual ColorMask 0 Cull [_Cull]
            HLSLPROGRAM
            #pragma vertex ShadowVert
            #pragma fragment EmptyFrag
            #pragma multi_compile_instancing
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
            float3 _LightDirection, _LightPosition;
            struct A { float4 positionOS:POSITION;half3 normalOS:NORMAL;UNITY_VERTEX_INPUT_INSTANCE_ID };
            float4 ShadowVert(A a):SV_POSITION
            {
                UNITY_SETUP_INSTANCE_ID(a);float3 p=TransformObjectToWorld(a.positionOS.xyz);half3 n=TransformObjectToWorldNormal(a.normalOS);
                #if defined(_CASTING_PUNCTUAL_LIGHT_SHADOW)
                float3 direction=normalize(_LightPosition-p);
                #else
                float3 direction=_LightDirection;
                #endif
                float4 clip=TransformWorldToHClip(ApplyShadowBias(p,n,direction));
                #if UNITY_REVERSED_Z
                clip.z=min(clip.z,clip.w*UNITY_NEAR_CLIP_VALUE);
                #else
                clip.z=max(clip.z,clip.w*UNITY_NEAR_CLIP_VALUE);
                #endif
                return clip;
            }
            half4 EmptyFrag():SV_Target{return 0;}
            ENDHLSL
        }
        Pass
        {
            Name "IndustrialDepth" Tags { "LightMode"="DepthOnly" }
            ZWrite On ColorMask R Cull [_Cull]
            HLSLPROGRAM
            #pragma vertex DepthVert
            #pragma fragment EmptyFrag
            #pragma multi_compile_instancing
            struct A {float4 positionOS:POSITION;UNITY_VERTEX_INPUT_INSTANCE_ID};
            float4 DepthVert(A a):SV_POSITION{UNITY_SETUP_INSTANCE_ID(a);return TransformObjectToHClip(a.positionOS.xyz);}
            half4 EmptyFrag():SV_Target{return 0;}
            ENDHLSL
        }
        Pass
        {
            Name "IndustrialNormals" Tags { "LightMode"="DepthNormals" }
            ZWrite On Cull [_Cull]
            HLSLPROGRAM
            #pragma vertex NormalVert
            #pragma fragment NormalFrag
            #pragma multi_compile_instancing
            #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT
            struct A {float4 positionOS:POSITION;half3 normalOS:NORMAL;half4 tangentOS:TANGENT;float2 uv:TEXCOORD0;UNITY_VERTEX_INPUT_INSTANCE_ID};
            struct V {float4 positionCS:SV_POSITION;half3 normalWS:TEXCOORD0;half4 tangentWS:TEXCOORD1;float2 uv:TEXCOORD2;};
            V NormalVert(A a){UNITY_SETUP_INSTANCE_ID(a);V v;v.positionCS=TransformObjectToHClip(a.positionOS.xyz);VertexNormalInputs n=GetVertexNormalInputs(a.normalOS,a.tangentOS);v.normalWS=n.normalWS;v.tangentWS=half4(n.tangentWS,a.tangentOS.w*GetOddNegativeScale());v.uv=TRANSFORM_TEX(a.uv,_BaseMap);return v;}
            half4 NormalFrag(V v):SV_Target
            {
                half3 bitangent=v.tangentWS.w*cross(v.normalWS,v.tangentWS.xyz);half3 n=UnpackNormalScale(SAMPLE_TEXTURE2D(_BumpMap,sampler_BumpMap,v.uv),_BumpScale);
                half3 normalWS=NormalizeNormalPerPixel(TransformTangentToWorld(n,half3x3(v.tangentWS.xyz,bitangent,v.normalWS)));
                #if defined(_GBUFFER_NORMALS_OCT)
                float2 oct=PackNormalOctQuadEncode(normalWS);return half4(PackFloat2To888(saturate(oct*.5+.5)),0);
                #else
                return half4(normalWS,0);
                #endif
            }
            ENDHLSL
        }
    }
}
