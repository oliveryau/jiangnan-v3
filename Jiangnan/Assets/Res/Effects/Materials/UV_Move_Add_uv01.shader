// URP 版：保留原 Shader Forge 逻辑（UV 滚动 + Mask + 顶点色 + Additive）
Shader "MOBO/UV_Move_Add_uv01"
{
    Properties
    {
        _Texture ("Texture", 2D) = "white" {}
        [HDR] _Color ("Color", Color) = (0.5, 0.5, 0.5, 1)
        _Mask ("Mask", 2D) = "white" {}
        _U_Move ("U_Move", Float) = 0
        _V_Move ("V_Move", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
        }

        Pass
        {
            Name "Forward"
            Tags { "LightMode" = "UniversalForward" }

            Blend One One
            Cull Off
            ZWrite Off
            ZTest LEqual

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_Texture);
            SAMPLER(sampler_Texture);
            TEXTURE2D(_Mask);
            SAMPLER(sampler_Mask);

            CBUFFER_START(UnityPerMaterial)
                float4 _Texture_ST;
                float4 _Mask_ST;
                half4 _Color;
                float _U_Move;
                float _V_Move;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;
                output.uv = input.uv;
                output.color = input.color;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // 与原 shader 一致：uv + (_U_Move * t, _V_Move * t)，再乘 _Texture_ST
                float scrollT = _Time.y;
                float2 scrolledUv = input.uv + float2(_U_Move * scrollT, _V_Move * scrollT);
                float2 texUv = scrolledUv * _Texture_ST.xy + _Texture_ST.zw;
                float2 maskUv = input.uv * _Mask_ST.xy + _Mask_ST.zw;

                half4 tex = SAMPLE_TEXTURE2D(_Texture, sampler_Texture, texUv);
                half4 mask = SAMPLE_TEXTURE2D(_Mask, sampler_Mask, maskUv);

                half3 emissive = _Color.rgb * tex.rgb * input.color.rgb * mask.rgb * _Color.a;
                emissive *= input.color.a * tex.a;

                return half4(emissive, 1.0h);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
