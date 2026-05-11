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
        /// 彩虹控制点列表
        /// </summary>
        private static readonly HUControlPoint[] _RainbowControlPoints;

        /// <summary>
        /// 热金属控制点列表
        /// </summary>
        private static readonly HUControlPoint[] _HotMetalControlPoints;

        /// <summary>
        /// 灰度控制点列表
        /// </summary>
        private static readonly DensityControlPoint[] _GrayControlPoints;

        /// <summary>
        /// 脑控制点列表
        /// </summary>
        private static readonly DensityControlPoint[] _BrainControlPoints;

        /// <summary>
        /// 心脏控制点列表
        /// </summary>
        private static readonly DensityControlPoint[] _CardiacControlPoints;

        /// <summary>
        /// 肝脏控制点列表
        /// </summary>
        private static readonly DensityControlPoint[] _LiverControlPoints;

        /// <summary>
        /// 肺控制点列表
        /// </summary>
        private static readonly DensityControlPoint[] _LungControlPoints;

        /// <summary>
        /// 腹部控制点列表
        /// </summary>
        private static readonly DensityControlPoint[] _AbdomenControlPoints;

        /// <summary>
        /// 骨骼控制点列表
        /// </summary>
        private static readonly DensityControlPoint[] _BoneControlPoints;

        /// <summary>
        /// 血管控制点列表
        /// </summary>
        private static readonly DensityControlPoint[] _VascularControlPoints;

        /// <summary>
        /// 预设控制点字典
        /// </summary>
        private static readonly IDictionary<string, DensityControlPoint[]> _PresetControlPointGroups;

        /// <summary>
        /// 静态构造器
        /// </summary>
        static ProtocolManager()
        {
            _RainbowControlPoints = GetRainbowControlPoints();
            _HotMetalControlPoints = GetHotMetalControlPoints();
            _GrayControlPoints = GetGrayControlPoints();
            _BrainControlPoints = GetBrainControlPoints();
            _CardiacControlPoints = GetCardiacControlPoints();
            _LiverControlPoints = GetLiverControlPoints();
            _LungControlPoints = GetLungControlPoints();
            _AbdomenControlPoints = GetAbdomenControlPoints();
            _BoneControlPoints = GetBoneControlPoints();
            _VascularControlPoints = GetVascularControlPoints();
            _PresetControlPointGroups = new Dictionary<string, DensityControlPoint[]>
            {
                { "灰度", _GrayControlPoints },
                { "脑", _BrainControlPoints },
                { "心脏", _CardiacControlPoints },
                { "肝脏", _LiverControlPoints },
                { "肺", _LungControlPoints },
                { "腹部", _AbdomenControlPoints },
                { "骨骼", _BoneControlPoints },
                { "血管", _VascularControlPoints }
            };
        }

        #endregion

        #region # 属性

        #region 只读属性 - 彩虹控制点列表 —— static IReadOnlyList<HUControlPoint> RainbowControlPoints
        /// <summary>
        /// 只读属性 - 彩虹控制点列表
        /// </summary>
        public static IReadOnlyList<HUControlPoint> RainbowControlPoints
        {
            get => _RainbowControlPoints;
        }
        #endregion 

        #region 只读属性 - 热金属控制点列表 —— static IReadOnlyList<HUControlPoint> HotMetalControlPoints
        /// <summary>
        /// 只读属性 - 热金属控制点列表
        /// </summary>
        public static IReadOnlyList<HUControlPoint> HotMetalControlPoints
        {
            get => _HotMetalControlPoints;
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

        #region 只读属性 - 脑控制点列表 —— static IReadOnlyList<DensityControlPoint> BrainControlPoints
        /// <summary>
        /// 只读属性 - 脑控制点列表
        /// </summary>
        public static IReadOnlyList<DensityControlPoint> BrainControlPoints
        {
            get => _BrainControlPoints;
        }
        #endregion

        #region 只读属性 - 心脏控制点列表 —— static IReadOnlyList<DensityControlPoint> CardiacControlPoints
        /// <summary>
        /// 只读属性 - 心脏控制点列表
        /// </summary>
        public static IReadOnlyList<DensityControlPoint> CardiacControlPoints
        {
            get => _CardiacControlPoints;
        }
        #endregion

        #region 只读属性 - 肝脏控制点列表 —— static IReadOnlyList<DensityControlPoint> LiverControlPoints
        /// <summary>
        /// 只读属性 - 肝脏控制点列表
        /// </summary>
        public static IReadOnlyList<DensityControlPoint> LiverControlPoints
        {
            get => _LiverControlPoints;
        }
        #endregion

        #region 只读属性 - 肺控制点列表 —— static IReadOnlyList<DensityControlPoint> LungControlPoints
        /// <summary>
        /// 只读属性 - 肺控制点列表
        /// </summary>
        public static IReadOnlyList<DensityControlPoint> LungControlPoints
        {
            get => _LungControlPoints;
        }
        #endregion

        #region 只读属性 - 腹部控制点列表 —— static IReadOnlyList<DensityControlPoint> AbdomenControlPoints
        /// <summary>
        /// 只读属性 - 腹部控制点列表
        /// </summary>
        public static IReadOnlyList<DensityControlPoint> AbdomenControlPoints
        {
            get => _AbdomenControlPoints;
        }
        #endregion

        #region 只读属性 - 骨骼控制点列表 —— static IReadOnlyList<DensityControlPoint> BoneControlPoints
        /// <summary>
        /// 只读属性 - 骨骼控制点列表
        /// </summary>
        public static IReadOnlyList<DensityControlPoint> BoneControlPoints
        {
            get => _BoneControlPoints;
        }
        #endregion

        #region 只读属性 - 血管控制点列表 —— static IReadOnlyList<DensityControlPoint> VascularControlPoints
        /// <summary>
        /// 只读属性 - 血管控制点列表
        /// </summary>
        public static IReadOnlyList<DensityControlPoint> VascularControlPoints
        {
            get => _VascularControlPoints;
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

        #region 获取彩虹控制点列表 —— static HUControlPoint[] GetRainbowControlPoints()
        /// <summary>
        /// 获取彩虹控制点列表
        /// </summary>
        /// <returns>控制点列表</returns>
        private static HUControlPoint[] GetRainbowControlPoints()
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

        #region 获取热金属控制点列表 —— static HUControlPoint[] GetHotMetalControlPoints()
        /// <summary>
        /// 获取热金属控制点列表
        /// </summary>
        /// <returns>控制点列表</returns>
        private static HUControlPoint[] GetHotMetalControlPoints()
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

        #region 获取脑控制点列表 —— static DensityControlPoint[] GetBrainControlPoints()
        /// <summary>
        /// 获取脑控制点列表
        /// </summary>
        /// <returns>控制点列表</returns>
        /// <remarks>配套窗宽80/窗位40</remarks>
        private static DensityControlPoint[] GetBrainControlPoints()
        {
            DensityControlPoint[] controlPoints =
            [
                //背景/脑脊液：全透明
                new(0.00f, new Vector4(0.00f, 0.00f, 0.00f, 0.000f)),
                new(0.15f, new Vector4(0.00f, 0.00f, 0.00f, 0.000f)),

                //灰质：开始浮现
                new(0.25f, new Vector4(0.30f, 0.30f, 0.35f, 0.020f)),
                new(0.35f, new Vector4(0.40f, 0.40f, 0.45f, 0.060f)),

                //透明凹陷
                new(0.42f, new Vector4(0.45f, 0.45f, 0.50f, 0.000f)),

                //白质：明显不透明
                new(0.55f, new Vector4(0.60f, 0.60f, 0.65f, 0.250f)),
                new(0.65f, new Vector4(0.75f, 0.75f, 0.80f, 0.550f)),

                //血管/钙化
                new(0.75f, new Vector4(0.85f, 0.85f, 0.88f, 0.780f)),
                new(0.85f, new Vector4(0.92f, 0.92f, 0.94f, 0.900f)),

                //骨骼
                new(0.95f, new Vector4(0.97f, 0.97f, 0.98f, 0.960f)),
                new(1.00f, new Vector4(1.00f, 1.00f, 1.00f, 1.000f))
            ];

            return controlPoints;
        }
        #endregion

        #region 获取心脏控制点列表 —— static DensityControlPoint[] GetCardiacControlPoints()
        /// <summary>
        /// 获取心脏控制点列表
        /// </summary>
        /// <returns>控制点列表</returns>
        /// <remarks>配套窗宽550/窗位60</remarks>
        private static DensityControlPoint[] GetCardiacControlPoints()
        {
            DensityControlPoint[] controlPoints =
            [
                //肺/背景：全透明
                new(0.00f, new Vector4(0.00f, 0.00f, 0.00f, 0.000f)),
                new(0.20f, new Vector4(0.00f, 0.00f, 0.00f, 0.000f)),

                //脂肪/心包：极淡
                new(0.30f, new Vector4(0.20f, 0.20f, 0.22f, 0.005f)),

                //透明凹陷
                new(0.38f, new Vector4(0.25f, 0.25f, 0.28f, 0.000f)),

                //心肌：开始可见
                new(0.45f, new Vector4(0.40f, 0.40f, 0.45f, 0.040f)),
                new(0.55f, new Vector4(0.55f, 0.55f, 0.60f, 0.120f)),

                //室间隔/乳头肌
                new(0.65f, new Vector4(0.70f, 0.70f, 0.75f, 0.350f)),

                //增强血管
                new(0.75f, new Vector4(0.82f, 0.82f, 0.85f, 0.600f)),

                //冠脉钙化
                new(0.85f, new Vector4(0.92f, 0.92f, 0.94f, 0.820f)),

                //骨骼
                new(0.95f, new Vector4(0.97f, 0.97f, 0.98f, 0.940f)),
                new(1.00f, new Vector4(1.00f, 1.00f, 1.00f, 1.000f))
            ];

            return controlPoints;
        }
        #endregion

        #region 获取肺控制点列表 —— static DensityControlPoint[] GetLungControlPoints()
        /// <summary>
        /// 获取肺控制点列表
        /// </summary>
        /// <returns>控制点列表</returns>
        /// <remarks>配套窗宽1500/窗位-600</remarks>
        private static DensityControlPoint[] GetLungControlPoints()
        {
            DensityControlPoint[] controlPoints =
            [
                //空气：全透明黑色
                new(0.00f, new Vector4(0.00f, 0.00f, 0.00f, 0.000f)),
                new(0.08f, new Vector4(0.00f, 0.00f, 0.00f, 0.000f)),

                //肺实质开始：淡蓝灰
                new(0.12f, new Vector4(0.12f, 0.15f, 0.20f, 0.008f)),
                new(0.18f, new Vector4(0.18f, 0.22f, 0.28f, 0.020f)),

                //透明凹陷
                new(0.25f, new Vector4(0.22f, 0.26f, 0.32f, 0.000f)),

                //肺纹理
                new(0.35f, new Vector4(0.28f, 0.32f, 0.38f, 0.050f)),
                new(0.45f, new Vector4(0.35f, 0.38f, 0.42f, 0.100f)),

                //透明凹陷
                new(0.52f, new Vector4(0.40f, 0.42f, 0.45f, 0.000f)),

                //肺门/血管
                new(0.60f, new Vector4(0.48f, 0.50f, 0.52f, 0.180f)),

                //软组织
                new(0.72f, new Vector4(0.60f, 0.55f, 0.50f, 0.350f)),

                //纵隔
                new(0.82f, new Vector4(0.75f, 0.70f, 0.65f, 0.650f)),

                //骨骼
                new(0.92f, new Vector4(0.90f, 0.88f, 0.82f, 0.880f)),
                new(1.00f, new Vector4(1.00f, 1.00f, 1.00f, 1.000f))
            ];

            return controlPoints;
        }
        #endregion

        #region 获取肝脏控制点列表 —— static DensityControlPoint[] GetLiverControlPoints()
        /// <summary>
        /// 获取肝脏控制点列表
        /// </summary>
        /// <returns>控制点列表</returns>
        /// <remarks>配套窗宽320/窗位60</remarks>
        private static DensityControlPoint[] GetLiverControlPoints()
        {
            DensityControlPoint[] controlPoints =
            [
                //背景：全透明
                new(0.00f, new Vector4(0.00f, 0.00f, 0.00f, 0.000f)),
                new(0.15f, new Vector4(0.00f, 0.00f, 0.00f, 0.000f)),

                //脂肪/腹水：极淡
                new(0.25f, new Vector4(0.18f, 0.18f, 0.20f, 0.005f)),

                //正常肝脏：开始着色
                new(0.40f, new Vector4(0.35f, 0.32f, 0.28f, 0.025f)),
                new(0.55f, new Vector4(0.50f, 0.45f, 0.38f, 0.070f)),

                //透明凹陷
                new(0.62f, new Vector4(0.55f, 0.50f, 0.42f, 0.000f)),

                //增强肝脏/血管
                new(0.72f, new Vector4(0.70f, 0.62f, 0.52f, 0.200f)),
                new(0.82f, new Vector4(0.82f, 0.72f, 0.60f, 0.500f)),

                //钙化/结石
                new(0.90f, new Vector4(0.92f, 0.88f, 0.80f, 0.800f)),

                //骨骼
                new(0.96f, new Vector4(0.97f, 0.95f, 0.90f, 0.930f)),
                new(1.00f, new Vector4(1.00f, 1.00f, 1.00f, 1.000f))
            ];

            return controlPoints;
        }
        #endregion

        #region 获取腹部控制点列表 —— static DensityControlPoint[] GetAbdomenControlPoints()
        /// <summary>
        /// 获取腹部控制点列表
        /// </summary>
        /// <returns>控制点列表</returns>
        /// <remarks>配套窗宽400/窗位40</remarks>
        private static DensityControlPoint[] GetAbdomenControlPoints()
        {
            DensityControlPoint[] controlPoints =
            [
                //背景：全透明
                new(0.00f, new Vector4(0.00f, 0.00f, 0.00f, 0.000f)),
                new(0.10f, new Vector4(0.00f, 0.00f, 0.00f, 0.000f)),

                //脂肪：微透明灰
                new(0.22f, new Vector4(0.20f, 0.18f, 0.15f, 0.008f)),

                //透明凹陷
                new(0.30f, new Vector4(0.25f, 0.22f, 0.20f, 0.000f)),

                //软组织/肌肉
                new(0.45f, new Vector4(0.40f, 0.35f, 0.30f, 0.030f)),

                //实质器官
                new(0.60f, new Vector4(0.55f, 0.48f, 0.38f, 0.080f)),

                //血管/增强
                new(0.75f, new Vector4(0.70f, 0.60f, 0.50f, 0.250f)),

                //骨骼边缘
                new(0.88f, new Vector4(0.85f, 0.80f, 0.70f, 0.600f)),

                //骨骼
                new(0.96f, new Vector4(0.95f, 0.92f, 0.85f, 0.900f)),
                new(1.00f, new Vector4(1.00f, 1.00f, 1.00f, 1.000f))
            ];

            return controlPoints;
        }
        #endregion

        #region 获取骨骼控制点列表 —— static DensityControlPoint[] GetBoneControlPoints()
        /// <summary>
        /// 获取骨骼控制点列表
        /// </summary>
        /// <returns>控制点列表</returns>
        /// <remarks>配套窗宽1200/窗位500</remarks>
        private static DensityControlPoint[] GetBoneControlPoints()
        {
            DensityControlPoint[] controlPoints =
            [
                //空气/软组织：全透明
                new(0.00f, new Vector4(0.00f, 0.00f, 0.00f, 0.000f)),
                new(0.25f, new Vector4(0.00f, 0.00f, 0.00f, 0.000f)),

                //松质骨过渡：开始着色
                new(0.35f, new Vector4(0.55f, 0.45f, 0.35f, 0.030f)),
                new(0.42f, new Vector4(0.70f, 0.58f, 0.42f, 0.080f)),

                //透明凹陷
                new(0.48f, new Vector4(0.78f, 0.68f, 0.50f, 0.000f)),

                //密质骨：陡峭上升
                new(0.52f, new Vector4(0.85f, 0.78f, 0.62f, 0.250f)),
                new(0.58f, new Vector4(0.92f, 0.88f, 0.72f, 0.550f)),

                //高密度骨
                new(0.68f, new Vector4(0.95f, 0.92f, 0.80f, 0.800f)),
                new(0.80f, new Vector4(0.98f, 0.96f, 0.88f, 0.930f)),

                //致密骨/金属
                new(0.92f, new Vector4(1.00f, 1.00f, 0.96f, 0.980f)),
                new(1.00f, new Vector4(1.00f, 1.00f, 1.00f, 1.000f))
            ];

            return controlPoints;
        }
        #endregion

        #region 获取血管控制点列表 —— static DensityControlPoint[] GetVascularControlPoints()
        /// <summary>
        /// 获取血管控制点列表
        /// </summary>
        /// <returns>控制点列表</returns>
        /// <remarks>配套窗宽600/窗位300</remarks>
        private static DensityControlPoint[] GetVascularControlPoints()
        {
            DensityControlPoint[] controlPoints =
            [
                //背景：全透明
                new(0.00f, new Vector4(0.00f, 0.00f, 0.00f, 0.000f)),
                new(0.15f, new Vector4(0.00f, 0.00f, 0.00f, 0.000f)),

                //软组织：极淡
                new(0.25f, new Vector4(0.18f, 0.18f, 0.20f, 0.003f)),

                //透明凹陷
                new(0.35f, new Vector4(0.25f, 0.25f, 0.28f, 0.000f)),

                //血管开始：对比增强
                new(0.45f, new Vector4(0.50f, 0.50f, 0.55f, 0.060f)),
                new(0.55f, new Vector4(0.70f, 0.70f, 0.75f, 0.200f)),

                //血管核心
                new(0.68f, new Vector4(0.85f, 0.85f, 0.88f, 0.500f)),
                new(0.80f, new Vector4(0.92f, 0.92f, 0.94f, 0.780f)),

                //钙化
                new(0.90f, new Vector4(0.96f, 0.96f, 0.97f, 0.920f)),

                //骨骼
                new(0.97f, new Vector4(0.99f, 0.99f, 0.99f, 0.970f)),
                new(1.00f, new Vector4(1.00f, 1.00f, 1.00f, 1.000f))
            ];

            return controlPoints;
        }
        #endregion

        #endregion
    }
}
