using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Caliburn.Micro;
using SD.Infrastructure.Avalonia.Caliburn.Aspects;
using SD.Infrastructure.Avalonia.Caliburn.Base;
using SD.Infrastructure.Avalonia.Caliburn.Extensions;
using SD.Infrastructure.Avalonia.Commands;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MedicalSharp.Client.ViewModels.CommonContext
{
    /// <summary>
    /// 图像查看视图模型
    /// </summary>
    public class ImageViewModel : ScreenBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 窗体管理器
        /// </summary>
        private readonly IWindowManager _windowManager;

        /// <summary>
        /// 依赖注入构造器
        /// </summary>
        public ImageViewModel(IWindowManager windowManager)
        {
            this._windowManager = windowManager;
        }

        #endregion

        #region # 属性

        //属性

        #region 标题 —— string Title
        /// <summary>
        /// 标题
        /// </summary>
        [DependencyProperty]
        public string Title { get; set; }
        #endregion

        #region 图像源 —— Bitmap Image
        /// <summary>
        /// 图像源
        /// </summary>
        [DependencyProperty]
        public Bitmap Image { get; set; }
        #endregion


        //命令

        #region 另存为图像命令 —— ICommand SaveAsImageCommand
        /// <summary>
        /// 另存为图像命令
        /// </summary>
        public ICommand SaveAsImageCommand => new AsyncRelayCommand(async _ =>
        {
            this.Busy();

            //保存文件对话框
            FilePickerSaveOptions openOptions = new FilePickerSaveOptions
            {
                Title = "保存图像",
                SuggestedFileName = this.Title,
                FileTypeChoices = [
                    new FilePickerFileType("JPEG图像")
                    {
                        Patterns = ["*.jpg", "*.jpeg"]
                    },
                    new FilePickerFileType("PNG图像")
                    {
                        Patterns = ["*.png"]
                    },
                    new FilePickerFileType("BMP图像")
                    {
                        Patterns = ["*.bmp"]
                    },
                    new FilePickerFileType("所有图像")
                    {
                        Patterns = ["*.jpg", "*.jpeg", "*.png", "*.bmp"]
                    }
                ]
            };

            //保存文件
            IStorageFile storageFile = await this.SaveFilePickerAsync(openOptions);
            if (storageFile != null)
            {
                string filePath = storageFile.TryGetLocalPath();
                await using FileStream fileStream = File.Create(filePath!);
                await Task.Run(() => this.Image.Save(fileStream));
            }

            this.Idle();
        }, _ => this.Image != null);
        #endregion

        #endregion

        #region # 方法

        #region 加载 —— void Load(Bitmap image...
        /// <summary>
        /// 加载
        /// </summary>
        /// <param name="image">图像</param>
        /// <param name="title">标题</param>
        public void Load(Bitmap image, string title = "查看图像")
        {
            this.Image = image;
            this.Title = title;
        }
        #endregion

        #endregion
    }
}
