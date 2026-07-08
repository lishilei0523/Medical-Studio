using MedicalSharp.Engine.Resources;
using System.Threading;

namespace MedicalSharp.Engine.Managers
{
    /// <summary>
    /// Shader管理器
    /// </summary>
    public static class ShaderManager
    {
        #region # 字段及构造器

        /// <summary>
        /// 是否已初始化
        /// </summary>
        private static bool _Initialized;

        /// <summary>
        /// 形状着色器程序
        /// </summary>
        private static ShaderProgram _ShapeProgram;

        /// <summary>
        /// 体积渲染着色器程序
        /// </summary>
        private static ShaderProgram _RaycastProgram;

        /// <summary>
        /// 体积渲染拾取着色器程序
        /// </summary>
        private static ShaderProgram _RaycastPickProgram;

        /// <summary>
        /// MPR渲染着色器程序
        /// </summary>
        private static ShaderProgram _MPRProgram;

        /// <summary>
        /// MPR渲染统计着色器程序
        /// </summary>
        private static ShaderProgram _MPRStatisticProgram;

        /// <summary>
        /// CPR拉直图着色器程序
        /// </summary>
        private static ShaderProgram _CPRStraightenedProgram;

        /// <summary>
        /// CPR投影图着色器程序
        /// </summary>
        private static ShaderProgram _CPRProjectedProgram;

        /// <summary>
        /// CPR剖面图着色器程序
        /// </summary>
        private static ShaderProgram _CPRCrossSectionalProgram;

        /// <summary>
        /// 同步锁
        /// </summary>
        private static readonly Lock _Sync;

        /// <summary>
        /// 静态构造器
        /// </summary>
        static ShaderManager()
        {
            _Initialized = false;
            _Sync = new Lock();
        }

        #endregion

        #region # 属性

        #region 只读属性 - 形状着色器程序 —— static ShaderProgram ShapeProgram
        /// <summary>
        /// 只读属性 - 形状着色器程序
        /// </summary>
        public static ShaderProgram ShapeProgram
        {
            get => _ShapeProgram;
        }
        #endregion

        #region 只读属性 - 体积渲染着色器程序 —— static ShaderProgram RaycastProgram
        /// <summary>
        /// 只读属性 - 体积渲染着色器程序
        /// </summary>
        public static ShaderProgram RaycastProgram
        {
            get => _RaycastProgram;
        }
        #endregion

        #region 只读属性 - 体积渲染拾取着色器程序 —— static ShaderProgram RaycastPickProgram
        /// <summary>
        /// 只读属性 - 体积渲染拾取着色器程序
        /// </summary>
        public static ShaderProgram RaycastPickProgram
        {
            get => _RaycastPickProgram;
        }
        #endregion

        #region 只读属性 - MPR渲染着色器程序 —— static ShaderProgram MPRProgram
        /// <summary>
        /// 只读属性 - MPR渲染着色器程序
        /// </summary>
        public static ShaderProgram MPRProgram
        {
            get => _MPRProgram;
        }
        #endregion

        #region 只读属性 - MPR渲染统计着色器程序 —— static ShaderProgram MPRStatisticProgram
        /// <summary>
        /// 只读属性 - MPR渲染统计着色器程序
        /// </summary>
        public static ShaderProgram MPRStatisticProgram
        {
            get => _MPRStatisticProgram;
        }
        #endregion

        #region 只读属性 - CPR拉直图着色器程序 —— static ShaderProgram CPRStraightenedProgram
        /// <summary>
        /// 只读属性 - CPR拉直图着色器程序
        /// </summary>
        public static ShaderProgram CPRStraightenedProgram
        {
            get => _CPRStraightenedProgram;
        }
        #endregion

        #region 只读属性 - CPR投影图着色器程序 —— static ShaderProgram CPRProjectedProgram
        /// <summary>
        /// 只读属性 - CPR投影图着色器程序
        /// </summary>
        public static ShaderProgram CPRProjectedProgram
        {
            get => _CPRProjectedProgram;
        }
        #endregion

        #region 只读属性 - CPR剖面图着色器程序 —— static ShaderProgram CPRCrossSectionalProgram
        /// <summary>
        /// 只读属性 - CPR剖面图着色器程序
        /// </summary>
        public static ShaderProgram CPRCrossSectionalProgram
        {
            get => _CPRCrossSectionalProgram;
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

                _ShapeProgram = CreateShapeProgram();
                _RaycastProgram = CreateRaycastProgram();
                _RaycastPickProgram = CreateRaycastPickProgram();
                _MPRProgram = CreateMPRProgram();
                _MPRStatisticProgram = CreateMPRStatisticProgram();
                _CPRStraightenedProgram = CreateCPRStraightenedProgram();
                _CPRProjectedProgram = CreateCPRProjectedProgram();
                _CPRCrossSectionalProgram = CreateCPRCrossSectionalProgram();
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
            _ShapeProgram?.Dispose();
            _RaycastProgram?.Dispose();
            _RaycastPickProgram?.Dispose();
            _MPRProgram?.Dispose();
            _MPRStatisticProgram?.Dispose();
            _CPRStraightenedProgram?.Dispose();
            _CPRProjectedProgram?.Dispose();
            _CPRCrossSectionalProgram?.Dispose();
        }
        #endregion


