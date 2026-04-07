using MedicalSharp.Primitives.Maths;
using MedicalSharp.Primitives.Models;
using OpenTK.Mathematics;
using System.Collections.Generic;

namespace MedicalSharp.Primitives.Managers
{
    /// <summary>
    /// 资源管理器
    /// </summary>
    public static class ResourceManager
    {
        #region # 字段及构造器

        /// <summary>
        /// 单位立方体
        /// </summary>
        private static readonly MeshGeometry _UnitCube;

        /// <summary>
        /// 单位平面
        /// </summary>
        private static readonly MeshGeometry _UnitPlane;

        /// <summary>
        /// 灰度控制点集
        /// </summary>
        private static readonly TFControlPoint[] _GrayControlPoints;

        /// <summary>
        /// 彩虹控制点集
        /// </summary>
        private static readonly TFControlPoint[] _RainbowControlPoints;

        /// <summary>
        /// 骨骼控制点集
        /// </summary>
        private static readonly TFControlPoint[] _BoneControlPoints;

        /// <summary>
        /// 静态构造器
        /// </summary>
        static ResourceManager()
        {
            ResourceManager._UnitCube = ResourceManager.GetUnitCube();
            ResourceManager._UnitPlane = ResourceManager.GetUnitPlane();
            ResourceManager._GrayControlPoints = ResourceManager.GetGrayControlPoints();
            ResourceManager._RainbowControlPoints = ResourceManager.GetRainbowControlPoints();
            ResourceManager._BoneControlPoints = ResourceManager.GetBoneControlPoints();
        }

        #endregion

        #region # 属性

        #region 只读属性 - 单位立方体 —— static MeshGeometry UnitCube
        /// <summary>
        /// 只读属性 - 单位立方体
        /// </summary>
        public static MeshGeometry UnitCube
        {
            get => ResourceManager._UnitCube;
        }
        #endregion 

        #region 只读属性 - 单位平面 —— static MeshGeometry UnitPlane
        /// <summary>
        /// 只读属性 - 单位平面
        /// </summary>
        public static MeshGeometry UnitPlane
        {
            get => ResourceManager._UnitPlane;
        }
        #endregion 

        #region 只读属性 - 灰度控制点列表 —— static IReadOnlyList<TFControlPoint> GrayControlPoints
        /// <summary>
        /// 只读属性 - 灰度控制点列表
        /// </summary>
        public static IReadOnlyList<TFControlPoint> GrayControlPoints
        {
            get => ResourceManager._GrayControlPoints.AsReadOnly();
        }
        #endregion 

        #region 只读属性 - 彩虹控制点列表 —— static IReadOnlyList<TFControlPoint> RainbowControlPoints
        /// <summary>
        /// 只读属性 - 彩虹控制点列表
        /// </summary>
        public static IReadOnlyList<TFControlPoint> RainbowControlPoints
        {
            get => ResourceManager._RainbowControlPoints.AsReadOnly();
        }
        #endregion 

        #region 只读属性 - 骨骼控制点列表 —— static IReadOnlyList<TFControlPoint> BoneControlPoints
        /// <summary>
        /// 只读属性 - 骨骼控制点列表
        /// </summary>
        public static IReadOnlyList<TFControlPoint> BoneControlPoints
        {
            get => ResourceManager._BoneControlPoints.AsReadOnly();
        }
        #endregion 

        #endregion

        #region # 方法

        //Private

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
                //位置(-0.5, -0.5, 0), 纹理坐标(0,0)
                new Vertex
                {
                    Position = new Vector3(-0.5f, -0.5f, 0),
                    TextureCoord = new Vector2(0, 0)
                },
                //位置(0.5, -0.5, 0), 纹理坐标(1,0)
                new Vertex
                {
                    Position = new Vector3(0.5f, -0.5f, 0),
                    TextureCoord = new Vector2(1, 0)
                },
                //位置(0.5, 0.5, 0), 纹理坐标(1,1)
                new Vertex
                {
                    Position = new Vector3(0.5f, 0.5f, 0),
                    TextureCoord = new Vector2(1, 1)
                },
                //位置(-0.5, 0.5, 0), 纹理坐标(0,1)
                new Vertex
                {
                    Position = new Vector3(-0.5f, 0.5f, 0),
                    TextureCoord = new Vector2(0, 1)
                }
            ];

