using MedicalSharp.Primitives.Enums;
using System;
using System.Collections.Generic;

namespace MedicalSharp.Primitives.Models
{
    /// <summary>
    /// 标记策略
    /// </summary>
    public sealed class MarkStrategy
    {
        #region # 字段及构造器

        /// <summary>
        /// 标记长度
        /// </summary>
        public const int MarkLength = 256;

        /// <summary>
        /// 标记模式列表
        /// </summary>
        private readonly MarkMode[] _markModes;

        /// <summary>
        /// 创建标记策略构造器
        /// </summary>
        public MarkStrategy()
        {
            this._markModes = new MarkMode[MarkLength];
            for (int index = 0; index < MarkLength; index++)
            {
                this._markModes[index] = MarkMode.Visible;
            }
            this.HighlightIntensity = 1.5f;
        }

        #endregion

        #region # 属性

        #region 高亮强度 —— float HighlightIntensity
        /// <summary>
        /// 高亮强度
        /// </summary>
        /// <remarks>值域：1.2~2.0</remarks>
        public float HighlightIntensity { get; private set; }
        #endregion

        #region 只读属性 - 标记模式列表 —— IReadOnlyList<MarkMode> MarkModes
        /// <summary>
        /// 只读属性 - 标记模式列表
        /// </summary>
        public IReadOnlyList<MarkMode> MarkModes
        {
            get => this._markModes;
        }
        #endregion

        #endregion

        #region # 方法

        #region 切换标记模式 —— void SwitchMarkMode(byte markValue, MarkMode markMode)
        /// <summary>
        /// 切换标记模式
        /// </summary>
        /// <param name="markValue">标记值</param>
        /// <param name="markMode">标记模式</param>
        public void SwitchMarkMode(byte markValue, MarkMode markMode)
        {
            this._markModes[markValue] = markMode;
        }
        #endregion

        #region 设置高亮强度 —— void SetHighlightIntensity(float highlightIntensity)
        /// <summary>
        /// 设置高亮强度
        /// </summary>
        /// <param name="highlightIntensity">高亮强度</param>
        public void SetHighlightIntensity(float highlightIntensity)
        {
            this.HighlightIntensity = Math.Clamp(highlightIntensity, 1.2f, 2.0f);
        }
        #endregion

        #endregion
    }
}
