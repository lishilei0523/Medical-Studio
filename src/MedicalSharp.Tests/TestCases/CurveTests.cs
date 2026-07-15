using MedicalSharp.Primitives.Algorithms;
using MedicalSharp.Primitives.Maths;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace MedicalSharp.Tests.TestCases
{
    /// <summary>
    /// 曲线测试
    /// </summary>
    [TestClass]
    public class CurveTests
    {
        #region # 测试生成Catmull-Rom曲线 —— void TestEvaluateCatmullRom()
        /// <summary>
        /// 测试生成Catmull-Rom曲线
        /// </summary>
        [TestMethod]
        public void TestEvaluateCatmullRom()
        {
            List<Vector3> controlPoints =
            [
                new Vector3(0, 0, 0),
                new Vector3(10, 20, 5),
                new Vector3(30, 15, -10),
                new Vector3(50, 30, 5),
                new Vector3(70, 10, 0)
            ];

            IReadOnlyList<Vector3> sampled = CurveAlgorithms.EvaluateCatmullRom(controlPoints, closed: false, tessellation: 10);

            Assert.IsNotNull(sampled);
            Assert.IsTrue(sampled.Count > 0);

            //首尾点应该接近控制点的首尾
            float distStart = (sampled[0] - controlPoints[0]).Length;
            float distEnd = (sampled[^1] - controlPoints[^1]).Length;

            Assert.IsTrue(distStart < 0.001f, $"起点偏离: {distStart}");
            Assert.IsTrue(distEnd < 0.001f, $"终点偏离: {distEnd}");
        }
        #endregion

        #region # 测试生成Catmull-Rom曲线(单点) —— void TestEvaluateCatmullRomSinglePoint()
        /// <summary>
        /// 测试生成Catmull-Rom曲线(单点)
        /// </summary>
        [TestMethod]
        public void TestEvaluateCatmullRomSinglePoint()
        {
            List<Vector3> controlPoints = [new Vector3(5, 5, 5)];
            IReadOnlyList<Vector3> sampled = CurveAlgorithms.EvaluateCatmullRom(controlPoints);

            Assert.AreEqual(1, sampled.Count);
            Assert.AreEqual(new Vector3(5, 5, 5), sampled[0]);
        }
        #endregion

        #region # 测试生成Catmull-Rom曲线(两点) —— void TestEvaluateCatmullRomTwoPoints()
        /// <summary>
        /// 测试生成Catmull-Rom曲线(两点)
        /// </summary>
        [TestMethod]
        public void TestEvaluateCatmullRomTwoPoints()
        {
            List<Vector3> controlPoints =
            [
                new Vector3(0, 0, 0),
                new Vector3(10, 0, 0)
            ];

            IReadOnlyList<Vector3> sampledPoint = CurveAlgorithms.EvaluateCatmullRom(controlPoints, tessellation: 5);

            Assert.AreEqual(6, sampledPoint.Count); //tessellation + 1
            Assert.AreEqual(new Vector3(0, 0, 0), sampledPoint[0]);
            Assert.AreEqual(new Vector3(10, 0, 0), sampledPoint[^1]);
        }
        #endregion

        #region # 测试计算累积弧长 —— void TestComputeArcLengths()
        /// <summary>
        /// 测试计算累积弧长 - 
        /// </summary>
        /// <remarks>长度递增且非负</remarks>
        [TestMethod]
        public void TestComputeArcLengths()
        {
            List<Vector3> points =
            [
                new Vector3(0, 0, 0),
                new Vector3(3, 0, 0),
                new Vector3(3, 4, 0)
            ];

            float[] arcLengths = CurveAlgorithms.ComputeArcLengths(points);

            Assert.AreEqual(points.Count, arcLengths.Length);
            Assert.AreEqual(0f, arcLengths[0]);
            Assert.AreEqual(3f, arcLengths[1], 0.001f);       //第一段长度
            Assert.AreEqual(7f, arcLengths[2], 0.001f);       //3 + 4
        }
        #endregion

        #region # 测试等弧长重采样 —— void TestResampleByArcLength1()
        /// <summary>
        /// 测试等弧长重采样
        /// </summary>
        /// <remarks>返回指定数量的点</remarks>
        [TestMethod]
        public void TestResampleByArcLength1()
        {
            List<Vector3> points =
            [
                new Vector3(0, 0, 0),
                new Vector3(10, 0, 0),
                new Vector3(20, 0, 0)
            ];
            float[] arcLengths = CurveAlgorithms.ComputeArcLengths(points);

            IReadOnlyList<Vector3> resampledPoints = CurveAlgorithms.ResampleByArcLength(points, arcLengths, 5);

            Assert.AreEqual(5, resampledPoints.Count);
        }
        #endregion

        #region # 测试等弧长重采样 —— void TestResampleByArcLength2()
        /// <summary>
        /// 测试等弧长重采样
        /// </summary>
        /// <remarks>点之间等距分布</remarks>
        [TestMethod]
        public void TestResampleByArcLength2()
        {
            // 一条总长20的直线
            List<Vector3> points =
            [
                new Vector3(0, 0, 0),
                new Vector3(20, 0, 0)
            ];
            float[] arcLengths = CurveAlgorithms.ComputeArcLengths(points);
            IReadOnlyList<Vector3> resampledPoints = CurveAlgorithms.ResampleByArcLength(points, arcLengths, 3);

            Assert.AreEqual(3, resampledPoints.Count);
            Assert.AreEqual(new Vector3(0, 0, 0), resampledPoints[0]);
            Assert.AreEqual(new Vector3(10, 0, 0), resampledPoints[1]);
            Assert.AreEqual(new Vector3(20, 0, 0), resampledPoints[2]);
        }
        #endregion

        #region # 测试构建Frenet框架 —— void TestBuildFrenetFrames1()
        /// <summary>
        /// 测试构建Frenet框架
        /// </summary>
        /// <remarks>直线上的框架切线一致</remarks>
        [TestMethod]
        public void TestBuildFrenetFrames1()
        {
            List<Vector3> points =
            [
                new Vector3(0, 0, 0),
                new Vector3(5, 0, 0),
                new Vector3(10, 0, 0)
            ];

            FrenetFrame[] frames = CurveAlgorithms.BuildFrenetFrames(points);

            Assert.AreEqual(points.Count, frames.Length);

            //直线上所有框架的切线应该一致（沿X轴）
            foreach (FrenetFrame frame in frames)
            {
                float dot = Vector3.Dot(frame.Tangent, Vector3.UnitX);
                Assert.IsTrue(dot > 0.99f, $"切线应与X轴平行，实际点积: {dot}");
            }
        }
        #endregion

        #region # 测试构建Frenet框架 —— void TestBuildFrenetFrames2()
        /// <summary>
        /// 测试构建Frenet框架
        /// </summary>
        /// <remarks>三轴正交性</remarks>
        [TestMethod]
        public void TestBuildFrenetFrames2()
        {
            List<Vector3> controlPoints =
            [
                new Vector3(0, 0, 0),
                new Vector3(10, 20, 5),
                new Vector3(30, 15, -10),
                new Vector3(50, 30, 5),
                new Vector3(70, 10, 0)
            ];
            IReadOnlyList<Vector3> sampledPoints = CurveAlgorithms.EvaluateCatmullRom(controlPoints, tessellation: 10);
            float[] arcLengths = CurveAlgorithms.ComputeArcLengths(sampledPoints);
            IReadOnlyList<Vector3> resampledPoints = CurveAlgorithms.ResampleByArcLength(sampledPoints, arcLengths, 50);
            FrenetFrame[] frenetFrames = CurveAlgorithms.BuildFrenetFrames(resampledPoints);

            foreach (FrenetFrame frame in frenetFrames)
            {
                float dotTN = Math.Abs(Vector3.Dot(frame.Tangent, frame.Normal));
                float dotTB = Math.Abs(Vector3.Dot(frame.Tangent, frame.Binormal));
                float dotNB = Math.Abs(Vector3.Dot(frame.Normal, frame.Binormal));

                Assert.IsTrue(dotTN < 0.001f, $"T·N = {dotTN}，应接近0");
                Assert.IsTrue(dotTB < 0.001f, $"T·B = {dotTB}，应接近0");
                Assert.IsTrue(dotNB < 0.001f, $"N·B = {dotNB}，应接近0");
            }
        }
        #endregion

        #region # 测试从控制点构造曲线 —— void TestCurveFromControlPoints()
        /// <summary>
        /// 测试从控制点构造曲线
        /// </summary>
        [TestMethod]
        public void TestCurveFromControlPoints()
        {
            List<Vector3> controlPoints =
            [
                new Vector3(0, 0, 0),
                new Vector3(10, 20, 5),
                new Vector3(30, 15, -10),
                new Vector3(50, 30, 5),
                new Vector3(70, 10, 0)
            ];

            Curve curve = new Curve(controlPoints, Matrix4.Identity, closed: false, tessellation: 10, resampleCount: 100);

            Assert.IsNotNull(curve);
            Assert.AreEqual(controlPoints.Count, curve.ControlPoints.Count);
            Assert.IsTrue(curve.SampledPoints.Count > 0);
            Assert.AreEqual(100, curve.ResampledPoints.Count);
            Assert.AreEqual(100, curve.FrenetFrames.Length);
            Assert.AreEqual(curve.SampledPoints.Count, curve.ArcLengths.Length);
            Assert.IsTrue(curve.TotalArcLength > 0);
        }
        #endregion

        #region # 测试单个控制点曲线 —— void TestSingleControlPointCurve()
        /// <summary>
        /// 测试单个控制点曲线
        /// </summary>
        [TestMethod]
        public void TestSingleControlPointCurve()
        {
            List<Vector3> controlPoints = [new Vector3(5, 5, 5)];
            Curve curve = new Curve(controlPoints, Matrix4.Identity);

            Assert.AreEqual(1, curve.SampledPoints.Count);
            Assert.AreEqual(1, curve.ResampledPoints.Count);
            Assert.AreEqual(0, curve.FrenetFrames.Length);
            Assert.AreEqual(0f, curve.TotalArcLength);
        }
        #endregion

        #region # 测试根据弧长获取曲线上的位置 —— void TestGetPointAtArcLength()
        /// <summary>
        /// 测试根据弧长获取曲线上的位置
        /// </summary>
        /// <remarks>起点、中点、终点</remarks>
        [TestMethod]
        public void TestGetPointAtArcLength()
        {
            List<Vector3> controlPoints =
            [
                new Vector3(0, 0, 0),
                new Vector3(0, 10, 0),
                new Vector3(0, 20, 0)
            ];

            Curve curve = new Curve(controlPoints, Matrix4.Identity, 5, 50);

            Vector3 start = curve.GetPointAtArcLength(0);
            Vector3 end = curve.GetPointAtArcLength(curve.TotalArcLength);
            Vector3 mid = curve.GetPointAtArcLength(curve.TotalArcLength * 0.5f);

            Assert.IsTrue((start - controlPoints[0]).Length < 0.1f, "起点偏离");
            Assert.IsTrue((end - controlPoints[^1]).Length < 0.1f, "终点偏离");
            Assert.IsTrue(mid.Y > 8 && mid.Y < 12, $"中点Y应在10附近，实际: {mid.Y}");
        }
        #endregion

        #region # 测试根据弧长获取Frenet框架 —— void TestGetFrameAtArcLength()
        /// <summary>
        /// 测试根据弧长获取Frenet框架
        /// </summary>
        /// <remarks>插值后的框架仍保持正交</remarks>
        [TestMethod]
        public void TestGetFrameAtArcLength()
        {
            List<Vector3> controlPoints =
            [
                new Vector3(0, 0, 0),
                new Vector3(10, 20, 5),
                new Vector3(30, 15, -10),
                new Vector3(50, 30, 5),
                new Vector3(70, 10, 0)
            ];

            Curve curve = new Curve(controlPoints, Matrix4.Identity, resampleCount: 100);

            //测试几个弧长位置的框架
            float[] testArcLengths = [0, curve.TotalArcLength * 0.25f, curve.TotalArcLength * 0.5f, curve.TotalArcLength * 0.75f, curve.TotalArcLength
            ];

            foreach (float arcLength in testArcLengths)
            {
                FrenetFrame frame = curve.GetFrameAtArcLength(arcLength);

                float dotTN = Math.Abs(Vector3.Dot(frame.Tangent, frame.Normal));
                float dotTB = Math.Abs(Vector3.Dot(frame.Tangent, frame.Binormal));
                float dotNB = Math.Abs(Vector3.Dot(frame.Normal, frame.Binormal));

                Assert.IsTrue(dotTN < 0.001f, $"弧长{arcLength}: T·N = {dotTN}");
                Assert.IsTrue(dotTB < 0.001f, $"弧长{arcLength}: T·B = {dotTB}");
                Assert.IsTrue(dotNB < 0.001f, $"弧长{arcLength}: N·B = {dotNB}");

                //切线应该是单位向量
                Assert.IsTrue(Math.Abs(frame.Tangent.Length - 1.0f) < 0.001f, $"切线长度: {frame.Tangent.Length}");
                Assert.IsTrue(Math.Abs(frame.Normal.Length - 1.0f) < 0.001f, $"法线长度: {frame.Normal.Length}");
                Assert.IsTrue(Math.Abs(frame.Binormal.Length - 1.0f) < 0.001f, $"副法线长度: {frame.Binormal.Length}");
            }
        }
        #endregion

        #region # 测试弧长一致性 —— void TestCurveArcLength()
        /// <summary>
        /// 测试弧长一致性
        /// </summary>
        /// <remarks>TotalArcLength与重采样首末点距离一致</remarks>
        [TestMethod]
        public void TestCurveArcLength()
        {
            List<Vector3> controlPoints =
            [
                new Vector3(0, 0, 0),
                new Vector3(10, 0, 0),
                new Vector3(20, 0, 0)
            ];

            Curve curve = new Curve(controlPoints, Matrix4.Identity, resampleCount: 20);

            Assert.AreEqual(20f, curve.TotalArcLength, 0.01f);

            //首尾重采样点应该和弧长0、TotalArcLength对应
            Vector3 start = curve.GetPointAtArcLength(0);
            Vector3 end = curve.GetPointAtArcLength(curve.TotalArcLength);

            Assert.IsTrue((start - curve.ResampledPoints[0]).Length < 0.001f);
            Assert.IsTrue((end - curve.ResampledPoints[^1]).Length < 0.001f);
        }
        #endregion
    }
}
