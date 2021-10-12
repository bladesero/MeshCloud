Shader "URP/BlitScreenCloud"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        //_Tex_Source ("Source",2D) = "white" {}
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _MeshCloudBuffer;
            sampler2D _CameraColorTexture;
            float4 _MainTex_TexelSize;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                //col=float4(1,1,0,1);
                //fixed4 col2 = tex2D(_MeshCloudBuffer, i.uv);
                //col.rgb +=col2;
                return fixed4(col);
            }
            ENDCG
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float4 _MainTex_ST;
            sampler2D _MeshCloudBuffer;
            float4 _MeshCloudBuffer_TexelSize;
            
            
            #define SAMPLE_BASEMAP(uv) tex2D(_MainTex, uv)

            float4 SimpleBlur(float2 uv,float2 delta)
            {
                float4 p0 = SAMPLE_BASEMAP(uv                             );
                float4 p1 = SAMPLE_BASEMAP(uv + float2(-delta.x, -delta.y));
                float4 p2 = SAMPLE_BASEMAP(uv + float2( delta.x, -delta.y));
                float4 p3 = SAMPLE_BASEMAP(uv + float2(-delta.x,  delta.y));
                float4 p4 = SAMPLE_BASEMAP(uv + float2( delta.x,  delta.y));
                return (p0+p1+p2+p3+p4)/5;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = SimpleBlur(i.uv,_MeshCloudBuffer_TexelSize.xy*2.5);
                return fixed4(col);
            }
            ENDCG
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _MeshCloudBuffer2;
            float4 _MainTex_TexelSize;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                //col=float4(1,1,0,1);
                fixed4 col2 = tex2D(_MeshCloudBuffer2, i.uv);
                fixed4 coloramp=lerp(fixed4(0.3,0.3,0.3,1),fixed4(1,1,1,1),col2.r);
                col=lerp(col,coloramp,col2.g);
                //col.rgb +=col2;
                return fixed4(col.rgb,1.0);
            }
            ENDCG
        }
    }
}
