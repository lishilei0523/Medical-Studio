using Avalonia;
using Avalonia.Media;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Controls.Interfaces;
using MedicalSharp.Engine.Renderables;
using MedicalSharp.Primitives.Enums;
using MedicalSharp.Primitives.Interfaces;
using OpenTK.Mathematics;
using System;

namespace MedicalSharp.Controls.Visual3Ds
{
    /// <summary>
    /// 文本3D元素
    /// </summary>
    public class TextVisual3D : ShapeVisual3D, IVisual2DIn3D, ITranslatable3D
    {
        #region # 字段及构造器

        /// <summary>
        /// 文本内容依赖属性
        /// </summary>
        public static readonly StyledProperty<string> TextProperty;

        /// <summary>
        /// 位置依赖属性
        /// </summary>
        public static readonly StyledProperty<Vector3D> PositionProperty;

        /// <summary>
        /// 字体大小依赖属性
        /// </summary>
        public static readonly StyledProperty<float> FontSizeProperty;

        /// <summary>
        /// 文本颜色依赖属性
        /// </summary>
        public static readonly StyledProperty<Color> ColorProperty;

        /// <summary>
        /// 渲染模式依赖属性
        /// </summary>
        public static readonly StyledProperty<TextRenderMode> RenderModeProperty;

        /// <summary>
        /// 法向量依赖属性
        /// </summary>
        public static readonly StyledProperty<Vector3D> NormalProperty;

        /// <summary>
        /// 是否锁定Y轴依赖属性
        /// </summary>
        public static readonly StyledProperty<bool> LockYAxisProperty;

        /// <summary>
        /// 静态构造器
        /// </summary>
        static TextVisual3D()
        {
            TextProperty = AvaloniaProperty.Register<TextVisual3D, string>(nameof(Text));
            PositionProperty = AvaloniaProperty.Register<TextVisual3D, Vector3D>(nameof(Position), new Vector3D(0, 0, 0));
            FontSizeProperty = AvaloniaProperty.Register<TextVisual3D, float>(nameof(FontSize), 14.0f);
            ColorProperty = AvaloniaProperty.Register<TextVisual3D, Color>(nameof(Color), Colors.White);
            RenderModeProperty = AvaloniaProperty.Register<TextVisual3D, TextRenderMode>(nameof(RenderMode), TextRenderMode.Billboard);
            NormalProperty = AvaloniaProperty.Register<TextVisual3D, Vector3D>(nameof(Normal), new Vector3D(0, 1, 0));
            LockYAxisProperty = AvaloniaProperty.Register<TextVisual3D, bool>(nameof(LockYAxis), true);
        }


        /// <summary>
        /// 默认构造器
        /// </summary>
        public TextVisual3D()
        {

        }

        #endregion

        #region # 属性

        #region U轴 —— Vector3D UAxis
        /// <summary>
        /// U轴
        /// </summary>
        public Vector3D UAxis { get; private set; }
        #endregion

        #region V轴 —— Vector3D VAxis
        /// <summary>
        /// V轴
        /// </summary>
        public Vector3D VAxis { get; private set; }
        #endregion

        #region 依赖属性 - 文本内容 —— string Text
        /// <summary>
        /// 依赖属性 - 文本内容
        /// </summary>
        public string Text
        {
            get => this.GetValue(TextProperty);
            set => this.SetValue(TextProperty, value);
        }
        #endregion

        #region 依赖属性 - 位置 —— Vector3D Position
        /// <summary>
        /// 依赖属性 - 位置
        /// </summary>
        public Vector3D Position
        {
            get => this.GetValue(PositionProperty);
            set => this.SetValue(PositionProperty, value);
        }
        #endregion

        #region 依赖属性 - 字体大小 —— float FontSize
        /// <summary>
        /// 依赖属性 - 字体大小
        /// </summary>
        public float FontSize
        {
            get => this.GetValue(FontSizeProperty);
            set => this.SetValue(FontSizeProperty, value);
        }
        #endregion

