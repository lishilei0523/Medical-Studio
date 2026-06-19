using Avalonia.Input;
using Avalonia.Media;
using MedicalSharp.Controls.Base;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Controls.Viewports;
using MedicalSharp.Controls.Visual3Ds;
using MedicalSharp.Primitives.Builders;
using OpenTK.Mathematics;
using System;

namespace MedicalSharp.Controls.Commands
{
    /// <summary>
    /// 绘制点3D元素命令
    /// </summary>
    public class DrawPointCommand : DrawShapeCommand
    {
        #region # 字段及构造器

        /// <summary>
        /// 默认构造器
        /// </summary>
        public DrawPointCommand()
        {

        }

        #endregion

        #region # 属性

        #region 绘制结束委托 —— Action<PointVisual3D> DrawEnd
        /// <summary>
        /// 绘制结束委托
        /// </summary>
        public Action<PointVisual3D> DrawEnd { get; set; }
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
                    PointVisual3D point = new PointVisual3D
                    {
                        Fill = ColorFactory.PointColor.ToColor(),
                        Position = mousePos3D.Value.ToVector3(),
                        PointSize = 5
                    };
                    this._isDrawing = true;
                    this.DrawEnd?.Invoke(point);

                    //请求下一帧
                    viewport.RequestNextFrameRendering();
                }
            }

            base.OnMouseDown(viewport, eventArgs);
        }
        #endregion 

        #region 鼠标松开事件 —— override void OnMouseUp(OpenTKViewport viewport...
        /// <summary>
        /// 鼠标松开事件
        /// </summary>
        public override void OnMouseUp(OpenTKViewport viewport, PointerReleasedEventArgs eventArgs)
        {
            base.OnMouseUp(viewport, eventArgs);

            //绘制结束
            this._isDrawing = false;
        }
        #endregion 

        #endregion
    }
}
