using Avalonia.Media;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MedicalSharp.Controls.UserControls
{
    /// <summary>
    /// 颜色控制点
    /// </summary>
    public class ColorControlPoint : INotifyPropertyChanged
    {
        /// <summary>
        /// 属性改变事件
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// HU值
        /// </summary>
        public short HU
        {
            get;
            set
            {
                field = value;
                this.OnPropertyChanged(nameof(this.HU));
            }
        }

        /// <summary>
        /// 颜色
        /// </summary>
        public Color Color
        {
            get;
            set
            {
                field = value;
                this.OnPropertyChanged(nameof(this.Color));
            }
        }

        /// <summary>
        /// 属性改变事件
        /// </summary>
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
