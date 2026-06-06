using MedicalSharp.Primitives.Enums;
using MedicalSharp.Primitives.Models;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MedicalSharp.Engine.Protocols
{
    /// <summary>
    /// 密度传递函数
    /// </summary>
    public sealed class DensityTransferFunction : TransferFunction
    {
        #region # 字段及构造器

        /// <summary>
        /// 控制点列表
        /// </summary>
        private readonly List<DensityControlPoint> _controlPoints;

        /// <summary>
        /// 创建密度传递函数构造器
        /// </summary>
        internal DensityTransferFunction()
        {
            this._controlPoints = [];
        }

        #endregion

        #region # 属性

        #region 只读属性 - 控制点列表 —— IReadOnlyList<DensityControlPoint> ControlPoints
        /// <summary>
        /// 只读属性 - 控制点列表
        /// </summary>
        public IReadOnlyList<DensityControlPoint> ControlPoints
        {
            get => this._controlPoints;
        }
        #endregion

        #endregion

        #region # 方法

        //Public

        #region 从控制点列表初始化 —— void InitFromControlPoints(IReadOnlyList<DensityControlPoint>...
        /// <summary>
        /// 从控制点列表初始化
        /// </summary>
        /// <param name="controlPoints">控制点列表</param>
        public void InitFromControlPoints(IReadOnlyList<DensityControlPoint> controlPoints)
        {
            #region # 验证

            if (controlPoints == null || !controlPoints.Any())
            {
                throw new ArgumentNullException(nameof(controlPoints), "控制点列表不可为空！");
            }

            #endregion

            this._controlPoints.Clear();
            foreach (DensityControlPoint controlPoint in controlPoints)
            {
                this._controlPoints.Add(controlPoint);
            }

            this.UpdateTexture();
        }
        #endregion

        #region 添加控制点 —— void AddControlPoint(in DensityControlPoint controlPoint)
        /// <summary>
        /// 添加控制点
        /// </summary>
        /// <param name="controlPoint">控制点</param>
        public void AddControlPoint(in DensityControlPoint controlPoint)
        {
            this._controlPoints.Add(controlPoint);
            this.UpdateTexture();
        }
        #endregion

        #region 删除控制点 —— void RemoveControlPoint(in DensityControlPoint controlPoint)
        /// <summary>
        /// 删除控制点
        /// </summary>
        /// <param name="controlPoint">控制点</param>
        public void RemoveControlPoint(in DensityControlPoint controlPoint)
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

            //确保控制点按位置排序
            this._controlPoints.Sort((a, b) => a.Position.CompareTo(b.Position));

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

        #endregion
    }
}
