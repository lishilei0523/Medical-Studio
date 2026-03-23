using OpenTK.Graphics.OpenGL4;
using System;
using System.Runtime.InteropServices;

namespace MedicalSharp.Engine.Resources
{
    /// <summary>
    /// 像素缓冲区(写)
    /// </summary>
    /// <remarks>CPU -> GPU</remarks>
    public class WritePixelBuffer : PixelBuffer
    {
        //TODO 重构，支持多种像素类型

        #region # 字段及构造器

        /// <summary>
        /// 创建像素缓冲区(写)构造器
        /// </summary>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="pixelFormat">像素格式</param>
        /// <param name="pixelType">像素类型</param>
        public WritePixelBuffer(int width, int height, PixelFormat pixelFormat = PixelFormat.Rgba,
            PixelType pixelType = PixelType.UnsignedByte)
            : base(width, height, pixelFormat)
        {
            base.CreateBuffer();
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

        /// <summary>
        /// 上传数据到 PBO
        /// </summary>
        public void UploadData(byte[] data)
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

        /// <summary>
        /// 上传结构体数据到PBO
        /// </summary>
        public unsafe void UploadData<T>(T data) where T : unmanaged
        {
            int size = sizeof(T);

            #region # 验证

            if (size != this.BufferSize)
            {
                throw new ArgumentOutOfRangeException($"数据尺寸\"{size}\"与缓冲区尺寸\"{this.BufferSize}\"不匹配");
            }

            #endregion

            this.Bind();

            IntPtr ptr = GL.MapBuffer(this.BufferTarget, BufferAccess.WriteOnly);
            if (ptr != IntPtr.Zero)
            {
                Marshal.StructureToPtr(data, ptr, false);
                GL.UnmapBuffer(this.BufferTarget);
            }

            this.Unbind();
        }

        /// <summary>
        /// 上传数据到2D纹理
        /// </summary>
        public void UploadToTexture(Texture2D texture, bool useFence = true)
        {
            texture.Bind();
            this.Bind();

            //从PBO传输到纹理（GPU异步执行）
            GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, this.Width, this.Height, this.PixelFormat, PixelType.UnsignedByte, IntPtr.Zero);

            if (useFence)
            {
                this.CreateFence();
            }

            this.Unbind();
            texture.Unbind();
        }

        /// <summary>
        /// 上传数据到3D纹理
        /// </summary>
        public void UploadToTexture(Texture3D texture, int depth, bool useFence = true)
        {
            texture.Bind();
            this.Bind();

            GL.TexSubImage3D(TextureTarget.Texture3D, 0, 0, 0, 0, this.Width, this.Height, depth, this.PixelFormat, PixelType.UnsignedByte, IntPtr.Zero);

            if (useFence)
            {
                this.CreateFence();
            }

            this.Unbind();
            texture.Unbind();
        }

        /// <summary>
        /// 清空缓冲区
        /// </summary>
        public void Clear()
        {
            this.Bind();

            IntPtr ptr = GL.MapBuffer(this.BufferTarget, BufferAccess.WriteOnly);
            if (ptr != IntPtr.Zero)
            {
                unsafe
                {
                    byte* p = (byte*)ptr.ToPointer();
                    for (int i = 0; i < this.BufferSize; i++)
                    {
                        p[i] = 0;
                    }
                }
                GL.UnmapBuffer(this.BufferTarget);
            }

            this.Unbind();
        }

        #endregion
    }
}
