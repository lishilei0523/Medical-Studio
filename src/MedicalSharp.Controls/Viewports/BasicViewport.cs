using Avalonia;
using Avalonia.Collections;
using Avalonia.Metadata;
using MedicalSharp.Controls.Base;
using MedicalSharp.Controls.InputManagers;
using MedicalSharp.Controls.Interfaces;
using MedicalSharp.Controls.Visuals;
using MedicalSharp.Engine.Renderers;
using MedicalSharp.Primitives.Cameras;
using MedicalSharp.Primitives.Maths;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace MedicalSharp.Controls.Viewports
{
    /// <summary>
    /// 基础渲染视口
    /// </summary>
    public class BasicViewport : OpenTKViewport, IPickVisual3D
    {
        #region # 字段及构造器

        /// <summary>
        /// 形状渲染器
        /// </summary>
        protected ShapeRenderer _shapeRenderer;

        /// <summary>
        /// 形状3D元素列表
        /// </summary>
        protected readonly IList<ShapeVisual3D> _shapeVisual3Ds;

        /// <summary>
        /// 默认构造器
        /// </summary>
        public BasicViewport()
        {
            this._shapeVisual3Ds = new List<ShapeVisual3D>();
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

        #region 只读属性 - 形状渲染器 —— ShapeRenderer ShapeRenderer
        /// <summary>
        /// 只读属性 - 形状渲染器
        /// </summary>
        public ShapeRenderer ShapeRenderer
        {
            get => this._shapeRenderer;
        }
        #endregion

        #endregion

        #region # 方法

        //Public

        #region 看向指定位置 —— void LookAt(Vector3 targetPosition)
        /// <summary>
        /// 看向指定位置
        /// </summary>
        /// <param name="targetPosition">目标位置（世界坐标）</param>
        public void LookAt(Vector3 targetPosition)
        {
            this.Camera?.LookAt(targetPosition);
        }
        #endregion

        #region 查找最近元素 —— bool FindNearest(Vector2 position, out Vector3 point...
        /// <summary>
        /// 查找最近元素
        /// </summary>
        /// <param name="position">2D位置</param>
        /// <param name="point">3D位置</param>
        /// <param name="normal">法向量</param>
        /// <param name="visual3D">3D元素</param>
        /// <param name="ray">射线</param>
        /// <returns>是否成功</returns>
        public bool FindNearest(Vector2 position, out Vector3 point, out Vector3 normal, out Visual3D visual3D, out Ray ray)
        {
            this.GlContext.MakeCurrent();

            ray = this.UnProject(position);

            //快速检测
            IDictionary<Visual3D, float> hitResults = new Dictionary<Visual3D, float>();
            foreach (ShapeVisual3D shapeVisual3D in this._shapeVisual3Ds.Where(x => x.IsVisible))
            {
                bool intersects = shapeVisual3D.Renderable.IntersectsRay(ray, out float distance);
                if (intersects)
                {
                    hitResults.Add(shapeVisual3D, distance);
                }
            }

            //精确检测
            if (hitResults.Any())
            {
                KeyValuePair<Visual3D, float> hitResult = hitResults.MinBy(x => x.Value);
                bool intersects;
                Vector3 hitPoint;
                Vector3 hitNormal;
                if (hitResult.Key is ShapeVisual3D shapeVisual3D)
                {
                    intersects = shapeVisual3D.Renderable.IntersectsRay(ray, out _, out hitPoint, out hitNormal, out _);
                }
                else if (hitResult.Key is TextVisual3D textVisual3D)
                {
                    intersects = textVisual3D.Renderable.IntersectsRay(ray, out _, out hitPoint, out hitNormal, out _);
                }
                else
                {
                    throw new NotSupportedException();
                }

                if (intersects)
                {
                    point = hitPoint;
                    normal = hitNormal;
                    visual3D = hitResult.Key;
                    return true;
                }
            }

            point = Vector3.Zero;
            normal = Vector3.Zero;
            visual3D = null;

            return false;
        }
        #endregion

        #region 查找最近位置 —— virtual Vector3? FindNearestPosition(Vector2 position)
        /// <summary>
        /// 查找最近位置
        /// </summary>
        /// <param name="position">2D位置</param>
        /// <returns>3D位置</returns>
        public virtual Vector3? FindNearestPosition(Vector2 position)
        {
            if (this.FindNearest(position, out Vector3 point, out _, out _, out _))
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

            this._shapeRenderer = new ShapeRenderer(this.Camera);
        }
        #endregion

        #region OpenTK渲染事件 —— override void OnOpenTKRender(PixelSize viewportSize)
        /// <summary>
        /// OpenTK渲染事件
        /// </summary>
        /// <param name="viewportSize">视口尺寸</param>
        protected override void OnOpenTKRender(PixelSize viewportSize)
        {
            //开启深度
            GL.DepthMask(true);

            //禁用面剔除
            GL.Disable(EnableCap.CullFace);

            //清空渲染对象
            this._shapeVisual3Ds.Clear();
            this._shapeRenderer.ClearItems();

            //填充渲染对象
            List<ShapeVisual3D> shapeVisual3Ds = this.GetShapeVisual3Ds();
            foreach (ShapeVisual3D shapeVisual3D in shapeVisual3Ds)
            {
                this._shapeVisual3Ds.Add(shapeVisual3D);
                this._shapeRenderer.AppendItem(shapeVisual3D.Renderable);
            }

            this._shapeRenderer.RenderFrame(viewportSize.Width, viewportSize.Height);
        }
        #endregion

        #region OpenTK卸载事件 —— override void OnOpenTKDeinit()
        /// <summary>
        /// OpenTK卸载事件
        /// </summary>
        protected override void OnOpenTKDeinit()
        {
            this._shapeVisual3Ds.Clear();
            this._shapeRenderer.Dispose();
        }
        #endregion


        //Protected

        #region 获取形状3D元素列表 —— virtual List<ShapeVisual3D> GetShapeVisual3Ds()
        /// <summary>
        /// 获取形状3D元素列表
        /// </summary>
        /// <returns>形状3D元素列表</returns>
        protected virtual List<ShapeVisual3D> GetShapeVisual3Ds()
        {
            List<ShapeVisual3D> shapeVisual3Ds = [];
            foreach (Visual3D visual3D in this.Children.Where(x => x.IsVisible))
            {
                if (visual3D is ShapeVisual3D shapeVisual3D)
                {
                    shapeVisual3D.EnsureRenderable();
                    shapeVisual3Ds.Add(shapeVisual3D);
                }
                if (visual3D is ShapePresenter shapePresenter && shapePresenter.Content.IsVisible)
                {
                    shapePresenter.Content.EnsureRenderable();
                    shapeVisual3Ds.Add(shapePresenter.Content);
                }
                if (visual3D is ShapesPresenter shapesPresenter)
                {
                    foreach (ShapeVisual3D item in shapesPresenter.ItemsSource.Where(x => x.IsVisible))
                    {
                        item.EnsureRenderable();
                        shapeVisual3Ds.Add(item);
                    }
                }
            }

            return shapeVisual3Ds;
        }
        #endregion


        //Events

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
                if (visual3D is ShapesPresenter shapesPresenter)
                {
                    shapesPresenter.ItemsSource.CollectionChanged += this.OnItemsPresenterItemsChanged;
                }
            }
        }
        #endregion

        #region 3D元素列表容器元素改变事件 —— void OnItemsPresenterItemsChanged(object sender...
        /// <summary>
        /// 3D元素列表容器元素改变事件
        /// </summary>
        private void OnItemsPresenterItemsChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
        {
            this.RequestNextFrameRendering();
        }
        #endregion

        #endregion
    }
}
