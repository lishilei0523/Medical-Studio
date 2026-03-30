using MedicalSharp.Engine.ValueTypes;
using OpenTK.Mathematics;
using System;

namespace MedicalSharp.Engine.Cameras
{
    /// <summary>
    /// 轨道相机
    /// </summary>
    public abstract class OrbitCamera : Camera
    {
        #region # 字段及构造器

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
        /// 偏航角-RY（角度）
        /// </summary>
        private float _yaw;

        /// <summary>
        /// 俯仰角-RX（角度）
        /// </summary>
        private float _pitch;

        /// <summary>
        /// 最小距离
        /// </summary>
        private float _minDistance;

        /// <summary>
        /// 最大距离
        /// </summary>
        private float _maxDistance;

        /// <summary>
        /// 最小俯仰角（角度）
        /// </summary>
        private float _minPitch;

        /// <summary>
        /// 最大俯仰角（角度）
        /// </summary>
        private float _maxPitch;

        /// <summary>
        /// 平移速度
        /// </summary>
        private float _moveSpeed;

        /// <summary>
        /// 旋转速度
        /// </summary>
        private float _rotateSpeed;

        /// <summary>
        /// 缩放速度
        /// </summary>
        private float _zoomSpeed;

        /// <summary>
        /// 世界坐标系上方向
        /// </summary>
        private Vector3 _worldUpDirection;

        /// <summary>
        /// 当前坐标系类型
        /// </summary>
        private CoordinateSystem _coordinateSystem;

        /// <summary>
        /// 创建轨道相机构造器
        /// </summary>
        /// <param name="targetPosition">目标位置</param>
        /// <param name="distance">相机到目标的距离</param>
        /// <param name="yaw">偏航角-RY（角度）</param>
        /// <param name="pitch">俯仰角-RX（角度）</param>
        /// <param name="worldUpDirection">世界坐标系上方向</param>
        /// <param name="nearPlaneDistance">近平面距离</param>
        /// <param name="farPlaneDistance">远平面距离</param>
        protected OrbitCamera(Vector3 targetPosition = default, float distance = 5.0f, float yaw = 0.0f, float pitch = 0.0f, Vector3 worldUpDirection = default, float nearPlaneDistance = 0.125f, float farPlaneDistance = 65535.0f)
            : base(nearPlaneDistance, farPlaneDistance)
        {
            //默认值
            this._minDistance = 0.1f;
            this._maxDistance = 100.0f;
            this._minPitch = -89.0f;
            this._maxPitch = 89.0f;
            this._moveSpeed = 2.0f;
            this._rotateSpeed = 0.1f;
            this._zoomSpeed = 0.5f;

            //设置世界坐标系上方向（默认 Y-up）
            Vector3 up = worldUpDirection == default
                ? new Vector3(0, 1, 0)
                : Vector3.Normalize(worldUpDirection);
            this.SetWorldUpDirectionInternal(up);

            // 设置相机参数
            this._targetPosition = targetPosition == default ? Vector3.Zero : targetPosition;
            this._distance = MathHelper.Clamp(distance, this._minDistance, this._maxDistance);
            this._yaw = yaw;
            this._pitch = MathHelper.Clamp(pitch, this._minPitch, this._maxPitch);

            // 根据角度计算方向
            this._lookDirection = CalculateDirectionFromAngles(this._yaw, this._pitch);
            this._cameraPosition = this._targetPosition - this._lookDirection * this._distance;

            //更新相机坐标系
            this.UpdateCameraVectors();
            this.UpdateViewMatrix();
        }

