using MedicalSharp.Engine.Managers;
using MedicalSharp.Engine.Resources;
using MedicalSharp.Primitives.Enums;
using MedicalSharp.Primitives.Managers;
using MedicalSharp.Primitives.Maths;
using MedicalSharp.Primitives.Models;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

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

        #region 应用矩形切割 —— void ApplyRectCut(float width, float height...
        /// <summary>
        /// 应用矩形切割
        /// </summary>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="center">中心位置</param>
        /// <param name="normal">法向量</param>
        /// <param name="uAxis">U轴</param>
        /// <param name="vAxis">V轴</param>
        /// <param name="localToWorld">局部到世界变换矩阵</param>
        /// <param name="cutMode">切割模式</param>
        /// <param name="markValue">标记值（1-255，0表示清除）</param>
        public void ApplyRectCut(float width, float height, Vector3 center, Vector3 normal, Vector3 uAxis, Vector3 vAxis, Matrix4 localToWorld, CutMode cutMode, byte markValue)
        {
            Matrix4 worldToLocal = localToWorld.Inverted();

            //立方体计算着色器
            ShaderProgram cutComputer = ComputerManager.RectCutComputer;

            //开启Shader程序
            cutComputer.Use();

            //绑定标记纹理为可读写
            this.MarkTexture.BindImageTexture(0, TextureAccess.ReadWrite);

            //设置矩形参数
            cutComputer.SetUniformFloat("u_RectHalfWidth", width / 2.0f);
            cutComputer.SetUniformFloat("u_RectHalfHeight", height / 2.0f);
            cutComputer.SetUniformVector3("u_RectCenter", center);
            cutComputer.SetUniformVector3("u_RectNormal", normal);
            cutComputer.SetUniformVector3("u_RectUAxis", uAxis);
            cutComputer.SetUniformVector3("u_RectVAxis", vAxis);
            cutComputer.SetUniformMatrix4("u_WorldToLocal", worldToLocal);

            //设置体积参数
            cutComputer.SetUniformVector3i("u_VolumeSize", this.VolumeData.Metadata.VolumeSize);
            cutComputer.SetUniformVector3("u_VolumeScale", this.VolumeData.Metadata.VolumeScale);

            //设置切割模式
            cutComputer.SetUniformInt("u_CutMode", (int)cutMode);

            //设置标记值
            cutComputer.SetUniformUInt("u_MarkValue", markValue);

            //调度执行
            ComputerManager.DispatchCompute3D(this.MarkTexture.Width, this.MarkTexture.Height, this.MarkTexture.Depth);

            //取消使用
            cutComputer.Unuse();
        }
        #endregion

        #region 应用立方体切割 —— void ApplyBoxCut(Vector3 boxLocalMin, Vector3 boxLocalMax...
        /// <summary>
        /// 应用立方体切割
        /// </summary>
        /// <param name="boxLocalMin">立方体最小点（局部空间）</param>
        /// <param name="boxLocalMax">立方体最大点（局部空间）</param>
        /// <param name="localToWorld">局部到世界变换矩阵</param>
        /// <param name="cutMode">切割模式</param>
        /// <param name="markValue">标记值（1-255，0表示清除）</param>
        public void ApplyBoxCut(Vector3 boxLocalMin, Vector3 boxLocalMax, Matrix4 localToWorld, CutMode cutMode, byte markValue)
        {
            #region # 验证

            if (boxLocalMin.X >= boxLocalMax.X || boxLocalMin.Y >= boxLocalMax.Y || boxLocalMin.Z >= boxLocalMax.Z)
            {
                throw new ArgumentException("立方体最小点必须小于最大点！");
            }

            #endregion

            Matrix4 worldToLocal = localToWorld.Inverted();

            //立方体计算着色器
            ShaderProgram cutComputer = ComputerManager.BoxCutComputer;

            //开启Shader程序
            cutComputer.Use();

            //绑定标记纹理为可读写
            this.MarkTexture.BindImageTexture(0, TextureAccess.ReadWrite);

            //设置立方体参数
            cutComputer.SetUniformVector3("u_BoxLocalMin", boxLocalMin);
            cutComputer.SetUniformVector3("u_BoxLocalMax", boxLocalMax);
            cutComputer.SetUniformMatrix4("u_WorldToLocal", worldToLocal);

            //设置体积参数
            cutComputer.SetUniformVector3i("u_VolumeSize", this.VolumeData.Metadata.VolumeSize);
            cutComputer.SetUniformVector3("u_VolumeScale", this.VolumeData.Metadata.VolumeScale);

            //设置切割模式
            cutComputer.SetUniformInt("u_CutMode", (int)cutMode);

            //设置标记值
            cutComputer.SetUniformUInt("u_MarkValue", markValue);

            //调度执行
            ComputerManager.DispatchCompute3D(this.MarkTexture.Width, this.MarkTexture.Height, this.MarkTexture.Depth);

            //取消使用
            cutComputer.Unuse();
        }
        #endregion

        #region 同步标记数据GPU->CPU —— void SyncMarkDataFromGpu()
        /// <summary>
        /// 同步标记数据GPU->CPU
        /// </summary>
        /// <remarks>将GPU标记纹理数据回读到CPU端的VolumeData.MarkData</remarks>
        public void SyncMarkDataFromGpu()
        {
            #region # 验证

            if (this.VolumeData.MarkData == IntPtr.Zero)
            {
                throw new InvalidOperationException("CPU标记数据未分配！");
            }
            if (!this.VolumeData.TryBeginGpuToCpu())
            {
                throw new InvalidOperationException($"标记数据正在同步中，当前状态: \"{this.VolumeData.SyncStatus}\"");
            }

            #endregion

            try
            {
                int width = this.MarkTexture.Width;
                int height = this.MarkTexture.Height;
                int depth = this.MarkTexture.Depth;
                int bufferSize = width * height * depth;

                //读取3D纹理到PBO
                using ReadPixelBuffer3D readBuffer = new ReadPixelBuffer3D(width, height, depth, PixelFormat.RedInteger, PixelType.UnsignedByte);
                readBuffer.ReadTexture3D(this.MarkTexture, true);

                //获取数据（阻塞等待）
                byte[] data = readBuffer.GetCpuBuffer();

                //复制到非托管内存
                if (data != null && data.Length == bufferSize)
                {
                    Marshal.Copy(data, 0, this.VolumeData.MarkData, bufferSize);
                }
            }
            finally
            {
                this.VolumeData.EndSync();
            }
        }
        #endregion

        #region 异步同步标记数据GPU->CPU —— async Task SyncMarkDataFromGpuAsync()
        /// <summary>
        /// 异步同步标记数据GPU->CPU
        /// </summary>
        public async Task SyncMarkDataFromGpuAsync()
        {
            #region # 验证

            if (this.VolumeData.MarkData == IntPtr.Zero)
            {
                throw new InvalidOperationException("CPU标记数据未分配！");
            }
            if (!this.VolumeData.TryBeginGpuToCpu())
            {
                throw new InvalidOperationException($"标记数据正在同步中，当前状态: \"{this.VolumeData.SyncStatus}\"");
            }

            #endregion

            try
            {
                int width = this.MarkTexture.Width;
                int height = this.MarkTexture.Height;
                int depth = this.MarkTexture.Depth;
                int bufferSize = width * height * depth;

                //异步读取3D纹理
                byte[] data = await Task.Run(() =>
                {
                    using ReadPixelBuffer3D readBuffer = new ReadPixelBuffer3D(width, height, depth, PixelFormat.RedInteger, PixelType.UnsignedByte);
                    readBuffer.ReadTexture3D(this.MarkTexture, true);
                    return readBuffer.GetCpuBuffer();
                });

                //复制到非托管内存
                if (data != null && data.Length == bufferSize)
                {
                    Marshal.Copy(data, 0, this.VolumeData.MarkData, bufferSize);
                }
            }
            finally
            {
                this.VolumeData.EndSync();
            }
        }
        #endregion

        #region 同步标记数据CPU->GPU —— void SyncMarkDataToGpu()
        /// <summary>
        /// 同步标记数据CPU->GPU
        /// </summary>
        /// <remarks>将CPU端VolumeData.MarkData上传到GPU标记纹理</remarks>
        public void SyncMarkDataToGpu()
        {
            #region # 验证

            if (this.VolumeData.MarkData == IntPtr.Zero)
            {
                throw new InvalidOperationException("CPU标记数据未分配！");
            }
            if (!this.VolumeData.TryBeginCpuToGpu())
            {
                throw new InvalidOperationException($"标记数据正在同步中，当前状态: \"{this.VolumeData.SyncStatus}\"");
            }

            #endregion

            try
            {
                int width = this.MarkTexture.Width;
                int height = this.MarkTexture.Height;
                int depth = this.MarkTexture.Depth;
                using WritePixelBuffer3D writeBuffer = WritePixelBuffer3D.CreateMark8(width, height, depth);

                //上传到PBO
                writeBuffer.UploadData(this.VolumeData.MarkData);

                //上传到纹理
                writeBuffer.UploadToTexture(this.MarkTexture, true);
            }
            finally
            {
                this.VolumeData.EndSync();
            }
        }
        #endregion

        #region 异步同步标记数据CPU->GPU —— async Task SyncMarkDataToGpuAsync()
        /// <summary>
        /// 异步同步标记数据CPU->GPU
        /// </summary>
        public async Task SyncMarkDataToGpuAsync()
        {
            #region # 验证

            if (this.VolumeData.MarkData == IntPtr.Zero)
            {
                throw new InvalidOperationException("CPU标记数据未分配！");
            }
            if (!this.VolumeData.TryBeginCpuToGpu())
            {
                throw new InvalidOperationException($"标记数据正在同步中，当前状态: \"{this.VolumeData.SyncStatus}\"");
            }

            #endregion

            try
            {
                int width = this.MarkTexture.Width;
                int height = this.MarkTexture.Height;
                int depth = this.MarkTexture.Depth;

                await Task.Run(() =>
                {
                    using WritePixelBuffer3D writeBuffer = WritePixelBuffer3D.CreateMark8(width, height, depth);

                    //上传到PBO
                    writeBuffer.UploadData(this.VolumeData.MarkData);

                    //上传到纹理
                    writeBuffer.UploadToTexture(this.MarkTexture, true);
                });
            }
            finally
            {
                this.VolumeData.EndSync();
            }
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
