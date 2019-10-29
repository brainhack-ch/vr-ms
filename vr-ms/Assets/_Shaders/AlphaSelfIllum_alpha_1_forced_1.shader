

Shader "Unlit/AlphaSelfIllum_alpha_1_forced_1" {
	Properties{


		_Color("Main Color (A=Opacity)", Color) = (1,1,1,1)
		_MainTex("Base (A=Opacity)", 2D) = ""
		//_Activate_Alpha("Activate alpha", Range(0.0, 1.0)) = 0.5




	}

		Category{


		Tags{ "Queue" = "Opaque" }
		//ZWrite Off
		Cull Off
		//Blend SrcAlpha OneMinusSrcAlpha

		SubShader{
		Pass{

		GLSLPROGRAM
		varying mediump vec2 uv;

#ifdef VERTEX
	uniform mediump vec4 _MainTex_ST;
	void main() {
		gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;
		uv = gl_MultiTexCoord0.xy * _MainTex_ST.xy + _MainTex_ST.zw;
	}
#endif

#ifdef FRAGMENT
	uniform lowp sampler2D _MainTex;
	uniform lowp vec4 _Color;
	void main() {
		gl_FragColor = texture * _Color;
	}
#endif     
	ENDGLSL
	}
	}

		SubShader{

		Pass{
			SetTexture[_MainTex]
		}
	}

	}

}