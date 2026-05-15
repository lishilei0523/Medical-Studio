using Silk.NET.OpenCL;
using Silk.NET.OpenCL.Extensions.KHR;
using System;

namespace MedicalSharp.Inspiration.Resources
{
    /// <summary>
    /// OpenCL-1D图像
    /// </summary>
    /// <remarks>对应image1d_t，对标OpenGL Texture1D</remarks>
    public sealed class ClImage1D : ClImage
    {
        #region # 字段及构造器

        /// <summary>
        /// 创建OpenCL-1D图像构造器
        /// </summary>
        /// <param name="cl">OpenCL实例</param>
        /// <param name="glSharing">OpenGL扩展实例</param>
        /// <param name="handle">图像句柄</param>
        /// <param name="width">宽度</param>
        /// <param name="memoryFlags">内存标识</param>
        /// <param name="channelOrder">通道排序</param>
        /// <param name="channelType">通道类型</param>
        /// <param name="isFromGl">是否从OpenGL纹理创建</param>
        private ClImage1D(CL cl, KhrGlSharing glSharing, IntPtr handle, int width, MemFlags memoryFlags, ChannelOrder channelOrder, ChannelType channelType, bool isFromGl = false)
            : base(cl, glSharing, handle, width, 1, 1, memoryFlags, channelOrder, channelType, isFromGl)
        {

        }

        #endregion

        #region # 属性

        #region 只读属性 - 图像维度 —— override uint Dimension
        /// <summary>
        /// 只读属性 - 图像维度
        /// </summary>
        public override uint Dimension
        {
            get => 1;
        }
        #endregion

        #endregion

        #region # 方法

        #region 创建OpenCL-1D图像 —— static unsafe ClImage1D Create(ClContext clContext, int width...
        /// <summary>
        /// 创建OpenCL-1D图像
        /// </summary>
        /// <param name="clContext">OpenCL上下文</param>
        /// <param name="width">宽度</param>
        /// <param name="memoryFlags">内存标识</param>
        /// <param name="channelOrder">通道排序</param>
        /// <param name="channelType">通道类型</param>
        /// <returns>OpenCL-1D图像</returns>
        public static unsafe ClImage1D Create(ClContext clContext, int width, MemFlags memoryFlags = MemFlags.ReadWrite, ChannelOrder channelOrder = ChannelOrder.Rgba, ChannelType channelType = ChannelType.UnsignedInt8)
        {
            CL cl = CL.GetApi();

            ImageFormat imageFormat = new ImageFormat
            {
                ImageChannelOrder = channelOrder,
                ImageChannelDataType = channelType
            };
            ImageDesc imageDesc = new ImageDesc
            {
                ImageType = MemObjectType.Image1D,
                ImageWidth = (UIntPtr)width,
                ImageHeight = 1,
                ImageDepth = 1
            };

            IntPtr handle = cl.CreateImage(clContext.Handle, memoryFlags, in imageFormat, in imageDesc, null, out int errorCode);
            ClException.ThrowOnError(errorCode, "CreateImage1D");

            ClImage1D image = new ClImage1D(cl, null, handle, width, memoryFlags, channelOrder, channelType, false);

            return image;
        }
        #endregion

        #region 从1D纹理创建OpenCL-1D图像 —— static ClImage1D FromTexture1D(ClContext clContext...
        /// <summary>
        /// 从1D纹理创建OpenCL-1D图像
        /// </summary>
        /// <param name="clContext">OpenCL上下文</param>
        /// <param name="glTextureId">OpenGL纹理Id</param>
        /// <param name="width">宽度</param>
        /// <param name="memoryFlags">内存标识</param>
        /// <returns>OpenCL-1D图像</returns>
        public static ClImage1D FromTexture1D(ClContext clContext, int glTextureId, int width, MemFlags memoryFlags = MemFlags.ReadWrite)
        {
            CL cl = CL.GetApi();
            KhrGlSharing glSharing = new KhrGlSharing(cl.Context);

            IntPtr handle = glSharing.CreateFromGltexture(clContext.Handle, memoryFlags, (uint)GlObjectType.Texture1D, 0, (uint)glTextureId, out int errorCode);
            ClException.ThrowOnError(errorCode, "CreateFromGLTexture1D");

            ClImage1D image = new ClImage1D(cl, glSharing, handle, width, memoryFlags, 0, 0, true);

            return image;
        }
        #endregion 

        #endregion
    }
}
