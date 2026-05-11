using MedicalSharp.Engine.Protocols;
using MedicalSharp.Primitives.Models;
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
            this.VolumeTexture = Texture3D.CreateFromVolume(width, height, depth, this.VolumeData.OriginalData);
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

        #region 体积纹理 —— Texture3D VolumeTexture
        /// <summary>
        /// 体积纹理
        /// </summary>
        public Texture3D VolumeTexture { get; private set; }
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
            this.VolumeTexture?.Dispose();
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
