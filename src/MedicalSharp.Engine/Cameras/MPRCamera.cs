using MedicalSharp.Engine.ValueTypes;
using OpenTK.Mathematics;
using System;

namespace MedicalSharp.Engine.Cameras
{
    /// <summary>
    /// MPR相机
    /// </summary>
    public class MPRCamera : Camera
    {
        #region # 字段及构造器

        /// <summary>
        /// 相机位置
        /// </summary>
        private Vector3 _cameraPosition;

        /// <summary>
        /// 视角方向
        /// </summary>
        private Vector3 _lookDirection;

        /// <summary>
        /// 相机上方向
        /// </summary>
        private Vector3 _upDirection;

        /// <summary>
        /// 相机右方向
        /// </summary>
        private Vector3 _rightDirection;

        /// <summary>
        /// 视图矩阵
        /// </summary>
        private Matrix4 _viewMatrix;

        /// <summary>
        /// 投影矩阵
        /// </summary>
        private Matrix4 _projectionMatrix;

        /// <summary>
        /// 平面类型
        /// </summary>
        private MPRPlaneType _planeType;

        /// <summary>
        /// 切片索引
        /// </summary>
        private int _sliceIndex;

        /// <summary>
        /// 最大切片数
        /// </summary>
        private int _maxSliceCount;

        /// <summary>
        /// 切片间距
        /// </summary>
        private float _sliceSpacing;

        /// <summary>
        /// 视口缩放比例
        /// </summary>
        private float _zoomFactor;

        /// <summary>
        /// 平移偏移量
        /// </summary>
        private Vector2 _panOffset;

        /// <summary>
        /// 创建MPR相机构造器
        /// </summary>
        /// <param name="planeType">平面类型</param>
        /// <param name="nearPlaneDistance">近平面距离</param>
        /// <param name="farPlaneDistance">远平面距离</param>
        public MPRCamera(MPRPlaneType planeType = MPRPlaneType.Axial, float nearPlaneDistance = -1000.0f, float farPlaneDistance = 1000.0f)
            : base(nearPlaneDistance, farPlaneDistance)
        {
            this._planeType = planeType;
            this._sliceIndex = 0;
            this._maxSliceCount = 100;
            this._sliceSpacing = 1.0f;
            this._zoomFactor = 1.0f;
            this._panOffset = Vector2.Zero;

            this.UpdateCameraVectors();
            this.UpdateMatrices();
        }

        #endregion

        #region # 属性

        #region 相机位置 —— override Vector3 CameraPosition
        /// <summary>
        /// 相机位置
        /// </summary>
        public override Vector3 CameraPosition => this._cameraPosition;
        #endregion

        #region 视角方向 —— override Vector3 LookDirection
        /// <summary>
        /// 视角方向
        /// </summary>
        public override Vector3 LookDirection => this._lookDirection;
        #endregion

        #region 相机上方向 —— override Vector3 UpDirection
        /// <summary>
        /// 相机上方向
        /// </summary>
        public override Vector3 UpDirection => this._upDirection;
        #endregion

        #region 相机右方向 —— override Vector3 RightDirection
        /// <summary>
        /// 相机右方向
        /// </summary>
        public override Vector3 RightDirection => this._rightDirection;
        #endregion

        #region 投影矩阵 —— override Matrix4 ProjectionMatrix
        /// <summary>
        /// 投影矩阵
        /// </summary>
        public override Matrix4 ProjectionMatrix => this._projectionMatrix;
        #endregion

        #region 视图矩阵 —— override Matrix4 ViewMatrix
        /// <summary>
        /// 视图矩阵
        /// </summary>
        public override Matrix4 ViewMatrix => this._viewMatrix;
        #endregion

        #region 平面类型 —— MPRPlaneType PlaneType
        /// <summary>
        /// 平面类型
        /// </summary>
        public MPRPlaneType PlaneType
        {
            get => this._planeType;
            set
            {
                if (this._planeType != value)
                {
                    this._planeType = value;
                    this.UpdateCameraVectors();
                    this.UpdateMatrices();
                }
            }
        }
        #endregion

        #region 切片索引 —— int SliceIndex
        /// <summary>
        /// 切片索引
        /// </summary>
        public int SliceIndex
        {
            get => this._sliceIndex;
            set
            {
                int newIndex = MathHelper.Clamp(value, 0, this._maxSliceCount - 1);
                if (this._sliceIndex != newIndex)
                {
                    this._sliceIndex = newIndex;
                    this.UpdateMatrices();
                }
            }
        }
        #endregion

        #region 最大切片数 —— int MaxSliceCount
        /// <summary>
        /// 最大切片数
        /// </summary>
        public int MaxSliceCount
        {
            get => this._maxSliceCount;
            set
            {
                if (this._maxSliceCount != value && value > 0)
                {
                    this._maxSliceCount = value;
                    this._sliceIndex = MathHelper.Clamp(this._sliceIndex, 0, this._maxSliceCount - 1);
                    this.UpdateMatrices();
                }
            }
        }
        #endregion

        #region 切片间距 —— float SliceSpacing
        /// <summary>
        /// 切片间距
        /// </summary>
        public float SliceSpacing
        {
            get => this._sliceSpacing;
            set
            {
                if (!this._sliceSpacing.Equals(value) && value > 0)
                {
                    this._sliceSpacing = value;
                    this.UpdateMatrices();
                }
            }
        }
        #endregion

        #region 缩放因子 —— float ZoomFactor
        /// <summary>
        /// 缩放因子
        /// </summary>
        public float ZoomFactor
        {
            get => this._zoomFactor;
            set
            {
                if (!this._zoomFactor.Equals(value) && value > 0)
                {
                    this._zoomFactor = value;
                    this.UpdateProjectionMatrix();
                }
            }
        }
        #endregion

