#ifndef VEIL_CITY_INCLUDED
#define VEIL_CITY_INCLUDED
// Globals the EnvironmentDirector sets once a frame. None is per material, so every city shader stays SRP-batcher safe.
// _VeilParams: x road distance in metres (wrapped to 1024, a multiple of every road pattern period), y presentation time,
// z wetness 0..1, w rain 0..1. _VeilLight: x window brightness, y lamp brightness, z sky reflection, w mirror reflection.
float4 _VeilParams, _VeilLight;
half4 _VeilSkyTint, _VeilHorizon;
TEXTURE2D(_VeilNoise); SAMPLER(sampler_VeilNoise);
TEXTURE2D(_VeilSkyTex); SAMPLER(sampler_VeilSkyTex);
TEXTURE2D(_VeilReflectionTex); SAMPLER(sampler_VeilReflectionTex);
// The camera's projection (OpenGL convention, as the reflection texture is addressed) times its view. A point on the
// ground plane is its own mirror image, so it lands on the same texel of the mirrored pass.
float4x4 _VeilMirrorVP;

// Road coordinates: world x and the distance along the road, continuous across streamed segments.
float2 VeilRoad(float3 positionWS) { return float2(positionWS.x, positionWS.z + _VeilParams.x); }
// _VeilCross: a crossroads' corner (xy) and its incoming road's heading (zw) in world space. _VeilCrossRoad: x the crossing's road
// distance (wrapped like _VeilParams.x), y 1 while a crossroads is laid out (StreetStreamer.Junction).
float4 _VeilCross, _VeilCrossRoad;
// Paving coordinates (metres right of the centre line, road distance). Through a crossroads the arms, and the pavement nearer an arm
// than the incoming road, take the arm's own coordinates, which are what VeilRoad gives once the street re-bases onto the chosen
// arm. `crossing` is 1 on the open crossing, where lane paint stops.
float2 VeilStreet(float3 positionWS, out float crossing)
{
    crossing = 0;
    if (_VeilCrossRoad.y < 0.5) return VeilRoad(positionWS);
    float2 rel = positionWS.xz - _VeilCross.xy, h = _VeilCross.zw;
    float2 m = float2(rel.x * h.y - rel.y * h.x, dot(rel, h));
    crossing = step(abs(m.x), 6.0) * step(abs(m.y), 6.0);
    float s = m.x < 0 ? -1.0 : 1.0;
    return abs(m.x) > abs(m.y) ? float2(-s * m.y, s * m.x + _VeilCrossRoad.x) : float2(m.x, m.y + _VeilCrossRoad.x);
}
float VeilHash(float2 p) { p = frac(p * float2(123.34, 456.21)); p += dot(p, p + 45.32); return frac(p.x * p.y); }
half4 VeilNoise(float2 uv) { return SAMPLE_TEXTURE2D(_VeilNoise, sampler_VeilNoise, uv); }

// The painted sky plate as seen along `dir`: a cylinder 200 degrees wide, horizon at v = 0.18. Below the horizon the
// plate fades into the horizon colour, so a reflection never samples past its bottom edge.
half3 VeilSky(float3 dir)
{
    float u = atan2(dir.x, dir.z) / (PI * 1.111) + 0.5, v = 0.18 + asin(clamp(dir.y, -1.0, 1.0)) / (PI * 0.5) * 0.82;
    half3 plate = SAMPLE_TEXTURE2D_LOD(_VeilSkyTex, sampler_VeilSkyTex, float2(u, saturate(v)), 0).rgb * _VeilSkyTint.rgb;
    return lerp(_VeilHorizon.rgb, plate, smoothstep(-0.05, 0.12, dir.y));
}

// The sky as standing water sees it: the plate's bright cloud band sits low, and a night sky is darkest overhead, so
// near-vertical reflections (a high or zoomed-out camera) fall off instead of turning puddles pale.
half3 VeilReflectedSky(float3 dir) { return VeilSky(dir) * lerp(1.0, 0.3, smoothstep(0.35, 0.85, dir.y)); }

// Wet-surface reflection: the painted sky along the reflected ray plus the mirrored emissive pass (lamps, windows,
// signs) at this pixel, offset by the surface normal so ripples and roughness break it up.
half3 VeilReflect(float3 normalWS, float3 viewWS, float2 mirrorUV, float roughness)
{
    float3 r = reflect(-viewWS, normalWS);
    half3 sky = VeilReflectedSky(r) * _VeilLight.z;
    float2 uv = mirrorUV + normalWS.xz * (0.004 + roughness * 0.03);
    half3 mirror = SAMPLE_TEXTURE2D_LOD(_VeilReflectionTex, sampler_VeilReflectionTex, uv, 0).rgb * _VeilLight.w;
    return sky + mirror;
}

float2 VeilMirrorUV(float3 positionWS) { float4 p = mul(_VeilMirrorVP, float4(positionWS.x, 0, positionWS.z, 1)); return p.xy / p.w * 0.5 + 0.5; }

// Rain rings on standing water: one expanding ring per 0.33 m cell with a random phase and centre.
float2 VeilRipple(float2 road)
{
    float2 g = road * 3.0, id = floor(g), f = frac(g) - 0.5;
    float h = VeilHash(id), t = frac(_VeilParams.y * 0.8 + h);
    float2 c = float2(VeilHash(id + 7.1), VeilHash(id + 3.3)) - 0.5;
    float2 d = f - c * 0.5; float len = length(d) + 1e-4;
    float ring = sin((len - t * 0.45) * 42.0) * saturate(1.0 - abs(len - t * 0.45) * 9.0) * (1.0 - t);
    return d / len * ring * step(0.35, h);
}
#endif
