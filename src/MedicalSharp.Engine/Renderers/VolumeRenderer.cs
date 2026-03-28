using MedicalSharp.Engine.Cameras;
using MedicalSharp.Engine.Renderables;
using MedicalSharp.Engine.Resources;
using MedicalSharp.Engine.Shaders;
using MedicalSharp.Engine.ValueTypes;
using OpenTK.Graphics.OpenGL4;
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
        /// 拾取帧缓冲区
        /// </summary>
        private FrameBuffer _pickFrameBuffer;

        /// <summary>
        /// 拾取Shader程序
        /// </summary>
        private ShaderProgram _pickProgram;

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
            this._unitCube = new VertexBuffer(ResourceManager.UnitCube);
            this._unitCube.Setup();
            this.TransferFunction = new TransferFunction();
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
            this._unitCube = new VertexBuffer(ResourceManager.UnitCube);
            this._unitCube.Setup();
            this.TransferFunction = new TransferFunction();
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
            this._unitCube = new VertexBuffer(ResourceManager.UnitCube);
            this._unitCube.Setup();
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

            #endregion

            //设置相机视口尺寸
            this.Camera.SetViewportSize(viewportWidth, viewportHeight);

            //渲染上下文
            RenderContext renderContext = new RenderContext(viewportWidth, viewportHeight, this.Camera.CameraPosition, this.Camera.LookDirection, this.Camera.ProjectionMatrix, this.Camera.ViewMatrix);

            //开启Shader程序
            this.Program.Use();

            //处理缩放
            Matrix4 volumeScaleMatrix = Matrix4.CreateScale(this.Renderable.VolumeScale);

            //设置MVP矩阵、相机位置、缩放
            this.Program.SetUniformMatrix4("u_ProjectionMatrix", renderContext.ProjectionMatrix);
            this.Program.SetUniformMatrix4("u_ViewMatrix", renderContext.ViewMatrix);
            this.Program.SetUniformMatrix4("u_ModelMatrix", this.Renderable.ModelMatrix * volumeScaleMatrix);
            this.Program.SetUniformVector3("u_CameraPosition", renderContext.CameraPosition);
            this.Program.SetUniformVector3("u_VolumeScale", this.Renderable.VolumeScale);

            this.Program.SetUniformFloat("u_RescaleSlope", this.Renderable.RescaleSlope);
            this.Program.SetUniformFloat("u_RescaleIntercept", this.Renderable.RescaleIntercept);

            //绑定纹理
            this.Renderable.VolumeTexture.Bind(0);
            this.TransferFunction.Texture.Bind(1);

            this.Program.SetUniformInt("u_VolumeTexture", 0);
            this.Program.SetUniformInt("u_TransferFunction", 1);

            //设置渲染参数
            this.Program.SetUniformFloat("u_WindowCenter", this.WindowCenter);
            this.Program.SetUniformFloat("u_WindowWidth", this.WindowWidth);
            this.Program.SetUniformFloat("u_StepSize", this.StepSize);
            this.Program.SetUniformFloat("u_Brightness", this.Brightness);
            this.Program.SetUniformFloat("u_DensityScale", this.DensityScale);
            this.Program.SetUniformInt("u_MaxStepsCount", this.MaxStepsCount);
            this.Program.SetUniformFloat("u_OpacityThreshold", this.OpacityThreshold);

            //绘制模型
            this._unitCube.Draw(PrimitiveType.Triangles);

            //解绑纹理
            this.Renderable.VolumeTexture.Unbind();
            this.TransferFunction.Texture.Unbind();

            //取消使用
            this.Program.Unuse();

            //触发渲染事件
            this.Renderable.OnRender(renderContext);
        }
        #endregion

        #region 拾取体素 —— Vector3i? PickVoxel(Ray ray, float viewportWidth, float viewportHeight)
        /// <summary>
        /// 拾取体素
        /// </summary>
        /// <param name="ray">射线（世界空间）</param>
        /// <param name="viewportWidth">视口宽度</param>
        /// <param name="viewportHeight">视口高度</param>
        /// <returns>体素坐标，未命中返回null</returns>
        public Vector3i? PickVoxel(Ray ray, float viewportWidth, float viewportHeight)
        {
            #region # 验证

            if (this.Renderable == null)
            {
                return null;
            }
            if (this.Camera == null)
            {
                return null;
            }
            if (this._pickProgram == null)
            {
                return null;
            }

            #endregion

            //使用1/4分辨率加速
            int pickWidth = Math.Max(1, (int)viewportWidth / 4);
            int pickHeight = Math.Max(1, (int)viewportHeight / 4);

            //确保拾取FBO存在
            this.InitPickFrameBuffer(pickWidth, pickHeight);

            //渲染到拾取FBO
            this.RenderPickFrameBuffer(ray, pickWidth, pickHeight);

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
                return null;
            }

            //转换体素坐标
            int voxelX = (int)Math.Ceiling(pixel[0] * this.Renderable.VolumeTexture.Width);
            int voxelY = (int)Math.Ceiling(pixel[1] * this.Renderable.VolumeTexture.Height);
            int voxelZ = (int)Math.Ceiling(pixel[2] * this.Renderable.VolumeTexture.Depth);
            voxelX = Math.Clamp(voxelX, 0, this.Renderable.VolumeTexture.Width - 1);
            voxelY = Math.Clamp(voxelY, 0, this.Renderable.VolumeTexture.Height - 1);
            voxelZ = Math.Clamp(voxelZ, 0, this.Renderable.VolumeTexture.Depth - 1);

            return new Vector3i(voxelX, voxelY, voxelZ);
        }
        #endregion

        #region 释放资源 —— void Dispose()
        /// <summary>
        /// 释放资源
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();

            this._pickFrameBuffer.Dispose();
            this._pickProgram.Dispose();
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

            this._pickProgram = new ShaderProgram();
            this._pickProgram.ReadVertexShaderFromFile("Shaders/GLSLs/raycast.vert");
            this._pickProgram.ReadFragmentShaderFromFile("Shaders/GLSLs/raycast_pick.frag");
            this._pickProgram.Build();
        }
        #endregion

        #region 渲染拾取帧缓冲区 —— void RenderPickFrameBuffer(Ray ray, int viewportWidth...
        /// <summary>
        /// 渲染拾取帧缓冲区
        /// </summary>
        /// <param name="ray">射线（世界空间）</param>
        /// <param name="viewportWidth">视口宽度</param>
        /// <param name="viewportHeight">视口高度</param>
        private void RenderPickFrameBuffer(Ray ray, int viewportWidth, int viewportHeight)
        {
            #region # 验证

            if (this._pickProgram == null)
            {
                return;
            }
            if (this.Renderable == null)
            {
                return;
            }

            #endregion

            //绑定拾取FBO
            this._pickFrameBuffer.Bind();
            GL.Viewport(0, 0, viewportWidth, viewportHeight);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            //使用拾取Shader
            this._pickProgram.Use();

            //设置矩阵
            Matrix4 volumeScaleMatrix = Matrix4.CreateScale(this.Renderable.VolumeScale);
            Matrix4 modelMatrix = this.Renderable.ModelMatrix * volumeScaleMatrix;

            this._pickProgram.SetUniformMatrix4("u_ProjectionMatrix", this.Camera.ProjectionMatrix);
            this._pickProgram.SetUniformMatrix4("u_ViewMatrix", this.Camera.ViewMatrix);
            this._pickProgram.SetUniformMatrix4("u_ModelMatrix", modelMatrix);

            //传入射线参数
            this._pickProgram.SetUniformVector3("u_RayOrigin", ray.Position);
            this._pickProgram.SetUniformVector3("u_RayDirection", ray.Direction);

            //相机参数
            this._pickProgram.SetUniformVector3("u_CameraPosition", this.Camera.CameraPosition);
            this._pickProgram.SetUniformVector3("u_VolumeScale", this.Renderable.VolumeScale);

            //DICOM参数
            this._pickProgram.SetUniformFloat("u_RescaleSlope", this.Renderable.RescaleSlope);
            this._pickProgram.SetUniformFloat("u_RescaleIntercept", this.Renderable.RescaleIntercept);

            //窗宽窗位（与渲染一致）
            this._pickProgram.SetUniformFloat("u_WindowCenter", this.WindowCenter);
            this._pickProgram.SetUniformFloat("u_WindowWidth", this.WindowWidth);

            //材质参数（与渲染一致）
            this._pickProgram.SetUniformFloat("u_Brightness", this.Brightness);
            this._pickProgram.SetUniformFloat("u_DensityScale", this.DensityScale);

            //采样参数（与渲染一致）
            this._pickProgram.SetUniformFloat("u_StepSize", this.StepSize);
            this._pickProgram.SetUniformInt("u_MaxStepsCount", this.MaxStepsCount);
            this._pickProgram.SetUniformFloat("u_OpacityThreshold", this.OpacityThreshold);

            //绑定纹理
            this.Renderable.VolumeTexture.Bind(0);
            this.TransferFunction.Texture.Bind(1);
            this._pickProgram.SetUniformInt("u_VolumeTexture", 0);
            this._pickProgram.SetUniformInt("u_TransferFunction", 1);

            //绘制
            this._unitCube.Draw(PrimitiveType.Triangles);

            //解绑纹理
            this.Renderable.VolumeTexture.Unbind();
            this.TransferFunction.Texture.Unbind();

            //取消使用
            this._pickProgram.Unuse();
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
