using Avalonia.Collections;
using Caliburn.Micro;
using MedicalSharp.Engine.Algorithms;
using MedicalSharp.Engine.Base;
using MedicalSharp.Engine.Managers;
using MedicalSharp.Engine.Resources;
using MedicalSharp.Presentation.Events;
using MedicalSharp.Presentation.Models;
using MedicalSharp.Primitives.Models;
using SD.Infrastructure.Avalonia.Caliburn.Aspects;
using SD.Infrastructure.Avalonia.Caliburn.Base;

namespace MedicalSharp.Client.ViewModels.AlgorithmContext
{
    /// <summary>
    /// 阈值分割视图模型
    /// </summary>
    public class ThresholdSegmentViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 事件聚合器
        /// </summary>
        private readonly IEventAggregator _eventAggregator;

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public ThresholdSegmentViewModel(IEventAggregator eventAggregator)
        {
            this._eventAggregator = eventAggregator;

            //默认值
            base.DisplayName = "阈值分割";
            this.MinHU = 400;
            this.MaxHU = 1000;
        }

        #endregion

        #region # 属性

        #region 体积数据 —— VolumeData VolumeData
        /// <summary>
        /// 体积数据
        /// </summary>
        public VolumeData VolumeData { get; set; }
        #endregion

        #region 最小HU值 —— int MinHU
        /// <summary>
        /// 最小HU值
        /// </summary>
        public int MinHU
        {
            get;
            set
            {
                field = value;
                this.NotifyOfPropertyChange();
                this.Apply();
            }
        }
        #endregion

        #region 最大HU值 —— int MaxHU
        /// <summary>
        /// 最大HU值
        /// </summary>
        public int MaxHU
        {
            get;
            set
            {
                field = value;
                this.NotifyOfPropertyChange();
                this.Apply();
            }
        }
        #endregion

        #region 已选组织 —— TissueInfo SelectedTissue
        /// <summary>
        /// 已选组织
        /// </summary>
        [DependencyProperty]
        public TissueInfo SelectedTissue { get; set; }
        #endregion

        #region 组织列表 —— AvaloniaList<TissueInfo> Tissues
        /// <summary>
        /// 组织列表
        /// </summary>
        [DependencyProperty]
        public AvaloniaList<TissueInfo> Tissues { get; set; }
        #endregion

        #endregion

        #region # 方法

        #region 应用 —— void Apply()
        /// <summary>
        /// 应用
        /// </summary>
        public void Apply()
        {
            #region # 验证

            if (this.MinHU >= this.MaxHU)
            {
                return;
            }
            if (this.SelectedTissue == null)
            {
                return;
            }
            if (this.SelectedTissue.MarkValue == 0)
            {
                return;
            }

            #endregion

            VolumeSession volumeSession = SessionManager.VolumeSessions[this.VolumeData.Metadata.Id];
            Texture3D previewTexture = volumeSession.PreviewTexture;
            Texture3D markTexture = volumeSession.MarkTexture;
            byte markValue = this.SelectedTissue.MarkValue;

            //先重置Mark值
            this.VolumeData.ResetMark(markTexture, markValue);

            //再分割
            this.VolumeData.ThresholdSegment(previewTexture, markTexture, this.MinHU, this.MaxHU, markValue);

            //发布消息
            SyncViewportEvent message = new SyncViewportEvent
            {
                Publisher = this
            };
            this._eventAggregator.PublishOnUIThreadAsync(message);
        }
        #endregion

        #endregion
    }
}
