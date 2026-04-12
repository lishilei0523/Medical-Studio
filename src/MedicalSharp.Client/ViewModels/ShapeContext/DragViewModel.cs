using Avalonia;
using Avalonia.Collections;
using Avalonia.Input;
using Avalonia.Media;
using Caliburn.Micro;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Controls.Viewports;
using MedicalSharp.Controls.Visuals;
using MedicalSharp.Primitives.Cameras;
using MedicalSharp.Primitives.Maths;
using OpenTK.Mathematics;
using SD.Infrastructure.Avalonia.Caliburn.Aspects;
using SD.Infrastructure.Avalonia.Caliburn.Base;
using System.Threading;
using System.Threading.Tasks;
using Vector3 = OpenTK.Mathematics.Vector3;

namespace MedicalSharp.Client.ViewModels.ShapeContext
{
    /// <summary>
    /// 拖拽视图模型
    /// </summary>
    public class DragViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 选中的3D元素
        /// </summary>
        private ShapeVisual3D _selectedVisual;

        /// <summary>
        /// 选中2D点
        /// </summary>
        private Point? _selectedPoint2D;

        /// <summary>
        /// 窗口管理器
        /// </summary>
        private readonly IWindowManager _windowManager;

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public DragViewModel(IWindowManager windowManager)
        {
            this._windowManager = windowManager;

            //初始化相机
            Vector3 cameraPosition = new Vector3(0, 7, 0);
            Vector3 targetPosition = new Vector3(0.0f);
            Vector3 upDirection = new Vector3(0, 0, 1);
            this.OrbitCamera = new OrbitPerspectiveCamera(cameraPosition, targetPosition, upDirection);
        }

        #endregion

        #region # 属性

        #region 轨道相机 —— OrbitCamera OrbitCamera
        /// <summary>
        /// 轨道相机
        /// </summary>
        [DependencyProperty]
        public OrbitCamera OrbitCamera { get; set; }
        #endregion

        #region 包围球3D元素 —— BoundingSphereVisual3D Sphere
        /// <summary>
        /// 包围球3D元素
        /// </summary>
        [DependencyProperty]
        public BoundingSphereVisual3D Sphere { get; set; }
        #endregion

        #region 3D元素列表 —— AvaloniaList<ShapeVisual3D> Visual3Ds
        /// <summary>
        /// 3D元素列表
        /// </summary>
        [DependencyProperty]
        public AvaloniaList<ShapeVisual3D> Visual3Ds { get; set; }
        #endregion

        #endregion

        #region # 方法

        //Initializations

        #region 初始化 —— override Task OnInitializedAsync(CancellationToken...
        /// <summary>
        /// 初始化
        /// </summary>
        protected override Task OnInitializedAsync(CancellationToken cancellationToken)
        {
            this.Sphere = new BoundingSphereVisual3D
            {
                Radius = 1,
                Center = new Vector3D(-2, 0, 0),
                Stroke = Colors.Green,
                StrokeThickness = 1,
                Fill = Color.Parse("#0FFF0000")
            };
            this.Visual3Ds =
            [
                new BoundingBoxVisual3D
                {
                    Center = new Vector3D(2,0,0),
                    Stroke = Colors.Blue,
                    StrokeThickness = 1,
                    Fill = Color.Parse("#0FFFFF00")
                }
            ];

            return base.OnInitializedAsync(cancellationToken);
        }
        #endregion


        //Actions

        #region 视口鼠标按下事件 —— void OnViewportPointerPressed(ShapeViewport viewport...
        /// <summary>
        /// 视口鼠标按下事件
        /// </summary>
        public void OnViewportPointerPressed(ShapeViewport viewport, PointerPressedEventArgs eventArgs)
        {
            if (eventArgs.Properties.IsLeftButtonPressed)
            {
                Point mousePos2D = eventArgs.GetPosition(viewport);
                bool success = viewport.FindNearestShape(mousePos2D.ToVector2(), out Vector3 mousePos3D, out Vector3 normal, out ShapeVisual3D element);
                if (success)
                {
                    this._selectedVisual = element;
                    this._selectedPoint2D = mousePos2D;

                    eventArgs.Handled = true;
                    return;
                }
            }
        }
        #endregion

