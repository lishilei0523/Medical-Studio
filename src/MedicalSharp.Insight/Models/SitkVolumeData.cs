using itk.simple;
using MedicalSharp.Primitives.Models;
using System;

namespace MedicalSharp.Insight.Models
{
    /// <summary>
    /// 体积数据
    /// </summary>
    public sealed class SitkVolumeData : VolumeData
    {
        #region # 字段及构造器

        /// <summary>
        /// 默认构造器
        /// </summary>
        internal SitkVolumeData()
            : base()
        {

        }

        #endregion

        #region # 属性

        #region SimpleITK图像 —— Image SitkImage
        /// <summary>
        /// SimpleITK图像
        /// </summary>
        public Image SitkImage { get; internal set; }
        #endregion

        #region SimpleITK标记图像 —— Image SitkMarkImage
        /// <summary>
        /// SimpleITK标记图像
        /// </summary>
        public Image SitkMarkImage { get; internal set; }
        #endregion

        #region 只读属性 - 原始数据 —— override IntPtr OriginalData
        /// <summary>
        /// 只读属性 - 原始数据
        /// </summary>
        public override IntPtr OriginalData
        {
            get
            {
                if (this.SitkImage == null)
                {
                    return IntPtr.Zero;
                }

                return this.SitkImage.GetBufferAsInt16();
            }
        }
        #endregion

        #region 只读属性 - 标记数据 —— override IntPtr MarkData
        /// <summary>
        /// 只读属性 - 标记数据
        /// </summary>
        public override IntPtr MarkData
        {
            get
            {
                if (this.SitkMarkImage == null)
                {
                    return IntPtr.Zero;
                }

                return this.SitkMarkImage.GetBufferAsUInt8();
            }
        }
        #endregion

        #endregion

        #region # 方法

        #region 分配标记数据 —— void AllocMarkData()
        /// <summary>
        /// 分配标记数据
        /// </summary>
        public void AllocMarkData()
        {
            #region # 验证

            if (this.Metadata == null)
            {
                throw new InvalidOperationException("体积信息未初始化");
            }
            if (this.MarkData != IntPtr.Zero)
            {
                this.SitkMarkImage?.Dispose();
            }

            #endregion

            uint[] sizeArray =
            [
                (uint)this.Metadata.VolumeSize.X,
                (uint)this.Metadata.VolumeSize.Y,
                (uint)this.Metadata.VolumeSize.Z
            ];
            VectorUInt32 size = new VectorUInt32(sizeArray);
            this.SitkMarkImage = new Image(size, PixelIDValueEnum.sitkUInt8, 1);

            //从原始图像复制空间元数据
            if (this.SitkImage != null)
            {
                this.SitkMarkImage.SetSpacing(this.SitkImage.GetSpacing());
                this.SitkMarkImage.SetOrigin(this.SitkImage.GetOrigin());
                this.SitkMarkImage.SetDirection(this.SitkImage.GetDirection());
            }
        }
        #endregion

        #region 释放资源 —— override void Dispose()
        /// <summary>
        /// 释放资源
        /// </summary>
        public override void Dispose()
        {
            if (this._disposed)
            {
                return;
            }

            this.SitkImage?.Dispose();
            this.SitkMarkImage?.Dispose();

            base.Dispose();
            this._disposed = true;
        }
        #endregion 

        #endregion
    }
}
