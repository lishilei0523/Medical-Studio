using MedicalSharp.Engine.Cameras;
using MedicalSharp.Engine.Renderables;
using MedicalSharp.Engine.Resources;
using MedicalSharp.Engine.Shaders;
using MedicalSharp.Engine.ValueTypes;
using OpenTK.Mathematics;
using System;

namespace MedicalSharp.Engine.Renderers
{
    /// <summary>
    /// 体积渲染器
    /// </summary>
    public class VolumeRenderer : Renderer
    {
        #region # 字段及构造器

        /// <summary>
        /// 单位立方体
        /// </summary>
        private readonly VertexBuffer _unitCube;

        /// <summary>
        /// 创建体积渲染器构造器
        /// </summary>
        public VolumeRenderer()
        {
            //默认值
            this._unitCube = CreateUnitCube();
            this.InitShaderProgram();
        }

        /// <summary>
        /// 创建体积渲染器构造器
        /// </summary>
        /// <param name="camera">相机</param>
        public VolumeRenderer(Camera camera)
            : base(camera)
        {
            //默认值
            this._unitCube = CreateUnitCube();
            this.InitShaderProgram();
        }

        /// <summary>
        /// 创建体积渲染器构造器
        /// </summary>
        /// <param name="camera">相机</param>
        /// <param name="program">Shader程序</param>
        public VolumeRenderer(Camera camera, ShaderProgram program)
            : base(camera, program)
        {
            //默认值
            this._unitCube = CreateUnitCube();
        }

        #endregion

        #region # 属性

        #region 窗宽 —— float WindowWidth
        /// <summary>
        /// 窗宽
        /// </summary>
        public float WindowWidth { get; private set; }
        #endregion

        #region 窗位 —— float WindowCenter
        /// <summary>
        /// 窗位
        /// </summary>
        public float WindowCenter { get; private set; }
        #endregion

        #region 亮度 —— float Brightness
        /// <summary>
        /// 亮度
        /// </summary>
        public float Brightness { get; private set; }
        #endregion

        #region 密度缩放 —— float DensityScale
        /// <summary>
        /// 密度缩放
        /// </summary>
        public float DensityScale { get; private set; }
        #endregion

        #region 步长 —— int StepSize
        /// <summary>
        /// 步长
        /// </summary>
        public int StepSize { get; private set; }
        #endregion

        #region 最大步数 —— int MaxStepsCount
        /// <summary>
        /// 最大步数
        /// </summary>
        public int MaxStepsCount { get; private set; }
        #endregion

        #region 透明度阈值 —— float OpacityThreshold
        /// <summary>
        /// 透明度阈值
        /// </summary>
        public float OpacityThreshold { get; private set; }
        #endregion

        #region 传输函数 —— TransferFunction TransferFunction
        /// <summary>
        /// 传输函数
        /// </summary>
        public TransferFunction TransferFunction { get; private set; }
        #endregion

        #region 体积渲染对象 —— VolumeRenderable Renderable
        /// <summary>
        /// 体积渲染对象
        /// </summary>
        public VolumeRenderable Renderable { get; private set; }
        #endregion

        #endregion

        #region # 方法

        #region 设置窗宽窗位 —— void SetWindowLevel(float windowWidth, float windowCenter)
        /// <summary>
        /// 设置窗宽窗位
        /// </summary>
        /// <param name="windowWidth">窗宽</param>
        /// <param name="windowCenter">窗位</param>
        public void SetWindowLevel(float windowWidth, float windowCenter)
        {
            this.WindowWidth = windowWidth;
            this.WindowCenter = windowCenter;
        }
        #endregion

        #region 设置材质选项 —— void SetMaterialOptions(float brightness, float densityScale)
        /// <summary>
        /// 设置材质选项
        /// </summary>
        /// <param name="brightness">亮度</param>
        /// <param name="densityScale">密度缩放</param>
        public void SetMaterialOptions(float brightness, float densityScale)
        {
            this.Brightness = brightness;
            this.DensityScale = densityScale;
        }
        #endregion

