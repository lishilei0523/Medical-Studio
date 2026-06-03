using Avalonia.Input;
using MedicalSharp.Controls.Base;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Controls.Viewports;
using MedicalSharp.Controls.Visual3Ds;
using OpenTK.Mathematics;
using System;

namespace MedicalSharp.Controls.Commands
{
    /// <summary>
    /// 绘制圆柱体3D元素命令
    /// </summary>
    public class DrawCylinderCommand : DrawShapeCommand
    {
        #region # 字段及构造器

        /// <summary>
        /// 起始位置
        /// </summary>
        private Vector3? _startPosition;

        /// <summary>
        /// 圆柱体3D元素
        /// </summary>
        private CylinderVisual3D _cylinder;

        /// <summary>
        /// 圆柱体绘制开始事件
        /// </summary>
        private readonly Action<CylinderVisual3D> _cylinderDrawStartEvent;

        /// <summary>
        /// 圆柱体绘制结束事件
        /// </summary>
        private readonly Action<CylinderVisual3D> _cylinderDrawEndEvent;

        /// <summary>
        /// 创建绘制圆柱体3D元素命令构造器
        /// </summary>
        /// <param name="drawStart">绘制开始回调</param>
        /// <param name="drawEnd">绘制结束回调</param>
        public DrawCylinderCommand(Action<CylinderVisual3D> drawStart, Action<CylinderVisual3D> drawEnd)
        {
            this._cylinderDrawStartEvent = drawStart;
            this._cylinderDrawEndEvent = drawEnd;
        }

        #endregion

        #region # 属性

        //

        #endregion

        #region # 方法

        #region 鼠标按下事件 —— override void OnMouseDown(OpenTKViewport viewport...
        /// <summary>
        /// 鼠标按下事件
        /// </summary>
        public override void OnMouseDown(OpenTKViewport viewport, PointerPressedEventArgs eventArgs)
        {
            base.OnMouseDown(viewport, eventArgs);
            if (eventArgs.Properties.IsLeftButtonPressed && viewport is BasicViewport basicViewport)
            {
                Vector2 mousePos2D = eventArgs.GetPosition(viewport).ToVector2();
                Vector3? mousePos3D = basicViewport.FindNearestPosition(mousePos2D);
                if (mousePos3D.HasValue)
                {
                    this._startPosition = mousePos3D.Value;
                    this._cylinder = new CylinderVisual3D
                    {
                        Stroke = new Vector4(0.1f, 0.3f, 0.6f, 1.0f).ToColor(),
                        Fill = new Vector4(0.6f, 0.8f, 1.0f, 0.4f).ToColor(),
                        Center = mousePos3D.Value.ToVector3(),
                        Radius = 0.01f,
                        Height = 0,
                        Segments = 32,
                        WithCaps = true
                    };
                    this._isDrawing = true;
                    this._cylinderDrawStartEvent?.Invoke(this._cylinder);
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
            if (eventArgs.Properties.IsLeftButtonPressed &&
                viewport is BasicViewport basicViewport &&
                this._startPosition.HasValue &&
                this._cylinder != null)
            {
                //设置光标
                viewport.Cursor = new Cursor(StandardCursorType.Cross);

                Vector2 mousePos2D = eventArgs.GetPosition(viewport).ToVector2();
                Vector3? mousePos3D = basicViewport.FindNearestPosition(mousePos2D);
                if (mousePos3D.HasValue)
                {
                    //计算偏移量
                    float offsetX = Math.Abs(mousePos3D.Value.X - this._startPosition.Value.X);
                    float offsetY = Math.Abs(mousePos3D.Value.Y - this._startPosition.Value.Y);
                    float offsetZ = Math.Abs(mousePos3D.Value.Z - this._startPosition.Value.Z);

                    //半径取水平偏移的最大值
                    float radius = Math.Max(offsetX, offsetY);
                    radius = Math.Max(radius, 0.01f);

                    //高度取垂直偏移
                    float height = offsetZ * 2.0f;

                    this._cylinder.Radius = Math.Max(radius, 0.05f);
                    this._cylinder.Height = Math.Max(height, 0.1f);

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

            //绘制结束
            this._isDrawing = false;
            this._cylinderDrawEndEvent?.Invoke(this._cylinder);

            //清空
            this._startPosition = null;
            this._cylinder = null;

            //请求下一帧
            viewport.RequestNextFrameRendering();
        }
        #endregion

        #endregion
    }
}