        //Private

        #region 创建形状着色器程序 —— static ShaderProgram CreateShapeProgram()
        /// <summary>
        /// 创建形状着色器程序
        /// </summary>
        private static ShaderProgram CreateShapeProgram()
        {
            ShaderProgram program = new ShaderProgram();
            program.ReadVertexShaderFromFile("Resources/GLSLs/shape.vert");
            program.ReadFragmentShaderFromFile("Resources/GLSLs/shape.frag");
            program.BuildDraw();

            return program;
        }
        #endregion 

        #region 创建体积渲染着色器程序 —— static ShaderProgram CreateRaycastProgram()
        /// <summary>
        /// 创建体积渲染着色器程序
        /// </summary>
        private static ShaderProgram CreateRaycastProgram()
        {
            ShaderProgram program = new ShaderProgram();
            program.ReadVertexShaderFromFile("Resources/GLSLs/raycast.vert");
            program.ReadFragmentShaderFromFile("Resources/GLSLs/raycast.frag");
            program.BuildDraw();

            return program;
        }
        #endregion 

        #region 创建体积渲染拾取着色器程序 —— static ShaderProgram CreateRaycastPickProgram()
        /// <summary>
        /// 创建体积渲染拾取着色器程序
        /// </summary>
        private static ShaderProgram CreateRaycastPickProgram()
        {
            ShaderProgram program = new ShaderProgram();
            program.ReadVertexShaderFromFile("Resources/GLSLs/raycast.vert");
            program.ReadFragmentShaderFromFile("Resources/GLSLs/raycast_pick.frag");
            program.BuildDraw();

            return program;
        }
        #endregion 

        #region 创建MPR渲染着色器程序 —— static ShaderProgram CreateMPRProgram()
        /// <summary>
        /// 创建MPR渲染着色器程序
        /// </summary>
        private static ShaderProgram CreateMPRProgram()
        {
            ShaderProgram program = new ShaderProgram();
            program.ReadVertexShaderFromFile("Resources/GLSLs/mpr.vert");
            program.ReadFragmentShaderFromFile("Resources/GLSLs/mpr.frag");
            program.BuildDraw();

            return program;
        }
        #endregion 

        #region 创建MPR渲染统计着色器程序 —— static ShaderProgram CreateMPRStatisticProgram()
        /// <summary>
        /// 创建MPR渲染统计着色器程序
        /// </summary>
        private static ShaderProgram CreateMPRStatisticProgram()
        {
            ShaderProgram program = new ShaderProgram();
            program.ReadVertexShaderFromFile("Resources/GLSLs/mpr.vert");
            program.ReadFragmentShaderFromFile("Resources/GLSLs/mpr_statistic.frag");
            program.BuildDraw();

            return program;
        }
        #endregion

        #region 创建CPR拉直图着色器程序 —— static ShaderProgram CreateCPRStraightenedProgram()
        /// <summary>
        /// 创建CPR拉直图着色器程序
        /// </summary>
        private static ShaderProgram CreateCPRStraightenedProgram()
        {
            ShaderProgram program = new ShaderProgram();
            program.ReadVertexShaderFromFile("Resources/GLSLs/cpr.vert");
            program.ReadFragmentShaderFromFile("Resources/GLSLs/cpr_straightened.frag");
            program.BuildDraw();

            return program;
        }
        #endregion

        #region 创建CPR投影图着色器程序 —— static ShaderProgram CreateCPRProjectedProgram()
        /// <summary>
        /// 创建CPR投影图着色器程序
        /// </summary>
        private static ShaderProgram CreateCPRProjectedProgram()
        {
            ShaderProgram program = new ShaderProgram();
            program.ReadVertexShaderFromFile("Resources/GLSLs/cpr.vert");
            program.ReadFragmentShaderFromFile("Resources/GLSLs/cpr_projected.frag");
            program.BuildDraw();

            return program;
        }
        #endregion

        #region 创建CPR剖面图着色器程序 —— static ShaderProgram CreateCPRCrossSectionalProgram()
        /// <summary>
        /// 创建CPR剖面图着色器程序
        /// </summary>
        private static ShaderProgram CreateCPRCrossSectionalProgram()
        {
            ShaderProgram program = new ShaderProgram();
            program.ReadVertexShaderFromFile("Resources/GLSLs/cpr.vert");
            program.ReadFragmentShaderFromFile("Resources/GLSLs/cpr_cross_sectional.frag");
            program.BuildDraw();

            return program;
        }
        #endregion

        #endregion
    }
}
