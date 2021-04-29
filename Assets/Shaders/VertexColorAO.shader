Shader "VloxyEngine/VertexColorAO" {
    Properties {
        _Glossiness ("Smoothness", Range(0,1)) = 0.2
        _Metallic ("Metallic", Range(0,1)) = 0.1
        _AOColor ("AO Color", Color) = (0,0,0,1)
        _AOCurve ("AO Curve", Vector) = (0.8, 0.4, 0.2, 0)
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 200
        
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma vertex vert
        #pragma target 3.0

        struct Input {
            float4 color: COLOR;
            half ao;
        };

        half _Glossiness;
        half _Metallic;
        half4 _AOColor;
        float4 _AOCurve;
        
        void vert (inout appdata_full v, out Input o) {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.ao = _AOCurve[v.texcoord1.x];
        }
        
        void surf (Input IN, inout SurfaceOutputStandard o) {
            o.Albedo = lerp(IN.color.rgb, _AOColor, IN.ao);
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = IN.color.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}