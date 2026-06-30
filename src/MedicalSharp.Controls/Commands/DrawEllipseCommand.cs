using Avalonia;
using Avalonia.Input;
using MedicalSharp.Controls.Base;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Controls.Viewports;
using MedicalSharp.Controls.Visual3Ds;
using MedicalSharp.Primitives.Builders;
using OpenTK.Mathematics;
using System;
using System.Linq;

namespace MedicalSharp.Controls.Commands
{
    /// <summary>
    /// 绘制椭圆形3D元素命令
    /// </summary>
    public class DrawEllipseCommand : DrawShapeCommand
    {
        #region # 字段及构造器

        /// <summary>
        /// 起始位置
        /// </summary>
        private Vector3? _startPosition;

        /// <summary>
        /// 椭圆形3D元素
        /// </summary>
        private EllipseVisual3D _ellipse;

        /// <summary>
        /// 默认构造器
        /// </summary>
        public DrawEllipseCommand()
        {

        }

        #endregion

        #region # 属性

        #region 获取法向量委托 —— Func<Vector3D> GetNormal
        /// <summary>
        /// 获取法向量委托
        /// </summary>
        public Func<Vector3D> GetNormal { get; set; }
        #endregion

        #region 绘制开始委托 —— Action<EllipseVisual3D> DrawStart
        /// <summary>
        /// 绘制开始委托
        /// </summary>
        public Action<EllipseVisual3D> DrawStart { get; set; }
        #endregion

        #region 绘制结束委托 —— Action<EllipseVisual3D> DrawEnd
        /// <summary>
        /// 绘制结束委托
        /// </summary>
        public Action<EllipseVisual3D> DrawEnd { get; set; }
        #endregion

        #endregion

        #region # 方法

        #region 鼠标按下事件 —— override void OnMouseDown(OpenTKViewport viewport...
        /// <summary>
        /// 鼠标按下事件
        /// </summary>
        public override void OnMouseDown(OpenTKViewport viewport, PointerPressedEventArgs eventArgs)
        {
            if (eventArgs.Properties.IsLeftButtonPressed && viewport is BasicViewport basicViewport)
            {
                Vector2 mousePos2D = eventArgs.GetPixelPosition(viewport).ToVector2();
                Vector3? mousePos3D = basicViewport.FindNearestPosition(mousePos2D);
                if (mousePos3D.HasValue)
                {
                    this._startPosition = mousePos3D.Value;
                    this._ellipse = new EllipseVisual3D
                    {
                        Fill = ColorFactory.Fill2D.ToColor(),
                        Width = 0.01f,
                        Height = 0.005f,
                        Center = mousePos3D.Value.ToVector3(),
                        Normal = this.GetNormal?.Invoke() ?? new Vector3D(0, 1, 0)
                    };
                    this._isDrawing = true;
                    this.DrawStart?.Invoke(this._ellipse);
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
            if (eventArgs.Properties.IsLeftButtonPressed &&
                viewport is BasicViewport basicViewport &&
                this._startPosition.HasValue &&
                this._ellipse != null)
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
                    this._ellipse.Width = sides[0];
                    this._ellipse.Height = sides[1];

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
            this.DrawEnd?.Invoke(this._ellipse);

            //清空
            this._startPosition = null;
            this._ellipse = null;

            //请求下一帧
            viewport.RequestNextFrameRendering();
        }
        #endregion 

        #endregion
    }
}
