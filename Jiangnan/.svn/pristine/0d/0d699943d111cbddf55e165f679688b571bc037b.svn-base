Shader "Berryera/Particles/Mask Alpha Blended Tint" 
{
	Properties
    {
        _TintColor("Tint Color",Color) = (0.5,0.5,0.5,0.5)
        _MainTex ("Particle Texture", 2D) = "white" {}
        _Mask("Mask",2D) = "white"{}
    }

    SubShader
    {
        Tags{"IgnoreProjector"="True" "Queue" = "Transparent" "RenderType" = "Transparent"}
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
				fixed4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 uv : TEXCOORD0;
				fixed4 color : COLOR;
                float4 vertex : SV_POSITION;
                float2 uv2: TEXCOORD1;
            };

            sampler2D _MainTex; 
			float4 _MainTex_ST;
            fixed4 _TintColor;
            sampler2D _Mask; 
			float4 _Mask_ST;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

                o.uv.xy = TRANSFORM_TEX(v.uv, _MainTex);

                o.uv.zw = TRANSFORM_TEX(v.uv,_Mask);
				
				o.color = v.color;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 c ;

                fixed4 mainTex = tex2D(_MainTex, i.uv.xy);
                c = mainTex;

                fixed4 maskTex = tex2D(_Mask,i.uv.zw);
                c *= maskTex;

				c *= _TintColor*2;
			
				c *= i.color;

                return  c;
            }
            ENDCG
        }
    }
}