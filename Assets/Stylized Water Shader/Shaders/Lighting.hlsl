float LightingSpecular(float3 L, float3 N, float3 V, float smoothness)
{
    float3 H = SafeNormalize(float3(L) + float3(V));
    float NdotH = saturate(dot(N, H));
    return pow(NdotH, smoothness);
}

void MainLighting_float(float3 normalWS, float3 positionWS, float3 viewWS, float smoothness, out float specular)
{
    specular = 0.0;

    #ifndef SHADERGRAPH_PREVIEW
    smoothness = exp2(10 * smoothness + 1);
        
    normalWS = normalize(normalWS);
    viewWS = SafeNormalize(viewWS);

    Light mainLight = GetMainLight(TransformWorldToShadowCoord(positionWS));
    specular = LightingSpecular(mainLight.direction, normalWS, viewWS, smoothness);
    #endif
}

void AdditionalLighting_float(float3 normalWS, float3 positionWS, float3 viewWS, float smoothness, float hardness, out float3 specular)
{
    specular = 0;

    #ifndef SHADERGRAPH_PREVIEW
    smoothness = exp2(10 * smoothness + 1);

    normalWS = normalize(normalWS);
    viewWS = SafeNormalize(viewWS);

    // additional lights
    int pixelLightCount = GetAdditionalLightsCount();
    for (int i = 0; i < pixelLightCount; ++i)
    {
        Light light = GetAdditionalLight(i, positionWS);
        float3 attenuatedLight = light.color * light.distanceAttenuation * light.shadowAttenuation;
        
        float specular_soft = LightingSpecular(light.direction, normalWS, viewWS, smoothness);
        float specular_hard = smoothstep(0.005,0.01,specular_soft);
        float specular_term = lerp(specular_soft, specular_hard, hardness);

        specular += specular_term * attenuatedLight;
    }
    #endif
}