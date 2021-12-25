//UNITY_SHADER_NO_UPGRADE
#ifndef COMPUTE_AO_HLSLINCLUDE_INCLUDED
#define COMPUTE_AO_HLSLINCLUDE_INCLUDED

float compute_ao(const float4 curve, float index, const float intensity, const float power) {
    return pow(curve[index] * intensity, power);
}

void compute_ao_float(const float4 curve, const float4 values, const float intensity, const float power, out float4 ao)
{
    ao = float4(
        compute_ao(curve, values[0], intensity, power),
        compute_ao(curve, values[1], intensity, power),
        compute_ao(curve, values[2], intensity, power),
        compute_ao(curve, values[3], intensity, power)
    );
}
#endif //COMPUTE_AO_HLSLINCLUDE_INCLUDED