        #region 依赖属性 - 文本颜色 —— Color Color
        /// <summary>
        /// 依赖属性 - 文本颜色
        /// </summary>
        public Color Color
        {
            get => this.GetValue(ColorProperty);
            set => this.SetValue(ColorProperty, value);
        }
        #endregion

        #region 依赖属性 - 渲染模式 —— TextRenderMode RenderMode
        /// <summary>
        /// 依赖属性 - 渲染模式
        /// </summary>
        public TextRenderMode RenderMode
        {
            get => this.GetValue(RenderModeProperty);
            set => this.SetValue(RenderModeProperty, value);
        }
        #endregion

        #region 依赖属性 - 法向量 —— Vector3D Normal
        /// <summary>
        /// 依赖属性 - 法向量
        /// </summary>
        public Vector3D Normal
        {
            get => this.GetValue(NormalProperty);
            set => this.SetValue(NormalProperty, value);
        }
        #endregion

        #region 依赖属性 - 是否锁定Y轴 —— bool LockYAxis
        /// <summary>
        /// 依赖属性 - 是否锁定Y轴
        /// </summary>
        public bool LockYAxis
        {
            get => this.GetValue(LockYAxisProperty);
            set => this.SetValue(LockYAxisProperty, value);
        }
        #endregion

        #region 只读属性 - 平面上一点 —— Vector3D PointOnPlane
        /// <summary>
        /// 只读属性 - 平面上一点
        /// </summary>
        public Vector3D PointOnPlane
        {
            get => this.Position;
        }
        #endregion

        #endregion

        #region # 方法

        #region 确保渲染对象 —— override void EnsureRenderable()
        /// <summary>
        /// 确保渲染对象
        /// </summary>
        internal override void EnsureRenderable()
        {
            if (this.Renderable == null)
            {
                TextRenderable renderable = this.RenderMode == TextRenderMode.Fixed
                    ? new TextRenderable(this.Text, this.Position.ToVector3(), this.FontSize, this.Color.ToVector4(), this.Normal.ToVector3())
                    : new TextRenderable(this.Text, this.Position.ToVector3(), this.FontSize, this.Color.ToVector4(), this.LockYAxis);
                this.Renderable = renderable;
            }
            else
            {
                TextRenderable renderable = (TextRenderable)this.Renderable;
                renderable.Update(this.Text, this.FontSize);
                renderable.SetColor(this.Color.ToVector4());
            }

            this.BuildBasis();
        }
        #endregion

        #region 构建UV正交基 —— void BuildBasis()
        /// <summary>
        /// 构建UV正交基
        /// </summary>
        private void BuildBasis()
        {
            Vector3 normal = this.Normal.ToVector3();

            //法向量接近Z轴
            if (Math.Abs(Vector3.Dot(normal, Vector3.UnitZ)) > 0.99f)
            {
                this.UAxis = Vector3.UnitX.ToVector3();
                this.VAxis = Vector3.UnitY.ToVector3();
            }
            //法向量接近Y轴
            else if (Math.Abs(Vector3.Dot(normal, Vector3.UnitY)) > 0.99f)
            {
                this.UAxis = Vector3.UnitX.ToVector3();
                this.VAxis = Vector3.UnitZ.ToVector3();
            }
            //法向量接近X轴
            else if (Math.Abs(Vector3.Dot(normal, Vector3.UnitX)) > 0.99f)
            {
                this.UAxis = Vector3.UnitY.ToVector3();
                this.VAxis = Vector3.UnitZ.ToVector3();
            }
            else
            {
                //如果法线被旋转过，重新构造正交基（保证U在XY平面内优先）
                this.UAxis = Vector3.Normalize(Vector3.Cross(Vector3.UnitZ, normal)).ToVector3();
                this.VAxis = Vector3.Normalize(Vector3.Cross(normal, this.UAxis.ToVector3())).ToVector3();
            }
        }
        #endregion

        #endregion
    }
}
