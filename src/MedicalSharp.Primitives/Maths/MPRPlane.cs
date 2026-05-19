using MedicalSharp.Primitives.Enums;
using MedicalSharp.Primitives.Managers;
using MedicalSharp.Primitives.Models;
using MedicalSharp.Primitives.Models.Arguments;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MedicalSharp.Primitives.Maths
{
    /// <summary>
    /// MPR平面
    /// </summary>
    public class MPRPlane
    {
        #region # 字段及构造器

        /// <summary>
        /// 平面变化事件
        /// </summary>
        public event EventHandler<MPRPlaneChangedEventArgs> PlaneChangedEvent;

        /// <summary>
        /// 首次变化
        /// </summary>
        private bool _firstChanged;

        /// <summary>
        /// 上次切片偏移量
        /// </summary>
        private float _previousSliceOffset;

        /// <summary>
        /// 最小投影（斜切平面使用）
        /// </summary>
        private float _minProjection;

        /// <summary>
        /// 最大投影（斜切平面使用）
        /// </summary>
        private float _maxProjection;

        /// <summary>
        /// 创建MPR平面构造器
        /// </summary>
        /// <param name="volumeMetadata">体积元数据</param>
        private MPRPlane(VolumeMetadata volumeMetadata)
        {
            this.VolumeMetadata = volumeMetadata;

            //默认值
            this._firstChanged = true;
        }

        #endregion

        #region # 属性

        #region 体积元数据 —— VolumeMetadata VolumeMetadata
        /// <summary>
        /// 体积元数据
        /// </summary>
        public VolumeMetadata VolumeMetadata { get; private set; }
        #endregion

        #region 平面中心 —— Vector3 Center
        /// <summary>
        /// 平面中心
        /// </summary>
        public Vector3 Center { get; private set; }
        #endregion

        #region U轴 —— Vector3 UAxis
        /// <summary>
        /// U轴
        /// </summary>
        /// <remarks>水平</remarks>
        public Vector3 UAxis { get; private set; }
        #endregion

        #region V轴 —— Vector3 VAxis
        /// <summary>
        /// V轴
        /// </summary>
        /// <remarks>垂直</remarks>
        public Vector3 VAxis { get; private set; }
        #endregion

        #region 法向量 —— Vector3 Normal
        /// <summary>
        /// 法向量
        /// </summary>
        public Vector3 Normal { get; private set; }
        #endregion

        #region 平面类型 —— MPRPlaneType PlaneType
        /// <summary>
        /// 平面类型
        /// </summary>
        public MPRPlaneType PlaneType { get; private set; }
        #endregion

        #region 原始平面类型 —— MPRPlaneType OriginalPlaneType
        /// <summary>
        /// 原始平面类型
        /// </summary>
        public MPRPlaneType OriginalPlaneType { get; private set; }
        #endregion

        #region 切片数量 —— int SlicesCount
        /// <summary>
        /// 切片数量
        /// </summary>
        public int SlicesCount { get; private set; }
        #endregion

        #region 切片索引 —— int SliceIndex
        /// <summary>
        /// 切片索引
        /// </summary>
        public int SliceIndex { get; private set; }
        #endregion

        #region 切片偏移差分量 —— float SliceOffsetDelta
        /// <summary>
        /// 切片偏移差分量
        /// </summary>
        public float SliceOffsetDelta { get; private set; }
        #endregion

        #region 只读属性 - 世界平面中心 —— Vector3 WorldCenter
        /// <summary>
        /// 只读属性 - 世界平面中心
        /// </summary>
        public Vector3 WorldCenter
        {
            get => this.GetModelMatrix().ExtractTranslation();
        }
        #endregion

        #region 只读属性 - 世界U轴 —— Vector3 WorldUAxis
        /// <summary>
        /// 只读属性 - 世界U轴
        /// </summary>
        public Vector3 WorldUAxis
        {
            get => (this.UAxis * this.VolumeMetadata.VolumeScale).Normalized();
        }
        #endregion

        #region 只读属性 - 世界V轴 —— Vector3 WorldVAxis
        /// <summary>
        /// 只读属性 - 世界V轴
        /// </summary>
        public Vector3 WorldVAxis
        {
            get => (this.VAxis * this.VolumeMetadata.VolumeScale).Normalized();
        }
        #endregion

        #region 只读属性 - 世界法向量 —— Vector3 WorldNormal
        /// <summary>
        /// 只读属性 - 世界法向量
        /// </summary>
        public Vector3 WorldNormal
        {
            get => (this.Normal * this.VolumeMetadata.VolumeScale).Normalized();
        }
        #endregion

        #region 只读属性 - 世界切片间距 —— Vector3 WorldSliceSpacing
        /// <summary>
        /// 只读属性 - 世界切片间距
        /// </summary>
        /// <remarks>沿世界法向量方向移动一个切片的世界位移（不含方向/数量）</remarks>
        public float WorldSliceSpacing
        {
            get => Math.Abs(this.Normal.X) * this.VolumeMetadata.VolumeScale.X +
                   Math.Abs(this.Normal.Y) * this.VolumeMetadata.VolumeScale.Y +
                   Math.Abs(this.Normal.Z) * this.VolumeMetadata.VolumeScale.Z;
        }
        #endregion

        #endregion

        #region # 方法

        //Static

        #region 创建横断面 —— static MPRPlane CreateAxialPlane(VolumeMetadata volumeMetadata)
        /// <summary>
        /// 创建横断面
        /// </summary>
        /// <param name="volumeMetadata">体积元数据</param>
        /// <returns>横断面</returns>
        public static MPRPlane CreateAxialPlane(VolumeMetadata volumeMetadata)
        {
            MPRPlane plane = new MPRPlane(volumeMetadata)
            {
                _minProjection = -0.5f,
                _maxProjection = 0.5f,
                Center = Vector3.Zero,
                UAxis = new Vector3(1, 0, 0),
                VAxis = new Vector3(0, -1, 0),
                Normal = new Vector3(0, 0, 1),
                PlaneType = MPRPlaneType.Axial,
                OriginalPlaneType = MPRPlaneType.Axial,
                SlicesCount = volumeMetadata.VolumeSize.Z
            };
            plane.SetSliceIndex(volumeMetadata.VolumeSize.Z / 2);

            return plane;
        }
        #endregion

        #region 创建冠状面 —— static MPRPlane CreateCoronalPlane(VolumeMetadata volumeMetadata)
        /// <summary>
        /// 创建冠状面
        /// </summary>
        /// <param name="volumeMetadata">体积元数据</param>
        /// <returns>冠状面</returns>
        public static MPRPlane CreateCoronalPlane(VolumeMetadata volumeMetadata)
        {
            MPRPlane plane = new MPRPlane(volumeMetadata)
            {
                _minProjection = -0.5f,
                _maxProjection = 0.5f,
                Center = Vector3.Zero,
                UAxis = new Vector3(1, 0, 0),
                VAxis = new Vector3(0, 0, 1),
                Normal = new Vector3(0, 1, 0),
                PlaneType = MPRPlaneType.Coronal,
                OriginalPlaneType = MPRPlaneType.Coronal,
                SlicesCount = volumeMetadata.VolumeSize.Y
            };
            plane.SetSliceIndex(volumeMetadata.VolumeSize.Y / 2);

            return plane;
        }
        #endregion

        #region 创建矢状面 —— static MPRPlane CreateSagittalPlane(VolumeMetadata volumeMetadata)
        /// <summary>
        /// 创建矢状面
        /// </summary>
        /// <param name="volumeMetadata">体积元数据</param>
        /// <returns>矢状面</returns>
        public static MPRPlane CreateSagittalPlane(VolumeMetadata volumeMetadata)
        {
            MPRPlane plane = new MPRPlane(volumeMetadata)
            {
                _minProjection = -0.5f,
                _maxProjection = 0.5f,
                Center = Vector3.Zero,
                UAxis = new Vector3(0, 1, 0),
                VAxis = new Vector3(0, 0, 1),
                Normal = new Vector3(-1, 0, 0),
                PlaneType = MPRPlaneType.Sagittal,
                OriginalPlaneType = MPRPlaneType.Sagittal,
                SlicesCount = volumeMetadata.VolumeSize.X
            };
            plane.SetSliceIndex(volumeMetadata.VolumeSize.X / 2);

            return plane;
        }
        #endregion


        //Public

        #region 设置切片索引 —— void SetSliceIndex(int sliceIndex...
        /// <summary>
        /// 设置切片索引
        /// </summary>
        /// <param name="sliceIndex">切片索引</param>
        /// <param name="triggerSource">触发源</param>
        public void SetSliceIndex(int sliceIndex, MPRPlaneChangeSource triggerSource = MPRPlaneChangeSource.SliceScroll)
        {
            sliceIndex = Math.Clamp(sliceIndex, 0, this.SlicesCount - 1);
            if (this.SliceIndex != sliceIndex)
            {
                this.SliceIndex = sliceIndex;

                //触发变化事件
                MPRPlaneChangedEventArgs eventArgs = new MPRPlaneChangedEventArgs(triggerSource);
                this.OnChanged(this, eventArgs);
            }
        }
        #endregion

        #region 重定位平面 —— void Relocate(Vector3 worldCenter...
        /// <summary>
        /// 重定位平面
        /// </summary>
        /// <param name="worldCenter">世界中心位置</param>
        /// <param name="triggerSource">触发源</param>
        public void Relocate(Vector3 worldCenter, MPRPlaneChangeSource triggerSource = MPRPlaneChangeSource.CrosshairDrag)
        {
            //将世界坐标转换到逻辑空间
            Vector3 localCenter = worldCenter / this.VolumeMetadata.VolumeScale;

            //计算切片索引
            float sliceOffset = Vector3.Dot(localCenter, this.Normal);
            float t = this.PlaneType switch
            {
                MPRPlaneType.Oblique => (sliceOffset - this._minProjection) / (this._maxProjection - this._minProjection),
                MPRPlaneType.Axial => localCenter.Z + 0.5f,
                MPRPlaneType.Coronal => localCenter.Y + 0.5f,
                MPRPlaneType.Sagittal => -localCenter.X + 0.5f,
                _ => throw new NotSupportedException()
            };
            int sliceIndex = (int)Math.Round(t * (this.SlicesCount - 1));
            this.SetSliceIndex(sliceIndex, triggerSource);
        }
        #endregion

        #region 重定位平面 —— void Relocate(Vector3 worldUAxis, Vector3 worldVAxis...
        /// <summary>
        /// 重定位平面
        /// </summary>
        /// <param name="worldUAxis">世界U轴</param>
        /// <param name="worldVAxis">世界V轴</param>
        /// <param name="worldCenter">世界中心位置</param>
        /// <param name="worldNormal">世界法向量</param>
        public void Relocate(Vector3 worldUAxis, Vector3 worldVAxis, Vector3 worldCenter, Vector3 worldNormal)
        {
            //更新MPR平面U/V轴
            this.UAxis = worldUAxis / this.VolumeMetadata.VolumeScale;
            this.VAxis = worldVAxis / this.VolumeMetadata.VolumeScale;
            this.Normal = worldNormal / this.VolumeMetadata.VolumeScale;

            //更新平面类型
            if (Math.Abs(this.Normal.Z) > 0.99f)
            {
                this.PlaneType = MPRPlaneType.Axial;
            }
            else if (Math.Abs(this.Normal.Y) > 0.99f)
            {
                this.PlaneType = MPRPlaneType.Coronal;
            }
            else if (Math.Abs(this.Normal.X) > 0.99f)
            {
                this.PlaneType = MPRPlaneType.Sagittal;
            }
            else
            {
                this.PlaneType = MPRPlaneType.Oblique;
            }

            //斜切面重新计算投影范围和切片数量
            if (this.PlaneType == MPRPlaneType.Oblique)
            {
                this.CalculateProjectionRange();
                this.SlicesCount = this.CalculateObliqueSlicesCount();
            }

            //复用中心定位
            this.Relocate(worldCenter, MPRPlaneChangeSource.ExternalSync);
        }
        #endregion

        #region 旋转平面 —— void Rotate(float deltaU, float deltaV)
        /// <summary>
        /// 旋转平面
        /// </summary>
        /// <param name="deltaU">绕U轴旋转角度（上下旋转）</param>
        /// <param name="deltaV">绕V轴旋转角度（左右旋转）</param>
        public void Rotate(float deltaU, float deltaV)
        {
            //绕U轴旋转（上下）
            Quaternion rotationU = Quaternion.FromAxisAngle(this.UAxis, MathHelper.DegreesToRadians(deltaU));

            //绕V轴旋转（左右）
            Quaternion rotationV = Quaternion.FromAxisAngle(this.VAxis, MathHelper.DegreesToRadians(deltaV));

            //组合旋转：先左右，再上下
            Quaternion rotation = rotationU * rotationV;

            //应用旋转
            this.UAxis = Vector3.Transform(this.UAxis, rotation).Normalized();
            this.VAxis = Vector3.Transform(this.VAxis, rotation).Normalized();
            this.Normal = Vector3.Transform(this.Normal, rotation).Normalized();

            //重新正交化
            this.Orthonormalize();

            //更新类型
            this.PlaneType = MPRPlaneType.Oblique;

            //计算投影范围
            this.CalculateProjectionRange();

            //重新计算切片数量
            this.SlicesCount = this.CalculateObliqueSlicesCount();

            //触发变化事件
            MPRPlaneChangedEventArgs eventArgs = new MPRPlaneChangedEventArgs(MPRPlaneChangeSource.ExternalSync);
            this.OnChanged(this, eventArgs);
        }
        #endregion

        #region 重置为标准平面 —— void ResetToStandard()
        /// <summary>
        /// 重置为标准平面
        /// </summary>
        public void ResetToStandard()
        {
            MPRPlane standardPlane = this.OriginalPlaneType switch
            {
                MPRPlaneType.Axial => CreateAxialPlane(this.VolumeMetadata),
                MPRPlaneType.Coronal => CreateCoronalPlane(this.VolumeMetadata),
                MPRPlaneType.Sagittal => CreateSagittalPlane(this.VolumeMetadata),
                _ => CreateAxialPlane(this.VolumeMetadata)
            };

            this.Center = standardPlane.Center;
            this.UAxis = standardPlane.UAxis;
            this.VAxis = standardPlane.VAxis;
            this.Normal = standardPlane.Normal;
            this.PlaneType = standardPlane.PlaneType;
            this.SlicesCount = standardPlane.SlicesCount;
            this.SliceIndex = standardPlane.SliceIndex;
            this._minProjection = standardPlane._minProjection;
            this._maxProjection = standardPlane._maxProjection;

            //触发变化事件
            MPRPlaneChangedEventArgs eventArgs = new MPRPlaneChangedEventArgs(MPRPlaneChangeSource.ExternalSync);
            this.OnChanged(this, eventArgs);
        }
        #endregion

        #region 屏幕坐标转换平面U/V坐标 —— Vector2? ScreenToPlaneUV(Vector2 mousePos2D...
        /// <summary>
        /// 屏幕坐标转换平面U/V坐标
        /// </summary>
        /// <param name="mousePos2D">鼠标2D位置</param>
        /// <param name="viewportSize">视口尺寸</param>
        /// <param name="projectionMatrix">投影矩阵</param>
        /// <param name="viewMatrix">视图矩阵</param>
        /// <param name="ray">射线</param>
        /// <returns>U/V坐标，[-1, 1]，如果不在平面上则返回null</returns>
        public Vector2? ScreenToPlaneUV(Vector2 mousePos2D, Vector2 viewportSize, Matrix4 projectionMatrix, Matrix4 viewMatrix, out Ray ray)
        {
            //将屏幕坐标转换到世界空间的射线起点（近平面上的点）
            float ndcX = (2.0f * mousePos2D.X) / viewportSize.X - 1.0f;
            float ndcY = 1.0f - (2.0f * mousePos2D.Y) / viewportSize.Y;

            Vector4 rayStartNDC = new Vector4(ndcX, ndcY, -1.0f, 1.0f);
            Matrix4 invProjection = Matrix4.Invert(projectionMatrix);
            Matrix4 invView = Matrix4.Invert(viewMatrix);

            Vector4 rayStartCamera = rayStartNDC * invProjection;
            rayStartCamera /= rayStartCamera.W;
            Vector3 rayStartWorld = Vector3.TransformPosition(rayStartCamera.Xyz, invView);

            //横断位特殊处理
            if (this.OriginalPlaneType == MPRPlaneType.Axial)
            {
                rayStartWorld *= new Vector3(-1, 1, 1);
            }

            //射线方向固定为平面世界法向量 = 相机视角方向
            Vector3 rayDirection = this.WorldNormal;

            //创建射线
            ray = new Ray(rayStartWorld, rayDirection);

            //射线与平面求交
            if (ray.IntersectsPlane(this.WorldCenter, this.WorldNormal, out Vector3 hitPoint, out _))
            {
                //转换到逻辑空间
                Vector3 localPoint = hitPoint / this.VolumeMetadata.VolumeScale;

                //投影到平面得到UV
                Vector2 uv = this.ProjectPoint(localPoint);

                //检查是否在平面范围内
                if (uv.X >= -1 && uv.X <= 1 && uv.Y >= -1 && uv.Y <= 1)
                {
                    return uv;
                }
            }

            return null;
        }
        #endregion

        #region 获取平面上的体素坐标 —— Vector3i GetVoxelPosition(float u, float v...
        /// <summary>
        /// 获取平面上的体素坐标
        /// </summary>
        /// <param name="u">U坐标，[-1, 1]</param>
        /// <param name="v">V坐标，[-1, 1]</param>
        /// <param name="textureCoord">纹理坐标</param>
        /// <param name="worldPosition">世界位置</param>
        /// <returns>体素坐标，[0, VolumeSize-1]</returns>
        public Vector3i GetVoxelPosition(float u, float v, out Vector3 textureCoord, out Vector3 worldPosition)
        {
            //逻辑空间点（-0.5到0.5）
            Vector3 localPosition = this.GetPointOnPlane(u, v);

            //逻辑空间 -> 纹理坐标（0到1）
            textureCoord = localPosition + new Vector3(0.5f);

            //逻辑空间 -> 世界位置（-0.5到0.5）
            worldPosition = localPosition * this.VolumeMetadata.VolumeScale;

            //纹理坐标 -> 体素坐标
            int x = (int)Math.Round(textureCoord.X * this.VolumeMetadata.VolumeSize.X);
            int y = (int)Math.Round(textureCoord.Y * this.VolumeMetadata.VolumeSize.Y);
            int z = (int)Math.Round(textureCoord.Z * this.VolumeMetadata.VolumeSize.Z);

            //边界裁剪
            x = Math.Clamp(x, 0, this.VolumeMetadata.VolumeSize.X - 1);
            y = Math.Clamp(y, 0, this.VolumeMetadata.VolumeSize.Y - 1);
            z = Math.Clamp(z, 0, this.VolumeMetadata.VolumeSize.Z - 1);

            return new Vector3i(x, y, z);
        }
        #endregion

        #region 将体素坐标投影到平面 —— Vector2 ProjectVoxel(Vector3i voxelPosition)
        /// <summary>
        /// 将体素坐标投影到平面
        /// </summary>
        /// <param name="voxelPosition">体素坐标，[0, VolumeSize-1]</param>
        /// <returns>平面U/V坐标，[-1, 1]</returns>
        public Vector2 ProjectVoxel(Vector3i voxelPosition)
        {
            //体素坐标 -> 纹理坐标（0到1）
            Vector3 texCoord = new Vector3(
                voxelPosition.X * 1.0f / (this.VolumeMetadata.VolumeSize.X - 1),
                voxelPosition.Y * 1.0f / (this.VolumeMetadata.VolumeSize.Y - 1),
                voxelPosition.Z * 1.0f / (this.VolumeMetadata.VolumeSize.Z - 1)
            );

            //纹理坐标 -> 逻辑空间点（-0.5到0.5）
            Vector3 localPoint = texCoord - new Vector3(0.5f);

            //投影到平面得到UV
            return this.ProjectPoint(localPoint);
        }
        #endregion

        #region 获取模型矩阵 —— Matrix4 GetModelMatrix()
        /// <summary>
        /// 获取模型矩阵
        /// </summary>
        /// <returns>模型矩阵</returns>
        public Matrix4 GetModelMatrix()
        {
            //切片偏移：逻辑空间 -0.5到0.5
            float sliceOffset = this.CalculateSliceOffset();

            //平面中心：逻辑空间 (0,0,0) -> 世界空间 (0,0,0)
            Vector3 worldCenter = Vector3.Zero;

            //法向量方向的世界偏移 = 逻辑偏移 * VolumeScale
            Vector3 worldOffset = new Vector3(
                this.Normal.X * this.VolumeMetadata.VolumeScale.X * sliceOffset,
                this.Normal.Y * this.VolumeMetadata.VolumeScale.Y * sliceOffset,
                this.Normal.Z * this.VolumeMetadata.VolumeScale.Z * sliceOffset
            );

            Matrix4 translation = Matrix4.CreateTranslation(worldCenter + worldOffset);
            Matrix4 basis = new Matrix4(
                new Vector4(this.WorldUAxis, 0),
                new Vector4(this.WorldVAxis, 0),
                new Vector4(this.WorldNormal, 0),
                new Vector4(0, 0, 0, 1)
            );

            //处理缩放
            Matrix4 scale = Matrix4.CreateScale(this.VolumeMetadata.VolumeScale);

            return basis * scale * translation;
        }
        #endregion


        //Private

        #region 计算投影范围 —— void CalculateProjectionRange()
        /// <summary>
        /// 计算投影范围
        /// </summary>
        private void CalculateProjectionRange()
        {
            this._minProjection = float.MaxValue;
            this._maxProjection = float.MinValue;
            IEnumerable<Vector3> corners = ResourceManager.UnitCube.Vertices.Select(vertex => vertex.Position);
            foreach (Vector3 corner in corners)
            {
                float projection = Vector3.Dot(corner, this.Normal);
                this._minProjection = Math.Min(this._minProjection, projection);
                this._maxProjection = Math.Max(this._maxProjection, projection);
            }
        }
        #endregion

        #region 计算切片偏移量 —— float CalculateSliceOffset()
        /// <summary>
        /// 计算切片偏移量
        /// </summary>
        /// <returns>切片偏移量</returns>
        public float CalculateSliceOffset()
        {
            #region # 验证

            if (this.SlicesCount <= 1)
            {
                return 0;
            }

            #endregion

            float sliceOffset;
            if (this.PlaneType == MPRPlaneType.Oblique)
            {
                //斜切平面：根据投影范围映射
                float t = this.SliceIndex * 1.0f / (this.SlicesCount - 1);
                sliceOffset = this._minProjection + t * (this._maxProjection - this._minProjection);
            }
            else
            {
                //标准平面：逻辑空间范围 -0.5到0.5
                float t = this.SliceIndex * 1.0f / (this.SlicesCount - 1);
                sliceOffset = -0.5f + t;
            }

            return sliceOffset;
        }
        #endregion

        #region 计算斜切面切片数量 —— int CalculateObliqueSlicesCount()
        /// <summary>
        /// 计算斜切面切片数量
        /// </summary>
        /// <returns>切片数量</returns>
        private int CalculateObliqueSlicesCount()
        {
            Vector3 absNormal = new Vector3(
                Math.Abs(this.Normal.X),
                Math.Abs(this.Normal.Y),
                Math.Abs(this.Normal.Z)
            );
            float projection =
                this.VolumeMetadata.VolumeSize.X * absNormal.X +
                this.VolumeMetadata.VolumeSize.Y * absNormal.Y +
                this.VolumeMetadata.VolumeSize.Z * absNormal.Z;

            int slicesCount = (int)Math.Floor(Math.Max(projection, 2));

            return slicesCount;
        }
        #endregion

        #region 正交化坐标轴 —— void Orthonormalize()
        /// <summary>
        /// 正交化坐标轴
        /// </summary>
        private void Orthonormalize()
        {
            //保存原始方向用于符号修正
            Vector3 originalV = this.VAxis;
            Vector3 originalN = this.Normal;
            Vector3 originalU = this.UAxis;

            //第一步：固定U轴，正交化N，移除N中与U平行的分量，保证N⟂U
            this.UAxis = this.UAxis.Normalized();
            this.Normal = this.Normal.Normalized();
            this.Normal = (this.Normal - Vector3.Dot(this.Normal, this.UAxis) * this.UAxis).Normalized();

            //第二步：V由N×U得到，保证右手系
            this.VAxis = Vector3.Cross(this.Normal, this.UAxis).Normalized();

            //第三步：符号修正——确保新基向量的方向和原始基向量一致
            if (Vector3.Dot(this.UAxis, originalU) < 0)
            {
                this.UAxis = -this.UAxis;
            }
            if (Vector3.Dot(this.VAxis, originalV) < 0)
            {
                this.VAxis = -this.VAxis;
            }
            if (Vector3.Dot(this.Normal, originalN) < 0)
            {
                this.Normal = -this.Normal;
            }
        }
        #endregion

        #region 获取平面上的点 —— Vector3 GetPointOnPlane(float u, float v)
        /// <summary>
        /// 获取平面上的点
        /// </summary>
        /// <param name="u">U坐标，范围[-1, 1]</param>
        /// <param name="v">V坐标，范围[-1, 1]</param>
        /// <returns>逻辑空间中的点，范围[-0.5, 0.5]</returns>
        /// <remarks>逻辑空间</remarks>
        private Vector3 GetPointOnPlane(float u, float v)
        {
            const float halfSize = 0.5f;
            float sliceOffset = this.CalculateSliceOffset();

            //平面上的点 = 中心 + 法向量方向偏移 + U方向偏移 + V方向偏移
            Vector3 point = this.Center + this.Normal * sliceOffset + this.UAxis * u * halfSize + this.VAxis * v * halfSize;

            return point;
        }
        #endregion

        #region 投影点到平面 —— Vector2 ProjectPoint(Vector3 point)
        /// <summary>
        /// 投影点到平面
        /// </summary>
        /// <param name="point">逻辑空间中的点，范围[-0.5, 0.5]</param>
        /// <returns>平面U/V坐标，范围[-1, 1]</returns>
        /// <remarks>逻辑空间</remarks>
        private Vector2 ProjectPoint(Vector3 point)
        {
            const float halfSize = 0.5f;
            float sliceOffset = this.CalculateSliceOffset();

            //计算相对于平面中心的偏移
            Vector3 relative = point - (this.Center + this.Normal * sliceOffset);

            //投影到U轴和V轴
            float u = Vector3.Dot(relative, this.UAxis) / halfSize;
            float v = Vector3.Dot(relative, this.VAxis) / halfSize;

            return new Vector2(u, v);
        }
        #endregion

        #region 触发平面变化事件 —— void OnChanged(object sender...
        /// <summary>
        /// 触发平面变化事件
        /// </summary>
        private void OnChanged(object sender, MPRPlaneChangedEventArgs eventArgs)
        {
            float currentOffset = this.CalculateSliceOffset();
            if (this._firstChanged)
            {
                this._firstChanged = false;
                this.SliceOffsetDelta = 0;
            }
            else
            {
                this.SliceOffsetDelta = currentOffset - this._previousSliceOffset;
            }
            this._previousSliceOffset = currentOffset;

            this.PlaneChangedEvent?.Invoke(this, eventArgs);
        }
        #endregion 

        #endregion
    }
}
