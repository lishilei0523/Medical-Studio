using MedicalSharp.Engine.Managers;
using MedicalSharp.Engine.Renderables;
using MedicalSharp.Engine.Resources;
using MedicalSharp.Primitives.Cameras;
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

        #region 传递函数 —— TransferFunction TransferFunction
        /// <summary>
        /// 传递函数
        /// </summary>
        public TransferFunction TransferFunction { get; private set; }
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

        //Public

        #region 绑定MPR平面 —— void BindPlane(MPRPlane plane)
        /// <summary>
        /// 绑定MPR平面
        /// </summary>
        /// <param name="plane">MPR平面</param>
        public void BindPlane(MPRPlane plane)
        {
            #region # 验证

            if (this._plane == plane)
            {
                return;
            }

            #endregion

            if (this._plane != null)
            {
                this._plane.PlaneChangedEvent -= this.OnPlaneChanged;
            }

            this._plane = plane;
            if (this._plane != null)
            {
                this._plane.PlaneChangedEvent += this.OnPlaneChanged;
                this.MPRCamera?.BindPlane(this._plane);
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

        #region 设置传递函数 —— void SetTransferFunction(TransferFunction transferFunction)
        /// <summary>
        /// 设置传递函数
        /// </summary>
        /// <param name="transferFunction">传递函数</param>
        public void SetTransferFunction(TransferFunction transferFunction)
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

            //设置相机视口
            this.Camera.SetViewportSize(viewportWidth, viewportHeight);

            //使用Shader
            ShaderProgram program = ShaderManager.MPRProgram;
            program.Use();

            //设置Uniform变量
            program.SetUniformMatrix4("u_ModelMatrix", this._modelMatrix);
            program.SetUniformMatrix4("u_ViewMatrix", this.Camera.ViewMatrix);
            program.SetUniformMatrix4("u_ProjectionMatrix", this.Camera.ProjectionMatrix);

            program.SetUniformFloat("u_WindowWidth", this.WindowWidth);
            program.SetUniformFloat("u_WindowCenter", this.WindowCenter);
            program.SetUniformFloat("u_Brightness", this.Brightness);
            program.SetUniformFloat("u_Contrast", this.Contrast);

            program.SetUniformFloat("u_RescaleSlope", this.Renderable.VolumeMetadata.RescaleSlope);
            program.SetUniformFloat("u_RescaleIntercept", this.Renderable.VolumeMetadata.RescaleIntercept);

            program.SetUniformVector3("u_VolumeScale", this.Renderable.VolumeMetadata.VolumeScale);

            //设置标记策略
            program.SetUniformIntArray("u_MarkModes", [.. this.MarkStrategy.MarkModes.Select(mode => (int)mode)]);

            //绑定纹理
            this.Renderable.VolumeTexture.Bind(0);
            this.Renderable.MarkTexture.Bind(1);
            this.TransferFunction.Texture.Bind(2);
            this.MarkStrategy.Texture.Bind(3);

            program.SetUniformInt("u_VolumeTexture", 0);
            program.SetUniformInt("u_MarkTexture", 1);
            program.SetUniformInt("u_TransferFunction", 2);
            program.SetUniformInt("u_MarkStrategy", 3);

            //绘制平面
            this._unitPlane.Draw(PrimitiveType.Triangles);

            //解绑纹理
            this.Renderable.VolumeTexture.Unbind();
            this.Renderable.MarkTexture.Unbind();
            this.TransferFunction.Texture.Unbind();

            //取消使用Shader
            program.Unuse();

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
            if (this._disposed)
            {
                return;
            }

            if (this._plane != null)
            {
                this._plane.PlaneChangedEvent -= this.OnPlaneChanged;
            }

            this._unitPlane.Dispose();
            this._disposed = true;
        }
        #endregion


        //Private

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
