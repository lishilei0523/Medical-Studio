using MedicalSharp.Primitives.Models;
using OpenTK.Mathematics;
using System.Collections.Generic;

namespace MedicalSharp.Primitives.Managers
{
    /// <summary>
    /// 协议管理器
    /// </summary>
    public static class ProtocolManager
    {
        #region # 字段及构造器

        /// <summary>
        /// 固体彩虹控制点列表
        /// </summary>
        private static readonly HUControlPoint[] _SolidRainbowControlPoints;

        /// <summary>
        /// 固体热金属控制点列表
        /// </summary>
        private static readonly HUControlPoint[] _SolidHotMetalControlPoints;

        /// <summary>
        /// 灰度控制点列表
        /// </summary>
        private static readonly DensityControlPoint[] _GrayControlPoints;

        /// <summary>
        /// 彩虹控制点列表
        /// </summary>
        private static readonly DensityControlPoint[] _RainbowControlPoints;

        /// <summary>
        /// 解剖控制点列表
        /// </summary>
        private static readonly DensityControlPoint[] _AnatomyControlPoints;

        /// <summary>
        /// 预设控制点字典
        /// </summary>
        private static readonly IDictionary<string, DensityControlPoint[]> _PresetControlPointGroups;

        /// <summary>
        /// 静态构造器
        /// </summary>
        static ProtocolManager()
        {
            _SolidRainbowControlPoints = GetSolidRainbowControlPoints();
            _SolidHotMetalControlPoints = GetSolidHotMetalControlPoints();
            _GrayControlPoints = GetGrayControlPoints();
            _RainbowControlPoints = GetRainbowControlPoints();
            _AnatomyControlPoints = GetAnatomyControlPoints();
            _PresetControlPointGroups = new Dictionary<string, DensityControlPoint[]>
            {
                { "灰度", _GrayControlPoints },
                { "彩虹", _RainbowControlPoints },
                { "解剖", _AnatomyControlPoints }
            };
        }

        #endregion

        #region # 属性

        #region 只读属性 - 固体彩虹控制点列表 —— static IReadOnlyList<HUControlPoint> SolidRainbowControlPoints
        /// <summary>
        /// 只读属性 - 固体彩虹控制点列表
        /// </summary>
        public static IReadOnlyList<HUControlPoint> SolidRainbowControlPoints
        {
            get => _SolidRainbowControlPoints;
        }
        #endregion 

        #region 只读属性 - 固体热金属控制点列表 —— static IReadOnlyList<HUControlPoint> SolidHotMetalControlPoints
        /// <summary>
        /// 只读属性 - 固体热金属控制点列表
        /// </summary>
        public static IReadOnlyList<HUControlPoint> SolidHotMetalControlPoints
        {
            get => _SolidHotMetalControlPoints;
        }
        #endregion 

        #region 只读属性 - 灰度控制点列表 —— static IReadOnlyList<DensityControlPoint> GrayControlPoints
        /// <summary>
        /// 只读属性 - 灰度控制点列表
        /// </summary>
        public static IReadOnlyList<DensityControlPoint> GrayControlPoints
        {
            get => _GrayControlPoints;
        }
        #endregion

        #region 只读属性 - 彩虹控制点列表 —— static IReadOnlyList<DensityControlPoint> RainbowControlPoints
        /// <summary>
        /// 只读属性 - 彩虹控制点列表
        /// </summary>
        public static IReadOnlyList<DensityControlPoint> RainbowControlPoints
        {
            get => _RainbowControlPoints;
        }
        #endregion 

        #region 只读属性 - 解剖控制点列表 —— static IReadOnlyList<DensityControlPoint> AnatomyControlPoints
        /// <summary>
        /// 只读属性 - 解剖控制点列表
        /// </summary>
        public static IReadOnlyList<DensityControlPoint> AnatomyControlPoints
        {
            get => _AnatomyControlPoints;
        }
        #endregion

        #region 只读属性 - 预设控制点字典 —— IDictionary<string, DensityControlPoint[]> PresetControlPointGroups
        /// <summary>
        /// 只读属性 - 预设控制点字典
        /// </summary>
        public static IDictionary<string, DensityControlPoint[]> PresetControlPointGroups
        {
            get => _PresetControlPointGroups;
        }
        #endregion

