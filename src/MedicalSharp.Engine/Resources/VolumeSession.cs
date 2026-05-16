using MedicalSharp.Engine.Managers;
using MedicalSharp.Engine.Protocols;
using MedicalSharp.Primitives.Models;
using OpenTK.Graphics.OpenGL4;
using System;

namespace MedicalSharp.Engine.Resources
{
    /// <summary>
    /// 体积会话
    /// </summary>
    public class VolumeSession : IDisposable
    {
        #region # 字段及构造器

        /// <summary>
        /// 释放标识
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// 创建体积会话构造器
        /// </summary>
        /// <param name="volumeData">体积数据</param>
        public VolumeSession(VolumeData volumeData)
        {
            #region # 验证

            if (volumeData == null)
            {
                throw new ArgumentNullException(nameof(volumeData), "体积数据不可为空！");
            }

            #endregion

            this.Id = volumeData.Metadata.Id;
            this.VolumeData = volumeData;

            //初始化
            int width = this.VolumeData.Metadata.VolumeSize.X;
            int height = this.VolumeData.Metadata.VolumeSize.Y;
            int depth = this.VolumeData.Metadata.VolumeSize.Z;
            this.OriginalTexture = Texture3D.CreateFromVolume(width, height, depth, this.VolumeData.OriginalData);
            this.PreviewTexture = Texture3D.CreateFromVolume(width, height, depth, this.VolumeData.PreviewData);
            this.MarkTexture = Texture3D.CreateFromMark(width, height, depth, this.VolumeData.MarkData);
            this.VRTransferFunction = new DensityTransferFunction();
            this.MPRTransferFunction = new HUTransferFunction();
            this.MarkStrategy = new MarkStrategy();
        }

        #endregion

        #region # 属性

        #region 标识Id —— string Id
        /// <summary>
        /// 标识Id
        /// </summary>
        public string Id { get; private set; }
        #endregion

        #region 体积数据 —— VolumeData VolumeData
        /// <summary>
        /// 体积数据
        /// </summary>
        public VolumeData VolumeData { get; private set; }
        #endregion

        #region 原始纹理 —— Texture3D OriginalTexture
        /// <summary>
        /// 原始纹理
        /// </summary>
        public Texture3D OriginalTexture { get; private set; }
        #endregion

        #region 预览纹理 —— Texture3D PreviewTexture
        /// <summary>
        /// 预览纹理
        /// </summary>
        public Texture3D PreviewTexture { get; private set; }
        #endregion

        #region 标记纹理 —— Texture3D MarkTexture
        /// <summary>
        /// 标记纹理
        /// </summary>
        public Texture3D MarkTexture { get; private set; }
        #endregion

        #region 体积渲染传递函数 —— DensityTransferFunction VRTransferFunction
        /// <summary>
        /// 体积渲染传递函数
        /// </summary>
        public DensityTransferFunction VRTransferFunction { get; private set; }
        #endregion

        #region MPR渲染传递函数 —— HUTransferFunction MPRTransferFunction
        /// <summary>
        /// MPR渲染传递函数
        /// </summary>
        public HUTransferFunction MPRTransferFunction { get; private set; }
        #endregion

        #region 标记策略 —— TransferFunction MarkStrategy
        /// <summary>
        /// 标记策略
        /// </summary>
        public MarkStrategy MarkStrategy { get; private set; }
        #endregion

        #endregion

        #region # 方法

        #region 重置预览纹理 —— void ResetPreviewTexture()
        /// <summary>
        /// 重置预览纹理
        /// </summary>
        /// <remarks>将预览纹理重置为原始纹理</remarks>
        public void ResetPreviewTexture()
        {
            #region # 验证

            if (this.OriginalTexture == null)
            {
                throw new InvalidOperationException("原始纹理未初始化！");
            }
            if (this.PreviewTexture == null)
            {
                throw new InvalidOperationException("预览纹理未初始化！");
            }
            if (this.PreviewTexture.Width != this.OriginalTexture.Width ||
                this.PreviewTexture.Height != this.OriginalTexture.Height ||
                this.PreviewTexture.Depth != this.OriginalTexture.Depth)
            {
                throw new InvalidOperationException("预览纹理与原始纹理尺寸不匹配！");
            }

            #endregion

            //确保之前的GPU操作完成
            GL.MemoryBarrier(MemoryBarrierFlags.TextureUpdateBarrierBit);

            //从原始纹理复制到预览纹理
            GL.CopyImageSubData(
                this.OriginalTexture.Id, ImageTarget.Texture3D, 0, 0, 0, 0,
                this.PreviewTexture.Id, ImageTarget.Texture3D, 0, 0, 0, 0,
                this.PreviewTexture.Width, this.PreviewTexture.Height, this.PreviewTexture.Depth);

            //确保复制完成后后续操作能读到数据
            GL.MemoryBarrier(MemoryBarrierFlags.TextureUpdateBarrierBit);

            //重置CPU端
            this.VolumeData.ResetPreviewData();
        }
        #endregion

        #region 重置标记纹理 —— void ResetMarkTexture()
        /// <summary>
        /// 重置标记纹理
        /// </summary>
        /// <remarks>将标记纹理全部设为0</remarks>
        public unsafe void ResetMarkTexture()
        {
            //清空标记纹理
            this.MarkTexture.Clear();

            //清空CPU端
            this.VolumeData.ResetMarkData();
        }
        #endregion

        #region 重置标记值 —— void ResetMarkValue(byte targetMarkValue)
        /// <summary>
        /// 重置标记值
        /// </summary>
        /// <param name="targetMarkValue">目标标记值（1~255）</param>
        /// <remarks>将给定标记值重置为0</remarks>
        public void ResetMarkValue(byte targetMarkValue)
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
            this.MarkTexture.BindImageTexture(0, TextureAccess.ReadWrite);

            //设置体积参数
            resetMarkValueComputer.SetUniformVector3i("u_VolumeSize", this.VolumeData.Metadata.VolumeSize);

            //设置标记值
            resetMarkValueComputer.SetUniformUInt("u_TargetMarkValue", targetMarkValue);

            //调度执行
            ComputerManager.DispatchCompute3D(this.VolumeData.Metadata.VolumeSize);

            //取消使用
            resetMarkValueComputer.Unuse();

            //CPU端重置
            this.VolumeData.ResetMarkValue(targetMarkValue);
        }
        #endregion

        #region 释放资源 —— void Dispose()
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (this._disposed)
            {
                return;
            }

            this.VolumeData?.Dispose();
            this.OriginalTexture?.Dispose();
            this.PreviewTexture?.Dispose();
            this.MarkTexture?.Dispose();
            this.VRTransferFunction?.Dispose();
            this.MPRTransferFunction?.Dispose();
            this.MarkStrategy?.Dispose();
            this._disposed = true;
        }
        #endregion

        #endregion
    }
}
