using MedicalSharp.Primitives.Enums;
using OpenTK.Mathematics;
using System;
using System.Text;

namespace MedicalSharp.Primitives.Models
{
    /// <summary>
    /// 四方向
    /// </summary>
    public sealed class FourDirection
    {
        #region # 字段及构造器

        /// <summary>
        /// 前方（Anterior）
        /// </summary>
        public const string AnteriorDirection = "A";

        /// <summary>
        /// 后方（Posterior）
        /// </summary>
        public const string PosteriorDirection = "P";

        /// <summary>
        /// 右侧（Right）
        /// </summary>
        public const string RightDirection = "R";

        /// <summary>
        /// 左侧（Left）
        /// </summary>
        public const string LeftDirection = "L";

        /// <summary>
        /// 上方（Superior）
        /// </summary>
        public const string SuperiorDirection = "S";

        /// <summary>
        /// 下方（Inferior）
        /// </summary>
        public const string InferiorDirection = "I";

        /// <summary>
        /// 默认构造器
        /// </summary>
        public FourDirection()
        {

        }

        #endregion

        #region # 属性

        #region 上 —— string Top
        /// <summary>
        /// 上
        /// </summary>
        public string Top { get; private set; }
        #endregion

        #region 下 —— string Bottom
        /// <summary>
        /// 下
        /// </summary>
        public string Bottom { get; private set; }
        #endregion

        #region 左 —— string Left
        /// <summary>
        /// 左
        /// </summary>
        public string Left { get; private set; }
        #endregion

        #region 右 —— string Right
        /// <summary>
        /// 右
        /// </summary>
        public string Right { get; private set; }
        #endregion

        #endregion

        #region # 方法

        #region 计算方向 —— void CalculateDirections(Vector3 uAxis...
        /// <summary>
        /// 计算方向
        /// </summary>
        /// <param name="uAxis">U轴</param>
        /// <param name="vAxis">V轴</param>
        /// <param name="originalPlaneType">原始平面类型</param>
        public void CalculateDirections(Vector3 uAxis, Vector3 vAxis, MPRPlaneType originalPlaneType)
        {
            if (originalPlaneType == MPRPlaneType.Axial)
            {
                vAxis = new Vector3(vAxis.X, -vAxis.Y, vAxis.Z);
            }
            if (originalPlaneType == MPRPlaneType.Coronal)
            {
                uAxis = new Vector3(uAxis.X, -uAxis.Y, uAxis.Z);
                vAxis = new Vector3(vAxis.X, -vAxis.Y, vAxis.Z);
            }
            if (originalPlaneType == MPRPlaneType.Sagittal)
            {
                uAxis = new Vector3(uAxis.X, -uAxis.Y, uAxis.Z);
                vAxis = new Vector3(vAxis.X, -vAxis.Y, vAxis.Z);
            }

            this.Top = GetDirectionIndicator(vAxis);
            this.Bottom = GetDirectionIndicator(-vAxis);
            this.Left = GetDirectionIndicator(-uAxis);
            this.Right = GetDirectionIndicator(uAxis);
        }
        #endregion

        #region 获取方向指示 —— static string GetDirectionIndicator(in Vector3 direction)
        /// <summary>
        /// 获取方向指示
        /// </summary>
        /// <param name="direction">方向</param>
        /// <returns>方向指示</returns>
        private static string GetDirectionIndicator(in Vector3 direction)
        {
            float absX = Math.Abs(direction.X);
            float absY = Math.Abs(direction.Y);
            float absZ = Math.Abs(direction.Z);
            StringBuilder builder = new StringBuilder();
            if (absX >= 0.1f)
            {
                builder.Append(direction.X > 0 ? LeftDirection : RightDirection);
            }
            if (absY >= 0.1f)
            {
                builder.Append(direction.Y > 0 ? AnteriorDirection : PosteriorDirection);
            }
            if (absZ >= 0.1f)
            {
                builder.Append(direction.Z > 0 ? SuperiorDirection : InferiorDirection);
            }

            return builder.ToString();
        }
        #endregion

        #endregion
    }
}
