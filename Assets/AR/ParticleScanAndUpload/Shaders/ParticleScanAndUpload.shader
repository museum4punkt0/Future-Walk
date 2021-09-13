

Shader "NEEEU/ParticleScanAndUpload" {
Properties {
    _Color ("Tint", Color) = (1, 1, 1, 1)
    _MainTex ("Texture", 2D) = "white" {}
    _EmissiveColor ("Emissive Tint", Color) = (0, 0, 0, 1)
    _EmissionTex ("Emission Texture", 2D) = "black" {}

    _Metallic ("Metallic", Range(0,1)) = .5
    _Smoothness ("Smoothness", Range(0,1)) = .5

    _WipeProgress ("Wipe Progress", Range(0,1)) = 0
    _WipeRandomOffset ("Wipe Random Offset", Range(0,1)) = .1
    _WipeNoiseScale ("Wipe Noise Scale", Range(0,1000)) = 400

    _FadeProgress ("Fade Progress", Range(0,1)) = 0

    _DisplacementTex ("Displacement Texture", 2D) = "black" {}

    _ExtrusionAmount ("Extrusion Amount", Range(-100,100)) = 0.5

    _NoiseScale ("Noise Scale", Range(0,1000)) = 400
    _NoiseAmount ("Noise Amount", Range(0,1)) = .5

    _Ring1Radius ("Ring 1 Radius", Range(0,2)) = 0
    _Ring2Radius ("Ring 2 Radius", Range(0,2)) = 0
    _Ring3Radius ("Ring 3 Radius", Range(0,2)) = 0

    _RingThickness ("Ring Thickness", Range(0,2)) = .5

    _RotationAngle ("Rotation Angle", Range(0,2)) = .5

    _NormalDistortion ("Normal Distortion", Range(0,3)) = .5
    
}
SubShader {
    Tags { "Queue" = "Transparent" "RenderType"="Transparent" }

    Cull Back
    ZWrite Off
    Blend SrcAlpha OneMinusSrcAlpha

    CGPROGRAM


    #pragma surface surf Standard vertex:vert alpha:fade 

    #pragma target 4.0

    float _Metallic, _Smoothness;
    float _ExtrusionAmount, _NoiseScale, _NoiseAmount, _WipeProgress, _WipeRandomOffset, _WipeNoiseScale, _FadeProgress;
    


    // - - - - - - - - - - NOISE - - - - - - - - - - -  //

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

    // - - - - - - - - - - NOISE END - - - - - - - - - - -  //

    // Rotation with angle (in radians) and axis

    float3x3 AngleAxis3x3(float angle, float3 axis)
    {
        float c, s;
        sincos(angle, s, c);

        float t = 1 - c;
        float x = axis.x;
        float y = axis.y;
        float z = axis.z;

        return float3x3(
            t * x * x + c,      t * x * y - s * z,  t * x * z + s * y,
            t * x * y + s * z,  t * y * y + c,      t * y * z - s * x,
            t * x * z - s * y,  t * y * z + s * x,  t * z * z + c
        );
    }

    // - - - - - - - - - - RING - - - - - - - - - - -  //



    float ring(float2 uv, float radius, float thickness)
    {

        float circularGradient = distance (float2(.5, .5), uv);

        float ringStart = radius - thickness;
        
        float ringEnd = radius ;

        float ringUP = smoothstep(ringStart, radius- thickness / 2, circularGradient );
        float ringDown= smoothstep(radius- thickness / 2, ringEnd, circularGradient );

        return ringUP - ringDown;


    }

    // - - - - - - - - - - WIPE - - - - - - - - - - -  //


    float wipe (float2 uv, float progress )
    {
        
        float gradientY = uv.y;

        float smooth = smoothstep (0, 1, gradientY);

        float noisy = smooth + noise (uv * _WipeNoiseScale) * _WipeRandomOffset;

        float remapProgress = lerp(-1.2, 1, progress);
       
        float intensity = clamp (remapProgress + noisy , 0, 1);

        return intensity;
        
    }

    // - - - - - - - - - - surface shader SETUP - - - - - - - - - - -  //

    struct Input {
        float2 uv_MainTex;    
        float2 uv2_Tex;
        float4 color;
        float3 normal;
       
    };

  
    sampler2D _DisplacementTex;
    float _Ring1Radius, _Ring2Radius, _Ring3Radius;
    float _RingThickness;
    float _RotationAngle;
    float _NormalDistortion;


    void vert (inout appdata_full v, out Input o) {

        UNITY_INITIALIZE_OUTPUT(Input,o);        

        float2 uv = v.texcoord.xy;

        // expects collapsed coordinates per quad
        float2 uv1 = v.texcoord1.xy;

        // different sets of UVs generate different effects when adding noise
        // connected quads - noise
        float noiseXY = noise (uv * _NoiseScale);
        // detached quads - noise
        float noise1XY = noise (uv1 * _NoiseScale);


        float noise3D = noise (v.vertex.xy);

        float4 color = float4(1, 1, 1, 1) * noiseXY;
    

        // Convert to world space
        float3 pullPos = mul(unity_ObjectToWorld,v.vertex);
        // Determine cam direction (needs Normalize)
        float3 camDirection=normalize(_WorldSpaceCameraPos - pullPos);
        // Pull in the direction of the camera by a fixed amount
        pullPos += camDirection * _WipeProgress * 3;
        // Convert to clip space
        float3 camPullPos = mul(unity_WorldToObject, pullPos);
    
        float ringsE = ring (uv1, _Ring1Radius, _RingThickness) + ring (uv1, _Ring2Radius, _RingThickness) + ring (uv1, _Ring3Radius, _RingThickness);
        float noiseE = (noiseXY * .1 + noise1XY) * _NoiseAmount;
        float wipeE = wipe (uv1, _WipeProgress) * camPullPos;

        v.vertex.xyz += v.normal * _ExtrusionAmount * (ringsE + noiseE) + wipeE;
                  


        o.color = color;

        //TODO: FIX make dependent on normal
        o.color.a = clamp(ringsE + noiseE, 0, 1);

       v.normal =  v.normal.xyz + noise1XY * _NormalDistortion;
       o.normal = v.normal;

    }

    sampler2D _MainTex;
    sampler2D _EmissionTex;
    fixed4 _Color, _EmissiveColor;


    void surf (Input IN, inout SurfaceOutputStandard o) {
        o.Normal = IN.normal;

        o.Albedo = tex2D (_MainTex, IN.uv_MainTex).rgb * _Color.rgb;
        o.Emission = tex2D (_EmissionTex, IN.uv_MainTex).rgb * _EmissiveColor.rgb;
       // o.Metallic = _Metallic;
       // o.Smoothness = _Smoothness;

        float width = .1;
        //o.Alpha = !;

        o.Alpha = (1 - smoothstep(_FadeProgress, _FadeProgress+width, IN.uv_MainTex.y)) * IN.color.a;    
    }
    ENDCG
} 
Fallback "Diffuse"
}