        #region 视口鼠标移动事件 —— void OnViewportPointerMoved(ShapeViewport viewport...
        /// <summary>
        /// 视口鼠标移动事件
        /// </summary>
        public void OnViewportPointerMoved(ShapeViewport viewport, PointerEventArgs eventArgs)
        {
            if (this._selectedVisual != null)
            {
                //计算模型位置
                Matrix4 modelMatrix = this._selectedVisual.Renderable.Transform.Matrix;  //模型变换矩阵
                Vector3 oldTranslation = modelMatrix.ExtractTranslation();               //模型平移
                Vector3 localCenter = this._selectedVisual.Renderable.BoundingBox.Center;
                Vector3 worldCenter = Vector3.TransformPosition(localCenter, modelMatrix);

                //获取鼠标射线
                Point mousePos2D = eventArgs.GetPosition(viewport);
                Ray ray = viewport.UnProject(mousePos2D);

                //将变换到局部空间
                Matrix4 worldToLocal = Matrix4.CreateTranslation(-oldTranslation); //Matrix4.Invert(modelMatrix);
                Ray localRay = ray.Transform(worldToLocal);
                Vector3 localLookDirection = Vector3.TransformNormal(viewport.Camera.LookDirection, worldToLocal);

                //移动平面上的交点
                bool success = localRay.IntersectsPlane(oldTranslation, localLookDirection, out Vector3 hitPoint);

                //旋转
                if (success &&
                    eventArgs.Properties.IsLeftButtonPressed &&
                    KeyModifiers.Alt == (eventArgs.KeyModifiers & KeyModifiers.Alt))
                {
                    //设置光标
                    viewport.Cursor = new Cursor(StandardCursorType.SizeWestEast);

                    float deltaX = (float)(mousePos2D.X - this._selectedPoint2D!.Value.X);
                    float deltaY = (float)(mousePos2D.Y - this._selectedPoint2D!.Value.Y);

                    //获取当前变换的分量
                    Vector3 translation = modelMatrix.ExtractTranslation();
                    Quaternion currentRotation = modelMatrix.ExtractRotation();
                    Vector3 scale = modelMatrix.ExtractScale();

                    //旋转轴
                    Vector3 axisY = viewport.Camera.UpDirection.Normalized();
                    Vector3 axisX = viewport.Camera.RightDirection.Normalized();

                    //计算绕包围盒中心的增量旋转
                    Quaternion deltaRotX = Quaternion.FromAxisAngle(axisY, deltaX * 0.01f);
                    Quaternion deltaRotY = Quaternion.FromAxisAngle(axisX, deltaY * 0.01f);
                    Quaternion deltaRotation = deltaRotX * deltaRotY;

                    //关键：计算新平移位置，物体当前位置与旋转中心的偏移
                    Vector3 offset = translation - worldCenter;

                    //旋转这个偏移
                    Vector3 rotatedOffset = Vector3.Transform(offset, deltaRotation);

                    //新位置 = 旋转中心 + 旋转后的偏移
                    Vector3 newTranslation = worldCenter + rotatedOffset;

                    //新旋转 = 增量旋转 × 当前旋转
                    Quaternion newRotation = Quaternion.Normalize(deltaRotation * currentRotation);

                    //重新构建矩阵
                    Matrix4 newMatrix = Matrix4.CreateScale(scale) *
                                        Matrix4.CreateFromQuaternion(newRotation) *
                                        Matrix4.CreateTranslation(newTranslation);

                    //应用到物体
                    this._selectedVisual.Renderable.Transform.SetMatrix(newMatrix);
                    viewport.RequestNextFrameRendering();

                    this._selectedPoint2D = mousePos2D;
                    eventArgs.Handled = true;
                    return;
                }

                //平移
                if (success && eventArgs.Properties.IsLeftButtonPressed)
                {
                    viewport.Cursor = new Cursor(StandardCursorType.Hand);

                    //将局部交点变换回世界空间
                    Vector3 worldHitPoint = Vector3.TransformPosition(hitPoint, modelMatrix);

                    //计算平移：当前平移 + (鼠标交点 - 世界包围盒中心)
                    Vector3 newTranslation = oldTranslation + (worldHitPoint - worldCenter);

                    this._selectedVisual.Renderable.Transform.SetPosition(newTranslation);
                    viewport.RequestNextFrameRendering();
                }
            }
        }
        #endregion

        #region 视口鼠标松开事件 —— void OnViewportPointerReleased(ShapeViewport viewport...
        /// <summary>
        /// 视口鼠标松开事件
        /// </summary>
        public void OnViewportPointerReleased(ShapeViewport viewport, PointerReleasedEventArgs eventArgs)
        {
            //设置光标
            viewport.Cursor = new Cursor(StandardCursorType.Arrow);

            //清空选中
            this._selectedVisual = null;
            this._selectedPoint2D = null;

            viewport.RequestNextFrameRendering();
        }
        #endregion

        #endregion
    }
}
