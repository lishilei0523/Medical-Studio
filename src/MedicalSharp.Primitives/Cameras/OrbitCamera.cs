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
            this.SetDefaultValues();

            //设置世界坐标系上方向
            worldUpDirection = Vector3.Normalize(worldUpDirection);
            this.SetWorldUpDirectionInternal(worldUpDirection);

            //设置相机位置和目标位置
            this.CameraPosition = cameraPosition;
            this.TargetPosition = targetPosition;

            //计算视角方向
            Vector3 lookDirectionRaw = this.TargetPosition - this.CameraPosition;
            float distance = lookDirectionRaw.Length;

            #region # 验证

            if (distance < float.Epsilon)
            {
                throw new InvalidOperationException("相机位置和目标位置重合，无法计算视角方向");
            }

            #endregion

            this.LookDirection = Vector3.Normalize(lookDirectionRaw);
            this.Distance = Math.Clamp(distance, this.MinDistance, this.MaxDistance);

            //从方向计算角度
            this.CalculateAngles(this.LookDirection);

            //更新相机坐标系
            this.UpdateCameraVectors();
            this.UpdateViewMatrix();
        }

        #endregion

        #region # 属性

        #region 平移速度 —— float MoveSpeed
        /// <summary>
        /// 平移速度
        /// </summary>
        public float MoveSpeed { get; private set; }
        #endregion

        #region 旋转速度 —— float RotateSpeed
        /// <summary>
        /// 旋转速度
        /// </summary>
        public float RotateSpeed { get; private set; }
        #endregion

        #region 缩放速度 —— float ZoomSpeed
        /// <summary>
        /// 缩放速度
        /// </summary>
        public float ZoomSpeed { get; private set; }
        #endregion

        #region 最小距离 —— float MinDistance
        /// <summary>
        /// 最小距离
        /// </summary>
        public float MinDistance { get; private set; }
        #endregion

        #region 最大距离 —— float MaxDistance
        /// <summary>
        /// 最大距离
        /// </summary>
        public float MaxDistance { get; private set; }
        #endregion

        #region 最小俯仰角 —— float MinPitch
        /// <summary>
        /// 最小俯仰角
        /// </summary>
        /// <remarks>角度</remarks>
        public float MinPitch { get; private set; }
        #endregion

        #region 最大俯仰角 —— float MaxPitch
        /// <summary>
        /// 最大俯仰角
        /// </summary>
        /// <remarks>角度</remarks>
        public float MaxPitch { get; private set; }
        #endregion

        #region 世界坐标系上方向 —— Vector3 WorldUpDirection
        /// <summary>
        /// 世界坐标系上方向
        /// </summary>
        public Vector3 WorldUpDirection { get; protected set; }
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

        #region 偏航角 —— float Yaw
        /// <summary>
        /// 偏航角
        /// </summary>
        /// <remarks>RY（角度）</remarks>
        public float Yaw { get; protected set; }
        #endregion

        #region 俯仰角 —— float Pitch
        /// <summary>
        /// 俯仰角
        /// </summary>
        /// <remarks>RX（角度）</remarks>
        public float Pitch { get; protected set; }
        #endregion

        #endregion

        #region # 方法

        //Public

        #region 设置速度 —— void SetSpeeds(float moveSpeed, float rotateSpeed...
        /// <summary>
        /// 设置速度
        /// </summary>
        /// <param name="moveSpeed">移动速度</param>
        /// <param name="rotateSpeed">旋转速度</param>
        /// <param name="zoomSpeed">缩放速度</param>
        public void SetSpeeds(float moveSpeed, float rotateSpeed, float zoomSpeed)
        {
            this.MoveSpeed = moveSpeed;
            this.RotateSpeed = rotateSpeed;
            this.ZoomSpeed = zoomSpeed;
        }
        #endregion

        #region 设置距离限制 —— void SetDistanceLimits(float minDistance, float maxDistance)
        /// <summary>
        /// 设置距离限制
        /// </summary>
        /// <param name="minDistance">最小距离</param>
        /// <param name="maxDistance">最大距离</param>
        public void SetDistanceLimits(float minDistance, float maxDistance)
        {
            this.MinDistance = minDistance;
            this.MaxDistance = maxDistance;
            this.Distance = Math.Clamp(this.Distance, this.MinDistance, this.MaxDistance);
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
            this.MinPitch = minPitch;
            this.MaxPitch = maxPitch;
            this.Pitch = Math.Clamp(this.Pitch, this.MinPitch, this.MaxPitch);
            this.UpdateCameraVectors();
            this.UpdateViewMatrix();
        }
        #endregion

        #region 设置世界坐标系 —— void SetWorldCoordinate(CoordinateType coordinateSystem)
        /// <summary>
        /// 设置世界坐标系
        /// </summary>
        /// <param name="coordinateSystem">坐标系类型</param>
        public void SetWorldCoordinate(CoordinateType coordinateSystem)
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
        /// <remarks>支持X-up、Y-up、Z-up</remarks>
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
            bool isSameDirection = Math.Abs(Vector3.Dot(this.WorldUpDirection, worldUpDirection)) > 0.9999f;

            this.SetWorldUpDirectionInternal(worldUpDirection);

            //坐标系改变时重新计算角度
            if (!isSameDirection)
            {
                this.CalculateAngles(this.LookDirection);
            }

            this.UpdateCameraVectors();
            this.UpdateProjectionMatrix();
            this.UpdateViewMatrix();
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
            this.Yaw = yaw;
            this.Pitch = Math.Clamp(pitch, this.MinPitch, this.MaxPitch);
            this.UpdateCameraVectors();
            this.UpdateProjectionMatrix();
            this.UpdateViewMatrix();
        }
        #endregion

        #region 看向指定位置 —— override void LookAt(Vector3 targetPosition)
        /// <summary>
        /// 看向指定位置
        /// </summary>
        /// <param name="targetPosition">目标位置（世界坐标）</param>
        public override void LookAt(Vector3 targetPosition)
        {
            this.TargetPosition = targetPosition;
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
            this.Yaw += deltaYaw * this.RotateSpeed;
            this.Pitch += deltaPitch * this.RotateSpeed;

            //限制俯仰角范围
            this.Pitch = Math.Clamp(this.Pitch, this.MinPitch, this.MaxPitch);

            //规范化偏航角到[0, 360]范围
            if (this.Yaw > 360.0f)
            {
                this.Yaw -= 360.0f;
            }
            if (this.Yaw < 0.0f)
            {
                this.Yaw += 360.0f;
            }

            this.UpdateCameraVectors();
            this.UpdateProjectionMatrix();
            this.UpdateViewMatrix();
        }
        #endregion

        #region 缩放相机 —— override void Zoom(float delta)
        /// <summary>
        /// 缩放相机
        /// </summary>
        /// <param name="delta">缩放变化量</param>
        public override void Zoom(float delta)
        {
            this.Distance -= delta * this.ZoomSpeed;
            this.Distance = Math.Clamp(this.Distance, this.MinDistance, this.MaxDistance);
            this.UpdateCameraVectors();
            this.UpdateProjectionMatrix();
            this.UpdateViewMatrix();
        }
        #endregion

        #region 平移相机 —— override void Pan(float deltaX, float deltaY)
        /// <summary>
        /// 平移相机
        /// </summary>
        /// <param name="deltaX">水平平移量</param>
        /// <param name="deltaY">垂直平移量</param>
        public override void Pan(float deltaX, float deltaY)
        {
            //计算平移向量
            float actualMoveSpeed = this.MoveSpeed * this.Distance * 0.01f;
            Vector3 panOffset = this.RightDirection * (-deltaX * actualMoveSpeed) + this.UpDirection * (deltaY * actualMoveSpeed);

            //平移目标点和相机位置
            this.TargetPosition += panOffset;

            this.UpdateCameraVectors();
            this.UpdateProjectionMatrix();
            this.UpdateViewMatrix();
        }
        #endregion

        #region 重置相机 —— override void Reset()
        /// <summary>
        /// 重置相机
        /// </summary>
        public override void Reset()
        {
            this.TargetPosition = Vector3.Zero;
            this.Distance = 5.0f;
            this.Yaw = 0.0f;
            this.Pitch = 0.0f;

            this.UpdateCameraVectors();
            this.UpdateProjectionMatrix();
            this.UpdateViewMatrix();
        }
        #endregion


        //Private

        #region 设置默认值 —— void SetDefaultValues()
        /// <summary>
        /// 设置默认值
        /// </summary>
        private void SetDefaultValues()
        {
            this.MinDistance = 0.1f;
            this.MaxDistance = 100.0f;
            this.MinPitch = -89.0f;
            this.MaxPitch = 89.0f;
            this.MoveSpeed = 3.0f;
            this.RotateSpeed = 0.15f;
            this.ZoomSpeed = 0.5f;
        }
        #endregion

        #region 更新相机向量 —— override void UpdateCameraVectors()
        /// <summary>
        /// 更新相机向量
        /// </summary>
        protected override void UpdateCameraVectors()
        {
            //从角度计算视线方向
            this.LookDirection = this.CalculateLookDirection(this.Yaw, this.Pitch);

            //计算相机位置
            this.CameraPosition = this.TargetPosition - this.LookDirection * this.Distance;

            //计算右方向和上方向
            float dot = Math.Abs(Vector3.Dot(this.WorldUpDirection, this.LookDirection));

            //当视线与世界上方向平行时，选择备选方向
            Vector3 upDirection = dot > 0.9999f ? this.GetAlternativeUpDirection() : this.WorldUpDirection;

            //计算右方向：look × up
            this.RightDirection = Vector3.Normalize(Vector3.Cross(this.LookDirection, upDirection));

            //计算真正的上方向：right × look
            this.UpDirection = Vector3.Normalize(Vector3.Cross(this.RightDirection, this.LookDirection));
        }
        #endregion

        #region 设置世界坐标系上方向 —— void SetWorldUpDirectionInternal(Vector3 worldUpDirection)
        /// <summary>
        /// 设置世界坐标系上方向
        /// </summary>
        /// <param name="worldUpDirection">世界坐标系上方向</param>
        private void SetWorldUpDirectionInternal(Vector3 worldUpDirection)
        {
            this.WorldUpDirection = worldUpDirection;

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
                    this.Yaw = MathHelper.RadiansToDegrees(MathF.Atan2(lookDirection.Z, lookDirection.Y));
                    float yzLength = MathF.Sqrt(lookDirection.Y * lookDirection.Y + lookDirection.Z * lookDirection.Z);
                    this.Pitch = MathHelper.RadiansToDegrees(MathF.Atan2(lookDirection.X, yzLength));
                    break;

                case CoordinateType.YUp:  //Y-up: 偏航角绕Y轴，俯仰角绕X轴
                    this.Yaw = MathHelper.RadiansToDegrees(MathF.Atan2(lookDirection.Z, lookDirection.X));
                    this.Pitch = MathHelper.RadiansToDegrees(MathF.Asin(Math.Clamp(lookDirection.Y, -1.0f, 1.0f)));
                    break;

                case CoordinateType.ZUp:  //Z-up: 偏航角绕Z轴，俯仰角绕X轴
                    this.Yaw = MathHelper.RadiansToDegrees(MathF.Atan2(lookDirection.Y, lookDirection.X));
                    float xyLength = MathF.Sqrt(lookDirection.X * lookDirection.X + lookDirection.Y * lookDirection.Y);
                    this.Pitch = MathHelper.RadiansToDegrees(MathF.Atan2(lookDirection.Z, xyLength));
                    break;
            }

            //标准化偏航角到[0, 360)
            if (this.Yaw < 0)
            {
                this.Yaw += 360.0f;
            }

            //限制俯仰角
            this.Pitch = Math.Clamp(this.Pitch, this.MinPitch, this.MaxPitch);
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
