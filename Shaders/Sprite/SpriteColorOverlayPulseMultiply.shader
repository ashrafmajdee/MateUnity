﻿//Only use for sprites
Shader "M8/Sprite/ColorOverlayPulseMultiply"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0
		[HideInInspector] _RendererColor("RendererColor", Color) = (1,1,1,1)
		[HideInInspector] _Flip("Flip", Vector) = (1,1,1,1)
		[PerRendererData] _AlphaTex("External Alpha", 2D) = "white" {}
		[PerRendererData] _EnableExternalAlpha("Enable External Alpha", Float) = 0

		[Header(Blending)]
    		[Enum(UnityEngine.Rendering.BlendMode)] _BlendSrc ("Source", Int) = 5
    		[Enum(UnityEngine.Rendering.BlendMode)] _BlendDst ("Destination", Int) = 10

		[Header(Overlay)]
		_ColorOverlayMin("Color Min", Color) = (0,0,0)
		_ColorOverlayMax("Color Max", Color) = (0,0,0)
		_PulseScale("Pulse Scale", Float) = 1
	}

		SubShader
		{
			Tags
			{
				"Queue" = "Transparent"
				"IgnoreProjector" = "True"
				"RenderType" = "Transparent"
				"PreviewType" = "Plane"
				"CanUseSpriteAtlas" = "True"
			}

			Cull Off
			Lighting Off
			ZWrite Off
			Blend [_BlendSrc] [_BlendDst]

			Pass
			{
			CGPROGRAM
				#pragma vertex SpriteVert_OverlayColorMult
				#pragma fragment SpriteFrag
				#pragma target 2.0
				#pragma multi_compile_instancing
				#pragma multi_compile_local _ PIXELSNAP_ON
				#pragma multi_compile _ ETC1_EXTERNAL_ALPHA
				#include "UnitySprites.cginc"

				fixed4 _ColorOverlayMin;
				fixed4 _ColorOverlayMax;
				float _PulseScale;

				v2f SpriteVert_OverlayColorMult(appdata_t IN)
				{
					v2f OUT;

					UNITY_SETUP_INSTANCE_ID(IN);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

					OUT.vertex = UnityFlipSprite(IN.vertex, _Flip);
					OUT.vertex = UnityObjectToClipPos(OUT.vertex);
					OUT.texcoord = IN.texcoord;
					OUT.color = IN.color * _Color * _RendererColor;

					#ifdef PIXELSNAP_ON
					OUT.vertex = UnityPixelSnap(OUT.vertex);
					#endif

					float sinT = sin(_Time.y * _PulseScale);
					float t = sinT * sinT;

					OUT.color *= lerp(_ColorOverlayMin, _ColorOverlayMax, t);

					return OUT;
				}
			ENDCG
			}
		}
}