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
        /// 文本着色器程序
        /// </summary>
        private static ShaderProgram _TextProgram;

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

        #region 只读属性 - 文本着色器程序 —— static ShaderProgram TextProgram
        /// <summary>
        /// 只读属性 - 文本着色器程序
        /// </summary>
        public static ShaderProgram TextProgram
        {
            get => _TextProgram;
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
                _TextProgram = CreateTextProgram();
                _RaycastProgram = CreateRaycastProgram();
                _RaycastPickProgram = CreateRaycastPickProgram();
                _MPRProgram = CreateMPRProgram();
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
            _TextProgram?.Dispose();
            _RaycastProgram?.Dispose();
            _RaycastPickProgram?.Dispose();
            _MPRProgram?.Dispose();
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

        #region 创建文本着色器程序 —— static ShaderProgram CreateTextProgram()
        /// <summary>
        /// 创建文本着色器程序
        /// </summary>
        private static ShaderProgram CreateTextProgram()
        {
            ShaderProgram program = new ShaderProgram();
            program.ReadVertexShaderFromFile("Resources/GLSLs/text.vert");
            program.ReadFragmentShaderFromFile("Resources/GLSLs/text.frag");
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

        #endregion
    }
}
