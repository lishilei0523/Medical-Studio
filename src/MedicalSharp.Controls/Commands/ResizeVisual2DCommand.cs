using Avalonia.Input;
using MedicalSharp.Controls.Base;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Controls.Viewports;
using MedicalSharp.Controls.Visuals;
using MedicalSharp.Primitives.Interfaces;
using MedicalSharp.Primitives.Maths;
using OpenTK.Mathematics;

namespace MedicalSharp.Controls.Commands
{
    /// <summary>
    /// 调整2D元素尺寸命令
    /// </summary>
    /// <remarks>MPR视图使用</remarks>
    public class ResizeVisual2DCommand : ViewportCommand
    {
        #region # 字段及构造器

        /// <summary>
        /// MPR平面
        /// </summary>
        private MPRPlane _mprPlane;

        /// <summary>
        /// 选中的3D元素
        /// </summary>
        private IResizable2D _selectedVisual;

        /// <summary>
        /// 创建调整2D图形尺寸命令构造器
        /// </summary>
        public ResizeVisual2DCommand()
        {
            this._mprPlane = null;
            this._selectedVisual = null;
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
            if (eventArgs.Properties.IsLeftButtonPressed && viewport is MPRViewport mprViewport)
            {
                //获取MPR平面
                this._mprPlane = mprViewport.Plane;

                //获取鼠标在平面上的UV坐标
                Vector2 mousePos2D = eventArgs.GetPosition(viewport).ToVector2();
                Vector2? planeUV = this._mprPlane.ScreenToPlaneUV(mousePos2D, mprViewport.Camera.LookDirection, mprViewport.ViewportSize.ToVector2(), mprViewport.Camera.ProjectionMatrix, mprViewport.Camera.ViewMatrix, out _);
                if (planeUV.HasValue && mprViewport.FindNearest(mousePos2D, out _, out _, out Visual3D visual3D, out _))
                {
                    if (visual3D is IResizable2D resizable2D)
                    {
                        this._selectedVisual = resizable2D;
                        this._selectedVisual.BeginResize(planeUV.Value);
                    }
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
            if (eventArgs.Properties.IsLeftButtonPressed && this._selectedVisual != null && viewport is MPRViewport mprViewport)
            {
                //设置光标
                viewport.Cursor = new Cursor(StandardCursorType.Cross);

                //获取鼠标在平面上的UV坐标
                Vector2 mousePos2D = eventArgs.GetPosition(viewport).ToVector2();
                Vector2? planeUV = this._mprPlane.ScreenToPlaneUV(mousePos2D, mprViewport.Camera.LookDirection, mprViewport.ViewportSize.ToVector2(), mprViewport.Camera.ProjectionMatrix, mprViewport.Camera.ViewMatrix, out _);
                if (planeUV.HasValue)
                {
                    this._selectedVisual.ApplyResize(planeUV.Value);

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

            //清空选中
            this._mprPlane = null;
            this._selectedVisual = null;

            //请求下一帧
            viewport.RequestNextFrameRendering();
        }
        #endregion

        #endregion
    }
}
