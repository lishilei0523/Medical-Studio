using MedicalSharp.Engine.Resources;
using MedicalSharp.Primitives.Managers;
using MedicalSharp.Primitives.Maths;
using MedicalSharp.Primitives.Models;
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
        /// <param name="volumeMetadata">体积元数据</param>
        public VolumeRenderable(Texture3D volumeTexture, VolumeMetadata volumeMetadata)
        {
            #region # 验证

            if (volumeTexture == null)
            {
                throw new ArgumentNullException(nameof(volumeTexture), "体积纹理不可为空！");
            }
            if (volumeMetadata == null)
            {
                throw new ArgumentNullException(nameof(volumeMetadata), "体积元数据不可为空！");
            }

            #endregion

            this.VolumeTexture = volumeTexture;
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

        #region 体积元数据 —— VolumeMetadata VolumeMetadata
        /// <summary>
        /// 体积元数据
        /// </summary>
        public VolumeMetadata VolumeMetadata { get; private set; }
        #endregion

        #endregion

        #region # 方法

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
