using Silk.NET.OpenCL;
using System;
using System.IO;
using System.Text;

namespace MedicalSharp.Inspiration.Resources
{
    /// <summary>
    /// OpenCL程序
    /// </summary>
    /// <remarks>对应一个.cl源文件的编译产物，一个ClProgram可以提取多个ClKernel</remarks>
    public sealed class ClProgram : IDisposable
    {
        private readonly CL _cl;
        private readonly nint _device;
        private bool _disposed;

        /// <summary>
        /// OpenCL 程序句柄
        /// </summary>
        public nint Handle { get; private set; }

        private ClProgram(CL cl, nint handle, nint device)
        {
            this._cl = cl;
            this.Handle = handle;
            this._device = device;
        }

        /// <summary>
        /// 从 .cl 源文件编译程序
        /// </summary>
        /// <param name="context">OpenCL 上下文</param>
        /// <param name="filePath">.cl 文件路径</param>
        /// <returns>编译好的 ClProgram</returns>
        public static ClProgram FromFile(ClContext context, string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new ClException($"内核文件不存在: {filePath}");
            }

            string source = File.ReadAllText(filePath);
            return ClProgram.FromSource(context, source);
        }

        /// <summary>
        /// 从源码字符串编译程序
        /// </summary>
        /// <param name="context">OpenCL 上下文</param>
        /// <param name="source">内核源码</param>
        /// <returns>编译好的 ClProgram</returns>
        public static unsafe ClProgram FromSource(ClContext context, string source)
        {
            CL cl = CL.GetApi();

            // 创建程序
            nint handle = cl.CreateProgramWithSource(context.Handle, 1, [source], null, out int err);
            ClException.ThrowOnError(err, "CreateProgramWithSource");
            if (handle == 0)
            {
                throw new ClException("CreateProgramWithSource 返回空句柄");
            }

            // 编译
            IntPtr device = context.Device;
            err = cl.BuildProgram(handle, 1, in device, (byte*)null, null, null);
            if (err != 0)
            {
                // 获取编译错误日志
                string buildLog = ClProgram.GetBuildLog(cl, handle, context.Device);
                cl.ReleaseProgram(handle);
                throw new ClException($"程序编译失败：\r\n{buildLog}");
            }

            return new ClProgram(cl, handle, context.Device);
        }

        /// <summary>
        /// 从程序创建内核
        /// </summary>
        /// <param name="kernelName">内核函数名（.cl 文件中的 kernel void xxx）</param>
        /// <returns>ClKernel 实例</returns>
        public ClKernel CreateKernel(string kernelName)
        {
            nint handle = this._cl.CreateKernel(this.Handle, kernelName, out int err);
            ClException.ThrowOnError(err, $"CreateKernel({kernelName})");
            if (handle == 0)
            {
                throw new ClException($"创建内核 '{kernelName}' 失败：返回空句柄");
            }

            return new ClKernel(this._cl, handle, kernelName);
        }

        public void Dispose()
        {
            if (this._disposed)
            {
                return;
            }
            if (this.Handle != 0)
            {
                this._cl.ReleaseProgram(this.Handle);
                this.Handle = 0;
            }
            this._disposed = true;
        }

        /// <summary>
        /// 获取编译错误日志
        /// </summary>
        private static unsafe string GetBuildLog(CL cl, nint program, nint device)
        {
            cl.GetProgramBuildInfo(program, device, ProgramBuildInfo.BuildLog, 0, null, out nuint size);
            if (size == 0)
            {
                return "(无编译日志)";
            }

            byte[] buffer = new byte[size];
            fixed (byte* ptr = buffer)
            {
                cl.GetProgramBuildInfo(program, device, ProgramBuildInfo.BuildLog, size, ptr, out _);
            }

            return Encoding.UTF8.GetString(buffer).TrimEnd('\0');
        }
    }
}
