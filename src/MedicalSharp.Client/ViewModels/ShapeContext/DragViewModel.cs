using Avalonia;
using Avalonia.Collections;
using Avalonia.Input;
using Avalonia.Media;
using Caliburn.Micro;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Controls.Viewports;
using MedicalSharp.Controls.Visuals;
using MedicalSharp.Primitives.Cameras;
using MedicalSharp.Primitives.Interfaces;
using MedicalSharp.Primitives.Maths;
using MedicalSharp.Primitives.Models;
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
        private Vector2? _selectedPoint2D;

        /// <summary>
        /// 选中改变尺寸上下文
        /// </summary>
        private ResizeContext? _selectedResizeContext;

        /// <summary>
        /// 选中的顶点拖拽约束
        /// </summary>
        private VertexDragConstraint? _selectedVertexConstraint;

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
                bool success = viewport.FindNearestShape(mousePos2D.ToVector2(), out Vector3 mousePos3D, out Vector3 normal, out ShapeVisual3D visual3D, out Ray ray);
                if (success)
                {
                    this._selectedVisual = visual3D;
                    this._selectedPoint2D = mousePos2D.ToVector2();

                    Matrix4 modelMatrix = this._selectedVisual.Transform.Matrix;
                    Matrix4 worldToLocal = Matrix4.Invert(modelMatrix);
                    Ray localRay = ray.Transform(worldToLocal);

                    //可改变尺寸对象
                    if (visual3D is IResizable resizable)
                    {
                        if (resizable.TryGetResizeAxis(localRay, out ResizeContext resizeContext))
                        {
                            this._selectedResizeContext = resizeContext;
                            this._selectedVertexConstraint = null;
                        }
                    }

                    //调整顶点
                    if (visual3D is IVertexEditable vertexEditable)
                    {
                        if (vertexEditable.TryGetVertexDrag(localRay, out VertexDragConstraint constraint))
                        {
                            this._selectedVertexConstraint = constraint;
                            this._selectedResizeContext = null;
                        }
                    }

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
                Matrix4 modelMatrix = this._selectedVisual.Transform.Matrix;
                Vector3 localCenter = this._selectedVisual.Bounds.Center;
                Vector3 worldCenter = Vector3.TransformPosition(localCenter, modelMatrix);

                //获取鼠标射线
                Vector2 mousePos2D = eventArgs.GetPosition(viewport).ToVector2();
                Ray ray = viewport.UnProject(mousePos2D);

                //移动平面上的交点
                bool success = ray.IntersectsPlane(worldCenter, viewport.Camera.LookDirection, out Vector3 hitPoint, out _);

                //调整尺寸
                if (success &&
                    eventArgs.Properties.IsLeftButtonPressed &&
                    KeyModifiers.Control == (eventArgs.KeyModifiers & KeyModifiers.Control))
                {
                    //设置光标
                    viewport.Cursor = new Cursor(StandardCursorType.Cross);

                    //构造局部射线
                    Matrix4 worldToLocal = Matrix4.Invert(modelMatrix);
                    Ray localRay = ray.Transform(worldToLocal);
                    Vector3 localLookDirection = Vector3.TransformNormal(viewport.Camera.LookDirection, worldToLocal).Normalized();

                    //可调整尺寸类型
                    if (this._selectedVisual is IResizable resizable && this._selectedResizeContext.HasValue)
                    {
                        //构造平面法向量：包含伸缩轴，且面向相机
                        ResizeContext resizeContext = this._selectedResizeContext.Value;
                        Vector3 planeNormal = Vector3.Cross(resizeContext.Axis, Vector3.Cross(localLookDirection, resizeContext.Axis));
                        if (planeNormal.LengthSquared < 0.001f)
                        {
                            planeNormal = Vector3.UnitY;  //兜底
                        }
                        planeNormal.Normalize();

                        if (localRay.IntersectsPlane(resizeContext.Anchor, planeNormal, out Vector3 localHitPoint, out _))
                        {
                            resizable.ApplyResize(resizeContext, localHitPoint);
                            viewport.RequestNextFrameRendering();
                        }
                    }

                    //可顶点编辑类型
                    if (this._selectedVisual is IVertexEditable vertexEditable && this._selectedVertexConstraint.HasValue)
                    {
                        VertexDragConstraint constraint = this._selectedVertexConstraint.Value;
                        if (localRay.IntersectsPlane(constraint.Anchor, localLookDirection, out Vector3 localHitPoint, out _))
                        {
                            vertexEditable.MoveVertex(constraint, localHitPoint);
                            viewport.RequestNextFrameRendering();
                        }
                    }

                    eventArgs.Handled = true;
                    return;
                }

                //旋转
                if (success &&
                    eventArgs.Properties.IsLeftButtonPressed &&
                    KeyModifiers.Alt == (eventArgs.KeyModifiers & KeyModifiers.Alt))
                {
                    float deltaX = (float)(mousePos2D.X - this._selectedPoint2D!.Value.X);
                    float deltaY = (float)(mousePos2D.Y - this._selectedPoint2D!.Value.Y);

                    //设置光标
                    if (deltaX != 0 && deltaY == 0)
                    {
                        viewport.Cursor = new Cursor(StandardCursorType.SizeWestEast);
                    }
                    if (deltaX == 0 && deltaY != 0)
                    {
                        viewport.Cursor = new Cursor(StandardCursorType.SizeNorthSouth);
                    }
                    if (deltaX != 0 && deltaY != 0)
                    {
                        viewport.Cursor = new Cursor(StandardCursorType.SizeAll);
                    }

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
            this._selectedResizeContext = null;
            this._selectedVertexConstraint = null;

            viewport.RequestNextFrameRendering();
        }
        #endregion

        #endregion
    }
}
