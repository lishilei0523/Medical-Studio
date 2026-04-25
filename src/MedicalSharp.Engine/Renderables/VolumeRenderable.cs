using MedicalSharp.Engine.Resources;
using MedicalSharp.Primitives.Managers;
using MedicalSharp.Primitives.Maths;
using MedicalSharp.Primitives.Models;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MedicalSharp.Engine.Renderables
{
    /// <summary>
    /// 体积渲染对象
    /// </summary>
    public class VolumeRenderable : Renderable
    {
        #region # 字段及构造器

        /// <summary>
        /// 创建体积渲染对象构造器
        /// </summary>
        /// <param name="volumeTexture">体积纹理</param>
        /// <param name="markTexture">标记纹理</param>
        /// <param name="volumeMetadata">体积元数据</param>
        public VolumeRenderable(Texture3D volumeTexture, Texture3D markTexture, VolumeMetadata volumeMetadata)
        {
            #region # 验证

            if (volumeTexture == null)
            {
                throw new ArgumentNullException(nameof(volumeTexture), "体积纹理不可为空！");
            }
            if (markTexture == null)
            {
                throw new ArgumentNullException(nameof(markTexture), "标记纹理不可为空！");
            }
            if (volumeMetadata == null)
            {
                throw new ArgumentNullException(nameof(volumeMetadata), "体积元数据不可为空！");
            }

            #endregion

            this.VolumeTexture = volumeTexture;
            this.MarkTexture = markTexture;
            this.VolumeMetadata = volumeMetadata;
        }

        #endregion

        #region # 属性

        #region 体积纹理 —— Texture3D VolumeTexture
        /// <summary>
        /// 体积纹理
        /// </summary>
        public Texture3D VolumeTexture { get; private set; }
        #endregion

        #region 标记纹理 —— Texture3D MarkTexture
        /// <summary>
        /// 标记纹理
        /// </summary>
        public Texture3D MarkTexture { get; private set; }
        #endregion

        #region 体积元数据 —— VolumeMetadata VolumeMetadata
        /// <summary>
        /// 体积元数据
        /// </summary>
        public VolumeMetadata VolumeMetadata { get; private set; }
        #endregion

        #endregion

        #region # 方法

        //Public

        #region 应用立方体ROI —— void ApplyBoxROI(Vector3 boxLocalMin, Vector3 boxLocalMax...
        /// <summary>
        /// 应用立方体ROI
        /// </summary>
        /// <param name="boxLocalMin">立方体最小点（局部空间）</param>
        /// <param name="boxLocalMax">立方体最大点（局部空间）</param>
        /// <param name="localToWorld">局部到世界变换矩阵</param>
        /// <param name="markValue">标记值（1-255，0表示清除）</param>
        public void ApplyBoxROI(Vector3 boxLocalMin, Vector3 boxLocalMax, Matrix4 localToWorld, byte markValue)
        {
            #region # 验证

            if (boxLocalMin.X >= boxLocalMax.X || boxLocalMin.Y >= boxLocalMax.Y || boxLocalMin.Z >= boxLocalMax.Z)
            {
                throw new ArgumentException("立方体最小点必须小于最大点！");
            }

            #endregion

            Matrix4 worldToLocal = localToWorld.Inverted();

            //初始化计算着色器
            ShaderProgram boxComputer = CreateBoxComputer();

            //开启Shader程序
            boxComputer.Use();

            //绑定标记纹理为可读写
            GL.BindImageTexture(0, this.MarkTexture.Id, 0, true, 0, TextureAccess.ReadWrite, SizedInternalFormat.R8ui);

            //设置立方体参数
            boxComputer.SetUniformVector3("u_BoxLocalMin", boxLocalMin);
            boxComputer.SetUniformVector3("u_BoxLocalMax", boxLocalMax);
            boxComputer.SetUniformMatrix4("u_WorldToLocal", worldToLocal);

            //设置纹理尺寸
            Vector3i textureSize = new Vector3i(this.MarkTexture.Width, this.MarkTexture.Height, this.MarkTexture.Depth);
            boxComputer.SetUniformVector3i("u_TextureSize", textureSize);

            //设置标记值
            boxComputer.SetUniformUInt("u_MarkValue", markValue);

            //调度执行
            this.DispatchCompute();

            //等待GPU完成
            GL.Finish();

            //取消使用并释放
            boxComputer.Unuse();
            boxComputer.Dispose();
        }
        #endregion

        #region 重置标记纹理 —— void ResetMarkTexture()
        /// <summary>
        /// 重置标记纹理
        /// </summary>
        /// <remarks>将标记纹理全部设为0</remarks>
        public void ResetMarkTexture()
        {
            //使用glClearTexImage清除整个纹理（OpenGL 4.4+）
            GL.ClearTexImage(this.MarkTexture.Id, 0, PixelFormat.RedInteger, PixelType.UnsignedByte, IntPtr.Zero);
        }
        #endregion


        //Protected & Private

        #region 计算包围盒 —— override BoundingBox CalculateBoundingBox()
        /// <summary>
        /// 计算包围盒
        /// </summary>
        protected override BoundingBox CalculateBoundingBox()
        {
            Matrix4 scaleMatrix = Matrix4.CreateScale(this.VolumeMetadata.VolumeScale);
            IEnumerable<Vector3> originalPositions = ResourceManager.UnitCube.Vertices.Select(x => x.Position);
            IEnumerable<Vector3> localPositions = originalPositions.Select(position => Vector3.TransformPosition(position, scaleMatrix));
            BoundingBox boundingBox = BoundingBox.FromPoints([.. localPositions]);

            return boundingBox;
        }
        #endregion

        #region 调度计算着色器 —— void DispatchCompute()
        /// <summary>
        /// 调度计算着色器
        /// </summary>
        private void DispatchCompute()
        {
            //计算工作组数量（每组8×8×8线程）
            int groupsX = (int)MathF.Ceiling(this.MarkTexture.Width / 8.0f);
            int groupsY = (int)MathF.Ceiling(this.MarkTexture.Height / 8.0f);
            int groupsZ = (int)MathF.Ceiling(this.MarkTexture.Depth / 8.0f);

            //调度执行计算着色器
            GL.DispatchCompute(groupsX, groupsY, groupsZ);

            //内存屏障：确保计算完成后渲染能读到新数据
            GL.MemoryBarrier(MemoryBarrierFlags.ShaderImageAccessBarrierBit);
        }
        #endregion


        //Static

        #region 创建立方体计算着色器 —— static ShaderProgram CreateBoxComputer()
        /// <summary>
        /// 创建立方体计算着色器
        /// </summary>
        private static ShaderProgram CreateBoxComputer()
        {
            ShaderProgram program = new ShaderProgram();
            program.ReadComputeShaderFromFile("Resources/GLSLs/box_roi.comp");
            program.BuildCompute();

            return program;
        }
        #endregion

        #endregion
    }
}
