using Avalonia.Media;
using Caliburn.Micro;
using MedicalSharp.Presentation.Events;
using MedicalSharp.Primitives.Enums;
using SD.Common;
using SD.Infrastructure.Avalonia.Caliburn.Aspects;
using SD.IOC.Core.Mediators;
using System;
using System.Collections.Generic;

namespace MedicalSharp.Presentation.Models
{
    /// <summary>
    /// 组织信息
    /// </summary>
    public class TissueInfo : PropertyChangedBase
    {
        #region 创建组织信息构造器 —— TissueInfo(string name...
        /// <summary>
        /// 创建组织信息构造器
        /// </summary>
        /// <param name="name">组织名称</param>
        /// <param name="markValue">标记值</param>
        /// <param name="markMode">标记模式</param>
        /// <param name="color">颜色</param>
        /// <param name="locked">是否锁定</param>
        public TissueInfo(string name, byte markValue, MarkMode markMode, Color color, bool locked = false)
        {
            this.Name = name;
            this.MarkValue = markValue;
            this.MarkMode = markMode;
            this.Color = color;
            this.Locked = locked;
            this.SelectedMarkMode = new KeyValuePair<string, string>(this.MarkMode.ToString(), this.MarkMode.GetEnumMember());
        }
        #endregion

        #region 组织名称 —— string Name
        /// <summary>
        /// 组织名称
        /// </summary>
        [DependencyProperty]
        public string Name { get; set; }
        #endregion

        #region 标记值 —— byte MarkValue
        /// <summary>
        /// 标记值
        /// </summary>
        [DependencyProperty]
        public byte MarkValue { get; set; }
        #endregion

        #region 标记模式 —— MarkMode MarkMode
        /// <summary>
        /// 标记模式
        /// </summary>
        public MarkMode MarkMode
        {
            get;
            set
            {
                field = value;
                this.NotifyOfPropertyChange();

                //发布事件
                MarkModeSwitchedEvent message = new MarkModeSwitchedEvent
                {
                    MarkValue = this.MarkValue,
                    MarkMode = value
                };
                IEventAggregator eventAggregator = ResolveMediator.Resolve<IEventAggregator>();
                eventAggregator.PublishOnUIThreadAsync(message);
            }
        }
        #endregion

        #region 颜色 —— Color Color
        /// <summary>
        /// 颜色
        /// </summary>
        [DependencyProperty]
        public Color Color { get; set; }
        #endregion

        #region 是否锁定 —— bool Locked
        /// <summary>
        /// 是否锁定
        /// </summary>
        [DependencyProperty]
        public bool Locked { get; set; }
        #endregion

        #region 已选标记模式 —— KeyValuePair<string, string> SelectedMarkMode
        /// <summary>
        /// 已选标记模式
        /// </summary>
        public KeyValuePair<string, string> SelectedMarkMode
        {
            get;
            set
            {
                field = value;
                this.NotifyOfPropertyChange();
                this.MarkMode = Enum.Parse<MarkMode>(value.Key);
            }
        }
        #endregion
    }
}
