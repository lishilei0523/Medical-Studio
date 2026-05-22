using MedicalSharp.Engine.Resources;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
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
        private static ShaderProgram _RectangleCutComputer;

        /// <summary>
        /// 圆形切割计算着色器
        /// </summary>
        private static ShaderProgram _CircleCutComputer;

        /// <summary>
        /// 椭圆形切割计算着色器
        /// </summary>
        private static ShaderProgram _EllipseCutComputer;

        /// <summary>
        /// 多边形切割计算着色器
        /// </summary>
        private static ShaderProgram _PolygonCutComputer;

        /// <summary>
        /// 立方体切割计算着色器
        /// </summary>
        private static ShaderProgram _BoxCutComputer;

        /// <summary>
        /// 球体切割计算着色器
        /// </summary>
        private static ShaderProgram _SphereCutComputer;

        /// <summary>
        /// 圆柱体切割计算着色器
        /// </summary>
        private static ShaderProgram _CylinderCutComputer;

        /// <summary>
        /// 凸多面体切割计算着色器
        /// </summary>
        private static ShaderProgram _ConvexPolyhedronCutComputer;

        /// <summary>
        /// 重置标记值计算着色器
        /// </summary>
        private static ShaderProgram _ResetMarkValueComputer;

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

        #region 只读属性 - 矩形切割计算着色器 —— static ShaderProgram RectangleCutComputer
        /// <summary>
        /// 只读属性 - 矩形切割计算着色器
        /// </summary>
        public static ShaderProgram RectangleCutComputer
        {
            get => _RectangleCutComputer;
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

        #region 只读属性 - 多边形切割计算着色器 —— static ShaderProgram PolygonCutComputer
        /// <summary>
        /// 只读属性 - 多边形切割计算着色器
        /// </summary>
        public static ShaderProgram PolygonCutComputer
        {
            get => _PolygonCutComputer;
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

        #region 只读属性 - 球体切割计算着色器 —— static ShaderProgram SphereCutComputer
        /// <summary>
        /// 只读属性 - 球体切割计算着色器
        /// </summary>
        public static ShaderProgram SphereCutComputer
        {
            get => _SphereCutComputer;
        }
        #endregion

        #region 只读属性 - 圆柱体切割计算着色器 —— static ShaderProgram CylinderCutComputer
        /// <summary>
        /// 只读属性 - 圆柱体切割计算着色器
        /// </summary>
        public static ShaderProgram CylinderCutComputer
        {
            get => _CylinderCutComputer;
        }
        #endregion

        #region 只读属性 - 凸多面体切割计算着色器 —— static ShaderProgram ConvexPolyhedronCutComputer
        /// <summary>
        /// 只读属性 - 凸多面体切割计算着色器
        /// </summary>
        public static ShaderProgram ConvexPolyhedronCutComputer
        {
            get => _ConvexPolyhedronCutComputer;
        }
        #endregion

        #region 只读属性 - 重置标记值计算着色器 —— static ShaderProgram ResetMarkValueComputer
        /// <summary>
        /// 只读属性 - 重置标记值计算着色器
        /// </summary>
        public static ShaderProgram ResetMarkValueComputer
        {
            get => _ResetMarkValueComputer;
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

                _CircleCutComputer = CreateCircleCutComputer();
                _EllipseCutComputer = CreateEllipseCutComputer();
                _RectangleCutComputer = CreateRectangleCutComputer();
                _PolygonCutComputer = CreatePolygonCutComputer();
                _BoxCutComputer = CreateBoxCutComputer();
                _SphereCutComputer = CreateSphereCutComputer();
                _CylinderCutComputer = CreateCylinderCutComputer();
                _ConvexPolyhedronCutComputer = CreateConvexPolyhedronCutComputer();
                _ResetMarkValueComputer = CreateResetMarkValueComputer();
                _Initialized = true;
            }
        }
        #endregion

        #region 调度计算着色器 —— static void DispatchCompute2D(Vector2i size)
        /// <summary>
        /// 调度计算着色器
        /// </summary>
        /// <param name="size">数据尺寸</param>
        public static void DispatchCompute2D(Vector2i size)
        {
            //计算工作组数量（每组16×16线程）
            int groupsX = (int)MathF.Ceiling(size.X / 16.0f);
            int groupsY = (int)MathF.Ceiling(size.Y / 16.0f);

            //调度执行计算着色器
            GL.DispatchCompute(groupsX, groupsY, 1);

            //内存屏障：确保计算完成后渲染能读到新数据
            GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit | MemoryBarrierFlags.ShaderStorageBarrierBit);

            //检查错误
            GlException.ThrowOnError(nameof(DispatchCompute2D));
        }
        #endregion

        #region 调度计算着色器 —— static void DispatchCompute3D(Vector3i size)
        /// <summary>
        /// 调度计算着色器
        /// </summary>
        /// <param name="size">数据尺寸</param>
        public static void DispatchCompute3D(Vector3i size)
        {
            //计算工作组数量（每组8×8×8线程）
            int groupsX = (int)MathF.Ceiling(size.X / 8.0f);
            int groupsY = (int)MathF.Ceiling(size.Y / 8.0f);
            int groupsZ = (int)MathF.Ceiling(size.Z / 8.0f);

            //调度执行计算着色器
            GL.DispatchCompute(groupsX, groupsY, groupsZ);

            //内存屏障：确保计算完成后渲染能读到新数据
            GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit | MemoryBarrierFlags.ShaderStorageBarrierBit);

            //检查错误
            GlException.ThrowOnError(nameof(DispatchCompute3D));
        }
        #endregion

        #region 清理资源 —— static void Cleanup()
        /// <summary>
        /// 清理资源
        /// </summary>
        public static void Cleanup()
        {
            _RectangleCutComputer?.Dispose();
            _CircleCutComputer?.Dispose();
            _EllipseCutComputer?.Dispose();
            _PolygonCutComputer?.Dispose();
            _BoxCutComputer?.Dispose();
            _SphereCutComputer?.Dispose();
            _CylinderCutComputer?.Dispose();
            _ConvexPolyhedronCutComputer?.Dispose();
            _ResetMarkValueComputer?.Dispose();
        }
        #endregion


        //Private

        #region 创建矩形切割计算着色器 —— static ShaderProgram CreateRectangleCutComputer()
        /// <summary>
        /// 创建矩形切割计算着色器
        /// </summary>
        private static ShaderProgram CreateRectangleCutComputer()
        {
            ShaderProgram program = new ShaderProgram();
            program.ReadComputeShaderFromFile("Resources/GLSLs/cut_rectangle.comp");
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

        #region 创建多边形切割计算着色器 —— static ShaderProgram CreatePolygonCutComputer()
        /// <summary>
        /// 创建多边形切割计算着色器
        /// </summary>
        private static ShaderProgram CreatePolygonCutComputer()
        {
            ShaderProgram program = new ShaderProgram();
            program.ReadComputeShaderFromFile("Resources/GLSLs/cut_polygon.comp");
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

        #region 创建球体切割计算着色器 —— static ShaderProgram CreateSphereCutComputer()
        /// <summary>
        /// 创建球体切割计算着色器
        /// </summary>
        private static ShaderProgram CreateSphereCutComputer()
        {
            ShaderProgram program = new ShaderProgram();
            program.ReadComputeShaderFromFile("Resources/GLSLs/cut_sphere.comp");
            program.BuildCompute();

            return program;
        }
        #endregion

        #region 创建圆柱体切割计算着色器 —— static ShaderProgram CreateCylinderCutComputer()
        /// <summary>
        /// 创建圆柱体切割计算着色器
        /// </summary>
        private static ShaderProgram CreateCylinderCutComputer()
        {
            ShaderProgram program = new ShaderProgram();
            program.ReadComputeShaderFromFile("Resources/GLSLs/cut_cylinder.comp");
            program.BuildCompute();

            return program;
        }
        #endregion

        #region 创建凸多面体切割计算着色器 —— static ShaderProgram CreateConvexPolyhedronCutComputer()
        /// <summary>
        /// 创建凸多面体切割计算着色器
        /// </summary>
        private static ShaderProgram CreateConvexPolyhedronCutComputer()
        {
            ShaderProgram program = new ShaderProgram();
            program.ReadComputeShaderFromFile("Resources/GLSLs/cut_convex_polyhedron.comp");
            program.BuildCompute();

            return program;
        }
        #endregion

        #region 创建重置标记值计算着色器 —— static ShaderProgram CreateResetMarkValueComputer()
        /// <summary>
        /// 创建重置标记值计算着色器
        /// </summary>
        private static ShaderProgram CreateResetMarkValueComputer()
        {
            ShaderProgram program = new ShaderProgram();
            program.ReadComputeShaderFromFile("Resources/GLSLs/reset_mark.comp");
            program.BuildCompute();

            return program;
        }
        #endregion

        #endregion
    }
}
