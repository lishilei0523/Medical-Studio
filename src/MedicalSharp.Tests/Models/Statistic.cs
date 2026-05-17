using System.Runtime.InteropServices;

namespace MedicalSharp.Tests.Models
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Statistic
    {
        public float Min;
        public float Max;
        public float Sum;
    }
}
