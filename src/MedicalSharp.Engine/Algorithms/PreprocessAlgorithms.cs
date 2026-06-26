using MedicalSharp.Engine.Managers;
using MedicalSharp.Engine.Resources;
using MedicalSharp.Primitives.Models;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;

namespace MedicalSharp.Engine.Algorithms
{
    /// <summary>
    /// 预处理算法
    /// </summary>
    public static class PreprocessAlgorithms
    {
        #region # 分配标记 —— static void AssignMark(this VolumeData volumeData...
        /// <summary>
        /// 分配标记
        /// </summary>
        /// <param name="volumeData">体积数据</param>
        /// <param name="markTexture">标记纹理</param>
        /// <param name="voxelPosition">体素坐标</param>
        /// <param name="markValue">标记值</param>
        public static void AssignMark(this VolumeData volumeData, Texture3D markTexture, Vector3i voxelPosition, byte markValue)
        {
            #region # 验证

            if (voxelPosition.X < 0 || voxelPosition.X >= volumeData.Metadata.VolumeSize.X ||
                voxelPosition.Y < 0 || voxelPosition.Y >= volumeData.Metadata.VolumeSize.Y ||
                voxelPosition.Z < 0 || voxelPosition.Z >= volumeData.Metadata.VolumeSize.Z)
            {
                throw new ArgumentOutOfRangeException(nameof(voxelPosition), "体素坐标超出体积范围！");
            }

            #endregion

            //分配标记计算着色器
            ShaderProgram assignMarkComputer = ComputerManager.AssignMarkComputer;

            //开启Shader程序
            assignMarkComputer.Use();

            //绑定纹理
            markTexture.BindImageTexture(0, TextureAccess.WriteOnly);

            //设置参数
            assignMarkComputer.SetUniformVector3i("u_VoxelCoord", voxelPosition);
            assignMarkComputer.SetUniformUInt("u_MarkValue", markValue);

            //调度执行
            GL.DispatchCompute(1, 1, 1);
            GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);

            //取消使用
            assignMarkComputer.Unuse();

            //修改CPU端
            volumeData.SetMarkValue(voxelPosition, markValue);
        }
        #endregion

        #region # 替换标记 —— static void ReplaceMark(this VolumeData volumeData...
        /// <summary>
        /// 替换标记
        /// </summary>
        /// <param name="volumeData">体积数据</param>
        /// <param name="markTexture">标记纹理</param>
        /// <param name="sourceMarkValue">源标记值</param>
        /// <param name="targetMarkValue">目标标记值</param>
        /// <param name="syncToCpu">同步CPU端</param>
        public static void ReplaceMark(this VolumeData volumeData, Texture3D markTexture, byte sourceMarkValue, byte targetMarkValue, bool syncToCpu = true)
        {
            //替换标记计算着色器
            ShaderProgram replaceShader = ComputerManager.ReplaceMarkComputer;

            //开启Shader程序
            replaceShader.Use();

            //绑定纹理
            markTexture.BindImageTexture(0, TextureAccess.ReadWrite);

            //设置参数
            replaceShader.SetUniformVector3i("u_VolumeSize", volumeData.Metadata.VolumeSize);
            replaceShader.SetUniformUInt("u_SourceMarkValue", sourceMarkValue);
            replaceShader.SetUniformUInt("u_TargetMarkValue", targetMarkValue);

            //调度执行
            ComputerManager.DispatchCompute3D(volumeData.Metadata.VolumeSize);

            //取消使用
            replaceShader.Unuse();

            //同步CPU端
            if (syncToCpu)
            {
                volumeData.SyncMarkDataFromGpu(markTexture);
            }
        }
        #endregion

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

        #region # 区域生长 —— static void RegionGrow(this VolumeData volumeData...
        /// <summary>
        /// 区域生长
        /// </summary>
        /// <param name="volumeData">体积数据</param>
        /// <param name="previewTexture">预览纹理</param>
        /// <param name="markTexture">标记纹理</param>
        /// <param name="minHU">最小HU值</param>
        /// <param name="maxHU">最大HU值</param>
        /// <param name="markValue">种子点标记值</param>
        /// <param name="maxIterations">最大迭代次数</param>
        /// <returns>是否成功生长（至少有一个新体素被标记）</returns>
        public static bool RegionGrow(this VolumeData volumeData, Texture3D previewTexture, Texture3D markTexture, float minHU, float maxHU, byte markValue, int maxIterations = 100)
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

            //定义临时标记
            const byte tempMarkA = 254;
            const byte tempMarkB = 255;

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

            //将最终结果拷贝回原始标记纹理
            Texture3D.Copy(pingTexture, markTexture);

            //将临时标记统一替换为种子点标记值
            volumeData.ReplaceMark(markTexture, tempMarkA, markValue, false);
            volumeData.ReplaceMark(markTexture, tempMarkB, markValue, false);

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
