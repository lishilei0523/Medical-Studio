using MedicalSharp.Engine.Resources;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace MedicalSharp.Engine.Managers
{
    /// <summary>
    /// 纹理管理器
    /// </summary>
    public static class TextureManager
    {
        #region # 字段及构造器

        /// <summary>
        /// 3D纹理字典
        /// </summary>
        private static readonly IDictionary<string, Texture3D> _Texture3Ds;

        /// <summary>
        /// 静态构造器
        /// </summary>
        static TextureManager()
        {
            _Texture3Ds = new ConcurrentDictionary<string, Texture3D>();
        }

        #endregion

        #region # 属性

        #region 只读属性 - 3D纹理字典 —— static IReadOnlyDictionary<string, Texture3D> Texture3Ds
        /// <summary>
        /// 只读属性 - 3D纹理字典
        /// </summary>
        public static IReadOnlyDictionary<string, Texture3D> Texture3Ds
        {
            get => _Texture3Ds.AsReadOnly();
        }
        #endregion 

        #endregion

        #region # 方法

        #region 添加3D纹理 —— static void AddTexture3D(string id, Texture3D texture3D)
        /// <summary>
        /// 添加3D纹理
        /// </summary>
        /// <param name="id">标识Id</param>
        /// <param name="texture3D">3D纹理</param>
        public static void AddTexture3D(string id, Texture3D texture3D)
        {
            _Texture3Ds.Add(id, texture3D);
        }
        #endregion

        #region 删除3D纹理 —— static void RemoveTexture3D(string id)
        /// <summary>
        /// 删除3D纹理
        /// </summary>
        /// <param name="id">标识Id</param>
        public static void RemoveTexture3D(string id)
        {
            if (_Texture3Ds.Remove(id, out Texture3D texture3D))
            {
                texture3D.Dispose();
            }
        }
        #endregion

        #endregion
    }
}
