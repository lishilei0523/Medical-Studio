using MedicalSharp.Engine.Resources;
using MedicalSharp.Primitives.Builders;
using MedicalSharp.Primitives.Maths;
using MedicalSharp.Primitives.Models;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Collections.Generic;
using System.Linq;

namespace MedicalSharp.Engine.Renderables
{
    /// <summary>
    /// 点云渲染对象
    /// </summary>
    public class PointCloudRenderable : ShapeRenderable
    {
        #region # 字段及构造器

        /// <summary>
        /// 顶点缓冲区
        /// </summary>
        private VertexBuffer _vertexBuffer;

        /// <summary>
        /// 默认构造器
        /// </summary>
        private PointCloudRenderable()
        {
            //默认值
            this.Fill = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
            this.PointSize = 2.0f;
        }

        /// <summary>
        /// 创建点云渲染对象构造器
        /// </summary>
        /// <param name="positions">位置列表</param>
        public PointCloudRenderable(IList<Vector3> positions)
            : this()
        {
            this.Positions = positions;

            //初始化缓冲区
            MeshGeometry pointCloudGeometry = MeshFactory.CreatePoints(positions);
            this._vertexBuffer = new VertexBuffer(pointCloudGeometry);
            this._vertexBuffer.Setup();
        }

        #endregion

        #region # 属性

        #region 位置列表 —— IList<Vector3> Positions
        /// <summary>
        /// 位置列表
        /// </summary>
        public IList<Vector3> Positions { get; private set; }
        #endregion

        #region 填充颜色 —— Vector4 Fill
        /// <summary>
        /// 填充颜色
        /// </summary>
        public Vector4 Fill { get; private set; }
        #endregion

        #region 点尺寸 —— float PointSize
        /// <summary>
        /// 点尺寸
        /// </summary>
        /// <remarks>像素尺寸</remarks>
        public float PointSize { get; private set; }
        #endregion

        #region 只读属性 - 顶点缓冲区 —— VertexBuffer VertexBuffer
        /// <summary>
        /// 只读属性 - 顶点缓冲区
        /// </summary>
        internal VertexBuffer VertexBuffer
        {
            get => this._vertexBuffer;
        }
        #endregion

        #endregion

        #region # 方法

        //Public

        #region 更新点云渲染对象 —— void Update(IList<Vector3> positions)
        /// <summary>
        /// 更新点云渲染对象
        /// </summary>
        /// <param name="positions">位置列表</param>
        public void Update(IList<Vector3> positions)
        {
            #region # 验证

            if (this.Positions.Equals(positions))
            {
                return;
            }

            #endregion

            this.Positions = positions;

            //先释放旧的
            this._vertexBuffer.Dispose();

            MeshGeometry pointGeometry = MeshFactory.CreatePoints(positions);
            this._vertexBuffer = new VertexBuffer(pointGeometry);
            this._vertexBuffer.Setup();

            //标记包围盒/包围球为脏
            base.InvalidateBoundings();
        }
        #endregion

        #region 设置填充 —— void SetFill(Vector4 fill, float pointSize)
        /// <summary>
        /// 设置填充
        /// </summary>
        /// <param name="fill">填充颜色</param>
        /// <param name="pointSize">点尺寸</param>
        public void SetFill(Vector4 fill, float pointSize)
        {
            this.Fill = fill;
            this.PointSize = pointSize;
        }
        #endregion

        #region 渲染 —— override void Render(ShaderProgram program)
        /// <summary>
        /// 渲染
        /// </summary>
        /// <param name="program">Shader程序</param>
        public override void Render(ShaderProgram program)
        {
            //绘制点模型
            GL.PointSize(this.PointSize);
            program.SetUniformVector4("u_Color", this.Fill);
            this.VertexBuffer.Draw(PrimitiveType.Points);
        }
        #endregion

        #region 检测射线相交 —— override bool IntersectsRay(Ray ray, out float distance...
        /// <summary>
        /// 检测射线相交
        /// </summary>
        /// <param name="ray">射线（世界空间）</param>
        /// <param name="distance">相交距离</param>
        /// <param name="hitPoint">命中点坐标</param>
        /// <param name="hitNormal">命中点法向量</param>
        /// <param name="hitTriangleIndex">命中三角形索引</param>
        /// <returns>是否相交</returns>
        public override bool IntersectsRay(Ray ray, out float distance, out Vector3 hitPoint, out Vector3 hitNormal, out int hitTriangleIndex)
        {
            distance = float.MaxValue;
            hitPoint = Vector3.Zero;
            hitNormal = Vector3.Zero;
            hitTriangleIndex = -1;

            //快速剔除：先检测包围盒
            if (!this.BoundingBox.Intersects(ray, out float boxDistance))
            {
                return false;
            }

            //精确检测
            IDictionary<BoundingSphere, float> hitPoints = new Dictionary<BoundingSphere, float>();
            foreach (Vector3 position in this.Positions)
            {
                BoundingSphere sphere = new BoundingSphere(position, 0.02f);
                if (sphere.Intersects(ray, out float pointDistance))
                {
                    hitPoints.Add(sphere, pointDistance);
                }
            }
            if (hitPoints.Any())
            {
                KeyValuePair<BoundingSphere, float> hit = hitPoints.MinBy(x => x.Value);
                distance = hit.Value;
                hitPoint = hit.Key.Center;
                hitTriangleIndex = this.Positions.IndexOf(hitPoint);

                return true;
            }

            return false;
        }
        #endregion

        #region 释放资源 —— override void Dispose()
        /// <summary>
        /// 释放资源
        /// </summary>
        public override void Dispose()
        {
            if (this._disposed)
            {
                return;
            }

            this._vertexBuffer.Dispose();

            this._disposed = true;
        }
        #endregion


        //Protected

        #region 计算包围盒 —— override BoundingBox CalculateBoundingBox()
        /// <summary>
        /// 计算包围盒
        /// </summary>
        protected override BoundingBox CalculateBoundingBox()
        {
            BoundingBox boundingBox = BoundingBox.FromPoints(this.Positions);

            return boundingBox;
        }
        #endregion

        #endregion
    }
}
