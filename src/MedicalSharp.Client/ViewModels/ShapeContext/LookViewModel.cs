using Avalonia;
using Avalonia.Collections;
using Avalonia.Input;
using Avalonia.Media;
using Caliburn.Micro;
using IconPacks.Avalonia.MaterialDesign;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Controls.Viewports;
using MedicalSharp.Controls.Visuals;
using MedicalSharp.Primitives.Cameras;
using OpenTK.Mathematics;
using SD.Infrastructure.Avalonia.Caliburn.Aspects;
using SD.Infrastructure.Avalonia.Caliburn.Base;
using SD.Infrastructure.Avalonia.CustomControls;
using SD.Infrastructure.Avalonia.Enums;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MedicalSharp.Client.ViewModels.ShapeContext
{
    /// <summary>
    /// 查看视图模型
    /// </summary>
    public class LookViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 窗口管理器
        /// </summary>
        private readonly IWindowManager _windowManager;

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public LookViewModel(IWindowManager windowManager)
        {
            this._windowManager = windowManager;

            //初始化相机
            Vector3 cameraPosition = new Vector3(0, 7, 0);
            Vector3 targetPosition = new Vector3(0.0f);
            Vector3 upDirection = new Vector3(0, 0, 1);
            this.OrbitCamera = new OrbitPerspectiveCamera(cameraPosition, targetPosition, upDirection);
        }

        #endregion

        #region # 属性

        #region 轨道相机 —— OrbitCamera OrbitCamera
        /// <summary>
        /// 轨道相机
        /// </summary>
        [DependencyProperty]
        public OrbitCamera OrbitCamera { get; set; }
        #endregion

        #region 边界球体3D元素 —— BoundingSphereVisual3D BoundingSphere
        /// <summary>
        /// 边界球体3D元素
        /// </summary>
        [DependencyProperty]
        public BoundingSphereVisual3D BoundingSphere { get; set; }
        #endregion

        #region 边界3D元素列表 —— AvaloniaList<BoundingVisual3D> BoundingVisuals
        /// <summary>
        /// 边界3D元素列表
        /// </summary>
        [DependencyProperty]
        public AvaloniaList<BoundingVisual3D> BoundingVisuals { get; set; }
        #endregion

        #endregion

        #region # 方法

        //Initializations

        #region 初始化 —— override Task OnInitializedAsync(CancellationToken...
        /// <summary>
        /// 初始化
        /// </summary>
        protected override Task OnInitializedAsync(CancellationToken cancellationToken)
        {
            this.BoundingSphere = new BoundingSphereVisual3D
            {
                Radius = 1,
                Center = new Vector3D(-2, 0, 0),
                Stroke = Colors.Green,
                StrokeThickness = 1,
                Fill = Color.Parse("#0FFF0000")
            };
            this.BoundingVisuals =
            [
                new BoundingBoxVisual3D
                {
                    Center = new Vector3D(2,0,0),
                    Stroke = Colors.Blue,
                    StrokeThickness = 1,
                    Fill = Color.Parse("#0FFFFF00")
                }
            ];

            return base.OnInitializedAsync(cancellationToken);
        }
        #endregion


        //Actions

        #region 视口鼠标按下事件 —— void OnViewportPointerPressed(ShapeViewport viewport...
        /// <summary>
        /// 视口鼠标按下事件
        /// </summary>
        public void OnViewportPointerPressed(ShapeViewport viewport, PointerPressedEventArgs eventArgs)
        {
            if (eventArgs.Properties.IsLeftButtonPressed)
            {
                Point mousePos2D = eventArgs.GetPosition(viewport);
                bool success = viewport.FindNearest(mousePos2D.ToVector2(), out Vector3 mousePos3D, out Vector3 normal, out BoundingVisual3D element);

                if (success)
                {
                    StringBuilder builder = new StringBuilder();
                    builder.AppendLine($"点击对象: {element?.GetType().Name}");
                    builder.AppendLine($"点击2D坐标: X:{mousePos2D.X}, Y:{mousePos2D.Y}");
                    builder.AppendLine($"点击3D坐标: X:{mousePos3D.X}, Y:{mousePos3D.Y}, Z:{mousePos3D.Z}");
                    builder.AppendLine($"法向量: X:{normal.X}, Y:{normal.Y}, Z:{normal.Z}");
                    MessageBox.Show(builder.ToString(), "成功", MessageBoxButton.OK, PackIconMaterialDesignKind.Info);
                }
                else
                {
                    MessageBox.Show("获取失败！", "错误", MessageBoxButton.OK, PackIconMaterialDesignKind.Error);
                }
            }
        }
        #endregion

        #endregion
    }
}
