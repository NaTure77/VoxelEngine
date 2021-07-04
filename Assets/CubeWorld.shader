Shader "Hidden/CubeWorld"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Position("Position", vector) = (0,0,0)
        _Rotation("Rotation", vector) = (0,0,0)
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

#define MAX_STEPS 40
            sampler2D _MainTex;
            float3 _Position;
            float3 _Rotation;
            float4 _Map[1024];

            float3 Rem(float3 b)
            {
                b.x = fmod(b.x,4);
                b.y = fmod(b.y,63);
                b.z = fmod(b.z,4);
                if (b.x < 0) b.x += 4;
                if (b.y < 0) b.y += 63;
                if (b.z < 0) b.z += 4;
                return b;
            }
            float4 RayMarch(float3 ro, float3 rd)
            {
                //현재 검사 블록 위치
                ro = Rem(ro);
                int3 b = ro;//round(ro / tileSize);
                
                //진행방향
                int3 d = sign(rd);
                
                //가중치. 요소의 값이 작을수록 더해지는 값이 더 커짐.
                float3 val = 1 / abs(rd);

                //초기 더해진 가중치 계산
                float3 target = ((0.5 * d) - (fmod(ro,1) - 0.5)) / rd;
                float4 result = float4(0,0,0,0);
                for (int i = 0; i < MAX_STEPS; i++)
                {
                    int idx = b.x + b.y * 16 + b.z * 4;
                    
                    if (_Map[idx].w == 1)
                    {
                        result = _Map[idx];
                        break;
                    }
                    if (target.x < target.y)
                    {
                        if (target.x < target.z)
                        {
                            target.x += val.x;
                            b.x += d.x;
                        }
                        else
                        {
                            target.z += val.z;
                            b.z += d.z;
                        }
                    }
                    else
                    {
                        if (target.y < target.z)
                        {
                            target.y += val.y;
                            b.y += d.y;
                        }
                        else
                        {
                            target.z += val.z;
                            b.z += d.z;
                        }
                    }
                    b = Rem(b);
                }
                return result;
            }
            float3 rot(float3 p, float3 a)
            {
                float sx = sin(a.x);
                float cx = cos(a.x);

                float sy = sin(a.y);
                float cy = cos(a.y);
                // p -= pivot;
                p = float3(p.x, p.y * cx - p.z * sx, p.y * sx + p.z * cx);
                p = float3(p.x * cy - p.z * sy, p.y, p.x * sy + p.z * cy);
                // p += pivot;

                return p;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv - 0.5;
                float3 ro = _Position;// float3(0, 0, -3);
                float3 rd = rot(normalize(float3(uv.x * _ScreenParams.x / _ScreenParams.y, uv.y, 0.86)), _Rotation);

                fixed4 col = RayMarch(ro, rd);
                return col;
            }
            ENDCG
        }
    }
}
