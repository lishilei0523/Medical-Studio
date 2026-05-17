using MedicalSharp.Engine.Renderables;
using MedicalSharp.Engine.Resources;
using MedicalSharp.Primitives.Models;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace MedicalSharp.Engine.Algorithms
{
    /// <summary>
    /// 同步算法
    /// </summary>
    public static class SyncAlgorithms
    {
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
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
            finally
            {
                volumeData.EndPreviewSync();
            }
        }
        #endregion

        #region # 异步同步预览数据GPU->CPU —— static async Task SyncPreviewDataFromGpuAsync(VolumeData volumeData...
        /// <summary>
        /// 异步同步预览数据GPU->CPU
        /// </summary>
        /// <param name="volumeData">体积数据</param>
        /// <param name="previewTexture">预览纹理</param>
        /// <repreviews>将GPU预览纹理数据回读到CPU端的VolumeData.PreviewData</repreviews>
        public static async Task SyncPreviewDataFromGpuAsync(VolumeData volumeData, Texture3D previewTexture)
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
                int bufferSize = width * height * depth;

                //异步读取3D纹理
                byte[] data = await Task.Run(() =>
                {
                    using ReadPixelBuffer3D readBuffer = ReadPixelBuffer3D.CreatePreview16(width, height, depth);
                    readBuffer.ReadTexture3D(previewTexture, true);
                    return readBuffer.GetCpuBuffer();
                });

                //复制到非托管内存
                if (data != null && data.Length == bufferSize)
                {
                    Marshal.Copy(data, 0, volumeData.PreviewData, bufferSize);
                }
            }
            finally
            {
                volumeData.EndPreviewSync();
            }
        }
        #endregion



        //TODO 优化实现


        #region # 同步预览数据GPU->CPU —— static void SyncPreviewDataFromGpu(this VolumeRenderable renderable)
        /// <summary>
        /// 同步预览数据GPU->CPU
        /// </summary>
        /// <param name="renderable">体积渲染对象</param>
        /// <repreviews>将GPU预览纹理数据回读到CPU端的VolumeData.PreviewData</repreviews>
        public static void SyncPreviewDataFromGpu(this VolumeRenderable renderable)
        {
            #region # 验证

            if (renderable.VolumeData.PreviewData == IntPtr.Zero)
            {
                throw new InvalidOperationException("CPU预览数据未分配！");
            }
            if (!renderable.VolumeData.TryBeginPreviewGpuToCpu())
            {
                throw new InvalidOperationException($"预览数据正在同步中，当前状态: \"{renderable.VolumeData.PreviewSyncStatus}\"");
            }

            #endregion

            try
            {
                int width = renderable.PreviewTexture.Width;
                int height = renderable.PreviewTexture.Height;
                int depth = renderable.PreviewTexture.Depth;
                int bufferSize = width * height * depth;

                //读取3D纹理到PBO
                using ReadPixelBuffer3D readBuffer = ReadPixelBuffer3D.CreatePreview16(width, height, depth);
                readBuffer.ReadTexture3D(renderable.PreviewTexture, true);

                //获取数据（阻塞等待）
                byte[] data = readBuffer.GetCpuBuffer();

                //复制到非托管内存
                if (data != null && data.Length == bufferSize)
                {
                    Marshal.Copy(data, 0, renderable.VolumeData.PreviewData, bufferSize);
                }
            }
            finally
            {
                renderable.VolumeData.EndPreviewSync();
            }
        }
        #endregion

        #region # 异步同步预览数据GPU->CPU —— static async Task SyncPreviewDataFromGpuAsync(this VolumeRenderable...
        /// <summary>
        /// 异步同步预览数据GPU->CPU
        /// </summary>
        /// <param name="renderable">体积渲染对象</param>
        /// <repreviews>将GPU预览纹理数据回读到CPU端的VolumeData.PreviewData</repreviews>
        public static async Task SyncPreviewDataFromGpuAsync(this VolumeRenderable renderable)
        {
            #region # 验证

            if (renderable.VolumeData.PreviewData == IntPtr.Zero)
            {
                throw new InvalidOperationException("CPU预览数据未分配！");
            }
            if (!renderable.VolumeData.TryBeginPreviewGpuToCpu())
            {
                throw new InvalidOperationException($"预览数据正在同步中，当前状态: \"{renderable.VolumeData.PreviewSyncStatus}\"");
            }

            #endregion

            try
            {
                int width = renderable.PreviewTexture.Width;
                int height = renderable.PreviewTexture.Height;
                int depth = renderable.PreviewTexture.Depth;
                int bufferSize = width * height * depth;

                //异步读取3D纹理
                byte[] data = await Task.Run(() =>
                {
                    using ReadPixelBuffer3D readBuffer = ReadPixelBuffer3D.CreatePreview16(width, height, depth);
                    readBuffer.ReadTexture3D(renderable.PreviewTexture, true);
                    return readBuffer.GetCpuBuffer();
                });

                //复制到非托管内存
                if (data != null && data.Length == bufferSize)
                {
                    Marshal.Copy(data, 0, renderable.VolumeData.PreviewData, bufferSize);
                }
            }
            finally
            {
                renderable.VolumeData.EndPreviewSync();
            }
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
            #region # 验证

            if (renderable.VolumeData.PreviewData == IntPtr.Zero)
            {
                throw new InvalidOperationException("CPU预览数据未分配！");
            }
            if (!renderable.VolumeData.TryBeginPreviewCpuToGpu())
            {
                throw new InvalidOperationException($"预览数据正在同步中，当前状态: \"{renderable.VolumeData.PreviewSyncStatus}\"");
            }

            #endregion

            try
            {
                int width = renderable.PreviewTexture.Width;
                int height = renderable.PreviewTexture.Height;
                int depth = renderable.PreviewTexture.Depth;
                using WritePixelBuffer3D writeBuffer = WritePixelBuffer3D.CreatePreview16(width, height, depth);

                //上传到PBO
                writeBuffer.UploadData(renderable.VolumeData.PreviewData);

                //上传到纹理
                writeBuffer.UploadToTexture(renderable.PreviewTexture, true);
            }
            finally
            {
                renderable.VolumeData.EndPreviewSync();
            }
        }
        #endregion

        #region # 异步同步预览数据CPU->GPU —— static async Task SyncPreviewDataToGpuAsync(this VolumeRenderable...
        /// <summary>
        /// 异步同步预览数据CPU->GPU
        /// </summary>
        /// <param name="renderable">体积渲染对象</param>
        /// <repreviews>将CPU端VolumeData.PreviewData上传到GPU预览纹理</repreviews>
        public static async Task SyncPreviewDataToGpuAsync(this VolumeRenderable renderable)
        {
            #region # 验证

            if (renderable.VolumeData.PreviewData == IntPtr.Zero)
            {
                throw new InvalidOperationException("CPU预览数据未分配！");
            }
            if (!renderable.VolumeData.TryBeginPreviewCpuToGpu())
            {
                throw new InvalidOperationException($"预览数据正在同步中，当前状态: \"{renderable.VolumeData.PreviewSyncStatus}\"");
            }

            #endregion

            try
            {
                int width = renderable.PreviewTexture.Width;
                int height = renderable.PreviewTexture.Height;
                int depth = renderable.PreviewTexture.Depth;

                await Task.Run(() =>
                {
                    using WritePixelBuffer3D writeBuffer = WritePixelBuffer3D.CreatePreview16(width, height, depth);

                    //上传到PBO
                    writeBuffer.UploadData(renderable.VolumeData.PreviewData);

                    //上传到纹理
                    writeBuffer.UploadToTexture(renderable.PreviewTexture, true);
                });
            }
            finally
            {
                renderable.VolumeData.EndPreviewSync();
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
            #region # 验证

            if (renderable.VolumeData.MarkData == IntPtr.Zero)
            {
                throw new InvalidOperationException("CPU标记数据未分配！");
            }
            if (!renderable.VolumeData.TryBeginMarkGpuToCpu())
            {
                throw new InvalidOperationException($"标记数据正在同步中，当前状态: \"{renderable.VolumeData.MarkSyncStatus}\"");
            }

            #endregion

            try
            {
                int width = renderable.MarkTexture.Width;
                int height = renderable.MarkTexture.Height;
                int depth = renderable.MarkTexture.Depth;
                int bufferSize = width * height * depth;

                //读取3D纹理到PBO
                using ReadPixelBuffer3D readBuffer = ReadPixelBuffer3D.CreateMark8(width, height, depth);
                readBuffer.ReadTexture3D(renderable.MarkTexture, true);

                //获取数据（阻塞等待）
                byte[] data = readBuffer.GetCpuBuffer();

                //复制到非托管内存
                if (data != null && data.Length == bufferSize)
                {
                    Marshal.Copy(data, 0, renderable.VolumeData.MarkData, bufferSize);
                }
            }
            finally
            {
                renderable.VolumeData.EndMarkSync();
            }
        }
        #endregion

        #region # 异步同步标记数据GPU->CPU —— static async Task SyncMarkDataFromGpuAsync(this VolumeRenderable...
        /// <summary>
        /// 异步同步标记数据GPU->CPU
        /// </summary>
        /// <param name="renderable">体积渲染对象</param>
        /// <remarks>将GPU标记纹理数据回读到CPU端的VolumeData.MarkData</remarks>
        public static async Task SyncMarkDataFromGpuAsync(this VolumeRenderable renderable)
        {
            #region # 验证

            if (renderable.VolumeData.MarkData == IntPtr.Zero)
            {
                throw new InvalidOperationException("CPU标记数据未分配！");
            }
            if (!renderable.VolumeData.TryBeginMarkGpuToCpu())
            {
                throw new InvalidOperationException($"标记数据正在同步中，当前状态: \"{renderable.VolumeData.MarkSyncStatus}\"");
            }

            #endregion

            try
            {
                int width = renderable.MarkTexture.Width;
                int height = renderable.MarkTexture.Height;
                int depth = renderable.MarkTexture.Depth;
                int bufferSize = width * height * depth;

                //异步读取3D纹理
                byte[] data = await Task.Run(() =>
                {
                    using ReadPixelBuffer3D readBuffer = ReadPixelBuffer3D.CreateMark8(width, height, depth);
                    readBuffer.ReadTexture3D(renderable.MarkTexture, true);
                    return readBuffer.GetCpuBuffer();
                });

                //复制到非托管内存
                if (data != null && data.Length == bufferSize)
                {
                    Marshal.Copy(data, 0, renderable.VolumeData.MarkData, bufferSize);
                }
            }
            finally
            {
                renderable.VolumeData.EndMarkSync();
            }
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
            #region # 验证

            if (renderable.VolumeData.MarkData == IntPtr.Zero)
            {
                throw new InvalidOperationException("CPU标记数据未分配！");
            }
            if (!renderable.VolumeData.TryBeginMarkCpuToGpu())
            {
                throw new InvalidOperationException($"标记数据正在同步中，当前状态: \"{renderable.VolumeData.MarkSyncStatus}\"");
            }

            #endregion

            try
            {
                int width = renderable.MarkTexture.Width;
                int height = renderable.MarkTexture.Height;
                int depth = renderable.MarkTexture.Depth;
                using WritePixelBuffer3D writeBuffer = WritePixelBuffer3D.CreateMark8(width, height, depth);

                //上传到PBO
                writeBuffer.UploadData(renderable.VolumeData.MarkData);

                //上传到纹理
                writeBuffer.UploadToTexture(renderable.MarkTexture, true);
            }
            finally
            {
                renderable.VolumeData.EndMarkSync();
            }
        }
        #endregion

        #region # 异步同步标记数据CPU->GPU —— static async Task SyncMarkDataToGpuAsync(this VolumeRenderable...
        /// <summary>
        /// 异步同步标记数据CPU->GPU
        /// </summary>
        /// <param name="renderable">体积渲染对象</param>
        /// <remarks>将CPU端VolumeData.MarkData上传到GPU标记纹理</remarks>
        public static async Task SyncMarkDataToGpuAsync(this VolumeRenderable renderable)
        {
            #region # 验证

            if (renderable.VolumeData.MarkData == IntPtr.Zero)
            {
                throw new InvalidOperationException("CPU标记数据未分配！");
            }
            if (!renderable.VolumeData.TryBeginMarkCpuToGpu())
            {
                throw new InvalidOperationException($"标记数据正在同步中，当前状态: \"{renderable.VolumeData.MarkSyncStatus}\"");
            }

            #endregion

            try
            {
                int width = renderable.MarkTexture.Width;
                int height = renderable.MarkTexture.Height;
                int depth = renderable.MarkTexture.Depth;

                await Task.Run(() =>
                {
                    using WritePixelBuffer3D writeBuffer = WritePixelBuffer3D.CreateMark8(width, height, depth);

                    //上传到PBO
                    writeBuffer.UploadData(renderable.VolumeData.MarkData);

                    //上传到纹理
                    writeBuffer.UploadToTexture(renderable.MarkTexture, true);
                });
            }
            finally
            {
                renderable.VolumeData.EndMarkSync();
            }
        }
        #endregion
    }
}
