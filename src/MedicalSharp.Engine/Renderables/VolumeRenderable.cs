using MedicalSharp.Engine.Resources;
using MedicalSharp.Primitives.Managers;
using MedicalSharp.Primitives.Maths;
using MedicalSharp.Primitives.Models;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

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
        /// <param name="sourceTexture">体积纹理</param>
        /// <param name="markTexture">标记纹理</param>
        /// <param name="volumeData">体积数据</param>
        public VolumeRenderable(Texture3D sourceTexture, Texture3D markTexture, VolumeData volumeData)
        {
            #region # 验证

            if (sourceTexture == null)
            {
                throw new ArgumentNullException(nameof(sourceTexture), "体积纹理不可为空！");
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

            this.OriginalTexture = sourceTexture;
            this.MarkTexture = markTexture;
            this.VolumeData = volumeData;
        }

        #endregion

        #region # 属性

        #region 原始纹理 —— Texture3D OriginalTexture
        /// <summary>
        /// 原始纹理
        /// </summary>
        public Texture3D OriginalTexture { get; private set; }
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

        #region 重置标记纹理 —— void ResetMarkTexture()
        /// <summary>
        /// 重置标记纹理
        /// </summary>
        /// <remarks>将标记纹理全部设为0</remarks>
        public unsafe void ResetMarkTexture()
        {
            //清空标记纹理
            this.MarkTexture.Clear();

            //清空CPU端内存
            if (this.VolumeData.MarkData != IntPtr.Zero)
            {
                int size = this.VolumeMetadata.VolumeSize.X * this.VolumeMetadata.VolumeSize.Y * this.VolumeMetadata.VolumeSize.Z;
                NativeMemory.Clear(this.VolumeData.MarkData.ToPointer(), (nuint)size);
            }
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
