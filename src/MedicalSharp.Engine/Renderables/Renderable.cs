using MedicalSharp.Engine.Resources;
using MedicalSharp.Engine.Shaders;
using MedicalSharp.Engine.ValueTypes;
using OpenTK.Mathematics;
using System;

namespace MedicalSharp.Engine.Renderables
{
    /// <summary>
    /// 渲染对象
    /// </summary>
    public abstract class Renderable
    {
        #region # 字段及构造器

        /// <summary>
        /// 创建渲染对象构造器
        /// </summary>
        protected Renderable()
        {
            this.Transform = new Transform();
        }

        #endregion

        #region # 属性

        #region 变换 —— Transform Transform
        /// <summary>
        /// 变换
        /// </summary>
        public Transform Transform { get; private set; }
        #endregion

        #region 只读属性 - 模型矩阵 —— Matrix4 ModelMatrix
        /// <summary>
        /// 只读属性 - 模型矩阵
        /// </summary>
        public Matrix4 ModelMatrix
        {
            get => this.Transform?.Matrix ?? Matrix4.Identity;
        }
        #endregion

        #endregion

        #region # 方法

        #region 设置变换 —— void SetTransform(Transform transform)
        /// <summary>
        /// 设置变换
        /// </summary>
        public void SetTransform(Transform transform)
        {
            if (transform == null)
            {
                throw new ArgumentNullException(nameof(transform), "变换不可为空！");
            }

            this.Transform = transform;
        }
        #endregion

        #region 渲染事件 —— internal virtual void OnRender(ShaderProgram program...
        /// <summary>
        /// 渲染事件
        /// </summary>
        /// <param name="program">Shader程序</param>
        /// <param name="renderContext">渲染上下文</param>
        protected internal virtual void OnRender(ShaderProgram program, RenderContext renderContext)
        {

        }
        #endregion

        #endregion
    }
}
