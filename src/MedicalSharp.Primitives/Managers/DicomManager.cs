using MedicalSharp.Primitives.Models;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace MedicalSharp.Primitives.Managers
{
    /// <summary>
    /// DICOM管理器
    /// </summary>
    public static class DicomManager
    {
        #region # 字段及构造器

        /// <summary>
        /// 体积数据字典
        /// </summary>
        private static readonly IDictionary<string, VolumeData> _VolumeDatas;

        /// <summary>
        /// 静态构造器
        /// </summary>
        static DicomManager()
        {
            _VolumeDatas = new ConcurrentDictionary<string, VolumeData>();
        }

        #endregion

        #region # 属性

        #region 只读属性 - 体积数据字典 —— static IReadOnlyDictionary<string, VolumeData> VolumeDatas
        /// <summary>
        /// 只读属性 - 体积数据字典
        /// </summary>
        public static IReadOnlyDictionary<string, VolumeData> VolumeDatas
        {
            get => _VolumeDatas.AsReadOnly();
        }
        #endregion

        #endregion

        #region # 方法

        #region 添加体积数据 —— static void AddVolumeData(VolumeData volumeData)
        /// <summary>
        /// 添加体积数据
        /// </summary>
        /// <param name="volumeData">体积数据</param>
        public static void AddVolumeData(VolumeData volumeData)
        {
            _VolumeDatas.Add(volumeData.Id, volumeData);
        }
        #endregion

        #region 删除体积数据 —— static void RemoveVolumeData(string id)
        /// <summary>
        /// 删除体积数据
        /// </summary>
        /// <param name="id">体积数据Id</param>
        public static void RemoveVolumeData(string id)
        {
            if (_VolumeDatas.Remove(id, out VolumeData volumeData))
            {
                volumeData.Dispose();
            }
        }
        #endregion

        #endregion
    }
}
