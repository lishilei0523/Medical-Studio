
using System;
using System.IO;

// ReSharper disable once CheckNamespace
namespace MedicalSharp.Primitives
{
    /// <summary>
    /// 常量
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// 最小HU值
        /// </summary>
        public const short MinHU = -1024;

        /// <summary>
        /// 最大HU值
        /// </summary>
        public const short MaxHU = 3071;

        /// <summary>
        /// 协议路径
        /// </summary>
        public static readonly string ProtocolPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources/Protocols");
    }
}
