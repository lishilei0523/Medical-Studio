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
        /// <param name="originalTexture">原始纹理</param>
        /// <param name="previewTexture">预览纹理</param>
        /// <param name="markTexture">标记纹理</param>
        /// <param name="volumeData">体积数据</param>
        public VolumeRenderable(Texture3D originalTexture, Texture3D previewTexture, Texture3D markTexture, VolumeData volumeData)
        {
            #region # 验证

            if (originalTexture == null)
            {
                throw new ArgumentNullException(nameof(originalTexture), "原始纹理不可为空！");
            }
            if (previewTexture == null)
            {
                throw new ArgumentNullException(nameof(previewTexture), "预览纹理不可为空！");
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

            this.OriginalTexture = originalTexture;
            this.PreviewTexture = previewTexture;
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

        #region 预览纹理 —— Texture3D PreviewTexture
        /// <summary>
        /// 预览纹理
        /// </summary>
        public Texture3D PreviewTexture { get; private set; }
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

        #region 重置预览纹理 —— void ResetPreviewTexture()
        /// <summary>
        /// 重置预览纹理
        /// </summary>
        /// <remarks>将预览纹理重置为原始纹理</remarks>
        public void ResetPreviewTexture()
        {
            #region # 验证

            if (this.OriginalTexture == null)
            {
                throw new InvalidOperationException("原始纹理未初始化！");
            }
            if (this.PreviewTexture == null)
            {
                throw new InvalidOperationException("预览纹理未初始化！");
            }
            if (this.PreviewTexture.Width != this.OriginalTexture.Width ||
                this.PreviewTexture.Height != this.OriginalTexture.Height ||
                this.PreviewTexture.Depth != this.OriginalTexture.Depth)
            {
                throw new InvalidOperationException("预览纹理与原始纹理尺寸不匹配！");
            }

            #endregion

            //确保之前的GPU操作完成
            GL.MemoryBarrier(MemoryBarrierFlags.TextureUpdateBarrierBit);

            //从原始纹理复制到预览纹理
            GL.CopyImageSubData(
                this.OriginalTexture.Id, ImageTarget.Texture3D, 0, 0, 0, 0,
                this.PreviewTexture.Id, ImageTarget.Texture3D, 0, 0, 0, 0,
                this.PreviewTexture.Width, this.PreviewTexture.Height, this.PreviewTexture.Depth);

            //确保复制完成后后续操作能读到数据
            GL.MemoryBarrier(MemoryBarrierFlags.TextureUpdateBarrierBit);

            //重置CPU端
            this.VolumeData.ResetPreviewData();
        }
        #endregion

        #region 重置标记纹理 —— void ResetMarkTexture()
        /// <summary>
        /// 重置标记纹理
        /// </summary>
        /// <remarks>将标记纹理全部设为0</remarks>
        public unsafe void ResetMarkTexture()
        {
            //清空标记纹理
            this.MarkTexture.Clear();

            //清空CPU端
            this.VolumeData.ResetMarkData();
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
