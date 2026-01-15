// Copyright (c) Meta Platforms, Inc. and affiliates.
// Modified for Matrix Digital Rain effect - replacing star/cyberpunk effects

Shader "TheWorldBeyond/OppyDimensionMatrix"
{
    Properties
    {
        _SaturationAmount("Saturation Amount", Range(0 , 1)) = 1
        _FogCubemap("Fog Cubemap", CUBE) = "white" {}
        _FogStrength("Fog Strength", Range(0 , 1)) = 1
        _FogStartDistance("Fog Start Distance", Range(0 , 100)) = 1
        _FogEndDistance("Fog End Distance", Range(0 , 2000)) = 100
        _FogExponent("Fog Exponent", Range(0 , 1)) = 1
        _LightingRamp("Lighting Ramp", 2D) = "white" {}
        _MainTex("MainTex", 2D) = "white" {}
        _TriPlanarFalloff("Triplanar Falloff", Range(0 , 10)) = 1
        _OppyPosition("Oppy Position", Vector) = (0,1000,0,0)
        _OppyRippleStrength("Oppy Ripple Strength", Range(0 , 1)) = 1
        _MaskRippleStrength("Mask Ripple Strength", Range(0, 1)) = 0

        _Color("Color", Color) = (0,0,0,0)

        // Matrix Digital Rain specific
        _MatrixTex("Matrix Texture", 2D) = "white" {}
        _MaskTex("Mask Texture", 2D) = "white" {}
        _MatrixColor("Matrix Color", Color) = (0, 1, 0.2, 1)
        _MatrixSpeed("Matrix Speed Y", Range(0, 5)) = 1.5
        _MatrixSpeedX("Matrix Speed X", Range(0, 1)) = 0
        _MatrixTiling("Matrix Tiling", Vector) = (3, 3, 0, 0)
        _MatrixEmission("Matrix Emission Strength", Range(0, 10)) = 3
        _MaskTiling("Mask Tiling", Vector) = (1.5, 1.5, 0, 0)
        _MaskOffsetMultiplier("Mask Offset Multiplier", Range(0, 5)) = 2

        // Raindrop effect parameters
        _DropLength("Drop Length", Range(0.1, 2.0)) = 0.6
        _DropDensity("Drop Density", Range(0.1, 3.0)) = 1.0
        _DropSpeed("Drop Speed Variation", Range(0, 2)) = 0.5
        _HeadBrightness("Head Brightness", Range(1, 5)) = 2.5
        _FadeLength("Fade Length", Range(0.1, 1.0)) = 0.4

        _EffectPosition("Effect Position", Vector) = (0,1000,0,1)
        _EffectTimer("Effect Timer", Range(0.0,1.0)) = 1.0
        _InvertedMask("Inverted Mask", float) = 1

        [HideInInspector] _texcoord("", 2D) = "white" {}
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque" "Queue" = "Geometry+0"
        }
        LOD 100

        CGINCLUDE
        #pragma target 3.0
        ENDCG
        AlphaToMask Off
        Cull Back
        ColorMask RGBA
        ZWrite On
        ZTest LEqual

        BlendOp Add, Min
        Blend One Zero, One Zero

        Offset 0 , 0

        Pass
        {
            Name "Base"
            Tags
            {
                "LightMode" = "ForwardBase"
            }
            CGPROGRAM
#ifndef UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX
            #define UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input)
#endif
#pragma vertex vert
#pragma fragment frag
#pragma multi_compile_instancing

#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "UnityShaderVariables.cginc"
#include "AutoLight.cginc"

            struct vertexInput {
                float4 vertex : POSITION;
                half3 normal : NORMAL;
                half4 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct vertexOutput {
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD0;
                half3 worldNormal : TEXCOORD1;
                half3 projNormal : TEXCOORD2;
                half3 normalSign : TEXCOORD3;
                half3 worldViewDirection : TEXCOORD4;
                half foggingRange : TEXCOORD5;

                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            uniform sampler2D _LightingRamp;
            uniform sampler2D _MainTex;
            uniform half4 _MainTex_ST;
            uniform half _TriPlanarFalloff;
            uniform half4 _Color;
            uniform samplerCUBE _FogCubemap;
            uniform half _FogStartDistance;
            uniform half _FogEndDistance;
            uniform half _FogExponent;
            uniform half _SaturationAmount;
            uniform half _FogStrength;
            uniform float3 _OppyPosition;
            uniform half _OppyRippleStrength;
            uniform half _MaskRippleStrength;
            uniform float4 _EffectPosition;
            uniform float _EffectTimer;
            uniform float _InvertedMask;

            // Matrix parameters
            uniform sampler2D _MatrixTex;
            uniform sampler2D _MaskTex;
            uniform half4 _MatrixColor;
            uniform half _MatrixSpeed;
            uniform half _MatrixSpeedX;
            uniform half4 _MatrixTiling;
            uniform half _MatrixEmission;
            uniform half4 _MaskTiling;
            uniform half _MaskOffsetMultiplier;

            // Raindrop parameters
            uniform half _DropLength;
            uniform half _DropDensity;
            uniform half _DropSpeed;
            uniform half _HeadBrightness;
            uniform half _FadeLength;

            // Hash function for pseudo-random values
            float hash(float n) {
                return frac(sin(n) * 43758.5453123);
            }

            float hash2(float2 p) {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453123);
            }

            inline half4 TriplanarSampler(
                    sampler2D projectedTexture,
                    float3 worldPos,
                    half3 normalSign,
                    half3 projNormal,
                    half2 tiling) {
                half4 xNorm = tex2D(
                        projectedTexture,
                        tiling * worldPos.zy * half2(normalSign.x, 1.0) + _MainTex_ST.zw);
                half4 yNorm = tex2D(
                        projectedTexture,
                        tiling * worldPos.xz * half2(normalSign.y, 1.0) + _MainTex_ST.zw);
                half4 zNorm = tex2D(
                        projectedTexture,
                        tiling * worldPos.xy * half2(-normalSign.z, 1.0) + _MainTex_ST.zw);
                return (xNorm * projNormal.x) + (yNorm * projNormal.y) + (zNorm * projNormal.z);
            }

            half3 fastPow(half3 a, half b) {
                return a / ((1.0 - b) * a + b);
            }

            // Calculate raindrop visibility and brightness for a single column
            half CalculateRaindrop(float columnId, float yPos, float time, float speedVariation) {
                // Generate multiple random values for this column using different seeds
                // These give continuous float values like 0.13, 0.67, 0.45 etc.
                float rand1 = hash(columnId * 127.1 + 311.7);
                float rand2 = hash(columnId * 269.5 + 183.3);
                float rand3 = hash(columnId * 419.2 + 571.9);

                // VERY LARGE random phase offset (0 to 50) for maximum stagger
                float basePhaseOffset = rand1 * 50.0 + rand2 * 25.0;

                // Random speed per column (0.2 to 1.5)
                float speedMult = 0.2 + rand2 * (1.3 + speedVariation);

                // Random drop length per column
                float dropLen = _DropLength * (0.4 + rand3 * 0.9);

                half totalBrightness = 0;

                // 2-3 drops per column
                for (int i = 0; i < 3; i++) {
                    // Unique continuous random values for each drop (not integer-based)
                    float dropSeed1 = hash(columnId * 73.13 + i * 151.71 + 0.31);
                    float dropSeed2 = hash(columnId * 37.91 + i * 293.37 + 0.79);
                    float dropSeed3 = hash(columnId * 191.37 + i * 97.13 + 0.17);

                    // Each drop has continuous fractional phase offset
                    // Values like 0.13, 7.45, 23.67 etc - not snapping to integers
                    float dropPhaseOffset = basePhaseOffset + dropSeed1 * 17.31 + dropSeed2 * 8.57 + i * 11.13;

                    // Speed variation per drop (continuous)
                    float dropSpeed = speedMult * (0.4 + dropSeed3 * 0.8);

                    // Animated phase - continuous values
                    float animPhase = time * dropSpeed * 0.15 + dropPhaseOffset;
                    float phase = frac(animPhase);

                    // Drop position
                    float dropHead = phase;
                    float dropTail = phase - dropLen;

                    // Y position with continuous random offset per column
                    // This shifts the whole column by a fractional amount (0.13, 0.67, etc)
                    float yShift = dropSeed1 * 0.97 + dropSeed2 * 0.43;
                    float yNorm = frac(yPos * _DropDensity + yShift);

                    // Check if inside drop (with wrap-around)
                    float inDrop = 0;
                    if (dropTail < 0) {
                        inDrop = (yNorm <= dropHead || yNorm >= (1.0 + dropTail)) ? 1 : 0;
                    } else {
                        inDrop = (yNorm >= dropTail && yNorm <= dropHead) ? 1 : 0;
                    }

                    if (inDrop > 0.5) {
                        float posInDrop;
                        if (dropTail < 0 && yNorm > 0.5) {
                            posInDrop = (yNorm - (1.0 + dropTail)) / dropLen;
                        } else {
                            posInDrop = (yNorm - dropTail) / dropLen;
                        }
                        posInDrop = saturate(posInDrop);

                        // Head glow + body brightness
                        float headGlow = pow(posInDrop, 0.3) * _HeadBrightness;
                        float bodyBright = posInDrop;

                        totalBrightness += headGlow * 0.5 + bodyBright * 0.4;
                    }
                }

                return saturate(totalBrightness);
            }

            // Matrix helper - sample with triplanar and animated UV + raindrop effect
            half4 SampleMatrixTriplanar(
                    sampler2D matrixTex,
                    sampler2D maskTex,
                    float3 worldPos,
                    half3 normalSign,
                    half3 projNormal,
                    half2 matrixTiling,
                    half2 maskTiling,
                    half speedY,
                    half speedX,
                    half maskMultiplier) {

                // Time-based offset
                half timeOffsetY = _Time.y * speedY;
                half timeOffsetX = _Time.y * speedX;

                // Sample mask for phase offset (creates fast/slow columns effect)
                half maskX = tex2D(maskTex, worldPos.zy * maskTiling).r;
                half maskY = tex2D(maskTex, worldPos.xz * maskTiling).r;
                half maskZ = tex2D(maskTex, worldPos.xy * maskTiling).r;

                // === XY plane (Z-facing surfaces) ===
                half2 uvXY = worldPos.xy * matrixTiling;
                uvXY.y += timeOffsetY + maskZ * maskMultiplier;
                uvXY.x += timeOffsetX;
                half4 matrixXY = tex2D(matrixTex, uvXY);
                // Calculate raindrop - use UV x coordinate for column ID with more precision
                float colIdXY = floor(uvXY.x * 7.0) + floor(worldPos.z * 3.0) * 100.0;
                half dropXY = CalculateRaindrop(colIdXY, uvXY.y, _Time.y * speedY, _DropSpeed);
                matrixXY *= dropXY;

                // === XZ plane (Y-facing surfaces like floor/ceiling) ===
                half2 uvXZ = worldPos.xz * matrixTiling;
                uvXZ.y += timeOffsetY + maskY * maskMultiplier;
                uvXZ.x += timeOffsetX;
                half4 matrixXZ = tex2D(matrixTex, uvXZ);
                float colIdXZ = floor(uvXZ.x * 7.0) + floor(worldPos.y * 3.0) * 100.0;
                half dropXZ = CalculateRaindrop(colIdXZ, uvXZ.y, _Time.y * speedY, _DropSpeed);
                matrixXZ *= dropXZ;

                // === YZ plane (X-facing surfaces) ===
                half2 uvYZ = worldPos.zy * matrixTiling;
                uvYZ.y += timeOffsetY + maskX * maskMultiplier;
                uvYZ.x += timeOffsetX;
                half4 matrixYZ = tex2D(matrixTex, uvYZ);
                float colIdYZ = floor(uvYZ.x * 7.0) + floor(worldPos.x * 3.0) * 100.0;
                half dropYZ = CalculateRaindrop(colIdYZ, uvYZ.y, _Time.y * speedY, _DropSpeed);
                matrixYZ *= dropYZ;

                // Blend based on normal direction (triplanar)
                return (matrixYZ * projNormal.x) + (matrixXZ * projNormal.y) + (matrixXY * projNormal.z);
            }

            vertexOutput vert(vertexInput v) {
                vertexOutput o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.worldViewDirection = normalize(UnityWorldSpaceViewDir(o.worldPos));

                o.projNormal = (pow(abs(o.worldNormal.xyz), _TriPlanarFalloff));
                o.projNormal /= (o.projNormal.x + o.projNormal.y + o.projNormal.z) + 0.00001;
                o.normalSign = sign(o.worldNormal.xyz);

                o.foggingRange = clamp(
                        ((distance(_WorldSpaceCameraPos, o.worldPos) - _FogStartDistance) / (
                            _FogEndDistance - _FogStartDistance)),
                        0.0,
                        1.0);
                o.foggingRange = fastPow(o.foggingRange, _FogExponent);

                return o;
            }

            fixed4 frag(vertexOutput i) : SV_Target {
                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                // === MATRIX DIGITAL RAIN EFFECT ===

                // Sample matrix texture with animated triplanar UV
                half4 matrixSample = SampleMatrixTriplanar(
                    _MatrixTex,
                    _MaskTex,
                    i.worldPos,
                    i.normalSign,
                    i.projNormal,
                    _MatrixTiling.xy,
                    _MaskTiling.xy,
                    _MatrixSpeed,
                    _MatrixSpeedX,
                    _MaskOffsetMultiplier
                );

                // Matrix character intensity (use alpha or luminance)
                half matrixIntensity = matrixSample.a;
                // Alternative: use luminance if texture has no alpha
                // half matrixIntensity = dot(matrixSample.rgb, half3(0.299, 0.587, 0.114));

                // === Oppy proximity effect - creates highlight near Oppy ===
                half distanceToOppy = distance(_OppyPosition, i.worldPos);
                half oppyProximity = saturate(1 - (distanceToOppy * 0.3));

                // Pulsing effect near Oppy
                half pulse = sin(_Time.w * 3 + distanceToOppy * 5) * 0.5 + 0.5;
                half oppyHighlight = oppyProximity * pulse * _OppyRippleStrength;

                // Base dark background
                half3 baseColor = half3(0.02, 0.02, 0.03);

                // Lighting (subtle)
                half halfLambert = dot(_WorldSpaceLightPos0.xyz, i.worldNormal) * 0.5 + 0.5;
                half4 lightingRamp = tex2D(_LightingRamp, halfLambert.xx);
                half4 finalLighting = (half4(_LightColor0.rgb, 0.0) * lightingRamp * 0.3) +
                                      half4(UNITY_LIGHTMODEL_AMBIENT.xyz * 0.2, 0);

                // Matrix emission color
                half3 matrixEmission = _MatrixColor.rgb * matrixIntensity * _MatrixEmission;

                // Add extra glow near Oppy
                matrixEmission += _MatrixColor.rgb * oppyHighlight * 2;

                // Combine base color with matrix emission
                half3 litColor = baseColor * finalLighting.rgb + matrixEmission;

                // Fogging (optional, can be reduced for stronger matrix effect)
                half4 foggingColor = texCUBE(_FogCubemap, i.worldViewDirection);
                half3 foggedColor = lerp(litColor, foggingColor.rgb * 0.5, (i.foggingRange * _FogStrength * 0.3));

                // Saturation control
                half desaturatedColor = dot(foggedColor, half3(0.299, 0.587, 0.114));
                half3 finalColor = lerp(desaturatedColor.xxx, foggedColor, _SaturationAmount);
                finalColor = fastPow(finalColor, 0.455);

                // Wall toggle effect (preserve original functionality)
                float radialDist = distance(i.worldPos, _EffectPosition) * 10;
                float dist = saturate(radialDist + 5 - _EffectTimer * 50);
                if (_EffectTimer >= 1.0) {
                    dist = 0;
                }
                float alpha = lerp(dist, 1 - dist, _InvertedMask);
                clip(alpha.r - 0.5);

                // Ball finder ripple effect - digital pulse style
                half distanceToBall = distance(_OppyPosition, i.worldPos);
                half digitalPulse = step(0.5, frac(distanceToBall * 15 - _Time.w * 2));
                half maskRipple = digitalPulse * saturate(1 - (distanceToBall * 0.5)) * 0.7;
                maskRipple *= saturate((distanceToBall - 0.2) * 5);

                return half4(finalColor, maskRipple * _MaskRippleStrength);
            }
            ENDCG
        }
    }
}
