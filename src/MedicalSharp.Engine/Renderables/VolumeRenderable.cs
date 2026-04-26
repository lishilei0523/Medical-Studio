using MedicalSharp.Engine.Managers;
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
        /// <param name="volumeData">体积数据</param>
        public VolumeRenderable(Texture3D volumeTexture, Texture3D markTexture, VolumeData volumeData)
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
            if (volumeData == null)
            {
                throw new ArgumentNullException(nameof(volumeData), "体积数据不可为空！");
            }

            #endregion

            this.VolumeTexture = volumeTexture;
            this.MarkTexture = markTexture;
            this.VolumeData = volumeData;
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

        #region 体积数据 —— VolumeData VolumeData
        /// <summary>
        /// 体积数据
        /// </summary>
        public VolumeData VolumeData { get; private set; }
        #endregion

        #region 只读属性 - 体积元数据 —— VolumeMetadata VolumeMetadata
        /// <summary>
        /// 只读属性 - 体积元数据
        /// </summary>
        public VolumeMetadata VolumeMetadata
        {
            get => this.VolumeData.Metadata;
        }
        #endregion

        #endregion

        #region # 方法

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
            ShaderProgram boxComputer = ComputerManager.BoxComputer;

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
            ComputerManager.DispatchCompute(this.MarkTexture.Width, this.MarkTexture.Height, this.MarkTexture.Depth);

            //取消使用并释放
            boxComputer.Unuse();
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

        #endregion
    }
}
