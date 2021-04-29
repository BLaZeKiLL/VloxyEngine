Shader "VloxyEngine/VertexColorAO" {
    Properties {
        _Glossiness ("Smoothness", Range(0,1)) = 0.2
        _Metallic ("Metallic", Range(0,1)) = 0.1
        _AOColor ("AO Color", Color) = (0,0,0,1)
        _AOCurve ("AO Curve", Vector) = (0.25, 0.175, 0.1, 0)
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 200
        
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma vertex vert

        #pragma target 3.0

        struct VertexAttribute {
            int ao;
        };

        struct AppData {
            float4 vertex : POSITION;
            float4 tangent : TANGENT;
            float3 normal : NORMAL;
            float4 texcoord : TEXCOORD0;
            float4 texcoord1 : TEXCOORD1;
            float4 texcoord2 : TEXCOORD2;
            float4 texcoord3 : TEXCOORD3;
            fixed4 color : COLOR;
            uint id : SV_VertexID;
        };
        
        struct Input {
            float4 color: COLOR;
            float4 ao;
        };

        half _Glossiness;
        half _Metallic;

        half4 _AOColor;
        float4 _AOCurve;

        #ifdef SHADER_API_D3D11	
            StructuredBuffer<VertexAttribute> attributes;
        #endif
        
        void vert (inout AppData v, out Input o) {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            #ifdef SHADER_API_D3D11
                o.ao.rgb = _AOColor;
                o.ao.a = _AOCurve[attributes[v.id].ao];
            #endif
        }
        
        void surf (Input IN, inout SurfaceOutputStandard o) {
            o.Albedo = lerp(IN.color.rgb, IN.ao.rgb, IN.ao.a);
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = IN.color.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}