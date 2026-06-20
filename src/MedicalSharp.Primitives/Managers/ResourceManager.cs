using MedicalSharp.Primitives.Maths;
using MedicalSharp.Primitives.Models;
using OpenTK.Mathematics;
using System.IO;

namespace MedicalSharp.Primitives.Managers
{
    /// <summary>
    /// 资源管理器
    /// </summary>
    public static class ResourceManager
    {
        #region # 字段及构造器

        /// <summary>
        /// 字体路径
        /// </summary>
        private const string FontPath = "Resources/Fonts/msyh.ttf";

        /// <summary>
        /// 字体字节数组
        /// </summary>
        private static readonly byte[] _FontBytes;

        /// <summary>
        /// 单位平面
        /// </summary>
        private static readonly MeshGeometry _UnitPlane;

        /// <summary>
        /// 单位立方体
        /// </summary>
        private static readonly MeshGeometry _UnitCube;

        /// <summary>
        /// 静态构造器
        /// </summary>
        static ResourceManager()
        {
            _FontBytes = File.ReadAllBytes(FontPath);
            _UnitCube = GetUnitCube();
            _UnitPlane = GetUnitPlane();
        }

        #endregion

        #region # 属性

        #region 只读属性 - 字体字节数组 —— static byte[] FontBytes
        /// <summary>
        /// 只读属性 - 字体字节数组
        /// </summary>
        public static byte[] FontBytes
        {
            get => _FontBytes;
        }
        #endregion

        #region 只读属性 - 单位立方体 —— static MeshGeometry UnitCube
        /// <summary>
        /// 只读属性 - 单位立方体
        /// </summary>
        public static MeshGeometry UnitCube
        {
            get => _UnitCube;
        }
        #endregion

        #region 只读属性 - 单位平面 —— static MeshGeometry UnitPlane
        /// <summary>
        /// 只读属性 - 单位平面
        /// </summary>
        public static MeshGeometry UnitPlane
        {
            get => _UnitPlane;
        }
        #endregion 

        #endregion

        #region # 方法

        #region 获取单位立方体 —— static MeshGeometry GetUnitCube()
        /// <summary>
        /// 获取单位立方体
        /// </summary>
        /// <returns>网格几何</returns>
        private static MeshGeometry GetUnitCube()
        {
            Vertex[] vertices =
            [
                new Vertex { Position = new Vector3(-0.5f, -0.5f, 0.5f) },
                new Vertex { Position = new Vector3(0.5f, -0.5f, 0.5f) },
                new Vertex { Position = new Vector3(0.5f, 0.5f, 0.5f) },
                new Vertex { Position = new Vector3(-0.5f, 0.5f, 0.5f) },
                new Vertex { Position = new Vector3(-0.5f, -0.5f, -0.5f) },
                new Vertex { Position = new Vector3(0.5f, -0.5f, -0.5f) },
                new Vertex { Position = new Vector3(0.5f, 0.5f, -0.5f) },
                new Vertex { Position = new Vector3(-0.5f, 0.5f, -0.5f) }
            ];
            uint[] indices =
            [
                0,1,2, 2,3,0, 1,5,6, 6,2,1,
                5,4,7, 7,6,5, 4,0,3, 3,7,4,
                3,2,6, 6,7,3, 4,5,1, 1,0,4
            ];

            MeshGeometry geometry = new MeshGeometry(vertices, indices);

            return geometry;
        }
        #endregion

        #region 获取单位平面 —— static MeshGeometry GetUnitPlane()
        /// <summary>
        /// 获取单位平面
        /// </summary>
        /// <returns>网格几何</returns>
        private static MeshGeometry GetUnitPlane()
        {
            //单位平面的顶点
            Vertex[] vertices =
            [
                //位置(-0.5, +0.5, 0) — 左上A
                new Vertex { Position = new Vector3(-0.5f, 0.5f, 0) },
                //位置(-0.5, -0.5, 0) — 左下B
                new Vertex { Position = new Vector3(-0.5f, -0.5f, 0) },
                //位置(+0.5, -0.5, 0) — 右下C
                new Vertex { Position = new Vector3(0.5f, -0.5f, 0) },
                //位置(+0.5, +0.5, 0) — 右上D
                new Vertex { Position = new Vector3(0.5f, 0.5f, 0) }
            ];

            //索引（两个三角形构成一个平面）
            uint[] indices =
            [
                1, 2, 3,  //第一个三角形 = △BCD
                3, 0, 1   //第二个三角形 = △DAB
            ];

            MeshGeometry geometry = new MeshGeometry(vertices, indices);

            return geometry;
        }
        #endregion

        #endregion
    }
}
