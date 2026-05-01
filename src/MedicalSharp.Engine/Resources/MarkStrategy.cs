using MedicalSharp.Primitives.Builders;
using MedicalSharp.Primitives.Enums;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace MedicalSharp.Engine.Resources
{
    /// <summary>
    /// 标记策略
    /// </summary>
    public sealed class MarkStrategy : IDisposable
    {
        #region # 字段及构造器

        /// <summary>
        /// 标记长度
        /// </summary>
        private const int MarkLength = 256;

        /// <summary>
        /// 释放标识
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// 标记颜色列表
        /// </summary>
        private readonly Vector4[] _markColors;

        /// <summary>
        /// 标记模式列表
        /// </summary>
        private readonly MarkMode[] _markModes;

        /// <summary>
        /// 创建标记策略构造器
        /// </summary>
        internal MarkStrategy()
        {
            this._markColors = new Vector4[MarkLength];
            this._markModes = new MarkMode[MarkLength];

            //默认Mark模式
            for (int index = 0; index < MarkLength; index++)
            {
                this._markModes[index] = MarkMode.Visible;
            }

            //初始化纹理
            this.Texture = new Texture1D(MarkLength, PixelInternalFormat.Rgba32f, PixelFormat.Rgba, PixelType.Float);
            this.Texture.SetFilter(TextureMinFilter.Linear, TextureMagFilter.Linear);

            //标准颜色
            this.Opacity = 0.6f;
            this.ApplyStandardMarkColors(this.Opacity);
        }

        #endregion

        #region # 属性

        #region 透明度 —— float Opacity
        /// <summary>
        /// 透明度
        /// </summary>
        /// <remarks>值域：0~1</remarks>
        public float Opacity { get; private set; }
        #endregion

        #region 标记颜色纹理 —— Texture1D Texture
        /// <summary>
        /// 标记颜色纹理
        /// </summary>
        internal Texture1D Texture { get; private set; }
        #endregion

        #region 只读属性 - 标记模式列表 —— IReadOnlyList<MarkMode> MarkModes
        /// <summary>
        /// 只读属性 - 标记模式列表
        /// </summary>
        public IReadOnlyList<MarkMode> MarkModes
        {
            get => this._markModes;
        }
        #endregion

        #endregion

        #region # 方法

        //Public

        #region 切换标记模式 —— void SwitchMarkMode(byte markValue, MarkMode markMode)
        /// <summary>
        /// 切换标记模式
        /// </summary>
        /// <param name="markValue">标记值</param>
        /// <param name="markMode">标记模式</param>
        public void SwitchMarkMode(byte markValue, MarkMode markMode)
        {
            this._markModes[markValue] = markMode;
        }
        #endregion

        #region 适用默认标记颜色 —— void ApplyDefaultMarkColors(float opacity = 0.6f)
        /// <summary>
        /// 适用默认标记颜色
        /// </summary>
        /// <param name="opacity">透明度</param>
        public void ApplyDefaultMarkColors(float opacity = 0.6f)
        {
            this.Opacity = opacity;

            Vector4[] colors = ColorFactory.GetDefaultMarkColors(opacity);
            Array.Copy(colors, this._markColors, MarkLength);
            this.UpdateTexture();
        }
        #endregion

        #region 适用标准标记颜色 —— void ApplyStandardMarkColors(float opacity = 0.6f)
        /// <summary>
        /// 适用标准标记颜色
        /// </summary>
        /// <param name="opacity">透明度</param>
        public void ApplyStandardMarkColors(float opacity = 0.6f)
        {
            this.Opacity = opacity;

            Vector4[] colors = ColorFactory.GetStandardMarkColors(opacity);
            Array.Copy(colors, this._markColors, MarkLength);
            this.UpdateTexture();
        }
        #endregion

        #region 设置透明度 —— void SetOpacity(float opacity)
        /// <summary>
        /// 设置透明度
        /// </summary>
        /// <param name="opacity">透明度</param>
        public void SetOpacity(float opacity)
        {
            this.Opacity = Math.Clamp(opacity, 0.01f, 1);

            for (int index = 0; index < MarkLength; index++)
            {
                Vector4 color = this._markColors[index];
                this._markColors[index] = new Vector4(color.X, color.Y, color.Z, this.Opacity);
            }

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

            this.Texture?.Dispose();
            this._disposed = true;
        }
        #endregion


        //Private

        #region 更新纹理 —— unsafe void UpdateTexture()
        /// <summary>
        /// 更新纹理
        /// </summary>
        private unsafe void UpdateTexture()
        {
            #region # 验证

            if (this._markColors == null)
            {
                return;
            }

            #endregion

            fixed (void* pointer = this._markColors)
            {
                this.Texture.Update(new IntPtr(pointer));
            }
        }
        #endregion

        #endregion
    }
}
