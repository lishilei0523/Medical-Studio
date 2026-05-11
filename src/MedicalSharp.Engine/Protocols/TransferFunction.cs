using MedicalSharp.Engine.Resources;
using MedicalSharp.Primitives.Enums;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;

namespace MedicalSharp.Engine.Protocols
{
    /// <summary>
    /// 传递函数
    /// </summary>
    public abstract class TransferFunction : IDisposable
    {
        #region # 字段及构造器

        /// <summary>
        /// 纹理宽度
        /// </summary>
        protected const int TextureWidth = 256;

        /// <summary>
        /// 释放标识
        /// </summary>
        protected bool _disposed;

        /// <summary>
        /// 纹理数据
        /// </summary>
        protected readonly Vector4[] _textureData;

        /// <summary>
        /// 创建传递函数构造器
        /// </summary>
        protected TransferFunction()
        {
            this._textureData = new Vector4[TextureWidth];
            this.InterpolationMode = InterpolationMode.Linear;
            this.Texture = new Texture1D(TextureWidth, PixelInternalFormat.Rgba32f, PixelFormat.Rgba, PixelType.Float);
        }

        #endregion

        #region # 属性

        #region 插值模式 —— InterpolationMode InterpolationMode
        /// <summary>
        /// 插值模式
        /// </summary>
        public InterpolationMode InterpolationMode { get; private set; }
        #endregion

        #region 传递函数纹理 —— Texture1D Texture
        /// <summary>
        /// 传递函数纹理
        /// </summary>
        internal Texture1D Texture { get; private set; }
        #endregion

        #endregion

        #region # 方法

        //Public

        #region 切换插值模式 —— void SwitchInterpolationMode(InterpolationMode interpolationMode)
        /// <summary>
        /// 切换插值模式
        /// </summary>
        /// <param name="interpolationMode">插值模式</param>
        public void SwitchInterpolationMode(InterpolationMode interpolationMode)
        {
            #region # 验证

            if (this.InterpolationMode == interpolationMode)
            {
                return;
            }

            #endregion

            this.InterpolationMode = interpolationMode;
            this.UpdateTexture();
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

            this.ReleaseManagedResources();
            this.Texture?.Dispose();
            this._disposed = true;
        }
        #endregion


        //Protected

        #region 更新纹理 —— virtual void UpdateTexture()
        /// <summary>
        /// 更新纹理
        /// </summary>
        protected virtual unsafe void UpdateTexture()
        {
            #region # 验证

            if (this.GetControlPointsCount() == 0)
            {
                return;
            }

            #endregion

            for (int index = 0; index < TextureWidth; index++)
            {
                float position = index * 1.0f / (TextureWidth - 1);
                this._textureData[index] = this.InterpolateControlPoints(position);
            }

            fixed (void* pointer = this._textureData)
            {
                this.Texture.Update(new IntPtr(pointer));
            }
        }
        #endregion

        #region 获取控制点数量 —— abstract int GetControlPointsCount()
        /// <summary>
        /// 获取控制点数量
        /// </summary>
        protected abstract int GetControlPointsCount();
        #endregion

        #region 插值控制点 —— abstract Vector4 InterpolateControlPoints(float position)
        /// <summary>
        /// 插值控制点
        /// </summary>
        protected abstract Vector4 InterpolateControlPoints(float position);
        #endregion

        #region 释放托管资源 —— abstract void ReleaseManagedResources()
        /// <summary>
        /// 释放托管资源
        /// </summary>
        protected abstract void ReleaseManagedResources();
        #endregion

        #endregion
    }
}
