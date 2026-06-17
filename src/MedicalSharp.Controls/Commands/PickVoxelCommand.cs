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
    public class PickVoxelCommand : ShapeCommand
    {
        #region # 字段及构造器

        /// <summary>
        /// 默认构造器
        /// </summary>
        public PickVoxelCommand()
        {

        }

        #endregion

        #region # 属性

        #region 体素已拾取委托 —— Action<VoxelPickedEventArgs> VoxelPicked
        /// <summary>
        /// 体素已拾取委托
        /// </summary>
        public Action<VoxelPickedEventArgs> VoxelPicked { get; set; }
        #endregion

        #endregion

        #region # 方法

        #region 鼠标按下事件 —— override void OnMouseDown(OpenTKViewport viewport...
        /// <summary>
        /// 鼠标按下事件
        /// </summary>
        public override void OnMouseDown(OpenTKViewport viewport, PointerPressedEventArgs eventArgs)
        {
            if (eventArgs.Properties.IsLeftButtonPressed && viewport is IPickVoxel pickVoxel)
            {
                Vector2 mousePos2D = eventArgs.GetPixelPosition(viewport).ToVector2();
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

                this.VoxelPicked?.Invoke(commandEventArgs);

                //看向目标
                if (KeyModifiers.Shift == (eventArgs.KeyModifiers & KeyModifiers.Shift))
                {
                    viewport.Camera.LookAt(worldPosition);
                }

                //请求下一帧
                viewport.RequestNextFrameRendering();
            }
            if (eventArgs.Properties.IsRightButtonPressed)
            {
                base.OnMouseDown(viewport, eventArgs);
            }
        }
        #endregion 

        #endregion
    }
}
