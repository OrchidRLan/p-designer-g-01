Shader "Custom/HSV_Cylinder"
{
    Properties
    {
        _Radius("Cylinder Radius", Float) = 1.0
        _CursorLocalPos("Cursor Local Position", Vector) = (0, 0, 0, 0)
    }

    SubShader
    {
        Tags { 
            "RenderType"="Opaque" 
            "Queue"="Geometry" 
        }

        Cull Back
        ZWrite On

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            float _Radius;
            float3 _CursorLocalPos;

            float3 HSVtoRGB(float h, float s, float v)
            {
                float3 k = float3(1.0, 2.0/3.0, 1.0/3.0);
                float3 p = abs(frac(h + k) * 6.0 - 3.0);
                return v * lerp(k.xxx, saturate(p - k.xxx), s);
            }

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 os : TEXCOORD0;
                float3 osModified : TEXCOORD1;
            };

            v2f vert(appdata v)
            {
                v2f o;
                
                o.os = v.vertex.xyz;
                o.osModified = v.vertex.xyz;
                
                float epsilon = 0.01;
                bool isTopCap = (v.vertex.y > 1.0 - epsilon);
                
                if (isTopCap)
                {
                    float clampedCursorY = _CursorLocalPos.y;
                    
                    if (clampedCursorY < -1.0)
                    {
                        clampedCursorY = -0.9;
                    }
                    else if (clampedCursorY > 1.0)
                    {
                        clampedCursorY = 1.0;
                    }
                    
                    o.osModified.y = clampedCursorY;
                    o.pos = UnityObjectToClipPos(float4(o.osModified, 1.0));
                }
                else
                {
                    o.pos = UnityObjectToClipPos(v.vertex);
                }
                
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float3 p = i.osModified;
                float yNorm = saturate((p.y + 1.0) / 2.0);

                float clampedCursorY = _CursorLocalPos.y;
                
                if (clampedCursorY < -1.0)
                {
                    clampedCursorY = -0.9;
                }
                else if (clampedCursorY > 1.0)
                {
                    clampedCursorY = 1.0;
                }
                
                float cursorYNorm = saturate((clampedCursorY + 1.0) / 2.0);

                float epsilon = 0.01;
                bool isTopCap = (i.os.y > 1.0 - epsilon);
                bool isBottomCap = (i.os.y < -1.0 + epsilon);

                float sat = saturate(length(p.xz) / _Radius);
                float angle = atan2(p.z, p.x);
                float hue = frac(angle / (2.0 * UNITY_PI) + 1.0);
                float3 colorWheel = HSVtoRGB(hue, sat, 1.0);

                float3 finalColor;

                if (isTopCap)
                {
                    finalColor = colorWheel * cursorYNorm;
                }
                else if (isBottomCap)
                {
                    finalColor = float3(0, 0, 0);
                }
                else
                {
                    if (yNorm > cursorYNorm)
                        discard;
                    
                    finalColor = colorWheel * yNorm;
                }

                return float4(finalColor, 1.0);
            }

            ENDCG
        }
    }
}