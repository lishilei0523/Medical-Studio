using OpenTK.Mathematics;
using System.Runtime.InteropServices;

namespace MedicalSharp.Primitives.Maths
{
    /// <summary>
    /// Frenet框架
    /// </summary>
    /// <remarks>描述曲线上一点的局部坐标系</remarks>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct FrenetFrame
    {
        /// <summary>
        /// 创建Frenet框架构造器
        /// </summary>
        /// <param name="position">曲线位置</param>
        /// <param name="tangent">切线方向</param>
        /// <param name="normal">法向量</param>
        /// <param name="binormal">副法向量</param>
        public FrenetFrame(Vector3 position, Vector3 tangent, Vector3 normal, Vector3 binormal)
            : this()
        {
            this.Position = position;
            this.Tangent = tangent;
            this.Normal = normal;
            this.Binormal = binormal;
        }

        /// <summary>
        /// 曲线位置
        /// </summary>
        public readonly Vector3 Position;

        /// <summary>
        /// 切线方向
        /// </summary>
        /// <remarks>单位向量，沿曲线前进方向</remarks>
        public readonly Vector3 Tangent;

        /// <summary>
        /// 法向量
        /// </summary>
        /// <remarks>单位向量，垂直于切线</remarks>
        public readonly Vector3 Normal;

        /// <summary>
        /// 副法向量
        /// </summary>
        /// <remarks>单位向量，Tangent × Normal</remarks>
        public readonly Vector3 Binormal;

        /// <summary>
        /// 转换字符串
        /// </summary>
        public override string ToString()
        {
            return $"Pos: {this.Position}, T: {this.Tangent}, N: {this.Normal}, B: {this.Binormal}";
        }
    }
}
