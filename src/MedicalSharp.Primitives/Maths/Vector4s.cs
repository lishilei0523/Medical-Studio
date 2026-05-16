using System.Runtime.InteropServices;

namespace MedicalSharp.Primitives.Maths
{
    /// <summary>
    /// Represents a 4D vector using four single-precision short numbers.
    /// </summary>
    /// <remarks>
    /// The Vector4s structure is suitable for interoperation with unmanaged code requiring four consecutive shorts.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Vector4s
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Vector4b"/> struct.
        /// </summary>
        /// <param name="value">The x,y,z,w value of the Vector4s.</param>
        public Vector4s(short value)
        {
            this.X = value;
            this.Y = value;
            this.Z = value;
            this.W = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Vector4b"/> struct.
        /// </summary>
        /// <param name="x">The x component of the Vector4s.</param>
        /// <param name="y">The y component of the Vector4s.</param>
        /// <param name="z">The z component of the Vector4s.</param>
        /// <param name="w">The w component of the Vector4s.</param>
        public Vector4s(short x, short y, short z, short w)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.W = w;
        }

        /// <summary>
        /// The X component of the Vector4.
        /// </summary>
        public short X;

        /// <summary>
        /// The Y component of the Vector4.
        /// </summary>
        public short Y;

        /// <summary>
        /// The Z component of the Vector4.
        /// </summary>
        public short Z;

        /// <summary>
        /// The W component of the Vector4.
        /// </summary>
        public short W;
    }
}
