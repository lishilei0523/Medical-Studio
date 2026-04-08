using MedicalSharp.Primitives.Enums;
using OpenTK.Mathematics;
using System;

namespace MedicalSharp.Primitives.Cameras
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
        /// 创建轨道相机构造器
        /// </summary>
        /// <param name="targetPosition">目标位置</param>
        /// <param name="distance">相机到目标距离</param>
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

            //设置世界坐标系上方向（默认Y-up）
            worldUpDirection = worldUpDirection == default
                ? new Vector3(0, 1, 0)
                : Vector3.Normalize(worldUpDirection);
            this.SetWorldUpDirectionInternal(worldUpDirection);

            //设置相机参数
            this._targetPosition = targetPosition == default ? Vector3.Zero : targetPosition;
            this._distance = MathHelper.Clamp(distance, this._minDistance, this._maxDistance);
            this._yaw = yaw;
            this._pitch = MathHelper.Clamp(pitch, this._minPitch, this._maxPitch);

            //从角度计算方向
            this._lookDirection = this.CalculateLookDirection(this._yaw, this._pitch);
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
            #region # 验证

            if (worldUpDirection.LengthSquared < float.Epsilon)
            {
                throw new ArgumentOutOfRangeException(nameof(worldUpDirection), "世界坐标系上方向向量不能为零");
            }

            #endregion

            //默认值
            this._minDistance = 0.1f;
            this._maxDistance = 100.0f;
            this._minPitch = -89.0f;
            this._maxPitch = 89.0f;
            this._moveSpeed = 2.0f;
            this._rotateSpeed = 0.1f;
            this._zoomSpeed = 0.5f;

            //设置世界坐标系上方向
            worldUpDirection = Vector3.Normalize(worldUpDirection);
            this.SetWorldUpDirectionInternal(worldUpDirection);

            //设置相机位置和目标位置
            this._cameraPosition = cameraPosition;
            this._targetPosition = targetPosition;

            //计算视角方向
            Vector3 lookDirectionRaw = this._targetPosition - this._cameraPosition;
            float distance = lookDirectionRaw.Length;

            #region # 验证

            if (distance < float.Epsilon)
            {
                throw new InvalidOperationException("相机位置和目标位置重合，无法计算视角方向");
            }

            #endregion

            this._lookDirection = Vector3.Normalize(lookDirectionRaw);
            this._distance = MathHelper.Clamp(distance, this._minDistance, this._maxDistance);

            //从方向计算角度
            this.CalculateAngles(this._lookDirection);

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

        #region 相机到目标距离 —— float Distance
        /// <summary>
        /// 相机到目标距离
        /// </summary>
        private float _distance;

        /// <summary>
        /// 相机到目标距离
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
        /// 偏航角-RY（角度）
        /// </summary>
        private float _yaw;

        /// <summary>
        /// 偏航角-RY（角度）
        /// </summary>
        public float Yaw => this._yaw;
        #endregion

        #region 俯仰角 —— float Pitch
        /// <summary>
        /// 俯仰角-RX（角度）
        /// </summary>
        private float _pitch;

        /// <summary>
        /// 俯仰角-RX（角度）
        /// </summary>
        public float Pitch => this._pitch;
        #endregion

        #region 世界坐标系上方向 —— Vector3 WorldUpDirection
        /// <summary>
        /// 世界坐标系上方向
        /// </summary>
        private Vector3 _worldUpDirection;

        /// <summary>
        /// 世界坐标系上方向
        /// </summary>
        public Vector3 WorldUpDirection => this._worldUpDirection;
        #endregion

        #region 当前坐标系类型 —— CoordinateType CoordinateType
        /// <summary>
        /// 当前坐标系类型
        /// </summary>
        private CoordinateType _coordinateType;

        /// <summary>
        /// 当前坐标系类型
        /// </summary>
        public CoordinateType CoordinateType => this._coordinateType;
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

        #region 设置坐标系类型 —— void SetCoordinateType(CoordinateType coordinateSystem)
        /// <summary>
        /// 设置坐标系类型
        /// </summary>
        /// <param name="coordinateSystem">坐标系类型</param>
        public void SetCoordinateType(CoordinateType coordinateSystem)
        {
            Vector3 worldUpDirection = coordinateSystem switch
            {
                CoordinateType.XUp => new Vector3(1, 0, 0),
                CoordinateType.YUp => new Vector3(0, 1, 0),
                CoordinateType.ZUp => new Vector3(0, 0, 1),
                _ => new Vector3(0, 1, 0)
            };
            this.SetWorldUpDirection(worldUpDirection);
        }
        #endregion

        #region 设置世界坐标系上方向 —— void SetWorldUpDirection(Vector3 worldUpDirection)
        /// <summary>
        /// 设置世界坐标系上方向
        /// </summary>
        /// <param name="worldUpDirection">世界坐标系上方向</param>
        /// <remarks>支持 X-up、Y-up、Z-up</remarks>
        public void SetWorldUpDirection(Vector3 worldUpDirection)
        {
            #region # 验证

            if (worldUpDirection.LengthSquared < float.Epsilon)
            {
                return;
            }

            #endregion

            worldUpDirection = Vector3.Normalize(worldUpDirection);

            //检查是否方向改变
            bool isSameDirection = Math.Abs(Vector3.Dot(this._worldUpDirection, worldUpDirection)) > 0.9999f;

            this.SetWorldUpDirectionInternal(worldUpDirection);

            //坐标系改变时重新计算角度
            if (!isSameDirection)
            {
                this.CalculateAngles(this._lookDirection);
            }

            this.UpdateCameraVectors();
            this.UpdateViewMatrix();
        }
        #endregion

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

            //限制俯仰角范围
            this._pitch = Math.Clamp(this._pitch, this._minPitch, this._maxPitch);

            //规范化偏航角到[0, 360]范围
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
            //计算平移向量
            float actualMoveSpeed = this._moveSpeed * this._distance * 0.01f;
            Vector3 panOffset = this._rightDirection * (-deltaX * actualMoveSpeed) + this._upDirection * (deltaY * actualMoveSpeed);

            //平移目标点和相机位置
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
            this._lookDirection = this.CalculateLookDirection(this._yaw, this._pitch);

            //计算相机位置
            this._cameraPosition = this._targetPosition - this._lookDirection * this._distance;

            //计算右方向和上方向
            float dot = Math.Abs(Vector3.Dot(this._worldUpDirection, this._lookDirection));

            //当视线与世界上方向平行时，选择备选方向
            Vector3 upDirection = dot > 0.9999f ? this.GetAlternativeUpDirection() : this._worldUpDirection;

            //计算右方向：look × up
            this._rightDirection = Vector3.Normalize(Vector3.Cross(this._lookDirection, upDirection));

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

        #region 设置世界坐标系上方向 —— void SetWorldUpDirectionInternal(Vector3 worldUpDirection)
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
                this._coordinateType = CoordinateType.XUp;
            }
            else if (Math.Abs(Vector3.Dot(worldUpDirection, new Vector3(0, 0, 1))) > 0.9999f)
            {
                this._coordinateType = CoordinateType.ZUp;
            }
            else
            {
                this._coordinateType = CoordinateType.YUp;
            }
        }
        #endregion

        #region 计算视角方向 —— Vector3 CalculateLookDirection(float yaw, float pitch)
        /// <summary>
        /// 计算视角方向
        /// </summary>
        /// <param name="yaw">偏航角-RY（角度）</param>
        /// <param name="pitch">俯仰角-RX（角度）</param>
        /// <returns>视角方向</returns>
        /// <remarks>根据世界坐标系上方向</remarks>
        private Vector3 CalculateLookDirection(float yaw, float pitch)
        {
            float yawRad = MathHelper.DegreesToRadians(yaw);
            float pitchRad = MathHelper.DegreesToRadians(pitch);
            float cosPitch = MathF.Cos(pitchRad);
            float sinPitch = MathF.Sin(pitchRad);
            float cosYaw = MathF.Cos(yawRad);
            float sinYaw = MathF.Sin(yawRad);

            switch (this._coordinateType)
            {
                case CoordinateType.XUp:  //X-up: 方向向量 = (-sin(pitch), cos(yaw)*cos(pitch), sin(yaw)*cos(pitch))
                    return new Vector3(-sinPitch, cosPitch * cosYaw, cosPitch * sinYaw).Normalized();

                case CoordinateType.YUp:  //Y-up: 方向向量 = (cos(yaw)*cos(pitch), sin(pitch), sin(yaw)*cos(pitch))
                    return new Vector3(cosYaw * cosPitch, sinPitch, sinYaw * cosPitch).Normalized();

                case CoordinateType.ZUp:  //Z-up: 方向向量 = (cos(yaw)*cos(pitch), -sin(yaw)*cos(pitch), sin(pitch))
                    return new Vector3(cosYaw * cosPitch, -sinYaw * cosPitch, sinPitch).Normalized();

                default:
                    return Vector3.UnitZ;
            }
        }
        #endregion

        #region 计算偏航角和俯仰角 —— void CalculateAngles(Vector3 lookDirection)
        /// <summary>
        /// 计算偏航角和俯仰角
        /// </summary>
        /// <param name="lookDirection">视角方向</param>
        /// <remarks>根据世界坐标系上方向</remarks>
        private void CalculateAngles(Vector3 lookDirection)
        {
            switch (this._coordinateType)
            {
                case CoordinateType.XUp:  //X-up: 偏航角绕X轴，俯仰角绕Y轴
                    this._yaw = MathHelper.RadiansToDegrees(MathF.Atan2(lookDirection.Z, lookDirection.Y));
                    float yzLength = MathF.Sqrt(lookDirection.Y * lookDirection.Y + lookDirection.Z * lookDirection.Z);
                    this._pitch = MathHelper.RadiansToDegrees(MathF.Atan2(lookDirection.X, yzLength));
                    break;

                case CoordinateType.YUp:  //Y-up: 偏航角绕Y轴，俯仰角绕X轴
                    this._yaw = MathHelper.RadiansToDegrees(MathF.Atan2(lookDirection.Z, lookDirection.X));
                    this._pitch = MathHelper.RadiansToDegrees(MathF.Asin(MathHelper.Clamp(lookDirection.Y, -1.0f, 1.0f)));
                    break;

                case CoordinateType.ZUp:  //Z-up: 偏航角绕Z轴，俯仰角绕X轴
                    this._yaw = MathHelper.RadiansToDegrees(MathF.Atan2(lookDirection.Y, lookDirection.X));
                    float xyLength = MathF.Sqrt(lookDirection.X * lookDirection.X + lookDirection.Y * lookDirection.Y);
                    this._pitch = MathHelper.RadiansToDegrees(MathF.Atan2(lookDirection.Z, xyLength));
                    break;
            }

            //标准化偏航角到[0, 360)
            if (this._yaw < 0)
            {
                this._yaw += 360.0f;
            }

            //限制俯仰角
            this._pitch = Math.Clamp(this._pitch, this._minPitch, this._maxPitch);
        }
        #endregion

        #region 获取备选上方向 —— Vector3 GetAlternativeUpDirection()
        /// <summary>
        /// 获取备选上方向
        /// </summary>
        /// <remarks>当视线与世界上方向平行时使用</remarks>
        private Vector3 GetAlternativeUpDirection()
        {
            return this._coordinateType switch
            {
                CoordinateType.XUp => Vector3.UnitY,
                CoordinateType.YUp => Vector3.UnitZ,
                CoordinateType.ZUp => Vector3.UnitY,
                _ => Vector3.UnitZ
            };
        }
        #endregion

        #endregion
    }
}
