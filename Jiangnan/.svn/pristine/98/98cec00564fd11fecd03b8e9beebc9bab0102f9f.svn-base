Shader "Custom/LoutiCustom"
{
    Properties
    {
        _BaseMap("Albedo", 2D) = "white" {}
        _BaseColor("Color", Color) = (1,1,1,1)
        _Alpha("Alpha (0=透明 1=不透明)", Range(0, 1)) = 1
        _OutlineColor("Outline Color", Color) = (0,0,0,1)
        _OutlineWidth("Outline Width", Range(0, 10)) = 2
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }

        // ── 主渲染 Pass ──
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite On
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile _ _ADDITIONAL_LIGHTS

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4  _BaseColor;
                half   _Alpha;
                half4  _OutlineColor;
                half   _OutlineWidth;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS  : SV_POSITION;
                float2 uv         : TEXCOORD0;
                float3 normalWS   : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
                float4 shadowCoord: TEXCOORD3;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv         = TRANSFORM_TEX(input.uv, _BaseMap);
                output.normalWS   = TransformObjectToWorldNormal(input.normalOS);
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.shadowCoord= TransformWorldToShadowCoord(output.positionWS);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv) * _BaseColor;

                half3 normalWS = normalize(input.normalWS);

                // 主光 + 阴影衰减
                ShadowSamplingData shadowData = GetMainLightShadowSamplingData();
                half shadowStrength = GetMainLightShadowStrength();
                Light mainLight = GetMainLight(input.shadowCoord);
                half NdotL = max(dot(normalWS, normalize(mainLight.direction)), 0.0h);

                half3 ambient = SampleSH(normalWS);
                half3 diffuse = albedo.rgb * mainLight.color * NdotL * mainLight.shadowAttenuation;

                // 附加光
                half3 additionalLightColor = 0;
                #ifdef _ADDITIONAL_LIGHTS
                uint lightsCount = GetAdditionalLightsCount();
                for (uint i = 0u; i < lightsCount; i++)
                {
                    Light light = GetAdditionalLight(i, input.positionWS);
                    additionalLightColor += albedo.rgb * light.color * max(dot(normalWS, light.direction), 0.0h) * light.distanceAttenuation * light.shadowAttenuation;
                }
                #endif

                half3 finalColor = ambient * albedo.rgb + diffuse + additionalLightColor;
                return half4(finalColor, albedo.a * _Alpha);
            }
            ENDHLSL
        }

        // ── ShadowCaster Pass (投射阴影) ──
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4  _BaseColor;
                half   _Alpha;
                half4  _OutlineColor;
                half   _OutlineWidth;
            CBUFFER_END

            float3 _LightDirection;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS   = TransformObjectToWorldNormal(input.normalOS);
                output.positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _LightDirection));
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }

        // ── 描边 Pass ──
        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "SRPDefaultUnlit" }

            Cull Front
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4  _BaseColor;
                half   _Alpha;
                half4  _OutlineColor;
                half   _OutlineWidth;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                float3 posVS    = TransformWorldToView(TransformObjectToWorld(input.positionOS.xyz));
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                float3 normalVS = normalize(mul((float3x3)UNITY_MATRIX_V, normalWS));

                float2 dir = normalVS.xy;
                float  len = max(length(dir), 0.0001);
                posVS.xy += (dir / len) * _OutlineWidth * 0.01 * abs(posVS.z);

                output.positionCS = mul(UNITY_MATRIX_P, float4(posVS, 1.0));
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half a = (_OutlineWidth > 0.0h) ? _OutlineColor.a : 0.0h;
                return half4(_OutlineColor.rgb, a * _Alpha);
            }
            ENDHLSL
        }
    }
}
