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
        /// 矩形切割计算着色器
        /// </summary>
        private static ShaderProgram _RectCutComputer;

        /// <summary>
        /// 圆形切割计算着色器
        /// </summary>
        private static ShaderProgram _CircleCutComputer;

        /// <summary>
        /// 椭圆形切割计算着色器
        /// </summary>
        private static ShaderProgram _EllipseCutComputer;

        /// <summary>
        /// 立方体切割计算着色器
        /// </summary>
        private static ShaderProgram _BoxCutComputer;

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

        #region 只读属性 - 矩形切割计算着色器 —— static ShaderProgram RectCutComputer
        /// <summary>
        /// 只读属性 - 矩形切割计算着色器
        /// </summary>
        public static ShaderProgram RectCutComputer
        {
            get => _RectCutComputer;
        }
        #endregion

        #region 只读属性 - 圆形切割计算着色器 —— static ShaderProgram CircleCutComputer
        /// <summary>
        /// 只读属性 - 圆形切割计算着色器
        /// </summary>
        public static ShaderProgram CircleCutComputer
        {
            get => _CircleCutComputer;
        }
        #endregion

        #region 只读属性 - 椭圆形切割计算着色器 —— static ShaderProgram EllipseCutComputer
        /// <summary>
        /// 只读属性 - 椭圆形切割计算着色器
        /// </summary>
        public static ShaderProgram EllipseCutComputer
        {
            get => _EllipseCutComputer;
        }
        #endregion

        #region 只读属性 - 立方体切割计算着色器 —— static ShaderProgram BoxCutComputer
        /// <summary>
        /// 只读属性 - 立方体切割计算着色器
        /// </summary>
        public static ShaderProgram BoxCutComputer
        {
            get => _BoxCutComputer;
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

                _BoxCutComputer = CreateBoxCutComputer();
                _CircleCutComputer = CreateCircleCutComputer();
                _EllipseCutComputer = CreateEllipseCutComputer();
                _RectCutComputer = CreateRectCutComputer();
                _Initialized = true;
            }
        }
        #endregion

        #region 调度计算着色器 —— static void DispatchCompute2D(int width, int height)
        /// <summary>
        /// 调度计算着色器
        /// </summary>
        public static void DispatchCompute2D(int width, int height)
        {
            //计算工作组数量（每组16×16线程）
            int groupsX = (int)MathF.Ceiling(width / 16.0f);
            int groupsY = (int)MathF.Ceiling(height / 16.0f);

            //调度执行计算着色器
            GL.DispatchCompute(groupsX, groupsY, 1);

            //内存屏障：确保计算完成后渲染能读到新数据
            GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);
        }
        #endregion

        #region 调度计算着色器 —— static void DispatchCompute3D(int width, int height, int depth)
        /// <summary>
        /// 调度计算着色器
        /// </summary>
        public static void DispatchCompute3D(int width, int height, int depth)
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
            _RectCutComputer?.Dispose();
            _CircleCutComputer?.Dispose();
            _EllipseCutComputer.Dispose();
            _BoxCutComputer?.Dispose();
        }
        #endregion


        //Private

        #region 创建矩形切割计算着色器 —— static ShaderProgram CreateRectCutComputer()
        /// <summary>
        /// 创建矩形切割计算着色器
        /// </summary>
        private static ShaderProgram CreateRectCutComputer()
        {
            ShaderProgram program = new ShaderProgram();
            program.ReadComputeShaderFromFile("Resources/GLSLs/cut_rect.comp");
            program.BuildCompute();

            return program;
        }
        #endregion 

        #region 创建圆形切割计算着色器 —— static ShaderProgram CreateCircleCutComputer()
        /// <summary>
        /// 创建圆形切割计算着色器
        /// </summary>
        private static ShaderProgram CreateCircleCutComputer()
        {
            ShaderProgram program = new ShaderProgram();
            program.ReadComputeShaderFromFile("Resources/GLSLs/cut_circle.comp");
            program.BuildCompute();

            return program;
        }
        #endregion 

        #region 创建椭圆形切割计算着色器 —— static ShaderProgram CreateEllipseCutComputer()
        /// <summary>
        /// 创建椭圆形切割计算着色器
        /// </summary>
        private static ShaderProgram CreateEllipseCutComputer()
        {
            ShaderProgram program = new ShaderProgram();
            program.ReadComputeShaderFromFile("Resources/GLSLs/cut_ellipse.comp");
            program.BuildCompute();

            return program;
        }
        #endregion 

        #region 创建立方体切割计算着色器 —— static ShaderProgram CreateBoxCutComputer()
        /// <summary>
        /// 创建立方体切割计算着色器
        /// </summary>
        private static ShaderProgram CreateBoxCutComputer()
        {
            ShaderProgram program = new ShaderProgram();
            program.ReadComputeShaderFromFile("Resources/GLSLs/cut_box.comp");
            program.BuildCompute();

            return program;
        }
        #endregion

        #endregion
    }
}
