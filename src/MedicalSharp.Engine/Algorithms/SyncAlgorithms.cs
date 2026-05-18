using MedicalSharp.Engine.Renderables;
using MedicalSharp.Engine.Resources;
using MedicalSharp.Primitives.Models;
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

        #region # 同步预览数据GPU->CPU —— static void SyncPreviewDataFromGpu(this VolumeRenderable renderable)
        /// <summary>
        /// 同步预览数据GPU->CPU
        /// </summary>
        /// <param name="renderable">体积渲染对象</param>
        /// <repreviews>将GPU预览纹理数据回读到CPU端的VolumeData.PreviewData</repreviews>
        public static void SyncPreviewDataFromGpu(this VolumeRenderable renderable)
        {
            SyncPreviewDataFromGpu(renderable.VolumeData, renderable.PreviewTexture);
        }
        #endregion

        #region # 同步预览数据CPU->GPU —— static void SyncPreviewDataToGpu(this VolumeRenderable renderable)
        /// <summary>
        /// 同步预览数据CPU->GPU
        /// </summary>
        /// <param name="renderable">体积渲染对象</param>
        /// <repreviews>将CPU端VolumeData.PreviewData上传到GPU预览纹理</repreviews>
        public static void SyncPreviewDataToGpu(this VolumeRenderable renderable)
        {
            SyncPreviewDataToGpu(renderable.VolumeData, renderable.PreviewTexture);
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

        #region # 同步标记数据GPU->CPU —— static void SyncMarkDataFromGpu(this VolumeRenderable renderable)
        /// <summary>
        /// 同步标记数据GPU->CPU
        /// </summary>
        /// <param name="renderable">体积渲染对象</param>
        /// <remarks>将GPU标记纹理数据回读到CPU端的VolumeData.MarkData</remarks>
        public static void SyncMarkDataFromGpu(this VolumeRenderable renderable)
        {
            SyncMarkDataFromGpu(renderable.VolumeData, renderable.MarkTexture);
        }
        #endregion

        #region # 同步标记数据CPU->GPU —— static void SyncMarkDataToGpu(this VolumeRenderable renderable)
        /// <summary>
        /// 同步标记数据CPU->GPU
        /// </summary>
        /// <param name="renderable">体积渲染对象</param>
        /// <remarks>将CPU端VolumeData.MarkData上传到GPU标记纹理</remarks>
        public static void SyncMarkDataToGpu(this VolumeRenderable renderable)
        {
            SyncMarkDataToGpu(renderable.VolumeData, renderable.MarkTexture);
        }
        #endregion
    }
}
