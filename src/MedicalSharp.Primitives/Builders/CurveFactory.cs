using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace MedicalSharp.Primitives.Builders
{
    /// <summary>
    /// 曲线工厂
    /// </summary>
    public static class CurveFactory
    {
        //Public

        #region # 生成Catmull-Rom曲线 —— static IReadOnlyList<Vector3> EvaluateCatmullRom(IReadOnlyList<Vector3> points...
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

            if (controlPoints.Count < 4)
            {
                throw new ArgumentException("Catmull-Rom曲线至少需要4个控制点");
            }

            #endregion

            List<Vector3> sampled = [];
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
                        float t = j / (float)tessellation;
                        sampled.Add(EvaluateCatmullRomSegment(controlPoints[p0], controlPoints[p1], controlPoints[p2], controlPoints[p3], t));
                    }
                }
            }

            return sampled;
        }
        #endregion


        //Private

        #region # Catmull-Rom单段求值 —— static Vector3 EvaluateCatmullRomSegment(Vector3 p0, Vector3 p1...
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
        ///
        /// 基函数推导：
        /// b0 = -0.5t³ + t² - 0.5t
        /// b1 = 1.5t³ - 2.5t² + 1.0
        /// b2 = -1.5t³ + 2.0t² + 0.5t
        /// b3 = 0.5t³ - 0.5t²
        ///
        /// 满足：b0+b1+b2+b3 = 1（仿射组合）
        /// t=0时，b1=1，其余为0 -> 曲线经过p1
        /// t=1时，b2=1，其余为0 -> 曲线经过p2
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

            return b0 * p0 + b1 * p1 + b2 * p2 + b3 * p3;
        }
        #endregion
    }
}
