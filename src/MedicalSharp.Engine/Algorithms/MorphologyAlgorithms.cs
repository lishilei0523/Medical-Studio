using MedicalSharp.Engine.Managers;
using MedicalSharp.Engine.Resources;
using MedicalSharp.Primitives.Models;
using OpenTK.Mathematics;
using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace MedicalSharp.Engine.Algorithms
{
    /// <summary>
    /// 形态学算法
    /// </summary>
    public static class MorphologyAlgorithms
    {
        //Public

        #region # 二值腐蚀 —— static void ErodeMark(this VolumeData volumeData...
        /// <summary>
        /// 二值腐蚀
        /// </summary>
        /// <param name="volumeData">体积数据</param>
        /// <param name="markTexture">标记纹理</param>
        /// <param name="markValue">标记值</param>
        /// <param name="kernelSize">核矩阵尺寸（3或5，默认3）</param>
        /// <param name="iterations">迭代次数（默认1）</param>
        /// <param name="syncToGpu">同步GPU端</param>
        /// <remarks>
        /// 将指定标记值的前景向内收缩，去除边缘毛刺、断开粘连结构；
        /// 原地修改MarkData，操作完成后同步到标记纹理；
        /// </remarks>
        public static unsafe void ErodeMark(this VolumeData volumeData, Texture3D markTexture, byte markValue, int kernelSize = 3, int iterations = 1, bool syncToGpu = true)
        {
            #region # 验证

            if (volumeData == null)
            {
                throw new ArgumentNullException(nameof(volumeData), "体积数据不可为空！");
            }
            if (volumeData.MarkData == IntPtr.Zero)
            {
                throw new ArgumentNullException(nameof(volumeData), "标记数据指针未分配！");
            }
            if (markValue == 0)
            {
                return;
            }

            #endregion

            int width = volumeData.Metadata.VolumeSize.X;
            int height = volumeData.Metadata.VolumeSize.Y;
            int depth = volumeData.Metadata.VolumeSize.Z;
            int totalVoxels = width * height * depth;
            int radius = kernelSize / 2;

            byte* markData = (byte*)volumeData.MarkData.ToPointer();
            for (int iteration = 0; iteration < iterations; iteration++)
            {
                //创建副本作为输入（避免腐蚀前沿被重复腐蚀）
                byte* markDataCopy = (byte*)NativeMemory.Alloc((uint)totalVoxels, sizeof(byte));
                Buffer.MemoryCopy(markData, markDataCopy, totalVoxels, totalVoxels);

                Partitioner<Tuple<int, int>> partitioner = Partitioner.Create(0, totalVoxels);
                Parallel.ForEach(partitioner, range =>
                {
                    for (int index = range.Item1; index < range.Item2; index++)
                    {
                        //只处理前景体素
                        if (markData[index] != markValue)
                        {
                            continue;
                        }

                        //检查邻域内是否有背景体素
                        if (HasNeighborMark(markDataCopy, volumeData.Metadata.VolumeSize, index, 0, radius))
                        {
                            markData[index] = 0;
                        }
                    }
                });

                //释放副本
                NativeMemory.Free(markDataCopy);
            }

            //同步到标记纹理
            if (syncToGpu)
            {
                volumeData.SyncMarkDataToGpu(markTexture);
            }
        }
        #endregion

        #region # 二值膨胀 —— static void DilateMark(this VolumeData volumeData...
        /// <summary>
        /// 二值膨胀
        /// </summary>
        /// <param name="volumeData">体积数据</param>
        /// <param name="markTexture">标记纹理</param>
        /// <param name="markValue">标记值</param>
        /// <param name="kernelSize">核矩阵尺寸（3或5，默认3）</param>
        /// <param name="iterations">迭代次数（默认1）</param>
        /// <param name="syncToGpu">同步GPU端</param>
        /// <remarks>
        /// 将指定标记值的前景向外扩张，填充内部空洞、连接断裂区域；
        /// 原地修改MarkData，操作完成后同步到标记纹理；
        /// </remarks>
        public static unsafe void DilateMark(this VolumeData volumeData, Texture3D markTexture, byte markValue, int kernelSize = 3, int iterations = 1, bool syncToGpu = true)
        {
            #region # 验证

            if (volumeData == null)
            {
                throw new ArgumentNullException(nameof(volumeData), "体积数据不可为空！");
            }
            if (volumeData.MarkData == IntPtr.Zero)
            {
                throw new ArgumentNullException(nameof(volumeData), "标记数据指针未分配！");
            }
            if (markValue == 0)
            {
                return;
            }

            #endregion

            int width = volumeData.Metadata.VolumeSize.X;
            int height = volumeData.Metadata.VolumeSize.Y;
            int depth = volumeData.Metadata.VolumeSize.Z;
            int totalVoxels = width * height * depth;
            int radius = kernelSize / 2;

            byte* markData = (byte*)volumeData.MarkData.ToPointer();
            for (int iteration = 0; iteration < iterations; iteration++)
            {
                //创建副本作为输入（避免膨胀前沿被重复膨胀）
                byte* markDataCopy = (byte*)NativeMemory.Alloc((uint)totalVoxels, sizeof(byte));
                Buffer.MemoryCopy(markData, markDataCopy, totalVoxels, totalVoxels);

                Partitioner<Tuple<int, int>> partitioner = Partitioner.Create(0, totalVoxels);
                Parallel.ForEach(partitioner, range =>
                {
                    for (int index = range.Item1; index < range.Item2; index++)
                    {
                        //只处理背景体素
                        if (markData[index] != 0)
                        {
                            continue;
                        }

                        //检查邻域内是否有前景标记值
                        if (HasNeighborMark(markDataCopy, volumeData.Metadata.VolumeSize, index, markValue, radius))
                        {
                            markData[index] = markValue;
                        }
                    }
                });

                //释放副本
                NativeMemory.Free(markDataCopy);
            }

            //同步到标记纹理
            if (syncToGpu)
            {
                volumeData.SyncMarkDataToGpu(markTexture);
            }
        }
        #endregion

        #region # 开运算 —— static void OpenMark(this VolumeData volumeData...
        /// <summary>
        /// 开运算
        /// </summary>
        /// <param name="volumeData">体积数据</param>
        /// <param name="markTexture">标记纹理</param>
        /// <param name="markValue">标记值</param>
        /// <param name="kernelSize">核矩阵尺寸（3或5，默认3）</param>
        /// <param name="iterations">迭代次数（默认1）</param>
        /// <param name="syncToGpu">同步GPU端</param>
        /// <remarks>先腐蚀后膨胀，平滑轮廓、去除孤立噪点，保持主体大小不变</remarks>
        public static void OpenMark(this VolumeData volumeData, Texture3D markTexture, byte markValue, int kernelSize = 3, int iterations = 1, bool syncToGpu = true)
        {
            volumeData.ErodeMark(markTexture, markValue, kernelSize, iterations, false);
            volumeData.DilateMark(markTexture, markValue, kernelSize, iterations, false);

            //同步到标记纹理
            if (syncToGpu)
            {
                volumeData.SyncMarkDataToGpu(markTexture);
            }
        }
        #endregion

        #region # 闭运算 —— static void CloseMark(this VolumeData volumeData...
        /// <summary>
        /// 闭运算
        /// </summary>
        /// <param name="volumeData">体积数据</param>
        /// <param name="markTexture">标记纹理</param>
        /// <param name="markValue">标记值</param>
        /// <param name="kernelSize">核矩阵尺寸（3或5，默认3）</param>
        /// <param name="iterations">迭代次数（默认1）</param>
        /// <param name="syncToGpu">同步GPU端</param>
        /// <remarks>先膨胀后腐蚀，填充内部小孔、连接邻近区域，保持主体大小不变</remarks>
        public static void CloseMark(this VolumeData volumeData, Texture3D markTexture, byte markValue, int kernelSize = 3, int iterations = 1, bool syncToGpu = true)
        {
            volumeData.DilateMark(markTexture, markValue, kernelSize, iterations, false);
            volumeData.ErodeMark(markTexture, markValue, kernelSize, iterations, false);

            //同步到标记纹理
            if (syncToGpu)
            {
                volumeData.SyncMarkDataToGpu(markTexture);
            }
        }
        #endregion


        //Private

        #region # 检查邻域内是否有目标标记值 —— static bool HasNeighborMark(byte* markData...
        /// <summary>
        /// 检查邻域内是否有目标标记值
        /// </summary>
        /// <param name="markData">标记数据指针</param>
        /// <param name="volumeSize">体积尺寸</param>
        /// <param name="voxelIndex">当前体素一维线性索引</param>
        /// <param name="markValue">目标标记值</param>
        /// <param name="radius">邻域半径（核矩阵尺寸的一半）</param>
        /// <returns>邻域内是否存在目标标记值</returns>
        /// <remarks>
        /// 遍历以当前体素为中心的(2 * radius + 1)³邻域，检查是否存在值为markValue的体素；
        /// 用于二值膨胀（检查邻域是否有前景）和二值腐蚀（检查邻域是否有背景）；
        /// 边界体素自动跳过超出体积范围的邻域；
        /// </remarks>
        private static unsafe bool HasNeighborMark(byte* markData, Vector3i volumeSize, int voxelIndex, byte markValue, int radius)
        {
            //将一维线性索引转换为三维体素坐标
            int z = voxelIndex / (volumeSize.X * volumeSize.Y);
            int remainder = voxelIndex % (volumeSize.X * volumeSize.Y);
            int y = remainder / volumeSize.X;
            int x = remainder % volumeSize.X;

            //遍历立方体邻域
            for (int dz = -radius; dz <= radius; dz++)
            {
                int nz = z + dz;
                if (nz < 0 || nz >= volumeSize.Z)
                {
                    continue;
                }

                for (int dy = -radius; dy <= radius; dy++)
                {
                    int ny = y + dy;
                    if (ny < 0 || ny >= volumeSize.Y)
                    {
                        continue;
                    }

                    for (int dx = -radius; dx <= radius; dx++)
                    {
                        int nx = x + dx;
                        if (nx < 0 || nx >= volumeSize.X)
                        {
                            continue;
                        }

                        //找到目标标记值，立即返回
                        int neighborVoxelIndex = nz * volumeSize.X * volumeSize.Y + ny * volumeSize.X + nx;
                        if (markData[neighborVoxelIndex] == markValue)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
        #endregion
    }
}
