using MedicalSharp.Engine.Resources;
using MedicalSharp.Primitives.Builders;
using MedicalSharp.Primitives.Maths;
using MedicalSharp.Primitives.Models;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace MedicalSharp.Engine.Renderables
{
    /// <summary>
    /// 坐标轴渲染对象
    /// </summary>
    public class AxisRenderable : ShapeRenderable
    {
        #region # 字段及构造器

        /// <summary>
        /// X轴轴线缓冲区
        /// </summary>
        private VertexBuffer _xShaftBuffer;

        /// <summary>
        /// Y轴轴线缓冲区
        /// </summary>
        private VertexBuffer _yShaftBuffer;

        /// <summary>
        /// Z轴轴线缓冲区
        /// </summary>
        private VertexBuffer _zShaftBuffer;

        /// <summary>
        /// X轴箭头缓冲区
        /// </summary>
        private VertexBuffer _xArrowBuffer;

        /// <summary>
        /// Y轴箭头缓冲区
        /// </summary>
        private VertexBuffer _yArrowBuffer;

        /// <summary>
        /// Z轴箭头缓冲区
        /// </summary>
        private VertexBuffer _zArrowBuffer;

        /// <summary>
        /// 创建坐标轴渲染对象
        /// </summary>
        /// <param name="position">位置</param>
        /// <param name="shaftLength">轴线长度</param>
        public AxisRenderable(Vector3 position, float shaftLength)
        {
            this.ShaftLength = shaftLength;

            //计算派生参数
            this.ArrowLength = shaftLength * 0.2f;
            this.ShaftRadius = shaftLength * 0.03f;
            this.ArrowRadius = shaftLength * 0.06f;

            //颜色配置
            this.XColor = new Vector4(1.0f, 0.2f, 0.2f, 1.0f);
            this.YColor = new Vector4(0.2f, 1.0f, 0.2f, 1.0f);
            this.ZColor = new Vector4(0.2f, 0.2f, 1.0f, 1.0f);

            //设置位置
            this.Transform.SetPosition(position);

            //创建几何体
            this.CreateShafts();
            this.CreateArrows();
        }

        #endregion

        #region # 属性

        #region 轴线长度 —— float ShaftLength
        /// <summary>
        /// 轴线长度
        /// </summary>
        public float ShaftLength { get; private set; }
        #endregion

        #region 轴线半径 —— float ShaftRadius
        /// <summary>
        /// 轴线半径
        /// </summary>
        public float ShaftRadius { get; private set; }
        #endregion

        #region 箭头长度 —— float ArrowLength
        /// <summary>
        /// 箭头长度
        /// </summary>
        public float ArrowLength { get; private set; }
        #endregion

        #region 箭头半径 —— float ArrowRadius
        /// <summary>
        /// 箭头半径
        /// </summary>
        public float ArrowRadius { get; private set; }
        #endregion

        #region X轴颜色 —— Vector4 XColor
        /// <summary>
        /// X轴颜色
        /// </summary>
        /// <remarks>红色</remarks>
        public Vector4 XColor { get; private set; }
        #endregion

        #region Y轴颜色 —— Vector4 YColor
        /// <summary>
        /// Y轴颜色
        /// </summary>
        /// <remarks>绿色</remarks>
        public Vector4 YColor { get; private set; }
        #endregion

        #region Z轴颜色 —— Vector4 ZColor 
        /// <summary>
        /// Z轴颜色
        /// </summary>
        /// <remarks>蓝色</remarks>
        public Vector4 ZColor { get; private set; }
        #endregion

        #endregion

        #region # 方法

        //Public

        #region 渲染 —— override void Render(ShaderProgram program, RenderContext context)
        /// <summary>
        /// 渲染
        /// </summary>
        /// <param name="program">Shader程序</param>
        /// <param name="context">渲染上下文</param>
        public override void Render(ShaderProgram program, RenderContext context)
        {
            //渲染X轴
            program.SetUniformInt("u_HasTexture", 0);
            program.SetUniformVector4("u_Color", this.XColor);
            this._xShaftBuffer.Draw(context.GlContext, PrimitiveType.Triangles);
            this._xArrowBuffer.Draw(context.GlContext, PrimitiveType.Triangles);

            //渲染Y轴
            program.SetUniformInt("u_HasTexture", 0);
            program.SetUniformVector4("u_Color", this.YColor);
            this._yShaftBuffer.Draw(context.GlContext, PrimitiveType.Triangles);
            this._yArrowBuffer.Draw(context.GlContext, PrimitiveType.Triangles);

            //渲染Z轴
            program.SetUniformInt("u_HasTexture", 0);
            program.SetUniformVector4("u_Color", this.ZColor);
            this._zShaftBuffer.Draw(context.GlContext, PrimitiveType.Triangles);
            this._zArrowBuffer.Draw(context.GlContext, PrimitiveType.Triangles);
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

            this._xShaftBuffer?.Dispose();
            this._yShaftBuffer?.Dispose();
            this._zShaftBuffer?.Dispose();
            this._xArrowBuffer?.Dispose();
            this._yArrowBuffer?.Dispose();
            this._zArrowBuffer?.Dispose();
            this._disposed = true;
        }
        #endregion


        //Protected && Private

        #region 计算包围盒 —— override BoundingBox CalculateBoundingBox()
        /// <summary>
        /// 计算包围盒
        /// </summary>
        protected override BoundingBox CalculateBoundingBox()
        {
            float half = (this.ShaftLength + this.ArrowLength) * 0.5f;
            BoundingBox boundingBox = new BoundingBox(new Vector3(-half, -half, -half), new Vector3(half, half, half));

            return boundingBox;
        }
        #endregion

        #region 创建轴线几何体 —— void CreateShafts()
        /// <summary>
        /// 创建轴线几何体
        /// </summary>
        private void CreateShafts()
        {
            float shaftLength = this.ShaftLength - this.ArrowLength;
            float halfShaftLength = shaftLength * 0.5f;

            //X轴轴线（沿X方向，需要旋转）
            MeshGeometry xShaftMesh = MeshFactory.CreateCylinder(this.ShaftRadius, shaftLength, Vector3.Zero);
            Matrix4 xShaftRotation = Matrix4.CreateRotationY(MathHelper.PiOver2);
            Matrix4 xShaftTranslation = Matrix4.CreateTranslation(new Vector3(halfShaftLength, 0, 0));
            MeshFactory.Transform(xShaftMesh, xShaftRotation * xShaftTranslation);
            this._xShaftBuffer = new VertexBuffer(xShaftMesh);

            //Y轴轴线（沿Y方向，需要旋转）
            MeshGeometry yShaftMesh = MeshFactory.CreateCylinder(this.ShaftRadius, shaftLength, Vector3.Zero);
            Matrix4 yShaftRotation = Matrix4.CreateRotationX(MathHelper.PiOver2);
            Matrix4 yShaftTranslation = Matrix4.CreateTranslation(new Vector3(0, halfShaftLength, 0));
            MeshFactory.Transform(yShaftMesh, yShaftRotation * yShaftTranslation);
            this._yShaftBuffer = new VertexBuffer(yShaftMesh);

            //Z轴轴线（沿Z方向）
            MeshGeometry zShaftMesh = MeshFactory.CreateCylinder(this.ShaftRadius, shaftLength, Vector3.Zero);
            Matrix4 zShaftTranslation = Matrix4.CreateTranslation(new Vector3(0, 0, halfShaftLength));
            MeshFactory.Transform(zShaftMesh, zShaftTranslation);
            this._zShaftBuffer = new VertexBuffer(zShaftMesh);
        }
        #endregion

        #region 创建箭头几何体 —— void CreateArrows()
        /// <summary>
        /// 创建箭头几何体
        /// </summary>
        private void CreateArrows()
        {
            float shaftLength = this.ShaftLength - this.ArrowLength;

            //X轴箭头（沿X方向，需要旋转）
            MeshGeometry xArrowMesh = MeshFactory.CreateCone(this.ArrowRadius, this.ArrowLength, Vector3.Zero);
            Matrix4 xArrowRotation = Matrix4.CreateRotationY(MathHelper.PiOver2);
            Matrix4 xArrowTranslation = Matrix4.CreateTranslation(new Vector3(shaftLength, 0, 0));
            MeshFactory.Transform(xArrowMesh, xArrowRotation * xArrowTranslation);
            this._xArrowBuffer = new VertexBuffer(xArrowMesh);

            //Y轴箭头（沿Y方向，需要旋转）
            MeshGeometry yArrowMesh = MeshFactory.CreateCone(this.ArrowRadius, this.ArrowLength, Vector3.Zero);
            Matrix4 yArrowRotation = Matrix4.CreateRotationX(-MathHelper.PiOver2);
            Matrix4 yArrowTranslation = Matrix4.CreateTranslation(new Vector3(0, shaftLength, 0));
            MeshFactory.Transform(yArrowMesh, yArrowRotation * yArrowTranslation);
            this._yArrowBuffer = new VertexBuffer(yArrowMesh);

            //Z轴箭头（沿Z方向）
            MeshGeometry zArrowMesh = MeshFactory.CreateCone(this.ArrowRadius, this.ArrowLength, Vector3.Zero);
            Matrix4 zArrowTranslation = Matrix4.CreateTranslation(new Vector3(0, 0, shaftLength));
            MeshFactory.Transform(zArrowMesh, zArrowTranslation);
            this._zArrowBuffer = new VertexBuffer(zArrowMesh);
        }
        #endregion

        #endregion
    }
}
