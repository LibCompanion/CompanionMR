/*
 * CompanionMR is a Windows Mixed Reality example project for Companion.
 * Copyright (C) 2018 Dimitri Kotlovky, Andreas Sekulski
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

Shader "CompanionMR/UIOverlay" {

    Properties {
        [PerRendererData] _MainTex ("Texture", 2D) = "white" {}
        [HideInInspector] _Color("Color", Color) = (1,1,1,1)
        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
    }
    
    SubShader {

        LOD 100

        Tags {
            "Queue" = "Overlay+1"
            "IgnoreProjector" = "True"
            "RenderType" = "Overlay"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Stencil {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp] 
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }
        
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest Always
        Offset -1, -1
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass {

            CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma multi_compile_instancing
                #include "UnityCG.cginc"

                struct vertInput {
                    float4 pos : POSITION;
                    float2 texcoord : TEXCOORD0;
                    float4 color : COLOR;
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };
    
                struct vertOutput {
                    float4 pos : SV_POSITION;
                    half2 texcoord : TEXCOORD0;
                    fixed4 color : COLOR;
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                    UNITY_VERTEX_OUTPUT_STEREO
                };

                sampler2D _MainTex;
                float4 _MainTex_ST;
                fixed4 _Color;
                fixed4 _TextureSampleAdd; // Font color support

                vertOutput vert (vertInput input) {
                    vertOutput o;
                    UNITY_SETUP_INSTANCE_ID(input);
                    UNITY_TRANSFER_INSTANCE_ID(input, o);
                    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                    o.pos = UnityObjectToClipPos(input.pos);
                    o.texcoord = TRANSFORM_TEX(input.texcoord, _MainTex);
                    o.color = input.color * _Color;
#ifdef UNITY_HALF_TEXEL_OFFSET
                    o.pos.xy += (_ScreenParams.zw-1.0) * float2(-1,1); // Based on the Unity UI overlay shader approach from the VR samples: https://assetstore.unity.com/packages/essentials/tutorial-projects/vr-samples-51519
#endif

                    return o;
                }

                fixed4 frag (vertOutput output) : SV_Target {
                    UNITY_SETUP_INSTANCE_ID(output);
                    fixed4 col = (tex2D(_MainTex, output.texcoord) + _TextureSampleAdd) * output.color;
                    clip(col.a - 0.01);
                    return col;
                }

            ENDCG
        }
    }
}
