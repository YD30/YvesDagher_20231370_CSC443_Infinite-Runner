// Minimal URP-compatible shader that bends world geometry away from the camera
// along Z (forward) using global parameters set by WorldCurve.cs.
// Property names (_BaseMap, _BaseColor) match URP/Lit so swapping the shader on
// existing materials preserves the texture/tint.
Shader "Custom/CurvedWorld"
{
    Properties
    {
        _BaseColor("Base Color", Color) = (1,1,1,1)
        _BaseMap("Base Map", 2D) = "white" {}
        _AmbientColor("Ambient Color", Color) = (0.32, 0.34, 0.40, 1)
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
            "RenderPipeline"="UniversalPipeline"
            "Queue"="Geometry"
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4  _BaseColor;
                half4  _AmbientColor;
            CBUFFER_END

            // Set globally by WorldCurve.cs via Shader.SetGlobalFloat.
            float _CurveY;
            float _CurveX;
            float _CurveFreq;

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS   : TEXCOORD0;
                float2 uv         : TEXCOORD1;
            };

            // World-space vertex bend. Quadratic Y falloff so the curve is invisible
            // close to the camera and dramatic in the distance, plus an optional
            // sinusoidal X sway for sweeping curves.
            float3 ApplyCurve(float3 worldPos)
            {
                float z = worldPos.z - _WorldSpaceCameraPos.z;
                worldPos.y -= _CurveY * z * z;
                worldPos.x += _CurveX * sin(z * _CurveFreq);
                return worldPos;
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                float3 positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                positionWS = ApplyCurve(positionWS);

                OUT.positionCS = TransformWorldToHClip(positionWS);
                OUT.normalWS   = TransformObjectToWorldNormal(IN.normalOS);
                OUT.uv         = TRANSFORM_TEX(IN.uv, _BaseMap);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 baseSample = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                half3 albedo     = baseSample.rgb * _BaseColor.rgb;

                Light mainLight = GetMainLight();
                float3 normalWS = normalize(IN.normalWS);
                float ndl = saturate(dot(normalWS, mainLight.direction));

                half3 lighting = mainLight.color.rgb * ndl + _AmbientColor.rgb;
                return half4(albedo * lighting, baseSample.a * _BaseColor.a);
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/InternalErrorShader"
}
