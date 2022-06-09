Shader "VoidPointer/Particle/ParticleMaster"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        [HDR]_MainColor("Main Color", COLOR) = (1,1,1,1)
        _UseVertexColor("Enable vertex colors", FLOAT) = 1
        [Toggle(_POLAR_COORDINATES)]_UsePolarCoordinates("Use Polar Coordinates", Float) = 0

        [Toggle(_SECONDARY_TEXTURE)]_UseSecondaryTexture("Use Secondary Texture", Float) = 0
        _SecondaryTexture("Secondary Texture", 2D) = "white" {}
        [Enum(Copy,0,Multiply,1,Add,2,Subtract,3)]_BlendMethod("Blend Method", Int) = 0
        _BlendingFactor("Blend", Range(0,1)) = 0.5

        _RedIsAlpha("R Channel Alpha", FLOAT) = 0
        _AlphaThreshold("Alpha Threshold", Range(0,1)) = 0
        _AlphaSmoothness("Alpha Smoothness", Range(0.01,1)) = 0.01
        [Toggle(_CIRCLE_MASK)]_CircleMask("Circle Mask", Float) = 1
        _InnerMask("Inner Mask", Range(0,1)) = 0
        _OuterMask("Outer Mask", Range(0,1)) = 0
        _CircleMaskSmoothness("Circle Mask Smoothness", Range(0,1)) = 1

        [Toggle(_BILLBOARD)]_IsBillboard("Is Billboard", Float) = 0
        [Enum(UnityEngine.Rendering.CullMode)] _Culling ("Cull Mode", Int) = 2
    }

    HLSLINCLUDE
    
        //Add library here
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl" //utils
        //#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl" //for unity lights

        //add non static properties in the command buffer to make the shader SRP Batchable
        CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            half4 _MainColor;
            half _UseVertexColor;

            //#ifdef _SECONDARY_TEXTURE
                float4 _SecondaryTexture_ST;
                int _BlendMethod;
                half _BlendingFactor;
            //#endif

            half _RedIsAlpha;
            half _AlphaThreshold;
            half _AlphaSmoothness;
            half _InnerMask;
            half _OuterMask;
            half _CircleMaskSmoothness;

            int _Culling;
        CBUFFER_END

    ENDHLSL

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderQueue" = "Transparent" "Queue" = "Transparent" }

        Pass
        {
            Name "Surface"
            Tags { "Lightmode" = "UniversalForward" }
            Cull [_Culling]
            //Blend One One
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM

                #pragma vertex vertSurface
                #pragma fragment fragSurface

                //Include some lights calculations
                // #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
                // #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
                // #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
                // #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
                // #pragma multi_compile _ _SHADOWS_SOFT

                #pragma shader_feature _SECONDARY_TEXTURE
                #pragma shader_feature _POLAR_COORDINATES
                #pragma shader_feature _CIRCLE_MASK
                #pragma shader_feature _BILLBOARD

                sampler2D _MainTex;
                #ifdef _SECONDARY_TEXTURE
                    sampler2D _SecondaryTexture;
                #endif

                struct Attributes //vertexInput
                {
                    float4 posOS : POSITION;
                    float2 uv : TEXCOORD0;
                    //float3 normalOS : NORMAL; //toggle if u need normals
                };

                struct Varyings //vertexOutput
                {
                    float2 uv : TEXCOORD0;
                    float4 posCS : SV_POSITION;
                    float4 screenPos : TEXCOORD1;
                    float4 posWS : TEXCOORD2;
                    //float3 normalWS : NORMAL; //toggle if u need normals
                };

                Varyings vertSurface(Attributes input)
                {
                    Varyings output;
					
					// VertexPositionInputs contains position in multiple spaces (world, view, homogeneous clip space)
					// The compiler will strip all unused references (say you don't use view space).
					// Therefore there is more flexibility at no additional cost with this struct.
                    VertexPositionInputs vertexInput = GetVertexPositionInputs(input.posOS.xyz);
					
					// Similar to VertexPositionInputs, VertexNormalInputs will contain normal, tangent and bitangent
					// in world space. If not used it will be stripped.
                    //VertexNormalInputs vertexNormalInput = GetVertexNormalInputs(input.normalOS); //toggle if u need normals.
                    #ifdef _BILLBOARD
                        float3 quadPivotPosOS = float3(0,0,0);
                        float3 quadPivotPosWS = TransformObjectToWorld(quadPivotPosOS);
                        float3 quadPivotPosVS = TransformWorldToView(quadPivotPosWS);

                        float2 scaleXY_WS = float2(
                            length(float3(GetObjectToWorldMatrix()[0].x, GetObjectToWorldMatrix()[1].x, GetObjectToWorldMatrix()[2].x)), // scale x axis
                            length(float3(GetObjectToWorldMatrix()[0].y, GetObjectToWorldMatrix()[1].y, GetObjectToWorldMatrix()[2].y)) // scale y axis
                            );

                        float3 posVS = quadPivotPosVS + float3(input.posOS.xy * scaleXY_WS,0);//recontruct quad 4 points in view space
                        vertexInput.positionCS = mul(GetViewToHClipMatrix(),float4(posVS,1));
                    #endif

                    output.posCS = vertexInput.positionCS;
                    output.posWS = float4(vertexInput.positionWS, 0); // --> posWS 

                    output.screenPos = ComputeScreenPos(output.posCS);
                    //output.normalWS = vertexNormalInput.normalWS;
                    output.uv = input.uv;
                    //output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                    return output;
                }

                float2 PolarCoordinates(float2 cartesian)
                {
                    float dist = length(cartesian);
                    float angle = atan2(cartesian.y, cartesian.x);
                    return float2(angle / 6.2831, dist);
                }

                half4 fragSurface(Varyings i) : SV_Target
                {
                    float2 scrPos = i.screenPos.xy / i.screenPos.w;

                    //switch to screenPos if needed
                    float2 uvs = 0;
                    #ifdef _POLAR_COORDINATES
                        uvs = PolarCoordinates(i.uv*2-1);
                    #else
                        uvs = i.uv;
                    #endif

                    half4 col = tex2D(_MainTex, uvs * _MainTex_ST.xy + (_MainTex_ST.zw * _Time.x));
                    #ifdef _SECONDARY_TEXTURE
                        half4 col2 = tex2D(_SecondaryTexture, uvs * _SecondaryTexture_ST.xy + (_SecondaryTexture_ST.zw*_Time.x));
                        switch(_BlendMethod)
                        {
                            case 0: //copy
                                col = lerp(col, col2, _BlendingFactor);
                                break;
                            case 1: //multiply
                                col = col * (col2*_BlendingFactor);
                                break;
                            case 2: //add
                                col = saturate((col*(1-_BlendingFactor)) + (col2*_BlendingFactor));
                                break;
                            case 3: //subtract
                                col = saturate((col*(1-_BlendingFactor)) - (col2*_BlendingFactor));
                                break;
                        }
                    #endif

                    col *= _MainColor;

                    half alpha = _RedIsAlpha == 0 ? col.a : col.r;
                    #ifdef _CIRCLE_MASK
                        half outerMask = smoothstep(0+(1-_OuterMask),1,saturate(1-length(i.uv*2-1)));//pow(saturate(1-length(i.uv*2-1)),_OuterMask);
                        half innerMask = smoothstep(0+(1-_InnerMask),1,outerMask);
                        half mask = saturate((outerMask-innerMask)/_CircleMaskSmoothness);
                        alpha *= mask;
                    #endif
                    alpha = smoothstep(_AlphaThreshold, saturate(_AlphaThreshold+_AlphaSmoothness), alpha);
                    
                    // sample the texture
                    return half4(col.rgb,alpha);
                }

            ENDHLSL
        }
        //add other passes if needed (CAUTION: Multiple passes shaders can't be batched!)
    }
    CustomEditor "ParticleMasterShaderEditor"
}
