Shader "Custom/CurvedWorldTest"
{
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
            "RenderPipeline"="UniversalPipeline"
        }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            float _CurveY;

            float3 ApplyCurve(float3 worldPos)
            {
                float z = worldPos.z - _WorldSpaceCameraPos.z;
                worldPos.y -= _CurveY * z * z;
                return worldPos;
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                float positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                positionWS = ApplyCurve(positionWS);
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                return half4(1, 1, 1, 1);
            }
            ENDHLSL
        }
    }
}