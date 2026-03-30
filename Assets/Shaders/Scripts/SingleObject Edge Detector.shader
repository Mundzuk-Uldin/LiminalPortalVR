Shader "Custom/SingleObjectEdgeDetector"
{
    Properties
    {
        _OutlineColor("Outline Color", Color) = (0, 0, 0, 1)
        _OutlineThickness("Outline Thickness", Range(0, 0.1)) = 0.02
    }

        SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

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
                float4 positionHCS : SV_POSITION;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                return half4(1, 1, 1, 1);
            }
            ENDHLSL
        }

        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "SRPDefaultUnlit" }
            Cull Front

            HLSLPROGRAM
            #pragma vertex vertOutline
            #pragma fragment fragOutline
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float  _OutlineThickness;
                float4 _OutlineColor;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
            };

            // --- Smooth normal helpers ---

            // How many vertices Unity's cube has
            #define MAX_VERTS 24

            // Stores all positions/normals so we can average per shared position
            static float3 positions[MAX_VERTS];
            static float3 normals[MAX_VERTS];
            static int    vertCount = 0;

            // Returns the average normal of all vertices that share the same object-space position
            float3 GetSmoothedNormal(float3 posOS, float3 normalOS)
            {
                float3 smoothed = float3(0, 0, 0);
                int    count = 0;

                for (int i = 0; i < MAX_VERTS; i++)
                {
                    // Compare positions with a small epsilon
                    if (length(positions[i] - posOS) < 0.001)
                    {
                        smoothed += normals[i];
                        count++;
                    }
                }

                return count > 0 ? normalize(smoothed) : normalOS;
            }

            Varyings vertOutline(Attributes IN)
            {
                Varyings OUT;

                float3 smoothNormal = GetSmoothedNormal(IN.positionOS.xyz, IN.normalOS);

                float4 clipPos = TransformObjectToHClip(IN.positionOS.xyz);
                float3 clipNormal = mul((float3x3)UNITY_MATRIX_VP,
                                       mul((float3x3)UNITY_MATRIX_M, smoothNormal));

                float2 offset = normalize(clipNormal.xy)
                                * _OutlineThickness
                                * clipPos.w;

                clipPos.xy += offset;
                OUT.positionHCS = clipPos;
                return OUT;
            }

            half4 fragOutline(Varyings IN) : SV_Target
            {
                return _OutlineColor;
            }
            ENDHLSL
        }
    }
}
