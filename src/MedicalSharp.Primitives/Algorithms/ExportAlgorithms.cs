using MedicalSharp.Primitives.Models;
using OpenTK.Mathematics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicalSharp.Primitives.Algorithms
{
    /// <summary>
    /// 导出算法
    /// </summary>
    public static class ExportAlgorithms
    {
        #region # 导出点云 —— static IReadOnlyList<Vector4> ExportPointCloud(this VolumeData volumeData...
        /// <summary>
        /// 导出点云
        /// </summary>
        /// <param name="volumeData">体积数据</param>
        /// <param name="markValue">标记值</param>
        /// <returns>点云</returns>
        public static unsafe IReadOnlyList<Vector4> ExportPointCloud(this VolumeData volumeData, byte markValue)
        {
            #region # 验证

            if (volumeData == null)
            {
                throw new ArgumentNullException(nameof(volumeData), "体积数据不可为空！");
            }
            if (volumeData.OriginalData == IntPtr.Zero)
            {
                throw new ArgumentNullException(nameof(volumeData), "原始数据指针未分配！");
            }
            if (volumeData.MarkData == IntPtr.Zero)
            {
                throw new ArgumentNullException(nameof(volumeData), "标记数据指针未分配！");
            }

            #endregion

            int width = volumeData.Metadata.VolumeSize.X;
            int height = volumeData.Metadata.VolumeSize.Y;
            int depth = volumeData.Metadata.VolumeSize.Z;
            long totalVoxels = (long)width * height * depth;

            short* dataPtr = (short*)volumeData.OriginalData.ToPointer();
            byte* markPtr = (byte*)volumeData.MarkData.ToPointer();
            Vector3 spacing = volumeData.Metadata.Spacing;

            //定义点云容器
            ConcurrentBag<Vector4> pointCloud = [];

            //动态分块
            Partitioner<Tuple<long, long>> partitioner = Partitioner.Create(0L, totalVoxels);
            Parallel.ForEach(partitioner, range =>
            {
                for (long index = range.Item1; index < range.Item2; index++)
                {
                    if (markPtr[index] == markValue)
                    {
                        //计算体素坐标
                        int z = (int)(index / (width * height));
                        int remainder = (int)(index % (width * height));
                        int y = remainder / width;
                        int x = remainder % width;

                        //转换为毫米坐标
                        float mmX = x * spacing.X;
                        float mmY = y * spacing.Y;
                        float mmZ = z * spacing.Z;

                        //获取HU值
                        float hu = dataPtr[index];

                        //添加点
                        Vector4 point = new Vector4(mmX, mmY, mmZ, hu);
                        pointCloud.Add(point);
                    }
                }
            });

            return [.. pointCloud];
        }
        #endregion

        #region # 编码PCD格式点云 —— static string EncodePointCloudToPCD(IReadOnlyList<Vector4> pointCloud)
        /// <summary>
        /// 编码PCD格式点云
        /// </summary>
        /// <param name="pointCloud">点云</param>
        /// <returns>PCD文本</returns>
        public static string EncodePointCloudToPCD(IReadOnlyList<Vector4> pointCloud)
        {
            #region # 验证

            if (pointCloud == null || !pointCloud.Any())
            {
                return string.Empty;
            }

            #endregion

            StringBuilder pcdBuilder = new StringBuilder();

            //PCD文件头
            pcdBuilder.AppendLine("# .PCD v0.7 - Point Cloud Data file format");
            pcdBuilder.AppendLine("VERSION 0.7");
            pcdBuilder.AppendLine("FIELDS x y z intensity");
            pcdBuilder.AppendLine("SIZE 4 4 4 4");
            pcdBuilder.AppendLine("TYPE F F F F");
            pcdBuilder.AppendLine("COUNT 1 1 1 1");
            pcdBuilder.AppendLine($"WIDTH {pointCloud.Count}");
            pcdBuilder.AppendLine("HEIGHT 1");
            pcdBuilder.AppendLine("VIEWPOINT 0 0 0 1 0 0 0");
            pcdBuilder.AppendLine($"POINTS {pointCloud.Count}");
            pcdBuilder.AppendLine("DATA ascii");

            //点数据
            foreach (Vector4 point in pointCloud)
            {
                pcdBuilder.AppendLine($"{point.X:F6} {point.Y:F6} {point.Z:F6} {point.W:F6}");
            }

            return pcdBuilder.ToString();
        }
        #endregion

        #region # 编码PLY格式点云 —— static string EncodePointCloudToPLY(IReadOnlyList<Vector4> pointCloud)
        /// <summary>
        /// 编码PLY格式点云
        /// </summary>
        /// <param name="pointCloud">点云</param>
        /// <returns>PLY文本</returns>
        public static string EncodePointCloudToPLY(IReadOnlyList<Vector4> pointCloud)
        {
            #region # 验证

            if (pointCloud == null || !pointCloud.Any())
            {
                return string.Empty;
            }

            #endregion

            StringBuilder plyBuilder = new StringBuilder();

            //PLY文件头
            plyBuilder.AppendLine("ply");
            plyBuilder.AppendLine("format ascii 1.0");
            plyBuilder.AppendLine($"element vertex {pointCloud.Count}");
            plyBuilder.AppendLine("property float x");
            plyBuilder.AppendLine("property float y");
            plyBuilder.AppendLine("property float z");
            plyBuilder.AppendLine("property float intensity");
            plyBuilder.AppendLine("end_header");

            //点数据
            foreach (Vector4 point in pointCloud)
            {
                plyBuilder.AppendLine($"{point.X:F6} {point.Y:F6} {point.Z:F6} {point.W:F6}");
            }

            return plyBuilder.ToString();
        }
        #endregion
    }
}
