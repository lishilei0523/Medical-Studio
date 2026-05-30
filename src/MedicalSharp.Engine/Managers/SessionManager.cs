using MedicalSharp.Engine.Base;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace MedicalSharp.Engine.Managers
{
    /// <summary>
    /// 体积会话管理器
    /// </summary>
    public static class SessionManager
    {
        #region # 字段及构造器

        /// <summary>
        /// 体积会话字典
        /// </summary>
        private static readonly ConcurrentDictionary<string, VolumeSession> _VolumeSessions;

        /// <summary>
        /// 静态构造器
        /// </summary>
        static SessionManager()
        {
            _VolumeSessions = new ConcurrentDictionary<string, VolumeSession>();
        }

        #endregion

        #region # 属性

        #region 只读属性 - 体积会话字典 —— static IReadOnlyDictionary<string, VolumeSession> VolumeSessions
        /// <summary>
        /// 只读属性 - 体积会话字典
        /// </summary>
        public static IReadOnlyDictionary<string, VolumeSession> VolumeSessions
        {
            get => _VolumeSessions;
        }
        #endregion

        #endregion

        #region # 方法

        #region 添加体积会话 —— static void AddVolumeSession(string id, VolumeSession volumeSession)
        /// <summary>
        /// 添加体积会话
        /// </summary>
        /// <param name="id">标识Id</param>
        /// <param name="volumeSession">体积会话</param>
        public static void AddVolumeSession(string id, VolumeSession volumeSession)
        {
            IDictionary<string, VolumeSession> volumeSessions = _VolumeSessions;
            volumeSessions.Add(id, volumeSession);
        }
        #endregion

        #region 删除体积会话 —— static void RemoveVolumeSession(string id)
        /// <summary>
        /// 删除体积会话
        /// </summary>
        /// <param name="id">标识Id</param>
        public static void RemoveVolumeSession(string id)
        {
            if (_VolumeSessions.Remove(id, out VolumeSession volumeSession))
            {
                volumeSession.Dispose();
            }
        }
        #endregion

        #endregion
    }
}
