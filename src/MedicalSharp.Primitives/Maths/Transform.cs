using OpenTK.Mathematics;

namespace MedicalSharp.Primitives.Maths
{
    /// <summary>
    /// 变换
    /// </summary>
    public class Transform
    {
        #region # 字段及构造器

        /// <summary>
        /// 变换矩阵
        /// </summary>
        private Matrix4 _matrix;

        /// <summary>
        /// 默认构造器
        /// </summary>
        public Transform()
        {
            this.Position = Vector3.Zero;
            this.Rotation = Quaternion.Identity;
            this.Scaling = Vector3.One;
            this._matrix = Matrix4.Identity;
            this.UpdateMatrix();
        }

        #endregion

        #region # 属性

        #region 位置 —— Vector3 Position
        /// <summary>
        /// 位置
        /// </summary>
        public Vector3 Position { get; private set; }
        #endregion

        #region 旋转 —— Quaternion Rotation
        /// <summary>
        /// 旋转
        /// </summary>
        public Quaternion Rotation { get; private set; }
        #endregion

        #region 缩放 —— Vector3 Scaling
        /// <summary>
        /// 缩放
        /// </summary>
        public Vector3 Scaling { get; private set; }
        #endregion

        #region 只读属性 - 变换矩阵 —— Matrix4 Matrix
        /// <summary>
        /// 只读属性 - 变换矩阵
        /// </summary>
        public Matrix4 Matrix
        {
            get => this._matrix;
        }
        #endregion

        #endregion

        #region # 方法

        //Public

        #region 设置矩阵 —— void SetMatrix(Matrix4 matrix)
        /// <summary>
        /// 设置矩阵
        /// </summary>
        /// <param name="matrix">变换矩阵</param>
        public void SetMatrix(Matrix4 matrix)
        {
            this._matrix = matrix;
            this.Position = matrix.ExtractTranslation();
            this.Rotation = matrix.ExtractRotation();
            this.Scaling = matrix.ExtractScale();
        }
        #endregion

        #region 设置位置 —— void SetPosition(Vector3 position)
        /// <summary>
        /// 设置位置
        /// </summary>
        /// <param name="position">位置</param>
        public void SetPosition(Vector3 position)
        {
            this.Position = position;
            this.UpdateMatrix();
        }
        #endregion

        #region 设置旋转 —— void SetRotation(Vector3 eulerAngles)
        /// <summary>
        /// 设置旋转
        /// </summary>
        /// <param name="eulerAngles">欧拉角</param>
        public void SetRotation(Vector3 eulerAngles)
        {
            this.Rotation = Quaternion.FromEulerAngles(eulerAngles);
            this.UpdateMatrix();
        }
        #endregion

        #region 设置位置和旋转 —— void SetPositionAndRotation(Vector3 position, Quaternion rotation)
        /// <summary>
        /// 设置位置和旋转
        /// </summary>
        /// <param name="position">位置</param>
        /// <param name="rotation">旋转四元数</param>
        public void SetPositionAndRotation(Vector3 position, Quaternion rotation)
        {
            this.Position = position;
            this.Rotation = rotation;
            this.UpdateMatrix();
        }
        #endregion

        #region 平移 —— void Translate(Vector3 translation)
        /// <summary>
        /// 平移
        /// </summary>
        /// <param name="translation">平移向量</param>
        public void Translate(Vector3 translation)
        {
            this.Position += translation;
            this.UpdateMatrix();
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
            this.Rotation = rotation * this.Rotation;
            this.UpdateMatrix();
        }
        #endregion

        #region 旋转 —— void Rotate(Quaternion rotation)
        /// <summary>
        /// 旋转
        /// </summary>
        /// <param name="rotation">旋转四元数</param>
        public void Rotate(Quaternion rotation)
        {
            this.Rotation = rotation * this.Rotation;
            this.UpdateMatrix();
        }
        #endregion

        #region 缩放 —— void Scale(Vector3 scaling)
        /// <summary>
        /// 缩放
        /// </summary>
        /// <param name="scaling">缩放向量</param>
        public void Scale(Vector3 scaling)
        {
            this.Scaling *= scaling;
            this.UpdateMatrix();
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
            return this.Rotation * direction;
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
            Matrix4 translation = Matrix4.CreateTranslation(this.Position);
            Matrix4 rotation = Matrix4.CreateFromQuaternion(this.Rotation);
            Matrix4 scale = Matrix4.CreateScale(this.Scaling);

            this._matrix = translation * rotation * scale;
        }
        #endregion 

        #endregion
    }
}
