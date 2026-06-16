using Avalonia;
using Avalonia.Input;
using Avalonia.Media;
using MedicalSharp.Controls.Base;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Controls.Viewports;
using MedicalSharp.Controls.Visual3Ds;
using OpenTK.Mathematics;
using System;
using System.Linq;

namespace MedicalSharp.Controls.Commands
{
    /// <summary>
    /// 绘制矩形3D元素命令
    /// </summary>
    public class DrawRectangleCommand : DrawShapeCommand
    {
        #region # 字段及构造器

        /// <summary>
        /// 起始位置
        /// </summary>
        private Vector3? _startPosition;

        /// <summary>
        /// 矩形3D元素
        /// </summary>
        private RectangleVisual3D _rectangle;

        /// <summary>
        /// 获取法向量
        /// </summary>
        private readonly Func<Vector3D> _getNormal;

        /// <summary>
        /// 矩形绘制开始事件
        /// </summary>
        private readonly Action<RectangleVisual3D> _rectangleDrawStartEvent;

        /// <summary>
        /// 矩形绘制结束事件
        /// </summary>
        private readonly Action<RectangleVisual3D> _rectangleDrawEndEvent;

        /// <summary>
        /// 创建绘制矩形3D元素命令构造器
        /// </summary>
        /// <param name="drawStart">绘制开始回调</param>
        /// <param name="drawEnd">绘制结束回调</param>
        /// <param name="getNormal">获取法向量</param>
        public DrawRectangleCommand(Action<RectangleVisual3D> drawStart, Action<RectangleVisual3D> drawEnd, Func<Vector3D> getNormal)
        {
            this._rectangleDrawStartEvent = drawStart;
            this._rectangleDrawEndEvent = drawEnd;
            this._getNormal = getNormal;
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
                Vector2 mousePos2D = eventArgs.GetPixelPosition(viewport).ToVector2();
                Vector3? mousePos3D = basicViewport.FindNearestPosition(mousePos2D);
                if (mousePos3D.HasValue)
                {
                    this._startPosition = mousePos3D.Value;
                    this._rectangle = new RectangleVisual3D
                    {
                        Fill = Color.Parse("#0F00FF00"),
                        Width = 0.01f,
                        Height = 0.005f,
                        Center = mousePos3D.Value.ToVector3(),
                        Normal = this._getNormal?.Invoke() ?? new Vector3D(0, 1, 0)
                    };
                    this._isDrawing = true;
                    this._rectangleDrawStartEvent?.Invoke(this._rectangle);
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
                this._rectangle != null)
            {
                //设置光标
                viewport.Cursor = new Cursor(StandardCursorType.Cross);

                Vector2 mousePos2D = eventArgs.GetPixelPosition(viewport).ToVector2();
                Vector3? mousePos3D = basicViewport.FindNearestPosition(mousePos2D);
                if (mousePos3D.HasValue)
                {
                    float offsetX = Math.Abs(mousePos3D.Value.X - this._startPosition.Value.X);
                    float offsetY = Math.Abs(mousePos3D.Value.Y - this._startPosition.Value.Y);
                    float offsetZ = Math.Abs(mousePos3D.Value.Z - this._startPosition.Value.Z);
                    float[] sides = new[] { offsetX, offsetY, offsetZ }.OrderByDescending(x => x).ToArray();
                    this._rectangle.Width = sides[0];
                    this._rectangle.Height = sides[1];

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
            this._rectangleDrawEndEvent?.Invoke(this._rectangle);

            //清空
            this._startPosition = null;
            this._rectangle = null;

            //请求下一帧
            viewport.RequestNextFrameRendering();
        }
        #endregion 

        #endregion
    }
}