        #region 设置采样选项 —— void SetSamplingOptions(int stepSize, int maxStepsCount...
        /// <summary>
        /// 设置采样选项
        /// </summary>
        /// <param name="stepSize">步长</param>
        /// <param name="maxStepsCount">最大步数</param>
        /// <param name="opacityThreshold">透明度阈值</param>
        public void SetSamplingOptions(int stepSize, int maxStepsCount, float opacityThreshold)
        {
            this.StepSize = stepSize;
            this.MaxStepsCount = maxStepsCount;
            this.OpacityThreshold = opacityThreshold;
        }
        #endregion

        #region 设置传输函数 —— void SetTransterFunction(TransferFunction transferFunction)
        /// <summary>
        /// 设置传输函数
        /// </summary>
        /// <param name="transferFunction">传输函数</param>
        public void SetTransterFunction(TransferFunction transferFunction)
        {
            #region # 验证

            if (transferFunction == null)
            {
                throw new ArgumentNullException(nameof(transferFunction), "传输函数不可为空！");
            }
            if (this.TransferFunction != null)
            {
                throw new InvalidOperationException("传输函数已设置，请尝试调整！");
            }

            #endregion

            this.TransferFunction = transferFunction;
        }
        #endregion

        #region 设置渲染对象 —— void SetRenderable(VolumeRenderable renderable)
        /// <summary>
        /// 设置渲染对象
        /// </summary>
        /// <param name="renderable">体积渲染对象</param>
        public void SetRenderable(VolumeRenderable renderable)
        {
            #region # 验证

            if (renderable == null)
            {
                throw new ArgumentNullException(nameof(renderable), "体积渲染对象不可为空！");
            }

            #endregion

            this.Renderable = renderable;
        }
        #endregion

        #region 渲染帧 —— override void RenderFrame(float viewportWidth, float viewportHeight)
        /// <summary>
        /// 渲染帧
        /// </summary>
        /// <param name="viewportWidth">视口宽度</param>
        /// <param name="viewportHeight">视口高度</param>
        public override void RenderFrame(float viewportWidth, float viewportHeight)
        {
            #region # 验证

            if (viewportWidth <= 0 || viewportHeight <= 0)
            {
                return;
            }
            if (this.Program == null)
            {
                throw new InvalidOperationException("Shader程序不可为空！");
            }
            if (this.Camera == null)
            {
                throw new InvalidOperationException("相机不可为空！");
            }

            #endregion

            //TODO 实现
        }
        #endregion

        #region 释放资源 —— void Dispose()
        /// <summary>
        /// 释放资源
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();

            this._unitCube.Dispose();
            this.TransferFunction.Dispose();
        }
        #endregion


        //Private

        #region 初始化Shader程序 —— void InitShaderProgram()
        /// <summary>
        /// 初始化Shader程序
        /// </summary>
        private void InitShaderProgram()
        {
            base.Program = new ShaderProgram();
            base.Program.ReadVertexShaderFromFile("Shaders/GLSLs/raycast.vert");
            base.Program.ReadFragmentShaderFromFile("Shaders/GLSLs/raycast.frag");
            base.Program.Build();
        }
        #endregion 

        #region 创建单位立方体 —— static VertexBuffer CreateUnitCube()
        /// <summary>
        /// 创建单位立方体
        /// </summary>
        private static VertexBuffer CreateUnitCube()
        {
            Vertex[] vertices =
            [
                new Vertex { Position = new Vector3(-0.5f, -0.5f, 0.5f) },
                new Vertex { Position = new Vector3(0.5f, -0.5f, 0.5f) },
                new Vertex { Position = new Vector3(0.5f, 0.5f, 0.5f) },
                new Vertex { Position = new Vector3(-0.5f, 0.5f, 0.5f) },
                new Vertex { Position = new Vector3(-0.5f, -0.5f, -0.5f) },
                new Vertex { Position = new Vector3(0.5f, -0.5f, -0.5f) },
                new Vertex { Position = new Vector3(0.5f, 0.5f, -0.5f) },
                new Vertex { Position = new Vector3(-0.5f, 0.5f, -0.5f) }
            ];
            uint[] indices =
            [
                0,1,2, 2,3,0, 1,5,6, 6,2,1,
                5,4,7, 7,6,5, 4,0,3, 3,7,4,
                3,2,6, 6,7,3, 4,5,1, 1,0,4
            ];

            MeshGeometry geometry = new MeshGeometry(vertices, indices);
            VertexBuffer vertexBuffer = new VertexBuffer(geometry);

            return vertexBuffer;
        }
        #endregion 

        #endregion
    }
}
