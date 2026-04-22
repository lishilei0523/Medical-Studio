using Avalonia;
using Avalonia.Input;
using MedicalSharp.Controls.Base;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Controls.Interfaces;
using MedicalSharp.Controls.Visuals;
using MedicalSharp.Primitives.Interfaces;
using MedicalSharp.Primitives.Maths;
using MedicalSharp.Primitives.Models;
using OpenTK.Mathematics;

namespace MedicalSharp.Controls.Commands
{
    /// <summary>
    /// 编辑顶点命令
    /// </summary>
    public class EditVertexCommand : ViewportCommand
    {
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

        /// <summary>
        /// 鼠标按下事件
        /// </summary>
        public override void OnMouseDown(OpenTKViewport viewport, PointerPressedEventArgs eventArgs)
        {
            base.OnMouseDown(viewport, eventArgs);
            if (eventArgs.Properties.IsLeftButtonPressed && viewport is IPickVisual3D pickVisual3D)
            {
                Point mousePos2D = eventArgs.GetPosition(viewport);
                bool success = pickVisual3D.FindNearest(mousePos2D.ToVector2(), out _, out _, out Visual3D visual3D, out Ray ray);
                if (success && visual3D is IVertexEditable vertexEditable)
                {
                    Matrix4 modelMatrix = visual3D.Transform.Matrix;
                    Matrix4 worldToLocal = Matrix4.Invert(modelMatrix);
                    Ray localRay = ray.Transform(worldToLocal);
                    Vector3 localLookDirection = Vector3.TransformNormal(viewport.Camera.LookDirection, worldToLocal).Normalized();
                    if (vertexEditable.TryGetVertexDrag(localRay, localLookDirection, out VertexDragConstraint constraint))
                    {
                        this._selectedVisual = vertexEditable;
                        this._selectedVertexConstraint = constraint;
                    }
                }
            }
        }

        /// <summary>
        /// 鼠标移动事件
        /// </summary>
        public override void OnMouseMove(OpenTKViewport viewport, PointerEventArgs eventArgs)
        {
            base.OnMouseMove(viewport, eventArgs);
            if (eventArgs.Properties.IsLeftButtonPressed && this._selectedVisual != null)
            {
                //计算模型位置
                Matrix4 modelMatrix = this._selectedVisual.Transform.Matrix;
                Vector3 localCenter = this._selectedVisual.Bounds.Center;
                Vector3 worldCenter = Vector3.TransformPosition(localCenter, modelMatrix);

                //获取鼠标射线
                Vector2 mousePos2D = eventArgs.GetPosition(viewport).ToVector2();
                Ray ray = viewport.UnProject(mousePos2D);

                //移动平面上的交点
                bool success = ray.IntersectsPlane(worldCenter, viewport.Camera.LookDirection, out _, out _);
                if (success)
                {
                    //设置光标
                    viewport.Cursor = new Cursor(StandardCursorType.Cross);

                    //构造局部射线
                    Matrix4 worldToLocal = Matrix4.Invert(modelMatrix);
                    Ray localRay = ray.Transform(worldToLocal);

                    //可顶点编辑类型
                    if (this._selectedVisual is IVertexEditable vertexEditable && this._selectedVertexConstraint.HasValue)
                    {
                        VertexDragConstraint constraint = this._selectedVertexConstraint.Value;
                        if (localRay.IntersectsPlane(constraint.Anchor, constraint.Normal, out Vector3 localHitPoint, out _))
                        {
                            vertexEditable.MoveVertex(constraint, localHitPoint);
                            viewport.RequestNextFrameRendering();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 鼠标松开事件
        /// </summary>
        public override void OnMouseUp(OpenTKViewport viewport, PointerReleasedEventArgs eventArgs)
        {
            base.OnMouseUp(viewport, eventArgs);

            //设置光标
            viewport.Cursor = new Cursor(StandardCursorType.Arrow);

            //清空选中
            this._selectedVisual = null;
            this._selectedVertexConstraint = null;

            //请求下一帧
            viewport.RequestNextFrameRendering();
        }
    }
}
