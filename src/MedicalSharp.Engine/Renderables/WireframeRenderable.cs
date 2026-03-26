using MedicalSharp.Engine.Resources;
using MedicalSharp.Engine.ValueTypes;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MedicalSharp.Engine.Renderables
{
    /// <summary>
    /// 线框渲染对象
    /// </summary>
    public class WireframeRenderable : Renderable, IDisposable
    {
        #region # 字段及构造器

        /// <summary>
        /// 无参构造器
        /// </summary>
        private WireframeRenderable()
        {
            this.Stroke = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
            this.StrokeThickness = 1.0f;
            this.Fill = new Vector4(1.0f, 0.0f, 0.0f, 0.1f);
        }

        /// <summary>
        /// 创建线框渲染对象构造器
        /// </summary>
        /// <param name="strokeBuffer">线框顶点缓冲区</param>
        /// <param name="fillBuffer">填充顶点缓冲区</param>
        public WireframeRenderable(VertexBuffer strokeBuffer, VertexBuffer fillBuffer)
            : this()
        {
            this.StrokeBuffer = strokeBuffer;
            this.FillBuffer = fillBuffer;
        }

        #endregion

        #region # 属性

        #region 线框顶点缓冲区 —— VertexBuffer StrokeBuffer
        /// <summary>
        /// 线框顶点缓冲区
        /// </summary>
        public VertexBuffer StrokeBuffer { get; private set; }
        #endregion

        #region 填充顶点缓冲区 —— VertexBuffer FillBuffer
        /// <summary>
        /// 填充顶点缓冲区
        /// </summary>
        public VertexBuffer FillBuffer { get; private set; }
        #endregion

        #region 线框颜色 —— Vector4 Stroke
        /// <summary>
        /// 线框颜色
        /// </summary>
        public Vector4 Stroke { get; private set; }
        #endregion

        #region 线框粗细 —— float StrokeThickness
        /// <summary>
        /// 线框粗细
        /// </summary>
        public float StrokeThickness { get; private set; }
        #endregion

        #region 填充颜色 —— Vector4 Fill
        /// <summary>
        /// 填充颜色
        /// </summary>
        public Vector4 Fill { get; private set; }
        #endregion

        #endregion

        #region # 方法

        //Public

        #region 设置颜色 —— void SetColor(Vector4 stroke, float strokeThickness...
        /// <summary>
        /// 设置颜色
        /// </summary>
        /// <param name="stroke">线框颜色</param>
        /// <param name="strokeThickness">线框粗细</param>
        /// <param name="fill">填充颜色</param>
        public void SetColor(Vector4 stroke, float strokeThickness, Vector4 fill)
        {
            this.Stroke = stroke;
            this.StrokeThickness = strokeThickness;
            this.Fill = fill;
        }
        #endregion

        #region 释放资源 —— void Dispose()
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            this.StrokeBuffer?.Dispose();
            this.FillBuffer?.Dispose();
        }
        #endregion 


        //Protected

        #region 计算局部包围盒 —— override BoundingBox CalculateLocalBoundingBox()
        /// <summary>
        /// 计算局部包围盒
        /// </summary>
        protected override BoundingBox CalculateLocalBoundingBox()
        {
            IEnumerable<Vector3> localPositions = this.StrokeBuffer.MeshGeometry.Vertices.Select(vertex => vertex.Position);
            BoundingBox boundingBox = BoundingBox.FromPoints([.. localPositions]);

            return boundingBox;
        }
        #endregion

        #endregion
    }
}
