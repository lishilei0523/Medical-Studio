using MedicalSharp.Engine.Managers;
using MedicalSharp.Engine.Protocols;
using MedicalSharp.Engine.Renderables;
using MedicalSharp.Engine.Resources;
using MedicalSharp.Primitives.Cameras;
using MedicalSharp.Primitives.Enums;
using MedicalSharp.Primitives.Managers;
using MedicalSharp.Primitives.Maths;
using MedicalSharp.Primitives.Models;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Linq;

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
            this.PreviewMode = PreviewMode.Preview;
            this.RenderMode = MPRRenderMode.Gray;
            this.WindowWidth = 400;
            this.WindowCenter = 40;
            this.Brightness = 1.0f;
            this.Contrast = 1.0f;
            this._unitPlane = new VertexBuffer(ResourceManager.UnitPlane);
            this._unitPlane.Setup();
        }

        #endregion

        #region # 属性

        #region 预览模式 —— PreviewMode PreviewMode
        /// <summary>
        /// 预览模式
        /// </summary>
        public PreviewMode PreviewMode { get; private set; }
        #endregion

        #region 渲染模式 —— MPRRenderMode RenderMode
        /// <summary>
        /// 渲染模式
        /// </summary>
        public MPRRenderMode RenderMode { get; private set; }
        #endregion

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

        #region 传递函数 —— HUTransferFunction TransferFunction
        /// <summary>
        /// 传递函数
        /// </summary>
        public HUTransferFunction TransferFunction { get; private set; }
        #endregion

        #region 标记策略 —— MarkStrategy MarkStrategy
        /// <summary>
        /// 标记策略
        /// </summary>
        public MarkStrategy MarkStrategy { get; private set; }
        #endregion

        #region 体积渲染对象 —— VolumeRenderable Renderable
        /// <summary>
        /// 体积渲染对象
        /// </summary>
        public VolumeRenderable Renderable { get; private set; }
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

        #region 只读属性 - MPR相机 —— MPRCamera MPRCamera
        /// <summary>
        /// 只读属性 - MPR相机
        /// </summary>
        public MPRCamera MPRCamera
        {
            get => base.Camera as MPRCamera;
        }
        #endregion

        #endregion

        #region # 方法

        #region 切换预览模式 —— void SwitchPreviewMode(PreviewMode previewMode)
        /// <summary>
        /// 切换预览模式
        /// </summary>
        /// <param name="previewMode">预览模式</param>
        public void SwitchPreviewMode(PreviewMode previewMode)
        {
            this.PreviewMode = previewMode;
        }
        #endregion

        #region 切换渲染模式 —— void SwitchRenderMode(MPRRenderMode renderMode)
        /// <summary>
        /// 切换渲染模式
        /// </summary>
        /// <param name="renderMode">渲染模式</param>
        public void SwitchRenderMode(MPRRenderMode renderMode)
        {
            this.RenderMode = renderMode;
        }
        #endregion

        #region 绑定MPR平面 —— void BindPlane(MPRPlane plane)
        /// <summary>
        /// 绑定MPR平面
        /// </summary>
        /// <param name="plane">MPR平面</param>
        public void BindPlane(MPRPlane plane)
        {
            #region # 验证

            if (plane == null)
            {
                throw new ArgumentNullException(nameof(plane), "MPR平面不可为空！");
            }
            if (this._plane == plane)
            {
                return;
            }

            #endregion

            this._plane = plane;
            this.MPRCamera?.BindPlane(this._plane);
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

        #region 设置传递函数 —— void SetTransferFunction(HUTransferFunction transferFunction)
        /// <summary>
        /// 设置传递函数
        /// </summary>
        /// <param name="transferFunction">传递函数</param>
        public void SetTransferFunction(HUTransferFunction transferFunction)
        {
            this.TransferFunction = transferFunction;
        }
        #endregion

        #region 设置标记策略 —— void SetMarkStrategy(MarkStrategy markStrategy)
        /// <summary>
        /// 设置标记策略
        /// </summary>
        /// <param name="markStrategy">标记策略</param>
        public void SetMarkStrategy(MarkStrategy markStrategy)
        {
            this.MarkStrategy = markStrategy;
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
            if (this.Plane == null)
            {
                throw new InvalidOperationException("MPR平面不可为空！");
            }
            if (this.Camera == null)
            {
                throw new InvalidOperationException("MPR相机不可为空！");
            }
            if (this.TransferFunction == null)
            {
                throw new InvalidOperationException("传递函数不可为空！");
            }
            if (this.MarkStrategy == null)
            {
                throw new InvalidOperationException("标记策略不可为空！");
            }
            if (this.Renderable == null)
            {
                throw new InvalidOperationException("渲染对象不可为空！");
            }

            #endregion

            //设置相机视口尺寸
            this.Camera.SetViewportSize(viewportWidth, viewportHeight);

            //渲染上下文
            RenderContext renderContext = new RenderContext(viewportWidth, viewportHeight, this.Camera.CameraMode, this.Camera.CameraPosition, this.Camera.LookDirection, this.Camera.ProjectionMatrix, this.Camera.ViewMatrix, this.MPRCamera.ZoomFactor);

            //开启Shader程序
            ShaderProgram program = ShaderManager.MPRProgram;
            program.Use();

            //设置MVP矩阵、相机位置、缩放
            Matrix4 modelMatrix = this._plane.GetModelMatrix();
            program.SetUniformMatrix4("u_ModelMatrix", modelMatrix);
            program.SetUniformMatrix4("u_ViewMatrix", renderContext.ViewMatrix);
            program.SetUniformMatrix4("u_ProjectionMatrix", renderContext.ProjectionMatrix);
            program.SetUniformVector3("u_VolumeScale", this.Renderable.VolumeMetadata.VolumeScale);
            program.SetUniformBoolean("u_IsAxial", this._plane.OriginalPlaneType == MPRPlaneType.Axial);

            //设置DICOM重缩放参数
            program.SetUniformFloat("u_RescaleSlope", this.Renderable.VolumeMetadata.RescaleSlope);
            program.SetUniformFloat("u_RescaleIntercept", this.Renderable.VolumeMetadata.RescaleIntercept);

            //设置预览、渲染模式
            program.SetUniformInt("u_PreviewMode", (int)this.PreviewMode);
            program.SetUniformInt("u_RenderMode", (int)this.RenderMode);

            //设置渲染参数
            program.SetUniformFloat("u_WindowWidth", this.WindowWidth);
            program.SetUniformFloat("u_WindowCenter", this.WindowCenter);
            program.SetUniformFloat("u_Brightness", this.Brightness);
            program.SetUniformFloat("u_Contrast", this.Contrast);
            program.SetUniformFloat("u_HUMin", this.TransferFunction.HUMin);
            program.SetUniformFloat("u_HUMax", this.TransferFunction.HUMax);

            //设置标记策略
            program.SetUniformIntArray("u_MarkModes", [.. this.MarkStrategy.MarkModes.Select(mode => (int)mode)]);

            //绑定纹理
            this.Renderable.OriginalTexture.Bind(0);
            this.Renderable.PreviewTexture.Bind(1);
            this.Renderable.MarkTexture.Bind(2);
            this.TransferFunction.Texture.Bind(3);
            this.MarkStrategy.Texture.Bind(4);
            program.SetUniformInt("u_OriginalTexture", 0);
            program.SetUniformInt("u_PreviewTexture", 1);
            program.SetUniformInt("u_MarkTexture", 2);
            program.SetUniformInt("u_TransferFunction", 3);
            program.SetUniformInt("u_MarkStrategy", 4);

            //绘制平面
            this._unitPlane.Draw(PrimitiveType.Triangles);

            //解绑纹理
            this.Renderable.OriginalTexture.Unbind();
            this.Renderable.PreviewTexture.Unbind();
            this.Renderable.MarkTexture.Unbind();
            this.TransferFunction.Texture.Unbind();
            this.MarkStrategy.Texture.Unbind();

            //取消使用
            program.Unuse();
        }
        #endregion

        #region 释放资源 —— override void Dispose()
        /// <summary>
        /// 释放资源
        /// </summary>
        public override void Dispose()
        {
            if (this._disposed)
            {
                return;
            }

            this._unitPlane.Dispose();
            this._disposed = true;
        }
        #endregion

        #endregion
    }
}
