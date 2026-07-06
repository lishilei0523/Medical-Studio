using MedicalSharp.Primitives.Algorithms;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MedicalSharp.Primitives.Maths
{
    /// <summary>
    /// 曲线
    /// </summary>
    public class Curve
    {
        #region # 字段及构造器

        /// <summary>
        /// 采样密度
        /// </summary>
        private readonly int _tessellation;

        /// <summary>
        /// 等弧长重采样点数
        /// </summary>
        private readonly int _resampleCount;

        /// <summary>
        /// 是否闭合
        /// </summary>
        private readonly bool _closed;

        /// <summary>
        /// 创建曲线构造器
        /// </summary>
        /// <param name="controlPoints">控制点列表</param>
        /// <param name="tessellation">采样密度</param>
        /// <param name="resampleCount">等弧长重采样点数</param>
        /// <param name="closed">是否闭合</param>
        public Curve(IReadOnlyList<Vector3> controlPoints, int tessellation = 20, int resampleCount = 200, bool closed = false)
        {
            #region # 验证

            if (controlPoints == null || !controlPoints.Any())
            {
                throw new ArgumentNullException(nameof(controlPoints), "控制点列表不可为空！");
            }

            #endregion

            this._tessellation = tessellation;
            this._resampleCount = resampleCount;
            this._closed = closed;
            this.ControlPoints = controlPoints;
            this.SampledPoints = CurveAlgorithms.EvaluateCatmullRom(controlPoints, closed, tessellation);

            if (this.SampledPoints.Count < 2)
            {
                this.ArcLengths = this.SampledPoints.Count == 1 ? [0] : [];
                this.ResampledPoints = this.SampledPoints;
                this.FrenetFrames = [];

                return;
            }

            //计算弧长
            this.ArcLengths = CurveAlgorithms.ComputeArcLengths(this.SampledPoints);

            //等弧长重采样
            this.ResampledPoints = CurveAlgorithms.ResampleByArcLength(this.SampledPoints, this.ArcLengths, resampleCount);

            //构建Frenet框架
            this.FrenetFrames = CurveAlgorithms.BuildFrenetFrames(this.ResampledPoints);
        }

        #endregion

        #region # 属性

        #region 控制点列表 —— IReadOnlyList<Vector3> ControlPoints
        /// <summary>
        /// 控制点列表
        /// </summary>
        public IReadOnlyList<Vector3> ControlPoints { get; }
        #endregion

        #region 采样点列表 —— IReadOnlyList<Vector3> SampledPoints
        /// <summary>
        /// 采样点列表
        /// </summary>
        public IReadOnlyList<Vector3> SampledPoints { get; }
        #endregion

        #region 重采样点列表 —— IReadOnlyList<Vector3> ResampledPoints
        /// <summary>
        /// 重采样点列表
        /// </summary>
        /// <remarks>等弧长重采样</remarks>
        public IReadOnlyList<Vector3> ResampledPoints { get; }
        #endregion

        #region 累积弧长列表 —— float[] ArcLengths
        /// <summary>
        /// 累积弧长列表
        /// </summary>
        /// <remarks>与SampledPoints等长，ArcLengths[0]为0</remarks>
        public float[] ArcLengths { get; }
        #endregion

        #region Frenet框架列表 —— FrenetFrame[] FrenetFrames
        /// <summary>
        /// Frenet框架列表
        /// </summary>
        /// <remarks>每个重采样点对应的Frenet框架，与ResampledPoints等长</remarks>
        public FrenetFrame[] FrenetFrames { get; }
        #endregion

        #region 只读属性 - 采样密度 —— int Tessellation
        /// <summary>
        /// 只读属性 - 采样密度
        /// </summary>
        public int Tessellation
        {
            get => this._tessellation;
        }
        #endregion

        #region 只读属性 - 等弧长重采样点数 —— int ResampleCount
        /// <summary>
        /// 只读属性 - 等弧长重采样点数
        /// </summary>
        public int ResampleCount
        {
            get => this._resampleCount;
        }
        #endregion

        #region 只读属性 - 是否闭合 —— bool Closed
        /// <summary>
        /// 只读属性 - 是否闭合
        /// </summary>
        public bool Closed
        {
            get => this._closed;
        }
        #endregion

        #region 只读属性 - 总弧长 —— float TotalArcLength
        /// <summary>
        /// 只读属性 - 总弧长
        /// </summary>
        public float TotalArcLength
        {
            get => this.ArcLengths.Length > 0 ? this.ArcLengths[^1] : 0f;
        }
        #endregion

        #endregion

        #region # 方法

        #region 根据弧长获取曲线上的位置 —— Vector3 GetPointAtArcLength(float arcLength)
        /// <summary>
        /// 根据弧长获取曲线上的位置
        /// </summary>
        /// <param name="arcLength">目标弧长，自动钳制到[0, TotalArcLength]</param>
        /// <returns>曲线上该弧长处的位置</returns>
        public Vector3 GetPointAtArcLength(float arcLength)
        {
            #region # 验证

            if (this.ResampledPoints.Count < 2)
            {
                return this.ResampledPoints.Count == 1
                    ? this.ResampledPoints[0]
                    : Vector3.Zero;
            }

            #endregion

            arcLength = Math.Clamp(arcLength, 0, this.TotalArcLength);
            float t = arcLength / this.TotalArcLength;
            float indexFloat = t * (this.ResampledPoints.Count - 1);
            int index0 = (int)indexFloat;
            int index1 = Math.Min(index0 + 1, this.ResampledPoints.Count - 1);
            float frac = indexFloat - index0;
            Vector3 resampledPoint = Vector3.Lerp(this.ResampledPoints[index0], this.ResampledPoints[index1], frac);

            return resampledPoint;
        }
        #endregion

        #region 根据弧长获取Frenet框架 —— FrenetFrame GetFrameAtArcLength(float arcLength)
        /// <summary>
        /// 根据弧长获取Frenet框架
        /// </summary>
        /// <param name="arcLength">目标弧长，自动钳制到[0, TotalArcLength]</param>
        /// <returns>插值后的Frenet框架，保证三轴正交</returns>
        /// <remarks>在重采样框架之间线性插值</remarks>
        public FrenetFrame GetFrameAtArcLength(float arcLength)
        {
            #region # 验证

            if (this.FrenetFrames.Length < 2)
            {
                return this.FrenetFrames.Length == 1
                    ? this.FrenetFrames[0]
                    : default;
            }

            #endregion

            arcLength = Math.Clamp(arcLength, 0, this.TotalArcLength);
            float t = arcLength / this.TotalArcLength;
            float indexFloat = t * (this.FrenetFrames.Length - 1);
            int index0 = (int)indexFloat;
            int index1 = Math.Min(index0 + 1, this.FrenetFrames.Length - 1);
            float frac = indexFloat - index0;

            Vector3 position = Vector3.Lerp(this.FrenetFrames[index0].Position, this.FrenetFrames[index1].Position, frac);
            Vector3 tangent = Vector3.Normalize(Vector3.Lerp(this.FrenetFrames[index0].Tangent, this.FrenetFrames[index1].Tangent, frac));
            Vector3 normal = Vector3.Normalize(Vector3.Lerp(this.FrenetFrames[index0].Normal, this.FrenetFrames[index1].Normal, frac));

            //通过叉积重建副法向量，保证正交
            Vector3 binormal = Vector3.Normalize(Vector3.Cross(tangent, normal));

            //用新的副法线修正法向量，保证完全正交
            normal = Vector3.Cross(binormal, tangent);

            FrenetFrame frenetFrame = new FrenetFrame(position, tangent, normal, binormal);

            return frenetFrame;
        }
        #endregion 

        #endregion
    }
}
