using MedicalSharp.Engine.Managers;
using MedicalSharp.Engine.Resources;
using MedicalSharp.Primitives.Models;
using OpenTK.Graphics.OpenGL4;
using System;

namespace MedicalSharp.Engine.Algorithms
{
    /// <summary>
    /// 同步算法
    /// </summary>
    public static class SyncAlgorithms
    {
        #region # 同步预览数据GPU->CPU —— static void SyncPreviewDataFromGpu(VolumeData volumeData...
        /// <summary>
        /// 同步预览数据GPU->CPU
        /// </summary>
        /// <param name="volumeData">体积数据</param>
        /// <param name="previewTexture">预览纹理</param>
        /// <repreviews>将GPU预览纹理数据回读到CPU端的VolumeData.PreviewData</repreviews>
        public static void SyncPreviewDataFromGpu(VolumeData volumeData, Texture3D previewTexture)
        {
            #region # 验证

            if (volumeData.PreviewData == IntPtr.Zero)
            {
                throw new InvalidOperationException("CPU预览数据未分配！");
            }
            if (!volumeData.TryBeginPreviewGpuToCpu())
            {
                throw new InvalidOperationException($"预览数据正在同步中，当前状态: \"{volumeData.PreviewSyncStatus}\"");
            }

            #endregion

            try
            {
                int width = previewTexture.Width;
                int height = previewTexture.Height;
                int depth = previewTexture.Depth;

                //读取3D纹理到PBO
                using ReadPixelBuffer3D readBuffer = ReadPixelBuffer3D.CreatePreview16(width, height, depth);
                readBuffer.ReadTexture3D(previewTexture, true);

                //读取数据到CPU
                readBuffer.GetCpuBuffer(volumeData.PreviewData);
            }
            finally
            {
                volumeData.EndPreviewSync();
            }
        }
        #endregion

        #region # 同步预览数据CPU->GPU —— static void SyncPreviewDataToGpu(VolumeData volumeData...
        /// <summary>
        /// 同步预览数据CPU->GPU
        /// </summary>
        /// <param name="volumeData">体积数据</param>
        /// <param name="previewTexture">预览纹理</param>
        /// <repreviews>将CPU端VolumeData.PreviewData上传到GPU预览纹理</repreviews>
        public static void SyncPreviewDataToGpu(VolumeData volumeData, Texture3D previewTexture)
        {
            #region # 验证

            if (volumeData.PreviewData == IntPtr.Zero)
            {
                throw new InvalidOperationException("CPU预览数据未分配！");
            }
            if (!volumeData.TryBeginPreviewCpuToGpu())
            {
                throw new InvalidOperationException($"预览数据正在同步中，当前状态: \"{volumeData.PreviewSyncStatus}\"");
            }

            #endregion

            try
            {
                int width = previewTexture.Width;
                int height = previewTexture.Height;
                int depth = previewTexture.Depth;
                using WritePixelBuffer3D writeBuffer = WritePixelBuffer3D.CreatePreview16(width, height, depth);

                //上传到PBO
                writeBuffer.UploadData(volumeData.PreviewData);

                //上传到纹理
                writeBuffer.UploadToTexture(previewTexture, true);
            }
            finally
            {
                volumeData.EndPreviewSync();
            }
        }
        #endregion

        #region # 同步标记数据GPU->CPU —— static void SyncMarkDataFromGpu(VolumeData volumeData...
        /// <summary>
        /// 同步标记数据GPU->CPU
        /// </summary>
        /// <param name="volumeData">体积数据</param>
        /// <param name="markTexture">标记纹理</param>
        /// <remarks>将GPU标记纹理数据回读到CPU端的VolumeData.MarkData</remarks>
        public static void SyncMarkDataFromGpu(VolumeData volumeData, Texture3D markTexture)
        {
            #region # 验证

            if (volumeData.MarkData == IntPtr.Zero)
            {
                throw new InvalidOperationException("CPU标记数据未分配！");
            }
            if (!volumeData.TryBeginMarkGpuToCpu())
            {
                throw new InvalidOperationException($"标记数据正在同步中，当前状态: \"{volumeData.MarkSyncStatus}\"");
            }

            #endregion

            try
            {
                int width = markTexture.Width;
                int height = markTexture.Height;
                int depth = markTexture.Depth;

                //读取3D纹理到PBO
                using ReadPixelBuffer3D readBuffer = ReadPixelBuffer3D.CreateMark8(width, height, depth);
                readBuffer.ReadTexture3D(markTexture, true);

                //读取数据到CPU
                readBuffer.GetCpuBuffer(volumeData.MarkData);
            }
            finally
            {
                volumeData.EndMarkSync();
            }
        }
        #endregion

