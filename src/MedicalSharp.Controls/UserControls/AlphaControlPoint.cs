using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MedicalSharp.Controls.UserControls
{
    /// <summary>
    /// Alpha控制点
    /// </summary>
    public class AlphaControlPoint : INotifyPropertyChanged
    {
        #region # 字段及构造器

        /// <summary>
        /// 属性改变事件
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 默认构造器
        /// </summary>
        public AlphaControlPoint()
        {

        }

        #endregion

        #region # 属性

        #region HU值 —— short HU
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
        #endregion

        #region 透明度 —— float Alpha
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
        #endregion

        #endregion

        #region # 方法

        #region 属性改变事件 —— void OnPropertyChanged(string propertyName = null)
        /// <summary>
        /// 属性改变事件
        /// </summary>
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion 

        #endregion
    }
}
