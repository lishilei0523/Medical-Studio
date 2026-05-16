using MedicalSharp.Primitives.Maths;
using MedicalSharp.Primitives.Models;
using OpenTK.Mathematics;

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
        public const string FontPath = "Content/Fonts/msyh.ttf";

        /// <summary>
        /// 单位平面
        /// </summary>
        private static readonly MeshGeometry _UnitPlane;

        /// <summary>
        /// 单位立方体
        /// </summary>
        private static readonly MeshGeometry _UnitCube;

        /// <summary>
        /// 横断位平面
        /// </summary>
        private static readonly MeshGeometry _AxialPlane;

        /// <summary>
        /// 冠状位平面
        /// </summary>
        private static readonly MeshGeometry _CoronalPlane;

        /// <summary>
        /// 矢状位平面
        /// </summary>
        private static readonly MeshGeometry _SagittalPlane;

        /// <summary>
        /// 静态构造器
        /// </summary>
        static ResourceManager()
        {
            _UnitCube = GetUnitCube();
            _UnitPlane = GetUnitPlane();
            _AxialPlane = GetAxialPlane();
            _CoronalPlane = GetCoronalPlane();
            _SagittalPlane = GetSagittalPlane();
        }

        #endregion

        #region # 属性

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

        #region 只读属性 - 横断位平面 —— static MeshGeometry AxialPlane
        /// <summary>
        /// 只读属性 - 横断位平面
        /// </summary>
        public static MeshGeometry AxialPlane
        {
            get => _AxialPlane;
        }
        #endregion 

        #region 只读属性 - 冠状位平面 —— static MeshGeometry CoronalPlane
        /// <summary>
        /// 只读属性 - 冠状位平面
        /// </summary>
        public static MeshGeometry CoronalPlane
        {
            get => _CoronalPlane;
        }
        #endregion 

        #region 只读属性 - 矢状位平面 —— static MeshGeometry SagittalPlane
        /// <summary>
        /// 只读属性 - 矢状位平面
        /// </summary>
        public static MeshGeometry SagittalPlane
        {
            get => _SagittalPlane;
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
            //单位平面的顶点（包含纹理坐标）
            Vertex[] vertices =
            [
                //位置(-0.5, +0.5, 0), 纹理坐标(0,1) — 左上A
                new Vertex
                {
                    Position = new Vector3(-0.5f, 0.5f, 0),
                    TextureCoord = new Vector2(0, 1)
                },
                //位置(-0.5, -0.5, 0), 纹理坐标(0,0) — 左下B
                new Vertex
                {
                    Position = new Vector3(-0.5f, -0.5f, 0),
                    TextureCoord = new Vector2(0, 0)
                },
                //位置(+0.5, -0.5, 0), 纹理坐标(1,0) — 右下C
                new Vertex
                {
                    Position = new Vector3(0.5f, -0.5f, 0),
                    TextureCoord = new Vector2(1, 0)
                },
                //位置(+0.5, +0.5, 0), 纹理坐标(1,1) — 右上D
                new Vertex
                {
                    Position = new Vector3(0.5f, 0.5f, 0),
                    TextureCoord = new Vector2(1, 1)
                },
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

        #region 获取横断位平面 —— static MeshGeometry GetAxialPlane()
        /// <summary>
        /// 获取横断位平面
        /// </summary>
        /// <returns>网格几何</returns>
        private static MeshGeometry GetAxialPlane()
        {
            //单位平面的顶点
            Vertex[] vertices =
            [
                //位置(-0.5, +0.5, 0) — 左上A
                new Vertex
                {
                    Position = new Vector3(-0.5f, 0.5f, 0)
                },
                //位置(-0.5, -0.5, 0) — 左下B
                new Vertex
                {
                    Position = new Vector3(-0.5f, -0.5f, 0)
                },
                //位置(+0.5, -0.5, 0) — 右下C
                new Vertex
                {
                    Position = new Vector3(0.5f, -0.5f, 0)
                },
                //位置(+0.5, +0.5, 0) — 右上D
                new Vertex
                {
                    Position = new Vector3(0.5f, 0.5f, 0)
                }
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

        #region 获取冠状位平面 —— static MeshGeometry GetCoronalPlane()
        /// <summary>
        /// 获取冠状位平面
        /// </summary>
        /// <returns>网格几何</returns>
        private static MeshGeometry GetCoronalPlane()
        {
            //单位平面的顶点
            Vertex[] vertices =
            [
                //位置(-0.5, +0.5, 0) — 左上A
                new Vertex
                {
                    Position = new Vector3(-0.5f, 0.5f, 0)
                },
                //位置(-0.5, -0.5, 0) — 左下B
                new Vertex
                {
                    Position = new Vector3(-0.5f, -0.5f, 0)
                },
                //位置(+0.5, -0.5, 0) — 右下C
                new Vertex
                {
                    Position = new Vector3(0.5f, -0.5f, 0)
                },
                //位置(+0.5, +0.5, 0) — 右上D
                new Vertex
                {
                    Position = new Vector3(0.5f, 0.5f, 0)
                }
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

        #region 获取矢状位平面 —— static MeshGeometry GetSagittalPlane()
        /// <summary>
        /// 获取矢状位平面
        /// </summary>
        /// <returns>网格几何</returns>
        private static MeshGeometry GetSagittalPlane()
        {
            //单位平面的顶点
            Vertex[] vertices =
            [
                //位置(-0.5, +0.5, 0) — 左上A
                new Vertex
                {
                    Position = new Vector3(-0.5f, 0.5f, 0)
                },
                //位置(-0.5, -0.5, 0) — 左下B
                new Vertex
                {
                    Position = new Vector3(-0.5f, -0.5f, 0)
                },
                //位置(+0.5, -0.5, 0) — 右下C
                new Vertex
                {
                    Position = new Vector3(0.5f, -0.5f, 0)
                },
                //位置(+0.5, +0.5, 0) — 右上D
                new Vertex
                {
                    Position = new Vector3(0.5f, 0.5f, 0)
                }
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