        #region # 同步标记数据CPU->GPU —— static void SyncMarkDataToGpu(VolumeData volumeData...
        /// <summary>
        /// 同步标记数据CPU->GPU
        /// </summary>
        /// <param name="volumeData">体积数据</param>
        /// <param name="markTexture">标记纹理</param>
        /// <remarks>将CPU端VolumeData.MarkData上传到GPU标记纹理</remarks>
        public static void SyncMarkDataToGpu(VolumeData volumeData, Texture3D markTexture)
        {
            #region # 验证

            if (volumeData.MarkData == IntPtr.Zero)
            {
                throw new InvalidOperationException("CPU标记数据未分配！");
            }
            if (!volumeData.TryBeginMarkCpuToGpu())
            {
                throw new InvalidOperationException($"标记数据正在同步中，当前状态: \"{volumeData.MarkSyncStatus}\"");
            }

            #endregion

            try
            {
                int width = markTexture.Width;
                int height = markTexture.Height;
                int depth = markTexture.Depth;
                using WritePixelBuffer3D writeBuffer = WritePixelBuffer3D.CreateMark8(width, height, depth);

                //上传到PBO
                writeBuffer.UploadData(volumeData.MarkData);

                //上传到纹理
                writeBuffer.UploadToTexture(markTexture, true);
            }
            finally
            {
                volumeData.EndMarkSync();
            }
        }
        #endregion

        #region # 重置预览纹理 —— static void ResetPreviewTexture(VolumeData volumeData...
        /// <summary>
        /// 重置预览纹理
        /// </summary>
        /// <param name="volumeData">体积数据</param>
        /// <param name="previewTexture">预览纹理</param>
        /// <remarks>将预览纹理重置为原始纹理</remarks>
        public static void ResetPreviewTexture(VolumeData volumeData, Texture3D previewTexture)
        {
            #region # 验证

            if (previewTexture == null)
            {
                throw new InvalidOperationException("预览纹理未初始化！");
            }

            #endregion

            //从原始数据更新预览纹理
            previewTexture.Update(volumeData.OriginalData);

            //确保复制完成后后续操作能读到数据
            GL.MemoryBarrier(MemoryBarrierFlags.TextureUpdateBarrierBit);

            //重置CPU端
            volumeData.ResetPreviewData();
        }
        #endregion

        #region # 重置标记纹理 —— static void ResetMarkTexture(VolumeData volumeData...
        /// <summary>
        /// 重置标记纹理
        /// </summary>
        /// <param name="volumeData">体积数据</param>
        /// <param name="markTexture">标记纹理</param>
        /// <remarks>将标记纹理全部设为0</remarks>
        public static void ResetMarkTexture(VolumeData volumeData, Texture3D markTexture)
        {
            //清空标记纹理
            markTexture.Clear();

            //清空CPU端
            volumeData.ResetMarkData();
        }
        #endregion

        #region # 重置标记值 —— static void ResetMarkValue(VolumeData volumeData, Texture3D markTexture...
        /// <summary>
        /// 重置标记值
        /// </summary>
        /// <param name="volumeData">体积数据</param>
        /// <param name="markTexture">标记纹理</param>
        /// <param name="targetMarkValue">目标标记值（1~255）</param>
        /// <remarks>将给定标记值重置为0</remarks>
        public static void ResetMarkValue(VolumeData volumeData, Texture3D markTexture, byte targetMarkValue)
        {
            #region # 验证

            if (targetMarkValue == 0)
            {
                return;
            }

            #endregion

            //重置标记值计算着色器
            ShaderProgram resetMarkValueComputer = ComputerManager.ResetMarkValueComputer;

            //开启Shader程序
            resetMarkValueComputer.Use();

            //绑定标记纹理为可读写
            markTexture.BindImageTexture(0, TextureAccess.ReadWrite);

            //设置体积参数
            resetMarkValueComputer.SetUniformVector3i("u_VolumeSize", volumeData.Metadata.VolumeSize);

            //设置标记值
            resetMarkValueComputer.SetUniformUInt("u_TargetMarkValue", targetMarkValue);

            //调度执行
            ComputerManager.DispatchCompute3D(volumeData.Metadata.VolumeSize);

            //取消使用
            resetMarkValueComputer.Unuse();

            //CPU端重置
            volumeData.ResetMarkValue(targetMarkValue);
        }
        #endregion
    }
}
