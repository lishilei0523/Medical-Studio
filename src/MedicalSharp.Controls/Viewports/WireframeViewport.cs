using Avalonia;
using Avalonia.Collections;
using Avalonia.Metadata;
using MedicalSharp.Controls.Base;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Controls.Inputs;
using MedicalSharp.Controls.Visuals;
using MedicalSharp.Engine.Cameras;
using MedicalSharp.Engine.Renderers;
using MedicalSharp.Engine.ValueTypes;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace MedicalSharp.Controls.Viewports
{
    /// <summary>
    /// 线框渲染视口
    /// </summary>
    public class WireframeViewport : OpenTKViewport
    {
        #region # 字段及构造器

        /// <summary>
        /// 线框渲染器
        /// </summary>
        private WireframeRenderer _renderer;

        /// <summary>
        /// 边界3D元素列表
        /// </summary>
        private readonly IList<BoundingVisual3D> _boundingVisual3Ds;

        /// <summary>
        /// 默认构造器
        /// </summary>
        public WireframeViewport()
        {
            this._boundingVisual3Ds = new List<BoundingVisual3D>();
            this.Children = new AvaloniaList<Visual3D>();
            this.Children.CollectionChanged += this.OnChildrenItemsChanged;
        }

        #endregion

        #region # 属性

        #region 子元素列表 —— AvaloniaList<Visual3D> Children
        /// <summary>
        /// 子元素列表
        /// </summary>
        [Content]
        public AvaloniaList<Visual3D> Children { get; private set; }
        #endregion

        #region 只读属性 - 线框渲染器 —— WireframeRenderer Renderer
        /// <summary>
        /// 只读属性 - 线框渲染器
        /// </summary>
        public WireframeRenderer Renderer
        {
            get => this._renderer;
        }
        #endregion

        #endregion

        #region # 方法

        #region 查找最近元素 —— bool FindNearest(Vector2 position, out Vector3 point...
        /// <summary>
        /// 查找最近元素
        /// </summary>
        /// <param name="position">2D位置</param>
        /// <param name="point">3D位置</param>
        /// <param name="normal">法向量</param>
        /// <param name="visual3D">边界3D元素</param>
        /// <returns>是否成功</returns>
        public bool FindNearest(Vector2 position, out Vector3 point, out Vector3 normal, out BoundingVisual3D visual3D)
        {
            Ray ray = Ray.UnProject(position, this.Camera.CameraPosition, this._viewportSize.ToVector2(), this.Camera.ProjectionMatrix, this.Camera.ViewMatrix);

            //快速检测
            IList<(float, BoundingVisual3D)> hitResults = new List<(float, BoundingVisual3D)>();
            foreach (BoundingVisual3D boundingVisual3D in this._boundingVisual3Ds)
            {
                bool intersects = boundingVisual3D.Renderable.IntersectsRay(ray, out float distance);
                if (intersects)
                {
                    hitResults.Add((distance, boundingVisual3D));
                }
            }

            //详细检测
            if (hitResults.Any())
            {
                (float, BoundingVisual3D) hitResult = hitResults.MinBy(x => x.Item1);
                bool intersects = hitResult.Item2.Renderable.IntersectsRay(ray, out float distance, out Vector3 hitPoint, out Vector3 hitNormal, out int hitTriangleIndex);
                if (intersects)
                {
                    point = hitPoint;
                    normal = hitNormal;
                    visual3D = hitResult.Item2;
                    return true;
                }
            }

            point = Vector3.Zero;
            normal = Vector3.Zero;
            visual3D = null;

            return false;
        }
        #endregion

        #region 查找最近位置 —— Vector3? FindNearestPoint(Vector2 position)
        /// <summary>
        /// 查找最近位置
        /// </summary>
        /// <param name="position">2D位置</param>
        /// <returns>3D位置</returns>
        public Vector3? FindNearestPoint(Vector2 position)
        {
            if (this.FindNearest(position, out Vector3 point, out _, out _))
            {
                return point;
            }

            return null;
        }
        #endregion

        #region 附加可视化树事件 —— override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs...
        /// <summary>
        /// 附加可视化树事件
        /// </summary>>
        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs eventArgs)
        {
            base.OnAttachedToVisualTree(eventArgs);

            foreach (Visual3D visual3D in this.Children)
            {
                visual3D.DataContext = this.DataContext;
            }
        }
        #endregion

        #region OpenTK初始化事件 —— override void OnOpenTKInit()
        /// <summary>
        /// OpenTK初始化事件
        /// </summary>
        protected override void OnOpenTKInit()
        {
            //InputManger默认值
            if (this.InputManager == null && this.Camera is OrbitCamera orbitCamera)
            {
                this.InputManager = new OrbitInputManager(orbitCamera);
            }

            this._renderer = new WireframeRenderer(this.Camera);
        }
        #endregion

        #region OpenTK渲染事件 —— override void OnOpenTKRender(PixelSize viewportSize)
        /// <summary>
        /// OpenTK渲染事件
        /// </summary>
        /// <param name="viewportSize">视口尺寸</param>
        protected override void OnOpenTKRender(PixelSize viewportSize)
        {
            //开启深度测试
            GL.Enable(EnableCap.DepthTest);

            //禁用面剔除
            GL.Disable(EnableCap.CullFace);

            //清空渲染对象
            this._boundingVisual3Ds.Clear();
            this._renderer.ClearItems();

            //填充渲染对象
            foreach (Visual3D visual3D in this.Children)
            {
                if (visual3D is BoundingVisual3D boundingVisual3D)
                {
                    this._boundingVisual3Ds.Add(boundingVisual3D);
                    this._renderer.AppendItem(boundingVisual3D.Renderable);
                }
                if (visual3D is BoundingItemPresenter itemPresenter)
                {
                    this._boundingVisual3Ds.Add(itemPresenter.Content);
                    this._renderer.AppendItem(itemPresenter.Content.Renderable);
                }
                if (visual3D is BoundingItemsPresenter itemsPresenter)
                {
                    foreach (BoundingVisual3D item in itemsPresenter.ItemsSource)
                    {
                        this._boundingVisual3Ds.Add(item);
                        this._renderer.AppendItem(item.Renderable);
                    }
                }
            }

            this._renderer.RenderFrame(viewportSize.Width, viewportSize.Height);
        }
        #endregion

        #region OpenTK卸载事件 —— override void OnOpenTKDeinit()
        /// <summary>
        /// OpenTK卸载事件
        /// </summary>
        protected override void OnOpenTKDeinit()
        {
            this._renderer.Dispose();
        }
        #endregion

        #region 子元素列表元素改变事件 —— void OnChildrenItemsChanged(object sender...
        /// <summary>
        /// 子元素列表元素改变事件
        /// </summary>
        private void OnChildrenItemsChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
        {
            if (eventArgs.Action == NotifyCollectionChangedAction.Add)
            {
                Visual3D visual3D = this.Children[eventArgs.NewStartingIndex];
                visual3D.DataContext = this.DataContext;
                if (visual3D is BoundingItemsPresenter itemsPresenter)
                {
                    itemsPresenter.ItemsSource.CollectionChanged += this.OnItemsPresenterItemsChanged;
                }
            }
        }
        #endregion

        #region 边界3D元素列表容器元素改变事件 —— void OnItemsPresenterItemsChanged(object sender...
        /// <summary>
        /// 边界3D元素列表容器元素改变事件
        /// </summary>
        private void OnItemsPresenterItemsChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
        {
            this.RequestNextFrameRendering();
        }
        #endregion

        #endregion
    }
}
