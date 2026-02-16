using OpenTK.Mathematics;

namespace MedicalSharp.Engine.Resources
{
    /// <summary>
    /// 变换
    /// </summary>
    public class Transform
    {
        #region # 字段及构造器

        /// <summary>
        /// 缓存矩阵
        /// </summary>
        private Matrix4 _cachedMatrix;

        /// <summary>
        /// 是否需要重新计算矩阵
        /// </summary>
        private bool _dirty;

        /// <summary>
        /// 默认构造器
        /// </summary>
        public Transform()
        {
            this._position = Vector3.Zero;
            this._rotation = Quaternion.Identity;
            this._scaling = Vector3.One;
            this._cachedMatrix = Matrix4.Identity;
            this._dirty = true;
        }

        #endregion

        #region # 属性

        #region 位置 —— Vector3 Position
        /// <summary>
        /// 位置
        /// </summary>
        private Vector3 _position;

        /// <summary>
        /// 位置
        /// </summary>
        public Vector3 Position
        {
            get => this._position;
            set
            {
                this._position = value;
                this._dirty = true;
            }
        }
        #endregion

        #region 旋转 —— Quaternion Rotation
        /// <summary>
        /// 旋转
        /// </summary>
        private Quaternion _rotation;

        /// <summary>
        /// 旋转
        /// </summary>
        public Quaternion Rotation
        {
            get => this._rotation;
            set
            {
                this._rotation = value;
                this._dirty = true;
            }
        }
        #endregion

        #region 缩放 —— Vector3 Scaling
        /// <summary>
        /// 缩放
        /// </summary>
        private Vector3 _scaling;

        /// <summary>
        /// 缩放
        /// </summary>
        public Vector3 Scaling
        {
            get => this._scaling;
            set
            {
                this._scaling = value;
                this._dirty = true;
            }
        }
        #endregion

        #region 只读属性 - 变换矩阵 —— Matrix4 Matrix
        /// <summary>
        /// 只读属性 - 变换矩阵
        /// </summary>
        public Matrix4 Matrix
        {
            get
            {
                if (this._dirty)
                {
                    this.UpdateMatrix();
                }

                return this._cachedMatrix;
            }
        }
        #endregion 

        #endregion

        #region # 方法

        //Public

        #region 设置旋转 —— void SetRotation(Vector3 eulerAngles)
        /// <summary>
        /// 设置旋转
        /// </summary>
        /// <param name="eulerAngles">欧拉角</param>
        public void SetRotation(Vector3 eulerAngles)
        {
            this._rotation = Quaternion.FromEulerAngles(eulerAngles);
            this._dirty = true;
        }
        #endregion

        #region 平移 —— void Translate(Vector3 translation)
        /// <summary>
        /// 平移
        /// </summary>
        /// <param name="translation">平移向量</param>
        public void Translate(Vector3 translation)
        {
            this._position += translation;
            this._dirty = true;
        }
        #endregion

        #region 旋转 —— void Rotate(float angle, Vector3 axis)
        /// <summary>
        /// 旋转
        /// </summary>
        /// <param name="angle">角度（度）</param>
        /// <param name="axis">旋转轴</param>
        public void Rotate(float angle, Vector3 axis)
        {
            Quaternion rotation = Quaternion.FromAxisAngle(axis.Normalized(), MathHelper.DegreesToRadians(angle));
            this._rotation = rotation * this._rotation;
            this._dirty = true;
        }
        #endregion

        #region 旋转 —— void Rotate(Quaternion rotation)
        /// <summary>
        /// 旋转
        /// </summary>
        /// <param name="rotation">旋转四元数</param>
        public void Rotate(Quaternion rotation)
        {
            this._rotation = rotation * this._rotation;
            this._dirty = true;
        }
        #endregion

        #region 缩放 —— void Scale(Vector3 scaling)
        /// <summary>
        /// 缩放
        /// </summary>
        /// <param name="scaling">缩放向量</param>
        public void Scale(Vector3 scaling)
        {
            this._scaling *= scaling;
            this._dirty = true;
        }
        #endregion

        #region 变换方向向量 —— ApplyToDirection(Vector3 direction)
        /// <summary>
        /// 变换方向向量
        /// </summary>
        /// <param name="direction">方向向量</param>
        /// <returns>方向向量</returns>
        public Vector3 ApplyToDirection(Vector3 direction)
        {
            return this._rotation * direction;
        }
        #endregion

        #region 获取世界变换矩阵 —— Matrix4 GetWorldMatrix(Transform parent)
        /// <summary>
        /// 获取世界变换矩阵
        /// </summary>
        /// <param name="parent">上级变换</param>
        /// <returns>变换矩阵</returns>
        public Matrix4 GetWorldMatrix(Transform parent)
        {
            if (parent != null)
            {
                return parent.GetWorldMatrix(null) * this.Matrix;
            }

            return this.Matrix;
        }
        #endregion


        //Private

        #region 更新变换矩阵 —— void UpdateMatrix()
        /// <summary>
        /// 更新变换矩阵
        /// </summary>
        private void UpdateMatrix()
        {
            Matrix4 translation = Matrix4.CreateTranslation(this._position);
            Matrix4 rotation = Matrix4.CreateFromQuaternion(this._rotation);
            Matrix4 scale = Matrix4.CreateScale(this._scaling);

            this._cachedMatrix = translation * rotation * scale;
            this._dirty = false;
        }
        #endregion 

        #endregion
    }
}
