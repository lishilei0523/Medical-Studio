using Silk.NET.OpenCL;
using Silk.NET.OpenCL.Extensions.KHR;
using System;

namespace MedicalSharp.Inspiration.Resources
{
    /// <summary>
    /// OpenCL-3D图像
    /// </summary>
    /// <remarks>对应image3d_t，对标OpenGL Texture3D</remarks>
    public sealed class ClImage3D : ClImage
    {
        #region # 字段及构造器

        /// <summary>
        /// OpenGL 3D纹理常量
        /// </summary>
        private const uint GL_TEXTURE_3D = 0x806F;

        /// <summary>
        /// 创建OpenCL-3D图像构造器
        /// </summary>
        /// <param name="cl">OpenCL实例</param>
        /// <param name="glSharing">OpenGL扩展实例</param>
        /// <param name="handle">图像句柄</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="depth">深度</param>
        /// <param name="memoryFlags">内存标识</param>
        /// <param name="channelOrder">通道排序</param>
        /// <param name="channelType">通道类型</param>
        /// <param name="isFromGl">是否从OpenGL纹理创建</param>
        private ClImage3D(CL cl, KhrGlSharing glSharing, IntPtr handle, int width, int height, int depth, MemFlags memoryFlags, ChannelOrder channelOrder, ChannelType channelType, bool isFromGl = false)
            : base(cl, glSharing, handle, width, height, depth, memoryFlags, channelOrder, channelType, isFromGl)
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
            get => 3;
        }
        #endregion

        #endregion

        #region # 方法

        #region 创建OpenCL-3D图像 —— static ClImage3D Create(ClContext clContext, int width...
        /// <summary>
        /// 创建OpenCL-3D图像
        /// </summary>
        /// <param name="clContext">OpenCL上下文</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="depth">深度</param>
        /// <param name="memoryFlags">内存标识</param>
        /// <param name="channelOrder">通道排序</param>
        /// <param name="channelType">通道类型</param>
        /// <returns>OpenCL-3D图像</returns>
        public static unsafe ClImage3D Create(ClContext clContext, int width, int height, int depth, MemFlags memoryFlags = MemFlags.ReadWrite, ChannelOrder channelOrder = ChannelOrder.Rgba, ChannelType channelType = ChannelType.UnsignedInt8)
        {
            CL cl = CL.GetApi();

            ImageFormat imageFormat = new ImageFormat
            {
                ImageChannelOrder = channelOrder,
                ImageChannelDataType = channelType
            };
            ImageDesc imageDesc = new ImageDesc
            {
                ImageType = MemObjectType.Image3D,
                ImageWidth = (UIntPtr)width,
                ImageHeight = (UIntPtr)height,
                ImageDepth = (UIntPtr)depth
            };

            IntPtr handle = cl.CreateImage(clContext.Handle, memoryFlags, in imageFormat, in imageDesc, null, out int errorCode);
            ClException.ThrowOnError(errorCode, "CreateImage3D");

            ClImage3D image = new ClImage3D(cl, null, handle, width, height, depth, memoryFlags, channelOrder, channelType, false);

            return image;
        }
        #endregion

        #region 从3D纹理创建OpenCL-3D图像 —— static ClImage3D FromTexture3D(ClContext clContext...
        /// <summary>
        /// 从3D纹理创建OpenCL-3D图像
        /// </summary>
        /// <param name="clContext">OpenCL上下文</param>
        /// <param name="glTextureId">OpenGL纹理ID</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="depth">深度</param>
        /// <param name="memoryFlags">内存标识</param>
        /// <returns>OpenCL-3D图像</returns>
        public static ClImage3D FromTexture3D(ClContext clContext, int glTextureId, int width, int height, int depth, MemFlags memoryFlags = MemFlags.ReadWrite)
        {
            CL cl = CL.GetApi();
            KhrGlSharing glSharing = new KhrGlSharing(cl.Context);

            IntPtr handle = glSharing.CreateFromGltexture3D(clContext.Handle, memoryFlags, GL_TEXTURE_3D, 0, (uint)glTextureId, out int errorCode);
            ClException.ThrowOnError(errorCode, "CreateFromGLTexture3D");

            ClImage3D image = new ClImage3D(cl, glSharing, handle, width, height, depth, memoryFlags, 0, 0, true);

            return image;
        }
        #endregion

        #endregion
    }
}
