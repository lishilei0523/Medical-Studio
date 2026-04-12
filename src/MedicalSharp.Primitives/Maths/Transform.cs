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
            this.Pivot = Vector3.Zero;
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

        #region 轴心点 —— Vector3 Pivot
        /// <summary>
        /// 轴心点
        /// </summary>
        /// <remarks>旋转和缩放的中心</remarks>
        public Vector3 Pivot { get; private set; }
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

        #region 设置轴心点 —— void SetPivot(Vector3 pivot)
        /// <summary>
        /// 设置轴心点
        /// </summary>
        /// <param name="pivot">轴心点</param>
        public void SetPivot(Vector3 pivot)
        {
            this.Pivot = pivot;
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
            //平移到轴心点
            Matrix4 toPivot = Matrix4.CreateTranslation(-this.Pivot);

            //应用旋转和缩放
            Matrix4 rotation = Matrix4.CreateFromQuaternion(this.Rotation);
            Matrix4 scale = Matrix4.CreateScale(this.Scaling);

            //平移回原位置
            Matrix4 fromPivot = Matrix4.CreateTranslation(this.Pivot);

            //应用世界位置平移
            Matrix4 translation = Matrix4.CreateTranslation(this.Position);

            // 组合：先移到轴心点，应用变换，移回，最后平移到世界位置
            this._matrix = scale * toPivot * rotation * fromPivot * translation;
        }
        #endregion 

        #endregion
    }
}
