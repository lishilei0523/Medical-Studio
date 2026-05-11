using MedicalSharp.Primitives.Enums;
using MedicalSharp.Primitives.Models;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MedicalSharp.Engine.Protocols
{
    /// <summary>
    /// HU传递函数
    /// </summary>
    public sealed class HUTransferFunction : TransferFunction
    {
        #region # 字段及构造器

        /// <summary>
        /// 控制点列表
        /// </summary>
        private readonly List<HUControlPoint> _controlPoints;

        /// <summary>
        /// 创建HU传递函数构造器
        /// </summary>
        internal HUTransferFunction()
        {
            this.HUMin = -1024;
            this.HUMax = 3071;
            this._controlPoints = [];
        }

        #endregion

        #region # 属性

        #region HU最小值 —— short HUMin
        /// <summary>
        /// HU最小值
        /// </summary>
        public short HUMin { get; private set; }
        #endregion

        #region HU最大值 —— short HUMax
        /// <summary>
        /// HU最大值
        /// </summary>
        public short HUMax { get; private set; }
        #endregion

        #region 只读属性 - HU窗宽 —— int HUWindowWidth
        /// <summary>
        /// 只读属性 - HU窗宽
        /// </summary>
        public int HUWindowWidth
        {
            get => this.HUMax - this.HUMin;
        }
        #endregion

        #region 只读属性 - HU窗位 —— int HUWindowCenter
        /// <summary>
        /// 只读属性 - HU窗位
        /// </summary>
        public int HUWindowCenter
        {
            get => (this.HUMin + this.HUMax) / 2;
        }
        #endregion

        #region 只读属性 - 控制点列表 —— IReadOnlyList<HUControlPoint> ControlPoints
        /// <summary>
        /// 只读属性 - 控制点列表
        /// </summary>
        public IReadOnlyList<HUControlPoint> ControlPoints
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

            this.HUMin = huMin;
            this.HUMax = huMax;
            this.UpdateTexture();
        }
        #endregion

        #region 从控制点列表初始化 —— void InitFromControlPoints(IReadOnlyList<HUControlPoint>...
        /// <summary>
        /// 从控制点列表初始化
        /// </summary>
        /// <param name="controlPoints">控制点列表</param>
        public void InitFromControlPoints(IReadOnlyList<HUControlPoint> controlPoints)
        {
            #region # 验证

            if (controlPoints == null || !controlPoints.Any())
            {
                throw new ArgumentNullException(nameof(controlPoints), "控制点列表不可为空！");
            }

            #endregion

            this._controlPoints.Clear();
            foreach (HUControlPoint controlPoint in controlPoints)
            {
                this._controlPoints.Add(controlPoint);
            }

            this.UpdateTexture();
        }
        #endregion

        #region 添加控制点 —— void AddControlPoint(in HUControlPoint controlPoint)
        /// <summary>
        /// 添加控制点
        /// </summary>
        /// <param name="controlPoint">控制点</param>
        public void AddControlPoint(in HUControlPoint controlPoint)
        {
            this._controlPoints.Add(controlPoint);
            this.UpdateTexture();
        }
        #endregion

        #region 删除控制点 —— void RemoveControlPoint(in HUControlPoint controlPoint)
        /// <summary>
        /// 删除控制点
        /// </summary>
        /// <param name="controlPoint">控制点</param>
        public void RemoveControlPoint(in HUControlPoint controlPoint)
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


        //Protected

        #region 更新纹理 —— override void UpdateTexture()
        /// <summary>
        /// 更新纹理
        /// </summary>
        protected override void UpdateTexture()
        {
            #region # 验证

            if (!this._controlPoints.Any())
            {
                return;
            }

            #endregion

            //确保控制点按HU值排序，按Position排序（HU转Position后位置顺序一致）
            this._controlPoints.Sort((a, b) => a.HU.CompareTo(b.HU));

            //排序后直接调基类UpdateTexture走插值
            base.UpdateTexture();
        }
        #endregion

        #region 获取控制点数量 —— override int GetControlPointsCount()
        /// <summary>
        /// 获取控制点数量
        /// </summary>
        protected override int GetControlPointsCount()
        {
            return this._controlPoints.Count;
        }
        #endregion

        #region 插值控制点 —— override Vector4 InterpolateControlPoints(float position)
        /// <summary>
        /// 插值控制点
        /// </summary>
        /// <param name="position">位置(0~1)</param>
        /// <returns>颜色</returns>
        protected override Vector4 InterpolateControlPoints(float position)
        {
            #region # 验证

            if (!this._controlPoints.Any())
            {
                return new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
            }

            #endregion

            //将控制点HU转为Position做插值
            float firstPosition = this.ToPosition(this._controlPoints[0].HU);
            float lastPosition = this.ToPosition(this._controlPoints[^1].HU);
            if (position <= firstPosition)
            {
                return this._controlPoints[0].Color;
            }
            if (position >= lastPosition)
            {
                return this._controlPoints[^1].Color;
            }

            for (int index = 0; index < this._controlPoints.Count - 1; index++)
            {
                float position1 = this.ToPosition(this._controlPoints[index].HU);
                float position2 = this.ToPosition(this._controlPoints[index + 1].HU);
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
                        InterpolationMode.SmoothStep => t * t * (3 - 2 * t),
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

        #region 释放托管资源 —— override void ReleaseManagedResources()
        /// <summary>
        /// 释放托管资源
        /// </summary>
        protected override void ReleaseManagedResources()
        {
            this._controlPoints.Clear();
        }
        #endregion

        #region HU值转归一化位置 —— float ToPosition(short huValue)
        /// <summary>
        /// HU值转归一化位置
        /// </summary>
        /// <param name="huValue">HU值</param>
        /// <returns>归一化位置(0~1)</returns>
        private float ToPosition(short huValue)
        {
            float position = Math.Clamp((huValue * 1.0f - this.HUMin) / (this.HUMax - this.HUMin), 0.0f, 1.0f);

            return position;
        }
        #endregion

        #endregion
    }
}
