using MedicalSharp.Primitives.Enums;
using MedicalSharp.Primitives.Models;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MedicalSharp.Engine.Resources
{
    /// <summary>
    /// 传递函数
    /// </summary>
    public class TransferFunction : IDisposable
    {
        #region # 字段及构造器

        /// <summary>
        /// 纹理宽度
        /// </summary>
        private const int TextureWidth = 256;

        /// <summary>
        /// 释放标识
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// 纹理数据
        /// </summary>
        private readonly Vector4[] _textureData;

        /// <summary>
        /// 控制点列表
        /// </summary>
        private readonly List<TFControlPoint> _controlPoints;

        /// <summary>
        /// 创建传递函数构造器
        /// </summary>
        internal TransferFunction()
        {
            this._textureData = new Vector4[TextureWidth];
            this._controlPoints = new List<TFControlPoint>();
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

        #region 只读属性 - 控制点列表 —— IReadOnlyList<TFControlPoint> ControlPoints
        /// <summary>
        /// 只读属性 - 控制点列表
        /// </summary>
        public IReadOnlyList<TFControlPoint> ControlPoints
        {
            get => this._controlPoints;
        }
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
            this.InterpolationMode = interpolationMode;
            this.UpdateTexture();
        }
        #endregion

        #region 从控制点列表初始化 —— void InitFromControlPoints(IReadOnlyList<TFControlPoint>...
        /// <summary>
        /// 从控制点列表初始化
        /// </summary>
        /// <param name="controlPoints">控制点列表</param>
        public void InitFromControlPoints(IReadOnlyList<TFControlPoint> controlPoints)
        {
            #region # 验证

            if (controlPoints == null || !controlPoints.Any())
            {
                throw new ArgumentNullException(nameof(controlPoints), "控制点列表不可为空！");
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
            if (this._disposed)
            {
                return;
            }

            this._controlPoints.Clear();
            this.Texture?.Dispose();
            this._disposed = true;
        }
        #endregion


        //Private

        #region 更新纹理 —— void UpdateTexture()
        /// <summary>
        /// 更新纹理
        /// </summary>
        private unsafe void UpdateTexture()
        {
            #region # 验证

            if (!this._controlPoints.Any())
            {
                return;
            }

            #endregion

            //确保控制点按位置排序
            this._controlPoints.Sort((a, b) => a.Position.CompareTo(b.Position));

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

        #region 插值控制点 —— Vector4 InterpolateControlPoints(float position)
        /// <summary>
        /// 插值控制点
        /// </summary>
        /// <param name="position">位置(0~1)</param>
        /// <returns>颜色</returns>
        private Vector4 InterpolateControlPoints(float position)
        {
            #region # 验证

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

            #endregion

            for (int index = 0; index < this._controlPoints.Count - 1; index++)
            {
                float position1 = this._controlPoints[index].Position;
                float position2 = this._controlPoints[index + 1].Position;
                Vector4 color1 = this._controlPoints[index].Color;
                Vector4 color2 = this._controlPoints[index + 1].Color;
                if ((position >= position1 && position <= position2) || (position >= position2 && position <= position1))
                {
                    float t = (position - position1) / (position2 - position1);

                    //根据插值模式调整t值
                    t = this.InterpolationMode switch
                    {
                        InterpolationMode.Linear => t,
                        InterpolationMode.Step => t < 0.5f ? 0.0f : 1.0f,
                        InterpolationMode.SmoothStep => t * t * (3 - 2 * t), //Hermite平滑
                        _ => t
                    };

                    //颜色插值
                    Vector4 color = Vector4.Lerp(color1, color2, t);

                    return color;
                }
            }

            return new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
        }
        #endregion 

        #endregion
    }
}