            //索引（两个三角形构成一个平面）
            uint[] indices =
            [
                0, 1, 2,  //第一个三角形
                2, 3, 0   //第二个三角形
            ];

            MeshGeometry geometry = new MeshGeometry(vertices, indices);

            return geometry;
        }
        #endregion

        #region 获取灰度控制点列表 —— static TFControlPoint[] GetGrayControlPoints()
        /// <summary>
        /// 获取灰度控制点列表
        /// </summary>
        /// <returns>控制点列表</returns>
        private static TFControlPoint[] GetGrayControlPoints()
        {
            TFControlPoint[] controlPoints =
            [
                new TFControlPoint(0.0f, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)),
                new TFControlPoint(1.0f, new Vector4(1.0f, 1.0f, 1.0f, 1.0f))
            ];

            return controlPoints;
        }
        #endregion

        #region 获取彩虹控制点列表 —— static TFControlPoint[] GetRainbowControlPoints()
        /// <summary>
        /// 获取彩虹控制点列表
        /// </summary>
        /// <returns>控制点列表</returns>
        private static TFControlPoint[] GetRainbowControlPoints()
        {
            TFControlPoint[] controlPoints =
            [
                new TFControlPoint(0.0f, new Vector4(0.0f, 0.0f, 0.5f, 0.0f)),
                new TFControlPoint(0.25f, new Vector4(0.0f, 0.5f, 1.0f, 0.3f)),
                new TFControlPoint(0.5f, new Vector4(0.0f, 1.0f, 0.5f, 0.6f)),
                new TFControlPoint(0.75f, new Vector4(1.0f, 1.0f, 0.0f, 0.8f)),
                new TFControlPoint(1.0f, new Vector4(1.0f, 0.0f, 0.0f, 1.0f))
            ];

            return controlPoints;
        }
        #endregion

        #region 获取骨骼控制点列表 —— static TFControlPoint[] GetBoneControlPoints()
        /// <summary>
        /// 获取骨骼控制点列表
        /// </summary>
        /// <returns>控制点列表</returns>
        private static TFControlPoint[] GetBoneControlPoints()
        {
            TFControlPoint[] controlPoints =
            [
                //完全透明背景（空气/背景）
                new TFControlPoint(0.00f, new Vector4(0.0f, 0.0f, 0.0f, 0.00f)),
                new TFControlPoint(0.30f, new Vector4(0.0f, 0.0f, 0.0f, 0.00f)),   //保持透明到30%

                //软组织：极低透明度（几乎透明）
                new TFControlPoint(0.35f, new Vector4(0.3f, 0.3f, 0.3f, 0.005f)),  //0.5%透明度
                new TFControlPoint(0.40f, new Vector4(0.4f, 0.4f, 0.4f, 0.008f)),  //0.8%透明度
                new TFControlPoint(0.45f, new Vector4(0.5f, 0.5f, 0.5f, 0.010f)),  //1.0%透明度

                //骨骼开始：陡峭变化
                new TFControlPoint(0.48f, new Vector4(0.7f, 0.6f, 0.5f, 0.02f)),   //过渡开始
                new TFControlPoint(0.50f, new Vector4(0.8f, 0.7f, 0.6f, 0.50f)),   //快速变不透明！
                new TFControlPoint(0.52f, new Vector4(0.9f, 0.8f, 0.7f, 0.85f)),   //非常不透明

                //标准骨骼：高不透明度
                new TFControlPoint(0.55f, new Vector4(1.0f, 0.9f, 0.8f, 0.92f)),
                new TFControlPoint(0.60f, new Vector4(1.0f, 0.95f, 0.85f, 0.95f)),
                new TFControlPoint(0.65f, new Vector4(1.0f, 0.97f, 0.90f, 0.97f)),

                //高密度骨骼：完全不透明
                new TFControlPoint(0.70f, new Vector4(1.0f, 0.98f, 0.93f, 0.98f)),
                new TFControlPoint(0.80f, new Vector4(1.0f, 1.0f, 0.96f, 0.99f)),
                new TFControlPoint(0.90f, new Vector4(1.0f, 1.0f, 0.98f, 0.995f)),
                new TFControlPoint(1.00f, new Vector4(1.0f, 1.0f, 1.0f, 1.000f))
            ];

            return controlPoints;
        }
        #endregion

        #endregion
    }
}
