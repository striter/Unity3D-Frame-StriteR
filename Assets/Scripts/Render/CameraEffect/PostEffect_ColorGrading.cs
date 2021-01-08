﻿
using UnityEngine;
using System;
using UnityEditor;

namespace Rendering.ImageEffect
{
    public class PostEffect_ColorGrading : PostEffectBase<ImageEffect_ColorGrading,ImageEffectParam_ColorGrading>{}

    public enum enum_MixChannel
    {
        None = 0,
        Red = 1,
        Green = 2,
        Blue = 3,
    }
    public enum enum_LUTCellCount
    {
        _16 = 16,
        _32 = 32,
        _64 = 64,
        _128 = 128,
    }

    [System.Serializable]
    public class ImageEffectParam_ColorGrading : ImageEffectParamBase
    {
        [Tooltip("总权重"), Range(0, 1)]
        public float m_Weight = 1;

        [Header("LUT_颜色对照表")]
        [Tooltip("颜色对照表")]
        public Texture2D m_LUT = null;
        [Tooltip("32格/16格")]
        public enum_LUTCellCount m_LUTCellCount = enum_LUTCellCount._16;

        [Header("BSC_亮度 饱和度 对比度")]
        [Tooltip("亮度"), Range(0, 2)]
        public float m_brightness = 1;
        [Tooltip("饱和度"), Range(0, 2)]
        public float m_saturation = 1;
        [Tooltip("对比度"), Range(0, 2)]
        public float m_contrast = 1;

        [Header("Channel Mixer_通道混合器")]
        [Tooltip("红色通道混合")]
        public Vector3 m_MixRed = Vector3.zero;
        [Tooltip("绿色通道混合")]
        public Vector3 m_MixGreen = Vector3.zero;
        [Tooltip("蓝色通道混合")]
        public Vector3 m_MixBlue = Vector3.zero;
    }

    public class ImageEffect_ColorGrading : ImageEffectBase<ImageEffectParam_ColorGrading>
    {
        #region ShaderProperties
        readonly int ID_Weight = Shader.PropertyToID("_Weight");

        const string KW_LUT = "_LUT";
        readonly int ID_LUT = Shader.PropertyToID("_LUTTex");
        readonly int ID_LUTCellCount = Shader.PropertyToID("_LUTCellCount");

        const string KW_BSC = "_BSC";
        readonly int ID_Brightness = Shader.PropertyToID("_Brightness");
        readonly int ID_Saturation = Shader.PropertyToID("_Saturation");
        readonly int ID_Contrast = Shader.PropertyToID("_Contrast");

        const string KW_MixChannel = "_CHANNEL_MIXER";
        readonly int ID_MixRed = Shader.PropertyToID("_MixRed");
        readonly int ID_MixGreen = Shader.PropertyToID("_MixGreen");
        readonly int ID_MixBlue = Shader.PropertyToID("_MixBlue");
        #endregion
        protected override void OnValidate(ImageEffectParam_ColorGrading _params, Material _material)
        {
            base.OnValidate(_params, _material);
            _material.SetFloat(ID_Weight, _params.m_Weight);

            _material.EnableKeyword(KW_LUT, _params.m_LUT);
            _material.SetTexture(ID_LUT, _params.m_LUT);
            _material.SetInt(ID_LUTCellCount, (int)_params.m_LUTCellCount);

            _material.EnableKeyword(KW_BSC, _params.m_brightness != 1 || _params.m_saturation != 1f || _params.m_contrast != 1);
            _material.SetFloat(ID_Brightness, _params.m_brightness);
            _material.SetFloat(ID_Saturation, _params.m_saturation);
            _material.SetFloat(ID_Contrast, _params.m_contrast);

            _material.EnableKeyword(KW_MixChannel, _params.m_MixRed != Vector3.zero || _params.m_MixBlue != Vector3.zero || _params.m_MixGreen != Vector3.zero);
            _material.SetVector(ID_MixRed, _params.m_MixRed);
            _material.SetVector(ID_MixGreen, _params.m_MixGreen);
            _material.SetVector(ID_MixBlue, _params.m_MixBlue);
        }
    }
}