        #endregion

        #region # 方法

        #region 获取固体彩虹控制点列表 —— static HUControlPoint[] GetSolidRainbowControlPoints()
        /// <summary>
        /// 获取固体彩虹控制点列表
        /// </summary>
        /// <returns>控制点列表</returns>
        private static HUControlPoint[] GetSolidRainbowControlPoints()
        {
            HUControlPoint[] controlPoints =
            [
                new(-1024, new Vector4(0.0f, 0.0f, 0.5f, 1.0f)),  //深蓝 - 空气
                new(-800, new Vector4(0.0f, 0.0f, 1.0f, 1.0f)),   //蓝 - 肺
                new(-400, new Vector4(0.0f, 0.8f, 0.8f, 1.0f)),   //青 - 肺纹理
                new(0, new Vector4(0.0f, 1.0f, 0.0f, 1.0f)),      //绿 - 水/软组织
                new(100, new Vector4(1.0f, 1.0f, 0.0f, 1.0f)),    //黄 - 肌肉
                new(400, new Vector4(1.0f, 0.5f, 0.0f, 1.0f)),    //橙 - 松质骨
                new(1000, new Vector4(1.0f, 0.0f, 0.0f, 1.0f)),   //红 - 密质骨
                new(3071, new Vector4(0.8f, 0.0f, 0.0f, 1.0f))    //深红 - 致密骨/金属
            ];

            return controlPoints;
        }
        #endregion

        #region 获取固体热金属控制点列表 —— static HUControlPoint[] GetSolidHotMetalControlPoints()
        /// <summary>
        /// 获取固体热金属控制点列表
        /// </summary>
        /// <returns>控制点列表</returns>
        private static HUControlPoint[] GetSolidHotMetalControlPoints()
        {
            HUControlPoint[] controlPoints =
            [
                new(-1024, new Vector4(0.0f, 0.0f, 0.0f, 1.0f)),  //黑 - 空气
                new(-400, new Vector4(0.3f, 0.0f, 0.0f, 1.0f)),   //暗红 - 肺
                new(0, new Vector4(0.8f, 0.2f, 0.0f, 1.0f)),      //红 - 软组织
                new(200, new Vector4(1.0f, 0.5f, 0.0f, 1.0f)),    //橙 - 肌肉
                new(500, new Vector4(1.0f, 0.8f, 0.0f, 1.0f)),    //橙黄 - 松质骨
                new(1000, new Vector4(1.0f, 1.0f, 0.0f, 1.0f)),   //黄 - 密质骨
                new(2000, new Vector4(1.0f, 1.0f, 0.5f, 1.0f)),   //浅黄 - 致密骨
                new(3071, new Vector4(1.0f, 1.0f, 1.0f, 1.0f))    //白 - 金属
            ];

            return controlPoints;
        }
        #endregion

        #region 获取灰度控制点列表 —— static DensityControlPoint[] GetGrayControlPoints()
        /// <summary>
        /// 获取灰度控制点列表
        /// </summary>
        /// <returns>控制点列表</returns>
        private static DensityControlPoint[] GetGrayControlPoints()
        {
            DensityControlPoint[] controlPoints =
            [
                new(0.0f, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)),
                new(1.0f, new Vector4(1.0f, 1.0f, 1.0f, 1.0f))
            ];

            return controlPoints;
        }
        #endregion

