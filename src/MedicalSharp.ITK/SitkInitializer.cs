using itk.simple;

namespace MedicalSharp.ITK
{
    /// <summary>
    /// SimpleITK初始化器
    /// </summary>
    public static class SitkInitializer
    {
        /// <summary>
        /// 初始化
        /// </summary>
        public static void Initialize()
        {
            ImageSeriesReader reader = new ImageSeriesReader();
            reader.Dispose();
        }
    }
}
