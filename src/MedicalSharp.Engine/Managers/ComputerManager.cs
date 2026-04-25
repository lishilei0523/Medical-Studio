using MedicalSharp.Engine.Resources;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Threading;

namespace MedicalSharp.Engine.Managers
{
    /// <summary>
    /// 计算着色器管理器
    /// </summary>
    public static class ComputerManager
    {
        #region # 字段及构造器

        /// <summary>
        /// 是否已初始化
        /// </summary>
        private static bool _Initialized;

        /// <summary>
        /// 立方体计算着色器
        /// </summary>
        private static ShaderProgram _BoxComputer;

        /// <summary>
        /// 同步锁
        /// </summary>
        private static readonly Lock _Sync;

        /// <summary>
        /// 静态构造器
        /// </summary>
        static ComputerManager()
        {
            _Initialized = false;
            _Sync = new Lock();
        }

        #endregion

        #region # 属性

        #region 只读属性 - 立方体计算着色器 —— static ShaderProgram BoxComputer
        /// <summary>
        /// 只读属性 - 立方体计算着色器
        /// </summary>
        public static ShaderProgram BoxComputer
        {
            get => _BoxComputer;
        }
        #endregion

        #endregion

        #region # 方法

        //Public

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

                _BoxComputer = CreateBoxComputer();
                _Initialized = true;
            }
        }
        #endregion

        #region 调度计算着色器 —— static void DispatchCompute(int width, int height, int depth)
        /// <summary>
        /// 调度计算着色器
        /// </summary>
        public static void DispatchCompute(int width, int height, int depth)
        {
            //计算工作组数量（每组8×8×8线程）
            int groupsX = (int)MathF.Ceiling(width / 8.0f);
            int groupsY = (int)MathF.Ceiling(height / 8.0f);
            int groupsZ = (int)MathF.Ceiling(depth / 8.0f);

            //调度执行计算着色器
            GL.DispatchCompute(groupsX, groupsY, groupsZ);

            //内存屏障：确保计算完成后渲染能读到新数据
            GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);
        }
        #endregion

        #region 清理资源 —— static void Cleanup()
        /// <summary>
        /// 清理资源
        /// </summary>
        public static void Cleanup()
        {
            _BoxComputer.Dispose();
        }
        #endregion


        //Private

        #region 创建立方体计算着色器 —— static ShaderProgram CreateBoxComputer()
        /// <summary>
        /// 创建立方体计算着色器
        /// </summary>
        private static ShaderProgram CreateBoxComputer()
        {
            ShaderProgram program = new ShaderProgram();
            program.ReadComputeShaderFromFile("Resources/GLSLs/box_roi.comp");
            program.BuildCompute();

            return program;
        }
        #endregion 

        #endregion
    }
}
