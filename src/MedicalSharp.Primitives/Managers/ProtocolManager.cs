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
        /// 灰度控制点列表
        /// </summary>
        private static readonly TFControlPoint[] _GrayControlPoints;

        /// <summary>
        /// 彩虹控制点列表
        /// </summary>
        private static readonly TFControlPoint[] _RainbowControlPoints;

        /// <summary>
        /// 热金属控制点列表
        /// </summary>
        private static readonly TFControlPoint[] _HotMetalControlPoints;

        /// <summary>
        /// 脑控制点列表
        /// </summary>
        private static readonly TFControlPoint[] _BrainControlPoints;

        /// <summary>
        /// 心脏控制点列表
        /// </summary>
        private static readonly TFControlPoint[] _CardiacControlPoints;

        /// <summary>
        /// 肝脏控制点列表
        /// </summary>
        private static readonly TFControlPoint[] _LiverControlPoints;

        /// <summary>
        /// 肺控制点列表
        /// </summary>
        private static readonly TFControlPoint[] _LungControlPoints;

        /// <summary>
        /// 腹部控制点列表
        /// </summary>
        private static readonly TFControlPoint[] _AbdomenControlPoints;

        /// <summary>
        /// 骨骼控制点列表
        /// </summary>
        private static readonly TFControlPoint[] _BoneControlPoints;

        /// <summary>
        /// 血管控制点列表
        /// </summary>
        private static readonly TFControlPoint[] _VascularControlPoints;

        /// <summary>
        /// 静态构造器
        /// </summary>
        static ProtocolManager()
        {
            _GrayControlPoints = GetGrayControlPoints();
            _RainbowControlPoints = GetRainbowControlPoints();
            _HotMetalControlPoints = GetHotMetalControlPoints();
            _BrainControlPoints = GetBrainControlPoints();
            _CardiacControlPoints = GetCardiacControlPoints();
            _LiverControlPoints = GetLiverControlPoints();
            _LungControlPoints = GetLungControlPoints();
            _AbdomenControlPoints = GetAbdomenControlPoints();
            _BoneControlPoints = GetBoneControlPoints();
            _VascularControlPoints = GetVascularControlPoints();
        }

        #endregion

        #region # 属性

        #region 只读属性 - 灰度控制点列表 —— static IReadOnlyList<TFControlPoint> GrayControlPoints
        /// <summary>
        /// 只读属性 - 灰度控制点列表
        /// </summary>
        public static IReadOnlyList<TFControlPoint> GrayControlPoints
        {
            get => _GrayControlPoints;
        }
        #endregion 

        #region 只读属性 - 彩虹控制点列表 —— static IReadOnlyList<TFControlPoint> RainbowControlPoints
        /// <summary>
        /// 只读属性 - 彩虹控制点列表
        /// </summary>
        public static IReadOnlyList<TFControlPoint> RainbowControlPoints
        {
            get => _RainbowControlPoints;
        }
        #endregion 

        #region 只读属性 - 热金属控制点列表 —— static IReadOnlyList<TFControlPoint> HotMetalControlPoints
        /// <summary>
        /// 只读属性 - 热金属控制点列表
        /// </summary>
        public static IReadOnlyList<TFControlPoint> HotMetalControlPoints
        {
            get => _HotMetalControlPoints;
        }
        #endregion 

        #region 只读属性 - 脑控制点列表 —— static IReadOnlyList<TFControlPoint> BrainControlPoints
        /// <summary>
        /// 只读属性 - 脑控制点列表
        /// </summary>
        public static IReadOnlyList<TFControlPoint> BrainControlPoints
        {
            get => _BrainControlPoints;
        }
        #endregion

        #region 只读属性 - 心脏控制点列表 —— static IReadOnlyList<TFControlPoint> CardiacControlPoints
        /// <summary>
        /// 只读属性 - 心脏控制点列表
        /// </summary>
        public static IReadOnlyList<TFControlPoint> CardiacControlPoints
        {
            get => _CardiacControlPoints;
        }
        #endregion

        #region 只读属性 - 肝脏控制点列表 —— static IReadOnlyList<TFControlPoint> LiverControlPoints
        /// <summary>
        /// 只读属性 - 肝脏控制点列表
        /// </summary>
        public static IReadOnlyList<TFControlPoint> LiverControlPoints
        {
            get => _LiverControlPoints;
        }
        #endregion

        #region 只读属性 - 肺控制点列表 —— static IReadOnlyList<TFControlPoint> LungControlPoints
        /// <summary>
        /// 只读属性 - 肺控制点列表
        /// </summary>
        public static IReadOnlyList<TFControlPoint> LungControlPoints
        {
            get => _LungControlPoints;
        }
        #endregion

        #region 只读属性 - 腹部控制点列表 —— static IReadOnlyList<TFControlPoint> AbdomenControlPoints
        /// <summary>
        /// 只读属性 - 腹部控制点列表
        /// </summary>
        public static IReadOnlyList<TFControlPoint> AbdomenControlPoints
        {
            get => _AbdomenControlPoints;
        }
        #endregion

        #region 只读属性 - 骨骼控制点列表 —— static IReadOnlyList<TFControlPoint> BoneControlPoints
        /// <summary>
        /// 只读属性 - 骨骼控制点列表
        /// </summary>
        public static IReadOnlyList<TFControlPoint> BoneControlPoints
        {
            get => _BoneControlPoints;
        }
        #endregion

        #region 只读属性 - 血管控制点列表 —— static IReadOnlyList<TFControlPoint> VascularControlPoints
        /// <summary>
        /// 只读属性 - 血管控制点列表
        /// </summary>
        public static IReadOnlyList<TFControlPoint> VascularControlPoints
        {
            get => _VascularControlPoints;
        }
        #endregion

        #endregion

        #region # 方法

        #region 获取灰度控制点列表 —— static TFControlPoint[] GetGrayControlPoints()
        /// <summary>
        /// 获取灰度控制点列表
        /// </summary>
        /// <returns>控制点列表</returns>
        private static TFControlPoint[] GetGrayControlPoints()
        {
            TFControlPoint[] controlPoints =
            [
                new TFControlPoint(-1024, new Vector4(0.0f, 0.0f, 0.0f, 0.0f)),
                new TFControlPoint(3071, new Vector4(1.0f, 1.0f, 1.0f, 1.0f))
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
                new TFControlPoint(-1024, new Vector4(0.0f, 0.0f, 0.5f, 1.0f)),  //深蓝 - 空气
                new TFControlPoint(-800, new Vector4(0.0f, 0.0f, 1.0f, 1.0f)),   //蓝 - 肺
                new TFControlPoint(-400, new Vector4(0.0f, 0.8f, 0.8f, 1.0f)),   //青 - 肺纹理
                new TFControlPoint(0, new Vector4(0.0f, 1.0f, 0.0f, 1.0f)),      //绿 - 水/软组织
                new TFControlPoint(100, new Vector4(1.0f, 1.0f, 0.0f, 1.0f)),    //黄 - 肌肉
                new TFControlPoint(400, new Vector4(1.0f, 0.5f, 0.0f, 1.0f)),    //橙 - 松质骨
                new TFControlPoint(1000, new Vector4(1.0f, 0.0f, 0.0f, 1.0f)),   //红 - 密质骨
                new TFControlPoint(3071, new Vector4(0.8f, 0.0f, 0.0f, 1.0f))    //深红 - 致密骨/金属
            ];

            return controlPoints;
        }
        #endregion

        #region 获取热金属控制点列表 —— static TFControlPoint[] GetHotMetalControlPoints()
        /// <summary>
        /// 获取热金属控制点列表
        /// </summary>
        /// <returns>控制点列表</returns>
        private static TFControlPoint[] GetHotMetalControlPoints()
        {
            TFControlPoint[] controlPoints =
            [
                new TFControlPoint(-1024, new Vector4(0.0f, 0.0f, 0.0f, 1.0f)),  //黑 - 空气
                new TFControlPoint(-400, new Vector4(0.3f, 0.0f, 0.0f, 1.0f)),   //暗红 - 肺
                new TFControlPoint(0, new Vector4(0.8f, 0.2f, 0.0f, 1.0f)),      //红 - 软组织
                new TFControlPoint(200, new Vector4(1.0f, 0.5f, 0.0f, 1.0f)),    //橙 - 肌肉
                new TFControlPoint(500, new Vector4(1.0f, 0.8f, 0.0f, 1.0f)),    //橙黄 - 松质骨
                new TFControlPoint(1000, new Vector4(1.0f, 1.0f, 0.0f, 1.0f)),   //黄 - 密质骨
                new TFControlPoint(2000, new Vector4(1.0f, 1.0f, 0.5f, 1.0f)),   //浅黄 - 致密骨
                new TFControlPoint(3071, new Vector4(1.0f, 1.0f, 1.0f, 1.0f))    //白 - 金属
            ];

            return controlPoints;
        }
        #endregion

        #region 获取脑控制点列表 —— static TFControlPoint[] GetBrainControlPoints()
        /// <summary>
        /// 获取脑控制点列表
        /// </summary>
        /// <returns>控制点列表</returns>
        private static TFControlPoint[] GetBrainControlPoints()
        {
            TFControlPoint[] controlPoints =
            [
                //空气/背景：完全透明
                new TFControlPoint(-1024, new Vector4(0.0f, 0.0f, 0.0f, 0.00f)),
                new TFControlPoint(-100, new Vector4(0.0f, 0.0f, 0.0f, 0.00f)),    //Position≈0.226

                //脑脊液：极低透明度
                new TFControlPoint(-20, new Vector4(0.20f, 0.22f, 0.25f, 0.005f)),  //Position≈0.245
                new TFControlPoint(0, new Vector4(0.25f, 0.28f, 0.30f, 0.010f)),    //Position≈0.250

                //灰质：中透明度
                new TFControlPoint(20, new Vector4(0.35f, 0.35f, 0.38f, 0.030f)),   //Position≈0.255
                new TFControlPoint(30, new Vector4(0.45f, 0.45f, 0.48f, 0.080f)),   //Position≈0.257
                new TFControlPoint(40, new Vector4(0.55f, 0.55f, 0.58f, 0.200f)),   //Position≈0.260

                //白质：明显不透明
                new TFControlPoint(50, new Vector4(0.65f, 0.65f, 0.68f, 0.450f)),   //Position≈0.262
                new TFControlPoint(60, new Vector4(0.75f, 0.75f, 0.78f, 0.700f)),   //Position≈0.265
                new TFControlPoint(70, new Vector4(0.85f, 0.85f, 0.88f, 0.880f)),   //Position≈0.267

                //血管/钙化：高不透明度
                new TFControlPoint(80, new Vector4(0.92f, 0.92f, 0.93f, 0.940f)),   //Position≈0.270
                new TFControlPoint(100, new Vector4(0.96f, 0.96f, 0.97f, 0.970f)),  //Position≈0.274

                //骨骼
                new TFControlPoint(200, new Vector4(0.98f, 0.98f, 0.98f, 0.980f)),  //Position≈0.299
                new TFControlPoint(500, new Vector4(1.00f, 1.00f, 1.00f, 0.990f)),  //Position≈0.372

                //窗外锚点
                new TFControlPoint(3071, new Vector4(1.0f, 1.0f, 1.0f, 1.000f))    //Position≈1.00
            ];

            return controlPoints;
        }
        #endregion

        #region 获取心脏控制点列表 —— static TFControlPoint[] GetCardiacControlPoints()
        /// <summary>
        /// 获取心脏控制点列表
        /// </summary>
        /// <returns>控制点列表</returns>
        private static TFControlPoint[] GetCardiacControlPoints()
        {
            TFControlPoint[] controlPoints =
            [
                //空气/肺：完全透明
                new TFControlPoint(-1024, new Vector4(0.0f, 0.0f, 0.0f, 0.00f)),
                new TFControlPoint(-200, new Vector4(0.0f, 0.0f, 0.0f, 0.00f)), //Position≈0.201

                //肺实质：极低透明度
                new TFControlPoint(-100, new Vector4(0.10f, 0.12f, 0.15f, 0.002f)), //Position≈0.226
                new TFControlPoint(-50, new Vector4(0.15f, 0.18f, 0.20f, 0.004f)), //Position≈0.238

                //脂肪/心包脂肪：低透明度
                new TFControlPoint(-30, new Vector4(0.20f, 0.22f, 0.22f, 0.006f)), //Position≈0.243
                new TFControlPoint(0, new Vector4(0.25f, 0.25f, 0.28f, 0.010f)), //Position≈0.250

                //心肌：中透明度
                new TFControlPoint(20, new Vector4(0.35f, 0.35f, 0.38f, 0.025f)), //Position≈0.255
                new TFControlPoint(40, new Vector4(0.50f, 0.50f, 0.52f, 0.060f)), //Position≈0.260
                new TFControlPoint(60, new Vector4(0.65f, 0.65f, 0.68f, 0.150f)), //Position≈0.265

                //心肌/室间隔核心：明显不透明
                new TFControlPoint(80, new Vector4(0.75f, 0.75f, 0.78f, 0.350f)), //Position≈0.270
                new TFControlPoint(100, new Vector4(0.82f, 0.82f, 0.85f, 0.550f)), //Position≈0.274

                //血管/增强心脏：高不透明度
                new TFControlPoint(150, new Vector4(0.88f, 0.88f, 0.90f, 0.750f)), //Position≈0.287
                new TFControlPoint(200, new Vector4(0.92f, 0.92f, 0.94f, 0.880f)), //Position≈0.299

                //冠状动脉钙化：极高不透明度
                new TFControlPoint(300, new Vector4(0.95f, 0.95f, 0.96f, 0.940f)), //Position≈0.323
                new TFControlPoint(400, new Vector4(0.97f, 0.97f, 0.98f, 0.970f)), //Position≈0.348

                //骨骼/金属
                new TFControlPoint(600, new Vector4(0.99f, 0.99f, 0.99f, 0.980f)), //Position≈0.396

                //窗外锚点
                new TFControlPoint(3071, new Vector4(1.0f, 1.0f, 1.0f, 1.000f)) //Position≈1.00
            ];

            return controlPoints;
        }
        #endregion

        #region 获取肺控制点列表 —— static TFControlPoint[] GetLungControlPoints()
        /// <summary>
        /// 获取肺控制点列表
        /// </summary>
        /// <returns>控制点列表</returns>
        private static TFControlPoint[] GetLungControlPoints()
        {
            TFControlPoint[] controlPoints =
            [
                //空气/背景：完全透明
                new TFControlPoint(-1024, new Vector4(0.0f, 0.0f, 0.0f, 0.00f)),
                new TFControlPoint(-950, new Vector4(0.0f, 0.0f, 0.0f, 0.00f)),   //Position≈0.018

                //肺实质：淡蓝灰，极低透明度
                new TFControlPoint(-900, new Vector4(0.15f, 0.18f, 0.22f, 0.003f)), //Position≈0.030
                new TFControlPoint(-850, new Vector4(0.20f, 0.24f, 0.28f, 0.008f)), //Position≈0.042

                //肺纹理/支气管：中灰蓝，低透明度
                new TFControlPoint(-800, new Vector4(0.25f, 0.30f, 0.35f, 0.020f)), //Position≈0.055
                new TFControlPoint(-700, new Vector4(0.30f, 0.35f, 0.40f, 0.040f)), //Position≈0.079

                //肺门/血管过渡
                new TFControlPoint(-500, new Vector4(0.40f, 0.42f, 0.45f, 0.080f)), //Position≈0.128
                new TFControlPoint(-300, new Vector4(0.50f, 0.50f, 0.50f, 0.150f)), //Position≈0.177

                //软组织/胸壁
                new TFControlPoint(-100, new Vector4(0.55f, 0.52f, 0.48f, 0.250f)), //Position≈0.226
                new TFControlPoint(0, new Vector4(0.60f, 0.55f, 0.50f, 0.400f)),    //Position≈0.250
                new TFControlPoint(50, new Vector4(0.65f, 0.60f, 0.55f, 0.550f)),   //Position≈0.262

                //纵隔结构
                new TFControlPoint(100, new Vector4(0.75f, 0.70f, 0.65f, 0.700f)),  //Position≈0.274
                new TFControlPoint(200, new Vector4(0.85f, 0.80f, 0.75f, 0.850f)),  //Position≈0.299

                //骨骼/钙化
                new TFControlPoint(400, new Vector4(0.92f, 0.88f, 0.82f, 0.920f)),  //Position≈0.348
                new TFControlPoint(600, new Vector4(0.96f, 0.93f, 0.88f, 0.960f)),  //Position≈0.396

                //窗外锚点
                new TFControlPoint(3071, new Vector4(1.0f, 1.0f, 1.0f, 1.000f))    //Position≈1.00
            ];

            return controlPoints;
        }
        #endregion

        #region 获取肝脏控制点列表 —— static TFControlPoint[] GetLiverControlPoints()
        /// <summary>
        /// 获取肝脏控制点列表
        /// </summary>
        /// <returns>控制点列表</returns>
        private static TFControlPoint[] GetLiverControlPoints()
        {
            TFControlPoint[] controlPoints =
            [
                //空气/背景：完全透明
                new TFControlPoint(-1024, new Vector4(0.0f, 0.0f, 0.0f, 0.00f)),
                new TFControlPoint(-160, new Vector4(0.0f, 0.0f, 0.0f, 0.00f)),    //Position≈0.211

                //脂肪/腹水：极低透明度
                new TFControlPoint(-80, new Vector4(0.15f, 0.15f, 0.18f, 0.003f)),  //Position≈0.231
                new TFControlPoint(-20, new Vector4(0.20f, 0.20f, 0.22f, 0.005f)),  //Position≈0.245

                //正常肝脏：低中透明度
                new TFControlPoint(20, new Vector4(0.30f, 0.28f, 0.25f, 0.012f)),   //Position≈0.255
                new TFControlPoint(40, new Vector4(0.45f, 0.40f, 0.35f, 0.030f)),   //Position≈0.260
                new TFControlPoint(60, new Vector4(0.55f, 0.50f, 0.42f, 0.080f)),   //Position≈0.265

                //肝脏/脾脏：中透明度
                new TFControlPoint(80, new Vector4(0.65f, 0.58f, 0.48f, 0.180f)),   //Position≈0.270
                new TFControlPoint(100, new Vector4(0.75f, 0.65f, 0.55f, 0.350f)),  //Position≈0.274

                //血管/增强肝脏：明显不透明
                new TFControlPoint(120, new Vector4(0.82f, 0.72f, 0.60f, 0.550f)),  //Position≈0.279
                new TFControlPoint(150, new Vector4(0.88f, 0.80f, 0.68f, 0.750f)),  //Position≈0.287

                //钙化/结石
                new TFControlPoint(200, new Vector4(0.92f, 0.88f, 0.80f, 0.880f)),  //Position≈0.299
                new TFControlPoint(300, new Vector4(0.96f, 0.93f, 0.88f, 0.940f)),  //Position≈0.323

                //骨骼
                new TFControlPoint(400, new Vector4(0.98f, 0.96f, 0.92f, 0.970f)),  //Position≈0.348

                //窗外锚点
                new TFControlPoint(3071, new Vector4(1.0f, 1.0f, 1.0f, 1.000f))    //Position≈1.00
            ];

            return controlPoints;
        }
        #endregion

        #region 获取腹部控制点列表 —— static TFControlPoint[] GetAbdomenControlPoints()
        /// <summary>
        /// 获取腹部控制点列表
        /// </summary>
        /// <returns>控制点列表</returns>
        private static TFControlPoint[] GetAbdomenControlPoints()
        {
            TFControlPoint[] controlPoints =
            [
                //空气/背景：完全透明
                new TFControlPoint(-1024, new Vector4(0.0f, 0.0f, 0.0f, 0.00f)),
                new TFControlPoint(-200, new Vector4(0.0f, 0.0f, 0.0f, 0.00f)),    //Position≈0.201

                //脂肪：微透明灰色
                new TFControlPoint(-100, new Vector4(0.20f, 0.18f, 0.15f, 0.003f)), //Position≈0.226
                new TFControlPoint(-50, new Vector4(0.25f, 0.22f, 0.18f, 0.005f)),  //Position≈0.238

                //软组织/肌肉：低透明灰褐色
                new TFControlPoint(0, new Vector4(0.30f, 0.25f, 0.20f, 0.008f)),    //Position≈0.250
                new TFControlPoint(40, new Vector4(0.35f, 0.30f, 0.25f, 0.012f)),   //Position≈0.260
                new TFControlPoint(80, new Vector4(0.40f, 0.35f, 0.30f, 0.020f)),   //Position≈0.270

                //肝脏/实质器官：中透明度
                new TFControlPoint(120, new Vector4(0.50f, 0.45f, 0.35f, 0.040f)),  //Position≈0.279
                new TFControlPoint(160, new Vector4(0.60f, 0.50f, 0.40f, 0.080f)),  //Position≈0.289

                //血管/增强区域
                new TFControlPoint(200, new Vector4(0.70f, 0.60f, 0.50f, 0.200f)),  //Position≈0.299
                new TFControlPoint(240, new Vector4(0.80f, 0.70f, 0.60f, 0.400f)),  //Position≈0.309

                //骨骼边缘
                new TFControlPoint(300, new Vector4(0.90f, 0.80f, 0.70f, 0.650f)),  //Position≈0.323
                new TFControlPoint(400, new Vector4(0.95f, 0.90f, 0.80f, 0.850f)),  //Position≈0.348

                //窗外锚点
                new TFControlPoint(3071, new Vector4(1.0f, 1.0f, 1.0f, 1.000f))    //Position≈1.00
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
                new TFControlPoint(-1024, new Vector4(0.0f, 0.0f, 0.0f, 0.00f)),
                new TFControlPoint(200, new Vector4(0.0f, 0.0f, 0.0f, 0.00f)), //Position≈0.30

                //软组织：极低透明度（几乎透明）
                new TFControlPoint(409, new Vector4(0.3f, 0.3f, 0.3f, 0.005f)), //Position≈0.35
                new TFControlPoint(614, new Vector4(0.4f, 0.4f, 0.4f, 0.008f)), //Position≈0.40
                new TFControlPoint(818, new Vector4(0.5f, 0.5f, 0.5f, 0.010f)), //Position≈0.45

                //骨骼开始：陡峭变化
                new TFControlPoint(941, new Vector4(0.7f, 0.6f, 0.5f, 0.02f)), //Position≈0.48
                new TFControlPoint(1023, new Vector4(0.8f, 0.7f, 0.6f, 0.50f)), //Position≈0.50 骨骼！
                new TFControlPoint(1105, new Vector4(0.9f, 0.8f, 0.7f, 0.85f)), //Position≈0.52

                //标准骨骼：高不透明度
                new TFControlPoint(1228, new Vector4(1.0f, 0.9f, 0.8f, 0.92f)), //Position≈0.55
                new TFControlPoint(1433, new Vector4(1.0f, 0.95f, 0.85f, 0.95f)), //Position≈0.60
                new TFControlPoint(1637, new Vector4(1.0f, 0.97f, 0.90f, 0.97f)), //Position≈0.65

                //高密度骨骼：完全不透明
                new TFControlPoint(1842, new Vector4(1.0f, 0.98f, 0.93f, 0.98f)), //Position≈0.70
                new TFControlPoint(2252, new Vector4(1.0f, 1.0f, 0.96f, 0.99f)), //Position≈0.80
                new TFControlPoint(2661, new Vector4(1.0f, 1.0f, 0.98f, 0.995f)), //Position≈0.90
                new TFControlPoint(3071, new Vector4(1.0f, 1.0f, 1.0f, 1.000f)) //Position≈1.00
            ];

            return controlPoints;
        }
        #endregion

        #region 获取血管控制点列表 —— static TFControlPoint[] GetVascularControlPoints()
        /// <summary>
        /// 获取血管控制点列表
        /// </summary>
        /// <returns>控制点列表</returns>
        private static TFControlPoint[] GetVascularControlPoints()
        {
            TFControlPoint[] controlPoints =
            [
                //空气/背景：完全透明
                new TFControlPoint(-1024, new Vector4(0.0f, 0.0f, 0.0f, 0.00f)),
                new TFControlPoint(-100, new Vector4(0.0f, 0.0f, 0.0f, 0.00f)),    //Position≈0.226

                //软组织：极低透明度
                new TFControlPoint(-50, new Vector4(0.15f, 0.15f, 0.15f, 0.002f)),  //Position≈0.238
                new TFControlPoint(0, new Vector4(0.20f, 0.20f, 0.20f, 0.004f)),    //Position≈0.250
                new TFControlPoint(50, new Vector4(0.25f, 0.25f, 0.25f, 0.008f)),   //Position≈0.262

                //软组织/肌肉：低透明度
                new TFControlPoint(80, new Vector4(0.30f, 0.30f, 0.32f, 0.015f)),   //Position≈0.270
                new TFControlPoint(100, new Vector4(0.35f, 0.35f, 0.38f, 0.025f)),  //Position≈0.274

                //血管开始：对比增强区
                new TFControlPoint(150, new Vector4(0.50f, 0.50f, 0.55f, 0.080f)),  //Position≈0.287
                new TFControlPoint(200, new Vector4(0.70f, 0.70f, 0.75f, 0.250f)),  //Position≈0.299
                new TFControlPoint(250, new Vector4(0.85f, 0.85f, 0.88f, 0.550f)),  //Position≈0.311

                //血管核心：高不透明度
                new TFControlPoint(300, new Vector4(0.92f, 0.92f, 0.94f, 0.780f)),  //Position≈0.323
                new TFControlPoint(350, new Vector4(0.95f, 0.95f, 0.96f, 0.900f)),  //Position≈0.335
                new TFControlPoint(400, new Vector4(0.97f, 0.97f, 0.98f, 0.950f)),  //Position≈0.348

                //骨骼/钙化：几乎不透明
                new TFControlPoint(500, new Vector4(0.98f, 0.98f, 0.99f, 0.970f)),  //Position≈0.372
                new TFControlPoint(600, new Vector4(0.99f, 0.99f, 0.99f, 0.980f)),  //Position≈0.396

                //窗外锚点
                new TFControlPoint(3071, new Vector4(1.0f, 1.0f, 1.0f, 1.000f))    //Position≈1.00
            ];

            return controlPoints;
        }
        #endregion

        #endregion
    }
}
