float3 RGBToHSV(float3 In)
{
    float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
    float4 P = lerp(float4(In.bg, K.wz), float4(In.gb, K.xy), step(In.b, In.g));
    float4 Q = lerp(float4(P.xyw, In.r), float4(In.r, P.yzx), step(P.x, In.r));
    float D = Q.x - min(Q.w, Q.y);
    float E = 1e-10;
    return float3(abs(Q.z + (Q.w - Q.y)/(6.0 * D + E)), D / (Q.x + E), Q.x);
}

float3 HSVToRGB(float3 In)
{
    float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    float3 P = abs(frac(In.xxx + K.xyz) * 6.0 - K.www);
    return In.z * lerp(K.xxx, saturate(P - K.xxx), In.y);
}

void HSVLerp_float(float4 A, float4 B, float T, out float4 Out)
{
    A.xyz = RGBToHSV(A.xyz);
    B.xyz = RGBToHSV(B.xyz);

    float t = T; // used to lerp alpha, needs to remain unchanged

    float hue;
    float d = B.x - A.x; // hue difference

    if(A.x > B.x) 
    {
        float temp = B.x;
        B.x = A.x;
        A.x = temp;

        d = -d;
        T = 1-T;
    }

    if(d > 0.5) 
    {
        A.x = A.x + 1;
        hue = (A.x + T * (B.x - A.x)) % 1;
    }

    if(d <= 0.5) hue = A.x + T * d;

    float sat = A.y + T * (B.y - A.y);
    float val = A.z + T * (B.z - A.z);
    float alpha = A.w + t * (B.w - A.w);

    float3 rgb = HSVToRGB(float3(hue,sat,val));
    
    Out = float4(rgb, alpha);
}