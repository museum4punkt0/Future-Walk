// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "NEEEU/GlowTransparent"
{
    Properties
    {
    	_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
        _Rim1Color ("Rim Tint", Color) = (1, 1, 1, 1)
        _Rim2Color ("Rim 2 Tint", Color) = (1, 1, 1, 1)

    	//_MatCapMap ("MatCapMap", 2D) = "bump" {}
        _NormalMap("NormalMap", 2D) = "bump" {}
    	
       _Fresnel1 ("Fresnel1", Range(0.0,2)) = 0.2
       _Fresnel2 ("Fresnel2", Range(0.0,2)) = 0.2

       _RefractionStrength("Refraction Strength",Range(0.0,10)) = 2.0

       _Light1XYZW ("Light1XYZW", Vector) = (0, 0, 0, 1)
       _Light2XYZW ("Light2XYZW", Vector) = (0, 0, 0, 1)
        
     
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }

        //Tags {"Queue"="AlphaTest" "IgnoreProjector"="True" "RenderType"="TransparentCutout"}

        GrabPass { }

        //Cull Off
       	Blend SrcAlpha OneMinusSrcAlpha
        ZWrite On

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest
 
            #include "UnityCG.cginc"
            #include "UnityStandardBRDF.cginc"
 
      
 
            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 texcoord : TEXCOORD0;
                float4 grabUV : TEXCOORD1;
                float3 worldPos:TEXCOORD2;
                float3 normal:TEXCOORD3;
   
            };

           

		    sampler2D _MainTex;
           	float4 _MainTex_ST;

            sampler2D _GrabTexture;
        
            sampler2D _NormalMap;

            float4 _GrabTexture_TexelSize;

           	float _RefractionStrength;

        
            float _Fresnel1, _Fresnel2;
            float4 _Rim1Color, _Rim2Color;
		

            float4 _Light1XYZW;
		    float4 _Light2XYZW;


            v2f vert (appdata_full v)
            {
                v2f o;


	            o.normal = v.normal;

                o.texcoord.xy = TRANSFORM_TEX(v.texcoord.xy, _MainTex);

                o.pos = UnityObjectToClipPos(v.vertex);

                o.worldPos = mul(unity_ObjectToWorld, v.vertex);

               


	            o.grabUV = ComputeGrabScreenPos(o.pos);

                return o;
            }
           
            fixed4 frag (v2f i) : SV_Target
            {

            	
                half2 uv = i.texcoord.xy;
	            // REFRACTION - - - - - - - - - - - -
	            // use normal provided by normal map and ignore the actual one, as we always facing the camera

                half2 normal = UnpackNormal(tex2D(_NormalMap, uv)).rg;

                half3 dir = _Light1XYZW.xyz - i.worldPos.xyz;
                half3 dir2 = _Light2XYZW.xyz - i.worldPos.xyz;

                // FRESNEL - - - - - - - - - - - -
	            half rim1 = saturate(dot (normalize(dir), normal));
	            rim1  = clamp(pow(rim1,_Fresnel1), 0, 1);

                half rim2 = saturate(dot (normalize(dir2), normal));
	            rim2  = clamp(pow(rim2,_Fresnel2), 0, 1);

	            // calculate the offset for the refraction
	            float2 offset = normal * _GrabTexture_TexelSize.xy * _RefractionStrength * _RefractionStrength;

	            /* calculate uvs with the new offset */

                half4 uv2 = half4(i.grabUV.x + offset.x, i.grabUV.y + offset.y, i.grabUV.z, i.grabUV.w);

	            //half fresnel = pow(1.0 - cosTheta, _Fresnel);

                fixed4 grabCol = tex2Dproj(_GrabTexture, uv2);

	            //float4 matCap = tex2D(_MatCapMap,normal *0.5 + 0.5);
                

	            // MIXING - - - - -  - - -
     

	            fixed4 col =  grabCol;

                // gradient from inside towards corners

                col.a =  1- smoothstep (.4, .5, distance (half2(.5, .5), uv));

                //col.rgb += matCap.rgb * matCap.a;

                col.rgb += rim1 * _Rim1Color.rgb * _Rim1Color.a;
                col.rgb += rim2 * _Rim2Color.rgb * _Rim2Color.a;

                return col;
            }

        ENDCG
        }
    }
}