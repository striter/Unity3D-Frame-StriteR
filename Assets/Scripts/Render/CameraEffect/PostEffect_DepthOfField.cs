﻿using System;
using UnityEngine;
namespace Rendering.ImageEffect
{
    public class PostEffect_DepthOfField : PostEffectBase<CameraEffect_DepthOfField, CameraEffectParam_DepthOfField>
    {
    }

    [System.Serializable]
    public class CameraEffectParam_DepthOfField : ImageEffectParamBase
    {
        [Tooltip("景深起始深度"), Range(0.01f, 1f)]
        public float m_DOFStart = 0.1f;
        [Tooltip("景深渐淡插值深度"), Range(.01f, .3f)]
        public float m_DOFLerp = .1f;
        [Tooltip("深度取值模糊")]
        public bool m_DepthBlurSample = true;
        [Tooltip("深度取值模糊像素偏差"), Range(.25f, 1.25f)]
        public float m_BlurSize = .5f;
        [SerializeField, Tooltip("采样图模糊参数")]
        public ImageEffectParam_Blurs m_BlurParams;
    }
    public class CameraEffect_DepthOfField : ImageEffectBase<CameraEffectParam_DepthOfField>
    {
        #region ShaderID
        static int ID_FocalStart = Shader.PropertyToID("_FocalStart");
        static int ID_FocalLerp = Shader.PropertyToID("_FocalLerp");
        static int ID_BlurTexture = Shader.PropertyToID("_BlurTex");
        static int ID_BlurSize = Shader.PropertyToID("_BlurSize");
        const string KW_UseBlurDepth = "_UseBlurDepth";
        #endregion

        ImageEffect_Blurs m_Blur;
        public CameraEffect_DepthOfField() : base() { m_Blur = new ImageEffect_Blurs(); }
        public override void Destroy()
        {
            base.Destroy();
            m_Blur.Destroy();
        }
        protected override void OnValidate(CameraEffectParam_DepthOfField _params, Material _material)
        {
            base.OnValidate(_params, _material);
            _material.SetFloat(ID_FocalStart, _params.m_DOFStart);
            _material.SetFloat(ID_FocalLerp, _params.m_DOFLerp);
            _material.EnableKeyword(KW_UseBlurDepth, _params.m_DepthBlurSample);
            _material.SetFloat(ID_BlurSize, _params.m_BlurSize);
            m_Blur.DoValidate(_params.m_BlurParams);
        }
        protected override void OnImageProcess(RenderTexture _src, RenderTexture _dst, Material _material, CameraEffectParam_DepthOfField _param)
        {
            RenderTexture _tempBlurTex = RenderTexture.GetTemporary(_src.width, _src.height, 0, _src.format);
            m_Blur.DoImageProcess(_src, _tempBlurTex,_param.m_BlurParams);
            _material.SetTexture(ID_BlurTexture, _tempBlurTex);
            Graphics.Blit(_src, _dst, _material);
            RenderTexture.ReleaseTemporary(_tempBlurTex);
        }

    }
}
