using Avalonia.Input;
using Avalonia.Media;
using MedicalSharp.Controls.Base;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Controls.Viewports;
using MedicalSharp.Controls.Visual3Ds;
using OpenTK.Mathematics;
using System;

namespace MedicalSharp.Controls.Commands
{
    /// <summary>
    /// 绘制点3D元素命令
    /// </summary>
    public class DrawPointCommand : ViewportCommand
    {
        #region # 字段及构造器

        /// <summary>
        /// 点绘制结束事件
        /// </summary>
        private readonly Action<PointVisual3D> _pointDrawEndEvent;

        /// <summary>
        /// 创建绘制点3D元素命令构造器
        /// </summary>
        /// <param name="drawEnd">绘制结束回调</param>
        public DrawPointCommand(Action<PointVisual3D> drawEnd)
        {
            this._pointDrawEndEvent = drawEnd;
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
                    PointVisual3D point = new PointVisual3D
                    {
                        Fill = Colors.Cyan,
                        Position = mousePos3D.Value.ToVector3(),
                        PointSize = 5
                    };
                    this._pointDrawEndEvent?.Invoke(point);

                    //请求下一帧
                    viewport.RequestNextFrameRendering();
                }
            }
        }
        #endregion 

        #endregion
    }
}
