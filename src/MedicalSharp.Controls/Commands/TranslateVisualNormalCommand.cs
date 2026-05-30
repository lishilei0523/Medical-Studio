using Avalonia.Input;
using MedicalSharp.Controls.Base;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Controls.Interfaces;
using MedicalSharp.Controls.Visual3Ds;
using MedicalSharp.Primitives.Interfaces;
using MedicalSharp.Primitives.Maths;
using OpenTK.Mathematics;
using System;

namespace MedicalSharp.Controls.Commands
{
    /// <summary>
    /// 沿法向量平移元素命令
    /// </summary>
    public class TranslateVisualNormalCommand : ViewportCommand
    {
        #region # 字段及构造器

        /// <summary>
        /// 拖拽起始点（世界坐标）
        /// </summary>
        private Vector3? _dragStartPoint;

        /// <summary>
        /// 物体起始位置
        /// </summary>
        private Vector3? _dragStartPosition;

        /// <summary>
        /// 选中的3D元素
        /// </summary>
        private ITranslatableNormal _selectedVisual;

        /// <summary>
        /// 平移结束事件
        /// </summary>
        private readonly Action<ITranslatableNormal> _translateEndEvent;

        /// <summary>
        /// 创建沿法向量平移元素命令构造器
        /// </summary>
        /// <param name="translateEnd">平移结束回调</param>
        public TranslateVisualNormalCommand(Action<ITranslatableNormal> translateEnd)
        {
            this._translateEndEvent = translateEnd;
            this._selectedVisual = null;
        }

        #endregion

        #region # 属性

        #region 平移中事件 —— Action<ITranslatableNormal> TranslatingEvent
        /// <summary>
        /// 平移中事件
        /// </summary>
        public Action<ITranslatableNormal> TranslatingEvent { get; set; }
        #endregion

        #endregion

        #region # 方法

        #region 鼠标按下事件 —— override void OnMouseDown(OpenTKViewport viewport...
        /// <summary>
        /// 鼠标按下事件
        /// </summary>
        public override void OnMouseDown(OpenTKViewport viewport, PointerPressedEventArgs eventArgs)
        {
            base.OnMouseDown(viewport, eventArgs);
            if (eventArgs.Properties.IsLeftButtonPressed && viewport is IPickVisual3D pickVisual3D)
            {
                Vector2 mousePos2D = eventArgs.GetPosition(viewport).ToVector2();
                bool success = pickVisual3D.FindNearest(mousePos2D, out Vector3 hitPoint, out _, out Visual3D visual3D, out _);
                if (success && visual3D is ITranslatableNormal translatable)
                {
                    #region # 验证

                    if (visual3D is IFixable { Fixed: true })
                    {
                        return;
                    }

                    #endregion

                    this._selectedVisual = translatable;
                    this._dragStartPoint = hitPoint;                            //物体上被点击的点
                    this._dragStartPosition = translatable.Transform.Position;  //物体起始位置
                }
            }
        }
        #endregion

        #region 鼠标移动事件 —— override void OnMouseMove(OpenTKViewport viewport...
        /// <summary>
        /// 鼠标移动事件
        /// </summary>
        public override void OnMouseMove(OpenTKViewport viewport, PointerEventArgs eventArgs)
        {
            base.OnMouseMove(viewport, eventArgs);
            if (eventArgs.Properties.IsLeftButtonPressed && this._selectedVisual is IVisual2DIn3D visual2DIn3D)
            {
                if (!this._dragStartPoint.HasValue || !this._dragStartPosition.HasValue)
                {
                    return;
                }

                //获取鼠标射线
                Vector2 mousePos2D = eventArgs.GetPosition(viewport).ToVector2();
                Ray ray = viewport.UnProject(mousePos2D);

                //移动平面上的交点
                bool success = ray.IntersectsPlane(this._dragStartPoint.Value, viewport.Camera.LookDirection, out Vector3 hitPoint, out _);
                if (success)
                {
                    //设置光标
                    viewport.Cursor = new Cursor(StandardCursorType.Hand);

                    //计算移动向量
                    Vector3 deltaWorld = hitPoint - this._dragStartPoint.Value;

                    //投影到法向量方向
                    Vector3 localNormal = visual2DIn3D.Normal.ToVector3().Normalized();
                    Vector3 worldNormal = Vector3.TransformNormal(localNormal, ((Visual3D)visual2DIn3D).Transform.Matrix);
                    float dot = Vector3.Dot(deltaWorld, worldNormal);
                    Vector3 translation = worldNormal * dot;

                    //应用平移（从起始位置计算，避免累积误差）
                    Vector3 newPosition = this._dragStartPosition.Value + translation;
                    this._selectedVisual.Transform.SetPosition(newPosition);

                    //平移中
                    this.TranslatingEvent?.Invoke(this._selectedVisual);

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

            //平移结束
            this._translateEndEvent?.Invoke(this._selectedVisual);

            //清空选中
            this._selectedVisual = null;
            this._dragStartPoint = null;
            this._dragStartPosition = null;

            //请求下一帧
            viewport.RequestNextFrameRendering();
        }
        #endregion 

        #endregion
    }
}
