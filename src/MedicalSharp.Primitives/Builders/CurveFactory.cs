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

        #region # 生成Catmull-Rom曲线 —— static IReadOnlyList<Vector3> EvaluateCatmullRom(...)
        /// <summary>
        /// 生成Catmull-Rom曲线
        /// </summary>
        public static IReadOnlyList<Vector3> EvaluateCatmullRom(IReadOnlyList<Vector3> points, bool closed = false, int tessellation = 20)
        {
            #region # 验证

            if (points.Count < 4)
            {
                throw new ArgumentException("Catmull-Rom曲线至少需要4个控制点");
            }

            #endregion

            List<Vector3> sampled = [];
            int n = points.Count;
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
                        sampled.Add(EvaluateCatmullRomSegment(points[p0], points[p1], points[p2], points[p3], t));
                    }
                }
            }

            return sampled;
        }
        #endregion


        //Private

        #region # Catmull-Rom单段求值 —— static Vector3 EvaluateCatmullRomSegment(...)
        /// <summary>
        /// Catmull-Rom单段求值
        /// </summary>
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