        #region 平移偏移量 —— Vector2 PanOffset
        /// <summary>
        /// 平移偏移量
        /// </summary>
        public Vector2 PanOffset
        {
            get => this._panOffset;
            set
            {
                if (this._panOffset != value)
                {
                    this._panOffset = value;
                    this.UpdateProjectionMatrix();
                }
            }
        }
        #endregion

        #endregion

        #region # 方法

        //Public

        #region 设置视口尺寸 —— override void SetViewportSize(float width, float height)
        /// <summary>
        /// 设置视口尺寸
        /// </summary>
        /// <param name="width">视口宽度</param>
        /// <param name="height">视口高度</param>
        public override void SetViewportSize(float width, float height)
        {
            base.SetViewportSize(width, height);
            this.UpdateProjectionMatrix();
        }
        #endregion

        #region 缩放 —— void Zoom(float delta)
        /// <summary>
        /// 缩放
        /// </summary>
        /// <param name="delta">缩放增量（正数放大，负数缩小）</param>
        public void Zoom(float delta)
        {
            this._zoomFactor *= (1.0f + delta * 0.1f);
            this._zoomFactor = Math.Max(0.1f, this._zoomFactor);
            this.UpdateProjectionMatrix();
        }
        #endregion

        #region 平移 —— void Pan(Vector2 delta)
        /// <summary>
        /// 平移
        /// </summary>
        /// <param name="delta">平移增量</param>
        public void Pan(Vector2 delta)
        {
            this._panOffset += delta * (2.0f / this._zoomFactor);
            this.UpdateProjectionMatrix();
        }
        #endregion

        #region 重置相机位置 —— void Reset()
        /// <summary>
        /// 重置相机位置
        /// </summary>
        public void Reset()
        {
            this._zoomFactor = 1.0f;
            this._panOffset = Vector2.Zero;
            this._sliceIndex = this._maxSliceCount / 2;
            this.UpdateMatrices();
        }
        #endregion


        //Private

        #region 更新相机向量 —— void UpdateCameraVectors()
        /// <summary>
        /// 更新相机向量
        /// </summary>
        private void UpdateCameraVectors()
        {
            switch (this._planeType)
            {
                case MPRPlaneType.Axial:    //横断面 - 从上往下看
                    this._cameraPosition = new Vector3(0, 1000, 0); //位于Y轴正方向
                    this._lookDirection = new Vector3(0, -1, 0);    //看向Y轴负方向
                    this._upDirection = new Vector3(0, 0, 1);       //Z轴向上
                    this._rightDirection = new Vector3(1, 0, 0);    //X轴向右
                    break;

                case MPRPlaneType.Coronal:  //冠状面 - 从前向后看
                    this._cameraPosition = new Vector3(0, 0, 1000); //位于Z轴正方向
                    this._lookDirection = new Vector3(0, 0, -1);    //看向Z轴负方向
                    this._upDirection = new Vector3(0, 1, 0);       //Y轴向上
                    this._rightDirection = new Vector3(1, 0, 0);    //X轴向右
                    break;

                case MPRPlaneType.Sagittal: //矢状面 - 从左向右看
                    this._cameraPosition = new Vector3(-1000, 0, 0); //位于X轴负方向
                    this._lookDirection = new Vector3(1, 0, 0);      //看向X轴正方向
                    this._upDirection = new Vector3(0, 1, 0);        //Y轴向上
                    this._rightDirection = new Vector3(0, 0, 1);     //Z轴向右
                    break;
            }
        }
        #endregion

        #region 更新投影矩阵 —— void UpdateProjectionMatrix()
        /// <summary>
        /// 更新投影矩阵
        /// </summary>
        private void UpdateProjectionMatrix()
        {
            if (this._viewportWidth <= 0 || this._viewportHeight <= 0)
            {
                this._projectionMatrix = Matrix4.Identity;
                return;
            }

            //计算正交投影范围
            float aspect = this._viewportWidth / this._viewportHeight;
            float size = 256.0f / this._zoomFactor; //基础大小除以缩放因子

            float left = -size * aspect + this._panOffset.X;
            float right = size * aspect + this._panOffset.X;
            float bottom = -size + this._panOffset.Y;
            float top = size + this._panOffset.Y;

            this._projectionMatrix = Matrix4.CreateOrthographicOffCenter(left, right, bottom, top, this._nearPlaneDistance, this._farPlaneDistance);
        }
        #endregion

        #region 更新视图矩阵 —— void UpdateViewMatrix()
        /// <summary>
        /// 更新视图矩阵
        /// </summary>
        private void UpdateViewMatrix()
        {
            //根据切片索引计算相机位置偏移
            float sliceOffset = this._sliceIndex * this._sliceSpacing;

            Vector3 targetPosition = this._planeType switch
            {
                MPRPlaneType.Axial => new Vector3(0, sliceOffset, 0),
                MPRPlaneType.Coronal => new Vector3(0, 0, sliceOffset),
                MPRPlaneType.Sagittal => new Vector3(sliceOffset, 0, 0),
                _ => Vector3.Zero
            };

            //计算视图矩阵（相机看向目标点）
            this._viewMatrix = Matrix4.LookAt(this._cameraPosition + targetPosition, targetPosition, this._upDirection);
        }
        #endregion

        #region 更新所有矩阵 —— void UpdateMatrices()
        /// <summary>
        /// 更新所有矩阵
        /// </summary>
        private void UpdateMatrices()
        {
            this.UpdateViewMatrix();
            this.UpdateProjectionMatrix();
        }
        #endregion

        #endregion
    }
}
