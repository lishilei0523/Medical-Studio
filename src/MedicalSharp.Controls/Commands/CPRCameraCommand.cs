using Avalonia;
using Avalonia.Input;
using MedicalSharp.Controls.Base;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Controls.Viewports;
using MedicalSharp.Primitives.Cameras;
using MedicalSharp.Primitives.Enums;
using OpenTK.Mathematics;
using System;

namespace MedicalSharp.Controls.Commands
{
    /// <summary>
    /// CPR相机命令
    /// </summary>
    public class CPRCameraCommand : CameraCommand
    {
        #region # 字段及构造器

        /// <summary>
        /// CPR相机
        /// </summary>
        private readonly CPRCamera _camera;

        /// <summary>
        /// 创建CPR相机命令构造器
        /// </summary>
        public CPRCameraCommand(CPRCamera camera)
        {
            this._camera = camera;
        }

        #endregion

        #region # 属性

        #region 只读属性 - CPR相机 —— CPRCamera Camera
        /// <summary>
        /// 只读属性 - CPR相机
        /// </summary>
        public CPRCamera Camera
        {
            get => this._camera;
        }
        #endregion

        #endregion

        #region # 方法

        #region 鼠标移动事件 —— override void OnMouseMove(OpenTKViewport viewport...
        /// <summary>
        /// 鼠标移动事件
        /// </summary>
        public override void OnMouseMove(OpenTKViewport viewport, PointerEventArgs eventArgs)
        {
            Point position = eventArgs.GetPixelPosition(viewport);
            CPRViewport cprViewport = (CPRViewport)viewport;
            if (this._mousePos2D.HasValue)
            {
                float deltaX = (float)(position.X - this._mousePos2D.Value.X);
                float deltaY = (float)(position.Y - this._mousePos2D.Value.Y);
                if (eventArgs.Properties.IsMiddleButtonPressed)
                {
                    this._camera.Pan(deltaX, deltaY);

                    //请求下一帧
                    viewport.RequestNextFrameRendering();
                }
                if (eventArgs.Properties.IsRightButtonPressed)
                {
                    if (cprViewport.CPRMode == CPRMode.Straightened)
                    {
                        if (cprViewport.StraightenDirection == CPRStraightenDirection.Horizontal)
                        {
                            cprViewport.RotationAngle += -deltaY;
                        }
                        if (cprViewport.StraightenDirection == CPRStraightenDirection.Vertical)
                        {
                            cprViewport.RotationAngle += deltaX;
                        }
                    }
                    if (cprViewport.CPRMode == CPRMode.Projected)
                    {
                        //绕Z轴旋转投影轴
                        float angle = -deltaX * 0.01f;
                        float cos = MathF.Cos(angle);
                        float sin = MathF.Sin(angle);
                        Vector3 axis = cprViewport.CPRRenderer.ProjectionAxis;
                        Vector3 rotatedAxis = new Vector3(
                            axis.X * cos - axis.Y * sin,
                            axis.X * sin + axis.Y * cos,
                            axis.Z
                        ).Normalized();

                        cprViewport.CPRRenderer.SwitchProjectionAxis(rotatedAxis);
                    }
                    if (cprViewport.CPRMode == CPRMode.CrossSectional)
                    {
                        this._camera.Zoom(-deltaY);
                    }

                    //请求下一帧
                    viewport.RequestNextFrameRendering();
                }
            }
            this._mousePos2D = position.ToVector2();
        }
        #endregion

        #region 鼠标滚轮事件 —— override void OnMouseWheel(OpenTKViewport viewport...
        /// <summary>
        /// 鼠标滚轮事件
        /// </summary>
        public override void OnMouseWheel(OpenTKViewport viewport, PointerWheelEventArgs eventArgs)
        {
            float delta = (float)eventArgs.Delta.Y;
            CPRViewport cprViewport = (CPRViewport)viewport;
            if (cprViewport.CPRMode == CPRMode.CrossSectional)
            {
                //剖面图：滚轮切换弧长位置
                float arcPosition = cprViewport.ArcPosition + delta * 0.01f;
                arcPosition = Math.Clamp(arcPosition, 0f, 1f);
                cprViewport.ArcPosition = arcPosition;
            }
            else
            {
                //拉直图/投影图：滚轮缩放
                this._camera.Zoom(delta);

                //请求下一帧
                viewport.RequestNextFrameRendering();
            }
        }
        #endregion

        #endregion
    }
}
