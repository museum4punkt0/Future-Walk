// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "NEEEU/GrabPass"
{
    Properties
    {
    	 _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
    	_MatCapMap ("MatCapMap", 2D) = "bump" {}
    	//_RefractionMap ("RefractionMap)", 2D) = "bump" {}

        _PhaseOffset ("PhaseOffset", Range(0,1)) = 0
        _Speed ("Speed", Range(0.1,10)) = 1
        _Depth ("Depth", Range(0.01,1)) = 0.2
        _Scale ("Scale", Range(0.1,20)) = 10


       _Fresnel ("Fresnel", Range(0.0,1)) = 0.2

       _RefractionStrength("Refraction Strength",Range(0.0,10)) = 2.0
       	
         _Ramp ("Shading Ramp", 2D) = "gray" {}
        _Attenuation ("Attenuation", Range(0.1,1)) = .4
	  	_SpecExpon ("Spec Power", Range (0, 125)) = 12
	  	_FilmDepth ("Film Depth", Range (0, 1)) = 0.05

		_Alpha ("Alpha", Range (0, 1)) = 1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
       
        GrabPass { }

        //Cull Off
       	Blend SrcAlpha OneMinusSrcAlpha
        //ZWrite On

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest
 
            #include "UnityCG.cginc"
            #include "UnityStandardBRDF.cginc"
 
            struct appdata
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                float3 normal:TEXCOORD1;
            };
 
            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 texcoord : TEXCOORD0;
                float4 grabUV : TEXCOORD1;
                float3 worldPos:TEXCOORD2;
                float3 normal:TEXCOORD3;
                float3 TtoV0 : TEXCOORD4;
				float3 TtoV1 : TEXCOORD5;
                float3 viewDir:TEXCOORD6;
            
            };

           

			// VERTEX NOISE
            float _PhaseOffset;
		    float _Speed;
		    float _Depth;
		    float _Scale;
		    float _Fresnel;

		    sampler2D _MainTex;
           	float4 _MainTex_ST;
            sampler2D _GrabTexture;
           // sampler2D _RefractionMap;
            sampler2D _MatCapMap;
            float4 _GrabTexture_TexelSize;
           	float _RefractionStrength;

            // THINFILM
          	sampler2D _Ramp;
			float _SurfColor;
		    float _SpecExpon;
			float _FilmDepth;
			float _Attenuation;

			float _Alpha;


            v2f vert (appdata_full v)
            {
                v2f o;

                TANGENT_SPACE_ROTATION;
                o.TtoV0 = mul(rotation, UNITY_MATRIX_IT_MV[0].xyz);
				o.TtoV1 = mul(rotation, UNITY_MATRIX_IT_MV[1].xyz);

		      	
              	// Obtain tangent space rotation matrix
		        //float3 binormal = cross( v.normal, v.tangent.xyz ) * v.tangent.w;
		        //float3x3 rotation = transpose( float3x3( v.tangent.xyz, binormal, v.normal ) );
		       
		        // Create two sample vectors (small +x/+y deflections from +z), put them in tangent space, normalize them, and halve the result.
		        // This is equivalent to sampling neighboring vertex data since we're on a unit sphere.
		       	float3 v1 = normalize( mul( rotation, float3(0.1, 0, 1) ) ) * 0.5;
		        float3 v2 = normalize( mul( rotation, float3(0, 0.1, 1) ) ) * 0.5;
		       
		        // Some animation values
		        float phase = _PhaseOffset * 3.14 * 2;
		        float speed = _Time.y * _Speed;

		        // Modify the real vertex and two theoretical samples by the distortion algorithm (here a simple sine wave on XZY
		        v.vertex.x += sin( phase + speed + (v.vertex.z * _Scale) ) * _Depth;
		        v.vertex.y += sin( phase + speed + (v.vertex.x * _Scale) ) * _Depth;
		        v.vertex.z += sin( phase + speed + (v.vertex.y * _Scale) ) * _Depth;

		        v1.x += sin( phase + speed + (v1.z * _Scale) ) * _Depth;
		        v1.y += sin( phase + speed + (v1.x * _Scale) ) * _Depth;
		        v1.z += sin( phase + speed + (v1.y * _Scale) ) * _Depth;

		        v2.x += sin( phase + speed + (v2.z * _Scale) ) * _Depth;
		        v2.y += sin( phase + speed + (v2.x * _Scale) ) * _Depth;
		        v2.z += sin( phase + speed + (v2.y * _Scale) ) * _Depth;

		        o.normal = v.normal;
		       // o.normal = mul(unity_ObjectToWorld, v.normal);


		        // Take the cross product of the sample-original positions, resulting in a dynamic normal
		        float3 vn = cross( v2-v.vertex.xyz , v1-v.vertex.xyz );
		       
		        // Normalize
		       	vn = normalize( vn );
		       
               
         		o.texcoord.xy = TRANSFORM_TEX(v.texcoord.xy, _MainTex);

		

		        o.viewDir = ObjSpaceViewDir ( v.vertex );
          
		        // Convert vertex position to world space
		        o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
					
		       
		       	

		       	o.pos = UnityObjectToClipPos(v.vertex);
		       	o.grabUV = ComputeGrabScreenPos(o.pos);
                return o;
            }
           
            fixed4 frag (v2f i) : SV_Target
            {

            	// FRESNEL - - - - - - - - - - - -
		      
		       	half rim = saturate(dot (normalize(i.viewDir), i.normal));
		       	rim  = clamp(pow(rim,_Fresnel * _Alpha), 0, 1);

		    	// REFRACTION - - - - - - - - - - - -
				// normal mapping
				half2 bump = i.normal.xy * _Alpha;// UnpackNormal(tex2D(_RefractionMap, N.xy)).rg;

				// calculate the offset for the refraction
				float2 offset = bump * _GrabTexture_TexelSize.xy * _RefractionStrength * _RefractionStrength;
				offset = clamp(offset, float2(0, 0), float2(1, 1));

				// calculate uvs with the new offset
				half4 uv2 = half4(i.grabUV.x + offset.x, i.grabUV.y + offset.y, i.grabUV.z, i.grabUV.w);

		    	//half fresnel = pow(1.0 - cosTheta, _Fresnel);

                fixed4 grabCol = tex2Dproj(_GrabTexture, uv2); //UNITY_PROJ_COORD(uv2));


                // THIN FILM - - - - - - - - - - - - - - - - 

                float3 N = i.normal;
		    	// Light vector from mesh's surface
		        float3 L = normalize(_WorldSpaceLightPos0.xyz);
		        // Viewport(camera) vector from mesh's surface
		       	float3 V = normalize(_WorldSpaceCameraPos - i.worldPos);
		      
                half3 Hn = normalize (L + V);

				half ndl = dot (N, L);
				half ndh = dot (N, Hn);
				half ndv = dot (N, V);

				float3 diff = max(0,ndl).xxx;
				float nh = max (0, ndh);
				float3 spec = pow (nh, _SpecExpon).xxx;
				float viewdepth = _FilmDepth/ndv;
				half3 ramp = tex2D (_Ramp, viewdepth.xx).rgb;

				//half3 thinFilm = (col* _SurfColor * diff + ramp * spec) * (_Attenuation * 2);
				half3 thinFilm = ramp * spec * _Attenuation * 2;


				half2 vn;
				vn.x = dot(normalize( i.TtoV0), float3(0,0,1));
				vn.y = dot(normalize( i.TtoV1), float3(0,0,1));
				
				float4 matCap = tex2D(_MatCapMap, vn*0.5 + 0.5);
				//matCap.a = _Alpha;

				// MIXING - - - - -  - - - 
				fixed4 col = lerp(grabCol, matCap,1-rim);
				col.rgb += thinFilm * _Alpha;
                col.a = _Alpha;

                //col.a = 1-rim;
               	//col.rgb = rim;
                return col;
            }
            ENDCG
        }
    }
}