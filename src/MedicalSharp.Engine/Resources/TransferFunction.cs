using MedicalSharp.Engine.ValueTypes;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MedicalSharp.Engine.Resources
{
    /// <summary>
    /// 传输函数
    /// </summary>
    public class TransferFunction : IDisposable
    {
        #region # 字段及构造器

        /// <summary>
        /// 纹理宽度
        /// </summary>
        private const int TextureWidth = 256;

        /// <summary>
        /// 纹理数据
        /// </summary>
        private readonly Vector4[] _textureData;

        /// <summary>
        /// 控制点列表
        /// </summary>
        private readonly IList<TFControlPoint> _controlPoints;

        /// <summary>
        /// 创建传输含税构造器
        /// </summary>
        internal TransferFunction()
        {
            this._textureData = new Vector4[TextureWidth];
            this._controlPoints = new List<TFControlPoint>();
            this.Texture1D = new Texture1D(TextureWidth, PixelInternalFormat.Rgba32f);
        }

        #endregion

        #region # 属性

        #region 1D纹理 —— Texture1D Texture1D
        /// <summary>
        /// 1D纹理
        /// </summary>
        internal Texture1D Texture1D { get; private set; }

        #endregion

        #endregion

        #region # 方法

        //Public

        #region 从控制点集初始化 —— void InitFromControlPoints(IEnumerable<TFControlPoint>...
        /// <summary>
        /// 从控制点集初始化
        /// </summary>
        /// <param name="controlPoints">控制点集</param>
        public void InitFromControlPoints(IEnumerable<TFControlPoint> controlPoints)
        {
            #region # 验证

            controlPoints = controlPoints?.ToArray() ?? [];
            if (controlPoints == null || !controlPoints.Any())
            {
                throw new ArgumentNullException(nameof(controlPoints), "控制点集不可为空！");
            }

            #endregion

            this._controlPoints.Clear();
            foreach (TFControlPoint controlPoint in controlPoints)
            {
                this._controlPoints.Add(controlPoint);
            }

            this.UpdateTexture();
        }
        #endregion

        #region 添加控制点 —— void AddControlPoint(in TFControlPoint controlPoint)
        /// <summary>
        /// 添加控制点
        /// </summary>
        /// <param name="controlPoint">控制点</param>
        public void AddControlPoint(in TFControlPoint controlPoint)
        {
            this._controlPoints.Add(controlPoint);
            this.UpdateTexture();
        }
        #endregion

        #region 删除控制点 —— void RemoveControlPoint(in TFControlPoint controlPoint)
        /// <summary>
        /// 删除控制点
        /// </summary>
        /// <param name="controlPoint">控制点</param>
        public void RemoveControlPoint(in TFControlPoint controlPoint)
        {
            this._controlPoints.Remove(controlPoint);
            this.UpdateTexture();
        }
        #endregion

        #region 清空控制点 —— void ClearControlPoints()
        /// <summary>
        /// 清空控制点
        /// </summary>
        public void ClearControlPoints()
        {
            this._controlPoints.Clear();
            this.UpdateTexture();
        }
        #endregion

        #region 释放资源 —— void Dispose()
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            this._controlPoints.Clear();
            this.Texture1D?.Dispose();
        }
        #endregion


        //Private

        #region 更新纹理 —— unsafe void UpdateTexture()
        /// <summary>
        /// 更新纹理
        /// </summary>
        private unsafe void UpdateTexture()
        {
            if (!this._controlPoints.Any())
            {
                return;
            }

            for (int index = 0; index < TextureWidth; index++)
            {
                float position = index * 1.0f / (TextureWidth - 1);
                this._textureData[index] = this.InterpolateControlPoints(position);
            }

            fixed (void* pointer = this._textureData)
            {
                this.Texture1D.Update(PixelFormat.Rgba, PixelType.Float, new IntPtr(pointer));
            }
        }
        #endregion

        #region 插值控制点 —— Vector4 InterpolateControlPoints(float position)
        /// <summary>
        /// 插值控制点
        /// </summary>
        /// <param name="position">位置(0~1)</param>
        /// <returns>颜色</returns>
        private Vector4 InterpolateControlPoints(float position)
        {
            if (this._controlPoints.Count == 0)
            {
                return new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
            }

            if (position <= this._controlPoints[0].Position)
            {
                return this._controlPoints[0].Color;
            }

            if (position >= this._controlPoints[^1].Position)
            {
                return this._controlPoints[^1].Color;
            }

            for (int index = 0; index < this._controlPoints.Count - 1; index++)
            {
                if (position >= this._controlPoints[index].Position && position <= this._controlPoints[index + 1].Position)
                {
                    //颜色插值
                    float t = (position - this._controlPoints[index].Position) /
                              (this._controlPoints[index + 1].Position - this._controlPoints[index].Position);
                    Vector4 result = Vector4.Lerp(this._controlPoints[index].Color, this._controlPoints[index + 1].Color, t);

                    return new Vector4(result.X, result.Y, result.Z, result.W);
                }
            }

            return new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
        }
        #endregion 

        #endregion
    }
}