        /// <summary>
        /// 创建轨道相机构造器
        /// </summary>
        /// <param name="cameraPosition">相机位置</param>
        /// <param name="targetPosition">目标位置</param>
        /// <param name="worldUpDirection">世界坐标系上方向</param>
        /// <param name="nearPlaneDistance">近平面距离</param>
        /// <param name="farPlaneDistance">远平面距离</param>
        protected OrbitCamera(Vector3 cameraPosition, Vector3 targetPosition, Vector3 worldUpDirection, float nearPlaneDistance = 0.125f, float farPlaneDistance = 65535.0f)
            : base(nearPlaneDistance, farPlaneDistance)
        {
            //参数验证
            if (worldUpDirection.LengthSquared < float.Epsilon)
            {
                throw new ArgumentException("世界坐标系上方向向量不能为零", nameof(worldUpDirection));
            }

            //初始化默认值
            this._minDistance = 0.1f;
            this._maxDistance = 100.0f;
            this._minPitch = -89.0f;
            this._maxPitch = 89.0f;
            this._moveSpeed = 2.0f;
            this._rotateSpeed = 0.1f;
            this._zoomSpeed = 0.5f;

            //设置世界坐标系上方向
            this.SetWorldUpDirectionInternal(Vector3.Normalize(worldUpDirection));

            //设置相机位置和目标位置
            this._cameraPosition = cameraPosition;
            this._targetPosition = targetPosition;

            //计算视角方向
            Vector3 lookDirectionRaw = this._targetPosition - this._cameraPosition;
            float distance = lookDirectionRaw.Length;
            if (distance < float.Epsilon)
            {
                throw new InvalidOperationException("相机位置和目标位置重合，无法计算视角方向");
            }

            this._lookDirection = Vector3.Normalize(lookDirectionRaw);
            this._distance = MathHelper.Clamp(distance, this._minDistance, this._maxDistance);

            //从方向计算角度
            this.CalculateAnglesFromDirection(this._lookDirection);

            //更新相机坐标系
            this.UpdateCameraVectors();
            this.UpdateViewMatrix();
        }

        #endregion

        #region # 属性

        #region 相机位置 —— override Vector3 CameraPosition
        /// <summary>
        /// 相机位置
        /// </summary>
        private Vector3 _cameraPosition;

        /// <summary>
        /// 相机位置
        /// </summary>
        public override Vector3 CameraPosition => this._cameraPosition;
        #endregion

