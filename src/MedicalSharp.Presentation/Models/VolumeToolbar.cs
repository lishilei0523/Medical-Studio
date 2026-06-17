using Caliburn.Micro;
using SD.Infrastructure.Avalonia.Caliburn.Aspects;

namespace MedicalSharp.Presentation.Models
{
    /// <summary>
    /// 体积渲染工具栏
    /// </summary>
    public class VolumeToolbar : PropertyChangedBase
    {
        #region # 字段及构造器

        /// <summary>
        /// 创建体积渲染工具栏构造器
        /// </summary>
        public VolumeToolbar()
        {
            this.PickVoxelEnabled = true;
            this.TranslateNormalEnabled = true;
            this.Translate3DEnabled = true;
            this.RotateUEnabled = true;
            this.RotateVEnabled = true;
            this.Rotate3DEnabled = true;
            this.ResizeEnabled = true;
            this.EditVertexEnabled = true;
            this.DrawPointEnabled = true;
            this.DrawLineSegmentEnabled = true;
            this.DrawRectangleEnabled = true;
            this.DrawCircleEnabled = true;
            this.DrawEllipseEnabled = true;
            this.DrawPolylineEnabled = true;
            this.DrawCurveEnabled = true;
            this.DrawPolygonEnabled = true;
            this.DrawClosedCurveEnabled = true;
            this.DrawBoxEnabled = true;
            this.DrawSphereEnabled = true;
            this.DrawCylinderEnabled = true;
            this.DrawConvexPolyhedronEnabled = true;
        }

        #endregion

        #region # 属性

        #region 拾取体素启用 —— bool PickVoxelEnabled
        /// <summary>
        /// 拾取体素启用
        /// </summary>
        [DependencyProperty]
        public bool PickVoxelEnabled { get; set; }
        #endregion

        #region 沿法向量平移启用 —— bool TranslateNormalEnabled
        /// <summary>
        /// 沿法向量平移启用
        /// </summary>
        [DependencyProperty]
        public bool TranslateNormalEnabled { get; set; }
        #endregion

        #region 3D平移启用 —— bool Translate3DEnabled
        /// <summary>
        /// 3D平移启用
        /// </summary>
        [DependencyProperty]
        public bool Translate3DEnabled { get; set; }
        #endregion

        #region U轴旋转启用 —— bool RotateUEnabled
        /// <summary>
        /// U轴旋转启用
        /// </summary>
        [DependencyProperty]
        public bool RotateUEnabled { get; set; }
        #endregion

        #region V轴旋转启用 —— bool RotateVEnabled
        /// <summary>
        /// V轴旋转启用
        /// </summary>
        [DependencyProperty]
        public bool RotateVEnabled { get; set; }
        #endregion

        #region 3D旋转启用 —— bool Rotate3DEnabled
        /// <summary>
        /// 3D旋转启用
        /// </summary>
        [DependencyProperty]
        public bool Rotate3DEnabled { get; set; }
        #endregion

        #region 调整尺寸启用 —— bool ResizeEnabled
        /// <summary>
        /// 调整尺寸启用
        /// </summary>
        [DependencyProperty]
        public bool ResizeEnabled { get; set; }
        #endregion

        #region 编辑顶点启用 —— bool EditVertexEnabled
        /// <summary>
        /// 编辑顶点启用
        /// </summary>
        [DependencyProperty]
        public bool EditVertexEnabled { get; set; }
        #endregion

        #region 绘制点启用 —— bool DrawPointEnabled
        /// <summary>
        /// 绘制点启用
        /// </summary>
        [DependencyProperty]
        public bool DrawPointEnabled { get; set; }
        #endregion

        #region 绘制线段启用 —— bool DrawLineSegmentEnabled
        /// <summary>
        /// 绘制线段启用
        /// </summary>
        [DependencyProperty]
        public bool DrawLineSegmentEnabled { get; set; }
        #endregion

        #region 绘制矩形启用 —— bool DrawRectangleEnabled
        /// <summary>
        /// 绘制矩形启用
        /// </summary>
        [DependencyProperty]
        public bool DrawRectangleEnabled { get; set; }
        #endregion

        #region 绘制圆形启用 —— bool DrawCircleEnabled
        /// <summary>
        /// 绘制圆形启用
        /// </summary>
        [DependencyProperty]
        public bool DrawCircleEnabled { get; set; }
        #endregion

        #region 绘制椭圆形启用 —— bool DrawEllipseEnabled
        /// <summary>
        /// 绘制椭圆形启用
        /// </summary>
        [DependencyProperty]
        public bool DrawEllipseEnabled { get; set; }
        #endregion

        #region 绘制折线启用 —— bool DrawPolylineEnabled
        /// <summary>
        /// 绘制折线启用
        /// </summary>
        [DependencyProperty]
        public bool DrawPolylineEnabled { get; set; }
        #endregion

        #region 绘制曲线启用 —— bool DrawCurveEnabled
        /// <summary>
        /// 绘制曲线启用
        /// </summary>
        [DependencyProperty]
        public bool DrawCurveEnabled { get; set; }
        #endregion

        #region 绘制多边形启用 —— bool DrawPolygonEnabled
        /// <summary>
        /// 绘制多边形启用
        /// </summary>
        [DependencyProperty]
        public bool DrawPolygonEnabled { get; set; }
        #endregion

        #region 绘制闭合曲线启用 —— bool DrawClosedCurveEnabled
        /// <summary>
        /// 绘制闭合曲线启用
        /// </summary>
        [DependencyProperty]
        public bool DrawClosedCurveEnabled { get; set; }
        #endregion

        #region 绘制立方体启用 —— bool DrawBoxEnabled
        /// <summary>
        /// 绘制立方体启用
        /// </summary>
        [DependencyProperty]
        public bool DrawBoxEnabled { get; set; }
        #endregion

        #region 绘制球体启用 —— bool DrawSphereEnabled
        /// <summary>
        /// 绘制球体启用
        /// </summary>
        [DependencyProperty]
        public bool DrawSphereEnabled { get; set; }
        #endregion

        #region 绘制圆柱体启用 —— bool DrawCylinderEnabled
        /// <summary>
        /// 绘制圆柱体启用
        /// </summary>
        [DependencyProperty]
        public bool DrawCylinderEnabled { get; set; }
        #endregion

        #region 绘制凸多面体启用 —— bool DrawConvexPolyhedronEnabled
        /// <summary>
        /// 绘制凸多面体启用
        /// </summary>
        [DependencyProperty]
        public bool DrawConvexPolyhedronEnabled { get; set; }
        #endregion 

        #endregion
    }
}
