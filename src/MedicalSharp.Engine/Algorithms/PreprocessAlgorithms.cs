using MedicalSharp.Engine.Managers;
using MedicalSharp.Engine.Resources;
using MedicalSharp.Primitives.Models;
using OpenTK.Graphics.OpenGL4;
using System;

namespace MedicalSharp.Engine.Algorithms
{
    /// <summary>
    /// 预处理算法
    /// </summary>
    public static class PreprocessAlgorithms
    {
        #region # 阈值分割 —— static void ThresholdSegment(this VolumeData volumeData...
        /// <summary>
        /// 阈值分割
        /// </summary>
        /// <param name="volumeData">体积数据</param>
        /// <param name="previewTexture">预览纹理</param>
        /// <param name="markTexture">标记纹理</param>
        /// <param name="minHU">最小HU值</param>
        /// <param name="maxHU">最大HU值</param>
        /// <param name="markValue">标记值</param>
        public static void ThresholdSegment(this VolumeData volumeData, Texture3D previewTexture, Texture3D markTexture, float minHU, float maxHU, byte markValue)
        {
            #region # 验证

            if (markValue == 0)
            {
                return;
            }
            if (minHU >= maxHU)
            {
                throw new ArgumentException("最小HU值必须小于最大HU值！");
            }

            #endregion

            //阈值分割计算着色器
            ShaderProgram segmentShader = ComputerManager.ThresholdSegmentComputer;

            //开启Shader程序
            segmentShader.Use();

            //绑定纹理
            previewTexture.BindImageTexture(0, TextureAccess.ReadOnly);
            markTexture.BindImageTexture(1, TextureAccess.WriteOnly);

            //设置参数
            segmentShader.SetUniformVector3i("u_VolumeSize", volumeData.Metadata.VolumeSize);
            segmentShader.SetUniformFloat("u_MinHU", minHU);
            segmentShader.SetUniformFloat("u_MaxHU", maxHU);
            segmentShader.SetUniformUInt("u_MarkValue", markValue);

            //调度执行
            ComputerManager.DispatchCompute3D(volumeData.Metadata.VolumeSize);

            //取消使用
            segmentShader.Unuse();

            //同步CPU端
            SyncAlgorithms.SyncMarkDataFromGpu(volumeData, markTexture);
        }
        #endregion
    }
}
