using MedicalSharp.Primitives.Enums;
using Silk.NET.OpenCL;
using System;
using System.Text;

namespace MedicalSharp.Inspiration.Resources
{
    /// <summary>
    /// OpenCL上下文
    /// </summary>
    /// <remarks>管理平台、设备、命令队列。通常一个应用只创建一个，生命周期与应用相同</remarks>
    public sealed class ClContext : IDisposable
    {
        #region # 字段及构造器

        /// <summary>
        /// OpenGL上下文句柄
        /// </summary>
        /// <remarks>cl_khr_gl_sharing扩展常量</remarks>
        private const int GLContextKhr = 0x2008;

        /// <summary>
        /// 释放标识
        /// </summary>
        private bool _disposed;

        /// <summary>
        /// OpenCL实例
        /// </summary>
        private readonly CL _cl;

        /// <summary>
        /// 创建OpenCL上下文构造器
        /// </summary>
        /// <param name="cl">OpenCL实例</param>
        /// <param name="context">上下文句柄</param>
        /// <param name="device">设备句柄</param>
        /// <param name="commandQueue">命令队列句柄</param>
        /// <param name="deviceName">设备名称</param>
        /// <param name="globalMemorySize">设备全局显存大小</param>
        /// <param name="supportsV20">是否支持OpenCL 2.0</param>
        private ClContext(CL cl, IntPtr context, IntPtr device, IntPtr commandQueue, string deviceName, ulong globalMemorySize, bool supportsV20)
        {
            this._cl = cl;
            this.Handle = context;
            this.Device = device;
            this.CommandQueue = commandQueue;
            this.DeviceName = deviceName;
            this.GlobalMemorySize = globalMemorySize;
            this.SupportsV20 = supportsV20;
        }

        #endregion

        #region # 属性

        #region 上下文句柄 —— IntPtr Handle
        /// <summary>
        /// 上下文句柄
        /// </summary>
        public IntPtr Handle { get; private set; }
        #endregion

        #region 设备句柄 —— IntPtr Device
        /// <summary>
        /// 设备句柄
        /// </summary>
        public IntPtr Device { get; private set; }
        #endregion

        #region 命令队列句柄 —— IntPtr CommandQueue
        /// <summary>
        /// 命令队列句柄
        /// </summary>
        public IntPtr CommandQueue { get; private set; }
        #endregion

        #region 设备名称 —— string DeviceName
        /// <summary>
        /// 设备名称
        /// </summary>
        /// <remarks>例：NVIDIA GeForce RTX 3060</remarks>
        public string DeviceName { get; private set; }
        #endregion

        #region 全局显存大小 —— ulong GlobalMemorySize
        /// <summary>
        /// 全局显存大小
        /// </summary>
        /// <remarks>单位：MB</remarks>
        public ulong GlobalMemorySize { get; private set; }
        #endregion

        #region 是否支持2.0 —— bool SupportsV20
        /// <summary>
        /// 是否支持2.0
        /// </summary>
        public bool SupportsV20 { get; private set; }
        #endregion

        #endregion

        #region # 方法

        //Public

        #region 创建默认OpenCL上下文 —— static ClContext Create()
        /// <summary>
        /// 创建默认OpenCL上下文
        /// </summary>
        /// <returns>OpenCL上下文实例</returns>
        /// <remarks>优先选择GPU，不可用时回退到CPU</remarks>
        public static unsafe ClContext Create()
        {
            CL cl = CL.GetApi();

            //获取第一个平台
            int errorCode = cl.GetPlatformIDs(1, out IntPtr platformId, out uint platformsCount);
            if (errorCode != (int)ErrorCodes.Success || platformsCount == 0)
            {
                throw new ClException("找不到任何OpenCL平台！");
            }

            //创建上下文（GPU优先，CPU回退）
            IntPtr[] contextProps = [(IntPtr)ContextProperties.Platform, platformId, IntPtr.Zero];
            IntPtr context;
            IntPtr device;
            fixed (IntPtr* propsPtr = contextProps)
            {
                context = cl.CreateContextFromType(propsPtr, DeviceType.Gpu, null, null, out errorCode);
                if (errorCode == (int)ErrorCodes.Success)
                {
                    device = GetFirstDevice(cl, context);
                }
                else
                {
                    context = cl.CreateContextFromType(propsPtr, DeviceType.Cpu, null, null, out errorCode);
                    ClException.ThrowOnError(errorCode, "CreateContextFromType(CPU)");
                    device = GetFirstDevice(cl, context);
                }
            }

            //创建命令队列
            IntPtr queue = cl.CreateCommandQueue(context, device, CommandQueueProperties.None, out errorCode);
            ClException.ThrowOnError(errorCode, "CreateCommandQueue");
            if (queue == IntPtr.Zero)
            {
                cl.ReleaseContext(context);
                throw new ClException("创建命令队列失败：返回空句柄！");
            }

            //查询设备信息
            string deviceName = GetDeviceInfoString(cl, device, DeviceInfo.Name);
            ulong globalMemorySize = GetDeviceInfoUlong(cl, device, DeviceInfo.GlobalMemSize) / (1024 * 1024);
            string version = GetDeviceInfoString(cl, device, DeviceInfo.Version);
            bool supportsV20 = version.Contains("2.0");

            ClContext clContext = new ClContext(cl, context, device, queue, deviceName, globalMemorySize, supportsV20);

            return clContext;
        }
        #endregion

