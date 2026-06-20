using itk.simple;
using MedicalSharp.Insight.Operators;
using MedicalSharp.Primitives.Algorithms;
using MedicalSharp.Primitives.Maths;
using MedicalSharp.Primitives.Models;
using OpenTK.Mathematics;
using System;

namespace MedicalSharp.Insight.Algorithms
{
    /// <summary>
    /// 统计算法
    /// </summary>
    public static class StatisticsAlgorithms
    {
        #region # 适用圆形统计 —— static StatisticResult ApplyCircleAnalyse(this VolumeData volumeData...
        /// <summary>
        /// 适用圆形统计
        /// </summary>
        /// <param name="volumeData">体积数据</param>
        /// <param name="plane">MPR平面</param>
        /// <param name="circleCenter">圆心（世界空间）</param>
        /// <param name="circleRadius">半径（世界空间）</param> 
        /// <returns>统计结果</returns>
        public static unsafe StatisticResult ApplyCircleAnalyse(this VolumeData volumeData, MPRPlane plane, Vector3 circleCenter, float circleRadius)
        {
            //世界空间 -> 患者毫米空间（切面参数）
            Vector3 patientSliceCenter = plane.WorldCenter.ToPatientPosition(volumeData.Metadata);

            //提取切片
            Vector2i sliceSize = plane.GetSliceSize();
            Resampler resampler = new Resampler(volumeData);
            using Image slice = resampler.ExtractSlice(patientSliceCenter, sliceSize, plane.UAxis, plane.VAxis, plane.Normal);
            VectorDouble sliceSpacing = slice.GetSpacing();
            VectorDouble sliceOrigin = slice.GetOrigin();
            VectorDouble sliceDirection = slice.GetDirection();
            Vector3 patientSliceOrigin = new Vector3((float)sliceOrigin[0], (float)sliceOrigin[1], (float)sliceOrigin[2]);
            Vector3 patientSliceUAxis = new Vector3((float)sliceDirection[0], (float)sliceDirection[1], (float)sliceDirection[2]);
            Vector3 patientSliceVAxis = new Vector3((float)sliceDirection[3], (float)sliceDirection[4], (float)sliceDirection[5]);

            //世界圆参数 -> 切片像素参数
            Vector3 patientCircleCenter = circleCenter.ToPatientPosition(volumeData.Metadata);
            Vector3 patientCircleCenterOffset = patientCircleCenter - patientSliceOrigin;
            float patientCircleCenterOffsetU = Vector3.Dot(patientCircleCenterOffset, patientSliceUAxis);
            float patientCircleCenterOffsetV = Vector3.Dot(patientCircleCenterOffset, patientSliceVAxis);
            float pixelCircleCenterX = patientCircleCenterOffsetU / (float)sliceSpacing[0];
            float pixelCircleCenterY = patientCircleCenterOffsetV / (float)sliceSpacing[1];

            //半径：世界单位 -> 毫米 -> 切片像素单位
            Vector3 pointOnCircle = circleCenter + plane.UAxis * circleRadius;
            Vector3 patientPointOnCircle = pointOnCircle.ToPatientPosition(volumeData.Metadata);
            float mmRadius = Vector3.Distance(patientCircleCenter, patientPointOnCircle);
            float pixelRadiusU = mmRadius / (float)sliceSpacing[0]; //行方向像素半径
            float pixelRadiusV = mmRadius / (float)sliceSpacing[1]; //列方向像素半径

            //计算圆形包围盒
            int startX = (int)Math.Round(pixelCircleCenterX - pixelRadiusU - 1);
            int endX = (int)Math.Round(pixelCircleCenterX + pixelRadiusU + 1);
            int startY = (int)Math.Round(pixelCircleCenterY - pixelRadiusV - 1);
            int endY = (int)Math.Round(pixelCircleCenterY + pixelRadiusV + 1);

            //裁剪到切片范围
            startX = Math.Max(0, startX);
            endX = Math.Min(sliceSize.X - 1, endX);
            startY = Math.Max(0, startY);
            endY = Math.Min(sliceSize.Y - 1, endY);

            //遍历包围盒范围内像素
            short* sliceVoxels = (short*)slice.GetBufferAsInt16().ToPointer();
            StatisticResult result = new StatisticResult();
            for (int y = startY; y <= endY; y++)
            {
                for (int x = startX; x <= endX; x++)
                {
                    Vector2 pixelPos = new Vector2(x + 0.5f, y + 0.5f);

                    //判断点是否在圆内（椭圆方程）
                    float normU = (pixelPos.X - pixelCircleCenterX) / pixelRadiusU;
                    float normV = (pixelPos.Y - pixelCircleCenterY) / pixelRadiusV;
                    float norm = normU * normU + normV * normV;
                    if (norm > 1.0f)
                    {
                        continue;
                    }

                    //还原HU值
                    float huValue = sliceVoxels[y * sliceSize.X + x];

                    //统计
                    if (huValue < result.MinHU)
                    {
                        result.MinHU = huValue;
                    }
                    if (huValue > result.MaxHU)
                    {
                        result.MaxHU = huValue;
                    }
                    result.HuSum += huValue;
                    result.HuSumSq += huValue * huValue;
                }
            }

            return result;
        }
        #endregion
    }
}
