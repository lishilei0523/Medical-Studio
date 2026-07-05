using MedicalSharp.Primitives.Maths;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MedicalSharp.Primitives.Algorithms
{
    /// <summary>
    /// 曲线算法
    /// </summary>
    public static class CurveAlgorithms
    {
        //Public

        #region # 生成Catmull-Rom曲线 —— static IReadOnlyList<Vector3> EvaluateCatmullRom(...
        /// <summary>
        /// 生成Catmull-Rom曲线
        /// </summary>
        /// <param name="controlPoints">控制点列表</param>
        /// <param name="closed">是否闭合</param>
        /// <param name="tessellation">采样密度（每段采样点数）</param>
        /// <returns>采样点列表</returns>
        public static IReadOnlyList<Vector3> EvaluateCatmullRom(IReadOnlyList<Vector3> controlPoints, bool closed = false, int tessellation = 20)
        {
            #region # 验证

            if (controlPoints == null || !controlPoints.Any())
            {
                return [];
            }
            if (controlPoints.Count == 1)
            {
                return controlPoints;
            }
            if (controlPoints.Count == 2)
            {
                return EvaluateLineSegment(controlPoints[0], controlPoints[1], tessellation);
            }
            if (controlPoints.Count == 3)
            {
                return EvaluateQuadraticBezier(controlPoints[0], controlPoints[1], controlPoints[2], tessellation);
            }

            #endregion

            List<Vector3> sampledPoints = [];
            int n = controlPoints.Count;
            for (int i = 0; i < (closed ? n : n - 1); i++)
            {
                //获取四个控制点
                int p0 = closed ? (i - 1 + n) % n : Math.Max(i - 1, 0);
                int p1 = i;
                int p2 = closed ? (i + 1) % n : Math.Min(i + 1, n - 1);
                int p3 = closed ? (i + 2) % n : Math.Min(i + 2, n - 1);

                //如果是闭合曲线或者不是最后一段
                if (closed || i < n - 1)
                {
                    for (int j = 0; j <= tessellation; j++)
                    {
                        float t = j * 1.0f / tessellation;
                        Vector3 sampledPoint = EvaluateCatmullRomSegment(controlPoints[p0], controlPoints[p1], controlPoints[p2], controlPoints[p3], t);
                        sampledPoints.Add(sampledPoint);
                    }
                }
            }

            return sampledPoints;
        }
        #endregion

        #region # 计算累积弧长 —— static float[] ComputeArcLengths(IReadOnlyList<Vector3>...
        /// <summary>
        /// 计算累积弧长
        /// </summary>
        /// <param name="sampledPoints">曲线采样点</param>
        /// <returns>累积弧长数组，长度与采样点相同，第一个元素为0</returns>
        /// <remarks>计算采样点列表的累积弧长列表</remarks>
        public static float[] ComputeArcLengths(IReadOnlyList<Vector3> sampledPoints)
        {
            #region # 验证

            if (sampledPoints == null || sampledPoints.Count < 2)
            {
                return sampledPoints?.Count == 1 ? [0] : [];
            }

            #endregion

            float[] arcLengths = new float[sampledPoints.Count];
            arcLengths[0] = 0;
            for (int index = 1; index < sampledPoints.Count; index++)
            {
                float segmentLength = (sampledPoints[index] - sampledPoints[index - 1]).Length;
                arcLengths[index] = arcLengths[index - 1] + segmentLength;
            }

            return arcLengths;
        }
        #endregion

        #region # 等弧长重采样 —— static IReadOnlyList<Vector3> ResampleByArcLength(IReadOnlyList<Vector3>...
        /// <summary>
        /// 等弧长重采样
        /// </summary>
        /// <param name="sampledPoints">原始曲线采样点</param>
        /// <param name="arcLengths">累积弧长数组</param>
        /// <param name="sampleCount">目标采样点数</param>
        /// <returns>等弧长分布的新采样点</returns>
        /// <remarks>沿曲线等弧长重新采样</remarks>
        public static IReadOnlyList<Vector3> ResampleByArcLength(IReadOnlyList<Vector3> sampledPoints, float[] arcLengths, int sampleCount)
        {
            #region # 验证

            if (sampledPoints == null || sampledPoints.Count < 2 || sampleCount < 2)
            {
                return sampledPoints ?? [];
            }

            #endregion

            float totalLength = arcLengths[^1];
            Vector3[] resampledPoints = new Vector3[sampleCount];
            for (int index = 0; index < sampleCount; index++)
            {
                float targetArc = (index / (float)(sampleCount - 1)) * totalLength;
                resampledPoints[index] = SampleByArcLength(sampledPoints, arcLengths, targetArc);
            }

            return resampledPoints;
        }
        #endregion

        #region # 构建Frenet框架 —— static FrenetFrame[] BuildFrenetFrames(IReadOnlyList<Vector3>...
        /// <summary>
        /// 构建Frenet框架
        /// </summary>
        /// <param name="resampledPoints">等弧长分布的曲线采样点</param>
        /// <returns>每个采样点对应的FrenetFrame数组</returns>
        /// <remarks>在等弧长采样点序列上构建Frenet框架，使用平行传输避免翻转</remarks>
        public static FrenetFrame[] BuildFrenetFrames(IReadOnlyList<Vector3> resampledPoints)
        {
            #region # 验证

            if (resampledPoints == null || resampledPoints.Count < 2)
            {
                return [];
            }

            #endregion

            int n = resampledPoints.Count;
            FrenetFrame[] frames = new FrenetFrame[n];

            //第一个点的切线（中心差分或前向差分）
            Vector3 tangent0 = Vector3.Normalize(resampledPoints[1] - resampledPoints[0]);

            //找与切线垂直的任意向量作为初始法向量
            Vector3 normal0 = GeometryAlgorithms.FindPerpendicularVector(tangent0);
            Vector3 binormal0 = Vector3.Normalize(Vector3.Cross(tangent0, normal0));
            frames[0] = new FrenetFrame(resampledPoints[0], tangent0, normal0, binormal0);

            //后续点用平行传输
            for (int index = 1; index < n; index++)
            {
                Vector3 tangent;
                if (index < n - 1)
                {
                    //中心差分
                    tangent = Vector3.Normalize(resampledPoints[index + 1] - resampledPoints[index - 1]);
                }
                else
                {
                    //最后一点用后向差分
                    tangent = Vector3.Normalize(resampledPoints[index] - resampledPoints[index - 1]);
                }

                Vector3 prevTangent = frames[index - 1].Tangent;
                Vector3 prevNormal = frames[index - 1].Normal;

                //旋转轴 = prevTangent × tangent
                Vector3 rotationAxis = Vector3.Cross(prevTangent, tangent);
                float axisLength = rotationAxis.Length;

                Vector3 transportedNormal;
                if (axisLength > 1e-8f)
                {
                    rotationAxis /= axisLength;
                    float angle = (float)Math.Asin(Math.Clamp(axisLength, -1.0, 1.0));
                    transportedNormal = GeometryAlgorithms.RotateAroundAxis(prevNormal, rotationAxis, angle);
                }
                else
                {
                    transportedNormal = prevNormal;
                }

                //施密特正交化
                transportedNormal = Vector3.Normalize(transportedNormal - Vector3.Dot(transportedNormal, tangent) * tangent);
                Vector3 binormal = Vector3.Normalize(Vector3.Cross(tangent, transportedNormal));

                frames[index] = new FrenetFrame(resampledPoints[index], tangent, transportedNormal, binormal);
            }

            return frames;
        }
        #endregion


        //Private

        #region # Catmull-Rom单段求值 —— static Vector3 EvaluateCatmullRomSegment(Vector3 p0...
        /// <summary>
        /// Catmull-Rom单段求值
        /// </summary>
        /// <param name="p0">第一个控制点（用于定义进入切线）</param>
        /// <param name="p1">第二个控制点（曲线起点）</param>
        /// <param name="p2">第三个控制点（曲线终点）</param>
        /// <param name="p3">第四个控制点（用于定义离开切线）</param>
        /// <param name="t">参数t，范围[0, 1]，0对应p1，1对应p2</param>
        /// <returns>曲线上参数t处的点</returns>
        /// <remarks>
        /// Catmull-Rom样条由四个控制点定义一段曲线，曲线从p1到p2。
        /// 切线方向由p0->p2和p1->p3决定，保证曲线在p1和p2处与相邻段C¹连续。
        /// 基函数推导：
        ///     b0 = -0.5t³ + t² - 0.5t
        ///     b1 = 1.5t³ - 2.5t² + 1.0
        ///     b2 = -1.5t³ + 2.0t² + 0.5t
        ///     b3 = 0.5t³ - 0.5t²
        /// 满足：b0+b1+b2+b3 = 1（仿射组合）
        ///     t=0时，b1=1，其余为0 -> 曲线经过p1
        ///     t=1时，b2=1，其余为0 -> 曲线经过p2
        /// </remarks>
        private static Vector3 EvaluateCatmullRomSegment(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            float t2 = t * t;
            float t3 = t2 * t;

            //Catmull-Rom基函数矩阵
            float b0 = -0.5f * t3 + t2 - 0.5f * t;
            float b1 = 1.5f * t3 - 2.5f * t2 + 1.0f;
            float b2 = -1.5f * t3 + 2.0f * t2 + 0.5f * t;
            float b3 = 0.5f * t3 - 0.5f * t2;

            //计算采样点
            Vector3 sampledPoint = b0 * p0 + b1 * p1 + b2 * p2 + b3 * p3;

            return sampledPoint;
        }
        #endregion

        #region # 生成线段 —— static IReadOnlyList<Vector3> EvaluateLineSegment(Vector3 start...
        /// <summary>
        /// 生成线段
        /// </summary>
        /// <param name="start">起始点</param>
        /// <param name="end">终止点</param>
        /// <param name="tessellation">采样密度（每段采样点数）</param>
        /// <returns>采样点列表</returns>
        private static IReadOnlyList<Vector3> EvaluateLineSegment(Vector3 start, Vector3 end, int tessellation)
        {
            List<Vector3> sampledPoints = [];
            for (int index = 0; index <= tessellation; index++)
            {
                float t = index * 1.0f / tessellation;
                Vector3 sampledPoint = Vector3.Lerp(start, end, t);
                sampledPoints.Add(sampledPoint);
            }

            return sampledPoints;
        }
        #endregion

        #region # 生成二次贝塞尔曲线 —— static IReadOnlyList<Vector3> EvaluateQuadraticBezier(...
        /// <summary>
        /// 生成二次贝塞尔曲线
        /// </summary>
        /// <param name="p0">起点</param>
        /// <param name="p1">控制点</param>
        /// <param name="p2">终点</param>
        /// <param name="tessellation">采样密度</param>
        /// <returns>曲线上的采样点</returns>
        /// <remarks>二次贝塞尔曲线公式：B(t) = (1-t)²P0 + 2(1-t)tP1 + t²P2</remarks>
        private static IReadOnlyList<Vector3> EvaluateQuadraticBezier(Vector3 p0, Vector3 p1, Vector3 p2, int tessellation)
        {
            //计算控制点Q，使得t=0.5时曲线经过p1
            //p1 = (1-0.5)²p0 + 2(1-0.5)*0.5Q + 0.5²p2 = 0.25p0 + 0.5Q + 0.25p2
            //=> Q = (p1 - 0.25p0 - 0.25p2) / 0.5 = 2p1 - 0.5p0 - 0.5p2
            Vector3 controlPoint = 2 * p1 - 0.5f * p0 - 0.5f * p2;
            List<Vector3> sampledPoints = [];
            for (int index = 0; index <= tessellation; index++)
            {
                float t = index * 1.0f / tessellation;
                float u = 1 - t;
                Vector3 sampledPoint = u * u * p0 + 2 * u * t * controlPoint + t * t * p2;
                sampledPoints.Add(sampledPoint);
            }

            return sampledPoints;
        }
        #endregion

        #region # 单段等弧长重采样 —— static Vector3 SampleByArcLength(IReadOnlyList<Vector3> sampledPoints...
        /// <summary>
        /// 单段等弧长重采样
        /// </summary>
        /// <param name="sampledPoints">采样点列表</param>
        /// <param name="arcLengths">累积弧长</param>
        /// <param name="targetArcLength">目标弧长</param>
        /// <returns>重采样点</returns>
        /// <remarks>根据目标弧长在采样点序列上线性插值得到3D位置</remarks>
        private static Vector3 SampleByArcLength(IReadOnlyList<Vector3> sampledPoints, float[] arcLengths, float targetArcLength)
        {
            float totalLength = arcLengths[^1];
            targetArcLength = Math.Clamp(targetArcLength, 0, totalLength);

            //二分查找目标弧长所在的段
            int index = Array.BinarySearch(arcLengths, targetArcLength);
            if (index >= 0)
            {
                return sampledPoints[index];
            }

            index = ~index;
            index = Math.Clamp(index, 1, arcLengths.Length - 1);

            float segmentStart = arcLengths[index - 1];
            float segmentEnd = arcLengths[index];
            float segmentLength = segmentEnd - segmentStart;
            float t = segmentLength > 1e-8f
                ? (targetArcLength - segmentStart) / segmentLength
                : 0f;
            Vector3 resampledPoint = Vector3.Lerp(sampledPoints[index - 1], sampledPoints[index], t);

            return resampledPoint;
        }
        #endregion
    }
}
