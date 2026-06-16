using MedicalSharp.Primitives.Models;
using System.Collections.Generic;

namespace MedicalSharp.Primitives.Interfaces
{
    /// <summary>
    /// DICOM加载器接口
    /// </summary>
    public interface IDicomLoader
    {
        #region # 加载SimpleITK图像 —— VolumeData LoadSitkImage(object image)
        /// <summary>
        /// 加载SimpleITK图像
        /// </summary>
        /// <param name="image">SimpleITK图像</param>
        /// <returns>体积数据</returns>
        VolumeData LoadSitkImage(object image);
        #endregion

        #region # 加载DICOM序列 —— VolumeData LoadSeries(string dicomFolder)
        /// <summary>
        /// 加载DICOM序列
        /// </summary>
        /// <param name="dicomFolder">DICOM文件夹</param>
        /// <returns>体积数据</returns>
        VolumeData LoadSeries(string dicomFolder);
        #endregion

        #region # 加载DICOM序列 —— VolumeData LoadSeries(IReadOnlyList<string> dicomPaths)
        /// <summary>
        /// 加载DICOM序列
        /// </summary>
        /// <param name="dicomPaths">DICOM文件路径集</param>
        /// <returns>体积数据</returns>
        VolumeData LoadSeries(IReadOnlyList<string> dicomPaths);
        #endregion

        #region # 加载NIFTI图像文件 —— VolumeData LoadNiiImage(string filePath)
        /// <summary>
        /// 加载NIFTI图像文件
        /// </summary>
        /// <param name="filePath">Nii文件路径</param>
        /// <returns>体积数据</returns>
        VolumeData LoadNiiImage(string filePath);
        #endregion

        #region # 加载MHD+RAW图像文件 —— VolumeData LoadRawImage(string filePath)
        /// <summary>
        /// 加载MHD+RAW图像文件
        /// </summary>
        /// <param name="filePath">MHD文件路径</param>
        /// <returns>体积数据</returns>
        VolumeData LoadRawImage(string filePath);
        #endregion

        #region # 加载NIFTI预览文件 —— void LoadNiiPreview(VolumeData volumeData, string filePath)
        /// <summary>
        /// 加载NIFTI预览文件
        /// </summary>
        /// <param name="volumeData">体积数据</param>
        /// <param name="filePath">Nii文件路径</param>
        void LoadNiiPreview(VolumeData volumeData, string filePath);
        #endregion

        #region # 加载MHD+RAW预览文件 —— void LoadRawPreview(VolumeData volumeData, string filePath)
        /// <summary>
        /// 加载MHD+RAW预览文件
        /// </summary>
        /// <param name="volumeData">体积数据</param>
        /// <param name="filePath">MHD文件路径</param>
        void LoadRawPreview(VolumeData volumeData, string filePath);
        #endregion

        #region # 加载NIFTI标记文件 —— void LoadNiiMark(VolumeData volumeData, string filePath)
        /// <summary>
        /// 加载NIFTI标记文件
        /// </summary>
        /// <param name="volumeData">体积数据</param>
        /// <param name="filePath">Nii文件路径</param>
        void LoadNiiMark(VolumeData volumeData, string filePath);
        #endregion

        #region # 加载MHD+RAW标记文件 —— void LoadRawMark(VolumeData volumeData, string filePath)
        /// <summary>
        /// 加载MHD+RAW标记文件
        /// </summary>
        /// <param name="volumeData">体积数据</param>
        /// <param name="filePath">MHD文件路径</param>
        void LoadRawMark(VolumeData volumeData, string filePath);
        #endregion

        #region # 保存原始NIFTI图像文件 —— void SaveOriginalNiiImage(VolumeData volumeData, string filePath)
        /// <summary>
        /// 保存原始NIFTI图像文件
        /// </summary>
        /// <param name="volumeData">体积数据</param>
        /// <param name="filePath">Nii文件路径</param>
        void SaveOriginalNiiImage(VolumeData volumeData, string filePath);
        #endregion

        #region # 保存原始MHD+RAW图像文件 —— void SaveOriginalRawImage(VolumeData volumeData, string filePath)
        /// <summary>
        /// 保存原始MHD+RAW图像文件
        /// </summary>
        /// <param name="volumeData">体积数据</param>
        /// <param name="filePath">MHD文件路径</param>
        void SaveOriginalRawImage(VolumeData volumeData, string filePath);
        #endregion

        #region # 保存预览NIFTI图像文件 —— void SavePreviewNiiImage(VolumeData volumeData, string filePath)
        /// <summary>
        /// 保存预览NIFTI图像文件
        /// </summary>
        /// <param name="volumeData">体积数据</param>
        /// <param name="filePath">Nii文件路径</param>
        void SavePreviewNiiImage(VolumeData volumeData, string filePath);
        #endregion

        #region # 保存预览MHD+RAW图像文件 —— void SavePreviewRawImage(VolumeData volumeData, string filePath)
        /// <summary>
        /// 保存预览MHD+RAW图像文件
        /// </summary>
        /// <param name="volumeData">体积数据</param>
        /// <param name="filePath">MHD文件路径</param>
        void SavePreviewRawImage(VolumeData volumeData, string filePath);
        #endregion

        #region # 保存标记NIFTI图像文件 —— void SaveMarkNiiImage(VolumeData volumeData, string filePath)
        /// <summary>
        /// 保存标记NIFTI图像文件
        /// </summary>
        /// <param name="volumeData">体积数据</param>
        /// <param name="filePath">Nii文件路径</param>
        void SaveMarkNiiImage(VolumeData volumeData, string filePath);
        #endregion

        #region # 保存标记MHD+RAW图像文件 —— void SaveMarkRawImage(VolumeData volumeData, string filePath)
        /// <summary>
        /// 保存标记MHD+RAW图像文件
        /// </summary>
        /// <param name="volumeData">体积数据</param>
        /// <param name="filePath">MHD文件路径</param>
        void SaveMarkRawImage(VolumeData volumeData, string filePath);
        #endregion
    }
}
