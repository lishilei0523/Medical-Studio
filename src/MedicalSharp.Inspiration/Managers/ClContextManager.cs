using MedicalSharp.Inspiration.Resources;
using MedicalSharp.Primitives.Enums;
using System;
using System.Threading;

namespace MedicalSharp.Inspiration.Managers
{
    /// <summary>
    /// OpenCL上下文管理器
    /// </summary>
    public static class ClContextManager
    {
        #region # 字段及构造器

        /// <summary>
        /// 同步锁
        /// </summary>
        private static readonly Lock _Sync;

        /// <summary>
        /// 是否已初始化
        /// </summary>
        private static bool _Initialized;

        /// <summary>
        /// OpenCL上下文
        /// </summary>
        private static ClContext _ClContext;

        /// <summary>
        /// 静态构造器
        /// </summary>
        static ClContextManager()
        {
            _Sync = new Lock();
            _Initialized = false;
        }

        #endregion

        #region # 属性

        #region 只读属性 - 当前OpenCL上下文 —— static ClContext Current
        /// <summary>
        /// 只读属性 - 当前OpenCL上下文
        /// </summary>
        public static ClContext Current
        {
            get => _ClContext;
        }
        #endregion

        #endregion

        #region # 方法

        #region 初始化 —— static void Initialize()
        /// <summary>
        /// 初始化
        /// </summary>
        public static void Initialize()
        {
            lock (_Sync)
            {
                if (_Initialized)
                {
                    return;
                }

                _ClContext = ClContext.Create();
                _Initialized = true;
            }
        }
        #endregion

        #region OpenGL共享初始化 —— static void InitializeWithGL()
        /// <summary>
        /// OpenGL共享初始化
        /// </summary>
        /// <param name="platform">平台操作系统</param>
        /// <param name="glContext">OpenGL上下文句柄</param>
        /// <param name="displayHandle">平台显示句柄（Windows: HDC, Linux: Display）</param>
        public static void InitializeWithGL(PlatformOS platform, IntPtr glContext, IntPtr displayHandle)
        {
            lock (_Sync)
            {
                if (_Initialized)
                {
                    return;
                }

                _ClContext = ClContext.CreateWithGL(platform, glContext, displayHandle);
                _Initialized = true;
            }
        }
        #endregion

        #region 清理资源 —— static void Cleanup()
        /// <summary>
        /// 清理资源
        /// </summary>
        public static void Cleanup()
        {
            _ClContext?.Dispose();
        }
        #endregion 

        #endregion
    }
}
