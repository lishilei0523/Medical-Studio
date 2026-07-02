using System.Runtime.InteropServices;

namespace MedicalSharp.Primitives.Maths
{
    /// <summary>
    /// Represents a 4D vector using four byte numbers.
    /// </summary>
    /// <remarks>
    /// The Vector4b structure is suitable for interoperation with unmanaged code requiring four consecutive bytes.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Vector4b
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Vector4b"/> struct.
        /// </summary>
        /// <param name="value">The x,y,z,w value of the Vector4b.</param>
        public Vector4b(byte value)
        {
            this.X = value;
            this.Y = value;
            this.Z = value;
            this.W = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Vector4b"/> struct.
        /// </summary>
        /// <param name="x">The x component of the Vector4b.</param>
        /// <param name="y">The y component of the Vector4b.</param>
        /// <param name="z">The z component of the Vector4b.</param>
        /// <param name="w">The w component of the Vector4b.</param>
        public Vector4b(byte x, byte y, byte z, byte w)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.W = w;
        }

        /// <summary>
        /// The X component of the Vector4b.
        /// </summary>
        public byte X;

        /// <summary>
        /// The Y component of the Vector4b.
        /// </summary>
        public byte Y;

        /// <summary>
        /// The Z component of the Vector4b.
        /// </summary>
        public byte Z;

        /// <summary>
        /// The W component of the Vector4b.
        /// </summary>
        public byte W;
    }
}
