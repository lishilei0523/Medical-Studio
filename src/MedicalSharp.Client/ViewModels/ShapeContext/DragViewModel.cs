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
                Matrix4 modelMatrix = this._selectedVisual.Renderable.Transform.Matrix;  //模型的变换矩阵
                Vector3 oldVisualPos3D = modelMatrix.ExtractTranslation();               //模型平移位置

                //获取鼠标射线
                Point mousePos2D = eventArgs.GetPosition(viewport);
                Ray ray = viewport.UnProject(mousePos2D);

                //将变换到局部空间
                Matrix4 worldToLocal = Matrix4.Invert(modelMatrix);
                Ray localRay = ray.Transform(worldToLocal);
                Vector3 localLookDirection = Vector3.TransformNormal(viewport.Camera.LookDirection, worldToLocal);

                //移动平面上的交点
                bool success = localRay.IntersectsPlane(oldVisualPos3D, localLookDirection, out Vector3 hitPoint);
                if (success && eventArgs.Properties.IsLeftButtonPressed)
                {
                    viewport.Cursor = new Cursor(StandardCursorType.Hand);

                    //将局部交点变换回世界空间
                    Vector3 worldHitPoint = Vector3.TransformPosition(hitPoint, modelMatrix);

                    //计算物体的包围盒中心在世界空间的位置
                    Vector3 localCenter = this._selectedVisual.Renderable.BoundingBox.Center;
                    Vector3 worldCenter = Vector3.TransformPosition(localCenter, modelMatrix);

                    //计算新位置：当前变换位置 + (鼠标交点 - 世界包围盒中心)
                    Vector3 newVisualPos3D = oldVisualPos3D + (worldHitPoint - worldCenter);

                    this._selectedVisual.Renderable.Transform.SetPosition(newVisualPos3D);
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
