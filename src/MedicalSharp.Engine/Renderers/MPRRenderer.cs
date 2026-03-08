using MedicalSharp.Engine.Cameras;
using MedicalSharp.Engine.Renderables;
using MedicalSharp.Engine.Resources;
using MedicalSharp.Engine.Shaders;
using MedicalSharp.Engine.ValueTypes;
using System;

namespace MedicalSharp.Engine.Renderers
{
    /// <summary>
    /// MPR渲染器
    /// </summary>
    public class MPRRenderer : Renderer
    {
        #region # 字段及构造器

        /// <summary>
        /// 单位平面
        /// </summary>
        private readonly VertexBuffer _unitPlane;

        /// <summary>
        /// 创建MPR渲染器构造器
        /// </summary>
        public MPRRenderer()
        {
            //默认值
            this._unitPlane = new VertexBuffer(ResourceManager.UnitPlane);
            this._unitPlane.Setup();
            this.InterpolationMode = InterpolationMode.Linear;
            this.TransferFunction = new TransferFunction();
            this.InitShaderProgram();
        }

        /// <summary>
        /// 创建MPR渲染器构造器
        /// </summary>
        /// <param name="camera">相机</param>
        public MPRRenderer(Camera camera)
            : base(camera)
        {
            //默认值
            this._unitPlane = new VertexBuffer(ResourceManager.UnitPlane);
            this._unitPlane.Setup();
            this.InterpolationMode = InterpolationMode.Linear;
            this.TransferFunction = new TransferFunction();
            this.InitShaderProgram();
        }

        /// <summary>
        /// 创建MPR渲染器构造器
        /// </summary>
        /// <param name="camera">相机</param>
        /// <param name="program">Shader程序</param>
        public MPRRenderer(Camera camera, ShaderProgram program)
            : base(camera, program)
        {
            //默认值
            this._unitPlane = new VertexBuffer(ResourceManager.UnitPlane);
            this._unitPlane.Setup();
            this.InterpolationMode = InterpolationMode.Linear;
            this.TransferFunction = new TransferFunction();
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

        #region 对比度 —— float Contrast
        /// <summary>
        /// 对比度
        /// </summary>
        public float Contrast { get; private set; }
        #endregion

        #region 反转灰度 —— bool InvertGrayscale
        /// <summary>
        /// 反转灰度
        /// </summary>
        public bool InvertGrayscale { get; private set; }
        #endregion

        #region 插值模式 —— InterpolationMode InterpolationMode
        /// <summary>
        /// 插值模式
        /// </summary>
        public InterpolationMode InterpolationMode { get; private set; }
        #endregion

        #region 传输函数 —— TransferFunction TransferFunction
        /// <summary>
        /// 传输函数
        /// </summary>
        public TransferFunction TransferFunction { get; }
        #endregion

        #region 体积渲染对象 —— VolumeRenderable Renderable
        /// <summary>
        /// 体积渲染对象
        /// </summary>
        public VolumeRenderable Renderable { get; private set; }
        #endregion 

        #endregion

        #region # 方法

        //Public

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

        #region 设置材质选项 —— void SetMaterialOptions(float brightness, float contrast...
        /// <summary>
        /// 设置材质选项
        /// </summary>
        /// <param name="brightness">亮度</param>
        /// <param name="contrast">对比度</param>
        /// <param name="invertGrayscale">反转灰度</param>
        public void SetMaterialOptions(float brightness, float contrast, bool invertGrayscale = false)
        {
            this.Brightness = brightness;
            this.Contrast = contrast;
            this.InvertGrayscale = invertGrayscale;
        }
        #endregion

        #region 设置插值模式 —— void SetInterpolationMode(InterpolationMode interpolationMode)
        /// <summary>
        /// 设置插值模式
        /// </summary>
        /// <param name="interpolationMode">插值模式</param>
        public void SetInterpolationMode(InterpolationMode interpolationMode)
        {
            this.InterpolationMode = interpolationMode;
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
            if (renderable == this.Renderable)
            {
                return;
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

            this._unitPlane.Dispose();
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
            base.Program.ReadVertexShaderFromFile("Shaders/GLSLs/mpr.vert");
            base.Program.ReadFragmentShaderFromFile("Shaders/GLSLs/mpr.frag");
            base.Program.Build();
        }
        #endregion  

        #endregion
    }
}