        #region 创建OpenGL共享OpenCL上下文 —— static ClContext CreateWithGL(...
        /// <summary>
        /// 创建OpenGL共享OpenCL上下文
        /// </summary>
        /// <param name="platform">平台操作系统</param>
        /// <param name="glContext">OpenGL上下文句柄</param>
        /// <param name="displayHandle">平台显示句柄（Windows: HDC, Linux: Display）</param>
        /// <returns>OpenCL上下文实例</returns>
        /// <remarks>
        /// 用于cl_khr_gl_sharing互操作，使OpenCL可以直接读写GL纹理、内存缓冲区
        /// 调用前必须确保：
        /// 1. GL Context在当前线程上是current的
        /// 2. 需要共享的GL纹理已经创建
        /// 3. 之后创建的GL纹理也能被共享
        /// </remarks>
        public static unsafe ClContext CreateWithGL(PlatformOS platform, IntPtr glContext, IntPtr displayHandle)
        {
            #region # 验证

            if (glContext == IntPtr.Zero)
            {
                throw new ClException("OpenGL上下文句柄不可为空！");
            }
            if (platform != PlatformOS.Mac && displayHandle == IntPtr.Zero)
            {
                throw new ClException("平台显示句柄不可为空！");
            }

            #endregion

            CL cl = CL.GetApi();

            //获取OpenCL平台
            int errorCode = cl.GetPlatformIDs(1, out IntPtr platformId, out uint platformsCount);
            if (errorCode != (int)ErrorCodes.Success || platformsCount == 0)
            {
                throw new ClException("找不到任何OpenCL平台");
            }

            //获取GPU设备
            errorCode = cl.GetDeviceIDs(platformId, DeviceType.Gpu, 1, out IntPtr device, out uint devicesCount);
            if (errorCode != (int)ErrorCodes.Success || devicesCount == 0)
            {
                throw new ClException("找不到GPU设备，GL互操作需要GPU");
            }

            //验证cl_khr_gl_sharing扩展可用
            string deviceExtensions = GetDeviceInfoString(cl, device, DeviceInfo.Extensions);
            if (!deviceExtensions.Contains("cl_khr_gl_sharing"))
            {
                throw new ClException("当前GPU不支持cl_khr_gl_sharing扩展，无法与OpenGL共享资源！");
            }

            //构建互操作属性列表，属性按键值对排列，以0结尾
            IntPtr[] contextProperties;
            if (platform != PlatformOS.Mac)
            {
                contextProperties =
                [
                    (IntPtr)GLContextKhr, glContext,         //GLContext句柄
                    (IntPtr)platform, displayHandle,         //Windows: 设备上下文HDC | Linux: X11 Display
                    (IntPtr)ContextProperties.Platform, platformId,
                    IntPtr.Zero
                ];
            }
            else
            {
                //MacOS不需要显示句柄
                contextProperties =
                [
                    (IntPtr)GLContextKhr, glContext,
                    (IntPtr)ContextProperties.Platform, platformId,
                    IntPtr.Zero
                ];
            }

            //创建上下文（带GL互操作属性）
            IntPtr context;
            fixed (IntPtr* contextPropertiesPtr = contextProperties)
            {
                context = cl.CreateContext(contextPropertiesPtr, 1, in device, null, null, out errorCode);
                if (errorCode != (int)ErrorCodes.Success || context == IntPtr.Zero)
                {
                    string errorMsg = errorCode switch
                    {
                        -1000 => "无效的GL Context句柄！",
                        -1001 => "GL Context与设备不兼容！",
                        -999 => "设备不可用或已被其他上下文独占！",
                        _ => $"错误码: {errorCode}"
                    };
                    throw new ClException($"创建OpenGL互操作上下文失败！错误消息：{errorMsg}", errorCode);
                }
            }

            //创建命令队列
            IntPtr queue = cl.CreateCommandQueue(context, device, CommandQueueProperties.None, out errorCode);
            ClException.ThrowOnError(errorCode, "CreateCommandQueue");
            if (queue == IntPtr.Zero)
            {
                cl.ReleaseContext(context);
                throw new ClException("创建命令队列失败：返回空句柄！");
            }

            //查询设备信息
            string deviceName = GetDeviceInfoString(cl, device, DeviceInfo.Name);
            ulong globalMemorySize = GetDeviceInfoUlong(cl, device, DeviceInfo.GlobalMemSize) / (1024 * 1024);
            string version = GetDeviceInfoString(cl, device, DeviceInfo.Version);
            bool supportsV20 = version.Contains("2.0");

            ClContext clContext = new ClContext(cl, context, device, queue, deviceName, globalMemorySize, supportsV20);

            return clContext;
        }
        #endregion

