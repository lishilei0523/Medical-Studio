namespace MedicalSharp.Primitives.Enums
{
    /// <summary>
    /// 标记同步状态
    /// </summary>
    public enum MarkSyncStatus : byte
    {
        /// <summary>
        /// 空闲，可安全访问
        /// </summary>
        Idle = 0,

        /// <summary>
        /// 正在从GPU读取到CPU
        /// </summary>
        GpuToCpu = 1,

        /// <summary>
        /// 正在从CPU写入到GPU
        /// </summary>
        CpuToGpu = 2
    }
}
