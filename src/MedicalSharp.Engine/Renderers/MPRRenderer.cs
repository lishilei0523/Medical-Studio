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
        /// <param name="camera">相机</param>
        public MPRRenderer(MPRCamera camera)
            : base(camera)
        {
            //默认值
            this._unitPlane = new VertexBuffer(ResourceManager.UnitPlane);
            this._unitPlane.Setup();
            this.TransferFunction = new TransferFunction();
            this.InitShaderProgram();
        }

        /// <summary>
        /// 创建MPR渲染器构造器
        /// </summary>
        /// <param name="camera">相机</param>
        /// <param name="program">Shader程序</param>
        public MPRRenderer(MPRCamera camera, ShaderProgram program)
            : base(camera, program)
        {
            //默认值
            this._unitPlane = new VertexBuffer(ResourceManager.UnitPlane);
            this._unitPlane.Setup();
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
            get
            {
                if (base.Camera == null)
                {
                    return null;
                }

                return (MPRCamera)base.Camera;
            }
        }
        #endregion

        #endregion

        #region # 方法

        //Public

        #region 设置相机 —— void SetCamera(MPRCamera camera)
        /// <summary>
        /// 设置相机
        /// </summary>
        public void SetCamera(MPRCamera camera)
        {
            base.SetCamera(camera);

            if (this.Renderable != null)
            {
                this.UpdateCameraFromRenderable();
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

            if (this.MPRCamera != null)
            {
                this.UpdateCameraFromRenderable();
            }
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
            #region 验证

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

            //设置相机视口
            this.Camera.SetViewportSize(viewportWidth, viewportHeight);

            //使用Shader
            this.Program.Use();

            //设置Uniform变量
            this.Program.SetUniformMatrix4("u_ModelMatrix", this.GetPlaneModelMatrix());
            this.Program.SetUniformMatrix4("u_ViewMatrix", this.Camera.ViewMatrix);
            this.Program.SetUniformMatrix4("u_ProjectionMatrix", this.Camera.ProjectionMatrix);

            this.Program.SetUniformInt("u_PlaneType", (int)this.MPRCamera.PlaneType);
            this.Program.SetUniformFloat("u_SliceIndex", this.MPRCamera.SliceIndex);

            this.Program.SetUniformFloat("u_WindowWidth", this.WindowWidth);
            this.Program.SetUniformFloat("u_WindowCenter", this.WindowCenter);
            this.Program.SetUniformFloat("u_Brightness", this.Brightness);
            this.Program.SetUniformFloat("u_Contrast", this.Contrast);

            this.Program.SetUniformFloat("u_RescaleSlope", this.Renderable.RescaleSlope);
            this.Program.SetUniformFloat("u_RescaleIntercept", this.Renderable.RescaleIntercept);

            this.Program.SetUniformVector3("u_VolumeScale", this.Renderable.VolumeScale);
            this.Program.SetUniformVector3("u_VoxelSize", this.Renderable.VoxelSize);
            this.Program.SetUniformVector3("u_Spacing", this.Renderable.Spacing);

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

        #region 根据渲染对象更新相机参数 —— void UpdateCameraFromRenderable()
        /// <summary>
        /// 根据渲染对象更新相机参数
        /// </summary>
        private void UpdateCameraFromRenderable()
        {
            #region # 验证

            if (this.Renderable?.VolumeTexture == null || this.MPRCamera == null)
            {
                return;
            }

            #endregion

            //设置体积实际尺寸
            this.MPRCamera.VolumeActualSize = this.Renderable.ActualSize;

            //设置目标位置为图像原点
            //this.MPRCamera.TargetPosition = this.Renderable.Origin;

            //根据平面类型设置最大切片数
            int maxSlicesCount = this.MPRCamera.PlaneType switch
            {
                MPRPlaneType.Axial => this.Renderable.VolumeTexture.Depth,
                MPRPlaneType.Coronal => this.Renderable.VolumeTexture.Height,
                MPRPlaneType.Sagittal => this.Renderable.VolumeTexture.Width,
                _ => 100
            };

            this.MPRCamera.MaxSliceCount = maxSlicesCount;

            //设置切片间距为体素间距
            float sliceSpacing = this.MPRCamera.PlaneType switch
            {
                MPRPlaneType.Axial => this.Renderable.Spacing.Z,
                MPRPlaneType.Coronal => this.Renderable.Spacing.Y,
                MPRPlaneType.Sagittal => this.Renderable.Spacing.X,
                _ => 1.0f
            };

            this.MPRCamera.SliceSpacing = sliceSpacing;
            this.MPRCamera.SliceIndex = maxSlicesCount / 2;
        }
        #endregion

        #region 获取平面模型矩阵 —— Matrix4 GetPlaneModelMatrix()
        /// <summary>
        /// 获取平面模型矩阵
        /// </summary>
        private Matrix4 GetPlaneModelMatrix()
        {
            #region # 验证

            if (this.Renderable?.VolumeTexture == null)
            {
                return Matrix4.Identity;
            }

            #endregion

            //构建方向矩阵
            Matrix4 orientationMatrix = new Matrix4(
                new Vector4(this.Renderable.RowDirection, 0),
                new Vector4(this.Renderable.ColDirection, 0),
                new Vector4(this.Renderable.SliceDirection, 0),
                new Vector4(0, 0, 0, 1)
            );

            //根据平面类型构建缩放和旋转
            Matrix4 planeTransform = this.MPRCamera.PlaneType switch
            {
                MPRPlaneType.Axial =>       //XY平面
                    Matrix4.CreateScale(this.Renderable.VolumeScale.X, this.Renderable.VolumeScale.Y, 1),
                MPRPlaneType.Coronal =>     //XZ平面
                    Matrix4.CreateScale(this.Renderable.VolumeScale.X, this.Renderable.VolumeScale.Z, 1) *
                    Matrix4.CreateRotationX(MathHelper.DegreesToRadians(-90.0f)),
                MPRPlaneType.Sagittal =>    //YZ平面
                    Matrix4.CreateScale(this.Renderable.VolumeScale.Y, this.Renderable.VolumeScale.Z, 1) *
                    Matrix4.CreateRotationY(MathHelper.DegreesToRadians(90.0f))
                    ,
                _ => Matrix4.Identity
            };

            //组合变换：方向 * 平面变换
            return orientationMatrix * planeTransform;
        }
        #endregion

        #endregion
    }
}
