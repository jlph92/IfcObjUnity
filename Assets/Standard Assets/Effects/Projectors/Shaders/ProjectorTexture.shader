// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Projector' with 'unity_Projector'
// Upgrade NOTE: replaced '_ProjectorClip' with 'unity_ProjectorClip'

Shader "Projector/Texture" {
	Properties {
		_ShadowTex ("Cookie", 2D) = "gray" {}
		_ProjectorDir("_ProjectorDir", Vector) = (0,0,0,0)
	}

	Subshader {
		Tags {"Queue"="Transparent"}
		Pass {
			ZWrite Off
			ColorMask RGB
			Blend DstColor Zero
			Offset -1, -1

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			#include "UnityCG.cginc"
			
			struct v2f {
				float4 uvShadow : TEXCOORD0;
				float4 uvFalloff : TEXCOORD1;
				UNITY_FOG_COORDS(2)
				float4 pos : SV_POSITION;
			};

			struct VertexInput {
				float4 vertex : POSITION;
				fixed3 normal : NORMAL;
			};
			
			float4x4 unity_Projector;
			float4x4 unity_ProjectorClip;
			fixed4 _ProjectorDir;
			
			v2f vert(VertexInput v)
			{
				v2f o;
				//_ProjectorDir = unity_ObjectToWorld._m30_m31_m32_m33;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uvShadow = mul(unity_Projector, v.vertex);
				o.uvFalloff = mul(unity_ProjectorClip, v.vertex);
				UNITY_TRANSFER_FOG(o, o.pos);
				o.uvFalloff.a = clamp(-1 * dot(normalize(_ProjectorDir), v.normal), 0, 1);

				return o;
			}
			
			sampler2D _ShadowTex;
			sampler2D _FalloffTex;

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 texS = tex2Dproj(_ShadowTex, UNITY_PROJ_COORD(i.uvShadow));
				texS.a = 1.0 - texS.a;

				fixed4 texF = tex2Dproj(_FalloffTex, UNITY_PROJ_COORD(i.uvFalloff));
				fixed4 res = lerp(fixed4(1, 1, 1, 0), texS, i.uvFalloff.a)

				UNITY_APPLY_FOG_COLOR(i.fogCoord, res, fixed4(1, 1, 1, 1));
				return res;
			}
			ENDCG
		}
	}
}
