using MedicalSharp.Engine.Resources;

namespace MedicalSharp.Engine.Interfaces
{
    /// <summary>
    /// 平面观察者接口
    /// </summary>
    public interface IPlaneObserver
    {
        /// <summary>
        /// 平面变化事件
        /// </summary>
        void OnPlaneChanged(MPRPlane plane);
    }
}
