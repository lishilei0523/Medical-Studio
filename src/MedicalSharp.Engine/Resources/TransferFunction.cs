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
            this._huMin = -1024;
            this._huMax = 3071;
            this._textureData = new Vector4[TextureWidth];
            this._controlPoints = new List<TFControlPoint>();
            this.InterpolationMode = InterpolationMode.Linear;
            this.Texture = new Texture1D(TextureWidth, PixelInternalFormat.Rgba32f, PixelFormat.Rgba, PixelType.Float);
        }

        #endregion

        #region # 属性

        #region HU最小值 —— short HUMin
        /// <summary>
        /// HU最小值
        /// </summary>
        private short _huMin;

        /// <summary>
        /// HU最小值
        /// </summary>
        public short HUMin
        {
            get => this._huMin;
            set
            {
                if (this._huMin != value && value < this._huMax)
                {
                    this._huMin = value;
                    this.UpdateControlPointsPositions();
                    this.UpdateTexture();
                }
            }
        }
        #endregion

        #region HU最大值 —— short HUMax
        /// <summary>
        /// HU最大值
        /// </summary>
        private short _huMax;

        /// <summary>
        /// HU最大值
        /// </summary>
        public short HUMax
        {
            get => this._huMax;
            set
            {
                if (this._huMax != value && value > this._huMin)
                {
                    this._huMax = value;
                    this.UpdateControlPointsPositions();
                    this.UpdateTexture();
                }
            }
        }
        #endregion

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

        #region 只读属性 - HU窗宽 —— int HUWindowWidth
        /// <summary>
        /// 只读属性 - HU窗宽
        /// </summary>
        public int HUWindowWidth
        {
            get => this._huMax - this._huMin;
        }
        #endregion

        #region 只读属性 - HU窗位 —— int HUWindowCenter
        /// <summary>
        /// 只读属性 - HU窗位
        /// </summary>
        public int HUWindowCenter
        {
            get => (this._huMin + this._huMax) / 2;
        }
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

        #region 设置HU范围 —— void SetHURange(short huMin, short huMax)
        /// <summary>
        /// 设置HU范围
        /// </summary>
        /// <param name="huMin">HU最小值</param>
        /// <param name="huMax">HU最大值</param>
        public void SetHURange(short huMin, short huMax)
        {
            #region # 验证

            if (huMin >= huMax)
            {
                throw new ArgumentException("HU最小值必须小于HU最大值！");
            }

            #endregion

            this._huMin = huMin;
            this._huMax = huMax;
            this.UpdateControlPointsPositions();
            this.UpdateTexture();
        }
        #endregion

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

            this.UpdateControlPointsPositions();
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
            this.UpdateControlPointsPositions();
            this.UpdateTexture();
        }
        #endregion

        #region 添加控制点 —— void AddControlPoint(short huValue, in Vector4 color)
        /// <summary>
        /// 添加控制点
        /// </summary>
        /// <param name="huValue">HU值</param>
        /// <param name="color">颜色</param>
        public void AddControlPoint(short huValue, in Vector4 color)
        {
            TFControlPoint controlPoint = new TFControlPoint(huValue, color);

            this._controlPoints.Add(controlPoint);
            this.UpdateControlPointsPositions();
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

        #region 归一化位置转HU值 —— short ToHU(float normalizedPosition)
        /// <summary>
        /// 归一化位置转HU值
        /// </summary>
        /// <param name="normalizedPosition">归一化位置(0~1)</param>
        /// <returns>HU值</returns>
        public short ToHU(float normalizedPosition)
        {
            short huValue = (short)Math.Ceiling(this._huMin + normalizedPosition * (this._huMax - this._huMin));

            return huValue;
        }
        #endregion

        #region HU值转归一化位置 —— float ToPosition(short huValue)
        /// <summary>
        /// HU值转归一化位置
        /// </summary>
        /// <param name="huValue">HU值</param>
        /// <returns>归一化位置(0~1)</returns>
        public float ToPosition(short huValue)
        {
            float position = Math.Clamp((huValue * 1.0f - this._huMin) / (this._huMax - this._huMin), 0.0f, 1.0f);

            return position;
        }
        #endregion

        #region 采样颜色 —— Vector4 SampleColor(short huValue)
        /// <summary>
        /// 采样颜色
        /// </summary>
        /// <param name="huValue">HU值</param>
        /// <returns>颜色</returns>
        public Vector4 SampleColor(short huValue)
        {
            float position = this.ToPosition(huValue);
            Vector4 color = this.InterpolateControlPoints(position);

            return color;
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

        #region 更新控制点位置 —— void UpdateControlPointsPositions()
        /// <summary>
        /// 更新控制点位置
        /// </summary>
        private void UpdateControlPointsPositions()
        {
            for (int index = 0; index < this._controlPoints.Count; index++)
            {
                TFControlPoint controlPoint = this._controlPoints[index];
                controlPoint.Position = this.ToPosition(controlPoint.HU);
                this._controlPoints[index] = controlPoint;
            }
        }
        #endregion

        #region 更新控制点HU值 —— void UpdateControlPointsHUs()
        /// <summary>
        /// 更新控制点HU值
        /// </summary>
        private void UpdateControlPointsHUs()
        {
            for (int index = 0; index < this._controlPoints.Count; index++)
            {
                TFControlPoint controlPoint = this._controlPoints[index];
                controlPoint.HU = this.ToHU(controlPoint.Position);
                this._controlPoints[index] = controlPoint;
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
