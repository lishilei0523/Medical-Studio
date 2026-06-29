using MedicalSharp.Inspiration.Resources;
using Silk.NET.OpenCL;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace MedicalSharp.Inspiration.Operators
{
    /// <summary>
    /// 3D区域生长算子
    /// </summary>
    public sealed class RegionGrowing3D : IDisposable
    {
        #region # 字段及构造器

        /// <summary>
        /// 释放标识
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// OpenCL上下文
        /// </summary>
        private readonly ClContext _clContext;

        /// <summary>
        /// OpenCL程序
        /// </summary>
        private readonly ClProgram _program;

        /// <summary>
        /// OpenCL内核
        /// </summary>
        private readonly ClKernel _kernel;

        /// <summary>
        /// 创建3D区域生长算子构造器
        /// </summary>
        /// <param name="clContext">OpenCL上下文</param>
        public RegionGrowing3D(ClContext clContext)
        {
            this._clContext = clContext;
            this._program = ClProgram.FromFile(this._clContext, "Resources/Kernels/region_grow_3d.cl");
            this._kernel = this._program.CreateKernel("region_grow");
        }

        #endregion

        #region # 属性

        #region 只读属性 - OpenCL程序 —— ClProgram Program
        /// <summary>
        /// 只读属性 - OpenCL程序
        /// </summary>
        public ClProgram Program
        {
            get => this._program;
        }
        #endregion

        #region 只读属性 - OpenCL内核 —— ClKernel Kernel
        /// <summary>
        /// 只读属性 - OpenCL内核
        /// </summary>
        public ClKernel Kernel
        {
            get => this._kernel;
        }
        #endregion

        #endregion

        #region # 方法

        #region 执行区域生长 —— bool Execute(ClImage3D image, IntPtr markData...
        /// <summary>
        /// 执行区域生长
        /// </summary>
        /// <param name="image">输入图像</param>
        /// <param name="markData">标记数据</param>
        /// <param name="minHU">最小HU值</param>
        /// <param name="maxHU">最大HU值</param>
        /// <param name="markValue">种子点标记值</param>
        /// <param name="maxIterations">最大迭代次数</param>
        /// <returns>是否成功生长（至少有一个新体素被标记）</returns>
        public unsafe bool Execute(ClImage3D image, IntPtr markData, float minHU, float maxHU, byte markValue, int maxIterations = 100)
        {
            #region # 验证

            if (markValue == 0)
            {
                return false;
            }
            if (minHU >= maxHU)
            {
                throw new ArgumentOutOfRangeException(nameof(minHU), "最小HU值必须小于最大HU值！");
            }
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image), "预览图像不可为空！");
            }
            if (markData == IntPtr.Zero)
            {
                throw new ArgumentNullException(nameof(markData), "标记数据不可为空！");
            }

            #endregion

            //定义临时标记
            const byte tempMarkA = 254;
            const byte tempMarkB = 255;

            int width = image.Width;
            int height = image.Height;
            int depth = image.Depth;
            int voxelsCount = width * height * depth;

            //创建乒乓缓冲区：ping持有当前标记数据，pong用于写入本轮结果
            ClBuffer pingBuffer = ClBuffer.Create(this._clContext, MemFlags.ReadWrite, markData, voxelsCount * sizeof(byte));
            ClBuffer pongBuffer = ClBuffer.CreateEmpty<byte>(this._clContext, MemFlags.ReadWrite, voxelsCount);

            //创建原子计数器缓冲区
            using ClBuffer countBuffer = ClBuffer.CreateEmpty<uint>(this._clContext, MemFlags.ReadWrite, 1);

            byte prevTempMark = markValue;      // 第一轮检查的"种子"是原始种子点
            byte currentTempMark = tempMarkA;   // 第一轮写入的临时标记
            bool hasNewVoxels = false;
            for (int iteration = 0; iteration < maxIterations; iteration++)
            {
                //重置原子计数器
                countBuffer.Write(this._clContext.CommandQueue, [0]);

                //设置参数
                this._kernel.SetImageKernelArg(0, image.Handle);
                this._kernel.SetBufferKernelArg(1, pingBuffer);
                this._kernel.SetBufferKernelArg(2, pongBuffer);
                this._kernel.SetKernelArg(3, minHU);
                this._kernel.SetKernelArg(4, maxHU);
                this._kernel.SetKernelArg(5, markValue);
                this._kernel.SetKernelArg(6, prevTempMark);
                this._kernel.SetKernelArg(7, currentTempMark);
                this._kernel.SetBufferKernelArg(8, countBuffer);

                //调度执行
                this._kernel.Enqueue3D(this._clContext.CommandQueue, (uint)width, (uint)height, (uint)depth);
                this._clContext.Finish();

                //检查本轮是否有新体素加入
                uint[] count = countBuffer.Read<uint>(this._clContext.CommandQueue, 1);
                hasNewVoxels = count[0] > 0;
                if (!hasNewVoxels)
                {
                    break;
                }

                //交换绑定缓冲区：ping <-> pong
                (pingBuffer, pongBuffer) = (pongBuffer, pingBuffer);

                //交换临时标记值
                byte swap = prevTempMark;
                prevTempMark = currentTempMark;
                currentTempMark = (swap == tempMarkA) ? tempMarkB : tempMarkA;
            }

            //将最终结果拷贝回标记纹理
            pingBuffer.Read(this._clContext.CommandQueue, markData);

            //将临时标记统一替换为种子点标记值
            byte* markDataPointer = (byte*)markData.ToPointer();
            Partitioner<Tuple<int, int>> partitioner = Partitioner.Create(0, voxelsCount);
            Parallel.ForEach(partitioner, range =>
            {
                for (int index = range.Item1; index < range.Item2; index++)
                {
                    if (markDataPointer[index] == tempMarkA || markDataPointer[index] == tempMarkB)
                    {
                        markDataPointer[index] = markValue;
                    }
                }
            });

            //释放临时缓冲区
            pingBuffer.Dispose();
            pongBuffer.Dispose();

            return hasNewVoxels;
        }
        #endregion

        #region 释放资源 —— void Dispose()
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (this._disposed)
            {
                return;
            }

            this._kernel?.Dispose();
            this._program?.Dispose();
            this._disposed = true;
        }
        #endregion

        #endregion
    }
}
