using Avalonia.Input;
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
    /// 绘制包围盒3D元素命令
    /// </summary>
    public class DrawBoundingBoxCommand : DrawShapeCommand
    {
        #region # 字段及构造器

        /// <summary>
        /// 起始位置
        /// </summary>
        private Vector3? _startPosition;

        /// <summary>
        /// 包围盒3D元素
        /// </summary>
        private BoundingBoxVisual3D _boundingBox;

        /// <summary>
        /// 包围盒绘制开始事件
        /// </summary>
        private readonly Action<BoundingBoxVisual3D> _boxDrawStartEvent;

        /// <summary>
        /// 包围盒绘制结束事件
        /// </summary>
        private readonly Action<BoundingBoxVisual3D> _boxDrawEndEvent;

        /// <summary>
        /// 创建绘制包围盒3D元素命令构造器
        /// </summary>
        /// <param name="drawStart">绘制开始回调</param>
        /// <param name="drawEnd">绘制结束回调</param>
        public DrawBoundingBoxCommand(Action<BoundingBoxVisual3D> drawStart, Action<BoundingBoxVisual3D> drawEnd)
        {
            this._boxDrawStartEvent = drawStart;
            this._boxDrawEndEvent = drawEnd;
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
                    this._boundingBox = new BoundingBoxVisual3D
                    {
                        Stroke = new Vector4(0.1f, 0.3f, 0.6f, 1.0f).ToColor(),
                        Fill = new Vector4(0.6f, 0.8f, 1.0f, 0.4f).ToColor(),
                        Width = 0.01f,
                        Height = 0.01f,
                        Depth = 0.01f,
                        Center = mousePos3D.Value.ToVector3()
                    };
                    this._boxDrawStartEvent?.Invoke(this._boundingBox);
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
                this._boundingBox != null)
            {
                //设置光标
                viewport.Cursor = new Cursor(StandardCursorType.Cross);

                Vector2 mousePos2D = eventArgs.GetPosition(viewport).ToVector2();
                Vector3? mousePos3D = basicViewport.FindNearestPosition(mousePos2D);
                if (mousePos3D.HasValue)
                {
                    float offsetX = Math.Abs(mousePos3D.Value.X - this._startPosition.Value.X);
                    float offsetY = Math.Abs(mousePos3D.Value.Y - this._startPosition.Value.Y);
                    float offsetZ = Math.Abs(mousePos3D.Value.Z - this._startPosition.Value.Z);
                    float[] sides = new[] { offsetX, offsetY, offsetZ }.OrderByDescending(x => x).ToArray();
                    this._boundingBox.Width = sides[0];
                    this._boundingBox.Height = sides[1];
                    this._boundingBox.Depth = sides[1];

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
            this._boxDrawEndEvent?.Invoke(this._boundingBox);

            //清空
            this._startPosition = null;
            this._boundingBox = null;

            //请求下一帧
            viewport.RequestNextFrameRendering();
        }
        #endregion 

        #endregion
    }
}
