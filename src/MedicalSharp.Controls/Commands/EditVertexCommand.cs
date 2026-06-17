using Avalonia.Input;
using MedicalSharp.Controls.Base;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Controls.Interfaces;
using MedicalSharp.Controls.Viewports;
using MedicalSharp.Controls.Visual3Ds;
using MedicalSharp.Primitives.Interfaces;
using MedicalSharp.Primitives.Maths;
using MedicalSharp.Primitives.Models;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace MedicalSharp.Controls.Commands
{
    /// <summary>
    /// 编辑顶点命令
    /// </summary>
    public class EditVertexCommand : EditShapeCommand
    {
        #region # 字段及构造器

        /// <summary>
        /// 选中的3D元素
        /// </summary>
        private IVertexEditable _selectedVisual;

        /// <summary>
        /// 选中的顶点拖拽约束
        /// </summary>
        private VertexDragConstraint? _selectedVertexConstraint;

        /// <summary>
        /// 默认构造器
        /// </summary>
        public EditVertexCommand()
        {
            this._selectedVisual = null;
            this._selectedVertexConstraint = null;
        }

        #endregion

        #region # 属性

        #region 已编辑顶点委托 —— Action<IVertexEditable> VertexEdited
        /// <summary>
        /// 已编辑顶点委托
        /// </summary>
        public Action<IVertexEditable> VertexEdited { get; set; }
        #endregion

        #endregion

        #region # 方法

        //Public

        #region 鼠标按下事件 —— override void OnMouseDown(OpenTKViewport viewport...
        /// <summary>
        /// 鼠标按下事件
        /// </summary>
        public override void OnMouseDown(OpenTKViewport viewport, PointerPressedEventArgs eventArgs)
        {
            if (eventArgs.Properties.IsLeftButtonPressed && viewport is IPickVisual3D pickVisual3D)
            {
                Vector2 mousePos2D = eventArgs.GetPixelPosition(viewport).ToVector2();
                bool success = pickVisual3D.FindNearest(mousePos2D, out Vector3 visualHitPoint, out _, out Visual3D visual3D, out Ray ray);
                if (success && visual3D is IVertexEditable vertexEditable)
                {
                    #region # 验证

                    if (visual3D is IFixable { Fixed: true })
                    {
                        return;
                    }

                    #endregion

                    Matrix4 worldToLocal = visual3D.Transform.Matrix.Inverted();
                    Ray localRay = ray.Transform(worldToLocal);
                    Vector3 localLookDirection = Vector3.TransformNormal(viewport.Camera.LookDirection, worldToLocal).Normalized();

                    //找最近顶点
                    if (vertexEditable.TryGetVertexDrag(localRay, localLookDirection, out VertexDragConstraint dragConstraint))
                    {
                        this._selectedVisual = vertexEditable;
                        this._selectedVertexConstraint = dragConstraint;
                    }
                    //插入新顶点
                    else
                    {
                        Vector3 localHitPoint = Vector3.TransformPosition(visualHitPoint, worldToLocal);
                        if (viewport is IPickVoxel pickVoxel)
                        {
                            bool pickedVoxel = pickVoxel.FindNearestVoxel(mousePos2D, out _, out Vector3 voxelHitPoint, out _, out _, out _, out _);
                            if (pickedVoxel)
                            {
                                localHitPoint = Vector3.TransformPosition(voxelHitPoint, worldToLocal);
                            }
                        }
                        if (vertexEditable.TryInsertVertex(localRay, localLookDirection, localHitPoint, out VertexDragConstraint insertConstraint))
                        {
                            this._selectedVisual = vertexEditable;
                            this._selectedVertexConstraint = insertConstraint;
                        }
                    }
                }
            }

            base.OnMouseDown(viewport, eventArgs);
        }
        #endregion

        #region 鼠标移动事件 —— override void OnMouseMove(OpenTKViewport viewport...
        /// <summary>
        /// 鼠标移动事件
        /// </summary>
        public override void OnMouseMove(OpenTKViewport viewport, PointerEventArgs eventArgs)
        {
            base.OnMouseMove(viewport, eventArgs);
            if (eventArgs.Properties.IsLeftButtonPressed && viewport is BasicViewport basicViewport &&
                this._selectedVisual != null && this._selectedVertexConstraint.HasValue)
            {
                Vector2 mousePos2D = eventArgs.GetPixelPosition(viewport).ToVector2();
                Vector3? hitPoint = basicViewport.FindNearestPosition(mousePos2D);
                if (hitPoint.HasValue)
                {
                    Matrix4 worldToLocal = this._selectedVisual.Transform.Matrix.Inverted();
                    Vector3 localHitPoint = Vector3.TransformPosition(hitPoint.Value, worldToLocal);
                    VertexDragConstraint constraint = this._selectedVertexConstraint.Value;
                    this._selectedVisual.MoveVertex(constraint, localHitPoint);

                    //请求下一帧
                    viewport.RequestNextFrameRendering();
                }
            }
        }
        #endregion

        #region 鼠标松开事件 —— override void OnMouseUp(OpenTKViewport viewport...
        /// <summary>
        /// 鼠标松开事件
        /// </summary>
        public override void OnMouseUp(OpenTKViewport viewport, PointerReleasedEventArgs eventArgs)
        {
            base.OnMouseUp(viewport, eventArgs);

            //设置光标
            viewport.Cursor = new Cursor(StandardCursorType.Arrow);

            //编辑顶点结束
            this.VertexEdited?.Invoke(this._selectedVisual);

            //清空选中
            this._selectedVisual = null;
            this._selectedVertexConstraint = null;

            //请求下一帧
            viewport.RequestNextFrameRendering();
        }
        #endregion 

        #region 获取上下文菜单项列表 —— override IReadOnlyList<ContextMenuItem> GetContextMenuItems(...
        /// <summary>
        /// 获取上下文菜单项列表
        /// </summary>
        /// <returns>上下文菜单项列表</returns>
        public override IReadOnlyList<ContextMenuItem> GetContextMenuItems(OpenTKViewport viewport, PointerReleasedEventArgs eventArgs)
        {
            if (viewport is IPickVisual3D pickVisual3D)
            {
                Vector2 mousePos2D = eventArgs.GetPixelPosition(viewport).ToVector2();
                bool success = pickVisual3D.FindNearest(mousePos2D, out _, out _, out Visual3D visual3D, out Ray ray);
                if (success && visual3D is IVertexEditable vertexEditable)
                {
                    Matrix4 modelMatrix = visual3D.Transform.Matrix;
                    Matrix4 worldToLocal = Matrix4.Invert(modelMatrix);
                    Ray localRay = ray.Transform(worldToLocal);
                    Vector3 localLookDirection = Vector3.TransformNormal(viewport.Camera.LookDirection, worldToLocal).Normalized();

                    //找最近顶点
                    if (vertexEditable.TryGetVertexDrag(localRay, localLookDirection, out VertexDragConstraint constraint))
                    {
                        List<ContextMenuItem> items =
                        [
                            new ContextMenuItem
                            {
                                Header = "删除顶点(_D)",
                                Command = () => this.RemoveVertex(viewport,vertexEditable,constraint.VertexIndex )
                            }
                        ];

                        return items;
                    }
                }
            }

            return base.GetContextMenuItems(viewport, eventArgs);
        }
        #endregion


        //Private

        #region 删除顶点 —— void RemoveVertex(OpenTKViewport viewport...
        /// <summary>
        /// 删除顶点
        /// </summary>
        private void RemoveVertex(OpenTKViewport viewport, IVertexEditable vertexEditable, int vertexIndex)
        {
            if (vertexEditable.TryRemoveVertex(vertexIndex))
            {
                //请求下一帧
                viewport.RequestNextFrameRendering();
            }
        }
        #endregion

        #endregion
    }
}
