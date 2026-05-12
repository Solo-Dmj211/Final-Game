Shader "SystemFailure/TowerPulse"
{
    // Properties exposed to the Inspector and settable from C#.
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color           ("Tint",            Color)  = (1,1,1,1)

        _GlowColor       ("Glow Color",      Color)  = (0, 0.8, 1, 1)
        _GlowIntensity   ("Glow Intensity",  Range(0, 5)) = 0.0
        _PulseSpeed      ("Pulse Speed",     Range(0, 20)) = 2.0
        _PulseMin        ("Pulse Min",       Range(0, 1)) = 0.5
        _PulseMax        ("Pulse Max",       Range(0, 2)) = 1.0
    }

    SubShader
    {
        // Tags so URP treats this like a transparent sprite.
        Tags
        {
            "RenderType"      = "Transparent"
            "Queue"           = "Transparent"
            "RenderPipeline"  = "UniversalPipeline"
            "IgnoreProjector" = "True"
            "PreviewType"     = "Plane"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "SpritePulse"

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color      : COLOR;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float4 color       : COLOR;
                float2 uv          : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float4 _GlowColor;
                float  _GlowIntensity;
                float  _PulseSpeed;
                float  _PulseMin;
                float  _PulseMax;
            CBUFFER_END

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv          = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.color       = IN.color * _Color;
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                // Sample the sprite texture and apply tint.
                half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv) * IN.color;

                // Time-driven pulse: sine wave remapped to [_PulseMin, _PulseMax].
                // sin returns [-1, 1]; (sin*0.5 + 0.5) gives [0, 1]; lerp into our range.
                float wave  = sin(_Time.y * _PulseSpeed) * 0.5 + 0.5;
                float pulse = lerp(_PulseMin, _PulseMax, wave);

                // Build the additive glow contribution:
                // glow = glow color * intensity * pulse, masked by the sprite's alpha
                // so the glow only shows on the visible silhouette.
                half3 glow = _GlowColor.rgb * _GlowIntensity * pulse * tex.a;

                // Add glow on top of the base sprite color.
                half3 finalRgb = tex.rgb + glow;

                return half4(finalRgb, tex.a);
            }
            ENDHLSL
        }
    }
}
