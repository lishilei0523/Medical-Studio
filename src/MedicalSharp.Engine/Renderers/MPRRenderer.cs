using MedicalSharp.Engine.Cameras;
using MedicalSharp.Engine.Renderables;
using MedicalSharp.Engine.Resources;
using MedicalSharp.Engine.Shaders;
using MedicalSharp.Primitives.Managers;
using MedicalSharp.Primitives.Models;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
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
        /// MPR平面
        /// </summary>
        private MPRPlane _plane;

        /// <summary>
        /// 模型矩阵
        /// </summary>
        private Matrix4 _modelMatrix;

        /// <summary>
        /// 单位平面
        /// </summary>
        private readonly VertexBuffer _unitPlane;

        /// <summary>
        /// 创建MPR渲染器构造器
        /// </summary>
        /// <param name="camera">MPR相机</param>
        public MPRRenderer(MPRCamera camera)
            : base(camera)
        {
            this._unitPlane = new VertexBuffer(ResourceManager.UnitPlane);
            this._unitPlane.Setup();
            this.WindowWidth = 400;
            this.WindowCenter = 40;
            this.Brightness = 1.0f;
            this.Contrast = 1.0f;
            this.TransferFunction = new TransferFunction();
            this.InitShaderProgram();
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

        #region 只读属性 - MPR相机 —— MPRCamera MPRCamera
        /// <summary>
        /// 只读属性 - MPR相机
        /// </summary>
        public MPRCamera MPRCamera
        {
            get => base.Camera as MPRCamera;
        }
        #endregion

        #region 只读属性 - MPR平面 —— MPRPlane Plane
        /// <summary>
        /// 只读属性 - MPR平面
        /// </summary>
        public MPRPlane Plane
        {
            get => this._plane;
        }
        #endregion

        #endregion

        #region # 方法

        //Public

        #region 绑定MPR平面 —— void BindPlane(MPRPlane plane)
        /// <summary>
        /// 绑定MPR平面
        /// </summary>
        /// <param name="plane">MPR平面</param>
        public void BindPlane(MPRPlane plane)
        {
            if (this._plane == plane)
            {
                return;
            }

            if (this._plane != null)
            {
                this._plane.PlaneChangedEvent -= this.OnPlaneChanged;
            }

            this._plane = plane;
            if (this._plane != null)
            {
                this._plane.PlaneChangedEvent += this.OnPlaneChanged;

                if (this.MPRCamera != null)
                {
                    this.MPRCamera.BindPlane(this._plane);
                }

                this.UpdateModelMatrix();
            }
        }
        #endregion

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

        #region 设置材质选项 —— void SetMaterialOptions(float brightness, float contrast)
        /// <summary>
        /// 设置材质选项
        /// </summary>
        /// <param name="brightness">亮度</param>
        /// <param name="contrast">对比度</param>
        public void SetMaterialOptions(float brightness, float contrast)
        {
            this.Brightness = brightness;
            this.Contrast = contrast;
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
            if (this.Renderable?.VolumeTexture == null)
            {
                return;
            }
            if (this._plane == null)
            {
                return;
            }

            #endregion

            //设置相机视口
            this.Camera.SetViewportSize(viewportWidth, viewportHeight);

            //使用Shader
            this.Program.Use();

            //设置Uniform变量
            this.Program.SetUniformMatrix4("u_ModelMatrix", this._modelMatrix);
            this.Program.SetUniformMatrix4("u_ViewMatrix", this.Camera.ViewMatrix);
            this.Program.SetUniformMatrix4("u_ProjectionMatrix", this.Camera.ProjectionMatrix);

            this.Program.SetUniformFloat("u_WindowWidth", this.WindowWidth);
            this.Program.SetUniformFloat("u_WindowCenter", this.WindowCenter);
            this.Program.SetUniformFloat("u_Brightness", this.Brightness);
            this.Program.SetUniformFloat("u_Contrast", this.Contrast);

            this.Program.SetUniformFloat("u_RescaleSlope", this.Renderable.RescaleSlope);
            this.Program.SetUniformFloat("u_RescaleIntercept", this.Renderable.RescaleIntercept);

            this.Program.SetUniformVector3("u_VolumeScale", this.Renderable.VolumeScale);

            //绑定纹理
            this.Renderable.VolumeTexture.Bind(0);
            this.Program.SetUniformInt("u_VolumeTexture", 0);

            //绘制平面
            this._unitPlane.Draw(PrimitiveType.Triangles);

            //解绑纹理
            this.Renderable.VolumeTexture.Unbind();

            //取消使用Shader
            this.Program.Unuse();

            //触发渲染事件
            RenderContext context = new RenderContext(viewportWidth, viewportHeight, this.Camera.CameraPosition, this.Camera.LookDirection, this.Camera.ProjectionMatrix, this.Camera.ViewMatrix);

            this.Renderable.OnRender(context);
        }
        #endregion

        #region 释放资源 —— override void Dispose()
        /// <summary>
        /// 释放资源
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();

            if (this._plane != null)
            {
                this._plane.PlaneChangedEvent -= this.OnPlaneChanged;
            }

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
            this.Program = new ShaderProgram();
            this.Program.ReadVertexShaderFromFile("Shaders/GLSLs/mpr.vert");
            this.Program.ReadFragmentShaderFromFile("Shaders/GLSLs/mpr.frag");
            this.Program.Build();
        }
        #endregion

        #region 更新模型矩阵 —— void UpdateModelMatrix()
        /// <summary>
        /// 更新模型矩阵
        /// </summary>
        private void UpdateModelMatrix()
        {
            if (this._plane != null)
            {
                this._modelMatrix = this._plane.GetModelMatrix();
            }
        }
        #endregion

        #region MPR平面变化事件 —— void OnPlaneChanged(MPRPlane plane)
        /// <summary>
        /// MPR平面变化事件
        /// </summary>
        /// <param name="plane">MPR平面</param>
        private void OnPlaneChanged(MPRPlane plane)
        {
            this.UpdateModelMatrix();
        }
        #endregion

        #endregion
    }
}
