Shader "CustomShader/water" {
    Properties {
        _color("color", Color) = (1,1,1,1)
        _albedo ("albedo", 2D) = "white" {}                       // 水面基础颜色贴图（也可理解为表面纹理）
        [NoScaleOffset] _normalMap ("normal map", 2D) = "bump" {} // 法线贴图（不允许Tiling/Offset：你要完全用代码控制）
        [NoScaleOffset] _displacementMap ("displacement map", 2D) = "white" {} // 位移/高度贴图：驱动顶点沿法线方向起伏

        _gloss ("gloss", Range(0,1)) = 1                         // 高光强度 + 高光锐度的控制（在后面映射到 specular power）
        _normalIntensity ("normal intensity", Range(0, 1)) = 1   // 法线扰动强度（0=平面法线，1=完全使用法线贴图）
        _displacementIntensity ("displacement intensity", Range(0,1)) = 0.5 // 顶点位移幅度
        _refractionIntensity ("refraction intensity", Range(0, 0.5)) = 0.1  // 折射强度（用法线xy偏移屏幕采样UV）
        _opacity ("opacity", Range(0,1)) = 0.9                   // “水覆盖背景”的程度（1=更像不透明表面，0=更像完全透明只看背景）
        _softFactor("Soft Factor", Range(0.01,3.0)) = 1.0       // 
        
    }
    SubShader {
        // this shader won't actually use transparency, but we want it to render with the transparent objects
        Tags {
            "RenderPipeline" = "UniversalPipeline" // URP专用
            "Queue" = "Transparent"                // 放进透明队列：确保能拿到 CameraOpaqueTexture（只包含不透明物体的帧缓冲）
            "IgnoreProjector" = "True"
        }
        Pass {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            // Core：坐标变换/时间/工具函数；Lighting：GetMainLight等光照接口
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            // 用于把_gloss映射到一个最大高光幂次，让高光范围更可控
            #define MAX_SPECULAR_POWER 256

            CBUFFER_START(UnityPerMaterial)
            float _gloss;                   // 高光参数（同时用于强度与幂次映射）
            float _normalIntensity;         // 法线贴图权重
            float _displacementIntensity;   // 位移幅度
            float _refractionIntensity;     // 折射UV偏移幅度
            float _opacity;                 // 混合背景 vs 表面光照
            float _softFactor;
            float4 _color;
            //sampler2D _CameraDepthTexture;
            
            float4 _albedo_ST;              // Unity自动生成：_albedo 的 tiling/offset
            CBUFFER_END
            

            // 常规贴图采样声明（URP推荐写法）
            TEXTURE2D(_albedo);
            SAMPLER(sampler_albedo);
            
            TEXTURE2D(_normalMap);
            SAMPLER(sampler_normalMap);
            
            TEXTURE2D(_displacementMap);
            SAMPLER(sampler_displacementMap);

            TEXTURE2D(_CameraDepthTexture);
            SAMPLER(sampler_CameraDepthTexture);

            

            // 由URP提供的不透明颜色缓冲（需要在 URP Asset 里开启 Opaque Texture）
            // 用它来做“折射”：从屏幕上采样背景画面
            TEXTURE2D(_CameraOpaqueTexture);
            SAMPLER(sampler_CameraOpaqueTexture);
            
            struct MeshData {
                float4 vertex : POSITION; // 物体空间顶点
                float3 normal : NORMAL;   // 物体空间法线
                float4 tangent : TANGENT; // 物体空间切线（w存手性：决定bitangent方向）
                float2 uv : TEXCOORD0;    // 模型UV
            };

            struct Interpolators {
                float4 vertex : SV_POSITION; // 裁剪空间位置（给光栅化用）

                float2 uv : TEXCOORD0;       // 传到frag的UV

                // 世界空间的TBN基向量：用于把切线空间法线转成世界空间法线
                float3 normal : TEXCOORD1;   
                float3 tangent : TEXCOORD2;
                float3 bitangent : TEXCOORD3;

                float3 posWorld : TEXCOORD4; // 世界空间位置：算视线方向用
                
                // 用两个float2做不同方向/速度的panning，打破重复感
                // uvPan.xy 和 uvPan.zw 各是一组（这里后面只用到了.xy）
                float4 uvPan : TEXCOORD5;

                // ComputeScreenPos的结果（xy/w 才是最终屏幕UV）
                // 注意：w 很关键（透视校正），它本质上来自 clip space 的 w
                float4 screenUV : TEXCOORD6;
            };

            Interpolators vert (MeshData v) {
                Interpolators o;

                // 应用_albedo的tiling/offset（来自_albedo_ST）
                o.uv = TRANSFORM_TEX(v.uv, _albedo);
                
                // panning：让贴图随时间滚动（_Time.x 通常是 time/20 左右的尺度）
                // 第一组速度更快（0.9,0.2），第二组更慢（0.5,-0.2）
                o.uvPan = float4(
                    float2(0.9, 0.2) * _Time.x,
                    float2(0.5, -0.2) * _Time.x
                );

                // 顶点位移（vertex displacement）：
                // 用位移贴图的r通道当高度，把顶点沿“物体法线”推开
                // SampleLevel(...,0) 强制mip0：保证顶点阶段稳定且不会因为mip变化抖动
                float height = _displacementMap.SampleLevel(
                    sampler_displacementMap,
                    o.uv + o.uvPan.xy,
                    0
                ).r;

                v.vertex.xyz += v.normal * height * _displacementIntensity;

                // 把法线/切线从物体空间变到世界空间
                o.normal = TransformObjectToWorldNormal(v.normal);

                // 这里用 TransformObjectToWorldNormal 处理 tangent（更严谨可用方向变换函数，但你这里足够用）
                o.tangent = TransformObjectToWorldNormal(v.tangent);

                // bitangent = cross(normal, tangent) * tangent.w
                // tangent.w 存“手性”，用来修正交叉乘会翻转的问题
                o.bitangent = cross(o.normal, o.tangent) * v.tangent.w;

                // 顶点从物体空间 -> 裁剪空间
                o.vertex = TransformObjectToHClip(v.vertex);
                
                // 计算屏幕空间位置（内部会处理平台差异）
                // 注意：返回的xy并不是直接可用的屏幕UV，需要除以w
                o.screenUV = ComputeScreenPos(o.vertex);
                
                // 世界空间位置（用于视线方向等）
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                
                return o;
            }

            float4 frag (Interpolators i) : SV_Target {
                float2 uv = i.uv;

                // 透视校正后的屏幕UV：screenPos.xy / screenPos.w
                float2 screenUV = i.screenUV.xy / i.screenUV.w;


                
                //================== opacity depending on Depth ======================
                float rawDepth = SAMPLE_TEXTURE2D(_CameraDepthTexture, sampler_CameraDepthTexture, screenUV).r;

                float sceneDepth = LinearEyeDepth(rawDepth, _ZBufferParams);
                float waterDepth = LinearEyeDepth(i.screenUV.w, _ZBufferParams);

                float depthFade = saturate(_softFactor * (sceneDepth - waterDepth));
                //=====================================================================


                
                
                // 从法线贴图采样切线空间法线（Tangent Space）
                // UnpackNormal 会把贴图里的(0..1)还原到(-1..1)并做对应的法线解码
                float3 tangentSpaceNormal =
                    UnpackNormal(_normalMap.Sample(sampler_normalMap, uv + i.uvPan.xy));

                // 第二次采样：作为“细节法线”（通常会用不同UV/不同panning做两层叠加）
                // 你这里采样完全相同，所以它本质是把同一张法线叠加一次（会更强一些）
                float3 tangentSpaceDetailNormal =
                    UnpackNormal(_normalMap.Sample(sampler_normalMap, uv + i.uvPan.xy));

                // RNM(Reoriented Normal Mapping) 混合法线：比直接相加更正确
                tangentSpaceNormal = BlendNormalRNM(tangentSpaceNormal, tangentSpaceDetailNormal);

                // 把法线强度从“平面法线(0,0,1)”插值到贴图法线
                tangentSpaceNormal = normalize(
                    lerp(float3(0, 0, 1), tangentSpaceNormal, _normalIntensity)
                );
                
                // 折射：用切线空间法线的xy偏移屏幕UV（假装光线被水面扰动）
                float2 refractionUV = screenUV.xy + (tangentSpaceNormal.xy * _refractionIntensity);

                // 从不透明帧缓冲采样背景颜色（即水下看到的世界）
                float3 background =
                    _CameraOpaqueTexture.Sample(sampler_CameraOpaqueTexture, refractionUV);

                // 组装 TBN 矩阵（切线空间 -> 世界空间）
                float3x3 tangentToWorld = float3x3 
                (
                    i.tangent.x,   i.bitangent.x,   i.normal.x,
                    i.tangent.y,   i.bitangent.y,   i.normal.y,
                    i.tangent.z,   i.bitangent.z,   i.normal.z
                );

                // 把切线空间法线转到世界空间，参与光照计算
                float3 normal = mul(tangentToWorld, tangentSpaceNormal);
                
                // ---- Blinn-Phong 光照 ----
                // 表面颜色：从_albedo采样（也用同样panning让纹理流动）
                float3 surfaceColor = _color;

                // 主光（方向光）信息：方向、颜色、阴影等（这里只用了方向与颜色）
                Light light = GetMainLight();
                
                // 视线方向：从表面点指向相机
                float3 viewDirection = normalize(GetCameraPositionWS() - i.posWorld);

                // Blinn-Phong 半角向量：view + lightDir 再归一
                float3 halfDirection = normalize(viewDirection + light.direction);

                // 漫反射：N·L
                float diffuseFalloff = max(0, dot(normal, light.direction));

                // 高光：N·H
                float specularFalloff = max(0, dot(normal, halfDirection));

                // specular power = _gloss * MAX_SPECULAR_POWER + 1
                // +1 避免为0导致 pow(x,0)=1 的奇怪情况
                float3 specular =
                    pow(specularFalloff, _gloss * MAX_SPECULAR_POWER + 1) * _gloss * light.color;

                float3 diffuse = diffuseFalloff * surfaceColor * light.color;

                // 把“折射背景”和“表面漫反射”按_opacity混合
                // _opacity 越大越像实体水面（更多diffuse），越小越像透明折射（更多background）
                

                // =========================== Water Edge Color Code ================================

                

                
                // opacity based on depth
                float alpha = saturate(_opacity * depthFade);
                float3 finalColor = lerp(background, diffuse, alpha) + specular;






                
                // alpha 这里固定1：虽然在透明队列渲染，但你并没有用alpha混合
                return float4(finalColor, alpha);
            }
            ENDHLSL
        }
    }
}
