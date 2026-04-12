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
                Vector3 localCenter = this._selectedVisual.Renderable.BoundingBox.Center;
                Vector3 worldCenter = Vector3.TransformPosition(localCenter, modelMatrix);

                //获取鼠标射线
                Point mousePos2D = eventArgs.GetPosition(viewport);
                Ray ray = viewport.UnProject(mousePos2D);

                //移动平面上的交点
                bool success = ray.IntersectsPlane(worldCenter, viewport.Camera.LookDirection, out Vector3 hitPoint);

                //旋转
                if (success &&
                    eventArgs.Properties.IsLeftButtonPressed &&
                    KeyModifiers.Alt == (eventArgs.KeyModifiers & KeyModifiers.Alt))
                {
                    //设置光标
                    viewport.Cursor = new Cursor(StandardCursorType.SizeWestEast);

                    float deltaX = (float)(mousePos2D.X - this._selectedPoint2D!.Value.X);
                    float deltaY = (float)(mousePos2D.Y - this._selectedPoint2D!.Value.Y);

                    //旋转轴
                    Vector3 axisY = viewport.Camera.UpDirection.Normalized();
                    Vector3 axisX = viewport.Camera.RightDirection.Normalized();
                    this._selectedVisual.Renderable.Transform.Rotate(deltaX, axisY);
                    this._selectedVisual.Renderable.Transform.Rotate(deltaY, axisX);

                    viewport.RequestNextFrameRendering();

                    this._selectedPoint2D = mousePos2D;
                    eventArgs.Handled = true;
                    return;
                }

                //平移
                if (success && eventArgs.Properties.IsLeftButtonPressed)
                {
                    viewport.Cursor = new Cursor(StandardCursorType.Hand);

                    this._selectedVisual.Renderable.Transform.SetPosition(hitPoint - localCenter);
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
