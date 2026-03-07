using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace MedicalSharp.Engine.Resources
{
    /// <summary>
    /// 资源管理器
    /// </summary>
    public static class ResourceManager
    {
        /// <summary>
        /// 3D纹理字典
        /// </summary>
        private static readonly IDictionary<Guid, Texture3D> _Texture3Ds;

        /// <summary>
        /// 静态构造器
        /// </summary>
        static ResourceManager()
        {
            _Texture3Ds = new ConcurrentDictionary<Guid, Texture3D>();
        }

        /// <summary>
        /// 只读属性 - 3D纹理字典
        /// </summary>
        public static IReadOnlyDictionary<Guid, Texture3D> Texture3Ds
        {
            get => _Texture3Ds.AsReadOnly();
        }


        public static void AddTexture3D(Guid id, Texture3D texture3D)
        {
            _Texture3Ds.Add(id, texture3D);
        }

        public static void RemoveTexture3D(Guid id)
        {
            if (_Texture3Ds.Remove(id, out Texture3D texture3D))
            {
                texture3D.Dispose();
            }
        }
    }
}
