using MedicalSharp.Engine.Renderables;
using MedicalSharp.Engine.Resources;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace MedicalSharp.Engine.Algorithms
{
    /// <summary>
    /// 同步算法
    /// </summary>
    public static class SyncAlgorithms
    {
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
            if (!renderable.VolumeData.TryBeginGpuToCpu())
            {
                throw new InvalidOperationException($"标记数据正在同步中，当前状态: \"{renderable.VolumeData.SyncStatus}\"");
            }

            #endregion

            try
            {
                int width = renderable.MarkTexture.Width;
                int height = renderable.MarkTexture.Height;
                int depth = renderable.MarkTexture.Depth;
                int bufferSize = width * height * depth;

                //读取3D纹理到PBO
                using ReadPixelBuffer3D readBuffer = new ReadPixelBuffer3D(width, height, depth, PixelFormat.RedInteger, PixelType.UnsignedByte);
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
                renderable.VolumeData.EndSync();
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
            if (!renderable.VolumeData.TryBeginGpuToCpu())
            {
                throw new InvalidOperationException($"标记数据正在同步中，当前状态: \"{renderable.VolumeData.SyncStatus}\"");
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
                    using ReadPixelBuffer3D readBuffer = new ReadPixelBuffer3D(width, height, depth, PixelFormat.RedInteger, PixelType.UnsignedByte);
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
                renderable.VolumeData.EndSync();
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
            if (!renderable.VolumeData.TryBeginCpuToGpu())
            {
                throw new InvalidOperationException($"标记数据正在同步中，当前状态: \"{renderable.VolumeData.SyncStatus}\"");
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
                renderable.VolumeData.EndSync();
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
            if (!renderable.VolumeData.TryBeginCpuToGpu())
            {
                throw new InvalidOperationException($"标记数据正在同步中，当前状态: \"{renderable.VolumeData.SyncStatus}\"");
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
                renderable.VolumeData.EndSync();
            }
        }
        #endregion
    }
}
