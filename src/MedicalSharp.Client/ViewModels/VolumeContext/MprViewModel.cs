using Avalonia.Collections;
using Caliburn.Micro;
using IconPacks.Avalonia.MaterialDesign;
using MedicalSharp.Controls.Commands;
using MedicalSharp.Controls.Commands.Arguments;
using MedicalSharp.Controls.InputManagers;
using MedicalSharp.Controls.Visuals;
using MedicalSharp.Primitives.Cameras;
using MedicalSharp.Primitives.Maths;
using MedicalSharp.Primitives.Models;
using OpenTK.Mathematics;
using SD.Infrastructure.Avalonia.Caliburn.Aspects;
using SD.Infrastructure.Avalonia.Caliburn.Base;
using SD.Infrastructure.Avalonia.CustomControls;
using SD.Infrastructure.Avalonia.Enums;
using System;
using System.Text;

namespace MedicalSharp.Client.ViewModels.VolumeContext
{
    /// <summary>
    /// MPR视图模型
    /// </summary>
    public class MprViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 窗口管理器
        /// </summary>
        private readonly IWindowManager _windowManager;

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public MprViewModel(IWindowManager windowManager, string title, MPRCamera camera, MPRInputManager inputManager)
        {
            this._windowManager = windowManager;
            this.Title = title;
            this.Camera = camera;
            this.InputManager = inputManager;
            this.PickVoxel();
        }

        #endregion

        #region # 属性

        #region 标题 —— string Title
        /// <summary>
        /// 标题
        /// </summary>
        [DependencyProperty]
        public string Title { get; set; }
        #endregion

        #region MPR平面 —— MPRPlane Plane
        /// <summary>
        /// MPR平面
        /// </summary>
        [DependencyProperty]
        public MPRPlane Plane { get; set; }
        #endregion

        #region MPR相机 —— MPRCamera Camera
        /// <summary>
        /// MPR相机
        /// </summary>
        [DependencyProperty]
        public MPRCamera Camera { get; set; }
        #endregion

        #region 输入管理器 —— MPRInputManager InputManager
        /// <summary>
        /// 输入管理器
        /// </summary>
        [DependencyProperty]
        public MPRInputManager InputManager { get; set; }
        #endregion

        #region 体积数据 —— VolumeData VolumeData
        /// <summary>
        /// 体积数据
        /// </summary>
        [DependencyProperty]
        public VolumeData VolumeData { get; set; }
        #endregion

        #region 选中的形状 —— ShapeVisual3D SelectedShape
        /// <summary>
        /// 选中的形状
        /// </summary>
        [DependencyProperty]
        public ShapeVisual3D SelectedShape { get; set; }
        #endregion

        #region 形状列表 —— AvaloniaList<ShapeVisual3D> Shapes
        /// <summary>
        /// 形状列表
        /// </summary>
        [DependencyProperty]
        public AvaloniaList<ShapeVisual3D> Shapes { get; set; }
        #endregion

        #endregion

        #region # 方法

        #region 拾取体素 —— void PickVoxel()
        /// <summary>
        /// 拾取体素
        /// </summary>
        public void PickVoxel()
        {
            Action<VoxelPickedEventArgs> picked = e =>
            {
                Vector2 mousePos2D = e.MousePos2D;
                Vector3? textureCoord = e.PickedTextureCoord;
                Vector3? worldPosition = e.PickedWorldPosition;
                Vector3i? voxelPostion = e.PickedVoxelPosition;
                short? voxelValue = e.PickedVoxelValue;
                byte? markValue = e.PickedMarkValue;
                if (textureCoord.HasValue)
                {
                    StringBuilder builder = new StringBuilder();
                    builder.AppendLine($"点击2D坐标: X:{mousePos2D.X}, Y:{mousePos2D.Y}");
                    builder.AppendLine($"点击纹理坐标: X:{textureCoord.Value.X}, Y:{textureCoord.Value.Y}, Z:{textureCoord.Value.Z}");
                    builder.AppendLine($"点击世界坐标: X:{worldPosition.Value.X}, Y:{worldPosition.Value.Y}, Z:{worldPosition.Value.Z}");
                    builder.AppendLine($"点击体素坐标: X:{voxelPostion.Value.X}, Y:{voxelPostion.Value.Y}, Z:{voxelPostion.Value.Z}");
                    builder.AppendLine($"点击体素HU值: {voxelValue}");
                    builder.AppendLine($"点击标记值: {markValue}");
                    MessageBox.Show(builder.ToString(), "成功", MessageBoxButton.OK, PackIconMaterialDesignKind.Info);
                }
                else
                {
                    MessageBox.Show("拾取失败！", "错误", MessageBoxButton.OK, PackIconMaterialDesignKind.Error);
                }
            };

            PickVoxelCommand command = new PickVoxelCommand(picked);
            this.InputManager.SwitchCommand(command);
        }
        #endregion

        #region 拾取形状 —— void PickShape()
        /// <summary>
        /// 拾取形状
        /// </summary>
        public void PickShape()
        {
            Action<Visual3DPickedEventArgs> picked = e =>
            {
                if (e.PickedVisual is ShapeVisual3D shapeVisual3D)
                {
                    this.SelectedShape = shapeVisual3D;
                }
            };
            Action<Visual3D> removed = visual =>
            {
                if (visual is ShapeVisual3D shapeVisual3D)
                {
                    this.SelectedShape = null;
                    this.Shapes.Remove(shapeVisual3D);
                }
            };

            PickVisual3DCommand command = new PickVisual3DCommand(picked, removed);
            this.InputManager.SwitchCommand(command);
        }
        #endregion

        #endregion
    }
}
