Shader "HXIME/IME_Engine"
{
    Properties
    {
        _PinyinTexA ("拼音数据 A（RGBA32）", 2D) = "black" {}
        _PinyinTexB ("拼音数据 B（RGBA32）", 2D) = "black" {}
        _PinyinFullTexA ("全拼数据 A（RGBA32）", 2D) = "black" {}
        _PinyinFullTexB ("全拼数据 B（RGBA32）", 2D) = "black" {}
        _MetaTex ("词频元数据（RGBA32 打包）", 2D) = "black" {}
        _InputCharsA ("输入字符 ID 1-4", Vector) = (0, 0, 0, 0)
        _InputCharsB ("输入字符 ID 5-8", Vector) = (0, 0, 0, 0)
        _InputLen ("输入长度", Range(0, 8)) = 0
        _FullInputCharsA ("全拼输入字母 ID 1-4", Vector) = (0, 0, 0, 0)
        _FullInputCharsB ("全拼输入字母 ID 5-8", Vector) = (0, 0, 0, 0)
        _FullInputLen ("全拼输入长度", Range(0, 8)) = 0
        [Toggle] _RawScore ("输出原始排序分数（用于 RFloat RT）", Float) = 0
        _PreviewMaxWeight ("预览最大词频", Float) = 5000000
        _MinBaseWeight ("最低词频过滤", Float) = 0
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Overlay+2000" }
        Cull Off
        ZWrite On
        ZTest LEqual
        Blend Off

        Pass
        {
            HLSLPROGRAM
            #pragma target 4.0
            #pragma vertex Vert
            #pragma fragment Frag

            #include "UnityCG.cginc"

            Texture2D<float4> _PinyinTexA;
            Texture2D<float4> _PinyinTexB;
            Texture2D<float4> _PinyinFullTexA;
            Texture2D<float4> _PinyinFullTexB;
            Texture2D<float4> _MetaTex;
            SamplerState sampler_PointClamp;

            static const int SCORE_STRIDE = 5000000;

            float4 _InputCharsA;
            float4 _InputCharsB;
            float _InputLen;
            float4 _FullInputCharsA;
            float4 _FullInputCharsB;
            float _FullInputLen;
            float _RawScore;
            float _PreviewMaxWeight;
            float _MinBaseWeight;

            struct Attributes
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = UnityObjectToClipPos(input.vertex);
                output.uv = input.uv;
                return output;
            }

            int DecodeId(float value)
            {
                return (int)round(saturate(value) * 255.0);
            }

            int DecodeInputId(float value)
            {
                return clamp((int)round(value), 0, 26);
            }

            int DecodeWeight(float4 packed)
            {
                int3 bytes = (int3)round(saturate(packed.rgb) * 255.0);
                return bytes.r + bytes.g * 256 + bytes.b * 65536;
            }

            float4 Frag(Varyings input) : SV_Target
            {
                int inputLength = clamp((int)round(_InputLen), 0, 8);
                if (inputLength == 0) return float4(0, 0, 0, 1);

                float2 uv = saturate(input.uv);
                float4 metadata = _MetaTex.SampleLevel(sampler_PointClamp, uv, 0);
                int baseWeight = clamp(DecodeWeight(metadata), 0, SCORE_STRIDE - 1);
                clip((float)(baseWeight - (int)round(max(_MinBaseWeight, 0.0))) + 0.5);

                float4 charsA = _PinyinTexA.SampleLevel(sampler_PointClamp, uv, 0);
                float4 charsB = _PinyinTexB.SampleLevel(sampler_PointClamp, uv, 0);
                float4 fullCharsA = _PinyinFullTexA.SampleLevel(sampler_PointClamp, uv, 0);
                float4 fullCharsB = _PinyinFullTexB.SampleLevel(sampler_PointClamp, uv, 0);
                int wordChars[8];
                int inputChars[8];
                int fullWordChars[8];
                int fullInputChars[8];

                wordChars[0] = DecodeId(charsA.r);
                wordChars[1] = DecodeId(charsA.g);
                wordChars[2] = DecodeId(charsA.b);
                wordChars[3] = DecodeId(charsA.a);
                wordChars[4] = DecodeId(charsB.r);
                wordChars[5] = DecodeId(charsB.g);
                wordChars[6] = DecodeId(charsB.b);
                wordChars[7] = DecodeId(charsB.a);

                inputChars[0] = DecodeInputId(_InputCharsA.x);
                inputChars[1] = DecodeInputId(_InputCharsA.y);
                inputChars[2] = DecodeInputId(_InputCharsA.z);
                inputChars[3] = DecodeInputId(_InputCharsA.w);
                inputChars[4] = DecodeInputId(_InputCharsB.x);
                inputChars[5] = DecodeInputId(_InputCharsB.y);
                inputChars[6] = DecodeInputId(_InputCharsB.z);
                inputChars[7] = DecodeInputId(_InputCharsB.w);

                fullWordChars[0] = DecodeId(fullCharsA.r);
                fullWordChars[1] = DecodeId(fullCharsA.g);
                fullWordChars[2] = DecodeId(fullCharsA.b);
                fullWordChars[3] = DecodeId(fullCharsA.a);
                fullWordChars[4] = DecodeId(fullCharsB.r);
                fullWordChars[5] = DecodeId(fullCharsB.g);
                fullWordChars[6] = DecodeId(fullCharsB.b);
                fullWordChars[7] = DecodeId(fullCharsB.a);

                fullInputChars[0] = DecodeInputId(_FullInputCharsA.x);
                fullInputChars[1] = DecodeInputId(_FullInputCharsA.y);
                fullInputChars[2] = DecodeInputId(_FullInputCharsA.z);
                fullInputChars[3] = DecodeInputId(_FullInputCharsA.w);
                fullInputChars[4] = DecodeInputId(_FullInputCharsB.x);
                fullInputChars[5] = DecodeInputId(_FullInputCharsB.y);
                fullInputChars[6] = DecodeInputId(_FullInputCharsB.z);
                fullInputChars[7] = DecodeInputId(_FullInputCharsB.w);

                int wordLength = 0;
                [unroll]
                for (int i = 0; i < 8; i++)
                {
                    if (wordChars[i] == 0) break;
                    wordLength++;
                }

                bool prefixMatch = inputLength <= wordLength;
                [unroll]
                for (int i = 0; i < 8; i++)
                {
                    if (i >= inputLength) break;
                    if (inputChars[i] == 0 || inputChars[i] != wordChars[i])
                        prefixMatch = false;
                }

                int tierKey = 0;
                int fullInputLength = clamp((int)round(_FullInputLen), 0, 8);
                int fullWordLength = (int)round(saturate(metadata.a) * 255.0);
                bool fullPrefixMatch = fullInputLength > 0 && fullWordLength >= fullInputLength;
                [unroll]
                for (int i = 0; i < 8; i++)
                {
                    if (i >= fullInputLength) break;
                    if (fullWordChars[i] == 0 || fullWordChars[i] != fullInputChars[i])
                        fullPrefixMatch = false;
                }

                // Incomplete syllables (for example "to") are not recognized by the full-pinyin segmenter.
                bool partialSpellingPrefix = fullInputLength == 0 && inputLength > 0 && fullWordLength >= inputLength;
                [unroll]
                for (int i = 0; i < 8; i++)
                {
                    if (i >= inputLength) break;
                    if (inputChars[i] == 0 || fullWordChars[i] != inputChars[i])
                        partialSpellingPrefix = false;
                }

                if (fullPrefixMatch)
                {
                    tierKey = fullWordLength == fullInputLength ? 4 : 3;
                }
                else if (partialSpellingPrefix)
                {
                    tierKey = 3;
                }
                else if (prefixMatch)
                {
                    tierKey = inputLength == wordLength ? 2 : 1;
                }
                else
                {
                    int inputIndex = 0;
                    [unroll]
                    for (int wordIndex = 0; wordIndex < 8; wordIndex++)
                    {
                        if (inputIndex >= inputLength || wordIndex >= wordLength) break;
                        if (wordChars[wordIndex] == inputChars[inputIndex]) inputIndex++;
                    }
                    if (inputIndex == inputLength) tierKey = 0;
                    else return float4(0, 0, 0, 1);
                }

                // Quantized output: 20 logarithmic weight bins per five match tiers, encoded as 0.01-1.00.
                float weightLog = log2((float)baseWeight + 1.0) / log2((float)SCORE_STRIDE);
                int weightBin = clamp((int)floor(saturate(weightLog) * 19.0), 0, 19);
                int bucket = tierKey * 20 + weightBin + 1;
                if (_RawScore > 0.5) return float4((float)bucket / 100.0, 0, 0, 1);

                float weightLevel = weightLog;
                float brightness = lerp(0.25, 1.0, saturate(weightLevel));
                float3 tierColor = tierKey >= 3 ? float3(0.15, 1.0, 0.35) :
                    tierKey == 2 ? float3(1.0, 0.58, 0.08) : float3(0.12, 0.55, 1.0);
                return float4(tierColor * brightness, 1.0);
            }
            ENDHLSL
        }
    }
}
