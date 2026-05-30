using MedicalSharp.Engine.Resources;
using MedicalSharp.Primitives.Models;
using OpenTK.Mathematics;
using System;

namespace MedicalSharp.Engine.Base
{
    /// <summary>
    /// 渲染对象(2D)
    /// </summary>
    public abstract class Renderable2D : Renderable, IDisposable
    {
        #region # 字段及构造器

        /// <summary>
        /// 默认构造器
        /// </summary>
        protected Renderable2D()
        {
            //默认值
            this.Stroke = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
            this.StrokeThickness = 1.0f;
            this.Fill = new Vector4(1.0f, 0.0f, 0.0f, 0.1f);
        }

        #endregion

        #region # 属性

        #region 屏幕位置 —— Vector2 ScreenPosition
        /// <summary>
        /// 屏幕位置
        /// </summary>
        public Vector2 ScreenPosition { get; protected set; }
        #endregion

        #region 屏幕尺寸 —— Vector2 ScreenSize
        /// <summary>
        /// 屏幕尺寸
        /// </summary>
        public Vector2 ScreenSize { get; protected set; }
        #endregion

        #region 线框颜色 —— Vector4 Stroke
        /// <summary>
        /// 线框颜色
        /// </summary>
        public Vector4 Stroke { get; protected set; }
        #endregion

        #region 线框粗细 —— float StrokeThickness
        /// <summary>
        /// 线框粗细
        /// </summary>
        public float StrokeThickness { get; protected set; }
        #endregion

        #region 填充颜色 —— Vector4 Fill
        /// <summary>
        /// 填充颜色
        /// </summary>
        public Vector4 Fill { get; protected set; }
        #endregion

        #endregion

        #region # 属性

        #region 渲染 —— abstract void Render(ShaderProgram program, RenderContext2D context)
        /// <summary>
        /// 渲染
        /// </summary>
        /// <param name="program">Shader程序</param>
        /// <param name="context">渲染上下文</param>
        public abstract void Render(ShaderProgram program, RenderContext2D context);
        #endregion

        #region 释放资源 —— abstract void Dispose()
        /// <summary>
        /// 释放资源
        /// </summary>
        public abstract void Dispose();
        #endregion  

        #endregion
    }
}