        #region 获取彩虹控制点列表 —— static DensityControlPoint[] GetRainbowControlPoints()
        /// <summary>
        /// 获取彩虹控制点列表
        /// </summary>
        /// <returns>控制点列表</returns>
        private static DensityControlPoint[] GetRainbowControlPoints()
        {
            DensityControlPoint[] controlPoints =
            [
                //空气：透明深蓝
                new DensityControlPoint(0.00f, new Vector4(0.00f, 0.00f, 0.50f, 0.000f)),

                //肺实质：蓝，极低Alpha——让光线穿透
                new DensityControlPoint(0.05f, new Vector4(0.00f, 0.00f, 1.00f, 0.001f)),
                new DensityControlPoint(0.12f, new Vector4(0.00f, 0.30f, 1.00f, 0.002f)),

                //肺纹理：青，低Alpha
                new DensityControlPoint(0.20f, new Vector4(0.00f, 0.80f, 0.80f, 0.004f)),
                new DensityControlPoint(0.28f, new Vector4(0.00f, 1.00f, 0.50f, 0.008f)),

                //软组织/水：绿，低Alpha
                new DensityControlPoint(0.36f, new Vector4(0.00f, 1.00f, 0.00f, 0.015f)),
                new DensityControlPoint(0.44f, new Vector4(0.30f, 1.00f, 0.00f, 0.025f)),

                //肌肉/肝脏：黄绿，中等Alpha
                new DensityControlPoint(0.52f, new Vector4(1.00f, 1.00f, 0.00f, 0.050f)),
                new DensityControlPoint(0.60f, new Vector4(1.00f, 0.80f, 0.00f, 0.120f)),

                //骨骼过渡区：橙色，Alpha快速拉升
                new DensityControlPoint(0.66f, new Vector4(1.00f, 0.60f, 0.05f, 0.300f)),
                new DensityControlPoint(0.70f, new Vector4(1.00f, 0.20f, 0.00f, 0.550f)),

                //密质骨：红色，高Alpha主导
                new DensityControlPoint(0.78f, new Vector4(1.00f, 0.00f, 0.00f, 0.800f)),
                new DensityControlPoint(0.88f, new Vector4(0.80f, 0.00f, 0.00f, 0.930f)),

                //致密骨/金属：深红
                new DensityControlPoint(1.00f, new Vector4(0.50f, 0.00f, 0.00f, 1.000f))
            ];

            return controlPoints;
        }
        #endregion

        #region 获取解剖控制点列表 —— static DensityControlPoint[] GetAnatomyControlPoints()
        /// <summary>
        /// 获取解剖控制点列表
        /// </summary>
        /// <returns>控制点列表</returns>
        private static DensityControlPoint[] GetAnatomyControlPoints()
        {
            DensityControlPoint[] controlPoints =
            [
                //空气：透明黑
                new DensityControlPoint(0.00f, new Vector4(0.00f, 0.00f, 0.00f, 0.000f)),

                //肺实质：淡粉灰
                new DensityControlPoint(0.08f, new Vector4(0.18f, 0.12f, 0.15f, 0.010f)),
                new DensityControlPoint(0.18f, new Vector4(0.35f, 0.22f, 0.28f, 0.025f)),

                //透明凹陷
                new DensityControlPoint(0.25f, new Vector4(0.30f, 0.20f, 0.25f, 0.000f)),

                //肌肉：红褐色
                new DensityControlPoint(0.35f, new Vector4(0.55f, 0.25f, 0.18f, 0.040f)),
                new DensityControlPoint(0.45f, new Vector4(0.68f, 0.32f, 0.22f, 0.080f)),

                //肝脏/实质器官：深红褐
                new DensityControlPoint(0.55f, new Vector4(0.60f, 0.20f, 0.15f, 0.150f)),
                new DensityControlPoint(0.65f, new Vector4(0.72f, 0.28f, 0.20f, 0.280f)),

                //透明凹陷
                new DensityControlPoint(0.70f, new Vector4(0.75f, 0.40f, 0.30f, 0.000f)),

                //骨骼：象牙白带暖色
                new DensityControlPoint(0.78f, new Vector4(0.88f, 0.72f, 0.50f, 0.450f)),
                new DensityControlPoint(0.88f, new Vector4(0.95f, 0.88f, 0.72f, 0.750f)),

                //密质骨/钙化：亮白
                new DensityControlPoint(0.95f, new Vector4(1.00f, 0.96f, 0.88f, 0.920f)),
                new DensityControlPoint(1.00f, new Vector4(1.00f, 1.00f, 1.00f, 1.000f))
            ];

            return controlPoints;
        }
        #endregion

        #endregion
    }
}
