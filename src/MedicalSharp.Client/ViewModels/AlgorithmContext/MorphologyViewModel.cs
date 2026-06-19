using Caliburn.Micro;
using MedicalSharp.Engine.Base;
using MedicalSharp.Engine.Managers;
using MedicalSharp.Inspiration.Managers;
using MedicalSharp.Inspiration.Operators;
using MedicalSharp.Inspiration.Resources;
using MedicalSharp.Presentation.Events;
using MedicalSharp.Primitives.Enums;
using MedicalSharp.Primitives.Models;
using SD.Infrastructure.Avalonia.Caliburn.Aspects;
using SD.Infrastructure.Avalonia.Caliburn.Base;
using Silk.NET.OpenCL;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MedicalSharp.Client.ViewModels.AlgorithmContext
{
    /// <summary>
    /// 形态学视图模型
    /// </summary>
    public class MorphologyViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 事件聚合器
        /// </summary>
        private readonly IEventAggregator _eventAggregator;

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public MorphologyViewModel(IEventAggregator eventAggregator)
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

        #region 形态学模式 —— MorphMode MorphMode
        /// <summary>
        /// 形态学模式
        /// </summary>
        [DependencyProperty]
        public MorphMode MorphMode { get; set; }
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
                using ClImage3D outputImage = ClImage3D.Create(clContext, width, height, depth, MemFlags.ReadWrite, ChannelOrder.Intensity, ChannelType.SNormInt16);
                inputImage.Write(clContext.CommandQueue, this.VolumeData.PreviewData);
                clContext.Finish();

                //执行算子
                using Morphology3D morphology = new Morphology3D(clContext);
                switch (this.MorphMode)
                {
                    case MorphMode.Erode:
                        morphology.Erode(inputImage, outputImage, this.KernelSize);
                        clContext.Finish();
                        break;
                    case MorphMode.Dilate:
                        morphology.Dilate(inputImage, outputImage, this.KernelSize);
                        clContext.Finish();
                        break;
                    case MorphMode.Open:
                        morphology.Open(inputImage, outputImage, this.KernelSize);
                        break;
                    case MorphMode.Close:
                        morphology.Close(inputImage, outputImage, this.KernelSize);
                        break;
                    case MorphMode.TopHat:
                        morphology.TopHat(inputImage, outputImage, this.KernelSize);
                        break;
                    case MorphMode.BlackHat:
                        morphology.BlackHat(inputImage, outputImage, this.KernelSize);
                        break;
                    case MorphMode.Gradient:
                        morphology.Gradient(inputImage, outputImage, this.KernelSize);
                        break;
                    default:
                        throw new NotSupportedException();
                }

                //读回CPU
                outputImage.Read(clContext.CommandQueue, this.VolumeData.PreviewData);
            });
            this.Idle();

            //同步到预览纹理
            this.VolumeData.SyncPreviewDataToGpu(volumeSession.PreviewTexture);

            //发布消息
            SyncViewportEvent message = new SyncViewportEvent();
            await this._eventAggregator.PublishOnUIThreadAsync(message);
        }
        #endregion

        #endregion
    }
}
