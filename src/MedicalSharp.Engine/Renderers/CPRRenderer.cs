using MedicalSharp.Engine.Base;
using MedicalSharp.Engine.Managers;
using MedicalSharp.Engine.Protocols;
using MedicalSharp.Engine.Renderables;
using MedicalSharp.Engine.Resources;
using MedicalSharp.Primitives.Algorithms;
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
    /// CPR渲染器
    /// </summary>
    public class CPRRenderer : Renderer
    {
        #region # 字段及构造器

        /// <summary>
        /// 曲线
        /// </summary>
        private Curve _curve;

        /// <summary>
        /// 曲线框架
        /// </summary>
        private CurveFrame _curveFrame;

        /// <summary>
        /// 单位平面
        /// </summary>
        private readonly VertexBuffer _unitPlane;

        /// <summary>
        /// 创建CPR渲染器构造器
        /// </summary>
        /// <param name="camera">CPR相机</param>
        public CPRRenderer(CPRCamera camera)
            : base(camera)
        {
            this._unitPlane = new VertexBuffer(ResourceManager.UnitPlane);
            this.RenderMode = CPRRenderMode.Gray;
            this.WindowWidth = 400;
            this.WindowCenter = 40;
            this.Brightness = 1.0f;
            this.Contrast = 1.0f;
            this.CPRMode = CPRMode.Straightened;
            this.RadialWidth = 0.1f;
            this.RotationAngle = 0f;
            this.ProjectionThickness = 0.05f;
            this.MaxStepsCount = 100;
            this.ProjectionMode = IntensityProjectionMode.AIP;
            this.ProjectionAxis = -Vector3.UnitY;
            this.StraightenDirection = CPRStraightenDirection.Horizontal;
            this.ArcPosition = 0.5f;
            this.CrossSectionSize = 0.1f;
        }

        #endregion

        #region # 属性

        #region 渲染模式 —— CPRRenderMode RenderMode
        /// <summary>
        /// 渲染模式
        /// </summary>
        public CPRRenderMode RenderMode { get; private set; }
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

        #region CPR模式 —— CPRMode CPRMode
        /// <summary>
        /// CPR模式
        /// </summary>
        public CPRMode CPRMode { get; private set; }
        #endregion

        #region 径向宽度 —— float RadialWidth
        /// <summary>
        /// 径向宽度
        /// </summary>
        /// <remarks>世界空间，Straightened/Projected使用</remarks>
        public float RadialWidth { get; private set; }
        #endregion

        #region 旋转角度 —— float RotationAngle
        /// <summary>
        /// 旋转角度
        /// </summary>
        /// <remarks>Straightened/Projected使用</remarks>
        public float RotationAngle { get; private set; }
        #endregion

        #region 投影厚度 —— float ProjectionThickness
        /// <summary>
        /// 投影厚度
        /// </summary>
        /// <remarks>世界空间，Projected使用</remarks>
        public float ProjectionThickness { get; private set; }
        #endregion

        #region 最大步数 —— int MaxStepsCount
        /// <summary>
        /// 最大步数
        /// </summary>
        /// <remarks>Projected使用</remarks>
        public int MaxStepsCount { get; private set; }
        #endregion

        #region 投影模式 —— IntensityProjectionMode ProjectionMode
        /// <summary>
        /// 投影模式
        /// </summary>
        /// <remarks>Projected使用</remarks>
        public IntensityProjectionMode ProjectionMode { get; private set; }
        #endregion

        #region 投影轴 —— Vector3 ProjectionAxis
        /// <summary>
        /// 投影轴
        /// </summary>
        /// <remarks>全局投影方向（单位向量），Projected使用。默认Y轴（水平投影）</remarks>
        public Vector3 ProjectionAxis { get; private set; }
        #endregion

        #region 投影范围 —— float ProjectionRange
        /// <summary>
        /// 投影范围
        /// </summary>
        /// <remarks>Projected使用</remarks>
        public float ProjectionRange { get; private set; }
        #endregion

        #region 拉直方向 —— CPRStraightenDirection StraightenDirection
        /// <summary>
        /// 拉直方向
        /// </summary>
        /// <remarks>Straightened使用</remarks>
        public CPRStraightenDirection StraightenDirection { get; private set; }
        #endregion

        #region 弧长位置 —— float ArcPosition
        /// <summary>
        /// 弧长位置
        /// </summary>
        /// <remarks>归一化值0~1，CrossSectional使用</remarks>
        public float ArcPosition { get; private set; }
        #endregion

        #region 剖面尺寸 —— float CrossSectionSize
        /// <summary>
        /// 剖面尺寸
        /// </summary>
        /// <remarks>世界空间，CrossSectional使用</remarks>
        public float CrossSectionSize { get; private set; }
        #endregion

        #region 只读属性 - 曲线 —— Curve Curve
        /// <summary>
        /// 只读属性 - 曲线
        /// </summary>
        public Curve Curve
        {
            get => this._curve;
        }
        #endregion

        #region 只读属性 - 曲线框架 —— CurveFrame CurveFrame
        /// <summary>
        /// 只读属性 - 曲线框架
        /// </summary>
        public CurveFrame CurveFrame
        {
            get => this._curveFrame;
        }
        #endregion

        #region 只读属性 - CPR相机 —— CPRCamera CPRCamera
        /// <summary>
        /// 只读属性 - CPR相机
        /// </summary>
        public CPRCamera CPRCamera
        {
            get => base.Camera as CPRCamera;
        }
        #endregion

        #endregion

        #region # 方法

        #region 切换渲染模式 —— void SwitchRenderMode(CPRRenderMode renderMode)
        /// <summary>
        /// 切换渲染模式
        /// </summary>
        /// <param name="renderMode">渲染模式</param>
        public void SwitchRenderMode(CPRRenderMode renderMode)
        {
            this.RenderMode = renderMode;
        }
        #endregion

        #region 切换CPR模式 —— void SwitchCPRMode(CPRMode cprMode)
        /// <summary>
        /// 切换CPR模式
        /// </summary>
        /// <param name="cprMode">CPR模式</param>
        public void SwitchCPRMode(CPRMode cprMode)
        {
            this.CPRMode = cprMode;
        }
        #endregion

        #region 切换投影模式 —— void SwitchProjectionMode(IntensityProjectionMode...
        /// <summary>
        /// 切换投影模式
        /// </summary>
        /// <param name="projectionMode">密度投影模式</param>
        public void SwitchProjectionMode(IntensityProjectionMode projectionMode)
        {
            this.ProjectionMode = projectionMode;
        }
        #endregion

        #region 切换投影轴 —— void SwitchProjectionAxis(Vector3 axis)
        /// <summary>
        /// 切换投影轴
        /// </summary>
        public void SwitchProjectionAxis(Vector3 axis)
        {
            this.ProjectionAxis = Vector3.Normalize(axis);
        }
        #endregion

        #region 切换拉直方向 —— void SwitchStraightenDirection(CPRStraightenDirection...
        /// <summary>
        /// 切换拉直方向
        /// </summary>
        /// <param name="straightenDirection">拉直方向</param>
        public void SwitchStraightenDirection(CPRStraightenDirection straightenDirection)
        {
            this.StraightenDirection = straightenDirection;
        }
        #endregion

        #region 绑定曲线 —— void BindCurve(Curve curve)
        /// <summary>
        /// 绑定曲线
        /// </summary>
        /// <param name="curve">曲线</param>
        public void BindCurve(Curve curve)
        {
            #region # 验证

            if (curve == null || !curve.FrenetFrames.Any())
            {
                throw new ArgumentNullException(nameof(curve), "曲线不可为空且必须包含Frenet框架！");
            }

            #endregion

            this._curve = curve;
            if (this._curveFrame == null)
            {
                this._curveFrame = new CurveFrame(curve);
            }
            else
            {
                this._curveFrame.Update(curve);
            }
        }
        #endregion

        #region 初始化投影范围 —— void InitProjectionRange(Vector3 volumeScale)
        /// <summary>
        /// 初始化投影范围
        /// </summary>
        /// <param name="volumeScale">体积缩放</param>
        public void InitProjectionRange(Vector3 volumeScale)
        {
            this.ProjectionRange = Math.Abs(Vector3.Dot(volumeScale, this.ProjectionAxis));
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

        #region 设置拉直图选项 —— void SetStraightenedOptions(float radialWidth, float rotationAngle)
        /// <summary>
        /// 设置拉直图选项
        /// </summary>
        /// <param name="radialWidth">径向宽度</param>
        /// <param name="rotationAngle">旋转角度</param>
        public void SetStraightenedOptions(float radialWidth = 0.1f, float rotationAngle = 0f)
        {
            #region # 验证

            if (radialWidth <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(radialWidth), "径向宽度必须大于0！");
            }

            #endregion

            this.RadialWidth = radialWidth;
            this.RotationAngle = GeometryAlgorithms.NormalizeAngle(rotationAngle);
        }
        #endregion

        #region 设置投影图选项 —— void SetProjectedOptions(float projectionThickness, int maxStepsCount)
        /// <summary>
        /// 设置投影图选项
        /// </summary>
        /// <param name="projectionThickness">投影厚度</param>
        /// <param name="maxStepsCount">最大步数</param>
        public void SetProjectedOptions(float projectionThickness = 0.05f, int maxStepsCount = 100)
        {
            #region # 验证

            if (projectionThickness <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(projectionThickness), "投影厚度必须大于0！");
            }
            if (maxStepsCount < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(maxStepsCount), "最大步数必须大于等于1！");
            }

            #endregion

            this.ProjectionThickness = projectionThickness;
            this.MaxStepsCount = maxStepsCount;
        }
        #endregion

        #region 设置剖面图选项 —— void SetCrossSectionalOptions(float arcPosition, float crossSectionSize)
        /// <summary>
        /// 设置剖面图选项
        /// </summary>
        /// <param name="arcPosition">弧长位置</param>
        /// <param name="crossSectionSize">剖面尺寸</param>
        public void SetCrossSectionalOptions(float arcPosition = 0.5f, float crossSectionSize = 0.1f)
        {
            #region # 验证

            if (arcPosition < 0f || arcPosition > 1f)
            {
                throw new ArgumentOutOfRangeException(nameof(arcPosition), "弧长位置必须在0~1之间！");
            }
            if (crossSectionSize <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(crossSectionSize), "剖面尺寸必须大于0！");
            }

            #endregion

            this.ArcPosition = arcPosition;
            this.CrossSectionSize = crossSectionSize;
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
                throw new InvalidOperationException("CPR相机不可为空！");
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
            if (this._curve == null)
            {
                throw new InvalidOperationException("曲线不可为空！");
            }
            if (this._curveFrame == null)
            {
                throw new InvalidOperationException("曲线框架不可为空！");
            }

            #endregion

            //设置相机视口尺寸
            this.Camera.SetViewportSize(viewportWidth, viewportHeight);

            //渲染上下文
            RenderContext3D renderContext = new RenderContext3D(glContext, viewportWidth, viewportHeight, this.Camera.CameraMode, this.Camera.CameraPosition, this.Camera.LookDirection, this.Camera.UpDirection, this.Camera.RightDirection, this.Camera.ProjectionMatrix, this.Camera.ViewMatrix, this.CPRCamera.ZoomFactor);

            //宽高比
            float aspectRatio = this.Curve.TotalArcLength / this.RadialWidth;

            //模型矩阵
            Matrix4 modelMatrix;
            if (this.CPRMode == CPRMode.CrossSectional)
            {
                modelMatrix = Matrix4.Identity;
            }
            else if (this.CPRMode == CPRMode.Projected)
            {
                float scaleX = this.ProjectionRange;        //投影轴范围映射到屏幕宽度
                float scaleY = this.Curve.TotalArcLength;   //弧长映射到屏幕高度
                modelMatrix = Matrix4.CreateScale(scaleX, scaleY, 1f);
            }
            else
            {
                bool arcOnY = this.CPRMode == CPRMode.Straightened && this.StraightenDirection == CPRStraightenDirection.Vertical;
                modelMatrix = arcOnY
                    ? Matrix4.CreateScale(1f, aspectRatio, 1f)
                    : Matrix4.CreateScale(aspectRatio, 1f, 1f);
            }

            //选择开启Shader程序
            ShaderProgram program = this.CPRMode switch
            {
                CPRMode.Straightened => ShaderManager.CPRStraightenedProgram,
                CPRMode.Projected => ShaderManager.CPRProjectedProgram,
                CPRMode.CrossSectional => ShaderManager.CPRCrossSectionalProgram,
                _ => ShaderManager.CPRStraightenedProgram
            };
            program.Use();

            //设置MVP矩阵、缩放
            program.SetUniformMatrix4("u_ModelMatrix", modelMatrix);
            program.SetUniformMatrix4("u_ViewMatrix", renderContext.ViewMatrix);
            program.SetUniformMatrix4("u_ProjectionMatrix", renderContext.ProjectionMatrix);
            program.SetUniformVector3("u_VolumeScale", this.Renderable.VolumeMetadata.VolumeScale);

            //设置渲染模式
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

            //设置CPR参数
            switch (this.CPRMode)
            {
                case CPRMode.Straightened:
                    program.SetUniformFloat("u_RadialWidth", this.RadialWidth);
                    program.SetUniformFloat("u_RotationAngle", this.RotationAngle);
                    program.SetUniformInt("u_StraightenDirection", (int)this.StraightenDirection);
                    break;
                case CPRMode.Projected:
                    program.SetUniformVector3("u_ProjectionAxis", this.ProjectionAxis);
                    program.SetUniformFloat("u_ProjectionRange", this.ProjectionRange);
                    program.SetUniformFloat("u_ProjectionThickness", this.Renderable.VolumeMetadata.VolumeScale.Length);
                    program.SetUniformInt("u_MaxStepsCount", this.MaxStepsCount);
                    program.SetUniformInt("u_ProjectionMode", (int)this.ProjectionMode);
                    break;
                case CPRMode.CrossSectional:
                    program.SetUniformFloat("u_ArcPosition", this.ArcPosition);
                    program.SetUniformFloat("u_CrossSectionSize", this.CrossSectionSize);
                    break;
            }

            //绑定体数据纹理（0~3号单元）
            this.Renderable.PreviewTexture.Bind(0);
            this.Renderable.MarkTexture.Bind(1);
            this.TransferFunction.Texture.Bind(2);
            this.MarkStrategy.Texture.Bind(3);
            program.SetUniformInt("u_PreviewTexture", 0);
            program.SetUniformInt("u_MarkTexture", 1);
            program.SetUniformInt("u_TransferFunction", 2);
            program.SetUniformInt("u_MarkStrategy", 3);

            //绑定CurveFrame纹理（4~7号单元）
            this._curveFrame.PositionTexture.Bind(4);
            this._curveFrame.TangentTexture.Bind(5);
            this._curveFrame.NormalTexture.Bind(6);
            this._curveFrame.BinormalTexture.Bind(7);
            program.SetUniformInt("u_PositionTexture", 4);
            program.SetUniformInt("u_TangentTexture", 5);
            program.SetUniformInt("u_NormalTexture", 6);
            program.SetUniformInt("u_BinormalTexture", 7);

            //绘制平面
            this._unitPlane.Draw(glContext, PrimitiveType.Triangles);

            //解绑纹理
            this.Renderable.PreviewTexture.Unbind();
            this.Renderable.MarkTexture.Unbind();
            this.TransferFunction.Texture.Unbind();
            this.MarkStrategy.Texture.Unbind();
            this._curveFrame.PositionTexture.Unbind();
            this._curveFrame.TangentTexture.Unbind();
            this._curveFrame.NormalTexture.Unbind();
            this._curveFrame.BinormalTexture.Unbind();

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

            this._curveFrame?.Dispose();
            this._unitPlane.Dispose();
            this._disposed = true;
        }
        #endregion

        #endregion
    }
}
