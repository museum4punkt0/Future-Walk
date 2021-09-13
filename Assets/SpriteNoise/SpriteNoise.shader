Shader "Sprite/SimpleNoise"{
	Properties{
		_Color ("Tint", Color) = (1, 1, 1, 1)
		_Color2 ("NoiseColor", Color) = (0, 0, 0, 1)
		_Speed ("Animation Speed", Range (0.0, 1.0)) = .75
		_NoiseScale ("Noise Scale", Range (0.0, 1000.0)) = 100.0
		//_BoxRounding ("Box Rounding", Range (0.0, 1.0)) = .02
		_MainTex ("Texture", 2D) = "white" {}

		_StencilComp ("Stencil Comparison", Float) = 8
		_Stencil ("Stencil ID", Float) = 0
		_StencilOp ("Stencil Operation", Float) = 0
		_StencilWriteMask ("Stencil Write Mask", Float) = 255
		_StencilReadMask ("Stencil Read Mask", Float) = 255

		_ColorMask ("Color Mask", Float) = 15
	}

	SubShader{
		Tags{ 
			"RenderType"="Transparent" 
			"Queue"="Transparent"
		}

		Blend SrcAlpha OneMinusSrcAlpha

		ZWrite off
		Cull off

		Pass{
			Stencil{
                Ref 1
                Comp equal
            }

			CGPROGRAM

			#include "UnityCG.cginc"

			#pragma vertex vert
			#pragma fragment frag

			sampler2D _MainTex;
			float4 _MainTex_ST;

			fixed4 _Color, _Color2;

			float _Speed, _NoiseScale, _BoxRounding;

			struct appdata{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				fixed4 color : COLOR;
			};

			struct v2f{
				float4 position : SV_POSITION;
				float2 uv : TEXCOORD0;
				fixed4 color : COLOR;
			};

			v2f vert(appdata v){
				v2f o;
				o.position = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.color = v.color;
				return o;
			}

			/*

			float rand(float n){return frac(sin(n) * 43758.5453123);}

			float noise(float p){
				float fl = floor(p);
				float fc = frac(p);
				return lerp(rand(fl), rand(fl + 1.0), fc);
			}
	
			float noise(float2 n) {
				const float2 d = float2(0.0, 1.0);
				float2 b = floor(n);
				float2 f = smoothstep(float2(0.0, 0.0), float2(1.0, 1.0), frac(n));

				return lerp(lerp(rand(b), rand(b + d.yx), f.x), lerp(rand(b + d.xy), rand(b + d.yy), f.x), f.y);
			}


			float hash( float n )
					{
						return frac(sin(n)*43758.5453);
					}
	
			float noise( float3 x )
				{
					// The noise function returns a value in the range -1.0f -> 1.0f

					float3 p = floor(x);
					float3 f = frac(x);

					f       = f*f*(3.0-2.0*f);
					float n = p.x + p.y*57.0 + 113.0*p.z;

					return lerp(lerp(lerp( hash(n+0.0), hash(n+1.0),f.x),
									lerp( hash(n+57.0), hash(n+58.0),f.x),f.y),
								lerp(lerp( hash(n+113.0), hash(n+114.0),f.x),
									lerp( hash(n+170.0), hash(n+171.0),f.x),f.y),f.z);
			}
			*/
			float rand(float2 n) { 
				return frac(sin(dot(n, float2(12.9898, 4.1414))) * 43758.5453);
			}
	
			float noise(float2 p){
				float2 ip = floor(p);
				float2 u = frac(p);
				u = u*u*(3.0-2.0*u);
	
				float res = lerp(
					lerp(rand(ip),rand(ip+float2(1.0,0.0)),u.x),
					lerp(rand(ip+float2(0.0,1.0)),rand(ip+float2(1.0,1.0)),u.x),u.y);
				return res*res;
			}

			


			fixed4 frag(v2f i) : SV_TARGET{

				fixed4 col = tex2D(_MainTex, i.uv);


				float magicNumber =  .42;
				// per pixel randomness
				// visually overrides noise function to a big extent
				
				float2 uvAnim = i.uv + float2 (rand(i.uv) * _Time.x * _Speed, rand(i.uv * magicNumber)*_Time.x * _Speed);
				float3 rgb = lerp(_Color.rgb , _Color2.rgb, noise (  uvAnim * _NoiseScale ));
				
				col.rgb = rgb; //noise( i.uv * _NoiseScale);


				return col;
			}

			ENDCG
		}
	}
}


