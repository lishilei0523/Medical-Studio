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
        private static readonly HUControlPoint[] _GrayControlPoints;

        /// <summary>
        /// 彩虹控制点列表
        /// </summary>
        private static readonly HUControlPoint[] _RainbowControlPoints;

        /// <summary>
        /// 解剖色控制点列表
        /// </summary>
        private static readonly HUControlPoint[] _AnatomyControlPoints;

        /// <summary>
        /// 预设控制点字典
        /// </summary>
        private static readonly IDictionary<string, HUControlPoint[]> _PresetControlPointGroups;

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
            _PresetControlPointGroups = new Dictionary<string, HUControlPoint[]>
            {
                { "灰度", _GrayControlPoints },
                { "彩虹", _RainbowControlPoints },
                { "解剖", _AnatomyControlPoints },
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

        #region 只读属性 - 灰度控制点列表 —— static IReadOnlyList<HUControlPoint> GrayControlPoints
        /// <summary>
        /// 只读属性 - 灰度控制点列表
        /// </summary>
        public static IReadOnlyList<HUControlPoint> GrayControlPoints
        {
            get => _GrayControlPoints;
        }
        #endregion

        #region 只读属性 - 彩虹控制点列表 —— static IReadOnlyList<HUControlPoint> RainbowControlPoints
        /// <summary>
        /// 只读属性 - 彩虹控制点列表
        /// </summary>
        public static IReadOnlyList<HUControlPoint> RainbowControlPoints
        {
            get => _RainbowControlPoints;
        }
        #endregion

        #region 只读属性 - 解剖色控制点列表 —— static IReadOnlyList<HUControlPoint> AnatomyControlPoints
        /// <summary>
        /// 只读属性 - 解剖色控制点列表
        /// </summary>
        public static IReadOnlyList<HUControlPoint> AnatomyControlPoints
        {
            get => _AnatomyControlPoints;
        }
        #endregion

        #region 只读属性 - 预设控制点字典 —— IDictionary<string, HUControlPoint[]> PresetControlPointGroups
        /// <summary>
        /// 只读属性 - 预设控制点字典
        /// </summary>
        public static IDictionary<string, HUControlPoint[]> PresetControlPointGroups
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

        #region 获取灰度控制点列表 —— static HUControlPoint[] GetGrayControlPoints()
        /// <summary>
        /// 获取灰度控制点列表
        /// </summary>
        /// <returns>控制点列表</returns>
        private static HUControlPoint[] GetGrayControlPoints()
        {
            HUControlPoint[] controlPoints =
            [
                new HUControlPoint(-1024, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)),
                new HUControlPoint(3071, new Vector4(1.0f, 1.0f, 1.0f, 1.0f))
            ];

            return controlPoints;
        }
        #endregion

        #region 获取彩虹控制点列表 —— static HUControlPoint[] GetRainbowControlPoints()
        /// <summary>
        /// 获取彩虹控制点列表
        /// </summary>
        /// <returns>控制点列表</returns>
        private static HUControlPoint[] GetRainbowControlPoints()
        {
            HUControlPoint[] controlPoints =
            [
                //空气：透明深蓝
                new HUControlPoint(-1024, new Vector4(0.0f, 0.0f, 0.5f, 0.000f)),

                //肺实质：蓝，极低透明度
                new HUControlPoint(-900, new Vector4(0.0f, 0.0f, 1.0f, 0.005f)),
                new HUControlPoint(-800, new Vector4(0.0f, 0.3f, 1.0f, 0.015f)),

                //肺纹理：青，低透明度
                new HUControlPoint(-600, new Vector4(0.0f, 0.8f, 0.8f, 0.030f)),
                new HUControlPoint(-400, new Vector4(0.0f, 1.0f, 0.5f, 0.050f)),

                //软组织/水：绿，中低透明度
                new HUControlPoint(-100, new Vector4(0.0f, 1.0f, 0.0f, 0.080f)),
                new HUControlPoint(0, new Vector4(0.3f, 1.0f, 0.0f, 0.120f)),

                //肌肉/肝脏：黄绿，中透明度
                new HUControlPoint(100, new Vector4(1.0f, 1.0f, 0.0f, 0.200f)),
                new HUControlPoint(200, new Vector4(1.0f, 0.8f, 0.0f, 0.300f)),

                //松质骨：橙，中高透明度
                new HUControlPoint(400, new Vector4(1.0f, 0.5f, 0.0f, 0.500f)),

                //密质骨：红，高透明度
                new HUControlPoint(700, new Vector4(1.0f, 0.0f, 0.0f, 0.750f)),
                new HUControlPoint(1000, new Vector4(0.9f, 0.0f, 0.0f, 0.900f)),

                //致密骨/金属：深红，完全不透明
                new HUControlPoint(2000, new Vector4(0.8f, 0.0f, 0.0f, 0.980f)),
                new HUControlPoint(3071, new Vector4(0.6f, 0.0f, 0.0f, 1.000f))
            ];

            return controlPoints;
        }
        #endregion

        #region 获取解剖色控制点列表 —— static HUControlPoint[] GetAnatomyControlPoints()
        /// <summary>
        /// 获取解剖色控制点列表
        /// </summary>
        /// <returns>控制点列表</returns>
        private static HUControlPoint[] GetAnatomyControlPoints()
        {
            HUControlPoint[] controlPoints =
            [
                //空气：透明黑
                new HUControlPoint(-1024, new Vector4(0.00f, 0.00f, 0.00f, 0.000f)),
                new HUControlPoint(-950, new Vector4(0.02f, 0.02f, 0.04f, 0.002f)),

                //肺实质：淡粉灰
                new HUControlPoint(-900, new Vector4(0.18f, 0.12f, 0.15f, 0.010f)),
                new HUControlPoint(-850, new Vector4(0.28f, 0.18f, 0.22f, 0.018f)),
                new HUControlPoint(-800, new Vector4(0.35f, 0.22f, 0.28f, 0.025f)),

                //透明凹陷
                new HUControlPoint(-700, new Vector4(0.30f, 0.20f, 0.25f, 0.000f)),

                //肌肉：红褐色
                new HUControlPoint(-500, new Vector4(0.55f, 0.25f, 0.18f, 0.040f)),
                new HUControlPoint(-350, new Vector4(0.62f, 0.30f, 0.20f, 0.060f)),
                new HUControlPoint(-200, new Vector4(0.68f, 0.32f, 0.22f, 0.080f)),

                //肝脏/实质器官：深红褐
                new HUControlPoint(-50, new Vector4(0.65f, 0.25f, 0.18f, 0.110f)),
                new HUControlPoint(0, new Vector4(0.60f, 0.20f, 0.15f, 0.150f)),
                new HUControlPoint(50, new Vector4(0.66f, 0.24f, 0.18f, 0.210f)),
                new HUControlPoint(100, new Vector4(0.72f, 0.28f, 0.20f, 0.280f)),

                //透明凹陷
                new HUControlPoint(200, new Vector4(0.75f, 0.40f, 0.30f, 0.000f)),

                //松质骨：Alpha缓慢爬升
                new HUControlPoint(250, new Vector4(0.62f, 0.28f, 0.10f, 0.050f)),
                new HUControlPoint(300, new Vector4(0.65f, 0.32f, 0.12f, 0.100f)),
                new HUControlPoint(350, new Vector4(0.68f, 0.36f, 0.15f, 0.180f)),
                new HUControlPoint(400, new Vector4(0.72f, 0.42f, 0.18f, 0.300f)),

                //密质骨：Alpha逐步提升
                new HUControlPoint(500, new Vector4(0.75f, 0.48f, 0.22f, 0.450f)),
                new HUControlPoint(600, new Vector4(0.78f, 0.52f, 0.25f, 0.580f)),
                new HUControlPoint(800, new Vector4(0.82f, 0.58f, 0.30f, 0.700f)),
                new HUControlPoint(1000, new Vector4(0.88f, 0.65f, 0.38f, 0.820f)),

                //高密度骨
                new HUControlPoint(1500, new Vector4(0.94f, 0.78f, 0.52f, 0.920f)),
                new HUControlPoint(2000, new Vector4(0.97f, 0.85f, 0.62f, 0.970f)),

                //金属
                new HUControlPoint(3071, new Vector4(1.00f, 1.00f, 1.00f, 1.000f))
            ];

            return controlPoints;
        }
        #endregion

        #endregion
    }
}
