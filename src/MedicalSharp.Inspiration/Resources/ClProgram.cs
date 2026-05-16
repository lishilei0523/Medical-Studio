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
        #region # 字段及构造器

        /// <summary>
        /// OpenCL 2.0支持宏
        /// </summary>
        private const string V20SupportOptions = "-cl-std=CL2.0 -D__OPENCL_VERSION__=200";

        /// <summary>
        /// 释放标识
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// OpenCL实例
        /// </summary>
        private readonly CL _cl;

        /// <summary>
        /// 创建OpenCL程序构造器
        /// </summary>
        /// <param name="cl">OpenCL实例</param>
        /// <param name="handle">程序句柄</param>
        /// <param name="device">设备句柄</param>
        private ClProgram(CL cl, IntPtr handle, IntPtr device)
        {
            this._cl = cl;
            this.Handle = handle;
            this.Device = device;
        }

        #endregion

        #region # 属性

        #region 程序句柄 —— IntPtr Handle
        /// <summary>
        /// 程序句柄
        /// </summary>
        public IntPtr Handle { get; private set; }
        #endregion

        #region 设备句柄 —— IntPtr Device
        /// <summary>
        /// 设备句柄
        /// </summary>
        public IntPtr Device { get; private set; }
        #endregion

        #endregion

        #region # 方法

        #region 从文件创建OpenCL程序 —— static ClProgram FromFile(ClContext clContext...
        /// <summary>
        /// 从文件创建OpenCL程序
        /// </summary>
        /// <param name="clContext">OpenCL上下文</param>
        /// <param name="filePath">文件路径</param>
        /// <returns>OpenCL程序</returns>
        public static ClProgram FromFile(ClContext clContext, string filePath)
        {
            #region # 验证

            if (!File.Exists(filePath))
            {
                throw new ClException($"内核文件不存在: {filePath}");
            }

            #endregion

            string source = File.ReadAllText(filePath);
            ClProgram program = FromSource(clContext, source);

            return program;
        }
        #endregion

        #region 从文本创建OpenCL程序 —— static unsafe ClProgram FromSource(ClContext clContext...
        /// <summary>
        /// 从文本创建OpenCL程序
        /// </summary>
        /// <param name="clContext">OpenCL上下文</param>
        /// <param name="sourceText">内核源文本</param>
        /// <returns>OpenCL程序</returns>
        public static unsafe ClProgram FromSource(ClContext clContext, string sourceText)
        {
            CL cl = CL.GetApi();

            //创建程序
            IntPtr handle = cl.CreateProgramWithSource(clContext.Handle, 1, [sourceText], null, out int errorCode);
            ClException.ThrowOnError(errorCode, "CreateProgramWithSource");
            if (handle == IntPtr.Zero)
            {
                throw new ClException("CreateProgramWithSource 返回空句柄");
            }

            //编译程序
            IntPtr device = clContext.Device;
            string options = clContext.SupportsV20 ? V20SupportOptions : string.Empty;
            errorCode = cl.BuildProgram(handle, 1, in device, options, null, null);
            if (errorCode != (int)ErrorCodes.Success)
            {
                //获取编译错误日志
                string buildLog = GetBuildLog(cl, handle, clContext.Device);
                cl.ReleaseProgram(handle);
                throw new ClException($"程序编译失败：\r\n{buildLog}");
            }

            ClProgram program = new ClProgram(cl, handle, clContext.Device);

            return program;
        }
        #endregion

        #region 创建OpenCL内核 —— ClKernel CreateKernel(string kernelName)
        /// <summary>
        /// 创建OpenCL内核
        /// </summary>
        /// <param name="kernelName">内核函数名</param>
        /// <returns>OpenCL内核实例</returns>
        public ClKernel CreateKernel(string kernelName)
        {
            IntPtr handle = this._cl.CreateKernel(this.Handle, kernelName, out int errorCode);
            ClException.ThrowOnError(errorCode, $"CreateKernel({kernelName})");
            if (handle == IntPtr.Zero)
            {
                throw new ClException($"创建内核 '{kernelName}' 失败：返回空句柄");
            }

            ClKernel kernel = new ClKernel(this._cl, handle, kernelName);

            return kernel;
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
            if (this.Handle != 0)
            {
                this._cl.ReleaseProgram(this.Handle);
                this.Handle = 0;
            }
            this._disposed = true;
        }
        #endregion

        #region 获取编译错误日志 —— static unsafe string GetBuildLog(CL cl...
        /// <summary>
        /// 获取编译错误日志
        /// </summary>
        /// <param name="cl">OpenCL实例</param>
        /// <param name="program">OpenCL程序句柄</param>
        /// <param name="device">设备句柄</param>
        /// <returns>日志</returns>
        private static unsafe string GetBuildLog(CL cl, IntPtr program, IntPtr device)
        {
            cl.GetProgramBuildInfo(program, device, ProgramBuildInfo.BuildLog, 0, null, out UIntPtr size);

            if (size == UIntPtr.Zero)
            {
                return "(无编译日志)";
            }

            byte[] buffer = new byte[size];
            fixed (byte* ptr = buffer)
            {
                cl.GetProgramBuildInfo(program, device, ProgramBuildInfo.BuildLog, size, ptr, out _);
            }

            string log = Encoding.UTF8.GetString(buffer).TrimEnd('\0');

            return log;
        }
        #endregion 

        #endregion
    }
}
