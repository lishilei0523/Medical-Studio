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
        public TransferFunction()
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
        public Texture1D Texture1D { get; private set; }

        #endregion

        #endregion

        #region # 方法

        //Public

        #region 添加控制点 —— void AddControlPoint(TFControlPoint controlPoint)
        /// <summary>
        /// 添加控制点
        /// </summary>
        /// <param name="controlPoint">控制点</param>
        public void AddControlPoint(TFControlPoint controlPoint)
        {
            this._controlPoints.Add(controlPoint);
            this.UpdateTexture();
        }
        #endregion

        #region 删除控制点 —— void RemoveControlPoint(TFControlPoint controlPoint)
        /// <summary>
        /// 删除控制点
        /// </summary>
        /// <param name="controlPoint">控制点</param>
        public void RemoveControlPoint(TFControlPoint controlPoint)
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

        #region 初始化灰度预设 —— void InitializeGrayPreset()
        /// <summary>
        /// 初始化灰度预设
        /// </summary>
        public void InitializeGrayPreset()
        {
            this._controlPoints.Clear();
            this._controlPoints.Add(new TFControlPoint(0.0f, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)));
            this._controlPoints.Add(new TFControlPoint(1.0f, new Vector4(1.0f, 1.0f, 1.0f, 1.0f)));

            this.UpdateTexture();
        }
        #endregion

        #region 初始化彩虹预设 —— void InitializeRainbowPreset()
        /// <summary>
        /// 初始化彩虹预设
        /// </summary>
        public void InitializeRainbowPreset()
        {
            this._controlPoints.Clear();
            this._controlPoints.Add(new TFControlPoint(0.0f, new Vector4(0.0f, 0.0f, 0.5f, 0.0f)));
            this._controlPoints.Add(new TFControlPoint(0.25f, new Vector4(0.0f, 0.5f, 1.0f, 0.3f)));
            this._controlPoints.Add(new TFControlPoint(0.5f, new Vector4(0.0f, 1.0f, 0.5f, 0.6f)));
            this._controlPoints.Add(new TFControlPoint(0.75f, new Vector4(1.0f, 1.0f, 0.0f, 0.8f)));
            this._controlPoints.Add(new TFControlPoint(1.0f, new Vector4(1.0f, 0.0f, 0.0f, 1.0f)));

            this.UpdateTexture();
        }
        #endregion

        #region 初始化骨骼预设 —— void InitializeBonePreset()
        /// <summary>
        /// 初始化骨骼预设
        /// </summary>
        public void InitializeBonePreset()
        {
            this._controlPoints.Clear();

            //完全透明背景（空气/背景）
            this._controlPoints.Add(new TFControlPoint(0.00f, new Vector4(0.0f, 0.0f, 0.0f, 0.00f)));
            this._controlPoints.Add(new TFControlPoint(0.30f, new Vector4(0.0f, 0.0f, 0.0f, 0.00f)));   //保持透明到30%

            //软组织：极低透明度（几乎透明）
            this._controlPoints.Add(new TFControlPoint(0.35f, new Vector4(0.3f, 0.3f, 0.3f, 0.005f)));  //0.5%透明度
            this._controlPoints.Add(new TFControlPoint(0.40f, new Vector4(0.4f, 0.4f, 0.4f, 0.008f)));  //0.8%透明度
            this._controlPoints.Add(new TFControlPoint(0.45f, new Vector4(0.5f, 0.5f, 0.5f, 0.010f)));  //1.0%透明度

            //骨骼开始：陡峭变化
            this._controlPoints.Add(new TFControlPoint(0.48f, new Vector4(0.7f, 0.6f, 0.5f, 0.02f)));   //过渡开始
            this._controlPoints.Add(new TFControlPoint(0.50f, new Vector4(0.8f, 0.7f, 0.6f, 0.50f)));   //快速变不透明！
            this._controlPoints.Add(new TFControlPoint(0.52f, new Vector4(0.9f, 0.8f, 0.7f, 0.85f)));   //非常不透明

            //标准骨骼：高不透明度
            this._controlPoints.Add(new TFControlPoint(0.55f, new Vector4(1.0f, 0.9f, 0.8f, 0.92f)));
            this._controlPoints.Add(new TFControlPoint(0.60f, new Vector4(1.0f, 0.95f, 0.85f, 0.95f)));
            this._controlPoints.Add(new TFControlPoint(0.65f, new Vector4(1.0f, 0.97f, 0.90f, 0.97f)));

            //高密度骨骼：完全不透明
            this._controlPoints.Add(new TFControlPoint(0.70f, new Vector4(1.0f, 0.98f, 0.93f, 0.98f)));
            this._controlPoints.Add(new TFControlPoint(0.80f, new Vector4(1.0f, 1.0f, 0.96f, 0.99f)));
            this._controlPoints.Add(new TFControlPoint(0.90f, new Vector4(1.0f, 1.0f, 0.98f, 0.995f)));
            this._controlPoints.Add(new TFControlPoint(1.00f, new Vector4(1.0f, 1.0f, 1.0f, 1.000f)));

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
