using MedicalSharp.Presentation.Models;
using MedicalSharp.Primitives.Maths;
using MedicalSharp.Primitives.Models;
using OpenTK.Mathematics;

namespace MedicalSharp.Presentation.Maps
{
    /// <summary>
    /// 协议相关映射
    /// </summary>
    public static class ProtocolMap
    {
        #region # 密度控制点映射Raycast协议控制点 —— static RaycastProtocolPoint ToRaycastProtocolPoint(...
        /// <summary>
        /// 密度控制点映射Raycast协议控制点
        /// </summary>
        public static RaycastProtocolPoint ToRaycastProtocolPoint(this DensityControlPoint controlPoint)
        {
            RaycastProtocolPoint protocolPoint = new RaycastProtocolPoint
            {
                Position = controlPoint.Position,
                Color = controlPoint.Color.ToColor4f()
            };

            return protocolPoint;
        }
        #endregion

        #region # Raycast协议控制点映射密度控制点 —— static DensityControlPoint ToDensityControlPoint(...
        /// <summary>
        /// Raycast协议控制点映射密度控制点
        /// </summary>
        public static DensityControlPoint ToDensityControlPoint(this RaycastProtocolPoint protocolPoint)
        {
            DensityControlPoint controlPoint = new DensityControlPoint(protocolPoint.Position, protocolPoint.Color.ToVector4());

            return controlPoint;
        }
        #endregion

        #region # Color4f映射Vector4 —— static Vector4 ToVector4(this Color4f color)
        /// <summary>
        /// Color4f映射Vector4
        /// </summary>
        public static Vector4 ToVector4(this Color4f color)
        {
            return new Vector4(color.R, color.G, color.B, color.A);
        }
        #endregion

        #region # Vector4映射Color4f —— static Color4f ToColor4f(this in Vector4 vector4)
        /// <summary>
        /// Vector4映射Color4f
        /// </summary>
        public static Color4f ToColor4f(this in Vector4 vector4)
        {
            return new Color4f(vector4.X, vector4.Y, vector4.Z, vector4.W);
        }
        #endregion
    }
}
