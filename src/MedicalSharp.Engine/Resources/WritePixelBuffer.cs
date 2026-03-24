using OpenTK.Graphics.OpenGL4;
using System;
using System.Runtime.InteropServices;

namespace MedicalSharp.Engine.Resources
{
    /// <summary>
    /// 像素缓冲区(写)
    /// </summary>
    /// <remarks>CPU -> GPU</remarks>
    public abstract class WritePixelBuffer : PixelBuffer
    {
        #region # 字段及构造器

        /// <summary>
        /// 创建像素缓冲区(写)构造器
        /// </summary>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="pixelFormat">像素格式</param>
        /// <param name="pixelType">像素类型</param>
        protected WritePixelBuffer(int width, int height, PixelFormat pixelFormat, PixelType pixelType)
            : base(width, height, pixelFormat, pixelType)
        {

        }

        #endregion

        #region # 属性

        #region 只读属性 - 缓冲区目标 —— override BufferTarget BufferTarget
        /// <summary>
        /// 只读属性 - 缓冲区目标
        /// </summary>
        protected override BufferTarget BufferTarget
        {
            get => BufferTarget.PixelUnpackBuffer;
        }
        #endregion

        #region 只读属性 - 缓冲区用途 —— override BufferUsageHint BufferUsage
        /// <summary>
        /// 只读属性 - 缓冲区用途
        /// </summary>
        protected override BufferUsageHint BufferUsage
        {
            get => BufferUsageHint.StreamDraw;
        }
        #endregion

        #endregion

        #region # 方法

        #region 上传byte数组 —— virtual void UploadData(byte[] data)
        /// <summary>
        /// 上传byte数组
        /// </summary>
        public virtual void UploadData(byte[] data)
        {
            #region # 验证

            if (data == null)
            {
                throw new ArgumentNullException(nameof(data), "数据不可为空！");
            }
            if (data.Length != this.BufferSize)
            {
                throw new ArgumentOutOfRangeException($"数据尺寸\"{data.Length}\"与缓冲区尺寸\"{this.BufferSize}\"不匹配");
            }

            #endregion

            this.Bind();

            IntPtr ptr = GL.MapBuffer(this.BufferTarget, BufferAccess.WriteOnly);
            if (ptr != IntPtr.Zero)
            {
                Marshal.Copy(data, 0, ptr, data.Length);
                GL.UnmapBuffer(this.BufferTarget);
            }

            this.Unbind();
        }
        #endregion

        #region 上传short数组 —— virtual void UploadData(short[] data)
        /// <summary>
        /// 上传short数组
        /// </summary>
        public virtual void UploadData(short[] data)
        {
            #region # 验证

            if (data == null)
            {
                throw new ArgumentNullException(nameof(data), "数据不可为空！");
            }

            int byteSize = data.Length * sizeof(short);
            if (byteSize != this.BufferSize)
            {
                throw new ArgumentOutOfRangeException($"数据尺寸\"{byteSize}\"与缓冲区尺寸\"{this.BufferSize}\"不匹配");
            }

            #endregion

            this.Bind();

            IntPtr ptr = GL.MapBuffer(this.BufferTarget, BufferAccess.WriteOnly);
            if (ptr != IntPtr.Zero)
            {
                Marshal.Copy(data, 0, ptr, data.Length);
                GL.UnmapBuffer(this.BufferTarget);
            }

            this.Unbind();
        }
        #endregion

        #region 上传int数组 —— virtual void UploadData(int[] data)
        /// <summary>
        /// 上传int数组
        /// </summary>
        public virtual void UploadData(int[] data)
        {
            #region # 验证

            if (data == null)
            {
                throw new ArgumentNullException(nameof(data), "数据不可为空！");
            }

            int byteSize = data.Length * sizeof(int);
            if (byteSize != this.BufferSize)
            {
                throw new ArgumentOutOfRangeException($"数据尺寸\"{byteSize}\"与缓冲区尺寸\"{this.BufferSize}\"不匹配");
            }

            #endregion

            this.Bind();

            IntPtr ptr = GL.MapBuffer(this.BufferTarget, BufferAccess.WriteOnly);
            if (ptr != IntPtr.Zero)
            {
                Marshal.Copy(data, 0, ptr, data.Length);
                GL.UnmapBuffer(this.BufferTarget);
            }

            this.Unbind();
        }
        #endregion

        #region 上传float数组 —— virtual void UploadData(float[] data)
        /// <summary>
        /// 上传float数组
        /// </summary>
        public virtual void UploadData(float[] data)
        {
            #region # 验证

            if (data == null)
            {
                throw new ArgumentNullException(nameof(data), "数据不可为空！");
            }

            int byteSize = data.Length * sizeof(float);
            if (byteSize != this.BufferSize)
            {
                throw new ArgumentOutOfRangeException($"数据尺寸\"{byteSize}\"与缓冲区尺寸\"{this.BufferSize}\"不匹配");
            }

            #endregion

            this.Bind();

            IntPtr ptr = GL.MapBuffer(this.BufferTarget, BufferAccess.WriteOnly);
            if (ptr != IntPtr.Zero)
            {
                Marshal.Copy(data, 0, ptr, data.Length);
                GL.UnmapBuffer(this.BufferTarget);
            }

            this.Unbind();
        }
        #endregion

        #endregion
    }
}
