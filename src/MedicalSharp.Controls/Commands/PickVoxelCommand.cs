using Avalonia.Input;
using MedicalSharp.Controls.Base;
using MedicalSharp.Controls.Commands.Arguments;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Controls.Interfaces;
using MedicalSharp.Primitives.Maths;
using OpenTK.Mathematics;
using System;

namespace MedicalSharp.Controls.Commands
{
    /// <summary>
    /// 拾取体素命令
    /// </summary>
    public class PickVoxelCommand : ViewportCommand
    {
        #region # 字段及构造器

        /// <summary>
        /// 体素拾取事件
        /// </summary>
        private readonly Action<VoxelPickedEventArgs> _voxelPickedEvent;

        /// <summary>
        /// 创建拾取体素命令构造器
        /// </summary>
        /// <param name="callback">体素拾取回调</param>
        public PickVoxelCommand(Action<VoxelPickedEventArgs> callback)
        {
            this._voxelPickedEvent = callback;
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
            if (eventArgs.Properties.IsLeftButtonPressed && viewport is IPickVoxel pickVoxel)
            {
                Vector2 mousePos2D = eventArgs.GetPosition(viewport).ToVector2();
                VoxelPickedEventArgs commandEventArgs = new VoxelPickedEventArgs
                {
                    Viewport = viewport,
                    MousePos2D = mousePos2D
                };
                if (pickVoxel.FindNearestVoxel(mousePos2D, out Vector3 textureCoord, out Vector3 worldPosition, out Vector3i voxelPosition, out short voxelValue, out byte markValue, out Ray ray))
                {
                    commandEventArgs.PickedTextureCoord = textureCoord;
                    commandEventArgs.PickedWorldPosition = worldPosition;
                    commandEventArgs.PickedVoxelPosition = voxelPosition;
                    commandEventArgs.PickedVoxelValue = voxelValue;
                    commandEventArgs.PickedMarkValue = markValue;
                    commandEventArgs.Ray = ray;
                }

                this._voxelPickedEvent?.Invoke(commandEventArgs);

                viewport.Camera.LookAt(worldPosition);

                //请求下一帧
                viewport.RequestNextFrameRendering();
            }
        }
        #endregion 

        #endregion
    }
}
