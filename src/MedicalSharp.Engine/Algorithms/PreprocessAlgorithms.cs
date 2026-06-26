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
            volumeData.SyncMarkDataFromGpu(markTexture);
        }
        #endregion

        #region # 区域生长 —— static void RegionGrowing(this VolumeData volumeData...
        /// <summary>
        /// 区域生长
        /// </summary>
        /// <param name="volumeData">体积数据</param>
        /// <param name="previewTexture">预览纹理</param>
        /// <param name="markTexture">标记纹理</param>
        /// <param name="minHU">最小HU值</param>
        /// <param name="maxHU">最大HU值</param>
        /// <param name="markValue">种子点标记值</param>
        /// <param name="tempMarkA">临时标记A（内部使用）</param>
        /// <param name="tempMarkB">临时标记B（内部使用）</param>
        /// <param name="maxIterations">最大迭代次数</param>
        /// <returns>是否成功生长（至少有一个新体素被标记）</returns>
        public static bool RegionGrowing(this VolumeData volumeData, Texture3D previewTexture, Texture3D markTexture, float minHU, float maxHU, byte markValue, byte tempMarkA = 253, byte tempMarkB = 254, int maxIterations = 100)
        {
            #region # 验证

            if (markValue == 0)
            {
                return false;
            }
            if (minHU >= maxHU)
            {
                throw new ArgumentException("最小HU值必须小于最大HU值！");
            }

            #endregion

            //区域生长计算着色器
            ShaderProgram regionGrowingShader = ComputerManager.RegionGrowComputer;
            regionGrowingShader.Use();

            //绑定预览纹理（只读）
            previewTexture.BindImageTexture(0, TextureAccess.ReadOnly);

            //创建两个临时标记纹理用于乒乓交换
            Texture3D pingTexture = Texture3D.CreateCopy(markTexture);
            Texture3D pongTexture = Texture3D.CreateCopy(markTexture);

            //创建原子计数器缓冲区
            using AtomicCounterBuffer counterBuffer = new AtomicCounterBuffer(1);
            counterBuffer.Bind(3);

            byte prevTempMark = markValue;      //第一轮检查的"种子"是原始种子点
            byte currentTempMark = tempMarkA;   //第一轮写入的临时标记
            bool hasNewVoxels = false;

            for (int iteration = 0; iteration < maxIterations; iteration++)
            {
                //重置原子计数器
                counterBuffer.Reset();

                //绑定输出纹理（本轮结果写入pong）
                pongTexture.BindImageTexture(1, TextureAccess.WriteOnly);

                //绑定输入纹理（上一轮结果从ping读取）
                pingTexture.BindImageTexture(2, TextureAccess.ReadOnly);

                //设置参数
                regionGrowingShader.SetUniformVector3i("u_VolumeSize", volumeData.Metadata.VolumeSize);
                regionGrowingShader.SetUniformFloat("u_MinHU", minHU);
                regionGrowingShader.SetUniformFloat("u_MaxHU", maxHU);
                regionGrowingShader.SetUniformUInt("u_MarkValue", markValue);
                regionGrowingShader.SetUniformUInt("u_PrevTempMark", prevTempMark);
                regionGrowingShader.SetUniformUInt("u_CurrentTempMark", currentTempMark);

                //调度执行
                ComputerManager.DispatchCompute3D(volumeData.Metadata.VolumeSize);

                //检查本轮是否有新体素加入
                uint newVoxelsCount = counterBuffer.ReadValue(0);
                hasNewVoxels = newVoxelsCount > 0;
                if (!hasNewVoxels)
                {
                    break;
                }

                //交换绑定纹理：ping <-> pong
                (pingTexture, pongTexture) = (pongTexture, pingTexture);

                //交换临时标记值
                byte swap = prevTempMark;
                prevTempMark = currentTempMark;
                currentTempMark = (swap == tempMarkA) ? tempMarkB : tempMarkA;
            }

            //算法结束：将临时标记统一替换为 markValue
            //TODO: 如果需要，可以在这里添加一个 Shader 将所有 tempMarkA/tempMarkB 替换为 markValue

            //将最终结果拷贝回原始 markTexture
            Texture3D.Copy(pingTexture, markTexture);

            //释放临时纹理
            pingTexture.Dispose();
            pongTexture.Dispose();

            //取消使用
            regionGrowingShader.Unuse();

            //同步CPU端
            volumeData.SyncMarkDataFromGpu(markTexture);

            return hasNewVoxels;
        }
        #endregion
    }
}
