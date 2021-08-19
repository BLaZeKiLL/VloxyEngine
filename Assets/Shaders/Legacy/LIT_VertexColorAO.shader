Shader "VloxyEngine/Lit/VertexColorAO" {
    Properties {
        _Glossiness ("Smoothness", Range(0,1)) = 0.0
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _AOColor ("AO Color", Color) = (0,0,0,1)
        _AOIntensity ("AO Intensity", Range(0, 1)) = 1.0
		_AOPower ("AO Power", Range(0, 1)) = 1.0
    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 200
        
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma vertex vert
        #pragma target 3.0

        struct Input {
            float4 color : COLOR;
            float2 aocoords;
            float4 aovector;
        };

        half _Glossiness;
        half _Metallic;
        
        half4 _AOColor;
        float _AOIntensity;
		float _AOPower;

        float compute_ao(float ao) {
            return pow(ao * _AOIntensity, _AOPower);
        }
        
        void vert (inout appdata_full v, out Input o) {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.aocoords = v.texcoord.xy;
            o.aovector = float4(compute_ao(v.texcoord1.x), compute_ao(v.texcoord1.y), compute_ao(v.texcoord1.z), compute_ao(v.texcoord1.w));
        }
        
        void surf (Input IN, inout SurfaceOutputStandard o) {
            float ao1 = lerp(IN.aovector.x, IN.aovector.z, IN.aocoords.x);
            float ao2 = lerp(IN.aovector.y, IN.aovector.w, IN.aocoords.x);
            float ao = lerp(ao1, ao2, IN.aocoords.y);
        
            o.Albedo = lerp(_AOColor.rgb, IN.color.rgb, ao);
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = IN.color.a;
        }
        
        ENDCG
    }
    FallBack "Diffuse"
}