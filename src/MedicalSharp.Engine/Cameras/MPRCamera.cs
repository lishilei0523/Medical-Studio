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
        /// 创建MPR相机构造器
        /// </summary>
        /// <param name="planeType">平面类型</param>
        /// <param name="nearPlaneDistance">近平面距离</param>
        /// <param name="farPlaneDistance">远平面距离</param>
        public MPRCamera(MPRPlaneType planeType = MPRPlaneType.Axial, float nearPlaneDistance = short.MinValue, float farPlaneDistance = short.MaxValue)
            : base(nearPlaneDistance, farPlaneDistance)
        {
            this._targetPosition = Vector3.Zero;
            this._planeType = planeType;
            this._sliceIndex = 0;
            this._maxSliceCount = 100;
            this._sliceSpacing = 1.0f;
            this._zoomFactor = 1.0f;
            this._panOffset = Vector2.Zero;
            this._volumeActualSize = Vector3.One;

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

        #region 目标位置 —— Vector3 TargetPosition
        /// <summary>
        /// 目标位置
        /// </summary>
        private Vector3 _targetPosition;

        /// <summary>
        /// 目标位置
        /// </summary>
        public Vector3 TargetPosition
        {
            get => this._targetPosition;
            set
            {
                if (this._targetPosition != value)
                {
                    this._targetPosition = value;
                    this.UpdateViewMatrix();
                }
            }
        }
        #endregion

        #region 平面类型 —— MPRPlaneType PlaneType
        /// <summary>
        /// 平面类型
        /// </summary>
        private MPRPlaneType _planeType;

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
        private int _sliceIndex;

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
                    this.UpdateViewMatrix();
                }
            }
        }
        #endregion

        #region 最大切片数 —— int MaxSliceCount
        /// <summary>
        /// 最大切片数
        /// </summary>
        private int _maxSliceCount;

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
                    this.UpdateViewMatrix();
                }
            }
        }
        #endregion

        #region 切片间距 —— float SliceSpacing
        /// <summary>
        /// 切片间距
        /// </summary>
        private float _sliceSpacing;

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
                    this.UpdateViewMatrix();
                }
            }
        }
        #endregion

        #region 缩放因子 —— float ZoomFactor
        /// <summary>
        /// 缩放因子
        /// </summary>
        private float _zoomFactor;

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
        private Vector2 _panOffset;

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

        #region 体积实际尺寸 —— Vector3 VolumeActualSize
        /// <summary>
        /// 体积实际尺寸
        /// </summary>
        private Vector3 _volumeActualSize;

        /// <summary>
        /// 体积实际尺寸
        /// </summary>
        public Vector3 VolumeActualSize
        {
            get => this._volumeActualSize;
            set
            {
                this._volumeActualSize = value;
                this.UpdateProjectionMatrix();
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
            this.UpdateCameraVectors();
            this.UpdateProjectionMatrix();
        }
        #endregion

        #region 缩放相机 —— void Zoom(float delta)
        /// <summary>
        /// 缩放相机
        /// </summary>
        /// <param name="delta">缩放增量（正数放大，负数缩小）</param>
        public void Zoom(float delta)
        {
            this._zoomFactor *= (1.0f + delta * 0.1f);
            this._zoomFactor = Math.Max(0.1f, Math.Min(10.0f, this._zoomFactor));
            this.UpdateProjectionMatrix();
        }
        #endregion

        #region 平移相机 —— void Pan(float deltaX, float deltaY)
        /// <summary>
        /// 平移相机
        /// </summary>
        /// <param name="deltaX">水平平移量</param>
        /// <param name="deltaY">垂直平移量</param>
        public void Pan(float deltaX, float deltaY)
        {
            //将鼠标移动归一化到 [-1, 1] 范围
            float normalizedDx = deltaX / this._viewportWidth * 2.0f;
            float normalizedDy = deltaY / this._viewportHeight * 2.0f;

            //考虑缩放因子
            float panSpeed = 0.5f * 100 / this._zoomFactor;  //0.5 是半宽

            this._panOffset.X -= normalizedDx * panSpeed;
            this._panOffset.Y += normalizedDy * panSpeed;  //Y轴方向反转

            this.UpdateProjectionMatrix();
        }
        #endregion

        #region 重置相机 —— void Reset()
        /// <summary>
        /// 重置相机
        /// </summary>
        public void Reset()
        {
            this._zoomFactor = 1.0f;
            this._panOffset = Vector2.Zero;
            this._targetPosition = Vector3.Zero;
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
                    this._cameraPosition = new Vector3(0, 1, 0);   //位于Y轴正方向
                    this._lookDirection = new Vector3(0, -1, 0);    //看向Y轴负方向
                    this._upDirection = new Vector3(0, 0, 1);       //Z轴向上
                    this._rightDirection = new Vector3(1, 0, 0);    //X轴向右
                    break;

                case MPRPlaneType.Coronal:  //冠状面 - 从前向后看
                    this._cameraPosition = new Vector3(0, 0, 1);   //位于Z轴正方向
                    this._lookDirection = new Vector3(0, 0, -1);    //看向Z轴负方向
                    this._upDirection = new Vector3(0, 1, 0);       //Y轴向上
                    this._rightDirection = new Vector3(1, 0, 0);    //X轴向右
                    break;

                case MPRPlaneType.Sagittal: //矢状面 - 从左向右看
                    this._cameraPosition = new Vector3(-1, 0, 0);   //位于X轴负方向
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
            float halfSideSize = 0.5f / this._zoomFactor;
            float left, right, bottom, top;
            if (aspect >= 1.0f)
            {
                //横屏
                left = -halfSideSize * aspect + this._panOffset.X;
                right = halfSideSize * aspect + this._panOffset.X;
                bottom = -halfSideSize + this._panOffset.Y;
                top = halfSideSize + this._panOffset.Y;
            }
            else
            {
                //竖屏
                left = -halfSideSize + this._panOffset.X;
                right = halfSideSize + this._panOffset.X;
                bottom = -halfSideSize / aspect + this._panOffset.Y;
                top = halfSideSize / aspect + this._panOffset.Y;
            }

            this._projectionMatrix = Matrix4.CreateOrthographicOffCenter(left, right, bottom, top, this._nearPlaneDistance, this._farPlaneDistance);
        }
        #endregion

        #region 更新视图矩阵 —— void UpdateViewMatrix()
        /// <summary>
        /// 更新视图矩阵
        /// </summary>
        private void UpdateViewMatrix()
        {
            //根据切片索引计算目标点偏移
            Vector3 sliceOffset = this._planeType switch
            {
                MPRPlaneType.Axial => new Vector3(0, 0, this._sliceIndex * this._sliceSpacing),
                MPRPlaneType.Coronal => new Vector3(0, this._sliceIndex * this._sliceSpacing, 0),
                MPRPlaneType.Sagittal => new Vector3(this._sliceIndex * this._sliceSpacing, 0, 0),
                _ => Vector3.Zero
            };

            //计算视图矩阵（相机始终看向目标点）
            this._viewMatrix = Matrix4.LookAt(this._cameraPosition + sliceOffset, this._targetPosition, this._upDirection);
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
