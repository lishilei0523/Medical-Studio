using MedicalSharp.Engine.Resources;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace MedicalSharp.Engine.Managers
{
    /// <summary>
    /// 顶点数组对象管理器
    /// </summary>
    public static class VertexArrayManager
    {
        #region # 字段及构造器

        /// <summary>
        /// 顶点数组对象字典
        /// </summary>
        private static readonly IDictionary<IntPtr, HashSet<VertexArray>> _VertexArray;

        /// <summary>
        /// 静态构造器
        /// </summary>
        static VertexArrayManager()
        {
            _VertexArray = new ConcurrentDictionary<IntPtr, HashSet<VertexArray>>();
        }

        #endregion

        #region # 属性

        #region 只读属性 - 顶点数组对象字典 —— static IReadOnlyDictionary<IntPtr, HashSet<VertexArray>> VertexArrays
        /// <summary>
        /// 只读属性 - 顶点数组对象字典
        /// </summary>
        public static IReadOnlyDictionary<IntPtr, HashSet<VertexArray>> VertexArrays
        {
            get => _VertexArray.AsReadOnly();
        }
        #endregion

        #endregion

        #region # 方法

        #region 注册顶点数组对象 —— static void RegisterVertexArray(IntPtr glContext, VertexArray vertexArray)
        /// <summary>
        /// 注册顶点数组对象
        /// </summary>
        /// <param name="glContext">OpenGL上下文句柄</param>
        /// <param name="vertexArray">顶点数组对象</param>
        public static void RegisterVertexArray(IntPtr glContext, VertexArray vertexArray)
        {
            if (!_VertexArray.TryGetValue(glContext, out HashSet<VertexArray> vaos))
            {
                vaos = [];
                _VertexArray.Add(glContext, vaos);
            }

            vaos.Add(vertexArray);
        }
        #endregion

        #region 释放顶点数据对象 —— static void ReleaseVertexArrays(IntPtr glContext)
        /// <summary>
        /// 释放顶点数据对象
        /// </summary>
        /// <param name="glContext">OpenGL上下文句柄</param>
        public static void ReleaseVertexArrays(IntPtr glContext)
        {
            if (!_VertexArray.TryGetValue(glContext, out HashSet<VertexArray> vertexArrays))
            {
                return;
            }

            VertexArray[] toDisposes = vertexArrays.Where(x => x.ShouldDipose).ToArray();
            foreach (VertexArray vertexArray in toDisposes)
            {
                vertexArray.Dispose();
                vertexArrays.Remove(vertexArray);
            }
        }
        #endregion

        #endregion
    }
}
