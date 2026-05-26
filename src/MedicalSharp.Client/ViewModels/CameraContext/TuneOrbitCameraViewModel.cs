using Caliburn.Micro;
using SD.Infrastructure.Avalonia.Caliburn.Aspects;
using SD.Infrastructure.Avalonia.Caliburn.Base;
using System.Threading.Tasks;

namespace MedicalSharp.Client.ViewModels.CameraContext
{
    /// <summary>
    /// 调节轨道相机视图模型
    /// </summary>
    public class TuneOrbitCameraViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 事件聚合器
        /// </summary>
        private readonly IEventAggregator _eventAggregator;

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public TuneOrbitCameraViewModel(IEventAggregator eventAggregator)
        {
            this._eventAggregator = eventAggregator;
        }

        #endregion

        #region # 属性

        #region 平移速度 —— float PanSpeed
        /// <summary>
        /// 平移速度
        /// </summary>
        [DependencyProperty]
        public float PanSpeed { get; set; }
        #endregion

        #region 旋转速度 —— float RotateSpeed
        /// <summary>
        /// 旋转速度
        /// </summary>
        [DependencyProperty]
        public float RotateSpeed { get; set; }
        #endregion

        #region 缩放速度 —— float ZoomSpeed
        /// <summary>
        /// 缩放速度
        /// </summary>
        [DependencyProperty]
        public float ZoomSpeed { get; set; }
        #endregion

        #region 最小距离 —— float MinDistance
        /// <summary>
        /// 最小距离
        /// </summary>
        [DependencyProperty]
        public float MinDistance { get; set; }
        #endregion

        #region 最大距离 —— float MaxDistance
        /// <summary>
        /// 最大距离
        /// </summary>
        [DependencyProperty]
        public float MaxDistance { get; set; }
        #endregion

        #endregion

        #region # 方法

        #region 提交 —— async Task Submit()
        /// <summary>
        /// 提交
        /// </summary>
        public async Task Submit()
        {
            await this.TryCloseAsync(true);
        }
        #endregion

        #endregion
    }
}
