using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MedicalSharp.Controls.UserControls
{
    /// <summary>
    /// Alpha控制点
    /// </summary>
    public class AlphaControlPoint : INotifyPropertyChanged
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
        /// 透明度
        /// </summary>
        public float Alpha
        {
            get;
            set
            {
                field = value;
                this.OnPropertyChanged(nameof(this.Alpha));
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