        #region 创建命令队列 —— IntPtr CreateCommandQueue()
        /// <summary>
        /// 创建命令队列
        /// </summary>
        /// <returns>新命令队列句柄</returns>
        /// <remarks>
        /// 用于异步传输等高级场景，大多数情况使用默认的 CommandQueue 即可
        /// </remarks>
        public IntPtr CreateCommandQueue()
        {
            IntPtr queue = this._cl.CreateCommandQueue(this.Handle, this.Device, CommandQueueProperties.None, out int errorCode);
            ClException.ThrowOnError(errorCode, "CreateCommandQueue");

            return queue;
        }
        #endregion

        #region 释放命令队列 —— void ReleaseCommandQueue(IntPtr queue)
        /// <summary>
        /// 释放命令队列
        /// </summary>
        /// <param name="queue">命令队列句柄</param>
        /// <remarks>由调用方负责</remarks>
        public void ReleaseCommandQueue(IntPtr queue)
        {
            if (queue != IntPtr.Zero)
            {
                this._cl.ReleaseCommandQueue(queue);
            }
        }
        #endregion

        #region 清空命令队列 —— void Flush()
        /// <summary>
        /// 清空命令队列
        /// </summary>
        /// <remarks>不等待完成</remarks>
        public void Flush()
        {
            int errorCode = this._cl.Flush(this.CommandQueue);
            ClException.ThrowOnError(errorCode, "Flush");
        }
        #endregion

        #region 等待命令队列所有操作完成 —— void Finish()
        /// <summary>
        /// 等待命令队列所有操作完成
        /// </summary>
        public void Finish()
        {
            int errorCode = this._cl.Finish(this.CommandQueue);
            ClException.ThrowOnError(errorCode, "Finish");
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

            if (this.CommandQueue != IntPtr.Zero)
            {
                this._cl.ReleaseCommandQueue(this.CommandQueue);
                this.CommandQueue = IntPtr.Zero;
            }
            if (this.Handle != IntPtr.Zero)
            {
                this._cl.ReleaseContext(this.Handle);
                this.Handle = IntPtr.Zero;
            }
            this._disposed = true;
        }
        #endregion


        //Private

        #region 从上下文获取第一个设备 —— static IntPtr GetFirstDevice(CL cl, IntPtr context)
        /// <summary>
        /// 从上下文获取第一个设备
        /// </summary>
        /// <param name="cl">OpenCL实例</param>
        /// <param name="context">上下文句柄</param>
        /// <returns>设备句柄</returns>
        private static unsafe IntPtr GetFirstDevice(CL cl, IntPtr context)
        {
            int errorCode = cl.GetContextInfo(context, ContextInfo.Devices, 0, null, out UIntPtr size);
            ClException.ThrowOnError(errorCode, "GetContextInfo(Devices size)");

            IntPtr[] devices = new IntPtr[size / (UIntPtr)sizeof(UIntPtr)];
            fixed (IntPtr* pointer = devices)
            {
                errorCode = cl.GetContextInfo(context, ContextInfo.Devices, size, pointer, out _);
            }
            ClException.ThrowOnError(errorCode, "GetContextInfo(Devices)");

            if (devices.Length == 0)
            {
                throw new ClException("上下文中没有可用设备！");
            }

            return devices[0];
        }
        #endregion

        #region 获取设备字符串信息 —— static string GetDeviceInfoString(CL cl, IntPtr device...
        /// <summary>
        /// 获取设备字符串信息
        /// </summary>
        /// <param name="cl">OpenCL实例</param>
        /// <param name="device">设备句柄</param>
        /// <param name="deviceInfo">设备信息</param>
        /// <returns>字符串信息</returns>
        private static unsafe string GetDeviceInfoString(CL cl, IntPtr device, DeviceInfo deviceInfo)
        {
            cl.GetDeviceInfo(device, deviceInfo, 0, null, out UIntPtr size);
            byte[] buffer = new byte[size];
            fixed (byte* pointer = buffer)
            {
                cl.GetDeviceInfo(device, deviceInfo, size, pointer, out _);
            }
            string info = Encoding.UTF8.GetString(buffer).TrimEnd('\0');

            return info;
        }
        #endregion

        #region 获取设备64位无符号整型信息 —— static ulong GetDeviceInfoUlong(CL cl, IntPtr device...
        /// <summary>
        /// 获取设备64位无符号整型信息
        /// </summary>
        private static unsafe ulong GetDeviceInfoUlong(CL cl, IntPtr device, DeviceInfo deviceInfo)
        {
            ulong value = 0;
            cl.GetDeviceInfo(device, deviceInfo, sizeof(ulong), &value, out _);

            return value;
        }
        #endregion 

        #endregion
    }
}
