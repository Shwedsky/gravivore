Shader "Gravivore/Diagnostics/Actor Combat Coverage"
{
    Properties { _BaseMap("Alpha mask",2D)="white"{} _BaseColor("Alpha",Color)=(1,1,1,1) }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            Blend One One ZWrite Off ZTest LEqual Cull Off
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            TEXTURE2D(_BaseMap);SAMPLER(sampler_BaseMap);
            CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;half4 _BaseColor;
            CBUFFER_END
            struct A { float4 positionOS:POSITION;float2 uv:TEXCOORD0;half4 color:COLOR; };
            struct V { float4 positionCS:SV_POSITION;float2 uv:TEXCOORD0;half4 color:COLOR; };
            V Vert(A a){V v;v.positionCS=TransformObjectToHClip(a.positionOS.xyz);v.uv=TRANSFORM_TEX(a.uv,_BaseMap);v.color=a.color;return v;}
            half4 Frag(V v):SV_Target{half alpha=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,v.uv).a*_BaseColor.a*v.color.a;clip(alpha-.025h);return half4(.0625h,0,0,0);}
            ENDHLSL
        }
    }
}
