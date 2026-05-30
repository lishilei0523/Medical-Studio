using MedicalSharp.Engine.Base;
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
    /// 体积渲染器
    /// </summary>
    public class VolumeRenderer : Renderer
    {
        #region # 字段及构造器

        /// <summary>
        /// 拾取帧缓冲区
        /// </summary>
        private FrameBuffer _pickFrameBuffer;

        /// <summary>
        /// 单位立方体
        /// </summary>
        private readonly VertexBuffer _unitCube;

        /// <summary>
        /// 创建体积渲染器构造器
        /// </summary>
        /// <param name="camera">相机</param>
        public VolumeRenderer(Camera camera)
            : base(camera)
        {
            //默认值
            this._unitCube = new VertexBuffer(ResourceManager.UnitCube);
            this.PreviewMode = PreviewMode.Preview;
            this.RenderMode = VolumeRenderMode.Raycast;
        }

        #endregion

        #region # 属性

        #region 预览模式 —— PreviewMode PreviewMode
        /// <summary>
        /// 预览模式
        /// </summary>
        public PreviewMode PreviewMode { get; private set; }
        #endregion

        #region 渲染模式 —— VolumeRenderMode RenderMode
        /// <summary>
        /// 渲染模式
        /// </summary>
        public VolumeRenderMode RenderMode { get; private set; }
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

        #region 密度缩放 —— float DensityScale
        /// <summary>
        /// 密度缩放
        /// </summary>
        public float DensityScale { get; private set; }
        #endregion

        #region 步长 —— float StepSize
        /// <summary>
        /// 步长
        /// </summary>
        public float StepSize { get; private set; }
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

        #region 传递函数 —— DensityTransferFunction TransferFunction
        /// <summary>
        /// 传递函数
        /// </summary>
        public DensityTransferFunction TransferFunction { get; private set; }
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

        #endregion

        #region # 方法

        //Public

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

        #region 切换渲染模式 —— void SwitchRenderMode(VolumeRenderMode renderMode)
        /// <summary>
        /// 切换渲染模式
        /// </summary>
        /// <param name="renderMode">渲染模式</param>
        public void SwitchRenderMode(VolumeRenderMode renderMode)
        {
            this.RenderMode = renderMode;
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

        #region 设置采样选项 —— void SetSamplingOptions(float stepSize, int maxStepsCount...
        /// <summary>
        /// 设置采样选项
        /// </summary>
        /// <param name="stepSize">步长</param>
        /// <param name="maxStepsCount">最大步数</param>
        /// <param name="opacityThreshold">透明度阈值</param>
        public void SetSamplingOptions(float stepSize, int maxStepsCount, float opacityThreshold)
        {
            this.StepSize = stepSize;
            this.MaxStepsCount = maxStepsCount;
            this.OpacityThreshold = opacityThreshold;
        }
        #endregion

        #region 设置传递函数 —— void SetTransferFunction(DensityTransferFunction transferFunction)
        /// <summary>
        /// 设置传递函数
        /// </summary>
        /// <param name="transferFunction">传递函数</param>
        public void SetTransferFunction(DensityTransferFunction transferFunction)
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

        #region 渲染帧 —— override void RenderFrame(float viewportWidth, float viewportHeight...
        /// <summary>
        /// 渲染帧
        /// </summary>
        /// <param name="viewportWidth">视口宽度</param>
        /// <param name="viewportHeight">视口高度</param>
        /// <param name="glContext">OpenGL上下文句柄</param>
        public override void RenderFrame(float viewportWidth, float viewportHeight, IntPtr glContext)
        {
            #region # 验证

            if (viewportWidth <= 0 || viewportHeight <= 0)
            {
                return;
            }
            if (this.Camera == null)
            {
                throw new InvalidOperationException("相机不可为空！");
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
            RenderContext renderContext = new RenderContext(glContext, viewportWidth, viewportHeight, this.Camera.CameraMode, this.Camera.CameraPosition, this.Camera.LookDirection, this.Camera.ProjectionMatrix, this.Camera.ViewMatrix);

            //开启Shader程序
            ShaderProgram program = ShaderManager.RaycastProgram;
            program.Use();

            //处理缩放
            Matrix4 volumeScaleMatrix = Matrix4.CreateScale(this.Renderable.VolumeMetadata.VolumeScale);

            //设置MVP矩阵、相机位置、缩放
            program.SetUniformMatrix4("u_ModelMatrix", this.Renderable.ModelMatrix * volumeScaleMatrix);
            program.SetUniformMatrix4("u_ViewMatrix", renderContext.ViewMatrix);
            program.SetUniformMatrix4("u_ProjectionMatrix", renderContext.ProjectionMatrix);
            program.SetUniformVector3("u_CameraPosition", renderContext.CameraPosition);
            program.SetUniformVector3("u_VolumeScale", this.Renderable.VolumeMetadata.VolumeScale);

            //设置预览、渲染模式
            program.SetUniformInt("u_PreviewMode", (int)this.PreviewMode);
            program.SetUniformInt("u_RenderMode", (int)this.RenderMode);

            //设置渲染参数
            program.SetUniformFloat("u_WindowCenter", this.WindowCenter);
            program.SetUniformFloat("u_WindowWidth", this.WindowWidth);
            program.SetUniformFloat("u_Brightness", this.Brightness);
            program.SetUniformFloat("u_DensityScale", this.DensityScale);
            program.SetUniformFloat("u_StepSize", this.StepSize);
            program.SetUniformInt("u_MaxStepsCount", this.MaxStepsCount);
            program.SetUniformFloat("u_OpacityThreshold", this.OpacityThreshold);

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

            //绘制模型
            this._unitCube.Draw(glContext, PrimitiveType.Triangles);

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

        #region 拾取体素 —— Vector3i? PickVoxel(IntPtr glContext, Ray ray...
        /// <summary>
        /// 拾取体素
        /// </summary>
        /// <param name="glContext">OpenGL上下文句柄</param>
        /// <param name="ray">射线（世界空间）</param>
        /// <param name="viewportWidth">视口宽度</param>
        /// <param name="viewportHeight">视口高度</param>
        /// <param name="textureCoord">纹理坐标</param>
        /// <returns>体素坐标，未命中返回null</returns>
        public Vector3i? PickVoxel(IntPtr glContext, Ray ray, float viewportWidth, float viewportHeight, out Vector3? textureCoord)
        {
            #region # 验证

            if (this.Renderable == null)
            {
                textureCoord = null;
                return null;
            }
            if (this.Camera == null)
            {
                textureCoord = null;
                return null;
            }

            #endregion

            //使用1/4分辨率加速
            int pickWidth = Math.Max(1, (int)viewportWidth / 4);
            int pickHeight = Math.Max(1, (int)viewportHeight / 4);

            //确保拾取FBO存在
            this.InitPickFrameBuffer(pickWidth, pickHeight);

            //渲染到拾取FBO
            this.RenderPickFrameBuffer(glContext, ray, pickWidth, pickHeight);

            //读取中心像素
            int textureCoordX = pickWidth / 2;
            int textureCoordY = pickHeight / 2;

            float[] pixel = new float[4];
            this._pickFrameBuffer.Bind();
            GL.ReadPixels(textureCoordX, textureCoordY, 1, 1, PixelFormat.Rgba, PixelType.Float, pixel);
            this._pickFrameBuffer.Unbind();

            //过滤纹理坐标
            if (pixel[0] < 0.001f && pixel[1] < 0.001f && pixel[2] < 0.001f)
            {
                textureCoord = null;
                return null;
            }

            //提取纹理坐标
            textureCoord = new Vector3(pixel[0], pixel[1], pixel[2]);

            //转换体素坐标
            int voxelX = (int)Math.Ceiling(pixel[0] * this.Renderable.OriginalTexture.Width);
            int voxelY = (int)Math.Ceiling(pixel[1] * this.Renderable.OriginalTexture.Height);
            int voxelZ = (int)Math.Ceiling(pixel[2] * this.Renderable.OriginalTexture.Depth);
            voxelX = Math.Clamp(voxelX, 0, this.Renderable.OriginalTexture.Width - 1);
            voxelY = Math.Clamp(voxelY, 0, this.Renderable.OriginalTexture.Height - 1);
            voxelZ = Math.Clamp(voxelZ, 0, this.Renderable.OriginalTexture.Depth - 1);
            Vector3i voxelPosition = new Vector3i(voxelX, voxelY, voxelZ);

            return voxelPosition;
        }
        #endregion

        #region 释放资源 —— void Dispose()
        /// <summary>
        /// 释放资源
        /// </summary>
        public override void Dispose()
        {
            if (this._disposed)
            {
                return;
            }

            this._pickFrameBuffer?.Dispose();
            this._unitCube.Dispose();
            this._disposed = true;
        }
        #endregion


        //Private

        #region 渲染拾取帧缓冲区 —— void RenderPickFrameBuffer(IntPtr glContext, Ray ray...
        /// <summary>
        /// 渲染拾取帧缓冲区
        /// </summary>
        /// <param name="glContext">OpenGL上下文句柄</param>
        /// <param name="ray">射线（世界空间）</param>
        /// <param name="viewportWidth">视口宽度</param>
        /// <param name="viewportHeight">视口高度</param>
        private void RenderPickFrameBuffer(IntPtr glContext, Ray ray, int viewportWidth, int viewportHeight)
        {
            #region # 验证

            if (viewportWidth <= 0 || viewportHeight <= 0)
            {
                return;
            }
            if (this.Camera == null)
            {
                throw new InvalidOperationException("相机不可为空！");
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

            //绑定拾取FBO
            this._pickFrameBuffer.Bind();
            GL.Viewport(0, 0, viewportWidth, viewportHeight);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            //开启Shader程序
            ShaderProgram pickProgram = ShaderManager.RaycastPickProgram;
            pickProgram.Use();

            //处理缩放
            Matrix4 volumeScaleMatrix = Matrix4.CreateScale(this.Renderable.VolumeMetadata.VolumeScale);
            Matrix4 modelMatrix = this.Renderable.ModelMatrix * volumeScaleMatrix;

            //设置MVP矩阵、相机位置、缩放
            pickProgram.SetUniformMatrix4("u_ModelMatrix", modelMatrix);
            pickProgram.SetUniformMatrix4("u_ViewMatrix", this.Camera.ViewMatrix);
            pickProgram.SetUniformMatrix4("u_ProjectionMatrix", this.Camera.ProjectionMatrix);
            pickProgram.SetUniformVector3("u_RayOrigin", ray.Position);
            pickProgram.SetUniformVector3("u_RayDirection", ray.Direction);
            pickProgram.SetUniformVector3("u_CameraPosition", this.Camera.CameraPosition);
            pickProgram.SetUniformVector3("u_VolumeScale", this.Renderable.VolumeMetadata.VolumeScale);

            //设置预览模式
            pickProgram.SetUniformInt("u_PreviewMode", (int)this.PreviewMode);

            //设置渲染参数
            pickProgram.SetUniformFloat("u_WindowCenter", this.WindowCenter);
            pickProgram.SetUniformFloat("u_WindowWidth", this.WindowWidth);
            pickProgram.SetUniformFloat("u_Brightness", this.Brightness);
            pickProgram.SetUniformFloat("u_DensityScale", this.DensityScale);
            pickProgram.SetUniformFloat("u_StepSize", this.StepSize);
            pickProgram.SetUniformInt("u_MaxStepsCount", this.MaxStepsCount);

            //设置标记策略
            pickProgram.SetUniformIntArray("u_MarkModes", [.. this.MarkStrategy.MarkModes.Select(mode => (int)mode)]);

            //绑定纹理
            this.Renderable.OriginalTexture.Bind(0);
            this.Renderable.PreviewTexture.Bind(1);
            this.Renderable.MarkTexture.Bind(2);
            this.TransferFunction.Texture.Bind(3);
            pickProgram.SetUniformInt("u_OriginalTexture", 0);
            pickProgram.SetUniformInt("u_PreviewTexture", 1);
            pickProgram.SetUniformInt("u_MarkTexture", 2);
            pickProgram.SetUniformInt("u_TransferFunction", 3);

            //绘制模型
            this._unitCube.Draw(glContext, PrimitiveType.Triangles);

            //解绑纹理
            this.Renderable.OriginalTexture.Unbind();
            this.Renderable.PreviewTexture.Unbind();
            this.Renderable.MarkTexture.Unbind();
            this.TransferFunction.Texture.Unbind();

            //取消使用
            pickProgram.Unuse();

            //解绑拾取FBO
            this._pickFrameBuffer.Unbind();
        }
        #endregion

        #region 初始化拾取帧缓冲区 —— void InitPickFrameBuffer(int viewportWidth...
        /// <summary>
        /// 初始化拾取帧缓冲区
        /// </summary>
        /// <param name="viewportWidth">视口宽度</param>
        /// <param name="viewportHeight">视口高度</param>
        private void InitPickFrameBuffer(int viewportWidth, int viewportHeight)
        {
            if (this._pickFrameBuffer == null)
            {
                this._pickFrameBuffer = FrameBuffer.CreateWithDepthBuffer(viewportWidth, viewportHeight);
            }
            else
            {
                if (this._pickFrameBuffer.Width == viewportWidth && this._pickFrameBuffer.Height == viewportHeight)
                {
                    return;
                }

                this._pickFrameBuffer.Dispose();
                this._pickFrameBuffer = FrameBuffer.CreateWithDepthBuffer(viewportWidth, viewportHeight);
            }
        }
        #endregion

        #endregion
    }
}
