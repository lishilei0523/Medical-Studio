using Caliburn.Micro;
using MedicalSharp.Engine.Algorithms;
using MedicalSharp.Engine.Managers;
using MedicalSharp.Engine.Resources;
using MedicalSharp.Inspiration.Algorithms;
using MedicalSharp.Inspiration.Managers;
using MedicalSharp.Inspiration.Resources;
using MedicalSharp.Presentation.Events;
using MedicalSharp.Primitives.Models;
using SD.Infrastructure.Avalonia.Caliburn.Aspects;
using SD.Infrastructure.Avalonia.Caliburn.Base;
using Silk.NET.OpenCL;
using System.Threading;
using System.Threading.Tasks;

namespace MedicalSharp.Client.ViewModels.AlgorithmContext
{
    /// <summary>
    /// Laplacian边缘检测视图模型
    /// </summary>
    public class LaplacianViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 事件聚合器
        /// </summary>
        private readonly IEventAggregator _eventAggregator;

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public LaplacianViewModel(IEventAggregator eventAggregator)
        {
            this._eventAggregator = eventAggregator;
        }

        #endregion

        #region # 属性

        #region 体积数据 —— VolumeData VolumeData
        /// <summary>
        /// 体积数据
        /// </summary>
        public VolumeData VolumeData { get; set; }
        #endregion

        #region 核矩阵尺寸 —— int KernelSize
        /// <summary>
        /// 核矩阵尺寸
        /// </summary>
        [DependencyProperty]
        public int KernelSize { get; set; }
        #endregion

        #endregion

        #region # 方法

        #region 初始化 —— override Task OnActivatedAsync(CancellationToken cancellationToken)
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override Task OnActivatedAsync(CancellationToken cancellationToken)
        {
            //默认值
            this.KernelSize = 3;

            return base.OnActivatedAsync(cancellationToken);
        }
        #endregion

        #region 应用 —— async Task Apply()
        /// <summary>
        /// 应用
        /// </summary>
        public async Task Apply()
        {
            VolumeSession volumeSession = SessionManager.VolumeSessions[this.VolumeData.Metadata.Id];
            ClContext clContext = ClContextManager.Current;

            int width = this.VolumeData.Metadata.VolumeSize.X;
            int height = this.VolumeData.Metadata.VolumeSize.Y;
            int depth = this.VolumeData.Metadata.VolumeSize.Z;

            this.Busy();
            await Task.Run(() =>
            {
                //创建图像
                using ClImage3D inputImage = ClImage3D.Create(clContext, width, height, depth, MemFlags.ReadWrite, ChannelOrder.Intensity, ChannelType.SNormInt16);
                inputImage.Write(clContext.CommandQueue, this.VolumeData.PreviewData);
                clContext.Finish();

                //执行算法
                using Laplacian3D laplacian = new Laplacian3D(clContext);
                laplacian.ExecuteInPlace(inputImage, this.KernelSize);

                //读回CPU
                inputImage.Read(clContext.CommandQueue, this.VolumeData.PreviewData);
            });
            this.Idle();

            //同步到预览纹理
            SyncAlgorithms.SyncPreviewDataToGpu(this.VolumeData, volumeSession.PreviewTexture);

            //发布消息
            SyncViewportEvent message = new SyncViewportEvent();
            await this._eventAggregator.PublishOnUIThreadAsync(message);
        }
        #endregion

        #endregion
    }
}
