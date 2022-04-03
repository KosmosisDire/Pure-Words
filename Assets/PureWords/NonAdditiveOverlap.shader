// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

 Shader "Custom/NonAdditiveOverlap" {
	 Properties {
	     [PerRendererData] _MainTex ("Base (RGB)", 2D) = "white" {}
         _Color ("Tint", Color) = (1,1,1,1)
	 }
	 
	 SubShader {
	    Tags
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}

	     
		Pass {
		    Stencil {
		        Ref 2
		        Comp NotEqual
		        Pass Replace
		    }

            Blend SrcAlpha OneMinusSrcAlpha     
            Cull Off
		    Lighting Off
		    ZWrite Off
            ZTest NotEqual
    
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            uniform sampler2D _MainTex;
            fixed4 _Color;



            struct appdata_t
			{
				half4 vertex   : POSITION;
				float4 color    : COLOR;
				half2 texcoord : TEXCOORD0;
			};
            
            struct v2f {
                half4 pos : POSITION;
                half2 uv : TEXCOORD0;
                fixed4 color    : COLOR;
            };
            
            v2f vert(appdata_t  v) {
                v2f o;
                o.pos = UnityObjectToClipPos (v.vertex);
                half2 uv = MultiplyUV( UNITY_MATRIX_TEXTURE0, v.texcoord );
                o.color = v.color * _Color;
                o.uv = uv;
                return o;
            }

            half4 frag (v2f i) : COLOR {
                half4 color = tex2D(_MainTex, i.uv);
                if (color.a == 0.0)
                    discard;

                color.a = (color.r + color.g + color.b)/3.0;
                
                return color * i.color;
            }

			 ENDCG
		}

	}
 
	Fallback off
}