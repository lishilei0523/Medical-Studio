using System.Runtime.InteropServices;

namespace MedicalSharp.Tests.Models
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Voxel
    {
        public float HU;          //4字节
        public short Label;       //2字节
        public byte Visited;      //1字节
        public byte Padding;      //1字节（手动对齐到4字节倍数）
    }
}
