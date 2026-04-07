using MedicalSharp.Primitives.Models;
using System.Collections.Generic;

namespace MedicalSharp.Primitives.Interfaces
{
    /// <summary>
    /// DICOM加载器接口
    /// </summary>
    public interface IDicomLoader
    {
        /// <summary>
        /// 加载DICOM序列
        /// </summary>
        /// <param name="dicomFolder">DICOM文件夹</param>
        /// <returns>体积数据</returns>
        VolumeData LoadSeries(string dicomFolder);

        /// <summary>
        /// 加载DICOM序列
        /// </summary>
        /// <param name="dicomPaths">DICOM文件路径集</param>
        /// <returns>体积数据</returns>
        VolumeData LoadSeries(ICollection<string> dicomPaths);
    }
}
