using MedicalSharp.Primitives.Models;

namespace MedicalSharp.Primitives.Managers
{
    /// <summary>
    /// 窗宽/窗位管理器
    /// </summary>
    public static class WindowLevelManager
    {
        #region # 字段及构造器

        /// <summary>
        /// 脑窗
        /// </summary>
        private static readonly WindowLevel _Brain;

        /// <summary>
        /// 心脏窗
        /// </summary>
        private static readonly WindowLevel _Cardiac;

        /// <summary>
        /// 肝脏窗
        /// </summary>
        private static readonly WindowLevel _Liver;

        /// <summary>
        /// 肺部窗
        /// </summary>
        private static readonly WindowLevel _Lung;

        /// <summary>
        /// 腹部窗
        /// </summary>
        private static readonly WindowLevel _Abdomen;

        /// <summary>
        /// 骨骼窗
        /// </summary>
        private static readonly WindowLevel _Bone;

        /// <summary>
        /// 血管窗
        /// </summary>
        private static readonly WindowLevel _Vascular;

        /// <summary>
        /// 纵隔窗
        /// </summary>
        private static readonly WindowLevel _Mediastinum;

        /// <summary>
        /// 静态构造器
        /// </summary>
        static WindowLevelManager()
        {
            _Brain = new WindowLevel("脑窗", 80, 40);
            _Cardiac = new WindowLevel("心脏窗", 550, 60);
            _Liver = new WindowLevel("肝脏窗", 320, 60);
            _Lung = new WindowLevel("肺窗", 1500, -600);
            _Abdomen = new WindowLevel("腹部窗", 400, 40);
            _Bone = new WindowLevel("骨骼窗", 1200, 500);
            _Vascular = new WindowLevel("血管窗", 600, 300);
            _Mediastinum = new WindowLevel("纵膈窗", 400, 40);
        }

        #endregion

        #region # 属性

        #region 只读属性 - 脑窗 —— static WindowLevel Brain
        /// <summary>
        /// 只读属性 - 脑窗
        /// </summary>
        public static WindowLevel Brain
        {
            get => _Brain;
        }
        #endregion

        #region 只读属性 - 心脏窗 —— static WindowLevel Cardiac
        /// <summary>
        /// 只读属性 - 心脏窗
        /// </summary>
        public static WindowLevel Cardiac
        {
            get => _Cardiac;
        }
        #endregion

        #region 只读属性 - 肝脏窗 —— static WindowLevel Liver
        /// <summary>
        /// 只读属性 - 肝脏窗
        /// </summary>
        public static WindowLevel Liver
        {
            get => _Liver;
        }
        #endregion

        #region 只读属性 - 肺窗 —— static WindowLevel Lung
        /// <summary>
        /// 只读属性 - 肺窗
        /// </summary>
        public static WindowLevel Lung
        {
            get => _Lung;
        }
        #endregion

        #region 只读属性 - 腹部窗 —— static WindowLevel Abdomen
        /// <summary>
        /// 只读属性 - 腹部窗
        /// </summary>
        public static WindowLevel Abdomen
        {
            get => _Abdomen;
        }
        #endregion

        #region 只读属性 - 骨骼窗 —— static WindowLevel Bone
        /// <summary>
        /// 只读属性 - 骨骼窗
        /// </summary>
        public static WindowLevel Bone
        {
            get => _Bone;
        }
        #endregion

        #region 只读属性 - 血管窗 —— static WindowLevel Vascular
        /// <summary>
        /// 只读属性 - 血管窗
        /// </summary>
        public static WindowLevel Vascular
        {
            get => _Vascular;
        }
        #endregion

        #region 只读属性 - 纵隔窗 —— static WindowLevel Mediastinum
        /// <summary>
        /// 只读属性 - 纵隔窗
        /// </summary>
        public static WindowLevel Mediastinum
        {
            get => _Mediastinum;
        }
        #endregion

        #endregion
    }
}
