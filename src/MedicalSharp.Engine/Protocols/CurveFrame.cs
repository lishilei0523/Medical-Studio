using MedicalSharp.Engine.Resources;
using MedicalSharp.Primitives.Maths;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Linq;

namespace MedicalSharp.Engine.Protocols
{
    /// <summary>
    /// 曲线框架
    /// </summary>
    /// <remarks>将Curve的Frenet框架数据上传为1D纹理，供Shader采样</remarks>
    public sealed class CurveFrame : IDisposable
    {
        #region # 字段及构造器

        /// <summary>
        /// 释放标识
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// 位置数据
        /// </summary>
        private Vector4[] _positionData;

        /// <summary>
        /// 切线数据
        /// </summary>
        private Vector4[] _tangentData;

        /// <summary>
        /// 法向量数据
        /// </summary>
        private Vector4[] _normalData;

        /// <summary>
        /// 副法向量数据
        /// </summary>
        private Vector4[] _binormalData;

        /// <summary>
        /// 创建曲线框架构造器
        /// </summary>
        /// <param name="curve">曲线</param>
        internal CurveFrame(Curve curve)
        {
            #region # 验证

            if (curve == null || !curve.FrenetFrames.Any())
            {
                throw new ArgumentNullException(nameof(curve), "曲线不可为空且必须包含Frenet框架！");
            }

            #endregion

            this.FramesCount = curve.FrenetFrames.Length;
            this.CreateTextures();
            this.FillTextures(curve);
        }

        #endregion

        #region # 属性

        #region 框架数量 —— int FramesCount
        /// <summary>
        /// 框架数量
        /// </summary>
        public int FramesCount { get; private set; }
        #endregion

        #region 位置纹理 —— Texture1D PositionTexture
        /// <summary>
        /// 位置纹理
        /// </summary>
        /// <remarks>RGB=Position, A=弧长归一化值</remarks>
        internal Texture1D PositionTexture { get; private set; }
        #endregion

        #region 切线纹理 —— Texture1D TangentTexture
        /// <summary>
        /// 切线纹理
        /// </summary>
        /// <remarks>RGB=Tangent, A=0</remarks>
        internal Texture1D TangentTexture { get; private set; }
        #endregion

        #region 法向量纹理 —— Texture1D NormalTexture
        /// <summary>
        /// 法向量纹理
        /// </summary>
        /// <remarks>RGB=Normal, A=0</remarks>
        internal Texture1D NormalTexture { get; private set; }
        #endregion

        #region 副法向量纹理 —— Texture1D BinormalTexture
        /// <summary>
        /// 副法向量纹理
        /// </summary>
        /// <remarks>RGB=Binormal, A=0</remarks>
        internal Texture1D BinormalTexture { get; private set; }
        #endregion

        #endregion

        #region # 方法

        //Public

        #region 更新曲线框架 —— void Update(Curve curve)
        /// <summary>
        /// 更新曲线框架
        /// </summary>
        /// <param name="curve">曲线</param>
        public void Update(Curve curve)
        {
            #region # 验证

            if (curve == null || !curve.FrenetFrames.Any())
            {
                throw new ArgumentNullException(nameof(curve), "曲线不可为空且必须包含Frenet框架！");
            }

            #endregion

            //框架数量变化时重建纹理
            if (curve.FrenetFrames.Length != this.FramesCount)
            {
                this.FramesCount = curve.FrenetFrames.Length;
                this.ReleaseTextures();
                this.CreateTextures();
            }

            this.FillTextures(curve);
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

            this.ReleaseTextures();
            this._disposed = true;
        }
        #endregion


        //Private

        #region 创建纹理 —— void CreateTextures()
        /// <summary>
        /// 创建纹理
        /// </summary>
        private void CreateTextures()
        {
            this._positionData = new Vector4[this.FramesCount];
            this._tangentData = new Vector4[this.FramesCount];
            this._normalData = new Vector4[this.FramesCount];
            this._binormalData = new Vector4[this.FramesCount];

            //初始化纹理
            this.PositionTexture = new Texture1D(this.FramesCount, PixelInternalFormat.Rgba32f, PixelFormat.Rgba, PixelType.Float);
            this.TangentTexture = new Texture1D(this.FramesCount, PixelInternalFormat.Rgba32f, PixelFormat.Rgba, PixelType.Float);
            this.NormalTexture = new Texture1D(this.FramesCount, PixelInternalFormat.Rgba32f, PixelFormat.Rgba, PixelType.Float);
            this.BinormalTexture = new Texture1D(this.FramesCount, PixelInternalFormat.Rgba32f, PixelFormat.Rgba, PixelType.Float);

            //框架之间线性插值
            this.PositionTexture.SetFilter(TextureMinFilter.Linear, TextureMagFilter.Linear);
            this.TangentTexture.SetFilter(TextureMinFilter.Linear, TextureMagFilter.Linear);
            this.NormalTexture.SetFilter(TextureMinFilter.Linear, TextureMagFilter.Linear);
            this.BinormalTexture.SetFilter(TextureMinFilter.Linear, TextureMagFilter.Linear);
        }
        #endregion

        #region 填充纹理 —— void FillTextures(Curve curve)
        /// <summary>
        /// 填充纹理
        /// </summary>
        /// <param name="curve">曲线</param>
        private unsafe void FillTextures(Curve curve)
        {
            for (int index = 0; index < this.FramesCount; index++)
            {
                //弧长归一化
                float normalizedArcLength = curve.TotalArcLength > 0
                    ? (index * 1.0f / (this.FramesCount - 1))
                    : 0f;

                FrenetFrame frame = curve.FrenetFrames[index];
                this._positionData[index] = new Vector4(frame.Position.X, frame.Position.Y, frame.Position.Z, normalizedArcLength);
                this._tangentData[index] = new Vector4(frame.Tangent.X, frame.Tangent.Y, frame.Tangent.Z, 0f);
                this._normalData[index] = new Vector4(frame.Normal.X, frame.Normal.Y, frame.Normal.Z, 0f);
                this._binormalData[index] = new Vector4(frame.Binormal.X, frame.Binormal.Y, frame.Binormal.Z, 0f);
            }

            //上传纹理
            fixed (void* positionPtr = this._positionData)
            {
                this.PositionTexture.Update(new IntPtr(positionPtr));
            }
            fixed (void* tangentPtr = this._tangentData)
            {
                this.TangentTexture.Update(new IntPtr(tangentPtr));
            }
            fixed (void* normalPtr = this._normalData)
            {
                this.NormalTexture.Update(new IntPtr(normalPtr));
            }
            fixed (void* binormalPtr = this._binormalData)
            {
                this.BinormalTexture.Update(new IntPtr(binormalPtr));
            }
        }
        #endregion

        #region 释放纹理 —— void ReleaseTextures()
        /// <summary>
        /// 释放纹理
        /// </summary>
        private void ReleaseTextures()
        {
            this.PositionTexture?.Dispose();
            this.TangentTexture?.Dispose();
            this.NormalTexture?.Dispose();
            this.BinormalTexture?.Dispose();
        }
        #endregion

        #endregion
    }
}
