using Avalonia.Collections;
using Caliburn.Micro;
using MedicalSharp.Controls.Commands;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Controls.Visual3Ds;
using MedicalSharp.Engine.Base;
using MedicalSharp.Engine.Managers;
using MedicalSharp.Inspiration.Managers;
using MedicalSharp.Inspiration.Operators;
using MedicalSharp.Inspiration.Resources;
using MedicalSharp.Presentation.Events;
using MedicalSharp.Presentation.Models;
using MedicalSharp.Primitives.Algorithms;
using MedicalSharp.Primitives.Models;
using OpenTK.Mathematics;
using SD.Infrastructure.Avalonia.Caliburn.Aspects;
using SD.Infrastructure.Avalonia.Caliburn.Base;
using SD.Infrastructure.Avalonia.CustomControls;
using Silk.NET.OpenCL;
using System;
using System.Threading.Tasks;

namespace MedicalSharp.Client.ViewModels.AlgorithmContext
{
    /// <summary>
    /// 区域生长视图模型
    /// </summary>
    public class RegionGrowViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 单点区域生长命令
        /// </summary>
        private MonoRegionGrowCommand _monoRegionGrowCommand;

        /// <summary>
        /// 事件聚合器
        /// </summary>
        private readonly IEventAggregator _eventAggregator;

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public RegionGrowViewModel(IEventAggregator eventAggregator)
        {
            this._eventAggregator = eventAggregator;
            base.DisplayName = "区域生长";
            this.MinHU = 400;
            this.MaxHU = 1000;
            this.MaxIterations = 100;

            //初始化命令
            this.InitCommands();
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
        [DependencyProperty]
        public int MinHU { get; set; }
        #endregion

        #region 最大HU值 —— int MaxHU
        /// <summary>
        /// 最大HU值
        /// </summary>
        [DependencyProperty]
        public int MaxHU { get; set; }
        #endregion

        #region 最大迭代次数 —— int MaxIterations
        /// <summary>
        /// 最大迭代次数
        /// </summary>
        [DependencyProperty]
        public int MaxIterations { get; set; }
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

        #region 切换单点区域生长 —— void SwitchToMonoRegionGrow()
        /// <summary>
        /// 切换单点区域生长
        /// </summary>
        public void SwitchToMonoRegionGrow()
        {
            //发布事件，将this._monoRegionGrowCommand同步给体积渲染和MPR三视图切换命令
            SwitchViewportCommandEvent message = new SwitchViewportCommandEvent
            {
                Publisher = this,
                Command = this._monoRegionGrowCommand
            };
            this._eventAggregator.PublishOnUIThreadAsync(message);
        }
        #endregion

        #region 切换多点区域生长 —— void SwitchToMultiRegionGrow()
        /// <summary>
        /// 切换多点区域生长
        /// </summary>
        public void SwitchToMultiRegionGrow()
        {
            //TODO 实现
        }
        #endregion

        #region 切换曲线区域生长 —— void SwitchToCurveRegionGrow()
        /// <summary>
        /// 切换曲线区域生长
        /// </summary>
        public void SwitchToCurveRegionGrow()
        {
            //TODO 实现
        }
        #endregion

        #region 初始化命令 —— void InitCommands()
        /// <summary>
        /// 初始化命令
        /// </summary>
        private void InitCommands()
        {
            this.InitMonoCommand();
        }
        #endregion

        #region 初始化单点区域生长命令 —— void InitMonoCommand()
        /// <summary>
        /// 初始化单点区域生长命令
        /// </summary>
        private void InitMonoCommand()
        {
            Action<PointVisual3D, PointVisual3D> picked = (current, prev) =>
            {
                current.DisplayName = "种子点";

                //删除上一点
                if (prev != null)
                {
                    RemoveShapeEvent removeMessage = new RemoveShapeEvent
                    {
                        Publisher = this,
                        Shape = prev
                    };
                    this._eventAggregator.PublishOnUIThreadAsync(removeMessage);
                }

                //添加当前点
                AppendShapeEvent appendMessage = new AppendShapeEvent
                {
                    Publisher = this,
                    Shape = current
                };
                this._eventAggregator.PublishOnUIThreadAsync(appendMessage);
            };
            Action<PointVisual3D> confirmed = async current =>
            {
                #region # 验证

                if (this.SelectedTissue == null || this.SelectedTissue.MarkValue == 0)
                {
                    await MessageBox.Show("当前未选中有效组织！", "错误");
                    return;
                }

                #endregion

                //将世界坐标转为体素坐标
                Vector3 worldPosition = current.Position.ToVector3();
                Vector3i voxelPosition = worldPosition.ToVoxelPosition(this.VolumeData.Metadata);

                //获取会话
                VolumeSession volumeSession = SessionManager.VolumeSessions[this.VolumeData.Metadata.Id];
                ClContext clContext = ClContextManager.Current;
                int width = this.VolumeData.Metadata.VolumeSize.X;
                int height = this.VolumeData.Metadata.VolumeSize.Y;
                int depth = this.VolumeData.Metadata.VolumeSize.Z;

                //获取标记值
                byte markValue = this.SelectedTissue.MarkValue;

                await Task.Run(() =>
                {
                    //设置种子点标记值
                    this.VolumeData.SetMarkValue(voxelPosition, markValue);

                    //创建图像
                    using ClImage3D previewImage = ClImage3D.Create(clContext, width, height, depth, MemFlags.ReadOnly, ChannelOrder.Intensity, ChannelType.SNormInt16);
                    previewImage.Write(clContext.CommandQueue, this.VolumeData.PreviewData);
                    clContext.Finish();

                    //执行算子
                    using RegionGrowing3D regionGrowing = new RegionGrowing3D(clContext);
                    regionGrowing.Execute(previewImage, this.VolumeData.MarkData, this.MinHU, this.MaxHU, markValue, this.MaxIterations);
                });

                //同步到标记纹理
                this.VolumeData.SyncMarkDataToGpu(volumeSession.MarkTexture);

                //删除当前点
                RemoveShapeEvent removeMessage = new RemoveShapeEvent
                {
                    Publisher = this,
                    Shape = current
                };
                await this._eventAggregator.PublishOnUIThreadAsync(removeMessage);
            };
            Action<PointVisual3D> cancelled = current =>
            {
                //删除当前点
                RemoveShapeEvent removeMessage = new RemoveShapeEvent
                {
                    Publisher = this,
                    Shape = current
                };
                this._eventAggregator.PublishOnUIThreadAsync(removeMessage);
            };
            this._monoRegionGrowCommand = new MonoRegionGrowCommand
            {
                SeedPointPicked = picked,
                RegionGrowConfirmed = confirmed,
                RegionGrowCancelled = cancelled
            };
        }
        #endregion

        #endregion
    }
}
