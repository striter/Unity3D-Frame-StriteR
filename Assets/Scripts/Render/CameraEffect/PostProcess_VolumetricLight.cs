using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Rendering.ImageEffect
{
    public class PostProcess_VolumetricLight : PostProcessComponentBase<PPCore_VolumetricLight, PPData_VolumetricLight>
    {
    }
    public enum enum_LightMarchTimes
    {
        _8=8,
        _16=16,
        _32=32,
        _64=64,
        _128=128,
    }
    [Serializable]
    public struct PPData_VolumetricLight
    {
        [Header("Color")]
        [Range(.1f,5f)]public float m_Strength;
        [Header("March")]
        [Clamp(0f)] public float m_Distance;
        public enum_LightMarchTimes m_MarchTimes;
        [Range(0, 2f)] public float m_MarchStrength;
        [Range(0.1f, 2f)] public float m_Pow;
        [Header("Optimize"),Range(1, 4)] public int m_DownSample;
        public bool m_Dither;
        public bool m_Debug;
        [Header("Blur")]
        [MTitle] public bool m_EnableBlur;
        [MFoldout(nameof(m_EnableBlur), true)] public PPData_Blurs m_BlurParam;
        public static readonly PPData_VolumetricLight m_Default = new PPData_VolumetricLight()
        {
            m_Strength = 1f,
            m_Pow = 2,
            m_MarchStrength = .3f,
            m_Distance = 20f,
            m_MarchTimes = enum_LightMarchTimes._64,
            m_DownSample=2,
            m_Dither=false,
            m_Debug = false,
            m_EnableBlur=false,
            m_BlurParam=PPData_Blurs.m_Default,
        };
    }

    public class PPCore_VolumetricLight : PostProcessCore<PPData_VolumetricLight>
    {
        #region ShaderProperties
        static readonly int ID_ColorStrength = Shader.PropertyToID("_ColorStrength");
        static readonly int ID_LightPow = Shader.PropertyToID("_LightPow");
        static readonly int ID_LightStrength = Shader.PropertyToID("_LightStrength");
        static readonly int ID_MarchTimes = Shader.PropertyToID("_MarchTimes");
        static readonly int ID_MarchDistance = Shader.PropertyToID("_MarchDistance");
        const string kW_DITHER = "_DITHER";

        static readonly int RT_ID_Sample = Shader.PropertyToID("_VolumetricLight_Sample");
        static RenderTargetIdentifier RT_Sample = new RenderTargetIdentifier(RT_ID_Sample);
        static readonly int RT_ID_Blur = Shader.PropertyToID("_VolumetricLight_Blur");
        static RenderTargetIdentifier RT_Blur = new RenderTargetIdentifier(RT_ID_Blur);
        #endregion
        public PPCore_Blurs m_CoreBlur;
        public override void Create()
        {
            base.Create();
            m_CoreBlur = new PPCore_Blurs();
        }
        public override void Destroy()
        {
            base.Destroy();
            m_CoreBlur.Destroy();
        }

        public override void OnValidate(PPData_VolumetricLight _params)
        {
            base.OnValidate(_params);
            m_Material.SetFloat(ID_ColorStrength,_params.m_Strength);
            m_Material.SetInt(ID_MarchTimes, (int)_params.m_MarchTimes);
            m_Material.SetFloat(ID_MarchDistance, _params.m_Distance);
            m_Material.SetFloat(ID_LightPow, _params.m_Pow);
            m_Material.SetFloat(ID_LightStrength, _params.m_MarchStrength);
            m_Material.EnableKeyword(kW_DITHER, _params.m_Dither);
            m_CoreBlur.OnValidate(_params.m_BlurParam);
        }
        public override void ExecutePostProcessBuffer(CommandBuffer _buffer, RenderTargetIdentifier _src, RenderTargetIdentifier _dst, RenderTextureDescriptor _descriptor, PPData_VolumetricLight ppData)
        {
            _descriptor.width /= ppData.m_DownSample;
            _descriptor.height /= ppData.m_DownSample;
            _descriptor.colorFormat = RenderTextureFormat.R8;
            _descriptor.depthBufferBits = 0;
            _buffer.GetTemporaryRT(RT_ID_Sample, _descriptor,FilterMode.Bilinear);

            if (!ppData.m_EnableBlur)
            {
                _buffer.Blit(_src, RT_ID_Sample, m_Material, 0);
            }
            else
            {
                _buffer.GetTemporaryRT(RT_ID_Blur, _descriptor, FilterMode.Bilinear);
                _buffer.Blit(_src, RT_ID_Blur, m_Material, 0);
                m_CoreBlur.ExecutePostProcessBuffer(_buffer, RT_ID_Blur, RT_ID_Sample, _descriptor,ppData.m_BlurParam); 
                _buffer.ReleaseTemporaryRT(RT_ID_Blur);
            }

            if(ppData.m_Debug)
                _buffer.Blit(RT_ID_Sample,_dst);
            else
                _buffer.Blit(_src, _dst, m_Material, 1);
            
            _buffer.ReleaseTemporaryRT(RT_ID_Sample);
        }
    }
}