        #region 视角方向 —— override Vector3 LookDirection
        /// <summary>
        /// 视角方向
        /// </summary>
        private Vector3 _lookDirection;

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
                this._targetPosition = value;
                this.UpdateCameraVectors();
                this.UpdateViewMatrix();
            }
        }
        #endregion

        #region 相机到目标的距离 —— float Distance
        /// <summary>
        /// 相机到目标的距离
        /// </summary>
        private float _distance;

        /// <summary>
        /// 相机到目标的距离
        /// </summary>
        public float Distance
        {
            get => this._distance;
            set
            {
                this._distance = MathHelper.Clamp(value, this._minDistance, this._maxDistance);
                this.UpdateCameraVectors();
                this.UpdateViewMatrix();
            }
        }
        #endregion

        #region 偏航角 —— float Yaw
        /// <summary>
        /// 偏航角（角度）
        /// </summary>
        public float Yaw => this._yaw;
        #endregion

        #region 俯仰角 —— float Pitch
        /// <summary>
        /// 俯仰角（角度）
        /// </summary>
        public float Pitch => this._pitch;
        #endregion

        #region 世界坐标系上方向 —— Vector3 WorldUpDirection
        /// <summary>
        /// 世界坐标系上方向
        /// </summary>
        public Vector3 WorldUpDirection => this._worldUpDirection;
        #endregion

        #region 当前坐标系类型 —— CoordinateSystem CoordinateSystem
        /// <summary>
        /// 当前坐标系类型
        /// </summary>
        public CoordinateSystem CoordinateSystem => this._coordinateSystem;
        #endregion

        #endregion

        #region # 方法

        //Public

        #region 设置移动速度 —— void SetMoveSpeed(float speed)
        /// <summary>
        /// 设置移动速度
        /// </summary>
        /// <param name="speed">移动速度</param>
        public void SetMoveSpeed(float speed)
        {
            this._moveSpeed = speed;
        }
        #endregion

        #region 设置旋转速度 —— void SetRotateSpeed(float speed)
        /// <summary>
        /// 设置旋转速度
        /// </summary>
        /// <param name="speed">旋转速度</param>
        public void SetRotateSpeed(float speed)
        {
            this._rotateSpeed = speed;
        }
        #endregion

        #region 设置缩放速度 —— void SetZoomSpeed(float speed)
        /// <summary>
        /// 设置缩放速度
        /// </summary>
        /// <param name="speed">缩放速度</param>
        public void SetZoomSpeed(float speed)
        {
            this._zoomSpeed = speed;
        }
        #endregion

        #region 设置旋转 —— void SetRotation(float yaw, float pitch)
        /// <summary>
        /// 设置旋转
        /// </summary>
        /// <param name="yaw">偏航角</param>
        /// <param name="pitch">俯仰角</param>
        public void SetRotation(float yaw, float pitch)
        {
            this._yaw = yaw;
            this._pitch = MathHelper.Clamp(pitch, this._minPitch, this._maxPitch);
            this.UpdateCameraVectors();
            this.UpdateViewMatrix();
        }
        #endregion

        #region 设置相机到目标距离限制 —— void SetDistanceLimits(float minDistance, float maxDistance)
        /// <summary>
        /// 设置相机到目标距离限制
        /// </summary>
        /// <param name="minDistance">最小距离</param>
        /// <param name="maxDistance">最大距离</param>
        public void SetDistanceLimits(float minDistance, float maxDistance)
        {
            this._minDistance = minDistance;
            this._maxDistance = maxDistance;
            this._distance = Math.Clamp(this._distance, this._minDistance, this._maxDistance);
            this.UpdateCameraVectors();
            this.UpdateViewMatrix();
        }
        #endregion

        #region 设置俯仰角限制 —— void SetPitchLimits(float minPitch, float maxPitch)
        /// <summary>
        /// 设置俯仰角限制
        /// </summary>
        /// <param name="minPitch">最小俯仰角</param>
        /// <param name="maxPitch">最大俯仰角</param>
        public void SetPitchLimits(float minPitch, float maxPitch)
        {
            this._minPitch = minPitch;
            this._maxPitch = maxPitch;
            this._pitch = Math.Clamp(this._pitch, this._minPitch, this._maxPitch);
            this.UpdateCameraVectors();
            this.UpdateViewMatrix();
        }
        #endregion

        /// <summary>
        /// 设置世界坐标系上方向（支持 X-up、Y-up、Z-up）
        /// </summary>
        public void SetWorldUpDirection(Vector3 worldUp)
        {
            if (worldUp.LengthSquared < float.Epsilon)
            {
                return;
            }

            Vector3 newWorldUp = Vector3.Normalize(worldUp);

            // 检查是否方向改变
            bool isSameDirection = Math.Abs(Vector3.Dot(this._worldUpDirection, newWorldUp)) > 0.9999f;

            this.SetWorldUpDirectionInternal(newWorldUp);

            // 坐标系改变时重新计算角度
            if (!isSameDirection)
            {
                this.CalculateAnglesFromDirection(this._lookDirection);
            }

            this.UpdateCameraVectors();
            this.UpdateViewMatrix();
        }

        /// <summary>
        /// 设置坐标系类型
        /// </summary>
        public void SetCoordinateSystem(CoordinateSystem system)
        {
            Vector3 up = system switch
            {
                CoordinateSystem.XUp => new Vector3(1, 0, 0),
                CoordinateSystem.ZUp => new Vector3(0, 0, 1),
                _ => new Vector3(0, 1, 0)
            };
            this.SetWorldUpDirection(up);
        }

        #region 旋转相机 —— void Rotate(float deltaYaw, float deltaPitch)
        /// <summary>
        /// 旋转相机
        /// </summary>
        /// <param name="deltaYaw">偏航角变化量</param>
        /// <param name="deltaPitch">俯仰角变化量</param>
        public void Rotate(float deltaYaw, float deltaPitch)
        {
            this._yaw += deltaYaw * this._rotateSpeed;
            this._pitch += deltaPitch * this._rotateSpeed;

            // 限制俯仰角范围
            this._pitch = Math.Clamp(this._pitch, this._minPitch, this._maxPitch);

            // 规范化偏航角到[0, 360]范围
            if (this._yaw > 360.0f)
            {
                this._yaw -= 360.0f;
            }
            if (this._yaw < 0.0f)
            {
                this._yaw += 360.0f;
            }

            this.UpdateCameraVectors();
            this.UpdateViewMatrix();
        }
        #endregion

        #region 缩放相机 —— void Zoom(float delta)
        /// <summary>
        /// 缩放相机
        /// </summary>
        /// <param name="delta">缩放变化量</param>
        public void Zoom(float delta)
        {
            this._distance -= delta * this._zoomSpeed;
            this._distance = MathHelper.Clamp(this._distance, this._minDistance, this._maxDistance);
            this.UpdateCameraVectors();
            this.UpdateViewMatrix();
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
            // 计算平移向量
            float actualMoveSpeed = this._moveSpeed * this._distance * 0.01f;
            Vector3 panOffset = this._rightDirection * (-deltaX * actualMoveSpeed) + this._upDirection * (deltaY * actualMoveSpeed);

            // 平移目标点和相机位置
            this._targetPosition += panOffset;

            this.UpdateCameraVectors();
            this.UpdateViewMatrix();
        }
        #endregion

        #region 重置相机位置 —— void Reset()
        /// <summary>
        /// 重置相机位置
        /// </summary>
        public void Reset()
        {
            this._targetPosition = Vector3.Zero;
            this._distance = 5.0f;
            this._yaw = 0.0f;
            this._pitch = 0.0f;

            this.UpdateCameraVectors();
            this.UpdateViewMatrix();
        }
        #endregion


        //Private

        #region 更新相机向量 —— void UpdateCameraVectors()
        /// <summary>
        /// 更新相机向量
        /// </summary>
        private void UpdateCameraVectors()
        {
            //从角度计算视线方向
            this._lookDirection = CalculateDirectionFromAngles(this._yaw, this._pitch);

            //计算相机位置
            this._cameraPosition = this._targetPosition - this._lookDirection * this._distance;

            //计算右方向和上方向
            float dot = Math.Abs(Vector3.Dot(this._worldUpDirection, this._lookDirection));

            //当视线与世界上方向平行时，选择备选方向
            Vector3 upReference = dot > 0.9999f ? GetAlternativeUpDirection() : this._worldUpDirection;

            //计算右方向：look × up
            this._rightDirection = Vector3.Normalize(Vector3.Cross(this._lookDirection, upReference));

            //计算真正的上方向：right × look
            this._upDirection = Vector3.Normalize(Vector3.Cross(this._rightDirection, this._lookDirection));
        }
        #endregion

        #region 更新视图矩阵 —— void UpdateViewMatrix()
        /// <summary>
        /// 更新视图矩阵
        /// </summary>
        private void UpdateViewMatrix()
        {
            this._viewMatrix = Matrix4.LookAt(this._cameraPosition, this._targetPosition, this._upDirection);
        }
        #endregion

        #region 设置世界坐标系上方向 —— 
        /// <summary>
        /// 设置世界坐标系上方向
        /// </summary>
        /// <param name="worldUpDirection">世界坐标系上方向</param>
        private void SetWorldUpDirectionInternal(Vector3 worldUpDirection)
        {
            this._worldUpDirection = worldUpDirection;

            //使用点积判断方向，避免浮点精度问题
            if (Math.Abs(Vector3.Dot(worldUpDirection, new Vector3(1, 0, 0))) > 0.9999f)
            {
                this._coordinateSystem = CoordinateSystem.XUp;
            }
            else if (Math.Abs(Vector3.Dot(worldUpDirection, new Vector3(0, 0, 1))) > 0.9999f)
            {
                this._coordinateSystem = CoordinateSystem.ZUp;
            }
            else
            {
                this._coordinateSystem = CoordinateSystem.YUp;
            }
        }
        #endregion

        /// <summary>
        /// 从方向向量计算偏航角和俯仰角（根据世界坐标系上方向）
        /// </summary>
        private void CalculateAnglesFromDirection(Vector3 direction)
        {
            direction = Vector3.Normalize(direction);

            switch (this._coordinateSystem)
            {
                case CoordinateSystem.YUp:
                    // Y-up: 偏航角绕 Y 轴，俯仰角绕 X 轴
                    this._yaw = MathHelper.RadiansToDegrees((float)Math.Atan2(direction.Z, direction.X));
                    this._pitch = MathHelper.RadiansToDegrees((float)Math.Asin(MathHelper.Clamp(direction.Y, -1.0f, 1.0f)));
                    break;

                case CoordinateSystem.ZUp:
                    // Z-up: 偏航角绕 Z 轴，俯仰角绕 X 轴
                    this._yaw = MathHelper.RadiansToDegrees((float)Math.Atan2(direction.Y, direction.X));
                    float xyLength = (float)Math.Sqrt(direction.X * direction.X + direction.Y * direction.Y);
                    this._pitch = MathHelper.RadiansToDegrees((float)Math.Atan2(direction.Z, xyLength));
                    break;

                case CoordinateSystem.XUp:
                    // X-up: 偏航角绕 X 轴，俯仰角绕 Y 轴
                    this._yaw = MathHelper.RadiansToDegrees((float)Math.Atan2(direction.Z, direction.Y));
                    float yzLength = (float)Math.Sqrt(direction.Y * direction.Y + direction.Z * direction.Z);
                    this._pitch = MathHelper.RadiansToDegrees((float)Math.Atan2(direction.X, yzLength));
                    break;
            }

            // 规范化偏航角到 [0, 360)
            if (this._yaw < 0) this._yaw += 360.0f;

            // 限制俯仰角
            this._pitch = Math.Clamp(this._pitch, this._minPitch, this._maxPitch);
        }

        /// <summary>
        /// 从偏航角和俯仰角计算方向向量（根据世界坐标系上方向）
        /// </summary>
        private Vector3 CalculateDirectionFromAngles(float yaw, float pitch)
        {
            float yawRad = MathHelper.DegreesToRadians(yaw);
            float pitchRad = MathHelper.DegreesToRadians(pitch);
            float cosPitch = (float)Math.Cos(pitchRad);
            float sinPitch = (float)Math.Sin(pitchRad);
            float cosYaw = (float)Math.Cos(yawRad);
            float sinYaw = (float)Math.Sin(yawRad);

            switch (this._coordinateSystem)
            {
                case CoordinateSystem.YUp:
                    // Y-up: 方向向量 = (cos(yaw)*cos(pitch), sin(pitch), sin(yaw)*cos(pitch))
                    return new Vector3(
                        cosYaw * cosPitch,
                        sinPitch,
                        sinYaw * cosPitch
                    ).Normalized();

                case CoordinateSystem.ZUp:
                    // Z-up: 方向向量 = (cos(yaw)*cos(pitch), sin(yaw)*cos(pitch), sin(pitch))
                    return new Vector3(
                        cosYaw * cosPitch,
                        -sinYaw * cosPitch,
                        sinPitch
                    ).Normalized();

                case CoordinateSystem.XUp:

                    // X-up: 方向向量 = (sin(pitch), cos(yaw)*cos(pitch), sin(yaw)*cos(pitch))
                    return new Vector3(
                        -sinPitch,          // X = sin(pitch)
                        cosPitch * cosYaw, // Y = cos(pitch)*cosYaw
                        cosPitch * sinYaw  // Z = cos(pitch)*sinYaw
                    ).Normalized();

                default:
                    return Vector3.UnitZ;
            }
        }

        /// <summary>
        /// 获取当前坐标系下的备选上方向（当视线与世界上方向平行时使用）
        /// </summary>
        private Vector3 GetAlternativeUpDirection()
        {
            return this._coordinateSystem switch
            {
                CoordinateSystem.XUp => Vector3.UnitY,
                CoordinateSystem.ZUp => Vector3.UnitY,
                _ => Vector3.UnitZ
            };
        }

        #endregion
    }
}
