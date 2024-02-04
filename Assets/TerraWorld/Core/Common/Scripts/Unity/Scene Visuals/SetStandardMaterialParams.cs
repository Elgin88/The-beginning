#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace TerraUnity.Runtime
{
    public class SetStandardMaterialParams
    {
        public enum BlendMode
        {
            Opaque,
            Cutout,
            Fade,   // Old school alpha-blending mode, fresnel does not affect amount of transparency
            Transparent, // Physically plausible transparency mode, implemented as alpha pre-multiply
            Additive,
            Subtractive,
            Modulate
        }

        public static void SwitchMaterialBlendingType (Material material, BlendMode blendMode)
        {
            // _Emission property is lost after assigning Standard shader to the material
            // thus transfer it before assigning the new shader
            if (material.HasProperty("_Emission") && material.HasProperty("_EmissionColor"))
                material.SetColor("_EmissionColor", material.GetColor("_Emission"));

            material.SetFloat("_Mode", (float)blendMode);
            MaterialChanged(material, blendMode);
        }

        private static void MaterialChanged(Material material, BlendMode blendMode)
        {
            SetupMaterialWithBlendMode(material, blendMode);
            SetMaterialKeywords(material);
        }

        private static void SetupMaterialWithBlendMode(Material material, BlendMode blendMode)
        {
            switch (blendMode)
            {
                case BlendMode.Opaque:
                    material.SetOverrideTag("RenderType", "");
                    material.SetInt("_BlendOp", (int)UnityEngine.Rendering.BlendOp.Add);
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    material.SetInt("_ZWrite", 1);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.DisableKeyword("_ALPHAMODULATE_ON");
                    material.renderQueue = -1;
                    break;
                case BlendMode.Cutout:
                    material.SetOverrideTag("RenderType", "TransparentCutout");
                    material.SetInt("_BlendOp", (int)UnityEngine.Rendering.BlendOp.Add);
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    material.SetInt("_ZWrite", 1);
                    material.EnableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.DisableKeyword("_ALPHAMODULATE_ON");
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
                    break;
                case BlendMode.Fade:
                    material.SetOverrideTag("RenderType", "Transparent");
                    material.SetInt("_BlendOp", (int)UnityEngine.Rendering.BlendOp.Add);
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.SetInt("_ZWrite", 0);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.EnableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.DisableKeyword("_ALPHAMODULATE_ON");
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                    break;
                case BlendMode.Transparent:
                    material.SetOverrideTag("RenderType", "Transparent");
                    material.SetInt("_BlendOp", (int)UnityEngine.Rendering.BlendOp.Add);
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.SetInt("_ZWrite", 0);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.DisableKeyword("_ALPHAMODULATE_ON");
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                    break;
                case BlendMode.Additive:
                    material.SetOverrideTag("RenderType", "Transparent");
                    material.SetInt("_BlendOp", (int)UnityEngine.Rendering.BlendOp.Add);
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_ZWrite", 0);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.EnableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.DisableKeyword("_ALPHAMODULATE_ON");
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                    break;
                case BlendMode.Subtractive:
                    material.SetOverrideTag("RenderType", "Transparent");
                    material.SetInt("_BlendOp", (int)UnityEngine.Rendering.BlendOp.ReverseSubtract);
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_ZWrite", 0);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.EnableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.DisableKeyword("_ALPHAMODULATE_ON");
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                    break;
                case BlendMode.Modulate:
                    material.SetOverrideTag("RenderType", "Transparent");
                    material.SetInt("_BlendOp", (int)UnityEngine.Rendering.BlendOp.Add);
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.DstColor);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.SetInt("_ZWrite", 0);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.EnableKeyword("_ALPHAMODULATE_ON");
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                    break;
            }
        }

        private static void SetMaterialKeywords(Material material)
        {
            // Z write doesn't work with distortion/fading
            bool hasZWrite = false;
            if (material.HasProperty("_ZWrite"))
                hasZWrite = (material.GetInt("_ZWrite") != 0);

            // Lit shader?
            bool useLighting = false;
            if (material.HasProperty("_LightingEnabled"))
                useLighting = (material.GetFloat("_LightingEnabled") > 0.0f);

            // Note: keywords must be based on Material value not on MaterialProperty due to multi-edit & material animation
            // (MaterialProperty value might come from renderer material property block)

            bool useDistortion = false;
            if (material.HasProperty("_DistortionEnabled"))
                useDistortion = !hasZWrite && (material.GetFloat("_DistortionEnabled") > 0.0f);

            if (material.HasProperty("_BumpMap"))
                SetKeyword(material, "_NORMALMAP", (useLighting || useDistortion) && material.GetTexture("_BumpMap"));

            if (material.HasProperty("_MetallicGlossMap"))
                SetKeyword(material, "_METALLICGLOSSMAP", useLighting && (material.GetTexture("_MetallicGlossMap") != null));

            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;

            if (material.HasProperty("_EmissionColor"))
            {
                if (material.IsKeywordEnabled("_EMISSION"))
                    material.EnableKeyword("_EMISSION");
                else
                {
                    material.DisableKeyword("_EMISSION");
                    material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
                }
            }
            else
            {
                material.DisableKeyword("_EMISSION");
                material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
            }


            // Terra_Standard Realted Features
            //-------------------------------------------------------------------------------------------------------------------------------
            if (material.HasProperty("_ProceduralLayer"))
            {
                if (material.GetFloat("_ProceduralLayer") == 1)
                    SetKeyword(material, "_PROCEDURALSNOW", true);
                else
                    SetKeyword(material, "_PROCEDURALSNOW", false);
            }

            if (material.HasProperty("_Wind"))
            {
                if (material.GetFloat("_Wind") == 1)
                    SetKeyword(material, "_WIND", true);
                else
                    SetKeyword(material, "_WIND", false);
            }

            if (material.HasProperty("_IsLeaves"))
            {
                if (material.GetFloat("_IsLeaves") == 1)
                    SetKeyword(material, "_IS_LEAVES", true);
                else
                    SetKeyword(material, "_IS_LEAVES", false);
            }

            if (material.HasProperty("_IsBark"))
            {
                if (material.GetFloat("_IsBark") == 1)
                    SetKeyword(material, "_IS_BARK", true);
                else
                    SetKeyword(material, "_IS_BARK", false);
            }

            if (material.HasProperty("_IsGrass"))
            {
                if (material.GetFloat("_IsGrass") == 1)
                    SetKeyword(material, "_IS_GRASS", true);
                else
                    SetKeyword(material, "_IS_GRASS", false);
            }

            if (material.HasProperty("_FlatShading"))
            {
                if (material.GetFloat("_FlatShading") == 1)
                    SetKeyword(material, "_FLATSHADING", true);
                else
                    SetKeyword(material, "_FLATSHADING", false);
            }

            if (material.HasProperty("_DetalUseUV3"))
            {
                if (material.GetFloat("_DetalUseUV3") == 1)
                    SetKeyword(material, "_DETALUSEUV3_ON", true);
                else
                    SetKeyword(material, "_DETALUSEUV3_ON", false);
            }

            if (material.HasProperty("_ColormapBlending"))
            {
                if (material.GetFloat("_ColormapBlending") == 1)
                    SetKeyword(material, "_COLORMAP_BLENDING", true);
                else
                    SetKeyword(material, "_COLORMAP_BLENDING", false);
            }


            // Terra_Clouds Realted Features
            //-------------------------------------------------------------------------------------------------------------------------------

            // Set the define for flipbook blending
            bool useFlipbookBlending = false;
            if (material.HasProperty("_FlipbookMode"))
                useFlipbookBlending = (material.GetFloat("_FlipbookMode") > 0.0f);

            SetKeyword(material, "_REQUIRE_UV2", useFlipbookBlending);

            // Clamp fade distances
            bool useSoftParticles = false;
            if (material.HasProperty("_SoftParticlesEnabled"))
                useSoftParticles = (material.GetFloat("_SoftParticlesEnabled") > 0.0f);

            bool useCameraFading = false;
            if (material.HasProperty("_CameraFadingEnabled"))
                useCameraFading = (material.GetFloat("_CameraFadingEnabled") > 0.0f);

            float softParticlesNearFadeDistance = 0;
            if (material.HasProperty("_SoftParticlesNearFadeDistance"))
            {
                softParticlesNearFadeDistance = material.GetFloat("_SoftParticlesNearFadeDistance");

                if (softParticlesNearFadeDistance < 0.0f)
                {
                    softParticlesNearFadeDistance = 0.0f;
                    material.SetFloat("_SoftParticlesNearFadeDistance", 0.0f);
                }
            }    

            float softParticlesFarFadeDistance = 0;
            if (material.HasProperty("_SoftParticlesFarFadeDistance"))
            {
                softParticlesFarFadeDistance = material.GetFloat("_SoftParticlesFarFadeDistance");

                if (softParticlesFarFadeDistance < 0.0f)
                {
                    softParticlesFarFadeDistance = 0.0f;
                    material.SetFloat("_SoftParticlesFarFadeDistance", 0.0f);
                }
            } 

            float cameraNearFadeDistance = 0;
            if (material.HasProperty("_CameraNearFadeDistance"))
            {
                cameraNearFadeDistance = material.GetFloat("_CameraNearFadeDistance");

                if (cameraNearFadeDistance < 0.0f)
                {
                    cameraNearFadeDistance = 0.0f;
                    material.SetFloat("_CameraNearFadeDistance", 0.0f);
                }
            }

            float cameraFarFadeDistance = 0;
            if (material.HasProperty("_CameraFarFadeDistance"))
            {
                cameraFarFadeDistance = material.GetFloat("_CameraFarFadeDistance");

                if (cameraFarFadeDistance < 0.0f)
                {
                    cameraFarFadeDistance = 0.0f;
                    material.SetFloat("_CameraFarFadeDistance", 0.0f);
                }
            }

            // Set the define for fading
            //if (material.HasProperty("_FADING_ON"))
            {
                bool useFading = (useSoftParticles || useCameraFading) && !hasZWrite;
                SetKeyword(material, "_FADING_ON", useFading);
            }

            if (material.HasProperty("_SoftParticleFadeParams"))
            {
                if (useSoftParticles)
                    material.SetVector("_SoftParticleFadeParams", new Vector4(softParticlesNearFadeDistance, 1.0f / (softParticlesFarFadeDistance - softParticlesNearFadeDistance), 0.0f, 0.0f));
                else
                    material.SetVector("_SoftParticleFadeParams", new Vector4(0.0f, 0.0f, 0.0f, 0.0f));
            }

            if (material.HasProperty("_CameraFadeParams"))
            {
                if (useCameraFading)
                    material.SetVector("_CameraFadeParams", new Vector4(cameraNearFadeDistance, 1.0f / (cameraFarFadeDistance - cameraNearFadeDistance), 0.0f, 0.0f));
                else
                    material.SetVector("_CameraFadeParams", new Vector4(0.0f, Mathf.Infinity, 0.0f, 0.0f));
            }

            // Set the define for distortion + grabpass
            //if (material.HasProperty("EFFECT_BUMP"))
            {
                SetKeyword(material, "EFFECT_BUMP", useDistortion);
            }

            //if (material.FindPass("Always") > 0)
                material.SetShaderPassEnabled("Always", useDistortion);

            if (material.HasProperty("_DistortionStrengthScaled") && material.HasProperty("_DistortionStrength"))
            {
                if (useDistortion)
                    material.SetFloat("_DistortionStrengthScaled", material.GetFloat("_DistortionStrength") * 0.1f);   // more friendly number scale than 1 unit per size of the screen
            }
        }

        private static void SetKeyword(Material m, string keyword, bool state)
        {
            if (state)
                m.EnableKeyword(keyword);
            else
                m.DisableKeyword(keyword);
        }
    }
}
#endif

