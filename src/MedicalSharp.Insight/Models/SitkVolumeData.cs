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

        #endregion

        #region # 方法

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
            this._disposed = true;
        }
        #endregion 

        #endregion
    }
}
